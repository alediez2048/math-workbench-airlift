using System.Collections.Generic;
using Airlift.Lessons;
using Airlift.Math;
using NUnit.Framework;

namespace Airlift.Tests
{
    public class CargoChapterTests
    {
        const int BigTruck = 0, TwoPickups = 1, FourVans = 2, SameShare = 3, TopItUp = 4;

        static CargoLessonModel At(int index) { var m = new CargoLessonModel(); m.StartChapter(index); return m; }

        // ---- chapter table ----

        [Test] public void ChapterTableMatchesContract()
        {
            var all = CargoChapter.All;
            Assert.That(all.Count, Is.EqualTo(5));
            string[] ids = { "big_truck", "two_pickups", "four_vans", "same_share", "top_it_up" };
            string[] kinds = { "truck", "pickup", "van", "pickup", "truck" };
            int[] vehicles = { 1, 2, 4, 1, 1 };
            int[] splitTo = { 0, 2, 4, 0, 0 };
            int[] requiredDen = { 0, 2, 4, 4, 4 };
            var targets = new[] { new FractionValue(1, 1), new FractionValue(1, 1), new FractionValue(1, 1), new FractionValue(1, 2), new FractionValue(1, 1) };
            string[] expressions = { "1", "1/2 + 1/2 = 1", "1/4 + 1/4 + 1/4 + 1/4 = 1", "2/4 = 1/2", "1/2 + 1/4 + 1/4 = 1" };
            for (int i = 0; i < 5; i++)
            {
                var c = all[i];
                Assert.That(c.Number, Is.EqualTo(i + 1));
                Assert.That(c.Id, Is.EqualTo(ids[i]));
                Assert.That(c.VehicleKind, Is.EqualTo(kinds[i]));
                Assert.That(c.VehicleCount, Is.EqualTo(vehicles[i]));
                Assert.That(c.SplitTo, Is.EqualTo(splitTo[i]));
                Assert.That(c.RequiredDenominator, Is.EqualTo(requiredDen[i]));
                Assert.That(c.Target, Is.EqualTo(targets[i]));
                Assert.That(c.Expression, Is.EqualTo(expressions[i]));
                Assert.That(c.ShowHalfMark, Is.EqualTo(i == SameShare));
            }
            Assert.That(all[BigTruck].StartPieces, Is.EqualTo(new[] { 1 }));
            Assert.That(all[TwoPickups].StartPieces, Is.EqualTo(new[] { 1 }));
            Assert.That(all[FourVans].StartPieces, Is.EqualTo(new[] { 2, 2 }));
            Assert.That(all[SameShare].StartPieces, Is.EqualTo(new[] { 4, 4, 4, 4 }));
            Assert.That(all[TopItUp].StartPieces, Is.EqualTo(new[] { 4, 4, 4, 4 }));
            for (int i = 0; i < 4; i++) Assert.That(all[i].LockedPieces, Is.Empty);
            Assert.That(all[TopItUp].LockedPieces, Is.EqualTo(new[] { 2 }));
        }

        [Test] public void ChapterCopyIsShortAsciiAndUsesCrateLanguage()
        {
            foreach (var c in CargoChapter.All)
            {
                foreach (var text in new[] { c.Title, c.Story, c.Task, c.Accepted, c.Expression })
                {
                    Assert.That(string.IsNullOrWhiteSpace(text), Is.False, c.Id);
                    Assert.That(text.ToLowerInvariant(), Does.Not.Contain("strap"), c.Id);
                    foreach (char ch in text) Assert.That(ch < 128, Is.True, c.Id + ": non-ASCII in " + text);
                }
                Assert.That(c.Task.Split('.').Length, Is.LessThanOrEqualTo(2), c.Id + " task is one sentence");
            }
            Assert.That(CargoChapter.All[FourVans].Story, Is.EqualTo("Four delivery vans pulled in. Each van fits one quarter of a container."));
            Assert.That(CargoChapter.All[TwoPickups].Accepted, Is.EqualTo("Both pickups are loaded."));
        }

        [Test] public void EveryChapterStartsWithFixedPieceIds()
        {
            Assert.That(At(BigTruck).PieceIds, Is.EquivalentTo(new[] { "whole" }));
            Assert.That(At(TwoPickups).PieceIds, Is.EquivalentTo(new[] { "whole" }));
            Assert.That(At(FourVans).PieceIds, Is.EquivalentTo(new[] { "half-1", "half-2" }));
            Assert.That(At(SameShare).PieceIds, Is.EquivalentTo(new[] { "quarter-1", "quarter-2", "quarter-3", "quarter-4" }));
            Assert.That(At(TopItUp).PieceIds, Is.EquivalentTo(new[] { "half-1", "quarter-1", "quarter-2", "quarter-3", "quarter-4" }));
            for (int i = 0; i < 5; i++)
            {
                var m = At(i);
                Assert.That(m.Chapter, Is.SameAs(CargoChapter.All[i]));
                Assert.That(m.ChapterComplete, Is.False);
                Assert.That(m.LastFeedback, Is.EqualTo(""));
            }
        }

        [Test] public void StartChapterOutOfRangeThrows()
        {
            var m = new CargoLessonModel();
            Assert.Throws<System.ArgumentOutOfRangeException>(() => m.StartChapter(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => m.StartChapter(5));
        }

        // ---- split ----

        [Test] public void SplitRefusedWhenChapterDoesNotAllowIt()
        {
            foreach (int i in new[] { BigTruck, SameShare, TopItUp })
            {
                var m = At(i);
                int gen = m.Generation;
                Assert.That(m.CanSplit, Is.False, CargoChapter.All[i].Id);
                Assert.That(m.Split(false, gen), Is.False, CargoChapter.All[i].Id);
                Assert.That(m.Generation, Is.EqualTo(gen));
            }
        }

        [Test] public void FourVansSplitFollowsLineageAndConservesCells()
        {
            var m = At(FourVans);
            Assert.That(m.Dock("half-1", false), Is.True);
            int gen = m.Generation;
            Assert.That(m.Split(true, gen), Is.False, "held refuses");
            Assert.That(m.Split(false, gen - 1), Is.False, "stale refuses");
            Assert.That(m.Split(false, gen), Is.True);
            Assert.That(m.Generation, Is.GreaterThan(gen));
            Assert.That(m.PieceIds, Is.EqualTo(new[] { "quarter-1", "quarter-2", "quarter-3", "quarter-4" }), "half-1 -> q1,q2; half-2 -> q3,q4");
            Assert.That(m.RulerCount, Is.EqualTo(0), "split undocks unlocked pieces");
            int cells = 0;
            foreach (var id in m.PieceIds) { cells += m.Piece(id).Cells; Assert.That(m.Piece(id).Notation.ToString(), Is.EqualTo("1/4")); }
            Assert.That(cells, Is.EqualTo(PlacementState.CellsPerWhole));
            Assert.That(m.LastFeedback, Is.EqualTo("The crates are split into 4 equal chunks. Each chunk is 1/4 of a container."));
            Assert.That(m.CanSplit, Is.False);
            Assert.That(m.Split(false, m.Generation), Is.False, "quarters cannot split in chapter 3");
        }

        [Test] public void TwoPickupsSplitLineageAndFeedback()
        {
            var m = At(TwoPickups);
            Assert.That(m.Split(false, m.Generation), Is.True);
            Assert.That(m.PieceIds, Is.EqualTo(new[] { "half-1", "half-2" }));
            Assert.That(m.LastFeedback, Is.EqualTo("The crate is split into 2 equal chunks. Each chunk is 1/2 of a container."));
        }

        [Test] public void PieceIdNamesAreFixed()
        {
            Assert.That(CargoLessonModel.PieceId(1, 1), Is.EqualTo("whole"));
            Assert.That(CargoLessonModel.PieceId(2, 2), Is.EqualTo("half-2"));
            Assert.That(CargoLessonModel.PieceId(4, 3), Is.EqualTo("quarter-3"));
        }

        // ---- dock / undock ----

        [Test] public void DockRefusesUnknownHeldDuplicateAndOverfull()
        {
            var m = At(SameShare);
            Assert.That(m.Dock("nope", false), Is.False);
            Assert.That(m.Dock(null, false), Is.False);
            Assert.That(m.Dock("quarter-1", true), Is.False);
            Assert.That(m.Dock("quarter-1", false), Is.True);
            Assert.That(m.Dock("quarter-1", false), Is.False);
            Assert.That(m.Undock("quarter-1"), Is.True);
            Assert.That(m.Undock("quarter-1"), Is.False, "not docked");

            var top = At(TopItUp);
            Assert.That(top.Dock("quarter-1", false), Is.True);
            Assert.That(top.Dock("quarter-2", false), Is.True);
            Assert.That(top.Dock("quarter-3", false), Is.False, "floor cannot exceed 8 cells");
            Assert.That(top.RulerQuantity, Is.EqualTo(new FractionValue(1, 1)));
        }

        [Test] public void TopItUpHalfIsLockedAtCellZero()
        {
            var m = At(TopItUp);
            Assert.That(m.IsLocked("half-1"), Is.True);
            Assert.That(m.IsLocked("quarter-1"), Is.False);
            Assert.That(m.IsDocked("half-1"), Is.True);
            Assert.That(m.StartCell("half-1"), Is.EqualTo(0));
            Assert.That(m.RulerQuantity, Is.EqualTo(new FractionValue(1, 2)));
            Assert.That(m.Dock("half-1", false), Is.False, "locked refuses dock");
            Assert.That(m.Undock("half-1"), Is.False, "locked refuses undock");
            Assert.That(m.Dock("quarter-3", false), Is.True);
            Assert.That(m.StartCell("quarter-3"), Is.EqualTo(4));
            m.ResetPieces();
            Assert.That(m.IsDocked("half-1"), Is.True, "reset keeps locked piece");
            Assert.That(m.IsDocked("quarter-3"), Is.False);
            Assert.That(m.StartCell("half-1"), Is.EqualTo(0));
        }

        // ---- submit ----

        [Test] public void EmptyFloorFeedback()
        {
            var m = At(FourVans);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("The container floor is empty. Load crates from 0, then check."));
        }

        [Test] public void TwoPickupsRefusesTheUnsplitWhole()
        {
            var m = At(TwoPickups);
            Assert.That(m.Dock("whole", false), Is.True);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("That fills the container, but the pickups need halves. Split the crate first."));
            Assert.That(m.IsDocked("whole"), Is.True, "wrong load stays docked and editable");
            Assert.That(m.Undock("whole"), Is.True);
        }

        [Test] public void FourVansUnderTargetNamesTheRemainder()
        {
            var m = At(FourVans);
            m.Split(false, m.Generation);
            m.Dock("quarter-1", false); m.Dock("quarter-2", false); m.Dock("quarter-3", false);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("That fills 3/4 of the container. 1/4 more fits."));
            m.Dock("quarter-4", false);
            Assert.That(m.Submit(), Is.True);
            Assert.That(m.Expression, Is.EqualTo("1/4 + 1/4 + 1/4 + 1/4 = 1"));
        }

        [Test] public void FourVansRefusesUnsplitHalves()
        {
            var m = At(FourVans);
            m.Dock("half-1", false); m.Dock("half-2", false);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Does.StartWith("That fills the container, but "));
            Assert.That(m.RulerCount, Is.EqualTo(2));
        }

        [Test] public void SameShareAcceptsExactlyTwoQuarters()
        {
            var m = At(SameShare);
            m.Dock("quarter-4", false);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("That fills 1/4 of the container. 1/4 more fits."));
            m.Dock("quarter-2", false); m.Dock("quarter-3", false);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("That is more than this pickup takes. It needs 1/2 of a container."));
            Assert.That(m.RulerCount, Is.EqualTo(3), "over-full load stays editable");
            m.Undock("quarter-3");
            Assert.That(m.Submit(), Is.True);
            Assert.That(m.LastFeedback, Is.EqualTo(CargoChapter.All[SameShare].Accepted + " 2/4 = 1/2"));
        }

        [Test] public void SameShareRefusesAHalfPiece()
        {
            var chapter = CargoChapter.All[SameShare];
            Assert.That(CargoLessonModel.Evaluate(chapter, new int[0], new[] { 2 }, out var feedback), Is.False);
            Assert.That(feedback, Is.EqualTo("That fills 1/2 of the container, but this pickup's boxes must be quarters."));
            Assert.That(CargoLessonModel.Evaluate(chapter, new int[0], new[] { 4, 4 }, out _), Is.True);
        }

        [Test] public void TopItUpCountsTheLockedHalf()
        {
            var m = At(TopItUp);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("That fills 1/2 of the container. 1/2 more fits."));
            m.Dock("quarter-1", false);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("That fills 3/4 of the container. 1/4 more fits."));
            m.Dock("quarter-2", false);
            Assert.That(m.Submit(), Is.True);
            Assert.That(m.Expression, Is.EqualTo("1/2 + 1/4 + 1/4 = 1"));
            Assert.That(m.AllChaptersComplete, Is.True);
        }

        [Test] public void EvaluateReducesDisplayedFractions()
        {
            var chapter = CargoChapter.All[FourVans];
            CargoLessonModel.Evaluate(chapter, new int[0], new[] { 4, 4 }, out var feedback);
            Assert.That(feedback, Is.EqualTo("That fills 1/2 of the container. 1/2 more fits."));
        }

        // ---- reset / restart / next ----

        [Test] public void ResetUndocksButKeepsSplitLevel()
        {
            var m = At(TwoPickups);
            m.Split(false, m.Generation);
            m.Dock("half-1", false);
            int gen = m.Generation;
            m.ResetPieces();
            Assert.That(m.Generation, Is.GreaterThan(gen));
            Assert.That(m.RulerCount, Is.EqualTo(0));
            Assert.That(m.PieceIds, Is.EquivalentTo(new[] { "half-1", "half-2" }));
        }

        [Test] public void RestartRestoresStartPieces()
        {
            var m = At(FourVans);
            m.Split(false, m.Generation);
            m.Dock("quarter-1", false);
            int gen = m.Generation;
            m.RestartChapter();
            Assert.That(m.Generation, Is.GreaterThan(gen));
            Assert.That(m.ChapterIndex, Is.EqualTo(FourVans));
            Assert.That(m.PieceIds, Is.EquivalentTo(new[] { "half-1", "half-2" }));
            Assert.That(m.RulerCount, Is.EqualTo(0));
            Assert.That(m.CanSplit, Is.True);
        }

        [Test] public void NextChapterOnlyAfterAcceptance()
        {
            var m = new CargoLessonModel();
            Assert.That(m.NextChapter(), Is.False);
            Assert.That(m.ChapterIndex, Is.EqualTo(BigTruck));
            m.Dock("whole", false);
            m.Submit();
            int gen = m.Generation;
            Assert.That(m.NextChapter(), Is.True);
            Assert.That(m.ChapterIndex, Is.EqualTo(TwoPickups));
            Assert.That(m.Generation, Is.GreaterThan(gen));
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.Expression, Is.EqualTo(""));
        }

        [Test] public void FullJourneyEndsWithAllChaptersComplete()
        {
            var m = new CargoLessonModel();
            var seen = new List<string>();

            m.Dock("whole", false);
            Assert.That(m.Submit(), Is.True); seen.Add(m.Chapter.Id);
            Assert.That(m.NextChapter(), Is.True);

            Assert.That(m.Split(false, m.Generation), Is.True);
            m.Dock("half-1", false); m.Dock("half-2", false);
            Assert.That(m.Submit(), Is.True); seen.Add(m.Chapter.Id);
            Assert.That(m.NextChapter(), Is.True);

            Assert.That(m.Split(false, m.Generation), Is.True);
            foreach (var id in new[] { "quarter-1", "quarter-2", "quarter-3", "quarter-4" }) m.Dock(id, false);
            Assert.That(m.Submit(), Is.True); seen.Add(m.Chapter.Id);
            Assert.That(m.NextChapter(), Is.True);

            m.Dock("quarter-1", false); m.Dock("quarter-2", false);
            Assert.That(m.Submit(), Is.True); seen.Add(m.Chapter.Id);
            Assert.That(m.AllChaptersComplete, Is.False);
            Assert.That(m.NextChapter(), Is.True);

            Assert.That(m.IsLastChapter, Is.True);
            m.Dock("quarter-3", false); m.Dock("quarter-4", false);
            Assert.That(m.Submit(), Is.True); seen.Add(m.Chapter.Id);
            Assert.That(m.AllChaptersComplete, Is.True);
            Assert.That(m.NextChapter(), Is.False, "no chapter after the last");

            Assert.That(seen, Is.EqualTo(new[] { "big_truck", "two_pickups", "four_vans", "same_share", "top_it_up" }));
        }
    }
}
