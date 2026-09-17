using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Airlift.Lessons;
using Airlift.Lessons.Garden;
using Airlift.Onboarding;
using Airlift.Presentation;
using Airlift.Presentation.Garden;
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

// CC-GD-02 / CC-GD-03: the dedicated Sunny Plot workbench, CargoCrew only. Idempotent: "Garden workbench" under the
// world-locked root is replaced on every run; Garden*.mat materials and the GardenTheme asset are created or updated.
// Builds a grass deck with the Cargo "Workbench" footprint, a raised bed with a pool of 8 × 8 soil cells, walls, row
// numbers and numbered fence spots, two seedling trays, 8 grabbable seedling strips (8 seedlings each, every seedling
// able to grow into lettuce, a carrot, a sunflower or beans), a grabbable fence divider, trees, bushes, a back fence,
// sunflowers, a Sunny Plot sign, the watering can and butterfly for the payoff, and the "Garden card" canvas in the
// Lesson interface frame. Cargo objects are not touched except the shared TableHandle piece list and the
// TypographyBindings inventory. Root inactive by default.
// Run: unity command run_script --file AgentScripts/BuildGardenWorkbench.cs --entry BuildGardenWorkbench.Run
public static class BuildGardenWorkbench
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string MeshFolder = "Assets/Airlift/Meshes";
    const string ThemePath = "Assets/Airlift/Themes/GardenTheme.asset";
    public const string RootName = "Garden workbench";
    public const string CardName = "Garden card";
    public const string CardId = "community_garden_multiplication";
    public const int StripCount = 8;
    public const float T = 0.0175f;                       // deck top (station-local)
    public const float Cell = 0.045f;
    public static readonly Vector3 BedCenter = new Vector3(0f, 0.0415f, -0.03f);
    // Two tidy seedling trays, each holding the three tray strips of its side (chapter 2 has six strips, up to 6 long).
    public const int TraySlots = 3, MaxTrayStripLength = 6;
    public const float TrayWidth = 0.30f, TrayDepth = 0.215f, TrayCenterX = 0.41f, TrayCenterZ = -0.125f, TrayBase = 0.008f, TrayRim = 0.008f;
    public const float TrayHeight = T + TrayBase + 0.006f;   // strip body (12 mm) resting on the tray base
    public const float TrayStepZ = 0.065f, TrayFirstZ = TrayCenterZ + TrayStepZ;
    public const float TrayLeftX = -TrayCenterX - TrayWidth / 2 + 0.015f, TrayRightX = TrayCenterX - TrayWidth / 2 + 0.015f;
    public const float RowLabelSize = 0.27f, SpotNumberSize = 0.21f, PartLabelSize = 0.27f, SignTextSize = 0.26f;
    public static readonly Vector3 FenceHome = new Vector3(0.215f, T, -0.03f);
    public static readonly Vector3 CanHome = new Vector3(0.44f, T, 0.12f);
    public static readonly Vector3 ButterflyHome = new Vector3(0.3f, T + 0.07f, 0.34f);
    const float WallThickness = 0.012f;
    const float RowY = -196f, RowHeight = 48f, RowWidth = 165f, RowStep = 172f;

    static AirliftStyle style;
    static LessonTheme theme;
    static Material grass, grassDark, soil, peat, wood, trayWood, bark, leaf, leafDark, lettuce, bean, sunflower, pink, carrot, can, ink;
    static readonly Dictionary<string, Mesh> meshCache = new Dictionary<string, Mesh>();

    /// Tray slot of strip view i: even views on the left tray, odd views on the right, back to front. Views 6 and 7
    /// exist only for planted chapters (their strips never rest in a tray) and share the last slot.
    public static Vector3 TrayLeft(int i) => new Vector3(i % 2 == 0 ? TrayLeftX : TrayRightX, TrayHeight, TrayFirstZ - Mathf.Min(i / 2, TraySlots - 1) * TrayStepZ);

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>(FindObjectsInactive.Include);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var cargo = d != null ? d.GetComponent<CargoLessonDirector>() : null;
        if (d == null || n == null || cargo == null) throw new InvalidOperationException("CargoCrew onboarding, lesson or Nerdy director missing.");
        if (d.gameObject.name != "Onboarding workbench - world locked") throw new InvalidOperationException("Unexpected world-locked root: " + d.gameObject.name);
        var deckSource = d.transform.Find("Workbench")?.GetComponent<MeshFilter>();
        var frame = d.transform.Find("Lesson interface");
        var template = cargo.submitButton;
        if (deckSource == null || deckSource.sharedMesh == null || frame == null || template == null) throw new InvalidOperationException("Workbench deck, Lesson interface or Cargo button template missing.");
        style = AssetDatabase.LoadAssetAtPath<AirliftStyle>("Assets/Airlift/Fonts/AirliftStyle.asset");
        if (style == null || style.headingFont == null || style.bodyFont == null) throw new InvalidOperationException("Airlift style/fonts missing.");
        meshCache.Clear();
        var report = new List<string>();

        // 1. Materials and theme.
        Materials();
        theme = Theme();
        report.Add("theme " + ThemePath + " + 16 Garden materials");

        // 2. Root and station.
        for (int i = d.transform.childCount - 1; i >= 0; i--)
            if (d.transform.GetChild(i).name == RootName) UnityEngine.Object.DestroyImmediate(d.transform.GetChild(i).gameObject);
        var rootGo = new GameObject(RootName);
        var root = rootGo.transform; root.SetParent(d.transform, false);
        root.localPosition = Vector3.zero; root.localRotation = Quaternion.identity; root.localScale = Vector3.one;
        var station = rootGo.AddComponent<GardenStation>();
        station.cardId = CardId; station.visualRoots = new[] { rootGo }; station.stationRoot = root; station.theme = theme;
        station.trayHeight = TrayHeight; station.deckTop = T; station.fenceHome = FenceHome;

        // 3. Deck: the Cargo footprint, a hair higher so two decks never z-fight if both show.
        var deckSize = deckSource.sharedMesh.bounds.size;
        Rounded("Garden deck", root, new Vector3(0, 0.0005f, 0), deckSize, grass, 0.016f);
        foreach (float x in new[] { -1f, 1f })
            Rounded(x < 0 ? "Deck edging left" : "Deck edging right", root, new Vector3(x * (deckSize.x / 2 - 0.02f), T + 0.002f, 0), new Vector3(0.02f, 0.004f, deckSize.z - 0.04f), grassDark, 0.0019f);
        report.Add("deck " + F(deckSize.x) + " x " + F(deckSize.z) + " m");

        // 4. Props (all under 0.19 m so nothing shows through the translucent card).
        var props = Group("Garden props", root, Vector3.zero);
        Tree(props, "Tree left", new Vector3(-0.56f, T, 0.3f));
        Tree(props, "Tree right", new Vector3(0.56f, T, 0.3f));
        Bush(props, "Bush 1", new Vector3(-0.62f, T, 0.2f));
        Bush(props, "Bush 2", new Vector3(0.62f, T, 0.2f));
        Bush(props, "Bush 3", new Vector3(-0.3f, T, 0.34f));
        Bush(props, "Bush 4", new Vector3(0.3f, T, 0.34f));
        BackFence(props);
        Sunflower(props, "Sunflower 1", new Vector3(-0.6f, T, -0.34f));
        Sunflower(props, "Sunflower 2", new Vector3(0.6f, T, -0.34f));
        Sunflower(props, "Sunflower 3", new Vector3(0.12f, T, 0.34f));
        Sign(props);
        var payoff = Group("Garden payoff", root, Vector3.zero);
        station.wateringCan = WateringCan(payoff); station.wateringCanHome = CanHome;
        station.butterfly = Butterfly(payoff); station.butterflyHome = ButterflyHome;
        report.Add("props: 2 trees, 4 bushes, back fence, 3 sunflowers, Sunny Plot sign, watering can, butterfly");

        // 5. The raised bed.
        station.bed = Bed(root);
        report.Add("bed: 64 soil cells, 4 walls (" + Cell * 1000 + " mm cells), 8 row numbers, 7 fence spots");

        // 6. Trays, strips and fence.
        SeedlingTray(root, "Seedling tray left", -TrayCenterX);
        SeedlingTray(root, "Seedling tray right", TrayCenterX);
        var pieces = Group("Garden pieces", root, Vector3.zero);
        station.pieces = pieces.gameObject;
        station.stripMeshes = new Mesh[GardenBedLayout.MaxColumns + 1];
        for (int len = 1; len <= GardenBedLayout.MaxColumns; len++) station.stripMeshes[len] = StripMesh(len);
        station.strips = new GardenStation.StripView[StripCount];
        for (int i = 0; i < StripCount; i++) station.strips[i] = Strip(pieces, i, station.stripMeshes);
        (station.fence, station.fenceGrabbable) = Fence(pieces);
        station.partLabels = new[] { DeckText(pieces, "Part label left", Vector3.zero, "", PartLabelSize, style.headingFont), DeckText(pieces, "Part label right", Vector3.zero, "", PartLabelSize, style.headingFont) };
        foreach (var l in station.partLabels) { l.rectTransform.sizeDelta = new Vector2(0.16f, 0.045f); l.gameObject.SetActive(false); }
        report.Add(StripCount + " grabbable seedling strips, a grabbable fence divider, 2 part labels");

        // 7. Card canvas in the Lesson interface frame.
        Card(station, n, root, frame, template, d);
        report.Add("card: heading, body, expression, say-hints, 6 buttons in one CanvasGroup");

        // 8. Shared table handle pauses garden pieces too.
        var handle = d.GetComponent<TableHandle>();
        if (handle != null)
        {
            var gardenGrabs = pieces.GetComponentsInChildren<GrabInteractable>(true);
            handle.pieceInteractables = (handle.pieceInteractables ?? new GrabInteractable[0]).Where(g => g != null && !g.transform.IsChildOf(root)).Concat(gardenGrabs).Distinct().ToArray();
            EditorUtility.SetDirty(handle);
            report.Add("table handle gates " + handle.pieceInteractables.Length + " piece interactables (" + gardenGrabs.Length + " garden)");
        }

        // 9. Typography inventory stays explicit for the layout test.
        var binding = d.GetComponent<TypographyBindings>();
        if (binding != null)
        {
            var gardenHeadings = root.GetComponentsInChildren<TMP_Text>(true).Where(t => t.font == style.headingFont).ToList();
            var headings = (binding.headings ?? new TMP_Text[0]).Where(t => t != null).Concat(gardenHeadings).Distinct().ToArray();
            binding.headings = headings;
            binding.bodies = d.GetComponentsInChildren<TMP_Text>(true).Except(headings).ToArray();
            binding.Apply();
            EditorUtility.SetDirty(binding);
        }

        // Closed state: chapter 1 bed laid out, pieces hidden, root inactive.
        station.bed.Apply(GardenChapter.All[0].Rows, GardenChapter.All[0].Columns);
        for (int i = 0; i < StripCount; i++) station.strips[i].piece.localPosition = GardenStation.TrayPosition(station.strips[i], MaxTrayStripLength, Cell);
        pieces.gameObject.SetActive(false);
        rootGo.SetActive(false);
        EditorUtility.SetDirty(station); EditorUtility.SetDirty(station.bed); EditorUtility.SetDirty(n);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        AssetDatabase.SaveAssets();
        return "Garden workbench built: " + string.Join("; ", report) + ". Root inactive. Scene saved.";
    }

    // ---- bed ----
    static GardenBedView Bed(Transform root)
    {
        var go = new GameObject("Garden bed"); go.transform.SetParent(root, false);
        var view = go.AddComponent<GardenBedView>();
        view.stationRoot = root; view.center = BedCenter; view.cell = Cell; view.wallThickness = WallThickness;
        view.cellHeight = T + 0.009f; view.wallHeight = T + 0.016f; view.labelHeight = T + 0.036f; view.rowLabelOffset = 0.03f;
        var turnable = Group("Bed turnable", go.transform, new Vector3(BedCenter.x, 0, BedCenter.z));
        view.turnable = turnable;
        view.cells = new GameObject[GardenBedLayout.MaxRows * GardenBedLayout.MaxColumns];
        for (int r = 0; r < GardenBedLayout.MaxRows; r++)
            for (int c = 0; c < GardenBedLayout.MaxColumns; c++)
                view.cells[r * GardenBedLayout.MaxColumns + c] = Rounded("Soil cell " + (r + 1) + "-" + (c + 1), turnable, Vector3.zero, new Vector3(Cell - 0.005f, 0.018f, Cell - 0.005f), soil, 0.004f);
        view.longWalls = new Mesh[GardenBedLayout.MaxColumns + 1];
        view.shortWalls = new Mesh[GardenBedLayout.MaxRows + 1];
        for (int k = 1; k <= GardenBedLayout.MaxColumns; k++) view.longWalls[k] = RoundedMesh(new Vector3(k * Cell + 2 * WallThickness, 0.032f, WallThickness), 0.004f);
        for (int k = 1; k <= GardenBedLayout.MaxRows; k++) view.shortWalls[k] = RoundedMesh(new Vector3(k * Cell, 0.032f, WallThickness), 0.004f);
        view.wallBack = Wall(turnable, "Wall back", view.longWalls[4]);
        view.wallFront = Wall(turnable, "Wall front", view.longWalls[4]);
        view.wallLeft = Wall(turnable, "Wall left", view.shortWalls[3]);
        view.wallRight = Wall(turnable, "Wall right", view.shortWalls[3]);
        view.rowLabels = new TMP_Text[GardenBedLayout.MaxRows];
        for (int r = 0; r < GardenBedLayout.MaxRows; r++)
        {
            view.rowLabels[r] = DeckText(go.transform, "Row label " + (r + 1), Vector3.zero, (r + 1).ToString(), RowLabelSize, style.headingFont);
            view.rowLabels[r].rectTransform.sizeDelta = new Vector2(0.04f, 0.045f);
        }
        view.fenceTicks = new GameObject[GardenBedLayout.MaxColumns - 1];
        for (int k = 1; k < GardenBedLayout.MaxColumns; k++)
        {
            var spot = Group("Fence spot " + k, go.transform, Vector3.zero);
            Rounded("Peg", spot, new Vector3(0, T + 0.035f, 0), new Vector3(0.004f, 0.006f, 0.016f), ink, 0.0015f);
            var number = DeckText(spot, "Spot number", new Vector3(0, T + 0.034f, -0.032f), k.ToString(), SpotNumberSize, style.headingFont);
            number.rectTransform.sizeDelta = new Vector2(0.04f, 0.04f);
            view.fenceTicks[k - 1] = spot.gameObject;
        }
        return view;
    }

    static MeshFilter Wall(Transform parent, string name, Mesh mesh)
    {
        var g = new GameObject(name); g.transform.SetParent(parent, false);
        var f = g.AddComponent<MeshFilter>(); f.sharedMesh = mesh;
        var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = wood; r.receiveShadows = true;
        return f;
    }

    // ---- pieces ----
    static Mesh StripMesh(int length) => RoundedMesh(new Vector3(length * Cell - 0.004f, 0.012f, 0.036f), 0.005f);

    static GardenStation.StripView Strip(Transform parent, int index, Mesh[] meshes)
    {
        int max = GardenBedLayout.MaxColumns;
        var root = new GameObject("Seedling strip " + (index + 1)); root.transform.SetParent(parent, false);
        var view = new GardenStation.StripView { trayLeft = TrayLeft(index) };
        root.transform.localPosition = GardenStation.TrayPosition(view, max, Cell);
        var collider = root.AddComponent<BoxCollider>(); collider.size = new Vector3(max * Cell, 0.04f, 0.04f); collider.center = new Vector3(0, 0.012f, 0);
        view.piece = root.transform; view.collider = collider;
        view.bodyLeft = Body(root.transform, "Body left", meshes[max]);
        view.bodyRight = Body(root.transform, "Body right", meshes[1]);
        view.bodyRight.gameObject.SetActive(false);
        view.seedlings = new Transform[max]; view.sprouts = new Transform[max]; view.blooms = new Transform[max];
        for (int k = 0; k < max; k++)
        {
            var seedling = Group("Seedling " + (k + 1), root.transform, new Vector3(GardenBedLayout.SeedlingX(Cell, k, max), 0.006f, 0));
            var sprout = Group("Sprout", seedling, Vector3.zero);
            Rounded("Stem", sprout, new Vector3(0, 0.008f, 0), new Vector3(0.004f, 0.016f, 0.004f), leafDark, 0.0015f);
            Rounded("Leaves", sprout, new Vector3(0, 0.017f, 0), new Vector3(0.022f, 0.007f, 0.012f), leaf, 0.003f);
            var bloom = Group("Bloom", seedling, Vector3.zero);
            Rounded("Lettuce head", bloom, new Vector3(0, 0.012f, 0), new Vector3(0.032f, 0.024f, 0.032f), lettuce, 0.01f);
            Rounded("Carrot top", bloom, new Vector3(0, 0.022f, 0), new Vector3(0.012f, 0.03f, 0.012f), leafDark, 0.004f);
            Rounded("Carrot root", bloom, new Vector3(0, 0.004f, 0), new Vector3(0.022f, 0.014f, 0.022f), carrot, 0.006f);
            Rounded("Sunflower stalk", bloom, new Vector3(0, 0.025f, 0), new Vector3(0.005f, 0.05f, 0.005f), leafDark, 0.002f);
            var petals = Rounded("Sunflower petals", bloom, new Vector3(0, 0.055f, -0.004f), new Vector3(0.034f, 0.034f, 0.006f), sunflower, 0.0027f);
            petals.transform.localRotation = Quaternion.Euler(25f, 0, 0);
            var centre = Rounded("Sunflower centre", bloom, new Vector3(0, 0.056f, -0.008f), new Vector3(0.015f, 0.015f, 0.008f), bark, 0.0036f);
            centre.transform.localRotation = Quaternion.Euler(25f, 0, 0);
            Rounded("Bean pole", bloom, new Vector3(0.006f, 0.03f, 0), new Vector3(0.004f, 0.06f, 0.004f), wood, 0.0015f);
            Rounded("Bean pods", bloom, new Vector3(-0.003f, 0.028f, 0), new Vector3(0.016f, 0.03f, 0.012f), bean, 0.005f);
            bloom.localScale = Vector3.zero;
            bloom.gameObject.SetActive(false);
            view.seedlings[k] = seedling; view.sprouts[k] = sprout; view.blooms[k] = bloom;
        }
        QuickActionsAPI.AddGrabInteraction(root);
        view.grabbable = root.GetComponentInChildren<Grabbable>();
        if (view.grabbable == null) throw new InvalidOperationException("SDK did not create Grabbable for strip " + (index + 1));
        view.grabbable.MaxGrabPoints = 1;
        return view;
    }

    static MeshFilter Body(Transform parent, string name, Mesh mesh)
    {
        var g = new GameObject(name); g.transform.SetParent(parent, false);
        var f = g.AddComponent<MeshFilter>(); f.sharedMesh = mesh;
        var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = peat; r.receiveShadows = true;
        return f;
    }

    static (Transform, Grabbable) Fence(Transform parent)
    {
        var root = new GameObject("Fence divider"); root.transform.SetParent(parent, false);
        root.transform.localPosition = FenceHome;
        var collider = root.AddComponent<BoxCollider>(); collider.size = new Vector3(0.03f, 0.08f, 0.4f); collider.center = new Vector3(0, 0.04f, 0);
        for (int i = 0; i < 5; i++)
            Rounded("Post " + (i + 1), root.transform, new Vector3(0, 0.03f, -0.18f + i * 0.09f), new Vector3(0.01f, 0.06f, 0.01f), wood, 0.003f);
        Rounded("Rail high", root.transform, new Vector3(0, 0.05f, 0), new Vector3(0.006f, 0.008f, 0.38f), wood, 0.0025f);
        Rounded("Rail low", root.transform, new Vector3(0, 0.025f, 0), new Vector3(0.006f, 0.008f, 0.38f), wood, 0.0025f);
        Rounded("Grip knob", root.transform, new Vector3(0, 0.068f, -0.19f), new Vector3(0.022f, 0.022f, 0.022f), sunflower, 0.008f);
        QuickActionsAPI.AddGrabInteraction(root);
        var grabbable = root.GetComponentInChildren<Grabbable>();
        if (grabbable == null) throw new InvalidOperationException("SDK did not create Grabbable for the fence");
        grabbable.MaxGrabPoints = 1;
        return (root.transform, grabbable);
    }

    // ---- props ----
    static void Tree(Transform parent, string name, Vector3 p)
    {
        var tree = Group(name, parent, p);
        Rounded("Trunk", tree, new Vector3(0, 0.04f, 0), new Vector3(0.024f, 0.08f, 0.024f), bark, 0.006f);
        Rounded("Canopy low", tree, new Vector3(0, 0.09f, 0), new Vector3(0.12f, 0.07f, 0.12f), leafDark, 0.03f);
        Rounded("Canopy top", tree, new Vector3(0.01f, 0.14f, -0.005f), new Vector3(0.08f, 0.045f, 0.08f), leaf, 0.02f);
    }

    static void Bush(Transform parent, string name, Vector3 p)
    {
        var bush = Group(name, parent, p);
        Rounded("Bush blob", bush, new Vector3(0, 0.025f, 0), new Vector3(0.08f, 0.05f, 0.07f), leafDark, 0.022f);
        Rounded("Bush top", bush, new Vector3(0.015f, 0.045f, -0.005f), new Vector3(0.05f, 0.04f, 0.05f), leaf, 0.018f);
    }

    static void BackFence(Transform parent)
    {
        var fence = Group("Back fence", parent, new Vector3(0, T, 0.37f));
        for (int i = 0; i < 7; i++) Rounded("Fence post " + (i + 1), fence, new Vector3(-0.45f + i * 0.15f, 0.035f, 0), new Vector3(0.014f, 0.07f, 0.014f), wood, 0.004f);
        Rounded("Fence rail high", fence, new Vector3(0, 0.05f, 0.006f), new Vector3(0.92f, 0.01f, 0.008f), wood, 0.003f);
        Rounded("Fence rail low", fence, new Vector3(0, 0.025f, 0.006f), new Vector3(0.92f, 0.01f, 0.008f), wood, 0.003f);
    }

    static void Sunflower(Transform parent, string name, Vector3 p)
    {
        var flower = Group(name, parent, p);
        Rounded("Pot", flower, new Vector3(0, 0.015f, 0), new Vector3(0.04f, 0.03f, 0.04f), trayWood, 0.008f);
        Rounded("Stalk", flower, new Vector3(0, 0.08f, 0), new Vector3(0.006f, 0.1f, 0.006f), leafDark, 0.0025f);
        Rounded("Leaf", flower, new Vector3(-0.014f, 0.075f, 0), new Vector3(0.028f, 0.006f, 0.014f), leaf, 0.0025f);
        var petals = Rounded("Petals", flower, new Vector3(0, 0.135f, -0.004f), new Vector3(0.06f, 0.06f, 0.008f), sunflower, 0.0036f);
        petals.transform.localRotation = Quaternion.Euler(20f, 0, 0);
        var centre = Rounded("Centre", flower, new Vector3(0, 0.135f, -0.009f), new Vector3(0.026f, 0.026f, 0.01f), bark, 0.0045f);
        centre.transform.localRotation = Quaternion.Euler(20f, 0, 0);
    }

    static void Sign(Transform parent)
    {
        var sign = Group("Sunny Plot sign", parent, new Vector3(-0.36f, T, 0.33f));
        foreach (float x in new[] { -0.055f, 0.055f }) Rounded(x < 0 ? "Sign post left" : "Sign post right", sign, new Vector3(x, 0.03f, 0.006f), new Vector3(0.008f, 0.06f, 0.008f), bark, 0.003f);
        Rounded("Sign board", sign, new Vector3(0, 0.1f, 0), new Vector3(0.15f, 0.085f, 0.008f), trayWood, 0.0036f);
        var text = WorldText("Sign text", sign, new Vector3(0, 0.1f, -0.0045f), "SUNNY\nPLOT", SignTextSize, Quaternion.identity, style.headingFont, ink);
        text.rectTransform.sizeDelta = new Vector2(0.14f, 0.08f);
        text.lineSpacing = -20f;
    }

    /// A tidy wooden seedling tray with a green rim, just big enough for three strips of up to 6 seedlings.
    static void SeedlingTray(Transform root, string name, float centerX)
    {
        var tray = Group(name, root, new Vector3(centerX, T, TrayCenterZ));
        Rounded("Tray base", tray, new Vector3(0, TrayBase / 2, 0), new Vector3(TrayWidth, TrayBase, TrayDepth), wood, 0.003f);
        Rounded("Rim back", tray, new Vector3(0, TrayBase + 0.004f, TrayDepth / 2 - TrayRim / 2), new Vector3(TrayWidth, 0.008f, TrayRim), leafDark, 0.003f);
        Rounded("Rim front", tray, new Vector3(0, TrayBase + 0.004f, -TrayDepth / 2 + TrayRim / 2), new Vector3(TrayWidth, 0.008f, TrayRim), leafDark, 0.003f);
        Rounded("Rim left", tray, new Vector3(-TrayWidth / 2 + TrayRim / 2, TrayBase + 0.004f, 0), new Vector3(TrayRim, 0.008f, TrayDepth - 2 * TrayRim), leafDark, 0.003f);
        Rounded("Rim right", tray, new Vector3(TrayWidth / 2 - TrayRim / 2, TrayBase + 0.004f, 0), new Vector3(TrayRim, 0.008f, TrayDepth - 2 * TrayRim), leafDark, 0.003f);
    }

    static Transform WateringCan(Transform parent)
    {
        var c = Group("Watering can", parent, CanHome);
        Rounded("Can body", c, new Vector3(0, 0.025f, 0), new Vector3(0.06f, 0.05f, 0.04f), can, 0.012f);
        var spout = Rounded("Spout", c, new Vector3(-0.045f, 0.035f, 0), new Vector3(0.05f, 0.008f, 0.008f), can, 0.0036f);
        spout.transform.localRotation = Quaternion.Euler(0, 0, -25f);
        Rounded("Rose", c, new Vector3(-0.07f, 0.047f, 0), new Vector3(0.008f, 0.016f, 0.016f), can, 0.0036f);
        Rounded("Handle", c, new Vector3(0.005f, 0.058f, 0), new Vector3(0.04f, 0.008f, 0.01f), can, 0.0036f);
        Rounded("Handle back", c, new Vector3(0.025f, 0.045f, 0), new Vector3(0.008f, 0.03f, 0.01f), can, 0.0036f);
        return c;
    }

    static Transform Butterfly(Transform parent)
    {
        var b = Group("Butterfly", parent, ButterflyHome);
        Rounded("Body", b, Vector3.zero, new Vector3(0.004f, 0.004f, 0.02f), ink, 0.0015f);
        var left = Rounded("Wing left", b, new Vector3(-0.009f, 0.002f, 0), new Vector3(0.016f, 0.002f, 0.014f), pink, 0.0009f);
        left.transform.localRotation = Quaternion.Euler(0, 0, 25f);
        var right = Rounded("Wing right", b, new Vector3(0.009f, 0.002f, 0), new Vector3(0.016f, 0.002f, 0.014f), sunflower, 0.0009f);
        right.transform.localRotation = Quaternion.Euler(0, 0, -25f);
        return b;
    }

    // ---- card ----
    static void Card(GardenStation station, NerdyDirector n, Transform root, Transform frame, Button template, OnboardingDirector d)
    {
        var card = new GameObject(CardName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        card.transform.SetParent(root, false);
        card.transform.localPosition = frame.localPosition; card.transform.localRotation = frame.localRotation; card.transform.localScale = frame.localScale;
        var rect = card.GetComponent<RectTransform>(); rect.sizeDelta = frame.GetComponent<RectTransform>().sizeDelta;
        var canvas = card.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
        var frameCanvas = frame.GetComponent<Canvas>();
        canvas.worldCamera = frameCanvas != null && frameCanvas.worldCamera != null ? frameCanvas.worldCamera : d.head.GetComponent<Camera>();
        var backdropSource = frame.Find("Backdrop")?.GetComponent<Image>();
        var backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image)); backdrop.transform.SetParent(card.transform, false);
        backdrop.GetComponent<RectTransform>().sizeDelta = rect.sizeDelta;
        var bg = backdrop.GetComponent<Image>(); bg.color = theme.cardPanel; bg.raycastTarget = false;
        if (backdropSource != null) { bg.sprite = backdropSource.sprite; bg.type = backdropSource.type; bg.pixelsPerUnitMultiplier = backdropSource.pixelsPerUnitMultiplier; }

        var headingSource = d.heading; var bodySource = d.body;
        station.heading = CardText(card.transform, "Heading", new Vector2(0, 175), new Vector2(850, 65), style.headingFont, headingSource != null ? headingSource.fontSize : 40, theme.cardHeading, TextAlignmentOptions.Center, false);
        station.body = CardText(card.transform, "Instruction", new Vector2(0, 14), new Vector2(830, 250), style.bodyFont, bodySource != null ? bodySource.fontSize : 27, theme.cardBody, bodySource != null ? bodySource.alignment : TextAlignmentOptions.TopLeft, true);
        station.expressionLine = CardText(card.transform, "Expression line", new Vector2(0, -127), new Vector2(830, 30), style.headingFont, 28, theme.accent, TextAlignmentOptions.Center, false);
        station.sayHints = CardText(card.transform, "Say hints", new Vector2(0, -157), new Vector2(830, 24), style.bodyFont, 18, Color.Lerp(theme.cardBody, theme.cardPanel, 0.25f), TextAlignmentOptions.Center, false);

        var row = new GameObject("Garden buttons", typeof(RectTransform)); row.transform.SetParent(card.transform, false);
        row.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        var group = row.AddComponent<CanvasGroup>();
        station.fallbackGroups = new[] { group };
        station.startButton = RowButton(row.transform, "Start", "Start", 0, 20, template, station.PressStart);
        station.turnButton = RowButton(row.transform, "Turn bed", "Turn bed", 0, 20, template, station.PressTurn);
        station.checkButton = RowButton(row.transform, "Check bed", "Check bed", 1, 20, template, station.PressCheck);
        station.clearButton = RowButton(row.transform, "Clear bed", "Clear bed", 2, 20, template, station.PressClear);
        station.nextButton = RowButton(row.transform, "Next chapter", "Next chapter", 3, 19, template, station.PressNext);
        station.backButton = RowButton(row.transform, "Back to lessons", "Back to lessons", 4, 17, template, station.Close);
        UnityEventTools.AddPersistentListener(station.backButton.onClick, n.OnLessonBack);
        foreach (var b in new[] { station.turnButton, station.checkButton, station.clearButton, station.nextButton }) b.gameObject.SetActive(false);
        QuickActionsAPI.AddRayCanvasInteraction(card);
    }

    static TMP_Text CardText(Transform parent, string name, Vector2 pos, Vector2 size, TMP_FontAsset font, float fontSize, Color color, TextAlignmentOptions align, bool wrap)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>(); t.font = font; t.fontSize = fontSize; t.color = color; t.text = "";
        t.alignment = align; t.raycastTarget = false; t.richText = true;
        t.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap; t.overflowMode = TextOverflowModes.Overflow;
        t.rectTransform.anchoredPosition = pos; t.rectTransform.sizeDelta = size;
        return t;
    }

    static Button RowButton(Transform parent, string name, string label, int slot, float labelSize, Button template, UnityAction action)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchoredPosition = new Vector2(-RowStep * 2 + slot * RowStep, RowY); r.sizeDelta = new Vector2(RowWidth, RowHeight);
        var img = go.GetComponent<Image>(); var source = template.GetComponent<Image>();
        if (source != null) { img.sprite = source.sprite; img.type = source.type; img.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier; }
        img.color = theme.buttonFill;
        var b = go.GetComponent<Button>(); b.targetGraphic = img; b.transition = template.transition; b.colors = template.colors;
        UnityEventTools.AddPersistentListener(b.onClick, action);
        var outline = go.AddComponent<Outline>(); var sourceOutline = template.GetComponent<Outline>();
        outline.effectColor = sourceOutline != null ? sourceOutline.effectColor : theme.cardHeading;
        outline.effectDistance = sourceOutline != null ? sourceOutline.effectDistance : new Vector2(3, -3);
        outline.enabled = false;
        go.AddComponent<FocusPointer>();
        go.AddComponent<UiPressLog>();
        var child = new GameObject("Label", typeof(RectTransform)); child.transform.SetParent(go.transform, false);
        var t = child.AddComponent<TextMeshProUGUI>(); t.font = style.bodyFont; t.fontSize = labelSize; t.color = theme.buttonLabel;
        t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false; t.textWrappingMode = TextWrappingModes.NoWrap; t.text = label;
        t.rectTransform.sizeDelta = new Vector2(RowWidth, RowHeight) - new Vector2(16, 10);
        return b;
    }

    // ---- materials and theme ----
    static void Materials()
    {
        var baseMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/CargoCream.mat");
        if (baseMaterial == null) throw new InvalidOperationException("Material missing: CargoCream");
        grass = Mat(baseMaterial, "GardenGrass", new Color32(126, 196, 102, 255), 0.25f);
        grassDark = Mat(baseMaterial, "GardenGrassDark", new Color32(92, 164, 84, 255), 0.25f);
        soil = Mat(baseMaterial, "GardenSoil", new Color32(146, 98, 64, 255), 0.15f);
        peat = Mat(baseMaterial, "GardenPeat", new Color32(104, 70, 50, 255), 0.15f);
        wood = Mat(baseMaterial, "GardenWood", new Color32(204, 156, 104, 255), 0.3f);
        trayWood = Mat(baseMaterial, "GardenTray", new Color32(232, 204, 158, 255), 0.3f);
        bark = Mat(baseMaterial, "GardenBark", new Color32(116, 80, 56, 255), 0.2f);
        leaf = Mat(baseMaterial, "GardenLeaf", new Color32(124, 204, 92, 255), 0.35f);
        leafDark = Mat(baseMaterial, "GardenLeafDark", new Color32(64, 144, 82, 255), 0.35f);
        lettuce = Mat(baseMaterial, "GardenLettuce", new Color32(170, 228, 120, 255), 0.4f);
        bean = Mat(baseMaterial, "GardenBean", new Color32(98, 176, 70, 255), 0.4f);
        sunflower = Mat(baseMaterial, "GardenSunflower", new Color32(255, 204, 62, 255), 0.4f);
        pink = Mat(baseMaterial, "GardenPetalPink", new Color32(246, 164, 198, 255), 0.4f);
        carrot = Mat(baseMaterial, "GardenCarrot", new Color32(244, 140, 52, 255), 0.4f);
        can = Mat(baseMaterial, "GardenCan", new Color32(110, 184, 226, 255), 0.55f);
        ink = Mat(baseMaterial, "GardenInk", new Color32(40, 54, 44, 255), 0.3f);
    }

    static Material Mat(Material baseMaterial, string name, Color color, float smoothness)
    {
        string path = "Assets/Airlift/Materials/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(baseMaterial) { name = name }; AssetDatabase.CreateAsset(m, path); }
        m.color = color;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(m);
        return m;
    }

    static LessonTheme Theme()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Airlift/Themes")) AssetDatabase.CreateFolder("Assets/Airlift", "Themes");
        var t = AssetDatabase.LoadAssetAtPath<LessonTheme>(ThemePath);
        if (t == null) { t = ScriptableObject.CreateInstance<LessonTheme>(); AssetDatabase.CreateAsset(t, ThemePath); }
        t.deck = grass.color; t.trim = wood.color; t.accent = sunflower.color; t.accentSoft = leaf.color;
        t.prop1 = carrot.color; t.prop2 = pink.color; t.prop3 = can.color;
        t.cardPanel = new Color32(24, 52, 36, 247);
        t.cardHeading = new Color32(255, 238, 176, 255);
        t.cardBody = new Color32(238, 246, 230, 255);
        t.buttonFill = new Color32(255, 214, 102, 255);
        t.buttonLabel = new Color32(40, 54, 44, 255);
        t.deckMaterial = grass; t.trimMaterial = wood; t.accentMaterial = sunflower;
        EditorUtility.SetDirty(t);
        return t;
    }

    // ---- kit ----
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
        string key = F(size.x) + "x" + F(size.y) + "x" + F(size.z) + "-r" + F(radius);
        if (meshCache.TryGetValue(key, out var cached)) return cached;
        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Application.dataPath), MeshFolder));
        string path = MeshFolder + "/RoundedBox-" + key.Replace(".", "_") + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null) { mesh = RoundedBoxMesh.Build(size, radius, 4); AssetDatabase.CreateAsset(mesh, path); }
        meshCache[key] = mesh; return mesh;
    }

    static string F(float v) => v.ToString("0.####", CultureInfo.InvariantCulture);

    /// Flat label lying on the deck, tilted toward the seated learner.
    static TMP_Text DeckText(Transform parent, string name, Vector3 p, string text, float size, TMP_FontAsset font)
        => WorldText(name, parent, p, text, size, Quaternion.Euler(70, 0, 0), font, ink);

    static TMP_Text WorldText(string name, Transform parent, Vector3 p, string text, float size, Quaternion rotation, TMP_FontAsset font, Material colorSource)
    {
        var g = Group(name, parent, p); g.localRotation = rotation;
        var label = g.gameObject.AddComponent<TextMeshPro>(); label.font = font; label.text = text; label.fontSize = size;
        label.color = colorSource != null ? colorSource.color : style.panel; label.alignment = TextAlignmentOptions.Center; label.textWrappingMode = TextWrappingModes.NoWrap;
        label.rectTransform.sizeDelta = new Vector2(0.12f, 0.03f);
        return label;
    }
}
