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
    public const float TrayZ = -0.225f;   // far enough in front of the dock-edge strip that waiting crates never cover its marks
    public static readonly Vector3 QuarterTrayOrigin = new Vector3(-0.265f, 0.047f, TrayZ);
    public const float QuarterTraySpacing = 0.09f;
    const float RowY = -196f, RowHeight = 48f, RowWidth = 165f, RowStep = 172f;
    // Round 2 geometry (station-local metres): bed floor top, bed depth, dock-edge strip z (ruler-local).
    public const float BedTop = 0.052f, BedDepth = 0.075f, EdgeZ = -0.085f;

    static AirliftStyle style;
    static Material orange, cream, teal, navy, rubber, yellow, slot;
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
        slot = SlotMaterial();
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
            v.trayPosition = new Vector3(v.trayPosition.x, v.trayPosition.y, TrayZ);
            v.piece.localPosition = new Vector3(v.piece.localPosition.x, v.piece.localPosition.y, TrayZ);
        }
        lesson.quarters = quarters;
        lesson.whole.id = "whole"; lesson.halfA.id = "half-1"; lesson.halfB.id = "half-2";
        lesson.halfA.lockedMark = LockedMark(lesson.halfA.piece, RulerLayout.PieceLength(4));
        lesson.halfB.lockedMark = LockedMark(lesson.halfB.piece, RulerLayout.PieceLength(4));
        report.Add("4 quarter crates + locked marks on both halves");

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
        lesson.restHeight = BedTop + 0.0275f;
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
        bay.ruler = ruler; bay.parkHeight = 0.0175f; bay.driveDistance = 0.45f; bay.driveSeconds = 1.5f;
        bay.idleKind = "truck"; bay.idlePosition = new Vector3(-0.49f, 0.0175f, -0.02f);
        bay.unusedCells = unusedCells; bay.notNeededLabel = notNeeded.gameObject;
        var list = new List<VehicleBay.Vehicle> { BuildVehicle(bayGo.transform, "Big truck", "truck", 8, yellow) };
        for (int i = 1; i <= 2; i++) list.Add(BuildVehicle(bayGo.transform, "Pickup " + i, "pickup", 4, i == 1 ? teal : orange));
        for (int i = 1; i <= 4; i++) list.Add(BuildVehicle(bayGo.transform, "Van " + i, "van", 2, i % 2 == 1 ? cream : teal));
        foreach (var v in list) v.root.gameObject.SetActive(false);
        list[0].root.localPosition = bay.idlePosition; list[0].root.gameObject.SetActive(true);
        bay.vehicles = list.ToArray();
        terminal.truck = list[0].root;
        lesson.vehicles = bay;
        EditorUtility.SetDirty(terminal);
        report.Add("terminal: " + migrated + " old props removed, 2 cranes, vehicles 1 truck (8 cells) + 2 pickups (4) + 4 vans (2)");

        // 4. Story card: expression line and say-hints under the briefing panel.
        var ui = d.transform.Find("Lesson interface");
        var briefing = d.briefing.transform;
        Replace(briefing, "Expression line"); Replace(briefing, "Say hints");
        lesson.expressionLine = CardText(briefing, "Expression line", new Vector2(0, -127), new Vector2(830, 30), style.headingFont, 28, style.primary);
        lesson.sayHints = CardText(briefing, "Say hints", new Vector2(0, -157), new Vector2(830, 24), style.bodyFont, 18, style.muted);

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
            binding.headings = allTexts.Where(t => headings.Contains(t) || t.name == "Catalog title" || t.name == "Heading" || t.name.StartsWith("Loaded tag text") || t.name == "ONE CONTAINER label").ToArray();
            binding.bodies = allTexts.Except(binding.headings).ToArray();
            binding.Apply();
            EditorUtility.SetDirty(binding);
        }

        // Default (closed-lesson) state.
        foreach (var q in quarters) q.piece.gameObject.SetActive(false);
        lesson.halfA.lockedMark.SetActive(false); lesson.halfB.lockedMark.SetActive(false);
        chapterRow.SetActive(false);
        lesson.chapterObjects.SetActive(false);

        EditorUtility.SetDirty(lesson); EditorUtility.SetDirty(n); EditorUtility.SetDirty(bay); EditorUtility.SetDirty(terminal);
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
        return new CargoLessonDirector.PieceView { id = "quarter-" + index, piece = root.transform, grabbable = grabbable, label = view, trayPosition = tray };
    }

    /// A navy strap across the crate with a small LOCKED plate: the pre-loaded half cannot move.
    static GameObject LockedMark(Transform piece, float length)
    {
        Replace(piece, "Locked mark");
        var mark = Group("Locked mark", piece, Vector3.zero);
        Rounded("Strap", mark, new Vector3(-length * 0.28f, 0, 0), new Vector3(0.012f, 0.058f, 0.064f), navy, 0.003f);
        var plate = Group("Plate", mark, new Vector3(length * 0.2f, 0.031f, 0)); plate.localRotation = Quaternion.Euler(90, 0, 0);
        Rounded("Plate body", plate, new Vector3(0, 0, 0.0008f), new Vector3(0.05f, 0.016f, 0.0016f), yellow, 0.0007f);
        var text = WorldText("Locked text", plate, Vector3.zero, "LOCKED", 0.055f, Quaternion.identity, style.headingFont, style.panel);
        text.rectTransform.sizeDelta = new Vector2(0.05f, 0.016f);
        mark.gameObject.SetActive(false);
        return mark.gameObject;
    }

    // ---- vehicles (rear toward the learner, cab +z; the bed is exactly bedCells container cells wide) ----
    static VehicleBay.Vehicle BuildVehicle(Transform parent, string name, string kind, int bedCells, Material cabMaterial)
    {
        float w = RulerLayout.PieceLength(bedCells);
        var root = Group(name, parent, Vector3.zero);
        Rounded("Chassis", root, new Vector3(0, 0.022f, 0.03f), new Vector3(w - 0.012f, 0.01f, 0.13f), rubber);
        Rounded("Rear bumper", root, new Vector3(0, 0.024f, -0.04f), new Vector3(w - 0.006f, 0.008f, 0.006f), rubber);
        Rounded("Bed floor", root, new Vector3(0, BedTop - 0.0175f - 0.003f, 0), new Vector3(w, 0.006f, BedDepth), cream, 0.0027f);
        foreach (float side in new[] { -1f, 1f })
            Rounded(side < 0 ? "Rail left" : "Rail right", root, new Vector3(side * (w / 2 - 0.001f), BedTop - 0.0175f + 0.003f, 0), new Vector3(0.002f, 0.006f, BedDepth), navy, 0.0009f);
        Rounded("Front wall", root, new Vector3(0, BedTop - 0.0175f + 0.014f, BedDepth / 2 + 0.002f), new Vector3(w - 0.004f, 0.028f, 0.004f), rubber, 0.0018f);
        Rounded("Cab", root, new Vector3(0, 0.052f, 0.07f), new Vector3(w - 0.008f, 0.05f, 0.05f), cabMaterial);
        Rounded("Rear window", root, new Vector3(0, 0.062f, 0.0435f), new Vector3(Mathf.Min(0.12f, w * 0.6f), 0.016f, 0.003f), navy, 0.0013f);
        float wheelX = w / 2 - 0.006f;
        foreach (float sx in new[] { -wheelX, wheelX }) foreach (float sz in new[] { -0.022f, 0.075f })
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder); wheel.name = "Wheel";
            UnityEngine.Object.DestroyImmediate(wheel.GetComponent<Collider>());
            wheel.transform.SetParent(root, false); wheel.transform.localPosition = new Vector3(sx, 0.011f, sz);
            wheel.transform.localRotation = Quaternion.Euler(0, 0, 90); wheel.transform.localScale = new Vector3(0.022f, 0.003f, 0.022f);
            wheel.GetComponent<Renderer>().sharedMaterial = rubber;
        }
        var tag = LoadedTag(root, new Vector3(0, 0.11f, 0.07f), bedCells >= 8 ? 1f : bedCells >= 4 ? 0.85f : 0.7f, Quaternion.identity);
        return new VehicleBay.Vehicle { kind = kind, bedCells = bedCells, root = root, loadedTag = tag };
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

    static Mesh RoundedMesh(Vector3 size, float radius)
    {
        string F(float v) => v.ToString("0.####", CultureInfo.InvariantCulture);
        string key = F(size.x) + "x" + F(size.y) + "x" + F(size.z) + "-r" + F(radius);
        if (meshCache.TryGetValue(key, out var cached)) return cached;
        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Application.dataPath), MeshFolder));
        string path = MeshFolder + "/RoundedBox-" + key.Replace(".", "_") + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null) { mesh = RoundedBoxMesh.Build(size, radius, 4); AssetDatabase.CreateAsset(mesh, path); }
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
