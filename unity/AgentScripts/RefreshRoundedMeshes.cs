using System;
using System.Globalization;
using System.IO;
using Airlift.Presentation;
using UnityEditor;
using UnityEngine;
// Rebuilds every RoundedBox mesh asset in place from the size/radius/cut flags encoded in
// its file name, so scene references survive generator fixes.
public static class RefreshRoundedMeshes
{
    public static string Run()
    {
        int refreshed = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Mesh", new[] { "Assets/Airlift/Meshes" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string key = Path.GetFileNameWithoutExtension(path);
            if (!key.StartsWith("RoundedBox-")) continue;
            var parts = key.Substring("RoundedBox-".Length).Split('-');
            var dims = parts[0].Replace("_", ".").Split('x');
            float P(string s) => float.Parse(s, CultureInfo.InvariantCulture);
            var size = new Vector3(P(dims[0]), P(dims[1]), P(dims[2]));
            float radius = P(parts[1].Substring(1).Replace("_", "."));
            bool roundMin = Array.IndexOf(parts, "cutL") < 0, roundMax = Array.IndexOf(parts, "cutR") < 0;
            var fresh = RoundedBoxMesh.Build(size, radius, 4, roundMin, roundMax);
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            mesh.Clear(); mesh.SetVertices(fresh.vertices); mesh.SetNormals(fresh.normals); mesh.SetUVs(0, fresh.uv); mesh.SetTriangles(fresh.triangles, 0);
            mesh.RecalculateBounds(); mesh.RecalculateTangents(); EditorUtility.SetDirty(mesh);
            UnityEngine.Object.DestroyImmediate(fresh); refreshed++;
        }
        AssetDatabase.SaveAssets();
        return refreshed + " rounded mesh assets rebuilt in place.";
    }
}
