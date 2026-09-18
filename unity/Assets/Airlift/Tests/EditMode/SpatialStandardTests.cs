using System.Collections.Generic;
using System.Linq;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Presentation;
using Airlift.Welcome;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Airlift.Tests
{
    /// Owner 2026-09-17: panels stood at 1.85 m and 0.65 m, in six different sizes, with headings landing at four
    /// different physical heights and the workbench cards still set in the pre-Nerdy font. This suite is what stops
    /// that coming back: one distance, one panel size, one type ramp, everywhere.
    /// Run AgentScripts/ApplySpatialStandards.cs after any builder that makes a panel.
    public class SpatialStandardTests
    {
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").isLoaded) loaded = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        static T Find<T>() where T : Object => SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity")
            .GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).FirstOrDefault();

        /// Every panel the learner reads from: the welcome cards and each lesson's card.
        static IEnumerable<(string name, RectTransform rect)> Panels()
        {
            var n = Find<NerdyDirector>();
            yield return ("consent", (RectTransform)n.consentRoot.transform);
            yield return ("welcome", (RectTransform)n.welcomeRoot.transform);
            yield return ("catalog", (RectTransform)n.catalogRoot.transform);
            foreach (var s in SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity")
                         .GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LessonStation>(true)))
                if (s.heading != null && s.heading.transform.parent is RectTransform card)
                    yield return (s.cardId, card);
        }

        [Test] public void EveryPanelIsTheSameSize()
        {
            foreach (var (name, rect) in Panels())
            {
                Assert.That(rect.sizeDelta.x, Is.EqualTo(NerdySpace.PanelWidth).Within(1f), name + " width");
                Assert.That(rect.sizeDelta.y, Is.EqualTo(NerdySpace.PanelHeight).Within(1f), name + " height");
            }
        }

        [Test] public void EveryPanelIsAtTheSameScaleSoAMillimetreIsAMillimetre()
        {
            foreach (var (name, rect) in Panels())
                Assert.That(rect.lossyScale.x, Is.EqualTo(NerdySpace.PanelScale).Within(0.00005f),
                    name + " draws its canvas units at a different physical size than the rest");
        }

        [Test] public void EveryPanelStandsAtTheSameDistance()
        {
            var n = Find<NerdyDirector>();
            var d = Find<OnboardingDirector>();
            Assert.That(n.welcomeDistance, Is.EqualTo(NerdySpace.PanelDistance).Within(0.01f), "welcome panel");
            Assert.That(d.content.boardDistance, Is.EqualTo(NerdySpace.PanelDistance).Within(0.01f), "lesson board");
            Assert.That(n.welcomeBelowEyes, Is.EqualTo(NerdySpace.PanelBelowEyes).Within(0.01f), "welcome panel height");
            Assert.That(d.content.boardBelowEyes, Is.EqualTo(NerdySpace.PanelBelowEyes).Within(0.01f), "lesson board height");
        }

        // The ray-pointable area is the welcome canvas rect. Smaller than the panel means edges nobody can press —
        // the gear "did nothing" for exactly this reason on 2026-09-17.
        [Test] public void ThePointableCanvasCoversTheWholePanel()
        {
            var n = Find<NerdyDirector>();
            var canvas = (RectTransform)n.hudWelcomeCanvas;
            Assert.That(canvas.sizeDelta.x, Is.GreaterThanOrEqualTo(NerdySpace.PanelWidth - 1f), "canvas narrower than the panel");
            Assert.That(canvas.sizeDelta.y, Is.GreaterThanOrEqualTo(NerdySpace.PanelHeight - 1f), "canvas shorter than the panel");
        }

        [Test] public void EveryHeadingIsTheSameSizeAndFont()
        {
            var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
            foreach (var (name, rect) in Panels())
            {
                var heading = rect.GetComponentsInChildren<TMP_Text>(true)
                    .FirstOrDefault(t => t.name == "Heading" || t.name == "Title");
                if (heading == null) continue;
                Assert.That(heading.fontSize, Is.EqualTo(NerdySpace.Heading).Within(0.5f), name + " heading size");
                Assert.That(heading.font, Is.EqualTo(style.displayFont),
                    name + " heading is set in " + (heading.font ? heading.font.name : "null") + ", not the Nerdy display font");
            }
        }

        /// The old Cargo-era font never left the workbench cards, because the font rule only ever scanned the
        /// welcome panel. Every text in the app now comes from the Nerdy families.
        [Test] public void NoTextAnywhereUsesThePreNerdyFonts()
        {
            var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
            var allowed = new[] { style.displayFont, style.displayItalic, style.bodyFont, style.bodySemibold, style.altFont, style.altBold };
            foreach (var t in SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity")
                         .GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TMP_Text>(true)))
                Assert.That(allowed.Contains(t.font), Is.True,
                    t.transform.root.name + "/" + t.name + " uses " + (t.font ? t.font.name : "null"));
        }
    }
}
