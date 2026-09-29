using UnityEngine;

namespace Airlift.Lounge
{
    /// Owner 2026-09-29: a toy stays where it is let go, at the size two hands made it. Pure limits.
    public static class ToyPlacementRule
    {
        public const float MinScale = 0.5f, MaxScale = 3f;
        /// Limits are relative to the size the toy was built at, so a rack toy can grow to three times itself.
        public static float ClampScale(float scale, float builtScale) => Mathf.Clamp(scale, MinScale * builtScale, MaxScale * builtScale);
        /// A toy released below the floor comes up to rest on it; anywhere above, it stays put.
        public static Vector3 Settle(Vector3 released, float toyHalfHeight) => released.y < toyHalfHeight ? new Vector3(released.x, toyHalfHeight, released.z) : released;
    }
}
