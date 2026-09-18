using System;
using System.Linq;
using Airlift.Presentation;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Owner 2026-09-18, approved on the canvas: ‹ Back and › Next circle actions in the board's bottom-right corner, on
// every screen, dimmed when there is nothing to do. They ride on Dee's bar so they follow it above the workbench in
// a lesson; the station HUD canvas grows so its ray surface still covers them there.
// Idempotent. Run after CompactAssistantBar. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/BuildNavArrows.cs --entry BuildNavArrows.Run
public static class BuildNavArrows
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    public const float Circle = 44f, Gap = 14f;

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var bar = (RectTransform)n.hudRoot.transform;

        var old = bar.Find("Nav arrows"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var root = new GameObject("Nav arrows", typeof(RectTransform)).GetComponent<RectTransform>(); root.SetParent(bar, false);
        // Bottom-right of the board: the bar sits at hudWelcomePosition inside the board's lower band, so the corner is
        // a fixed offset from the bar's centre. (Panel half-size minus the edge padding; labels hang under the circles.)
        float cornerX = NerdySpace.PanelWidth / 2f - NerdySpace.EdgePadding - Circle / 2f;
        float cornerY = -NerdySpace.PanelHeight / 2f + NerdySpace.EdgePadding + Circle / 2f + 10f;
        root.anchoredPosition = new Vector2(cornerX - Circle / 2f - Gap / 2f, cornerY - n.hudWelcomePosition.y);
        root.sizeDelta = Vector2.zero;

        n.navBack = Circle_(root, "Back arrow", "‹", "Back", new Vector2(-(Circle + Gap) / 2f, 0f), style, n, "PressBack");
        n.navNext = Circle_(root, "Next arrow", "›", "Next", new Vector2((Circle + Gap) / 2f, 0f), style, n, "PressNext");

        // The station HUD canvas was exactly the bar; the arrows sit below and right of it there, so the canvas and
        // its clipper grow to keep them pointable above the workbench.
        var station = (RectTransform)n.hudStationCanvas;
        var stationSize = new Vector2(Mathf.Max(station.sizeDelta.x, 2f * (Mathf.Abs(root.anchoredPosition.x) + Circle + Gap + 20f)), Mathf.Max(station.sizeDelta.y, 2f * (Mathf.Abs(root.anchoredPosition.y) + Circle + 30f)));
        station.sizeDelta = stationSize;
        foreach (var c in station.GetComponentsInChildren<Component>(true))
        {
            if (c == null || !c.GetType().Name.Contains("BoundsClipper")) continue;
            var so = new SerializedObject(c); var size = so.FindProperty("_size");
            if (size != null) { size.vector3Value = new Vector3(stationSize.x, stationSize.y, size.vector3Value.z); so.ApplyModifiedProperties(); }
        }

        EditorUtility.SetDirty(n);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "‹ › at bar offset " + root.anchoredPosition + " (board corner " + cornerX + "," + cornerY + "); station canvas " + stationSize;
    }

    /// The guide's circle action: 1.5 px white outline, transparent fill, one glyph; a tiny label under it.
    static Button Circle_(RectTransform parent, string name, string glyph, string label, Vector2 pos, NerdyStyle style, NerdyDirector n, string method)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup)); go.transform.SetParent(parent, false);
        var r = (RectTransform)go.transform; r.anchoredPosition = pos; r.sizeDelta = new Vector2(Circle, Circle);
        var ring = go.GetComponent<Image>(); ring.sprite = style.pill; ring.type = Image.Type.Sliced; ring.color = style.text;
        ring.pixelsPerUnitMultiplier = style.pill.border.x / (Circle / 2f) * (100f / style.pill.pixelsPerUnit);
        var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image)); inner.transform.SetParent(go.transform, false);
        var ir = (RectTransform)inner.transform; ir.sizeDelta = new Vector2(Circle - 3f, Circle - 3f);
        var ii = inner.GetComponent<Image>(); ii.sprite = style.pill; ii.type = Image.Type.Sliced; ii.color = style.surface; ii.raycastTarget = false;
        ii.pixelsPerUnitMultiplier = style.pill.border.x / ((Circle - 3f) / 2f) * (100f / style.pill.pixelsPerUnit);
        var t = new GameObject("Glyph", typeof(RectTransform)).AddComponent<TextMeshProUGUI>(); t.transform.SetParent(go.transform, false);
        t.font = style.bodyFont; t.fontSize = NerdySpace.Subhead + 4f; t.color = style.text; t.text = glyph; t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
        t.rectTransform.sizeDelta = new Vector2(Circle, Circle); t.rectTransform.anchoredPosition = new Vector2(0f, 2f);
        var l = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>(); l.transform.SetParent(go.transform, false);
        l.font = style.altFont; l.fontSize = NerdySpace.Micro; l.color = style.textMuted; l.text = label; l.alignment = TextAlignmentOptions.Center; l.raycastTarget = false;
        l.rectTransform.sizeDelta = new Vector2(80f, 16f); l.rectTransform.anchoredPosition = new Vector2(0f, -Circle / 2f - 10f);
        var b = go.GetComponent<Button>(); b.targetGraphic = ring;
        var colors = b.colors; colors.highlightedColor = new Color(0.9f, 0.9f, 1f); colors.pressedColor = new Color(0.75f, 0.75f, 0.95f); b.colors = colors;
        UnityEventTools.AddPersistentListener(b.onClick, (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), n, method));
        go.AddComponent<FocusPointer>();
        return b;
    }
}
