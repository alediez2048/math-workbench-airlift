using UnityEngine.XR;

namespace Airlift.Lounge
{
    /// CC-FD-09: every pulse in the app routes through here, so the Haptic feedback switch is one switch.
    public static class NerdyHaptics
    {
        public static bool Enabled = true;

        public static void Tick() => Pulse(0.25f, 0.02f);
        public static void Pulse(float amplitude, float seconds, XRNode hand = XRNode.RightHand)
        {
            if (!Enabled) return;
            try
            {
                var device = InputDevices.GetDeviceAtXRNode(hand);
                if (device.isValid && device.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
                    device.SendHapticImpulse(0, amplitude, seconds);
            }
            catch (System.Exception) { /* no device in the editor */ }
        }
    }
}
