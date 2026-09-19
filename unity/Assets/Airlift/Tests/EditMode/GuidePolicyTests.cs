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
    

        // Owner 2026-09-18: "once I click the gear icon settings get stuck and none of the buttons work". The settings
        // card lives on the welcome canvas, whose ray surface is off during a lesson so it cannot steal presses from
        // the workbench; it has to come back for as long as the card is open.
        [Test] public void TheWelcomeCanvasIsPointableWhileSettingsAreOpenInsideALesson()
        {
            Assert.That(GuidePolicy.WelcomeRaySurfaceOn(WelcomePhase.Lesson, settingsOpen: false), Is.False, "the workbench card must win");
            Assert.That(GuidePolicy.WelcomeRaySurfaceOn(WelcomePhase.Lesson, settingsOpen: true), Is.True, "Done has to be pressable");
            Assert.That(GuidePolicy.WelcomeRaySurfaceOn(WelcomePhase.Catalog, settingsOpen: false), Is.True);
            Assert.That(GuidePolicy.WelcomeRaySurfaceOn(WelcomePhase.Consent, settingsOpen: false), Is.True);
        }

        // Owner 2026-09-18: Dee never heard "start the cargo lesson" because nothing turns the mic on for a new learner
        // once the mic pill and Mic button were retired. After the questions the mic comes on by itself for an adult
        // with the voice guide on; Stop still cuts it. Minors and "prefer not to say" stay off (adult testers only).
        [Test] public void TheMicComesOnAfterTheQuestionsForAdultsOnly()
        {
            Assert.That(GuidePolicy.MicAutoOnAfterQuestions("adult", voiceGuide: true, paused: false), Is.True);
            Assert.That(GuidePolicy.MicAutoOnAfterQuestions("prefer_not_to_say", voiceGuide: true, paused: false), Is.False);
            Assert.That(GuidePolicy.MicAutoOnAfterQuestions("14_to_17", voiceGuide: true, paused: false), Is.False);
            Assert.That(GuidePolicy.MicAutoOnAfterQuestions("", voiceGuide: true, paused: false), Is.False, "skipped questions");
            Assert.That(GuidePolicy.MicAutoOnAfterQuestions("adult", voiceGuide: false, paused: false), Is.False, "Voice guide off in settings");
            Assert.That(GuidePolicy.MicAutoOnAfterQuestions("adult", voiceGuide: true, paused: true), Is.False, "Stop means stopped");
        }
    

        // Owner 2026-09-18: "updated the language to spanish and Dee kept speaking to me in english". The session is
        // minted and locked in one language, so a change while Dee is connected has to reconnect her; while she is
        // offline or stopped the new language simply waits for the next Play.
        [Test] public void ALanguageChangeReconnectsDeeOnlyWhileSheIsConnected()
        {
            Assert.That(GuidePolicy.ReconnectForLanguage("en", "es", connected: true, paused: false), Is.True, "live, or a mint in flight in the old language");
            Assert.That(GuidePolicy.ReconnectForLanguage("en", "en", connected: true, paused: false), Is.False, "same language: nothing to do");
            Assert.That(GuidePolicy.ReconnectForLanguage("en", "es", connected: false, paused: false), Is.False, "Begin will mint in the new language");
            Assert.That(GuidePolicy.ReconnectForLanguage("en", "es", connected: true, paused: true), Is.False, "Stop means stopped; Play reconnects in the new language");
        }
    

        // Owner 2026-09-18 ("keep the gate, add a clear notice"): when Dee cannot listen, the bar says why in one line.
        [Test] public void TheBarSaysWhyDeeCannotListen()
        {
            Assert.That(GuidePolicy.MicNotice("", voiceGuide: true, paused: false), Is.EqualTo(GuidePolicy.AgeNotice));
            Assert.That(GuidePolicy.MicNotice("prefer_not_to_say", voiceGuide: true, paused: false), Is.EqualTo(GuidePolicy.AdultsOnlyNotice));
            Assert.That(GuidePolicy.MicNotice("under_10", voiceGuide: true, paused: false), Is.EqualTo(GuidePolicy.AdultsOnlyNotice));
            Assert.That(GuidePolicy.MicNotice("adult", voiceGuide: true, paused: false), Is.EqualTo(""), "nothing to explain");
            Assert.That(GuidePolicy.MicNotice("", voiceGuide: false, paused: false), Is.EqualTo(GuidePolicy.VoiceOffNotice));
            Assert.That(GuidePolicy.MicNotice("", voiceGuide: true, paused: true), Is.Null, "Stop has its own Play notice");
        }
    }
}
