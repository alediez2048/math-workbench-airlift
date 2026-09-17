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

        [Test] public void BedCellsMatchContractAndTarget()
        {
            var all = CargoChapter.All;
            Assert.That(all[BigTruck].BedCells, Is.EqualTo(new[] { 8 }));
            Assert.That(all[TwoPickups].BedCells, Is.EqualTo(new[] { 4, 4 }));
            Assert.That(all[FourVans].BedCells, Is.EqualTo(new[] { 2, 2, 2, 2 }));
            Assert.That(all[SameShare].BedCells, Is.EqualTo(new[] { 4 }));
            Assert.That(all[TopItUp].BedCells, Is.EqualTo(new[] { 8 }));
            foreach (var c in all)
            {
                Assert.That(c.BedCells.Length, Is.EqualTo(c.VehicleCount), c.Id);
                int sum = 0; foreach (int b in c.BedCells) sum += b;
                Assert.That(sum, Is.LessThanOrEqualTo(PlacementState.CellsPerWhole), c.Id);
                Assert.That(new FractionValue(sum, PlacementState.CellsPerWhole), Is.EqualTo(c.Target), c.Id + ": full beds are the target");
            }
        }

        [Test] public void ChapterCopyIsShortAsciiAndDescribesTrucksAtTheDock()
        {
            foreach (var c in CargoChapter.All)
            {
                foreach (var text in new[] { c.Title, c.Story, c.Task, c.Accepted, c.Expression })
                {
                    Assert.That(string.IsNullOrWhiteSpace(text), Is.False, c.Id);
                    string lower = text.ToLowerInvariant();
                    foreach (var banned in new[] { "strap", "floor", "aircraft", "airplane", "plane", "jet" })
                        Assert.That(lower, Does.Not.Contain(banned), c.Id + ": " + text);
                    foreach (char ch in text) Assert.That(ch < 128, Is.True, c.Id + ": non-ASCII in " + text);
                }
                Assert.That(c.Task.Split('.').Length, Is.LessThanOrEqualTo(2), c.Id + " task is one sentence");
            }
            Assert.That(CargoChapter.All[FourVans].Story, Is.EqualTo("Four delivery vans backed up to the dock. Each van fits one quarter of a container."));
            Assert.That(CargoChapter.All[BigTruck].Story, Does.Contain("cranes"));
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

        // ---- beds ----

        [Test] public void BedGeometryPerChapter()
        {
            var vans = At(FourVans);
            Assert.That(vans.BedCount, Is.EqualTo(4));
            for (int b = 0; b < 4; b++)
            {
                Assert.That(vans.BedStartCell(b), Is.EqualTo(2 * b));
                Assert.That(vans.BedCapacity(b), Is.EqualTo(2));
                Assert.That(vans.BedFill(b), Is.EqualTo(0));
            }
            Assert.That(vans.BedAtCell(0), Is.EqualTo(0));
            Assert.That(vans.BedAtCell(3), Is.EqualTo(1));
            Assert.That(vans.BedAtCell(7), Is.EqualTo(3));
            Assert.That(vans.BedAtCell(8), Is.EqualTo(-1));
            Assert.That(vans.BedAtCell(-1), Is.EqualTo(-1));
            Assert.That(vans.BedStartCell(4), Is.EqualTo(-1));

            var pickups = At(TwoPickups);
            Assert.That(pickups.BedCount, Is.EqualTo(2));
            Assert.That(pickups.BedStartCell(1), Is.EqualTo(4));
            Assert.That(pickups.BedAtCell(4), Is.EqualTo(1));

            var one = At(SameShare);
            Assert.That(one.BedCount, Is.EqualTo(1));
            Assert.That(one.BedCapacity(0), Is.EqualTo(4));
            Assert.That(one.BedAtCell(3), Is.EqualTo(0));
            Assert.That(one.BedAtCell(4), Is.EqualTo(-1), "cells 4-8 are not needed in chapter 4");
        }

        [Test] public void DockIntoAChosenBedPacksFromItsStart()
        {
            var m = At(FourVans);
            m.Split(false, m.Generation);
            Assert.That(m.Dock("quarter-1", false, 3), Is.True);
            Assert.That(m.BedOf("quarter-1"), Is.EqualTo(3));
            Assert.That(m.StartCell("quarter-1"), Is.EqualTo(6), "StartCell is global");
            Assert.That(m.Dock("quarter-2", false, 0), Is.True);
            Assert.That(m.DockedIndex("quarter-2"), Is.EqualTo(0), "docked index runs left to right across beds");
            Assert.That(m.DockedIndex("quarter-1"), Is.EqualTo(1));
            Assert.That(m.BedFill(3), Is.EqualTo(2));
            Assert.That(m.Dock("quarter-3", false, 3), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("This van is already full."));
            Assert.That(m.Dock("quarter-3", false, 4), Is.False, "bad bed");
            Assert.That(m.Dock("quarter-3", false, -1), Is.False, "outside the beds");
            Assert.That(m.Dock("quarter-3", true, 1), Is.False, "held");
            Assert.That(m.Dock("quarter-1", false, 1), Is.False, "already docked");
            Assert.That(m.Dock("nope", false, 1), Is.False, "unknown");
            Assert.That(m.Dock("quarter-3", false), Is.True, "first bed with room");
            Assert.That(m.BedOf("quarter-3"), Is.EqualTo(1));
        }

        [Test] public void UndockShiftsTheRestOfThatBed()
        {
            var m = At(SameShare);
            m.Dock("quarter-1", false, 0); m.Dock("quarter-2", false, 0);
            Assert.That(m.StartCell("quarter-2"), Is.EqualTo(2));
            Assert.That(m.Undock("quarter-1"), Is.True);
            Assert.That(m.StartCell("quarter-2"), Is.EqualTo(0));
            Assert.That(m.BedFill(0), Is.EqualTo(2));
            Assert.That(m.BedOf("quarter-1"), Is.EqualTo(-1));
            Assert.That(m.Undock("quarter-1"), Is.False, "not docked");
        }

        [Test] public void CrateLongerThanTheBedIsRefusedWithASizeMessage()
        {
            var vans = At(FourVans);
            Assert.That(vans.Dock("half-1", false, 0), Is.False);
            Assert.That(vans.LastFeedback, Is.EqualTo("That crate is too long for this van. Split it first."));
            Assert.That(vans.IsDocked("half-1"), Is.False);
            Assert.That(vans.Dock("half-2", false), Is.False, "no van bed fits a half crate");
            Assert.That(vans.LastFeedback, Is.EqualTo("That crate is too long for this van. Split it first."));

            var pickups = At(TwoPickups);
            Assert.That(pickups.Dock("whole", false, 1), Is.False);
            Assert.That(pickups.LastFeedback, Is.EqualTo("That crate is too long for this pickup. Split it first."));
            Assert.That(pickups.RulerCount, Is.EqualTo(0));
        }

        [Test] public void FullSingleBedSaysSo()
        {
            var m = At(SameShare);
            Assert.That(m.Dock("quarter-1", false), Is.True);
            Assert.That(m.Dock("quarter-2", false), Is.True);
            Assert.That(m.Dock("quarter-3", false), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("This pickup is already full."));

            var top = At(TopItUp);
            Assert.That(top.Dock("quarter-1", false), Is.True);
            Assert.That(top.Dock("quarter-2", false), Is.True);
            Assert.That(top.Dock("quarter-3", false), Is.False, "the truck bed cannot exceed 8 cells");
            Assert.That(top.LastFeedback, Is.EqualTo("This big truck is already full."));
            Assert.That(top.RulerQuantity, Is.EqualTo(new FractionValue(1, 1)));
        }

        [Test] public void FirstFitSkipsFullBeds()
        {
            var m = At(FourVans);
            m.Split(false, m.Generation);
            Assert.That(m.Dock("quarter-1", false, 0), Is.True);
            Assert.That(m.Dock("quarter-2", false, 2), Is.True);
            Assert.That(m.Dock("quarter-3", false), Is.True);
            Assert.That(m.BedOf("quarter-3"), Is.EqualTo(1));
            Assert.That(m.Dock("quarter-4", false), Is.True);
            Assert.That(m.BedOf("quarter-4"), Is.EqualTo(3));
            Assert.That(m.StartCell("quarter-4"), Is.EqualTo(6));
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
            int gen = m.Generation;
            Assert.That(m.Split(true, gen), Is.False, "held refuses");
            Assert.That(m.Split(false, gen - 1), Is.False, "stale refuses");
            Assert.That(m.Split(false, gen), Is.True);
            Assert.That(m.Generation, Is.GreaterThan(gen));
            Assert.That(m.PieceIds, Is.EqualTo(new[] { "quarter-1", "quarter-2", "quarter-3", "quarter-4" }), "half-1 -> q1,q2; half-2 -> q3,q4");
            Assert.That(m.RulerCount, Is.EqualTo(0));
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

        [Test] public void VehicleNamesInFeedback()
        {
            Assert.That(CargoLessonModel.VehicleName(CargoChapter.All[FourVans], 2), Is.EqualTo("Van 3"));
            Assert.That(CargoLessonModel.VehicleName(CargoChapter.All[TwoPickups], 1), Is.EqualTo("Pickup 2"));
            Assert.That(CargoLessonModel.VehicleName(CargoChapter.All[SameShare], 0), Is.EqualTo("The pickup"));
            Assert.That(CargoLessonModel.VehicleName(CargoChapter.All[TopItUp], 0), Is.EqualTo("The big truck"));
        }

        // ---- dock / undock basics ----

        [Test] public void DockRefusesUnknownHeldAndDuplicate()
        {
            var m = At(SameShare);
            Assert.That(m.Dock("nope", false), Is.False);
            Assert.That(m.Dock(null, false), Is.False);
            Assert.That(m.Dock("quarter-1", true), Is.False);
            Assert.That(m.Dock("quarter-1", false), Is.True);
            Assert.That(m.Dock("quarter-1", false), Is.False);
            Assert.That(m.Undock("quarter-1"), Is.True);
        }

        [Test] public void TopItUpHalfIsLockedInBedZeroAtCellZero()
        {
            var m = At(TopItUp);
            Assert.That(m.IsLocked("half-1"), Is.True);
            Assert.That(m.IsLocked("quarter-1"), Is.False);
            Assert.That(m.BedOf("half-1"), Is.EqualTo(0));
            Assert.That(m.StartCell("half-1"), Is.EqualTo(0));
            Assert.That(m.BedFill(0), Is.EqualTo(4), "fill includes the locked half");
            Assert.That(m.RulerQuantity, Is.EqualTo(new FractionValue(1, 2)));
            Assert.That(m.Dock("half-1", false), Is.False, "locked refuses dock");
            Assert.That(m.Dock("half-1", false, 0), Is.False, "locked refuses bed dock");
            Assert.That(m.Undock("half-1"), Is.False, "locked refuses undock");
            Assert.That(m.Dock("quarter-3", false, 0), Is.True);
            Assert.That(m.StartCell("quarter-3"), Is.EqualTo(4));
            m.ResetPieces();
            Assert.That(m.IsDocked("half-1"), Is.True, "reset keeps locked piece");
            Assert.That(m.IsDocked("quarter-3"), Is.False);
            Assert.That(m.StartCell("half-1"), Is.EqualTo(0));
        }

        // ---- submit ----

        [Test] public void EmptyFeedback()
        {
            var m = At(FourVans);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo(CargoLessonModel.EmptyFloorFeedback));
            Assert.That(CargoLessonModel.EmptyFloorFeedback.ToLowerInvariant(), Does.Not.Contain("floor"));
        }

        [Test] public void TwoPickupsNamesTheEmptyPickup()
        {
            var m = At(TwoPickups);
            m.Split(false, m.Generation);
            Assert.That(m.Dock("half-1", false, 1), Is.True);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Pickup 1 is still empty."));
            Assert.That(m.IsDocked("half-1"), Is.True, "wrong load stays docked and editable");
        }

        [Test] public void TwoPickupsRefusesTheUnsplitWhole()
        {
            var m = At(TwoPickups);
            Assert.That(m.Dock("whole", false), Is.False, "a whole crate does not fit a pickup bed");
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(CargoLessonModel.Evaluate(CargoChapter.All[TwoPickups], new int[0], new[] { 1 }, out var feedback), Is.False);
            Assert.That(feedback, Is.EqualTo("That fills a whole container, but the pickups need halves. Split the crate first."));
        }

        [Test] public void FourVansAcceptsOnlyWhenEveryVanIsFull()
        {
            var m = At(FourVans);
            m.Split(false, m.Generation);
            m.Dock("quarter-1", false, 0); m.Dock("quarter-2", false, 1); m.Dock("quarter-3", false, 3);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Van 3 is still empty."));
            Assert.That(m.ChapterComplete, Is.False);
            m.Dock("quarter-4", false, 2);
            Assert.That(m.Submit(), Is.True);
            Assert.That(m.Expression, Is.EqualTo("1/4 + 1/4 + 1/4 + 1/4 = 1"));
        }

        [Test] public void FourVansRefusesUnsplitHalves()
        {
            Assert.That(CargoLessonModel.Evaluate(CargoChapter.All[FourVans], new int[0], new[] { 2, 2 }, out var feedback), Is.False);
            Assert.That(feedback, Does.StartWith("That fills a whole container, but "));
        }

        [Test] public void SameShareSingleHalfSizeBedTakesExactlyTwoQuarters()
        {
            var m = At(SameShare);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo(CargoLessonModel.EmptyFloorFeedback));
            m.Dock("quarter-4", false);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("The pickup has room for 1/4 of a container more."));
            m.Dock("quarter-2", false);
            Assert.That(m.Dock("quarter-3", false), Is.False, "a third quarter does not fit the half-size bed");
            Assert.That(m.Submit(), Is.True);
            Assert.That(m.RulerQuantity, Is.EqualTo(new FractionValue(1, 2)));
            Assert.That(m.LastFeedback, Is.EqualTo(CargoChapter.All[SameShare].Accepted + " 2/4 = 1/2"));
        }

        [Test] public void SameShareVerdictsForHalfAndTooMany()
        {
            var chapter = CargoChapter.All[SameShare];
            Assert.That(CargoLessonModel.Evaluate(chapter, new int[0], new[] { 2 }, out var half), Is.False);
            Assert.That(half, Is.EqualTo("That fills 1/2 of a container, but this pickup's boxes must be quarters."));
            Assert.That(CargoLessonModel.Evaluate(chapter, new int[0], new[] { 4, 4, 4 }, out var over), Is.False);
            Assert.That(over, Is.EqualTo("That is more than this pickup takes. It needs 1/2 of a container."));
            Assert.That(CargoLessonModel.Evaluate(chapter, new int[0], new[] { 4, 4 }, out _), Is.True);
        }

        [Test] public void TopItUpCountsTheLockedHalf()
        {
            var m = At(TopItUp);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("The big truck has room for 1/2 of a container more."));
            m.Dock("quarter-1", false);
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("The big truck has room for 1/4 of a container more."));
            m.Dock("quarter-2", false);
            Assert.That(m.Submit(), Is.True);
            Assert.That(m.Expression, Is.EqualTo("1/2 + 1/4 + 1/4 = 1"));
            Assert.That(m.AllChaptersComplete, Is.True);
        }

        [Test] public void EvaluatePacksBedsAndReducesDisplayedFractions()
        {
            Assert.That(CargoLessonModel.Evaluate(CargoChapter.All[BigTruck], new int[0], new[] { 2, 2 }, out var truck), Is.True, "two halves fill the big truck");
            Assert.That(CargoLessonModel.Evaluate(CargoChapter.All[BigTruck], new int[0], new[] { 4, 4, 4 }, out truck), Is.False);
            Assert.That(truck, Is.EqualTo("The big truck has room for 1/4 of a container more."), "6/8 left over as 2/8 shows reduced");
            Assert.That(CargoLessonModel.Evaluate(CargoChapter.All[TopItUp], new[] { 2 }, new int[0], out var top), Is.False);
            Assert.That(top, Is.EqualTo("The big truck has room for 1/2 of a container more."));
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
            Assert.That(m.BedFill(0), Is.EqualTo(0));
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
            Assert.That(m.BedCount, Is.EqualTo(2));
            Assert.That(m.Generation, Is.GreaterThan(gen));
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.Expression, Is.EqualTo(""));
        }

        [Test] public void FullJourneyEndsWithAllChaptersComplete()
        {
            var m = new CargoLessonModel();
            var seen = new List<string>();

            Assert.That(m.Dock("whole", false, 0), Is.True);
            Assert.That(m.Submit(), Is.True); seen.Add(m.Chapter.Id);
            Assert.That(m.NextChapter(), Is.True);

            Assert.That(m.Split(false, m.Generation), Is.True);
            Assert.That(m.Dock("half-1", false, 0), Is.True);
            Assert.That(m.Dock("half-2", false, 1), Is.True);
            Assert.That(m.Submit(), Is.True); seen.Add(m.Chapter.Id);
            Assert.That(m.NextChapter(), Is.True);

            Assert.That(m.Split(false, m.Generation), Is.True);
            string[] quarters = { "quarter-1", "quarter-2", "quarter-3", "quarter-4" };
            for (int b = 0; b < 4; b++) Assert.That(m.Dock(quarters[b], false, b), Is.True);
            Assert.That(m.Submit(), Is.True); seen.Add(m.Chapter.Id);
            Assert.That(m.NextChapter(), Is.True);

            Assert.That(m.Dock("quarter-1", false, 0), Is.True);
            Assert.That(m.Dock("quarter-2", false, 0), Is.True);
            Assert.That(m.Submit(), Is.True); seen.Add(m.Chapter.Id);
            Assert.That(m.AllChaptersComplete, Is.False);
            Assert.That(m.NextChapter(), Is.True);

            Assert.That(m.IsLastChapter, Is.True);
            Assert.That(m.Dock("quarter-3", false, 0), Is.True);
            Assert.That(m.Dock("quarter-4", false, 0), Is.True);
            Assert.That(m.Submit(), Is.True); seen.Add(m.Chapter.Id);
            Assert.That(m.AllChaptersComplete, Is.True);
            Assert.That(m.NextChapter(), Is.False, "no chapter after the last");

            Assert.That(seen, Is.EqualTo(new[] { "big_truck", "two_pickups", "four_vans", "same_share", "top_it_up" }));
        }
    }
}
