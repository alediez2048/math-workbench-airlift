using UnityEngine;

namespace Airlift.Presentation
{
    /// Keeps a card's soft shadow (a sibling drawn behind the card) visible exactly while the card is.
    [ExecuteAlways]
    public sealed class CardShadowLink : MonoBehaviour
    {
        public GameObject shadow;
        void OnEnable() { if (shadow != null) shadow.SetActive(true); }
        void OnDisable() { if (shadow != null) shadow.SetActive(false); }
    }
}
