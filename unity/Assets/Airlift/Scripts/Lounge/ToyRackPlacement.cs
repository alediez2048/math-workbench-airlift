using UnityEngine;

namespace Airlift.Lounge
{
    /// Owner 2026-09-29: "the rack needs to be set against a wall with no window". The lounge shell (BuildLounge) is a
    /// ring of 12 wall segments, 30° each, radius 3.1 m, with glazed bays at segments 2, 3, 9 and 10. The rack goes on
    /// the solid segment nearest to the learner's left of the board, its back to the wall. Pure so it is testable.
    public static class ToyRackPlacement
    {
        // WallRadius keeps a 2.3 m rack's ends inside the neighbouring wall segments of the 12-gon (they angle inward).
        public const int Segments = 12; public const float RoomRadius = 3.1f, WallRadius = RoomRadius - 0.4f;
        static readonly int[] Windows = { 2, 3, 9, 10 };

        /// Yaw (degrees, 0 = +z, 90 = +x) of the point beside the board on the learner's left.
        public static float PreferredAngle(Vector3 boardPosition, Vector3 boardForward, float besideBoard)
        {
            var forward = Vector3.ProjectOnPlane(boardForward, Vector3.up).normalized; if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            var left = -Vector3.Cross(Vector3.up, forward).normalized;   // the learner faces the board along `forward`
            var p = boardPosition + left * besideBoard;
            return Mathf.Atan2(p.x, p.z) * Mathf.Rad2Deg;
        }

        /// The centre angle of the nearest segment that is solid and has solid neighbours on both sides: the rack is
        /// wider than one segment, so a neighbour that is a window would show through its end (owner 2026-09-29).
        public static float SolidWallAngle(float preferredDegrees)
        {
            float step = 360f / Segments, best = 0f, bestDist = float.MaxValue;
            for (int i = 0; i < Segments; i++)
            {
                if (IsWindow(i) || IsWindow((i + 1) % Segments) || IsWindow((i + Segments - 1) % Segments)) continue;
                float a = i * step, d = Mathf.Abs(Mathf.DeltaAngle(preferredDegrees, a));
                if (d < bestDist - 0.001f) { bestDist = d; best = a; }
            }
            return best;
        }

        static bool IsWindow(int segment) => System.Array.IndexOf(Windows, segment) >= 0;

        /// Against the wall at that angle, on the floor, facing the room's centre.
        public static (Vector3 position, Quaternion rotation) Pose(float wallDegrees)
        {
            var dir = Quaternion.Euler(0f, wallDegrees, 0f) * Vector3.forward;
            return (dir * WallRadius, Quaternion.LookRotation(-dir, Vector3.up));
        }
    }
}
