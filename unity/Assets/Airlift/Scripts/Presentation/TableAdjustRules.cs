using UnityEngine;

namespace Airlift.Presentation
{
    /// Pure rules for the player moving and resizing the whole table by its handle.
    /// The table always settles level (yaw only), within a height band and a scale band,
    /// and it may only be adjusted while no piece is held.
    public static class TableAdjustRules
    {
        public const float MinScale = 0.5f;
        public const float MaxScale = 2f;

        public static bool CanAdjust(bool anyPieceHeld) => !anyPieceHeld;

        public static Quaternion Level(Quaternion rotation)
        {
            Vector3 forward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up);
            if (forward.sqrMagnitude < 1e-6f) forward = Vector3.ProjectOnPlane(rotation * Vector3.up, Vector3.up);
            if (forward.sqrMagnitude < 1e-6f) forward = Vector3.forward;
            return Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        public static float ClampScale(float scale)
        {
            if (!float.IsFinite(scale) || scale <= 0f) return 1f;
            return Mathf.Clamp(scale, MinScale, MaxScale);
        }

        /// One-hand carry: the grabbed point stays in the hand and the table turns so its
        /// front edge faces the player (root forward points away from the head). Yaw only.
        /// grabLocal is the grabbed point in unscaled table-local space; scale is uniform.
        public static Pose CarryPose(Vector3 handWorld, Vector3 headWorld, Vector3 grabLocal, float scale, Quaternion previousRotation)
        {
            Vector3 away = Vector3.ProjectOnPlane(handWorld - headWorld, Vector3.up);
            Quaternion rotation = away.sqrMagnitude < 1e-4f ? Level(previousRotation) : Quaternion.LookRotation(away.normalized, Vector3.up);
            Vector3 position = handWorld - rotation * (grabLocal * scale);
            return new Pose(position, rotation);
        }

        public static Pose Settle(Pose pose, float minimumHeight, float maximumHeight)
        {
            Vector3 p = pose.position;
            if (!float.IsFinite(p.x) || !float.IsFinite(p.y) || !float.IsFinite(p.z)) p = new Vector3(0, minimumHeight, 0);
            p.y = Mathf.Clamp(p.y, minimumHeight, maximumHeight);
            return new Pose(p, Level(pose.rotation));
        }
    }
}
