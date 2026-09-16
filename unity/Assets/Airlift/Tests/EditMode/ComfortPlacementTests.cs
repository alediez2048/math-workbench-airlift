using Airlift.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    public class ComfortPlacementTests
    {
        [Test] public void RecenterWhileHeldRejected()
        {
            var root = new GameObject("test station");
            try
            {
                var placement = root.AddComponent<ComfortPlacement>(); placement.stationRoot = root.transform;
                Assert.That(placement.TryPlace(Vector3.one, Quaternion.identity, true), Is.False);
                Assert.That(root.transform.position, Is.EqualTo(Vector3.zero));
            }
            finally { Object.DestroyImmediate(root); }
        }
        [Test] public void AdjustmentMovesWholeStation()
        {
            var root = new GameObject("test station"); var child = new GameObject("cargo");
            child.transform.SetParent(root.transform); child.transform.localPosition = new Vector3(0.2f,0,0.1f);
            try
            {
                var placement = root.AddComponent<ComfortPlacement>(); placement.stationRoot = root.transform;
                var local = child.transform.localPosition;
                Assert.That(placement.TryPlace(Vector3.one, Quaternion.Euler(0,30,0), false), Is.True);
                Assert.That(child.transform.localPosition, Is.EqualTo(local));
                Assert.That(child.transform.position, Is.EqualTo(root.transform.TransformPoint(local)));
            }
            finally { Object.DestroyImmediate(root); }
        }
        [Test] public void InvalidPositionRejected()
        {
            var root = new GameObject("test station");
            try
            {
                var placement = root.AddComponent<ComfortPlacement>(); placement.stationRoot = root.transform;
                Assert.That(placement.TryPlace(new Vector3(float.NaN,1,0), Quaternion.identity, false), Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
