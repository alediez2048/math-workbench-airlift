using Airlift.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Airlift.Tests
{
    public class NerdyStyleTests
    {
        const string Glyphs = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz .,;:!?'-/()[]+−=<>×÷·é—–“”’";
        static NerdyStyle Style => AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");

        [Test] public void TokensMatchTheStyleGuide()
        {
            var s = Style; Assert.That(s, Is.Not.Null, "NerdyStyle asset");
            Assert.That(ColorUtility.ToHtmlStringRGB(s.baseColor), Is.EqualTo("202344"));
            Assert.That(ColorUtility.ToHtmlStringRGB(s.surface), Is.EqualTo("161C2C"));
            Assert.That(ColorUtility.ToHtmlStringRGB(s.line), Is.EqualTo("6C6E87"));
            Assert.That(ColorUtility.ToHtmlStringRGB(s.indigo), Is.EqualTo("3C4CDB"));
            Assert.That(ColorUtility.ToHtmlStringRGB(s.lavender), Is.EqualTo("9E97FF"));
            Assert.That(ColorUtility.ToHtmlStringRGB(s.amber), Is.EqualTo("FFC32B"));
            Assert.That(ColorUtility.ToHtmlStringRGB(s.magenta), Is.EqualTo("FB43DA"));
            Assert.That(ColorUtility.ToHtmlStringRGB(s.orchid), Is.EqualTo("D684FF"));
            Assert.That(ColorUtility.ToHtmlStringRGB(s.cyan), Is.EqualTo("17E2EA"));
            Assert.That(s.textMuted.a, Is.EqualTo(0.64f).Within(0.005f));
            Assert.That(NerdyStyle.RadiusCard, Is.EqualTo(20f)); Assert.That(NerdyStyle.RadiusPill, Is.EqualTo(100f));
        }

        [Test] public void FontsArePoppinsAndKarlaWithFullGlyphCoverage()
        {
            var s = Style; Assert.That(s, Is.Not.Null);
            var fonts = new[] { s.displayFont, s.displayItalic, s.bodyFont, s.bodySemibold, s.altFont, s.altBold };
            foreach (var f in fonts)
            {
                Assert.That(f, Is.Not.Null, "font slot assigned");
                Assert.That(f.name.StartsWith("Poppins") || f.name.StartsWith("Karla"), Is.True, f.name);
                Assert.That(f.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Static), f.name + " static atlas");
                Assert.That(f.HasCharacters(Glyphs, out var missing), Is.True, f.name + " missing " + missing);
            }
            Assert.That(s.displayFont.name, Does.StartWith("Poppins-Medium"));
            Assert.That(s.altFont.name, Does.StartWith("Karla-Medium"));
        }

        [Test] public void SpritesExistAndAreSliced()
        {
            var s = Style; Assert.That(s, Is.Not.Null);
            foreach (var sp in new[] { s.pill, s.card, s.small, s.brandGradient, s.spectrumGradient, s.glass })
                Assert.That(sp, Is.Not.Null, "sprite assigned");
            Assert.That(s.pill.border.x, Is.GreaterThan(0f), "pill is 9-sliced");
            Assert.That(s.card.border.x, Is.GreaterThan(0f), "card is 9-sliced");
        }
    }
}
