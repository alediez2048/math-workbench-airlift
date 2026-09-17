using System;
using System.Linq;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Owner 2026-09-17: "the ai panel is sitting on top of the bottom of the dashboard, it needs to go right below.
// Also the language and lounge buttons are all crammed up." Two real faults: the consent card grew two option rows
// without growing itself, so the scenery row overlapped the Allow/Continue buttons; and the assistant bar sat at a
// hand-set offset that the taller card now reaches past.
// This lays the card out on one vertical rhythm and then places the bar from measured world bounds, so it lands
// just below the card whatever the card's size or scale. Idempotent. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/FixWelcomeLayout.cs --entry FixWelcomeLayout.Run
public static class FixWelcomeLayout
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const float CardWidth = 760f, CardHeight = 580f;   // 470 could not hold two option rows and the buttons
    const float RowHeight = 40f, LabelWidth = 220f, PillWidth = 176f, PillGap = 26f;   // 40 keeps the pill ends true half-circles

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include)
                ?? throw new InvalidOperationException("NerdyDirector missing.");
        var consent = (RectTransform)n.consentRoot.transform;
        consent.sizeDelta = new Vector2(CardWidth, CardHeight);

        // One vertical rhythm, top to bottom, with real air between the groups.
        Move(consent, "Logo", new Vector2(0f, 226f));
        Move(consent, "Heading", new Vector2(0f, 146f));
        Move(consent, "Body", new Vector2(0f, 52f));
        Row(consent, "Language row", -52f);
        Row(consent, "Scenery row", -116f);
        Move(consent, "Allow voice", new Vector2(-150f, -206f));
        Move(consent, "No voice", new Vector2(170f, -206f));

        // Owner 2026-09-17: "my face is like 5 inches away from the whiteboard". 1.15 m was set when the panel was
        // a floating card; mounting a 1.6 m board on it made that distance far too close to take in at a glance.
        n.welcomeDistance = 1.85f;
        n.welcomeBelowEyes = 0.22f;

        string bar = PlaceBarBelowCard(n, consent);
        EditorUtility.SetDirty(n);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Consent card " + CardWidth + "x" + CardHeight + " at " + n.welcomeDistance + "m; rows re-spaced; " + bar;
    }

    /// A row is a right-aligned label and two pills of equal width with a real gap, on one baseline.
    static void Row(RectTransform consent, string rowName, float y)
    {
        var row = consent.Find(rowName) as RectTransform;
        if (row == null) throw new InvalidOperationException(rowName + " missing (run AddLanguageChoice and BuildLounge first).");
        row.anchoredPosition = new Vector2(0f, y);
        row.sizeDelta = new Vector2(640f, RowHeight);

        float pillsWidth = PillWidth * 2f + PillGap;
        float firstX = (640f - pillsWidth) / 2f - 640f / 2f + PillWidth / 2f + LabelWidth / 2f + 30f;

        int pill = 0;
        foreach (RectTransform child in row)
        {
            var label = child.GetComponent<TMP_Text>();
            if (label != null && child.GetComponent<Button>() == null)
            {
                child.anchoredPosition = new Vector2(-640f / 2f + LabelWidth / 2f + 10f, 0f);
                child.sizeDelta = new Vector2(LabelWidth, RowHeight);
                label.alignment = TextAlignmentOptions.Right;
                continue;
            }
            child.anchoredPosition = new Vector2(firstX + pill * (PillWidth + PillGap), 0f);
            child.sizeDelta = new Vector2(PillWidth, RowHeight);
            var text = child.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.rectTransform.anchoredPosition = Vector2.zero;
                text.rectTransform.sizeDelta = new Vector2(PillWidth - 16f, RowHeight - 8f);
            }
            var gradient = child.Find("Gradient") as RectTransform;
            if (gradient != null) { gradient.anchoredPosition = Vector2.zero; gradient.sizeDelta = new Vector2(PillWidth, RowHeight); }
            pill++;
        }
    }

    static void Move(RectTransform parent, string name, Vector2 at)
    {
        var t = parent.Find(name) as RectTransform;
        if (t == null) throw new InvalidOperationException(name + " missing from the consent card.");
        t.anchoredPosition = at;
    }

    /// The bar is on its own canvas, so hand-set offsets drift whenever the card changes. Measure both in world
    /// space and put the bar's top a finger's width under the card's bottom.
    static string PlaceBarBelowCard(NerdyDirector n, RectTransform consent)
    {
        var hud = (RectTransform)n.hudRoot.transform;
        var parent = n.hudWelcomeCanvas;
        var prior = hud.parent;
        var priorPos = hud.anchoredPosition;
        hud.SetParent(parent, false);
        hud.anchoredPosition = n.hudWelcomePosition;
        Canvas.ForceUpdateCanvases();

        var cardCorners = new Vector3[4]; consent.GetWorldCorners(cardCorners);
        var barCorners = new Vector3[4]; hud.GetWorldCorners(barCorners);
        float cardBottom = cardCorners.Min(c => c.y);
        float barTop = barCorners.Max(c => c.y);
        float barHeightWorld = barCorners.Max(c => c.y) - barCorners.Min(c => c.y);
        float gap = barHeightWorld * 0.35f;                    // a finger's width at this scale

        float dropWorld = barTop - (cardBottom - gap);
        float unitsPerWorld = hud.sizeDelta.y / Mathf.Max(barHeightWorld, 1e-6f);
        var placed = new Vector2(n.hudWelcomePosition.x, n.hudWelcomePosition.y - dropWorld * unitsPerWorld);

        n.hudWelcomePosition = placed;
        hud.anchoredPosition = placed;
        if (prior != parent) hud.SetParent(prior, false);
        return "assistant bar moved from " + priorPos.y.ToString("F0") + " to " + placed.y.ToString("F0")
               + " (card bottom " + cardBottom.ToString("F2") + "m, gap " + gap.ToString("F3") + "m)";
    }
}
