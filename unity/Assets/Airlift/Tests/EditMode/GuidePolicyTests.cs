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
            Assert.That(GuideIntro.Spoken, Does.Contain("Point at Let's begin and press the trigger.").And.Not.Contain("mouse"));
            Assert.That(GuideIntro.SpokenSpanish, Does.Not.Contain("ratón").And.Not.Contain("mouse"));
            Assert.That(GuideIntro.GreetingPrompt, Does.Contain("word for word"));
            Assert.That(GuideIntro.GreetingPrompt, Does.Contain(GuideIntro.Spoken));
            Assert.That(GuideIntro.OfflineCaption, Does.StartWith("Welcome to Nerdy AI+VR! My name is Dee"));
            Assert.That(GuideIntro.OfflineCaption, Does.Contain("Let's begin").And.Not.Contain("answers"));
        }

        // Owner 2026-09-17: Dee greets on arrival, so her bar has to be there from the first card — a voice with
        // no Mute, Pause or Help in reach is worse than no voice.
        [Test] public void TheAssistantBarIsThereFromTheFirstCard()
        {
            Assert.That(GuidePolicy.AssistantBarVisible(WelcomePhase.Consent, arriving: false), Is.True);
            Assert.That(GuidePolicy.AssistantBarVisible(WelcomePhase.Welcome, arriving: false), Is.True);
            Assert.That(GuidePolicy.AssistantBarVisible(WelcomePhase.Lesson, arriving: false), Is.True);
        }

        [Test] public void TheAssistantBarWaitsForTheLogoToFinish()
        {
            Assert.That(GuidePolicy.AssistantBarVisible(WelcomePhase.Consent, arriving: true), Is.False);
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

        [Test] public void MicListensDuringTheRundownSoStopFourCanHearHello()
        {
            Assert.That(GuidePolicy.MicOn(WelcomePhase.Rundown, false, false), Is.True);
            Assert.That(GuidePolicy.MicOn(WelcomePhase.Rundown, muted: true, paused: false), Is.False);
        }

        // CC-FD-09: Voice guide off in settings means Dee neither listens nor speaks; captions stay on the bar.
        [Test] public void VoiceGuideOffSilencesDeeCompletely()
        {
            Assert.That(GuidePolicy.MicOn(WelcomePhase.Lesson, false, false, voiceGuide: false), Is.False);
            Assert.That(GuidePolicy.MicOn(WelcomePhase.Lesson, false, false, voiceGuide: true), Is.True);
            Assert.That(GuidePolicy.CanPrompt(paused: false, live: true, voiceGuide: false), Is.False);
            Assert.That(GuidePolicy.CanPrompt(paused: false, live: true, voiceGuide: true), Is.True);
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
