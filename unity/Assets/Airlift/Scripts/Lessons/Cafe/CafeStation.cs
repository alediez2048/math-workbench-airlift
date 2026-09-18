using System.Collections;
using System.Collections.Generic;
using System.Text;
using Airlift.Presentation;
using Airlift.Presentation.Cafe;
using Airlift.Welcome;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Airlift.Lessons.Cafe
{
    /// Neighborhood Café / Corner Café workbench. All arithmetic decisions come from CafeModel; this class maps grabs,
    /// releases, buttons and voice tools to model commands and mirrors the model into the pastry pool, the plates or
    /// boxes, the story card and the payoff. Pastries released over a plate or box are placed there; anywhere else
    /// they go back to the tray. Nothing is judged until Check. The first Start of an app run shows the concept intro
    /// "What is dividing?" (ConceptIntros.Cafe) before chapter 1; its visuals pose the pieces, never the model.
    public sealed class CafeStation : LessonStation
    {
        [System.Serializable]
        public sealed class ItemView
        {
            public string id;                       // "item-1".."item-15"
            public Transform piece;
            public Grabbable grabbable;
            public GameObject croissant, cookie, muffin;
            [System.NonSerialized] public bool held;
            [System.NonSerialized] public MonoBehaviour[] interactables;
        }

        public const string CardId = "neighborhood_cafe_division";
        public const string DealTool = "cafe_deal_round", CheckTool = "cafe_check_order", ClearTool = "cafe_clear_table";
        public const string HeldReason = "Let go of the pastry first.";
        public const string ClosedReason = "The café lesson is not open.";
        public const string BriefingReason = "Start the café lesson first: say yes or press Start.";
        public const string CheckedReason = "This order is already checked. Go to the next chapter or start this one over.";
        public const string NotCompleteReason = "Check the order first.";
        public const string LastChapterReason = "That was the last chapter.";
        public const string ClearedReason = "The pastries are back on the tray.";
        public const string WelcomeLine = "Welcome to the Corner Café.";
        public const string StartLine = "Say yes or press Start.";
        public const string IntroReason = "Let's finish the intro first: say next or press Next.";
        static readonly string[] IntroTools = { "advance_step", "request_help", "back_to_lessons" };

        [Tooltip("Station-local space for releases and layout; the café root when empty.")]
        public Transform space;
        public LessonTheme theme;
        [Header("Pastry pool (ids item-1..item-15)")]
        public ItemView[] items = new ItemView[0];
        [Header("Drop targets")]
        public Transform[] plates = new Transform[0];
        public Transform[] boxes = new Transform[0];
        public TMP_Text[] plateLabels = new TMP_Text[0];
        public TMP_Text[] boxLabels = new TMP_Text[0];
        public TMP_Text[] boxCapacityLabels = new TMP_Text[0];
        public GameObject[] orderUpTags = new GameObject[0];     // one per box, child of the box
        public GameObject piecesRoot;                              // hidden during the briefing
        public CafePayoff payoff;
        [Header("Card")]
        public TMP_Text expressionLine;
        public TMP_Text sayHints;
        public Button startButton, dealButton, checkButton, clearButton, nextButton, backButton;

        CafeModel model;
        bool open, briefing = true, initialised;
        int introIndex = -1;                                       // step on the card, -1 outside the intro
        bool introSeen;                                            // finished once this app run
        float bodyBaseSize;
        string feedback = "";
        readonly List<string>[] order = { new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>() };
        readonly Dictionary<string, ItemView> byId = new Dictionary<string, ItemView>();

        // ---- LessonStation ----
        public override string Title => "Neighborhood Café";
        public override string StoryName => "Corner Café";
        public override bool IsOpen => open;
        public override bool ChapterActive => open && !briefing;   // the intro is part of the briefing
        static IReadOnlyList<IntroStep> Intro => ConceptIntros.Cafe;
        bool InIntro => open && introIndex >= 0 && introIndex < Intro.Count;
        public override IntroStep CurrentIntro => InIntro ? Intro[introIndex] : null;
        bool LastIntroStep => introIndex == Intro.Count - 1;
        public override bool AnyHeld { get { if (items != null) foreach (var v in items) if (v != null && v.held) return true; return false; } }

        /// Read-only view of the engine for tests and editor previews. Buttons and tools go through the actions.
        public CafeModel Model { get { EnsureInit(); return model; } }
        public string Feedback => feedback;

        public override LessonChapterFacts Chapter
        {
            get
            {
                if (!ChapterActive) return default;
                var c = model.Chapter;
                return new LessonChapterFacts(c.Number, c.Id, c.Title, c.Story, model.StageTask, c.Accepted,
                    model.Expression, feedback, model.ChapterComplete, model.IsLastChapter);
            }
        }

        public override GuideStep CurrentStep()
        {
            if (!open) return new GuideStep("cards", "The lesson cards are showing; no table.", false);
            EnsureInit();
            if (InIntro) return CafeSteps.Intro(CurrentIntro, LastIntroStep);
            return briefing ? CafeSteps.Briefing() : CafeSteps.ForChapter(model);
        }

        public override string[] ToolNames => new[] { DealTool, CheckTool, ClearTool };

        public override string[] ToolsNow
        {
            get
            {
                if (!open) return new string[0];
                EnsureInit();
                if (InIntro) return (string[])IntroTools.Clone();
                return ToolsFor(briefing, model.Stage.Kind, model.ChapterComplete, model.IsLastChapter);
            }
        }

        /// Shared and café tools that make sense in a state. Pure; unit-tested.
        public static string[] ToolsFor(bool inBriefing, CafeTargetKind kind, bool complete, bool isLast)
        {
            var tools = new List<string> { "request_help" };
            if (inBriefing) tools.Add("advance_step");
            else if (complete)
            {
                if (!isLast) { tools.Add("advance_step"); tools.Add("next_chapter"); }
                tools.Add("restart_chapter");
            }
            else
            {
                if (kind == CafeTargetKind.Plates) tools.Add(DealTool);
                tools.Add(CheckTool); tools.Add(ClearTool); tools.Add("restart_chapter");
            }
            tools.Add("back_to_lessons");
            return tools.ToArray();
        }

        // ---- lifecycle ----
        void Awake() => EnsureInit();

        void EnsureInit()
        {
            if (initialised) return;
            initialised = true;
            model = new CafeModel();
            byId.Clear();
            if (items != null)
                foreach (var view in items)
                {
                    if (view == null || string.IsNullOrEmpty(view.id)) continue;
                    byId[view.id] = view;
                    CacheInteractables(view);
                    if (view.grabbable != null) { var v = view; view.grabbable.WhenPointerEventRaised += evt => OnPointer(v, evt); }
                }
            if (body != null && bodyBaseSize <= 0) bodyBaseSize = body.fontSize;
        }

        static void CacheInteractables(ItemView view)
        {
            if (view.piece == null) return;
            var list = new List<MonoBehaviour>();
            foreach (var behaviour in view.piece.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour is IInteractable) list.Add(behaviour);
            view.interactables = list.ToArray();
        }

        /// Shows the café and its briefing card. Coming back after an accepted chapter continues with the next one.
        public override void Open()
        {
            EnsureInit();
            open = true;
            briefing = true;
            introIndex = -1;
            feedback = "";
            ReleaseHolds();
            SetVisualsActive(true);
            if (piecesRoot != null) piecesRoot.SetActive(false);
            ShowStage();
        }

        /// A chapter tile on the wall: open, then straight into that chapter. Chapter 0 is Open() as today.
        public override void Open(int chapterIndex)
        {
            Open();
            if (chapterIndex <= 0) return;
            JumpToChapter(Mathf.Clamp(chapterIndex - 1, 0, CafeChapter.All.Count - 1));
        }

        /// Hides the café, stops the payoff and puts every pastry of an unfinished order back on the tray.
        public override void Close()
        {
            EnsureInit();
            StopAllCoroutines();
            if (payoff != null) payoff.ResetAll();
            ReleaseHolds();
            bool wasIntro = InIntro;
            introIndex = -1;                 // an unfinished intro counts as not seen
            if (!model.ChapterComplete) model.ResetTable();
            ClearOrder();
            feedback = "";
            if (wasIntro) PlaceStage();      // undo the intro's plates or boxes
            if (open) LayoutAll();
            open = false;
            briefing = true;
            if (expressionLine != null) expressionLine.text = "";
            if (sayHints != null) sayHints.text = "";
            if (body != null && bodyBaseSize > 0) body.fontSize = bodyBaseSize;
            SetVisualsActive(false);
        }

        void ReleaseHolds() { if (items != null) foreach (var v in items) if (v != null) v.held = false; }

        /// Development and editor preview only: jump straight into a chapter. Not wired to any button or tool.
        public void JumpToChapter(int index)
        {
            EnsureInit();
            if (payoff != null) payoff.ResetAll();
            model.StartChapter(index);
            briefing = false;
            introIndex = -1;
            feedback = "";
            if (piecesRoot != null) piecesRoot.SetActive(true);
            ClearOrder();
            ShowStage();
        }

        // ---- grabbing ----
        void OnPointer(ItemView view, PointerEvent evt)
        {
            if (evt.Type == PointerEventType.Select)
            {
                if (!ChapterActive || Locked) return;
                view.held = true;
                model.Remove(view.id);
            }
            else if (evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
            {
                // Always clear the hold, even if the lesson left the chapter meanwhile, so no action stays refused.
                bool wasHeld = view.held;
                view.held = false;
                if (wasHeld && ChapterActive && isActiveAndEnabled) StartCoroutine(SettleAfterRelease(view));
            }
        }

        IEnumerator SettleAfterRelease(ItemView view)
        {
            yield return null; // let the SDK finish transferring ownership before the pastry moves
            if (!ChapterActive || view.held || view.piece == null) yield break;
            DropAt(view.id, Space.InverseTransformPoint(view.piece.position));
        }

        Transform Space => space != null ? space : transform;
        bool Locked => model.ChapterComplete || (payoff != null && payoff.Busy);

        /// A released pastry at a station-local position: the plate or box under it takes it, otherwise it goes back
        /// to the tray. A refused placement (a full box) also goes back to the tray and the card says why. Public for
        /// tests and editor drives.
        public bool DropAt(string itemId, Vector3 stationLocal)
        {
            EnsureInit();
            if (!ChapterActive || itemId == null || !byId.TryGetValue(itemId, out var view) || view.piece == null) return false;
            if (!ContainsId(itemId)) return false;
            if (payoff != null) payoff.FinishNow();
            if (model.ChapterComplete) { feedback = CheckedReason; LayoutAll(); Refresh(); return false; }
            int container = CafeLayout.NearestContainer(model.Stage.Kind, model.ContainerCount, stationLocal);
            if (container < 0)
            {
                model.Remove(itemId);
                LayoutAll(); Refresh();
                return false;
            }
            if (model.Place(itemId, false, container))
            {
                feedback = "";
                LayoutAll(); Refresh();
                return true;
            }
            string reason = model.LastFeedback;
            model.Remove(itemId);
            feedback = string.IsNullOrEmpty(reason) ? model.ContainerName(container) + " cannot take that." : reason;
            LayoutAll(); Refresh();
            return false;
        }

        bool ContainsId(string id) { foreach (var i in model.ItemIds) if (i == id) return true; return false; }

        /// Accepted and departing pastries cannot be grabbed. TableHandle re-enables every piece after a carry, so the
        /// rule is re-applied each frame.
        void LateUpdate()
        {
            if (!open || items == null) return;
            bool locked = briefing || Locked;
            foreach (var view in items)
            {
                if (view?.interactables == null || view.piece == null || !view.piece.gameObject.activeInHierarchy) continue;
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

        // ---- actions (buttons call the Press* wrappers; the guide calls the result methods) ----
        public void PressStart() => Advance();
        public void PressDealRound() => DealRound();
        public void PressCheck() => Check();
        public void PressClear() => ResetTable();
        public void PressNextChapter() => NextChapter();

        public override LessonActionResult Advance()
        {
            EnsureInit();
            if (!open) return new LessonActionResult(false, ClosedReason);
            if (InIntro)
            {
                if (!LastIntroStep) return ShowIntro(introIndex + 1);
                introIndex = -1;
                introSeen = true;
                return StartFromBriefing();
            }
            if (!briefing) return NextChapter();
            if (!introSeen && Intro.Count > 0) return ShowIntro(0);
            return StartFromBriefing();
        }

        /// Briefing (or the intro's last step) -> the chapter the café resumes at.
        LessonActionResult StartFromBriefing()
        {
            if (model.AllChaptersComplete) model.StartChapter(0);
            else if (model.ChapterComplete) model.NextChapter();
            else model.ResetTable();
            briefing = false;
            feedback = "";
            if (piecesRoot != null) piecesRoot.SetActive(true);
            ClearOrder();
            ShowStage();
            return new LessonActionResult(true, TitleFor(model.Chapter) + ". " + model.Chapter.Story + " " + model.StageTask);
        }

        public LessonActionResult DealRound()
        {
            if (!Ready(out var refusal)) return refusal;
            if (model.ChapterComplete) return Refuse(CheckedReason);
            if (!model.DealRound(false)) return Refuse(string.IsNullOrEmpty(model.LastFeedback) ? "There is nothing to deal." : model.LastFeedback);
            feedback = model.LastFeedback;
            LayoutAll(); Refresh();
            return new LessonActionResult(true, feedback);
        }

        public override LessonActionResult Check()
        {
            if (!Ready(out var refusal)) return refusal;
            if (model.ChapterComplete) return new LessonActionResult(true, "This order is already checked. " + model.Chapter.Accepted);
            var kind = model.Stage.Kind;
            int stageBefore = model.StageIndex, chapterBefore = model.ChapterIndex;
            var groups = CaptureGroups(kind);
            var tags = new List<GameObject>();
            if (kind == CafeTargetKind.Boxes)
                for (int b = 0; b < model.ContainerCount; b++)
                    if (model.CountIn(b) > 0 && b < orderUpTags.Length && orderUpTags[b] != null) tags.Add(orderUpTags[b]);
            bool ok = model.Check();
            feedback = model.LastFeedback;
            if (!ok) { Refresh(); return new LessonActionResult(false, feedback); }
            bool stageChanged = !model.ChapterComplete && model.StageIndex != stageBefore;
            Refresh();
            if (payoff == null) { if (stageChanged) ShowStage(); }
            else if (kind == CafeTargetKind.Plates)
                payoff.Serve(groups, stageChanged ? (System.Action)(() => AfterStagePayoff(chapterBefore, stageBefore + 1)) : null);
            else
                payoff.Ship(groups, tags, stageChanged ? (System.Action)(() => AfterStagePayoff(chapterBefore, stageBefore + 1)) : null);
            return new LessonActionResult(true, feedback);
        }

        /// Chapter 5: after the plates are served, the same pastries come back to the tray and the boxes come out.
        void AfterStagePayoff(int chapter, int stage)
        {
            if (!open || model.ChapterIndex != chapter || model.StageIndex != stage) return;
            ClearOrder();
            ShowStage();
        }

        public override LessonActionResult ResetTable()
        {
            if (!Ready(out var refusal)) return refusal;
            if (model.ChapterComplete) return Refuse(CheckedReason);
            model.ResetTable();
            feedback = ClearedReason;
            ClearOrder();
            LayoutAll(); Refresh();
            return new LessonActionResult(true, feedback);
        }

        public override LessonActionResult NextChapter()
        {
            if (!Ready(out var refusal)) return refusal;
            if (!model.ChapterComplete) return Refuse(NotCompleteReason);
            if (model.IsLastChapter) return Refuse(LastChapterReason);
            if (!model.NextChapter()) return Refuse(NotCompleteReason);
            feedback = "";
            ClearOrder();
            ShowStage();
            return new LessonActionResult(true, TitleFor(model.Chapter) + ". " + model.Chapter.Story + " " + model.StageTask);
        }

        public override LessonActionResult RestartChapter()
        {
            if (!Ready(out var refusal)) return refusal;
            model.RestartChapter();
            feedback = "";
            ClearOrder();
            ShowStage();
            return new LessonActionResult(true, "Chapter restarted. " + model.StageTask);
        }

        public override bool TryLessonTool(string name, Newtonsoft.Json.Linq.JObject args, out LessonActionResult result)
        {
            switch (name)
            {
                case DealTool: result = DealRound(); return true;
                case CheckTool: result = Check(); return true;
                case ClearTool: result = ResetTable(); return true;
                default: result = new LessonActionResult(false, "That is not part of " + Title + "."); return false;
            }
        }

        /// Shared gate for every in-chapter action: open, past the briefing, nothing in a hand. Finishes a running
        /// payoff first so the next action starts from a settled table.
        bool Ready(out LessonActionResult refusal)
        {
            EnsureInit();
            refusal = default;
            if (!open) { refusal = new LessonActionResult(false, ClosedReason); return false; }
            if (InIntro) { refusal = Refuse(IntroReason); return false; }
            if (briefing) { refusal = Refuse(BriefingReason); return false; }
            if (AnyHeld) { refusal = Refuse(HeldReason); return false; }
            if (payoff != null) payoff.FinishNow();
            return true;
        }

        LessonActionResult Refuse(string reason)
        {
            feedback = reason;
            Refresh();
            return new LessonActionResult(false, reason);
        }

        List<CafeRideGroup> CaptureGroups(CafeTargetKind kind)
        {
            var groups = new List<CafeRideGroup>();
            var containers = kind == CafeTargetKind.Plates ? plates : boxes;
            for (int c = 0; c < model.ContainerCount; c++)
            {
                if (containers == null || c >= containers.Length || containers[c] == null || model.CountIn(c) == 0) continue;
                var labels = kind == CafeTargetKind.Plates ? plateLabels : boxLabels;
                var group = new CafeRideGroup { container = containers[c], label = labels != null && c < labels.Length && labels[c] != null ? labels[c].transform : null };
                foreach (var id in model.ItemIds)
                    if (model.ContainerOf(id) == c && byId.TryGetValue(id, out var view) && view.piece != null) group.items.Add(view.piece);
                groups.Add(group);
            }
            return groups;
        }

        // ---- presentation ----
        /// Puts this stage's plates or boxes out, hides the rest, restores the payoff props, lays out the pastries.
        void ShowStage()
        {
            if (payoff != null) payoff.ResetAll();
            PlaceStage();
            LayoutAll();
            Refresh();
        }

        void PlaceStage() => PlaceTargets(model.Stage.Kind, model.ContainerCount, model.Stage.BoxCapacity);

        void PlaceTargets(CafeTargetKind kind, int count, int capacity)
        {
            PlaceContainers(plates, plateLabels, CafeTargetKind.Plates, kind == CafeTargetKind.Plates ? count : 0, "Plate ");
            PlaceContainers(boxes, boxLabels, CafeTargetKind.Boxes, kind == CafeTargetKind.Boxes ? count : 0, "Box ");
            if (boxCapacityLabels != null)
                foreach (var label in boxCapacityLabels) if (label != null) label.text = capacity.ToString();
            if (orderUpTags != null) foreach (var tag in orderUpTags) if (tag != null) tag.SetActive(false);
        }

        /// Puts intro step index on the card and poses its visual: croissants on the tray, dealt onto plates or packed
        /// into boxes. CafeModel is never touched, so the chapter starts from a clean table afterwards.
        LessonActionResult ShowIntro(int index)
        {
            introIndex = index;
            feedback = "";
            var step = Intro[index];
            var scene = CafeLayout.IntroScene(step.Visual);
            if (payoff != null) payoff.ResetAll();
            PlaceTargets(scene.Kind, scene.Containers, scene.BoxCapacity);
            if (piecesRoot != null) piecesRoot.SetActive(true);
            for (int i = 0; items != null && i < items.Length; i++)
            {
                var view = items[i]; if (view?.piece == null) continue;
                bool on = i < scene.Items;
                view.piece.gameObject.SetActive(on);
                if (!on) continue;
                ShowShape(view, CafeLayout.ShapeFor(scene.ItemName, i));
                SetLocal(view.piece, CafeLayout.IntroItemPosition(scene, i));
                view.piece.localRotation = Quaternion.identity;
                view.piece.localScale = Vector3.one;
            }
            Refresh();
            return new LessonActionResult(true, step.Say);
        }

        static void ShowShape(ItemView view, CafeItemShape shape)
        {
            if (view.croissant != null) view.croissant.SetActive(shape == CafeItemShape.Croissant);
            if (view.cookie != null) view.cookie.SetActive(shape == CafeItemShape.Cookie);
            if (view.muffin != null) view.muffin.SetActive(shape == CafeItemShape.Muffin);
        }

        void PlaceContainers(Transform[] containers, TMP_Text[] labels, CafeTargetKind kind, int count, string prefix)
        {
            for (int i = 0; containers != null && i < containers.Length; i++)
            {
                var t = containers[i]; if (t == null) continue;
                bool on = i < count;
                if (on)
                {
                    SetLocal(t, CafeLayout.ContainerCenter(kind, i, count));
                    t.localRotation = Quaternion.identity; t.localScale = Vector3.one;
                }
                t.gameObject.SetActive(on);
            }
            for (int i = 0; labels != null && i < labels.Length; i++)
            {
                var label = labels[i]; if (label == null) continue;
                bool on = i < count;
                if (on) { SetLocal(label.transform, CafeLayout.LabelPosition(kind, CafeLayout.ContainerCenter(kind, i, count))); label.text = prefix + (i + 1); }
                label.gameObject.SetActive(on);
            }
        }

        void SetLocal(Transform t, Vector3 stationLocal)
        {
            if (t.parent == Space) t.localPosition = stationLocal;
            else t.position = Space.TransformPoint(stationLocal);
        }

        void ClearOrder() { foreach (var list in order) list.Clear(); }

        /// Keeps each container's pastries in the order they arrived so nothing on a plate jumps when one more lands.
        void SyncOrder()
        {
            for (int c = 0; c < order.Length; c++)
            {
                order[c].RemoveAll(id => model.ContainerOf(id) != c);
                if (c >= model.ContainerCount) continue;
                foreach (var id in model.ItemIds) if (model.ContainerOf(id) == c && !order[c].Contains(id)) order[c].Add(id);
            }
        }

        /// Shows exactly the pastries of this chapter in its shape, each on the tray or in its container slot.
        void LayoutAll()
        {
            SyncOrder();
            var kind = model.Stage.Kind; int count = model.ContainerCount;
            var ids = model.ItemIds;
            var index = new Dictionary<string, int>();
            for (int i = 0; i < ids.Count; i++) index[ids[i]] = i;
            if (items == null) return;
            foreach (var view in items)
            {
                if (view?.piece == null) continue;
                bool exists = index.TryGetValue(view.id ?? "", out int n);
                view.piece.gameObject.SetActive(exists);
                if (!exists) continue;
                ShowShape(view, CafeLayout.ShapeFor(model.Chapter.ItemName, n));
                if (view.held) continue;
                int c = model.ContainerOf(view.id);
                Vector3 p = c < 0 ? CafeLayout.TrayPosition(n)
                    : CafeLayout.SlotPosition(kind, CafeLayout.ContainerCenter(kind, c, count), order[c].IndexOf(view.id));
                SetLocal(view.piece, p);
                view.piece.localRotation = Quaternion.identity;
                view.piece.localScale = Vector3.one;
            }
        }

        void Refresh()
        {
            if (!open) return;
            if (InIntro) { RefreshIntro(); return; }
            SetStartLabel("Start");
            var chapter = model.Chapter;
            bool complete = model.ChapterComplete, plateStage = model.Stage.Kind == CafeTargetKind.Plates;
            if (heading != null) heading.text = briefing ? Title + " · " + StoryName : TitleFor(chapter);
            if (body != null)
            {
                body.text = briefing ? BriefingBody(ResumeChapter()) : ComposeBody(chapter.Story, model.StageTask, feedback);
                if (bodyBaseSize > 0) body.fontSize = CargoLessonDirector.FitFontSize(body, body.text, bodyBaseSize);
            }
            if (expressionLine != null) expressionLine.text = briefing ? "" : model.Expression;
            if (sayHints != null) sayHints.text = SayHintsFor(briefing, plateStage, complete, model.IsLastChapter);
            Show(startButton, briefing);
            Show(dealButton, !briefing && !complete && plateStage);
            Show(checkButton, !briefing && !complete);
            Show(clearButton, !briefing && !complete);
            Show(nextButton, !briefing && complete && !model.IsLastChapter);
            Show(backButton, true);
        }

        void RefreshIntro()
        {
            var step = CurrentIntro;
            if (heading != null) heading.text = step.Heading;
            if (body != null)
            {
                body.text = step.Say;
                if (bodyBaseSize > 0) body.fontSize = CargoLessonDirector.FitFontSize(body, body.text, bodyBaseSize);
            }
            if (expressionLine != null) expressionLine.text = step.Expression ?? "";
            if (sayHints != null) sayHints.text = IntroHintsFor(LastIntroStep);
            Show(startButton, true);
            SetStartLabel(LastIntroStep ? "Start" : "Next");
            Show(dealButton, false); Show(checkButton, false); Show(clearButton, false); Show(nextButton, false);
            Show(backButton, true);
        }

        void SetStartLabel(string text)
        {
            var label = startButton != null ? startButton.GetComponentInChildren<TMP_Text>(true) : null;
            if (label != null && label.text != text) label.text = text;
        }

        static void Show(Button button, bool on) { if (button != null && button.gameObject.activeSelf != on) button.gameObject.SetActive(on); }

        /// The chapter Advance will enter from the briefing, or null when that is chapter 1 from the start.
        CafeChapter ResumeChapter()
        {
            if (model.AllChaptersComplete) return null;
            if (model.ChapterComplete) return CafeChapter.All[model.ChapterIndex + 1];
            return model.ChapterIndex > 0 || model.StageIndex > 0 ? model.Chapter : null;
        }

        // ---- pure text helpers (unit-tested) ----
        public static string TitleFor(CafeChapter chapter) => "Chapter " + chapter.Number + " · " + chapter.Title;

        /// Story line, task line, then feedback. The guide reads this text, so it always starts with story and task.
        public static string ComposeBody(string story, string task, string feedback)
        {
            var sb = new StringBuilder();
            sb.Append(story).Append('\n').Append(task);
            if (!string.IsNullOrEmpty(feedback)) sb.Append("\n\n").Append(feedback);
            return sb.ToString();
        }

        public static string BriefingBody(CafeChapter resume)
        {
            var sb = new StringBuilder(WelcomeLine).Append(' ').Append(CafeChapter.BriefingStory);
            if (resume != null) sb.Append("\nWe pick up at ").Append(TitleFor(resume)).Append('.');
            return sb.Append('\n').Append(StartLine).ToString();
        }

        /// Spoken phrase that moves the concept intro on.
        public static string IntroHintsFor(bool lastStep) => lastStep ? "Say \"start\"" : "Say \"next\"";

        /// Spoken phrases the guide understands right now, e.g. Say "deal a round" · "check the order".
        public static string SayHintsFor(bool inBriefing, bool plates, bool complete, bool isLast)
        {
            var hints = new List<string>();
            if (inBriefing) hints.Add("\"yes\"");
            else if (complete)
            {
                if (!isLast) hints.Add("\"next chapter\"");
                hints.Add("\"start this chapter over\"");
                if (isLast) hints.Add("\"back to the lessons\"");
            }
            else
            {
                if (plates) hints.Add("\"deal a round\"");
                hints.Add("\"check the order\"");
                hints.Add("\"clear the table\"");
            }
            return "Say " + string.Join(" · ", hints);
        }
    }
}
