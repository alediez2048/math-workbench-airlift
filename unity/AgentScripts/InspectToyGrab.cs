using System.Linq;
using System.Text;
using Airlift.Lounge;
using Airlift.Welcome;
using Oculus.Interaction;
using UnityEditor.SceneManagement;
using UnityEngine;

// Diagnostic: what is on a rack toy and what the rig's ray interactors can reach. Read-only.
public static class InspectToyGrab
{
    public static string Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var sb = new StringBuilder();
        var slot = n.toyRack.slots[0];
        sb.AppendLine("toy " + slot.id + " active=" + slot.toy.activeInHierarchy + " layer=" + LayerMask.LayerToName(slot.toy.layer) + " scale=" + slot.toy.transform.localScale.x);
        foreach (var c in slot.toy.GetComponentsInChildren<Component>(true))
            if (c != null && !(c is Transform) && !(c is MeshFilter) && !(c is MeshRenderer)) sb.AppendLine("  " + c.gameObject.name + " : " + c.GetType().Name + (c is Behaviour b ? " enabled=" + b.enabled : "") + (c is Collider col ? " colliderEnabled=" + col.enabled + " size=" + (col as BoxCollider)?.size : ""));
        var rays = slot.toy.GetComponentsInChildren<RayInteractable>(true);
        foreach (var r in rays) sb.AppendLine("  RayInteractable: pointable=" + (r.PointableElement != null ? r.PointableElement.GetType().Name : "null") + " surface=" + (r.Surface != null ? r.Surface.GetType().Name : "null"));
        var grabbables = slot.toy.GetComponentsInChildren<Grabbable>(true);
        sb.AppendLine("  grabbables=" + grabbables.Length + " maxGrabPoints=" + string.Join(",", grabbables.Select(g => g.MaxGrabPoints)));
        foreach (var ri in Object.FindObjectsByType<RayInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            sb.AppendLine("rig " + ri.gameObject.name + " active=" + ri.gameObject.activeInHierarchy + " enabled=" + ri.enabled + " maxRay=" + ri.MaxRayLength);
        foreach (var di in Object.FindObjectsByType<DistanceGrabInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            sb.AppendLine("rig " + di.gameObject.name + " active=" + di.gameObject.activeInHierarchy + " enabled=" + di.enabled);
        return sb.ToString();
    }
}
