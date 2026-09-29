using System.Linq;
using System.Text;
using Oculus.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Diagnostic 2026-09-29: the comprehensive rig's distance grab reach/cone and the toy's distance-grab interactable wiring.
public static class InspectDistanceGrab
{
    static string Dump(Object o)
    {
        if (o == null) return "null"; var so = new SerializedObject(o); var it = so.GetIterator(); bool enter = true; var sb = new StringBuilder(o.GetType().Name + ": ");
        while (it.NextVisible(enter)) { enter = false; if (it.name == "m_Script") continue;
            switch (it.propertyType) {
                case SerializedPropertyType.Float: sb.Append(it.name + "=" + it.floatValue.ToString("F2") + " "); break;
                case SerializedPropertyType.Integer: sb.Append(it.name + "=" + it.intValue + " "); break;
                case SerializedPropertyType.Boolean: sb.Append(it.name + "=" + it.boolValue + " "); break;
                case SerializedPropertyType.Enum: sb.Append(it.name + "=" + it.enumDisplayNames.ElementAtOrDefault(it.enumValueIndex) + "(" + it.intValue + ") "); break;
                case SerializedPropertyType.LayerMask: sb.Append(it.name + "=mask" + it.intValue + " "); break;
                case SerializedPropertyType.ObjectReference: sb.Append(it.name + "=" + (it.objectReferenceValue == null ? "null" : it.objectReferenceValue.name + ":" + it.objectReferenceValue.GetType().Name) + " "); break; } }
        return sb.ToString();
    }
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        var d = Object.FindObjectsByType<DistanceGrabInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None).First(x => x.gameObject.activeInHierarchy);
        sb.AppendLine("interactor " + Dump(d));
        foreach (var c in d.GetComponents<Component>()) if (!(c is Transform) && !(c is DistanceGrabInteractor)) sb.AppendLine("   + " + Dump(c));
        foreach (var c in d.GetComponentsInChildren<Component>(true)) if (c.transform != d.transform && !(c is Transform)) sb.AppendLine("   child " + c.transform.name + " " + Dump(c));
        var g = Object.FindObjectsByType<GrabInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None).First(x => x.gameObject.activeInHierarchy);
        sb.AppendLine("near grab " + Dump(g));
        foreach (var c in g.GetComponentsInChildren<Component>(true)) if (!(c is Transform) && !(c is GrabInteractor)) sb.AppendLine("   " + c.transform.name + " " + Dump(c));
        var n = Object.FindAnyObjectByType<Airlift.Welcome.NerdyDirector>(FindObjectsInactive.Include);
        var toy = n.toyRack.slots[0].toy;
        sb.AppendLine("toy " + toy.name + " layer=" + toy.layer + " children=" + string.Join(", ", toy.GetComponentsInChildren<Transform>(true).Select(t => t.name + "(" + string.Join("+", t.GetComponents<Component>().Select(c => c.GetType().Name)) + ")")));
        foreach (var c in toy.GetComponentsInChildren<Component>(true)) { var tn = c.GetType().Name; if (tn.Contains("Interactable") || tn == "Grabbable" || tn.Contains("Transformer") || tn.Contains("Collider") || tn == "Rigidbody" || tn.Contains("Surface")) sb.AppendLine("   " + c.transform.name + " " + Dump(c)); }
        return sb.ToString();
    }
}
