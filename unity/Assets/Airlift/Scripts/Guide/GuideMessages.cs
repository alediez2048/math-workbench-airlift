using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Airlift.Guide
{
    /// One parsed Realtime server event, reduced to what the app needs.
    public struct GuideEvent
    {
        public string Type;
        public string Delta;          // audio base64 or transcript text delta
        public string Transcript;     // completed transcript text
        public string ToolName;
        public string CallId;
        public string ArgumentsJson;
        public string ErrorMessage;
        public string ResponseStatus;
    }

    /// Pure parser and builders for the OpenAI Realtime GA wire format. No Unity state.
    public static class GuideMessages
    {
        public static GuideEvent Parse(string json)
        {
            var e = new GuideEvent();
            JObject o;
            try { o = JObject.Parse(json); } catch (Exception) { e.Type = "malformed"; return e; }
            e.Type = (string)o["type"] ?? "";
            switch (e.Type)
            {
                case "response.output_audio.delta":
                case "response.output_audio_transcript.delta":
                case "response.function_call_arguments.delta":
                    e.Delta = (string)o["delta"] ?? ""; break;
                case "response.output_audio_transcript.done":
                case "conversation.item.input_audio_transcription.completed":
                    e.Transcript = (string)o["transcript"] ?? ""; break;
                case "response.function_call_arguments.done":
                    e.ToolName = (string)o["name"] ?? ""; e.CallId = (string)o["call_id"] ?? ""; e.ArgumentsJson = (string)o["arguments"] ?? "{}"; break;
                case "response.done":
                    e.ResponseStatus = (string)o.SelectToken("response.status") ?? ""; break;
                case "error":
                    e.ErrorMessage = (string)o.SelectToken("error.message") ?? json; break;
            }
            return e;
        }

        public static string AudioAppend(byte[] pcm16, int count)
            => new JObject { ["type"] = "input_audio_buffer.append", ["audio"] = Convert.ToBase64String(pcm16, 0, count) }.ToString(Newtonsoft.Json.Formatting.None);

        public static string UserText(string text)
            => Message("user", text);

        /// Structured app context (validated lesson state) as a system message; never free learner text.
        public static string SystemContext(string text)
            => Message("system", text);

        static string Message(string role, string text)
            => new JObject { ["type"] = "conversation.item.create", ["item"] = new JObject { ["type"] = "message", ["role"] = role,
                   ["content"] = new JArray(new JObject { ["type"] = "input_text", ["text"] = text }) } }.ToString(Newtonsoft.Json.Formatting.None);

        public static string ToolOutput(string callId, string outputJson)
            => new JObject { ["type"] = "conversation.item.create", ["item"] = new JObject { ["type"] = "function_call_output", ["call_id"] = callId, ["output"] = outputJson } }.ToString(Newtonsoft.Json.Formatting.None);

        public static string ResponseCreate(string instructions = null)
        {
            var o = new JObject { ["type"] = "response.create" };
            if (!string.IsNullOrEmpty(instructions)) o["response"] = new JObject { ["instructions"] = instructions };
            return o.ToString(Newtonsoft.Json.Formatting.None);
        }

        public static byte[] ToPcm16(float[] samples, int count)
        {
            var bytes = new byte[count * 2];
            for (int i = 0; i < count; i++)
            {
                short s = (short)Mathf.Clamp(samples[i] * 32767f, -32768f, 32767f);
                bytes[i * 2] = (byte)(s & 0xff); bytes[i * 2 + 1] = (byte)((s >> 8) & 0xff);
            }
            return bytes;
        }

        public static int FromPcm16(byte[] pcm, float[] into, int offset)
        {
            int n = pcm.Length / 2;
            for (int i = 0; i < n; i++) { short s = (short)(pcm[i * 2] | (pcm[i * 2 + 1] << 8)); into[(offset + i) % into.Length] = s / 32768f; }
            return n;
        }
    }
}
