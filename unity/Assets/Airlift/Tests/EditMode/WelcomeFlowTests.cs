using Airlift.Guide;
using Airlift.Welcome;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Airlift.Tests
{
    public class WelcomeFlowTests
    {
        [Test] public void ConsentThenWelcomeThenCatalogThenLessonAndBack()
        {
            var f = new WelcomeFlow();
            Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Consent));
            Assert.That(f.RecordProfile("{\"ageBand\":\"adult\"}"), Is.False, "no profile before consent");
            f.Consent(true); Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Welcome)); Assert.That(f.VoiceConsented, Is.True);
            Assert.That(f.OpenLesson("cargo_crew_fractions"), Is.False, "cannot open a lesson from the welcome");
            Assert.That(f.EndWelcome(), Is.True); Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Catalog));
            Assert.That(f.OpenLesson("neighborhood_cafe_division"), Is.False, "previews never launch");
            Assert.That(f.OpenLesson("cargo_crew_fractions"), Is.True); Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Lesson));
            Assert.That(f.BackToCatalog(), Is.True); Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Catalog));
        }

        [Test] public void DeclinedVoiceStillReachesTheCatalog()
        {
            var f = new WelcomeFlow(); f.Consent(false);
            Assert.That(f.VoiceConsented, Is.False); Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Welcome));
            Assert.That(f.RecordProfile("{\"goal\":\"curious\"}"), Is.True);
            Assert.That(f.EndWelcome(), Is.True);
        }

        [Test] public void ProfileAcceptsOnlyKnownTagsAndCapsInterests()
        {
            var p = new LearnerProfile();
            Assert.That(p.Merge("{\"ageBand\":\"1999-01-01\",\"goal\":\"win\"}"), Is.False, "unknown values rejected");
            Assert.That(p.Merge("{\"ageBand\":\"14_to_17\",\"interests\":[\"Soccer\",\"soccer\",\"space\",\"\",\"x\",\"y\",\"z\",\"w\"],\"goal\":\"homework_help\",\"name\":\"Sam\"}"), Is.True);
            Assert.That(p.ageBand, Is.EqualTo("14_to_17")); Assert.That(p.IsMinor, Is.True);
            Assert.That(p.interests.Count, Is.EqualTo(5)); Assert.That(p.interests[0], Is.EqualTo("soccer"));
            Assert.That(p.ToJson(), Does.Not.Contain("Sam"), "names are never stored");
            var round = LearnerProfile.FromJson(p.ToJson());
            Assert.That(round.goal, Is.EqualTo("homework_help")); Assert.That(round.interests.Count, Is.EqualTo(5));
            Assert.That(new LearnerProfile().Merge("not json"), Is.False);
        }

        [Test] public void CatalogHasThreeCardsOnePlayableAndDescribesFromFacts()
        {
            Assert.That(LessonCatalog.Cards.Length, Is.EqualTo(3));
            Assert.That(System.Array.FindAll(LessonCatalog.Cards, c => c.Playable).Length, Is.EqualTo(1));
            var d = JObject.Parse(LessonCatalog.DescribeJson("cargo_crew_fractions"));
            Assert.That((bool)d["playable"], Is.True); Assert.That(((JArray)d["facts"]).Count, Is.GreaterThanOrEqualTo(3));
            Assert.That((string)JObject.Parse(LessonCatalog.DescribeJson("nope"))["error"], Is.EqualTo("unknown card"));
        }

        [Test] public void GuideContextNeverCarriesFreeTextBeyondAppStringsAndIsBounded()
        {
            string ctx = GuideContextBuilder.Lesson("Cargo Crew · Fractions", new string('x', 2000));
            Assert.That(ctx, Does.StartWith("APP CONTEXT "));
            var o = JObject.Parse(ctx.Substring("APP CONTEXT ".Length));
            Assert.That(((string)o["instruction"]).Length, Is.EqualTo(GuideContextBuilder.MaxLength));
            Assert.That((string)o["rules"], Does.Contain("Do not judge correctness"));
            var e = JObject.Parse(GuideContextBuilder.Entered("Cargo Crew", new[] { "a", "b" }).Substring("APP CONTEXT ".Length));
            Assert.That(((JArray)e["facts"]).Count, Is.EqualTo(2));
        }
    }
}
