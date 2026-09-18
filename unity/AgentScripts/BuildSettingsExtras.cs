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
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// CC-FD-09. The rest of the settings card: Voice guide, Captions, Haptic feedback, Show button labels, the three
// volumes, Replay the rundown, Clear saved data (two presses), Reset settings and the version line. The rows that
// BuildSettingsPanel moved in (Voice language, Where you learn, Dee) are MOVED into the left column, never rebuilt.
// Idempotent. Run after BuildSettingsPanel, before ApplySpatialStandards. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/BuildSettingsExtras.cs --entry BuildSettingsExtras.Run
public static class BuildSettingsExtras
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const float LeftX = -260f, RightX = 400f;
    static NerdyStyle style;
    static GameObject pillTemplate;

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var settings = n.GetComponent<LoungeSettings>() ?? throw new InvalidOperationException("Run BuildSettingsPanel first.");
        var panel = (RectTransform)settings.panel.transform;
        pillTemplate = n.helpButton != null ? n.helpButton.gameObject : throw new InvalidOperationException("Help pill missing.");

        var sp = panel.GetComponent<SettingsPanel>() ?? panel.gameObject.AddComponent<SettingsPanel>();
        sp.lounge = n.lounge; sp.labels.Clear();
        n.settingsPanel = sp;

        var previous = panel.Find("Extras"); if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var extras = new GameObject("Extras", typeof(RectTransform)).GetComponent<RectTransform>(); extras.SetParent(panel, false); extras.sizeDelta = Vector2.zero;

        // Left column: the rows that already exist, then volume.
        var heading = panel.Find("Heading") as RectTransform; if (heading != null) heading.anchoredPosition = new Vector2(0f, 300f);
        Move(panel, "Language row", new Vector2(LeftX, 190f));
        Move(panel, "Scenery row", new Vector2(LeftX, 110f));
        Move(panel, "Guide row", new Vector2(LeftX, 30f));
        var labelSource = panel.Find("Language row")?.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.GetComponent<Button>() == null && t.transform.parent.name == "Language row");
        var volume = Row(extras, "Volume row", new Vector2(LeftX, -50f), 640f, "Volume", labelSource);
        float vx = -640f / 2f + 220f + 70f;
        foreach (var bus in new[] { "Voice", "Music", "Effects" })
        {
            var b = Pill(volume, bus + " volume", bus + " 100%", new Vector2(vx, 0f), 140f, sp, "CycleVolume", bus);
            sp.labels.Add(new SettingsPanel.LabelRef { key = bus, label = b.GetComponentInChildren<TMP_Text>(true) });
            vx += 140f + 12f;
        }

        // Right column: four switches.
        float y = 190f;
        foreach (var (key, text) in new[] { ("VoiceGuide", "Voice guide"), ("Captions", "Captions"), ("Haptics", "Haptic feedback"), ("ButtonLabels", "Show button labels") })
        {
            var row = Row(extras, text + " row", new Vector2(RightX, y), 380f, text, labelSource);
            var b = Pill(row, key, "On", new Vector2(380f / 2f - 59f - 10f, 0f), 118f, sp, "Toggle", key);
            sp.labels.Add(new SettingsPanel.LabelRef { key = key, label = b.GetComponentInChildren<TMP_Text>(true) });
            y -= 80f;
        }

        // Actions, one row across both columns.
        var actions = new GameObject("Actions row", typeof(RectTransform)).GetComponent<RectTransform>(); actions.SetParent(extras, false); actions.anchoredPosition = new Vector2(0f, -150f); actions.sizeDelta = new Vector2(900f, NerdySpace.PillHeight);
        Pill(actions, "Replay the rundown", "Replay the tour", new Vector2(-300f, 0f), 250f, sp, "ReplayRundown");
        var clear = Pill(actions, "Clear saved data", SettingsPanel.ClearLabel, new Vector2(0f, 0f), 250f, sp, "ClearSavedData");
        sp.labels.Add(new SettingsPanel.LabelRef { key = "Clear", label = clear.GetComponentInChildren<TMP_Text>(true) });
        Pill(actions, "Reset settings", "Reset settings", new Vector2(300f, 0f), 250f, sp, "ResetToDefaults");

        // Done and the version line.
        var done = panel.Find("Done") as RectTransform; if (done != null) done.anchoredPosition = new Vector2(0f, -240f);
        var version = new GameObject("Version", typeof(RectTransform)).AddComponent<TextMeshProUGUI>(); version.transform.SetParent(extras, false);
        version.font = style.altFont; version.fontSize = NerdySpace.Micro; version.color = style.textMuted; version.alignment = TextAlignmentOptions.Center; version.raycastTarget = false;
        version.text = "Nerdy AI+VR " + Application.version; version.rectTransform.anchoredPosition = new Vector2(0f, -300f); version.rectTransform.sizeDelta = new Vector2(600f, 22f);
        sp.versionText = version;

        foreach (var o in new UnityEngine.Object[] { n, sp, settings }) EditorUtility.SetDirty(o);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Settings card: 3 rows moved to the left column, 4 switches, 3 volumes, 3 actions, version; " + sp.labels.Count + " live labels";
    }

    static void Move(RectTransform panel, string name, Vector2 pos) { var r = panel.Find(name) as RectTransform; if (r != null) r.anchoredPosition = pos; }

    static RectTransform Row(RectTransform parent, string name, Vector2 pos, float width, string labelText, TMP_Text labelSource)
    {
        var row = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); row.SetParent(parent, false);
        row.anchoredPosition = pos; row.sizeDelta = new Vector2(width, NerdySpace.PillHeight);
        TMP_Text label;
        if (labelSource != null) { label = UnityEngine.Object.Instantiate(labelSource.gameObject, row).GetComponent<TMP_Text>(); }
        else { label = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>(); label.transform.SetParent(row, false); label.font = style.bodyFont; label.fontSize = NerdySpace.Label; label.color = style.textMuted; label.alignment = TextAlignmentOptions.Right; }
        label.name = "Label"; label.text = labelText; label.raycastTarget = false;
        label.rectTransform.anchoredPosition = new Vector2(-width / 2f + 220f / 2f - 12f, 0f); label.rectTransform.sizeDelta = new Vector2(220f, NerdySpace.PillHeight);
        return row;
    }

    static Button Pill(RectTransform parent, string name, string text, Vector2 pos, float width, UnityEngine.Object target, string method, string arg = null)
    {
        var go = UnityEngine.Object.Instantiate(pillTemplate, parent); go.name = name;
        var r = (RectTransform)go.transform; r.anchoredPosition = pos; r.sizeDelta = new Vector2(width, NerdySpace.PillHeight);
        var img = go.GetComponent<Image>(); if (img != null && img.sprite != null && img.sprite.border.x > 0f) img.pixelsPerUnitMultiplier = img.sprite.border.x / (NerdySpace.PillHeight / 2f);
        var label = go.GetComponentInChildren<TMP_Text>(true); if (label != null) { label.text = text; label.rectTransform.anchoredPosition = Vector2.zero; label.rectTransform.sizeDelta = new Vector2(width - 12f, 32f); }
        var b = go.GetComponent<Button>();
        for (int k = b.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(b.onClick, k);
        if (arg != null) UnityEventTools.AddStringPersistentListener(b.onClick, (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>), target, method), arg);
        else UnityEventTools.AddPersistentListener(b.onClick, (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, method));
        return b;
    }
}
