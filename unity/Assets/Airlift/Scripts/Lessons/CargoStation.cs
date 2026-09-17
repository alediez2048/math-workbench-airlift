using Airlift.Onboarding;
using Airlift.Welcome;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Airlift.Lessons
{
    /// Cargo Crew as a LessonStation: wraps the untouched OnboardingDirector (briefing, demo, practice) and
    /// CargoLessonDirector (Dock 7 chapters). Every gate and reason is the one NerdyDirector used before the
    /// multi-lesson rewire, so the Cargo voice script stays byte-identical (CargoVoiceCharacterizationTests).
    public sealed class CargoStation : LessonStation
    {
        public const string CardId = "cargo_crew_fractions";
        public OnboardingDirector onboarding;
        public CargoLessonDirector lesson;

        OnboardingDirector D { get { if (onboarding == null) onboarding = GetComponent<OnboardingDirector>(); return onboarding; } }
        CargoLessonDirector L { get { if (lesson == null) lesson = GetComponent<CargoLessonDirector>(); return lesson; } }

        public override string Title => "Cargo Crew";
        public override string StoryName => "Dock 7";
        public override bool IsOpen => D != null && D.Stage != OnboardingStage.Catalog;
        public override bool ChapterActive => L != null && L.IsActive;
        public override bool AnyHeld => PracticeHeld || (L != null && L.AnyPieceHeld);
        /// Integrator ruling (CC-PL-03): Cargo Crew sends no tools_now, so its accepted lesson_state and tool-result
        /// JSON stay byte-identical (CargoVoiceCharacterizationTests). Wrong-lesson tools are still refused by the router.
        public override string[] ToolsNow => null;
        public override string[] ToolNames => LessonToolRouter.CargoTools;

        OnboardingStage Stage => D != null ? D.Stage : OnboardingStage.Catalog;
        bool PracticeHeld => D != null && D.IsHolding;
        bool PrimaryAvailable => D != null && D.primary != null && D.primary.gameObject.activeInHierarchy && D.primary.interactable;

        public override LessonChapterFacts Chapter
        {
            get
            {
                if (!ChapterActive || L.Chapter == null) return default;
                var c = L.Chapter;
                return new LessonChapterFacts(c.Number, c.Id, c.Title, c.Story, c.Task, c.Accepted, L.ExpressionText, L.Feedback, L.ChapterComplete, L.IsLastChapter);
            }
        }

        int cachedStepKey = -1; GuideStep cachedChapterStep;
        public override GuideStep CurrentStep()
        {
            if (!ChapterActive || L.Chapter == null) return GuideSteps.ForOnboarding(Stage);
            var chapter = L.Chapter; bool complete = L.ChapterComplete; bool canSplit = L.CanSplit;
            int key = chapter.Number * 4 + (complete ? 2 : 0) + (canSplit ? 1 : 0);   // cached: the director asks every frame
            if (key != cachedStepKey) { cachedStepKey = key; cachedChapterStep = GuideSteps.ForChapter(chapter, complete, canSplit); }
            return cachedChapterStep;
        }

        /// The card the learner reads (chapter card or briefing card; the same TMP object in the scene), without the
        /// temporary practice readout.
        public override string Instruction
        {
            get
            {
                var shown = ChapterActive && L.body != null ? L.body : D != null ? D.body : body;
                return shown != null ? GuideSteps.StripDiagnostics(shown.text) : "";
            }
        }

        public override void Open() { SetVisualsActive(true); D?.ChooseCargo(); }
        /// Same as the Back button: OnboardingDirector.Back then CargoLessonDirector.Exit.
        public override void Close() { D?.Back(); L?.Exit(); }

        /// "yes / next": the on-card primary button during onboarding, next chapter inside a chapter.
        public override LessonActionResult Advance()
        {
            switch (GuideTools.AdvanceStep(WelcomePhase.Lesson, ChapterActive, PrimaryAvailable, PracticeHeld))
            {
                case AdvanceRoute.NextChapter: return L.TryNextChapter();
                case AdvanceRoute.OnboardingStep: D.Continue(); return new LessonActionResult(true, "");
                default: return new LessonActionResult(false, GuideTools.AdvanceRefusal(WelcomePhase.Lesson, Stage, PracticeHeld));
            }
        }

        public override LessonActionResult Check() => ChapterAction(l => l.TryLoad());
        public override LessonActionResult ResetTable() => ChapterAction(l => l.TryReset());
        public override LessonActionResult NextChapter() => ChapterAction(l => l.TryNextChapter());
        public override LessonActionResult RestartChapter() => ChapterAction(l => l.TryRestartChapter());

        public override bool TryLessonTool(string name, JObject args, out LessonActionResult result)
        {
            switch (name)
            {
                case "split_cargo": result = ChapterAction(l => l.TrySplit()); return true;
                case "check_load": result = Check(); return true;
                case "reset_cargo": result = ResetTable(); return true;
                case "replay_demo": result = ReplayDemo(); return true;
                default: result = default; return false;
            }
        }

        LessonActionResult ChapterAction(System.Func<CargoLessonDirector, LessonActionResult> action)
        {
            var gate = GuideTools.ChapterTool(WelcomePhase.Lesson, ChapterActive, Stage);
            if (!gate.Ok) return new LessonActionResult(false, gate.Reason);
            return action(L);
        }

        /// Same as the Help button during practice; ok only when the demo actually started.
        LessonActionResult ReplayDemo()
        {
            var gate = GuideTools.ReplayDemo(WelcomePhase.Lesson, ChapterActive, Stage, PracticeHeld);
            if (!gate.Ok || D == null) return new LessonActionResult(false, gate.Ok ? GuideTools.NoLesson : gate.Reason);
            D.Help();
            bool started = D.Stage == OnboardingStage.Demonstration;
            return new LessonActionResult(started, started ? "" : "The demo could not start right now.");
        }
    }
}
