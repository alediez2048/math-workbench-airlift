using System.Linq;
using System.Text;
using Airlift.Lounge;
using UnityEditor.SceneManagement;
using UnityEngine;
// Read-only: UV ranges of the room meshes that carry tiled textures.
public static class InspectRoomUVs
{
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var shell = Object.FindAnyObjectByType<LoungeRoom>(FindObjectsInactive.Include).shell.transform;
        var sb = new StringBuilder();
        foreach (var n in new[] { "Wall 0", "Floor" })
        {
            var m = shell.Find(n).GetComponent<MeshFilter>().sharedMesh;
            var uv = m.uv; var v = m.vertices; var nr = m.normals;
            sb.AppendLine(n + " mesh=" + m.name + " verts=" + v.Length + " uv u " + uv.Min(p => p.x).ToString("F2") + ".." + uv.Max(p => p.x).ToString("F2") + " v " + uv.Min(p => p.y).ToString("F2") + ".." + uv.Max(p => p.y).ToString("F2"));
            // UV spread on the face that points into the room (-z for walls, +y for floor)
            var dir = n == "Floor" ? Vector3.up : Vector3.back;
            var face = Enumerable.Range(0, v.Length).Where(i => Vector3.Dot(nr[i], dir) > 0.9f).ToArray();
            if (face.Length > 0) sb.AppendLine("   face verts " + face.Length + " u " + face.Min(i => uv[i].x).ToString("F2") + ".." + face.Max(i => uv[i].x).ToString("F2") + " v " + face.Min(i => uv[i].y).ToString("F2") + ".." + face.Max(i => uv[i].y).ToString("F2") + " x " + face.Min(i => v[i].x).ToString("F2") + ".." + face.Max(i => v[i].x).ToString("F2"));
            var mat = shell.Find(n).GetComponent<MeshRenderer>().sharedMaterial;
            sb.AppendLine("   mat " + mat.name + " ST " + mat.GetVector("_BaseMap_ST") + " scale " + shell.Find(n).lossyScale);
        }
        return sb.ToString();
    }
}
