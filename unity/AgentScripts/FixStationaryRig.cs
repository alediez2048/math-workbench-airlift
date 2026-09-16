using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class FixStationaryRig
{
    public static string Run()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.path!="Assets/Airlift/Scenes/CargoCrew.unity")
            scene=EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity",OpenSceneMode.Additive);
        if(scene.isDirty)throw new InvalidOperationException("CargoCrew has unsaved changes; preserve before editing.");
        var locomotors=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
            .Where(b=>b.gameObject.scene==scene && b.GetType().Name=="FirstPersonLocomotor").ToArray();
        if(locomotors.Length!=1)throw new InvalidOperationException("Review unexpected locomotor count: "+locomotors.Length);
        foreach(var component in locomotors)
        {
            component.enabled=false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            EditorUtility.SetDirty(component);
        }
        EditorSceneManager.SaveScene(scene);
        return "Disabled gravity locomotor through CargoCrew scene override; vendor prefab and reference scenes unchanged.";
    }
}
