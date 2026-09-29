using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Removes the InteractionProbe diagnostic added by AddInteractionProbe.cs (idempotent).
public static class RemoveInteractionProbe
{
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var go = GameObject.Find("Interaction probe");
        if (go == null) return "no probe in scene";
        UnityEngine.Object.DestroyImmediate(go);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "probe removed";
    }
}
