using UnityEngine;

namespace Airlift.Presentation
{
    // Static narrative props. Never authoritative fraction units or job completion.
    public sealed class CargoTerminalView : MonoBehaviour
    {
        public Transform truck;
        public Transform[] containers;
        public Transform staging;
        public Transform aircraft;
        public Transform measuringPlatform;
        public bool HasRequiredProps => truck != null && containers != null && containers.Length >= 2
            && containers[0] != null && containers[1] != null && staging != null
            && aircraft != null && measuringPlatform != null;
    }
}
