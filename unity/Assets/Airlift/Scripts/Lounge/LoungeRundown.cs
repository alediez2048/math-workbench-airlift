using System;
using System.Collections.Generic;
using System.Linq;
using Airlift.Presentation.Dashboard;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Airlift.Lounge
{
    /// The host tour in the scene: an overlay on whatever screen is up. Everything but the stop's target dims and
    /// stops taking presses; a ring and a bouncing arrow sit on the target; Dee says one line. NerdyDirector starts
    /// it, narrates each stop and reports the gates (a pill, the arrows, a filter, the gear).
    public sealed class LoungeRundown : MonoBehaviour
    {
        [Header("Targets")]
        public DashboardWall wall;
        public Button[] consentPills = new Button[0];
        public Button nextArrow, backArrow, gearButton;
        public Button conversationButton, yourRoomButton, nerdyLoungeButton;
        public LoungeRoom lounge;
        [Tooltip("The questions' own Skip to lessons pill: dimmed during the tour so the arrow is the only way on.")]
        public Button questionsSkip;
        [Header("Overlay, on the welcome canvas, drawn last")]
        public RectTransform pointerRoot, ring, arrow;
        public GameObject header;
        public TMP_Text stepLabel;
        public Button skipButton;
        [Header("Dimmed when not the target")]
        public Button[] barControls = new Button[0];
        public RectTransform[] pagerAndScenery = new RectTransform[0];
        public LoungeSettings settings;
        public float dimAlpha = 0.32f, ringPadding = 14f, arrowBounce = 12f;

        public RundownScript Script { get; } = new RundownScript();
        public bool Running => Script.Running;
        public bool Voice { get; private set; }
        public event Action<RundownStep, string> StepShown;
        public event Action<bool> Ended;
        /// The lesson the tour ends on: Cargo Crew, chapter 1.
        public const string FlagshipLessonId = "cargo_crew_fractions";

        bool subscribed, settingsWereOpened;
        Scenery lastScenery;
        RectTransform settingsDone;
        public const string SettingsInstruction = "Here you can adjust sound and replay the tour. Press the highlighted Done button to return to the lessons.";
        public string CurrentInstruction => !Running ? "" : settingsWereOpened ? SettingsInstruction : Voice ? Script.Current.Say : Script.Current.SayWithoutVoice;
        readonly List<CanvasGroup> dimmed = new List<CanvasGroup>();
        Vector2 arrowBase;

        void Awake()
        {
            if (header != null) header.SetActive(false);
            if (pointerRoot != null) pointerRoot.gameObject.SetActive(false);
            Script.StepShown += OnStep;
            Script.Ended += OnEnded;
        }

        void Subscribe()
        {
            if (subscribed) return; subscribed = true;
            if (settings != null)
            {
                settingsDone = settings.panel != null ? settings.panel.transform.Find("Done") as RectTransform : null;
                settings.Opened += () => { settingsWereOpened = Running && Script.Current.Gate == RundownGate.SettingsOpened; if (settingsWereOpened) OnStep(Script.Current); else if (pointerRoot != null) pointerRoot.gameObject.SetActive(false); };
                settings.Closed += () => { bool completed = settingsWereOpened; settingsWereOpened = false; if (completed) Report(RundownGate.SettingsOpened); Repoint(); };
            }
            if (wall != null)
            {
                wall.FilterChanged += _ => { Report(RundownGate.FilterPressed); Repoint(); };
                wall.PageMoved += direction => { Report(direction > 0 ? RundownGate.PageNext : RundownGate.PageBack); Repoint(); };
            }
            if (lounge != null)
            {
                lastScenery = lounge.Mode;
                lounge.SceneryChanged += mode => { bool changed = mode != lastScenery; lastScenery = mode; if (changed) Report(RundownGate.SceneryChanged); Repoint(); };
            }
        }

        public void Begin(bool voice, int at = 0)
        {
            Voice = voice;
            Subscribe();
            if (header != null) header.SetActive(true);
            Script.Start(at);
        }

        public void SetVoice(bool enabled)
        {
            if (Voice == enabled) return;
            Voice = enabled;
            if (Running) OnStep(Script.Current);
        }

        void OnStep(RundownStep step)
        {
            if (Running && wall != null && wall.PageCount <= 1 && (step.Gate == RundownGate.PageNext || step.Gate == RundownGate.PageBack)) { Report(step.Gate); return; }
            if (Running && step.Gate == RundownGate.SceneryChanged && (lounge == null || yourRoomButton == null || nerdyLoungeButton == null)) { Report(step.Gate); return; }
            if (stepLabel != null) stepLabel.text = step.Eyebrow;
            // The last stop ends on Cargo Crew. A filter chosen at stop 1 (Newest) leaves it off the page, so the wall
            // goes back to Featured first; every playable tile stays pressable (owner 2026-09-18: "forced to start the coffee lesson").
            if (step.Target == TourTarget.FirstTile && wall != null && !wall.Visible.Any(v => v.openable && v.lessonId == FlagshipLessonId && v.chapter == 0))
                wall.SetFilter(Airlift.Welcome.DashboardFilter.Featured);
            string say = settingsWereOpened ? SettingsInstruction : Voice ? step.Say : step.SayWithoutVoice;
            Point(step);
            StepShown?.Invoke(step, say);
        }

        void OnEnded(bool skipped)
        {
            Undim();
            if (pointerRoot != null) pointerRoot.gameObject.SetActive(false);
            if (header != null) header.SetActive(false);
            Ended?.Invoke(skipped);
        }

        // ---- the gates, each reported by the thing itself ----
        public void ReportConsent() => Report(RundownGate.ConsentPressed);
        public void ReportNext() => Report(RundownGate.NextPressed);
        public void PressContinue() => Report(RundownGate.ContinuePressed);
        public void ReportLessonOpened() => Report(RundownGate.LessonOpened);
        public void Skip() { Script.Skip(); }
        /// B or ‹: the last stop's own gate; anywhere else in the tour it is the skip.
        public void ReportBack()
        {
            if (!Running) return;
            if (Script.Current.Gate == RundownGate.PageBack) wall?.PreviousPage();
        }
        void Report(RundownGate gate) { if (Running && Script.Report(gate)) NerdyHaptics.Tick(); }

        /// The › arrow does something on this stop.
        public bool ContinueLit => Running && (Script.Current.Gate == RundownGate.ContinuePressed || Script.Current.Gate == RundownGate.NextPressed);
        /// Repoint after the screen under the overlay changed (the wall laid out, a card swapped).
        public void Repoint() { if (Running) Point(Script.Current); }

        // ---- pointing ----
        /// Editor preview and tests: lay the pointer out for a stop without running the gates.
        public void PreviewStep(int index) { OnStep(RundownScript.Steps[Mathf.Clamp(index, 0, RundownScript.StopCount - 1)]); if (header != null) header.SetActive(true); }

        IEnumerable<RectTransform> TargetsOf(TourTarget target)
        {
            switch (target)
            {
                case TourTarget.ConsentPills: foreach (var b in consentPills) if (b != null) yield return (RectTransform)b.transform; break;
                case TourTarget.NextArrow: if (nextArrow != null) yield return (RectTransform)nextArrow.transform; break;
                case TourTarget.FirstTile:
                {
                    // The flagship when it is on the page (owner 2026-09-18: a filter had put the café first), else the first tile.
                    var t = wall != null ? wall.Visible.FirstOrDefault(v => v.openable && v.lessonId == FlagshipLessonId && v.chapter == 0) ?? wall.Visible.FirstOrDefault() : null;
                    if (t != null) yield return (RectTransform)t.transform; break;
                }
                case TourTarget.Filters: foreach (var b in new[] { wall?.featuredButton, wall?.newestButton, wall?.mostViewedButton }) if (b != null) yield return (RectTransform)b.transform; break;
                case TourTarget.Gear: if (gearButton != null) yield return (RectTransform)gearButton.transform; break;
                case TourTarget.BackArrow: if (backArrow != null) yield return (RectTransform)backArrow.transform; break;
                case TourTarget.NextPage: if (wall?.nextButton != null) yield return (RectTransform)wall.nextButton.transform; break;
                case TourTarget.PreviousPage: if (wall?.previousButton != null) yield return (RectTransform)wall.previousButton.transform; break;
                case TourTarget.Scenery:
                    var choice = lounge != null && lounge.Mode == Scenery.YourRoom ? nerdyLoungeButton : yourRoomButton;
                    if (choice != null) yield return (RectTransform)choice.transform;
                    break;
            }
        }

        /// The control the stop's gate is pressed on: it stays live even when it is not what Dee points at.
        IEnumerable<RectTransform> GateControls(RundownGate gate)
        {
            switch (gate)
            {
                case RundownGate.ConsentPressed: foreach (var b in consentPills) if (b != null) yield return (RectTransform)b.transform; break;
                case RundownGate.NextPressed: case RundownGate.ContinuePressed: if (nextArrow != null) yield return (RectTransform)nextArrow.transform; break;
                case RundownGate.FilterPressed: foreach (var b in new[] { wall?.featuredButton, wall?.newestButton, wall?.mostViewedButton }) if (b != null) yield return (RectTransform)b.transform; break;
                case RundownGate.SettingsOpened: if (gearButton != null) yield return (RectTransform)gearButton.transform; break;
                case RundownGate.BackPressed: if (backArrow != null) yield return (RectTransform)backArrow.transform; break;
                case RundownGate.PageNext: if (wall?.nextButton != null) yield return (RectTransform)wall.nextButton.transform; break;
                case RundownGate.PageBack: if (wall?.previousButton != null) yield return (RectTransform)wall.previousButton.transform; break;
                case RundownGate.LessonOpened: if (wall != null) foreach (var t in wall.Visible) if (t.openable) yield return (RectTransform)t.transform; break;
                case RundownGate.SceneryChanged:
                    if (yourRoomButton != null) yield return (RectTransform)yourRoomButton.transform;
                    if (nerdyLoungeButton != null) yield return (RectTransform)nerdyLoungeButton.transform;
                    break;
            }
        }

        /// Everything that dims and stops taking presses when it is not the target: tiles, toolbar, bar controls, arrows.
        IEnumerable<RectTransform> Dimmables()
        {
            if (wall != null && wall.gameObject.activeInHierarchy) foreach (var t in wall.Visible) yield return (RectTransform)t.transform;
            foreach (var b in new[] { wall?.featuredButton, wall?.newestButton, wall?.mostViewedButton }) if (b != null) yield return (RectTransform)b.transform;
            foreach (var r in pagerAndScenery) if (r != null) yield return r;
            foreach (var b in barControls) if (b != null) yield return (RectTransform)b.transform;
            if (backArrow != null) yield return (RectTransform)backArrow.transform;
            if (nextArrow != null) yield return (RectTransform)nextArrow.transform;
            if (questionsSkip != null) yield return (RectTransform)questionsSkip.transform;
        }

        void Point(RundownStep step)
        {
            var targets = TargetsOf(step.Target).Where(t => t.gameObject.activeInHierarchy).ToList();
            if (settingsWereOpened) { targets.Clear(); if (settingsDone != null) targets.Add(settingsDone); }
            // Looking at a card is not permission to open it before the final stop.
            var live = new HashSet<RectTransform>(); foreach (var g in GateControls(step.Gate)) live.Add(g);
            if (conversationButton != null) live.Add((RectTransform)conversationButton.transform);
            if (Script.Index > 3) {
                if (yourRoomButton != null) live.Add((RectTransform)yourRoomButton.transform);
                if (nerdyLoungeButton != null) live.Add((RectTransform)nerdyLoungeButton.transform);
            }
            Undim();
            foreach (var r in Dimmables())
            {
                if (live.Contains(r)) continue;
                var g = r.GetComponent<CanvasGroup>(); if (g == null) g = r.gameObject.AddComponent<CanvasGroup>();   // Unity's fake null defeats ??
                g.alpha = dimAlpha; g.interactable = false; g.blocksRaycasts = false;   // gated: dim and unpressable
                dimmed.Add(g);
            }
            if (pointerRoot == null || targets.Count == 0) { if (pointerRoot != null) pointerRoot.gameObject.SetActive(false); return; }
            pointerRoot.gameObject.SetActive(true);
            var b = Bounds(targets);
            if (ring != null) { ring.anchoredPosition = b.center; ring.sizeDelta = b.size + Vector2.one * ringPadding * 2f; }
            arrowBase = new Vector2(b.center.x, b.yMax + ringPadding + 26f);
            if (arrow != null) arrow.anchoredPosition = arrowBase;
        }

        Rect Bounds(List<RectTransform> targets)
        {
            var corners = new Vector3[4]; float xMin = float.MaxValue, yMin = float.MaxValue, xMax = float.MinValue, yMax = float.MinValue;
            foreach (var t in targets)
            {
                t.GetWorldCorners(corners);
                foreach (var c in corners)
                {
                    var l = pointerRoot.InverseTransformPoint(c);
                    xMin = Mathf.Min(xMin, l.x); yMin = Mathf.Min(yMin, l.y); xMax = Mathf.Max(xMax, l.x); yMax = Mathf.Max(yMax, l.y);
                }
            }
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        void Undim() { foreach (var g in dimmed) if (g != null) { g.alpha = 1f; g.interactable = true; g.blocksRaycasts = true; } dimmed.Clear(); }

        void Update()
        {
            if (arrow != null && pointerRoot != null && pointerRoot.gameObject.activeSelf)
                arrow.anchoredPosition = arrowBase + Vector2.up * (arrowBounce * (0.5f + 0.5f * Mathf.Sin(Time.time * 7f)));
        }
    }
}
