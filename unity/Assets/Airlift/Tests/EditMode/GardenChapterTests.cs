using System.Collections.Generic;
using Airlift.Lessons.Garden;
using Airlift.Welcome;
using NUnit.Framework;

namespace Airlift.Tests
{
    public class GardenChapterTests
    {
        const int FirstRows = 0, EqualRows = 1, TurnTheBed = 2, SplitTheBed = 3, YourOwnSplit = 4;

        static GardenModel At(int index) { var m = new GardenModel(); m.StartChapter(index); return m; }

        // ---- chapter table ----

        [Test] public void ChapterTableMatchesContract()
        {
            var all = GardenChapter.All;
            Assert.That(all.Count, Is.EqualTo(5));
            string[] ids = { "first_rows", "equal_rows", "turn_the_bed", "split_the_bed", "your_own_split" };
            string[] titles = { "First rows", "Equal rows", "Turn the bed", "Split the bed", "Your own split" };
            int[] rows = { 3, 4, 3, 7, 8 };
            int[] columns = { 4, 5, 4, 6, 7 };
            bool[] planted = { false, false, true, true, true };
            bool[] turn = { false, false, true, false, false };
            bool[] fence = { false, false, false, true, true };
            int[] required = { 0, 0, 0, 5, 0 };
            string[] expressions = { "3 × 4 = 12", "4 × 5 = 20", "3 × 4 = 4 × 3 = 12", "7 × 6 = 7 × 5 + 7 × 1 = 42", "" };
            for (int i = 0; i < 5; i++)
            {
                var c = all[i];
                Assert.That(c.Number, Is.EqualTo(i + 1));
                Assert.That(c.Id, Is.EqualTo(ids[i]));
                Assert.That(c.Title, Is.EqualTo(titles[i]));
                Assert.That(c.Rows, Is.EqualTo(rows[i]), c.Id);
                Assert.That(c.Columns, Is.EqualTo(columns[i]), c.Id);
                Assert.That(c.StartsPlanted, Is.EqualTo(planted[i]), c.Id);
                Assert.That(c.NeedsTurn, Is.EqualTo(turn[i]), c.Id);
                Assert.That(c.HasFence, Is.EqualTo(fence[i]), c.Id);
                Assert.That(c.RequiredFenceColumn, Is.EqualTo(required[i]), c.Id);
                Assert.That(c.Expression, Is.EqualTo(expressions[i]), c.Id);
            }
            Assert.That(all[FirstRows].StripLengths, Is.EqualTo(new[] { 4, 4, 4, 3, 3 }));
            Assert.That(all[EqualRows].StripLengths, Is.EqualTo(new[] { 4, 5, 5, 5, 5, 6 }));
            for (int i = TurnTheBed; i <= YourOwnSplit; i++) Assert.That(all[i].StripLengths, Is.Empty, all[i].Id);
        }

        [Test] public void TrayChaptersHaveEnoughFullStripsAndSomeWrongOnes()
        {
            foreach (int i in new[] { FirstRows, EqualRows })
            {
                var c = GardenChapter.All[i];
                int full = 0, wrong = 0;
                foreach (int s in c.StripLengths) { if (s == c.Columns) full++; else wrong++; }
                Assert.That(full, Is.EqualTo(c.Rows), c.Id + ": exactly one full strip per row");
                Assert.That(wrong, Is.GreaterThan(0), c.Id + ": unequal rows must be possible");
            }
        }

        [Test] public void ExpressionsAreArithmeticallyTrue()
        {
            Assert.That(3 * 4, Is.EqualTo(12)); Assert.That(4 * 5, Is.EqualTo(20)); Assert.That(4 * 3, Is.EqualTo(12));
            Assert.That(7 * 5 + 7 * 1, Is.EqualTo(7 * 6));
            Assert.That(GardenChapter.SplitExpression(7, 6, 5), Is.EqualTo(GardenChapter.All[SplitTheBed].Expression));
            Assert.That(GardenChapter.SplitExpression(8, 7, 5), Is.EqualTo("8 × 7 = 8 × 5 + 8 × 2 = 56"));
            Assert.That(GardenChapter.SplitExpression(8, 7, 1), Is.EqualTo("8 × 7 = 8 × 1 + 8 × 6 = 56"));
            foreach (var c in GardenChapter.All)
            {
                if (c.Expression == "") continue;
                Assert.That(c.Expression, Does.StartWith(c.Rows + " × " + c.Columns + " = "), c.Id + ": rows × columns first");
                Assert.That(c.Expression, Does.EndWith("= " + c.Rows * c.Columns), c.Id);
            }
        }

        [Test] public void ChapterCopyIsShortAllowedCharactersAndSecondPerson()
        {
            foreach (var c in GardenChapter.All)
            {
                foreach (var text in new[] { c.Title, c.Story, c.Task, c.Accepted })
                {
                    Assert.That(string.IsNullOrWhiteSpace(text), Is.False, c.Id);
                    AssertAllowedCharacters(text, c.Id);
                    string lower = text.ToLowerInvariant();
                    foreach (var banned in new[] { "great", "awesome", "well done", "good job", "amazing", "perfect", "!", "hurry", "seconds", "star", "score" })
                        Assert.That(lower, Does.Not.Contain(banned), c.Id + ": " + text);
                }
                AssertAllowedCharacters(c.Expression, c.Id);
                Assert.That(c.Task.Split('.').Length, Is.LessThanOrEqualTo(2), c.Id + " task is one sentence");
                Assert.That(c.Task, Does.EndWith("."), c.Id);
                Assert.That(c.Story.Length, Is.LessThanOrEqualTo(140), c.Id + " story is short");
                Assert.That(c.Task.Length, Is.LessThanOrEqualTo(80), c.Id + " task is short");
            }
            Assert.That(GardenChapter.All[FirstRows].Story, Does.Contain("Sunny Plot"));
            Assert.That(GardenChapter.All[SplitTheBed].Task, Does.Contain("column 5"));
        }

        static void AssertAllowedCharacters(string text, string context)
        {
            foreach (char ch in text)
                Assert.That(ch < 128 || ch == '·' || ch == '—' || ch == '×' || ch == '÷', Is.True,
                    context + ": character U+" + ((int)ch).ToString("X4") + " in " + text);
        }

        // ---- guide steps ----

        [Test] public void BriefingNamesNothingToGrab()
        {
            var step = GardenSteps.Briefing();
            Assert.That(step.Id, Is.EqualTo("garden_briefing"));
            Assert.That(step.CanGrabNow, Is.False);
            Assert.That(step.OnTableNow, Does.Contain("yes"));
            Assert.That(step.OnTableNow.Length, Is.LessThanOrEqualTo(GuideSteps.MaxOnTableLength));
            AssertAllowedCharacters(step.OnTableNow, "briefing");
        }

        [Test] public void StepIdsFollowChapterNumberAndId()
        {
            string[] expected = { "garden_chapter1_first_rows", "garden_chapter2_equal_rows", "garden_chapter3_turn_the_bed",
                                  "garden_chapter4_split_the_bed", "garden_chapter5_your_own_split" };
            for (int i = 0; i < 5; i++) Assert.That(GardenSteps.ForChapter(At(i)).Id, Is.EqualTo(expected[i]));
        }

        [Test] public void FirstRowsStepDescribesBedTrayAndPlantedRows()
        {
            var m = At(FirstRows);
            var step = GardenSteps.ForChapter(m);
            Assert.That(step.OnTableNow, Does.Contain("Bed 3 × 4: 3 rows of 4."));
            Assert.That(step.OnTableNow, Does.Contain("No rows planted."));
            Assert.That(step.OnTableNow, Does.Contain("Tray strips: 4, 4, 4, 3, 3."));
            Assert.That(step.CanGrabNow, Is.True);

            Assert.That(m.Plant("strip-4", false, 1), Is.True);
            step = GardenSteps.ForChapter(m);
            Assert.That(step.OnTableNow, Does.Contain("Planted rows (seedlings): 2 (3)."));
            Assert.That(step.OnTableNow, Does.Contain("Empty rows: 1, 3."));
            Assert.That(step.OnTableNow, Does.Contain("Tray strips: 4, 4, 4, 3."));

            m.Plant("strip-1", false, 0); m.Plant("strip-2", false, 2);
            step = GardenSteps.ForChapter(m);
            Assert.That(step.CanGrabNow, Is.True, "the short row still has to come out");
            Assert.That(m.Unplant("strip-4"), Is.True);
            Assert.That(m.Plant("strip-3", false, 1), Is.True);
            step = GardenSteps.ForChapter(m);
            Assert.That(step.OnTableNow, Does.Contain("All 3 rows planted with strips of 4 (12 seedlings)."));
            Assert.That(step.CanGrabNow, Is.False, "full bed, nothing left to plant");

            Assert.That(m.Check(), Is.True);
            step = GardenSteps.ForChapter(m);
            Assert.That(step.OnTableNow, Does.Contain("Bed accepted: 3 × 4 = 12."));
            Assert.That(step.CanGrabNow, Is.False);
        }

        [Test] public void TurnStepReportsTurnState()
        {
            var m = At(TurnTheBed);
            var step = GardenSteps.ForChapter(m);
            Assert.That(step.OnTableNow, Does.Contain("All 3 rows planted with strips of 4 (12 seedlings)."));
            Assert.That(step.OnTableNow, Does.Contain("Not turned yet"));
            Assert.That(step.OnTableNow, Does.Not.Contain("Tray"));
            Assert.That(step.CanGrabNow, Is.False, "the turn is a button or voice action, nothing to grab");
            m.TurnBed(false);
            step = GardenSteps.ForChapter(m);
            Assert.That(step.OnTableNow, Does.Contain("Turned bed, now 4 × 3: 4 rows of 3."));
            Assert.That(step.OnTableNow, Does.Contain("Turned once"));
        }

        [Test] public void FenceStepReportsFenceAndParts()
        {
            var m = At(SplitTheBed);
            var step = GardenSteps.ForChapter(m);
            Assert.That(step.OnTableNow, Does.Contain("Bed 7 × 6: 7 rows of 6."));
            Assert.That(step.OnTableNow, Does.Contain("No fence yet; it goes after column 1 to 5."));
            Assert.That(step.CanGrabNow, Is.True);
            m.SetFence(2, false);
            step = GardenSteps.ForChapter(m);
            Assert.That(step.OnTableNow, Does.Contain("Fence after column 2: parts 7 × 2 and 7 × 4."));
            m.SetFence(5, false); m.Check();
            step = GardenSteps.ForChapter(m);
            Assert.That(step.CanGrabNow, Is.False);
            Assert.That(step.OnTableNow, Does.Contain("7 × 6 = 7 × 5 + 7 × 1 = 42"));
        }

        [Test] public void EveryReachableStepFitsLessonState()
        {
            int checkedSteps = 0;
            for (int i = 0; i < GardenChapter.All.Count; i++)
            {
                foreach (var m in ReachableStates(i))
                {
                    var step = GardenSteps.ForChapter(m);
                    Assert.That(step.OnTableNow.Length, Is.LessThanOrEqualTo(GuideSteps.MaxOnTableLength), step.OnTableNow);
                    AssertAllowedCharacters(step.OnTableNow, step.Id);
                    Assert.That(step.OnTableNow, Does.Contain(m.Rows + " × " + m.Columns));
                    checkedSteps++;
                }
            }
            Assert.That(checkedSteps, Is.GreaterThan(40));
        }

        /// Longest-text states per chapter: partly planted with short strips, every fence column, turned, accepted.
        static IEnumerable<GardenModel> ReachableStates(int index)
        {
            var c = GardenChapter.All[index];
            yield return At(index);
            if (!c.StartsPlanted)
            {
                var m = At(index);
                // plant the shortest strips first, leave the last row empty
                var ids = new List<string>(m.StripIds);
                ids.Sort((a, b) => m.StripLength(a).CompareTo(m.StripLength(b)));
                int row = 0;
                foreach (var id in ids) if (row < c.Rows - 1 && m.Plant(id, false, row)) { row++; yield return m; }
            }
            else
            {
                var m = At(index);
                // take out every other row
                for (int r = 0; r < c.Rows; r += 2) { m.Unplant(m.StripInRow(r)); yield return m; }
                if (c.HasFence)
                    for (int f = 1; f < c.Columns; f++)
                    {
                        var full = At(index); full.SetFence(f, false); yield return full;
                        var p = At(index); p.Unplant(p.StripInRow(c.Rows - 1)); p.SetFence(f, false); yield return p;
                    }
                if (c.NeedsTurn) { var t = At(index); t.TurnBed(false); yield return t; }
            }
            yield return Solved(index);
        }

        static GardenModel Solved(int index)
        {
            var m = At(index);
            var c = m.Chapter;
            if (!c.StartsPlanted)
            {
                int row = 0;
                foreach (var id in m.StripIds) if (m.StripLength(id) == c.Columns) m.Plant(id, false, row++);
            }
            if (c.NeedsTurn) m.TurnBed(false);
            if (c.HasFence) m.SetFence(c.RequiredFenceColumn != 0 ? c.RequiredFenceColumn : 1, false);
            Assert.That(m.Check(), Is.True, c.Id + ": " + m.LastFeedback);
            return m;
        }
    }
}
