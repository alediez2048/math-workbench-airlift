using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Airlift.Lessons;
using Airlift.Lessons.Cafe;
using Airlift.Onboarding;
using Airlift.Presentation;
using Airlift.Presentation.Cafe;
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

// CC-CF-02 / CC-CF-03: the dedicated Corner Café workbench, CargoCrew only. Idempotent: "Cafe workbench" under the
// world-locked root is replaced on every run; Cafe*.mat materials and the CafeTheme asset are created or updated.
// Builds a cream deck with the Cargo "Workbench" footprint, the coffee counter, espresso machine, awning, menu board,
// guest table and delivery bike, 4 plates and 6 boxes as drop targets, a pool of 15 grabbable pastries and the
// "Cafe card" canvas in the Lesson interface frame. Owner 2026-09-17: pastries are 2x the first build
// (CafeLayout.PieceScale); the tray and one row of plates or boxes fill the front half, and the counter, guest table
// and bike stand in the back half (CafeLayout footprints). Cargo objects are not touched except the shared TableHandle
// piece list and the TypographyBindings inventory. Root inactive by default.
// Run: unity command run_script --file AgentScripts/BuildCafeWorkbench.cs --entry BuildCafeWorkbench.Run
public static class BuildCafeWorkbench
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string MeshFolder = "Assets/Airlift/Meshes";
    const string ThemePath = "Assets/Airlift/Themes/CafeTheme.asset";
    public const string RootName = "Cafe workbench";
    public const string CardName = "Cafe card";
    const float RowY = -196f, RowHeight = 48f, RowWidth = 165f, RowStep = 172f;
    const float T = CafeLayout.DeckTop;

    static AirliftStyle style;
    static LessonTheme theme;
    static Material cream, latte, coffee, espresso, pink, mint, butter, plateWhite, croissantGold, cookieTan, chocolate, muffinBrown, steamWhite, rubber;
    static readonly Dictionary<string, Mesh> meshCache = new Dictionary<string, Mesh>();

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
        report.Add("theme " + ThemePath + " + 14 Cafe materials");

        // 2. Root, station and payoff.
        for (int i = d.transform.childCount - 1; i >= 0; i--)
            if (d.transform.GetChild(i).name == RootName) UnityEngine.Object.DestroyImmediate(d.transform.GetChild(i).gameObject);
        var rootGo = new GameObject(RootName);
        var root = rootGo.transform; root.SetParent(d.transform, false);
        root.localPosition = Vector3.zero; root.localRotation = Quaternion.identity; root.localScale = Vector3.one;
        var station = rootGo.AddComponent<CafeStation>();
        station.cardId = CafeStation.CardId; station.visualRoots = new[] { rootGo }; station.space = root; station.theme = theme;
        var payoff = rootGo.AddComponent<CafePayoff>();
        payoff.space = root; station.payoff = payoff;

        // 3. Deck: the Cargo footprint, a hair higher so the two decks never z-fight if both show.
        var deckSize = deckSource.sharedMesh.bounds.size;
        Rounded("Cafe deck", root, new Vector3(0, 0.0005f, 0), deckSize, cream, 0.016f);
        var row = CafeLayout.ContainerCenter(CafeTargetKind.Boxes, 0, 1);
        Rounded("Counter mat", root, new Vector3(0, T + 0.0012f, row.z), new Vector3(1.16f, 0.002f, CafeLayout.BoxDepth + 0.01f), latte, 0.001f);
        foreach (float x in new[] { -1f, 1f })
            Rounded(x < 0 ? "Deck trim left" : "Deck trim right", root, new Vector3(x * (deckSize.x / 2 - 0.02f), T + 0.002f, 0), new Vector3(0.02f, 0.004f, deckSize.z - 0.04f), latte, 0.0019f);
        report.Add("deck " + F(deckSize.x) + " x " + F(deckSize.z) + " m");

        // 4. Props.
        var props = Group("Cafe props", root, Vector3.zero);
        CoffeeCounter(props);
        MenuBoard(props);
        var steam = GuestTable(props);
        var bike = DeliveryBike(props);
        payoff.steamPuffs = steam; payoff.bike = bike; payoff.bikeParked = CafeLayout.BikeParked; payoff.serveScale = CafeLayout.ServeScale;
        payoff.rackCenter = CafeLayout.RackCenter; payoff.rackScale = CafeLayout.RackScale;
        payoff.rackColumnOffset = CafeLayout.RackColumnOffset; payoff.rackLayerHeight = CafeLayout.RackLayerHeight;
        report.Add("props: coffee counter, espresso machine, awning, menu board, guest table (" + steam.Length + " steaming cups), delivery bike");

        // 5. Drop targets and labels.
        var targets = Group("Cafe drop targets", root, Vector3.zero);
        var labels = Group("Cafe labels", root, Vector3.zero);
        station.plates = new Transform[CafeLayout.MaxPlates]; station.plateLabels = new TMP_Text[CafeLayout.MaxPlates];
        for (int i = 0; i < CafeLayout.MaxPlates; i++)
        {
            var c = CafeLayout.ContainerCenter(CafeTargetKind.Plates, i, CafeLayout.MaxPlates);
            station.plates[i] = Plate(targets, "Plate " + (i + 1), c);
            station.plateLabels[i] = DeckLabel(labels, "Plate label " + (i + 1), CafeLayout.LabelPosition(CafeTargetKind.Plates, c), "Plate " + (i + 1));
        }
        station.boxes = new Transform[CafeLayout.MaxBoxes]; station.boxLabels = new TMP_Text[CafeLayout.MaxBoxes];
        station.boxCapacityLabels = new TMP_Text[CafeLayout.MaxBoxes]; station.orderUpTags = new GameObject[CafeLayout.MaxBoxes];
        var boxMaterials = new[] { pink, mint, butter };
        for (int i = 0; i < CafeLayout.MaxBoxes; i++)
        {
            var c = CafeLayout.ContainerCenter(CafeTargetKind.Boxes, i, CafeLayout.MaxBoxes);
            station.boxes[i] = Box(targets, "Box " + (i + 1), c, boxMaterials[i % 3], out station.boxCapacityLabels[i], out station.orderUpTags[i]);
            station.boxLabels[i] = DeckLabel(labels, "Box label " + (i + 1), CafeLayout.LabelPosition(CafeTargetKind.Boxes, c), "Box " + (i + 1));
            station.boxes[i].gameObject.SetActive(false); station.boxLabels[i].gameObject.SetActive(false);
        }
        report.Add(CafeLayout.MaxPlates + " plates, " + CafeLayout.MaxBoxes + " boxes with capacity numbers and ORDER UP tags");

        // 6. Pastry tray and pool.
        PastryTray(root);
        var pieces = Group("Cafe pieces", root, Vector3.zero);
        station.piecesRoot = pieces.gameObject;
        station.items = new CafeStation.ItemView[CafeLayout.MaxItems];
        for (int i = 0; i < CafeLayout.MaxItems; i++) station.items[i] = Pastry(pieces, i);
        report.Add(CafeLayout.MaxItems + " grabbable pastries (croissant, cookie, muffin shapes)");

        // 7. Card canvas in the Lesson interface frame.
        Card(station, n, root, frame, template, d);
        report.Add("card: heading, body, expression, say-hints, 6 buttons in one CanvasGroup");

        // 8. Shared table handle pauses café pastries too.
        var handle = d.GetComponent<TableHandle>();
        if (handle != null)
        {
            var cafeGrabs = pieces.GetComponentsInChildren<GrabInteractable>(true);
            handle.pieceInteractables = (handle.pieceInteractables ?? new GrabInteractable[0]).Where(g => g != null && !g.transform.IsChildOf(root)).Concat(cafeGrabs).Distinct().ToArray();
            EditorUtility.SetDirty(handle);
            report.Add("table handle gates " + handle.pieceInteractables.Length + " piece interactables (" + cafeGrabs.Length + " café)");
        }

        // 9. Typography inventory stays explicit for the layout test.
        var binding = d.GetComponent<TypographyBindings>();
        if (binding != null)
        {
            var cafeHeadings = root.GetComponentsInChildren<TMP_Text>(true).Where(t => t.font == style.headingFont).ToList();
            var headings = (binding.headings ?? new TMP_Text[0]).Where(t => t != null).Concat(cafeHeadings).Distinct().ToArray();
            binding.headings = headings;
            binding.bodies = d.GetComponentsInChildren<TMP_Text>(true).Except(headings).ToArray();
            binding.Apply();
            EditorUtility.SetDirty(binding);
        }

        // Closed state.
        pieces.gameObject.SetActive(false);
        rootGo.SetActive(false);
        EditorUtility.SetDirty(station); EditorUtility.SetDirty(payoff); EditorUtility.SetDirty(n);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        AssetDatabase.SaveAssets();
        return "Cafe workbench built: " + string.Join("; ", report) + ". Root inactive. Scene saved.";
    }

    // ---- props ----
    /// Back-left corner: counter with the espresso machine, three cups in front of it and an awning on a low wall at
    /// the back edge. Everything stays inside CafeLayout.CounterRect and under the card sightline.
    static void CoffeeCounter(Transform parent)
    {
        var counter = Group("Coffee counter", parent, CafeLayout.CounterPosition);
        float w = CafeLayout.CounterSize.x, dz = CafeLayout.CounterSize.y;
        Rounded("Counter body", counter, new Vector3(0, 0.035f, 0), new Vector3(w - 0.02f, 0.07f, dz - 0.02f), coffee, 0.008f);
        Rounded("Counter top", counter, new Vector3(0, 0.074f, 0), new Vector3(w, 0.008f, dz), latte, 0.0039f);
        var stripes = new[] { pink, mint, butter };
        for (int i = 0; i < 3; i++)
            Rounded("Counter panel " + (i + 1), counter, new Vector3((w - 0.02f) / 2 + 0.0015f, 0.035f, -0.075f + i * 0.075f), new Vector3(0.004f, 0.04f, 0.06f), stripes[i], 0.0019f);
        float top = 0.078f;
        var machine = Group("Espresso machine", counter, new Vector3(-0.03f, top, 0.0f));
        Rounded("Machine body", machine, new Vector3(0, 0.035f, 0), new Vector3(0.12f, 0.07f, 0.08f), pink, 0.012f);
        Rounded("Machine top", machine, new Vector3(0, 0.075f, 0), new Vector3(0.124f, 0.01f, 0.084f), espresso, 0.0045f);
        Rounded("Pressure dial", machine, new Vector3(0.028f, 0.05f, -0.042f), new Vector3(0.022f, 0.022f, 0.004f), mint, 0.0019f);
        Rounded("Group head", machine, new Vector3(-0.02f, 0.042f, -0.05f), new Vector3(0.03f, 0.014f, 0.02f), espresso, 0.0063f);
        Rounded("Portafilter handle", machine, new Vector3(-0.02f, 0.036f, -0.07f), new Vector3(0.008f, 0.008f, 0.03f), espresso, 0.0036f);
        Rounded("Drip tray", machine, new Vector3(0, 0.003f, -0.055f), new Vector3(0.1f, 0.006f, 0.03f), latte, 0.0027f);
        Cup(machine, "Espresso cup", new Vector3(-0.02f, 0.006f, -0.055f), butter, 0.02f, 0.02f, false);
        for (int i = 0; i < 3; i++)
            Cup(counter, "Counter cup " + (i + 1), new Vector3(0.07f, top, -0.1f + i * 0.05f), stripes[i], 0.022f, 0.024f, true);
        // Awning on a low back wall behind the machine: canopy stripes stay under the card sightline.
        var awning = Group("Awning", counter, new Vector3(0, 0, dz / 2 - 0.0085f));
        Rounded("Cafe wall", awning, new Vector3(0, 0.075f, 0), new Vector3(w, 0.15f, 0.015f), latte, 0.006f);
        // Four stripes, 0.2 m wide in total, so the awning stays on the counter footprint.
        for (int i = 0; i < 4; i++)
        {
            var stripe = Rounded("Awning stripe " + (i + 1), awning, new Vector3(-0.075f + i * 0.05f, 0.155f, -0.035f), new Vector3(0.05f, 0.008f, 0.07f), i % 2 == 0 ? pink : cream, 0.0036f);
            stripe.transform.localRotation = Quaternion.Euler(-15f, 0, 0);
        }
        for (int i = 0; i < 4; i++)
            Rounded("Awning scallop " + (i + 1), awning, new Vector3(-0.075f + i * 0.05f, 0.139f, -0.07f), new Vector3(0.046f, 0.012f, 0.006f), i % 2 == 0 ? pink : cream, 0.0027f);
    }

    /// Small chalkboard on two legs behind the middle of the guest table, between the chairs, facing the learner.
    /// Low enough to read under the lesson card; nowhere near the bike's ride-off lane on the right.
    static void MenuBoard(Transform parent)
    {
        var board = Group("Menu board", parent, CafeLayout.MenuBoardPosition);
        var face = Group("Board face", board, new Vector3(0, 0.1f, 0)); face.localRotation = Quaternion.Euler(8f, 0, 0);
        Rounded("Frame", face, new Vector3(0, 0, 0.003f), new Vector3(0.15f, 0.09f, 0.006f), latte, 0.0027f);
        Rounded("Chalkboard", face, new Vector3(0, 0, -0.001f), new Vector3(0.136f, 0.076f, 0.003f), espresso, 0.0013f);
        Text3D("Menu title", face, new Vector3(0, 0.024f, -0.003f), "MENU", 0.12f, style.headingFont, theme.accent, new Vector2(0.12f, 0.02f));
        string[] lines = { "croissant", "cookie", "muffin" };
        var colors = new[] { theme.prop3, theme.accentSoft, theme.accent };
        for (int i = 0; i < lines.Length; i++)
            Text3D("Menu line " + (i + 1), face, new Vector3(0, 0.004f - i * 0.017f, -0.003f), lines[i], 0.085f, style.bodyFont, colors[i], new Vector2(0.12f, 0.016f));
        foreach (float x in new[] { -0.06f, 0.06f })
            Rounded("Board leg", board, new Vector3(x, 0.03f, 0.003f), new Vector3(0.006f, 0.06f, 0.006f), coffee, 0.0027f);
    }

    /// Serving tray for two rows of pastries: cream base, pink rim, butter handles.
    static void PastryTray(Transform root)
    {
        float w = CafeLayout.TrayWidth, dz = CafeLayout.TrayDepth;
        var tray = Group("Pastry tray", root, new Vector3(0, T, CafeLayout.TrayCenterZ));
        Rounded("Tray base", tray, new Vector3(0, 0.004f, 0), new Vector3(w, 0.008f, dz), plateWhite, 0.0039f);
        foreach (float side in new[] { -1f, 1f })
        {
            Rounded(side < 0 ? "Rim front" : "Rim back", tray, new Vector3(0, 0.006f, side * (dz / 2 - 0.004f)), new Vector3(w, 0.012f, 0.008f), pink, 0.0039f);
            Rounded(side < 0 ? "Rim left" : "Rim right", tray, new Vector3(side * (w / 2 - 0.004f), 0.006f, 0), new Vector3(0.008f, 0.012f, dz), pink, 0.0039f);
            Rounded(side < 0 ? "Handle left" : "Handle right", tray, new Vector3(side * (w / 2 + 0.008f), 0.008f, 0), new Vector3(0.02f, 0.01f, 0.05f), butter, 0.0045f);
        }
    }

    /// Guest table in the back middle, sized for four served plates (CafeLayout.ServedCenter), a steaming cup behind
    /// each plate place and a ladder-back chair behind the table.
    static GameObject[] GuestTable(Transform parent)
    {
        float w = CafeLayout.GuestTableWidth, dz = CafeLayout.GuestTableDepth;
        var table = Group("Guest table", parent, new Vector3(0, T, CafeLayout.GuestTableZ));
        float topY = CafeLayout.GuestTableTop - T;
        Rounded("Table top", table, new Vector3(0, topY - 0.005f, 0), new Vector3(w, 0.01f, dz), latte, 0.0045f);
        foreach (float x in new[] { -(w / 2 - 0.02f), w / 2 - 0.02f }) foreach (float z in new[] { -(dz / 2 - 0.02f), dz / 2 - 0.02f })
            Rounded("Table leg", table, new Vector3(x, (topY - 0.01f) / 2, z), new Vector3(0.014f, topY - 0.01f, 0.014f), coffee, 0.006f);
        var seats = new[] { pink, mint, butter, pink };
        var puffs = new List<GameObject>();
        for (int i = 0; i < CafeLayout.ChairX.Length; i++)
        {
            // Ladder-back chair facing the table: legs, seat, two back posts, a slat and a rounded top rail.
            var chair = Group("Guest chair " + (i + 1), parent, new Vector3(CafeLayout.ChairX[i], T, CafeLayout.ChairZ));
            foreach (float lx in new[] { -0.017f, 0.017f }) foreach (float lz in new[] { -0.013f, 0.013f })
                Rounded("Chair leg", chair, new Vector3(lx, 0.015f, lz), new Vector3(0.005f, 0.03f, 0.005f), coffee, 0.0022f);
            Rounded("Seat", chair, new Vector3(0, 0.034f, 0), new Vector3(0.042f, 0.008f, 0.034f), seats[i], 0.0036f);
            foreach (float px in new[] { -0.018f, 0.018f })
                Rounded("Back post", chair, new Vector3(px, 0.063f, 0.014f), new Vector3(0.005f, 0.05f, 0.005f), coffee, 0.0022f);
            Rounded("Back slat", chair, new Vector3(0, 0.066f, 0.014f), new Vector3(0.036f, 0.006f, 0.004f), seats[i], 0.0018f);
            Rounded("Top rail", chair, new Vector3(0, 0.086f, 0.014f), new Vector3(0.046f, 0.01f, 0.007f), seats[i], 0.0031f);
        }
        for (int i = 0; i < CafeLayout.MaxPlates; i++)
        {
            float x = CafeLayout.ServedCenter(i, CafeLayout.MaxPlates).x;
            var cup = Cup(table, "Guest cup " + (i + 1), new Vector3(x, topY, CafeLayout.GuestCupZ), cream, 0.02f, 0.022f, true);
            var puff = Group("Steam puff " + (i + 1), cup, new Vector3(0, 0.036f, 0));
            Rounded("Puff", puff, Vector3.zero, new Vector3(0.014f, 0.014f, 0.014f), steamWhite, 0.0069f);
            Rounded("Puff small", puff, new Vector3(0.006f, 0.012f, 0), new Vector3(0.009f, 0.009f, 0.009f), steamWhite, 0.0044f);
            puff.gameObject.SetActive(false);
            puffs.Add(puff.gameObject);
        }
        return puffs.ToArray();
    }

    /// Toy delivery bike facing +z with a rear rack; accepted boxes load onto the rack and ride off +z.
    static Transform DeliveryBike(Transform parent)
    {
        var bike = Group("Delivery bike", parent, CafeLayout.BikeParked);
        foreach (float z in new[] { -0.075f, 0.075f })
        {
            var wheel = Cylinder(z < 0 ? "Rear wheel" : "Front wheel", bike, new Vector3(0, 0.025f, z), new Vector3(0.05f, 0.004f, 0.05f), rubber);
            wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            var hub = Cylinder(z < 0 ? "Rear hub" : "Front hub", bike, new Vector3(0, 0.025f, z), new Vector3(0.02f, 0.0045f, 0.02f), butter);
            hub.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        Rounded("Down tube", bike, new Vector3(0, 0.045f, 0), new Vector3(0.008f, 0.008f, 0.15f), mint, 0.0036f);
        Rounded("Seat post", bike, new Vector3(0, 0.06f, -0.04f), new Vector3(0.008f, 0.04f, 0.008f), mint, 0.0036f);
        Rounded("Saddle", bike, new Vector3(0, 0.082f, -0.035f), new Vector3(0.022f, 0.008f, 0.035f), pink, 0.0036f);
        Rounded("Head tube", bike, new Vector3(0, 0.062f, 0.065f), new Vector3(0.008f, 0.05f, 0.008f), mint, 0.0036f);
        Rounded("Handlebar", bike, new Vector3(0, 0.088f, 0.065f), new Vector3(0.06f, 0.007f, 0.007f), espresso, 0.0031f);
        foreach (float x in new[] { -0.03f, 0.03f })
            Rounded("Grip", bike, new Vector3(x, 0.088f, 0.065f), new Vector3(0.012f, 0.009f, 0.009f), pink, 0.004f);
        // Small cargo crate over the rear wheel: butter floor, mint rim; accepted boxes ride in it at RackScale.
        var rack = Group("Cargo crate", bike, new Vector3(CafeLayout.RackCenter.x, CafeLayout.RackCenter.y - 0.004f, CafeLayout.RackCenter.z));
        float rw = CafeLayout.RackWidth, rd = CafeLayout.RackDepth;
        Rounded("Crate floor", rack, new Vector3(0, 0.002f, 0), new Vector3(rw, 0.004f, rd), butter, 0.0018f);
        foreach (float side in new[] { -1f, 1f })
        {
            Rounded(side < 0 ? "Crate rim front" : "Crate rim back", rack, new Vector3(0, 0.008f, side * (rd / 2 - 0.002f)), new Vector3(rw, 0.012f, 0.004f), mint, 0.0018f);
            Rounded(side < 0 ? "Crate rim left" : "Crate rim right", rack, new Vector3(side * (rw / 2 - 0.002f), 0.008f, 0), new Vector3(0.004f, 0.012f, rd), mint, 0.0018f);
            Rounded("Crate strut", bike, new Vector3(side * 0.03f, 0.04f, CafeLayout.RackCenter.z + 0.005f), new Vector3(0.005f, 0.035f, 0.005f), mint, 0.0022f);
        }
        return bike;
    }

    static Transform Cup(Transform parent, string name, Vector3 p, Material m, float diameter, float height, bool saucer)
    {
        var cup = Group(name, parent, p);
        if (saucer) Cylinder("Saucer", cup, new Vector3(0, 0.002f, 0), new Vector3(diameter * 1.5f, 0.002f, diameter * 1.5f), cream);
        float lift = saucer ? 0.004f : 0f;
        Cylinder("Cup", cup, new Vector3(0, lift + height / 2, 0), new Vector3(diameter, height / 2, diameter), m);
        Cylinder("Coffee", cup, new Vector3(0, lift + height + 0.0002f, 0), new Vector3(diameter * 0.8f, 0.0004f, diameter * 0.8f), espresso);
        return cup;
    }

    // ---- drop targets ----
    static Transform Plate(Transform parent, string name, Vector3 center)
    {
        var plate = Group(name, parent, center);
        Cylinder("Plate", plate, new Vector3(0, CafeLayout.PlateHeight / 2, 0), new Vector3(CafeLayout.PlateDiameter, CafeLayout.PlateHeight / 2, CafeLayout.PlateDiameter), plateWhite);
        Cylinder("Plate well", plate, new Vector3(0, CafeLayout.PlateHeight + 0.0003f, 0), new Vector3(CafeLayout.PlateDiameter * 0.78f, 0.0004f, CafeLayout.PlateDiameter * 0.78f), cream);
        return plate;
    }

    static Transform Box(Transform parent, string name, Vector3 center, Material m, out TMP_Text capacity, out GameObject tag)
    {
        float w = CafeLayout.BoxWidth, dz = CafeLayout.BoxDepth, h = CafeLayout.BoxHeight, wall = CafeLayout.BoxWall;
        var box = Group(name, parent, center);
        Rounded("Box floor", box, new Vector3(0, wall / 2, 0), new Vector3(w, wall, dz), m, 0.0036f);
        Rounded("Front wall", box, new Vector3(0, h / 2, -dz / 2 + wall / 2), new Vector3(w, h, wall), m, 0.0036f);
        Rounded("Back wall", box, new Vector3(0, h / 2, dz / 2 - wall / 2), new Vector3(w, h, wall), m, 0.0036f);
        Rounded("Left wall", box, new Vector3(-w / 2 + wall / 2, h / 2, 0), new Vector3(wall, h, dz), m, 0.0036f);
        Rounded("Right wall", box, new Vector3(w / 2 - wall / 2, h / 2, 0), new Vector3(wall, h, dz), m, 0.0036f);
        // Capacity tag on the back rim, facing the learner: above the pastries, never behind the front "Box N" label.
        var tagCenter = CafeLayout.CapacityTagCenter; var tagSize = CafeLayout.CapacityTagSize;
        Rounded("Capacity tag", box, tagCenter, new Vector3(tagSize.x, tagSize.y, 0.004f), cream, 0.0018f);
        capacity = Text3D("Capacity number", box, tagCenter + new Vector3(0, 0, -0.0025f), "4", 0.34f, style.headingFont, theme.cardPanel, tagSize);
        var tagGroup = Group("Order up tag", box, new Vector3(0, CafeLayout.OrderUpTagY, 0));
        Rounded("Tag plate", tagGroup, new Vector3(0, 0, 0.003f), new Vector3(0.14f, 0.03f, 0.004f), butter, 0.0018f);
        Rounded("Tag string", tagGroup, new Vector3(0, -0.03f, 0.003f), new Vector3(0.003f, 0.03f, 0.003f), espresso, 0.0013f);
        Text3D("Order up text", tagGroup, Vector3.zero, "ORDER UP", 0.13f, style.headingFont, theme.cardPanel, new Vector2(0.14f, 0.03f));
        tagGroup.gameObject.SetActive(false);
        tag = tagGroup.gameObject;
        return box;
    }

    static TMP_Text DeckLabel(Transform parent, string name, Vector3 p, string text)
    {
        var label = Text3D(name, parent, p, text, CafeLayout.LabelFontSize, style.headingFont, theme.cardPanel, new Vector2(CafeLayout.LabelWidth, CafeLayout.LabelHeight));
        label.transform.localRotation = Quaternion.Euler(CafeLayout.LabelTilt, 0, 0);
        return label;
    }

    // ---- pastries ----
    /// Every size, offset and corner radius is the first build's times CafeLayout.PieceScale.
    static CafeStation.ItemView Pastry(Transform parent, int index)
    {
        float k = CafeLayout.PieceScale;
        Vector3 V(float x, float y, float z) => new Vector3(x, y, z) * k;
        var tray = CafeLayout.TrayPosition(index);
        var root = new GameObject("Pastry " + (index + 1)); root.transform.SetParent(parent, false); root.transform.localPosition = tray;
        var collider = root.AddComponent<BoxCollider>(); collider.size = CafeLayout.PieceColliderSize; collider.center = CafeLayout.PieceColliderCenter;
        float b = -CafeLayout.ItemHalfHeight / k;   // bottom of every shape, first-build units

        var croissant = Group("Croissant", root.transform, Vector3.zero);
        Rounded("Croissant body", croissant, V(0, b + 0.01f, 0), V(0.026f, 0.02f, 0.024f), croissantGold, 0.009f * k);
        Rounded("Croissant ridge", croissant, V(0, b + 0.01f, 0), V(0.004f, 0.021f, 0.025f), cookieTan, 0.0018f * k);
        var left = Rounded("Croissant horn left", croissant, V(-0.014f, b + 0.0065f, -0.004f), V(0.014f, 0.013f, 0.015f), croissantGold, 0.0055f * k);
        left.transform.localRotation = Quaternion.Euler(0, 25f, 0);
        var right = Rounded("Croissant horn right", croissant, V(0.014f, b + 0.0065f, -0.004f), V(0.014f, 0.013f, 0.015f), croissantGold, 0.0055f * k);
        right.transform.localRotation = Quaternion.Euler(0, -25f, 0);

        var cookie = Group("Cookie", root.transform, Vector3.zero);
        Rounded("Cookie disc", cookie, V(0, b + 0.005f, 0), V(0.036f, 0.01f, 0.036f), cookieTan, 0.0049f * k);
        foreach (var chip in new[] { new Vector3(-0.008f, 0, 0.006f), new Vector3(0.009f, 0, 0.004f), new Vector3(0.001f, 0, -0.009f) })
            Rounded("Chocolate chip", cookie, V(chip.x, b + 0.0105f, chip.z), V(0.006f, 0.004f, 0.006f), chocolate, 0.0019f * k);

        var muffin = Group("Muffin", root.transform, Vector3.zero);
        var cups = new[] { pink, mint, butter };
        Rounded("Muffin cup", muffin, V(0, b + 0.007f, 0), V(0.026f, 0.014f, 0.026f), cups[index % 3], 0.0049f * k);
        Rounded("Muffin top", muffin, V(0, b + 0.019f, 0), V(0.032f, 0.014f, 0.032f), muffinBrown, 0.0069f * k);
        Rounded("Berry", muffin, V(0, b + 0.0265f, 0), V(0.007f, 0.006f, 0.007f), pink, 0.0029f * k);

        cookie.gameObject.SetActive(false); muffin.gameObject.SetActive(false);
        QuickActionsAPI.AddGrabInteraction(root);
        var grabbable = root.GetComponentInChildren<Grabbable>();
        if (grabbable == null) throw new InvalidOperationException("SDK did not create Grabbable for pastry " + (index + 1));
        grabbable.MaxGrabPoints = 1;
        return new CafeStation.ItemView { id = "item-" + (index + 1), piece = root.transform, grabbable = grabbable, croissant = croissant.gameObject, cookie = cookie.gameObject, muffin = muffin.gameObject };
    }

    // ---- card ----
    static void Card(CafeStation station, NerdyDirector n, Transform root, Transform frame, Button template, OnboardingDirector d)
    {
        var card = new GameObject(CardName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        card.transform.SetParent(root, false);
        card.transform.localPosition = frame.localPosition; card.transform.localRotation = frame.localRotation; card.transform.localScale = frame.localScale;
        var rect = card.GetComponent<RectTransform>(); rect.sizeDelta = frame.GetComponent<RectTransform>().sizeDelta;
        var canvas = card.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
        var frameCanvas = frame.GetComponent<Canvas>();
        canvas.worldCamera = frameCanvas != null && frameCanvas.worldCamera != null ? frameCanvas.worldCamera : d.head.GetComponent<Camera>();
        // Card coordinates are authored for the 920 x 470 frame.
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

        var row = new GameObject("Cafe buttons", typeof(RectTransform)); row.transform.SetParent(card.transform, false);
        row.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        var group = row.AddComponent<CanvasGroup>();
        station.fallbackGroups = new[] { group };
        station.startButton = RowButton(row.transform, "Start", "Start", 0, 20, template, station.PressStart);
        station.dealButton = RowButton(row.transform, "Deal a round", "Deal a round", 0, 19, template, station.PressDealRound);
        station.checkButton = RowButton(row.transform, "Check order", "Check order", 1, 19, template, station.PressCheck);
        station.clearButton = RowButton(row.transform, "Clear table", "Clear table", 2, 19, template, station.PressClear);
        station.nextButton = RowButton(row.transform, "Next chapter", "Next chapter", 3, 19, template, station.PressNextChapter);
        station.backButton = RowButton(row.transform, "Back to lessons", "Back to lessons", 4, 17, template, station.Close);
        UnityEventTools.AddPersistentListener(station.backButton.onClick, n.OnLessonBack);
        foreach (var b in new[] { station.dealButton, station.checkButton, station.clearButton, station.nextButton }) b.gameObject.SetActive(false);
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
        rubber = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/CargoRubber.mat") ?? baseMaterial;
        cream = Mat(baseMaterial, "CafeCream", new Color32(250, 238, 216, 255), 0.35f);
        latte = Mat(baseMaterial, "CafeLatte", new Color32(214, 170, 128, 255), 0.3f);
        coffee = Mat(baseMaterial, "CafeCoffee", new Color32(124, 80, 56, 255), 0.3f);
        espresso = Mat(baseMaterial, "CafeEspresso", new Color32(62, 40, 32, 255), 0.35f);
        pink = Mat(baseMaterial, "CafePink", new Color32(247, 170, 190, 255), 0.4f);
        mint = Mat(baseMaterial, "CafeMint", new Color32(166, 228, 204, 255), 0.4f);
        butter = Mat(baseMaterial, "CafeButter", new Color32(253, 225, 140, 255), 0.4f);
        plateWhite = Mat(baseMaterial, "CafePlate", new Color32(255, 252, 246, 255), 0.6f);
        croissantGold = Mat(baseMaterial, "CafeCroissant", new Color32(232, 164, 72, 255), 0.45f);
        cookieTan = Mat(baseMaterial, "CafeCookie", new Color32(206, 146, 84, 255), 0.3f);
        chocolate = Mat(baseMaterial, "CafeChocolate", new Color32(78, 46, 34, 255), 0.4f);
        muffinBrown = Mat(baseMaterial, "CafeMuffin", new Color32(160, 100, 62, 255), 0.35f);
        steamWhite = Mat(baseMaterial, "CafeSteam", new Color32(245, 245, 248, 255), 0.2f);
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
        t.deck = cream.color; t.trim = latte.color; t.accent = pink.color; t.accentSoft = mint.color;
        t.prop1 = pink.color; t.prop2 = mint.color; t.prop3 = butter.color;
        t.cardPanel = new Color32(52, 34, 28, 247);
        t.cardHeading = new Color32(255, 243, 224, 255);
        t.cardBody = new Color32(246, 234, 218, 255);
        t.buttonFill = new Color32(250, 196, 208, 255);
        t.buttonLabel = new Color32(62, 40, 32, 255);
        t.deckMaterial = cream; t.trimMaterial = latte; t.accentMaterial = pink;
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

    /// Plates, cups and wheels: a Unity cylinder without a collider (Dock 7 wheels use the same primitive).
    static GameObject Cylinder(string name, Transform parent, Vector3 p, Vector3 scale, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder); g.name = name;
        UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false); g.transform.localPosition = p; g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = m;
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

    static TMP_Text Text3D(string name, Transform parent, Vector3 p, string text, float size, TMP_FontAsset font, Color color, Vector2 rect)
    {
        var g = Group(name, parent, p);
        var label = g.gameObject.AddComponent<TextMeshPro>(); label.font = font; label.text = text; label.fontSize = size;
        label.color = color; label.alignment = TextAlignmentOptions.Center; label.textWrappingMode = TextWrappingModes.NoWrap;
        label.rectTransform.sizeDelta = rect;
        return label;
    }
}
