using Airlift.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    public class TableAdjustRulesTests
    {
        [Test] public void LevelKeepsYawAndRemovesPitchAndRoll()
        {
            var levelled = TableAdjustRules.Level(Quaternion.Euler(20f, 135f, -10f)).eulerAngles;
            Assert.That(levelled.y, Is.EqualTo(135f).Within(0.5f));
            Assert.That(Mathf.DeltaAngle(levelled.x, 0f), Is.EqualTo(0f).Within(0.01f));
            Assert.That(Mathf.DeltaAngle(levelled.z, 0f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test] public void LevelHandlesTableFacingStraightDown()
        {
            var r = TableAdjustRules.Level(Quaternion.Euler(90f, 40f, 0f));
            Assert.That(Vector3.Dot(r * Vector3.up, Vector3.up), Is.EqualTo(1f).Within(1e-4f));
        }

        [Test] public void ScaleIsClampedAndNonFiniteFallsBackToOne()
        {
            Assert.That(TableAdjustRules.ClampScale(0.1f), Is.EqualTo(TableAdjustRules.MinScale));
            Assert.That(TableAdjustRules.ClampScale(5f), Is.EqualTo(TableAdjustRules.MaxScale));
            Assert.That(TableAdjustRules.ClampScale(1.3f), Is.EqualTo(1.3f));
            Assert.That(TableAdjustRules.ClampScale(float.NaN), Is.EqualTo(1f));
            Assert.That(TableAdjustRules.ClampScale(0f), Is.EqualTo(1f));
        }

        [Test] public void SettleClampsHeightAndLevels()
        {
            var settled = TableAdjustRules.Settle(new Pose(new Vector3(1f, 0.1f, 2f), Quaternion.Euler(30f, 90f, 5f)), 0.35f, 1.4f);
            Assert.That(settled.position, Is.EqualTo(new Vector3(1f, 0.35f, 2f)));
            Assert.That(settled.rotation.eulerAngles.y, Is.EqualTo(90f).Within(0.5f));
            Assert.That(TableAdjustRules.Settle(new Pose(new Vector3(0, 3f, 0), Quaternion.identity), 0.35f, 1.4f).position.y, Is.EqualTo(1.4f));
            Assert.That(TableAdjustRules.Settle(new Pose(new Vector3(float.NaN, 1f, 0), Quaternion.identity), 0.35f, 1.4f).position.y, Is.EqualTo(0.35f));
        }

        [Test] public void TableCannotBeAdjustedWhileAPieceIsHeld()
        {
            Assert.That(TableAdjustRules.CanAdjust(anyPieceHeld: true), Is.False);
            Assert.That(TableAdjustRules.CanAdjust(anyPieceHeld: false), Is.True);
        }
        [Test] public void CarryKeepsGrabPointInHandAndFacesPlayer()
        {
            var head = new Vector3(0f, 1.6f, 0f); var hand = new Vector3(0.8f, 1.0f, 0.5f);
            var grabLocal = new Vector3(0.03f, 0f, -0.412f);
            var pose = TableAdjustRules.CarryPose(hand, head, grabLocal, 1.5f, Quaternion.identity);
            Vector3 handleWorld = pose.position + pose.rotation * (grabLocal * 1.5f);
            Assert.That(Vector3.Distance(handleWorld, hand), Is.LessThan(1e-4f), "grabbed point stays in the hand");
            Vector3 away = Vector3.ProjectOnPlane(hand - head, Vector3.up).normalized;
            Assert.That(Vector3.Dot(pose.rotation * Vector3.forward, away), Is.EqualTo(1f).Within(1e-4f), "front edge faces the player");
            Assert.That(Vector3.Dot(pose.rotation * Vector3.up, Vector3.up), Is.EqualTo(1f).Within(1e-4f), "level");
        }

        [Test] public void CarryDirectlyAboveHeadKeepsPreviousHeading()
        {
            var previous = Quaternion.Euler(0f, 70f, 0f);
            var pose = TableAdjustRules.CarryPose(new Vector3(0f, 1.0f, 0f), new Vector3(0f, 1.6f, 0f), Vector3.zero, 1f, Quaternion.Euler(10f, 70f, 0f));
            Assert.That(Quaternion.Angle(pose.rotation, previous), Is.LessThan(0.01f));
        }
    }
}
