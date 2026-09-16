using Oculus.Interaction;
using UnityEngine;

namespace Airlift.Presentation
{
    /// One-hand transformer for the table handle: the grabbed point follows the hand and
    /// the table keeps its front edge toward the player. Wrist tilt and twist are ignored,
    /// so the table never swings about the hand. Rotation is eased to avoid jitter.
    public sealed class TableCarryTransformer : MonoBehaviour, ITransformer
    {
        public Transform head;
        [Range(0.05f, 1f)] public float turnEase = 0.3f;
        IGrabbable grabbable;
        Vector3 grabLocal;

        public void Initialize(IGrabbable target) { grabbable = target; }

        public void BeginTransform()
        {
            var root = grabbable.Transform;
            grabLocal = root.InverseTransformPoint(grabbable.GrabPoints[0].position);
        }

        public void UpdateTransform()
        {
            var root = grabbable.Transform;
            Vector3 hand = grabbable.GrabPoints[0].position;
            Vector3 headPosition = head != null ? head.position : hand - root.forward;
            var target = TableAdjustRules.CarryPose(hand, headPosition, grabLocal, root.localScale.x, root.rotation);
            Quaternion eased = Quaternion.Slerp(root.rotation, target.rotation, turnEase);
            Vector3 position = hand - eased * (grabLocal * root.localScale.x);
            root.SetPositionAndRotation(position, eased);
        }

        public void EndTransform() { }
    }
}
