using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class CreateCargoCrew
{
    public static string Run()
    {
        const string source = "Assets/Airlift/Scenes/Onboarding.unity";
        const string destination = "Assets/Airlift/Scenes/CargoCrew.unity";
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Save or review dirty scenes before creating CargoCrew.");
        if (!File.Exists(source)) throw new FileNotFoundException("Onboarding baseline missing.");
        if (File.Exists(destination)) return "CargoCrew already exists; no files changed.";
        if (!AssetDatabase.CopyAsset(source, destination))
            throw new InvalidOperationException("Could not copy onboarding baseline.");
        AssetDatabase.ImportAsset(destination);
        return "Created CargoCrew from Onboarding without changing either reference scene or the open scene.";
    }
}
