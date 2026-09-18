using System;
using System.Linq;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor.Events;

// Owner 2026-09-18: "replace the copy and CTAs from the Meet your Nerdy guide section ... just talk about the Nerdy AI
// assistant, and for the buttons just have turn on mic or continue without voice." Copy only; positions stay
// (FixWelcomeLayout owns them). Idempotent. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/PatchConsentCopy.cs --entry PatchConsentCopy.Run
public static class PatchConsentCopy
{
    public const string Heading = "Meet Dee, your Nerdy AI + VR guide";
    public const string Body = "Explore math with Dee. Select Let's begin to get started.\n\nUse buttons throughout, or enable the mic later to talk with Dee. This preview is for adult testers.";
    public const string AllowVoice = "Let's begin", NoVoice = "";

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var consent = n.consentRoot.transform;
        Set(consent, "Heading", Heading);
        Set(consent, "Body", Body);
        var allow = (RectTransform)consent.Find("Allow voice"); var no = (RectTransform)consent.Find("No voice");
        Set(allow, "Label", AllowVoice); Set(no, "Label", NoVoice);
        no.gameObject.SetActive(false);
        allow.anchoredPosition = new Vector2(0f, allow.anchoredPosition.y);
        var button = allow.GetComponent<Button>();
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(button.onClick, i);
        UnityEventTools.AddPersistentListener(button.onClick, n.BeginWelcome);
        foreach (var text in n.welcomeRoot.GetComponentsInChildren<TMP_Text>(true))
            if (text.name == "Heading" && text.text.Contains("Nerdy AI")) text.text = "Welcome to Nerdy AI + VR";
        allow.sizeDelta = new Vector2(300f, allow.sizeDelta.y);
        var allowLabel = allow.GetComponentInChildren<TMP_Text>(true); if (allowLabel != null) allowLabel.rectTransform.sizeDelta = new Vector2(288f, allowLabel.rectTransform.sizeDelta.y);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "consent copy: '" + Heading + "' / '" + AllowVoice + "' · '" + NoVoice + "'";
    }

    static void Set(Transform root, string name, string text)
    {
        var t = root.Find(name)?.GetComponent<TMP_Text>() ?? throw new InvalidOperationException(name + " missing under " + root.name);
        t.text = text; EditorUtility.SetDirty(t);
    }
}
