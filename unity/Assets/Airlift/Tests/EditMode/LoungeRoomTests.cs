using Airlift.Lounge;
using NUnit.Framework;

namespace Airlift.Tests
{
    /// CC-FD-01. The lounge is the home: the learner arrives there, is onboarded there and browses there, and it
    /// hides only while a lesson runs. These are the decisions that can be wrong without anyone noticing on a
    /// headset: what each scenery actually shows, and whether the arrival bar reports real work or theatre.
    public class LoungeRoomTests
    {
        [Test] public void NerdyLoungeShowsTheRoomAndTurnsPassthroughOff()
        {
            var s = LoungeScenery.For(Scenery.NerdyLounge);
            Assert.That(s.ShellVisible, Is.True, "the designed room is the point of this scenery");
            Assert.That(s.PassthroughOn, Is.False);
            Assert.That(s.FurnitureVisible, Is.True);
            Assert.That(s.SkyVisible, Is.True, "the room looks out on a sky, not on black");
            Assert.That(s.FogOn, Is.True, "depth is what makes it a place");
        }

        [Test] public void YourRoomKeepsPassthroughAndDropsOnlyTheShell()
        {
            var s = LoungeScenery.For(Scenery.YourRoom);
            Assert.That(s.PassthroughOn, Is.True, "AR shows the learner's actual room");
            Assert.That(s.ShellVisible, Is.False, "no solid backdrop over passthrough");
            Assert.That(s.FurnitureVisible, Is.True, "the furniture and glows stay; only the shell goes");
            Assert.That(s.SkyVisible, Is.False, "a skybox over passthrough would paint out the learner's room");
            Assert.That(s.FogOn, Is.False, "fog would grey out passthrough");
        }

        [Test] public void ArIsTheDefaultUntilTheLearnerChooses()
        {
            Assert.That(LoungeScenery.Default, Is.EqualTo(Scenery.YourRoom));
        }

        // "A pill progress bar appears only if something is genuinely still loading" — spec, Arrival.
        [Test] public void ProgressReportsRealWorkOnly()
        {
            var w = new StartupWork();
            Assert.That(w.Progress, Is.EqualTo(0f));
            Assert.That(w.Ready, Is.False);

            w.Complete(StartupStep.Assets);
            Assert.That(w.Progress, Is.EqualTo(1f / 3f).Within(0.001f));

            w.Complete(StartupStep.Guide);
            w.Complete(StartupStep.Microphone);
            Assert.That(w.Progress, Is.EqualTo(1f));
            Assert.That(w.Ready, Is.True);
        }

        [Test] public void CompletingTheSameStepTwiceDoesNotInflateProgress()
        {
            var w = new StartupWork();
            w.Complete(StartupStep.Guide);
            w.Complete(StartupStep.Guide);
            Assert.That(w.Progress, Is.EqualTo(1f / 3f).Within(0.001f));
        }

        [Test] public void NoBarWhenThereIsNothingToWaitFor()
        {
            var w = new StartupWork();
            w.Complete(StartupStep.Assets);
            w.Complete(StartupStep.Guide);
            w.Complete(StartupStep.Microphone);
            Assert.That(w.ShouldShowBar, Is.False, "never a fake timer over finished work");

            var pending = new StartupWork();
            pending.Complete(StartupStep.Assets);
            Assert.That(pending.ShouldShowBar, Is.True);
        }

        // The lounge is the home, so it hides for exactly one reason: a lesson is running.
        [Test] public void TheLoungeHidesOnlyForALesson()
        {
            Assert.That(LoungeVisibility.ShouldShow(lessonRunning: false, onboardingDone: false), Is.True);
            Assert.That(LoungeVisibility.ShouldShow(lessonRunning: false, onboardingDone: true), Is.True,
                "browsing happens in the lounge every session, not just the first run");
            Assert.That(LoungeVisibility.ShouldShow(lessonRunning: true, onboardingDone: true), Is.False);
        }
    }
}
