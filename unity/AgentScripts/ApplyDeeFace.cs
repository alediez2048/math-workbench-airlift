using System;
using Airlift.Welcome;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Dee (owner-picked robot, Higgsfield) sits on the assistant bar's Orb. The Orb keeps its runtime state tint and pulse
// (NerdyDirector.LateUpdate) and now reads as a halo; Dee's face is a child, so it pulses with it but keeps its colours.
// Run after CompactAssistantBar (which recreates the Orb). Idempotent. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/ApplyDeeFace.cs --entry ApplyDeeFace.Run
public static class ApplyDeeFace
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string FacePath = "Assets/Airlift/Art/Generated/Dee/Dee-icon.png";

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(FacePath) ?? throw new InvalidOperationException(FacePath + " missing or not a sprite: run ApplyArtImportSettings.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        if (n.orb == null) throw new InvalidOperationException("No Orb on the bar: run CompactAssistantBar first.");

        var old = n.orb.transform.Find("Dee face"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var face = new GameObject("Dee face", typeof(RectTransform), typeof(Image)); face.transform.SetParent(n.orb.transform, false);
        var orbSize = ((RectTransform)n.orb.transform).rect.size;
        var r = (RectTransform)face.transform; r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f); r.anchoredPosition = Vector2.zero;
        r.sizeDelta = orbSize * 0.86f;
        var img = face.GetComponent<Image>(); img.sprite = sprite; img.type = Image.Type.Simple; img.preserveAspect = true;
        img.color = Color.white; img.raycastTarget = false;

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Dee face " + r.sizeDelta + " on the " + orbSize + " Orb";
    }
}
