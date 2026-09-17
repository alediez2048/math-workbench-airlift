using System;
using UnityEngine;

namespace Airlift.Guide
{
    /// Captures the default microphone at 24 kHz and hands out PCM16 chunks of about 100 ms.
    public sealed class MicStreamer
    {
        public const int Rate = 24000;
        AudioClip clip; string device; int position;
        public bool IsRunning => clip != null;

        public void Start()
        {
            device = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
            clip = Microphone.Start(device, true, 10, Rate); position = 0;
        }

        public void Stop() { if (clip != null) Microphone.End(device); clip = null; }

        /// Returns a PCM16 chunk when at least 100 ms of new audio is available, else null.
        public byte[] Poll()
        {
            if (clip == null) return null;
            int now = Microphone.GetPosition(device); int count = now - position; if (count < 0) count += clip.samples;
            if (count < Rate / 10) return null;
            var samples = new float[count]; clip.GetData(samples, position); position = now;
            return GuideMessages.ToPcm16(samples, count);
        }
    }
}
