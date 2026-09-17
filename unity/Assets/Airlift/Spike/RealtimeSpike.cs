using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;

// CC-P0-03 THROWAWAY: two-way voice with the OpenAI Realtime API on the Quest.
// Mints an ephemeral client secret from the LAN dev proxy, streams mic PCM16 24 kHz,
// plays guide audio, shows transcripts, and logs latency as [SPIKE] lines.
public sealed class RealtimeSpike : MonoBehaviour
{
    public string mintUrl = "http://192.168.86.20:8787/session";
    public TMP_Text status;
    public TMP_Text transcript;
    const int Rate = 24000;
    const string Model = "gpt-realtime";

    ClientWebSocket socket;
    CancellationTokenSource cts;
    AudioClip mic; int micPos; string micDevice;
    readonly float[] ring = new float[Rate * 30]; int ringWrite, ringRead; readonly object ringLock = new object();
    int outputRate; float resamplePos;
    readonly Queue<string> incoming = new Queue<string>(); readonly object inLock = new object();
    readonly StringBuilder guideText = new StringBuilder(); readonly StringBuilder userText = new StringBuilder();
    double speechStoppedAt = -1; int exchanges; double lastLatencyMs = -1; bool firstDeltaSeen; string state = "starting";
    static readonly System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();

    void Start()
    {
        outputRate = AudioSettings.outputSampleRate;
        var src = gameObject.AddComponent<AudioSource>(); src.loop = true; src.spatialBlend = 0f; src.Play();
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
            float until = Time.time + 20f;
            while (!Permission.HasUserAuthorizedPermission(Permission.Microphone) && Time.time < until) { Set("waiting for mic permission"); yield return null; }
        }
        Log("mic permission=" + Permission.HasUserAuthorizedPermission(Permission.Microphone));
        Set("minting session");
        string secret = null;
        using (var req = UnityWebRequest.PostWwwForm(mintUrl, ""))
        {
            req.timeout = 15; yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) { Set("mint failed: " + req.error); Log("mint failed " + req.error + " " + req.responseCode); yield break; }
            secret = Extract(req.downloadHandler.text, "value");
        }
        if (string.IsNullOrEmpty(secret)) { Set("mint returned no secret"); yield break; }
        Set("connecting");
        var connect = Connect(secret);
        while (!connect.IsCompleted) yield return null;
        if (connect.IsFaulted) { Set("connect failed: " + connect.Exception?.GetBaseException().Message); Log("connect failed " + connect.Exception?.GetBaseException()); yield break; }
        Set("connected");
        Send("{\"type\":\"session.update\",\"session\":{\"type\":\"realtime\",\"instructions\":\"You are Nerdy, a warm, brief learning guide. Keep replies to one or two short sentences.\",\"audio\":{\"input\":{\"format\":{\"type\":\"audio/pcm\",\"rate\":24000},\"turn_detection\":{\"type\":\"server_vad\",\"silence_duration_ms\":500},\"transcription\":{\"model\":\"gpt-4o-mini-transcribe\"}},\"output\":{\"format\":{\"type\":\"audio/pcm\",\"rate\":24000},\"voice\":\"marin\"}}}}");
        Send("{\"type\":\"response.create\",\"response\":{\"instructions\":\"Greet the tester in one sentence and ask how their day is going.\"}}");
        StartMic();
        _ = ReceiveLoop();
    }

    async Task Connect(string secret)
    {
        socket = new ClientWebSocket(); cts = new CancellationTokenSource();
        socket.Options.SetRequestHeader("Authorization", "Bearer " + secret);
        // No OpenAI-Beta header: it selects the retired beta shape (error beta_api_shape_disabled).
        await socket.ConnectAsync(new Uri("wss://api.openai.com/v1/realtime?model=" + Model), cts.Token);
    }

    void StartMic()
    {
        micDevice = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        mic = Microphone.Start(micDevice, true, 10, Rate);
        micPos = 0; Log("mic started device=" + (micDevice ?? "default") + " clipRate=" + (mic != null ? mic.frequency : -1));
    }

    void Update()
    {
        // Pump mic samples to the API in ~100 ms chunks.
        if (mic != null && socket != null && socket.State == WebSocketState.Open)
        {
            int pos = Microphone.GetPosition(micDevice); int count = pos - micPos; if (count < 0) count += mic.samples;
            if (count >= Rate / 10)
            {
                var samples = new float[count]; mic.GetData(samples, micPos); micPos = pos;
                var bytes = new byte[count * 2];
                for (int i = 0; i < count; i++) { short s = (short)Mathf.Clamp(samples[i] * 32767f, -32768f, 32767f); bytes[i * 2] = (byte)(s & 0xff); bytes[i * 2 + 1] = (byte)(s >> 8); }
                Send("{\"type\":\"input_audio_buffer.append\",\"audio\":\"" + Convert.ToBase64String(bytes) + "\"}");
            }
        }
        // Handle server events on the main thread.
        while (true)
        {
            string msg; lock (inLock) { if (incoming.Count == 0) break; msg = incoming.Dequeue(); }
            Handle(msg);
        }
        if (status != null) status.text = "state: " + state + "\nexchanges: " + exchanges + "  last latency: " + (lastLatencyMs < 0 ? "-" : lastLatencyMs.ToString("0") + " ms") + "\nsocket: " + (socket != null ? socket.State.ToString() : "none");
        if (transcript != null) transcript.text = "You: " + Tail(userText, 160) + "\n\nNerdy: " + Tail(guideText, 220);
    }

    readonly HashSet<string> seenTypes = new HashSet<string>();
    void Handle(string json)
    {
        string type = Extract(json, "type");
        if (seenTypes.Add(type)) Log("event " + type);
        switch (type)
        {
            case "input_audio_buffer.speech_started": state = "listening"; break;
            case "input_audio_buffer.speech_stopped": speechStoppedAt = clock.Elapsed.TotalMilliseconds; firstDeltaSeen = false; state = "thinking"; break;
            case "response.output_audio.delta":
            case "response.audio.delta":
                if (!firstDeltaSeen) { firstDeltaSeen = true; if (speechStoppedAt >= 0) { lastLatencyMs = clock.Elapsed.TotalMilliseconds - speechStoppedAt; exchanges++; Log("latency_ms=" + lastLatencyMs.ToString("0") + " exchange=" + exchanges); } }
                state = "speaking"; Enqueue(Convert.FromBase64String(Extract(json, "delta"))); break;
            case "response.output_audio_transcript.delta":
            case "response.audio_transcript.delta": guideText.Append(Extract(json, "delta")); break;
            case "response.output_audio_transcript.done":
            case "response.audio_transcript.done": guideText.Append("\n"); Log("guide said: " + Extract(json, "transcript")); break;
            case "conversation.item.input_audio_transcription.completed": { var t = Extract(json, "transcript"); userText.Append(t).Append("\n"); Log("user said: " + t); break; }
            case "response.done": state = "ready"; Log("response.done status=" + Extract(json, "status")); break;
            case "error": state = "error"; Log("error " + json); break;
            case "session.created": case "session.updated": Log(type); break;
        }
    }

    void Enqueue(byte[] pcm)
    {
        lock (ringLock)
        {
            for (int i = 0; i + 1 < pcm.Length; i += 2)
            {
                short s = (short)(pcm[i] | (pcm[i + 1] << 8));
                ring[ringWrite] = s / 32768f; ringWrite = (ringWrite + 1) % ring.Length;
            }
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        float step = (float)Rate / outputRate;
        lock (ringLock)
        {
            int available = (ringWrite - ringRead + ring.Length) % ring.Length;
            for (int frame = 0; frame < data.Length / channels; frame++)
            {
                float sample = 0f;
                if (available > 1)
                {
                    int i0 = ringRead, i1 = (ringRead + 1) % ring.Length;
                    sample = Mathf.Lerp(ring[i0], ring[i1], resamplePos);
                    resamplePos += step;
                    while (resamplePos >= 1f && available > 1) { resamplePos -= 1f; ringRead = (ringRead + 1) % ring.Length; available--; }
                }
                for (int c = 0; c < channels; c++) data[frame * channels + c] = sample;
            }
        }
    }

    async Task ReceiveLoop()
    {
        var buffer = new byte[1 << 16]; var sb = new StringBuilder();
        try
        {
            while (socket.State == WebSocketState.Open && !cts.IsCancellationRequested)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                if (result.MessageType == WebSocketMessageType.Close) { Log("socket closed " + result.CloseStatus); break; }
                sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                if (result.EndOfMessage) { var msg = sb.ToString(); sb.Clear(); lock (inLock) incoming.Enqueue(msg); }
            }
        }
        catch (Exception e) { Log("receive loop ended: " + e.GetBaseException().Message); }
    }

    readonly Queue<string> outgoing = new Queue<string>(); readonly object outLock = new object(); bool senderRunning;
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
                var bytes = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
            }
        }
        catch (Exception e) { Log("send failed " + e.GetBaseException().Message); lock (outLock) senderRunning = false; }
    }

    // Minimal JSON string field extraction (top-level or nested first occurrence); enough for the spike.
    static string Extract(string json, string key)
    {
        int k = json.IndexOf("\"" + key + "\":\"", StringComparison.Ordinal); if (k < 0) return "";
        int start = k + key.Length + 4; var sb = new StringBuilder();
        for (int i = start; i < json.Length; i++)
        {
            char c = json[i];
            if (c == '\\' && i + 1 < json.Length) { char n = json[++i]; sb.Append(n == 'n' ? '\n' : n == 't' ? '\t' : n); continue; }
            if (c == '"') break; sb.Append(c);
        }
        return sb.ToString();
    }
    static string Tail(StringBuilder sb, int n) { var s = sb.ToString(); return s.Length <= n ? s : s.Substring(s.Length - n); }
    void Set(string s) { state = s; Log(s); }
    static void Log(string s) { Debug.Log("[SPIKE] " + s); }
    void OnDestroy() { try { cts?.Cancel(); socket?.Dispose(); } catch { } if (mic != null) Microphone.End(micDevice); }
}
