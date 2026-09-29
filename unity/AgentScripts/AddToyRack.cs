using System;
using System.Collections.Generic;
using System.Linq;
using Airlift.Lounge;
using Airlift.Presentation;
using Airlift.Welcome;
using Oculus.Interaction;
using Oculus.Interaction.Editor.QuickActions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Toy rack (spec docs/superpowers/specs/2026-09-29-lounge-toy-rack-design.md). Fifteen slots on three shelves under the
// lounge furniture (hidden in Your room), one placeholder toy per chapter built from rounded boxes in the toy's colour,
// each grabbable (Meta quick action, one grab point, kinematic) with ToyReturn; a world-space progress board above.
// Idempotent. Run after ApplyRoomFinish. Run: unity command run_script --file AgentScripts/AddToyRack.cs --entry AddToyRack.Run
public static class AddToyRack
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity", MatDir = "Assets/Airlift/Materials/Lounge/Toys";
    // Owner 2026-09-29: "make it much bigger, it looks very small compared to the rest of the space".
    const float RackWidth = 2.3f, ShelfDepth = 0.36f, ShelfThick = 0.04f, SlotStep = 0.46f, ToyScale = 1.7f, Uprights = 2.05f;
    static readonly float[] ShelfY = { 1.7f, 1.15f, 0.6f };   // Cargo on top, Café, Garden
    static NerdyStyle style;

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var room = n.lounge ?? throw new InvalidOperationException("Run BuildLounge first.");
        var furniture = room.furniture != null ? room.furniture.transform : throw new InvalidOperationException("Lounge furniture missing.");
        style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset") ?? throw new InvalidOperationException("NerdyStyle.asset missing.");
        if (!AssetDatabase.IsValidFolder(MatDir)) AssetDatabase.CreateFolder("Assets/Airlift/Materials/Lounge", "Toys");

        foreach (var name in new[] { "Toy rack", "Progress board" }) { var old = furniture.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }

        // ---- the rack ----
        var rackGo = new GameObject("Toy rack"); rackGo.transform.SetParent(furniture, false);
        var rack = rackGo.AddComponent<ToyRack>();
        var frame = Mat("RackFrame", Hex("#2c3260"), 0.7f); var shelfMat = Mat("RackShelf", Hex("#3a3f6a"), 0.6f);
        foreach (float sx in new[] { -RackWidth / 2f, RackWidth / 2f })
            Box(rackGo.transform, "Upright", new Vector3(sx, Uprights / 2f, 0f), new Vector3(0.07f, Uprights, ShelfDepth), frame);
        for (int s = 0; s < 3; s++) Box(rackGo.transform, "Shelf " + s, new Vector3(0f, ShelfY[s] - ShelfThick / 2f, 0f), new Vector3(RackWidth, ShelfThick, ShelfDepth), shelfMat);
        // Local +z is the rack's front (it faces the room's centre); the back panel hugs the wall behind.
        Box(rackGo.transform, "Back", new Vector3(0f, 1.15f, -(ShelfDepth / 2f - 0.01f)), new Vector3(RackWidth, 1.9f, 0.02f), frame);

        var locked = Mat("ToyLocked", new Color(0.42f, 0.43f, 0.53f, 1f), 0.9f);
        var glow = Mat("ToyGlow", style.amber, 0.5f, emissive: true);
        var slots = new List<ToyRack.Slot>();
        var models = ToyRackModel.Slots;
        for (int i = 0; i < models.Count; i++)
        {
            var m = models[i]; int shelf = i / 5, col = i % 5;
            var anchor = new GameObject("Slot " + m.Id).transform; anchor.SetParent(rackGo.transform, false);
            anchor.localPosition = new Vector3(2f * SlotStep - col * SlotStep, ShelfY[shelf], 0.02f);   // chapter 1 at the learner's left (the rack faces the room)
            var toy = BuildToy(anchor, m); toy.name = "Toy " + m.Id; toy.transform.localScale = Vector3.one * ToyScale;
            toy.transform.SetParent(rackGo.transform, true);   // sibling of the anchor: it moves freely, the anchor is home
            var renderers = toy.GetComponentsInChildren<MeshRenderer>(true);
            var colourMats = renderers.Select(r => r.sharedMaterial).ToArray();
            var bounds = renderers[0].bounds; foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            var collider = toy.AddComponent<BoxCollider>(); collider.center = toy.transform.InverseTransformPoint(bounds.center); collider.size = (bounds.size + Vector3.one * 0.01f) / ToyScale;
            // Owner 2026-09-29: walking to the wall leaves the Guardian. Pull-to-hand distance grab: point, squeeze, the toy
            // flies to the hand and can be held and turned; the rig gains a distance-grab interactor per hand (once).
            QuickActionsAPI.AddDistanceGrabInteraction(toy, DistanceGrabMode.PullToHand);
            QuickActionsAPI.AddGrabInteraction(toy);   // the second hand grabs it up close
            QuickActionsAPI.AddRayGrabInteraction(toy); // the trigger grabs it on the ray, and locked toys explain themselves
            var grabbable = toy.GetComponentInChildren<Grabbable>(true) ?? throw new InvalidOperationException("SDK did not create Grabbable for " + m.Id);
            // Owner 2026-09-29: "make it bigger or smaller by grabbing it with both controllers and expanding it".
            grabbable.MaxGrabPoints = 2;
            var free = grabbable.GetComponent<GrabFreeTransformer>(); if (free == null) free = grabbable.gameObject.AddComponent<GrabFreeTransformer>();
            var axis = new TransformerUtils.ConstrainedAxis { ConstrainAxis = true, AxisRange = new TransformerUtils.FloatRange { Min = ToyPlacementRule.MinScale * ToyScale, Max = ToyPlacementRule.MaxScale * ToyScale } };
            free.InjectOptionalScaleConstraints(new TransformerUtils.ScaleConstraints { ConstraintsAreRelative = false, XAxis = axis, YAxis = axis, ZAxis = axis });
            grabbable.InjectOptionalOneGrabTransformer(free);
            grabbable.InjectOptionalTwoGrabTransformer(free);
            var body = toy.GetComponent<Rigidbody>(); if (body == null) body = toy.AddComponent<Rigidbody>();
            body.isKinematic = true; body.useGravity = false;
            var place = toy.AddComponent<ToyPlace>(); place.grabbable = grabbable; place.home = anchor; place.halfHeight = bounds.size.y / 2f / ToyScale; place.homeScale = ToyScale;
            var hint = toy.AddComponent<ToyLockedHint>(); hint.ray = toy.GetComponentInChildren<RayInteractable>(true);
            slots.Add(new ToyRack.Slot { id = m.Id, anchor = anchor, toy = toy, place = place, hint = hint, renderers = renderers, colourMaterials = colourMats, lockedMaterial = locked, glowMaterial = glow });
        }
        rack.slots = slots.ToArray();
        rack.Refresh(new LibraryState());   // owner 2026-09-29: every toy grey and ungrabbable until its chapter is complete, from the first frame

        // ---- the progress board ----
        var board = BuildBoard(furniture, rackGo.transform);

        // Edit-time placement beside the board's scene pose (runtime PlaceBeside repeats this once the board is placed).
        if (n.welcomeAnchor != null) rack.PlaceBeside(n.welcomeAnchor);
        // A world-space canvas reads from its -z side, so it turns its back to the wall like the rack does.
        board.transform.SetPositionAndRotation(rackGo.transform.TransformPoint(0f, Uprights + 0.28f, -0.1f), rackGo.transform.rotation * Quaternion.Euler(0f, 180f, 0f));
        board.transform.SetParent(rackGo.transform, true);

        n.toyRack = rack; n.progressBoard = board;
        foreach (var o in new UnityEngine.Object[] { n, rack, board }) EditorUtility.SetDirty(o);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Toy rack: 15 slots on 3 shelves, grabbable placeholder toys, progress board (3 rows x 5)";
    }

    // ---- toys: placeholder compositions in the toy's colour (real props follow as a separate step) ----
    static GameObject BuildToy(Transform anchor, ToySlot m)
    {
        var root = new GameObject("toy"); root.transform.SetParent(anchor, false);
        var main = Mat("Toy_" + m.Id.Replace('#', '_'), Hex(m.Colour), 0.6f);
        var dark = Mat("ToyDark", Hex("#161C2C"), 0.7f); var green = Mat("ToyGreen", Hex("#4CC24A"), 0.6f); var brown = Mat("ToyBrown", Hex("#7A5230"), 0.8f); var cream = Mat("ToyCream", Hex("#E8E4F5"), 0.5f); var pastry = Mat("ToyPastry", Hex("#D28A4A"), 0.7f);
        string k = m.LessonId + "#" + m.Chapter;
        switch (k)
        {
            case "cargo_crew_fractions#1": Box(root.transform, "container", new Vector3(0, 0.045f, 0), new Vector3(0.16f, 0.09f, 0.09f), main, 0.008f); break;
            case "cargo_crew_fractions#2": Box(root.transform, "bed", new Vector3(-0.02f, 0.05f, 0), new Vector3(0.12f, 0.06f, 0.08f), main, 0.008f); Box(root.transform, "cab", new Vector3(0.065f, 0.045f, 0), new Vector3(0.05f, 0.07f, 0.08f), main, 0.01f); Wheels(root.transform, dark, 0.11f); break;
            case "cargo_crew_fractions#3": Box(root.transform, "bed", new Vector3(-0.025f, 0.04f, 0), new Vector3(0.07f, 0.035f, 0.07f), main, 0.006f); Box(root.transform, "cab", new Vector3(0.03f, 0.045f, 0), new Vector3(0.05f, 0.06f, 0.07f), main, 0.01f); Wheels(root.transform, dark, 0.08f); break;
            case "cargo_crew_fractions#4": Box(root.transform, "van", new Vector3(0, 0.05f, 0), new Vector3(0.12f, 0.07f, 0.07f), main, 0.02f); Wheels(root.transform, dark, 0.08f); break;
            case "cargo_crew_fractions#5": Box(root.transform, "post", new Vector3(0, 0.08f, 0), new Vector3(0.025f, 0.16f, 0.025f), main, 0.004f); Box(root.transform, "arm", new Vector3(0.03f, 0.155f, 0), new Vector3(0.14f, 0.02f, 0.02f), main, 0.004f); Box(root.transform, "base", new Vector3(0, 0.01f, 0), new Vector3(0.08f, 0.02f, 0.06f), dark, 0.004f); break;
            case "neighborhood_cafe_division#1": Box(root.transform, "plate", new Vector3(0, 0.006f, 0), new Vector3(0.14f, 0.012f, 0.14f), cream, 0.005f); Box(root.transform, "pastry", new Vector3(-0.03f, 0.03f, 0), new Vector3(0.05f, 0.035f, 0.05f), pastry, 0.014f); Box(root.transform, "pastry 2", new Vector3(0.03f, 0.03f, 0), new Vector3(0.05f, 0.035f, 0.05f), pastry, 0.014f); break;
            case "neighborhood_cafe_division#2": Box(root.transform, "top", new Vector3(0, 0.09f, 0), new Vector3(0.14f, 0.015f, 0.14f), main, 0.005f); Box(root.transform, "leg", new Vector3(0, 0.04f, 0), new Vector3(0.02f, 0.08f, 0.02f), dark, 0.004f); Box(root.transform, "foot", new Vector3(0, 0.005f, 0), new Vector3(0.08f, 0.01f, 0.08f), dark, 0.004f); break;
            case "neighborhood_cafe_division#3": Box(root.transform, "box", new Vector3(0, 0.04f, 0), new Vector3(0.12f, 0.08f, 0.09f), main, 0.008f); Box(root.transform, "lid", new Vector3(0, 0.085f, 0), new Vector3(0.125f, 0.012f, 0.095f), cream, 0.004f); break;
            case "neighborhood_cafe_division#4": Box(root.transform, "tray", new Vector3(0, 0.008f, 0), new Vector3(0.18f, 0.016f, 0.1f), dark, 0.005f); for (int i = 0; i < 3; i++) Box(root.transform, "pastry " + i, new Vector3(-0.055f + i * 0.055f, 0.034f, 0), new Vector3(0.045f, 0.035f, 0.05f), pastry, 0.014f); break;
            case "neighborhood_cafe_division#5": Box(root.transform, "pot", new Vector3(0, 0.05f, 0), new Vector3(0.08f, 0.1f, 0.08f), main, 0.015f); Box(root.transform, "handle", new Vector3(0.055f, 0.055f, 0), new Vector3(0.03f, 0.05f, 0.015f), dark, 0.005f); Box(root.transform, "spout", new Vector3(-0.055f, 0.075f, 0), new Vector3(0.04f, 0.015f, 0.015f), main, 0.005f); break;
            case "community_garden_multiplication#1": Box(root.transform, "tray", new Vector3(0, 0.015f, 0), new Vector3(0.16f, 0.03f, 0.09f), brown, 0.005f); for (int i = 0; i < 3; i++) Box(root.transform, "sprout " + i, new Vector3(-0.05f + i * 0.05f, 0.05f, 0), new Vector3(0.025f, 0.04f, 0.025f), green, 0.01f); break;
            case "community_garden_multiplication#2": Box(root.transform, "bed", new Vector3(0, 0.02f, 0), new Vector3(0.18f, 0.04f, 0.1f), brown, 0.006f); for (int i = 0; i < 3; i++) Box(root.transform, "plant " + i, new Vector3(-0.055f + i * 0.055f, 0.06f, 0), new Vector3(0.03f, 0.05f, 0.03f), green, 0.012f); break;
            case "community_garden_multiplication#3": Box(root.transform, "can", new Vector3(0, 0.045f, 0), new Vector3(0.08f, 0.09f, 0.07f), main, 0.012f); Box(root.transform, "spout", new Vector3(-0.06f, 0.07f, 0), new Vector3(0.06f, 0.014f, 0.014f), main, 0.005f); Box(root.transform, "handle", new Vector3(0, 0.1f, 0), new Vector3(0.06f, 0.012f, 0.012f), dark, 0.005f); break;
            case "community_garden_multiplication#4": for (int i = 0; i < 3; i++) Box(root.transform, "post " + i, new Vector3(-0.06f + i * 0.06f, 0.05f, 0), new Vector3(0.018f, 0.1f, 0.018f), cream, 0.004f); Box(root.transform, "rail", new Vector3(0, 0.07f, 0), new Vector3(0.16f, 0.014f, 0.012f), main, 0.004f); Box(root.transform, "rail 2", new Vector3(0, 0.035f, 0), new Vector3(0.16f, 0.014f, 0.012f), main, 0.004f); break;
            case "community_garden_multiplication#5": Box(root.transform, "stem", new Vector3(0, 0.05f, 0), new Vector3(0.014f, 0.1f, 0.014f), green, 0.004f); Box(root.transform, "head", new Vector3(0, 0.12f, 0), new Vector3(0.07f, 0.07f, 0.02f), main, 0.03f); Box(root.transform, "centre", new Vector3(0, 0.12f, -0.012f), new Vector3(0.03f, 0.03f, 0.01f), brown, 0.012f); break;
            default: Box(root.transform, "block", new Vector3(0, 0.04f, 0), new Vector3(0.08f, 0.08f, 0.08f), main, 0.014f); break;
        }
        return root;
    }

    static void Wheels(Transform parent, Material dark, float span)
    {
        foreach (float x in new[] { -span / 2f, span / 2f }) foreach (float z in new[] { -0.035f, 0.035f })
            Box(parent, "wheel", new Vector3(x, 0.012f, z), new Vector3(0.024f, 0.024f, 0.012f), dark, 0.01f);
    }

    // ---- the board ----
    static ProgressBoard BuildBoard(Transform furniture, Transform rack)
    {
        var go = new GameObject("Progress board", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(furniture, false);
        var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
        var rt = (RectTransform)go.transform; rt.sizeDelta = new Vector2(1900f, 360f); rt.localScale = Vector3.one * 0.001f;
        var bg = new GameObject("Glass", typeof(RectTransform), typeof(Image)).GetComponent<Image>(); bg.transform.SetParent(go.transform, false);
        var br = (RectTransform)bg.transform; br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one; br.offsetMin = Vector2.zero; br.offsetMax = Vector2.zero;
        bg.sprite = style.card; bg.type = Image.Type.Sliced; bg.color = new Color(0.086f, 0.11f, 0.173f, 0.94f); bg.raycastTarget = false;
        Text(go.transform, "Heading", "What you've built", style.displayFont, 44f, Color.white, new Vector2(-470f, 125f), new Vector2(880f, 56f), TextAlignmentOptions.Left);
        Text(go.transform, "Eyebrow", "PROGRESS", style.altBold, 20f, style.lavender, new Vector2(730f, 125f), new Vector2(300f, 34f), TextAlignmentOptions.Right);
        var board = go.AddComponent<ProgressBoard>();
        var rows = new List<ProgressBoard.Row>();
        var lessons = new[] { (ToyRackModel.Cargo, "Cargo Crew"), (ToyRackModel.Cafe, "Neighborhood Café"), (ToyRackModel.Garden, "Community Garden") };
        for (int r = 0; r < lessons.Length; r++)
        {
            float y = 45f - r * 72f;
            var row = new ProgressBoard.Row { lessonId = lessons[r].Item1 };
            row.title = Text(go.transform, "Title " + r, lessons[r].Item2, style.bodyFont, 32f, Color.white, new Vector2(-470f, y), new Vector2(700f, 48f), TextAlignmentOptions.Left);
            var marks = new List<Image>();
            for (int i = 0; i < 5; i++)
            {
                var mark = new GameObject("Mark " + i, typeof(RectTransform), typeof(Image)).GetComponent<Image>(); mark.transform.SetParent(go.transform, false);
                mark.sprite = style.pill; mark.type = Image.Type.Sliced; mark.raycastTarget = false; mark.color = board.todoColour;
                var mr = (RectTransform)mark.transform; mr.sizeDelta = new Vector2(48f, 48f); mr.anchoredPosition = new Vector2(220f + i * 66f, y);
                var tick = Text(mark.transform, "Tick", "✓", style.altBold, 28f, new Color(0.086f, 0.11f, 0.173f), Vector2.zero, new Vector2(48f, 48f), TextAlignmentOptions.Center);
                tick.gameObject.SetActive(false);
                marks.Add(mark);
            }
            row.marks = marks.ToArray();
            row.count = Text(go.transform, "Count " + r, "0 of 5", style.altFont, 26f, style.textMuted, new Vector2(730f, y), new Vector2(240f, 48f), TextAlignmentOptions.Right);
            rows.Add(row);
        }
        board.rows = rows.ToArray();
        board.Refresh(new LibraryState());
        return board;
    }

    static TMP_Text Text(Transform parent, string name, string text, TMP_FontAsset font, float size, Color colour, Vector2 pos, Vector2 sizeDelta, TextAlignmentOptions align)
    {
        var t = new GameObject(name, typeof(RectTransform)).AddComponent<TextMeshProUGUI>(); t.transform.SetParent(parent, false);
        t.text = text; t.font = font; t.fontSize = size; t.color = colour; t.alignment = align; t.raycastTarget = false; t.textWrappingMode = TextWrappingModes.NoWrap;
        t.rectTransform.anchoredPosition = pos; t.rectTransform.sizeDelta = sizeDelta;
        return t;
    }

    // ---- helpers (each AgentScript compiles alone) ----
    static GameObject Box(Transform parent, string name, Vector3 centre, Vector3 size, Material material, float radius = 0.012f)
    {
        var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false); go.transform.localPosition = centre;
        go.GetComponent<MeshFilter>().sharedMesh = RoundedBoxMesh.Build(size, Mathf.Min(radius, Mathf.Min(size.x, Mathf.Min(size.y, size.z)) / 2.2f));
        var mr = go.GetComponent<MeshRenderer>(); mr.sharedMaterial = material; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false; mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        return go;
    }

    static Material Mat(string name, Color colour, float roughness, bool emissive = false)
    {
        string path = MatDir + "/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
        mat.shader = shader; mat.color = colour; mat.SetFloat("_Smoothness", Mathf.Clamp01(1f - roughness)); mat.SetFloat("_Metallic", 0f);
        if (emissive) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", colour * 1.8f); }
        mat.enableInstancing = true; EditorUtility.SetDirty(mat);
        return mat;
    }

    static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
}
