using Airlift.Guide;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Welcome;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Airlift.Tests
{
    /// Owner report 2026-09-16 18:58: the guide could not open Cargo Crew by voice, and inside the lesson
    /// it told the learner to grab an orange strap while the briefing step showed nothing to grab.
    /// Phase 1R: steps name Dock 7 crates and the chapter on the table.
    public class GuideGroundingTests
    {
        [Test] public void OpenLessonByVoiceOnlyOpensThePlayableCardWhileCardsShow()
        {
            Assert.That(GuideTools.OpenLesson(WelcomePhase.Catalog, "cargo_crew_fractions"), Is.EqualTo(OpenLessonDecision.Open));
            Assert.That(GuideTools.OpenLesson(WelcomePhase.Catalog, "neighborhood_cafe_division"), Is.EqualTo(OpenLessonDecision.ComingSoon));
            Assert.That(GuideTools.OpenLesson(WelcomePhase.Catalog, "made_up"), Is.EqualTo(OpenLessonDecision.Unknown));
            Assert.That(GuideTools.OpenLesson(WelcomePhase.Welcome, "cargo_crew_fractions"), Is.EqualTo(OpenLessonDecision.NotShowing));
            Assert.That(GuideTools.OpenLesson(WelcomePhase.Lesson, "cargo_crew_fractions"), Is.EqualTo(OpenLessonDecision.NotShowing));
        }

        [Test] public void CardIdIsReadFromToolArgumentsAndBadJsonIsEmpty()
        {
            Assert.That(GuideTools.CardId("{\"cardId\":\"cargo_crew_fractions\"}"), Is.EqualTo("cargo_crew_fractions"));
            Assert.That(GuideTools.CardId("not json"), Is.EqualTo(""));
            Assert.That(GuideTools.CardId("{}"), Is.EqualTo(""));
        }

        [Test] public void NothingIsGrabbableBeforePractice()
        {
            foreach (var s in new[] { OnboardingStage.Overview, OnboardingStage.Orientation, OnboardingStage.Demonstration })
                Assert.That(GuideSteps.ForOnboarding(s).CanGrabNow, Is.False, s.ToString());
            Assert.That(GuideSteps.ForOnboarding(OnboardingStage.Overview).OnTableNow, Does.Contain("no crate"));
            Assert.That(GuideSteps.ForOnboarding(OnboardingStage.Practice).CanGrabNow, Is.True);
            Assert.That(GuideSteps.ForOnboarding(OnboardingStage.Ready).OnTableNow, Does.Contain("start loading"));
            foreach (var s in new[] { OnboardingStage.Catalog, OnboardingStage.Overview, OnboardingStage.Orientation, OnboardingStage.Demonstration, OnboardingStage.Practice, OnboardingStage.Ready })
                Assert.That(GuideSteps.ForOnboarding(s).OnTableNow, Does.Not.Contain("strap"), s.ToString());
        }

        [Test] public void ChapterStepIdsNameTheDock7Chapter()
        {
            var ids = CargoChapter.All.Select(c => GuideSteps.ForChapter(c, false, c.SplitTo > 0).Id).ToArray();
            Assert.That(ids, Is.EqualTo(new[] { "chapter1_big_truck", "chapter2_two_pickups", "chapter3_four_vans", "chapter4_same_share", "chapter5_top_it_up" }));
            foreach (var c in CargoChapter.All)
            {
                Assert.That(GuideSteps.ForChapter(c, false, c.SplitTo > 0).CanGrabNow, Is.True, c.Id);
                Assert.That(GuideSteps.ForChapter(c, true, false).Id, Is.EqualTo(ids[c.Number - 1]), "completion keeps the chapter id");
            }
        }

        [Test] public void ChapterOnTableDescribesTheContainerAndCratesAtTheirCurrentSize()
        {
            var ch = CargoChapter.All;
            string truck = GuideSteps.ForChapter(ch[0], false, false).OnTableNow;
            Assert.That(truck, Does.Contain("one whole container")); Assert.That(truck, Does.Contain("one full crate"));
            Assert.That(truck, Does.Not.Contain("1/2"));

            Assert.That(GuideSteps.ForChapter(ch[1], false, true).OnTableNow, Does.Contain("one full crate").And.Contain("split"));
            Assert.That(GuideSteps.ForChapter(ch[1], false, false).OnTableNow, Does.Contain("two 1/2 crates"), "after the split");

            Assert.That(GuideSteps.ForChapter(ch[2], false, true).OnTableNow, Does.Contain("two 1/2 crates"));
            Assert.That(GuideSteps.ForChapter(ch[2], false, false).OnTableNow, Does.Contain("four 1/4 crates"));

            string same = GuideSteps.ForChapter(ch[3], false, false).OnTableNow;
            Assert.That(same, Does.Contain("four 1/4 crates")); Assert.That(same, Does.Contain("1/2 mark"));

            string topUp = GuideSteps.ForChapter(ch[4], false, false).OnTableNow;
            Assert.That(topUp, Does.Contain("Locked on the floor from 0: one 1/2 crate")); Assert.That(topUp, Does.Contain("four 1/4 crates"));
        }

        [Test] public void ChapterVerdictAppearsOnlyWhenTheAppAcceptedTheLoad()
        {
            foreach (var c in CargoChapter.All)
            {
                bool splitsAtStart = c.SplitTo > 0;
                string open = GuideSteps.ForChapter(c, false, splitsAtStart).OnTableNow;
                string done = GuideSteps.ForChapter(c, true, false).OnTableNow;
                Assert.That(open, Does.Not.Contain("accepted"), c.Id);
                Assert.That(done, Does.Contain("accepted").And.Contain(c.Expression), c.Id);
                foreach (var text in new[] { open, done, GuideSteps.ForChapter(c, false, false).OnTableNow })
                {
                    Assert.That(text.Length, Is.LessThanOrEqualTo(GuideSteps.MaxOnTableLength), c.Id + " fits lesson_state");
                    Assert.That(text, Does.Not.Contain("strap"));
                }
            }
            Assert.That(GuideSteps.ForChapter(null, false, false).CanGrabNow, Is.True, "null chapter is safe");
        }

        [Test] public void PracticeDiagnosticsNeverReachTheGuide()
        {
            string shown = "Touch the orange crate with a controller.\n\n<size=70%>OVR L g0.00 t0.00 | R g0.12</size>";
            Assert.That(GuideSteps.StripDiagnostics(shown), Is.EqualTo("Touch the orange crate with a controller."));
            Assert.That(GuideSteps.StripDiagnostics("Plain instruction."), Is.EqualTo("Plain instruction."));
            Assert.That(GuideSteps.StripDiagnostics(null), Is.EqualTo(""));
        }

        [Test] public void LessonStateCarriesStepAndWhatIsOnTheTable()
        {
            var step = GuideSteps.ForOnboarding(OnboardingStage.Overview);
            var o = JObject.Parse(GuideContextBuilder.LessonStep("Cargo Crew", step.Id, step.OnTableNow, step.CanGrabNow, "Join the Cargo Crew.").Substring("APP CONTEXT ".Length));
            Assert.That((string)o["kind"], Is.EqualTo("lesson_state"));
            Assert.That((string)o["step"], Is.EqualTo(step.Id));
            Assert.That((bool)o["can_grab_now"], Is.False);
            Assert.That((string)o["on_table_now"], Is.EqualTo(step.OnTableNow));
            Assert.That((string)o["instruction"], Is.EqualTo("Join the Cargo Crew."));
        }

        [Test] public void CardFactsAreLabelledAsAnOverviewNotTheCurrentStep()
        {
            var e = JObject.Parse(GuideContextBuilder.Entered("Cargo Crew", new[] { "a" }).Substring("APP CONTEXT ".Length));
            Assert.That((string)e["note"], Does.Contain("not the current step"));
        }
    }
}
