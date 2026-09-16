using System;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
// Preview-driven contrast fixes: ruler ticks/marks were cream-on-cream and navy-on-navy.
public static class FixFractionChapterLook
{
    public static string Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        var lesson = d.GetComponent<CargoLessonDirector>();
        var style = AssetDatabase.LoadAssetAtPath<AirliftStyle>("Assets/Airlift/Fonts/AirliftStyle.asset");
        var navy = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/CargoNavy.mat");
        int ticks = 0, marks = 0;
        foreach (Transform child in lesson.ruler)
        {
            if (child.name.StartsWith("Tick")) { child.GetComponent<Renderer>().sharedMaterial = navy; ticks++; }
            if (child.name.StartsWith("Mark")) { child.GetComponent<TMP_Text>().color = style.text; marks++; }
        }
        var reset = lesson.resetButton.GetComponent<RectTransform>();
        reset.anchoredPosition = new Vector2(-265, -232); reset.sizeDelta = new Vector2(250, 46);
        EditorSceneManager.SaveScene(scene);
        return "Ruler ticks navy (" + ticks + "), marks light (" + marks + "), Reset button repositioned.";
    }
}
