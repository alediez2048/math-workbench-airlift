using System.Collections;
using Airlift.Presentation;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Airlift.Lessons
{
    /// Presentation adapter for the whole-and-halves chapter. All arithmetic decisions
    /// come from CargoLessonModel; this class only maps grabs, releases and buttons to
    /// model commands and mirrors the model back into positions, labels and text.
    public sealed class CargoLessonDirector : MonoBehaviour
    {
        [System.Serializable]
        public sealed class PieceView
        {
            public string id;
            public Transform piece;
            public Grabbable grabbable;
            public FractionNotationView label;
            public Vector3 trayPosition;
            [System.NonSerialized] public bool held;
        }

        public Transform stationRoot;
        public Transform ruler;
        public float restHeight = 0.08f;
        public PieceView whole;
        public PieceView halfA;
        public PieceView halfB;
        public GameObject chapterObjects;
        public GameObject chapterButtons;
        public Button splitButton;
        public Button submitButton;
        public Button resetButton;
        public TMP_Text heading;
        public TMP_Text body;
        public GameObject[] hideWhileActive;

        readonly CargoLessonModel model = new CargoLessonModel();
        bool active;
        bool wholeAccepted;

        void Awake()
        {
            Subscribe(whole); Subscribe(halfA); Subscribe(halfB);
            SetActiveObjects(false);
        }

        void OnDestroy() { Unsubscribe(whole); Unsubscribe(halfA); Unsubscribe(halfB); }

        void Subscribe(PieceView view) { if (view?.grabbable != null) view.grabbable.WhenPointerEventRaised += evt => OnPointer(view, evt); }
        void Unsubscribe(PieceView view) { }

        public void Begin()
        {
            active = true;
            wholeAccepted = false;
            SetActiveObjects(true);
            foreach (var go in hideWhileActive) if (go != null) go.SetActive(false);
            PlaceInTray(whole); PlaceInTray(halfA); PlaceInTray(halfB);
            whole.piece.gameObject.SetActive(true);
            halfA.piece.gameObject.SetActive(false);
            halfB.piece.gameObject.SetActive(false);
            whole.label.ShowWhole(1);
            halfA.label.Show(1, 2); halfB.label.Show(1, 2);
            Refresh();
        }

        public void Exit()
        {
            active = false;
            SetActiveObjects(false);
        }

        void SetActiveObjects(bool on)
        {
            if (chapterObjects != null) chapterObjects.SetActive(on);
            if (chapterButtons != null) chapterButtons.SetActive(on);
        }

        bool AnyHeld => whole.held || halfA.held || halfB.held;
        public bool AnyPieceHeld => active && AnyHeld;

        void OnPointer(PieceView view, PointerEvent evt)
        {
            if (!active) return;
            if (evt.Type == PointerEventType.Select)
            {
                view.held = true;
                model.Undock(view.id);
            }
            else if (evt.Type == PointerEventType.Unselect || evt.Type == PointerEventType.Cancel)
            {
                view.held = false;
                StartCoroutine(SettleAfterRelease(view));
            }
        }

        IEnumerator SettleAfterRelease(PieceView view)
        {
            yield return null; // let the SDK finish transferring ownership before we move the piece
            if (!active || view.held) yield break;
            Vector3 local = stationRoot.InverseTransformPoint(view.piece.position);
            if (RulerLayout.InDockZone(ruler.localPosition, local) && model.Dock(view.id, false))
                SnapDocked();
            Refresh();
        }

        void SnapDocked()
        {
            foreach (var view in new[] { whole, halfA, halfB })
            {
                if (view.piece == null || !model.IsDocked(view.id)) continue;
                var piece = model.Piece(view.id);
                view.piece.localPosition = RulerLayout.SnapPosition(ruler.localPosition, model.StartCell(view.id), piece.Cells, restHeight);
                view.piece.localRotation = Quaternion.identity;
            }
        }

        public void Split()
        {
            if (!active) return;
            if (AnyHeld) { body.text = "Release the strap before splitting."; return; }
            if (!model.Split(false, model.Generation)) return;
            whole.piece.gameObject.SetActive(false);
            halfA.piece.gameObject.SetActive(true);
            halfB.piece.gameObject.SetActive(true);
            PlaceInTray(halfA); PlaceInTray(halfB);
            Refresh(model.LastFeedback);
        }

        public void Submit()
        {
            if (!active) return;
            if (AnyHeld) { body.text = "Release the piece, then submit."; return; }
            bool ok = model.Submit();
            if (ok && model.Stage == CargoLessonStage.Whole) wholeAccepted = true;
            Refresh(model.LastFeedback);
        }

        public void ResetPieces()
        {
            if (!active || AnyHeld) return;
            model.ResetPieces();
            if (model.Stage == CargoLessonStage.Whole) PlaceInTray(whole); else { PlaceInTray(halfA); PlaceInTray(halfB); }
            Refresh();
        }

        void PlaceInTray(PieceView view)
        {
            if (view?.piece == null) return;
            view.piece.localPosition = view.trayPosition;
            view.piece.localRotation = Quaternion.identity;
        }

        void Refresh(string feedback = null)
        {
            heading.text = "Cargo Crew · Fractions";
            splitButton.gameObject.SetActive(model.Stage == CargoLessonStage.Whole && wholeAccepted);
            switch (model.Stage)
            {
                case CargoLessonStage.Whole:
                    body.text = wholeAccepted
                        ? "One whole strap: 1.\n\nThe next parcel needs half this length. Choose Split to cut the whole into two equal parts."
                        : "This strap is one whole, labeled 1. The ruler on the pad runs from 0 to 1.\n\nGrab the strap, lay it along the ruler, release, then choose Submit.";
                    break;
                case CargoLessonStage.Halves:
                    body.text = "Two equal parts. Each is one half: 1/2.\n\nMove the halves. Dock both on the ruler, side by side from 0, then Submit to rebuild the whole.";
                    break;
                case CargoLessonStage.Rebuilt:
                    body.text = "1/2 + 1/2 = 1. Two halves rebuild the whole strap.\n\nKeep moving the pieces to compare them with the ruler, or return to lessons.";
                    break;
            }
            if (!string.IsNullOrEmpty(feedback)) body.text += "\n\n" + feedback;
        }
    }
}
