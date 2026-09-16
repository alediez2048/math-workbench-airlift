using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Airlift.Presentation
{
    [RequireComponent(typeof(Outline))]
    public sealed class FocusPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        Outline outline;
        void Awake(){outline=GetComponent<Outline>();outline.enabled=false;}
        public void OnPointerEnter(PointerEventData evt){if(outline!=null)outline.enabled=true;}
        public void OnPointerExit(PointerEventData evt){if(outline!=null)outline.enabled=false;}
        void OnDisable(){if(outline!=null)outline.enabled=false;}
    }
}
