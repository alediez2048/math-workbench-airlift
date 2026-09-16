using Airlift.Lessons;
using Airlift.Math;
using NUnit.Framework;

namespace Airlift.Tests
{
    public class CargoLessonModelTests
    {
        [Test] public void StartsWithOneWholeLabeledOne()
        {
            var m = new CargoLessonModel();
            Assert.That(m.Stage, Is.EqualTo(CargoLessonStage.Whole));
            Assert.That(m.Piece("whole").Notation.ToString(), Is.EqualTo("1/1"));
            Assert.That(m.Piece("whole").Cells, Is.EqualTo(PlacementState.CellsPerWhole));
        }
        [Test] public void WholeSubmitRequiresDocking()
        {
            var m = new CargoLessonModel();
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.Dock("whole", held: true), Is.False);
            Assert.That(m.Dock("whole", held: false), Is.True);
            Assert.That(m.RulerQuantity, Is.EqualTo(new FractionValue(1, 1)));
            Assert.That(m.Submit(), Is.True);
        }
        [Test] public void SplitConservesTheWholeExactlyOnce()
        {
            var m = new CargoLessonModel();
            int generation = m.Generation;
            Assert.That(m.Split(held: true, generation), Is.False);
            Assert.That(m.Split(held: false, generation), Is.True);
            Assert.That(m.Stage, Is.EqualTo(CargoLessonStage.Halves));
            Assert.That(m.Piece("whole"), Is.Null);
            Assert.That(m.Piece("half-1").Cells + m.Piece("half-2").Cells, Is.EqualTo(PlacementState.CellsPerWhole));
            Assert.That(m.Piece("half-1").Notation.ToString(), Is.EqualTo("1/2"));
            Assert.That(m.Split(held: false, generation), Is.False, "stale callback must not split again");
            Assert.That(m.Split(held: false, m.Generation), Is.False, "halves cannot split in this chapter");
        }
        [Test] public void OneHalfIsFeedbackNotFailure()
        {
            var m = new CargoLessonModel(); m.Split(false, m.Generation);
            Assert.That(m.Dock("half-1", false), Is.True);
            Assert.That(m.RulerQuantity, Is.EqualTo(new FractionValue(1, 2)));
            Assert.That(m.Submit(), Is.False);
            Assert.That(m.Stage, Is.EqualTo(CargoLessonStage.Halves));
            Assert.That(m.Undock("half-1"), Is.True);
            Assert.That(m.RulerCount, Is.EqualTo(0));
        }
        [Test] public void TwoHalvesRebuildTheWhole()
        {
            var m = new CargoLessonModel(); m.Split(false, m.Generation);
            Assert.That(m.Dock("half-2", false), Is.True);
            Assert.That(m.Dock("half-1", false), Is.True);
            Assert.That(m.StartCell("half-2"), Is.EqualTo(0));
            Assert.That(m.StartCell("half-1"), Is.EqualTo(4));
            Assert.That(m.Dock("half-1", false), Is.False, "no duplicate docking");
            Assert.That(m.Submit(), Is.True);
            Assert.That(m.Stage, Is.EqualTo(CargoLessonStage.Rebuilt));
        }
        [Test] public void PiecesFromAnotherWholeCannotMix()
        {
            var m = new CargoLessonModel();
            var foreign = new PieceState("x", "strap-B", 2);
            var ruler = new PlacementState(CargoLessonModel.WholeId);
            Assert.That(ruler.TryInsert(foreign, 0), Is.False);
        }
    }
}
