using Airlift.Lessons;
using Airlift.Math;
using NUnit.Framework;

namespace Airlift.Tests
{
    public class CargoLessonModelTests
    {
        const int BigTruck = 0, TwoPickups = 1;

        [Test] public void StartsWithOneWholeLabeledOne()
        {
            var m = new CargoLessonModel();
            Assert.That(m.ChapterIndex, Is.EqualTo(BigTruck));
            Assert.That(m.Chapter.Id, Is.EqualTo("big_truck"));
            Assert.That(m.PieceIds, Is.EquivalentTo(new[] { "whole" }));
            Assert.That(m.Piece("whole").Notation.ToString(), Is.EqualTo("1/1"));
            Assert.That(m.Piece("whole").Cells, Is.EqualTo(PlacementState.CellsPerWhole));
            Assert.That(m.BedCount, Is.EqualTo(1));
            Assert.That(m.BedCapacity(0), Is.EqualTo(PlacementState.CellsPerWhole));
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.Expression, Is.EqualTo(""));
        }

        [Test] public void WholeSubmitRequiresDocking()
        {
            var m = new CargoLessonModel();
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo(CargoLessonModel.EmptyFloorFeedback));
            Assert.That(m.Dock("whole", held: true), Is.False);
            Assert.That(m.Dock("whole", held: false), Is.True);
            Assert.That(m.BedOf("whole"), Is.EqualTo(0));
            Assert.That(m.RulerQuantity, Is.EqualTo(new FractionValue(1, 1)));
            Assert.That(m.Submit(), Is.True);
            Assert.That(m.ChapterComplete, Is.True);
            Assert.That(m.Expression, Is.EqualTo("1"));
            Assert.That(m.LastFeedback, Is.EqualTo(m.Chapter.Accepted + " 1"));
        }

        [Test] public void SplitConservesTheWholeExactlyOnce()
        {
            var m = new CargoLessonModel();
            m.StartChapter(TwoPickups);
            int generation = m.Generation;
            Assert.That(m.CanSplit, Is.True);
            Assert.That(m.Split(held: true, generation), Is.False);
            Assert.That(m.Split(held: false, generation), Is.True);
            Assert.That(m.Piece("whole"), Is.Null);
            Assert.That(m.PieceIds, Is.EquivalentTo(new[] { "half-1", "half-2" }));
            Assert.That(m.Piece("half-1").Cells + m.Piece("half-2").Cells, Is.EqualTo(PlacementState.CellsPerWhole));
            Assert.That(m.Piece("half-1").Notation.ToString(), Is.EqualTo("1/2"));
            Assert.That(m.Split(held: false, generation), Is.False, "stale callback must not split again");
            Assert.That(m.CanSplit, Is.False);
            Assert.That(m.Split(held: false, m.Generation), Is.False, "halves cannot split in this chapter");
        }

        [Test] public void OneHalfIsFeedbackNotFailure()
        {
            var m = new CargoLessonModel(); m.StartChapter(TwoPickups); m.Split(false, m.Generation);
            Assert.That(m.Dock("half-1", false), Is.True);
            Assert.That(m.BedOf("half-1"), Is.EqualTo(0));
            Assert.That(m.RulerQuantity, Is.EqualTo(new FractionValue(1, 2)));
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Pickup 2 is still empty."));
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.IsDocked("half-1"), Is.True, "a wrong load stays in the bed");
            Assert.That(m.Undock("half-1"), Is.True);
            Assert.That(m.RulerCount, Is.EqualTo(0));
        }

        [Test] public void TwoHalvesRebuildTheWhole()
        {
            var m = new CargoLessonModel(); m.StartChapter(TwoPickups); m.Split(false, m.Generation);
            Assert.That(m.Dock("half-2", false), Is.True);
            Assert.That(m.Dock("half-1", false), Is.True);
            Assert.That(m.StartCell("half-2"), Is.EqualTo(0));
            Assert.That(m.StartCell("half-1"), Is.EqualTo(4));
            Assert.That(m.BedOf("half-1"), Is.EqualTo(1));
            Assert.That(m.Dock("half-1", false), Is.False, "no duplicate docking");
            Assert.That(m.Submit(), Is.True);
            Assert.That(m.ChapterComplete, Is.True);
            Assert.That(m.Expression, Is.EqualTo("1/2 + 1/2 = 1"));
            Assert.That(m.LastFeedback, Is.EqualTo(m.Chapter.Accepted + " 1/2 + 1/2 = 1"));
        }

        [Test] public void PiecesFromAnotherWholeCannotMix()
        {
            var foreign = new PieceState("x", "strap-B", 2);
            var ruler = new PlacementState(CargoLessonModel.WholeId);
            Assert.That(ruler.TryInsert(foreign, 0), Is.False);
        }
    }
}
