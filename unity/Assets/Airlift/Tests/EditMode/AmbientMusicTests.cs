using System;
using System.Linq;
using Airlift.Guide;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    public class AmbientMusicTests
    {
        [Test] public void TargetVolumeDucksUnderGuideSpeechAndOpenMic()
        {
            Assert.That(AmbientMusic.TargetVolume(enabled: false, guideSpeaking: false, micOpen: false), Is.EqualTo(0f));
            Assert.That(AmbientMusic.TargetVolume(true, false, false), Is.EqualTo(AmbientMusic.BaseVolume).Within(1e-5f));
            Assert.That(AmbientMusic.BaseVolume, Is.EqualTo(0.15f).Within(1e-5f), "about 15 percent");
            Assert.That(AmbientMusic.TargetVolume(true, true, false), Is.EqualTo(AmbientMusic.BaseVolume * AmbientMusic.DuckFactor).Within(1e-5f));
            Assert.That(AmbientMusic.TargetVolume(true, false, true), Is.EqualTo(AmbientMusic.BaseVolume * AmbientMusic.DuckFactor).Within(1e-5f));
            Assert.That(AmbientMusic.DuckFactor, Is.LessThan(0.5f).And.GreaterThan(0f));
        }

        [Test] public void SynthesizedLoopIsSeamlessAndNotSilent()
        {
            const int rate = 24000;
            float[] s = AmbientMusic.Synthesize(rate);
            Assert.That(s.Length, Is.EqualTo(rate * AmbientMusic.LoopSeconds));
            Assert.That(s.Any(float.IsNaN), Is.False);
            float peak = s.Max(x => Mathf.Abs(x));
            Assert.That(peak, Is.LessThanOrEqualTo(0.9f).And.GreaterThan(0.05f));
            double rms = System.Math.Sqrt(s.Sum(x => (double)x * x) / s.Length);
            Assert.That(rms, Is.GreaterThan(0.005));
            Assert.That(s.Take(200).Max(x => Mathf.Abs(x)), Is.LessThan(0.02f), "loop start is quiet");
            Assert.That(s.Skip(s.Length - 200).Max(x => Mathf.Abs(x)), Is.LessThan(0.02f), "loop end is quiet");
        }

        [Test] public void EnabledDefaultsToOnAndPersists()
        {
            PlayerPrefs.DeleteKey(AmbientMusic.PrefKey);
            try
            {
                Assert.That(AmbientMusic.LoadEnabled(), Is.True, "on by default");
                AmbientMusic.SaveEnabled(false); Assert.That(AmbientMusic.LoadEnabled(), Is.False);
                AmbientMusic.SaveEnabled(true); Assert.That(AmbientMusic.LoadEnabled(), Is.True);
            }
            finally { PlayerPrefs.DeleteKey(AmbientMusic.PrefKey); }
        }
    }
}
