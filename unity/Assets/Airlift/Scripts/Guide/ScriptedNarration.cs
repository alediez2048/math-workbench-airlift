using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Airlift.Guide
{
    /// A tour line owns its response. Never play generated instructions until its transcript
    /// matches the current script. Old responses cannot become current again after cancellation.
    public sealed class ScriptedNarration
    {
        public bool Active { get; private set; }
        public string Token { get; private set; }
        public string Line { get; private set; }
        public string Result { get; private set; } = "idle";
        readonly HashSet<string> scripted = new HashSet<string>();
        readonly HashSet<string> ignored = new HashSet<string>();
        readonly MemoryStream audio = new MemoryStream();
        string responseId, transcript;
        bool overflow;
        const int MaxBytes = AudioPlayback.InputRate * 2 * 25; // below playback's 30-second ring

        public string Begin(string line)
        {
            Invalidate(); Active = true; Line = line; Token = Guid.NewGuid().ToString("N");
            Result = "pending"; return Token;
        }

        public void Ignore(string id) { if (!string.IsNullOrEmpty(id)) ignored.Add(id); }
        public void Invalidate()
        {
            Ignore(responseId); responseId = null; Token = null; transcript = null;
            audio.SetLength(0); overflow = false; Result = "cancelled";
        }
        public void End() { Invalidate(); Active = false; }

        // Returns true when this event belongs to the isolated narration lane, including stale lines.
        public bool Handle(GuideEvent e, out byte[] approvedAudio)
        {
            approvedAudio = null;
            if (!string.IsNullOrEmpty(e.NarrationToken))
            {
                if (!string.IsNullOrEmpty(e.ResponseId)) scripted.Add(e.ResponseId);
                if (Active && e.NarrationToken == Token && e.Type == "response.created") responseId = e.ResponseId;
            }
            if (string.IsNullOrEmpty(e.ResponseId) || !scripted.Contains(e.ResponseId)) return false;
            if (!Active || e.ResponseId != responseId || ignored.Contains(e.ResponseId)) return true;
            switch (e.Type)
            {
                case "response.output_audio.delta":
                    try
                    {
                        var bytes = Convert.FromBase64String(e.Delta);
                        if (audio.Length + bytes.Length <= MaxBytes && !overflow) audio.Write(bytes, 0, bytes.Length);
                        else { overflow = true; audio.SetLength(0); }
                    }
                    catch (FormatException) { overflow = true; audio.SetLength(0); }
                    break;
                case "response.output_audio_transcript.done": transcript = e.Transcript; break;
                case "response.done":
                    bool match = e.ResponseStatus == "completed" && !overflow && audio.Length > 0
                        && Normalize(transcript) == Normalize(Line);
                    if (match) approvedAudio = audio.ToArray();
                    Result = match ? "verified" : "rejected";
                    Ignore(responseId); audio.SetLength(0); break;
            }
            return true;
        }

        public bool SuppressMedia(string id) => Active || (!string.IsNullOrEmpty(id) && ignored.Contains(id));
        static string Normalize(string text)
        {
            var result = new StringBuilder();
            foreach (char c in text ?? "") if (char.IsLetterOrDigit(c)) result.Append(char.ToLowerInvariant(c));
            return result.ToString();
        }
    }
}
