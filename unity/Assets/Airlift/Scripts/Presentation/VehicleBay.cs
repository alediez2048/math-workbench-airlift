using System.Collections;
using System.Collections.Generic;
using Airlift.Lessons;
using UnityEngine;

namespace Airlift.Presentation
{
    /// Dock 7 toy vehicles. Narrative payoff only: it never decides whether a load is correct. The lesson
    /// director calls MarkLoaded only after the model accepted a load. No stars, timers or scores.
    public sealed class VehicleBay : MonoBehaviour
    {
        [System.Serializable]
        public sealed class Vehicle
        {
            public string kind;              // "truck" | "pickup" | "van"
            public Transform root;
            public GameObject loadedTag;
            [Tooltip("Roll distance for this vehicle; 0 uses the bay's rollDistance.")]
            public float distance;
            [System.NonSerialized] public Vector3 home;
            [System.NonSerialized] public bool homeKnown;
            [System.NonSerialized] public bool rolledOut;
        }

        public Vehicle[] vehicles = new Vehicle[0];
        [Tooltip("How far a loaded vehicle rolls along its own forward (cab) direction, in station metres.")]
        public float rollDistance = 0.12f;
        public float rollSeconds = 1f;
        [Tooltip("Vehicle kind shown when no lesson is open.")]
        public string idleKind = "truck";

        readonly List<Vehicle> current = new List<Vehicle>();
        readonly Dictionary<Vehicle, Coroutine> rolling = new Dictionary<Vehicle, Coroutine>();

        public IReadOnlyList<Vehicle> Current => current;
        public int CountOf(string kind) { int n = 0; foreach (var v in vehicles) if (v != null && v.kind == kind) n++; return n; }

        void Awake() { RememberHomes(); }

        void RememberHomes()
        {
            foreach (var v in vehicles)
                if (v != null && v.root != null && !v.homeKnown) { v.home = v.root.localPosition; v.homeKnown = true; }
        }

        /// Shows only this chapter's vehicles, parked in their bays without tags.
        public void ShowChapter(CargoChapter chapter)
        {
            if (chapter == null) { ResetBay(); return; }
            Show(chapter.VehicleKind, chapter.VehicleCount);
        }

        /// The idle look outside the lesson: the original truck parked.
        public void ResetBay() => Show(idleKind, 1);

        void Show(string kind, int count)
        {
            RememberHomes();
            StopRolling();
            current.Clear();
            int shown = 0;
            foreach (var v in vehicles)
            {
                if (v == null || v.root == null) continue;
                bool on = v.kind == kind && shown < count;
                if (on) { shown++; current.Add(v); }
                v.root.gameObject.SetActive(on);
                v.root.localPosition = v.home;
                v.rolledOut = false;
                if (v.loadedTag != null) v.loadedTag.SetActive(false);
            }
        }

        /// Tags the current chapter's vehicles LOADED and rolls them a short way out of their bays.
        public void MarkLoaded()
        {
            foreach (var v in current) RollOut(v);
        }

        /// After the last chapter: every vehicle appears, gets its tag and rolls out.
        public void DispatchAll()
        {
            RememberHomes();
            foreach (var v in vehicles)
            {
                if (v == null || v.root == null) continue;
                if (!v.root.gameObject.activeSelf) { v.root.localPosition = v.home; v.root.gameObject.SetActive(true); }
                if (!current.Contains(v)) current.Add(v);
                RollOut(v);
            }
        }

        void RollOut(Vehicle v)
        {
            if (v == null || v.root == null || v.rolledOut) return;
            v.rolledOut = true;
            if (v.loadedTag != null) v.loadedTag.SetActive(true);
            Vector3 end = RolledPosition(v);
            if (!Application.isPlaying || !isActiveAndEnabled || rollSeconds <= 0f) { v.root.localPosition = end; return; }
            rolling[v] = StartCoroutine(Roll(v, end));
        }

        /// Home position moved away from the learner (+z in the bay's parent space, toward the ship and
        /// containers), whatever way the toy is parked. Rolling toward the learner read as a vehicle driving at them.
        public Vector3 RolledPosition(Vehicle v)
        {
            Vector3 forward = Vector3.forward;
            return v.home + forward.normalized * (v.distance > 0f ? v.distance : rollDistance);
        }

        IEnumerator Roll(Vehicle v, Vector3 end)
        {
            Vector3 start = v.root.localPosition;
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / rollSeconds);
                float eased = t * t * (3f - 2f * t);
                v.root.localPosition = Vector3.Lerp(start, end, eased);
                yield return null;
            }
            rolling.Remove(v);
        }

        void StopRolling()
        {
            foreach (var pair in rolling) if (pair.Value != null) StopCoroutine(pair.Value);
            rolling.Clear();
        }
    }
}
