using Oculus.Interaction;
using UnityEngine;

namespace Airlift.Lounge
{
    /// One toy on the rack: remembers its slot, counts the hands holding it, and when the last hand lets go keeps the
    /// toy exactly there (settled onto the floor if dropped below it) and reports the pose so the library can save it.
    /// The first pick-up is reported to the rack (it ends the "new" glow).
    public sealed class ToyPlace : MonoBehaviour
    {
        public Grabbable grabbable;
        public Transform home;
        [Tooltip("Half the toy's height at scale 1, so a toy let go below the floor comes up to rest on it.")]
        public float halfHeight = 0.05f;
        [Tooltip("The size the toy was built at; the size it returns to and the base for the two-hand limits.")]
        public float homeScale = 1f;
        public System.Action<ToyPlace> FirstGrab, Released;
        int hands; bool everGrabbed;
        Rigidbody body;

        void Awake() { body = GetComponent<Rigidbody>(); if (grabbable == null) grabbable = GetComponentInChildren<Grabbable>(true); }
        void OnEnable() { if (grabbable != null) grabbable.WhenPointerEventRaised += OnPointer; }
        void OnDisable() { if (grabbable != null) grabbable.WhenPointerEventRaised -= OnPointer; }

        public bool Held => hands > 0;

        void OnPointer(PointerEvent evt)
        {
            if (evt.Type == PointerEventType.Select)
            {
                hands++;
                if (!everGrabbed) { everGrabbed = true; FirstGrab?.Invoke(this); }
            }
            else if (evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
            {
                if (hands == 0) return;   // a hover cancel from the ray is not a release
                hands--;
                if (hands == 0) Settle();
            }
        }

        void Settle()
        {
            float scale = ToyPlacementRule.ClampScale(transform.localScale.x, homeScale);
            transform.localScale = Vector3.one * scale;
            transform.position = ToyPlacementRule.Settle(transform.position, halfHeight * scale);
            if (body != null) { body.isKinematic = true; body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
            Released?.Invoke(this);
        }

        /// Put the toy at a pose (world space) and size, or back in its slot when no pose is given.
        public void Apply(Vector3? position, Quaternion? rotation, float? scale)
        {
            if (Held) return;
            transform.localScale = Vector3.one * ToyPlacementRule.ClampScale(scale ?? homeScale, homeScale);
            if (position.HasValue && rotation.HasValue) transform.SetPositionAndRotation(position.Value, rotation.Value);
            else if (home != null) transform.SetPositionAndRotation(home.position, home.rotation);
        }

        public void SnapHome() { if (!Held && home != null) transform.SetPositionAndRotation(home.position, home.rotation); }
    }
}
