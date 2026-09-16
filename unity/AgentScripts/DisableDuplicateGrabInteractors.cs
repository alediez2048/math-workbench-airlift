using System;
using System.Linq;
using Oculus.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Root cause of the table-handle "jump / no rotation" report (2026-09-16): CargoCrew has two
// active ControllerGrabInteractors per hand (comprehensive rig + a standalone Controller
// Interactions block), so one squeeze produced two grab points and the two-hand transformer
// ran with coincident points. Keep the comprehensive rig's interactors; disable the extras.
public static class DisableDuplicateGrabInteractors
{
    static string Path(Transform t) { var s = t.name; while (t.parent != null) { t = t.parent; s = t.name + "/" + s; } return s; }
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var extras = UnityEngine.Object.FindObjectsByType<GrabInteractor>(FindObjectsInactive.Include)
            .Where(g => g.gameObject.activeInHierarchy && Path(g.transform).Contains("[BuildingBlock] Controller Interactions/")).ToArray();
        if (extras.Length != 2) throw new InvalidOperationException("Expected two duplicate controller grab interactors, found " + extras.Length);
        foreach (var g in extras) g.gameObject.SetActive(false);
        int remaining = UnityEngine.Object.FindObjectsByType<GrabInteractor>(FindObjectsInactive.Include).Count(g => g.gameObject.activeInHierarchy);
        if (remaining != 2) throw new InvalidOperationException("Expected two active grab interactors after fix, found " + remaining);
        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        return "Disabled " + string.Join(", ", extras.Select(e => Path(e.transform))) + "; 2 active controller grab interactors remain (one per hand).";
    }
}
