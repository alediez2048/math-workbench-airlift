using System.Collections.Generic;
using System.Linq;
using Airlift.Onboarding;
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
    /// Owner 2026-09-17: "the edges ... don't look polished, from beginning to end". One card language on every screen
    /// (consent, welcome, catalog, assistant bar, the three lesson cards): anti-aliased rendering, high-resolution
    /// rounded sprites that are never squashed into ovals, a hairline stroke and a soft shadow on every card.
    public class CardPolishTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity", Sprites = "Assets/Airlift/Sprites/";
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }
        static IEnumerable<T> All<T>() where T : Component => SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
        static NerdyDirector Director() => All<NerdyDirector>().First();

        static List<Image> Cards(bool includeHud)
        {
            var n = Director();
            var cards = new List<Image> { n.consentRoot.GetComponent<Image>(), n.welcomeRoot.GetComponent<Image>() };
            cards.AddRange(n.catalogRoot.GetComponentsInChildren<Button>(true).Where(b => b.name.StartsWith("Card ")).Select(b => b.GetComponent<Image>()));
            cards.AddRange(All<Image>().Where(i => i.name == "Backdrop"));
            if (includeHud) cards.Add(n.hudRoot.GetComponent<Image>());
            return cards;
        }

        static float UnitsPerSpritePixel(Image image)
        {
            var scaler = image.GetComponentInParent<CanvasScaler>(true);
            float reference = scaler != null ? scaler.referencePixelsPerUnit : 100f;
            return reference / (image.sprite.pixelsPerUnit * image.pixelsPerUnitMultiplier);
        }

        [Test] public void QuestRendersAntiAliasedAtFullScale()
        {
            foreach (var name in new[] { "Mobile_RPAsset", "PC_RPAsset" })
            {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Settings/" + name + ".asset");
                var so = new SerializedObject(asset);
                Assert.That(so.FindProperty("m_MSAA").intValue, Is.EqualTo(4), name + " 4x MSAA smooths every card and prop edge");
                Assert.That(so.FindProperty("m_RenderScale").floatValue, Is.EqualTo(1f).Within(1e-4f), name + " renders at full resolution");
            }
        }

        [Test] public void CardSpritesAreHighResolution()
        {
            foreach (var (name, minSize) in new[] { ("NerdyCard", 384), ("NerdyPill", 240), ("NerdyStroke", 384), ("NerdyShadow", 240), ("NerdyCardArt", 512), ("NerdyFade", 64) })
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(Sprites + name + ".png");
                Assert.That(importer, Is.Not.Null, name);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), name);
                Assert.That(importer.mipmapEnabled, Is.True, name + " mipmaps stop shimmer at a distance");
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Sprites + name + ".png");
                Assert.That(Mathf.Max(tex.width, tex.height), Is.GreaterThanOrEqualTo(minSize), name);
            }
        }

        [Test] public void RoundedCornersAreNeverSquashedIntoOvals()
        {
            foreach (var image in All<Image>().Where(i => i.sprite != null && i.type == Image.Type.Sliced && i.sprite.border != Vector4.zero))
            {
                float u = UnitsPerSpritePixel(image); var b = image.sprite.border; var rect = image.rectTransform.rect;
                Assert.That((b.x + b.z) * u, Is.LessThanOrEqualTo(rect.width + 0.5f), image.name + " corners fit its width");
                Assert.That((b.y + b.w) * u, Is.LessThanOrEqualTo(rect.height + 0.5f), image.name + " corners fit its height");
            }
        }

        [Test] public void PillsAreTrueCapsules()
        {
            var pills = All<Image>().Where(i => i.sprite != null && i.sprite.name == "NerdyPill").ToList();
            Assert.That(pills.Count, Is.GreaterThan(20));
            foreach (var pill in pills)
            {
                var rect = pill.rectTransform.rect;
                Assert.That(pill.type, Is.EqualTo(Image.Type.Sliced), pill.name);
                Assert.That(pill.sprite.border.x * UnitsPerSpritePixel(pill) * 2, Is.EqualTo(Mathf.Min(rect.width, rect.height)).Within(1f), pill.name + " ends are half circles");
            }
            Assert.That(All<Image>().Any(i => i.sprite != null && i.sprite.name == "RoundedRect"), Is.False, "old low-resolution rounded rect retired");
        }

        /// Stencil masks cannot anti-alias, so gradient pills (subject badges, Allow voice) are baked gradient capsules.
        [Test] public void GradientPillsAreBakedNotStencilMasked()
        {
            Assert.That(All<Mask>().Where(m => m.enabled).Select(m => m.name), Is.Empty, "no stencil-masked shapes");
            var gradientPills = All<Image>().Where(i => i.sprite != null && i.sprite.name == "NerdyPillGradient").ToList();
            Assert.That(gradientPills.Count, Is.GreaterThanOrEqualTo(4), "three subject badges and Allow voice");
            foreach (var pill in gradientPills)
            {
                var rect = pill.rectTransform.rect;
                Assert.That(pill.sprite.border.x * UnitsPerSpritePixel(pill) * 2, Is.EqualTo(Mathf.Min(rect.width, rect.height)).Within(1f), pill.name + " ends are half circles");
                Assert.That(pill.transform.Find("Gradient") == null || !pill.transform.Find("Gradient").gameObject.activeSelf, Is.True, pill.name + " no square gradient overlay");
            }
        }

        [Test] public void EveryCardSharesTheNerdyShapeWithStrokeAndShadow()
        {
            var cards = Cards(includeHud: true);
            Assert.That(cards.Count, Is.GreaterThanOrEqualTo(9), "consent, welcome, 3 catalog cards, 3 lesson cards, assistant bar");
            foreach (var card in cards)
            {
                Assert.That(card.sprite.name, Is.EqualTo("NerdyCard"), card.name + " uses the shared card shape");
                var stroke = card.transform.Find("Stroke")?.GetComponent<Image>();
                Assert.That(stroke, Is.Not.Null, card.name + " has a hairline stroke");
                Assert.That(stroke.sprite.name, Is.EqualTo("NerdyStroke")); Assert.That(stroke.raycastTarget, Is.False);
                Assert.That(stroke.rectTransform.anchorMin, Is.EqualTo(Vector2.zero)); Assert.That(stroke.rectTransform.anchorMax, Is.EqualTo(Vector2.one));
                Assert.That(stroke.color.a, Is.InRange(0.05f, 0.35f), card.name + " stroke stays subtle");
            }
            foreach (var card in Cards(includeHud: false))
            {
                var shadow = card.transform.parent.Find(card.name + " shadow")?.GetComponent<Image>();
                Assert.That(shadow, Is.Not.Null, card.name + " has a soft shadow");
                Assert.That(shadow.sprite.name, Is.EqualTo("NerdyShadow")); Assert.That(shadow.raycastTarget, Is.False);
                Assert.That(shadow.transform.GetSiblingIndex(), Is.LessThan(card.transform.GetSiblingIndex()), card.name + " shadow draws behind the card");
                var layout = shadow.GetComponent<LayoutElement>(); Assert.That(layout != null && layout.ignoreLayout, Is.True, card.name + " shadow never takes a layout slot");
                var link = card.GetComponent<Airlift.Presentation.CardShadowLink>();
                Assert.That(link != null && link.shadow == shadow.gameObject, Is.True, card.name + " shadow shows and hides with its card");
                Assert.That(shadow.gameObject.activeSelf, Is.EqualTo(card.gameObject.activeSelf), card.name);
            }
        }

        [Test] public void LessonButtonsArePills()
        {
            foreach (var backdrop in All<Image>().Where(i => i.name == "Backdrop"))
                foreach (var button in backdrop.transform.parent.GetComponentsInChildren<Button>(true))
                {
                    var image = button.GetComponent<Image>(); if (image == null || image.sprite == null) continue;
                    var size = image.rectTransform.rect; if (Mathf.Min(size.width, size.height) >= 150f) continue;   // card-sized tiles keep the card shape
                    Assert.That(image.sprite.name, Is.EqualTo("NerdyPill"), backdrop.transform.parent.name + "/" + button.name);
                }
        }

        [Test] public void CatalogArtFollowsTheCardCorners()
        {
            foreach (var card in Director().catalogRoot.GetComponentsInChildren<Button>(true).Where(b => b.name.StartsWith("Card ")))
            {
                Assert.That(card.transform.Find("Art").GetComponent<Image>().sprite.name, Is.EqualTo("NerdyCardArt"), card.name + " art has rounded top corners");
                Assert.That(card.transform.Find("Fade").GetComponent<Image>().sprite.name, Is.EqualTo("NerdyFade"), card.name + " art fades softly into the card");
            }
        }

        [Test] public void AssistantBarCaptionFitsInsideTheBar()
        {
            var n = Director(); var bar = n.hudRoot.GetComponent<RectTransform>().rect; var caption = n.captionText.rectTransform;
            float top = caption.anchoredPosition.y + caption.rect.height / 2, bottom = caption.anchoredPosition.y - caption.rect.height / 2;
            Assert.That(top, Is.LessThanOrEqualTo(bar.height / 2 - 6f), "caption keeps a margin under the bar's top edge");
            Assert.That(bottom, Is.GreaterThanOrEqualTo(-bar.height / 2 + 6f));
            Assert.That(n.captionText.enableAutoSizing, Is.True, "long captions shrink instead of spilling over the edge");
            Assert.That(n.captionText.fontSizeMin, Is.GreaterThanOrEqualTo(12f));
            var glass = n.hudRoot.transform.Find("Glass")?.GetComponent<Image>();
            if (glass != null) Assert.That(glass.sprite.name, Is.EqualTo("NerdyCard"), "the bar's sheen follows its rounded corners");
        }
    }
}
