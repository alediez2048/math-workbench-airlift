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

// Owner 2026-09-28: "a button on the settings that restarts the entire app with absolutely no progress data".
// A fourth pill, Restart fresh, in the settings actions row (Replay the tour · Clear saved data · Reset settings ·
// Restart fresh), wired to SettingsPanel.RestartFresh (two presses) with a live label. Idempotent. Run after AddAgeRow.
// Run: unity command run_script --file AgentScripts/AddRestartPill.cs --entry AddRestartPill.Run
public static class AddRestartPill
{
    const float Width = 210f, Gap = 15f;
    static readonly string[] Order = { "Replay the rundown", "Clear saved data", "Reset settings", "Restart fresh" };

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var settings = n.GetComponent<LoungeSettings>() ?? throw new InvalidOperationException("Run BuildSettingsPanel first.");
        var sp = n.settingsPanel ?? throw new InvalidOperationException("Run BuildSettingsExtras first.");
        var actions = settings.panel.transform.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(r => r.name == "Actions row") ?? throw new InvalidOperationException("Actions row missing.");
        var template = actions.Find("Reset settings") as RectTransform ?? throw new InvalidOperationException("Reset settings pill missing.");

        var old = actions.Find("Restart fresh"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        sp.labels.RemoveAll(l => l == null || l.key == "Restart");

        var pill = UnityEngine.Object.Instantiate(template.gameObject, actions).GetComponent<RectTransform>(); pill.name = "Restart fresh";
        var button = pill.GetComponent<Button>();
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(button.onClick, i);
        UnityEventTools.AddPersistentListener(button.onClick, sp.RestartFresh);
        var label = pill.GetComponentInChildren<TMP_Text>(true); label.text = SettingsPanel.RestartLabel;
        // One size for all four labels: the one the Age pill uses (the switches' size), shrinking only if a label overflows.
        float size = settings.panel.transform.GetComponentsInChildren<TMP_Text>(true).First(t => t.transform.parent.name == "Age").fontSize;
        sp.labels.Add(new SettingsPanel.LabelRef { key = "Restart", label = label });

        float x0 = -(Order.Length * Width + (Order.Length - 1) * Gap) / 2f + Width / 2f;
        for (int i = 0; i < Order.Length; i++)
        {
            var r = actions.Find(Order[i]) as RectTransform ?? throw new InvalidOperationException(Order[i] + " missing.");
            r.sizeDelta = new Vector2(Width, r.sizeDelta.y); r.anchoredPosition = new Vector2(x0 + i * (Width + Gap), 0f);
            var t = r.GetComponentInChildren<TMP_Text>(true); if (t != null) { t.rectTransform.sizeDelta = new Vector2(Width - 12f, t.rectTransform.sizeDelta.y); t.fontSize = size; t.enableAutoSizing = true; t.fontSizeMin = 11; t.fontSizeMax = size; }
        }

        foreach (var o in new UnityEngine.Object[] { n, sp, settings }) EditorUtility.SetDirty(o);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "actions row: four pills of " + Width + " (Restart fresh added)";
    }
}
