using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Diagnostic 2026-09-29: puts the InteractionProbe logger in the scene (idempotent). Remove with RemoveInteractionProbe.cs.
public static class AddInteractionProbe
{
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var go = GameObject.Find("Interaction probe");
        if (go == null) { go = new GameObject("Interaction probe"); go.AddComponent<Airlift.Diagnostics.InteractionProbe>(); }
        else if (go.GetComponent<Airlift.Diagnostics.InteractionProbe>() == null) go.AddComponent<Airlift.Diagnostics.InteractionProbe>();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "probe in scene";
    }
}
