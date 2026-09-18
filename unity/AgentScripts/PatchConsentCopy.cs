using System;
using System.Linq;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Owner 2026-09-18: "replace the copy and CTAs from the Meet your Nerdy guide section ... just talk about the Nerdy AI
// assistant, and for the buttons just have turn on mic or continue without voice." Copy only; positions stay
// (FixWelcomeLayout owns them). Idempotent. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/PatchConsentCopy.cs --entry PatchConsentCopy.Run
public static class PatchConsentCopy
{
    public const string Heading = "Meet Dee, your Nerdy AI assistant";
    public const string Body = "Dee talks with you and helps with every lesson. Turn on the mic to talk with her; what you say is not saved.\n\nThis preview is for adult testers.";
    public const string AllowVoice = "Turn on the mic", NoVoice = "Continue without voice";

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
