using UnityEngine;

namespace Airlift.Guide
{
    /// Plays streamed 24 kHz PCM through an AudioSource via a ring buffer with linear resampling.
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioPlayback : MonoBehaviour
    {
        public const int InputRate = 24000;
        readonly float[] ring = new float[InputRate * 30];
        int write, read; float pos; int outputRate;
        readonly object gate = new object();
        public bool IsSpeaking { get { lock (gate) return ((write - read + ring.Length) % ring.Length) > InputRate / 20; } }

        void Awake()
        {
            outputRate = AudioSettings.outputSampleRate;
            var src = GetComponent<AudioSource>(); src.loop = true; src.spatialBlend = 0f; if (!src.isPlaying) src.Play();
        }

        public void Enqueue(byte[] pcm16)
        {
            lock (gate) { int n = GuideMessages.FromPcm16(pcm16, ring, write); write = (write + n) % ring.Length; }
        }

        public void Clear() { lock (gate) { read = write; pos = 0f; } }

        void OnAudioFilterRead(float[] data, int channels)
        {
            float step = (float)InputRate / outputRate;
            lock (gate)
            {
                int available = (write - read + ring.Length) % ring.Length;
                for (int frame = 0; frame < data.Length / channels; frame++)
                {
                    float sample = 0f;
                    if (available > 1)
                    {
                        sample = Mathf.Lerp(ring[read], ring[(read + 1) % ring.Length], pos);
                        pos += step;
                        while (pos >= 1f && available > 1) { pos -= 1f; read = (read + 1) % ring.Length; available--; }
                    }
                    for (int c = 0; c < channels; c++) data[frame * channels + c] = sample;
                }
            }
        }
    }
}
