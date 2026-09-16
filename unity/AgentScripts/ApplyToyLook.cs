using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Owner request 2026-09-16: centre the measuring pad, remove the station controls, and
// give every board object a playful toy look (rounded blocks, satin plastic, warm light,
// pill buttons). CargoCrew only; Onboarding stays as the regression reference.
public static class ApplyToyLook
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string MeshFolder = "Assets/Airlift/Meshes";
    static readonly Vector3 Target = new Vector3(0f, 0.08f, 0.02f);
    static readonly Vector3 Tray = new Vector3(-0.13f, 0.08f, -0.17f);
    static readonly Dictionary<string, Mesh> meshCache = new Dictionary<string, Mesh>();

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        var lesson = d != null ? d.GetComponent<CargoLessonDirector>() : null;
        if (d == null || lesson == null) throw new InvalidOperationException("CargoCrew director or fraction chapter missing.");
        if (d.transform.Find("Lesson interface/Raise station") == null) throw new InvalidOperationException("Toy look already applied; refusing to run twice.");
        var style = AssetDatabase.LoadAssetAtPath<AirliftStyle>("Assets/Airlift/Fonts/AirliftStyle.asset");
        var report = new List<string>();

        // 1. Layout: pad and ruler at the board centre, tray row forward and clear of the pad.
        d.content.targetPosition = Target;
        d.content.trayPosition = Tray;
        d.content.orientation = d.content.orientation.Replace("Adjust the station below if needed. ", "").Replace("Adjust the station below if needed.", "");
        EditorUtility.SetDirty(d.content);
        foreach (Transform child in d.station.transform) child.localPosition = new Vector3(Target.x, child.localPosition.y, Target.z);
        lesson.ruler.localPosition = new Vector3(Target.x, lesson.ruler.localPosition.y, Target.z);
        d.strap.localPosition = Tray; d.demonstrationStrap.localPosition = Tray;
        lesson.whole.trayPosition = Tray;
        lesson.halfA.trayPosition = Tray + new Vector3(-0.08f, 0, 0);
        lesson.halfB.trayPosition = Tray + new Vector3(0.08f, 0, 0);
        foreach (var view in new[] { lesson.whole, lesson.halfA, lesson.halfB }) view.piece.localPosition = view.trayPosition;
        report.Add("pad/ruler at " + Target + ", tray at " + Tray);

        // 2. Station controls removed; placement guidance moves onto the card.
        var ui = d.transform.Find("Lesson interface");
        int removed = 0;
        foreach (var name in new[] { "Lower station", "Recenter", "Raise station" })
        {
            var control = ui.Find(name); if (control == null) continue;
            UnityEngine.Object.DestroyImmediate(control.gameObject); removed++;
        }
        var placement = d.GetComponent<ComfortPlacement>();
        if (placement != null && placement.statusText != null)
        {
            var rect = placement.statusText.rectTransform; rect.anchoredPosition = new Vector2(265, -226); rect.sizeDelta = new Vector2(380, 36);
            placement.statusText.fontSize = 18;
        }
        report.Add(removed + " station controls removed");

        // 3. Materials: satin toy plastic in a warmer, brighter family.
        Tune("CargoNavy", new Color32(58, 78, 118, 255), 0.35f);
        Tune("CargoTeal", new Color32(88, 222, 190, 255), 0.5f);
        Tune("CargoOrange", new Color32(255, 168, 78, 255), 0.5f);
        Tune("CargoCream", new Color32(250, 247, 238, 255), 0.45f);
        Tune("CargoRubber", new Color32(40, 50, 78, 255), 0.3f);
        var yellow = Tune("CargoYellow", new Color32(255, 214, 92, 255), 0.5f);
        var cream = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/CargoCream.mat");
        foreach (var r in d.GetComponentsInChildren<Renderer>(true))
            if (r.name == "Tape" || r.name == "Tail fin") r.sharedMaterial = yellow;

        // 4. Fraction label stickers before the mesh pass so they get rounded too.
        foreach (var view in new[] { lesson.whole, lesson.halfA, lesson.halfB })
        {
            var sticker = GameObject.CreatePrimitive(PrimitiveType.Cube); sticker.name = "Sticker";
            UnityEngine.Object.DestroyImmediate(sticker.GetComponent<Collider>());
            sticker.transform.SetParent(view.piece, false);
            sticker.transform.localPosition = new Vector3(0, 0.0285f, 0); sticker.transform.localScale = new Vector3(0.052f, 0.002f, 0.048f);
            sticker.GetComponent<Renderer>().sharedMaterial = cream;
            var label = view.label.transform; label.localPosition = new Vector3(0, 0.0299f, 0);
        }

        // 5. Rounded geometry: every raw cube becomes a rounded block with exact size.
        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Application.dataPath), MeshFolder));
        var filters = d.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null && f.sharedMesh.name == "Cube").ToList();
        int rounded = 0;
        foreach (var filter in filters)
        {
            var t = filter.transform; Vector3 size = t.localScale;
            if (size.x <= 0 || size.y <= 0 || size.z <= 0) continue;
            bool roundMin = true, roundMax = true; float radius = Mathf.Min(0.012f, 0.45f * Mathf.Min(size.x, Mathf.Min(size.y, size.z)));
            if (t.name == "Body" && t.parent == lesson.halfA.piece) roundMax = false;   // cut face on the right
            if (t.name == "Body" && t.parent == lesson.halfB.piece) roundMin = false;   // cut face on the left
            if (t.name == "Body" || t == d.strap || t == d.demonstrationStrap) radius = 0.014f;
            if (t.name == "Workbench") radius = 0.016f;
            if (t.name == "Carton" || t.name == "Cargo box" || t.name == "Cab" || t.name == "Shell") radius = 0.012f;
            filter.sharedMesh = RoundedMesh(size, radius, roundMin, roundMax);
            // Children and colliders keep their world geometry when the scale collapses to 1.
            foreach (Transform child in t) { child.localPosition = Vector3.Scale(child.localPosition, size); child.localScale = Vector3.Scale(child.localScale, size); }
            var box = t.GetComponent<BoxCollider>();
            if (box != null) { box.size = Vector3.Scale(box.size, size); box.center = Vector3.Scale(box.center, size); }
            t.localScale = Vector3.one;
            var renderer = filter.GetComponent<MeshRenderer>();
            if (renderer != null) { renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; renderer.receiveShadows = true; }
            rounded++;
        }
        report.Add(rounded + " cubes rounded into " + meshCache.Count + " mesh assets");

        // 6. Warm key light plus sky-to-ground ambient so blocks read as solid objects.
        var light = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(l => l.type == LightType.Directional);
        if (light != null)
        {
            light.color = new Color32(255, 243, 224, 255); light.intensity = 1.15f;
            light.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
            light.shadows = LightShadows.Soft; light.shadowStrength = 0.55f;
        }
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.80f, 0.84f, 0.92f);
        RenderSettings.ambientEquatorColor = new Color(0.56f, 0.60f, 0.70f);
        RenderSettings.ambientGroundColor = new Color(0.30f, 0.33f, 0.44f);
        report.Add(light != null ? "key light warmed, soft shadows, trilight ambient" : "WARNING no directional light");

        // 7. Pill buttons and rounded cards.
        var sprite = RoundedSprite();
        int imaged = 0;
        foreach (var image in ui.GetComponentsInChildren<Image>(true))
        {
            if (image.color.a <= 0.01f) continue;
            image.sprite = sprite; image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 1f; imaged++;
        }
        var backdrop = ui.Find("Backdrop")?.GetComponent<Image>();
        if (backdrop != null) backdrop.color = new Color32(30, 44, 74, 244);
        report.Add(imaged + " panels/buttons rounded");

        // Typography inventory stays explicit for the layout test.
        var binding = d.GetComponent<TypographyBindings>();
        if (binding != null) { binding.bodies = d.GetComponentsInChildren<TMP_Text>(true).Except(binding.headings).ToArray(); binding.Apply(); }

        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        return string.Join("; ", report);
    }

    static Material Tune(string name, Color color, float smoothness)
    {
        string path = "Assets/Airlift/Materials/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name }; AssetDatabase.CreateAsset(m, path); }
        m.color = color; m.SetFloat("_Smoothness", smoothness); m.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(m); return m;
    }

    static Mesh RoundedMesh(Vector3 size, float radius, bool roundMin, bool roundMax)
    {
        string key = size.x.ToString("0.####") + "x" + size.y.ToString("0.####") + "x" + size.z.ToString("0.####") + "-r" + radius.ToString("0.####") + (roundMin ? "" : "-cutL") + (roundMax ? "" : "-cutR");
        if (meshCache.TryGetValue(key, out var cached)) return cached;
        string path = MeshFolder + "/RoundedBox-" + key.Replace(".", "_") + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null)
        {
            mesh = RoundedBoxMesh.Build(size, radius, 4, roundMin, roundMax);
            AssetDatabase.CreateAsset(mesh, path);
        }
        meshCache[key] = mesh; return mesh;
    }

    static Sprite RoundedSprite()
    {
        const int size = 96, corner = 30; string path = "Assets/Airlift/Sprites/RoundedRect.png";
        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Application.dataPath), "Assets/Airlift/Sprites"));
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float cx = Mathf.Clamp(x + 0.5f, corner, size - corner), cy = Mathf.Clamp(y + 0.5f, corner, size - corner);
            float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
            float alpha = Mathf.Clamp01(corner - dist + 0.5f);
            tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
        }
        tex.Apply();
        File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath), path), tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = new Vector4(corner, corner, corner, corner); importer.spritePixelsPerUnit = 100;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? throw new InvalidOperationException("Rounded sprite import failed.");
    }
}
