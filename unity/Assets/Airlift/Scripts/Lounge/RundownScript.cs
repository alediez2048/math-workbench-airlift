using System;
using System.Collections.Generic;

namespace Airlift.Lounge
{
    /// The one control the helper lights for a stop (kept for the settings switch and future stops).
    public enum ControllerButton { None, Trigger, Grip, B, Thumbstick }

    /// What the learner has to actually do before a stop counts as done. Every stop is gated on the real thing.
    public enum RundownGate { ConsentPressed, NextPressed, ContinuePressed, FilterPressed, SettingsOpened, BackPressed }

    /// What Dee points at for a stop.
    public enum TourTarget { ConsentPills, NextArrow, FirstTile, Filters, Gear, BackArrow }

    public sealed class RundownStep
    {
        public readonly string Id, Eyebrow, Say, SayWithoutVoice;
        public readonly RundownGate Gate;
        public readonly TourTarget Target;
        public RundownStep(string id, string eyebrow, string say, RundownGate gate, TourTarget target, string sayWithoutVoice = null)
        { Id = id; Eyebrow = eyebrow; Say = say; SayWithoutVoice = sayWithoutVoice ?? say; Gate = gate; Target = target; }
    }

    /// The host tour (spec 2026-09-18-host-tour-design.md): Dee shows the learner around her lounge, from the first
    /// card to the wall, one thing at a time; each stop is gated on the real control. Lessons are left alone.
    /// Skippable at any moment (Skip pill, ‹, B); replayable from the gear (wall stops). Copy for a 7–10 year old.
    public sealed class RundownScript
    {
        public const int StopCount = 6;
        /// Where a replay from the gear starts: the first stop on the wall.
        public const int WallStart = 2;
        public static readonly IReadOnlyList<RundownStep> Steps = new[]
        {
            new RundownStep("hello", "1 OF 6",
                "Hi, I'm Dee. This is my lounge, and this board is where we start. Turn on the mic if you'd like to talk with me.",
                RundownGate.ConsentPressed, TourTarget.ConsentPills,
                sayWithoutVoice: "Hi, I'm Dee. This is my lounge, and this board is where we start. Pick one to go on."),
            new RundownStep("questions", "2 OF 6",
                "Tell me a little about you, then press the arrow down here. It is always in that corner.",
                RundownGate.NextPressed, TourTarget.NextArrow),
            new RundownStep("wall", "3 OF 6",
                "This is my wall. Every tile is a place we can go. Point the beam and press the trigger to open one. Press Next to go on.",
                RundownGate.ContinuePressed, TourTarget.FirstTile),
            new RundownStep("filters", "4 OF 6",
                "These sort the wall. Try one.",
                RundownGate.FilterPressed, TourTarget.Filters),
            new RundownStep("gear", "5 OF 6",
                "The gear is where you set things the way you like. Pause, Again and Mute live there too. Press it, then Done.",
                RundownGate.SettingsOpened, TourTarget.Gear),
            new RundownStep("back", "6 OF 6",
                "Press Back, or B on your controller, to come back here from anywhere. Try it, and then pick a lesson: I'll see you there.",
                RundownGate.BackPressed, TourTarget.BackArrow),
        };

        int index = -1;
        public bool Running => index >= 0 && index < StopCount;
        public int Index => index;
        public RundownStep Current => Running ? Steps[index] : null;
        public bool Satisfied { get; private set; }
        public bool Finished { get; private set; }
        public bool Skipped { get; private set; }
        public event Action<RundownStep> StepShown;
        public event Action<bool> Ended;   // true = skipped

        public void Start(int at = 0) { index = System.Math.Max(0, System.Math.Min(StopCount - 1, at)); Satisfied = false; Finished = Skipped = false; StepShown?.Invoke(Current); }

        /// Something happened. Only the current stop's own gate counts; anything else is ignored.
        public bool Report(RundownGate gate)
        {
            if (!Running || Satisfied) return false;
            if (Current.Gate != gate) return false;
            Satisfied = true;
            Advance();
            return true;
        }

        void Advance()
        {
            Satisfied = false;
            index++;
            if (index >= StopCount) { index = StopCount; Finished = true; Ended?.Invoke(false); return; }
            StepShown?.Invoke(Current);
        }

        /// Always available, from any stop: the Skip pill, ‹ and B.
        public bool Skip()
        {
            if (!Running) return false;
            index = StopCount; Skipped = true; Finished = true; Satisfied = false;
            Ended?.Invoke(true);
            return true;
        }
    }
}
