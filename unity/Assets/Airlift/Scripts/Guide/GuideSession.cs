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
        readonly Queue<string> incoming = new Queue<string>(); readonly object inLock = new object();
        readonly Queue<string> outgoing = new Queue<string>(); readonly object outLock = new object(); bool senderRunning;
        double speechStoppedAt = -1; bool firstDeltaSeen; bool wasOpen; int reconnects;
        float lastGuideAudioAt = -10f; const float MicHoldAfterSpeechSeconds = 0.6f;
        /// Half-duplex: the Quest mic hears the Quest speaker, so never stream while the guide speaks.
        readonly PromptGate gate = new PromptGate();
        public bool MicGateOpen => micEnabled && (playback == null || !playback.IsSpeaking) && Time.time - lastGuideAudioAt > MicHoldAfterSpeechSeconds;

        public void Begin() { StartCoroutine(Run()); }

        public void End()
        {
            mic.Stop();
            try { cts?.Cancel(); socket?.Dispose(); } catch { }
            socket = null; SetState("ended");
        }

        public void SetMicEnabled(bool on) { micEnabled = on; if (!on) Send("{\"type\":\"input_audio_buffer.clear\"}"); }
        public bool MicEnabled => micEnabled;
        /// Stops the guide mid-sentence: cancel the active response (if any), drop queued prompts and audio.
        public void Hush() { if (gate.Cancel(Time.time)) Send("{\"type\":\"response.cancel\"}"); if (playback != null) playback.Clear(); }
        public void SendUserText(string text) { Send(GuideMessages.UserText(text)); Queue(null); }
        public void PushContext(string contextText) { Send(GuideMessages.SystemContext(contextText)); }
        public void SubmitToolResult(string callId, string outputJson) { Send(GuideMessages.ToolOutput(callId, outputJson)); Queue(null); }
        /// Queued behind any active response; the gate sends response.create when the server is free.
        public void Prompt(string instructions) { Queue(instructions); }
        void Queue(string instructions) { if (socket == null || socket.State != WebSocketState.Open) return; gate.Request(instructions, Time.time); }

        IEnumerator Run()
        {
            SetState("permission");
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
                float until = Time.time + 30f;
                while (!Permission.HasUserAuthorizedPermission(Permission.Microphone) && Time.time < until) yield return null;
            }
            bool micOk = Permission.HasUserAuthorizedPermission(Permission.Microphone);
            SetState("minting");
            string secret = null;
            if (!string.IsNullOrEmpty(mintUrl))
            {
                var body = "{\"launchNonce\":\"" + Guid.NewGuid().ToString("N") + "\",\"build\":\"" + build + "\"}";
                using (var req = new UnityWebRequest(mintUrl, "POST"))
                {
                    req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)); req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Content-Type", "application/json"); req.SetRequestHeader("x-nerdy-client", clientMarker); req.timeout = 12;
                    yield return req.SendWebRequest();
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
                var task = Connect(secret);
                while (!task.IsCompleted) yield return null;
                open = !task.IsFaulted && socket != null && socket.State == WebSocketState.Open;
                if (task.IsFaulted) Error?.Invoke("connect failed: " + task.Exception?.GetBaseException().Message);
            }
            SetMode(GuideRouting.Decide(micOk, minted, open, adultTesterOnlySatisfied));
            if (Mode == GuideMode.Live)
            {
                wasOpen = true; SetState("ready");
                if (micOk) mic.Start();
                _ = ReceiveLoop();
            }
            else SetState("offline");
        }

        async Task Connect(string secret)
        {
            socket = new ClientWebSocket(); cts = new CancellationTokenSource();
            socket.Options.SetRequestHeader("Authorization", "Bearer " + secret);
            await socket.ConnectAsync(new Uri("wss://api.openai.com/v1/realtime?model=" + Model), cts.Token);
        }

        void Update()
        {
            if (Mode == GuideMode.Live && socket != null && socket.State == WebSocketState.Open)
            {
                var chunk = mic.Poll(); // always drain so stale audio never accumulates
                if (chunk != null && MicGateOpen) Send(GuideMessages.AudioAppend(chunk, chunk.Length));
                if (gate.TryTake(Time.time, out var instructions)) Send(GuideMessages.ResponseCreate(instructions));
            }
            while (true)
            {
                string msg; lock (inLock) { if (incoming.Count == 0) break; msg = incoming.Dequeue(); }
                Handle(GuideMessages.Parse(msg));
            }
        }

        void Handle(GuideEvent e)
        {
            switch (e.Type)
            {
                case "input_audio_buffer.speech_started": SetState("listening"); break;
                case "response.created": gate.ResponseStarted(); Send("{\"type\":\"input_audio_buffer.clear\"}"); break;
                case "input_audio_buffer.speech_stopped": speechStoppedAt = clock.Elapsed.TotalMilliseconds; firstDeltaSeen = false; SetState("thinking"); break;
                case "response.output_audio.delta":
                    if (!firstDeltaSeen) { firstDeltaSeen = true; if (speechStoppedAt >= 0) LatencyMeasured?.Invoke(clock.Elapsed.TotalMilliseconds - speechStoppedAt); }
                    SetState("speaking"); lastGuideAudioAt = Time.time; if (playback != null) playback.Enqueue(Convert.FromBase64String(e.Delta)); break;
                case "response.output_audio_transcript.delta": GuideTranscriptDelta?.Invoke(e.Delta); break;
                case "response.output_audio_transcript.done": GuideTranscriptDone?.Invoke(e.Transcript); break;
                case "conversation.item.input_audio_transcription.completed": UserTranscriptDone?.Invoke(e.Transcript); break;
                case "response.function_call_arguments.done": ToolCall?.Invoke(e.ToolName, e.CallId, e.ArgumentsJson); break;
                case "response.done": gate.ResponseFinished(); SetState("ready"); break;
                case "error":
                    // A cancel that raced an already-finished response reports no active response: free the gate.
                    if (e.ErrorMessage != null && e.ErrorMessage.IndexOf("no active response", StringComparison.OrdinalIgnoreCase) >= 0) gate.ResponseFinished();
                    Error?.Invoke(e.ErrorMessage); break;
            }
        }

        async Task ReceiveLoop()
        {
            var buffer = new byte[1 << 16]; var sb = new StringBuilder();
            try
            {
                while (socket != null && socket.State == WebSocketState.Open && !cts.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close) break;
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    if (result.EndOfMessage) { var msg = sb.ToString(); sb.Clear(); lock (inLock) incoming.Enqueue(msg); }
                }
            }
            catch (Exception ex) { lock (inLock) incoming.Enqueue("{\"type\":\"error\",\"error\":{\"message\":\"receive: " + ex.GetBaseException().Message.Replace("\"", "'") + "\"}}"); }
            lock (inLock) incoming.Enqueue("{\"type\":\"socket.closed\"}");
            if (GuideRouting.ShouldReconnect(reconnects, wasOpen)) { reconnects++; UnityMainThread(() => StartCoroutine(Run())); }
            else UnityMainThread(() => { SetMode(GuideMode.Offline); SetState("offline"); });
        }

        readonly Queue<Action> mainThread = new Queue<Action>();
        void UnityMainThread(Action a) { lock (mainThread) mainThread.Enqueue(a); }
        void LateUpdate() { while (true) { Action a; lock (mainThread) { if (mainThread.Count == 0) break; a = mainThread.Dequeue(); } a(); } }

        void Send(string json)
        {
            if (socket == null || socket.State != WebSocketState.Open) return;
            lock (outLock) { outgoing.Enqueue(json); if (senderRunning) return; senderRunning = true; }
            _ = SendLoop();
        }

        async Task SendLoop()
        {
            try
            {
                while (true)
                {
                    string json; lock (outLock) { if (outgoing.Count == 0) { senderRunning = false; return; } json = outgoing.Dequeue(); }
                    await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json)), WebSocketMessageType.Text, true, cts.Token);
                }
            }
            catch (Exception) { lock (outLock) senderRunning = false; }
        }

        void SetState(string s) { State = s; StateChanged?.Invoke(s); }
        void SetMode(GuideMode m) { Mode = m; ModeChanged?.Invoke(m); }
        void OnDestroy() { End(); }
    }
}
