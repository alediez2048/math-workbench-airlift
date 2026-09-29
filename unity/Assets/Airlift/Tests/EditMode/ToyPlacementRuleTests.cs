using Airlift.Lounge;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    /// Owner 2026-09-29: toys stay where they are let go and can be resized with two hands within sane limits.
    public class ToyPlacementRuleTests
    {
        [Test] public void ScaleIsClampedBetweenHalfAndThreeTimesTheBuiltSize()
        {
            const float built = 1.7f;
            Assert.That(ToyPlacementRule.ClampScale(0.1f, built), Is.EqualTo(ToyPlacementRule.MinScale * built).Within(0.001f));
            Assert.That(ToyPlacementRule.ClampScale(2f, built), Is.EqualTo(2f));
            Assert.That(ToyPlacementRule.ClampScale(9f, built), Is.EqualTo(ToyPlacementRule.MaxScale * built).Within(0.001f));
        }

        [Test] public void AToyLetGoBelowTheFloorComesUpToTheFloor()
        {
            var p = ToyPlacementRule.Settle(new Vector3(1f, -0.4f, 1f), toyHalfHeight: 0.05f);
            Assert.That(p.y, Is.EqualTo(0.05f).Within(0.001f));
            Assert.That(ToyPlacementRule.Settle(new Vector3(1f, 0.9f, 1f), 0.05f).y, Is.EqualTo(0.9f), "above the floor it stays where it is");
        }
        [Test] public void AToyPutBackOnItsSlotAtItsBuiltSizeIsHome()
        {
            var home = new Vector3(-0.92f, 1.7f, 0f);
            Assert.That(ToyPlacementRule.IsHome(home + new Vector3(0.03f, 0f, 0.02f), home, 1.7f, 1.7f), Is.True, "a few centimetres off the slot still counts");
            Assert.That(ToyPlacementRule.IsHome(home + new Vector3(0.3f, 0f, 0f), home, 1.7f, 1.7f), Is.False, "30 cm away is a place of its own");
            Assert.That(ToyPlacementRule.IsHome(home, home, 2.5f, 1.7f), Is.False, "resized on the slot: the size is worth remembering");
        }
    }
}
