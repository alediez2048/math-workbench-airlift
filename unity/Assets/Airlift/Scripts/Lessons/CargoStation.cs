using System.Collections.Generic;
using Airlift.Onboarding;
using Airlift.Welcome;
using Newtonsoft.Json.Linq;
using Oculus.Interaction;
using UnityEngine;

namespace Airlift.Lessons
{
    /// Cargo Crew as a LessonStation: wraps the untouched OnboardingDirector (briefing, demo, practice) and
    /// CargoLessonDirector (Dock 7 chapters). Every gate and reason is the one NerdyDirector used before the
    /// multi-lesson rewire, so the Cargo voice script stays byte-identical (CargoVoiceCharacterizationTests). Owner
    /// 2026-09-17: the "What is a fraction?" concept intro (CargoConceptIntro) runs between Start fractions and chapter 1.
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
        public override IntroStep CurrentIntro { get { SyncIntro(); return intro.Current; } }
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
            if (CurrentIntro != null) return CargoConceptIntro.StepFor(intro.Current, intro.IsLast);
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
        public override void Close() { D?.Back(); L?.Exit(); SyncIntro(); }

        /// "yes / next": the on-card primary button during onboarding, the next intro step, next chapter inside a chapter.
        public override LessonActionResult Advance()
        {
            if (CurrentIntro != null) return ContinueIntro();
            switch (GuideTools.AdvanceStep(WelcomePhase.Lesson, ChapterActive, PrimaryAvailable, PracticeHeld))
            {
                case AdvanceRoute.NextChapter: return L.TryNextChapter();
                case AdvanceRoute.OnboardingStep:
                    if (Stage == OnboardingStage.Ready && intro.ShouldRun && L != null) return ContinueIntro();
                    D.Continue(); return new LessonActionResult(true, "");
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
                case "replay_demo": result = CurrentIntro != null ? IntroRefusal : ReplayDemo(); return true;
                default: result = default; return false;
            }
        }

        LessonActionResult ChapterAction(System.Func<CargoLessonDirector, LessonActionResult> action)
        {
            if (CurrentIntro != null) return IntroRefusal;
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

        // ---- concept intro: "What is a fraction?" ----
        readonly CargoConceptIntro intro = new CargoConceptIntro();
        readonly List<MonoBehaviour> introDisabled = new List<MonoBehaviour>();
        static readonly LessonActionResult IntroRefusal = new LessonActionResult(false, CargoConceptIntro.FinishFirstReason);

        /// Wired to OnboardingDirector.whenReadyContinue (BuildDockWorkbench): Start fractions and every Next press on the
        /// intro card. The first time in this app run it shows the intro; after its last step, or once seen, chapter 1.
        public void ContinueFromReady() => ContinueIntro();

        LessonActionResult ContinueIntro()
        {
            SyncIntro();
            if (L == null) return new LessonActionResult(false, CargoLessonDirector.ClosedReason);
            bool wasRunning = intro.Running;
            IntroStep step = wasRunning ? intro.Next() : intro.ShouldRun ? intro.Start() : null;
            if (step != null) { ShowIntro(step); return new LessonActionResult(true, step.Say); }
            if (wasRunning) RestoreIntroTable(false);
            L.Begin();
            return new LessonActionResult(true, "");
        }

        /// Ends a running intro that the table no longer shows: a chapter started some other way counts as seen; leaving
        /// the Ready card (Back, Close) cancels it, puts the crates back and counts as not seen.
        void SyncIntro()
        {
            if (!intro.Running) return;
            if (L != null && L.IsActive) { intro.Finish(); SetIntroGrabs(true); return; }
            if (D == null || L == null || D.Stage != OnboardingStage.Ready) { intro.Cancel(); RestoreIntroTable(true); }
        }

        IEnumerable<CargoLessonDirector.PieceView> PieceViews()
        {
            if (L == null) yield break;
            foreach (var view in new[] { L.whole, L.halfA, L.halfB }) if (view?.piece != null) yield return view;
            if (L.quarters != null) foreach (var view in L.quarters) if (view?.piece != null) yield return view;
        }

        /// The lesson crates posed over the ruler on the measuring pad, labelled; the card shows the step. The practice
        /// crate and the demo replay step aside; nothing can be grabbed. CargoLessonModel is not touched.
        void ShowIntro(IntroStep step)
        {
            var l = L; var d = D;
            if (l.chapterObjects != null && !l.chapterObjects.activeSelf) l.chapterObjects.SetActive(true);
            if (l.halfMarks != null) foreach (var mark in l.halfMarks) if (mark != null) mark.SetActive(false);
            if (d != null)
            {
                if (d.strap != null) d.strap.gameObject.SetActive(false);
                if (d.help != null) d.help.gameObject.SetActive(false);
                if (d.primaryLabel != null) d.primaryLabel.text = intro.PrimaryLabel;
            }
            float height = d != null && d.content != null ? d.content.targetPosition.y : l.restHeight;
            var poses = CargoConceptIntro.Poses(step.Visual);
            foreach (var view in PieceViews())
            {
                int k = -1;
                for (int i = 0; i < poses.Count; i++) if (poses[i].Id == view.id) k = i;
                view.piece.gameObject.SetActive(k >= 0);
                if (view.lockedMark != null) view.lockedMark.SetActive(false);
                if (k < 0) continue;
                var pose = poses[k];
                view.piece.localPosition = l.ruler != null ? RulerLayout.SnapPosition(l.ruler.localPosition, pose.StartCell, pose.Cells, height) : view.trayPosition;
                view.piece.localRotation = Quaternion.identity;
                if (view.label != null) { if (pose.Denominator == 1) view.label.ShowWhole(1); else view.label.Show(1, pose.Denominator); }
            }
            SetIntroGrabs(false);
            var heading = l.heading != null ? l.heading : d != null ? d.heading : null;
            var shown = l.body != null ? l.body : d != null ? d.body : null;
            if (heading != null) heading.text = step.Heading;
            if (shown != null) shown.text = step.Say;
            if (l.expressionLine != null) l.expressionLine.text = step.Expression ?? "";
            if (l.sayHints != null) l.sayHints.text = "Say \"next\"";
        }

        /// Crates back in the tray (quarters put away), grabs enabled again, intro lines cleared. The chapter's own
        /// layout (CargoLessonDirector.Begin) or onboarding's Refresh restores everything else.
        void RestoreIntroTable(bool hideChapterObjects)
        {
            if (L == null) return;
            foreach (var view in PieceViews())
            {
                view.piece.localPosition = view.trayPosition;
                view.piece.localRotation = Quaternion.identity;
                view.piece.gameObject.SetActive(!view.id.StartsWith("quarter"));
            }
            SetIntroGrabs(true);
            if (L.expressionLine != null) L.expressionLine.text = "";
            if (L.sayHints != null) L.sayHints.text = "";
            if (hideChapterObjects && !L.IsActive && L.chapterObjects != null) L.chapterObjects.SetActive(false);
        }

        /// Same rule as CargoLessonDirector.LateUpdate for locked crates: behaviour off and interactable disabled. Only
        /// what the intro turned off is turned back on.
        void SetIntroGrabs(bool on)
        {
            if (on)
            {
                foreach (var behaviour in introDisabled) if (behaviour != null && !behaviour.enabled) behaviour.enabled = true;
                introDisabled.Clear();
                return;
            }
            foreach (var view in PieceViews())
                foreach (var behaviour in view.piece.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (!(behaviour is IInteractable interactable)) continue;
                    if (behaviour.enabled) { behaviour.enabled = false; if (!introDisabled.Contains(behaviour)) introDisabled.Add(behaviour); }
                    if (view.piece.gameObject.activeInHierarchy && interactable.State != InteractableState.Disabled) interactable.Disable();
                }
        }

        /// TableHandle re-enables every piece after a carry, so the intro's no-grab rule is re-applied each frame.
        void LateUpdate()
        {
            SyncIntro();
            if (intro.Running) SetIntroGrabs(false);
        }
    }
}
