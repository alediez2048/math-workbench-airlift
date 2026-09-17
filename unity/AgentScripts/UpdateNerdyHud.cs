using System;
using System.Linq;
using Airlift.Guide;
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

// 2026-09-16 evening: adds Pause/Play and a music toggle to the Guide HUD, the synthesized
// ambient music source, and re-lays out the bar so five pills fit. Idempotent; saves CargoCrew.
public static class UpdateNerdyHud
{
    static NerdyStyle S;
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        S = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        if (n == null || n.hudRoot == null) throw new InvalidOperationException("Run CreateNerdyWelcome first.");
        var hud = n.hudRoot.transform;

        foreach (var name in new[] { "Pause", "Music" }) { var old = hud.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
        var oldMusic = n.transform.Find("Nerdy music"); if (oldMusic != null) UnityEngine.Object.DestroyImmediate(oldMusic.gameObject);

        // Layout: orb + state + music (left), caption/you-said (middle), Pause · Again · Mute · Help (right).
        Place(n.captionText.rectTransform, new Vector2(-165, 16), new Vector2(440, 52));
        Place(n.userText.rectTransform, new Vector2(-105, -30), new Vector2(330, 24));
        Place(n.stateText.rectTransform, new Vector2(-420, -34), new Vector2(90, 24)); n.stateText.fontSize = 11;
        Place(n.repeatButton.GetComponent<RectTransform>(), new Vector2(210, 0), new Vector2(88, 44));
        Place(n.muteButton.GetComponent<RectTransform>(), new Vector2(302, 0), new Vector2(88, 44));
        Place(n.helpButton.GetComponent<RectTransform>(), new Vector2(394, 0), new Vector2(88, 44));
        foreach (var b in new[] { n.repeatButton, n.muteButton, n.helpButton }) FitLabel(b, new Vector2(88, 44));

        n.pauseButton = Pill(hud, "Pause", "Pause", false, new Vector2(118, 0), new Vector2(88, 44), n.TogglePause);
        n.pauseLabel = n.pauseButton.GetComponentInChildren<TMP_Text>();
        n.musicButton = Pill(hud, "Music", "Music on", false, new Vector2(-320, -30), new Vector2(84, 28), n.ToggleMusic);
        n.musicLabel = n.musicButton.GetComponentInChildren<TMP_Text>(); n.musicLabel.fontSize = 11;

        var musicGo = new GameObject("Nerdy music", typeof(AudioSource), typeof(AmbientMusic)); musicGo.transform.SetParent(n.transform, false);
        var src = musicGo.GetComponent<AudioSource>(); src.playOnAwake = false; src.loop = true; src.spatialBlend = 0f; src.volume = 0f;
        var music = musicGo.GetComponent<AmbientMusic>(); music.guide = n.guide; music.playback = n.playback; n.music = music;

        // Make the station canvas's serialized local position match its anchored position (belt and braces).
        var station = n.hudStationCanvas as RectTransform; if (station != null) station.anchoredPosition3D = new Vector3(0f, 0.77f, 0.24f);

        EditorUtility.SetDirty(n); EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "HUD updated: Pause/Play + Music pills, Nerdy music source, bar re-laid out; scene saved.";
    }

    static void Place(RectTransform r, Vector2 pos, Vector2 size) { r.anchoredPosition = pos; r.sizeDelta = size; }
    static void FitLabel(Button b, Vector2 size) { var l = b.GetComponentInChildren<TMP_Text>(); if (l != null) l.rectTransform.sizeDelta = size - new Vector2(12, 4); }

    static Button Pill(Transform parent, string name, string label, bool primary, Vector2 pos, Vector2 size, UnityAction action)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchoredPosition = pos; r.sizeDelta = size;
        var img = go.GetComponent<Image>(); img.sprite = S.pill; img.type = UnityEngine.UI.Image.Type.Sliced; img.color = primary ? Color.white : new Color(1, 1, 1, 0.10f);
        var outline = go.AddComponent<Outline>(); outline.effectColor = new Color(S.line.r, S.line.g, S.line.b, 0.9f); outline.effectDistance = new Vector2(1.2f, -1.2f);
        var b = go.GetComponent<Button>(); b.targetGraphic = img; var colors = b.colors; colors.highlightedColor = new Color(0.9f, 0.9f, 1f); colors.pressedColor = new Color(0.75f, 0.75f, 0.95f); b.colors = colors;
        if (action != null) UnityEventTools.AddPersistentListener(b.onClick, action);
        var lgo = new GameObject("Label", typeof(RectTransform)); lgo.transform.SetParent(go.transform, false);
        var t = lgo.AddComponent<TextMeshProUGUI>(); t.font = S.displayFont; t.fontSize = 14; t.color = Color.white; t.text = label; t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
        t.rectTransform.anchoredPosition = Vector2.zero; t.rectTransform.sizeDelta = size - new Vector2(12, 4); t.textWrappingMode = TextWrappingModes.NoWrap; t.overflowMode = TextOverflowModes.Overflow;
        go.AddComponent<FocusPointer>();
        return b;
    }
}
