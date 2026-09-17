using Airlift.Lessons.Garden;
using NUnit.Framework;

namespace Airlift.Tests
{
    public class GardenModelTests
    {
        const int FirstRows = 0, EqualRows = 1, TurnTheBed = 2, SplitTheBed = 3, YourOwnSplit = 4;

        static GardenModel At(int index) { var m = new GardenModel(); m.StartChapter(index); return m; }

        /// Chapter 1 strips: strip-1..3 have 4 seedlings, strip-4 and strip-5 have 3.
        static GardenModel FirstRowsPlanted(string row1, string row2, string row3)
        {
            var m = At(FirstRows);
            Assert.That(m.Plant(row1, false, 0), Is.True);
            Assert.That(m.Plant(row2, false, 1), Is.True);
            Assert.That(m.Plant(row3, false, 2), Is.True);
            return m;
        }

        // ---- start state ----

        [Test] public void StartsAtFirstRowsWithStripsInTheTray()
        {
            var m = new GardenModel();
            Assert.That(m.ChapterIndex, Is.EqualTo(FirstRows));
            Assert.That(m.Chapter.Id, Is.EqualTo("first_rows"));
            Assert.That(m.Rows, Is.EqualTo(3));
            Assert.That(m.Columns, Is.EqualTo(4));
            Assert.That(m.StripIds, Is.EqualTo(new[] { "strip-1", "strip-2", "strip-3", "strip-4", "strip-5" }));
            int[] lengths = { 4, 4, 4, 3, 3 };
            for (int i = 0; i < 5; i++)
            {
                Assert.That(m.StripLength("strip-" + (i + 1)), Is.EqualTo(lengths[i]));
                Assert.That(m.RowOf("strip-" + (i + 1)), Is.EqualTo(-1));
            }
            for (int r = 0; r < 3; r++) Assert.That(m.StripInRow(r), Is.Null);
            Assert.That(m.PlantCount, Is.EqualTo(0));
            Assert.That(m.Turned, Is.False);
            Assert.That(m.FenceColumn, Is.EqualTo(0));
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.Expression, Is.EqualTo(""));
            Assert.That(m.LastFeedback, Is.EqualTo(""));
            Assert.That(m.StripLength("nope"), Is.EqualTo(0));
            Assert.That(m.StripLength(null), Is.EqualTo(0));
            Assert.That(m.RowOf(null), Is.EqualTo(-1));
            Assert.That(m.StripInRow(-1), Is.Null);
            Assert.That(m.StripInRow(3), Is.Null);
        }

        [Test] public void PlantedChaptersStartWithOneFullStripPerRow()
        {
            foreach (int i in new[] { TurnTheBed, SplitTheBed, YourOwnSplit })
            {
                var m = At(i);
                var c = m.Chapter;
                Assert.That(m.StripIds.Count, Is.EqualTo(c.Rows), c.Id);
                for (int r = 0; r < c.Rows; r++)
                {
                    Assert.That(m.StripInRow(r), Is.EqualTo("strip-" + (r + 1)), c.Id);
                    Assert.That(m.StripLength(m.StripInRow(r)), Is.EqualTo(c.Columns), c.Id);
                }
                Assert.That(m.PlantCount, Is.EqualTo(c.Rows * c.Columns), c.Id);
                Assert.That(m.TrayStripIds, Is.Empty, c.Id);
            }
        }

        // ---- plant ----

        [Test] public void PlantRefusesHeldUnknownBadRowOccupiedAndAlreadyPlanted()
        {
            var m = At(FirstRows);
            Assert.That(m.Plant("strip-1", held: true, 0), Is.False, "held");
            Assert.That(m.Plant("strip-9", false, 0), Is.False, "unknown");
            Assert.That(m.Plant(null, false, 0), Is.False, "null");
            Assert.That(m.Plant("strip-1", false, -1), Is.False, "row below range");
            Assert.That(m.Plant("strip-1", false, 3), Is.False, "row above range");
            Assert.That(m.PlantCount, Is.EqualTo(0));

            Assert.That(m.Plant("strip-1", false, 0), Is.True);
            Assert.That(m.RowOf("strip-1"), Is.EqualTo(0));
            Assert.That(m.StripInRow(0), Is.EqualTo("strip-1"));
            Assert.That(m.PlantCount, Is.EqualTo(4));

            Assert.That(m.Plant("strip-2", false, 0), Is.False, "occupied");
            Assert.That(m.LastFeedback, Is.EqualTo("Row 1 already has a strip."));
            Assert.That(m.StripInRow(0), Is.EqualTo("strip-1"));
            Assert.That(m.Plant("strip-1", false, 1), Is.False, "already planted elsewhere");
            Assert.That(m.RowOf("strip-1"), Is.EqualTo(0));
        }

        [Test] public void PlantRefusesAStripLongerThanTheRow()
        {
            var m = At(EqualRows);
            Assert.That(m.StripLength("strip-6"), Is.EqualTo(6));
            Assert.That(m.Plant("strip-6", false, 0), Is.False);
            Assert.That(m.RowOf("strip-6"), Is.EqualTo(-1));
            Assert.That(m.LastFeedback, Is.EqualTo("That strip has 6 seedlings; a row in this bed has room for 5."));
            Assert.That(m.Plant("strip-1", false, 0), Is.True, "a shorter strip may be planted");
        }

        [Test] public void UnplantReturnsTheStripToTheTray()
        {
            var m = At(FirstRows);
            Assert.That(m.Unplant("strip-1"), Is.False, "not planted");
            Assert.That(m.Unplant("nope"), Is.False);
            m.Plant("strip-4", false, 2);
            Assert.That(m.Unplant("strip-4"), Is.True);
            Assert.That(m.RowOf("strip-4"), Is.EqualTo(-1));
            Assert.That(m.StripInRow(2), Is.Null);
            Assert.That(m.Plant("strip-1", false, 2), Is.True, "the row is free again");
        }

        // ---- check: rows ----

        [Test] public void CheckOnAnEmptyBedAsksForPlanting()
        {
            var m = At(FirstRows);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo(GardenModel.EmptyBedFeedback));
            Assert.That(m.ChapterComplete, Is.False);
        }

        [Test] public void CheckNamesTheFirstEmptyRow()
        {
            var m = At(FirstRows);
            m.Plant("strip-1", false, 0); m.Plant("strip-2", false, 1);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Row 3 is still empty."));
        }

        [Test] public void ShortRowFailsOnCheckNamingRowAndCountsAndStaysPlanted()
        {
            var m = FirstRowsPlanted("strip-1", "strip-4", "strip-2");
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Row 2 has 3 seedlings; the other rows have 4."));
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.Expression, Is.EqualTo(""));
            Assert.That(m.RowOf("strip-4"), Is.EqualTo(1), "wrong rows stay planted and editable");

            Assert.That(m.Unplant("strip-4"), Is.True);
            Assert.That(m.Plant("strip-3", false, 1), Is.True);
            Assert.That(m.Check(), Is.True);
        }

        [Test] public void EmptyRowIsReportedBeforeShortRows()
        {
            var m = At(FirstRows);
            m.Plant("strip-4", false, 0); m.Plant("strip-5", false, 1);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Row 3 is still empty."));
        }

        [Test] public void FirstRowsAcceptsThreeRowsOfFour()
        {
            var m = FirstRowsPlanted("strip-3", "strip-1", "strip-2");
            Assert.That(m.PlantCount, Is.EqualTo(12));
            Assert.That(m.Check(), Is.True);
            Assert.That(m.ChapterComplete, Is.True);
            Assert.That(m.Expression, Is.EqualTo("3 × 4 = 12"));
            Assert.That(m.LastFeedback, Is.EqualTo(m.Chapter.Accepted + " 3 × 4 = 12"));
        }

        [Test] public void EqualRowsRejectsMixedRows()
        {
            // strips: 1=4, 2..5=5, 6=6
            var m = At(EqualRows);
            m.Plant("strip-1", false, 0); m.Plant("strip-2", false, 1); m.Plant("strip-3", false, 2); m.Plant("strip-4", false, 3);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Row 1 has 4 seedlings; the other rows have 5."));
            Assert.That(m.ChapterComplete, Is.False);

            m.Unplant("strip-1");
            Assert.That(m.Plant("strip-5", false, 0), Is.True);
            Assert.That(m.PlantCount, Is.EqualTo(20));
            Assert.That(m.Check(), Is.True);
            Assert.That(m.Expression, Is.EqualTo("4 × 5 = 20"));
        }

        [Test] public void SeveralShortRowsAreListed()
        {
            var m = At(FirstRows);
            m.Plant("strip-4", false, 0); m.Plant("strip-1", false, 1); m.Plant("strip-5", false, 2);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Row 1 has 3 and row 3 has 3; a full row in this bed has 4."));
        }

        // ---- turn ----

        [Test] public void TurnTheBedSwapsRowsAndColumnsAndKeepsTheSeedlings()
        {
            var m = At(TurnTheBed);
            int generation = m.Generation;
            Assert.That(m.Check(), Is.False, "not turned yet");
            Assert.That(m.LastFeedback, Is.EqualTo(GardenModel.NeedsTurnFeedback));

            Assert.That(m.TurnBed(held: true), Is.False);
            Assert.That(m.TurnBed(false), Is.True);
            Assert.That(m.Turned, Is.True);
            Assert.That(m.Rows, Is.EqualTo(4));
            Assert.That(m.Columns, Is.EqualTo(3));
            Assert.That(m.PlantCount, Is.EqualTo(12));
            Assert.That(m.Generation, Is.GreaterThan(generation));
            Assert.That(m.StripIds, Is.EqualTo(new[] { "strip-1", "strip-2", "strip-3", "strip-4" }));
            for (int r = 0; r < 4; r++)
            {
                Assert.That(m.StripInRow(r), Is.EqualTo("strip-" + (r + 1)));
                Assert.That(m.StripLength("strip-" + (r + 1)), Is.EqualTo(3));
            }

            Assert.That(m.TurnBed(false), Is.True, "a second turn goes back");
            Assert.That(m.Turned, Is.False);
            Assert.That(m.Rows, Is.EqualTo(3));
            Assert.That(m.Columns, Is.EqualTo(4));
            Assert.That(m.StripIds, Is.EqualTo(new[] { "strip-1", "strip-2", "strip-3" }));
            Assert.That(m.PlantCount, Is.EqualTo(12));
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo(GardenModel.NeedsTurnFeedback));

            m.TurnBed(false);
            Assert.That(m.Check(), Is.True);
            Assert.That(m.Expression, Is.EqualTo("3 × 4 = 4 × 3 = 12"));
            Assert.That(m.LastFeedback, Is.EqualTo(m.Chapter.Accepted + " 3 × 4 = 4 × 3 = 12"));
        }

        [Test] public void TurnNeedsEveryRowPlantedWithAFullStrip()
        {
            var m = At(TurnTheBed);
            m.Unplant("strip-2");
            Assert.That(m.TurnBed(false), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo(GardenModel.TurnNeedsFullRowsFeedback));
            Assert.That(m.Rows, Is.EqualTo(3));
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Row 2 is still empty."));
            Assert.That(m.Plant("strip-2", false, 1), Is.True);
            Assert.That(m.TurnBed(false), Is.True);
        }

        [Test] public void OnlyTheTurningChapterTurns()
        {
            var m = FirstRowsPlanted("strip-1", "strip-2", "strip-3");
            Assert.That(m.TurnBed(false), Is.False);
            Assert.That(m.Rows, Is.EqualTo(3));
            var s = At(SplitTheBed);
            Assert.That(s.TurnBed(false), Is.False);
            Assert.That(s.Rows, Is.EqualTo(7));
        }

        // ---- fence ----

        [Test] public void FenceAcceptsOnlyColumnsBetweenColumns()
        {
            var m = At(SplitTheBed);
            Assert.That(m.SetFence(3, held: true), Is.False);
            Assert.That(m.SetFence(0, false), Is.False);
            Assert.That(m.SetFence(6, false), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("The fence goes after column 1 to 5."));
            Assert.That(m.FenceColumn, Is.EqualTo(0));
            for (int f = 1; f <= 5; f++) { Assert.That(m.SetFence(f, false), Is.True); Assert.That(m.FenceColumn, Is.EqualTo(f)); }
            Assert.That(At(FirstRows).SetFence(1, false), Is.False, "no fence in a tray chapter");
            Assert.That(At(TurnTheBed).SetFence(1, false), Is.False, "no fence in the turning chapter");
        }

        [Test] public void SplitTheBedAcceptsOnlyTheFenceAfterColumnFive()
        {
            var m = At(SplitTheBed);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo(GardenModel.NeedsFenceFeedback));
            Assert.That(m.LastFeedback, Is.EqualTo("Put the fence between two columns first."));

            m.SetFence(3, false);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("The fence is after column 3, so the parts are 7 × 3 and 7 × 3. Put it after column 5."));
            Assert.That(m.FenceColumn, Is.EqualTo(3), "the fence stays where it was put");

            m.SetFence(5, false);
            Assert.That(m.Check(), Is.True);
            Assert.That(m.Expression, Is.EqualTo("7 × 6 = 7 × 5 + 7 × 1 = 42"));
            Assert.That(m.PlantCount, Is.EqualTo(42));
        }

        [Test] public void YourOwnSplitAcceptsEveryFenceAndBuildsItsExpression()
        {
            for (int f = 1; f <= 6; f++)
            {
                var m = At(YourOwnSplit);
                Assert.That(m.Check(), Is.False);
                Assert.That(m.LastFeedback, Is.EqualTo(GardenModel.NeedsFenceFeedback));
                Assert.That(m.SetFence(f, false), Is.True);
                Assert.That(m.Check(), Is.True, "fence " + f);
                string expected = "8 × 7 = 8 × " + f + " + 8 × " + (7 - f) + " = 56";
                Assert.That(m.Expression, Is.EqualTo(expected));
                Assert.That(m.LastFeedback, Is.EqualTo(m.Chapter.Accepted + " " + expected));
                Assert.That(8 * f + 8 * (7 - f), Is.EqualTo(56));
            }
            Assert.That(At(YourOwnSplit).SetFence(7, false), Is.False);
        }

        [Test] public void FenceChapterStillNeedsFullRows()
        {
            var m = At(YourOwnSplit);
            m.Unplant("strip-8");
            m.SetFence(4, false);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Row 8 is still empty."));
        }

        // ---- finished bed ----

        [Test] public void AcceptedBedNoLongerChanges()
        {
            var m = At(SplitTheBed);
            m.SetFence(5, false); m.Check();
            Assert.That(m.SetFence(2, false), Is.False);
            Assert.That(m.FenceColumn, Is.EqualTo(5));
            Assert.That(m.Unplant("strip-1"), Is.False);
            int generation = m.Generation;
            m.ResetTable();
            Assert.That(m.Generation, Is.EqualTo(generation));
            Assert.That(m.ChapterComplete, Is.True);
            Assert.That(m.Check(), Is.True, "checking again repeats the accepted line");
            Assert.That(m.Expression, Is.EqualTo("7 × 6 = 7 × 5 + 7 × 1 = 42"));

            var t = At(TurnTheBed);
            t.TurnBed(false); t.Check();
            Assert.That(t.TurnBed(false), Is.False);
            Assert.That(t.Turned, Is.True);

            var f = FirstRowsPlanted("strip-1", "strip-2", "strip-3");
            f.Check();
            Assert.That(f.Plant("strip-4", false, 0), Is.False);
        }

        // ---- reset, restart, next ----

        [Test] public void ResetTableUnplantsTrayChapters()
        {
            var m = FirstRowsPlanted("strip-1", "strip-4", "strip-2");
            m.Check();
            int generation = m.Generation;
            m.ResetTable();
            Assert.That(m.Generation, Is.GreaterThan(generation));
            Assert.That(m.PlantCount, Is.EqualTo(0));
            foreach (var id in m.StripIds) Assert.That(m.RowOf(id), Is.EqualTo(-1));
            Assert.That(m.StripIds.Count, Is.EqualTo(5));
            Assert.That(m.LastFeedback, Is.EqualTo(""));
        }

        [Test] public void ResetTableClearsTurnAndFenceInPlantedChapters()
        {
            var t = At(TurnTheBed);
            t.TurnBed(false);
            t.ResetTable();
            Assert.That(t.Turned, Is.False);
            Assert.That(t.Rows, Is.EqualTo(3));
            Assert.That(t.Columns, Is.EqualTo(4));
            Assert.That(t.StripIds.Count, Is.EqualTo(3));
            Assert.That(t.PlantCount, Is.EqualTo(12));

            var s = At(SplitTheBed);
            s.SetFence(2, false);
            s.Unplant("strip-3");
            s.ResetTable();
            Assert.That(s.FenceColumn, Is.EqualTo(0));
            Assert.That(s.StripInRow(2), Is.EqualTo("strip-3"), "the planted start bed comes back");
            Assert.That(s.PlantCount, Is.EqualTo(42));
        }

        [Test] public void RestartClearsCompletionAndBumpsGeneration()
        {
            var m = FirstRowsPlanted("strip-1", "strip-2", "strip-3");
            m.Check();
            int generation = m.Generation;
            m.RestartChapter();
            Assert.That(m.ChapterIndex, Is.EqualTo(FirstRows));
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.Expression, Is.EqualTo(""));
            Assert.That(m.PlantCount, Is.EqualTo(0));
            Assert.That(m.Generation, Is.GreaterThan(generation));
        }

        [Test] public void NextChapterOnlyWhenCompleteAndNotLast()
        {
            var m = new GardenModel();
            Assert.That(m.NextChapter(), Is.False, "not complete");
            m.Plant("strip-1", false, 0); m.Plant("strip-2", false, 1); m.Plant("strip-3", false, 2);
            m.Check();
            int generation = m.Generation;
            Assert.That(m.NextChapter(), Is.True);
            Assert.That(m.ChapterIndex, Is.EqualTo(EqualRows));
            Assert.That(m.Generation, Is.GreaterThan(generation));
            Assert.That(m.ChapterComplete, Is.False);

            var last = At(YourOwnSplit);
            Assert.That(last.IsLastChapter, Is.True);
            Assert.That(last.AllChaptersComplete, Is.False);
            last.SetFence(3, false); last.Check();
            Assert.That(last.AllChaptersComplete, Is.True);
            Assert.That(last.NextChapter(), Is.False, "no chapter after the last");
            Assert.That(last.ChapterIndex, Is.EqualTo(YourOwnSplit));
        }

        [Test] public void StartChapterRejectsOutOfRange()
        {
            var m = new GardenModel();
            Assert.Throws<System.ArgumentOutOfRangeException>(() => m.StartChapter(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => m.StartChapter(5));
        }

        [Test] public void WholeLessonPlaysThroughInOrder()
        {
            var m = new GardenModel();
            m.Plant("strip-1", false, 0); m.Plant("strip-2", false, 1); m.Plant("strip-3", false, 2);
            Assert.That(m.Check(), Is.True); Assert.That(m.NextChapter(), Is.True);
            m.Plant("strip-2", false, 0); m.Plant("strip-3", false, 1); m.Plant("strip-4", false, 2); m.Plant("strip-5", false, 3);
            Assert.That(m.Check(), Is.True); Assert.That(m.NextChapter(), Is.True);
            Assert.That(m.TurnBed(false), Is.True);
            Assert.That(m.Check(), Is.True); Assert.That(m.NextChapter(), Is.True);
            Assert.That(m.SetFence(5, false), Is.True);
            Assert.That(m.Check(), Is.True); Assert.That(m.NextChapter(), Is.True);
            Assert.That(m.SetFence(2, false), Is.True);
            Assert.That(m.Check(), Is.True);
            Assert.That(m.Expression, Is.EqualTo("8 × 7 = 8 × 2 + 8 × 5 = 56"));
            Assert.That(m.AllChaptersComplete, Is.True);
        }
    }
}
