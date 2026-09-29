using System;
using System.Linq;
using Oculus.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 2026-09-29, same root cause as DisableDuplicateGrabInteractors (2026-09-16): the standalone "[BuildingBlock] Controller
// Interactions" block duplicates the comprehensive rig's controller interactors. Its grab interactor was disabled then;
// its ray stayed, and today's distance-grab wizard added a distance interactor to both blocks. One trigger or squeeze
// then handed a two-point toy two coincident grab points and nothing moved. Keep the comprehensive rig's; disable the extras.
// Run: unity command run_script --file AgentScripts/DisableDuplicateRayAndDistanceInteractors.cs --entry DisableDuplicateRayAndDistanceInteractors.Run
public static class DisableDuplicateRayAndDistanceInteractors
{
    static string Path(Transform t) { var s = t.name; while (t.parent != null) { t = t.parent; s = t.name + "/" + s; } return s; }
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        bool Extra(Component c) => c.gameObject.activeInHierarchy && Path(c.transform).Contains("[BuildingBlock] Controller Interactions/");
        var rays = UnityEngine.Object.FindObjectsByType<RayInteractor>(FindObjectsInactive.Include).Where(r => Extra(r) && r.name == "ControllerRayInteractor").ToArray();
        var distance = UnityEngine.Object.FindObjectsByType<DistanceGrabInteractor>(FindObjectsInactive.Include).Where(Extra).ToArray();
        foreach (var c in rays.Cast<Component>().Concat(distance)) c.gameObject.SetActive(false);
        int raysLeft = UnityEngine.Object.FindObjectsByType<RayInteractor>(FindObjectsInactive.Include).Count(r => r.gameObject.activeInHierarchy && r.name == "ControllerRayInteractor");
        int distLeft = UnityEngine.Object.FindObjectsByType<DistanceGrabInteractor>(FindObjectsInactive.Include).Count(d => d.gameObject.activeInHierarchy);
        if (raysLeft != 2 || distLeft != 2) throw new InvalidOperationException("Expected one ray and one distance grab per hand, found " + raysLeft + " rays, " + distLeft + " distance");
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "disabled " + rays.Length + " duplicate controller rays and " + distance.Length + " duplicate distance grabs; 2 + 2 remain";
    }
}
