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

// Owner 2026-09-28: a "Skip onboarding" switch for testers on the settings card, right column between Show button
// labels and Age. The six right-column rows are re-spaced to 68 so the Age row stays clear of the actions row.
// Clones the Captions row, re-aims the pill at SettingsPanel.Toggle("SkipOnboarding"). Idempotent. Run after AddAgeRow.
// Run: unity command run_script --file AgentScripts/AddSkipOnboardingRow.cs --entry AddSkipOnboardingRow.Run
public static class AddSkipOnboardingRow
{
    const string DefaultScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    /// A one-off scene path can be given in Temp/nerdy-scene-override.txt (used to add the row to another branch's
    /// copy of the scene without touching the working tree's). The default scene is reopened afterwards.
    static string ScenePath => System.IO.File.Exists("Temp/nerdy-scene-override.txt") ? System.IO.File.ReadAllText("Temp/nerdy-scene-override.txt").Trim() : DefaultScenePath;
    const float RightX = 400f, Top = 190f, Step = 68f;
    static readonly string[] RightColumn = { "Voice guide row", "Captions row", "Haptic feedback row", "Show button labels row", "Skip onboarding row", "Age row" };

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var settings = n.GetComponent<LoungeSettings>() ?? throw new InvalidOperationException("Run BuildSettingsPanel first.");
        var panel = (RectTransform)settings.panel.transform;
        var sp = n.settingsPanel ?? throw new InvalidOperationException("Run BuildSettingsExtras first.");
        var extras = panel.Find("Extras") as RectTransform ?? throw new InvalidOperationException("Run BuildSettingsExtras first.");
        var template = extras.Find("Captions row") as RectTransform ?? throw new InvalidOperationException("Captions row missing.");
        if (extras.Find("Age row") == null) throw new InvalidOperationException("Run AddAgeRow first.");

        var old = extras.Find("Skip onboarding row"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        sp.labels.RemoveAll(l => l == null || l.key == "SkipOnboarding");

        var row = UnityEngine.Object.Instantiate(template.gameObject, extras).GetComponent<RectTransform>();
        row.name = "Skip onboarding row";
        var label = row.GetComponentsInChildren<TMP_Text>(true).First(t => t.GetComponent<Button>() == null && t.transform.parent == row);
        label.text = "Skip onboarding";
        var pill = row.GetComponentInChildren<Button>(true); pill.name = "SkipOnboarding";
        var pillLabel = pill.GetComponentInChildren<TMP_Text>(true); pillLabel.text = SettingsPanel.OnOff(false);
        for (int k = pill.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(pill.onClick, k);
        UnityEventTools.AddStringPersistentListener(pill.onClick, sp.Toggle, "SkipOnboarding");
        sp.labels.Add(new SettingsPanel.LabelRef { key = "SkipOnboarding", label = pillLabel });

        // Six rows, evenly spaced, Age last; the actions row (y -205) stays where AddAgeRow put it.
        for (int i = 0; i < RightColumn.Length; i++)
        {
            var r = extras.Find(RightColumn[i]) as RectTransform ?? throw new InvalidOperationException(RightColumn[i] + " missing.");
            r.anchoredPosition = new Vector2(RightX, Top - Step * i);
            r.SetSiblingIndex(extras.childCount - 1);
        }

        foreach (var o in new UnityEngine.Object[] { n, sp, settings }) EditorUtility.SetDirty(o);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        if (ScenePath != DefaultScenePath) EditorSceneManager.OpenScene(DefaultScenePath, OpenSceneMode.Single);
        return "[" + ScenePath + "] Skip onboarding row added (right column, six rows at 68 spacing, Age at y " + (Top - Step * 5) + ")";
    }
}
