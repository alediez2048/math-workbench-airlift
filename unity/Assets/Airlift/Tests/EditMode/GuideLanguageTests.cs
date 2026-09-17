using System.Collections.Generic;
using Airlift.Guide;
using Airlift.Welcome;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;

namespace Airlift.Tests
{
    /// Owner 2026-09-17: "voice ai keeps switching from english to spanish ... select a language ... and stick to only that
    /// language". The consent card picks English or Español; the mint request carries it and the server locks the session.
    public class GuideLanguageTests
    {
        [Test] public void EnglishAndSpanishAreOfferedAndUnknownCodesMeanEnglish()
        {
            Assert.That(GuideLanguage.Codes, Is.EqualTo(new[] { "en", "es" }));
            Assert.That(GuideLanguage.Default, Is.EqualTo("en"));
            Assert.That(GuideLanguage.Label("en"), Is.EqualTo("English"));
            Assert.That(GuideLanguage.Label("es"), Is.EqualTo("Español"));
            Assert.That(GuideLanguage.Normalize("fr"), Is.EqualTo("en"));
            Assert.That(GuideLanguage.Normalize(null), Is.EqualTo("en"));
            Assert.That(GuideLanguage.Normalize("es"), Is.EqualTo("es"));
        }

        [Test] public void MintRequestCarriesTheChosenLanguage()
        {
            var body = JObject.Parse(GuideMessages.MintRequestBody("abc12345nonce", "0.2.0", "es"));
            Assert.That((string)body["language"], Is.EqualTo("es"));
            Assert.That((string)body["launchNonce"], Is.EqualTo("abc12345nonce"));
            Assert.That((string)body["build"], Is.EqualTo("0.2.0"));
            Assert.That((string)JObject.Parse(GuideMessages.MintRequestBody("abc12345nonce", "0.2.0", "xx"))["language"], Is.EqualTo("en"));
        }

        [Test] public void DeeGreetsInTheChosenLanguage()
        {
            Assert.That(GuideIntro.PromptFor("en"), Does.Contain(GuideIntro.Spoken));
            Assert.That(GuideIntro.SpokenFor("es"), Does.StartWith("¡Bienvenido a Nerdy AI plus VR! Me llamo Dee"));
            Assert.That(GuideIntro.PromptFor("es"), Does.Contain(GuideIntro.SpokenFor("es")));
            Assert.That(GuideIntro.PromptFor("es"), Does.Contain("word for word"));
        }

        /// Nerdy is the app; the guide character in every story is Dee (owner 2026-09-17).
        [Test] public void StoriesCallTheGuideDeeNotNerdy()
        {
            var copy = new List<string> { Airlift.Lessons.Cafe.CafeChapter.BriefingStory };
            foreach (var chapter in Airlift.Lessons.Cafe.CafeChapter.All) copy.Add(chapter.Story);
            foreach (var card in LessonCatalog.Cards) copy.AddRange(card.Facts);
            foreach (var line in copy)
                Assert.That(System.Text.RegularExpressions.Regex.IsMatch(line, @"\bNerdy (is|baked|has|set)\b"), Is.False, line);
            Assert.That(Airlift.Lessons.Cafe.CafeChapter.BriefingStory, Does.StartWith("Dee is the head barista"));
        }

        [Test] public void NerdyFontsDrawSpanishCaptions()
        {
            const string spanish = "áéíóúüñÁÉÍÓÚÜÑ¡¿";
            foreach (var name in new[] { "Poppins-Regular", "Poppins-Medium", "Poppins-MediumItalic", "Poppins-SemiBold", "Karla-Medium", "Karla-Bold" })
            {
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Airlift/Fonts/Nerdy/" + name + " SDF.asset");
                Assert.That(font, Is.Not.Null, name);
                Assert.That(font.HasCharacters(spanish, out var missing), Is.True, name + " missing " + (missing == null ? "" : string.Join(",", missing)));
            }
        }
    }
}
