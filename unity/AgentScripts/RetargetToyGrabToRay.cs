using System;
using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 2026-09-29, from the device probe: the cone (distance) grab on each toy outranked the ray in the hand's interactor group
// and switched the ray off while it pointed at an unlocked toy (no cursor, no trigger). The ray alone takes a toy from
// afar now, pulling it to the hand (MoveTowardsTargetProvider, travel data copied from the cone grab); the near grab stays.
// Idempotent; AddToyRack builds new racks this way already. Run after AddToyRack on an existing scene.
public static class RetargetToyGrabToRay
{
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<Airlift.Welcome.NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("no director");
        if (n.toyRack == null) throw new InvalidOperationException("no toy rack: run AddToyRack first");
        int removed = 0, retargeted = 0;
        foreach (var slot in n.toyRack.slots)
        {
            if (slot.toy == null) continue;
            var ray = slot.toy.GetComponentInChildren<RayInteractable>(true) ?? throw new InvalidOperationException(slot.id + " has no RayInteractable");
            var cone = slot.toy.GetComponentInChildren<DistanceGrabInteractable>(true);
            var so = new SerializedObject(ray);
            if (!(so.FindProperty("_movementProvider").objectReferenceValue is MoveTowardsTargetProvider))
            {
                var pull = ray.gameObject.AddComponent<MoveTowardsTargetProvider>();
                var source = cone != null ? cone.GetComponent<MoveTowardsTargetProvider>() : null;
                if (source != null) EditorUtility.CopySerialized(source, pull);   // the wizard's pull-to-hand travel data
                ray.InjectOptionalMovementProvider(pull);
                foreach (var old in ray.GetComponents<MoveFromTargetProvider>()) UnityEngine.Object.DestroyImmediate(old);
                retargeted++;
            }
            if (cone != null) { UnityEngine.Object.DestroyImmediate(cone.gameObject); removed++; }
            foreach (var h in slot.toy.GetComponentsInChildren<DistanceHandGrabInteractable>(true)) { UnityEngine.Object.DestroyImmediate(h.gameObject); removed++; }
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "retargeted " + retargeted + " toys to ray pull-to-hand, removed " + removed + " cone-grab objects";
    }
}
