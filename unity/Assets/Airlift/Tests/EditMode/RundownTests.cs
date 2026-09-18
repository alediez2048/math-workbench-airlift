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
            Assert.That(RundownScript.Steps.Select(s => s.Id), Is.EqualTo(new[] { "wall", "page-next", "page-back", "scenery", "gear", "lesson" }));
            Assert.That(RundownScript.Steps.Select(s => s.Target), Is.EqualTo(new[] { TourTarget.Filters, TourTarget.NextPage, TourTarget.PreviousPage, TourTarget.Scenery, TourTarget.Gear, TourTarget.FirstTile }));
            Assert.That(RundownScript.Steps.Select(s => s.Gate), Is.EqualTo(new[] { RundownGate.FilterPressed, RundownGate.PageNext, RundownGate.PageBack, RundownGate.SceneryChanged, RundownGate.SettingsOpened, RundownGate.LessonOpened }));
            Assert.That(RundownScript.Steps[5].Say, Does.Contain("Choose a playable lesson"));
            Assert.That(RundownScript.WallStart, Is.Zero);
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
            Assert.That(r.Current.Id, Is.EqualTo("wall"));
            Assert.That(r.Report(RundownGate.NextPressed), Is.False, "the arrow during stop 1 does not skip ahead");
            Assert.That(r.Report(RundownGate.ConsentPressed), Is.False);
            Assert.That(r.Report(RundownGate.ContinuePressed), Is.False);
            Assert.That(r.Report(RundownGate.FilterPressed), Is.True); Assert.That(r.Current.Id, Is.EqualTo("page-next"));
            Assert.That(r.Report(RundownGate.PageNext), Is.True); Assert.That(r.Current.Id, Is.EqualTo("page-back"));
            Assert.That(r.Report(RundownGate.PageNext), Is.False, "duplicate next cannot skip back practice");
            Assert.That(r.Report(RundownGate.PageBack), Is.True); Assert.That(r.Current.Id, Is.EqualTo("scenery"));
            Assert.That(r.Report(RundownGate.SceneryChanged), Is.True); Assert.That(r.Current.Id, Is.EqualTo("gear"));
            Assert.That(r.Report(RundownGate.SettingsOpened), Is.True); Assert.That(r.Current.Id, Is.EqualTo("lesson"));
            Assert.That(r.Finished, Is.False);
            Assert.That(r.Report(RundownGate.LessonOpened), Is.True);
            Assert.That(r.Report(RundownGate.LessonOpened), Is.False);
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
            r.Start(99); Assert.That(r.Current.Id, Is.EqualTo("lesson"), "clamped");
        }

        [Test] public void TheVoiceOffWordingNeverAsksTheLearnerToTalk()
        {
            Assert.That(RundownScript.Steps[4].Say, Does.Contain("Dee, open settings"));
            Assert.That(RundownScript.Steps[5].Say, Does.Contain("Dee, open Cargo Crew"));
            foreach (var step in RundownScript.Steps) Assert.That(step.SayWithoutVoice, Does.Not.Contain("say,").And.Not.Contain("talk"));
        }
    }
}
