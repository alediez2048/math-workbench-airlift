using System;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Owner 2026-09-29: "a mute/unmute button next to the pause button". A Mute pill on Dee's bar between Play/Stop and the
// gear, wired to NerdyDirector.ToggleMute with its own label; Play/Stop narrows and the caption gives way. Idempotent.
// Run after ApplyControlCleanup / CompactAssistantBar. Run: unity command run_script --file AgentScripts/AddBarMute.cs --entry AddBarMute.Run
public static class AddBarMute
{
    const float MuteWidth = 84f, PlayWidth = 150f, Gap = 12f;   // 12 px: the bar test's spacing

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var play = n.conversationButton != null ? (RectTransform)n.conversationButton.transform : throw new InvalidOperationException("Run ApplyControlCleanup first.");
        var bar = play.parent;
        var gear = n.rundown != null && n.rundown.gearButton != null ? (RectTransform)n.rundown.gearButton.transform : null;

        var old = bar.Find("Mute"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var mute = UnityEngine.Object.Instantiate(play.gameObject, bar).GetComponent<RectTransform>(); mute.name = "Mute";
        mute.gameObject.SetActive(true);
        var button = mute.GetComponent<Button>();
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(button.onClick, i);
        UnityEventTools.AddPersistentListener(button.onClick, n.ToggleMute);
        var label = mute.GetComponentInChildren<TMP_Text>(true); label.text = "Mute"; label.rectTransform.sizeDelta = new Vector2(MuteWidth - 12f, label.rectTransform.sizeDelta.y);
        n.barMuteLabel = label;
        // A copy of Play/Stop may carry the notice line; the Mute pill has no notice.
        foreach (var t in mute.GetComponentsInChildren<TMP_Text>(true)) if (t != label) UnityEngine.Object.DestroyImmediate(t.gameObject);

        // Layout, from the gear leftwards: gear · Mute · Play/Stop.
        float gearLeft = gear != null ? gear.anchoredPosition.x - gear.sizeDelta.x / 2f : 371f;
        mute.sizeDelta = new Vector2(MuteWidth, play.sizeDelta.y); mute.anchoredPosition = new Vector2(gearLeft - Gap - MuteWidth / 2f, play.anchoredPosition.y);
        play.sizeDelta = new Vector2(PlayWidth, play.sizeDelta.y); play.anchoredPosition = new Vector2(mute.anchoredPosition.x - MuteWidth / 2f - Gap - PlayWidth / 2f, play.anchoredPosition.y);
        var playLabel = play.GetComponentInChildren<TMP_Text>(true); if (playLabel != null) { playLabel.rectTransform.sizeDelta = new Vector2(PlayWidth - 12f, playLabel.rectTransform.sizeDelta.y); playLabel.enableAutoSizing = true; playLabel.fontSizeMin = 11f; }
        // The notice line under Play/Stop keeps its 320 px (room for the age notice), whatever an earlier run left.
        if (n.conversationNotice != null) n.conversationNotice.rectTransform.sizeDelta = new Vector2(320f, 20f);
        // The caption ends before Play/Stop.
        float playLeft = play.anchoredPosition.x - PlayWidth / 2f;
        if (n.captionText != null) { var c = n.captionText.rectTransform; float left = c.anchoredPosition.x - c.sizeDelta.x / 2f; c.sizeDelta = new Vector2(playLeft - 12f - left, c.sizeDelta.y); c.anchoredPosition = new Vector2(left + c.sizeDelta.x / 2f, c.anchoredPosition.y); }
        if (n.userText != null) { var u = n.userText.rectTransform; float left = u.anchoredPosition.x - u.sizeDelta.x / 2f; u.sizeDelta = new Vector2(playLeft - 12f - left, u.sizeDelta.y); u.anchoredPosition = new Vector2(left + u.sizeDelta.x / 2f, u.anchoredPosition.y); }
        mute.SetSiblingIndex(play.GetSiblingIndex() + 1);

        EditorUtility.SetDirty(n);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "bar: Play/Stop " + PlayWidth + " at x " + play.anchoredPosition.x + ", Mute " + MuteWidth + " at x " + mute.anchoredPosition.x + ", gear left edge " + gearLeft;
    }
}
