using Airlift.Lessons;
using Airlift.Welcome;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Airlift.Tests
{
    /// Concept intro narration: a step shown by a button is said once from Update; a step reached through a voice tool
    /// is said once from the tool result's say_exactly, never twice.
    public class IntroNarrationTests
    {
        static IntroStep Whole => ConceptIntros.Cargo[0];
        static IntroStep Halves => ConceptIntros.Cargo[1];

        [Test] public void PromptAsksForTheStepWordForWordLikeTheGreeting()
        {
            Assert.That(IntroNarration.PromptFor(Whole), Is.EqualTo("Say these exact words word for word, then stop: \"" + Whole.Say + "\""));
            Assert.That(GuideIntro.PromptFor("en"), Does.StartWith("Say these exact words word for word, then stop: \""));
        }

        [Test] public void ANewStepIsNarratedOnce()
        {
            Assert.That(IntroNarration.ShouldNarrate(null, Whole), Is.True, "first step after the briefing");
            Assert.That(IntroNarration.ShouldNarrate("whole", Whole), Is.False, "already said");
            Assert.That(IntroNarration.ShouldNarrate("whole", Halves), Is.True, "Next pressed");
        }

        [Test] public void NothingIsNarratedOutsideTheIntro()
        {
            Assert.That(IntroNarration.ShouldNarrate(null, null), Is.False);
            Assert.That(IntroNarration.ShouldNarrate("quarters", null), Is.False, "chapter 1 started: the story line narrates");
        }

        [Test] public void ToolResultCarriesSayExactlyOnlyDuringTheIntro()
        {
            Assert.That(IntroNarration.WithSayExactly(null, null), Is.Null, "Cargo tool results stay byte-identical outside the intro");
            var say = new JObject { ["say"] = "keep" };
            Assert.That(IntroNarration.WithSayExactly(say, null), Is.SameAs(say));

            var merged = IntroNarration.WithSayExactly(say, Halves);
            Assert.That((string)merged["say_exactly"], Is.EqualTo(Halves.Say));
            Assert.That((string)merged["say"], Is.EqualTo("keep"));
            Assert.That(say["say_exactly"], Is.Null, "the caller's object is not changed");
            Assert.That((string)IntroNarration.WithSayExactly(null, Whole)["say_exactly"], Is.EqualTo(Whole.Say));
        }

        [Test] public void SayExactlyReachesTheStationToolResult()
        {
            var step = new GuideStep("intro", "One whole crate over the ruler, labelled 1.", false);
            var json = JObject.Parse(GuideTools.ToolResult(true, Halves.Say, step, Halves.Say, default,
                IntroNarration.WithSayExactly(null, Halves), "Cargo Crew", null));
            Assert.That((string)json["say_exactly"], Is.EqualTo(Halves.Say));
            Assert.That((string)json["step"], Is.EqualTo("intro"));
            Assert.That((bool)json["can_grab_now"], Is.False);
            Assert.That(json["chapter"], Is.Null);
        }

        /// Integration 2026-09-17: the line travels once, in say_exactly; the reason stays empty so Dee never says it twice.
        [Test] public void ReasonIsEmptiedWhenSayExactlyCarriesTheLine()
        {
            Assert.That(IntroNarration.ReasonFor(Halves.Say, Halves), Is.EqualTo(""));
            Assert.That(IntroNarration.ReasonFor("Let go of the crate first.", Halves), Is.EqualTo("Let go of the crate first."), "other reasons stay");
            Assert.That(IntroNarration.ReasonFor(Halves.Say, null), Is.EqualTo(Halves.Say), "outside the intro nothing changes");
        }
    }
}
