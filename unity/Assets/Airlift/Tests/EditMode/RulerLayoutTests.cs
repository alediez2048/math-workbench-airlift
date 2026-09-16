using Airlift.Lessons;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    public class RulerLayoutTests
    {
        static readonly Vector3 Ruler = new Vector3(0.13f, 0.08f, 0.12f);

        [Test] public void WholeSnapsToCoverZeroToOne()
        {
            var p = RulerLayout.SnapPosition(Ruler, 0, 8, 0.08f);
            Assert.That(p.x, Is.EqualTo(Ruler.x).Within(1e-5f));
            Assert.That(RulerLayout.PieceLength(8), Is.EqualTo(RulerLayout.WholeLength).Within(1e-6f));
        }
        [Test] public void HalvesShareTheWholeLengthAndEndAtTheSamePoint()
        {
            var first = RulerLayout.SnapPosition(Ruler, 0, 4, 0.08f);
            var second = RulerLayout.SnapPosition(Ruler, 4, 4, 0.08f);
            float half = RulerLayout.PieceLength(4);
            Assert.That(half * 2, Is.EqualTo(RulerLayout.WholeLength).Within(1e-6f));
            Assert.That(first.x + half * 0.5f, Is.EqualTo(second.x - half * 0.5f).Within(1e-5f), "second half starts where the first ends");
            Assert.That(second.x + half * 0.5f, Is.EqualTo(RulerLayout.Origin(Ruler).x + RulerLayout.WholeLength).Within(1e-5f), "reaches the 1 mark");
        }
        [Test] public void ReleaseNearRulerDocksAndFarReleaseDoesNot()
        {
            Assert.That(RulerLayout.InDockZone(Ruler, Ruler + new Vector3(0.05f, 0.04f, 0.03f)), Is.True);
            Assert.That(RulerLayout.InDockZone(Ruler, Ruler + new Vector3(0, 0, 0.2f)), Is.False);
            Assert.That(RulerLayout.InDockZone(Ruler, Ruler + new Vector3(-0.4f, 0, 0)), Is.False);
        }
    }
}
