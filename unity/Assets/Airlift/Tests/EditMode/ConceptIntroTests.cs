using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Airlift.Lessons;
using Airlift.Welcome;
using NUnit.Framework;

namespace Airlift.Tests
{
    /// Owner 2026-09-17: elementary learners get a short spoken concept intro before chapter 1 of every lesson
    /// (docs/00-build/INTRO-SPLITTER-CONTRACTS.md). Dee reads Say word for word and the card shows it.
    public class ConceptIntroTests
    {
        static readonly string[] PraiseWords = { "great", "awesome", "good job", "well done", "amazing", "perfect", "excellent", "super" };
        static readonly string[] GameWords = { "timer", "star", "score", "points", "hurry" };

        static IEnumerable<(string name, IReadOnlyList<IntroStep> steps, string question)> Lessons()
        {
            yield return ("Cargo", ConceptIntros.Cargo, "What is a fraction?");
            yield return ("Cafe", ConceptIntros.Cafe, "What is dividing?");
            yield return ("Garden", ConceptIntros.Garden, "What is multiplying?");
        }

        [Test] public void EachLessonHasItsStepsInOrder()
        {
            Assert.That(ConceptIntros.Cargo.Select(s => s.Id), Is.EqualTo(new[] { "whole", "halves", "sum", "quarters" }));
            Assert.That(ConceptIntros.Cafe.Select(s => s.Id), Is.EqualTo(new[] { "share", "groups", "boxes" }));
            Assert.That(ConceptIntros.Garden.Select(s => s.Id), Is.EqualTo(new[] { "row", "rows", "times" }));
        }

        [Test] public void ForFindsTheLessonByCardIdAndUnknownIdsGetNoIntro()
        {
            Assert.That(ConceptIntros.For("cargo_crew_fractions"), Is.SameAs(ConceptIntros.Cargo));
            Assert.That(ConceptIntros.For("neighborhood_cafe_division"), Is.SameAs(ConceptIntros.Cafe));
            Assert.That(ConceptIntros.For("community_garden_multiplication"), Is.SameAs(ConceptIntros.Garden));
            Assert.That(ConceptIntros.For("cargo_grid"), Is.Empty);
            Assert.That(ConceptIntros.For(""), Is.Empty);
            Assert.That(ConceptIntros.For(null), Is.Empty);
            foreach (var card in LessonCatalog.Cards) Assert.That(ConceptIntros.For(card.Id), Is.Not.Empty, card.Id);
        }

        [Test] public void IdsAreUniqueAndEachVisualIsItsId()
        {
            foreach (var (name, steps, _) in Lessons())
            {
                Assert.That(steps.Select(s => s.Id).Distinct().Count(), Is.EqualTo(steps.Count), name);
                foreach (var s in steps)
                {
                    Assert.That(s.Id, Is.Not.Empty, name);
                    Assert.That(s.Visual, Is.EqualTo(s.Id), name + " " + s.Id);
                }
            }
        }

        [Test] public void HeadingsAskTheQuestionAndCountTheSteps()
        {
            foreach (var (name, steps, question) in Lessons())
                for (int i = 0; i < steps.Count; i++)
                    Assert.That(steps[i].Heading, Is.EqualTo(question + " · " + (i + 1) + " of " + steps.Count), name);
        }

        [Test] public void DeeSaysShortSentencesWithoutPraiseOrGameWords()
        {
            foreach (var (name, steps, _) in Lessons())
                foreach (var s in steps)
                {
                    Assert.That(s.Say, Is.Not.Null.And.Not.Empty, name + " " + s.Id);
                    Assert.That(s.Say, Does.EndWith("."), name + " " + s.Id);
                    foreach (var sentence in s.Say.Split(new[] { ". ", "! ", "? " }, System.StringSplitOptions.RemoveEmptyEntries))
                        Assert.That(sentence.Split(' ').Length, Is.LessThanOrEqualTo(16), name + " " + s.Id + ": " + sentence);
                    string lower = s.Say.ToLowerInvariant();
                    foreach (var word in PraiseWords.Concat(GameWords))
                        Assert.That(Regex.IsMatch(lower, @"\b" + word + @"s?\b"), Is.False, name + " " + s.Id + " says " + word);
                    Assert.That(s.Expression, Is.Not.Null, name + " " + s.Id);
                }
        }

        [Test] public void FractionsAreReadAsOneOverTheNumberOfEqualParts()
        {
            Assert.That(ConceptIntros.Cargo[1].Say, Does.Contain("1 over 2"));
            Assert.That(ConceptIntros.Cargo[1].Say, Does.Contain("equal parts"));
            Assert.That(ConceptIntros.Cargo[3].Say, Does.Contain("1 over 4"));
            Assert.That(ConceptIntros.Cargo[3].Say, Does.Contain("More equal parts means smaller parts."));
        }

        [Test] public void ExpressionsMatchTheContract()
        {
            Assert.That(ConceptIntros.Cargo.Select(s => s.Expression), Is.EqualTo(new[] { "", "", "1/2 + 1/2 = 1", "" }));
            Assert.That(ConceptIntros.Cafe.Select(s => s.Expression), Is.EqualTo(new[] { "", "6 ÷ 2 = 3", "" }));
            Assert.That(ConceptIntros.Garden.Select(s => s.Expression), Is.EqualTo(new[] { "", "", "3 × 4 = 12" }));
        }

        [Test] public void DeeSaysTheContractLinesWordForWord()
        {
            Assert.That(ConceptIntros.Cargo.Select(s => s.Say), Is.EqualTo(new[]
            {
                "Before we load, let's learn fractions. This crate is one whole container. We write one whole as 1.",
                "Cut the whole into 2 equal parts. Each part is one half, written 1 over 2. The bottom number tells how many equal parts. The top number tells how many parts you have.",
                "Put the two halves together and they fill the whole container again. One half plus one half makes one.",
                "Cut it into 4 equal parts and each part is one quarter, written 1 over 4. More equal parts means smaller parts. Now let's load the trucks.",
            }));
            Assert.That(ConceptIntros.Cafe.Select(s => s.Say), Is.EqualTo(new[]
            {
                "Dividing means sharing into equal groups. Here are 6 croissants and 2 plates.",
                "Give one to each plate, again and again, until none are left. Now each plate has 3.",
                "We can also divide by packing boxes of the same size and counting the boxes. Let's open the café.",
            }));
            Assert.That(ConceptIntros.Garden.Select(s => s.Say), Is.EqualTo(new[]
            {
                "Multiplying counts equal rows quickly. Here is one row of 4 seedlings.",
                "Three equal rows of 4 make 4, 8, 12 plants.",
                "We write 3 rows of 4 as 3 times 4, which equals 12. Rows first, then how many in each row. Let's plant.",
            }));
        }
    }
}
