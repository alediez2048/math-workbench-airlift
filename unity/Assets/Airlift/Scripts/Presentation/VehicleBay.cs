using System.Collections;
using System.Collections.Generic;
using Airlift.Lessons;
using UnityEngine;

namespace Airlift.Presentation
{
    /// Dock 7 toy vehicles. Each chapter's vehicles back up to the dock edge with their open beds side by side
    /// exactly over the container cells (the visible whole), rear toward the learner and cab pointing +z. When the
    /// model accepts a load the vehicles drive +z off the deck carrying the docked crates. Narrative only: it never
    /// decides whether a load is correct. No stars, timers or scores.
    public sealed class VehicleBay : MonoBehaviour
    {
        [System.Serializable]
        public sealed class Vehicle
        {
            public string kind;              // "truck" | "pickup" | "van"
            public int bedCells;             // bed width in container cells: truck 8, pickup 4, van 2
            public Transform root;
            public GameObject loadedTag;
        }

        public Vehicle[] vehicles = new Vehicle[0];
        [Tooltip("The container ruler; bed positions are computed from its cells.")]
        public Transform ruler;
        [Tooltip("Station-local height of the vehicles' wheel contact (deck top).")]
        public float parkHeight = 0.0175f;
        [Tooltip("How far vehicles drive +z (bay space) when a load is accepted; enough to leave the deck.")]
        public float driveDistance = 0.45f;
        public float driveSeconds = 1.5f;
        [Tooltip("Cells of the container not used by the chapter show these faded outlines (index = cell).")]
        public GameObject[] unusedCells = new GameObject[0];
        public GameObject notNeededLabel;
        [Tooltip("Outside the lesson one vehicle of this kind stays parked at idlePosition (bay space).")]
        public string idleKind = "truck";
        public Vector3 idlePosition = new Vector3(-0.49f, 0.0175f, -0.02f);

        readonly List<Vehicle> current = new List<Vehicle>();
        Coroutine driving;

        public IReadOnlyList<Vehicle> Current => current;
        public bool DrivenAway { get; private set; }
        public int CountOf(string kind) { int n = 0; foreach (var v in vehicles) if (v != null && v.kind == kind) n++; return n; }
        public int CountOf(string kind, int bedCells) { int n = 0; foreach (var v in vehicles) if (v != null && v.kind == kind && v.bedCells == bedCells) n++; return n; }

        /// Bay-space position that puts a vehicle's bed centre over the given run of container cells.
        public Vector3 ParkPosition(int startCell, int cells)
        {
            if (ruler == null) return Vector3.zero;
            var rulerLocal = ruler.localPosition;
            var inRulerParent = new Vector3(RulerLayout.CellsCenterX(rulerLocal, startCell, cells), parkHeight, rulerLocal.z);
            Vector3 world = ruler.parent != null ? ruler.parent.TransformPoint(inRulerParent) : inRulerParent;
            return transform.InverseTransformPoint(world);
        }

        /// Parks this chapter's vehicles with their beds aligned to the chapter's BedCells, left to right from cell 0.
        public void ShowChapter(CargoChapter chapter)
        {
            if (chapter == null) { ResetBay(); return; }
            StopDriving();
            current.Clear();
            foreach (var v in vehicles) Park(v, false, Vector3.zero);
            var beds = chapter.BedCells;
            if (beds == null || beds.Length == 0) beds = DefaultBeds(chapter);
            int start = 0;
            foreach (int cells in beds)
            {
                var v = Pick(chapter.VehicleKind, cells);
                if (v == null) { start += cells; continue; }
                current.Add(v);
                Park(v, true, ParkPosition(start, cells));
                start += cells;
            }
            ShowUnused(start);
        }

        /// Outside the lesson: everything parked away, one idle vehicle in its old bay.
        public void ResetBay()
        {
            StopDriving();
            current.Clear();
            foreach (var v in vehicles) Park(v, false, Vector3.zero);
            ShowUnused(PlacementState.CellsPerWhole);
            foreach (var v in vehicles)
                if (v != null && v.root != null && v.kind == idleKind) { Park(v, true, idlePosition); break; }
        }

        /// Accepted load: tags on, then the chapter's vehicles drive +z off the deck and the crates ride along.
        /// Everything that drove away is hidden at the end; ShowChapter or ResetBay restores the vehicles and the
        /// lesson director restores the crates.
        public void DriveAway(IList<Transform> cargo)
        {
            StopDriving();
            var movers = new List<Transform>();
            foreach (var v in current)
            {
                if (v?.root == null) continue;
                if (v.loadedTag != null) v.loadedTag.SetActive(true);
                movers.Add(v.root);
            }
            if (cargo != null) foreach (var t in cargo) if (t != null && !movers.Contains(t)) movers.Add(t);
            DrivenAway = true;
            Vector3 delta = transform.TransformVector(Vector3.forward * driveDistance);
            var starts = new Vector3[movers.Count];
            for (int i = 0; i < movers.Count; i++) starts[i] = movers[i].position;
            if (!Application.isPlaying || !isActiveAndEnabled || driveSeconds <= 0f) { Finish(movers, starts, delta); return; }
            driving = StartCoroutine(Drive(movers, starts, delta));
        }

        IEnumerator Drive(List<Transform> movers, Vector3[] starts, Vector3 delta)
        {
            yield return new WaitForSeconds(0.3f); // let the LOADED tags read first
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / driveSeconds);
                float eased = t * t; // pull away gently, then speed up
                for (int i = 0; i < movers.Count; i++) if (movers[i] != null) movers[i].position = starts[i] + delta * eased;
                yield return null;
            }
            driving = null;
            Finish(movers, starts, delta);
        }

        static void Finish(List<Transform> movers, Vector3[] starts, Vector3 delta)
        {
            for (int i = 0; i < movers.Count; i++)
            {
                if (movers[i] == null) continue;
                movers[i].position = starts[i] + delta;
                movers[i].gameObject.SetActive(false);
            }
        }

        Vehicle Pick(string kind, int cells)
        {
            foreach (var v in vehicles) if (v?.root != null && v.kind == kind && v.bedCells == cells && !current.Contains(v)) return v;
            foreach (var v in vehicles) if (v?.root != null && v.kind == kind && !current.Contains(v)) return v;
            return null;
        }

        static void Park(Vehicle v, bool on, Vector3 position)
        {
            if (v?.root == null) return;
            if (on) { v.root.localPosition = position; v.root.localRotation = Quaternion.identity; }
            v.root.gameObject.SetActive(on);
            if (v.loadedTag != null) v.loadedTag.SetActive(false);
        }

        void ShowUnused(int usedCells)
        {
            bool any = false;
            if (unusedCells != null)
                for (int i = 0; i < unusedCells.Length; i++)
                {
                    if (unusedCells[i] == null) continue;
                    bool unused = i >= usedCells && usedCells < PlacementState.CellsPerWhole;
                    unusedCells[i].SetActive(unused); any |= unused;
                }
            if (notNeededLabel != null)
            {
                notNeededLabel.SetActive(any);
                if (any && ruler != null)
                {
                    var p = notNeededLabel.transform.localPosition;
                    p.x = RulerLayout.CellsCenterX(Vector3.zero, usedCells, PlacementState.CellsPerWhole - usedCells);
                    notNeededLabel.transform.localPosition = p;
                }
            }
        }

        static int[] DefaultBeds(CargoChapter chapter)
        {
            int count = Mathf.Max(1, chapter.VehicleCount);
            int cells = PlacementState.CellsPerWhole / count;
            var beds = new int[count];
            for (int i = 0; i < count; i++) beds[i] = cells;
            return beds;
        }

        void StopDriving()
        {
            if (driving != null) StopCoroutine(driving);
            driving = null;
            DrivenAway = false;
        }
    }
}
