using System;
using System.Collections.Generic;
using UnityEngine;

namespace Airlift.Lounge
{
    /// The lounge toy rack: fifteen slots in ToyRackModel order. Locked toys are grey silhouettes and cannot be grabbed;
    /// unlocked toys are in colour and grabbable; newly unlocked toys glow until first picked up. Placed beside the
    /// whiteboard once the board is placed (PlaceBeside), so it is always to the learner's left of it.
    public sealed class ToyRack : MonoBehaviour
    {
        [Serializable]
        public sealed class Slot
        {
            public string id; public Transform anchor; public GameObject toy; public ToyPlace place; public ToyLockedHint hint;
            public Renderer[] renderers; public Material[] colourMaterials; public Material lockedMaterial; public Material glowMaterial;
        }
        public Slot[] slots = new Slot[0];
        [Tooltip("Distance from the board's centre to the rack's centre, along the board's left.")]
        public float besideBoard = 1.75f;
        public event Action<ToySlot> ToyFirstGrabbed;
        /// A toy was let go somewhere: its lounge-local pose and size are in the library; the director saves.
        public event Action<ToySlot> ToyMoved;
        /// The learner pointed at (or pulled the trigger on) a toy that is still locked.
        public event Action<ToySlot> LockedToyPointed;
        LibraryState library;
        Transform Room { get { var l = GetComponentInParent<LoungeRoom>(true); return l != null ? l.transform : null; } }

        void Awake()
        {
            foreach (var s in slots)
            {
                if (s.place != null) { s.place.FirstGrab = _ => OnFirstGrab(s); s.place.Released = _ => OnReleased(s); }
                if (s.hint != null) s.hint.Pointed = _ => { var m = FindModel(s.id); if (m != null) LockedToyPointed?.Invoke(m); };
            }
        }

        void OnReleased(Slot s)
        {
            var model = FindModel(s.id); if (model == null || library == null || s.toy == null) return;
            var room = Room; var t = s.toy.transform;
            var pos = room != null ? room.InverseTransformPoint(t.position) : t.position;
            var rot = room != null ? Quaternion.Inverse(room.rotation) * t.rotation : t.rotation;
            library.RecordToyPose(s.id, pos, rot, t.localScale.x);
            ToyMoved?.Invoke(model);
        }

        void ApplySavedPose(Slot s)
        {
            if (s.place == null) return;
            var saved = library?.ToyPose(s.id);
            if (saved == null) { s.place.Apply(null, null, null); return; }
            var room = Room;
            var pos = room != null ? room.TransformPoint(saved.Value.position) : saved.Value.position;
            var rot = room != null ? room.rotation * saved.Value.rotation : saved.Value.rotation;
            s.place.Apply(pos, rot, saved.Value.scale);
        }

        /// Owner 2026-09-29: against a windowless wall, the nearest one on the learner's left of the board. The lounge
        /// shell sits at the room origin; the pose is in that space.
        public void PlaceBeside(Transform board)
        {
            if (board == null) return;
            float angle = ToyRackPlacement.SolidWallAngle(ToyRackPlacement.PreferredAngle(board.position, board.forward, besideBoard));
            var (pos, rot) = ToyRackPlacement.Pose(angle);
            var lounge = GetComponentInParent<LoungeRoom>(true);                 // the shell's ring is centred on the lounge root
            var room = lounge != null ? lounge.transform : null;
            if (room != null) { pos = room.TransformPoint(pos); rot = room.rotation * rot; }
            transform.SetPositionAndRotation(pos, rot);
            foreach (var s in slots) ApplySavedPose(s);
        }

        public void Refresh(LibraryState lib)
        {
            library = lib;
            foreach (var s in slots)
            {
                var model = FindModel(s.id); if (model == null || s.toy == null) continue;
                bool unlocked = lib != null && lib.IsCompleted(model.LessonId, model.Chapter);
                bool fresh = unlocked && !lib.IsToySeen(model.LessonId, model.Chapter);
                ApplyLook(s, unlocked, fresh);
                SetGrabbable(s, unlocked);
                if (unlocked) ApplySavedPose(s); else s.place?.SnapHome();
            }
        }

        public IReadOnlyList<ToySlot> NewlyUnlocked() => ToyRackModel.NewlyUnlocked(library);

        void OnFirstGrab(Slot s)
        {
            var model = FindModel(s.id); if (model == null) return;
            library?.MarkToySeen(model.LessonId, model.Chapter);
            ApplyLook(s, true, false);
            ToyFirstGrabbed?.Invoke(model);
        }

        static ToySlot FindModel(string id) { foreach (var m in ToyRackModel.Slots) if (m.Id == id) return m; return null; }

        static void ApplyLook(Slot s, bool unlocked, bool fresh)
        {
            if (s.renderers == null) return;
            for (int i = 0; i < s.renderers.Length; i++)
            {
                var r = s.renderers[i]; if (r == null) continue;
                var colour = s.colourMaterials != null && i < s.colourMaterials.Length ? s.colourMaterials[i] : null;
                r.sharedMaterial = !unlocked ? (s.lockedMaterial != null ? s.lockedMaterial : colour) : fresh && s.glowMaterial != null ? s.glowMaterial : colour;
            }
        }

        /// Locked: the collider and the ray interactable stay live so the toy can explain itself; every grab is off.
        static void SetGrabbable(Slot s, bool on)
        {
            if (s.hint != null) s.hint.Locked = !on;
            var g = s.toy.GetComponentInChildren<Oculus.Interaction.Grabbable>(true); if (g != null) g.enabled = on;
            foreach (var i in s.toy.GetComponentsInChildren<Oculus.Interaction.GrabInteractable>(true)) i.enabled = on;
            foreach (var i in s.toy.GetComponentsInChildren<Oculus.Interaction.HandGrab.HandGrabInteractable>(true)) i.enabled = on;
            foreach (var i in s.toy.GetComponentsInChildren<Oculus.Interaction.DistanceGrabInteractable>(true)) i.enabled = on;
            foreach (var i in s.toy.GetComponentsInChildren<Oculus.Interaction.HandGrab.DistanceHandGrabInteractable>(true)) i.enabled = on;
        }
    }
}
