using UnityEngine;

namespace Airlift.Presentation
{
    // Static narrative props. Never authoritative fraction units or job completion.
    public sealed class CargoTerminalView : MonoBehaviour
    {
        public Transform truck;
        public Transform[] containers;
        public Transform staging;
        public Transform[] cranes;
        public Transform measuringPlatform;
        public bool HasRequiredProps => truck != null && containers != null && containers.Length >= 2
            && containers[0] != null && containers[1] != null && staging != null
            && cranes != null && cranes.Length >= 2 && cranes[0] != null && cranes[1] != null
            && measuringPlatform != null;
    }
}
