using System;
using UnityEngine;
using UnityEngine.XR;

namespace Airlift.Lounge
{
    /// CC-FD-05a: the physical B button (the right controller's secondary button), read through Unity XR so it works
    /// under the OpenXR plugin regardless of the Meta input layer. Edge-triggered; NerdyDirector decides what it means.
    public sealed class BackButtonWatcher : MonoBehaviour
    {
        public event Action Pressed;
        bool wasDown;
        /// Editor and tests: press it from code.
        public void Press() => Pressed?.Invoke();

        void Update()
        {
            bool down = IsDown();
            if (down && !wasDown) Pressed?.Invoke();
            wasDown = down;
        }

        static bool IsDown()
        {
            try
            {
                var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                return right.isValid && right.TryGetFeatureValue(CommonUsages.secondaryButton, out bool b) && b;
            }
            catch (Exception) { return false; }
        }
    }
}
