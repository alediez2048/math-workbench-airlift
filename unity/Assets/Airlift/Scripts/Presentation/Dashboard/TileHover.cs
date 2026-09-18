using Airlift.Lounge;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Airlift.Presentation.Dashboard
{
    /// Hover raises the tile a touch and ticks the controller; the guide's focus ring is drawn by FocusPointer.
    public sealed class TileHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public float raise = 1.03f;
        Vector3 rest = Vector3.one;
        void Awake() { rest = transform.localScale; }
        public void OnPointerEnter(PointerEventData evt) { transform.localScale = rest * raise; NerdyHaptics.Tick(); }
        public void OnPointerExit(PointerEventData evt) { transform.localScale = rest; }
        void OnDisable() { transform.localScale = rest; }
    }
}
