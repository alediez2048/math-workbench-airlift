using System;
using System.Linq;
using Airlift.Onboarding;
using Airlift.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// CC-P0-03 THROWAWAY: a copy of CargoCrew with the lesson hidden and a status panel + spike component.
public static class CreateRealtimeSpike
{
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var scene = SceneManager.GetActiveScene();
        if (!EditorSceneManager.SaveScene(scene, "Assets/Airlift/Scenes/RealtimeSpike.unity", false)) throw new InvalidOperationException("Could not save spike scene copy.");
        scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/RealtimeSpike.unity", OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        var head = d.head;
        var style = AssetDatabase.LoadAssetAtPath<AirliftStyle>("Assets/Airlift/Fonts/AirliftStyle.asset");
        d.gameObject.SetActive(false);
        var panel = new GameObject("Spike panel"); panel.transform.SetParent(head, false);
        panel.transform.localPosition = new Vector3(0, -0.1f, 1.0f);
        var bg = GameObject.CreatePrimitive(PrimitiveType.Quad); bg.name = "Backdrop"; UnityEngine.Object.DestroyImmediate(bg.GetComponent<Collider>());
        bg.transform.SetParent(panel.transform, false); bg.transform.localScale = new Vector3(0.9f, 0.5f, 1);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = new Color(0.09f, 0.11f, 0.17f, 1f) }; bg.GetComponent<Renderer>().sharedMaterial = mat;
        TMP_Text Make(string name, Vector3 pos, float size, Vector2 rect)
        {
            var go = new GameObject(name); go.transform.SetParent(panel.transform, false); go.transform.localPosition = pos;
            var t = go.AddComponent<TextMeshPro>(); t.font = style.bodyFont; t.fontSize = size; t.color = Color.white; t.alignment = TextAlignmentOptions.TopLeft;
            t.rectTransform.sizeDelta = rect; t.textWrappingMode = TextWrappingModes.Normal; return t;
        }
        var spike = panel.AddComponent<RealtimeSpike>();
        spike.status = Make("Status", new Vector3(0, 0.16f, -0.01f), 0.6f, new Vector2(0.84f, 0.16f));
        spike.transcript = Make("Transcript", new Vector3(0, -0.02f, -0.01f), 0.5f, new Vector2(0.84f, 0.3f));
        spike.status.text = "Nerdy voice spike"; spike.transcript.text = "";
        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        return "RealtimeSpike.unity saved (CargoCrew copy, lesson hidden, panel + RealtimeSpike on head).";
    }
}
