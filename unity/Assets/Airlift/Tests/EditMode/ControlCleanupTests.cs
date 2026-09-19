using System.Reflection;
using Airlift.Guide;
using Airlift.Lounge;
using Airlift.Welcome;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Airlift.Tests
{
    public class ControlCleanupTests
    {
        static void Invoke(object o, string name, params object[] args) => o.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(o, args);

        [TestCase("adult", "Play enables microphone")]
        [TestCase("", "Microphone stays off")]
        public void PlayLabelIsSimpleAndMicrophoneNoticeIsSeparate(string age, string notice)
        {
            var go = new GameObject("play label");
            try {
                var n = go.AddComponent<NerdyDirector>();
                n.conversationLabel = new GameObject("label", typeof(RectTransform)).AddComponent<TMPro.TextMeshProUGUI>();
                n.conversationLabel.transform.SetParent(go.transform);
                n.conversationNotice = new GameObject("notice", typeof(RectTransform)).AddComponent<TMPro.TextMeshProUGUI>();
                n.conversationNotice.transform.SetParent(go.transform);
                n.Flow.Begin(); n.Flow.EndWelcome(); n.Flow.Profile.ageBand = age;
                n.TogglePause();
                Assert.That(n.conversationLabel.text, Is.EqualTo("▶ Play"));
                Assert.That(n.conversationNotice.text, Is.EqualTo(notice));
                n.TogglePause();
                Assert.That(n.conversationLabel.text, Is.EqualTo("■ Stop"));
                // Owner 2026-09-18: while Dee cannot listen the notice says why (no age answer yet); adults get no notice.
                Assert.That(n.conversationNotice.text, Is.EqualTo(age == "adult" ? "" : GuidePolicy.AgeNotice));
            } finally { Object.DestroyImmediate(go); }
        }

        [TestCase("")] [TestCase("prefer_not_to_say")] [TestCase("under_10")] [TestCase("14_to_17")]
        public void CaptureRequiresPositivelyAdultProfile(string age)
        {
            var go = new GameObject("capture eligibility");
            try {
                var n = go.AddComponent<NerdyDirector>(); n.guide = go.AddComponent<GuideSession>();
                n.Flow.Begin(); n.Flow.EndWelcome(); n.Flow.Profile.ageBand = age;
                n.Flow.SetVoiceConsent(true); Invoke(n, "ApplyMicPolicy");
                Assert.That(n.guide.MicEnabled, Is.False);
            } finally { Object.DestroyImmediate(go); }
        }

        [Test] public void StopRejectsDelayedToolAndTranscript()
        {
            var go = new GameObject("stop seam");
            try {
                var n = go.AddComponent<NerdyDirector>(); n.guide = go.AddComponent<GuideSession>();
                int calls = 0; n.guide.ToolCall += (_, __, ___) => calls++;
                string heard = null; n.guide.GuideTranscriptDone += t => heard = t;
                n.TogglePause();
                Invoke(n.guide, "Handle", new GuideEvent { Type = "response.function_call_arguments.done", ToolName = "open_lesson", CallId = "late", ArgumentsJson = "{}" });
                Invoke(n.guide, "Handle", new GuideEvent { Type = "response.output_audio_transcript.done", ResponseId = "late", Transcript = "Old instruction" });
                Assert.That(calls, Is.Zero); Assert.That(heard, Is.Null);
                Assert.That(n.guide.MicEnabled, Is.False);
            } finally { Object.DestroyImmediate(go); }
        }

        [Test] public void SharedCornerArrowsStayHiddenInCatalog()
        {
            var go = new GameObject("corner controls");
            try {
                var n = go.AddComponent<NerdyDirector>();
                n.navNext = new GameObject("old Next", typeof(RectTransform), typeof(Button)).GetComponent<Button>(); n.navNext.transform.SetParent(go.transform);
                n.navBack = new GameObject("old Back", typeof(RectTransform), typeof(Button)).GetComponent<Button>(); n.navBack.transform.SetParent(go.transform);
                n.Flow.Begin(); n.Flow.EndWelcome(); Invoke(n, "RefreshNavArrows");
                Assert.That(n.navNext.gameObject.activeSelf || n.navBack.gameObject.activeSelf, Is.False);
            } finally { Object.DestroyImmediate(go); }
        }

        [Test] public void TourNeverRequiresRetiredCornerArrow()
        {
            foreach (var step in RundownScript.Steps) {
                Assert.That(step.Target, Is.Not.EqualTo(TourTarget.NextArrow).And.Not.EqualTo(TourTarget.BackArrow));
                Assert.That(step.Gate, Is.Not.EqualTo(RundownGate.ContinuePressed));
            }
        }

        [Test] public void StopInvalidatesOldConnectionEvenAfterResume()
        {
            var go = new GameObject("generation seam");
            try {
                var guide = go.AddComponent<GuideSession>(); int old = guide.ConnectionGeneration;
                int calls = 0; guide.ToolCall += (_, __, ___) => calls++;
                guide.End();
                Assert.That(guide.ConnectionGeneration, Is.GreaterThan(old));
                // Simulate the active state of the next connection without contacting a provider.
                typeof(GuideSession).GetField("stopped", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(guide, false);
                Invoke(guide, "HandleIncoming", old, "{\"type\":\"response.function_call_arguments.done\",\"name\":\"open_lesson\",\"arguments\":\"{}\"}");
                Assert.That(calls, Is.Zero);
                Invoke(guide, "HandleIncoming", guide.ConnectionGeneration, "{\"type\":\"response.function_call_arguments.done\",\"name\":\"open_settings\",\"arguments\":\"{}\"}");
                Assert.That(calls, Is.EqualTo(1));
            } finally { Object.DestroyImmediate(go); }
        }

        [TestCase("adult", true)] [TestCase("", false)] [TestCase("prefer_not_to_say", false)] [TestCase("10_to_13", false)]
        public void ExplicitPlayOnlyGrantsEligibleConsent(string age, bool expected)
        {
            var go = new GameObject("play consent");
            try {
                var n = go.AddComponent<NerdyDirector>(); n.Flow.Begin(); n.Flow.EndWelcome(); n.Flow.Profile.ageBand = age;
                n.TogglePause(); Assert.That(n.Flow.VoiceConsented, Is.False);
                n.TogglePause(); Assert.That(n.Flow.VoiceConsented, Is.EqualTo(expected));
                Assert.That(n.Flow.Phase, Is.EqualTo(WelcomePhase.Catalog)); Assert.That(n.Flow.RundownSeen, Is.False);
            } finally { Object.DestroyImmediate(go); }
        }
    }
}
