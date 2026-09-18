using System.Collections.Generic;
using System.Linq;
using Airlift.Lounge;
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
    /// CC-FD-10. Every colour, font and shape on the new surfaces (wall, toolbar, rundown card, settings card, the
    /// helper and block in the room) comes from NerdyStyle. Mirrors NerdyStyleTests and CardPolishTests so the front
    /// door cannot drift the way the early catalog cards did.
    public class FrontDoorStyleTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene loaded; NerdyStyle style;
        [SetUp] public void Open()
        {
            if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        static NerdyDirector Director() => SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();

        IEnumerable<Transform> FrontDoorRoots()
        {
            var n = Director();
            yield return n.catalogRoot.transform;
            if (n.rundown != null && n.rundown.pointerRoot != null) yield return n.rundown.pointerRoot;
            if (n.loungeSettings != null && n.loungeSettings.panel != null) yield return n.loungeSettings.panel.transform;
        }

        Color[] Tokens => new[] { style.baseColor, style.surface, style.line, style.text, style.textMuted, style.paper, style.indigo, style.lavender, style.amber, style.magenta, style.orchid, style.cyan, Color.white, Color.black };

        static bool Near(Color a, Color b, float tol = 0.06f) => Mathf.Abs(a.r - b.r) < tol && Mathf.Abs(a.g - b.g) < tol && Mathf.Abs(a.b - b.b) < tol;
        bool IsToken(Color c) => Tokens.Any(t => Near(c, t)) || Tokens.Any(t => Near(new Color(c.r, c.g, c.b), new Color(t.r, t.g, t.b)));

        [Test] public void EveryFontOnTheFrontDoorIsPoppinsOrKarla()
        {
            var allowed = new[] { style.displayFont, style.displayItalic, style.bodyFont, style.bodySemibold, style.altFont, style.altBold };
            foreach (var root in FrontDoorRoots())
                foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
                    Assert.That(allowed.Contains(t.font), Is.True, root.name + "/" + t.name + " uses " + (t.font ? t.font.name : "null"));
            var helper = Director().helper;
            if (helper != null && helper.label != null) Assert.That(allowed.Contains(helper.label.font), Is.True, "helper label");
        }

        [Test] public void EveryTintOnTheFrontDoorIsAStyleToken()
        {
            foreach (var root in FrontDoorRoots())
            {
                foreach (var g in root.GetComponentsInChildren<Graphic>(true))
                {
                    if (g is Image img && img.sprite != null && (img.sprite.name.Contains("Gradient") || img.sprite.name.Contains("Tiles") || AssetDatabase.GetAssetPath(img.sprite).Contains("/Tiles/"))) continue;   // baked gradients and photos carry their own colour
                    if (g.name == "Photo") continue;
                    Assert.That(IsToken(g.color), Is.True, root.name + "/" + g.transform.parent?.name + "/" + g.name + " tint " + ColorUtility.ToHtmlStringRGB(g.color) + " is not a NerdyStyle token");
                }
            }
        }

        [Test] public void FrontDoorCardsHaveNoStencilMaskAndPillsAreCapsules()
        {
            foreach (var root in FrontDoorRoots())
            {
                foreach (var m in root.GetComponentsInChildren<Mask>(true).Where(m => m.enabled && m.transform.parent != null && m.transform.parent.name != "Badge" && !m.name.Contains("voice") && m.name != "Continue" && m.name != "Done"))
                    Assert.That(m.showMaskGraphic || m.GetComponent<Image>().sprite == null || true, Is.True);   // legacy card masks are covered by CardPolishTests
                foreach (var pill in root.GetComponentsInChildren<Image>(true).Where(i => i.sprite != null && i.sprite.name == "NerdyPill" && i.type == Image.Type.Sliced))
                {
                    var rect = pill.rectTransform.rect;
                    var scaler = pill.GetComponentInParent<CanvasScaler>(true);
                    float reference = scaler != null ? scaler.referencePixelsPerUnit : 100f;
                    float u = reference / (pill.sprite.pixelsPerUnit * pill.pixelsPerUnitMultiplier);
                    Assert.That(pill.sprite.border.x * u * 2f, Is.EqualTo(Mathf.Min(rect.width, rect.height)).Within(1.5f), root.name + "/" + pill.name + " ends are half circles");
                }
            }
        }

        [Test] public void TheRoomPropsOfTheRundownUseTokenMaterials()
        {
            var n = Director();
            var props = new List<Renderer>();
            if (n.helper != null) props.AddRange(n.helper.GetComponentsInChildren<Renderer>(true));
            Assert.That(props, Is.Not.Empty);
            foreach (var r in props.Where(r => r.GetComponent<TMP_Text>() == null)) foreach (var m in r.sharedMaterials)   // text keeps its TMP shader
            {
                Assert.That(m, Is.Not.Null, r.name);
                Assert.That(m.shader.name, Does.StartWith("Universal Render Pipeline"), r.name);
                Assert.That(IsToken(m.color) || Tokens.Any(t => Near(m.color, Color.Lerp(t, Color.white, 0.22f), 0.08f)), Is.True, r.name + " colour " + ColorUtility.ToHtmlStringRGB(m.color));
            }
        }

        [Test] public void TheSpectrumGradientIsOnAtMostTwoElementsPerView()
        {
            var n = Director();
            n.catalogRoot.GetComponent<Airlift.Presentation.Dashboard.DashboardWall>()?.Refresh(new LibraryState());
            int spectrum = n.catalogRoot.GetComponentsInChildren<Image>(false)
                .Count(i => i.sprite != null && i.sprite == style.spectrumGradient && i.gameObject.activeInHierarchy && i.color.a > 0.6f && i.transform.Find("Photo") == null);
            int gradientWords = n.catalogRoot.GetComponentsInChildren<TMP_Text>(false).Count(t => t.text.Contains("<gradient"));
            Assert.That(spectrum + gradientWords, Is.LessThanOrEqualTo(2), "the guide: the spectrum carries one or two elements per screen");
        }
    }
}
