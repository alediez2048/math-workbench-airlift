using System;
using System.Linq;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Owner 2026-09-17: "select a language ... and stick to only that language". Adds a "Voice language" row with English and
// Español pills to the consent card (between the explanation and the voice buttons), wired to NerdyDirector.ChooseLanguage.
// Pills copy the "No voice" button (outline, focus pointer, press log). Idempotent. Run after PolishCards. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/AddLanguageChoice.cs --entry AddLanguageChoice.Run
public static class AddLanguageChoice
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var consent = n.consentRoot.transform;
        var template = consent.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "No voice") ?? throw new InvalidOperationException("No voice button missing.");
        var body = consent.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name == "Body") ?? throw new InvalidOperationException("Consent body missing.");
        var chosen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Airlift/Sprites/NerdyPillGradient.png");
        var idle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Airlift/Sprites/NerdyPill.png");
        if (chosen == null || idle == null) throw new InvalidOperationException("Pill sprites missing (run PolishCards first).");

        var old = consent.Find("Language row"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var row = new GameObject("Language row", typeof(RectTransform)).GetComponent<RectTransform>();
        row.SetParent(consent, false); row.anchoredPosition = new Vector2(0, -112); row.sizeDelta = new Vector2(640, 40);

        var label = UnityEngine.Object.Instantiate(body.gameObject, row).GetComponent<TMP_Text>();
        label.name = "Language label"; label.text = "Voice language"; label.fontSize = 16; label.alignment = TextAlignmentOptions.Right;
        label.rectTransform.anchoredPosition = new Vector2(-170, 0); label.rectTransform.sizeDelta = new Vector2(200, 40);

        var buttons = new Button[GuideLanguage.Codes.Length];
        float[] xs = { 20f, 185f };
        for (int i = 0; i < buttons.Length; i++)
        {
            string code = GuideLanguage.Codes[i];
            var go = UnityEngine.Object.Instantiate(template.gameObject, row); go.name = "Language " + code;
            var b = go.GetComponent<Button>();
            for (int k = b.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(b.onClick, k);
            UnityEventTools.AddStringPersistentListener(b.onClick, n.ChooseLanguage, code);
            var r = go.GetComponent<RectTransform>(); r.anchoredPosition = new Vector2(xs[i], 0); r.sizeDelta = new Vector2(150, 40);
            var text = go.GetComponentInChildren<TMP_Text>(true); text.text = GuideLanguage.Label(code); text.fontSize = 17;
            text.rectTransform.sizeDelta = new Vector2(138, 36); text.rectTransform.anchoredPosition = Vector2.zero;
            buttons[i] = b;
        }
        n.languageButtons = buttons; n.languageChosen = chosen; n.languageIdle = idle;
        n.ChooseLanguage(GuideLanguage.Default);
        PlayerPrefs.DeleteKey(GuideLanguage.PrefsKey);   // the editor run must not leave a saved choice behind
        EditorUtility.SetDirty(n);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Language row added: Voice language · English · Español on the consent card; scene saved.";
    }
}
