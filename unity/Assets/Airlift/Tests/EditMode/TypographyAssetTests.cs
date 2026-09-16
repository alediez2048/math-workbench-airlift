using NUnit.Framework;
using Airlift.Presentation;
using TMPro;
using UnityEditor;

namespace Airlift.Tests
{
    public class TypographyAssetTests
    {
        [Test] public void StyleBindingsResolve()
        {
            var style=AssetDatabase.LoadAssetAtPath<AirliftStyle>("Assets/Airlift/Fonts/AirliftStyle.asset");
            Assert.That(style,Is.Not.Null);Assert.That(style.headingFont,Is.Not.Null);Assert.That(style.bodyFont,Is.Not.Null);
            Assert.That(style.headingFont.atlasPopulationMode,Is.EqualTo(AtlasPopulationMode.Static));
            Assert.That(style.bodyFont.atlasPopulationMode,Is.EqualTo(AtlasPopulationMode.Static));
        }
    }
    public class FractionGlyphCoverageTests
    {
        [Test] public void DigitsAndOperatorsExistWithoutFallback()
        {
            var style=AssetDatabase.LoadAssetAtPath<AirliftStyle>("Assets/Airlift/Fonts/AirliftStyle.asset");
            foreach(var font in new[]{style.headingFont,style.bodyFont})
            foreach(char c in "0123456789+−=<>×÷é")Assert.That(font.HasCharacter(c,false,false),Is.True,"Missing: "+c);
        }
    }
}
