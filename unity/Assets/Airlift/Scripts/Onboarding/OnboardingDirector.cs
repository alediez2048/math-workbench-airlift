using System.Collections;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Airlift.Onboarding
{
    public sealed class OnboardingDirector : MonoBehaviour
    {
        public OnboardingContent content;
        public Transform head;
        public Transform strap;
        public Grabbable grabbable;
        public Transform demonstrationStrap;
        public GameObject station;
        public GameObject catalog;
        public GameObject briefing;
        public TMP_Text heading;
        public TMP_Text body;
        public TMP_Text primaryLabel;
        public Button primary;
        public Button help;
        public UnityEvent whenReadyContinue = new UnityEvent();
        public bool IsReady => flow.Stage == OnboardingStage.Ready;
        // TEMPORARY: show live input readings on the practice panel until the grab defect is closed.
        public bool showInputDiagnostics = true;
        readonly OnboardingFlow flow = new OnboardingFlow();
        Coroutine demonstration;
        Coroutine release;
        bool holding;
        int generation;
        string baseBody = "";
        float nextDiagnosticLog;
        float diagnosticLogUntil;

        void Start()
        {
            grabbable.WhenPointerEventRaised += OnPointer;
            Refresh();
            StartCoroutine(PlaceWhenTracked());
            diagnosticLogUntil = Time.time + 120f;
        }

        // The camera reports the origin until tracking is acquired, which used to put the
        // board on the floor. Keep the authored scene pose until the head is tracked.
        IEnumerator PlaceWhenTracked()
        {
            while (!OnboardingPlacement.IsHeadTracked(head.position)) yield return null;
            var pose = OnboardingPlacement.BoardPose(head.position, head.forward, content.boardDistance, content.boardBelowEyes);
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        void Update()
        {
            if (!showInputDiagnostics || flow.Stage != OnboardingStage.Practice) return;
            var target = strap.GetComponentInChildren<GrabInteractable>(true);
            string readout = InputDiagnostics.Describe(target);
            body.text = baseBody + "\n\n<size=70%>" + readout + "</size>";
            if (Time.time < diagnosticLogUntil && Time.time >= nextDiagnosticLog)
            {
                nextDiagnosticLog = Time.time + 0.5f;
                Debug.Log("[DEBUG-cargo-input] " + readout.Replace('\n', ' '));
            }
        }

        void OnDestroy() { if (grabbable != null) grabbable.WhenPointerEventRaised -= OnPointer; }

        public void ChooseCargo()
        {
            if (flow.OpenLesson(0)) Refresh();
        }

        public void Continue()
        {
            if (holding) { SetBody("Release the strap before changing steps."); return; }
            if (flow.Stage == OnboardingStage.Ready) { whenReadyContinue.Invoke(); return; }
            if (!flow.Continue()) return;
            Refresh();
            if (flow.Stage == OnboardingStage.Demonstration) StartDemonstration();
        }

        public void Help()
        {
            if (holding) { SetBody("Keep holding the grip to move the strap. Release above the outlined pad. Release it before replaying the demo."); return; }
            if (flow.ReplayDemonstration(false)) { Refresh(); StartDemonstration(); }
            else Refresh();
        }

        public void Back()
        {
            if (!flow.BackToCatalog(holding)) { SetBody("Release the strap before returning to lessons."); return; }
            generation++;
            if (demonstration != null) StopCoroutine(demonstration);
            if (release != null) StopCoroutine(release);
            demonstration = null;
            release = null;
            ResetStrap();
            Refresh();
        }

        void StartDemonstration()
        {
            generation++;
            if (release != null) StopCoroutine(release);
            if (demonstration != null) StopCoroutine(demonstration);
            ResetStrap();
            demonstration = StartCoroutine(ShowDemonstration());
        }

        IEnumerator ShowDemonstration()
        {
            float elapsed = 0;
            while (elapsed < content.demonstrationSeconds)
            {
                float t = Mathf.Clamp01(elapsed / content.demonstrationSeconds);
                demonstrationStrap.localPosition = Vector3.Lerp(content.trayPosition, content.targetPosition, t)
                    + Vector3.up * (Mathf.Sin(t * Mathf.PI) * 0.16f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            demonstration = null;
            flow.FinishDemonstration();
            ResetStrap();
            Refresh();
        }

        void OnPointer(PointerEvent evt)
        {
            if (evt.Type == PointerEventType.Select)
            {
                generation++;
                holding = true;
                flow.RecordGrab();
            }
            else if (evt.Type == PointerEventType.Unselect)
            {
                holding = false;
                if (release != null) StopCoroutine(release);
                release = StartCoroutine(CheckRelease(generation));
            }
            else if (evt.Type == PointerEventType.Cancel)
            {
                generation++;
                holding = false;
            }
        }

        IEnumerator CheckRelease(int expectedGeneration)
        {
            yield return null; // Let the SDK finish release ownership before moving the strap.
            if (holding || expectedGeneration != generation || flow.Stage != OnboardingStage.Practice) yield break;
            bool inside = Vector3.Distance(strap.localPosition, content.targetPosition) <= content.placementRadius;
            if (flow.RecordRelease(inside))
            {
                strap.localPosition = content.targetPosition;
                strap.localRotation = Quaternion.identity;
                Refresh();
            }
            else
            {
                ResetStrap();
                SetBody(content.retry);
            }
            release = null;
        }

        void ResetStrap()
        {
            strap.localPosition = content.trayPosition;
            strap.localRotation = Quaternion.identity;
        }

        void SetBody(string text) { baseBody = text; body.text = text; }

        void Refresh()
        {
            bool isCatalog = flow.Stage == OnboardingStage.Catalog;
            catalog.SetActive(isCatalog);
            briefing.SetActive(!isCatalog);
            station.SetActive(!isCatalog);
            strap.gameObject.SetActive(flow.Stage == OnboardingStage.Practice || flow.Stage == OnboardingStage.Ready);
            demonstrationStrap.gameObject.SetActive(flow.Stage == OnboardingStage.Demonstration || flow.Stage == OnboardingStage.Orientation);
            if (flow.Stage == OnboardingStage.Orientation) demonstrationStrap.localPosition = content.trayPosition;
            heading.text = "Cargo Crew · Fractions";
            primary.gameObject.SetActive(flow.Stage == OnboardingStage.Overview || flow.Stage == OnboardingStage.Orientation || flow.Stage == OnboardingStage.Ready);
            help.gameObject.SetActive(flow.Stage != OnboardingStage.Demonstration && !isCatalog);
            switch (flow.Stage)
            {
                case OnboardingStage.Overview: SetBody(content.overview); primaryLabel.text = "Begin briefing"; break;
                case OnboardingStage.Orientation: SetBody(content.orientation); primaryLabel.text = "Watch demo"; break;
                case OnboardingStage.Demonstration: SetBody(content.demonstration); break;
                case OnboardingStage.Practice: SetBody(content.practice); break;
                case OnboardingStage.Ready: SetBody(content.ready); primaryLabel.text = "Start fractions"; break;
            }
        }
    }
}
