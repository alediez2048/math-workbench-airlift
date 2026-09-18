using System;
using Airlift.Welcome;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
// Owner 2026-09-18: "this floating sphere is everywhere, delete it entirely across the entire experience."
// Deletes Dee's orb from the bar and switches the controller helper (its glow sphere) off. Saves CargoCrew.
public static class RemoveOrb
{
    public static string Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        int removed = 0;
        if (n.orb != null) { UnityEngine.Object.DestroyImmediate(n.orb.gameObject); n.orb = null; removed++; }
        var stray = n.hudRoot.transform.Find("Orb"); if (stray != null) { UnityEngine.Object.DestroyImmediate(stray.gameObject); removed++; }
        var helper = n.transform.Find("Controller helper"); if (helper != null && helper.gameObject.activeSelf) helper.gameObject.SetActive(false);
        foreach (var go in scene.GetRootGameObjects())
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
                if (t.name == "Button glow" || t.name == "Dee glow" || t.name == "Dee seat") { t.gameObject.SetActive(false); }
        EditorUtility.SetDirty(n);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "orb objects deleted: " + removed + "; controller helper off";
    }
}
