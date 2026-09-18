using System;
using System.Collections.Generic;

namespace Airlift.Lounge
{
    /// The one control the helper lights for a stop (kept for the settings switch and future stops).
    public enum ControllerButton { None, Trigger, Grip, B, Thumbstick }

    /// What the learner has to actually do before a stop counts as done. Every stop is gated on the real thing.
    public enum RundownGate { ConsentPressed, NextPressed, ContinuePressed, FilterPressed, SettingsOpened, BackPressed, PageNext, PageBack, LessonOpened, SceneryChanged }

    /// What Dee points at for a stop.
    public enum TourTarget { ConsentPills, NextArrow, FirstTile, Filters, Gear, BackArrow, NextPage, PreviousPage, Scenery }

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
        public const int WallStart = 0;
        public static readonly IReadOnlyList<RundownStep> Steps = new[]
        {
            new RundownStep("wall", "1 OF 6",
                "Welcome to the Nerdy AI plus VR math lounge. Explore fractions, division, and multiplication through hands-on adventures. Each card opens a lesson. Featured, Newest and Most viewed sort the lessons. Try one highlighted filter, or say, Dee, show newest lessons.",
                RundownGate.FilterPressed, TourTarget.Filters,
                "Welcome to the Nerdy AI plus VR math lounge. Explore fractions, division, and multiplication through hands-on adventures. Each card opens a lesson. Featured, Newest and Most viewed sort the lessons. Try one highlighted filter."),
            new RundownStep("page-next", "2 OF 6",
                "There are more lessons on the next page. Press the highlighted right arrow.",
                RundownGate.PageNext, TourTarget.NextPage),
            new RundownStep("page-back", "3 OF 6",
                "Now press the highlighted left arrow to return to the previous page.",
                RundownGate.PageBack, TourTarget.PreviousPage),
            new RundownStep("scenery", "4 OF 6",
                "Your room shows your surroundings. Nerdy Lounge shows our virtual room. Try the highlighted environment. Your lessons stay in place.",
                RundownGate.SceneryChanged, TourTarget.Scenery),
            new RundownStep("gear", "5 OF 6",
                "Stop silences me and stops listening. Play resumes; the nearby notice tells you whether the microphone will turn on. You can keep using buttons while I'm stopped. Open the highlighted gear, or say, Dee, open settings.",
                RundownGate.SettingsOpened, TourTarget.Gear,
                "Stop silences me and stops listening. Play resumes; the nearby notice tells you whether the microphone will turn on. You can keep using buttons while I'm stopped. Open the highlighted gear to explore settings."),
            new RundownStep("lesson", "6 OF 6",
                "You're ready. Choose a playable lesson, or say, Dee, open Cargo Crew. Inside, Exit lesson brings you back here.",
                RundownGate.LessonOpened, TourTarget.FirstTile,
                "You're ready. Click a playable lesson to begin. Inside, Exit lesson brings you back here."),
        };

        int index = -1;
        public bool Running => index >= 0 && index < StopCount;
        public int Index => index;
        public RundownStep Current => Running ? Steps[index] : null;
        public bool Satisfied { get; private set; }
        public bool Finished { get; private set; }
        public bool Skipped { get; private set; }
        public bool CanOpenLesson => !Running || Current.Gate == RundownGate.LessonOpened;
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
