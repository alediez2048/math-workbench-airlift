using System;
using System.Collections.Generic;
using Airlift.Presentation;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// CC-CF-04: restyles the baked lesson cards from LessonCatalog, the way CreateNerdyWelcome.Card builds them, without
// rerunning CreateNerdyWelcome. Playable cards get the subject badge on the brand gradient, full-opacity panel, art and
// arrow; previews keep the amber "COMING SOON" badge and faded panel. Description text is copied from the catalog.
// CC-GD-04: the badge widens for MULTIPLICATION and keeps the playable left edge.
// Idempotent: only differing values change; the Cargo card already matches and is left as it is. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/PatchCatalogCards.cs --entry PatchCatalogCards.Run
public static class PatchCatalogCards
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var S = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        if (S == null || S.brandGradient == null || S.pill == null) throw new InvalidOperationException("NerdyStyle asset or sprites missing.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        if (n == null || n.catalogRoot == null) throw new InvalidOperationException("NerdyDirector or catalog root missing.");
        var report = new List<string>();
        foreach (var card in LessonCatalog.Cards)
        {
            var go = n.catalogRoot.transform.Find("Card " + card.Id); if (go == null) throw new InvalidOperationException("missing card " + card.Id);
            var changes = new List<string>();
            Color panel = card.Playable ? S.surface : new Color(S.surface.r, S.surface.g, S.surface.b, 0.75f);
            SetColor(go.GetComponent<Image>(), panel, "panel", changes);
            SetColor(Child<Image>(go, "Art"), card.Playable ? new Color(1, 1, 1, 0.9f) : new Color(1, 1, 1, 0.35f), "art", changes);
            SetColor(Child<Image>(go, "Arrow inner"), panel, "arrow", changes);

            var desc = Child<TMP_Text>(go, "Description");
            if (desc.text != card.Description) { desc.text = card.Description; EditorUtility.SetDirty(desc); changes.Add("description"); }

            var badge = go.Find("Badge"); if (badge == null) throw new InvalidOperationException("missing badge on " + card.Id);
            StyleBadge(badge, card, S, changes);

            if (changes.Count > 0) report.Add(card.Id + ": " + string.Join(", ", changes));
        }
        if (report.Count == 0) return "PatchCatalogCards: cards already match the catalog.";
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "PatchCatalogCards: " + string.Join(" | ", report) + " | scene saved.";
    }

    static T Child<T>(Transform parent, string name) where T : Component
    {
        var t = parent.Find(name); var c = t != null ? t.GetComponent<T>() : null;
        if (c == null) throw new InvalidOperationException("missing " + name + " on " + parent.name);
        return c;
    }

    static void SetColor(Graphic g, Color c, string what, List<string> changes)
    {
        if (g.color == c) return;
        g.color = c; EditorUtility.SetDirty(g); changes.Add(what);
    }

    /// Mirrors CreateNerdyWelcome.Pill + Card: a playable badge is a white pill masking a brand-gradient child with the
    /// subject in capitals (110 wide); a preview badge is an amber outlined pill reading COMING SOON (130 wide).
    static void StyleBadge(Transform badge, LessonCard card, NerdyStyle S, List<string> changes)
    {
        var rect = badge.GetComponent<RectTransform>(); var img = badge.GetComponent<Image>();
        var label = badge.GetComponentInChildren<TMP_Text>(true);
        if (label == null) throw new InvalidOperationException("badge label missing on " + card.Id);
        string text = card.Playable ? card.Subject.ToUpper() : "COMING SOON";
        if (label.text != text) { label.text = text; EditorUtility.SetDirty(label); changes.Add("badge text"); }
        // A playable badge is 110 wide (FRACTIONS, DIVISION) and grows for longer subjects (MULTIPLICATION), keeping
        // the left edge CreateNerdyWelcome gave it (x -125). A preview badge keeps its 130 width centred at x -70.
        float width = card.Playable ? Mathf.Max(110f, Mathf.Ceil(text.Length * 7.5f) + 24f) : 130f;
        var size = new Vector2(width, 26);
        var position = new Vector2(card.Playable ? -125f + width / 2f : -70f, 30f);
        if (rect.sizeDelta != size) { rect.sizeDelta = size; EditorUtility.SetDirty(rect); changes.Add("badge size"); }
        if (rect.anchoredPosition != position) { rect.anchoredPosition = position; EditorUtility.SetDirty(rect); changes.Add("badge position"); }
        if (label.rectTransform.sizeDelta != size - new Vector2(12, 4)) { label.rectTransform.sizeDelta = size - new Vector2(12, 4); EditorUtility.SetDirty(label.rectTransform); }

        var gradient = badge.Find("Gradient");
        var outline = badge.GetComponent<Outline>();
        var mask = badge.GetComponent<Mask>();
        if (card.Playable)
        {
            SetColor(img, Color.white, "badge fill", changes);
            if (outline != null) { UnityEngine.Object.DestroyImmediate(outline); changes.Add("badge outline removed"); }
            if (gradient == null)
            {
                var go = new GameObject("Gradient", typeof(RectTransform), typeof(Image)); go.transform.SetParent(badge, false);
                go.transform.SetSiblingIndex(0);   // behind the label, as Pill builds it
                var g = go.GetComponent<Image>(); g.sprite = S.brandGradient; g.preserveAspect = false; g.type = Image.Type.Simple; g.raycastTarget = false;
                gradient = go.transform; changes.Add("badge gradient");
            }
            var gr = gradient.GetComponent<RectTransform>();
            if (gr.sizeDelta != size || gr.anchoredPosition != Vector2.zero) { gr.sizeDelta = size; gr.anchoredPosition = Vector2.zero; EditorUtility.SetDirty(gr); }
            if (mask == null) { mask = badge.gameObject.AddComponent<Mask>(); mask.showMaskGraphic = false; changes.Add("badge mask"); }
        }
        else
        {
            if (gradient != null) { UnityEngine.Object.DestroyImmediate(gradient.gameObject); changes.Add("badge gradient removed"); }
            if (mask != null) { UnityEngine.Object.DestroyImmediate(mask); changes.Add("badge mask removed"); }
            if (outline == null)
            {
                outline = badge.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(S.line.r, S.line.g, S.line.b, 0.9f); outline.effectDistance = new Vector2(1.2f, -1.2f);
                changes.Add("badge outline");
            }
            SetColor(img, new Color(S.amber.r, S.amber.g, S.amber.b, 0.85f), "badge fill", changes);
        }
    }
}
