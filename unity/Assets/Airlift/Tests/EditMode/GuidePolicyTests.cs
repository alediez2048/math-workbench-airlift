using Airlift.Welcome;
using NUnit.Framework;

namespace Airlift.Tests
{
    public class GuidePolicyTests
    {
        // Owner 2026-09-17: the guide is Dee and the story starts with this introduction, spoken word for word.
        [Test] public void DeeIntroducesNerdyAiVrWithTheOwnerApprovedWords()
        {
            const string intro = "Welcome to Nerdy AI plus VR! My name is Dee, and I will be your AI assistant throughout your elementary math journey.";
            Assert.That(GuideIntro.Spoken, Does.StartWith(intro));
            Assert.That(GuideIntro.GreetingPrompt, Does.Contain("word for word"));
            Assert.That(GuideIntro.GreetingPrompt, Does.Contain(GuideIntro.Spoken));
            Assert.That(GuideIntro.OfflineCaption, Does.StartWith("Welcome to Nerdy AI+VR! My name is Dee"));
            Assert.That(GuideIntro.OfflineCaption, Does.Contain("Tap the answers below"));
        }

        [Test] public void MicStaysOffDuringConsentAndChipQuestions()
        {
            Assert.That(GuidePolicy.MicOn(WelcomePhase.Consent, muted: false, paused: false), Is.False);
            Assert.That(GuidePolicy.MicOn(WelcomePhase.Welcome, muted: false, paused: false), Is.False);
        }

        [Test] public void MicIsOnInCatalogAndLessonUnlessMutedOrPaused()
        {
            Assert.That(GuidePolicy.MicOn(WelcomePhase.Catalog, false, false), Is.True);
            Assert.That(GuidePolicy.MicOn(WelcomePhase.Lesson, false, false), Is.True);
            Assert.That(GuidePolicy.MicOn(WelcomePhase.Lesson, muted: true, paused: false), Is.False);
            Assert.That(GuidePolicy.MicOn(WelcomePhase.Lesson, muted: false, paused: true), Is.False);
        }

        [Test] public void PromptsAreBlockedWhilePausedOrOffline()
        {
            Assert.That(GuidePolicy.CanPrompt(paused: false, live: true), Is.True);
            Assert.That(GuidePolicy.CanPrompt(paused: true, live: true), Is.False);
            Assert.That(GuidePolicy.CanPrompt(paused: false, live: false), Is.False);
        }

        [Test] public void FallbackButtonsHideOnlyWhileTheLiveGuideListens()
        {
            Assert.That(GuidePolicy.ShowFallbackButtons(live: true, micOn: true), Is.False, "voice-first while listening");
            Assert.That(GuidePolicy.ShowFallbackButtons(live: false, micOn: true), Is.True, "offline or mint down");
            Assert.That(GuidePolicy.ShowFallbackButtons(live: true, micOn: false), Is.True, "muted or paused");
            Assert.That(GuidePolicy.ShowFallbackButtons(live: false, micOn: false), Is.True);
        }

        [Test] public void MuteAndPauseInTheLessonBringTheButtonsBack()
        {
            Assert.That(GuidePolicy.ShowFallbackButtons(true, GuidePolicy.MicOn(WelcomePhase.Lesson, muted: false, paused: false)), Is.False);
            Assert.That(GuidePolicy.ShowFallbackButtons(true, GuidePolicy.MicOn(WelcomePhase.Lesson, muted: true, paused: false)), Is.True);
            Assert.That(GuidePolicy.ShowFallbackButtons(true, GuidePolicy.MicOn(WelcomePhase.Lesson, muted: false, paused: true)), Is.True);
            Assert.That(GuidePolicy.ShowFallbackButtons(false, GuidePolicy.MicOn(WelcomePhase.Lesson, false, false)), Is.True, "continue without voice");
        }
    }
}
