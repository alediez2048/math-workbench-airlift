using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Presentation;
using Airlift.Welcome;
using Oculus.Interaction;
using Oculus.Interaction.Editor.QuickActions;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Phase 1R (Dock 7) workbench, CargoCrew only. Idempotent: every object it creates is found by name and
// replaced. Round 2 (2026-09-16 night): vehicles back up over the container cells and drive away with the load,
// the dock-edge strip replaces the container floor, and two cranes replace the parked aircraft (migrated). Adds the quarter crates and locked marks to the piece pool, the container floor skin on the
// ruler, the vehicle bay, the story card lines, CanvasGroup button rows (voice-first fallback), UiPressLog on
// every lesson/HUD button, and removes the double-wired Pause/Music listeners on the Guide HUD.
// Run: unity command run_script --file AgentScripts/BuildDockWorkbench.cs --entry BuildDockWorkbench.Run
public static class BuildDockWorkbench
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string MeshFolder = "Assets/Airlift/Meshes";
    public const float TrayZ = -0.30f;   // far enough in front of the dock-edge strip and the exit lane that waiting crates never cover them
    // Owner 2026-09-17: crates twice as tall and deep (5.5 x 6 cm -> 11 x 12 cm); length stays exact to the ruler cells.
    public const float CrateHeight = 0.11f, CrateDepth = 0.12f, TrayY = 0.0175f + CrateHeight / 2;
    public static readonly Vector3 QuarterTrayOrigin = new Vector3(-0.265f, TrayY, TrayZ);
    public const float QuarterTraySpacing = 0.09f;
    const float RowY = -196f, RowHeight = 48f, RowWidth = 165f, RowStep = 172f;
    // Round 2 geometry (station-local metres): bed floor top, bed depth, dock-edge strip z (ruler-local).
    public const float BedTop = 0.052f, BedDepth = 0.13f, EdgeZ = -0.11f, RearZ = -0.0725f;
    // Owner 2026-09-17: station-space exit lane z and dock exit gate x.
    public const float LaneZ = -0.05f, ExitGateX = -0.40f;
    // Owner 2026-09-17: splitter station at the front right of the dock (station space). The pad holds a whole crate and
    // sits between the crate tray (x <= 0.04), the table handle (z <= -0.387), the exit lane (z >= -0.195) and the staging
    // platform (x >= 0.38, z >= -0.22); the console stands to its right, tilted toward the seated learner.
    public static readonly Vector3 SplitterPadCenter = new Vector3(0.23f, 0.0255f, -0.305f);   // top surface centre
    public static readonly Vector2 SplitterPadSize = new Vector2(0.30f, 0.14f);
    public static readonly Vector3 SplitterConsoleCenter = new Vector3(0.505f, 0.075f, -0.31f);
    const float ConsolePitch = 62f, ConsoleYaw = 15f;
    static readonly Vector2 ConsoleSize = new Vector2(190f, 130f);   // canvas units, 1 mm each

    static AirliftStyle style;
    static Material orange, cream, teal, navy, rubber, yellow, slot, red;
    static readonly Dictionary<string, Mesh> meshCache = new Dictionary<string, Mesh>();

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>(FindObjectsInactive.Include);
        var lesson = d != null ? d.GetComponent<CargoLessonDirector>() : null;
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var terminal = d != null ? d.GetComponentInChildren<CargoTerminalView>(true) : null;
        if (d == null || lesson == null || n == null || terminal == null) throw new InvalidOperationException("CargoCrew director, lesson, Nerdy director or terminal missing.");
        if (lesson.chapterObjects == null || lesson.ruler == null || lesson.halfA?.piece == null || lesson.halfB?.piece == null) throw new InvalidOperationException("Fraction chapter objects missing.");
        style = AssetDatabase.LoadAssetAtPath<AirliftStyle>("Assets/Airlift/Fonts/AirliftStyle.asset");
        if (style == null || style.headingFont == null || style.bodyFont == null) throw new InvalidOperationException("Airlift style/fonts missing.");
        orange = Mat("CargoOrange"); cream = Mat("CargoCream"); teal = Mat("CargoTeal"); navy = Mat("CargoNavy"); rubber = Mat("CargoRubber"); yellow = Mat("CargoYellow");
        slot = SlotMaterial(); red = RedMaterial();
        meshCache.Clear();
        var report = new List<string>();

        // 1. Piece pool: four quarter crates (2 cells) and a locked mark on each half.
        var chapter = lesson.chapterObjects.transform;
        var quarters = new CargoLessonDirector.PieceView[4];
        for (int i = 1; i <= 4; i++)
        {
            Replace(chapter, "Quarter crate " + i);
            quarters[i - 1] = Quarter(chapter, i, QuarterTrayOrigin + new Vector3((i - 1) * QuarterTraySpacing, 0, 0));
        }
        foreach (var v in new[] { lesson.whole, lesson.halfA, lesson.halfB })
        {
            if (v == null || v.piece == null) continue;
            v.trayPosition = new Vector3(v.trayPosition.x, TrayY, TrayZ);
            v.piece.localPosition = new Vector3(v.piece.localPosition.x, TrayY, TrayZ);
        }
        ChunkyCrate(lesson.whole, 8, true, true);
        ChunkyCrate(lesson.halfA, 4, true, false);   // cut face on the right
        ChunkyCrate(lesson.halfB, 4, false, true);   // cut face on the left
        lesson.quarters = quarters;
        lesson.whole.id = "whole"; lesson.halfA.id = "half-1"; lesson.halfB.id = "half-2";
        lesson.halfA.lockedMark = LockedMark(lesson.halfA.piece, RulerLayout.PieceLength(4));
        lesson.halfB.lockedMark = LockedMark(lesson.halfB.piece, RulerLayout.PieceLength(4));
        report.Add("4 quarter crates + locked marks on both halves");

        // 1b. Owner 2026-09-17: the practice and demonstration crates were smaller than the lesson crates. Same chunky
        //     size now; they rest on the deck in the tray and on the measuring pad's inset when placed.
        PracticeCrate(d.strap); PracticeCrate(d.demonstrationStrap);
        var inset = d.station.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f => f.name == "Pad inset");
        if (inset == null) throw new InvalidOperationException("Practice pad inset missing.");
        float padTop = StationBounds(d.transform, inset).max.y;
        d.content.trayPosition = new Vector3(d.content.trayPosition.x, TrayY, d.content.trayPosition.z);
        d.content.targetPosition = new Vector3(d.content.targetPosition.x, padTop + 0.001f + CrateHeight / 2, d.content.targetPosition.z);
        EditorUtility.SetDirty(d.content);
        d.strap.localPosition = d.content.trayPosition; d.demonstrationStrap.localPosition = d.content.trayPosition;
        report.Add("practice crates chunky; tray y " + TrayY.ToString("0.0000", CultureInfo.InvariantCulture) + ", pad y " + d.content.targetPosition.y.ToString("0.0000", CultureInfo.InvariantCulture));

        // 2. Dock edge on the ruler (Round 2): the vehicles' beds are the container floor now. A thin strip in front
        //    keeps the 0, 1/4, 1/2, 3/4, 1 ticks and the ONE CONTAINER label; unused cells get faded outlines.
        var ruler = lesson.ruler;
        foreach (var old in new[] { "Container floor", "Dock edge", "Not needed", "Tick 0", "Tick 1/2", "Tick 1", "Mark 0", "Mark 1/2", "Mark 1" }) Replace(ruler, old);
        var rulerBar = ruler.Find("Ruler bar"); if (rulerBar != null) rulerBar.gameObject.SetActive(false);
        float len = RulerLayout.WholeLength, cell = len / PlacementState.CellsPerWhole;
        // Ruler-local heights: the ruler sits at station y 0.031; the deck top is 0.0175.
        const float deckTop = 0.0175f - 0.031f;
        var edge = Group("Dock edge", ruler, Vector3.zero);
        Rounded("Edge strip", edge, new Vector3(0, deckTop + 0.003f, EdgeZ), new Vector3(len + 0.03f, 0.006f, 0.04f), cream);
        string[] marks = { "0", "1/4", "1/2", "3/4", "1" };
        for (int k = 0; k < marks.Length; k++)
        {
            float x = -len / 2 + k * 2 * cell;
            Rounded("Tick " + marks[k], edge, new Vector3(x, deckTop + 0.0068f, EdgeZ + 0.012f), new Vector3(0.003f, 0.0016f, k % 2 == 0 ? 0.016f : 0.01f), navy, 0.0007f);
            var mark = WorldText("Mark " + marks[k], edge, new Vector3(x, deckTop + 0.008f, EdgeZ - 0.008f), marks[k], 0.085f, Quaternion.Euler(60, 0, 0), style.bodyFont, style.panel);
            mark.rectTransform.sizeDelta = new Vector2(0.06f, 0.03f);
        }
        var containerLabel = WorldText("ONE CONTAINER label", edge, new Vector3(0, deckTop + 0.004f, EdgeZ - 0.037f), "ONE CONTAINER", 0.055f, Quaternion.Euler(60, 0, 0), style.headingFont, style.panel);
        containerLabel.rectTransform.sizeDelta = new Vector2(0.22f, 0.025f);
        var unused = Group("Not needed", ruler, Vector3.zero);
        var unusedCells = new GameObject[PlacementState.CellsPerWhole];
        for (int i = 0; i < unusedCells.Length; i++)
        {
            var c = Group("Not needed cell " + (i + 1), unused, new Vector3(-len / 2 + (i + 0.5f) * cell, deckTop + 0.001f, 0));
            Rounded("Outline back", c, new Vector3(0, 0, 0.034f), new Vector3(cell - 0.004f, 0.002f, 0.003f), slot, 0.0009f);
            Rounded("Outline front", c, new Vector3(0, 0, -0.034f), new Vector3(cell - 0.004f, 0.002f, 0.003f), slot, 0.0009f);
            Rounded("Outline left", c, new Vector3(-(cell - 0.004f) / 2, 0, 0), new Vector3(0.003f, 0.002f, 0.068f), slot, 0.0009f);
            Rounded("Outline right", c, new Vector3((cell - 0.004f) / 2, 0, 0), new Vector3(0.003f, 0.002f, 0.068f), slot, 0.0009f);
            c.gameObject.SetActive(false); unusedCells[i] = c.gameObject;
        }
        var notNeeded = WorldText("Not needed label", unused, new Vector3(len / 4, deckTop + 0.004f, 0), "not needed", 0.06f, Quaternion.Euler(80, 0, 0), style.bodyFont, style.muted);
        notNeeded.rectTransform.sizeDelta = new Vector2(0.14f, 0.025f);
        notNeeded.gameObject.SetActive(false);
        var halfHighlight = Rounded("Half mark highlight", edge, new Vector3(0, deckTop + 0.0072f, EdgeZ), new Vector3(0.005f, 0.0012f, 0.038f), teal, 0.0005f);
        halfHighlight.SetActive(false);
        lesson.halfMarks = new[] { halfHighlight };   // chapter 4 (ShowHalfMark) emphasises 1/2 on the strip
        lesson.restHeight = BedTop + CrateHeight / 2;
        var hide = (lesson.hideWhileActive ?? new GameObject[0]).Where(g => g != null).ToList();
        if (!hide.Contains(d.station)) hide.Add(d.station);   // the practice pad would sit under the beds; onboarding re-shows it
        lesson.hideWhileActive = hide.ToArray();
        report.Add("dock edge: 5 ticks, label, 8 not-needed cells; crate rest height " + lesson.restHeight.ToString("0.0000", CultureInfo.InvariantCulture));

        // 3. Terminal migration: aircraft out, two dock cranes in; vehicles rebuilt to back up over the cells.
        int migrated = 0;
        foreach (var old in new[] { "Parked aircraft", "AIR DELIVERY tag", "Delivery truck", "Dock crane 1", "Dock crane 2" })
            for (int i = terminal.transform.childCount - 1; i >= 0; i--)
                if (terminal.transform.GetChild(i).name == old) { UnityEngine.Object.DestroyImmediate(terminal.transform.GetChild(i).gameObject); migrated++; }
        terminal.cranes = new[] {
            // In front of the containers at the deck's sides so the whole crane is visible; jibs reach out over the
            // table edges like harbour cranes over the water, never over the tray, beds or card.
            Crane(terminal.transform, "Dock crane 1", new Vector3(-0.52f, 0.0175f, 0.115f), -1f),
            Crane(terminal.transform, "Dock crane 2", new Vector3(0.52f, 0.0175f, 0.115f), +1f) };
        Replace(terminal.transform, "Vehicle bay");
        var bayGo = Group("Vehicle bay", terminal.transform, Vector3.zero).gameObject;
        var bay = bayGo.AddComponent<VehicleBay>();
        bay.ruler = ruler; bay.parkHeight = 0.0175f;
        // Owner 2026-09-17: loaded vehicles turn left and leave through a DOCK EXIT gate. Station-space lane between the
        // crate tray (z <= -0.195) and the left crane base (z >= 0.08); vehicles vanish once fully past the left edge.
        Vector3 BaySpace(Vector3 station) => bayGo.transform.InverseTransformPoint(d.transform.TransformPoint(station));
        bay.laneZ = BaySpace(new Vector3(0, 0, LaneZ)).z;
        bay.exitX = BaySpace(new Vector3(-0.75f, 0, 0)).x;
        bay.turnSlide = 0.09f; bay.turnSeconds = 0.8f; bay.driveSpeed = 0.4f; bay.departGap = 0.9f; bay.tagSeconds = 0.4f;
        bay.idleKind = "truck"; bay.idlePosition = BaySpace(new Vector3(0.12f, 0.0175f, 0.25f)); bay.idleYaw = -90f;   // waiting at the back of the dock
        Replace(terminal.transform, "Dock exit");
        DockExit(terminal.transform, d.transform);
        bay.unusedCells = unusedCells; bay.notNeededLabel = notNeeded.gameObject;
        var list = new List<VehicleBay.Vehicle> { BuildVehicle(bayGo.transform, "Big truck", "truck", 8, yellow) };
        for (int i = 1; i <= 2; i++) list.Add(BuildVehicle(bayGo.transform, "Pickup " + i, "pickup", 4, i == 1 ? teal : orange));
        for (int i = 1; i <= 4; i++) list.Add(BuildVehicle(bayGo.transform, "Van " + i, "van", 2, i % 2 == 1 ? cream : teal));
        foreach (var v in list) v.root.gameObject.SetActive(false);
        list[0].root.localPosition = bay.idlePosition; list[0].root.localRotation = Quaternion.Euler(0, bay.idleYaw, 0); list[0].root.gameObject.SetActive(true);
        bay.vehicles = list.ToArray();
        terminal.truck = list[0].root;
        lesson.vehicles = bay;
        EditorUtility.SetDirty(terminal);
        report.Add("terminal: " + migrated + " old props removed, 2 cranes, dock exit, vehicles 1 truck (8 cells) + 2 pickups (4) + 4 vans (2)");

        // 4. Story card: expression line and say-hints under the briefing panel.
        var ui = d.transform.Find("Lesson interface");
        var briefing = d.briefing.transform;
        Replace(briefing, "Expression line"); Replace(briefing, "Say hints");
        lesson.expressionLine = CardText(briefing, "Expression line", new Vector2(0, -127), new Vector2(830, 30), style.headingFont, 28, style.primary);
        lesson.sayHints = CardText(briefing, "Say hints", new Vector2(0, -157), new Vector2(830, 24), style.bodyFont, 18, style.muted);

        // 4b. Owner 2026-09-17: splitter station (splitting only worked by voice). Under the chapter objects so it shows
        //     with a chapter; CrateSplitter hides it during the concept intro. Never in a fallback button group.
        Replace(chapter, "Dock splitter");
        var splitter = Splitter(chapter, d, lesson);
        // The STAGING tag lay on the deck in front of the staging platform, where the console stands: it moves onto the
        // platform's front edge, still in front of the parcels.
        var stagingTag = terminal.transform.Find("STAGING tag") as RectTransform;
        if (stagingTag != null) stagingTag.anchoredPosition3D = new Vector3(0.49f, 0.047f, -0.205f);
        report.Add("splitter pad " + SplitterPadSize + " at " + SplitterPadCenter + ", console with 2 buttons");

        // 5. Button rows, each inside one CanvasGroup so the guide can hide them while it listens.
        var chapterRow = lesson.chapterButtons != null ? lesson.chapterButtons : ui.Find("Fraction controls")?.gameObject;
        if (chapterRow == null || lesson.submitButton == null || lesson.splitButton == null || lesson.resetButton == null) throw new InvalidOperationException("Chapter controls missing.");
        lesson.chapterButtons = chapterRow;
        lesson.chapterButtonGroup = Ensure<CanvasGroup>(chapterRow);
        RowButton(lesson.splitButton, "Split", "Split", 0);
        RowButton(lesson.submitButton, "Load", "Load", 1);
        RowButton(lesson.resetButton, "Reset", "Reset", 2);
        Replace(chapterRow.transform, "Next chapter");
        lesson.nextButton = MakeButton(chapterRow.transform, "Next chapter", lesson.submitButton, lesson.NextChapter);
        RowButton(lesson.nextButton, "Next chapter", "Next chapter", 3);

        var onboardingRow = briefing.Find("Onboarding buttons");
        if (onboardingRow == null)
        {
            var go = new GameObject("Onboarding buttons", typeof(RectTransform)); go.transform.SetParent(briefing, false);
            go.GetComponent<RectTransform>().sizeDelta = Vector2.zero; onboardingRow = go.transform;
        }
        onboardingRow.SetAsLastSibling();
        var onboardingGroup = Ensure<CanvasGroup>(onboardingRow.gameObject);
        var back = ui.GetComponentsInChildren<Button>(true).First(b => b.name == "Back to lessons");
        foreach (var b in new[] { d.primary, d.help, back }) if (b.transform.parent != onboardingRow) b.transform.SetParent(onboardingRow, false);
        Place(d.primary, new Vector2(-265, RowY), new Vector2(250, RowHeight), 23);
        Place(d.help, new Vector2(0, RowY), new Vector2(250, RowHeight), 23);
        Place(back, new Vector2(RowStep * 2, RowY), new Vector2(RowWidth, RowHeight), 17);
        var placement = d.GetComponent<ComfortPlacement>();
        if (placement != null && placement.statusText != null)
        {
            var r = placement.statusText.rectTransform; r.anchoredPosition = new Vector2(0, 219); r.sizeDelta = new Vector2(880, 26); placement.statusText.fontSize = 16;
        }
        n.fallbackButtons = new[] { onboardingGroup, lesson.chapterButtonGroup };
        report.Add("button rows grouped: onboarding (" + onboardingRow.GetComponentsInChildren<Button>(true).Length + "), chapter (" + chapterRow.GetComponentsInChildren<Button>(true).Length + ")");

        // 6. Regression fix: Pause/Music were wired twice (persistent listener + NerdyDirector.Start AddListener),
        //    so each press toggled on and off again. Keep the runtime listener only.
        int removed = 0;
        foreach (var (button, method) in new[] { (n.pauseButton, "TogglePause"), (n.musicButton, "ToggleMusic") })
        {
            if (button == null) continue;
            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                if (button.onClick.GetPersistentTarget(i) is NerdyDirector && button.onClick.GetPersistentMethodName(i) == method)
                { UnityEventTools.RemovePersistentListener(button.onClick, i); removed++; }
        }
        report.Add("double-wired HUD listeners removed: " + removed);

        // 7. Press diagnostics on every lesson card and assistant bar button.
        int logged = 0;
        var buttons = ui.GetComponentsInChildren<Button>(true).ToList();
        if (n.hudRoot != null) buttons.AddRange(n.hudRoot.GetComponentsInChildren<Button>(true));
        foreach (var b in buttons) if (b.GetComponent<UiPressLog>() == null) { b.gameObject.AddComponent<UiPressLog>(); logged++; }
        report.Add("UiPressLog added to " + logged + " of " + buttons.Count + " buttons");

        // 8. Table handle pauses every piece grab, quarters included.
        var handle = d.GetComponent<TableHandle>();
        if (handle != null)
        {
            handle.pieceInteractables = d.GetComponentsInChildren<GrabInteractable>(true).Where(g => g != handle.handleInteractable).ToArray();
            handle.lesson = lesson;
            EditorUtility.SetDirty(handle);
            report.Add("table handle gates " + handle.pieceInteractables.Length + " piece interactables");
        }

        // 9. Typography inventory stays explicit for the layout test.
        var binding = d.GetComponent<TypographyBindings>();
        if (binding != null)
        {
            var headings = binding.headings.Where(t => t != null).ToList();
            if (!headings.Contains(lesson.expressionLine)) headings.Add(lesson.expressionLine);
            var allTexts = d.GetComponentsInChildren<TMP_Text>(true);
            binding.headings = allTexts.Where(t => headings.Contains(t) || t.name == "Catalog title" || t.name == "Heading" || t.name.StartsWith("Loaded tag text") || t.name == "ONE CONTAINER label" || t.name == "Splitter title").ToArray();
            binding.bodies = allTexts.Except(binding.headings).ToArray();
            binding.Apply();
            EditorUtility.SetDirty(binding);
        }

        // 10. Owner 2026-09-17: "What is a fraction?" runs between Start fractions and chapter 1. Start fractions and the
        //     intro's Next go through CargoStation.ContinueFromReady, which starts chapter 1 after the last step.
        var cargoStation = d.GetComponent<CargoStation>();
        if (cargoStation == null) cargoStation = d.gameObject.AddComponent<CargoStation>();
        cargoStation.onboarding = d; cargoStation.lesson = lesson; cargoStation.cardId = CargoStation.CardId;
        for (int i = d.whenReadyContinue.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(d.whenReadyContinue, i);
        UnityEventTools.AddPersistentListener(d.whenReadyContinue, cargoStation.ContinueFromReady);
        EditorUtility.SetDirty(d); EditorUtility.SetDirty(cargoStation);
        report.Add("whenReadyContinue -> CargoStation.ContinueFromReady (concept intro, then chapter 1)");

        // Default (closed-lesson) state.
        foreach (var q in quarters) q.piece.gameObject.SetActive(false);
        lesson.halfA.lockedMark.SetActive(false); lesson.halfB.lockedMark.SetActive(false);
        chapterRow.SetActive(false);
        lesson.chapterObjects.SetActive(false);

        EditorUtility.SetDirty(lesson); EditorUtility.SetDirty(n); EditorUtility.SetDirty(bay); EditorUtility.SetDirty(terminal); EditorUtility.SetDirty(splitter);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        AssetDatabase.SaveAssets();
        return "Dock workbench built: " + string.Join("; ", report) + ". Scene saved.";
    }

    // ---- pieces ----
    static CargoLessonDirector.PieceView Quarter(Transform parent, int index, Vector3 tray)
    {
        float length = RulerLayout.PieceLength(2);
        var root = new GameObject("Quarter crate " + index); root.transform.SetParent(parent, false); root.transform.localPosition = tray;
        var collider = root.AddComponent<BoxCollider>(); collider.size = new Vector3(length, 0.055f, 0.06f);
        Rounded("Body", root.transform, Vector3.zero, new Vector3(length, 0.055f, 0.06f), orange, 0.014f);
        Rounded("Cut edge left", root.transform, new Vector3(-(length * 0.5f - 0.0016f), 0, 0), new Vector3(0.003f, 0.057f, 0.062f), cream, 0.0014f);
        Rounded("Cut edge right", root.transform, new Vector3(length * 0.5f - 0.0016f, 0, 0), new Vector3(0.003f, 0.057f, 0.062f), cream, 0.0014f);
        Rounded("Sticker", root.transform, new Vector3(0, 0.0285f, 0), new Vector3(0.052f, 0.002f, 0.048f), cream, 0.0009f);
        QuickActionsAPI.AddGrabInteraction(root);
        var grabbable = root.GetComponentInChildren<Grabbable>();
        if (grabbable == null) throw new InvalidOperationException("SDK did not create Grabbable for quarter " + index);
        grabbable.MaxGrabPoints = 1;
        var label = Group("Label", root.transform, new Vector3(0, 0.0299f, 0)); label.localRotation = Quaternion.Euler(90, 0, 0);
        var view = label.gameObject.AddComponent<FractionNotationView>();
        view.numerator = WorldText("Numerator", label, new Vector3(0, 0.017f, 0), "1", 0.16f, Quaternion.identity, style.bodyFont, style.panel);
        view.denominator = WorldText("Denominator", label, new Vector3(0, -0.017f, 0), "4", 0.16f, Quaternion.identity, style.bodyFont, style.panel);
        view.fractionBar = Rounded("Fraction bar", label, Vector3.zero, new Vector3(0.022f, 0.0025f, 0.001f), navy, 0.0005f);
        var quarter = new CargoLessonDirector.PieceView { id = "quarter-" + index, piece = root.transform, grabbable = grabbable, label = view, trayPosition = tray };
        ChunkyCrate(quarter, 2, true, true);
        return quarter;
    }

    /// Owner 2026-09-17: resizes an existing crate in place to CrateHeight x CrateDepth (length stays its cells): grab
    /// box, body (unrounded on a cut face), cut caps, sticker and a label twice as large on the top face. Idempotent.
    static void ChunkyCrate(CargoLessonDirector.PieceView v, int cells, bool roundMin, bool roundMax)
    {
        float length = RulerLayout.PieceLength(cells);
        var size = new Vector3(length, CrateHeight, CrateDepth);
        v.piece.GetComponent<BoxCollider>().size = size;
        var body = v.piece.Find("Body") ?? throw new InvalidOperationException("Body missing on " + v.id);
        body.localPosition = Vector3.zero; body.localScale = Vector3.one;
        body.GetComponent<MeshFilter>().sharedMesh = RoundedMesh(size, 0.02f, roundMin, roundMax);
        foreach (Transform t in v.piece)
        {
            if (t.name.StartsWith("Cut edge"))
            {
                t.localScale = Vector3.one; t.localPosition = new Vector3(t.localPosition.x, 0, 0);
                t.GetComponent<MeshFilter>().sharedMesh = RoundedMesh(new Vector3(0.003f, CrateHeight + 0.002f, CrateDepth + 0.002f), 0.0014f);
            }
            if (t.name == "Sticker")
            {
                t.localScale = Vector3.one; t.localPosition = new Vector3(0, CrateHeight / 2 + 0.001f, 0);
                t.GetComponent<MeshFilter>().sharedMesh = RoundedMesh(new Vector3(Mathf.Min(length - 0.014f, 0.08f), 0.002f, 0.1f), 0.0012f);
            }
        }
        var label = v.label ?? throw new InvalidOperationException("Label missing on " + v.id);
        label.transform.localPosition = new Vector3(0, CrateHeight / 2 + 0.0024f, 0);
        foreach (var (text, y) in new[] { (label.numerator, 0.034f), (label.denominator, -0.034f) })
        {
            text.transform.localPosition = new Vector3(0, y, 0); text.fontSize = 0.32f; text.rectTransform.sizeDelta = new Vector2(0.06f, 0.05f);
        }
        label.fractionBar.transform.localScale = Vector3.one;
        label.fractionBar.GetComponent<MeshFilter>().sharedMesh = RoundedMesh(new Vector3(0.044f, 0.005f, 0.001f), 0.0005f);
    }

    /// Owner 2026-09-17: the practice (and striped demonstration) crate resized in place to the chunky lesson crate: body
    /// mesh and grab box 28 x 11 x 12 cm, stripe on the new top face. The SDK grab setup is untouched. Idempotent.
    static void PracticeCrate(Transform crate)
    {
        if (crate == null) throw new InvalidOperationException("Practice crate missing.");
        var size = new Vector3(RulerLayout.WholeLength, CrateHeight, CrateDepth);
        crate.localScale = Vector3.one; crate.localRotation = Quaternion.identity;
        var filter = crate.GetComponent<MeshFilter>() ?? throw new InvalidOperationException("Mesh missing on " + crate.name);
        filter.sharedMesh = RoundedMesh(size, 0.02f);
        var box = crate.GetComponent<BoxCollider>();
        if (box != null) { box.size = size; box.center = Vector3.zero; }
        var stripe = crate.Find("Demo stripe");
        if (stripe != null)
        {
            stripe.localScale = Vector3.one; stripe.localRotation = Quaternion.identity;
            stripe.localPosition = new Vector3(0, CrateHeight / 2 + 0.0006f, 0);
            stripe.GetComponent<MeshFilter>().sharedMesh = RoundedMesh(new Vector3(0.0364f, 0.0022f, CrateDepth * 1.02f), 0.001f);
        }
    }

    // ---- splitter ----
    static CrateSplitter Splitter(Transform chapter, OnboardingDirector d, CargoLessonDirector lesson)
    {
        var root = new GameObject("Dock splitter"); root.transform.SetParent(chapter, false);
        // Station-aligned frame so the constants below are station-local.
        root.transform.localPosition = chapter.InverseTransformPoint(d.transform.position);
        root.transform.localRotation = Quaternion.Inverse(chapter.rotation) * d.transform.rotation;
        var visuals = Group("Splitter visuals", root.transform, Vector3.zero);

        const float deckTop = 0.0175f;
        var c = SplitterPadCenter; var size = SplitterPadSize;
        Rounded("Splitter pad", visuals, new Vector3(c.x, (deckTop + c.y) / 2, c.z), new Vector3(size.x, c.y - deckTop, size.y), navy, 0.003f);
        Rounded("Splitter inset", visuals, new Vector3(c.x, c.y + 0.0005f, c.z), new Vector3(size.x - 0.015f, 0.002f, size.y - 0.015f), teal, 0.0009f);
        foreach (float x in new[] { -0.07f, 0f, 0.07f })   // cut guides: halves in the middle, quarters on both sides
            Rounded(x == 0f ? "Cut guide half" : "Cut guide quarter", visuals, new Vector3(c.x + x, c.y + 0.0019f, c.z), new Vector3(x == 0f ? 0.004f : 0.0025f, 0.001f, size.y - 0.03f), x == 0f ? yellow : cream, 0.0004f);

        var console = Group("Splitter console", visuals, new Vector3(SplitterConsoleCenter.x, 0, SplitterConsoleCenter.z));
        console.localRotation = Quaternion.Euler(0, ConsoleYaw, 0);
        Rounded("Console foot", console, new Vector3(0, deckTop + 0.005f, 0), new Vector3(0.12f, 0.01f, 0.08f), navy, 0.003f);
        Rounded("Console post", console, new Vector3(0, deckTop + 0.01f + 0.03f / 2, 0), new Vector3(0.03f, 0.03f, 0.03f), navy, 0.006f);
        var tilt = Quaternion.Euler(ConsolePitch, 0, 0);
        var back = Rounded("Console back", console, new Vector3(0, SplitterConsoleCenter.y, 0) + tilt * new Vector3(0, 0, 0.0035f), new Vector3(ConsoleSize.x / 1000f, ConsoleSize.y / 1000f, 0.006f), navy, 0.0025f);
        back.transform.localRotation = tilt;

        var canvasGo = new GameObject("Splitter canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(console, false);
        canvasGo.transform.localPosition = new Vector3(0, SplitterConsoleCenter.y, 0); canvasGo.transform.localRotation = tilt;
        canvasGo.transform.localScale = Vector3.one * 0.001f;
        canvasGo.GetComponent<RectTransform>().sizeDelta = ConsoleSize;
        var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = d.head != null ? d.head.GetComponent<Camera>() : null;

        var cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Airlift/Sprites/NerdyCard.png");
        var pillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Airlift/Sprites/NerdyPill.png");
        var backdrop = d.transform.Find("Lesson interface/Backdrop")?.GetComponent<Image>();
        var panel = new GameObject("Console panel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(canvasGo.transform, false);
        panel.GetComponent<RectTransform>().sizeDelta = ConsoleSize;
        var panelImage = panel.GetComponent<Image>(); panelImage.raycastTarget = false;
        panelImage.sprite = cardSprite; panelImage.type = Image.Type.Sliced; panelImage.pixelsPerUnitMultiplier = 1f;
        panelImage.color = backdrop != null ? new Color(backdrop.color.r, backdrop.color.g, backdrop.color.b, 1f) : new Color32(30, 44, 74, 255);

        var title = CardText(canvasGo.transform, "Splitter title", new Vector2(0, 52), new Vector2(170, 22), style.headingFont, 15, d.heading.color);
        title.text = "Splitter";
        var splitter = root.AddComponent<CrateSplitter>();
        splitter.lesson = lesson; splitter.stationRoot = d.transform; splitter.visuals = visuals.gameObject;
        splitter.padCenter = SplitterPadCenter; splitter.padSize = SplitterPadSize;
        splitter.halvesButton = SplitterButton(canvasGo.transform, "Halves", SplitterRules.ButtonLabel(2), new Vector2(0, 25), d.primary, pillSprite, splitter.ChooseHalves);
        splitter.quartersButton = SplitterButton(canvasGo.transform, "Quarters", SplitterRules.ButtonLabel(4), new Vector2(0, -8), d.primary, pillSprite, splitter.ChooseQuarters);
        var feedback = CardText(canvasGo.transform, "Splitter feedback", new Vector2(0, -44), new Vector2(180, 40), style.bodyFont, 10, d.body.color);
        feedback.textWrappingMode = TextWrappingModes.Normal;
        splitter.feedback = feedback;
        QuickActionsAPI.AddRayCanvasInteraction(canvasGo);
        return splitter;
    }

    static Button SplitterButton(Transform parent, string name, string label, Vector2 pos, Button template, Sprite pill, UnityAction action)
    {
        var size = new Vector2(170, 30);
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchoredPosition = pos; r.sizeDelta = size;
        var img = go.GetComponent<Image>(); var source = template.GetComponent<Image>();
        img.sprite = pill != null ? pill : source.sprite; img.type = Image.Type.Sliced; img.color = source.color;
        // Capsule: the sprite's corner border is exactly half the pill height (CardPolishTests.PillsAreTrueCapsules).
        img.pixelsPerUnitMultiplier = img.sprite != null && img.sprite.border.x > 0 ? img.sprite.border.x * 2f * 100f / (img.sprite.pixelsPerUnit * size.y) : 1f;
        var b = go.GetComponent<Button>(); b.targetGraphic = img; b.transition = template.transition; b.colors = template.colors;
        UnityEventTools.AddPersistentListener(b.onClick, action);
        var outline = go.AddComponent<Outline>(); var sourceOutline = template.GetComponent<Outline>();
        if (sourceOutline != null) { outline.effectColor = sourceOutline.effectColor; outline.effectDistance = sourceOutline.effectDistance; }
        outline.enabled = false;
        go.AddComponent<FocusPointer>();
        go.AddComponent<UiPressLog>();
        var sourceLabel = template.GetComponentInChildren<TMP_Text>(true);
        var text = CardText(go.transform, "Label", Vector2.zero, size - new Vector2(16, 6), style.bodyFont, 15, sourceLabel != null ? sourceLabel.color : style.panel);
        text.text = label;
        return b;
    }

    /// Station-local axis-aligned bounds of one mesh.
    static Bounds StationBounds(Transform root, MeshFilter f)
    {
        var m = f.sharedMesh.bounds; var b = new Bounds(); bool has = false;
        foreach (float x in new[] { m.min.x, m.max.x }) foreach (float y in new[] { m.min.y, m.max.y }) foreach (float z in new[] { m.min.z, m.max.z })
        {
            var p = root.InverseTransformPoint(f.transform.TransformPoint(new Vector3(x, y, z)));
            if (!has) { b = new Bounds(p, Vector3.zero); has = true; } else b.Encapsulate(p);
        }
        return b;
    }

    /// A navy strap across the crate with a small LOCKED plate: the pre-loaded half cannot move.
    static GameObject LockedMark(Transform piece, float length)
    {
        Replace(piece, "Locked mark");
        var mark = Group("Locked mark", piece, Vector3.zero);
        Rounded("Strap", mark, new Vector3(-length * 0.28f, 0, 0), new Vector3(0.012f, CrateHeight + 0.003f, CrateDepth + 0.004f), navy, 0.003f);
        var plate = Group("Plate", mark, new Vector3(length * 0.32f, CrateHeight / 2 + 0.0035f, 0)); plate.localRotation = Quaternion.Euler(90, 0, 0);
        Rounded("Plate body", plate, new Vector3(0, 0, 0.0008f), new Vector3(0.044f, 0.022f, 0.0016f), yellow, 0.0007f);
        var text = WorldText("Locked text", plate, Vector3.zero, "LOCKED", 0.07f, Quaternion.identity, style.headingFont, style.panel);
        text.rectTransform.sizeDelta = new Vector2(0.044f, 0.022f);
        mark.gameObject.SetActive(false);
        return mark.gameObject;
    }

    // ---- vehicles (rear toward the learner, cab +z; the bed is exactly bedCells container cells wide) ----
    // Owner 2026-09-17: "the trucks don't really look like trucks". Root-local, y 0 = deck top, bed centred on x 0 over
    // the cells, rear bumper at RearZ. Every part stays inside the bed width so neighbours never overlap, low parts
    // stay inside the lane clearances (inset wheels), and the tops stay under the lesson-card sightline. Beds are
    // deep enough for the chunky crates and cabs rise above them.
    static VehicleBay.Vehicle BuildVehicle(Transform parent, string name, string kind, int bedCells, Material body)
    {
        float w = RulerLayout.PieceLength(bedCells), bed = BedTop - 0.0175f;   // bed floor top, root-local
        var root = Group(name, parent, Vector3.zero);
        Rounded("Bed floor", root, new Vector3(0, bed - 0.003f, 0), new Vector3(w, 0.006f, BedDepth), cream, 0.0027f);
        float nose; GameObject tag;
        if (kind == "truck")
        {
            // Semi: long flatbed trailer (the bed) pulled by a narrower tractor with a sleeper cab and exhaust stacks.
            Rounded("Chassis", root, new Vector3(0, 0.024f, 0.12f), new Vector3(w - 0.06f, 0.01f, 0.365f), rubber);
            Rounded("Trailer deck", root, new Vector3(0, bed - 0.003f, 0.11125f), new Vector3(w, 0.006f, 0.0925f), cream, 0.0027f);
            foreach (float side in new[] { -1f, 1f })
                Rounded(side < 0 ? "Rail left" : "Rail right", root, new Vector3(side * (w / 2 - 0.002f), bed + 0.004f, 0.04625f), new Vector3(0.004f, 0.008f, 0.2225f), navy, 0.0015f);
            Rounded("Front wall", root, new Vector3(0, bed + 0.025f, 0.1605f), new Vector3(w - 0.004f, 0.05f, 0.006f), rubber, 0.0025f);
            Rounded("Cab", root, new Vector3(0, 0.0875f, 0.2025f), new Vector3(0.16f, 0.115f, 0.07f), body, 0.012f);
            Rounded("Rear window", root, new Vector3(0, 0.12f, 0.166f), new Vector3(0.1f, 0.024f, 0.003f), navy, 0.0012f);
            Rounded("Windshield", root, new Vector3(0, 0.117f, 0.239f), new Vector3(0.13f, 0.034f, 0.003f), navy, 0.0012f);
            foreach (float side in new[] { -1f, 1f })
                Rounded(side < 0 ? "Side window left" : "Side window right", root, new Vector3(side * 0.0815f, 0.117f, 0.2175f), new Vector3(0.003f, 0.03f, 0.03f), navy, 0.0012f);
            for (int i = -1; i <= 1; i++)
                Rounded("Marker light", root, new Vector3(i * 0.03f, 0.1475f, 0.2275f), new Vector3(0.01f, 0.005f, 0.006f), orange, 0.002f);
            Rounded("Hood", root, new Vector3(0, 0.05f, 0.2725f), new Vector3(0.14f, 0.04f, 0.07f), body, 0.01f);
            Rounded("Grille", root, new Vector3(0, 0.049f, 0.309f), new Vector3(0.08f, 0.03f, 0.004f), cream, 0.0015f);
            for (int i = 0; i < 3; i++) Rounded("Grille bar", root, new Vector3(0, 0.039f + i * 0.01f, 0.3113f), new Vector3(0.072f, 0.003f, 0.002f), rubber, 0.0009f);
            Lamps(root, "Headlight", 0.058f, 0.057f, 0.309f, new Vector3(0.02f, 0.014f, 0.004f), cream);
            Rounded("Front bumper", root, new Vector3(0, 0.024f, 0.3145f), new Vector3(0.15f, 0.012f, 0.01f), rubber, 0.003f);
            foreach (float side in new[] { -1f, 1f })
            {
                Rounded("Exhaust stack", root, new Vector3(side * 0.092f, 0.09f, 0.1795f), new Vector3(0.009f, 0.12f, 0.009f), cream, 0.004f);
                Rounded(side < 0 ? "Side mirror left" : "Side mirror right", root, new Vector3(side * 0.095f, 0.125f, 0.2325f), new Vector3(0.006f, 0.02f, 0.012f), rubber, 0.002f);
                Rounded("Fender", root, new Vector3(side * 0.09f, 0.043f, 0.2775f), new Vector3(0.018f, 0.007f, 0.045f), body, 0.003f);
                Rounded("Mud flap", root, new Vector3(side * 0.105f, 0.017f, -0.0615f), new Vector3(0.026f, 0.022f, 0.003f), rubber, 0.0012f);
            }
            Rounded("Rear bumper", root, new Vector3(0, 0.02f, -0.0685f), new Vector3(w - 0.04f, 0.01f, 0.008f), rubber, 0.003f);
            Lamps(root, "Tail light", 0.11f, 0.029f, -0.07f, new Vector3(0.024f, 0.01f, 0.004f), red);
            Rounded("License plate", root, new Vector3(0, 0.032f, -0.071f), new Vector3(0.03f, 0.013f, 0.003f), yellow, 0.0012f);
            foreach (float z in new[] { -0.035f, 0.035f }) Wheels(root, 0.105f, z, 0.014f, 0.014f);   // under the flatbed, never through it
            foreach (float z in new[] { 0.1925f, 0.2775f }) Wheels(root, 0.09f, z, 0.019f, 0.014f);
            nose = 0.3195f;
            tag = LoadedTag(root, new Vector3(0, 0.155f, 0.2f), 1f, Quaternion.identity);
        }
        else if (kind == "pickup")
        {
            Rounded("Chassis", root, new Vector3(0, 0.024f, 0.065f), new Vector3(w - 0.03f, 0.01f, 0.255f), rubber);
            foreach (float side in new[] { -1f, 1f })
                Rounded(side < 0 ? "Rail left" : "Rail right", root, new Vector3(side * (w / 2 - 0.0015f), bed + 0.006f, 0.001f), new Vector3(0.003f, 0.012f, 0.135f), body, 0.0012f);
            Rounded("Tailgate", root, new Vector3(0, bed + 0.005f, -0.0675f), new Vector3(w - 0.004f, 0.02f, 0.004f), body, 0.0015f);
            Rounded("Front wall", root, new Vector3(0, bed + 0.012f, 0.067f), new Vector3(w - 0.004f, 0.024f, 0.004f), body, 0.0015f);
            Rounded("Cab", root, new Vector3(0, 0.08f, 0.1075f), new Vector3(0.13f, 0.1f, 0.07f), body, 0.012f);
            Rounded("Rear window", root, new Vector3(0, 0.105f, 0.071f), new Vector3(0.09f, 0.026f, 0.003f), navy, 0.0012f);
            Rounded("Windshield", root, new Vector3(0, 0.105f, 0.144f), new Vector3(0.11f, 0.028f, 0.003f), navy, 0.0012f);
            foreach (float side in new[] { -1f, 1f })
                Rounded(side < 0 ? "Side window left" : "Side window right", root, new Vector3(side * 0.0655f, 0.105f, 0.1075f), new Vector3(0.003f, 0.026f, 0.04f), navy, 0.0012f);
            Rounded("Roof light", root, new Vector3(0, 0.1325f, 0.1075f), new Vector3(0.06f, 0.006f, 0.01f), yellow, 0.002f);
            Rounded("Hood", root, new Vector3(0, 0.045f, 0.175f), new Vector3(0.126f, 0.03f, 0.065f), body, 0.008f);
            Rounded("Grille", root, new Vector3(0, 0.045f, 0.209f), new Vector3(0.07f, 0.02f, 0.004f), navy, 0.0015f);
            Lamps(root, "Headlight", 0.048f, 0.05f, 0.209f, new Vector3(0.018f, 0.012f, 0.004f), cream);
            Rounded("Front bumper", root, new Vector3(0, 0.024f, 0.2135f), new Vector3(w - 0.004f, 0.01f, 0.01f), cream, 0.003f);
            Rounded("Rear bumper", root, new Vector3(0, 0.022f, -0.0685f), new Vector3(w - 0.004f, 0.01f, 0.008f), cream, 0.003f);
            Lamps(root, "Tail light", 0.06f, 0.045f, -0.07f, new Vector3(0.012f, 0.018f, 0.004f), red);
            Rounded("License plate", root, new Vector3(0, 0.033f, -0.071f), new Vector3(0.028f, 0.012f, 0.003f), yellow, 0.0012f);
            foreach (float side in new[] { -1f, 1f })
                Rounded(side < 0 ? "Side mirror left" : "Side mirror right", root, new Vector3(side * 0.067f, 0.11f, 0.1395f), new Vector3(0.006f, 0.014f, 0.01f), rubber, 0.002f);
            foreach (float z in new[] { -0.03f, 0.1675f }) Wheels(root, 0.058f, z, 0.015f, 0.012f);
            nose = 0.2185f;
            tag = LoadedTag(root, new Vector3(0, 0.155f, 0.1075f), 0.85f, Quaternion.identity);
        }
        else
        {
            // Van: small cab-over delivery truck with an open bed.
            Rounded("Chassis", root, new Vector3(0, 0.024f, 0.04f), new Vector3(w - 0.02f, 0.01f, 0.205f), rubber);
            foreach (float side in new[] { -1f, 1f })
                Rounded(side < 0 ? "Rail left" : "Rail right", root, new Vector3(side * (w / 2 - 0.0015f), bed + 0.006f, 0.001f), new Vector3(0.003f, 0.012f, 0.133f), body, 0.0012f);
            Rounded("Tailgate", root, new Vector3(0, bed + 0.003f, -0.0675f), new Vector3(w - 0.004f, 0.016f, 0.004f), body, 0.0015f);
            Rounded("Front wall", root, new Vector3(0, bed + 0.015f, 0.067f), new Vector3(w - 0.004f, 0.03f, 0.004f), body, 0.0015f);
            Rounded("Cab", root, new Vector3(0, 0.083f, 0.1055f), new Vector3(w - 0.004f, 0.11f, 0.07f), body, 0.012f);
            Rounded("Rear window", root, new Vector3(0, 0.11f, 0.069f), new Vector3(0.044f, 0.024f, 0.003f), navy, 0.0012f);
            Rounded("Windshield", root, new Vector3(0, 0.105f, 0.142f), new Vector3(0.058f, 0.036f, 0.003f), navy, 0.0012f);
            foreach (float side in new[] { -1f, 1f })
                Rounded(side < 0 ? "Side window left" : "Side window right", root, new Vector3(side * 0.0335f, 0.108f, 0.1195f), new Vector3(0.003f, 0.028f, 0.035f), navy, 0.0012f);
            Rounded("Grille", root, new Vector3(0, 0.042f, 0.142f), new Vector3(0.04f, 0.012f, 0.003f), navy, 0.0012f);
            Lamps(root, "Headlight", 0.022f, 0.06f, 0.142f, new Vector3(0.012f, 0.01f, 0.003f), cream);
            Rounded("Front bumper", root, new Vector3(0, 0.024f, 0.1445f), new Vector3(w - 0.004f, 0.01f, 0.008f), rubber, 0.003f);
            Rounded("Rear bumper", root, new Vector3(0, 0.022f, -0.0695f), new Vector3(w - 0.004f, 0.008f, 0.006f), rubber, 0.0025f);
            Lamps(root, "Tail light", 0.027f, 0.04f, -0.071f, new Vector3(0.01f, 0.014f, 0.003f), red);
            Rounded("License plate", root, new Vector3(0, 0.031f, -0.071f), new Vector3(0.022f, 0.01f, 0.003f), yellow, 0.0012f);
            foreach (float side in new[] { -1f, 1f })
                Rounded(side < 0 ? "Side mirror left" : "Side mirror right", root, new Vector3(side * 0.033f, 0.115f, 0.1355f), new Vector3(0.004f, 0.012f, 0.008f), rubber, 0.0015f);
            foreach (float z in new[] { -0.035f, 0.1175f }) Wheels(root, 0.029f, z, 0.014f, 0.01f);
            nose = 0.1485f;
            tag = LoadedTag(root, new Vector3(0, 0.155f, 0.1055f), 0.7f, Quaternion.identity);
        }
        return new VehicleBay.Vehicle { kind = kind, bedCells = bedCells, root = root, loadedTag = tag, noseZ = nose };
    }

    static void Lamps(Transform root, string name, float x, float y, float z, Vector3 size, Material m)
    {
        Rounded(name + " left", root, new Vector3(-x, y, z), size, m, Mathf.Min(size.x, size.y) * 0.3f);
        Rounded(name + " right", root, new Vector3(x, y, z), size, m, Mathf.Min(size.x, size.y) * 0.3f);
    }

    /// A pair of wheels on one axle, centred at ±x, with cream hubs on the outer faces.
    static void Wheels(Transform root, float x, float z, float radius, float width)
    {
        foreach (float side in new[] { -1f, 1f })
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder); wheel.name = "Wheel";
            UnityEngine.Object.DestroyImmediate(wheel.GetComponent<Collider>());
            wheel.transform.SetParent(root, false); wheel.transform.localPosition = new Vector3(side * x, radius, z);
            wheel.transform.localRotation = Quaternion.Euler(0, 0, 90); wheel.transform.localScale = new Vector3(radius * 2, width / 2, radius * 2);
            wheel.GetComponent<Renderer>().sharedMaterial = rubber;
            var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder); hub.name = "Hub";
            UnityEngine.Object.DestroyImmediate(hub.GetComponent<Collider>());
            hub.transform.SetParent(root, false); hub.transform.localPosition = new Vector3(side * (x + width / 2 - 0.001f), radius, z);
            hub.transform.localRotation = Quaternion.Euler(0, 0, 90); hub.transform.localScale = new Vector3(radius, 0.001f, radius);
            hub.GetComponent<Renderer>().sharedMaterial = cream;
        }
    }

    /// Owner 2026-09-17: a DOCK EXIT gate over the exit lane on the left, with lane chevrons pointing to it. Posts stand
    /// outside the widest vehicle's lane band; the beam clears the tallest cab; the sign faces the learner.
    static void DockExit(Transform terminal, Transform station)
    {
        var gate = Group("Dock exit", terminal, terminal.InverseTransformPoint(station.TransformPoint(new Vector3(ExitGateX, 0.0175f, LaneZ))));
        gate.localRotation = Quaternion.Inverse(terminal.rotation) * station.rotation;
        const float postZ = 0.155f, top = 0.166f;   // beam clears the tallest cab and the chunky crates in the beds
        foreach (float side in new[] { -1f, 1f })
        {
            var post = Group(side < 0 ? "Front post" : "Back post", gate, new Vector3(0, 0, side * postZ));
            Rounded("Post", post, new Vector3(0, top / 2, 0), new Vector3(0.016f, top, 0.016f), navy, 0.005f);
            for (int i = 0; i < 3; i++) Rounded("Stripe", post, new Vector3(0, 0.03f + i * 0.045f, 0), new Vector3(0.018f, 0.014f, 0.018f), yellow, 0.005f);
            Rounded("Foot", post, new Vector3(0, 0.004f, 0), new Vector3(0.03f, 0.008f, 0.03f), navy, 0.003f);
        }
        Rounded("Beam", gate, new Vector3(0, top - 0.004f, 0), new Vector3(0.02f, 0.014f, 2 * postZ + 0.02f), yellow, 0.005f);
        var sign = Group("Sign", gate, new Vector3(0, top - 0.02f, -postZ - 0.014f));
        Rounded("Sign board", sign, Vector3.zero, new Vector3(0.14f, 0.034f, 0.005f), navy, 0.004f);
        var text = WorldText("Sign text", sign, new Vector3(0.01f, 0, -0.0031f), "DOCK EXIT", 0.14f, Quaternion.identity, style.headingFont, Color.white);
        text.rectTransform.sizeDelta = new Vector2(0.11f, 0.03f);
        foreach (float a in new[] { -40f, 40f })
        {
            var bar = Rounded("Arrow", sign, new Vector3(-0.051f, a < 0 ? -0.004f : 0.004f, -0.003f), new Vector3(0.014f, 0.0035f, 0.002f), yellow, 0.0012f);   // "<" toward the exit
            bar.transform.localRotation = Quaternion.Euler(0, 0, a);
        }
        for (int i = 0; i < 3; i++)
        {
            // Station x -0.20, -0.28, -0.52: clear of the ruler's dock-edge strip (|x| <= 0.155) and the gate posts.
            var chevron = Group("Lane chevron", gate, new Vector3(new[] { 0.2f, 0.12f, -0.12f }[i], 0.001f, 0));
            chevron.localRotation = Quaternion.Euler(0, 180f, 0);   // point left, toward the exit
            foreach (float a in new[] { -35f, 35f })
            {
                var bar = Rounded("Chevron bar", chevron, new Vector3(0.008f, 0, a < 0 ? -0.011f : 0.011f), new Vector3(0.03f, 0.002f, 0.006f), yellow, 0.0009f);
                bar.transform.localRotation = Quaternion.Euler(0, a, 0);
            }
        }
    }

    /// Toy dock crane: base, tower, operator cab, jib (pointing along x by jibDir), counter-jib and weight,
    /// cable, hook and a crate being unloaded. Rounded meshes only, no colliders.
    static Transform Crane(Transform parent, string name, Vector3 p, float jibDir)
    {
        var root = Group(name, parent, p);
        // Compact: every part stays under ~0.19 m board height so it reads below the lesson card from the seated
        // head instead of showing through the translucent card (owner render check 2026-09-16).
        Rounded("Base", root, new Vector3(0, 0.008f, 0), new Vector3(0.07f, 0.016f, 0.07f), navy);
        Rounded("Tower", root, new Vector3(0, 0.08f, 0), new Vector3(0.02f, 0.13f, 0.02f), yellow);
        Rounded("Operator cab", root, new Vector3(0, 0.13f, -0.018f), new Vector3(0.036f, 0.026f, 0.028f), teal);
        Rounded("Jib", root, new Vector3(jibDir * 0.1f, 0.152f, 0), new Vector3(0.23f, 0.012f, 0.016f), yellow);
        Rounded("Counter jib", root, new Vector3(-jibDir * 0.045f, 0.152f, 0), new Vector3(0.07f, 0.011f, 0.014f), yellow);
        Rounded("Counterweight", root, new Vector3(-jibDir * 0.072f, 0.14f, 0), new Vector3(0.024f, 0.022f, 0.024f), navy);
        Rounded("Cable", root, new Vector3(jibDir * 0.19f, 0.139f, 0), new Vector3(0.002f, 0.022f, 0.002f), cream, 0.0009f);
        Rounded("Hook", root, new Vector3(jibDir * 0.19f, 0.126f, 0), new Vector3(0.012f, 0.008f, 0.012f), navy);
        Rounded("Hanging crate", root, new Vector3(jibDir * 0.19f, 0.108f, 0), new Vector3(0.036f, 0.024f, 0.032f), orange);
        return root;
    }

    /// Upright yellow plate reading LOADED, facing the player (station -z).
    static GameObject LoadedTag(Transform vehicle, Vector3 p, float scale, Quaternion faceRotation)
    {
        var tag = Group("Loaded tag", vehicle, p); tag.localRotation = faceRotation; tag.localScale = Vector3.one * scale;
        Rounded("Plate", tag, new Vector3(0, 0, 0.003f), new Vector3(0.09f, 0.028f, 0.004f), yellow, 0.0018f);
        var text = WorldText("Loaded tag text", tag, Vector3.zero, "LOADED", 0.1f, Quaternion.identity, style.headingFont, style.panel);
        text.rectTransform.sizeDelta = new Vector2(0.09f, 0.028f);
        tag.gameObject.SetActive(false);
        return tag.gameObject;
    }

    // ---- UI ----
    static TMP_Text CardText(Transform parent, string name, Vector2 pos, Vector2 size, TMP_FontAsset font, float fontSize, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>(); t.font = font; t.fontSize = fontSize; t.color = color; t.text = "";
        t.alignment = TextAlignmentOptions.Center; t.textWrappingMode = TextWrappingModes.NoWrap; t.overflowMode = TextOverflowModes.Overflow; t.raycastTarget = false;
        t.rectTransform.anchoredPosition = pos; t.rectTransform.sizeDelta = size;
        return t;
    }

    static void RowButton(Button button, string name, string label, int slotIndex)
    {
        button.name = name;
        Place(button, new Vector2(-RowStep * 2 + slotIndex * RowStep, RowY), new Vector2(RowWidth, RowHeight), 20);
        var text = button.GetComponentInChildren<TMP_Text>(true); if (text != null) text.text = label;
    }

    static void Place(Button button, Vector2 pos, Vector2 size, float labelSize)
    {
        var r = button.GetComponent<RectTransform>(); r.anchoredPosition = pos; r.sizeDelta = size;
        var text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null) { text.rectTransform.anchoredPosition = Vector2.zero; text.rectTransform.sizeDelta = size - new Vector2(16, 10); text.fontSize = labelSize; }
    }

    static Button MakeButton(Transform parent, string name, Button template, UnityAction action)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>(); var source = template.GetComponent<Image>();
        img.sprite = source.sprite; img.type = source.type; img.color = source.color; img.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        var b = go.GetComponent<Button>(); b.targetGraphic = img; b.transition = template.transition; b.colors = template.colors;
        UnityEventTools.AddPersistentListener(b.onClick, action);
        var outline = go.AddComponent<Outline>(); var sourceOutline = template.GetComponent<Outline>();
        if (sourceOutline != null) { outline.effectColor = sourceOutline.effectColor; outline.effectDistance = sourceOutline.effectDistance; }
        outline.enabled = false;
        go.AddComponent<FocusPointer>();
        var sourceLabel = template.GetComponentInChildren<TMP_Text>(true);
        var child = new GameObject("Label", typeof(RectTransform)); child.transform.SetParent(go.transform, false);
        var t = child.AddComponent<TextMeshProUGUI>(); t.font = sourceLabel != null ? sourceLabel.font : style.bodyFont; t.color = sourceLabel != null ? sourceLabel.color : style.panel;
        t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false; t.textWrappingMode = TextWrappingModes.NoWrap; t.text = name;
        return b;
    }

    // ---- kit ----
    static T Ensure<T>(GameObject go) where T : Component
    {
        var existing = go.GetComponent<T>();
        return existing != null ? existing : go.AddComponent<T>();
    }

    static void Replace(Transform parent, string name)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
            if (parent.GetChild(i).name == name) UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    static Transform Group(string name, Transform parent, Vector3 p) { var g = new GameObject(name); g.transform.SetParent(parent, false); g.transform.localPosition = p; return g.transform; }

    static GameObject Rounded(string name, Transform parent, Vector3 p, Vector3 size, Material m, float radius = -1f)
    {
        if (radius < 0) radius = Mathf.Min(0.012f, 0.45f * Mathf.Min(size.x, Mathf.Min(size.y, size.z)));
        var g = new GameObject(name); g.transform.SetParent(parent, false); g.transform.localPosition = p;
        g.AddComponent<MeshFilter>().sharedMesh = RoundedMesh(size, radius);
        var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = m; r.receiveShadows = true;
        return g;
    }

    static Mesh RoundedMesh(Vector3 size, float radius, bool roundMinX = true, bool roundMaxX = true)
    {
        string F(float v) => v.ToString("0.####", CultureInfo.InvariantCulture);
        string key = F(size.x) + "x" + F(size.y) + "x" + F(size.z) + "-r" + F(radius) + (roundMinX ? "" : "-cutL") + (roundMaxX ? "" : "-cutR");
        if (meshCache.TryGetValue(key, out var cached)) return cached;
        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Application.dataPath), MeshFolder));
        string path = MeshFolder + "/RoundedBox-" + key.Replace(".", "_") + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null) { mesh = RoundedBoxMesh.Build(size, radius, 4, roundMinX, roundMaxX); AssetDatabase.CreateAsset(mesh, path); }
        meshCache[key] = mesh; return mesh;
    }

    static TMP_Text WorldText(string name, Transform parent, Vector3 p, string text, float size, Quaternion rotation, TMP_FontAsset font, Color color)
    {
        var g = Group(name, parent, p); g.localRotation = rotation;
        var label = g.gameObject.AddComponent<TextMeshPro>(); label.font = font; label.text = text; label.fontSize = size;
        label.color = color; label.alignment = TextAlignmentOptions.Center; label.textWrappingMode = TextWrappingModes.NoWrap;
        label.rectTransform.sizeDelta = new Vector2(0.12f, 0.03f);
        return label;
    }

    static Material Mat(string name)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/" + name + ".mat");
        if (m == null) throw new InvalidOperationException("Material missing: " + name);
        return m;
    }

    static Material RedMaterial()
    {
        const string path = "Assets/Airlift/Materials/CargoRed.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m != null) return m;
        m = new Material(orange) { name = "CargoRed" };
        m.color = new Color32(226, 72, 72, 255);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static Material SlotMaterial()
    {
        const string path = "Assets/Airlift/Materials/CargoSlot.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m != null) return m;
        m = new Material(teal) { name = "CargoSlot" };
        m.color = new Color32(150, 236, 214, 255); m.SetFloat("_Smoothness", 0.3f);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
