using UnityEngine;

namespace Airlift.Onboarding
{
    /// Pure placement rules for the world-locked station. Before head tracking is
    /// acquired the camera reports the origin, which previously placed the board on
    /// the floor; placement must wait for a tracked head.
    public static class OnboardingPlacement
    {
        /// Below a seated adult's eye height the headset is on a desk or a lap, not on a head: the board waits.
        /// (Owner 2026-09-29: an adb launch with the headset awake on the desk placed the board under the floor.)
        public const float MinimumHeadHeight = 0.9f;
        public const float MinimumBoardHeight = 0.35f;

        public static bool IsHeadTracked(Vector3 headPosition)
        {
            return float.IsFinite(headPosition.y) && headPosition.y > MinimumHeadHeight;
        }

        public static Pose BoardPose(Vector3 headPosition, Vector3 headForward, float distance, float belowEyes)
        {
            Vector3 forward = Vector3.ProjectOnPlane(headForward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            Vector3 position = new Vector3(headPosition.x, Mathf.Max(MinimumBoardHeight, headPosition.y - belowEyes), headPosition.z)
                + forward * distance;
            return new Pose(position, Quaternion.LookRotation(forward, Vector3.up));
        }
    }
}
