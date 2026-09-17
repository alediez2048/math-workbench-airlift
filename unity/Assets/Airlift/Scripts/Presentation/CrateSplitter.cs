using Airlift.Lessons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Airlift.Presentation
{
    /// The Dock 7 splitter station: a pad for one crate and two buttons, Halves · 1/2 and Quarters · 1/4. A choice is
    /// decided by SplitterRules; the chapter's size splits through CargoLessonDirector.TrySplit (the same path as the
    /// voice tool split_cargo), so the director lays the chunks out in the tray. Visible whenever a chapter is on the
    /// table, including while the guide listens (it is never in a fallback button group).
    public sealed class CrateSplitter : MonoBehaviour
    {
        public CargoLessonDirector lesson;
        public Transform stationRoot;
        [Tooltip("Station-local centre of the pad's top surface.")]
        public Vector3 padCenter;
        [Tooltip("Pad footprint in metres: x and z.")]
        public Vector2 padSize = new Vector2(0.30f, 0.14f);
        [Tooltip("Pad and console; hidden while no chapter is active (the concept intro).")]
        public GameObject visuals;
        public Button halvesButton;
        public Button quartersButton;
        public TMP_Text feedback;

        int shownChapter = -1;

        /// A loose lesson crate (active, not loaded, not locked) has its centre over the pad.
        public bool CrateOnPad
        {
            get
            {
                if (lesson == null) return false;
                var root = stationRoot != null ? stationRoot : lesson.stationRoot;
                if (root == null) return false;
                foreach (var view in new[] { lesson.whole, lesson.halfA, lesson.halfB }) if (Over(root, view)) return true;
                if (lesson.quarters != null) foreach (var view in lesson.quarters) if (Over(root, view)) return true;
                return false;
            }
        }

        bool Over(Transform root, CargoLessonDirector.PieceView view) =>
            view?.piece != null && view.piece.gameObject.activeInHierarchy && lesson.IsLoosePiece(view.id)
            && SplitterRules.OverPad(padCenter, padSize, root.InverseTransformPoint(view.piece.position));

        public void ChooseHalves() => Choose(2);
        public void ChooseQuarters() => Choose(4);

        public LessonActionResult Choose(int denominator)
        {
            if (lesson == null || !lesson.IsActive || lesson.Chapter == null) return Show(new LessonActionResult(false, CargoLessonDirector.ClosedReason));
            var chapter = lesson.Chapter;
            var rule = SplitterRules.Choose(chapter.SplitTo, denominator, CrateOnPad, lesson.CanSplit, chapter.VehicleKind);
            return Show(rule.Ok ? lesson.TrySplit() : rule);
        }

        LessonActionResult Show(LessonActionResult result)
        {
            if (feedback != null) feedback.text = result.Reason;
            return result;
        }

        void Update()
        {
            bool on = lesson != null && lesson.IsActive;
            if (visuals != null && visuals.activeSelf != on) visuals.SetActive(on);
            int chapter = on && lesson.Chapter != null ? lesson.Chapter.Number : 0;
            if (chapter == shownChapter) return;
            shownChapter = chapter;
            if (feedback != null) feedback.text = "";
        }
    }
}
