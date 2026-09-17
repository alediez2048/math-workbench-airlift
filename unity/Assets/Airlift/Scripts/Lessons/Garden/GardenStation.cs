using System.Collections;
using System.Collections.Generic;
using System.Text;
using Airlift.Presentation.Garden;
using Airlift.Welcome;
using Newtonsoft.Json.Linq;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Airlift.Lessons.Garden
{
    /// Community Garden / Sunny Plot. Presentation adapter only: every arithmetic decision comes from GardenModel.
    /// Maps strip and fence grabs, buttons and voice tools to model commands and mirrors the model into the bed,
    /// the seedling strip pool, the fence and the Garden card. Accepted beds grow into flowers and vegetables.
    public sealed class GardenStation : LessonStation
    {
        [System.Serializable]
        public sealed class StripView
        {
            public Transform piece;
            public Grabbable grabbable;
            public BoxCollider collider;
            public MeshFilter bodyLeft, bodyRight;
            [Tooltip("Seedling k: its group, the sprout inside it, and the hidden bloom it grows into.")]
            public Transform[] seedlings = new Transform[0];
            public Transform[] sprouts = new Transform[0];
            public Transform[] blooms = new Transform[0];
            [Tooltip("Station-local left end of this strip's tray slot (y = tray rest height).")]
            public Vector3 trayLeft;
            [System.NonSerialized] public string id;
            [System.NonSerialized] public int length;
            [System.NonSerialized] public bool held;
            [System.NonSerialized] public MonoBehaviour[] interactables;
        }

        public const string LessonTitle = "Community Garden";
        public const string Story = "Sunny Plot";
        public const string ToolTurn = "garden_turn_bed", ToolSplit = "garden_split_bed", ToolCheck = "garden_check_bed", ToolClear = "garden_clear_bed";
        public const string HeldReason = "Let go of the seedlings first.";
        public const string FenceHeldReason = "Let go of the fence first.";
        public const string ClosedReason = "The garden lesson is not open.";
        public const string BriefingReason = "Start the first chapter first: say yes or press Start.";
        public const string TurningReason = "The bed is still turning.";
        public const string CheckedReason = "This bed is already checked. Go to the next chapter or start this one over.";
        public const string NotCompleteReason = "Plant the bed and check it first.";
        public const string LastChapterReason = "That was the last chapter.";
        public const string NoTurnReason = "Plant every row with a full strip before turning the bed.";
        public const string NoFenceReason = "There is no fence in this chapter.";
        public const string FenceWhereReason = "Say where the fence goes, for example split it at five.";
        public const string BriefingHeading = "Sunny Plot · Community Garden";
        public const string BriefingBody = "Welcome to Sunny Plot, the neighborhood's shared garden. Seedlings come in strips, and each strip plants one row of the bed.\nPlant equal rows, turn the bed and split it with a fence to see how many seedlings grow.\nSay yes or press Start.";

        [Header("Station")]
        public Transform stationRoot;
        public Airlift.Presentation.LessonTheme theme;
        public GardenBedView bed;
        public GameObject pieces;
        public StripView[] strips = new StripView[0];
        [Tooltip("Index n: strip body covering n seedlings.")]
        public Mesh[] stripMeshes = new Mesh[GardenBedLayout.MaxColumns + 1];
        public float trayHeight = 0.0315f;
        public float stripDepth = 0.04f, stripHeight = 0.04f;
        public float splitGap = 0.03f;
        public float turnSeconds = 0.8f;
        [Header("Fence")]
        public Transform fence;
        public Grabbable fenceGrabbable;
        public Vector3 fenceHome = new Vector3(0.215f, 0.0175f, -0.03f);
        public float deckTop = 0.0175f;
        [Header("Payoff")]
        public Transform wateringCan;
        public Vector3 wateringCanHome;
        public Transform butterfly;
        public Vector3 butterflyHome;
        public TMP_Text[] partLabels = new TMP_Text[2];
        [Tooltip("Distance of the part labels in front of the bed's front wall face.")]
        public float partLabelOffset = 0.03f;
        [Header("Card")]
        public TMP_Text expressionLine;
        public TMP_Text sayHints;
        public Button startButton, turnButton, checkButton, clearButton, nextButton, backButton;

        readonly GardenModel model = new GardenModel();
        bool active, briefing, animating, payoffShown, initialized, fenceHeld;
        float bodyBaseSize;
        string feedback = "";
        Coroutine running;
        MonoBehaviour[] fenceInteractables;

        // ---- LessonStation ----
        public override string Title => LessonTitle;
        public override string StoryName => Story;
        public override bool IsOpen => active;
        public override bool ChapterActive => active && !briefing;
        public override bool AnyHeld { get { if (!active) return false; if (fenceHeld) return true; foreach (var v in Views()) if (v.held) return true; return false; } }
        public override string[] ToolNames => new[] { ToolTurn, ToolSplit, ToolCheck, ToolClear };

        public override LessonChapterFacts Chapter
        {
            get
            {
                if (!ChapterActive) return default;
                var c = model.Chapter;
                return new LessonChapterFacts(c.Number, c.Id, c.Title, c.Story, c.Task, c.Accepted, model.Expression, feedback, model.ChapterComplete, model.IsLastChapter);
            }
        }

        public override GuideStep CurrentStep()
        {
            if (!active) return new GuideStep("cards", "The lesson cards are showing; no table.", false);
            if (briefing) return GardenSteps.Briefing();
            return GardenSteps.ForChapter(model);
        }

        public override string[] ToolsNow
        {
            get
            {
                var tools = new List<string> { "request_help" };
                if (!active) return tools.ToArray();
                if (briefing) { tools.Add("advance_step"); tools.Add("back_to_lessons"); return tools.ToArray(); }
                var c = model.Chapter;
                if (model.ChapterComplete)
                {
                    if (!model.IsLastChapter) { tools.Add("advance_step"); tools.Add("next_chapter"); }
                }
                else
                {
                    if (CanTurnNow) tools.Add(ToolTurn);
                    if (c.HasFence) tools.Add(ToolSplit);
                    tools.Add(ToolCheck);
                    tools.Add(ToolClear);
                }
                tools.Add("restart_chapter");
                tools.Add("back_to_lessons");
                return tools.ToArray();
            }
        }

        bool CanTurnNow => ChapterActive && !model.ChapterComplete && model.Chapter.NeedsTurn && !model.Turned;

        // ---- public state for tests, previews and the guide ----
        public GardenModel Model => model;
        public bool InBriefing => active && briefing;
        public bool Animating => animating;
        public bool PayoffShown => payoffShown;
        public string Feedback => feedback;
        public string ExpressionText => model.Expression;
        public GardenBedLayout BedLayout => bed != null ? bed.LayoutFor(model.Rows, model.Columns) : new GardenBedLayout(new Vector3(0, 0.0415f, -0.03f), 0.045f, model.Rows, model.Columns);

        public StripView ViewOf(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var v in Views()) if (v.id == id) return v;
            return null;
        }

        IEnumerable<StripView> Views()
        {
            if (strips == null) yield break;
            foreach (var v in strips) if (v != null) yield return v;
        }

        // ---- lifecycle ----
        void Awake()
        {
            EnsureInit();
            foreach (var v in Views()) if (v.grabbable != null) { var view = v; view.grabbable.WhenPointerEventRaised += evt => OnStripPointer(view, evt); }
            if (fenceGrabbable != null) fenceGrabbable.WhenPointerEventRaised += OnFencePointer;
        }

        void EnsureInit()
        {
            if (initialized) return;
            initialized = true;
            if (stationRoot == null) stationRoot = transform;
            foreach (var v in Views()) v.interactables = InteractablesOf(v.piece);
            fenceInteractables = InteractablesOf(fence);
            if (body != null && bodyBaseSize <= 0) bodyBaseSize = body.fontSize;
        }

        static MonoBehaviour[] InteractablesOf(Transform root)
        {
            var list = new List<MonoBehaviour>();
            if (root != null) foreach (var b in root.GetComponentsInChildren<MonoBehaviour>(true)) if (b is IInteractable) list.Add(b);
            return list.ToArray();
        }

        public override void Open()
        {
            EnsureInit();
            StopRunning();
            active = true; briefing = true; feedback = "";
            fenceHeld = false;
            foreach (var v in Views()) v.held = false;
            SetRoots(true);
            RestorePayoff();
            LayoutAll();
            Refresh();
        }

        public override void Close()
        {
            EnsureInit();
            StopRunning();
            RestorePayoff();
            fenceHeld = false;
            foreach (var v in Views()) v.held = false;
            if (!model.ChapterComplete) model.RestartChapter();
            bool wasActive = active;
            active = false; briefing = false; feedback = "";
            LayoutAll();
            if (expressionLine != null) expressionLine.text = "";
            if (sayHints != null) sayHints.text = "";
            if (body != null && bodyBaseSize > 0) body.fontSize = bodyBaseSize;
            SetRoots(false);
            if (wasActive) Debug.Log("[Nerdy] Garden closed at chapter " + model.Chapter.Number);
        }

        void SetRoots(bool on)
        {
            if (visualRoots == null) return;
            foreach (var root in visualRoots) if (root != null) root.SetActive(on);
        }

        void StopRunning()
        {
            if (running != null) StopCoroutine(running);
            running = null;
            animating = false;
            if (bed != null && bed.turnable != null) bed.turnable.localRotation = Quaternion.identity;
            if (bed != null) bed.ShowLabels(true);
        }

        // ---- shared actions ----
        public override LessonActionResult Advance()
        {
            if (!active) return new LessonActionResult(false, ClosedReason);
            if (AnyHeld) return Refuse(HeldReasonNow());
            if (!briefing) return NextChapter();
            briefing = false;
            if (model.AllChaptersComplete || (model.ChapterComplete && model.IsLastChapter)) model.StartChapter(0);
            else if (model.ChapterComplete) model.NextChapter();
            else model.RestartChapter();
            feedback = "";
            ShowChapter();
            return new LessonActionResult(true, TitleFor(model.Chapter) + ". " + model.Chapter.Story + " " + model.Chapter.Task);
        }

        public override LessonActionResult Check()
        {
            var gate = Gate(); if (gate.HasValue) return gate.Value;
            if (model.ChapterComplete) return new LessonActionResult(true, "This bed is already checked. " + model.Chapter.Accepted);
            bool ok = model.Check();
            feedback = model.LastFeedback;
            if (ok) StartPayoff(); else { LayoutAll(); Refresh(); }
            return new LessonActionResult(ok, model.LastFeedback);
        }

        public override LessonActionResult ResetTable()
        {
            var gate = Gate(); if (gate.HasValue) return gate.Value;
            if (model.ChapterComplete) return Refuse(CheckedReason);
            model.ResetTable();
            feedback = model.Chapter.StartsPlanted ? "The bed is back the way it started." : "The seedling strips are back in the tray.";
            LayoutAll();
            Refresh();
            return new LessonActionResult(true, feedback);
        }

        public override LessonActionResult NextChapter()
        {
            var gate = Gate(); if (gate.HasValue) return gate.Value;
            if (!model.ChapterComplete) return Refuse(NotCompleteReason);
            if (model.IsLastChapter) return Refuse(LastChapterReason);
            if (!model.NextChapter()) return Refuse(NotCompleteReason);
            feedback = "";
            ShowChapter();
            return new LessonActionResult(true, TitleFor(model.Chapter) + ". " + model.Chapter.Story);
        }

        public override LessonActionResult RestartChapter()
        {
            var gate = Gate(); if (gate.HasValue) return gate.Value;
            model.RestartChapter();
            feedback = "";
            ShowChapter();
            return new LessonActionResult(true, "Chapter restarted. " + model.Chapter.Task);
        }

        public override bool TryLessonTool(string name, JObject args, out LessonActionResult result)
        {
            switch (name)
            {
                case ToolTurn: result = TurnBed(); return true;
                case ToolSplit: result = SplitAt(ColumnsArg(args)); return true;
                case ToolCheck: result = Check(); return true;
                case ToolClear: result = ResetTable(); return true;
                default: result = default; return false;
            }
        }

        static int ColumnsArg(JObject args)
        {
            var token = args?["columns"];
            if (token == null) return 0;
            try
            {
                if (token.Type == JTokenType.Integer) return token.Value<int>();
                if (token.Type == JTokenType.Float) { double d = token.Value<double>(); return System.Math.Abs(d - System.Math.Round(d)) < 1e-6 ? (int)System.Math.Round(d) : -1; }
                if (token.Type == JTokenType.String && int.TryParse(token.Value<string>(), out int n)) return n;
            }
            catch (System.Exception) { }
            return -1;
        }

        // ---- garden actions (buttons call the void wrappers) ----
        public void PressStart() => Advance();
        public void PressTurn() => TurnBed();
        public void PressCheck() => Check();
        public void PressClear() => ResetTable();
        public void PressNext() => NextChapter();

        public LessonActionResult TurnBed()
        {
            var gate = Gate(); if (gate.HasValue) return gate.Value;
            if (model.ChapterComplete) return Refuse(CheckedReason);
            var before = bed != null ? BedLayout : default;
            var turning = new List<(Transform t, Vector3 p)>();
            foreach (var v in Views())
                if (v.piece != null && v.piece.gameObject.activeSelf && v.id != null && model.RowOf(v.id) >= 0) turning.Add((v.piece, v.piece.localPosition));
            if (!model.TurnBed(false)) return Refuse(string.IsNullOrEmpty(model.LastFeedback) ? NoTurnReason : model.LastFeedback);
            feedback = string.IsNullOrEmpty(model.LastFeedback) ? "The bed turned: " + model.Rows + " rows of " + model.Columns + "." : model.LastFeedback;
            if (Application.isPlaying && isActiveAndEnabled && turnSeconds > 0f && bed != null) running = StartCoroutine(TurnAnimation(before, turning));
            else { LayoutAll(); Refresh(); }
            return new LessonActionResult(true, feedback);
        }

        /// Fence after `columns` columns (voice "split it at five" or the fence dropped on the bed).
        public LessonActionResult SplitAt(int columns)
        {
            var gate = Gate(); if (gate.HasValue) return gate.Value;
            if (!model.Chapter.HasFence) return Refuse(NoFenceReason);
            if (model.ChapterComplete) return Refuse(CheckedReason);
            if (columns == 0) return Refuse(FenceWhereReason);
            if (!model.SetFence(columns, false))
            {
                string reason = "The fence goes between two columns: after column 1 up to after column " + (model.Columns - 1) + ".";
                LayoutAll();
                return Refuse(reason);
            }
            feedback = string.IsNullOrEmpty(model.LastFeedback) ? "The fence is after column " + columns + "." : model.LastFeedback;
            LayoutAll();
            Refresh();
            return new LessonActionResult(true, feedback);
        }

        LessonActionResult? Gate()
        {
            if (!active) return new LessonActionResult(false, ClosedReason);
            if (briefing) return new LessonActionResult(false, BriefingReason);
            if (AnyHeld) return Refuse(HeldReasonNow());
            if (animating) return new LessonActionResult(false, TurningReason);
            return null;
        }

        string HeldReasonNow() => fenceHeld ? FenceHeldReason : HeldReason;

        LessonActionResult Refuse(string reason)
        {
            feedback = reason;
            Refresh();
            return new LessonActionResult(false, reason);
        }

        // ---- grabbing ----
        void OnStripPointer(StripView view, PointerEvent evt)
        {
            if (!ChapterActive) return;
            if (evt.Type == PointerEventType.Select)
            {
                if (StripLocked(view)) return;
                view.held = true;
                if (view.id != null && model.RowOf(view.id) >= 0) model.Unplant(view.id);
            }
            else if (evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
            {
                if (!view.held) return;
                view.held = false;
                StartCoroutine(SettleStrip(view));
            }
        }

        void OnFencePointer(PointerEvent evt)
        {
            if (!ChapterActive) return;
            if (evt.Type == PointerEventType.Select) { if (!FenceLocked()) fenceHeld = true; }
            else if (evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
            {
                if (!fenceHeld) return;
                fenceHeld = false;
                StartCoroutine(SettleFence());
            }
        }

        IEnumerator SettleStrip(StripView view)
        {
            yield return null; // let the SDK finish transferring ownership before we move the strip
            if (!ChapterActive || view.held || view.piece == null) yield break;
            DropAt(view.id, Local(view.piece.position));
        }

        IEnumerator SettleFence()
        {
            yield return null;
            if (!ChapterActive || fenceHeld || fence == null) yield break;
            DropFenceAt(Local(fence.position));
        }

        Vector3 Local(Vector3 world) => (stationRoot != null ? stationRoot : transform).InverseTransformPoint(world);

        /// A released strip at a station-local position: the bed row under its centre plants it there. Anywhere else
        /// (or a refused row) the strip goes back to its tray slot and the card says why. Public for tests and drives.
        public bool DropAt(string stripId, Vector3 stationLocal)
        {
            EnsureInit();
            var view = ViewOf(stripId);
            if (!ChapterActive || view?.piece == null) return false;
            if (animating || StripLocked(view)) { LayoutAll(); return false; }
            int row = BedLayout.RowAt(stationLocal);
            int current = model.RowOf(stripId);
            if (row < 0)
            {
                if (current >= 0) model.Unplant(stripId);
                LayoutAll(); Refresh();
                return false;
            }
            if (current == row) { LayoutAll(); return true; }
            if (current >= 0) model.Unplant(stripId);
            if (model.Plant(stripId, false, row))
            {
                feedback = "";
                LayoutAll(); Refresh();
                return true;
            }
            string reason = !string.IsNullOrEmpty(model.LastFeedback) ? model.LastFeedback
                : model.StripInRow(row) != null ? "Row " + (row + 1) + " already has a strip." : "That strip does not fit in row " + (row + 1) + ".";
            feedback = reason;
            LayoutAll(); Refresh();
            return false;
        }

        /// A released fence: over the bed it snaps to the nearest line between two columns; elsewhere it returns to
        /// where it was (its line, or its home beside the bed).
        public bool DropFenceAt(Vector3 stationLocal)
        {
            EnsureInit();
            if (!ChapterActive || fence == null || !model.Chapter.HasFence) return false;
            if (animating || FenceLocked()) { LayoutAll(); return false; }
            int k = BedLayout.FenceAt(stationLocal);
            if (k < 0) { LayoutAll(); return false; }
            return SplitAt(k).Ok;
        }

        bool StripLocked(StripView view) =>
            !ChapterActive || animating || model.ChapterComplete || model.Chapter.StartsPlanted || view.id == null;

        bool FenceLocked() => !ChapterActive || animating || model.ChapterComplete || !model.Chapter.HasFence;

        /// Planted-chapter strips, accepted beds and a checked fence cannot be grabbed. TableHandle re-enables every
        /// piece after a carry, so the rule is re-applied each frame.
        void LateUpdate()
        {
            if (!active) return;
            foreach (var v in Views()) Lock(v.interactables, v.piece, StripLocked(v) && !v.held);
            Lock(fenceInteractables, fence, FenceLocked() && !fenceHeld);
        }

        static void Lock(MonoBehaviour[] interactables, Transform piece, bool locked)
        {
            if (interactables == null || piece == null || !piece.gameObject.activeInHierarchy) return;
            foreach (var behaviour in interactables)
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

        // ---- presentation ----
        void ShowChapter()
        {
            StopRunning();
            RestorePayoff();
            LayoutAll();
            Refresh();
        }

        /// Mirrors the model: bed size, strip ids bound to views in order, planted strips in their rows, the rest in
        /// the tray, the fence on its line or at home, and the opened split once an accepted fence chapter is shown.
        void LayoutAll()
        {
            bool showPieces = ChapterActive;
            if (pieces != null) pieces.SetActive(showPieces);
            int split = payoffShown && model.Chapter.HasFence ? model.FenceColumn : 0;
            float gap = split > 0 ? splitGap : 0f;
            if (bed != null) bed.Apply(model.Rows, model.Columns, split, gap);
            var layout = BedLayout;
            var ids = model.StripIds;
            int i = 0;
            foreach (var v in Views())
            {
                v.id = ids != null && i < ids.Count ? ids[i] : null;
                i++;
                if (v.piece == null) continue;
                bool on = showPieces && v.id != null;
                v.piece.gameObject.SetActive(on);
                if (v.id == null) continue;
                int length = Mathf.Clamp(model.StripLength(v.id), 1, GardenBedLayout.MaxColumns);
                int row = model.RowOf(v.id);
                SetLength(v, length, layout.Cell, row >= 0 ? split : 0, gap);
                if (v.held) continue;
                v.piece.localPosition = row >= 0 ? layout.StripPosition(row, length) : TrayPosition(v, length, layout.Cell);
                v.piece.localRotation = Quaternion.identity;
            }
            if (fence != null)
            {
                bool on = showPieces && model.Chapter.HasFence;
                fence.gameObject.SetActive(on);
                if (on && !fenceHeld)
                {
                    int k = model.FenceColumn;
                    fence.localPosition = k > 0 ? new Vector3(layout.BoundaryX(k) + gap * 0.5f, deckTop, layout.Center.z) : fenceHome;
                    fence.localRotation = Quaternion.identity;
                }
            }
            if (partLabels != null)
            {
                bool on = showPieces && split > 0;
                for (int p = 0; p < partLabels.Length; p++)
                {
                    var label = partLabels[p];
                    if (label == null) continue;
                    label.gameObject.SetActive(on);
                    if (!on) continue;
                    int cols = p == 0 ? split : model.Columns - split;
                    label.text = model.Rows + " × " + cols + " = " + model.Rows * cols;
                    label.transform.localPosition = PartLabelPosition(layout, split, gap, p, bed != null ? bed.wallThickness : 0.012f, partLabelOffset, deckTop);
                }
            }
        }

        /// Part label `part` (0 left of the fence, 1 right) centred under its part, just in front of the front wall.
        public static Vector3 PartLabelPosition(GardenBedLayout layout, int split, float gap, int part, float wallThickness, float offset, float deckTop)
        {
            float x = part == 0 ? layout.Left + split * layout.Cell * 0.5f : layout.BoundaryX(split) + gap + (layout.Columns - split) * layout.Cell * 0.5f;
            return new Vector3(x, deckTop + 0.004f, layout.Front - wallThickness - offset);
        }

        public static Vector3 TrayPosition(StripView view, int length, float cell) => view.trayLeft + new Vector3(length * cell * 0.5f, 0, 0);

        /// Shows `length` seedlings centred on the strip root. With a split after `split` columns the seedlings and
        /// body right of the fence slide right by the gap.
        void SetLength(StripView v, int length, float cell, int split, float gap)
        {
            v.length = length;
            bool open = split > 0 && split < length;
            if (v.collider != null) { v.collider.size = new Vector3(length * cell, stripHeight, stripDepth); v.collider.center = new Vector3(0, stripHeight * 0.3f, 0); }
            if (v.bodyLeft != null)
            {
                int n = open ? split : length;
                if (stripMeshes != null && n < stripMeshes.Length && stripMeshes[n] != null) v.bodyLeft.sharedMesh = stripMeshes[n];
                v.bodyLeft.transform.localPosition = new Vector3(-length * cell * 0.5f + n * cell * 0.5f, 0, 0);
            }
            if (v.bodyRight != null)
            {
                v.bodyRight.gameObject.SetActive(open);
                if (open)
                {
                    int n = length - split;
                    if (stripMeshes != null && n < stripMeshes.Length && stripMeshes[n] != null) v.bodyRight.sharedMesh = stripMeshes[n];
                    v.bodyRight.transform.localPosition = new Vector3(-length * cell * 0.5f + split * cell + gap + n * cell * 0.5f, 0, 0);
                }
            }
            if (v.seedlings == null) return;
            for (int k = 0; k < v.seedlings.Length; k++)
            {
                var s = v.seedlings[k];
                if (s == null) continue;
                bool on = k < length;
                s.gameObject.SetActive(on);
                if (!on) continue;
                var p = s.localPosition;
                s.localPosition = new Vector3(GardenBedLayout.SeedlingX(cell, k, length) + (open && k >= split ? gap : 0f), p.y, p.z);
            }
        }

        void Refresh()
        {
            if (body != null && bodyBaseSize <= 0) bodyBaseSize = body.fontSize;
            if (!active)
            {
                SetButtons(false, false, false, false, false);
                return;
            }
            if (briefing)
            {
                if (heading != null) heading.text = BriefingHeading;
                SetBody(BriefingBody);
                if (expressionLine != null) expressionLine.text = "";
                if (sayHints != null) sayHints.text = "Say \"yes\" · \"back to the lessons\"";
                SetButtons(true, false, false, false, false);
                return;
            }
            var c = model.Chapter;
            if (heading != null) heading.text = TitleFor(c);
            SetBody(ComposeBody(c, feedback));
            if (expressionLine != null) expressionLine.text = model.Expression;
            if (sayHints != null) sayHints.text = SayHintsFor(c, model.ChapterComplete, model.IsLastChapter, model.Turned);
            bool complete = model.ChapterComplete;
            SetButtons(false, !complete && c.NeedsTurn && !model.Turned, !complete, !complete, complete && !model.IsLastChapter);
        }

        void SetBody(string text)
        {
            if (body == null) return;
            body.text = text;
            if (bodyBaseSize > 0) body.fontSize = CargoLessonDirector.FitFontSize(body, text, bodyBaseSize);
        }

        void SetButtons(bool start, bool turn, bool check, bool clear, bool next)
        {
            if (startButton != null) startButton.gameObject.SetActive(start);
            if (turnButton != null) turnButton.gameObject.SetActive(turn);
            if (checkButton != null) checkButton.gameObject.SetActive(check);
            if (clearButton != null) clearButton.gameObject.SetActive(clear);
            if (nextButton != null) nextButton.gameObject.SetActive(next);
            if (backButton != null) backButton.gameObject.SetActive(true);
        }

        // ---- turn animation ----
        IEnumerator TurnAnimation(GardenBedLayout before, List<(Transform t, Vector3 p)> turning)
        {
            animating = true;
            bed.ShowLabels(false);
            if (partLabels != null) foreach (var l in partLabels) if (l != null) l.gameObject.SetActive(false);
            if (fence != null && fence.gameObject.activeSelf) fence.localPosition = fenceHome;
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / turnSeconds);
                float eased = t * t * (3f - 2f * t);
                float degrees = 90f * eased;
                if (bed.turnable != null) bed.turnable.localRotation = Quaternion.Euler(0, degrees, 0);
                foreach (var (piece, start) in turning)
                {
                    if (piece == null) continue;
                    piece.localPosition = before.TurnPoint(start, degrees);
                    piece.localRotation = Quaternion.Euler(0, degrees, 0);
                }
                yield return null;
            }
            animating = false;
            running = null;
            bed.ShowLabels(true);
            LayoutAll();
            Refresh();
        }

        // ---- payoff: rows grow into flowers and vegetables, the watering can passes, a butterfly lands ----
        void StartPayoff()
        {
            StopRunning();
            payoffShown = true;
            LayoutAll();
            Refresh();
            if (Application.isPlaying && isActiveAndEnabled) running = StartCoroutine(PayoffAnimation());
            else ApplyPayoffFinal();
        }

        IEnumerable<StripView> PlantedViews()
        {
            foreach (var v in Views())
                if (v.piece != null && v.piece.gameObject.activeSelf && v.id != null && model.RowOf(v.id) >= 0) yield return v;
        }

        void SetGrowth(StripView v, float growth)
        {
            int row = v.id != null ? model.RowOf(v.id) : 0;
            for (int k = 0; k < v.length && k < v.seedlings.Length; k++)
            {
                if (v.sprouts != null && k < v.sprouts.Length && v.sprouts[k] != null) v.sprouts[k].localScale = Vector3.one * Mathf.Lerp(1f, 0.6f, growth);
                if (v.blooms == null || k >= v.blooms.Length || v.blooms[k] == null) continue;
                var bloom = v.blooms[k];
                if (growth > 0f && !bloom.gameObject.activeSelf) ShowKind(bloom, BloomKindFor(model.Chapter, row, k, model.FenceColumn));
                bloom.gameObject.SetActive(growth > 0f);
                bloom.localScale = Vector3.one * growth;
            }
        }

        public const string Lettuce = "Lettuce", Carrot = "Carrot", Sunflower = "Sunflower", Bean = "Bean";

        /// What a planted seedling grows into, matching the chapter's accepted line: lettuce, carrots, sunflowers
        /// and lettuce on a turned bed, and on split beds a different crop on each side of the fence.
        public static string BloomKindFor(GardenChapter chapter, int row, int column, int fenceColumn)
        {
            switch (chapter?.Id)
            {
                case "first_rows": return Lettuce;
                case "equal_rows": return Carrot;
                case "turn_the_bed": return row % 2 == 0 ? Sunflower : Lettuce;
                case "split_the_bed": return fenceColumn > 0 && column >= fenceColumn ? Sunflower : Carrot;
                case "your_own_split": return fenceColumn > 0 && column >= fenceColumn ? Bean : Sunflower;
                default: return Sunflower;
            }
        }

        /// A bloom holds every crop as children named "<Kind> <part>"; only the chosen kind is shown.
        static void ShowKind(Transform bloom, string kind)
        {
            for (int i = 0; i < bloom.childCount; i++)
            {
                var child = bloom.GetChild(i);
                child.gameObject.SetActive(child.name.StartsWith(kind + " "));
            }
        }

        IEnumerator PayoffAnimation()
        {
            var planted = new List<StripView>(PlantedViews());
            var layout = BedLayout;
            float t = 0f, growSeconds = 0.6f, stagger = 0.12f;
            float total = growSeconds + stagger * Mathf.Max(0, planted.Count - 1);
            while (t < total)
            {
                t += Time.deltaTime;
                foreach (var v in planted)
                {
                    int row = v.id != null ? model.RowOf(v.id) : 0;
                    float g = Mathf.Clamp01((t - stagger * Mathf.Max(0, row)) / growSeconds);
                    SetGrowth(v, g * g * (3f - 2f * g));
                }
                yield return null;
            }
            foreach (var v in planted) SetGrowth(v, 1f);

            if (wateringCan != null)
            {
                float y = 0.13f;
                var from = new Vector3(layout.Left - 0.06f, y, layout.Center.z);
                var to = new Vector3(layout.Right + splitGap + 0.06f, y, layout.Center.z);
                wateringCan.localRotation = Quaternion.Euler(0, 0, -20f);
                for (t = 0f; t < 1f;)
                {
                    t = Mathf.Min(1f, t + Time.deltaTime / 1.4f);
                    wateringCan.localPosition = Vector3.Lerp(from, to, t) + Vector3.up * (0.01f * Mathf.Sin(t * Mathf.PI * 4f));
                    yield return null;
                }
                wateringCan.localPosition = wateringCanHome;
                wateringCan.localRotation = Quaternion.identity;
            }

            if (butterfly != null)
            {
                var landing = ButterflyLanding(layout);
                var start = butterflyHome;
                for (t = 0f; t < 1f;)
                {
                    t = Mathf.Min(1f, t + Time.deltaTime / 1.3f);
                    var p = Vector3.Lerp(start, landing, t);
                    p.y += 0.06f * Mathf.Sin(t * Mathf.PI) + 0.006f * Mathf.Sin(t * Mathf.PI * 10f) * (1f - t);
                    butterfly.localPosition = p;
                    yield return null;
                }
            }
            ApplyPayoffFinal();
            running = null;
        }

        Vector3 ButterflyLanding(GardenBedLayout layout)
        {
            float x = layout.ColumnX(Mathf.Max(0, layout.Columns / 2 - 1));
            return new Vector3(x, layout.Center.y + 0.075f, layout.RowZ(0));
        }

        void ApplyPayoffFinal()
        {
            foreach (var v in PlantedViews()) SetGrowth(v, 1f);
            if (wateringCan != null) { wateringCan.localPosition = wateringCanHome; wateringCan.localRotation = Quaternion.identity; }
            if (butterfly != null) butterfly.localPosition = ButterflyLanding(BedLayout);
        }

        void RestorePayoff()
        {
            payoffShown = false;
            foreach (var v in Views())
            {
                if (v.seedlings == null) continue;
                for (int k = 0; k < v.seedlings.Length; k++)
                {
                    if (v.sprouts != null && k < v.sprouts.Length && v.sprouts[k] != null) v.sprouts[k].localScale = Vector3.one;
                    if (v.blooms != null && k < v.blooms.Length && v.blooms[k] != null) { v.blooms[k].localScale = Vector3.zero; v.blooms[k].gameObject.SetActive(false); }
                }
            }
            if (wateringCan != null) { wateringCan.localPosition = wateringCanHome; wateringCan.localRotation = Quaternion.identity; }
            if (butterfly != null) butterfly.localPosition = butterflyHome;
        }

        /// Development and editor preview only: jump straight to a chapter inside the open station.
        public void JumpToChapter(int index)
        {
            model.StartChapter(index);
            feedback = "";
            if (active) { briefing = false; ShowChapter(); }
        }

        // ---- pure text helpers (unit-tested) ----
        public static string TitleFor(GardenChapter chapter) => "Chapter " + chapter.Number + " · " + chapter.Title;

        /// Story line, task line, then feedback. The guide reads this text, so it always starts with the story.
        public static string ComposeBody(GardenChapter chapter, string feedback)
        {
            var sb = new StringBuilder();
            sb.Append(chapter.Story).Append('\n').Append(chapter.Task);
            if (!string.IsNullOrEmpty(feedback)) sb.Append("\n\n").Append(feedback);
            return sb.ToString();
        }

        /// Spoken phrases the guide understands for the current state.
        public static string SayHintsFor(GardenChapter chapter, bool complete, bool isLast, bool turned)
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
                if (chapter.NeedsTurn && !turned) hints.Add("\"turn the bed\"");
                if (chapter.HasFence) hints.Add(chapter.RequiredFenceColumn > 0 ? "\"split it at " + NumberWord(chapter.RequiredFenceColumn) + "\"" : "\"split it at five\"");
                hints.Add("\"check the bed\"");
                hints.Add("\"clear the bed\"");
            }
            return "Say " + string.Join(" · ", hints);
        }

        static string NumberWord(int n)
        {
            string[] words = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight" };
            return n >= 0 && n < words.Length ? words[n] : n.ToString();
        }
    }
}
