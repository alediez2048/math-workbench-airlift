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

// Owner 2026-09-18 ("keep the gate, add a clear notice"): an Age row on the settings card, right column under the
// four switches, one pill wired to SettingsPanel.CycleAge with a live label; the actions row, Done and the version
// line move down to make room; Dee's bar notice widens for the age line. Idempotent. Run after ApplyControlCleanup.
// Run: unity command run_script --file AgentScripts/AddAgeRow.cs --entry AddAgeRow.Run
public static class AddAgeRow
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const float RightX = 400f;

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

        var old = extras.Find("Age row"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        sp.labels.RemoveAll(l => l == null || l.key == "Age");

        // Clone the Captions row: same label style, same pill, then re-aim the pill at CycleAge.
        var row = UnityEngine.Object.Instantiate(template.gameObject, extras).GetComponent<RectTransform>();
        row.name = "Age row"; row.anchoredPosition = new Vector2(RightX, -130f);
        var label = row.GetComponentsInChildren<TMP_Text>(true).First(t => t.GetComponent<Button>() == null && t.transform.parent == row);
        label.text = "Age";
        var pill = row.GetComponentInChildren<Button>(true); pill.name = "Age";
        var pr = (RectTransform)pill.transform; pr.sizeDelta = new Vector2(150f, NerdySpace.PillHeight); pr.anchoredPosition = new Vector2(380f / 2f - 75f - 10f, 0f);   // clear of the 220-wide label
        var pillLabel = pill.GetComponentInChildren<TMP_Text>(true); pillLabel.text = SettingsPanel.AgeLabel(""); pillLabel.rectTransform.sizeDelta = new Vector2(150f - 12f, 32f);
        pillLabel.enableAutoSizing = true; pillLabel.fontSizeMin = 12; pillLabel.fontSizeMax = pillLabel.fontSize;
        for (int k = pill.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(pill.onClick, k);
        UnityEventTools.AddPersistentListener(pill.onClick, (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), sp, "CycleAge"));
        sp.labels.Add(new SettingsPanel.LabelRef { key = "Age", label = pillLabel });

        // Make room below.
        var actions = extras.Find("Actions row") as RectTransform; if (actions != null) actions.anchoredPosition = new Vector2(0f, -205f);
        var done = panel.Find("Done") as RectTransform; if (done != null) done.anchoredPosition = new Vector2(0f, -280f);
        if (sp.versionText != null) sp.versionText.rectTransform.anchoredPosition = new Vector2(0f, -322f);

        // The bar notice has to fit "Set your age in settings to talk with Dee".
        if (n.conversationNotice != null) n.conversationNotice.rectTransform.sizeDelta = new Vector2(320f, 20f);

        foreach (var o in new UnityEngine.Object[] { n, sp, settings }) EditorUtility.SetDirty(o);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Age row added to the settings card (right column, y -130); actions/Done/version moved down; bar notice 320 wide";
    }
}
