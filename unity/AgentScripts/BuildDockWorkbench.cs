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
// replaced. Adds the quarter crates and locked marks to the piece pool, the container floor skin on the
// ruler, the vehicle bay, the story card lines, CanvasGroup button rows (voice-first fallback), UiPressLog on
// every lesson/HUD button, and removes the double-wired Pause/Music listeners on the Guide HUD.
// Run: unity command run_script --file AgentScripts/BuildDockWorkbench.cs --entry BuildDockWorkbench.Run
public static class BuildDockWorkbench
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string MeshFolder = "Assets/Airlift/Meshes";
    public static readonly Vector3 QuarterTrayOrigin = new Vector3(-0.265f, 0.047f, -0.17f);
    public const float QuarterTraySpacing = 0.09f;
    const float RowY = -196f, RowHeight = 48f, RowWidth = 165f, RowStep = 172f;

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
        lesson.quarters = quarters;
        lesson.whole.id = "whole"; lesson.halfA.id = "half-1"; lesson.halfB.id = "half-2";
        lesson.halfA.lockedMark = LockedMark(lesson.halfA.piece, RulerLayout.PieceLength(4));
        lesson.halfB.lockedMark = LockedMark(lesson.halfB.piece, RulerLayout.PieceLength(4));
        report.Add("4 quarter crates + locked marks on both halves");

        // 2. Container floor skin on the ruler.
        var ruler = lesson.ruler;
        Replace(ruler, "Container floor");
        var floor = Group("Container floor", ruler, Vector3.zero);
        float len = RulerLayout.WholeLength, depth = 0.08f, bar = 0.005f;
        Rounded("Outline back", floor, new Vector3(0, 0.004f, depth / 2), new Vector3(len + 2 * bar, 0.004f, bar), navy);
        Rounded("Outline front", floor, new Vector3(0, 0.004f, -depth / 2), new Vector3(len + 2 * bar, 0.004f, bar), navy);
        Rounded("Outline left", floor, new Vector3(-len / 2 - bar / 2, 0.004f, 0), new Vector3(bar, 0.004f, depth), navy);
        Rounded("Outline right", floor, new Vector3(len / 2 + bar / 2, 0.004f, 0), new Vector3(bar, 0.004f, depth), navy);
        float cell = len / PlacementState.CellsPerWhole;
        for (int i = 0; i < PlacementState.CellsPerWhole; i++)
            Rounded("Slot " + (i + 1), floor, new Vector3(-len / 2 + (i + 0.5f) * cell, 0.0035f, 0), new Vector3(cell - 0.004f, 0.0012f, depth - 0.012f), slot);
        var containerLabel = WorldText("ONE CONTAINER label", floor, new Vector3(0, 0.006f, depth / 2 + 0.022f), "ONE CONTAINER", 0.07f, Quaternion.Euler(60, 0, 0), style.headingFont, style.panel);
        containerLabel.rectTransform.sizeDelta = new Vector2(0.22f, 0.03f);
        var halfMarks = new List<GameObject>();
        foreach (Transform child in ruler)
        {
            if (child.name == "Mark 0" || child.name == "Mark 1" || child.name == "Tick 0" || child.name == "Tick 1") child.gameObject.SetActive(true);
            if (child.name == "Mark 1/2" || child.name == "Tick 1/2") { halfMarks.Add(child.gameObject); child.gameObject.SetActive(false); }
        }
        lesson.halfMarks = halfMarks.ToArray();
        report.Add("container floor: outline, 8 slots, label; half marks " + halfMarks.Count);

        // 3. Vehicle bay: existing truck plus 2 pickups (right bay, cabs to the ship) and 4 vans (front right).
        Replace(terminal.transform, "Vehicle bay");
        var bayGo = Group("Vehicle bay", terminal.transform, Vector3.zero).gameObject;
        var bay = bayGo.AddComponent<VehicleBay>();
        bay.rollDistance = 0.08f; bay.rollSeconds = 1f; bay.idleKind = "truck";
        var list = new List<VehicleBay.Vehicle>();
        Replace(terminal.truck, "Loaded tag");
        list.Add(new VehicleBay.Vehicle { kind = "truck", root = terminal.truck, loadedTag = LoadedTag(terminal.truck, new Vector3(0, 0.165f, 0.02f), 1f, Quaternion.identity) });
        foreach (var (x, i) in new[] { (0.235f, 1), (0.33f, 2) })
            list.Add(new VehicleBay.Vehicle { kind = "pickup", root = Pickup(bayGo.transform, i, new Vector3(x, 0.041f, -0.06f)), distance = 0.08f });
        for (int i = 1; i <= 4; i++)
            list.Add(new VehicleBay.Vehicle { kind = "van", root = Van(bayGo.transform, i, new Vector3(0.13f + (i - 1) * 0.075f, 0.036f, -0.28f)), distance = 0.05f });
        foreach (var v in list.Skip(1)) { v.loadedTag = v.root.Find("Loaded tag").gameObject; v.root.gameObject.SetActive(false); }
        foreach (var v in list) v.loadedTag.SetActive(false);
        bay.vehicles = list.ToArray();
        lesson.vehicles = bay;
        report.Add("vehicle bay: 1 truck, 2 pickups, 4 vans");

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

        EditorUtility.SetDirty(lesson); EditorUtility.SetDirty(n); EditorUtility.SetDirty(bay);
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

    // ---- vehicles (cab faces local -z; the bay rolls each vehicle along its facing) ----
    static Transform Pickup(Transform parent, int index, Vector3 p)
    {
        var root = Group("Pickup " + index, parent, p); root.localRotation = Quaternion.Euler(0, 180, 0);
        Rounded("Chassis", root, Vector3.zero, new Vector3(0.075f, 0.018f, 0.16f), rubber);
        Rounded("Cab", root, new Vector3(0, 0.034f, -0.045f), new Vector3(0.07f, 0.05f, 0.055f), yellow);
        Rounded("Windscreen", root, new Vector3(0, 0.042f, -0.0735f), new Vector3(0.055f, 0.02f, 0.003f), navy);
        Rounded("Bed", root, new Vector3(0, 0.019f, 0.035f), new Vector3(0.07f, 0.02f, 0.085f), cream);
        Wheels(root, 0.04f, 0.05f, -0.008f, 0.03f, 0.008f);
        LoadedTag(root, new Vector3(0, 0.1f, 0), 0.85f, Quaternion.Euler(0, 180, 0));
        return root;
    }

    static Transform Van(Transform parent, int index, Vector3 p)
    {
        var root = Group("Van " + index, parent, p);
        Rounded("Chassis", root, Vector3.zero, new Vector3(0.058f, 0.014f, 0.12f), rubber);
        Rounded("Body", root, new Vector3(0, 0.037f, 0), new Vector3(0.056f, 0.06f, 0.115f), cream);
        Rounded("Windscreen", root, new Vector3(0, 0.05f, -0.0585f), new Vector3(0.046f, 0.02f, 0.003f), navy);
        Rounded("Stripe", root, new Vector3(0, 0.03f, 0.008f), new Vector3(0.0575f, 0.01f, 0.09f), teal);
        Wheels(root, 0.031f, 0.04f, -0.006f, 0.024f, 0.006f);
        LoadedTag(root, new Vector3(0, 0.09f, 0), 0.7f, Quaternion.identity);
        return root;
    }

    static void Wheels(Transform root, float x, float z, float y, float diameter, float width)
    {
        foreach (float sx in new[] { -x, x }) foreach (float sz in new[] { -z, z })
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder); wheel.name = "Wheel";
            UnityEngine.Object.DestroyImmediate(wheel.GetComponent<Collider>());
            wheel.transform.SetParent(root, false); wheel.transform.localPosition = new Vector3(sx, y, sz);
            wheel.transform.localRotation = Quaternion.Euler(0, 0, 90); wheel.transform.localScale = new Vector3(diameter, width, diameter);
            wheel.GetComponent<Renderer>().sharedMaterial = rubber;
        }
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
