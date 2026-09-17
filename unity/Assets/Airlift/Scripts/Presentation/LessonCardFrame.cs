using UnityEngine;

namespace Airlift.Presentation
{
    /// The one place lesson benches read the story card frame from: the Cargo "Lesson interface" canvas under the
    /// world-locked station root (local pose), world-space scale 0.001 and a 920 × 470 canvas. A bench's own card
    /// canvas copies this frame so every lesson's card sits where the learner already looks.
    public static class LessonCardFrame
    {
        public const string InterfaceName = "Lesson interface";
        public const float Scale = 0.001f;
        public static readonly Vector2 Size = new Vector2(920f, 470f);

        /// Local position and rotation of the Cargo card canvas under the station root; false when it is missing.
        public static bool TryRead(Transform stationRoot, out Vector3 localPosition, out Quaternion localRotation)
        {
            localPosition = Vector3.zero; localRotation = Quaternion.identity;
            if (stationRoot == null) return false;
            foreach (Transform child in stationRoot)
            {
                if (child.name != InterfaceName) continue;
                localPosition = child.localPosition; localRotation = child.localRotation;
                return true;
            }
            return false;
        }

        /// Places a card canvas in the frame: same local pose as the Cargo card, scale 0.001, 920 × 470.
        public static bool Apply(Transform stationRoot, RectTransform card)
        {
            if (card == null || !TryRead(stationRoot, out var position, out var rotation)) return false;
            card.localPosition = position; card.localRotation = rotation; card.localScale = Vector3.one * Scale;
            card.sizeDelta = Size;
            return true;
        }
    }
}
