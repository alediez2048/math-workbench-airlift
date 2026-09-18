using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;

namespace Airlift.Guide
{
    /// One live guide session: mint → connect → stream mic → play audio → surface transcripts and
    /// tool calls to the app. All network work runs off the main thread; events are raised on it.
    /// The proxy owns instructions and tools; this class never sends instructions.
    public sealed class GuideSession : MonoBehaviour
    {
        public string mintUrl = "";
        [Tooltip("Voice language (en or es) the session is minted and locked in; set from the consent card before Begin.")]
        public string language = "en";
        public string clientMarker = "nerdy-quest";
        public string build = "0.2.0";
        public bool adultTesterOnlySatisfied = true;
        public AudioPlayback playback;

        public event Action<GuideMode> ModeChanged;
        public event Action<string> StateChanged;
        public event Action<string> GuideTranscriptDelta;
        public event Action<string> GuideTranscriptDone;
        public event Action<string> UserTranscriptDone;
        public event Action<string, string, string> ToolCall;   // name, callId, argumentsJson
        public event Action<double> LatencyMeasured;
        public event Action<string> Error;

        public GuideMode Mode { get; private set; } = GuideMode.Offline;
        public string State { get; private set; } = "idle";
        public bool MicActive => mic.IsRunning && micEnabled;
        /// True while mic audio is actually being streamed to the guide (used to duck the music).
        public bool MicStreaming => Mode == GuideMode.Live && mic.IsRunning && MicGateOpen;

        const string Model = "gpt-realtime";
        static readonly System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
        ClientWebSocket socket; CancellationTokenSource cts;
        readonly MicStreamer mic = new MicStreamer(); bool micEnabled = true;
        readonly Queue<(int Generation, string Json)> incoming = new Queue<(int, string)>(); readonly object inLock = new object();
        readonly Queue<string> outgoing = new Queue<string>(); readonly object outLock = new object(); bool senderRunning;
        double speechStoppedAt = -1; bool firstDeltaSeen; bool wasOpen; int reconnects;
        float lastGuideAudioAt = -10f; const float MicHoldAfterSpeechSeconds = 0.6f;
        /// Half-duplex: the Quest mic hears the Quest speaker, so never stream while the guide speaks.
        readonly PromptGate gate = new PromptGate();
        readonly ScriptedNarration narration = new ScriptedNarration();
        string activeResponseId;
        int generation;
        bool stopped, connecting;
        public bool ConversationStopped => stopped;
        public int ConnectionGeneration => generation;
        public string NarrationStatus => narration.Result;
        public string NarrationLine => narration.Line;
        /// Test seam (CargoVoiceCharacterizationTests): every message the app hands this session, in order, including
        /// response requests, whether or not a socket is open. Null in the app.
        [NonSerialized] public Action<string> OutgoingTap;
        public bool MicGateOpen => micEnabled && (playback == null || !playback.IsSpeaking) && Time.time - lastGuideAudioAt > MicHoldAfterSpeechSeconds;

        public void Begin()
        {
            if (connecting || (!stopped && socket != null && socket.State == WebSocketState.Open)) return;
            stopped = false; connecting = true; generation++;
            StartCoroutine(Run(generation));
        }

        public void End()
        {
            stopped = true; generation++; connecting = false;
            StopAllCoroutines(); micEnabled = false; mic.Stop();
            Hush(); gate.ResponseFinished(); activeResponseId = null;
            lock (inLock) incoming.Clear();
            lock (outLock) { outgoing.Clear(); senderRunning = false; }
            lock (mainThread) mainThread.Clear();
            try { cts?.Cancel(); socket?.Dispose(); } catch { }
            socket = null; SetMode(GuideMode.Offline); SetState("stopped");
        }

        public void SetMicEnabled(bool on)
        {
            on &= !stopped;
            bool changed = micEnabled != on;
            micEnabled = on;
            if (!on) { mic.Stop(); if (changed) Send("{\"type\":\"input_audio_buffer.clear\"}"); }
            else if (Mode == GuideMode.Live && !mic.IsRunning && Permission.HasUserAuthorizedPermission(Permission.Microphone)) mic.Start();
        }
        public bool MicEnabled => micEnabled;
        /// Stops the guide mid-sentence: cancel the active response (if any), drop queued prompts and audio.
        public void Hush() { narration.Ignore(activeResponseId); narration.Invalidate(); if (gate.Cancel(Time.time)) Send("{\"type\":\"response.cancel\"}"); if (playback != null) playback.Clear(); }
        /// Out-of-band narration does not contend with the conversational response queue.
        public void SpeakScripted(string line, bool speak = true)
        {
            Hush(); var token = narration.Begin(line);
            if (speak && !stopped) { SetState("thinking"); Send(GuideMessages.ScriptedResponse(line, token)); }
        }
        public void EndScriptedNarration() { Hush(); narration.End(); }
        public void SendUserText(string text) { Send(GuideMessages.UserText(text)); Queue(null); }
        public void PushContext(string contextText) { Send(GuideMessages.SystemContext(contextText)); }
        public void SubmitToolResult(string callId, string outputJson) { Send(GuideMessages.ToolOutput(callId, outputJson)); Queue(null); }
        /// Tool output without asking for a spoken reply (used while the learner has paused the guide).
        public void SubmitToolResult(string callId, string outputJson, bool respond) { if (respond) { SubmitToolResult(callId, outputJson); return; } Send(GuideMessages.ToolOutput(callId, outputJson)); }
        /// Queued behind any active response; the gate sends response.create when the server is free.
        public void Prompt(string instructions) { Queue(instructions); }
        void Queue(string instructions) { if (stopped) return; OutgoingTap?.Invoke(GuideMessages.ResponseCreate(instructions)); if (socket == null || socket.State != WebSocketState.Open) return; gate.Request(instructions, Time.time); }

        IEnumerator Run(int epoch)
        {
            // Permission is requested by an explicit eligible Play action, never a silent connection.
            if (epoch != generation || stopped) yield break;
            bool micOk = Permission.HasUserAuthorizedPermission(Permission.Microphone);
            SetState("minting");
            string secret = null;
            if (!string.IsNullOrEmpty(mintUrl))
            {
                var body = GuideMessages.MintRequestBody(Guid.NewGuid().ToString("N"), build, language);
                using (var req = new UnityWebRequest(mintUrl, "POST"))
                {
                    req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)); req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Content-Type", "application/json"); req.SetRequestHeader("x-nerdy-client", clientMarker); req.timeout = 12;
                    yield return req.SendWebRequest();
                    if (epoch != generation || stopped) yield break;
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        try { secret = (string)Newtonsoft.Json.Linq.JObject.Parse(req.downloadHandler.text)["value"]; } catch { secret = null; }
                    }
                    else Error?.Invoke("mint failed: " + req.error);
                }
            }
            bool minted = !string.IsNullOrEmpty(secret);
            bool open = false;
            if (minted)
            {
                SetState("connecting");
                var task = Connect(secret, epoch);
                while (!task.IsCompleted) yield return null;
                if (epoch != generation || stopped) yield break;
                open = !task.IsFaulted && socket != null && socket.State == WebSocketState.Open;
                if (task.IsFaulted) Error?.Invoke("connect failed: " + task.Exception?.GetBaseException().Message);
            }
            connecting = false;
            SetMode(GuideRouting.Decide(micOk, minted, open, adultTesterOnlySatisfied));
            if (Mode == GuideMode.Live)
            {
                wasOpen = true; SetState("ready");
                if (micOk && micEnabled) mic.Start();
                _ = ReceiveLoop(socket, cts.Token, epoch);
            }
            else SetState("offline");
        }

        async Task Connect(string secret, int epoch)
        {
            if (epoch != generation || stopped) return;
            socket = new ClientWebSocket(); cts = new CancellationTokenSource();
            socket.Options.SetRequestHeader("Authorization", "Bearer " + secret);
            await socket.ConnectAsync(new Uri("wss://api.openai.com/v1/realtime?model=" + Model), cts.Token);
        }

        void Update()
        {
            if (stopped) return;
            if (narration.Active && narration.Result == "verified" && State == "speaking"
                && playback != null && !playback.IsSpeaking && Time.time - lastGuideAudioAt > MicHoldAfterSpeechSeconds) SetState("ready");
            if (Mode == GuideMode.Live && socket != null && socket.State == WebSocketState.Open)
            {
                if (micEnabled && !mic.IsRunning && Permission.HasUserAuthorizedPermission(Permission.Microphone)) mic.Start();
                var chunk = mic.Poll(); // always drain so stale audio never accumulates
                if (chunk != null && MicGateOpen) Send(GuideMessages.AudioAppend(chunk, chunk.Length));
                if (gate.TryTake(Time.time, out var instructions)) Send(GuideMessages.ResponseCreate(instructions));
            }
            while (true)
            {
                (int Generation, string Json) msg; lock (inLock) { if (incoming.Count == 0) break; msg = incoming.Dequeue(); }
                HandleIncoming(msg.Generation, msg.Json);
            }
        }

        void HandleIncoming(int epoch, string json)
        {
            if (!stopped && epoch == generation) Handle(GuideMessages.Parse(json));
        }

        void Handle(GuideEvent e)
        {
            if (stopped) return;
            if (narration.Handle(e, out var approvedAudio))
            {
                if (approvedAudio != null)
                {
                    playback?.Enqueue(approvedAudio); lastGuideAudioAt = Time.time; SetState("speaking");
                    GuideTranscriptDone?.Invoke(narration.Line);
                }
                else if (e.Type == "response.done" && narration.Result == "rejected")
                {
                    SetState("ready"); Debug.LogWarning("[Guide] Tour narration rejected; current written instruction remains available.");
                }
                return;
            }
            if (narration.SuppressMedia(e.ResponseId) && (e.Type == "response.output_audio.delta"
                || e.Type == "response.output_audio_transcript.delta" || e.Type == "response.output_audio_transcript.done")) return;
            switch (e.Type)
            {
                case "input_audio_buffer.speech_started": SetState("listening"); break;
                case "response.created": activeResponseId = e.ResponseId; if (narration.Active) narration.Ignore(e.ResponseId); gate.ResponseStarted(); Send("{\"type\":\"input_audio_buffer.clear\"}"); break;
                case "input_audio_buffer.speech_stopped": speechStoppedAt = clock.Elapsed.TotalMilliseconds; firstDeltaSeen = false; SetState("thinking"); break;
                case "response.output_audio.delta":
                    if (!firstDeltaSeen) { firstDeltaSeen = true; if (speechStoppedAt >= 0) LatencyMeasured?.Invoke(clock.Elapsed.TotalMilliseconds - speechStoppedAt); }
                    SetState("speaking"); lastGuideAudioAt = Time.time; if (playback != null) playback.Enqueue(Convert.FromBase64String(e.Delta)); break;
                case "response.output_audio_transcript.delta": GuideTranscriptDelta?.Invoke(e.Delta); break;
                case "response.output_audio_transcript.done": GuideTranscriptDone?.Invoke(e.Transcript); break;
                case "conversation.item.input_audio_transcription.completed": UserTranscriptDone?.Invoke(e.Transcript); break;
                case "response.function_call_arguments.done": ToolCall?.Invoke(e.ToolName, e.CallId, e.ArgumentsJson); break;
                case "response.done": if (e.ResponseId == activeResponseId) { activeResponseId = null; gate.ResponseFinished(); SetState("ready"); } break;
                case "error":
                    // A cancel that raced an already-finished response reports no active response: free the gate.
                    if (e.ErrorMessage != null && e.ErrorMessage.IndexOf("no active response", StringComparison.OrdinalIgnoreCase) >= 0) gate.ResponseFinished();
                    Error?.Invoke(e.ErrorMessage); break;
            }
        }

        async Task ReceiveLoop(ClientWebSocket connection, CancellationToken token, int epoch)
        {
            var buffer = new byte[1 << 16]; var sb = new StringBuilder();
            try
            {
                while (epoch == generation && connection.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await connection.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if (result.MessageType == WebSocketMessageType.Close) break;
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    if (result.EndOfMessage) { var msg = sb.ToString(); sb.Clear(); lock (inLock) { if (epoch == generation && !stopped) incoming.Enqueue((epoch, msg)); } }
                }
            }
            catch (Exception) { /* Cancellation and network loss are handled on the main thread below. */ }
            UnityMainThread(() => {
                if (epoch != generation || stopped) return;
                if (GuideRouting.ShouldReconnect(reconnects, wasOpen)) { reconnects++; connecting = false; Begin(); }
                else { SetMode(GuideMode.Offline); SetState("offline"); }
            });
        }

        readonly Queue<Action> mainThread = new Queue<Action>();
        void UnityMainThread(Action a) { lock (mainThread) mainThread.Enqueue(a); }
        void LateUpdate() { while (true) { Action a; lock (mainThread) { if (mainThread.Count == 0) break; a = mainThread.Dequeue(); } a(); } }

        void Send(string json)
        {
            if (stopped) return;
            OutgoingTap?.Invoke(json);
            if (socket == null || socket.State != WebSocketState.Open) return;
            lock (outLock) { outgoing.Enqueue(json); if (senderRunning) return; senderRunning = true; }
            _ = SendLoop(socket, cts.Token, generation);
        }

        async Task SendLoop(ClientWebSocket connection, CancellationToken token, int epoch)
        {
            try
            {
                while (true)
                {
                    string json; lock (outLock) { if (epoch != generation || stopped) return; if (outgoing.Count == 0) { senderRunning = false; return; } json = outgoing.Dequeue(); }
                    if (epoch != generation || stopped) return;
                    await connection.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, token);
                }
            }
            catch (Exception) { lock (outLock) { if (epoch == generation) senderRunning = false; } }
        }

        void SetState(string s) { State = s; StateChanged?.Invoke(s); }
        void SetMode(GuideMode m) { Mode = m; ModeChanged?.Invoke(m); }
        void OnDestroy() { End(); }
    }
}
