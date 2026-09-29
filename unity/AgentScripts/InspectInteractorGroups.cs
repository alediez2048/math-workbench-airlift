using System.Linq;
using System.Text;
using Oculus.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Diagnostic 2026-09-29: per hand, the BestHoverInteractorGroup's member list (index = priority) and each controller
// interactor's selector / active-state wiring.
public static class InspectInteractorGroups
{
    static string Path(Transform t) { var s = t.name; while (t.parent != null) { t = t.parent; s = t.name + "/" + s; } return s; }
    static string Short(Object o) { if (o == null) return "null"; if (o is Component c) { var p = Path(c.transform); int i = p.IndexOf("Interactors/"); return (i >= 0 ? p.Substring(i + 12) : p) + ":" + c.GetType().Name; } return o.name; }
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        foreach (var g in Object.FindObjectsByType<InteractorGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var so = new SerializedObject(g);
            var list = so.FindProperty("_interactors");
            sb.AppendLine(g.GetType().Name + " @ " + Path(g.transform) + " active=" + g.gameObject.activeInHierarchy + " activeState=" + Short(so.FindProperty("_activeState")?.objectReferenceValue) + " comparer=" + Short(so.FindProperty("_candidateComparer")?.objectReferenceValue));
            for (int i = 0; list != null && i < list.arraySize; i++) sb.AppendLine("   [" + i + "] " + Short(list.GetArrayElementAtIndex(i).objectReferenceValue));
        }
        sb.AppendLine("== controller interactors: selector + active state");
        foreach (var c in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var t = c.GetType().Name;
            if (!(t == "RayInteractor" || t == "GrabInteractor" || t == "DistanceGrabInteractor")) continue;
            if (!Path(c.transform).Contains("Controller")) continue;
            var so = new SerializedObject(c);
            var tracker = c.GetComponent<ActiveStateTracker>();
            string trackerState = tracker != null ? Short(new SerializedObject(tracker).FindProperty("_activeState")?.objectReferenceValue) : "(none)";
            sb.AppendLine(t + " @ " + Short(c) + " active=" + c.gameObject.activeInHierarchy + " selector=" + Short(so.FindProperty("_selector")?.objectReferenceValue) + " activeState(field)=" + Short(so.FindProperty("_activeState")?.objectReferenceValue) + " tracker=" + trackerState + " maxSelectingInteractors=" + (so.FindProperty("_maxSelectingInteractors")?.intValue.ToString() ?? "-"));
            var sel = so.FindProperty("_selector")?.objectReferenceValue as Component;
            if (sel != null) { var ss = new SerializedObject(sel); var it = ss.GetIterator(); bool enter = true; var refs = new StringBuilder(); while (it.NextVisible(enter)) { enter = false; if (it.propertyType == SerializedPropertyType.Enum) refs.Append(it.name + "=" + it.enumDisplayNames.ElementAtOrDefault(it.enumValueIndex) + " "); else if (it.propertyType == SerializedPropertyType.Integer) refs.Append(it.name + "=" + it.intValue + " "); else if (it.propertyType == SerializedPropertyType.ObjectReference) refs.Append(it.name + "=" + Short(it.objectReferenceValue) + " "); } sb.AppendLine("      selector " + sel.GetType().Name + ": " + refs); }
        }
        return sb.ToString();
    }
}
