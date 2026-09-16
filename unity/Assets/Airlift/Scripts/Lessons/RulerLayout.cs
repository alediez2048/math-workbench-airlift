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

        public static float PieceLength(int cells) => WholeLength * cells / PlacementState.CellsPerWhole;
    }
}
