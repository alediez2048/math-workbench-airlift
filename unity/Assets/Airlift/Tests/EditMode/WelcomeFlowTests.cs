using System.Linq;
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
            Assert.That(f.EndWelcome(), Is.True); Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Catalog), "the host tour is an overlay, not a phase");
            f.MarkRundownSeen(); Assert.That(f.RundownSeen, Is.True);
            Assert.That(f.OpenLesson("made_up_lesson"), Is.False, "unknown cards never launch");
            Assert.That(f.OpenLesson("cargo_crew_fractions"), Is.True); Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Lesson));
            Assert.That(f.BackToCatalog(), Is.True); Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Catalog));
        }

        [Test] public void TheTourCanBeReplayedOnlyWhileBrowsing()
        {
            var f = new WelcomeFlow { RundownSeen = true };
            f.Consent(true); f.EndWelcome();
            Assert.That(f.CanReplayRundown, Is.True);
            f.OpenLesson("cargo_crew_fractions");
            Assert.That(f.CanReplayRundown, Is.False, "never over a lesson");
        }

        [Test] public void AReturningLearnerGoesFromConsentStraightToTheWall()
        {
            var f = new WelcomeFlow { RundownSeen = true };
            f.Profile.Merge("{\"ageBand\":\"adult\",\"interests\":[\"space\"],\"goal\":\"curious\"}");
            Assert.That(f.Profile.IsComplete, Is.True);
            f.Consent(true);
            Assert.That(f.Returning, Is.True);
            Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Catalog), "two presses and they are back where they were");
        }

        [Test] public void AHalfAnsweredProfileStillGetsTheQuestions()
        {
            var f = new WelcomeFlow { RundownSeen = true };
            f.Profile.Merge("{\"ageBand\":\"adult\"}");
            f.Consent(false);
            Assert.That(f.Returning, Is.False);
            Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Welcome));
        }

        // Owner 2026-09-18: onboarding off for now — the welcome card leads straight to the wall.
        [Test] public void WithOnboardingOffTheWelcomeCardLeadsStraightToTheWall()
        {
            var f = new WelcomeFlow { SkipOnboarding = true };
            f.Consent(true);
            Assert.That(f.Phase, Is.EqualTo(WelcomePhase.Catalog)); Assert.That(f.Returning, Is.False);
            Assert.That(f.OpenLesson("cargo_crew_fractions"), Is.True);
            Assert.That(NerdyDirector.OnboardingEnabled, Is.False, "the shipped switch");
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

        [Test] public void CatalogHasThreePlayableCardsAndDescribesFromFacts()
        {
            Assert.That(LessonCatalog.Cards.Length, Is.EqualTo(3));
            Assert.That(System.Array.FindAll(LessonCatalog.Cards, c => c.Playable).Select(c => c.Id).ToArray(), Is.EqualTo(new[] { "cargo_crew_fractions", "neighborhood_cafe_division", "community_garden_multiplication" }),
                "CC-GD-04: all three lessons are playable");
            var d = JObject.Parse(LessonCatalog.DescribeJson("cargo_crew_fractions"));
            Assert.That((bool)d["playable"], Is.True); Assert.That(((JArray)d["facts"]).Count, Is.GreaterThanOrEqualTo(3));
            Assert.That((string)JObject.Parse(LessonCatalog.DescribeJson("nope"))["error"], Is.EqualTo("unknown card"));
        }

        [Test] public void CafeCardTellsTheCornerCafeStoryAndItsVoicePhrases()
        {
            var cafe = LessonCatalog.Find("neighborhood_cafe_division");
            Assert.That(cafe.Playable, Is.True);
            Assert.That(cafe.Description, Does.StartWith("Corner Café").And.Not.Contain("Coming soon"));
            Assert.That(cafe.Description.Length, Is.LessThanOrEqualTo(LessonCatalog.Find("cargo_crew_fractions").Description.Length + 5), "fits the card like Cargo's");
            string facts = string.Join(" ", cafe.Facts);
            foreach (var phrase in new[] { "plates", "boxes", "Fact family", "head barista", "deal a round", "check the order", "clear the table", "12 ÷ 4 = 3", "3 × 4 = 12" })
                Assert.That(facts, Does.Contain(phrase));
            foreach (var word in new[] { "crate", "truck", "Dock 7", "not available" }) Assert.That(facts, Does.Not.Contain(word));
            var d = JObject.Parse(LessonCatalog.DescribeJson("neighborhood_cafe_division"));
            Assert.That((bool)d["playable"], Is.True); Assert.That(((JArray)d["facts"]).Count, Is.GreaterThanOrEqualTo(5));
        }

        [Test] public void GardenCardTellsTheSunnyPlotStoryAndItsVoicePhrases()
        {
            var garden = LessonCatalog.Find("community_garden_multiplication");
            Assert.That(garden.Playable, Is.True);
            Assert.That(garden.Description, Does.StartWith("Sunny Plot").And.Not.Contain("Coming soon"));
            Assert.That(garden.Description.Length, Is.LessThanOrEqualTo(LessonCatalog.Find("cargo_crew_fractions").Description.Length + 5), "fits the card like Cargo's");
            string facts = string.Join(" ", garden.Facts);
            foreach (var phrase in new[] { "head gardener", "strips", "rows × columns", "3 rows of 4", "fence", "turn the bed", "split it at five", "check the bed", "clear the bed", "7 × 6 = 7 × 5 + 7 × 1 = 42" })
                Assert.That(facts, Does.Contain(phrase));
            foreach (var word in new[] { "crate", "truck", "pastr", "not available" }) Assert.That(facts, Does.Not.Contain(word));
            var d = JObject.Parse(LessonCatalog.DescribeJson("community_garden_multiplication"));
            Assert.That((bool)d["playable"], Is.True); Assert.That(((JArray)d["facts"]).Count, Is.GreaterThanOrEqualTo(5));
        }

        [Test] public void CatalogLinesNameAllThreePlayableLessons()
        {
            Assert.That(LessonCatalog.ReadyPhrase(), Is.EqualTo("Cargo Crew, Neighborhood Café and Community Garden are ready"));
            Assert.That(LessonCatalog.CatalogNote(), Is.EqualTo("Three lesson cards are in front of the learner. All of them can be opened: Cargo Crew (fractions), Neighborhood Café (division) and Community Garden (multiplication)."));
            var o = JObject.Parse(GuideContextBuilder.Catalog().Substring("APP CONTEXT ".Length));
            Assert.That((string)o["kind"], Is.EqualTo("catalog")); Assert.That((string)o["note"], Is.EqualTo(LessonCatalog.CatalogNote()));
            Assert.That(GuideTools.NoLesson, Does.Not.Contain("Cargo Crew"), "the refusal no longer names a single lesson");
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
