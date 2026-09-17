using System;
using System.Linq;
using Airlift.Guide;
using Airlift.Onboarding;
using Airlift.Presentation;
using Airlift.Welcome;
using Oculus.Interaction.Editor.QuickActions;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// CC-P0-05/06/07: builds the Nerdy entry flow into CargoCrew: consent card, voice welcome with
// answer chips, three lesson cards, guide HUD, and wiring into the existing Cargo workbench.
public static class CreateNerdyWelcome
{
    static NerdyStyle S;
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var existing = GameObject.Find("Nerdy welcome - world locked");
        if (existing != null)
        {
            var d0 = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
            var back0 = d0.transform.Find("Lesson interface").GetComponentsInChildren<Button>(true).First(b => b.name == "Back to lessons");
            for (int i = back0.onClick.GetPersistentEventCount() - 1; i >= 0; i--) if (back0.onClick.GetPersistentTarget(i) is NerdyDirector) UnityEventTools.RemovePersistentListener(back0.onClick, i);
            UnityEngine.Object.DestroyImmediate(existing);
            var oldHudCanvas = d0.transform.Find("Guide HUD canvas"); if (oldHudCanvas != null) UnityEngine.Object.DestroyImmediate(oldHudCanvas.gameObject);
        }
        S = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        if (S == null || S.displayFont == null) throw new InvalidOperationException("Run ApplyNerdyStyle first.");
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        var logo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Airlift/Branding/nerdy-logo-green.png") ?? LogoSprite();

        var root = new GameObject("Nerdy welcome - world locked");
        var director = root.AddComponent<NerdyDirector>();
        director.onboarding = d; director.head = d.head; director.welcomeAnchor = root.transform;

        // Guide session + playback
        var guideGo = new GameObject("Nerdy guide"); guideGo.transform.SetParent(root.transform, false);
        var playback = guideGo.AddComponent<AudioPlayback>(); var session = guideGo.AddComponent<GuideSession>();
        session.playback = playback; session.mintUrl = GuideEndpoints.MintUrl; session.build = PlayerSettings.bundleVersion;
        director.guide = session; director.playback = playback;

        // Canvas
        var ui = new GameObject("Welcome interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        ui.transform.SetParent(root.transform, false); ui.transform.localPosition = new Vector3(0, 0.05f, 0); ui.transform.localScale = Vector3.one * 0.001f;
        ui.GetComponent<RectTransform>().sizeDelta = new Vector2(960, 640);
        var canvas = ui.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = d.head.GetComponent<Camera>();
        var uiT = ui.transform;

        // ---- Consent card ----
        var consent = Panel("Consent", uiT, Vector2.zero, new Vector2(760, 470), S.card, S.surface);
        Image(consent.transform, "Logo", logo, new Vector2(0, 170), new Vector2(220, 90));
        Text(consent.transform, "Heading", "Meet your Nerdy guide", S.displayFont, 34, S.text, new Vector2(0, 86), new Vector2(680, 50), TextAlignmentOptions.Center);
        Text(consent.transform, "Body", "Your guide talks with you. With the microphone on, what you say is sent to OpenAI for live transcription and is not stored by Nerdy. Nothing is saved except a few tags about what you like.\n\nThis preview is for adult testers.", S.bodyFont, 17, S.textMuted, new Vector2(0, -10), new Vector2(640, 150), TextAlignmentOptions.Center);
        Pill(consent.transform, "Allow voice", "I'm an adult tester · turn on the mic", true, new Vector2(-150, -170), new Vector2(360, 56), director.ConsentAllowVoice);
        Pill(consent.transform, "No voice", "Continue without voice", false, new Vector2(170, -170), new Vector2(260, 56), director.ConsentNoVoice);
        consent.transform.localScale = Vector3.one * 1.45f;
        director.consentRoot = consent;

        // ---- Welcome (chips) ----
        var welcome = Panel("Welcome", uiT, new Vector2(0, 40), new Vector2(940, 440), S.card, S.surface);
        Eyebrow(welcome.transform, "WELCOME", new Vector2(0, 190));
        Text(welcome.transform, "Heading", "Hello to <gradient=\"NerdySpectrum\">Nerdy AI</gradient>", S.displayFont, 36, S.text, new Vector2(0, 150), new Vector2(820, 50), TextAlignmentOptions.Center);
        Text(welcome.transform, "Hint", "Tap your answers below. Your guide talks with you inside the lesson.", S.altFont, 15, S.textMuted, new Vector2(0, 112), new Vector2(820, 30), TextAlignmentOptions.Center);
        ChipRow(welcome.transform, director, "Age", 60, new[] {
            ("Under 10", "I'm under ten.", "{\"ageBand\":\"under_10\"}"), ("10 to 13", "I'm between ten and thirteen.", "{\"ageBand\":\"10_to_13\"}"),
            ("14 to 17", "I'm between fourteen and seventeen.", "{\"ageBand\":\"14_to_17\"}"), ("Adult", "I'm an adult.", "{\"ageBand\":\"adult\"}"),
            ("Rather not say", "I'd rather not say my age.", "{\"ageBand\":\"prefer_not_to_say\"}") });
        ChipRow(welcome.transform, director, "I enjoy", -5, new[] {
            ("Sports", "I enjoy sports.", "{\"interests\":[\"sports\"]}"), ("Games", "I enjoy video games.", "{\"interests\":[\"games\"]}"), ("Music", "I enjoy music.", "{\"interests\":[\"music\"]}"),
            ("Art", "I enjoy art.", "{\"interests\":[\"art\"]}"), ("Science", "I enjoy science.", "{\"interests\":[\"science\"]}"), ("Animals", "I enjoy animals.", "{\"interests\":[\"animals\"]}") });
        ChipRow(welcome.transform, director, "I'm here to", -70, new[] {
            ("Catch up", "I want to catch up in math.", "{\"goal\":\"catch_up\"}"), ("Get ahead", "I want to get ahead in math.", "{\"goal\":\"get_ahead\"}"),
            ("Homework help", "I need help with homework.", "{\"goal\":\"homework_help\"}"), ("Just curious", "I'm just curious.", "{\"goal\":\"curious\"}"),
            ("Teacher or parent", "I'm a teacher or a parent looking around.", "{\"goal\":\"teacher_or_parent\"}") });
        director.skipButton = Pill(welcome.transform, "Skip", "Skip to lessons", false, new Vector2(0, -160), new Vector2(240, 50), null);
        director.welcomeRoot = welcome;

        // ---- Catalog ----
        var catalog = new GameObject("Catalog", typeof(RectTransform)); catalog.transform.SetParent(uiT, false); catalog.GetComponent<RectTransform>().sizeDelta = new Vector2(960, 640);
        Eyebrow(catalog.transform, "LESSONS", new Vector2(0, 250));
        Text(catalog.transform, "Heading", "Pick a <gradient=\"NerdySpectrum\">lesson</gradient>", S.displayFont, 40, S.text, new Vector2(0, 205), new Vector2(900, 56), TextAlignmentOptions.Center);
        float[] xs = { -305, 0, 305 };
        for (int i = 0; i < LessonCatalog.Cards.Length; i++) Card(catalog.transform, director, LessonCatalog.Cards[i], new Vector2(xs[i], -30));
        director.catalogRoot = catalog;

        // ---- HUD ----
        var hud = Panel("Guide HUD", uiT, new Vector2(0, -262), new Vector2(920, 104), S.card, S.surface);
        hud.GetComponent<Image>().color = new Color(S.surface.r, S.surface.g, S.surface.b, 0.92f);
        var hudMask = hud.AddComponent<Mask>(); hudMask.showMaskGraphic = true;
        var glass = Image(hud.transform, "Glass", S.glass, Vector2.zero, new Vector2(920, 104)); glass.type = UnityEngine.UI.Image.Type.Sliced; glass.raycastTarget = false;
        var orb = Image(hud.transform, "Orb", S.pill, new Vector2(-420, 12), new Vector2(36, 36)); orb.color = S.lavender; orb.raycastTarget = false; director.orb = orb;
        director.captionText = Text(hud.transform, "Caption", "", S.bodyFont, 18, S.text, new Vector2(-105, 16), new Vector2(560, 52), TextAlignmentOptions.Left);
        director.userText = Text(hud.transform, "You said", "", S.altFont, 13, S.textMuted, new Vector2(-105, -30), new Vector2(560, 24), TextAlignmentOptions.Left);
        director.stateText = Text(hud.transform, "State", "", S.altFont, 12, S.textMuted, new Vector2(-420, -30), new Vector2(120, 24), TextAlignmentOptions.Center);
        director.muteButton = Pill(hud.transform, "Mute", "Mute", false, new Vector2(305, 0), new Vector2(100, 44), null); director.muteLabel = director.muteButton.GetComponentInChildren<TMP_Text>();
        director.helpButton = Pill(hud.transform, "Help", "Help", false, new Vector2(415, 0), new Vector2(100, 44), null);
        director.repeatButton = Pill(hud.transform, "Repeat", "Again", false, new Vector2(195, 0), new Vector2(100, 44), null);
        director.hudRoot = hud; director.hudWelcomeCanvas = uiT; director.hudWelcomePosition = new Vector2(0, -262);

        // Second canvas above the workbench: the assistant card rides with the table in the lesson.
        var stationUi = new GameObject("Guide HUD canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        stationUi.transform.SetParent(d.transform, false); stationUi.transform.localPosition = new Vector3(0, 0.77f, 0.24f); stationUi.transform.localScale = Vector3.one * 0.001f;
        stationUi.GetComponent<RectTransform>().sizeDelta = new Vector2(920, 104);
        var sc = stationUi.GetComponent<Canvas>(); sc.renderMode = RenderMode.WorldSpace; sc.worldCamera = d.head.GetComponent<Camera>();
        director.hudStationCanvas = stationUi.transform;
        QuickActionsAPI.AddRayCanvasInteraction(stationUi);

        // Carry handle under the welcome panel: same feel as the table handle.
        var lavender = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/NerdyLavender.mat");
        if (lavender == null) { lavender = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "NerdyLavender", color = S.lavender }; lavender.SetFloat("_Smoothness", 0.5f); AssetDatabase.CreateAsset(lavender, "Assets/Airlift/Materials/NerdyLavender.mat"); }
        var handleMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Airlift/Meshes/RoundedBox-0_18x0_035x0_035-r0_012.asset");
        var bar = new GameObject("Panel handle"); bar.transform.SetParent(root.transform, false); bar.transform.localPosition = new Vector3(0f, -0.31f, 0f);
        bar.AddComponent<MeshFilter>().sharedMesh = handleMesh; bar.AddComponent<MeshRenderer>().sharedMaterial = lavender;
        bar.AddComponent<BoxCollider>().size = new Vector3(0.2f, 0.05f, 0.05f);
        var body = bar.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
        var grab = bar.AddComponent<Oculus.Interaction.Grabbable>(); grab.InjectOptionalTargetTransform(root.transform); grab.MaxGrabPoints = 2;
        var carry = bar.AddComponent<TableCarryTransformer>(); carry.head = d.head;
        var two = bar.AddComponent<Oculus.Interaction.TwoGrabPlaneTransformer>(); two.InjectOptionalPlaneTransform(root.transform);
        two.InjectOptionalConstraints(new Oculus.Interaction.TwoGrabPlaneTransformer.TwoGrabPlaneConstraints {
            MinScale = new Oculus.Interaction.FloatConstraint { Constrain = true, Value = 0.6f }, MaxScale = new Oculus.Interaction.FloatConstraint { Constrain = true, Value = 1.8f },
            MinY = new Oculus.Interaction.FloatConstraint(), MaxY = new Oculus.Interaction.FloatConstraint() });
        grab.InjectOptionalOneGrabTransformer(carry); grab.InjectOptionalTwoGrabTransformer(two);
        QuickActionsAPI.AddGrabInteraction(bar);
        var panelHandle = root.AddComponent<TableHandle>();
        panelHandle.grabbable = grab; panelHandle.stationRoot = root.transform; panelHandle.handleInteractable = bar.GetComponentInChildren<Oculus.Interaction.GrabInteractable>();

        // ---- Station wiring ----
        var station = d.transform;
        director.stationVisuals = new[] { "Workbench", "Cargo terminal", "Table handle", "Lesson interface" }.Select(n => station.Find(n)?.gameObject).Where(g => g != null).ToArray();
        var back = station.Find("Lesson interface").GetComponentsInChildren<Button>(true).First(b => b.name == "Back to lessons");
        UnityEventTools.AddPersistentListener(back.onClick, director.OnLessonBack);
        QuickActionsAPI.AddRayCanvasInteraction(ui);
        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        return "Nerdy welcome built: consent, welcome chips (" + welcome.GetComponentsInChildren<Button>(true).Length + " buttons), 3 cards, HUD, guide session (mint " + session.mintUrl + "), station visuals " + director.stationVisuals.Length + ".";
    }

    // ---- kit ----
    static GameObject Panel(string name, Transform parent, Vector2 pos, Vector2 size, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchoredPosition = pos; r.sizeDelta = size;
        var img = go.GetComponent<Image>(); img.sprite = sprite; img.type = UnityEngine.UI.Image.Type.Sliced; img.color = color; img.raycastTarget = false; return go;
    }
    static Image Image(Transform parent, string name, Sprite sprite, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchoredPosition = pos; r.sizeDelta = size;
        var img = go.GetComponent<Image>(); img.sprite = sprite; img.preserveAspect = true; img.raycastTarget = false; return img;
    }
    static TMP_Text Text(Transform parent, string name, string text, TMP_FontAsset font, float size, Color color, Vector2 pos, Vector2 rect, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>(); t.font = font; t.fontSize = size; t.color = color; t.text = text; t.alignment = align; t.raycastTarget = false;
        t.rectTransform.anchoredPosition = pos; t.rectTransform.sizeDelta = rect; t.textWrappingMode = TextWrappingModes.Normal; t.richText = true; return t;
    }
    static void Eyebrow(Transform parent, string text, Vector2 pos)
    {
        var t = Text(parent, "Eyebrow", text, S.altFont, 12, S.lavender, pos, new Vector2(600, 20), TextAlignmentOptions.Center); t.characterSpacing = 14;
    }
    static Button Pill(Transform parent, string name, string label, bool primary, Vector2 pos, Vector2 size, UnityAction action)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchoredPosition = pos; r.sizeDelta = size;
        var img = go.GetComponent<Image>(); img.sprite = S.pill; img.type = UnityEngine.UI.Image.Type.Sliced; img.color = primary ? Color.white : new Color(1, 1, 1, 0.10f);
        if (primary) { var grad = Image(go.transform, "Gradient", S.brandGradient, Vector2.zero, size); grad.preserveAspect = false; grad.type = UnityEngine.UI.Image.Type.Simple; var mask = go.AddComponent<Mask>(); mask.showMaskGraphic = false; }
        else { var outline = go.AddComponent<Outline>(); outline.effectColor = new Color(S.line.r, S.line.g, S.line.b, 0.9f); outline.effectDistance = new Vector2(1.2f, -1.2f); }
        var b = go.GetComponent<Button>(); b.targetGraphic = img; var colors = b.colors; colors.highlightedColor = new Color(0.9f, 0.9f, 1f); colors.pressedColor = new Color(0.75f, 0.75f, 0.95f); b.colors = colors;
        if (action != null) UnityEventTools.AddPersistentListener(b.onClick, action);
        var lbl = Text(go.transform, "Label", label, S.displayFont, size.y > 50 ? 16 : 14, Color.white, Vector2.zero, size - new Vector2(12, 4), TextAlignmentOptions.Center);
        lbl.textWrappingMode = TextWrappingModes.NoWrap; lbl.overflowMode = TextOverflowModes.Overflow;
        go.AddComponent<FocusPointer>();
        return b;
    }
    static void ChipRow(Transform parent, NerdyDirector director, string label, float y, (string chip, string spoken, string json)[] chips)
    {
        Text(parent, label + " label", label, S.altFont, 14, S.textMuted, new Vector2(-350, y), new Vector2(180, 30), TextAlignmentOptions.Right);
        float x = -250f;
        foreach (var (chip, spoken, json) in chips)
        {
            float w = 30 + chip.Length * 7.8f;
            var b = Pill(parent, "Chip " + chip, chip, false, new Vector2(x + w / 2, y), new Vector2(w, 40), null);
            UnityEventTools.AddStringPersistentListener(b.onClick, new UnityAction<string>(director.AnswerChipPacked), spoken + "|" + json);
            x += w + 8;
        }
    }
    static void Card(Transform parent, NerdyDirector director, LessonCard card, Vector2 pos)
    {
        var size = new Vector2(280, 330);
        var go = new GameObject("Card " + card.Id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Mask)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchoredPosition = pos; r.sizeDelta = size;
        var img = go.GetComponent<Image>(); img.sprite = S.card; img.type = UnityEngine.UI.Image.Type.Sliced; img.color = card.Playable ? S.surface : new Color(S.surface.r, S.surface.g, S.surface.b, 0.75f);
        go.GetComponent<Mask>().showMaskGraphic = true;
        var art = Image(go.transform, "Art", S.spectrumGradient, new Vector2(0, 95), new Vector2(280, 140)); art.preserveAspect = false; art.type = UnityEngine.UI.Image.Type.Simple; art.color = card.Playable ? new Color(1, 1, 1, 0.9f) : new Color(1, 1, 1, 0.35f);
        var fade = Image(go.transform, "Fade", S.brandGradient, new Vector2(0, 40), new Vector2(280, 60)); fade.preserveAspect = false; fade.type = UnityEngine.UI.Image.Type.Simple; fade.color = new Color(S.surface.r, S.surface.g, S.surface.b, 0.85f);
        var badgeGo = Pill(go.transform, "Badge", card.Playable ? card.Subject.ToUpper() : "COMING SOON", card.Playable, new Vector2(-70, 30), new Vector2(card.Playable ? 110 : 130, 26), null).gameObject;
        UnityEngine.Object.DestroyImmediate(badgeGo.GetComponent<FocusPointer>()); UnityEngine.Object.DestroyImmediate(badgeGo.GetComponent<Button>());
        var badgeLabel = badgeGo.GetComponentInChildren<TMP_Text>(); badgeLabel.font = S.altBold; badgeLabel.fontSize = 10; badgeLabel.characterSpacing = 6;
        if (!card.Playable) badgeGo.GetComponent<Image>().color = new Color(S.amber.r, S.amber.g, S.amber.b, 0.85f);
        Text(go.transform, "Title", card.Title, S.displayFont, 24, S.text, new Vector2(0, -32), new Vector2(240, 62), TextAlignmentOptions.TopLeft);
        Text(go.transform, "Description", card.Description, S.bodyFont, 13, S.textMuted, new Vector2(-20, -108), new Vector2(200, 64), TextAlignmentOptions.TopLeft);
        var ring = Image(go.transform, "Arrow ring", S.pill, new Vector2(103, -118), new Vector2(34, 34)); ring.color = Color.white;
        var inner = Image(go.transform, "Arrow inner", S.pill, new Vector2(103, -118), new Vector2(31, 31)); inner.color = card.Playable ? S.surface : new Color(S.surface.r, S.surface.g, S.surface.b, 0.75f);
        Text(go.transform, "Arrow", "→", S.bodyFont, 16, Color.white, new Vector2(103, -117), new Vector2(34, 34), TextAlignmentOptions.Center);
        var b = go.GetComponent<Button>(); b.targetGraphic = img; UnityEventTools.AddStringPersistentListener(b.onClick, new UnityAction<string>(director.SelectCard), card.Id);
        go.AddComponent<FocusPointer>();
    }
    static Sprite LogoSprite()
    {
        string path = "Assets/Airlift/Branding/nerdy-logo-green.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? throw new InvalidOperationException("Logo sprite import failed.");
    }
}
