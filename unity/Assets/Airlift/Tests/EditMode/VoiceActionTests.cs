using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Welcome;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Airlift.Tests
{
    /// Phase 1R voice-first workbench: every lesson action can be spoken, each tool is state-gated with a
    /// one-sentence reason, and the tool output (not the model) carries what happened.
    public class VoiceActionTests
    {
        static readonly OnboardingStage[] AllStages =
            { OnboardingStage.Catalog, OnboardingStage.Overview, OnboardingStage.Orientation, OnboardingStage.Demonstration, OnboardingStage.Practice, OnboardingStage.Ready };

        [Test] public void LessonToolNamesMatchTheProxy()
        {
            Assert.That(GuideTools.LessonToolNames, Is.EqualTo(new[] { "advance_step", "replay_demo", "split_cargo", "check_load", "reset_cargo", "next_chapter", "restart_chapter", "back_to_lessons" }));
        }

        [Test] public void AdvanceStepMovesOnboardingThenActsAsNextChapter()
        {
            Assert.That(GuideTools.AdvanceStep(WelcomePhase.Lesson, chapterActive: false, primaryAvailable: true, holding: false), Is.EqualTo(AdvanceRoute.OnboardingStep));
            Assert.That(GuideTools.AdvanceStep(WelcomePhase.Lesson, chapterActive: true, primaryAvailable: false, holding: false), Is.EqualTo(AdvanceRoute.NextChapter));
            Assert.That(GuideTools.AdvanceStep(WelcomePhase.Lesson, false, primaryAvailable: false, holding: false), Is.EqualTo(AdvanceRoute.Refuse), "demo playing or practice");
            Assert.That(GuideTools.AdvanceStep(WelcomePhase.Lesson, false, true, holding: true), Is.EqualTo(AdvanceRoute.Refuse), "practice crate in hand");
            Assert.That(GuideTools.AdvanceStep(WelcomePhase.Catalog, true, true, false), Is.EqualTo(AdvanceRoute.Refuse));
        }

        [Test] public void AdvanceRefusalsSayWhatIsLeft()
        {
            Assert.That(GuideTools.AdvanceRefusal(WelcomePhase.Catalog, OnboardingStage.Catalog, false), Is.EqualTo(GuideTools.NoLesson));
            Assert.That(GuideTools.AdvanceRefusal(WelcomePhase.Lesson, OnboardingStage.Practice, false), Is.EqualTo(GuideTools.FinishPractice));
            Assert.That(GuideTools.AdvanceRefusal(WelcomePhase.Lesson, OnboardingStage.Demonstration, false), Does.Contain("demo"));
            Assert.That(GuideTools.AdvanceRefusal(WelcomePhase.Lesson, OnboardingStage.Ready, true), Is.EqualTo(GuideTools.LetGo));
        }

        [Test] public void ReplayDemoOnlyInPracticeOrReadyWithNothingHeld()
        {
            foreach (var stage in AllStages)
            {
                var gate = GuideTools.ReplayDemo(WelcomePhase.Lesson, chapterActive: false, stage, holding: false);
                bool expected = stage == OnboardingStage.Practice || stage == OnboardingStage.Ready;
                Assert.That(gate.Ok, Is.EqualTo(expected), stage.ToString());
                if (!gate.Ok) Assert.That(gate.Reason, Is.Not.Empty, stage + " refusal has a reason");
            }
            Assert.That(GuideTools.ReplayDemo(WelcomePhase.Lesson, false, OnboardingStage.Practice, holding: true).Reason, Is.EqualTo(GuideTools.LetGo));
            Assert.That(GuideTools.ReplayDemo(WelcomePhase.Lesson, chapterActive: true, OnboardingStage.Ready, false).Ok, Is.False, "no demo over a chapter");
            Assert.That(GuideTools.ReplayDemo(WelcomePhase.Catalog, false, OnboardingStage.Ready, false).Reason, Is.EqualTo(GuideTools.NoLesson));
            Assert.That(GuideTools.ReplayDemo(WelcomePhase.Lesson, false, OnboardingStage.Demonstration, false).Reason, Is.EqualTo(GuideTools.DemoPlaying));
        }

        [Test] public void ChapterToolsNeedAChapterOnTheTable()
        {
            Assert.That(GuideTools.ChapterTool(WelcomePhase.Lesson, chapterActive: true, OnboardingStage.Ready).Ok, Is.True);
            Assert.That(GuideTools.ChapterTool(WelcomePhase.Lesson, false, OnboardingStage.Ready).Reason, Is.EqualTo(GuideTools.SayStartLoading));
            Assert.That(GuideTools.ChapterTool(WelcomePhase.Lesson, false, OnboardingStage.Overview).Reason, Is.EqualTo(GuideTools.ChaptersNotStarted));
            Assert.That(GuideTools.ChapterTool(WelcomePhase.Catalog, true, OnboardingStage.Catalog).Reason, Is.EqualTo(GuideTools.NoLesson));
            foreach (var phase in new[] { WelcomePhase.Consent, WelcomePhase.Welcome, WelcomePhase.Catalog })
                Assert.That(GuideTools.ChapterTool(phase, true, OnboardingStage.Ready).Ok, Is.False, phase.ToString());
        }

        [Test] public void BackToLessonsRefusesWhileHeldOrOutsideALesson()
        {
            Assert.That(GuideTools.BackToLessons(WelcomePhase.Lesson, holding: false).Ok, Is.True);
            Assert.That(GuideTools.BackToLessons(WelcomePhase.Lesson, holding: true).Reason, Is.EqualTo(GuideTools.LetGo));
            Assert.That(GuideTools.BackToLessons(WelcomePhase.Catalog, false).Reason, Is.EqualTo(GuideTools.CardsAlreadyShowing));
            Assert.That(GuideTools.BackToLessons(WelcomePhase.Welcome, false).Ok, Is.False);
        }

        [Test] public void RefusalReasonsAreOneShortSpokenSentence()
        {
            foreach (var r in new[] { GuideTools.LetGo, GuideTools.NoLesson, GuideTools.CardsAlreadyShowing, GuideTools.ChaptersNotStarted, GuideTools.SayStartLoading,
                GuideTools.DemoOnlyInPractice, GuideTools.DemoAfterBriefing, GuideTools.DemoPlaying, GuideTools.FinishPractice, GuideTools.NoNextStep })
            {
                Assert.That(r.Length, Is.LessThan(110), r);
                Assert.That(r, Does.EndWith("."), r);
                Assert.That(r, Does.Not.Contain("strap"), r);
                foreach (char ch in r) Assert.That(ch < 128, Is.True, "ASCII only: " + r);
            }
        }

        [Test] public void ToolResultCarriesOkReasonStepAndChapterFacts()
        {
            var chapter = CargoChapter.All[2];
            var step = GuideSteps.ForChapter(chapter, true, false);
            var o = JObject.Parse(GuideTools.ToolResult(true, "", step, "Four delivery vans pulled in.", chapter, true, chapter.Expression, "All four vans are loaded.",
                new JObject { ["say"] = "one line" }));
            Assert.That((bool)o["ok"], Is.True);
            Assert.That((string)o["reason"], Is.EqualTo(""));
            Assert.That((string)o["say"], Is.EqualTo("one line"));
            Assert.That((string)o["step"], Is.EqualTo("chapter3_four_vans"));
            Assert.That((string)o["on_table_now"], Is.EqualTo(step.OnTableNow));
            Assert.That((bool)o["can_grab_now"], Is.True);
            Assert.That((string)o["instruction"], Is.EqualTo("Four delivery vans pulled in."));
            Assert.That((int)o["chapter"], Is.EqualTo(3));
            Assert.That((string)o["chapter_title"], Is.EqualTo(chapter.Title));
            Assert.That((string)o["story"], Is.EqualTo(chapter.Story));
            Assert.That((string)o["task"], Is.EqualTo(chapter.Task));
            Assert.That((bool)o["chapter_complete"], Is.True);
            Assert.That((string)o["expression"], Is.EqualTo("1/4 + 1/4 + 1/4 + 1/4 = 1"));
            Assert.That((string)o["feedback"], Is.EqualTo("All four vans are loaded."));
        }

        [Test] public void RefusedToolResultHasNoVerdictAndOnboardingResultHasNoChapter()
        {
            var step = GuideSteps.ForOnboarding(OnboardingStage.Practice);
            var o = JObject.Parse(GuideTools.ToolResult(false, GuideTools.SayStartLoading, step, "Touch the orange crate."));
            Assert.That((bool)o["ok"], Is.False);
            Assert.That((string)o["reason"], Is.EqualTo(GuideTools.SayStartLoading));
            Assert.That(o["chapter"], Is.Null);
            Assert.That(o["expression"], Is.Null);
            Assert.That((string)o["step"], Is.EqualTo("practice"));

            var refused = JObject.Parse(GuideTools.ToolResult(false, "Load this container correctly first.", GuideSteps.ForChapter(CargoChapter.All[0], false, false), "", CargoChapter.All[0], false, "", null));
            Assert.That((string)refused["expression"], Is.EqualTo(""), "no expression until the app accepts the load");
            Assert.That((bool)refused["chapter_complete"], Is.False);
            Assert.That((string)refused["feedback"], Is.EqualTo(""), "null feedback is an empty string");
        }

        [Test] public void StoryLineForButtonDrivenChapterChanges()
        {
            var ch = CargoChapter.All;
            Assert.That(GuideTools.StoryLine(0, false, ch[0], false), Is.EqualTo(ch[0].Story), "chapter 1 starts from Start fractions");
            Assert.That(GuideTools.StoryLine(1, false, ch[0], false), Is.Null, "nothing changed");
            Assert.That(GuideTools.StoryLine(1, false, ch[0], true), Is.EqualTo(ch[0].Accepted), "load accepted");
            Assert.That(GuideTools.StoryLine(1, true, ch[0], true), Is.Null, "said once");
            Assert.That(GuideTools.StoryLine(1, true, ch[1], false), Is.EqualTo(ch[1].Story), "next chapter");
            Assert.That(GuideTools.StoryLine(2, true, ch[1], false), Is.Null, "restart is not narrated");
            Assert.That(GuideTools.StoryLine(3, false, null, false), Is.Null, "lesson closed");
        }
    }
}
