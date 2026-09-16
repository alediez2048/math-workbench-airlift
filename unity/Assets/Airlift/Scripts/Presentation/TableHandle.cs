using System.Collections;
using Airlift.Lessons;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

namespace Airlift.Presentation
{
    /// Lets the player carry and resize the whole table by a handle. The SDK transformers
    /// move the station root while the handle is held; this component only gates the grab
    /// (no piece may be held), pauses piece grabs meanwhile, and settles the table on release.
    public sealed class TableHandle : MonoBehaviour
    {
        public Grabbable grabbable;
        public Transform stationRoot;
        public ComfortPlacement placement;
        public CargoLessonDirector lesson;
        public GrabInteractable handleInteractable;
        public GrabInteractable[] pieceInteractables = new GrabInteractable[0];
        public TMP_Text statusText;
        public bool IsHeld { get; private set; }
        bool handleEnabled = true;

        bool AnyPieceHeld => (placement != null && placement.IsHeld) || (lesson != null && lesson.AnyPieceHeld);

        void OnEnable() { if (grabbable != null) grabbable.WhenPointerEventRaised += OnPointer; }
        void OnDisable() { if (grabbable != null) grabbable.WhenPointerEventRaised -= OnPointer; }

        void Update()
        {
            if (handleInteractable == null || IsHeld) return;
            bool allow = TableAdjustRules.CanAdjust(AnyPieceHeld);
            if (allow == handleEnabled) return;
            handleEnabled = allow;
            if (allow) handleInteractable.Enable(); else handleInteractable.Disable();
        }

        void OnPointer(PointerEvent evt)
        {
            if (evt.Type == PointerEventType.Select)
            {
                IsHeld = true;
                SetPieces(false);
            }
            else if (evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
            {
                StartCoroutine(SettleAfterRelease());
            }
        }

        IEnumerator SettleAfterRelease()
        {
            yield return null; // let the SDK drop the grab point first
            if (grabbable == null || grabbable.GrabPoints.Count > 0) yield break;
            IsHeld = false;
            Settle();
            SetPieces(true);
        }

        public void Settle()
        {
            if (stationRoot == null) return;
            float min = placement != null ? placement.minimumHeight : 0.35f, max = placement != null ? placement.maximumHeight : 1.4f;
            var pose = TableAdjustRules.Settle(new Pose(stationRoot.position, stationRoot.rotation), min, max);
            stationRoot.SetPositionAndRotation(pose.position, pose.rotation);
            stationRoot.localScale = Vector3.one * TableAdjustRules.ClampScale(stationRoot.localScale.x);
            if (statusText != null) statusText.text = "Table placed. Virtual surface — do not lean on it.";
        }

        void SetPieces(bool on)
        {
            foreach (var piece in pieceInteractables)
            {
                if (piece == null) continue;
                if (on) piece.Enable(); else piece.Disable();
            }
        }
    }
}
