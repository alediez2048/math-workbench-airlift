using System;
using System.IO;
using System.Linq;
using Airlift.Lounge;
using Airlift.Presentation;
using Airlift.Presentation.Dashboard;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// CC-FD-07/07b. The three catalog cards become the heroes of a wall of 24 tiles (3 lessons, 15 chapters, 6 coming
// soon), eight per page, with the filter bar under the wall: Featured · Newest · Most viewed, a pager and the scenery
// selector. The heroes are the SAME objects (moved and resized, never copied and deleted), so every existing card
// test still finds "Card <id>" with its polish. Chapter and coming-soon tiles are clones of their lesson's hero.
// Tile art for chapters is cropped from the seated-head chapter renders in artifacts/ into Assets/Airlift/Art/Tiles.
// Idempotent. Run after PolishCards and BuildSettingsPanel, before ApplySpatialStandards. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/BuildDashboard.cs --entry BuildDashboard.Run
public static class BuildDashboard
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string ArtDir = "Assets/Airlift/Art/Tiles";
    static NerdyStyle style;
    static readonly Vector2 Tile = DashboardWall.TileSize;   // 292 x 190

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var catalog = (RectTransform)n.catalogRoot.transform;
        var lounge = n.lounge ?? throw new InvalidOperationException("Run BuildLounge first.");

        // Clones and the toolbar from a previous run go; the heroes stay.
        foreach (Transform child in catalog.Cast<Transform>().ToArray())
            if (child.name.StartsWith("Tile ") || child.name.StartsWith("Soon ") || child.name == "Toolbar" || child.name.EndsWith(" shadow") && !child.name.StartsWith("Card "))
                UnityEngine.Object.DestroyImmediate(child.gameObject);

        var wall = catalog.GetComponent<DashboardWall>() ?? catalog.gameObject.AddComponent<DashboardWall>();
        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Application.dataPath), ArtDir));

        // Heading block at the top of the panel.
        var eyebrow = catalog.Find("Eyebrow") as RectTransform; if (eyebrow != null) eyebrow.anchoredPosition = new Vector2(0f, 345f);
        var heading = catalog.Find("Heading") as RectTransform; if (heading != null) heading.anchoredPosition = new Vector2(0f, 305f);
        var headingText = heading != null ? heading.GetComponent<TMP_Text>() : null;
        if (headingText != null) headingText.text = "Pick a <gradient=\"NerdySpectrum\">lesson</gradient>";

        var views = new System.Collections.Generic.List<DashboardTileView>();
        int artCount = 0;
        foreach (var card in LessonCatalog.Cards)
        {
            var hero = catalog.Find("Card " + card.Id) ?? throw new InvalidOperationException("Card " + card.Id + " missing.");
            ShapeTile(hero, wall);
            views.Add(View(hero, DashboardCatalog.TileId(card.Id, 0), card.Id, 0, true, wall));
            SetMinutes(hero, DashboardCatalog.LessonMinutes + " min");
            // The hero shows its first chapter's table, so the spectrum gradient stays with the heading alone
            // (the guide: one or two spectrum elements per screen). The Art plate underneath keeps its shape.
            var oldPhoto = hero.Find("Art/Photo"); if (oldPhoto != null) UnityEngine.Object.DestroyImmediate(oldPhoto.gameObject);
            if (Photo(hero, card.Id, 1)) artCount++;
            foreach (var (number, title) in DashboardCatalog.Chapters(card.Id))
            {
                var tile = Clone(hero, catalog, "Tile " + DashboardCatalog.TileId(card.Id, number));
                SetText(tile, "Title", title);
                SetText(tile, "Description", card.Title + " · Chapter " + number);
                SetBadge(tile, "CHAPTER " + number, playable: true);
                SetMinutes(tile, DashboardCatalog.ChapterMinutes + " min");
                if (Photo(tile, card.Id, number)) artCount++;
                views.Add(View(tile, DashboardCatalog.TileId(card.Id, number), card.Id, number, true, wall));
            }
        }
        var template = catalog.Find("Card " + LessonCatalog.Cards[0].Id);
        foreach (var world in DashboardCatalog.ComingSoon)
        {
            var tile = Clone(template, catalog, "Soon " + world.Id);
            SetText(tile, "Title", world.Title);
            SetText(tile, "Description", world.Job);
            SetBadge(tile, "COMING SOON", playable: false);
            SetMinutes(tile, world.Subject);
            GreyOut(tile);
            views.Add(View(tile, world.Id, null, 0, false, wall));
        }
        wall.tiles = views.ToArray();
        BuildToolbar(n, catalog, wall, lounge);
        n.wall = wall;

        // Everything parked off-screen until the wall lays a page out at runtime; the preview script calls Refresh.
        foreach (var v in views) v.SetShown(false, Vector2.zero);
        wall.Refresh(new LibraryState());

        foreach (var o in new UnityEngine.Object[] { n, wall }) EditorUtility.SetDirty(o);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return views.Count + " tiles (" + artCount + " with chapter art), toolbar under the wall, page 1 of " + DashboardCatalog.PageCount(views.Count);
    }

    // ---- one tile's shape: art on top, badge, title, one muted line, minutes, the circle action ----------------
    static void ShapeTile(Transform tile, DashboardWall wall)
    {
        var r = (RectTransform)tile; r.sizeDelta = Tile;
        Rect(tile, "Art", new Vector2(0f, Tile.y / 4f), new Vector2(Tile.x, Tile.y / 2f));
        Rect(tile, "Fade", new Vector2(0f, 20f), new Vector2(Tile.x, 50f));
        Rect(tile, "Badge", new Vector2(-70f, -8f), null);
        var title = Rect(tile, "Title", new Vector2(-10f, -36f), new Vector2(Tile.x - 60f, 30f));
        if (title != null) { var t = title.GetComponent<TMP_Text>(); t.fontSize = NerdySpace.Subhead; t.enableAutoSizing = false; t.textWrappingMode = TextWrappingModes.NoWrap; t.overflowMode = TextOverflowModes.Ellipsis; t.alignment = TextAlignmentOptions.Left; }
        var desc = Rect(tile, "Description", new Vector2(-20f, -66f), new Vector2(Tile.x - 80f, 34f));
        if (desc != null) { var t = desc.GetComponent<TMP_Text>(); t.fontSize = NerdySpace.Micro; t.enableAutoSizing = false; t.overflowMode = TextOverflowModes.Ellipsis; t.alignment = TextAlignmentOptions.TopLeft; }
        foreach (var name in new[] { "Arrow ring", "Arrow inner", "Arrow" }) Rect(tile, name, new Vector2(Tile.x / 2f - 30f, -Tile.y / 2f + 30f + (name == "Arrow" ? 1f : 0f)), null);
        var stroke = tile.Find("Stroke") as RectTransform; if (stroke != null) { stroke.anchorMin = Vector2.zero; stroke.anchorMax = Vector2.one; stroke.offsetMin = Vector2.zero; stroke.offsetMax = Vector2.zero; }
        // The shadow PolishCards made: a sibling, resized to the tile.
        var shadow = tile.parent.Find(tile.name + " shadow") as RectTransform;
        if (shadow != null) shadow.sizeDelta = Tile + new Vector2(48f, 48f);
        // Ribbon: over the art's top-left corner, off until the wall says so.
        var old = tile.Find("Continue ribbon"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var ribbon = new GameObject("Continue ribbon", typeof(RectTransform), typeof(Image)); ribbon.transform.SetParent(tile, false);
        var rr = (RectTransform)ribbon.transform; rr.anchoredPosition = new Vector2(-Tile.x / 2f + 58f, Tile.y / 2f - 22f); rr.sizeDelta = new Vector2(92f, 26f);
        var ri = ribbon.GetComponent<Image>(); ri.sprite = style.pill; ri.type = Image.Type.Sliced; ri.color = style.amber; ri.raycastTarget = false;
        if (ri.sprite != null && ri.sprite.border.x > 0f) ri.pixelsPerUnitMultiplier = ri.sprite.border.x / 13f;
        var label = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>(); label.transform.SetParent(ribbon.transform, false);
        label.font = style.altBold; label.fontSize = NerdySpace.Micro; label.color = style.surface; label.text = "CONTINUE"; label.characterSpacing = 4; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
        label.rectTransform.sizeDelta = new Vector2(92f, 26f);
        ribbon.SetActive(false);
        if (tile.GetComponent<TileHover>() == null) tile.gameObject.AddComponent<TileHover>();
    }

    static RectTransform Rect(Transform tile, string name, Vector2 pos, Vector2? size)
    {
        var r = tile.Find(name) as RectTransform; if (r == null) return null;
        r.anchoredPosition = pos; if (size != null) r.sizeDelta = size.Value; return r;
    }

    static void SetMinutes(Transform tile, string text)
    {
        var existing = tile.Find("Minutes");
        var t = existing != null ? existing.GetComponent<TMP_Text>() : null;
        if (t == null)
        {
            var go = new GameObject("Minutes", typeof(RectTransform)); go.transform.SetParent(tile, false);
            t = go.AddComponent<TextMeshProUGUI>(); t.raycastTarget = false;
        }
        t.font = style.altFont; t.fontSize = NerdySpace.Micro; t.color = style.textMuted; t.alignment = TextAlignmentOptions.Left; t.text = text;
        t.rectTransform.anchoredPosition = new Vector2(-30f, -Tile.y / 2f + 22f); t.rectTransform.sizeDelta = new Vector2(Tile.x - 120f, 20f);
        t.textWrappingMode = TextWrappingModes.NoWrap; t.overflowMode = TextOverflowModes.Ellipsis;
    }

    static DashboardTileView View(Transform tile, string id, string lessonId, int chapter, bool openable, DashboardWall wall)
    {
        var view = tile.GetComponent<DashboardTileView>() ?? tile.gameObject.AddComponent<DashboardTileView>();
        view.id = id; view.lessonId = lessonId; view.chapter = chapter; view.openable = openable;
        view.ribbon = tile.Find("Continue ribbon")?.gameObject;
        var shadow = tile.parent.Find(tile.name + " shadow"); view.shadow = shadow != null ? shadow.gameObject : null;
        view.shadowDrop = 10f;
        var button = tile.GetComponent<Button>();
        if (button != null)
        {
            for (int k = button.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(button.onClick, k);
            UnityEventTools.AddStringPersistentListener(button.onClick, new UnityAction<string>(wall.PressTile), id);
            button.interactable = openable;
        }
        return view;
    }

    /// A chapter or coming-soon tile is a clone of its lesson's hero, shadow included, so it carries the same polish.
    static Transform Clone(Transform hero, RectTransform catalog, string name)
    {
        var tile = UnityEngine.Object.Instantiate(hero.gameObject, catalog).transform; tile.name = name;
        var heroShadow = catalog.Find(hero.name + " shadow");
        if (heroShadow != null)
        {
            var shadow = UnityEngine.Object.Instantiate(heroShadow.gameObject, catalog); shadow.name = name + " shadow";
            shadow.transform.SetSiblingIndex(tile.GetSiblingIndex());   // behind its tile
            var link = tile.GetComponent<CardShadowLink>(); if (link != null) link.shadow = shadow;
        }
        var photo = tile.Find("Art/Photo"); if (photo != null) UnityEngine.Object.DestroyImmediate(photo.gameObject);
        return tile;
    }

    static void SetText(Transform tile, string child, string text) { var t = tile.Find(child)?.GetComponent<TMP_Text>(); if (t != null) t.text = text; }

    static void SetBadge(Transform tile, string text, bool playable)
    {
        var badge = tile.Find("Badge"); if (badge == null) return;
        var label = badge.GetComponentInChildren<TMP_Text>(true); if (label != null) label.text = text;
        var rect = (RectTransform)badge; rect.sizeDelta = new Vector2(playable ? 110f : 130f, 26f);
        rect.anchoredPosition = new Vector2(-125f + rect.sizeDelta.x / 2f, rect.anchoredPosition.y);
        var image = badge.GetComponent<Image>();
        if (!playable)
        {
            var grad = badge.Find("Gradient"); if (grad != null) UnityEngine.Object.DestroyImmediate(grad.gameObject);
            var mask = badge.GetComponent<Mask>(); if (mask != null) UnityEngine.Object.DestroyImmediate(mask);
            if (image != null) { image.color = new Color(style.amber.r, style.amber.g, style.amber.b, 0.85f); if (image.sprite != null && image.sprite.border.x > 0f) image.pixelsPerUnitMultiplier = image.sprite.border.x / 13f; }
            if (badge.GetComponent<Outline>() == null) { var o = badge.gameObject.AddComponent<Outline>(); o.effectColor = new Color(style.amber.r, style.amber.g, style.amber.b, 0.9f); o.effectDistance = new Vector2(1.2f, -1.2f); }
        }
    }

    static void GreyOut(Transform tile)
    {
        var panel = tile.GetComponent<Image>(); if (panel != null) panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, 0.75f);
        var art = tile.Find("Art")?.GetComponent<Image>(); if (art != null) art.color = new Color(1f, 1f, 1f, 0.35f);
        var inner = tile.Find("Arrow inner")?.GetComponent<Image>(); if (inner != null) inner.color = new Color(style.surface.r, style.surface.g, style.surface.b, 0.75f);
        foreach (var name in new[] { "Arrow ring", "Arrow inner", "Arrow" }) { var a = tile.Find(name); if (a != null) a.gameObject.SetActive(false); }
        var hover = tile.GetComponent<TileHover>(); if (hover != null) UnityEngine.Object.DestroyImmediate(hover);
        var focus = tile.GetComponent<FocusPointer>(); if (focus != null) UnityEngine.Object.DestroyImmediate(focus);
    }

    // ---- chapter art, cropped from the seated-head renders --------------------------------------------------------
    static bool Photo(Transform tile, string lessonId, int chapter)
    {
        var art = tile.Find("Art"); if (art == null) return false;
        string src = RenderPath(lessonId, chapter);
        string assetPath = ArtDir + "/" + lessonId + "-" + chapter + ".png";
        string full = Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath);
        if (!File.Exists(full))
        {
            if (src == null || !File.Exists(src)) return false;
            Crop(src, full, 584, 166);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.mipmapEnabled = true; importer.alphaIsTransparency = false;
            importer.SaveAndReimport();
        }
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath); if (sprite == null) return false;
        var photo = new GameObject("Photo", typeof(RectTransform), typeof(Image)); photo.transform.SetParent(art, false);
        var pr = (RectTransform)photo.transform; pr.anchoredPosition = new Vector2(0f, -6f); pr.sizeDelta = new Vector2(Tile.x - 24f, Tile.y / 2f - 12f);
        var img = photo.GetComponent<Image>(); img.sprite = sprite; img.type = Image.Type.Simple; img.preserveAspect = false; img.raycastTarget = false; img.color = new Color(1f, 1f, 1f, 0.95f);
        var artImage = art.GetComponent<Image>(); if (artImage != null) artImage.color = new Color(1f, 1f, 1f, 0.55f);
        return true;
    }

    static string RenderPath(string lessonId, int chapter)
    {
        string root = Path.Combine(Path.GetDirectoryName(Application.dataPath), "../artifacts");
        switch (lessonId)
        {
            case "cargo_crew_fractions": return Path.Combine(root, "dock7", chapter + "a-start.png");
            case "neighborhood_cafe_division": return Path.Combine(root, "cafe", chapter == 5 ? "5s1a-start.png" : chapter + "a-start.png");
            case "community_garden_multiplication": return Path.Combine(root, "garden", chapter + "a-start.png");
            default: return null;
        }
    }

    /// The middle of the render (the table and what is on it), resampled to the tile's art size.
    static void Crop(string src, string dst, int w, int h)
    {
        var tex = new Texture2D(2, 2); tex.LoadImage(File.ReadAllBytes(src));
        const float x0 = 0.22f, x1 = 0.78f, y0 = 0.22f, y1 = 0.72f;   // fractions of the source, bottom-up
        var outTex = new Texture2D(w, h, TextureFormat.RGB24, false);
        var pixels = new Color[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                pixels[y * w + x] = tex.GetPixelBilinear(x0 + (x1 - x0) * (x + 0.5f) / w, y0 + (y1 - y0) * (y + 0.5f) / h);
        outTex.SetPixels(pixels); outTex.Apply();
        File.WriteAllBytes(dst, outTex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(outTex);
    }

    // ---- the bar under the wall ----------------------------------------------------------------------------------
    static void BuildToolbar(NerdyDirector n, RectTransform catalog, DashboardWall wall, LoungeRoom lounge)
    {
        var bar = new GameObject("Toolbar", typeof(RectTransform)).GetComponent<RectTransform>(); bar.SetParent(catalog, false);
        bar.anchoredPosition = new Vector2(0f, -196f); bar.sizeDelta = new Vector2(1240f, NerdySpace.PillHeight);
        var template = n.helpButton != null ? n.helpButton.gameObject : throw new InvalidOperationException("Help pill missing.");

        Button Pill(string name, string text, float x, float w, UnityEngine.Object target, string method, string arg = null)
        {
            var go = UnityEngine.Object.Instantiate(template, bar); go.name = name;
            var r = (RectTransform)go.transform; r.anchoredPosition = new Vector2(x, 0f); r.sizeDelta = new Vector2(w, NerdySpace.PillHeight);
            var img = go.GetComponent<Image>(); if (img != null && img.sprite != null && img.sprite.border.x > 0f) img.pixelsPerUnitMultiplier = img.sprite.border.x / (NerdySpace.PillHeight / 2f);
            var label = go.GetComponentInChildren<TMP_Text>(true); if (label != null) { label.text = text; label.rectTransform.anchoredPosition = Vector2.zero; label.rectTransform.sizeDelta = new Vector2(w - 12f, 32f); }
            var b = go.GetComponent<Button>();
            for (int k = b.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(b.onClick, k);
            if (arg != null) UnityEventTools.AddStringPersistentListener(b.onClick, (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>), target, method), arg);
            else UnityEventTools.AddPersistentListener(b.onClick, (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, method));
            return b;
        }

        wall.featuredButton = Pill("Featured", "Featured", -505f, 130f, wall, "SetFilter", "featured");
        wall.newestButton = Pill("Newest", "Newest", -360f, 130f, wall, "SetFilter", "newest");
        wall.mostViewedButton = Pill("Most viewed", "Most viewed", -205f, 150f, wall, "SetFilter", "most_viewed");
        wall.filterPills = new[] { wall.featuredButton.GetComponent<Image>(), wall.newestButton.GetComponent<Image>(), wall.mostViewedButton.GetComponent<Image>() };
        wall.filterChosen = n.languageChosen; wall.filterIdle = n.languageIdle;

        wall.previousButton = Pill("Previous page", "‹", -20f, NerdySpace.PillHeight, wall, "PreviousPage");
        wall.nextButton = Pill("Next page", "›", 80f, NerdySpace.PillHeight, wall, "NextPage");
        var page = new GameObject("Page", typeof(RectTransform)).AddComponent<TextMeshProUGUI>(); page.transform.SetParent(bar, false);
        page.font = style.altFont; page.fontSize = NerdySpace.Label; page.color = style.textMuted; page.alignment = TextAlignmentOptions.Center; page.text = "1 / 3"; page.raycastTarget = false;
        page.rectTransform.anchoredPosition = new Vector2(30f, 0f); page.rectTransform.sizeDelta = new Vector2(56f, 30f);
        wall.pageLabel = page;

        // The scenery selector, beside the gear (the gear sits on the assistant bar right under this row).
        var scenery = new GameObject("Scenery label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>(); scenery.transform.SetParent(bar, false);
        scenery.font = style.altFont; scenery.fontSize = NerdySpace.Label; scenery.color = style.textMuted; scenery.alignment = TextAlignmentOptions.Right; scenery.text = "Scenery"; scenery.raycastTarget = false;
        scenery.rectTransform.anchoredPosition = new Vector2(230f, 0f); scenery.rectTransform.sizeDelta = new Vector2(110f, 30f);
        Pill("Your room", "Your room", 365f, 130f, lounge, "ChooseYourRoom");
        Pill("Nerdy lounge", "Nerdy lounge", 515f, 150f, lounge, "ChooseNerdyLounge");
    }
}
