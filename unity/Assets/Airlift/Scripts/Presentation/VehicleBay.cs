using System.Collections;
using System.Collections.Generic;
using Airlift.Lessons;
using UnityEngine;

namespace Airlift.Presentation
{
    /// Dock 7 toy vehicles. Each chapter's vehicles back up to the dock with their open beds side by side exactly over
    /// the container cells (the visible whole), rear toward the learner and cab pointing +z. When the model accepts a
    /// load the vehicles leave one by one, leftmost first: each turns left into the exit lane and drives out through
    /// the DOCK EXIT gate on the left, carrying its crates. Narrative only: it never decides whether a load is correct.
    /// No stars, timers or scores.
    public sealed class VehicleBay : MonoBehaviour
    {
        [System.Serializable]
        public sealed class Vehicle
        {
            public string kind;              // "truck" | "pickup" | "van"
            public int bedCells;             // bed width in container cells: truck 8, pickup 4, van 2
            public Transform root;
            public GameObject loadedTag;
            [Tooltip("Root-local z of the front bumper (the rear bumper is at -0.045).")]
            public float noseZ = 0.12f;
        }

        /// Where a departing vehicle is at a moment: bay-space position, yaw in degrees (0 = parked facing +z,
        /// -90 = facing the exit on the left), and whether it has already left through the exit.
        public struct Pose { public Vector3 position; public float yaw; public bool gone; }

        public Vehicle[] vehicles = new Vehicle[0];
        [Tooltip("The container ruler; bed positions are computed from its cells.")]
        public Transform ruler;
        [Tooltip("Station-local height of the vehicles' wheel contact (deck top).")]
        public float parkHeight = 0.0175f;
        [Header("Departure through the dock exit")]
        [Tooltip("Bay-space z of the exit lane, between the crate tray and the cranes.")]
        public float laneZ = -0.05f;
        [Tooltip("Bay-space x where a departing vehicle has fully left the deck and disappears.")]
        public float exitX = -0.75f;
        [Tooltip("How far a vehicle moves left while it turns, so its rear swing clears the neighbour.")]
        public float turnSlide = 0.09f;
        public float turnSeconds = 0.8f;
        [Tooltip("Metres per second along the lane.")]
        public float driveSpeed = 0.4f;
        [Tooltip("Seconds between vehicles leaving, leftmost first.")]
        public float departGap = 0.9f;
        [Tooltip("Seconds the LOADED tags show before the first vehicle moves.")]
        public float tagSeconds = 0.4f;
        [Tooltip("Cells of the container not used by the chapter show these faded outlines (index = cell).")]
        public GameObject[] unusedCells = new GameObject[0];
        public GameObject notNeededLabel;
        [Tooltip("Outside the lesson one vehicle of this kind stays parked at idlePosition (bay space) facing idleYaw.")]
        public string idleKind = "truck";
        public Vector3 idlePosition = new Vector3(0.12f, 0.0175f, 0.25f);
        public float idleYaw = -90f;

        readonly List<Vehicle> current = new List<Vehicle>();
        readonly Dictionary<Vehicle, Vector3> parkedAt = new Dictionary<Vehicle, Vector3>();
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
            current.Clear(); parkedAt.Clear();
            foreach (var v in vehicles) Park(v, false, Vector3.zero, 0f);
            var beds = chapter.BedCells;
            if (beds == null || beds.Length == 0) beds = DefaultBeds(chapter);
            int start = 0;
            foreach (int cells in beds)
            {
                var v = Pick(chapter.VehicleKind, cells);
                if (v == null) { start += cells; continue; }
                current.Add(v);
                var at = ParkPosition(start, cells);
                parkedAt[v] = at;
                Park(v, true, at, 0f);
                start += cells;
            }
            ShowUnused(start);
        }

        /// Outside the lesson: everything parked away, one idle vehicle waiting at the back of the dock.
        public void ResetBay()
        {
            StopDriving();
            current.Clear(); parkedAt.Clear();
            foreach (var v in vehicles) Park(v, false, Vector3.zero, 0f);
            ShowUnused(PlacementState.CellsPerWhole);
            foreach (var v in vehicles)
                if (v != null && v.root != null && v.kind == idleKind) { Park(v, true, idlePosition, idleYaw); break; }
        }

        /// Seconds after DriveAway at which this vehicle starts to move (leftmost vehicle first).
        public float DepartureStart(Vehicle vehicle)
        {
            int order = 0;
            float x = ParkedAt(vehicle).x;
            foreach (var other in current) if (other != vehicle && ParkedAt(other).x < x) order++;
            return tagSeconds + order * departGap;
        }

        /// Seconds after DriveAway until the last vehicle has left through the exit.
        public float DepartureSeconds
        {
            get
            {
                float longest = 0f;
                foreach (var v in current) longest = Mathf.Max(longest, DepartureStart(v) + TravelSeconds(ParkedAt(v)));
                return longest;
            }
        }

        /// Pure pose of a departing vehicle t seconds after DriveAway: parked, then a left turn into the lane while
        /// sliding left, then straight along the lane until it passes exitX.
        public Pose DeparturePose(Vehicle vehicle, Vector3 parked, float t)
        {
            float local = t - DepartureStart(vehicle);
            if (local <= 0f) return new Pose { position = parked, yaw = 0f };
            if (local < turnSeconds)
            {
                // The slide leads the turn so the rear's outward swing never reaches the neighbour still parked beside it.
                float u = local / turnSeconds;
                float slide = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(u / 0.6f));
                float turn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((u - 0.2f) / 0.8f));
                return new Pose { position = new Vector3(parked.x - turnSlide * slide, parked.y, Mathf.Lerp(parked.z, laneZ, turn)), yaw = -90f * turn };
            }
            float x = parked.x - turnSlide - driveSpeed * (local - turnSeconds);
            bool gone = x <= exitX;
            return new Pose { position = new Vector3(gone ? exitX : x, parked.y, laneZ), yaw = -90f, gone = gone };
        }

        /// Accepted load: tags on, then the chapter's vehicles leave through the dock exit and the crates ride along in
        /// their beds. Everything that left is hidden at the end; ShowChapter or ResetBay restores the vehicles and the
        /// lesson director restores the crates.
        public void DriveAway(IList<Transform> cargo)
        {
            StopDriving();
            var riders = Board(cargo);
            DrivenAway = true;
            if (!Application.isPlaying || !isActiveAndEnabled || driveSpeed <= 0f) { Apply(DepartureSeconds + 0.01f, riders); return; }
            driving = StartCoroutine(Drive(riders));
        }

        /// Editor previews: pose the chapter's vehicles and their crates as they are the given seconds into the departure.
        public void ShowDepartureAt(IList<Transform> cargo, float seconds) { StopDriving(); Apply(seconds, Board(cargo)); }

        sealed class Rider { public Transform cargo; public Vehicle vehicle; public Vector3 offset; public Quaternion rotation; }

        List<Rider> Board(IList<Transform> cargo)
        {
            var riders = new List<Rider>();
            foreach (var v in current)
                if (v?.root != null && v.loadedTag != null) v.loadedTag.SetActive(true);
            if (cargo != null)
                foreach (var t in cargo)
                {
                    var carrier = CarrierOf(t);
                    if (t == null || carrier == null) continue;
                    riders.Add(new Rider { cargo = t, vehicle = carrier, offset = carrier.root.InverseTransformPoint(t.position), rotation = Quaternion.Inverse(carrier.root.rotation) * t.rotation });
                }
            return riders;
        }

        IEnumerator Drive(List<Rider> riders)
        {
            float t = 0f, end = DepartureSeconds + 0.01f;
            while (t < end)
            {
                Apply(t, riders);
                yield return null;
                t += Time.deltaTime;
            }
            driving = null;
            Apply(end, riders);
        }

        void Apply(float t, List<Rider> riders)
        {
            foreach (var v in current)
            {
                if (v?.root == null) continue;
                var pose = DeparturePose(v, ParkedAt(v), t);
                v.root.localPosition = pose.position;
                v.root.localRotation = Quaternion.Euler(0f, pose.yaw, 0f);
                if (pose.gone && v.root.gameObject.activeSelf) v.root.gameObject.SetActive(false);
            }
            foreach (var r in riders)
            {
                if (r.cargo == null) continue;
                r.cargo.position = r.vehicle.root.TransformPoint(r.offset);
                r.cargo.rotation = r.vehicle.root.rotation * r.rotation;
                if (!r.vehicle.root.gameObject.activeSelf && r.cargo.gameObject.activeSelf) r.cargo.gameObject.SetActive(false);
            }
        }

        /// The vehicle whose bed the crate sits over (nearest bed centre along the dock when between beds).
        Vehicle CarrierOf(Transform cargo)
        {
            if (cargo == null) return null;
            Vehicle best = null; float bestDistance = float.MaxValue;
            foreach (var v in current)
            {
                if (v?.root == null) continue;
                float d = Mathf.Abs(v.root.InverseTransformPoint(cargo.position).x);
                if (d < bestDistance) { bestDistance = d; best = v; }
            }
            return best;
        }

        Vector3 ParkedAt(Vehicle v) => v != null && parkedAt.TryGetValue(v, out var p) ? p : (v?.root != null ? v.root.localPosition : Vector3.zero);

        float TravelSeconds(Vector3 parked) => turnSeconds + Mathf.Max(0f, parked.x - turnSlide - exitX) / Mathf.Max(0.001f, driveSpeed);

        Vehicle Pick(string kind, int cells)
        {
            foreach (var v in vehicles) if (v?.root != null && v.kind == kind && v.bedCells == cells && !current.Contains(v)) return v;
            foreach (var v in vehicles) if (v?.root != null && v.kind == kind && !current.Contains(v)) return v;
            return null;
        }

        static void Park(Vehicle v, bool on, Vector3 position, float yaw)
        {
            if (v?.root == null) return;
            if (on) { v.root.localPosition = position; v.root.localRotation = Quaternion.Euler(0f, yaw, 0f); }
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
