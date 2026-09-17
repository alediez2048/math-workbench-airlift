using Airlift.Onboarding;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class ListBoardProps
{
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var d = Object.FindAnyObjectByType<OnboardingDirector>(FindObjectsInactive.Include); var root = d.transform; var sb = new System.Text.StringBuilder();
        void Row(Transform t) { var b = new Bounds(); bool has = false; foreach (var r in t.GetComponentsInChildren<Renderer>(true)) { var lb = new Bounds(root.InverseTransformPoint(r.bounds.center), r.bounds.size); if (!has) { b = lb; has = true; } else b.Encapsulate(lb); } sb.AppendLine(t.name + " center=" + b.center.ToString("F3") + " size=" + b.size.ToString("F3") + " active=" + t.gameObject.activeSelf); }
        foreach (var name in new[] { "Workbench", "Cargo terminal", "Vehicle bay" }) { var t = FindDeep(root, name); if (t == null) { sb.AppendLine(name + " missing"); continue; } Row(t); foreach (Transform c in t) Row(c); }
        return sb.ToString();
    }
    static Transform FindDeep(Transform t, string n) { if (t.name == n) return t; foreach (Transform c in t) { var f = FindDeep(c, n); if (f != null) return f; } return null; }
}
