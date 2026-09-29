using System;
using System.IO;
using Airlift.Lounge;
using Airlift.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// CC-HF-03. The Nerdy lounge as a finished room: fluted acoustic panels on the solid walls and a herringbone floor
// (Higgsfield swatches made tileable by art_tools.detail_map, grey, so the token tints still set the palette), and a
// darker lower panel with a lavender rail on every solid wall, echoing the board's rim. Sills, lintels, ceiling and
// cove keep LoungeWall. Run after BuildLounge (which rebuilds the shell). Idempotent. Saves CargoCrew.
// Sources: artifacts/higgsfield/room/{wall,floor}.png (see the generation log).
// Run: unity command run_script --file AgentScripts/ApplyRoomFinish.cs --entry ApplyRoomFinish.Run
public static class ApplyRoomFinish
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string ArtDir = "Assets/Airlift/Art/Generated/Room/";
    const string MatDir = "Assets/Airlift/Materials/Lounge/";
    const float FluteTile = 0.8f, FloorTile = 1.3f, PanelHeight = 0.95f;

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        var wallTex = Import("Wall.png", "wall.png", 512);
        var floorTex = Import("Floor.png", "floor.png", 512);

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var room = UnityEngine.Object.FindAnyObjectByType<LoungeRoom>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("LoungeRoom missing: run BuildLounge.");
        var shell = room.shell != null ? room.shell.transform : throw new InvalidOperationException("No shell.");

        Transform anyWall = null; for (int i = 0; i < 12 && anyWall == null; i++) anyWall = shell.Find("Wall " + i);
        var wallSize = anyWall.GetComponent<MeshFilter>().sharedMesh.bounds.size;

        // The texture averages 0.86, so the tint is lifted to land on the same wall value as before.
        var panelMat = Mat("LoungeWallPanel", Lerp(style.baseColor, Color.white, 0.2f), 0.1f);
        panelMat.SetTexture("_BaseMap", wallTex);
        // RoundedBoxMesh lays its faces out in a cross, so the room-facing face spans only part of UV space; tile per
        // metre of that face, not of the whole unwrap.
        var span = FaceUvSpan(anyWall.GetComponent<MeshFilter>().sharedMesh, Vector3.back);
        panelMat.SetTextureScale("_BaseMap", new Vector2(wallSize.x / FluteTile / span.x, wallSize.y / FluteTile / span.y));

        var floor = shell.Find("Floor");
        var floorMat = floor.GetComponent<MeshRenderer>().sharedMaterial;   // LoungeFloor
        float floorWidth = floor.GetComponent<MeshRenderer>().bounds.size.x;
        floorMat.SetColor("_BaseColor", Lerp(style.baseColor, Color.white, 0.1f));
        floorMat.SetTexture("_BaseMap", floorTex);
        floorMat.SetTextureScale("_BaseMap", new Vector2(floorWidth / FloorTile, floorWidth / FloorTile));
        floorMat.SetFloat("_Smoothness", 0.12f);
        EditorUtility.SetDirty(floorMat);

        var wainscot = Mat("LoungeWainscot", Lerp(style.baseColor, Color.black, 0.18f), 0.38f);
        var rail = Mat("LoungeRail", Lerp(style.lavender, Color.white, 0.08f), 0.72f);

        int walls = 0;
        for (int i = 0; i < 12; i++)
        {
            var wall = shell.Find("Wall " + i); if (wall == null) continue;
            wall.GetComponent<MeshRenderer>().sharedMaterial = panelMat;
            var inward = -wall.forward;
            var foot = new Vector3(wall.position.x, 0f, wall.position.z) + inward * (wallSize.z / 2f);
            Slab(shell, "Wainscot " + i, foot + inward * 0.015f + Vector3.up * (PanelHeight / 2f), wall.rotation, new Vector3(wallSize.x * 0.985f, PanelHeight, 0.03f), wainscot);
            Slab(shell, "Rail " + i, foot + inward * 0.03f + Vector3.up * PanelHeight, wall.rotation, new Vector3(wallSize.x * 0.985f, 0.024f, 0.045f), rail);
            walls++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return walls + " walls panelled (" + wallSize.x.ToString("F2") + " m wide), floor " + floorWidth.ToString("F1") + " m herringbone";
    }

    static Vector2 FaceUvSpan(Mesh mesh, Vector3 normal)
    {
        var uv = mesh.uv; var n = mesh.normals;
        float u0 = 1f, u1 = 0f, v0 = 1f, v1 = 0f;
        for (int i = 0; i < uv.Length; i++)
            if (Vector3.Dot(n[i], normal) > 0.9f) { u0 = Mathf.Min(u0, uv[i].x); u1 = Mathf.Max(u1, uv[i].x); v0 = Mathf.Min(v0, uv[i].y); v1 = Mathf.Max(v1, uv[i].y); }
        return new Vector2(Mathf.Max(0.05f, u1 - u0), Mathf.Max(0.05f, v1 - v0));
    }

    static Color Lerp(Color a, Color b, float t) => Color.Lerp(a, b, t);

    static void Slab(Transform parent, string name, Vector3 at, Quaternion rot, Vector3 size, Material mat)
    {
        var t = parent.Find(name);
        if (t == null) { var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false); t = go.transform; }
        t.SetPositionAndRotation(at, rot); t.localScale = Vector3.one;
        t.GetComponent<MeshFilter>().sharedMesh = RoundedBoxMesh.Build(size, Mathf.Min(size.y, size.z) * 0.4f);
        var mr = t.GetComponent<MeshRenderer>(); mr.sharedMaterial = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
    }

    static Texture2D Import(string file, string source, int cap)
    {
        string src = Path.GetFullPath(Path.Combine(Application.dataPath, "../../artifacts/higgsfield/room/" + source));
        string dst = ArtDir + file;
        Directory.CreateDirectory(ArtDir);
        if (!File.Exists(dst)) { if (!File.Exists(src)) throw new InvalidOperationException(src + " missing"); File.Copy(src, dst); }
        AssetDatabase.ImportAsset(dst, ImportAssetOptions.ForceUpdate);
        var imp = (TextureImporter)AssetImporter.GetAtPath(dst);
        imp.textureType = TextureImporterType.Default; imp.sRGBTexture = true; imp.mipmapEnabled = true;
        imp.wrapMode = TextureWrapMode.Repeat; imp.anisoLevel = 4; imp.maxTextureSize = cap; imp.isReadable = false;
        imp.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true, maxTextureSize = cap, format = TextureImporterFormat.ASTC_6x6 });
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(dst);
    }

    static Material Mat(string name, Color colour, float smoothness)
    {
        string path = MatDir + name + ".mat";
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
        mat.shader = shader; mat.SetColor("_BaseColor", colour); mat.color = colour;
        mat.SetFloat("_Smoothness", smoothness); mat.SetFloat("_Metallic", 0f); mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
