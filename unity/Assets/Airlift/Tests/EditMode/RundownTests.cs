using System.Linq;
using Airlift.Lounge;
using NUnit.Framework;

namespace Airlift.Tests
{
    /// The host tour (spec 2026-09-18-host-tour-design.md): six stops from the first card to the wall, each gated on
    /// the real control; skippable at any moment; lessons untouched.
    public class RundownTests
    {
        [Test] public void SixStopsFromTheFirstCardToTheWall()
        {
            Assert.That(RundownScript.Steps.Count, Is.EqualTo(RundownScript.StopCount));
            Assert.That(RundownScript.Steps.Select(s => s.Id), Is.EqualTo(new[] { "hello", "questions", "wall", "filters", "gear", "back" }));
            Assert.That(RundownScript.Steps.Select(s => s.Target), Is.EqualTo(new[] { TourTarget.ConsentPills, TourTarget.NextArrow, TourTarget.FirstTile, TourTarget.Filters, TourTarget.Gear, TourTarget.BackArrow }));
            Assert.That(RundownScript.Steps.Select(s => s.Gate), Is.EqualTo(new[] { RundownGate.ConsentPressed, RundownGate.NextPressed, RundownGate.ContinuePressed, RundownGate.FilterPressed, RundownGate.SettingsOpened, RundownGate.BackPressed }));
            Assert.That(RundownScript.Steps[0].Say, Does.StartWith("Hi, I'm Dee. This is my lounge"), "the host frame");
            Assert.That(RundownScript.Steps[5].Say, Does.Contain("pick a lesson"), "the tour hands over to the wall, never a lesson");
            Assert.That(RundownScript.WallStart, Is.EqualTo(2), "a replay from the gear starts on the wall");
            for (int i = 0; i < RundownScript.Steps.Count; i++)
            {
                var s = RundownScript.Steps[i];
                Assert.That(s.Eyebrow, Is.EqualTo((i + 1) + " OF 6"));
                foreach (var banned in new[] { "great job", "well done", "score", "star", "timer", "crate", "dock" }) Assert.That(s.Say.ToLowerInvariant(), Does.Not.Contain(banned), s.Id);
                Assert.That(s.Say.Split('.').Max(x => x.Trim().Split(' ').Length), Is.LessThanOrEqualTo(18), s.Id + " keeps sentences short for a 7-10 year old");
            }
        }

        [Test] public void EachStopWaitsForItsOwnGateAndNothingElse()
        {
            var r = new RundownScript(); r.Start();
            Assert.That(r.Current.Id, Is.EqualTo("hello"));
            Assert.That(r.Report(RundownGate.NextPressed), Is.False, "the arrow during stop 1 does not skip ahead");
            Assert.That(r.Report(RundownGate.ConsentPressed), Is.True); Assert.That(r.Current.Id, Is.EqualTo("questions"));
            Assert.That(r.Report(RundownGate.FilterPressed), Is.False);
            Assert.That(r.Report(RundownGate.NextPressed), Is.True); Assert.That(r.Current.Id, Is.EqualTo("wall"));
            Assert.That(r.Report(RundownGate.ContinuePressed), Is.True); Assert.That(r.Current.Id, Is.EqualTo("filters"));
            Assert.That(r.Report(RundownGate.FilterPressed), Is.True); Assert.That(r.Current.Id, Is.EqualTo("gear"));
            Assert.That(r.Report(RundownGate.SettingsOpened), Is.True); Assert.That(r.Current.Id, Is.EqualTo("back"));
            Assert.That(r.Finished, Is.False);
            Assert.That(r.Report(RundownGate.BackPressed), Is.True);
            Assert.That(r.Finished, Is.True); Assert.That(r.Skipped, Is.False); Assert.That(r.Running, Is.False);
        }

        [Test] public void SkipEndsItFromAnyStopAndCountsAsSeen()
        {
            var r = new RundownScript(); bool? endedSkipped = null; r.Ended += s => endedSkipped = s;
            r.Start(); r.Report(RundownGate.ConsentPressed);
            Assert.That(r.Skip(), Is.True);
            Assert.That(r.Finished, Is.True); Assert.That(r.Skipped, Is.True); Assert.That(endedSkipped, Is.True);
            Assert.That(r.Skip(), Is.False, "nothing to skip once it ended");
        }

        [Test] public void AReplayFromTheGearStartsOnTheWall()
        {
            var r = new RundownScript(); int shown = 0; r.StepShown += _ => shown++;
            r.Start(RundownScript.WallStart);
            Assert.That(r.Current.Id, Is.EqualTo("wall")); Assert.That(shown, Is.EqualTo(1));
            r.Start(99); Assert.That(r.Current.Id, Is.EqualTo("back"), "clamped");
        }

        [Test] public void TheVoiceOffWordingNeverAsksTheLearnerToTalk()
        {
            var hello = RundownScript.Steps.First(s => s.Id == "hello");
            Assert.That(hello.Say, Does.Contain("mic"));
            Assert.That(hello.SayWithoutVoice, Does.Not.Contain("mic").And.Not.Contain("talk"));
        }
    }
}
