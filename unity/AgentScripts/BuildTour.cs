using System;
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

// The pointing tour (owner, approved on the canvas 2026-09-18): replaces the rundown card. On the wall: a header
// (step count + Skip) top-right, and a pointer overlay (cyan ring + bouncing arrow) drawn last on the welcome
// canvas. The old card, block and handle gate are removed; the controller helper stays for the trigger and B stops.
// Idempotent. Run after BuildDashboard, BuildNavArrows and CompactAssistantBar. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/BuildTour.cs --entry BuildTour.Run
public static class BuildTour
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var settings = n.GetComponent<LoungeSettings>() ?? throw new InvalidOperationException("Run BuildSettingsPanel first.");
        var wall = n.catalogRoot.GetComponent<DashboardWall>() ?? throw new InvalidOperationException("Run BuildDashboard first.");
        if (n.navBack == null) throw new InvalidOperationException("Run BuildNavArrows first.");
        var canvas = (RectTransform)n.hudWelcomeCanvas;
        var catalog = (RectTransform)n.catalogRoot.transform;
        var rundown = n.GetComponent<LoungeRundown>() ?? n.gameObject.AddComponent<LoungeRundown>();

        // The card era goes: the card, the block and the handle gate. The settings card no longer needs to hide it.
        foreach (var name in new[] { "Rundown card" }) { var old = canvas.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
        foreach (var name in new[] { "Rundown block" }) { var old = n.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
        settings.hideWhileOpen = (settings.hideWhileOpen ?? new GameObject[0]).Where(g => g != null).ToArray();

        // Header: step count and Skip, top-right of the wall, clear of the heading.
        foreach (var parent in new[] { (Transform)catalog, canvas }) { var oldHeader = parent.Find("Tour header"); if (oldHeader != null) UnityEngine.Object.DestroyImmediate(oldHeader.gameObject); }
        // On the canvas, not the wall: the tour starts on the welcome card and runs through the questions too.
        var header = new GameObject("Tour header", typeof(RectTransform)).GetComponent<RectTransform>(); header.SetParent(canvas, false);
        header.anchoredPosition = new Vector2(NerdySpace.PanelWidth / 2f - 150f, 330f); header.sizeDelta = Vector2.zero;
        var label = new GameObject("Step", typeof(RectTransform)).AddComponent<TextMeshProUGUI>(); label.transform.SetParent(header, false);
        label.font = style.altBold; label.fontSize = NerdySpace.Label; label.color = style.lavender; label.characterSpacing = 8; label.text = "1 OF 6"; label.alignment = TextAlignmentOptions.Right; label.raycastTarget = false;
        label.rectTransform.anchoredPosition = new Vector2(-70f, 0f); label.rectTransform.sizeDelta = new Vector2(100f, 30f);
        var skipGo = UnityEngine.Object.Instantiate(n.helpButton.gameObject, header); skipGo.name = "Skip tour";
        var skipRect = (RectTransform)skipGo.transform; skipRect.anchoredPosition = new Vector2(40f, 0f); skipRect.sizeDelta = new Vector2(90f, NerdySpace.PillHeight);
        var skipImg = skipGo.GetComponent<Image>(); if (skipImg != null && skipImg.sprite != null) skipImg.pixelsPerUnitMultiplier = skipImg.sprite.border.x / (NerdySpace.PillHeight / 2f);
        var skipLabel = skipGo.GetComponentInChildren<TMP_Text>(true); if (skipLabel != null) { skipLabel.text = "Skip"; skipLabel.rectTransform.anchoredPosition = Vector2.zero; skipLabel.rectTransform.sizeDelta = new Vector2(80f, 32f); }
        var skip = skipGo.GetComponent<Button>();
        for (int k = skip.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(skip.onClick, k);
        UnityEventTools.AddPersistentListener(skip.onClick, (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), rundown, "Skip"));

        // Pointer overlay: a cyan stroke ring and a lavender arrow, on the canvas, never a raycast target.
        var oldPointer = canvas.Find("Tour pointer"); if (oldPointer != null) UnityEngine.Object.DestroyImmediate(oldPointer.gameObject);
        var pointer = new GameObject("Tour pointer", typeof(RectTransform)).GetComponent<RectTransform>(); pointer.SetParent(canvas, false);
        pointer.anchoredPosition = Vector2.zero; pointer.sizeDelta = Vector2.zero;
        var strokeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Airlift/Sprites/NerdyStroke.png");
        var ring = new GameObject("Ring", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>(); ring.SetParent(pointer, false);
        var ringImg = ring.GetComponent<Image>(); ringImg.sprite = strokeSprite; ringImg.type = Image.Type.Sliced; ringImg.color = style.cyan; ringImg.raycastTarget = false;
        ringImg.pixelsPerUnitMultiplier = 2.2f;   // a firmer line than the hairline card stroke
        ring.sizeDelta = new Vector2(300f, 200f);
        var arrowT = new GameObject("Arrow", typeof(RectTransform)).AddComponent<TextMeshProUGUI>(); arrowT.transform.SetParent(pointer, false);
        arrowT.font = style.displayFont; arrowT.fontSize = 40f; arrowT.color = style.lavender; arrowT.text = "↓"; arrowT.alignment = TextAlignmentOptions.Center; arrowT.raycastTarget = false;
        arrowT.rectTransform.sizeDelta = new Vector2(60f, 60f);
        pointer.SetAsLastSibling();

        // Wire it.
        rundown.wall = wall; rundown.pointerRoot = pointer; rundown.ring = ring; rundown.arrow = arrowT.rectTransform;
        rundown.header = header.gameObject; rundown.stepLabel = label; rundown.skipButton = skip;
        var bar = (RectTransform)n.hudRoot.transform;
        rundown.barControls = new[] { n.musicButton, bar.Find("Settings gear")?.GetComponent<Button>(), n.helpButton }.Where(b => b != null).ToArray();
        var toolbar = catalog.Find("Toolbar");
        rundown.pagerAndScenery = new[] { "Previous page", "Page", "Next page", "Scenery label", "Your room", "Nerdy lounge" }.Select(name => toolbar.Find(name) as RectTransform).Where(r => r != null).ToArray();
        rundown.backArrow = n.navBack; rundown.nextArrow = n.navNext;
        rundown.gearButton = bar.Find("Settings gear")?.GetComponent<Button>();
        var consent = n.consentRoot.transform;
        rundown.consentPills = new[] { consent.Find("Allow voice")?.GetComponent<Button>(), consent.Find("No voice")?.GetComponent<Button>() }.Where(b => b != null).ToArray();
        rundown.settings = settings;
        rundown.questionsSkip = n.welcomeRoot.transform.Find("Skip")?.GetComponent<Button>();
        header.SetAsLastSibling(); pointer.SetAsLastSibling();
        header.gameObject.SetActive(false); pointer.gameObject.SetActive(false);

        foreach (var o in new UnityEngine.Object[] { n, rundown, settings }) EditorUtility.SetDirty(o);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "host tour: header + Skip on the canvas, pointer overlay, " + rundown.consentPills.Length + " consent pills, gear " + (rundown.gearButton != null) + ", " + rundown.barControls.Length + " bar controls, " + rundown.pagerAndScenery.Length + " pager/scenery";
    }
}
