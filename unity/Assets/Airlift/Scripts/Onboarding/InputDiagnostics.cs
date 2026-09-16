using System.Collections.Generic;
using System.Text;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.XR;

namespace Airlift.Onboarding
{
    /// TEMPORARY adult-device diagnostic for the practice grab defect. Reads raw
    /// controller input at three layers (OVRInput, Unity XR, Interaction SDK
    /// selector) so one headset check shows where the grip signal is lost.
    /// Contains no identifiers or persistent data. Remove after the defect is closed.
    public static class InputDiagnostics
    {
        static ControllerSelector[] selectors;
        static GrabInteractor[] interactors;

        public static string Describe(GrabInteractable target)
        {
            if (selectors == null || selectors.Length == 0)
                selectors = Object.FindObjectsByType<ControllerSelector>(FindObjectsInactive.Include);
            if (interactors == null || interactors.Length == 0)
                interactors = Object.FindObjectsByType<GrabInteractor>(FindObjectsInactive.Include);
            var sb = new StringBuilder();
            sb.Append("OVR L g").Append(Axis(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch))
              .Append(" t").Append(Axis(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
              .Append(" | R g").Append(Axis(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch))
              .Append(" t").Append(Axis(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
              .Append(" | con=").Append(OVRInput.GetConnectedControllers()).Append(" act=").Append(OVRInput.GetActiveController());
            sb.Append("\nXR  L ").Append(Xr(XRNode.LeftHand)).Append(" | R ").Append(Xr(XRNode.RightHand));
            sb.Append("\nSDK");
            foreach (var selector in selectors)
            {
                if ((selector.ControllerButtonUsage & ControllerButtonUsage.GripButton) == 0) continue;
                var controller = selector.Controller;
                sb.Append(' ').Append(Hand(selector.transform)).Append(":sel=").Append(selector.isActiveAndEnabled ? "on" : "off")
                  .Append(" use=").Append((int)selector.ControllerButtonUsage)
                  .Append(controller == null ? " ctl=null" : " conn=" + YesNo(controller.IsConnected)
                      + " grip=" + YesNo(controller.IsButtonUsageAnyActive(ControllerButtonUsage.GripButton))
                      + " trig=" + YesNo(controller.IsButtonUsageAnyActive(ControllerButtonUsage.TriggerButton)));
            }
            sb.Append("\nGRAB");
            foreach (var interactor in interactors)
            {
                if (!interactor.isActiveAndEnabled) continue;
                sb.Append(' ').Append(Hand(interactor.transform)).Append(':').Append(interactor.State)
                  .Append(interactor.Candidate == target ? " cand" : "").Append(interactor.SelectedInteractable == target ? " SEL" : "");
            }
            return sb.ToString();
        }

        static string Axis(OVRInput.Axis1D axis, OVRInput.Controller controller) => OVRInput.Get(axis, controller).ToString("F2");
        static string YesNo(bool value) => value ? "Y" : "N";

        static string Xr(XRNode node)
        {
            var device = InputDevices.GetDeviceAtXRNode(node);
            if (!device.isValid) return "none";
            device.TryGetFeatureValue(CommonUsages.grip, out float grip);
            device.TryGetFeatureValue(CommonUsages.trigger, out float trigger);
            return "g" + grip.ToString("F2") + " t" + trigger.ToString("F2");
        }

        static string Hand(Transform transform)
        {
            for (var t = transform; t != null; t = t.parent)
            {
                if (t.name.Contains("Left")) return "L";
                if (t.name.Contains("Right")) return "R";
            }
            return "?";
        }
    }
}
