using System;
using Airlift.Presentation;
using Airlift.Welcome;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// CC-HF-02. Dee (owner-picked robot) stands at the left end of the assistant bar, which already travels between the
// wall and every lesson. DeeAvatar swaps idle / listening / speaking poses from the voice state and floats gently.
// Run after CompactAssistantBar (which rebuilds the bar). Idempotent. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/ApplyDeeAvatar.cs --entry ApplyDeeAvatar.Run
public static class ApplyDeeAvatar
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string Dir = "Assets/Airlift/Art/Generated/Dee/";
    const float Size = 140f;   // bar units; the bar is 84 tall, so Dee stands a little taller than it

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        Sprite Load(string n) => AssetDatabase.LoadAssetAtPath<Sprite>(Dir + "Dee-" + n + ".png") ?? throw new InvalidOperationException("Dee-" + n + " is not a sprite: run ApplyArtImportSettings.");
        var idle = Load("idle"); var listening = Load("listening"); var speaking = Load("speaking");

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var bar = n.hudRoot != null ? (RectTransform)n.hudRoot.transform : throw new InvalidOperationException("No assistant bar: run CompactAssistantBar.");
        var old = bar.Find("Dee"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);

        var go = new GameObject("Dee", typeof(RectTransform), typeof(Image), typeof(DeeAvatar));
        go.transform.SetParent(bar, false);
        var r = (RectTransform)go.transform;
        r.anchorMin = r.anchorMax = new Vector2(0f, 0.5f);            // the bar's left edge
        r.pivot = new Vector2(1f, 0.72f);                             // beside it, top level with the bar so the pills above stay clear
        r.sizeDelta = new Vector2(Size, Size);
        r.anchoredPosition = new Vector2(-10f, 0f);
        var img = go.GetComponent<Image>(); img.sprite = idle; img.preserveAspect = true; img.raycastTarget = false;
        var dee = go.GetComponent<DeeAvatar>(); dee.director = n; dee.idle = idle; dee.listening = listening; dee.speaking = speaking;

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Dee stands at the bar's left edge (" + Size + " units)";
    }
}
