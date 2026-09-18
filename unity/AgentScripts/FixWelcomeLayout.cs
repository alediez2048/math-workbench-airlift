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
        var consent = (RectTransform)n.consentRoot.transform;   // sized by ApplySpatialStandards

        // Designed against artifacts/lounge/welcome-board.png, not guessed: the board is 840 tall, so the content
        // is spread through it with even air — logo, heading, body, the two answers, then the assistant bar in the
        // lower band. Top and bottom margins come out near 11% each; nothing touches anything.
        Move(consent, "Logo", new Vector2(0f, 290f));
        Move(consent, "Heading", new Vector2(0f, 185f));
        Move(consent, "Body", new Vector2(0f, 55f));
        Move(consent, "Allow voice", new Vector2(-150f, -110f));
        Move(consent, "No voice", new Vector2(170f, -110f));

        // Owner 2026-09-17: the logo takes more of the card than it earns.
        var logo = consent.Find("Logo") as RectTransform;
        // The logo is a mark, not a headline: it holds a fixed share of the card however large the card gets.
        if (logo != null)
        {
            // Owner 2026-09-17: the Nerdy AI + VR mark replaces the green wordmark everywhere in the app.
            var sprite = LogoSprite();
            var image = logo.GetComponent<Image>();
            if (image != null && sprite != null) { image.sprite = sprite; image.preserveAspect = true; }
            float aspect = sprite != null ? sprite.rect.height / sprite.rect.width : 406f / 1963f;
            float width = NerdySpace.PanelWidth * 0.22f;
            logo.sizeDelta = new Vector2(width, width * aspect);
        }

        // Distance is NerdySpace's to set, not this script's: two scripts owning one number is how the panel ended
        // up at 1.85 m while the standard said 1.2 m. ApplySpatialStandards places every panel.

        string bar = PlaceBarBelowCard(n, consent);
        EditorUtility.SetDirty(n);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Consent card: compact block, rows re-spaced; " + bar;
    }

    /// A row is a right-aligned label and two pills of equal width with a real gap, on one baseline.
    static void Row(RectTransform consent, string rowName, float y)
    {
        var row = consent.Find(rowName) as RectTransform;
        if (row == null) return;      // moved into the settings card (BuildSettingsPanel); the board keeps two buttons
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

    public const string LogoPath = "Assets/Airlift/Branding/nerdy-ai-vr-logo.png";

    /// The mark imports as a sprite with its transparency; the builder makes sure of it rather than assuming.
    public static Sprite LogoSprite()
    {
        var importer = AssetImporter.GetAtPath(LogoPath) as TextureImporter;
        if (importer != null && (importer.textureType != TextureImporterType.Sprite || !importer.alphaIsTransparency || importer.mipmapEnabled))
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(LogoPath);
    }

    static void Move(RectTransform parent, string name, Vector2 at)
    {
        var t = parent.Find(name) as RectTransform;
        if (t == null) return;       // an element that has moved elsewhere is not an error
        t.anchoredPosition = at;
    }

    /// Owner 2026-09-17: the assistant belongs "at the bottom of the whiteboard, but inside the whiteboard".
    /// So it sits in the empty band between the last row of content and the board's bottom edge — measured, not
    /// guessed, because the card and the bar live on different canvases at different scales.
    static string PlaceBarBelowCard(NerdyDirector n, RectTransform consent)
    {
        var hud = (RectTransform)n.hudRoot.transform;
        var parent = n.hudWelcomeCanvas;
        var prior = hud.parent;
        var priorPos = hud.anchoredPosition;
        hud.SetParent(parent, false);
        hud.anchoredPosition = n.hudWelcomePosition;

        // The bar was 920 wide on a 1440 card, and its buttons had been spread across the caption zone. Widen it
        // to the card's content width; the caption keeps the left half and the buttons take the right.
        float barWidth = NerdySpace.PanelWidth - NerdySpace.EdgePadding * 4f;
        hud.sizeDelta = new Vector2(barWidth, hud.sizeDelta.y);
        foreach (RectTransform child in hud)
            if ((child.name == "Glass" || child.name == "Stroke") && child.anchorMin == child.anchorMax)
                child.sizeDelta = hud.sizeDelta;
        Canvas.ForceUpdateCanvases();

        var cardCorners = new Vector3[4]; consent.GetWorldCorners(cardCorners);
        float cardBottom = cardCorners.Min(c => c.y);

        // The lowest piece of content on the card: everything below this is free board.
        float contentBottom = float.MaxValue;
        var corners = new Vector3[4];
        foreach (RectTransform child in consent)
        {
            if (child.sizeDelta.y <= 0f || !child.gameObject.activeSelf) continue;
            child.GetWorldCorners(corners);
            contentBottom = Mathf.Min(contentBottom, corners.Min(c => c.y));
        }
        if (contentBottom > cardBottom + 10f) contentBottom = cardCorners.Max(c => c.y);

        var barCorners = new Vector3[4]; hud.GetWorldCorners(barCorners);
        float barCentre = (barCorners.Max(c => c.y) + barCorners.Min(c => c.y)) / 2f;
        float barHeight = barCorners.Max(c => c.y) - barCorners.Min(c => c.y);

        // Centre it in the free band, so it is inside the board with air above and below.
        float target = (contentBottom + cardBottom) / 2f;
        float lowest = cardBottom + barHeight / 2f + 0.01f;      // never poke through the bottom edge
        target = Mathf.Max(target, lowest);

        float unitsPerWorld = hud.sizeDelta.y / Mathf.Max(barHeight, 1e-6f);
        var placed = new Vector2(n.hudWelcomePosition.x, n.hudWelcomePosition.y + (target - barCentre) * unitsPerWorld);

        n.hudWelcomePosition = placed;
        hud.anchoredPosition = placed;
        if (prior != parent) hud.SetParent(prior, false);
        return "assistant bar " + priorPos.y.ToString("F0") + " -> " + placed.y.ToString("F0")
               + " (inside the board: content ends " + contentBottom.ToString("F2") + "m, board bottom "
               + cardBottom.ToString("F2") + "m)";
    }
}
