using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Airlift.Presentation.Cafe
{
    /// A plate or box and the pastries on it; they move together during a payoff. Its "Plate N" / "Box N" deck label
    /// is hidden once the container has left, so no label stays on an empty spot.
    public sealed class CafeRideGroup
    {
        public Transform container;
        public Transform label;
        public readonly List<Transform> items = new List<Transform>();
    }

    /// Corner Café payoff, narrative only: it never decides whether an order is correct. Accepted plates slide onto
    /// the guest table (lined up by CafeLayout.ServedCenter, at serveScale) and steam rises from the cups; accepted
    /// full boxes show ORDER UP, load onto the delivery bike's rack and ride off the deck. Outside Play mode (tests, editor previews) every motion jumps to its end.
    /// The station restores containers and pastries; ResetAll restores the bike and the steam.
    public sealed class CafePayoff : MonoBehaviour
    {
        [Tooltip("Café root: the station-local space every pose is expressed in.")]
        public Transform space;
        [Header("Serve plates")]
        public float serveScale = CafeLayout.ServeScale;
        public float serveSeconds = 1.1f;
        public GameObject[] steamPuffs = new GameObject[0];
        [Header("Ship boxes")]
        public Transform bike;
        public Vector3 bikeParked = CafeLayout.BikeParked;
        [Tooltip("Bike-local centre of the rack top where boxes ride, two across and two high.")]
        public Vector3 rackCenter = CafeLayout.RackCenter;
        public float rackScale = CafeLayout.RackScale;
        public float rackColumnOffset = CafeLayout.RackColumnOffset;
        public float rackLayerHeight = CafeLayout.RackLayerHeight;
        public float tagSeconds = 0.5f, loadSeconds = 0.8f, rideSeconds = 1.4f;
        public float rideDistance = 0.55f;
        [Tooltip("Editor previews only: stop with the boxes on the parked bike instead of riding away.")]
        public bool holdOnRack;

        public bool Busy { get; private set; }
        public bool Served { get; private set; }
        public bool Shipped { get; private set; }

        Coroutine motion, steam;
        Action finish;
        Vector3[] puffBase;

        Transform Space => space != null ? space : transform;

        // ---- serve ----
        public void Serve(IList<CafeRideGroup> groups, Action done)
        {
            FinishNow();
            var list = Valid(groups);
            var from = Capture(list);
            Served = true;
            Run(ServeRoutine(list, from), () =>
            {
                ApplyServe(list, from, 1f);
                HideLabels(list);
                SetSteam(true);
                done?.Invoke();
            });
        }

        IEnumerator ServeRoutine(List<CafeRideGroup> groups, Pose[][] from)
        {
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / Mathf.Max(0.01f, serveSeconds));
                ApplyServe(groups, from, t * t * (3f - 2f * t));
                yield return null;
            }
        }

        /// Blend each plate from its counter pose to its place on the guest table; pastries keep their offsets.
        void ApplyServe(List<CafeRideGroup> groups, Pose[][] from, float t)
        {
            for (int g = 0; g < groups.Count; g++)
                Blend(groups[g], from[g], Space.TransformPoint(CafeLayout.ServedCenter(g, groups.Count)), serveScale, t);
        }

        // ---- ship ----
        public void Ship(IList<CafeRideGroup> groups, IList<GameObject> tags, Action done)
        {
            FinishNow();
            if (tags != null) foreach (var tag in tags) if (tag != null) tag.SetActive(true);
            var list = Valid(groups);
            var from = Capture(list);
            Shipped = true;
            Run(ShipRoutine(list, from), () =>
            {
                ApplyPose(list, from, 1f);
                HideLabels(list);
                if (!holdOnRack && bike != null)
                {
                    Vector3 delta = Space.TransformVector(Vector3.forward * rideDistance);
                    bike.position += delta;
                    bike.gameObject.SetActive(false);
                    foreach (var g in list)
                    {
                        g.container.position += delta; g.container.gameObject.SetActive(false);
                        foreach (var item in g.items) if (item != null) { item.position += delta; item.gameObject.SetActive(false); }
                    }
                }
                done?.Invoke();
            });
        }

        IEnumerator ShipRoutine(List<CafeRideGroup> groups, Pose[][] from)
        {
            yield return new WaitForSeconds(tagSeconds);   // let ORDER UP read first
            HideLabels(groups);                              // the boxes leave their spots now
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / Mathf.Max(0.01f, loadSeconds));
                ApplyPose(groups, from, t * t * (3f - 2f * t));
                yield return null;
            }
            if (holdOnRack || bike == null) yield break;
            var movers = new List<Transform> { bike };
            foreach (var g in groups) { movers.Add(g.container); movers.AddRange(g.items); }
            var starts = new Vector3[movers.Count];
            for (int i = 0; i < movers.Count; i++) if (movers[i] != null) starts[i] = movers[i].position;
            Vector3 delta = Space.TransformVector(Vector3.forward * rideDistance);
            t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.deltaTime / Mathf.Max(0.01f, rideSeconds));
                for (int i = 0; i < movers.Count; i++) if (movers[i] != null) movers[i].position = starts[i] + delta * (t * t);
                yield return null;
            }
            // The finish action re-applies the rack pose and adds the ride delta, so park everything back first.
            for (int i = 0; i < movers.Count; i++) if (movers[i] != null) movers[i].position = starts[i];
        }

        /// Start positions of every container (index 0) and its pastries (index 1..n), per group.
        static Pose[][] Capture(List<CafeRideGroup> groups)
        {
            var poses = new Pose[groups.Count][];
            for (int g = 0; g < groups.Count; g++)
            {
                var group = groups[g];
                poses[g] = new Pose[group.items.Count + 1];
                poses[g][0] = new Pose(group.container.position, Quaternion.identity);
                for (int i = 0; i < group.items.Count; i++)
                    poses[g][i + 1] = new Pose(group.items[i] != null ? group.items[i].position : Vector3.zero, Quaternion.identity);
            }
            return poses;
        }

        /// Blend each group from its start pose to its rack slot: container at the slot, pastries keep their offsets
        /// from the container scaled down, everything scaled by rackScale.
        void ApplyPose(List<CafeRideGroup> groups, Pose[][] from, float t)
        {
            if (bike == null) return;
            for (int g = 0; g < groups.Count; g++) Blend(groups[g], from[g], bike.TransformPoint(RackSlot(g)), rackScale, t);
        }

        /// Moves a container from its start pose toward target (world) and scales it toward endScale; its pastries keep
        /// their offsets from the container, scaled with it.
        static void Blend(CafeRideGroup group, Pose[] from, Vector3 target, float endScale, float t)
        {
            float scale = Mathf.Lerp(1f, endScale, t);
            Vector3 start = from[0].position;
            Vector3 pos = Vector3.Lerp(start, target, t);
            group.container.position = pos;
            group.container.localScale = Vector3.one * scale;
            for (int i = 0; i < group.items.Count; i++)
            {
                var item = group.items[i]; if (item == null) continue;
                item.position = pos + (from[i + 1].position - start) * scale;
                item.localScale = Vector3.one * scale;
            }
        }

        static List<CafeRideGroup> Valid(IList<CafeRideGroup> groups)
        {
            var list = new List<CafeRideGroup>();
            if (groups != null) foreach (var g in groups) if (g?.container != null) list.Add(g);
            return list;
        }

        static void HideLabels(List<CafeRideGroup> groups)
        {
            foreach (var g in groups) if (g.label != null) g.label.gameObject.SetActive(false);
        }

        /// Bike-local rack slot for the index-th box: two across, then a second layer on top.
        public Vector3 RackSlot(int index)
        {
            int col = index % 2, layer = index / 2;
            return rackCenter + new Vector3(col == 0 ? -rackColumnOffset : rackColumnOffset, layer * rackLayerHeight, 0f);
        }

        // ---- control ----
        /// Jumps a running motion to its end state and runs its completion (e.g. before the next action).
        public void FinishNow()
        {
            if (motion != null) { StopCoroutine(motion); motion = null; }
            var f = finish; finish = null; Busy = false;
            f?.Invoke();
        }

        /// Stops everything without completing it: bike parked and visible, steam off. The station re-lays out
        /// containers and pastries afterwards.
        public void ResetAll()
        {
            if (motion != null) { StopCoroutine(motion); motion = null; }
            finish = null; Busy = false; Served = false; Shipped = false;
            SetSteam(false);
            if (bike != null)
            {
                bike.localPosition = bikeParked; bike.localRotation = Quaternion.identity; bike.localScale = Vector3.one;
                bike.gameObject.SetActive(true);
            }
        }

        void Run(IEnumerator routine, Action finishAction)
        {
            finish = finishAction;
            if (!Application.isPlaying || !isActiveAndEnabled) { FinishNow(); return; }
            Busy = true;
            motion = StartCoroutine(Wrap(routine));
        }

        IEnumerator Wrap(IEnumerator routine)
        {
            yield return routine;
            motion = null;
            FinishNow();
        }

        // ---- steam ----
        void SetSteam(bool on)
        {
            if (steamPuffs == null) return;
            if (puffBase == null || puffBase.Length != steamPuffs.Length)
            {
                puffBase = new Vector3[steamPuffs.Length];
                for (int i = 0; i < steamPuffs.Length; i++) if (steamPuffs[i] != null) puffBase[i] = steamPuffs[i].transform.localPosition;
            }
            if (steam != null) { StopCoroutine(steam); steam = null; }
            for (int i = 0; i < steamPuffs.Length; i++)
            {
                if (steamPuffs[i] == null) continue;
                steamPuffs[i].transform.localPosition = puffBase[i];
                steamPuffs[i].transform.localScale = Vector3.one;
                steamPuffs[i].SetActive(on);
            }
            if (on && Application.isPlaying && isActiveAndEnabled) steam = StartCoroutine(Steam());
        }

        IEnumerator Steam()
        {
            while (true)
            {
                float time = Time.time;
                for (int i = 0; i < steamPuffs.Length; i++)
                {
                    if (steamPuffs[i] == null) continue;
                    float phase = Mathf.Repeat(time * 0.6f + i * 0.27f, 1f);
                    steamPuffs[i].transform.localPosition = puffBase[i] + Vector3.up * (0.045f * phase);
                    steamPuffs[i].transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.35f, phase);
                }
                yield return null;
            }
        }
    }
}
