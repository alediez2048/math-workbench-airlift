using System;
using System.Linq;
using Airlift.Presentation;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Owner 2026-09-18: "all the bar items are not compact, we need to make this bar very compact, not having so much
// space between elements." One bar of NerdySpace.BarWidth x BarHeight: orb and state on the left, the caption beside
// it, and every control packed on the right at BarGap. Idempotent. Run LAST, after FixWelcomeLayout. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/CompactAssistantBar.cs --entry CompactAssistantBar.Run
public static class CompactAssistantBar
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var bar = (RectTransform)n.hudRoot.transform;
        float w = NerdySpace.BarWidth, h = NerdySpace.BarHeight, half = w / 2f, pad = NerdySpace.BarPadding, gap = NerdySpace.BarGap;
        bar.sizeDelta = new Vector2(w, h);
        foreach (RectTransform child in bar)
            if ((child.name == "Glass" || child.name == "Stroke") && child.anchorMin == child.anchorMax) child.sizeDelta = bar.sizeDelta;

        // Owner 2026-09-18: "get rid of any instances of a blue orb floating". The orb is off everywhere; the state
        // line keeps its corner and the caption starts at the bar's left padding.
        var orb = Place(bar, "Orb", new Vector2(-half + pad + 18f, 8f), new Vector2(36f, 36f));
        if (orb != null && orb.gameObject.activeSelf) orb.gameObject.SetActive(false);
        Place(bar, "State", new Vector2(-half + pad + 45f, -26f), new Vector2(90f, 20f));

        // Right: Music · gear · Help, packed from the right edge.
        var controls = new[] { n.helpButton, n.muteButton != null && n.muteButton.transform.parent == bar ? n.muteButton : null, bar.Find("Settings gear")?.GetComponent<Button>(), n.musicButton }
            .Where(b => b != null && b.transform.parent == bar).Select(b => (RectTransform)b.transform).ToArray();
        float x = half - pad;
        foreach (var r in controls)
        {
            if (r.name == "Music") { r.sizeDelta = new Vector2(96f, NerdySpace.PillHeight); Retune(r); var l = r.GetComponentInChildren<TMP_Text>(true); if (l != null) { l.fontSize = NerdySpace.Label - 1f; l.rectTransform.sizeDelta = new Vector2(88f, 30f); l.rectTransform.anchoredPosition = Vector2.zero; } }
            if (r.name == "Help") r.sizeDelta = new Vector2(78f, NerdySpace.PillHeight);
            Retune(r);
            r.anchoredPosition = new Vector2(x - r.sizeDelta.x / 2f, 6f);
            x -= r.sizeDelta.x + gap;
        }
        float controlsLeft = x + gap;

        // Middle: caption from the orb to the controls, transcript line under it.
        float capLeft = -half + pad, capRight = controlsLeft - 14f;
        var caption = Place(bar, "Caption", new Vector2((capLeft + capRight) / 2f, 8f), new Vector2(capRight - capLeft, 56f));
        var you = Place(bar, "You said", new Vector2((capLeft + capRight) / 2f, -29f), new Vector2(capRight - capLeft, 20f));
        if (caption != null) { var t = caption.GetComponent<TMP_Text>(); if (t != null) { t.alignment = TextAlignmentOptions.MidlineLeft; t.enableAutoSizing = true; t.fontSizeMin = 12f; t.fontSizeMax = Mathf.Max(t.fontSizeMax, NerdySpace.Body); } }
        if (you != null) { var t = you.GetComponent<TMP_Text>(); if (t != null) t.alignment = TextAlignmentOptions.MidlineLeft; }

        // The bar draws above every card on the canvas: a card instantiated after it (settings, rundown) covered its
        // buttons, and the owner's gear press went into the card's background instead. Last sibling = drawn last.
        bar.SetAsLastSibling();
        // Drawn last, it would sit over the settings card's Done button; the card hides the bar while it is open
        // (Pause / Again / Mute are inside the card) and Done brings it back.
        var settings = n.GetComponent<Airlift.Lounge.LoungeSettings>();
        if (settings != null && !(settings.hideWhileOpen ?? new GameObject[0]).Contains(n.hudRoot))
        {
            settings.hideWhileOpen = (settings.hideWhileOpen ?? new GameObject[0]).Concat(new[] { n.hudRoot }).ToArray();
            EditorUtility.SetDirty(settings);
        }

        EditorUtility.SetDirty(n);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "bar " + w + "x" + h + "; controls " + string.Join(" · ", controls.Select(r => r.name)) + " packed at " + gap + "; caption " + (capRight - capLeft).ToString("F0") + " wide";
    }

    static RectTransform Place(RectTransform bar, string name, Vector2 pos, Vector2 size)
    {
        var r = bar.Find(name) as RectTransform; if (r == null) return null;
        r.anchoredPosition = pos; r.sizeDelta = size; return r;
    }

    static void Retune(RectTransform r)
    {
        var img = r.GetComponent<Image>();
        if (img != null && img.sprite != null && img.sprite.border.x > 0f && img.type == Image.Type.Sliced)
            img.pixelsPerUnitMultiplier = img.sprite.border.x / (Mathf.Min(r.sizeDelta.x, r.sizeDelta.y) / 2f) * (100f / img.sprite.pixelsPerUnit) / (100f / img.sprite.pixelsPerUnit) ;
    }
}
