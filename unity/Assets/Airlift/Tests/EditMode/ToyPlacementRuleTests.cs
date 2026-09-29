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
    }
}
