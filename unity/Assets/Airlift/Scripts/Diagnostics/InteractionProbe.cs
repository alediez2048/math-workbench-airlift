using System.Collections.Generic;
using System.Text;
using Oculus.Interaction;
using UnityEngine;

namespace Airlift.Diagnostics
{
    /// Device diagnostic (2026-09-29, toy grab): logs every state change of every controller ray / grab / distance-grab
    /// interactor with what it targets, plus grip and trigger transitions, and once the rack's toy wiring. "[Probe]" lines
    /// only; changes nothing. Remove with RemoveInteractionProbe.cs once the grab is settled.
    public sealed class InteractionProbe : MonoBehaviour
    {
        readonly Dictionary<Component, string> last = new Dictionary<Component, string>();
        Component[] interactors = new Component[0];
        bool lGrip, rGrip, lTrig, rTrig;

        static string Path(Transform t) { var s = t.name; while (t.parent != null) { t = t.parent; s = t.name + "/" + s; } return s; }
        static string Short(Transform t) { var p = Path(t); int i = p.IndexOf("Interactors/"); return i >= 0 ? p.Substring(i + 12) : p; }
        static string Name(Component x) => x == null ? "-" : (x.transform.parent != null ? x.transform.parent.name + "/" : "") + x.name;

        void Start() { Invoke(nameof(Inventory), 5f); }

        void Inventory()
        {
            var list = new List<Component>();
            foreach (var r in FindObjectsByType<RayInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None)) list.Add(r);
            foreach (var g in FindObjectsByType<GrabInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None)) list.Add(g);
            foreach (var d in FindObjectsByType<DistanceGrabInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None)) list.Add(d);
            interactors = list.ToArray();
            var sb = new StringBuilder("[Probe] interactors:\n");
            foreach (var c in interactors)
                sb.Append("  ").Append(c.GetType().Name).Append(' ').Append(Short(c.transform)).Append(" active=").Append(c.gameObject.activeInHierarchy).Append(" enabled=").Append(((MonoBehaviour)c).enabled).Append('\n');
            Debug.Log(sb.ToString());
            var rack = FindAnyObjectByType<Airlift.Lounge.ToyRack>(FindObjectsInactive.Include);
            if (rack == null) { Debug.Log("[Probe] no toy rack in the scene"); return; }
            sb = new StringBuilder("[Probe] toys:\n");
            foreach (var s in rack.slots)
            {
                if (s.toy == null) continue;
                var g = s.toy.GetComponentInChildren<Grabbable>(true); var ray = s.toy.GetComponentInChildren<RayInteractable>(true);
                var gi = s.toy.GetComponentInChildren<GrabInteractable>(true); var dg = s.toy.GetComponentInChildren<DistanceGrabInteractable>(true);
                var col = s.toy.GetComponentInChildren<Collider>(true); var body = s.toy.GetComponentInChildren<Rigidbody>(true);
                sb.Append("  ").Append(s.id).Append(" active=").Append(s.toy.activeInHierarchy)
                  .Append(" grabbable=").Append(g != null && g.enabled).Append(" ray=").Append(ray != null && ray.enabled)
                  .Append(" grab=").Append(gi != null && gi.enabled).Append(" dist=").Append(dg != null && dg.enabled)
                  .Append(" collider=").Append(col != null ? col.GetType().Name + " enabled=" + col.enabled + " layer=" + col.gameObject.layer : "none")
                  .Append(" body=").Append(body != null ? "kinematic=" + body.isKinematic : "none")
                  .Append(" pos=").Append(s.toy.transform.position.ToString("F2")).Append(" scale=").Append(s.toy.transform.localScale.x.ToString("F2")).Append('\n');
            }
            Debug.Log(sb.ToString());
        }

        void Update()
        {
            Button(ref lGrip, OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.5f, "L grip");
            Button(ref rGrip, OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.5f, "R grip");
            Button(ref lTrig, OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) > 0.5f, "L trigger");
            Button(ref rTrig, OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.5f, "R trigger");
            foreach (var c in interactors)
            {
                if (c == null || !c.gameObject.activeInHierarchy) continue;
                string s = Describe(c);
                if (last.TryGetValue(c, out var prev) && prev == s) continue;
                last[c] = s;
                Debug.Log("[Probe] " + c.GetType().Name + " " + Short(c.transform) + " " + s);
            }
        }

        static string Describe(Component c)
        {
            switch (c)
            {
                case RayInteractor r: return r.State + " target=" + Name(r.Interactable) + " candidate=" + Name(r.Candidate) + (r.CollisionInfo.HasValue ? " hit@" + r.CollisionInfo.Value.Distance.ToString("F2") + "m" : "");
                case GrabInteractor g: return g.State + " target=" + Name(g.Interactable) + " candidate=" + Name(g.Candidate);
                case DistanceGrabInteractor d: return d.State + " target=" + Name(d.Interactable) + " candidate=" + Name(d.Candidate);
            }
            return "?";
        }

        void Button(ref bool was, bool now, string label) { if (now == was) return; was = now; Debug.Log("[Probe] input " + label + (now ? " down" : " up")); }
    }
}
