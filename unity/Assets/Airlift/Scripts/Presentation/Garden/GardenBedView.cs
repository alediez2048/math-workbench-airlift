using TMPro;
using UnityEngine;

namespace Airlift.Presentation.Garden
{
    /// The raised bed of Sunny Plot. A pool of 8 × 8 soil cells, four wooden walls, row numbers along the left wall
    /// and numbered fence spots along the front wall; Apply shows exactly the current rows × columns grid, sized by
    /// GardenBedLayout.Fit (full-size cells unless the bed is too deep for the table). Narrative and layout only:
    /// whether a bed is correct is decided by GardenModel.
    public sealed class GardenBedView : MonoBehaviour
    {
        [Tooltip("Station-local x of the bed centre.")]
        public float centerX = 0f;
        [Tooltip("Station-local rest height of a planted strip.")]
        public float restHeight = GardenTableLayout.PlantedRestY;
        [Tooltip("Station-local z of the inside of the front wall of the deepest bed; every bed grows back from here.")]
        public float frontZ = GardenTableLayout.BedFrontZ;
        [Tooltip("Deepest bed inside the walls; beds whose longer side does not fit at full-size cells get smaller cells.")]
        public float maxDepth = GardenTableLayout.BedMaxDepth;
        [Tooltip("Largest cell: one seedling of a full-size strip.")]
        public float cell = GardenTableLayout.StripCell;
        [Tooltip("Cell size the soil and wall meshes were built for; smaller beds scale them.")]
        public float meshCell = GardenTableLayout.StripCell;
        public float wallThickness = GardenTableLayout.WallThickness;
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
        public float cellHeight = GardenTableLayout.DeckTop + GardenTableLayout.SoilHeight * 0.5f;
        public float wallHeight = GardenTableLayout.DeckTop + GardenTableLayout.WallHeight * 0.5f, labelHeight = GardenTableLayout.DeckTop + 0.04f;
        [Tooltip("Gap between the left wall face and the centre of a row number.")]
        public float rowLabelOffset = GardenTableLayout.RowLabelOffset;
        [Tooltip("Station space for every position above; defaults to this view's parent.")]
        public Transform stationRoot;

        public GardenBedLayout Layout { get; private set; }
        public int SplitColumn { get; private set; }
        public float SplitGap { get; private set; }

        public GardenBedLayout LayoutFor(int rows, int columns) => GardenBedLayout.Fit(centerX, restHeight, frontZ, maxDepth, cell, rows, columns);

        /// Shows the rows × columns grid. With splitColumn in 1..columns-1 and a gap, the columns right of the fence
        /// (cells, right wall, back and front walls stay whole) slide right by the gap: the bed opens at the fence.
        public void Apply(int rows, int columns, int splitColumn = 0, float gap = 0f)
        {
            var layout = LayoutFor(rows, columns);
            Layout = layout;
            SplitColumn = splitColumn > 0 && splitColumn < layout.Columns ? splitColumn : 0;
            SplitGap = SplitColumn > 0 ? Mathf.Max(0f, gap) : 0f;
            if (turnable != null)
            {
                // The turnable group turns about the bed centre, which moves with the bed size.
                turnable.localRotation = Quaternion.identity;
                turnable.localPosition = LocalOf(turnable, new Vector3(layout.Center.x, 0f, layout.Center.z));
            }
            float cellScale = meshCell > 0f ? layout.Cell / meshCell : 1f;

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
                    go.transform.localScale = new Vector3(cellScale, 1f, cellScale);
                }

            float t = wallThickness, extra = SplitGap;
            Wall(wallBack, longWalls, layout.Columns, layout.Cell, new Vector3(layout.Center.x + extra * 0.5f, wallHeight, layout.Back + t * 0.5f), false, extra);
            Wall(wallFront, longWalls, layout.Columns, layout.Cell, new Vector3(layout.Center.x + extra * 0.5f, wallHeight, layout.Front - t * 0.5f), false, extra);
            Wall(wallLeft, shortWalls, layout.Rows, layout.Cell, new Vector3(layout.Left - t * 0.5f, wallHeight, layout.Center.z), true, 0f);
            Wall(wallRight, shortWalls, layout.Rows, layout.Cell, new Vector3(layout.Right + extra + t * 0.5f, wallHeight, layout.Center.z), true, 0f);

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

        public void ShowLabels(bool on) => ShowLabels(on, on);

        /// Row numbers and fence spot numbers separately (the concept intro shows row numbers only).
        public void ShowLabels(bool rows, bool spots)
        {
            if (rowLabels != null) foreach (var label in rowLabels) if (label != null) label.enabled = rows;
            if (fenceTicks != null) foreach (var tick in fenceTicks) if (tick != null) foreach (var text in tick.GetComponentsInChildren<TMP_Text>(true)) text.enabled = spots;
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

        /// Wall meshes are built for meshCell cells; a bed with smaller cells or an opened split stretches them along x.
        void Wall(MeshFilter wall, Mesh[] meshes, int cells, float bedCell, Vector3 stationLocal, bool alongZ, float extraLength)
        {
            if (wall == null) return;
            bool on = cells > 0;
            wall.gameObject.SetActive(on);
            if (!on) return;
            if (meshes != null && cells < meshes.Length && meshes[cells] != null) wall.sharedMesh = meshes[cells];
            wall.transform.localPosition = LocalOf(wall.transform, stationLocal);
            wall.transform.localRotation = alongZ ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
            float corners = alongZ ? 0f : 2f * wallThickness;
            float baseLength = cells * (meshCell > 0f ? meshCell : bedCell) + corners;
            float length = cells * bedCell + corners + Mathf.Max(0f, extraLength);
            wall.transform.localScale = baseLength > 0f && Mathf.Abs(length - baseLength) > 1e-6f ? new Vector3(length / baseLength, 1f, 1f) : Vector3.one;
        }
    }
}
