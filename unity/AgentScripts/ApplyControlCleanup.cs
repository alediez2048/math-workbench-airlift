using System;
using System.Linq;
using Airlift.Lounge;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Final, idempotent presentation pass. Keeps legacy callback/template references intact under inactive
// parents, so station refreshes cannot resurrect retired controls. Does not run any lesson builder.
public static class ApplyControlCleanup
{
    public static string Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play first.");
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        if (n == null || n.rundown == null || n.helpButton == null) throw new InvalidOperationException("Missing presentation references.");
        var bar = (RectTransform)n.hudRoot.transform;
        if (n.conversationButton == null)
        {
            var go = UnityEngine.Object.Instantiate(n.helpButton.gameObject, bar); go.name = "Conversation"; go.SetActive(true);
            n.conversationButton = go.GetComponent<Button>();
            for (int i = n.conversationButton.onClick.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(n.conversationButton.onClick, i);
        }
        n.conversationLabel = n.conversationButton.GetComponentInChildren<TMP_Text>(true);
        n.conversationLabel.text = "■ Stop"; n.conversationLabel.enableAutoSizing = true; n.conversationLabel.fontSizeMin = 14; n.conversationLabel.fontSizeMax = 18;
        n.conversationLabel.rectTransform.sizeDelta = new Vector2(182, 36);
        if (n.conversationNotice == null) {
            n.conversationNotice = UnityEngine.Object.Instantiate(n.conversationLabel, bar);
            n.conversationNotice.name = "Conversation notice";
        }
        n.conversationNotice.text = ""; n.conversationNotice.raycastTarget = false;
        n.conversationNotice.rectTransform.anchoredPosition = new Vector2(260, -28);
        n.conversationNotice.rectTransform.sizeDelta = new Vector2(320, 20);
        n.conversationNotice.fontSizeMin = 12; n.conversationNotice.fontSizeMax = 14;
        n.conversationNotice.alignment = TextAlignmentOptions.Center;
        var control = (RectTransform)n.conversationButton.transform;
        control.anchoredPosition = new Vector2(260, 6); control.sizeDelta = new Vector2(202, 44);
        var gear = n.rundown.gearButton;
        if (gear != null) ((RectTransform)gear.transform).anchoredPosition = new Vector2(393, 6);
        n.captionText.rectTransform.anchoredPosition = new Vector2(-105, 8);
        n.captionText.rectTransform.sizeDelta = new Vector2(490, 56);
        n.captionText.enableAutoSizing = true; n.captionText.fontSizeMin = 14;
        if (n.userText != null) { n.userText.rectTransform.anchoredPosition = new Vector2(-105, -29); n.userText.rectTransform.sizeDelta = new Vector2(490, 20); }
        foreach (var b in new[] { n.micButton, n.musicButton, n.helpButton, n.navBack, n.navNext }) Retire(b);

        // Settings already owns a pause control, so Stop remains available there while the bar is hidden.
        if (n.pauseButton != null) { n.pauseButton.gameObject.SetActive(true); if (n.pauseLabel != null) n.pauseLabel.text = "Stop"; }
        if (n.pauseButton != null) {
            n.settingsConversationNotice = n.pauseButton.transform.parent.Find("Guide label").GetComponent<TMP_Text>();
            n.settingsConversationNotice.text = "Conversation";
            n.settingsConversationNotice.enableAutoSizing = true;
            n.settingsConversationNotice.fontSizeMin = 14;
        }
        int exits = 0;
        foreach (var root in scene.GetRootGameObjects())
            foreach (var b in root.GetComponentsInChildren<Button>(true))
            {
                var label = b.GetComponentInChildren<TMP_Text>(true);
                if (b.name == "Back to lessons" || (label != null && label.text.Trim().Equals("Back to lessons", StringComparison.OrdinalIgnoreCase))) { Retire(b); exits++; }
            }
        if (exits < 3) throw new InvalidOperationException("Expected return controls for all three lessons; found " + exits);
        var toolbar = n.catalogRoot.transform.Find("Toolbar");
        n.rundown.conversationButton = n.conversationButton;
        n.rundown.lounge = n.lounge;
        n.rundown.yourRoomButton = toolbar.Find("Your room").GetComponent<Button>();
        n.rundown.nerdyLoungeButton = toolbar.Find("Nerdy lounge").GetComponent<Button>();
        n.rundown.barControls = new[] { gear, n.conversationButton }.Where(b => b != null).ToArray();
        n.rundown.nextArrow = null; n.rundown.backArrow = null;
        n.rundown.skipButton.gameObject.SetActive(true);
        bar.SetAsLastSibling(); n.rundown.header.transform.SetAsLastSibling(); n.rundown.pointerRoot.SetAsLastSibling();
        foreach (var o in new UnityEngine.Object[] { n, n.rundown }) EditorUtility.SetDirty(o);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Conversation toggle wired; retired corner arrows and companion shortcuts; " + exits + " legacy exits suppressed; scenery tour wired. Settings retains Stop.";
    }

    static void Retire(Button button)
    {
        if (button == null) return;
        const string prefix = "Retired control — ";
        if (button.transform.parent.name.StartsWith(prefix, StringComparison.Ordinal)) { button.transform.parent.gameObject.SetActive(false); return; }
        var parent = button.transform.parent;
        int sibling = button.transform.GetSiblingIndex();
        var wrapper = new GameObject(prefix + button.name, typeof(RectTransform)).GetComponent<RectTransform>();
        wrapper.SetParent(parent, false); wrapper.SetSiblingIndex(sibling);
        wrapper.anchorMin = Vector2.zero; wrapper.anchorMax = Vector2.one; wrapper.offsetMin = wrapper.offsetMax = Vector2.zero;
        button.transform.SetParent(wrapper, false); wrapper.gameObject.SetActive(false);
        // Keep activeSelf for templates and old station code; inactive parent is the presentation boundary.
    }
}
