using System.Linq;
using Airlift.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    public class RoundedBoxMeshTests
    {
        static readonly Vector3 Strap = new Vector3(0.28f, 0.055f, 0.06f);

        [Test] public void BoundsMatchRequestedSize()
        {
            var mesh = RoundedBoxMesh.Build(Strap, 0.012f);
            Assert.That(mesh.bounds.size.x, Is.EqualTo(Strap.x).Within(1e-5f));
            Assert.That(mesh.bounds.size.y, Is.EqualTo(Strap.y).Within(1e-5f));
            Assert.That(mesh.bounds.size.z, Is.EqualTo(Strap.z).Within(1e-5f));
            Assert.That(mesh.bounds.center.magnitude, Is.LessThan(1e-5f));
        }

        [Test] public void NormalsAreUnitAndFinite()
        {
            var mesh = RoundedBoxMesh.Build(Strap, 0.012f);
            Assert.That(mesh.normals.Length, Is.EqualTo(mesh.vertexCount));
            foreach (var n in mesh.normals)
            {
                Assert.That(float.IsNaN(n.x) || float.IsNaN(n.y) || float.IsNaN(n.z), Is.False);
                Assert.That(n.magnitude, Is.EqualTo(1f).Within(1e-4f));
            }
        }

        [Test] public void TrianglesAreWellFormed()
        {
            var mesh = RoundedBoxMesh.Build(Strap, 0.012f);
            Assert.That(mesh.triangles.Length % 3, Is.Zero);
            Assert.That(mesh.triangles.Length, Is.GreaterThan(0));
            Assert.That(mesh.triangles.All(i => i >= 0 && i < mesh.vertexCount), Is.True);
        }

        [Test] public void TrianglesFaceOutwardLikeUnityCube()
        {
            // Same orientation test applied to Unity's own cube, so the convention is checked, not assumed.
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try { Assert.That(InwardTriangles(cube.GetComponent<MeshFilter>().sharedMesh), Is.Zero, "reference cube"); }
            finally { Object.DestroyImmediate(cube); }
            Assert.That(InwardTriangles(RoundedBoxMesh.Build(Strap, 0.012f)), Is.Zero, "rounded strap");
            Assert.That(InwardTriangles(RoundedBoxMesh.Build(Strap, 0.012f, 4, false, true)), Is.Zero, "cut strap");
        }

        static int InwardTriangles(Mesh mesh)
        {
            var v = mesh.vertices; var t = mesh.triangles; int inward = 0;
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 a = v[t[i]], b = v[t[i + 1]], c = v[t[i + 2]];
                double nx = (double)(b.y - a.y) * (c.z - a.z) - (double)(b.z - a.z) * (c.y - a.y);
                double ny = (double)(b.z - a.z) * (c.x - a.x) - (double)(b.x - a.x) * (c.z - a.z);
                double nz = (double)(b.x - a.x) * (c.y - a.y) - (double)(b.y - a.y) * (c.x - a.x);
                double cx = (a.x + b.x + c.x) / 3.0, cy = (a.y + b.y + c.y) / 3.0, cz = (a.z + b.z + c.z) / 3.0;
                if (nx * nx + ny * ny + nz * nz < 1e-30) continue;
                if (nx * cx + ny * cy + nz * cz <= 0) inward++;
            }
            return inward;
        }

        [Test] public void FlatCutEndKeepsASharpEdge()
        {
            // A clean cut through a rounded strap is a flat face whose outline follows the
            // rounded cross-section, so the top face must reach the cut end without rounding.
            var cut = RoundedBoxMesh.Build(Strap, 0.012f, 4, roundMinX: false, roundMaxX: true);
            var round = RoundedBoxMesh.Build(Strap, 0.012f);
            bool TopReachesMinX(Mesh m) => m.vertices.Any(p =>
                Mathf.Abs(p.x + Strap.x * 0.5f) < 1e-5f && Mathf.Abs(p.y - Strap.y * 0.5f) < 1e-5f);
            Assert.That(TopReachesMinX(cut), Is.True, "cut end keeps a sharp top edge");
            Assert.That(TopReachesMinX(round), Is.False, "rounded end has no sharp top edge");
            Assert.That(cut.vertices.Any(p => Mathf.Abs(p.x - Strap.x * 0.5f) < 1e-5f && Mathf.Abs(p.y - Strap.y * 0.5f) < 1e-5f), Is.False, "other end stays rounded");
            Assert.That(cut.bounds.size.x, Is.EqualTo(Strap.x).Within(1e-5f), "length is exact either way");
        }

        [Test] public void RadiusIsClampedForThinBoxes()
        {
            var thin = new Vector3(0.3f, 0.004f, 0.012f);
            var mesh = RoundedBoxMesh.Build(thin, 0.05f);
            Assert.That(mesh.bounds.size.y, Is.EqualTo(thin.y).Within(1e-5f));
            Assert.That(mesh.vertices.All(p => !float.IsNaN(p.x) && !float.IsNaN(p.y) && !float.IsNaN(p.z)), Is.True);
        }

        [Test] public void ZeroRadiusIsAPlainBox()
        {
            var mesh = RoundedBoxMesh.Build(Strap, 0f);
            Assert.That(mesh.vertexCount, Is.EqualTo(24));
            Assert.That(mesh.triangles.Length, Is.EqualTo(36));
        }
    }
}
