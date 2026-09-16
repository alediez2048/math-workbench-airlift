using System;
using Airlift.Presentation;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RefineCargoTerminal
{
    public static string Run()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.path!="Assets/Airlift/Scenes/CargoCrew.unity" || scene.isDirty) throw new InvalidOperationException("Open clean CargoCrew first.");
        var terminal=UnityEngine.Object.FindAnyObjectByType<CargoTerminalView>();
        foreach(var label in terminal.GetComponentsInChildren<TextMeshPro>())
        {
            label.fontSize=0.14f;label.textWrappingMode=TextWrappingModes.NoWrap;
            label.ForceMeshUpdate();
            if(label.isTextOverflowing)throw new InvalidOperationException("Destination tag overflow: "+label.name);
        }
        EditorSceneManager.SaveScene(scene);
        return "Resized destination tags to world-space label scale; no overflow.";
    }
}
