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

// Owner 2026-09-29: "a way to exit the app entirely, in settings, call it Exit app". A fifth pill in the settings actions
// row (Replay the tour · Clear saved data · Reset settings · Restart fresh · Exit app), two presses, wired to
// SettingsPanel.ExitApp with a live label. Idempotent. Run after AddRestartPill.
// Run: unity command run_script --file AgentScripts/AddExitPill.cs --entry AddExitPill.Run
public static class AddExitPill
{
    const float Width = 176f, Gap = 12f;
    static readonly string[] Order = { "Replay the rundown", "Clear saved data", "Reset settings", "Restart fresh", "Exit app" };

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var settings = n.GetComponent<LoungeSettings>() ?? throw new InvalidOperationException("Run BuildSettingsPanel first.");
        var sp = n.settingsPanel ?? throw new InvalidOperationException("Run BuildSettingsExtras first.");
        var actions = settings.panel.transform.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(r => r.name == "Actions row") ?? throw new InvalidOperationException("Actions row missing.");
        var template = actions.Find("Restart fresh") as RectTransform ?? throw new InvalidOperationException("Run AddRestartPill first.");

        var old = actions.Find("Exit app"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        sp.labels.RemoveAll(l => l == null || l.key == "Exit");

        var pill = UnityEngine.Object.Instantiate(template.gameObject, actions).GetComponent<RectTransform>(); pill.name = "Exit app";
        var button = pill.GetComponent<Button>();
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(button.onClick, i);
        UnityEventTools.AddPersistentListener(button.onClick, sp.ExitApp);
        var label = pill.GetComponentInChildren<TMP_Text>(true); label.text = SettingsPanel.ExitLabel;
        sp.labels.Add(new SettingsPanel.LabelRef { key = "Exit", label = label });

        actions.sizeDelta = new Vector2(Order.Length * Width + (Order.Length - 1) * Gap + 20f, actions.sizeDelta.y);
        float x0 = -(Order.Length * Width + (Order.Length - 1) * Gap) / 2f + Width / 2f;
        for (int i = 0; i < Order.Length; i++)
        {
            var r = actions.Find(Order[i]) as RectTransform ?? throw new InvalidOperationException(Order[i] + " missing.");
            r.sizeDelta = new Vector2(Width, r.sizeDelta.y); r.anchoredPosition = new Vector2(x0 + i * (Width + Gap), 0f);
            var t = r.GetComponentInChildren<TMP_Text>(true); if (t != null) { t.rectTransform.sizeDelta = new Vector2(Width - 12f, t.rectTransform.sizeDelta.y); t.enableAutoSizing = true; t.fontSizeMin = 10; }
        }

        foreach (var o in new UnityEngine.Object[] { n, sp, settings }) EditorUtility.SetDirty(o);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "actions row: five pills of " + Width + " (Exit app added)";
    }
}
