using System.Collections.Generic;
using Airlift.Lessons.Cafe;
using Airlift.Welcome;
using NUnit.Framework;

namespace Airlift.Tests
{
    public class CafeChapterTests
    {
        const int TwoFriends = 0, TableOfThree = 1, BoxItUp = 2, BiggerOrder = 3, FactFamily = 4;

        static CafeModel At(int index) { var m = new CafeModel(); m.StartChapter(index); return m; }

        // ---- chapter table ----

        [Test] public void ChapterTableMatchesContract()
        {
            var all = CafeChapter.All;
            Assert.That(all.Count, Is.EqualTo(5));
            string[] ids = { "two_friends", "table_of_three", "box_it_up", "bigger_order", "fact_family" };
            string[] titles = { "Two friends", "Table of three", "Box it up", "Bigger order", "Fact family" };
            string[] names = { "croissant", "pastry", "cookie", "muffin", "muffin" };
            string[] plurals = { "croissants", "pastries", "cookies", "muffins", "muffins" };
            int[] items = { 6, 12, 12, 15, 12 };
            string[] expressions = { "6 ÷ 2 = 3", "12 ÷ 3 = 4", "12 ÷ 4 = 3", "15 ÷ 5 = 3", "12 ÷ 4 = 3 · 12 ÷ 3 = 4 · 3 × 4 = 12" };
            for (int i = 0; i < 5; i++)
            {
                var c = all[i];
                Assert.That(c.Number, Is.EqualTo(i + 1));
                Assert.That(c.Id, Is.EqualTo(ids[i]));
                Assert.That(c.Title, Is.EqualTo(titles[i]));
                Assert.That(c.ItemName, Is.EqualTo(names[i]));
                Assert.That(c.ItemPlural, Is.EqualTo(plurals[i]));
                Assert.That(c.Items, Is.EqualTo(items[i]));
                Assert.That(c.Expression, Is.EqualTo(expressions[i]));
            }
            AssertStages(all[TwoFriends], (CafeTargetKind.Plates, 2, 0));
            AssertStages(all[TableOfThree], (CafeTargetKind.Plates, 3, 0));
            AssertStages(all[BoxItUp], (CafeTargetKind.Boxes, 5, 4));
            AssertStages(all[BiggerOrder], (CafeTargetKind.Boxes, 5, 5));
            AssertStages(all[FactFamily], (CafeTargetKind.Plates, 4, 0), (CafeTargetKind.Boxes, 6, 3));
        }

        static void AssertStages(CafeChapter c, params (CafeTargetKind kind, int containers, int capacity)[] expected)
        {
            Assert.That(c.Stages.Length, Is.EqualTo(expected.Length), c.Id);
            for (int s = 0; s < expected.Length; s++)
            {
                Assert.That(c.Stages[s].Kind, Is.EqualTo(expected[s].kind), c.Id);
                Assert.That(c.Stages[s].Containers, Is.EqualTo(expected[s].containers), c.Id);
                Assert.That(c.Stages[s].BoxCapacity, Is.EqualTo(expected[s].capacity), c.Id);
            }
        }

        [Test] public void EveryStageHasAnExactAnswerThatTheTableCanShow()
        {
            foreach (var c in CafeChapter.All)
                foreach (var s in c.Stages)
                {
                    if (s.Kind == CafeTargetKind.Plates)
                        Assert.That(c.Items % s.Containers, Is.EqualTo(0), c.Id + ": plates share evenly");
                    else
                    {
                        Assert.That(s.BoxCapacity, Is.GreaterThan(0), c.Id);
                        Assert.That(c.Items % s.BoxCapacity, Is.EqualTo(0), c.Id + ": boxes pack exactly");
                        Assert.That(c.Items / s.BoxCapacity, Is.LessThan(s.Containers), c.Id + ": more boxes than the order needs");
                    }
                }
        }

        [Test] public void ExpressionsAreTheDivisionsTheTableShows()
        {
            foreach (var c in CafeChapter.All)
            {
                var parts = new List<string>();
                foreach (var s in c.Stages)
                {
                    int divisor = s.Kind == CafeTargetKind.Plates ? s.Containers : s.BoxCapacity;
                    parts.Add(c.Items + " ÷ " + divisor + " = " + c.Items / divisor);
                }
                if (c.Stages.Length == 1) Assert.That(c.Expression, Is.EqualTo(parts[0]), c.Id);
                else
                {
                    Assert.That(c.Expression, Does.StartWith(parts[0] + " · " + parts[1]), c.Id);
                    var joined = new List<string>();
                    foreach (var s in c.Stages) joined.Add(s.Expression);
                    Assert.That(c.Expression, Is.EqualTo(string.Join(" · ", joined)), c.Id + ": chapter expression is the stages' expressions");
                }
            }
            Assert.That(CafeChapter.All[FactFamily].Expression, Does.EndWith("3 × 4 = 12"));
        }

        [Test] public void OnlyMultiStageChaptersCarryStageCopy()
        {
            foreach (var c in CafeChapter.All)
                foreach (var s in c.Stages)
                {
                    if (c.Stages.Length == 1)
                    {
                        Assert.That(s.Task, Is.Null, c.Id);
                        Assert.That(s.Expression, Is.Null, c.Id);
                        Assert.That(s.Accepted, Is.Null, c.Id);
                    }
                    else
                    {
                        Assert.That(s.Task, Is.Not.Empty, c.Id);
                        Assert.That(s.Expression, Is.Not.Empty, c.Id);
                    }
                }
            Assert.That(CafeChapter.All[FactFamily].Stages[0].Accepted, Is.Not.Empty);
        }

        [Test] public void ChapterCopyIsShortPlainAndDescribesTheCafe()
        {
            var texts = new List<(string id, string text)> { ("briefing", CafeChapter.BriefingStory) };
            foreach (var c in CafeChapter.All)
            {
                foreach (var t in new[] { c.Title, c.Story, c.Task, c.Accepted, c.Expression }) texts.Add((c.Id, t));
                foreach (var s in c.Stages)
                    foreach (var t in new[] { s.Task, s.Accepted, s.Expression }) if (t != null) texts.Add((c.Id, t));
                Assert.That(c.Task.Split('.').Length, Is.LessThanOrEqualTo(2), c.Id + " task is one sentence");
                foreach (var s in c.Stages) if (s.Task != null) Assert.That(s.Task.Split('.').Length, Is.LessThanOrEqualTo(2), c.Id);
                Assert.That(c.Story.Length, Is.LessThanOrEqualTo(140), c.Id);
                Assert.That(c.Task.Length, Is.LessThanOrEqualTo(140), c.Id);
                Assert.That(c.Task, Does.Contain(c.ItemPlural), c.Id + ": the task names the pastries");
                Assert.That(c.Task, Does.Contain(c.Items.ToString()), c.Id + ": the task names the count");
            }
            foreach (var (id, text) in texts)
            {
                Assert.That(string.IsNullOrWhiteSpace(text), Is.False, id);
                foreach (char ch in text)
                    Assert.That(ch < 128 || ch == '·' || ch == '—' || ch == '×' || ch == '÷' || ch == 'é', Is.True, id + ": character " + ch + " in " + text);
                string lower = text.ToLowerInvariant();
                Assert.That(lower, Does.Not.Contain("cafe"), id + ": spell it café: " + text);
                foreach (var banned in new[] { "great", "good job", "awesome", "well done", "perfect", "amazing", "hurry", "quick",
                                               "fast", "timer", "second", "minute", "star", "score", "point", "streak", "crate", "truck", "dock" })
                    Assert.That(lower, Does.Not.Contain(banned), id + ": " + text);
            }
        }

        [Test] public void PlatesAreForGuestsAndBoxesAreForOrders()
        {
            foreach (var c in CafeChapter.All)
            {
                var last = c.Stages[c.Stages.Length - 1];
                Assert.That(c.Accepted, Does.Contain(last.Kind == CafeTargetKind.Plates ? "plate" : "box"), c.Id);
                Assert.That(c.Task, Does.Contain(c.Stages[0].Kind == CafeTargetKind.Plates ? "plates" : "box"), c.Id);
            }
            Assert.That(CafeChapter.BriefingStory, Does.Contain("Dee"));
            Assert.That(CafeChapter.BriefingStory, Does.Contain("barista"));
            Assert.That(CafeChapter.BriefingStory, Does.Not.Contain("Welcome"), "the station says the welcome line");
            Assert.That(CafeChapter.BriefingStory.ToLowerInvariant(), Does.Not.Contain("caf"), "the welcome line already names the café");
        }

        [Test] public void ItemWordIsSingularOnlyForOne()
        {
            var c = CafeChapter.All[TableOfThree];
            Assert.That(c.ItemWord(1), Is.EqualTo("pastry"));
            Assert.That(c.ItemWord(0), Is.EqualTo("pastries"));
            Assert.That(c.ItemWord(2), Is.EqualTo("pastries"));
        }

        // ---- guide steps ----

        [Test] public void BriefingHasNothingToGrab()
        {
            var step = CafeSteps.Briefing();
            Assert.That(step.Id, Is.EqualTo("cafe_briefing"));
            Assert.That(step.CanGrabNow, Is.False);
            Assert.That(step.OnTableNow, Does.Contain("yes"));
            Assert.That(step.OnTableNow, Does.StartWith("The Corner Café briefing card only;"));
            Assert.That(step.OnTableNow.Length, Is.LessThanOrEqualTo(GuideSteps.MaxOnTableLength));
        }

        [Test] public void StepIdsFollowTheChapterIds()
        {
            string[] expected = { "cafe_chapter1_two_friends", "cafe_chapter2_table_of_three", "cafe_chapter3_box_it_up",
                                  "cafe_chapter4_bigger_order", "cafe_chapter5_fact_family" };
            for (int i = 0; i < 5; i++) Assert.That(CafeSteps.ForChapter(At(i)).Id, Is.EqualTo(expected[i]));
        }

        [Test] public void StepAtChapterStartNamesContainersCountsAndLooseItems()
        {
            var plates = CafeSteps.ForChapter(At(TwoFriends));
            Assert.That(plates.CanGrabNow, Is.True);
            Assert.That(plates.OnTableNow, Does.Contain("2 plates on the counter, holding 0 and 0."));
            Assert.That(plates.OnTableNow, Does.Contain("6 croissants loose on the tray."));
            Assert.That(plates.OnTableNow, Does.Contain("deal one round"));

            var boxes = CafeSteps.ForChapter(At(BoxItUp));
            Assert.That(boxes.CanGrabNow, Is.True);
            Assert.That(boxes.OnTableNow, Does.Contain("5 boxes (each holds 4) on the counter, holding 0, 0, 0, 0 and 0."));
            Assert.That(boxes.OnTableNow, Does.Contain("12 cookies loose on the tray."));
            Assert.That(boxes.OnTableNow, Does.Not.Contain("deal"));
        }

        [Test] public void StepFollowsTheTable()
        {
            var m = At(TableOfThree);
            m.DealRound(false);
            m.Place("item-4", false, 0);
            var step = CafeSteps.ForChapter(m);
            Assert.That(step.OnTableNow, Does.Contain("3 plates on the counter, holding 2, 1 and 1."));
            Assert.That(step.OnTableNow, Does.Contain("8 pastries loose on the tray."));
            for (int i = 5; i <= 11; i++) m.Place("item-" + i, false, 1);
            Assert.That(CafeSteps.ForChapter(m).OnTableNow, Does.Contain("1 pastry loose on the tray."));
        }

        [Test] public void FactFamilyStepsShowThePartAndWhatIsEarned()
        {
            var m = At(FactFamily);
            Assert.That(CafeSteps.ForChapter(m).OnTableNow, Does.Contain("Part 1 of 2."));
            Assert.That(CafeSteps.ForChapter(m).OnTableNow, Does.Not.Contain("Done so far"));
            while (m.DealRound(false)) { }
            Assert.That(m.Check(), Is.True);
            var step = CafeSteps.ForChapter(m);
            Assert.That(step.Id, Is.EqualTo("cafe_chapter5_fact_family"));
            Assert.That(step.CanGrabNow, Is.True);
            Assert.That(step.OnTableNow, Does.Contain("Part 2 of 2."));
            Assert.That(step.OnTableNow, Does.Contain("6 boxes (each holds 3) on the counter, holding 0, 0, 0, 0, 0 and 0."));
            Assert.That(step.OnTableNow, Does.Contain("12 muffins loose on the tray."));
            Assert.That(step.OnTableNow, Does.Contain("Done so far: 12 ÷ 4 = 3."));
        }

        [Test] public void CompleteChapterHasNothingToGrabAndSaysTheExpression()
        {
            var plates = At(TwoFriends);
            while (plates.DealRound(false)) { }
            Assert.That(plates.Check(), Is.True);
            var step = CafeSteps.ForChapter(plates);
            Assert.That(step.CanGrabNow, Is.False);
            Assert.That(step.OnTableNow, Does.Contain("6 ÷ 2 = 3"));
            Assert.That(step.OnTableNow, Does.Contain("plates go to the guests"));

            var boxes = At(BoxItUp);
            for (int i = 1; i <= 12; i++) boxes.Place("item-" + i, false, (i - 1) / 4);
            Assert.That(boxes.Check(), Is.True);
            step = CafeSteps.ForChapter(boxes);
            Assert.That(step.CanGrabNow, Is.False);
            Assert.That(step.OnTableNow, Does.Contain("12 ÷ 4 = 3"));
            Assert.That(step.OnTableNow, Does.Contain("delivery bike"));
        }

        [Test] public void StepTextStaysWithinTheGuideLimitInEveryState()
        {
            int longest = 0;
            var random = new System.Random(7);
            for (int chapter = 0; chapter < CafeChapter.All.Count; chapter++)
            {
                for (int trial = 0; trial < 60; trial++)
                {
                    var m = At(chapter);
                    for (int stage = 0; stage < m.Chapter.Stages.Length && !m.ChapterComplete; stage++)
                    {
                        foreach (var id in m.ItemIds)
                        {
                            if (random.Next(3) == 0) continue;
                            m.Place(id, false, random.Next(m.ContainerCount));
                            longest = System.Math.Max(longest, Check(m));
                        }
                        SolveStage(m);
                        longest = System.Math.Max(longest, Check(m));
                        m.Check();
                        longest = System.Math.Max(longest, Check(m));
                    }
                }
            }
            Assert.That(longest, Is.LessThanOrEqualTo(GuideSteps.MaxOnTableLength));
        }

        static int Check(CafeModel m)
        {
            var step = CafeSteps.ForChapter(m);
            Assert.That(step.OnTableNow.Length, Is.LessThanOrEqualTo(GuideSteps.MaxOnTableLength), step.OnTableNow);
            foreach (char ch in step.OnTableNow)
                Assert.That(ch < 128 || ch == '·' || ch == '×' || ch == '÷' || ch == 'é', Is.True, step.OnTableNow);
            Assert.That(step.OnTableNow.ToLowerInvariant(), Does.Not.Contain("cafe"), step.OnTableNow);
            Assert.That(step.CanGrabNow, Is.EqualTo(!m.ChapterComplete));
            return step.OnTableNow.Length;
        }

        static void SolveStage(CafeModel m)
        {
            m.ResetTable();
            var stage = m.Stage;
            int per = stage.Kind == CafeTargetKind.Plates ? m.Chapter.Items / stage.Containers : stage.BoxCapacity;
            int i = 0;
            foreach (var id in m.ItemIds) m.Place(id, false, i++ / per);
        }

        [Test] public void NullModelGivesAGenericStep()
        {
            var step = CafeSteps.ForChapter(null);
            Assert.That(step.Id, Is.EqualTo("cafe_chapter"));
            Assert.That(step.OnTableNow.Length, Is.GreaterThan(0));
        }
    }
}
