using System;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Owner 2026-09-28: "the skip onboarding button should be right next to Let's begin". The welcome card's second pill
// (hidden since PatchConsentCopy) becomes Skip onboarding, wired to NerdyDirector.SkipOnboardingNow; Let's begin moves
// left so the two sit side by side. Idempotent. Run after PatchConsentCopy and FixWelcomeLayout.
// Run: unity command run_script --file AgentScripts/AddSkipOnboardingPill.cs --entry AddSkipOnboardingPill.Run
public static class AddSkipOnboardingPill
{
    const float Width = 270f, Gap = 24f;

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var consent = n.consentRoot.transform;
        var begin = consent.Find("Allow voice") as RectTransform ?? throw new InvalidOperationException("Allow voice (Let's begin) missing.");
        var skip = (consent.Find("Skip onboarding") ?? consent.Find("No voice")) as RectTransform ?? throw new InvalidOperationException("No voice pill missing.");

        skip.name = "Skip onboarding"; skip.gameObject.SetActive(true);
        float y = begin.anchoredPosition.y;
        begin.sizeDelta = new Vector2(Width, begin.sizeDelta.y); skip.sizeDelta = new Vector2(Width, begin.sizeDelta.y);
        begin.anchoredPosition = new Vector2(-(Width + Gap) / 2f, y); skip.anchoredPosition = new Vector2((Width + Gap) / 2f, y);
        foreach (var r in new[] { begin, skip })
        {
            var label = r.GetComponentInChildren<TMP_Text>(true);
            if (label != null) { label.rectTransform.sizeDelta = new Vector2(Width - 12f, label.rectTransform.sizeDelta.y); label.text = r == begin ? "Let's begin" : "Skip onboarding"; }
        }
        var button = skip.GetComponent<Button>();
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(button.onClick, i);
        UnityEventTools.AddPersistentListener(button.onClick, n.SkipOnboardingNow);

        EditorUtility.SetDirty(n);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "welcome card: Let's begin at x " + begin.anchoredPosition.x + ", Skip onboarding at x " + skip.anchoredPosition.x + " (width " + Width + ")";
    }
}
