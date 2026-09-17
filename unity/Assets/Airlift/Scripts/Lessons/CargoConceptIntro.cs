using System.Collections.Generic;
using Airlift.Welcome;

namespace Airlift.Lessons
{
    /// Dock 7 "What is a fraction?" intro state (owner 2026-09-17): four steps between onboarding's Start fractions and
    /// chapter 1, once per app run. Leaving during the intro counts as not seen. The poses place the real lesson crates
    /// over the 0 to 1 ruler by cells; CargoStation applies them without touching CargoLessonModel.
    public sealed class CargoConceptIntro
    {
        public const string FinishFirstReason = "Let's finish the intro first: say next or press Next.";

        /// One crate shown in a step: pool id, first ruler cell, length in cells, label denominator (1 = whole).
        public readonly struct PiecePose
        {
            public readonly string Id; public readonly int StartCell, Cells, Denominator;
            public PiecePose(string id, int startCell, int cells, int denominator) { Id = id; StartCell = startCell; Cells = cells; Denominator = denominator; }
        }

        static readonly PiecePose[] WholePose = { new PiecePose("whole", 0, 8, 1) };
        static readonly PiecePose[] HalvesPose = { new PiecePose("half-1", 0, 4, 2), new PiecePose("half-2", 4, 4, 2) };
        static readonly PiecePose[] QuartersPose = { new PiecePose("quarter-1", 0, 2, 4), new PiecePose("quarter-2", 2, 2, 4), new PiecePose("quarter-3", 4, 2, 4), new PiecePose("quarter-4", 6, 2, 4) };

        readonly IReadOnlyList<IntroStep> steps;
        int index = -1;

        public CargoConceptIntro() : this(ConceptIntros.Cargo) { }
        public CargoConceptIntro(IReadOnlyList<IntroStep> steps) { this.steps = steps ?? new IntroStep[0]; }

        public bool Seen { get; private set; }
        public bool Running => index >= 0;
        public IntroStep Current => Running ? steps[index] : null;
        public bool IsLast => Running && index == steps.Count - 1;
        /// The first move toward chapter 1 in this app run shows the intro.
        public bool ShouldRun => !Seen && !Running && steps.Count > 0;
        public string PrimaryLabel => IsLast ? "Start" : "Next";

        public IntroStep Start() { index = steps.Count > 0 ? 0 : -1; return Current; }

        /// The next step, or null when the last step was showing: the intro is over and counts as seen.
        public IntroStep Next()
        {
            if (!Running) return null;
            if (index < steps.Count - 1) { index++; return Current; }
            Finish();
            return null;
        }

        public void Finish() { if (Running) { index = -1; Seen = true; } }
        public void Cancel() { index = -1; }

        public static IReadOnlyList<PiecePose> Poses(string visual)
        {
            switch (visual)
            {
                case "whole": return WholePose;
                case "halves":
                case "sum": return HalvesPose;
                case "quarters": return QuartersPose;
                default: return new PiecePose[0];
            }
        }

        /// What the learner sees during a step, for the guide's on_table_now. Nothing can be grabbed.
        public static GuideStep StepFor(IntroStep step, bool isLast)
        {
            string next = isLast ? " Next: say next or press Start to begin chapter 1." : " Next: say next or press Next.";
            string shown;
            switch (step != null ? step.Visual : "")
            {
                case "whole": shown = "one whole crate lies over the 0 to 1 ruler, labelled 1"; break;
                case "halves": shown = "two half crates lie side by side over the 0 to 1 ruler, each labelled 1/2"; break;
                case "sum": shown = "two half crates lie side by side over the 0 to 1 ruler, each labelled 1/2, and the card shows 1/2 + 1/2 = 1"; break;
                case "quarters": shown = "four quarter crates lie side by side over the 0 to 1 ruler, each labelled 1/4"; break;
                default: shown = "the 0 to 1 ruler"; break;
            }
            return new GuideStep("intro", "What is a fraction? intro, no loading yet: " + shown + ". Nothing can be grabbed." + next, false);
        }
    }
}
