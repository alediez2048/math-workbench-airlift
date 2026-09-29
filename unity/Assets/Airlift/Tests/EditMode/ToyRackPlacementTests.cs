using Airlift.Lounge;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    /// Owner 2026-09-29: "the rack needs to be set against a wall with no window". The lounge is a 12-segment ring with
    /// glazed bays at segments 2, 3, 9 and 10; the rack snaps to the nearest solid segment on the learner's left of the board.
    public class ToyRackPlacementTests
    {
        // Owner 2026-09-29: "the rack is overlapping the window". A 2.3 m rack is wider than one 1.75 m segment, so it
        // only fits a segment whose two neighbours are solid too: segment 0 (between 11 and 1) and segments 5, 6, 7.
        [Test] public void SnapsToTheNearestSegmentWithSolidNeighbours()
        {
            Assert.That(ToyRackPlacement.SolidWallAngle(10f), Is.EqualTo(0f).Within(0.01f), "segment 0 and its neighbours are solid");
            Assert.That(ToyRackPlacement.SolidWallAngle(40f), Is.EqualTo(0f).Within(0.01f), "segment 1 borders the window at 2");
            Assert.That(ToyRackPlacement.SolidWallAngle(125f), Is.EqualTo(150f).Within(0.01f), "segment 4 borders the window at 3; 5 is clear");
            Assert.That(ToyRackPlacement.SolidWallAngle(200f), Is.EqualTo(210f).Within(0.01f));
            Assert.That(ToyRackPlacement.SolidWallAngle(250f), Is.EqualTo(210f).Within(0.01f), "segment 8 borders the window at 9");
            Assert.That(ToyRackPlacement.SolidWallAngle(-20f), Is.EqualTo(0f).Within(0.01f), "angles wrap");
        }

        [Test] public void ThePoseHugsTheWallAndFacesTheRoom()
        {
            var pose = ToyRackPlacement.Pose(90f);
            Assert.That(pose.position.x, Is.EqualTo(ToyRackPlacement.WallRadius).Within(0.01f)); Assert.That(pose.position.z, Is.EqualTo(0f).Within(0.01f)); Assert.That(pose.position.y, Is.EqualTo(0f));
            Assert.That(Vector3.Dot(pose.rotation * Vector3.forward, Vector3.right), Is.LessThan(-0.99f), "its back to the wall, its front to the centre");
        }

        [Test] public void PrefersTheWallOnTheLearnersLeftOfTheBoard()
        {
            // The learner faces +z at the board; their left is -x, which is around 270°, a window bay: expect 240°.
            float angle = ToyRackPlacement.PreferredAngle(boardPosition: new Vector3(0f, 1.4f, 1.15f), boardForward: Vector3.forward, besideBoard: 1.75f);
            Assert.That(ToyRackPlacement.SolidWallAngle(angle), Is.EqualTo(0f).Within(0.01f).Or.EqualTo(210f).Within(0.01f));
        }
    }
}
