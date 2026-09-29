using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// CC-HF-02. Each lesson card shows its place (dock, café, garden; Higgsfield) behind a scrim in the card's own colour,
// clipped to the rounded backdrop, so the card reads as designed and the text stays as legible as before. The heading,
// instruction, buttons and wiring are siblings of the Backdrop and are untouched. Idempotent. Saves CargoCrew.
// Run after BuildCafeWorkbench / BuildGardenWorkbench (which rebuild their cards).
// Run: unity command run_script --file AgentScripts/ApplyCardArt.cs --entry ApplyCardArt.Run
public static class ApplyCardArt
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    static readonly (string card, string art)[] Cards = { ("Lesson interface", "dock"), ("Cafe card", "cafe"), ("Garden card", "garden") };
    const float ScrimAlpha = 0.62f;

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var root = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Onboarding workbench - world locked")?.transform
                   ?? throw new InvalidOperationException("Onboarding workbench missing.");
        int done = 0;
        foreach (var (cardName, art) in Cards)
        {
            var card = FindDeep(root, cardName) ?? throw new InvalidOperationException(cardName + " missing.");
            var backdrop = card.Find("Backdrop") ?? throw new InvalidOperationException(cardName + "/Backdrop missing.");
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Airlift/Art/Generated/Card/Card-" + art + ".png")
                         ?? throw new InvalidOperationException("Card-" + art + " is not a sprite: run ApplyArtImportSettings.");
            var panel = backdrop.GetComponent<Image>();
            var panelColour = panel.color;

            foreach (var old in new[] { "Art", "Scrim" }) { var o = backdrop.Find(old); if (o != null) UnityEngine.Object.DestroyImmediate(o.gameObject); }
            var mask = backdrop.GetComponent<Mask>() ?? backdrop.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var size = ((RectTransform)backdrop).sizeDelta;
            var artImg = Layer(backdrop, "Art", size); artImg.sprite = sprite; artImg.preserveAspect = false; artImg.color = Color.white;
            var scrim = Layer(backdrop, "Scrim", size); scrim.color = new Color(panelColour.r, panelColour.g, panelColour.b, ScrimAlpha);
            done++;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return done + " lesson cards carry their place behind a " + ScrimAlpha + " scrim";
    }

    static Image Layer(Transform parent, string name, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
        var r = (RectTransform)go.transform; r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f); r.sizeDelta = size; r.anchoredPosition = Vector2.zero;
        var img = go.GetComponent<Image>(); img.raycastTarget = false; return img;
    }

    static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t) { var hit = FindDeep(c, name); if (hit != null) return hit; }
        return null;
    }
}
