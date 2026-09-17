using System.Collections;
using System.Collections.Generic;
using System.Text;
using Airlift.Presentation;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Airlift.Lessons
{
    /// Result of a lesson action requested by a button or by the voice guide. Reason is short and is
    /// spoken as-is, so it describes the load or the table, never the learner.
    public readonly struct LessonActionResult
    {
        public readonly bool Ok; public readonly string Reason;
        public LessonActionResult(bool ok, string reason) { Ok = ok; Reason = reason; }
    }

    /// Presentation adapter for the Dock 7 chapters. All arithmetic decisions come from CargoLessonModel;
    /// this class maps grabs, releases, buttons and voice tools to model commands and mirrors the model
    /// back into a pool of crate views, the story card and the vehicle bay. Crates are dropped into the beds
    /// of vehicles backed up over the container cells; an accepted load drives the vehicles away.
    public sealed class CargoLessonDirector : MonoBehaviour
    {
        [System.Serializable]
        public sealed class PieceView
        {
            public string id;
            public Transform piece;
            public Grabbable grabbable;
            public FractionNotationView label;
            public Vector3 trayPosition;
            public GameObject lockedMark;
            [System.NonSerialized] public bool held;
            [System.NonSerialized] public MonoBehaviour[] interactables;
        }

        public const string HeldReason = "Let go of the crate first.";
        public const string ClosedReason = "The cargo lesson is not open.";
        public const string NoSplitReason = "There is nothing to split in this chapter.";
        public const string SplitDoneReason = "The crates are already split as far as this chapter needs.";
        public const string NotCompleteReason = "Load the vehicles correctly first.";
        public const string LastChapterReason = "That was the last chapter.";
        public const string LoadedReason = "This load is already checked. Go to the next chapter or start this one over.";
        public const string NoVehicleReason = "No vehicle is parked at that part of the dock in this chapter.";
        public const string NoFitReason = "That crate does not fit there.";

        public Transform stationRoot;
        public Transform ruler;
        public float restHeight = 0.08f;
        [Header("Piece pool (ids: whole; half-1, half-2; quarter-1..4)")]
        public PieceView whole;
        public PieceView halfA;
        public PieceView halfB;
        public PieceView[] quarters = new PieceView[0];
        [Header("Scene objects")]
        public GameObject chapterObjects;
        public GameObject chapterButtons;
        public CanvasGroup chapterButtonGroup;
        public Button splitButton;
        public Button submitButton;
        public Button resetButton;
        public Button nextButton;
        public GameObject[] halfMarks = new GameObject[0];
        public VehicleBay vehicles;
        [Header("Story card")]
        public TMP_Text heading;
        public TMP_Text body;
        public TMP_Text expressionLine;
        public TMP_Text sayHints;
        public GameObject[] hideWhileActive;

        readonly CargoLessonModel model = new CargoLessonModel();
        readonly List<CargoChapter> completed = new List<CargoChapter>();
        bool active;
        float bodyBaseSize;
        string feedback = "";

        // ---- public state ----
        public bool AnyPieceHeld => active && AnyHeld;
        public bool IsActive => active;
        public CargoChapter Chapter => model.Chapter;
        public bool ChapterComplete => model.ChapterComplete;
        public bool AllChaptersComplete => model.AllChaptersComplete;
        public bool IsLastChapter => model.IsLastChapter;
        public bool CanSplit => active && model.CanSplit;
        public string ExpressionText => model.Expression;
        public string Feedback => feedback;

        void Awake()
        {
            foreach (var view in Views()) { CacheInteractables(view); Subscribe(view); }
            SetActiveObjects(false);
        }

        IEnumerable<PieceView> Views()
        {
            if (whole != null) yield return whole;
            if (halfA != null) yield return halfA;
            if (halfB != null) yield return halfB;
            if (quarters != null) foreach (var q in quarters) if (q != null) yield return q;
        }

        static void CacheInteractables(PieceView view)
        {
            if (view?.piece == null) return;
            var list = new List<MonoBehaviour>();
            foreach (var behaviour in view.piece.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour is IInteractable) list.Add(behaviour);
            view.interactables = list.ToArray();
        }

        void Subscribe(PieceView view) { if (view?.grabbable != null) view.grabbable.WhenPointerEventRaised += evt => OnPointer(view, evt); }

        // ---- lifecycle ----
        public void Begin()
        {
            active = true;
            foreach (var view in Views()) view.held = false;
            if (model.AllChaptersComplete) { completed.Clear(); model.StartChapter(0); }
            else if (model.ChapterComplete) model.NextChapter();
            else model.RestartChapter();
            SetActiveObjects(true);
            if (hideWhileActive != null) foreach (var go in hideWhileActive) if (go != null) go.SetActive(false);
            feedback = "";
            if (body != null && bodyBaseSize <= 0) bodyBaseSize = body.fontSize;
            ShowChapter();
        }

        public void Exit()
        {
            active = false;
            SetActiveObjects(false);
            if (body != null && bodyBaseSize > 0) body.fontSize = bodyBaseSize;
            if (expressionLine != null) expressionLine.text = "";
            if (sayHints != null) sayHints.text = "";
            if (vehicles != null) vehicles.ResetBay();
        }

        /// Development and editor preview only: jump straight to a chapter. Not wired to any button or tool.
        public void JumpToChapter(int index)
        {
            model.StartChapter(index);
            feedback = "";
            if (active) ShowChapter();
        }

        void SetActiveObjects(bool on)
        {
            if (chapterObjects != null) chapterObjects.SetActive(on);
            if (chapterButtons != null) chapterButtons.SetActive(on);
        }

        bool AnyHeld { get { foreach (var view in Views()) if (view.held) return true; return false; } }

        // ---- grabbing ----
        void OnPointer(PieceView view, PointerEvent evt)
        {
            if (!active) return;
            if (evt.Type == PointerEventType.Select)
            {
                if (model.IsLocked(view.id)) return;
                view.held = true;
                if (model.Undock(view.id)) SnapDocked();
            }
            else if (evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
            {
                view.held = false;
                StartCoroutine(SettleAfterRelease(view));
            }
        }

        IEnumerator SettleAfterRelease(PieceView view)
        {
            yield return null; // let the SDK finish transferring ownership before we move the piece
            if (!active || view.held) yield break;
            if (model.IsLocked(view.id)) { SnapDocked(); yield break; }
            DropAt(view.id, stationRoot.InverseTransformPoint(view.piece.position));
        }

        /// A released crate at a station-local position: the container cell under its centre picks the vehicle bed.
        /// Returns true when the model docked it. Outside the dock zone it stays where it was put; a refused drop
        /// on the dock goes back to the tray and the card shows why. Public for tests and editor drives.
        public bool DropAt(string id, Vector3 stationLocal)
        {
            var view = ViewOf(id);
            if (!active || view?.piece == null || model.Piece(id) == null || model.IsLocked(id)) return false;
            int cell = RulerLayout.CellAt(ruler.localPosition, stationLocal);
            if (cell < 0) { Refresh(); return false; }
            if (model.ChapterComplete) return RefuseDrop(view, LoadedReason);
            int bed = model.BedAtCell(cell);
            if (bed < 0) return RefuseDrop(view, NoVehicleReason);
            if (model.Dock(id, false, bed))
            {
                SnapDocked();
                feedback = "";
                Refresh();
                return true;
            }
            return RefuseDrop(view, string.IsNullOrEmpty(model.LastFeedback) ? NoFitReason : model.LastFeedback);
        }

        bool RefuseDrop(PieceView view, string reason)
        {
            view.piece.localPosition = view.trayPosition;
            view.piece.localRotation = Quaternion.identity;
            feedback = reason;
            Refresh();
            return false;
        }

        PieceView ViewOf(string id)
        {
            foreach (var view in Views()) if (view.id == id) return view;
            return null;
        }

        /// Transforms of the crates currently loaded in the beds (locked ones included): they ride away.
        List<Transform> DockedCargo()
        {
            var cargo = new List<Transform>();
            foreach (var view in Views())
                if (view.piece != null && view.piece.gameObject.activeSelf && model.IsDocked(view.id)) cargo.Add(view.piece);
            return cargo;
        }

        /// Locked crates, and crates loaded into an accepted (departing) vehicle, cannot be grabbed.
        /// TableHandle re-enables every piece after a carry, so the rule is re-applied each frame.
        void LateUpdate()
        {
            if (!active) return;
            foreach (var view in Views())
            {
                if (view.interactables == null || view.piece == null || !view.piece.gameObject.activeInHierarchy) continue;
                bool locked = model.IsLocked(view.id) || (model.ChapterComplete && model.IsDocked(view.id));
                foreach (var behaviour in view.interactables)
                {
                    if (behaviour == null) continue;
                    if (locked)
                    {
                        if (behaviour.enabled) behaviour.enabled = false;
                        var interactable = (IInteractable)behaviour;
                        if (interactable.State != InteractableState.Disabled) interactable.Disable();
                    }
                    else if (!behaviour.enabled) behaviour.enabled = true;
                }
            }
        }

        void SnapDocked()
        {
            foreach (var view in Views())
            {
                if (view.piece == null || !model.IsDocked(view.id)) continue;
                var piece = model.Piece(view.id);
                view.piece.localPosition = RulerLayout.SnapPosition(ruler.localPosition, model.StartCell(view.id), piece.Cells, restHeight);
                view.piece.localRotation = Quaternion.identity;
            }
        }

        // ---- actions (buttons call the void wrappers; the voice guide calls Try*) ----
        public void Split() => TrySplit();
        public void Submit() => TryLoad();
        public void ResetPieces() => TryReset();
        public void NextChapter() => TryNextChapter();
        public void RestartChapter() => TryRestartChapter();

        public LessonActionResult TrySplit()
        {
            if (!active) return new LessonActionResult(false, ClosedReason);
            if (AnyHeld) return Refuse(HeldReason);
            if (model.Chapter.SplitTo <= 0) return Refuse(NoSplitReason);
            if (!model.CanSplit) return Refuse(SplitDoneReason);
            if (!model.Split(false, model.Generation)) return Refuse(SplitDoneReason);
            feedback = model.LastFeedback;
            LayoutPieces();
            Refresh();
            return new LessonActionResult(true, model.LastFeedback);
        }

        public LessonActionResult TryLoad()
        {
            if (!active) return new LessonActionResult(false, ClosedReason);
            if (AnyHeld) return Refuse(HeldReason);
            if (model.ChapterComplete)
                return new LessonActionResult(true, "This load is already checked. " + model.Chapter.Accepted);
            bool ok = model.Submit();
            feedback = model.LastFeedback;
            if (ok)
            {
                if (!completed.Contains(model.Chapter)) completed.Add(model.Chapter);
                if (vehicles != null) vehicles.DriveAway(DockedCargo());
            }
            Refresh();
            return new LessonActionResult(ok, model.LastFeedback);
        }

        public LessonActionResult TryReset()
        {
            if (!active) return new LessonActionResult(false, ClosedReason);
            if (AnyHeld) return Refuse(HeldReason);
            if (model.ChapterComplete) return Refuse(LoadedReason);
            model.ResetPieces();
            feedback = "The crates are back in the tray.";
            LayoutPieces();
            Refresh();
            return new LessonActionResult(true, feedback);
        }

        public LessonActionResult TryNextChapter()
        {
            if (!active) return new LessonActionResult(false, ClosedReason);
            if (AnyHeld) return Refuse(HeldReason);
            if (!model.ChapterComplete) return Refuse(NotCompleteReason);
            if (model.IsLastChapter) return Refuse(LastChapterReason);
            if (!model.NextChapter()) return Refuse(NotCompleteReason);
            feedback = "";
            ShowChapter();
            return new LessonActionResult(true, TitleFor(model.Chapter) + ". " + model.Chapter.Story);
        }

        public LessonActionResult TryRestartChapter()
        {
            if (!active) return new LessonActionResult(false, ClosedReason);
            if (AnyHeld) return Refuse(HeldReason);
            model.RestartChapter();
            feedback = "";
            ShowChapter();
            return new LessonActionResult(true, "Chapter restarted. " + model.Chapter.Task);
        }

        LessonActionResult Refuse(string reason)
        {
            feedback = reason;
            Refresh();
            return new LessonActionResult(false, reason);
        }

        // ---- presentation ----
        void ShowChapter()
        {
            if (vehicles != null) vehicles.ShowChapter(model.Chapter);
            if (halfMarks != null) foreach (var mark in halfMarks) if (mark != null) mark.SetActive(model.Chapter.ShowHalfMark);
            LayoutPieces();
            Refresh();
        }

        /// Activates exactly the views whose ids exist in the model (restoring crates that drove away); locked pieces
        /// sit docked in their bed, the rest wait in the tray.
        void LayoutPieces()
        {
            var ids = new HashSet<string>(model.PieceIds);
            foreach (var view in Views())
            {
                if (view.piece == null) continue;
                bool exists = ids.Contains(view.id);
                view.piece.gameObject.SetActive(exists);
                bool locked = exists && model.IsLocked(view.id);
                if (view.lockedMark != null) view.lockedMark.SetActive(locked);
                if (!exists) continue;
                var state = model.Piece(view.id);
                if (view.label != null)
                {
                    if (state.Notation.Denominator == 1) view.label.ShowWhole(1);
                    else view.label.Show(1, state.Notation.Denominator);
                }
                if (!model.IsDocked(view.id))
                {
                    view.piece.localPosition = view.trayPosition;
                    view.piece.localRotation = Quaternion.identity;
                }
            }
            SnapDocked();
        }

        void Refresh()
        {
            var chapter = model.Chapter;
            if (heading != null) heading.text = TitleFor(chapter);
            string recap = model.AllChaptersComplete ? RecapLine(completed) : "";
            if (body != null)
            {
                body.text = ComposeBody(chapter, feedback, recap);
                if (bodyBaseSize > 0) body.fontSize = FitFontSize(body, body.text, bodyBaseSize);
            }
            if (expressionLine != null) expressionLine.text = model.Expression;
            if (sayHints != null) sayHints.text = SayHintsFor(chapter, model.ChapterComplete, model.CanSplit, model.IsLastChapter);
            if (splitButton != null) splitButton.gameObject.SetActive(chapter.SplitTo > 0 && model.CanSplit);
            if (nextButton != null) nextButton.gameObject.SetActive(model.ChapterComplete && !model.IsLastChapter);
        }

        // ---- pure text helpers (unit-tested) ----
        public static string TitleFor(CargoChapter chapter) => "Chapter " + chapter.Number + " · " + chapter.Title;

        /// Story line, task line, then feedback and the dispatch recap. The guide reads this text, so it always
        /// starts with the chapter's story and task.
        public static string ComposeBody(CargoChapter chapter, string feedback, string recap)
        {
            var sb = new StringBuilder();
            sb.Append(chapter.Story).Append('\n').Append(chapter.Task);
            if (!string.IsNullOrEmpty(feedback)) sb.Append("\n\n").Append(feedback);
            if (!string.IsNullOrEmpty(recap)) sb.Append(string.IsNullOrEmpty(feedback) ? "\n\n" : "\n").Append(recap);
            return sb.ToString();
        }

        /// One honest line naming only the chapters that were actually loaded in this session.
        public static string RecapLine(IReadOnlyList<CargoChapter> done)
        {
            if (done == null || done.Count == 0) return "";
            var names = new List<string>();
            foreach (var chapter in done) names.Add(chapter.Title);
            return "Dispatched from Dock 7: " + string.Join(" · ", names) + ".";
        }

        /// Spoken phrases the guide understands for the current state, e.g. Say "split it" · "load it".
        public static string SayHintsFor(CargoChapter chapter, bool complete, bool canSplit, bool isLast)
        {
            var hints = new List<string>();
            if (complete)
            {
                if (!isLast) hints.Add("\"next chapter\"");
                hints.Add("\"start this chapter over\"");
                if (isLast) hints.Add("\"back to the lessons\"");
            }
            else
            {
                if (chapter.SplitTo > 0 && canSplit) hints.Add("\"split it\"");
                hints.Add("\"load it\"");
                hints.Add("\"reset\"");
            }
            return "Say " + string.Join(" · ", hints);
        }

        /// Largest font size (100% down to 65% of the card's base size) at which the text fits the fixed card
        /// rect, so the story card never overflows. The text itself stays free of size tags for the guide.
        public static float FitFontSize(TMP_Text target, string text, float baseSize)
        {
            if (target == null || baseSize <= 0) return baseSize;
            var rect = target.rectTransform.rect;
            if (rect.width <= 0 || rect.height <= 0) return baseSize;
            float original = target.fontSize;
            float chosen = baseSize * 0.6f;
            try
            {
                foreach (float scale in new[] { 1f, 0.9f, 0.8f, 0.72f, 0.65f })
                {
                    target.fontSize = baseSize * scale;
                    if (target.GetPreferredValues(text, rect.width, 10000f).y <= rect.height) { chosen = baseSize * scale; break; }
                }
            }
            finally { target.fontSize = original; }
            return chosen;
        }
    }
}
