using System.Linq;
using Airlift.Lessons;
using Airlift.Welcome;
using NUnit.Framework;

namespace Airlift.Tests
{
    /// CC-FD-05a/09: the pure decisions behind the B button and the wall's voice tools.
    public class FrontDoorToolTests
    {
        [Test] public void BSkipsTheRundownBacksOutOfALessonAndDoesNothingElsewhere()
        {
            Assert.That(GuideTools.BackButton(WelcomePhase.Catalog, holding: false, rundownRunning: true), Is.EqualTo(BackAction.SkipRundown), "‹ and B skip the tour (or finish its last stop) on any screen");
            Assert.That(GuideTools.BackButton(WelcomePhase.Consent, holding: false, rundownRunning: true), Is.EqualTo(BackAction.SkipRundown));
            Assert.That(GuideTools.BackButton(WelcomePhase.Lesson, holding: false, rundownRunning: false), Is.EqualTo(BackAction.BackToLessons));
            Assert.That(GuideTools.BackButton(WelcomePhase.Lesson, holding: true, rundownRunning: false), Is.EqualTo(BackAction.Nothing), "never while a piece is in hand");
            Assert.That(GuideTools.BackButton(WelcomePhase.Catalog, false, false), Is.EqualTo(BackAction.Nothing));
            Assert.That(GuideTools.BackButton(WelcomePhase.Consent, false, false), Is.EqualTo(BackAction.Nothing));
            Assert.That(GuideTools.BackButton(WelcomePhase.Welcome, false, false), Is.EqualTo(BackAction.BackToWelcomeCard), "approved 2026-09-18: B does what the ‹ arrow does");
        }

        // Owner 2026-09-18, approved on the canvas: ‹ heads toward the main screen (settings closed first), › forward.
        [Test] public void TheBackArrowClosesSettingsFirstAndBacksTheQuestionsUp()
        {
            Assert.That(GuideTools.BackButton(WelcomePhase.Catalog, false, false, settingsOpen: true), Is.EqualTo(BackAction.CloseSettings));
            Assert.That(GuideTools.BackButton(WelcomePhase.Lesson, false, false, settingsOpen: true), Is.EqualTo(BackAction.CloseSettings));
            Assert.That(GuideTools.BackButton(WelcomePhase.Welcome, false, false, settingsOpen: false), Is.EqualTo(BackAction.BackToWelcomeCard));
            Assert.That(GuideTools.BackButton(WelcomePhase.Catalog, false, false, settingsOpen: false), Is.EqualTo(BackAction.Nothing), "the wall is home: dimmed");
            Assert.That(GuideTools.BackButton(WelcomePhase.Consent, false, false, settingsOpen: false), Is.EqualTo(BackAction.Nothing));
        }

        [Test] public void TheNextArrowMovesForwardAndIsDimmedWhenThereIsNothingToDo()
        {
            Assert.That(GuideTools.NextButton(WelcomePhase.Welcome, false, false, false, false, false, false, false), Is.EqualTo(NextAction.SkipQuestions));
            Assert.That(GuideTools.NextButton(WelcomePhase.Welcome, false, true, true, false, false, false, false, questionsAnswered: false), Is.EqualTo(NextAction.Nothing), "during the tour › waits for every row");
            Assert.That(GuideTools.NextButton(WelcomePhase.Welcome, false, true, true, false, false, false, false, questionsAnswered: true), Is.EqualTo(NextAction.SkipQuestions));
            Assert.That(GuideTools.NextButton(WelcomePhase.Catalog, false, true, continueLit: true, false, false, false, false), Is.EqualTo(NextAction.RundownContinue));
            Assert.That(GuideTools.NextButton(WelcomePhase.Catalog, false, true, continueLit: false, false, false, false, false), Is.EqualTo(NextAction.Nothing), "the stop is not done yet");
            Assert.That(GuideTools.NextButton(WelcomePhase.Catalog, false, false, false, false, false, false, false), Is.EqualTo(NextAction.NextPage));
            Assert.That(GuideTools.NextButton(WelcomePhase.Lesson, false, false, false, chapterActive: true, chapterComplete: true, isLastChapter: false, holding: false), Is.EqualTo(NextAction.NextChapter));
            Assert.That(GuideTools.NextButton(WelcomePhase.Lesson, false, false, false, chapterActive: true, chapterComplete: false, isLastChapter: false, holding: false), Is.EqualTo(NextAction.Nothing), "accepted first");
            Assert.That(GuideTools.NextButton(WelcomePhase.Lesson, false, false, false, true, true, isLastChapter: true, holding: false), Is.EqualTo(NextAction.Nothing), "last chapter");
            Assert.That(GuideTools.NextButton(WelcomePhase.Lesson, false, false, false, true, true, false, holding: true), Is.EqualTo(NextAction.Nothing), "piece in hand");
            Assert.That(GuideTools.NextButton(WelcomePhase.Catalog, settingsOpen: true, false, false, false, false, false, false), Is.EqualTo(NextAction.Nothing), "settings open");
            Assert.That(GuideTools.NextButton(WelcomePhase.Consent, false, false, false, false, false, false, false), Is.EqualTo(NextAction.Nothing), "the two pills are the choice");
        }

        [Test] public void TheQuestionsCanBackUpToTheWelcomeCard()
        {
            var f = new WelcomeFlow(); f.Consent(true);
            Assert.That(f.BackToConsent(), Is.True); Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Consent)); Assert.That(f.VoiceConsented, Is.False);
            Assert.That(f.BackToConsent(), Is.False);
        }

        [Test] public void TheWallToolsAreRoutedAsWelcomeTools()
        {
            Assert.That(GuideTools.DashboardToolNames, Is.EqualTo(new[] { "dashboard_open_tile", "dashboard_filter", "open_settings", "replay_rundown" }));
            Assert.That(GuideTools.DashboardToolNames.Except(LessonToolRouter.WelcomeTools).ToArray(), Is.Empty);
            foreach (var name in GuideTools.DashboardToolNames)
                Assert.That(LessonToolRouter.Route(name, null, new LessonStation[0]).Route, Is.EqualTo(ToolRoute.Welcome), name);
        }

        [Test] public void OpenTileOpensLessonsAndChaptersOnlyFromTheWall()
        {
            Assert.That(GuideTools.OpenTile(WelcomePhase.Catalog, "cargo_crew_fractions", 0), Is.EqualTo(OpenLessonDecision.Open));
            Assert.That(GuideTools.OpenTile(WelcomePhase.Catalog, "neighborhood_cafe_division", 5), Is.EqualTo(OpenLessonDecision.Open));
            Assert.That(GuideTools.OpenTile(WelcomePhase.Catalog, "neighborhood_cafe_division", 6), Is.EqualTo(OpenLessonDecision.Unknown), "no sixth chapter");
            Assert.That(GuideTools.OpenTile(WelcomePhase.Catalog, "soon_route_9", 0), Is.EqualTo(OpenLessonDecision.ComingSoon), "refused in one honest line");
            Assert.That(GuideTools.OpenTile(WelcomePhase.Lesson, "cargo_crew_fractions", 1), Is.EqualTo(OpenLessonDecision.NotShowing));
            Assert.That(GuideTools.OpenTile(WelcomePhase.Rundown, "cargo_crew_fractions", 1), Is.EqualTo(OpenLessonDecision.NotShowing));
            Assert.That(GuideTools.OpenTile(WelcomePhase.Catalog, "nope", 1), Is.EqualTo(OpenLessonDecision.Unknown));
        }

        [Test] public void ToolArgumentsParseLeniently()
        {
            Assert.That(GuideTools.TileArgs("{\"lesson_id\":\"cargo_crew_fractions\",\"chapter\":3}"), Is.EqualTo(("cargo_crew_fractions", 3)));
            Assert.That(GuideTools.TileArgs("{\"lessonId\":\"x\"}"), Is.EqualTo(("x", 0)));
            Assert.That(GuideTools.TileArgs("not json"), Is.EqualTo(("", 0)));
            Assert.That(GuideTools.ParseFilter("{\"filter\":\"most_viewed\"}"), Is.EqualTo(DashboardFilter.MostViewed));
            Assert.That(GuideTools.ParseFilter("{\"filter\":\"Newest\"}"), Is.EqualTo(DashboardFilter.Newest));
            Assert.That(GuideTools.ParseFilter("{\"filter\":\"bookmarks\"}"), Is.Null);
        }
    }
}
