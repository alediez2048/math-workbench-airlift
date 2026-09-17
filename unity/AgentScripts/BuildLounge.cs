using System;
using System.IO;
using System.Linq;
using Airlift.Lounge;
using Airlift.Presentation;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// CC-FD-01. Builds the Nerdy lounge: the room the app opens in, where onboarding happens and where the learner
// browses. Not a box with cubes in it — a round room with a sky beyond its windows, a lit ceiling cove, a rug, two
// couches, plants, a lamp, a shelf and the card framed into the wall, in the rounded toy vocabulary the workbenches
// use and entirely from NerdyStyle tokens (value shifts only, no new hues).
// Shell and sky drop in Your room; furniture and glows stay. Nothing stands in the workbench volume.
// Idempotent: deletes and rebuilds the root, the sky assets and the consent row. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/BuildLounge.cs --entry BuildLounge.Run
public static class BuildLounge
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string MatDir = "Assets/Airlift/Materials/Lounge";

    // The seated player sits at the origin facing +Z; the table and cards own roughly x +/-0.9, z 0.1..1.25.
    const float Radius = 3.1f, WallHeight = 2.85f;
    const int Segments = 12;

    static NerdyStyle style;
    static Material matRugInner;
    static Material matFloor, matRug, matWall, matCove, matCouch, matCushion, matWood, matLeaf, matPot, matGlow, matSky, matFrame;

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset")
                ?? throw new InvalidOperationException("NerdyStyle.asset missing.");
        Directory.CreateDirectory(MatDir);
        BuildMaterials();

        var existing = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Nerdy lounge - world locked");
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

        var root = new GameObject("Nerdy lounge - world locked");
        var room = root.AddComponent<LoungeRoom>();
        room.sky = BuildSkyMaterial();
        room.shell = BuildShell(root.transform);
        room.furniture = BuildFurniture(root.transform);
        room.passthrough = UnityEngine.Object.FindAnyObjectByType<OVRPassthroughLayer>(FindObjectsInactive.Include);

        room.board = BuildBoard(room);
        var arrival = BuildArrival(room);
        room.arrival = arrival;
        var director0 = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        director0.arrival = arrival;
        string rowNote = BuildSceneryRow(room);
        var director = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        director.lounge = room;
        EditorUtility.SetDirty(director);

        room.Apply(LoungeScenery.Default);
        EditorUtility.SetDirty(room);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        AssetDatabase.SaveAssets();

        return "Lounge built: " + root.GetComponentsInChildren<Renderer>(true).Length + " renderers, "
               + root.GetComponentsInChildren<Light>(true).Length + " lights, sky "
               + (room.sky != null ? "assigned" : "missing") + "; " + rowNote;
    }

    // ---- shell: a round room, windows onto the sky, a lit cove -------------------------------------------------

    static GameObject BuildShell(Transform parent)
    {
        var shell = Child(parent, "Shell");

        Cylinder(shell.transform, "Floor", new Vector3(0f, -0.03f, 0f), new Vector3(Radius * 2.06f, 0.03f, Radius * 2.06f), matFloor);
        Cylinder(shell.transform, "Ceiling", new Vector3(0f, WallHeight, 0f), new Vector3(Radius * 2.02f, 0.03f, Radius * 2.02f), matWall);

        // Wall segments in a ring; three bays behind the board are glazed, so the sky reads from every seat.
        float step = 360f / Segments, width = 2f * Mathf.PI * Radius / Segments * 1.08f;
        for (int i = 0; i < Segments; i++)
        {
            float a = i * step;
            var dir = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
            var rot = Quaternion.Euler(0f, a, 0f);
            bool window = i == 2 || i == 3 || i == 9 || i == 10;

            if (window)
            {
                Place(Box(shell.transform, "Window sill " + i, dir * Radius + Vector3.up * 0.3f, new Vector3(width, 0.6f, 0.16f), matWall), rot);
                Place(Box(shell.transform, "Window lintel " + i, dir * Radius + Vector3.up * (WallHeight - 0.24f), new Vector3(width, 0.48f, 0.16f), matWall), rot);
                Place(Box(shell.transform, "Sky pane " + i, dir * (Radius + 0.01f) + Vector3.up * (WallHeight / 2f + 0.06f), new Vector3(width, WallHeight - 1.08f, 0.03f), matSky), rot);
                Place(Box(shell.transform, "Mullion " + i, dir * (Radius - 0.04f) + Vector3.up * (WallHeight / 2f + 0.06f), new Vector3(0.05f, WallHeight - 1.08f, 0.1f), matFrame), rot);
            }
            else Place(Box(shell.transform, "Wall " + i, dir * Radius + Vector3.up * (WallHeight / 2f), new Vector3(width, WallHeight, 0.14f), matWall), rot);

            // Ceiling cove: an inset ring that hides the light strip — the guide's page glow made architectural.
            Place(Box(shell.transform, "Cove " + i, dir * (Radius - 0.36f) + Vector3.up * (WallHeight - 0.18f), new Vector3(width, 0.22f, 0.3f), matCove), rot);
            Place(Box(shell.transform, "Cove strip " + i, dir * (Radius - 0.5f) + Vector3.up * (WallHeight - 0.3f), new Vector3(width * 0.9f, 0.03f, 0.06f), matGlow), rot);
        }
        return shell;
    }

    // ---- furniture: a place to be, not props on a stage ---------------------------------------------------------

    static GameObject BuildFurniture(Transform parent)
    {
        var f = Child(parent, "Furniture");

        // A rug, so the middle of the room is a place to sit rather than empty floor.
        Cylinder(f.transform, "Rug", new Vector3(0f, 0.006f, 0.35f), new Vector3(3.6f, 0.006f, 3.6f), matRug);
        Cylinder(f.transform, "Rug ring", new Vector3(0f, 0.014f, 0.35f), new Vector3(3.0f, 0.005f, 3.0f), matRugInner);

        // Owner 2026-09-17: "lets get rid of everything but the carpet and the whiteboard, I want nothing else
        // in here for now." The couches, coffee table, plants, lamp, shelf and Dee's pedestal are gone. The room
        // shell and its light stay, because they are the room rather than things standing in it.

        // Light: cool key from the window side, warm fill opposite, a cove wash and a low bounce.
        Glow(f.transform, "Key glow", new Vector3(-2.2f, 2.5f, 2.2f), style.cyan, 4.2f, 12f);
        Glow(f.transform, "Fill glow", new Vector3(2.4f, 1.8f, 0.2f), style.amber, 2.4f, 10f);
        Glow(f.transform, "Cove glow", new Vector3(0f, 2.55f, 1.4f), style.lavender, 2.6f, 9f);
        Glow(f.transform, "Lamp glow", new Vector3(-2.55f, 1.55f, 0.95f), style.amber, 1.8f, 4.5f);

        var wash = Child(f.transform, "Room wash");
        wash.transform.rotation = Quaternion.Euler(52f, 18f, 0f);
        var w = wash.AddComponent<Light>();
        w.type = LightType.Directional; w.color = Lift(style.lavender, -0.12f); w.intensity = 0.55f; w.shadows = LightShadows.None;
        return f;
    }

    static void Couch(Transform parent, string name, Vector3 at, float yaw)
    {
        var c = Child(parent, name);
        c.transform.position = at;
        c.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        Local(c.transform, "Base", new Vector3(0f, 0.2f, 0f), new Vector3(1.9f, 0.4f, 0.8f), matCouch);
        Local(c.transform, "Back", new Vector3(0f, 0.52f, -0.32f), new Vector3(1.9f, 0.66f, 0.2f), matCouch);
        Local(c.transform, "Arm left", new Vector3(-0.92f, 0.38f, 0f), new Vector3(0.2f, 0.36f, 0.8f), matCouch);
        Local(c.transform, "Arm right", new Vector3(0.92f, 0.38f, 0f), new Vector3(0.2f, 0.36f, 0.8f), matCouch);
        for (int i = 0; i < 3; i++)
            Local(c.transform, "Cushion " + i, new Vector3(-0.6f + i * 0.6f, 0.44f, 0.02f), new Vector3(0.56f, 0.14f, 0.7f), matCushion);
        for (int i = 0; i < 4; i++)
            Local(c.transform, "Foot " + i, new Vector3(i % 2 == 0 ? -0.8f : 0.8f, 0.03f, i < 2 ? -0.3f : 0.3f), new Vector3(0.08f, 0.06f, 0.08f), matWood);
    }

    static void CoffeeTable(Transform parent, Vector3 at)
    {
        var t = Child(parent, "Coffee table");
        t.transform.position = at;
        Local(t.transform, "Top", new Vector3(0f, 0.42f, 0f), new Vector3(0.78f, 0.07f, 0.56f), matWood);
        for (int i = 0; i < 4; i++)
            Local(t.transform, "Leg " + i, new Vector3(i % 2 == 0 ? -0.3f : 0.3f, 0.2f, i < 2 ? -0.19f : 0.19f), new Vector3(0.06f, 0.4f, 0.06f), matFrame);
        Local(t.transform, "Cup", new Vector3(0.18f, 0.5f, 0.06f), new Vector3(0.1f, 0.1f, 0.1f), matGlow);
        Local(t.transform, "Books", new Vector3(-0.16f, 0.48f, -0.02f), new Vector3(0.3f, 0.06f, 0.22f), matCushion);
    }

    static void Plant(Transform parent, string name, Vector3 at, float scale)
    {
        var p = Child(parent, name);
        p.transform.position = at;
        Cylinder(p.transform, "Pot", at + new Vector3(0f, 0.2f * scale, 0f), new Vector3(0.34f, 0.2f, 0.34f) * scale, matPot);
        Cylinder(p.transform, "Soil", at + new Vector3(0f, 0.4f * scale, 0f), new Vector3(0.3f * scale, 0.01f, 0.3f * scale), matWood);
        // Leaves as flattened spheres on stems: silhouette is what reads in a headset.
        var rng = new System.Random(name.GetHashCode());
        for (int i = 0; i < 7; i++)
        {
            float a = i * (360f / 7f) + (float)rng.NextDouble() * 18f;
            float h = (0.55f + (float)rng.NextDouble() * 0.5f) * scale;
            float outward = (0.12f + (float)rng.NextDouble() * 0.2f) * scale;
            var dir = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
            var stem = Box(p.transform, "Stem " + i, at + Vector3.up * (0.4f * scale + h * 0.45f) + dir * (outward * 0.5f), new Vector3(0.022f, h, 0.022f), matWood);
            stem.transform.rotation = Quaternion.Euler(dir.z * 13f, a, -dir.x * 13f);
            var leaf = Sphere(p.transform, "Leaf " + i, at + Vector3.up * (0.4f * scale + h * 0.95f) + dir * outward, new Vector3(0.3f, 0.07f, 0.22f) * scale, matLeaf);
            leaf.transform.rotation = Quaternion.Euler(-18f, a, 0f);
        }
    }

    static void Lamp(Transform parent, string name, Vector3 at)
    {
        var l = Child(parent, name);
        l.transform.position = at;
        Cylinder(l.transform, "Foot", at + new Vector3(0f, 0.03f, 0f), new Vector3(0.3f, 0.03f, 0.3f), matFrame);
        Cylinder(l.transform, "Pole", at + new Vector3(0f, 0.8f, 0f), new Vector3(0.05f, 0.8f, 0.05f), matFrame);
        Cylinder(l.transform, "Shade", at + new Vector3(0f, 1.66f, 0f), new Vector3(0.42f, 0.22f, 0.42f), matGlow);
    }

    static void Shelf(Transform parent, Vector3 at, float yaw)
    {
        var s = Child(parent, "Shelf");
        s.transform.position = at;
        s.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        Local(s.transform, "Frame left", new Vector3(-0.5f, 0.6f, 0f), new Vector3(0.06f, 1.2f, 0.32f), matWood);
        Local(s.transform, "Frame right", new Vector3(0.5f, 0.6f, 0f), new Vector3(0.06f, 1.2f, 0.32f), matWood);
        for (int i = 0; i < 3; i++)
            Local(s.transform, "Board " + i, new Vector3(0f, 0.3f + i * 0.42f, 0f), new Vector3(1.05f, 0.05f, 0.32f), matWood);
        Local(s.transform, "Books a", new Vector3(-0.25f, 0.46f, 0f), new Vector3(0.34f, 0.26f, 0.2f), matCushion);
        Local(s.transform, "Books b", new Vector3(0.22f, 0.88f, 0f), new Vector3(0.28f, 0.24f, 0.2f), matCouch);
        Local(s.transform, "Ornament", new Vector3(-0.2f, 1.34f, 0f), new Vector3(0.18f, 0.18f, 0.18f), matGlow);
    }

    /// The scenery choice on the consent card. The arrival's two big pills come with CC-FD-03.
    static string BuildSceneryRow(LoungeRoom room)
    {
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include)
                ?? throw new InvalidOperationException("NerdyDirector missing.");
        var consent = n.consentRoot.transform;
        var template = consent.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.StartsWith("Language "))
                       ?? throw new InvalidOperationException("Language pills missing (run AddLanguageChoice first).");
        var label = consent.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name == "Language label")
                    ?? throw new InvalidOperationException("Language label missing.");

        var old = consent.Find("Scenery row"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var row = new GameObject("Scenery row", typeof(RectTransform)).GetComponent<RectTransform>();
        row.SetParent(consent, false);
        row.anchoredPosition = new Vector2(0, -158);
        row.sizeDelta = new Vector2(640, 40);

        var rowLabel = UnityEngine.Object.Instantiate(label.gameObject, row).GetComponent<TMP_Text>();
        rowLabel.name = "Scenery label"; rowLabel.text = "Where you learn";
        rowLabel.rectTransform.anchoredPosition = new Vector2(-170, 0);

        string[] names = { "Scenery your room", "Scenery nerdy lounge" };
        string[] copy = { "Your room", "Nerdy lounge" };
        string[] methods = { "ChooseYourRoom", "ChooseNerdyLounge" };
        float[] xs = { 20f, 185f };
        for (int i = 0; i < 2; i++)
        {
            var go = UnityEngine.Object.Instantiate(template.gameObject, row); go.name = names[i];
            var b = go.GetComponent<Button>();
            for (int k = b.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(b.onClick, k);
            var call = (UnityEngine.Events.UnityAction)Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), room, methods[i]);
            UnityEventTools.AddPersistentListener(b.onClick, call);
            var r = go.GetComponent<RectTransform>(); r.anchoredPosition = new Vector2(xs[i], 0); r.sizeDelta = new Vector2(150, 40);
            go.GetComponentInChildren<TMP_Text>(true).text = copy[i];
        }
        EditorUtility.SetDirty(n);
        return "\"Where you learn\" row on the consent card";
    }

    // ---- sky -----------------------------------------------------------------------------------------------------

    /// A gradient sky in the guide's colours: a cyan glow low down where the page glow would be, indigo through the
    /// middle, deep base overhead. Generated, so it costs one small texture and no purchase.
    static Material BuildSkyMaterial()
    {
        const string texPath = MatDir + "/LoungeSkyGradient.png";
        const int w = 8, h = 256;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            float t = y / (float)(h - 1);
            Color c = t < 0.45f
                ? Color.Lerp(Lift(style.cyan, -0.5f), style.indigo, Mathf.InverseLerp(0f, 0.45f, t))
                : Color.Lerp(style.indigo, Lift(style.baseColor, -0.3f), Mathf.InverseLerp(0.45f, 1f, t));
            for (int x = 0; x < w; x++) tex.SetPixel(x, y, c);
        }
        tex.Apply();
        File.WriteAllBytes(Path.GetFullPath(texPath), tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(texPath);
        if (importer != null) { importer.wrapMode = TextureWrapMode.Clamp; importer.mipmapEnabled = false; importer.SaveAndReimport(); }
        var gradient = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

        string matPath = MatDir + "/LoungeSky.mat";
        var sky = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (sky == null) { sky = new Material(Shader.Find("Skybox/Panoramic")); AssetDatabase.CreateAsset(sky, matPath); }
        sky.SetTexture("_MainTex", gradient);
        sky.SetFloat("_Exposure", 1.05f);
        sky.SetFloat("_Mapping", 1f);
        EditorUtility.SetDirty(sky);

        matSky.mainTexture = gradient;      // the panes show the same gradient as the sky behind them
        EditorUtility.SetDirty(matSky);
        return sky;
    }

    // ---- materials and primitives ---------------------------------------------------------------------------------

    static void BuildMaterials()
    {
        matFloor   = Mat("LoungeFloor", Lift(style.baseColor, -0.26f), 0.88f);
        matRug     = Mat("LoungeRug", Lift(style.indigo, -0.55f), 0.97f);
        matRugInner= Mat("LoungeRugInner", Lift(style.indigo, -0.42f), 0.97f);
        matWall    = Mat("LoungeWall", Lift(style.baseColor, 0.16f), 0.94f);
        matCove    = Mat("LoungeCove", Lift(style.baseColor, 0.3f), 0.9f);
        matCouch   = Mat("LoungeCouch", Lift(style.indigo, 0.12f), 0.96f);
        matCushion = Mat("LoungeCushion", Lift(style.lavender, 0.05f), 0.96f);
        matWood    = Mat("LoungeWood", Lift(style.line, -0.1f), 0.8f);
        matLeaf    = Mat("LoungeLeaf", Lift(style.cyan, -0.28f), 0.85f);
        matPot     = Mat("LoungePot", Lift(style.amber, -0.12f), 0.8f);
        matGlow    = Mat("LoungeGlow", style.cyan, 0.4f, emissive: true);
        matFrame   = Mat("LoungeFrame", Lift(style.line, 0.12f), 0.65f);
        matSky     = Mat("LoungeSkyPane", Color.white, 0.5f, emissive: false, unlit: true);
    }

    static Color Lift(Color c, float amount) => amount >= 0f ? Color.Lerp(c, Color.white, amount) : Color.Lerp(c, Color.black, -amount);


    // ---- the board: one surface, mounted on the welcome panel ----------------------------------------------------

    /// Owner 2026-09-17: "our onboarding dashboard is the whiteboard, not having two". The board is built as a
    /// child of the welcome panel, so the card sits ON it and the existing handle still moves and resizes both.
    static GameObject BuildBoard(LoungeRoom room)
    {
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include)
                ?? throw new InvalidOperationException("NerdyDirector missing.");
        var panel = n.transform;
        var old = panel.Find("Board"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

        var board = new GameObject("Board");
        board.transform.SetParent(panel, false);
        board.transform.localPosition = Vector3.zero;
        board.transform.localRotation = Quaternion.identity;

        // Panel space is the card's own: +Z points away from the learner, so the backing sits at positive z.
        const float w = 1.62f, h = 1.12f, back = 0.035f;
        LocalOn(board.transform, "Board face", new Vector3(0f, 0f, back), new Vector3(w, h, 0.05f), matWall);
        LocalOn(board.transform, "Board frame top", new Vector3(0f, h / 2f, back - 0.012f), new Vector3(w + 0.06f, 0.05f, 0.08f), matFrame);
        LocalOn(board.transform, "Board frame bottom", new Vector3(0f, -h / 2f, back - 0.012f), new Vector3(w + 0.06f, 0.05f, 0.08f), matFrame);
        LocalOn(board.transform, "Board frame left", new Vector3(-w / 2f, 0f, back - 0.012f), new Vector3(0.05f, h + 0.06f, 0.08f), matFrame);
        LocalOn(board.transform, "Board frame right", new Vector3(w / 2f, 0f, back - 0.012f), new Vector3(0.05f, h + 0.06f, 0.08f), matFrame);
        LocalOn(board.transform, "Board ledge", new Vector3(0f, -h / 2f - 0.06f, back - 0.05f), new Vector3(w + 0.06f, 0.04f, 0.16f), matWood);
        LocalOn(board.transform, "Board cove", new Vector3(0f, h / 2f + 0.05f, back - 0.06f), new Vector3(w - 0.1f, 0.02f, 0.03f), matGlow);
        return board;
    }

    // ---- the arrival: the logo, before anything else -------------------------------------------------------------

    /// CC-FD-03. A world-space canvas on the panel: dots converge, the Nerdy logo assembles, a pill bar reports
    /// whatever is genuinely still loading. LoungeArrival animates it; this only builds it.
    static LoungeArrival BuildArrival(LoungeRoom room)
    {
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var panel = n.transform;
        var old = panel.Find("Arrival"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

        var go = new GameObject("Arrival", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
        go.transform.SetParent(panel, false);
        var rect = go.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(0f, 0f, -0.02f);      // just in front of the board
        rect.localRotation = Quaternion.identity;
        rect.sizeDelta = new Vector2(900f, 620f);
        rect.localScale = Vector3.one * 0.0018f;               // panel canvases work in the same small scale

        var arrival = go.AddComponent<LoungeArrival>();
        arrival.group = go.GetComponent<CanvasGroup>();

        var logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Airlift/Branding/nerdy-logo-green.png");
        var logo = new GameObject("Logo", typeof(RectTransform), typeof(Image));
        logo.transform.SetParent(go.transform, false);
        var logoRect = logo.GetComponent<RectTransform>();
        logoRect.sizeDelta = new Vector2(620f, 253f);
        var logoImage = logo.GetComponent<Image>();
        logoImage.sprite = logoSprite;
        logoImage.preserveAspect = true;
        logoImage.color = new Color(1f, 1f, 1f, 0f);
        arrival.logo = logoRect;

        // Five dots in the accent order the guide gives: amber, magenta, orchid, cyan, lavender.
        var accents = new[] { style.amber, style.magenta, style.orchid, style.cyan, style.lavender };
        var dots = new RectTransform[accents.Length];
        var pill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Airlift/Sprites/NerdyPill.png");
        var knob = (Sprite)AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        for (int i = 0; i < accents.Length; i++)
        {
            var dot = new GameObject("Dot " + i, typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(go.transform, false);
            var r = dot.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(26f, 26f);
            var img = dot.GetComponent<Image>();
            img.sprite = knob; img.type = Image.Type.Simple; img.color = accents[i];
            dots[i] = r;
        }
        arrival.dots = dots;

        var barRoot = new GameObject("Progress", typeof(RectTransform));
        barRoot.transform.SetParent(go.transform, false);
        var barRect = barRoot.GetComponent<RectTransform>();
        barRect.anchoredPosition = new Vector2(0f, -190f);
        barRect.sizeDelta = new Vector2(360f, 12f);

        var track = new GameObject("Track", typeof(RectTransform), typeof(Image));
        track.transform.SetParent(barRoot.transform, false);
        track.GetComponent<RectTransform>().sizeDelta = new Vector2(360f, 12f);
        var trackImage = track.GetComponent<Image>();
        trackImage.sprite = pill; trackImage.type = Image.Type.Sliced;
        trackImage.pixelsPerUnitMultiplier = pill != null && pill.border.x > 0f ? pill.border.x / 6f : 1f;
        trackImage.color = new Color(style.line.r, style.line.g, style.line.b, 0.45f);

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(barRoot.transform, false);
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchorMin = new Vector2(0.5f, 0.5f); fillRect.anchorMax = new Vector2(0.5f, 0.5f);
        fillRect.anchoredPosition = new Vector2(-180f, 0f);
        fillRect.sizeDelta = new Vector2(12f, 12f);   // never narrower than its own round ends
        var fillImage = fill.GetComponent<Image>();
        var spectrum = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Airlift/Sprites/NerdyPillGradient.png");
        fillImage.sprite = spectrum != null ? spectrum : pill;
        fillImage.type = Image.Type.Sliced;
        if (fillImage.sprite != null && fillImage.sprite.border.x > 0f) fillImage.pixelsPerUnitMultiplier = fillImage.sprite.border.x / 6f;
        arrival.barFill = fillRect;
        arrival.barRoot = barRoot;
        arrival.barWidth = 360f;
        return arrival;
    }

    static GameObject LocalOn(Transform parent, string name, Vector3 localCentre, Vector3 size, Material material)
    {
        var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localCentre;
        go.GetComponent<MeshFilter>().sharedMesh = RoundedBoxMesh.Build(size, Radius3(size));
        Finish(go, material);
        return go;
    }

    static GameObject Child(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    static GameObject Place(GameObject go, Quaternion rot) { go.transform.rotation = rot; return go; }

    static GameObject Box(Transform parent, string name, Vector3 centre, Vector3 size, Material material)
    {
        var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(parent, false);
        go.transform.position = centre;
        go.GetComponent<MeshFilter>().sharedMesh = RoundedBoxMesh.Build(size, Radius3(size));
        Finish(go, material);
        return go;
    }

    static GameObject Local(Transform parent, string name, Vector3 localCentre, Vector3 size, Material material)
    {
        var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localCentre;
        go.GetComponent<MeshFilter>().sharedMesh = RoundedBoxMesh.Build(size, Radius3(size));
        Finish(go, material);
        return go;
    }

    static float Radius3(Vector3 size) => Mathf.Min(0.07f, Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * 0.45f);

    static GameObject Cylinder(Transform parent, string name, Vector3 centre, Vector3 size, Material material) =>
        Primitive(parent, name, centre, new Vector3(size.x, Mathf.Max(size.y, 0.004f), size.z), material, PrimitiveType.Cylinder);

    static GameObject Sphere(Transform parent, string name, Vector3 centre, Vector3 size, Material material) =>
        Primitive(parent, name, centre, size, material, PrimitiveType.Sphere);

    static GameObject Primitive(Transform parent, string name, Vector3 centre, Vector3 size, Material material, PrimitiveType type)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.position = centre;
        go.transform.localScale = size;
        Finish(go, material);
        return go;
    }

    static void Finish(GameObject go, Material material)
    {
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = material;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
    }

    static void Glow(Transform parent, string name, Vector3 at, Color colour, float intensity, float range)
    {
        var go = Child(parent, name);
        go.transform.position = at;
        var light = go.AddComponent<Light>();
        light.type = LightType.Point; light.color = colour; light.intensity = intensity; light.range = range;
        light.shadows = LightShadows.None;
    }

    static Material Mat(string name, Color colour, float roughness, bool emissive = false, bool unlit = false)
    {
        string path = MatDir + "/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit");
        if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
        mat.shader = shader;
        mat.color = colour;
        if (!unlit)
        {
            mat.SetFloat("_Smoothness", Mathf.Clamp01(1f - roughness));
            mat.SetFloat("_Metallic", 0f);
        }
        if (emissive) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", colour * 2.6f); }
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
