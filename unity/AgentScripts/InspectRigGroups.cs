using System.Linq;
using System.Text;
using Oculus.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Diagnostic 2026-09-29: how the rig decides which interactor group is live, what the deactivated block carried, and
// which RayInteractor the visible ray/cursor visuals follow.
public static class InspectRigGroups
{
    static string Path(Transform t) { var s = t.name; while (t.parent != null) { t = t.parent; s = t.name + "/" + s; } return s; }
    static string Ref(SerializedProperty p) => p == null ? "(no field)" : p.objectReferenceValue == null ? "null" : (p.objectReferenceValue is Component c ? Path(c.transform) + ":" + c.GetType().Name : p.objectReferenceValue.name);
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        var all = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine("== visuals / cursors / canvas module");
        foreach (var c in all)
        {
            var t = c.GetType().Name;
            if (t.Contains("Visual") && (t.Contains("Ray") || t.Contains("Cursor")) || t == "PointableCanvasModule" || t.Contains("CanvasCursor"))
            {
                var so = new SerializedObject(c);
                var refs = new StringBuilder();
                var it = so.GetIterator(); bool enter = true;
                while (it.NextVisible(enter)) { enter = false; if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue is Component) refs.Append(it.name + "=" + Ref(it) + " "); }
                sb.AppendLine(t + " | " + Path(c.transform) + " | active=" + c.gameObject.activeInHierarchy + " | " + refs);
            }
        }
        sb.AppendLine("== group activation (ActiveState*) under the comprehensive interactors and the old block");
        foreach (var c in all)
        {
            var t = c.GetType().Name; var p = Path(c.transform);
            if (!(p.Contains("ComprehensiveInteractors") || p.Contains("[BuildingBlock] Controller Interactions"))) continue;
            if (t.Contains("ActiveState") || t.Contains("Selector") || t.Contains("Toggle") || t.Contains("Tracker") || t.Contains("InteractorGroup"))
            {
                var so = new SerializedObject(c);
                var refs = new StringBuilder();
                var it = so.GetIterator(); bool enter = true;
                while (it.NextVisible(enter)) { enter = false; if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue is Component) refs.Append(it.name + "=" + Ref(it) + " "); else if (it.propertyType == SerializedPropertyType.Enum || it.propertyType == SerializedPropertyType.Integer || it.propertyType == SerializedPropertyType.Boolean) refs.Append(it.name + "=" + (it.propertyType == SerializedPropertyType.Enum ? it.enumDisplayNames.ElementAtOrDefault(it.enumValueIndex) : it.propertyType == SerializedPropertyType.Boolean ? it.boolValue.ToString() : it.intValue.ToString()) + " "); }
                sb.AppendLine(t + " | " + p + " | active=" + c.gameObject.activeInHierarchy + " | " + refs);
            }
        }
        sb.AppendLine("== components on the old block's controller ray GOs and the comprehensive controller ray GOs");
        foreach (var r in Object.FindObjectsByType<RayInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!r.name.StartsWith("Controller")) continue;
            sb.AppendLine(Path(r.transform) + " active=" + r.gameObject.activeInHierarchy + " children=" + r.transform.childCount + " comps=" + string.Join(",", r.GetComponents<Component>().Select(x => x.GetType().Name)) + " childComps=" + string.Join(",", r.GetComponentsInChildren<Component>(true).Where(x => x.transform != r.transform).Select(x => x.GetType().Name).Distinct()));
            var so = new SerializedObject(r);
            foreach (var f in new[] { "_selector", "_rayOrigin", "_maxRayLength", "_rayInteractableLayers" }) { var p = so.FindProperty(f); if (p == null) continue; sb.AppendLine("   " + f + " = " + (p.propertyType == SerializedPropertyType.ObjectReference ? Ref(p) : p.propertyType == SerializedPropertyType.Float ? p.floatValue.ToString() : p.propertyType == SerializedPropertyType.LayerMask ? p.intValue.ToString() : p.propertyType.ToString())); }
        }
        return sb.ToString();
    }
}
