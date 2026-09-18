using Airlift.Welcome;
using NUnit.Framework;

namespace Airlift.Tests
{
    public class TourAutostartTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void LeavingQuestionsAlwaysMakesCatalogTourDue(bool answerQuestions)
        {
            var flow = new WelcomeFlow { RundownSeen = true };
            flow.Begin();
            Assert.That(flow.Phase, Is.EqualTo(WelcomePhase.Welcome));
            if (answerQuestions) flow.RecordProfile("{\"ageBand\":\"adult\",\"interests\":[\"space\"],\"goal\":\"curious\"}");
            Assert.That(flow.EndWelcome(), Is.True);
            Assert.That(flow.Phase, Is.EqualTo(WelcomePhase.Catalog));
            Assert.That(flow.RundownSeen, Is.False, "old tour completion must not suppress the tour after questions");
            flow.MarkRundownSeen();
            Assert.That(flow.EndWelcome(), Is.False, "duplicate transitions cannot reset a finished tour");
            Assert.That(flow.RundownSeen, Is.True);
        }

        [Test] public void ReturningFromLessonDoesNotRestartCompletedTour()
        {
            var flow = new WelcomeFlow(); flow.Begin(); flow.EndWelcome(); flow.MarkRundownSeen();
            flow.OpenLesson("cargo_crew_fractions"); flow.BackToCatalog();
            Assert.That(flow.RundownSeen, Is.True);
        }

        [Test] public void CompletedReturningProfileStillSkipsQuestionsAndTour()
        {
            var flow = new WelcomeFlow { RundownSeen = true };
            flow.Profile.Merge("{\"ageBand\":\"adult\",\"interests\":[\"space\"],\"goal\":\"curious\"}");
            flow.Begin();
            Assert.That(flow.Returning, Is.True);
            Assert.That(flow.Phase, Is.EqualTo(WelcomePhase.Catalog));
            Assert.That(flow.RundownSeen, Is.True);
        }
    }
}
