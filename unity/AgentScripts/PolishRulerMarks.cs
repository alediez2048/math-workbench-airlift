using System;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Ruler marks were light text on the white pad rim. Make them navy, and deepen the pad so
// the marks sit on a wider front rim where the player reads them past the docked pieces.
public static class PolishRulerMarks
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        var lesson = d.GetComponent<CargoLessonDirector>();
        var style = AssetDatabase.LoadAssetAtPath<AirliftStyle>("Assets/Airlift/Fonts/AirliftStyle.asset");
        var pad = d.station.transform.Find("Outlined placement pad");
        var filter = pad.GetComponent<MeshFilter>();
        if (filter.sharedMesh.name.Contains("0_16")) throw new InvalidOperationException("Already applied.");
        var size = new Vector3(0.36f, 0.008f, 0.16f);
        string path = "Assets/Airlift/Meshes/RoundedBox-0_36x0_008x0_16-r0_0036.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh == null) { mesh = RoundedBoxMesh.Build(size, 0.0036f); AssetDatabase.CreateAsset(mesh, path); }
        filter.sharedMesh = mesh;
        int marks = 0;
        foreach (Transform child in lesson.ruler)
        {
            if (!child.name.StartsWith("Mark")) continue;
            var text = child.GetComponent<TMP_Text>(); text.color = style.panel; text.fontSize = 0.1f;
            child.localPosition = new Vector3(child.localPosition.x, 0.004f, -0.066f); marks++;
        }
        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        return "Pad deepened to 0.16 m; " + marks + " ruler marks navy on the front rim.";
    }
}
