using UnityEngine;

namespace Airlift.Presentation.Garden
{
    /// Pure station-local geometry of the Sunny Plot bed: a rows × columns grid of square cells centred on
    /// Center (x, z). Row 0 is the back row (the top line of the array, like a page); column 0 is on the left.
    /// Center.y is the rest height of a planted strip. Every seedling strip is one row, packed from column 0.
    /// A centred grid turned a quarter turn lands exactly on the columns × rows grid, so the turn animation
    /// can hand over to the regenerated layout without moving any seedling.
    public readonly struct GardenBedLayout
    {
        public const int MaxRows = 8, MaxColumns = 8;

        public readonly Vector3 Center;
        public readonly float Cell;
        public readonly int Rows, Columns;

        public GardenBedLayout(Vector3 center, float cell, int rows, int columns)
        {
            Center = center; Cell = cell;
            Rows = Mathf.Clamp(rows, 0, MaxRows); Columns = Mathf.Clamp(columns, 0, MaxColumns);
        }

        public float Width => Columns * Cell;
        public float Depth => Rows * Cell;
        public float Left => Center.x - Width * 0.5f;
        public float Right => Center.x + Width * 0.5f;
        public float Back => Center.z + Depth * 0.5f;
        public float Front => Center.z - Depth * 0.5f;

        public float RowZ(int row) => Back - (row + 0.5f) * Cell;
        public float ColumnX(int column) => Left + (column + 0.5f) * Cell;
        /// x of the line after `columnsLeft` columns: 0 is the left edge, Columns the right edge.
        public float BoundaryX(int columnsLeft) => Left + columnsLeft * Cell;
        public Vector3 CellCenter(int row, int column) => new Vector3(ColumnX(column), Center.y, RowZ(row));

        /// Root position of a planted strip of `length` seedlings in `row` (strip roots are centred on their body).
        public Vector3 StripPosition(int row, int length) => new Vector3(Left + length * Cell * 0.5f, Center.y, RowZ(row));

        /// Offset of seedling k inside a strip of `length` seedlings, strip-local x.
        public static float SeedlingX(float cell, int k, int length) => (k + 0.5f) * cell - length * cell * 0.5f;

        /// Row under a released strip's centre, or -1. The strip may be released up to one cell beyond the side
        /// walls (a long strip is held by one end) and up to 0.4 cell beyond the front and back walls.
        public int RowAt(Vector3 local)
        {
            if (Rows <= 0 || Columns <= 0) return -1;
            if (Mathf.Abs(local.y - Center.y) > 0.15f) return -1;
            if (local.x < Left - Cell || local.x > Right + Cell) return -1;
            if (local.z > Back + Cell * 0.4f || local.z < Front - Cell * 0.4f) return -1;
            int row = Mathf.FloorToInt((Back - local.z) / Cell);
            return Mathf.Clamp(row, 0, Rows - 1);
        }

        /// Fence position (columns left of the fence, 1..Columns-1) nearest a released fence, or -1 when the fence
        /// is not over the bed. Snaps to the nearest line between two columns.
        public int FenceAt(Vector3 local)
        {
            if (Columns < 2) return -1;
            if (Mathf.Abs(local.y - Center.y) > 0.15f) return -1;
            if (local.z > Back + Cell || local.z < Front - Cell) return -1;
            if (local.x < Left || local.x > Right) return -1;
            int k = Mathf.RoundToInt((local.x - Left) / Cell);
            return k >= 1 && k <= Columns - 1 ? k : -1;
        }

        public GardenBedLayout Turned() => new GardenBedLayout(Center, Cell, Columns, Rows);

        /// The bed for rows × columns: cells as large as maxCell while the longer side fits maxDepth (smaller only when it
        /// must), centred on centerX, growing back from the front line frontZ. The longer side sets both the cell and the
        /// centre, so a bed and its quarter turn share them and turning moves no seedling.
        public static GardenBedLayout Fit(float centerX, float restY, float frontZ, float maxDepth, float maxCell, int rows, int columns)
        {
            int span = Mathf.Max(1, Mathf.Max(rows, columns));
            float cell = Mathf.Min(maxCell, maxDepth / span);
            return new GardenBedLayout(new Vector3(centerX, restY, frontZ + span * cell * 0.5f), cell, rows, columns);
        }

        /// Where a point of the grid goes when the bed turns a quarter turn clockwise seen from above
        /// (positive yaw), about the bed centre.
        public Vector3 TurnPoint(Vector3 local, float degrees)
        {
            var pivot = new Vector3(Center.x, local.y, Center.z);
            return pivot + Quaternion.Euler(0, degrees, 0) * (local - pivot);
        }
    }
}
