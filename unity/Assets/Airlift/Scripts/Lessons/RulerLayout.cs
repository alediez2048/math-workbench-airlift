using UnityEngine;

namespace Airlift.Lessons
{
    /// Pure geometry for the zero-to-one ruler in station-local space. The ruler is the
    /// visible whole: every docked piece is packed from the zero mark by exact cells.
    public static class RulerLayout
    {
        public const float WholeLength = 0.28f;
        public const float DockTolerance = 0.08f;

        public static Vector3 Origin(Vector3 rulerCenter) => rulerCenter - new Vector3(WholeLength * 0.5f, 0, 0);

        public static bool InDockZone(Vector3 rulerCenter, Vector3 pieceLocal)
        {
            Vector3 origin = Origin(rulerCenter);
            return Mathf.Abs(pieceLocal.z - rulerCenter.z) <= DockTolerance
                && Mathf.Abs(pieceLocal.y - rulerCenter.y) <= 0.15f
                && pieceLocal.x >= origin.x - DockTolerance
                && pieceLocal.x <= origin.x + WholeLength + DockTolerance;
        }

        public static Vector3 SnapPosition(Vector3 rulerCenter, int startCell, int cells, float restHeight)
        {
            float cell = WholeLength / PlacementState.CellsPerWhole;
            Vector3 origin = Origin(rulerCenter);
            return new Vector3(origin.x + (startCell + cells * 0.5f) * cell, restHeight, rulerCenter.z);
        }

        /// Container cell (0..7) under a released crate's centre, or -1 outside the dock zone. Vehicles back
        /// up over these cells, so the cell picks the bed the crate is dropped into.
        public static int CellAt(Vector3 rulerCenter, Vector3 pieceLocal)
        {
            if (!InDockZone(rulerCenter, pieceLocal)) return -1;
            float cell = WholeLength / PlacementState.CellsPerWhole;
            int index = Mathf.FloorToInt((pieceLocal.x - Origin(rulerCenter).x) / cell + 1e-4f);
            return Mathf.Clamp(index, 0, PlacementState.CellsPerWhole - 1);
        }

        /// Station-local x of the centre of a run of cells.
        public static float CellsCenterX(Vector3 rulerCenter, int startCell, int cells) =>
            Origin(rulerCenter).x + (startCell + cells * 0.5f) * (WholeLength / PlacementState.CellsPerWhole);

        public static float PieceLength(int cells) => WholeLength * cells / PlacementState.CellsPerWhole;
    }
}
