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
            // Deployed 2026-09-17: the scene mints from the Vercel proxy, not the Mac's LAN alias, which dies on reboot.
            Assert.That(n.guide.mintUrl, Is.EqualTo(GuideEndpoints.MintUrl), "bake the endpoint with AgentScripts/SetMintUrl.cs");
            Assert.That(n.guide.mintUrl, Does.StartWith("https://"), "no cleartext mint in a shipped scene");
            Assert.That(n.hudWelcomeCanvas, Is.Not.Null); Assert.That(n.hudStationCanvas, Is.Not.Null);
            Assert.That(n.hudStationCanvas.IsChildOf(n.onboarding.transform), Is.True, "assistant card rides with the workbench in the lesson");
            var panelHandle = n.GetComponent<TableHandle>(); Assert.That(panelHandle, Is.Not.Null, "welcome panel has a carry handle");
            Assert.That(panelHandle.grabbable.Transform, Is.EqualTo(n.transform));
            // The consent card used to be scaled up on its own (owner, Sept 16: "bigger consent card"). Since the
            // spatial standard every panel is one size at one canvas scale, so its size is asserted by
            // SpatialStandardTests instead and a local scale of 1 is now the correct answer here.
            Assert.That(n.consentRoot.transform.localScale.x, Is.EqualTo(1f).Within(0.05f), "consent card is standard-sized, not specially scaled");
        }

        // Owner 2026-09-17: the logo vanished from the consent card after the launcher-icon attempt re-imported the
        // logo as a plain texture, which removes the sprite the Image points at.
        [Test] public void ConsentCardShowsTheNerdyLogo()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Airlift/Branding/nerdy-ai-vr-logo.png");
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), "logo imports as a sprite");
            var logo = Director().consentRoot.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.name == "Logo");
            Assert.That(logo, Is.Not.Null, "consent card has a Logo image");
            Assert.That(logo.gameObject.activeSelf && logo.enabled, Is.True);
            Assert.That(logo.sprite, Is.Not.Null, "Logo image has its sprite");
            Assert.That(logo.sprite.texture.name, Is.EqualTo("nerdy-ai-vr-logo"), "owner 2026-09-17: the AI + VR mark everywhere");
            Assert.That(importer.alphaIsTransparency, Is.True, "the navy plate behind the mark is keyed out");
        }

        [Test] public void ConsentCardChoosesTheVoiceLanguage()
        {
            var n = Director();
            Assert.That(n.languageButtons, Is.Not.Null); Assert.That(n.languageButtons.Length, Is.EqualTo(2));
            string[] codes = { "en", "es" }; string[] labels = { "English", "Español" };
            for (int i = 0; i < 2; i++)
            {
                var b = n.languageButtons[i];
                // Owner 2026-09-17: the board asks voice or no voice; language moved behind the gear on the bar.
                var settings = n.GetComponent<Airlift.Lounge.LoungeSettings>();
                Assert.That(settings, Is.Not.Null, "run AgentScripts/BuildSettingsPanel.cs");
                Assert.That(b.transform.IsChildOf(settings.panel.transform), Is.True, "language lives in the settings card");
                Assert.That(b.GetComponentInChildren<TMP_Text>(true).text, Is.EqualTo(labels[i]));
                bool wired = Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Any(k => b.onClick.GetPersistentTarget(k) == n && b.onClick.GetPersistentMethodName(k) == "ChooseLanguage");
                Assert.That(wired, Is.True, labels[i] + " calls ChooseLanguage");
            }
            Assert.That(n.GetComponent<Airlift.Lounge.LoungeSettings>().panel
                .GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == "Voice language"), Is.True);
            string saved = PlayerPrefs.GetString(GuideLanguage.PrefsKey, "");
            try
            {
                n.ChooseLanguage("es");
                Assert.That(n.Language, Is.EqualTo("es")); Assert.That(n.guide.language, Is.EqualTo("es"));
                Assert.That(n.languageButtons[1].GetComponent<Image>().sprite.name, Is.EqualTo("NerdyPillGradient"), "Español shows as chosen");
                Assert.That(n.languageButtons[0].GetComponent<Image>().sprite.name, Is.EqualTo("NerdyPill"));
                n.ChooseLanguage("en");
                Assert.That(n.guide.language, Is.EqualTo("en"));
                Assert.That(n.languageButtons[0].GetComponent<Image>().sprite.name, Is.EqualTo("NerdyPillGradient"));
            }
            finally { if (saved == "") PlayerPrefs.DeleteKey(GuideLanguage.PrefsKey); else PlayerPrefs.SetString(GuideLanguage.PrefsKey, saved); }
        }

        [Test] public void HudHasPauseAndMusicControlsAndSitsAboveTheLessonPanel()
        {
            var n = Director();
            // Owner 2026-09-17: Pause, Again and Mute live behind the gear now; Music and Help stay on the bar.
            var settings = n.GetComponent<Airlift.Lounge.LoungeSettings>();
            Assert.That(n.pauseButton, Is.Not.Null); Assert.That(n.pauseButton.transform.IsChildOf(settings.panel.transform), Is.True, "Pause/Play lives in the settings card");
            Assert.That(n.repeatButton.transform.IsChildOf(settings.panel.transform), Is.True, "Again lives in the settings card");
            Assert.That(n.muteButton.transform.IsChildOf(settings.panel.transform), Is.True, "Mute lives in the settings card");
            Assert.That(n.musicButton, Is.Not.Null); Assert.That(n.musicButton.transform.IsChildOf(n.hudRoot.transform), Is.True, "music toggle lives on the assistant bar");
            Assert.That(n.music, Is.Not.Null); Assert.That(n.music.GetComponent<AudioSource>(), Is.Not.Null);
            Assert.That(n.music.guide, Is.EqualTo(n.guide)); Assert.That(n.music.playback, Is.EqualTo(n.playback));
            Assert.That(n.hudStationCanvas.localPosition.y, Is.GreaterThan(0.6f), "assistant card sits above the lesson panel");
            foreach (var b in new[] { n.musicButton, n.helpButton })
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
