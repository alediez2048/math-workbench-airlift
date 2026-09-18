using System;
using System.Linq;
using Airlift.Lounge;
using Airlift.Presentation;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Owner 2026-09-17: "lets leave only two buttons visible on the dashboard, I'm an adult tester and continue without
// voice, everything else should be inside a gear icon next to the ai assistant."
// Moves the language and scenery rows off the consent card into a settings card, and puts a gear on the assistant
// bar that opens it. The welcome board is then one question with two answers.
// Idempotent. Run after BuildLounge, before ApplySpatialStandards and FixWelcomeLayout. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/BuildSettingsPanel.cs --entry BuildSettingsPanel.Run
public static class BuildSettingsPanel
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include)
                ?? throw new InvalidOperationException("NerdyDirector missing.");
        var consent = (RectTransform)n.consentRoot.transform;

        // Find the real row objects wherever they are now — still on the card on a first run, already in the
        // settings card on a rerun. They are MOVED, never copied: the director holds references to these exact
        // objects, and copying them left it pointing at ones this script had just deleted.
        var previous = consent.parent.Find("Settings panel") ?? n.transform.Find("Settings panel");
        var rows = new[] { "Language row", "Scenery row" }
            .Select(name => (RectTransform)(consent.Find(name) ?? (previous != null ? previous.Find(name) : null)))
            .Where(r => r != null)
            .ToArray();
        foreach (var row in rows) row.SetParent(n.transform, false);     // park them while the card is rebuilt
        // Same for the guide controls: they are the director's own objects, and a rerun once destroyed them with
        // the previous panel they had been moved into.
        foreach (var b in new[] { n.pauseButton, n.repeatButton, n.muteButton })
            if (b != null) b.transform.SetParent(n.transform, false);
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);

        // The settings card copies the consent card, so it inherits the polished frame, shadow and stroke.
        // Under the same canvas as the consent card: a UI card parented anywhere else has no Canvas above it and
        // draws nothing — the owner saw a shadow with no card in it.
        var panel = UnityEngine.Object.Instantiate(n.consentRoot, consent.parent);
        panel.name = "Settings panel";
        var panelRect = (RectTransform)panel.transform;
        panelRect.localPosition = consent.localPosition;
        panelRect.localRotation = consent.localRotation;
        panelRect.localScale = consent.localScale;
        panelRect.sizeDelta = consent.sizeDelta;

        // Strip the copy back to a frame with a heading and one Done button.
        foreach (Transform child in panelRect.Cast<Transform>().ToArray())
        {
            switch (child.name)
            {
                case "Heading": case "Stroke": break;
                case "Allow voice": child.name = "Done"; break;
                default: UnityEngine.Object.DestroyImmediate(child.gameObject); break;
            }
        }

        var heading = panelRect.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t2 => t2.name == "Heading");
        if (heading != null) { heading.text = "Settings"; heading.rectTransform.anchoredPosition = new Vector2(0f, 200f); }

        // Now hand the real rows to the settings card.
        float[] ys = { 90f, 0f };
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].SetParent(panelRect, false);
            rows[i].anchoredPosition = new Vector2(0f, i < ys.Length ? ys[i] : -30f - i * 60f);
        }

        // Owner 2026-09-17: "remove the pause, again and mute buttons ... these should live inside the gear icon."
        // The director keeps its references — these are the same objects, moved — so its listeners still fire.
        var guideRow = panelRect.Find("Guide row") as RectTransform;
        if (guideRow == null)
        {
            guideRow = new GameObject("Guide row", typeof(RectTransform)).GetComponent<RectTransform>();
            guideRow.SetParent(panelRect, false);
        }
        guideRow.anchoredPosition = new Vector2(0f, -110f);
        guideRow.sizeDelta = new Vector2(640f, 40f);
        var guideLabelSource = rows.Length > 0 ? rows[0].GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(x => x.GetComponent<Button>() == null && x.transform.parent == rows[0]) : null;
        if (guideRow.Find("Guide label") == null && guideLabelSource != null)
        {
            var guideLabel = UnityEngine.Object.Instantiate(guideLabelSource.gameObject, guideRow).GetComponent<TMP_Text>();
            guideLabel.name = "Guide label"; guideLabel.text = "Dee";
            guideLabel.rectTransform.anchoredPosition = new Vector2(-640f / 2f + 220f / 2f - 12f, 0f);   // clear of the first pill
            guideLabel.rectTransform.sizeDelta = new Vector2(220f, 40f);
        }
        // A rerun once destroyed these with the previous panel. The director wires them by reference in Start
        // (AddListener on pauseButton / repeatButton / muteButton), so a fresh pill assigned to the field is enough.
        var pillTemplate = n.helpButton != null ? n.helpButton.gameObject : null;
        Button Recreate(string name, string text)
        {
            if (pillTemplate == null) return null;
            var go = UnityEngine.Object.Instantiate(pillTemplate, guideRow); go.name = name;
            var b = go.GetComponent<Button>();
            for (int k = b.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(b.onClick, k);
            var label = go.GetComponentInChildren<TMP_Text>(true); if (label != null) label.text = text;
            return b;
        }
        if (n.pauseButton == null) { n.pauseButton = Recreate("Pause", "Pause"); n.pauseLabel = n.pauseButton != null ? n.pauseButton.GetComponentInChildren<TMP_Text>(true) : null; }
        if (n.repeatButton == null) n.repeatButton = Recreate("Repeat", "Again");
        if (n.muteButton == null) n.muteButton = Recreate("Mute", "Mute");
        var guideButtons = new[] { n.pauseButton, n.repeatButton, n.muteButton }.Where(b => b != null).ToArray();
        float gx = -30f;
        foreach (var b in guideButtons)
        {
            var r = (RectTransform)b.transform;
            r.SetParent(guideRow, false);
            r.anchoredPosition = new Vector2(gx, 0f);
            r.sizeDelta = new Vector2(118f, NerdySpace.PillHeight);
            // Cloned from the 44-tall Help pill: retune the slice so the ends are half-circles at 40.
            var pillImage = b.GetComponent<Image>();
            if (pillImage != null && pillImage.sprite != null && pillImage.sprite.border.x > 0f)
                pillImage.pixelsPerUnitMultiplier = pillImage.sprite.border.x / (NerdySpace.PillHeight / 2f);
            var txt = b.GetComponentInChildren<TMP_Text>(true);
            if (txt != null) { txt.rectTransform.anchoredPosition = Vector2.zero; txt.rectTransform.sizeDelta = new Vector2(106f, 32f); }
            gx += 118f + 14f;
        }

        var done = panelRect.Find("Done");
        var settings = n.gameObject.GetComponent<LoungeSettings>() ?? n.gameObject.AddComponent<LoungeSettings>();
        settings.panel = panel;
        settings.hideWhileOpen = new[] { n.consentRoot, n.welcomeRoot, n.catalogRoot };

        if (done != null)
        {
            var rect = (RectTransform)done;
            rect.anchoredPosition = new Vector2(0f, -220f);
            rect.sizeDelta = new Vector2(300f, 56f);
            var label = done.GetComponentInChildren<TMP_Text>(true);
            if (label != null) { label.text = "Done"; label.rectTransform.sizeDelta = new Vector2(288f, 52f); }
            var button = done.GetComponent<Button>();
            for (int k = button.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(button.onClick, k);
            UnityEventTools.AddPersistentListener(button.onClick,
                (UnityEngine.Events.UnityAction)Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), settings, "Close"));
        }

        string gearNote = BuildGear(n, settings);
        panel.SetActive(false);

        EditorUtility.SetDirty(n); EditorUtility.SetDirty(settings);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Settings card holds " + rows.Length + " rows + " + guideButtons.Length + " guide controls; the welcome board keeps two buttons; " + gearNote;
    }

    static void PlaceRow(RectTransform panel, string rowName, float y)
    {
        var row = panel.Find(rowName) as RectTransform;
        if (row == null) return;
        row.anchoredPosition = new Vector2(0f, y);
    }

    /// The gear sits on the assistant bar, beside Mute and Help, where the learner's attention already is.
    /// It carries a drawn sprite: the Nerdy fonts have no gear glyph, and a missing character renders as a box.
    static string BuildGear(NerdyDirector n, LoungeSettings settings)
    {
        var bar = (RectTransform)n.hudRoot.transform;
        var existing = bar.Find("Settings gear");
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

        var template = n.muteButton != null ? n.muteButton.gameObject : n.helpButton.gameObject;
        var gear = UnityEngine.Object.Instantiate(template, bar);
        gear.name = "Settings gear";
        var rect = (RectTransform)gear.transform;
        // Square, at the standard pill height: the pill sprite's slicing is tuned so a 40-unit pill has true
        // half-circle ends (CardPolishTests). Copying the template's 44 broke that.
        rect.sizeDelta = Vector2.one * NerdySpace.PillHeight;
        // "True capsule" means the sliced ends' diameter equals the height: the multiplier copied from the 44-tall
        // Help pill has to be retuned for 40, or the ends read 44 on a 40 body.
        var gearImage = gear.GetComponent<Image>();
        if (gearImage != null && gearImage.sprite != null && gearImage.sprite.border.x > 0f)
            gearImage.pixelsPerUnitMultiplier = gearImage.sprite.border.x / (NerdySpace.PillHeight / 2f);

        // The label goes; the icon takes its place.
        var label = gear.GetComponentInChildren<TMP_Text>(true);
        if (label != null) UnityEngine.Object.DestroyImmediate(label.gameObject);

        const string gearPath = "Assets/Airlift/Sprites/NerdyGear.png";
        var importer = AssetImporter.GetAtPath(gearPath) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(gearPath);

        var icon = new GameObject("Gear icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(gear.transform, false);
        var iconRect = (RectTransform)icon.transform;
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = Vector2.one * (rect.sizeDelta.y * 0.56f);
        var image = icon.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        if (style != null) image.color = style.text;

        var button = gear.GetComponent<Button>();
        for (int k = button.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(button.onClick, k);
        UnityEventTools.AddPersistentListener(button.onClick,
            (UnityEngine.Events.UnityAction)Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), settings, "Toggle"));

        return LayOutBar(bar);
    }

    /// Owner 2026-09-17: "the settings option is overlapping with another button." Two mistakes to undo: the gear
    /// was placed by arithmetic that ran out of bar, and the fix that followed spread every button across the
    /// whole bar — over the orb, the caption and the transcript on the left. The bar has two halves: text on the
    /// left, controls on the right. Controls are laid out in the right half only, one row, even gaps.
    static string LayOutBar(RectTransform bar)
    {
        var controls = bar.Cast<Transform>()
            .Select(t => t as RectTransform)
            .Where(r => r != null && r.GetComponent<Button>() != null && r.gameObject.activeSelf && r.name != "Music")
            .OrderBy(r => r.anchoredPosition.x)
            .ToArray();
        if (controls.Length == 0) return "no controls on the bar";

        const float minGap = 14f, padding = 24f;
        float half = bar.sizeDelta.x / 2f;
        // The controls start where the text actually ends, measured — not at a guessed fraction of the bar.
        float textRight = bar.Cast<Transform>()
            .Select(t => t as RectTransform)
            .Where(r => r != null && r.GetComponent<Button>() == null && r.name != "Glass" && r.name != "Stroke")
            .Select(r => r.anchoredPosition.x + r.sizeDelta.x / 2f)
            .DefaultIfEmpty(0f).Max();
        float zoneLeft = textRight + 28f;
        float zoneRight = half - padding;
        float available = zoneRight - zoneLeft;
        float widths = controls.Sum(r => r.sizeDelta.x);
        float gap = controls.Length > 1 ? (available - widths) / (controls.Length - 1) : 0f;

        if (gap < minGap && controls.Length > 1)
        {
            float target = available - minGap * (controls.Length - 1);
            float factor = Mathf.Clamp(target / widths, 0.5f, 1f);
            foreach (var r in controls) r.sizeDelta = new Vector2(r.sizeDelta.x * factor, r.sizeDelta.y);
            widths = controls.Sum(r => r.sizeDelta.x);
            gap = minGap;
        }
        gap = Mathf.Min(gap, 40f);                            // even, but not scattered
        float x = zoneRight - (widths + gap * (controls.Length - 1));   // right-aligned block
        foreach (var r in controls)
        {
            r.anchoredPosition = new Vector2(x + r.sizeDelta.x / 2f, r.anchoredPosition.y);
            x += r.sizeDelta.x + gap;
        }
        return controls.Length + " controls in the bar's right half, gap " + gap.ToString("F0");
    }
}
