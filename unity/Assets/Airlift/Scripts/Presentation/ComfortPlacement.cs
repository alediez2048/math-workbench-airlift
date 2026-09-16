using Oculus.Interaction;
using UnityEngine;

namespace Airlift.Presentation
{
    public sealed class ComfortPlacement : MonoBehaviour
    {
        public Transform stationRoot;
        public Transform head;
        public Grabbable grabbable;
        public TMPro.TMP_Text statusText;
        public float distance = 0.65f;
        public float belowEyes = 0.55f;
        public float heightStep = 0.05f;
        public float minimumHeight = 0.35f;
        public float maximumHeight = 1.4f;
        public bool IsHeld { get; private set; }
        public string LastMessage { get; private set; }

        void OnEnable() { if (grabbable != null) grabbable.WhenPointerEventRaised += HandlePointer; }
        void OnDisable() { if (grabbable != null) grabbable.WhenPointerEventRaised -= HandlePointer; }
        void HandlePointer(PointerEvent evt)
        {
            if (evt.Type == PointerEventType.Select) IsHeld = true;
            if (evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel) IsHeld = false;
        }
        public bool TryPlace(Vector3 position, Quaternion rotation, bool held)
        {
            if (held || IsHeld) { Message("Release the strap before moving the station."); return false; }
            if (stationRoot == null || !float.IsFinite(position.x) || !float.IsFinite(position.y) || !float.IsFinite(position.z)) return false;
            position.y = Mathf.Clamp(position.y, minimumHeight, maximumHeight);
            stationRoot.SetPositionAndRotation(position, rotation);
            Message("Station adjusted. Virtual surface — do not lean on it.");
            return true;
        }
        void Message(string message){LastMessage=message;if(statusText!=null)statusText.text=message;}
        public void Raise() { if (stationRoot != null) TryPlace(stationRoot.position + Vector3.up * heightStep, stationRoot.rotation, false); }
        public void Lower() { if (stationRoot != null) TryPlace(stationRoot.position - Vector3.up * heightStep, stationRoot.rotation, false); }
        public void Recenter()
        {
            if (head == null) return;
            var forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            TryPlace(head.position + forward * distance - Vector3.up * belowEyes, Quaternion.LookRotation(forward), false);
        }
    }
}
