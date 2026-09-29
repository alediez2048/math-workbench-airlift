using Oculus.Interaction;
using UnityEngine;

namespace Airlift.Lounge
{
    /// Owner 2026-09-29: pointing at a locked toy should say what earns it instead of doing nothing. Listens to the
    /// toy's ray interactable; while the rack says the toy is locked, a hover or a trigger press raises Pointed.
    public sealed class ToyLockedHint : MonoBehaviour
    {
        public RayInteractable ray;
        public bool Locked = true;
        public System.Action<ToyLockedHint> Pointed;

        void Awake() { if (ray == null) ray = GetComponentInChildren<RayInteractable>(true); }
        void OnEnable() { if (ray != null) ray.WhenPointerEventRaised += OnPointer; }
        void OnDisable() { if (ray != null) ray.WhenPointerEventRaised -= OnPointer; }

        void OnPointer(PointerEvent evt)
        {
            if (!Locked) return;
            if (evt.Type == PointerEventType.Hover || evt.Type == PointerEventType.Select) Pointed?.Invoke(this);
        }
    }
}
