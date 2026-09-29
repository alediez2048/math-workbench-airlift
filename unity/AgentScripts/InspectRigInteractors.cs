using System.Text;
using Oculus.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Diagnostic: every ray / grab / distance-grab interactor in the rig with its path, and the rack toy's ray wiring.
public static class InspectRigInteractors
{
    static string Path(Transform t) { var s = t.name; while (t.parent != null) { t = t.parent; s = t.name + "/" + s; } return s; }
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        foreach (var c in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var t = c.GetType().Name;
            if (t.EndsWith("RayInteractor") || t.EndsWith("GrabInteractor") || t.Contains("DistanceGrabInteractor") || t.Contains("DistanceHandGrabInteractor"))
                sb.AppendLine(t + " | " + Path(c.transform) + " | active=" + c.gameObject.activeInHierarchy + " enabled=" + c.enabled);
        }
        var n = Object.FindAnyObjectByType<Airlift.Welcome.NerdyDirector>(FindObjectsInactive.Include);
        var ray = n.toyRack.slots[0].toy.GetComponentInChildren<RayInteractable>(true);
        var so = new SerializedObject(ray);
        foreach (var f in new[] { "_pointableElement", "_surface", "_selectSurface", "_movementProvider" })
        { var p = so.FindProperty(f); sb.AppendLine("toy ray " + f + " = " + (p == null ? "(no field)" : p.objectReferenceValue != null ? p.objectReferenceValue.name + ":" + p.objectReferenceValue.GetType().Name : "null")); }
        return sb.ToString();
    }
}
