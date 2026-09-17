using TMPro;
using UnityEngine;

namespace Airlift.Presentation.Garden
{
    /// The raised bed of Sunny Plot. A pool of 8 × 8 soil cells, four wooden walls, row numbers along the left wall
    /// and numbered fence spots along the front wall; Apply shows exactly the current rows × columns grid. Narrative
    /// and layout only: whether a bed is correct is decided by GardenModel.
    public sealed class GardenBedView : MonoBehaviour
    {
        [Tooltip("Station-local centre of the bed (x, z) and the rest height of a planted strip (y).")]
        public Vector3 center = new Vector3(0f, 0.0415f, -0.03f);
        public float cell = 0.045f;
        public float wallThickness = 0.012f;
        [Tooltip("Rotated as one piece while the bed turns; holds the cells and walls.")]
        public Transform turnable;
        [Tooltip("Row r, column c is cells[r * 8 + c].")]
        public GameObject[] cells = new GameObject[GardenBedLayout.MaxRows * GardenBedLayout.MaxColumns];
        public MeshFilter wallBack, wallFront, wallLeft, wallRight;
        [Tooltip("Index n: a wall along x that covers n cells plus both corner posts.")]
        public Mesh[] longWalls = new Mesh[GardenBedLayout.MaxColumns + 1];
        [Tooltip("Index n: a wall that covers exactly n cells (the side walls, rotated along z).")]
        public Mesh[] shortWalls = new Mesh[GardenBedLayout.MaxRows + 1];
        public TMP_Text[] rowLabels = new TMP_Text[GardenBedLayout.MaxRows];
        [Tooltip("Index k-1: the peg and number for a fence after k columns (k = 1..7).")]
        public GameObject[] fenceTicks = new GameObject[GardenBedLayout.MaxColumns - 1];
        public float cellHeight = 0.0265f, wallHeight = 0.0335f, labelHeight = 0.052f;
        [Tooltip("Gap between the left wall face and the centre of a row number.")]
        public float rowLabelOffset = 0.03f;
        [Tooltip("Station space for every position above; defaults to this view's parent.")]
        public Transform stationRoot;

        public GardenBedLayout Layout { get; private set; }
        public int SplitColumn { get; private set; }
        public float SplitGap { get; private set; }

        public GardenBedLayout LayoutFor(int rows, int columns) => new GardenBedLayout(center, cell, rows, columns);

        /// Shows the rows × columns grid. With splitColumn in 1..columns-1 and a gap, the columns right of the fence
        /// (cells, right wall, back and front walls stay whole) slide right by the gap: the bed opens at the fence.
        public void Apply(int rows, int columns, int splitColumn = 0, float gap = 0f)
        {
            var layout = LayoutFor(rows, columns);
            Layout = layout;
            SplitColumn = splitColumn > 0 && splitColumn < layout.Columns ? splitColumn : 0;
            SplitGap = SplitColumn > 0 ? Mathf.Max(0f, gap) : 0f;
            if (turnable != null) turnable.localRotation = Quaternion.identity;

            for (int r = 0; r < GardenBedLayout.MaxRows; r++)
                for (int c = 0; c < GardenBedLayout.MaxColumns; c++)
                {
                    var go = Cell(r, c);
                    if (go == null) continue;
                    bool on = r < layout.Rows && c < layout.Columns;
                    go.SetActive(on);
                    if (!on) continue;
                    float x = layout.ColumnX(c) + (SplitColumn > 0 && c >= SplitColumn ? SplitGap : 0f);
                    go.transform.localPosition = LocalOf(go.transform, new Vector3(x, cellHeight, layout.RowZ(r)));
                }

            float t = wallThickness, extra = SplitGap;
            Wall(wallBack, longWalls, layout.Columns, new Vector3(layout.Center.x + extra * 0.5f, wallHeight, layout.Back + t * 0.5f), false, extra);
            Wall(wallFront, longWalls, layout.Columns, new Vector3(layout.Center.x + extra * 0.5f, wallHeight, layout.Front - t * 0.5f), false, extra);
            Wall(wallLeft, shortWalls, layout.Rows, new Vector3(layout.Left - t * 0.5f, wallHeight, layout.Center.z), true, 0f);
            Wall(wallRight, shortWalls, layout.Rows, new Vector3(layout.Right + extra + t * 0.5f, wallHeight, layout.Center.z), true, 0f);

            for (int r = 0; r < rowLabels.Length; r++)
            {
                var label = rowLabels[r];
                if (label == null) continue;
                bool on = r < layout.Rows;
                label.gameObject.SetActive(on);
                if (!on) continue;
                label.text = (r + 1).ToString();
                label.transform.localPosition = LocalOf(label.transform, new Vector3(layout.Left - t - rowLabelOffset, labelHeight, layout.RowZ(r)));
            }
            for (int k = 1; k <= fenceTicks.Length; k++)
            {
                var tick = fenceTicks[k - 1];
                if (tick == null) continue;
                bool on = k < layout.Columns && SplitColumn == 0;   // an opened split shows its part labels there instead
                tick.SetActive(on);
                if (!on) continue;
                float x = layout.BoundaryX(k) + (SplitColumn > 0 && k > SplitColumn ? SplitGap : SplitColumn == k ? SplitGap * 0.5f : 0f);
                tick.transform.localPosition = LocalOf(tick.transform, new Vector3(x, 0f, layout.Front - t * 0.5f));
            }
        }

        public void ShowLabels(bool on)
        {
            foreach (var label in rowLabels) if (label != null) label.enabled = on;
            foreach (var tick in fenceTicks) if (tick != null) foreach (var text in tick.GetComponentsInChildren<TMP_Text>(true)) text.enabled = on;
        }

        public GameObject Cell(int row, int column)
        {
            int i = row * GardenBedLayout.MaxColumns + column;
            return cells != null && i >= 0 && i < cells.Length ? cells[i] : null;
        }

        public int ActiveCellCount()
        {
            int n = 0;
            if (cells != null) foreach (var c in cells) if (c != null && c.activeSelf) n++;
            return n;
        }

        /// Converts a station-local point into the local space of target's parent (cells and walls live under the
        /// turnable group, which sits at the bed centre; labels and ticks live directly under the view).
        Vector3 LocalOf(Transform target, Vector3 stationLocal)
        {
            var station = stationRoot != null ? stationRoot : transform.parent;
            var parent = target.parent;
            if (station == null || parent == null) return stationLocal;
            return parent.InverseTransformPoint(station.TransformPoint(stationLocal));
        }

        void Wall(MeshFilter wall, Mesh[] meshes, int cells, Vector3 stationLocal, bool alongZ, float extraLength)
        {
            if (wall == null) return;
            bool on = cells > 0;
            wall.gameObject.SetActive(on);
            if (!on) return;
            if (meshes != null && cells < meshes.Length && meshes[cells] != null) wall.sharedMesh = meshes[cells];
            wall.transform.localPosition = LocalOf(wall.transform, stationLocal);
            wall.transform.localRotation = alongZ ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
            float baseLength = cells * cell + (alongZ ? 0f : 2f * wallThickness);
            wall.transform.localScale = extraLength > 0f && baseLength > 0f ? new Vector3((baseLength + extraLength) / baseLength, 1f, 1f) : Vector3.one;
        }
    }
}
