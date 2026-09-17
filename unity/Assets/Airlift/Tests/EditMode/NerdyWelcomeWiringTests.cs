using System.Linq;
using Airlift.Guide;
using Airlift.Presentation;
using Airlift.Welcome;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Airlift.Tests
{
    public class NerdyWelcomeWiringTests
    {
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").isLoaded) loaded = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }
        NerdyDirector Director() => SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).FirstOrDefault();

        [Test] public void EntryFlowIsWiredIntoTheWorkbench()
        {
            var n = Director(); Assert.That(n, Is.Not.Null, "NerdyDirector");
            Assert.That(n.guide, Is.Not.Null); Assert.That(n.playback, Is.Not.Null); Assert.That(n.onboarding, Is.Not.Null); Assert.That(n.head, Is.Not.Null);
            Assert.That(n.consentRoot, Is.Not.Null); Assert.That(n.welcomeRoot, Is.Not.Null); Assert.That(n.catalogRoot, Is.Not.Null); Assert.That(n.hudRoot, Is.Not.Null);
            Assert.That(n.stationVisuals.Length, Is.GreaterThanOrEqualTo(4), "table, props, handle and lesson card hide during welcome");
            Assert.That(n.catalogRoot.GetComponentsInChildren<Button>(true).Count(b => b.name.StartsWith("Card ")), Is.EqualTo(3));
            Assert.That(n.welcomeRoot.GetComponentsInChildren<Button>(true).Count(b => b.name.StartsWith("Chip ")), Is.EqualTo(16));
            var back = n.onboarding.transform.Find("Lesson interface").GetComponentsInChildren<Button>(true).First(b => b.name == "Back to lessons");
            bool hooked = Enumerable.Range(0, back.onClick.GetPersistentEventCount()).Any(i => back.onClick.GetPersistentTarget(i) is NerdyDirector);
            Assert.That(hooked, Is.True, "Back returns to the Nerdy catalog");
            Assert.That(n.guide.mintUrl, Does.StartWith("http"));
            Assert.That(n.hudWelcomeCanvas, Is.Not.Null); Assert.That(n.hudStationCanvas, Is.Not.Null);
            Assert.That(n.hudStationCanvas.IsChildOf(n.onboarding.transform), Is.True, "assistant card rides with the workbench in the lesson");
            var panelHandle = n.GetComponent<TableHandle>(); Assert.That(panelHandle, Is.Not.Null, "welcome panel has a carry handle");
            Assert.That(panelHandle.grabbable.Transform, Is.EqualTo(n.transform));
            Assert.That(n.consentRoot.transform.localScale.x, Is.GreaterThan(1.3f), "consent card enlarged");
        }

        [Test] public void HudHasPauseAndMusicControlsAndSitsAboveTheLessonPanel()
        {
            var n = Director();
            Assert.That(n.pauseButton, Is.Not.Null); Assert.That(n.pauseButton.transform.IsChildOf(n.hudRoot.transform), Is.True, "Pause/Play lives on the assistant bar");
            Assert.That(n.musicButton, Is.Not.Null); Assert.That(n.musicButton.transform.IsChildOf(n.hudRoot.transform), Is.True, "music toggle lives on the assistant bar");
            Assert.That(n.music, Is.Not.Null); Assert.That(n.music.GetComponent<AudioSource>(), Is.Not.Null);
            Assert.That(n.music.guide, Is.EqualTo(n.guide)); Assert.That(n.music.playback, Is.EqualTo(n.playback));
            Assert.That(n.hudStationCanvas.localPosition.y, Is.GreaterThan(0.6f), "assistant card sits above the lesson panel");
            foreach (var b in new[] { n.pauseButton, n.musicButton, n.muteButton, n.helpButton, n.repeatButton })
            {
                var r = b.GetComponent<RectTransform>(); float half = n.hudRoot.GetComponent<RectTransform>().sizeDelta.x / 2f;
                Assert.That(Mathf.Abs(r.anchoredPosition.x) + r.sizeDelta.x / 2f, Is.LessThanOrEqualTo(half), b.name + " stays inside the bar");
            }
        }

        [Test] public void WelcomeUiUsesNerdyFontsOnly()
        {
            var n = Director(); var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
            var fonts = new[] { style.displayFont, style.displayItalic, style.bodyFont, style.bodySemibold, style.altFont, style.altBold };
            foreach (var t in n.GetComponentsInChildren<TMP_Text>(true)) Assert.That(fonts.Contains(t.font), Is.True, t.name + " uses " + (t.font ? t.font.name : "null"));
        }
    }
}
