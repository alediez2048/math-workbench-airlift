using System;
using UnityEngine;

namespace Airlift.Guide
{
    /// Synthesised ambient loop (no audio asset): four soft chords with a gentle pentatonic sparkle.
    /// Quiet by design, ducks under guide speech and while the mic is streaming, HUD toggle,
    /// remembered in PlayerPrefs, on by default.
    [RequireComponent(typeof(AudioSource))]
    public sealed class AmbientMusic : MonoBehaviour
    {
        public const float BaseVolume = 0.15f;
        public const float DuckFactor = 0.3f;
        public const int LoopSeconds = 16;
        public const string PrefKey = "nerdy.music";
        public const float FadeSpeed = 0.6f;    // volume units per second

        public GuideSession guide;
        public AudioPlayback playback;
        public bool Enabled { get; private set; }
        /// CC-FD-09: the settings card's music volume, 1 = the 40 % default.
        [System.NonSerialized] public float volumeScale = 1f;
        AudioSource source;

        public static float TargetVolume(bool enabled, bool guideSpeaking, bool micOpen) =>
            !enabled ? 0f : (guideSpeaking || micOpen) ? BaseVolume * DuckFactor : BaseVolume;

        public static bool LoadEnabled() => PlayerPrefs.GetInt(PrefKey, 1) == 1;
        public static void SaveEnabled(bool on) { PlayerPrefs.SetInt(PrefKey, on ? 1 : 0); PlayerPrefs.Save(); }
        public void SetEnabled(bool on) { Enabled = on; SaveEnabled(on); }

        void Awake()
        {
            Enabled = LoadEnabled();
            source = GetComponent<AudioSource>();
            const int rate = 24000;
            var clip = AudioClip.Create("Nerdy ambient loop", rate * LoopSeconds, 1, rate, false);
            clip.SetData(Synthesize(rate), 0);
            source.clip = clip; source.loop = true; source.spatialBlend = 0f; source.playOnAwake = false; source.volume = 0f;
            source.Play();
        }

        void Update()
        {
            if (source == null) return;
            bool speaking = playback != null && playback.IsSpeaking;
            bool micOpen = guide != null && guide.MicStreaming;
            source.volume = Mathf.MoveTowards(source.volume, TargetVolume(Enabled, speaking, micOpen) * Mathf.Clamp(volumeScale, 0f, 2.5f), Time.deltaTime * FadeSpeed);
        }

        /// Deterministic mono loop. Chords: Cmaj7 · Am7 · Fmaj7 · G6, four seconds each, raised-cosine
        /// attack/release so the loop seam is silent; seeded plucks on a C pentatonic.
        public static float[] Synthesize(int rate)
        {
            int n = rate * LoopSeconds; var s = new float[n];
            float[][] chords = {
                new[] { 130.81f, 164.81f, 196.00f, 246.94f },   // C E G B
                new[] { 110.00f, 130.81f, 164.81f, 196.00f },   // A C E G
                new[] { 87.31f, 110.00f, 130.81f, 164.81f },    // F A C E
                new[] { 98.00f, 123.47f, 146.83f, 164.81f } };  // G B D E
            const float seg = 4f, edge = 0.6f;
            for (int c = 0; c < chords.Length; c++)
            {
                int start = (int)(c * seg * rate), end = Mathf.Min(n, (int)((c + 1) * seg * rate));
                for (int i = start; i < end; i++)
                {
                    float t = (i - start) / (float)rate;
                    float env = t < edge ? 0.5f - 0.5f * Mathf.Cos(Mathf.PI * t / edge) : t > seg - edge ? 0.5f - 0.5f * Mathf.Cos(Mathf.PI * (seg - t) / edge) : 1f;
                    float tremolo = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * 0.17f * (i / (float)rate));
                    float v = 0f; float tt = i / (float)rate;
                    for (int k = 0; k < chords[c].Length; k++)
                    {
                        float f = chords[c][k];
                        v += Mathf.Sin(2f * Mathf.PI * f * tt) * 0.22f;
                        v += Mathf.Sin(2f * Mathf.PI * (f + 0.35f) * tt) * 0.12f;      // slow beat for warmth
                        v += Mathf.Sin(2f * Mathf.PI * f * 2f * tt) * 0.05f;           // faint octave
                    }
                    s[i] += v * env * tremolo;
                }
            }
            float[] penta = { 523.25f, 587.33f, 659.25f, 783.99f, 880.00f };
            var rng = new System.Random(2026);
            for (float at = 0.4f; at < LoopSeconds - 0.8f; at += 0.75f)
            {
                if (rng.NextDouble() < 0.35) continue;
                float f = penta[rng.Next(penta.Length)]; int start = (int)(at * rate); int len = (int)(0.5f * rate);
                for (int i = 0; i < len && start + i < n; i++)
                {
                    float t = i / (float)rate; float env = Mathf.Exp(-t * 7f) * (t < 0.01f ? t / 0.01f : 1f);
                    s[start + i] += Mathf.Sin(2f * Mathf.PI * f * t) * env * 0.16f;
                }
            }
            float peak = 0f; for (int i = 0; i < n; i++) peak = Mathf.Max(peak, Mathf.Abs(s[i]));
            if (peak > 0f) { float g = 0.6f / peak; for (int i = 0; i < n; i++) s[i] *= g; }
            return s;
        }
    }
}
