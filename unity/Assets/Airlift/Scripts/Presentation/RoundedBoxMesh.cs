using System.Collections.Generic;
using UnityEngine;

namespace Airlift.Presentation
{
    /// Procedural rounded box: a toy-brick silhouette with exact outer dimensions.
    /// Flat regions stay flat, edges and corners are quarter-rounded with smooth normals.
    /// Either x end can be left square to show a cut face (a half strap keeps one flat cut).
    public static class RoundedBoxMesh
    {
        public static Mesh Build(Vector3 size, float radius, int segments = 4, bool roundMinX = true, bool roundMaxX = true)
        {
            Vector3 half = size * 0.5f;
            float r = Mathf.Max(0f, Mathf.Min(radius, Mathf.Min(half.x, Mathf.Min(half.y, half.z)) - 1e-6f));
            if (r < 1e-6f) r = 0f;
            segments = Mathf.Max(1, segments);

            Vector3 innerMin = new Vector3(roundMinX ? -half.x + r : -half.x, -half.y + r, -half.z + r);
            Vector3 innerMax = new Vector3(roundMaxX ? half.x - r : half.x, half.y - r, half.z - r);
            float[][] samples =
            {
                AxisSamples(half.x, r, segments, roundMinX, roundMaxX),
                AxisSamples(half.y, r, segments, true, true),
                AxisSamples(half.z, r, segments, true, true)
            };

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            for (int axis = 0; axis < 3; axis++)
            {
                int u = (axis + 1) % 3, v = (axis + 2) % 3;
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    Vector3 faceNormal = Vector3.zero; faceNormal[axis] = sign;
                    bool flatFace = axis == 0 && r > 0f && ((sign < 0 && !roundMinX) || (sign > 0 && !roundMaxX));
                    if (flatFace) { AddFlatFace(vertices, normals, uvs, triangles, axis, u, v, sign, half, r, segments); continue; }
                    float[] us = samples[u], vs = samples[v];
                    int start = vertices.Count;
                    for (int j = 0; j < vs.Length; j++)
                    for (int i = 0; i < us.Length; i++)
                    {
                        Vector3 p = Vector3.zero;
                        p[axis] = sign * half[axis]; p[u] = us[i]; p[v] = vs[j];
                        Vector3 inner = new Vector3(
                            Mathf.Clamp(p.x, innerMin.x, innerMax.x),
                            Mathf.Clamp(p.y, innerMin.y, innerMax.y),
                            Mathf.Clamp(p.z, innerMin.z, innerMax.z));
                        Vector3 d = p - inner;
                        Vector3 n; Vector3 pos;
                        if (r <= 0f || d.sqrMagnitude < 1e-12f) { n = faceNormal; pos = p; }
                        else { n = d.normalized; pos = inner + n * r; }
                        vertices.Add(pos); normals.Add(n);
                        uvs.Add(new Vector2((float)i / (us.Length - 1), (float)j / (vs.Length - 1)));
                    }
                    for (int j = 0; j < vs.Length - 1; j++)
                    for (int i = 0; i < us.Length - 1; i++)
                    {
                        int a = start + j * us.Length + i, b = a + 1, c = a + us.Length, d2 = c + 1;
                        // Wind so the quad faces along faceNormal (u x v flips with sign and axis parity).
                        Vector3 uDir = Vector3.zero; uDir[u] = 1; Vector3 vDir = Vector3.zero; vDir[v] = 1;
                        bool flip = Vector3.Dot(Vector3.Cross(uDir, vDir), faceNormal) < 0f;
                        // Unity front faces wind clockwise when viewed from outside (left-handed).
                        if (flip) { triangles.AddRange(new[] { a, c, b, b, c, d2 }); }
                        else { triangles.AddRange(new[] { a, b, c, b, d2, c }); }
                    }
                }
            }

            var mesh = new Mesh { name = "RoundedBox " + size.x.ToString("0.###") + "x" + size.y.ToString("0.###") + "x" + size.z.ToString("0.###") };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds(); mesh.RecalculateTangents();
            return mesh;
        }

        /// A square cut end: a flat face whose outline is the rounded cross-section, fanned
        /// from its centre so no grid cell collapses onto the outline arc.
        static void AddFlatFace(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles,
            int axis, int u, int v, int sign, Vector3 half, float r, int segments)
        {
            Vector3 faceNormal = Vector3.zero; faceNormal[axis] = sign;
            Vector3 uDir = Vector3.zero; uDir[u] = 1; Vector3 vDir = Vector3.zero; vDir[v] = 1;
            bool flip = Vector3.Dot(Vector3.Cross(uDir, vDir), faceNormal) < 0f;
            int center = vertices.Count;
            Vector3 c = Vector3.zero; c[axis] = sign * half[axis];
            vertices.Add(c); normals.Add(faceNormal); uvs.Add(new Vector2(0.5f, 0.5f));
            int count = 4 * segments;
            float hu = half[u], hv = half[v];
            for (int k = 0; k < count; k++)
            {
                float theta = k / (float)count * Mathf.PI * 2f; int quadrant = k / segments;
                float cu = (quadrant == 0 || quadrant == 3) ? hu - r : -(hu - r);
                float cv = (quadrant == 0 || quadrant == 1) ? hv - r : -(hv - r);
                Vector3 p = c; p[u] = cu + r * Mathf.Cos(theta); p[v] = cv + r * Mathf.Sin(theta);
                vertices.Add(p); normals.Add(faceNormal);
                uvs.Add(new Vector2(0.5f + p[u] / (2f * hu), 0.5f + p[v] / (2f * hv)));
            }
            for (int k = 0; k < count; k++)
            {
                int a = center + 1 + k, b = center + 1 + (k + 1) % count;
                if (flip) triangles.AddRange(new[] { center, b, a }); else triangles.AddRange(new[] { center, a, b });
            }
        }

        static float[] AxisSamples(float half, float r, int segments, bool roundMin, bool roundMax)
        {
            var list = new List<float>();
            if (r <= 0f) { list.Add(-half); list.Add(half); return list.ToArray(); }
            if (roundMin) for (int i = 0; i <= segments; i++) list.Add(-half + r * (1f - Mathf.Cos(i / (float)segments * Mathf.PI * 0.5f)));
            else list.Add(-half);
            if (roundMax) for (int i = segments; i >= 0; i--) list.Add(half - r * (1f - Mathf.Cos(i / (float)segments * Mathf.PI * 0.5f)));
            else list.Add(half);
            // Remove duplicates where the flat span collapses (r == half).
            var cleaned = new List<float>();
            foreach (var x in list) if (cleaned.Count == 0 || x - cleaned[cleaned.Count - 1] > 1e-7f) cleaned.Add(x);
            return cleaned.ToArray();
        }
    }
}
