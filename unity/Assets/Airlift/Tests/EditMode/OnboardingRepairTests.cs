using System.Reflection;
using Airlift.Welcome;
using Airlift.Lounge;
using Airlift.Guide;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    public class OnboardingRepairTests
    {
        [Test] public void BeginDoesNotEnableMicrophoneAndRepeatedBeginDoesNothing()
        {
            var f = new WelcomeFlow(); f.Begin(); f.Begin();
            Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Welcome));
            Assert.That(f.VoiceConsented, Is.False);
            Assert.That(f.RundownSeen, Is.False);
        }

        [Test] public void CompleteProfileSkipsQuestionsEvenWhenTourUnseen()
        {
            var f = new WelcomeFlow();
            f.Profile.Merge("{\"ageBand\":\"adult\",\"interests\":[\"space\"],\"goal\":\"curious\"}");
            f.Begin();
            Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Catalog));
            Assert.That(f.RundownSeen || f.VoiceConsented, Is.False);
        }

        [Test] public void TourIsDueOnlyInCatalog()
        {
            var go = new GameObject("repair test");
            try
            {
                var n = go.AddComponent<NerdyDirector>(); n.rundown = go.AddComponent<LoungeRundown>();
                var due = typeof(NerdyDirector).GetProperty("TourDue", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(due.GetValue(n), Is.False);
                n.Flow.Begin(); Assert.That(due.GetValue(n), Is.False);
                n.Flow.EndWelcome(); Assert.That(due.GetValue(n), Is.True);
                n.Flow.MarkRundownSeen(); Assert.That(due.GetValue(n), Is.False);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test] public void MicrophoneRemainsOffUntilExplicitChoice()
        {
            var go = new GameObject("mic policy test");
            try
            {
                var n = go.AddComponent<NerdyDirector>(); n.guide = go.AddComponent<GuideSession>();
                n.Flow.Begin(); n.Flow.EndWelcome();
                var apply = typeof(NerdyDirector).GetMethod("ApplyMicPolicy", BindingFlags.Instance | BindingFlags.NonPublic);
                apply.Invoke(n, null); Assert.That(n.guide.MicEnabled, Is.False);
                n.Flow.Profile.ageBand = "adult";
                n.Flow.SetVoiceConsent(true); apply.Invoke(n, null); Assert.That(n.guide.MicEnabled, Is.True);
                n.Flow.SetVoiceConsent(false); apply.Invoke(n, null); Assert.That(n.guide.MicEnabled, Is.False);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test] public void EarlyLessonOpenIsBlockedUntilFinalGateOrSkip()
        {
            var r = new RundownScript(); r.Start();
            for (int i = 0; i < RundownScript.StopCount - 1; i++)
            {
                Assert.That(r.CanOpenLesson, Is.False);
                Assert.That(r.Report(RundownGate.LessonOpened), Is.False);
                r.Report(r.Current.Gate);
            }
            Assert.That(r.CanOpenLesson, Is.True);
            r.Start(); r.Skip(); Assert.That(r.CanOpenLesson, Is.True);
        }
    

        // Owner 2026-09-18: the small bar notice was missed; when the questions are skipped without an age answer the
        // caption (and Dee) say why she cannot listen. With no tour in the scene the caption stays as written.
        [Test] public void SkippingTheQuestionsWithoutAnAgeSaysWhyDeeCannotListen()
        {
            var go = new GameObject("skip notice test");
            try
            {
                var n = go.AddComponent<NerdyDirector>();
                n.captionText = new GameObject("caption", typeof(RectTransform)).AddComponent<TMPro.TextMeshProUGUI>();
                n.captionText.transform.SetParent(go.transform);
                n.Flow.Begin(); n.SkipQuestions();
                Assert.That(n.Flow.Phase, Is.EqualTo(WelcomePhase.Catalog));
                Assert.That(n.captionText.text, Does.Contain(GuidePolicy.AgeNotice));
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
