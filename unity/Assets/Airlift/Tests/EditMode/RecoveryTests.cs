using Airlift.Welcome;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    /// Lock item L-2 (CC-P1-09 / CC-P3-03): interruptions leave the lesson and the guide coherent.
    public class RecoveryTests
    {
        [Test] public void WhilePausedAVoiceActionDoesNothingAndTheGuideStaysSilent()
        {
            Assert.That(GuidePolicy.SpeakAfterToolResult(paused: true), Is.False);
            Assert.That(GuidePolicy.SpeakAfterToolResult(paused: false), Is.True);
            var o = JObject.Parse(GuideTools.PausedResult);
            Assert.That((bool)o["ok"], Is.False);
            Assert.That((string)o["reason"], Does.Contain("paused"));
        }

        static NerdyDirector LessonDirector(out GameObject go)
        {
            go = new GameObject("nerdy (test)");
            var n = go.AddComponent<NerdyDirector>();
            n.Flow.Consent(false); n.Flow.EndWelcome(); n.Flow.OpenLesson("cargo_crew_fractions");
            return n;
        }

        [Test] public void TakingTheHeadsetOffPausesTheGuideUntilPlayIsPressed()
        {
            var n = LessonDirector(out var go);
            try
            {
                n.HandleAppPause(true);
                Assert.That(n.Paused, Is.True, "headset off pauses the guide");
                n.HandleAppPause(false);
                Assert.That(n.Paused, Is.True, "putting the headset back on does not start talking by itself");
                n.TogglePause();
                Assert.That(n.Paused, Is.False, "Play resumes");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test] public void HeadsetOffNeverUnpausesAGuideTheLearnerPaused()
        {
            var n = LessonDirector(out var go);
            try
            {
                n.TogglePause(); Assert.That(n.Paused, Is.True);
                n.HandleAppPause(true); n.HandleAppPause(false);
                Assert.That(n.Paused, Is.True);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test] public void HeadsetOffDuringConsentChangesNothing()
        {
            var go = new GameObject("nerdy consent (test)"); var n = go.AddComponent<NerdyDirector>();
            try { n.HandleAppPause(true); Assert.That(n.Paused, Is.False); }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
