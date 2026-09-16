using UnityEngine;

namespace Airlift.Onboarding
{
    [CreateAssetMenu(menuName = "Airlift/Onboarding Content")]
    public sealed class OnboardingContent : ScriptableObject
    {
        [TextArea] public string overview = "Join the cargo crew preparing an aircraft delivery. Different packages need different strap lengths.\n\nFirst: learn to pick up and place a strap. Later: build and compare fractions of one whole.\n\nThis MVP includes onboarding only.";
        [TextArea] public string orientation = "The orange piece is a cargo strap. The outlined pad is its measuring station.\n\nStay comfortably seated or standing. Bring a controller to the strap; hold the middle-finger grip to lift it, then let go to release.\n\nChoose Watch demo to see the movement first.";
        [TextArea] public string demonstration = "DEMONSTRATION\n\nWatch the striped example strap lift from the tray and move onto the outlined pad.\n\nYou will make the same movement with the orange strap. This is practice, not a math question.";
        [TextArea] public string practice = "YOUR TURN\n\nTouch the orange strap with either controller. Hold the grip under your middle finger, move it over the outlined pad, then release.\n\nChoose Help / replay demo whenever you need it.";
        [TextArea] public string retry = "The strap is back in the tray. Hold the grip while moving, then release above the outlined pad. There is no penalty—try again, or replay the demonstration.";
        [TextArea] public string ready = "You picked up the strap and placed it at the measuring station.\n\nThis strap will represent one whole. Next, the cargo crew will ask for equal parts of that whole.\n\nOnboarding complete. The fraction activity is not included in this build yet.";
        public Vector3 trayPosition = new Vector3(-0.13f, 0.08f, -0.08f);
        public Vector3 targetPosition = new Vector3(0.13f, 0.08f, 0.12f);
        [Min(1)] public float demonstrationSeconds = 4;
        [Min(0.03f)] public float placementRadius = 0.16f;
        [Min(0.3f)] public float boardDistance = 0.65f;
        public float boardBelowEyes = 0.5f;
    }
}
