using Oculus.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Airlift.Presentation
{
    /// Diagnostic for the button regression reported on build 190518 (plan 3.4). Logs pointer enter, down,
    /// up and click on this button with its path and canvas, and (once per app) every ray select/unselect
    /// the Interaction SDK canvas module forwards, including presses that hit no button at all.
    /// Log lines start with "[Nerdy] UI" so the Mac logcat filter keeps them. No learner data is logged.
    public sealed class UiPressLog : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        static bool hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void HookCanvasModule()
        {
            if (hooked) return;
            hooked = true;
            PointableCanvasModule.WhenSelected += args => Debug.Log("[Nerdy] UI ray select canvas=" + Name(args.Canvas) + " hovered=" + PathOf(args.Hovered) + " dragging=" + args.Dragging);
            PointableCanvasModule.WhenUnselected += args => Debug.Log("[Nerdy] UI ray unselect canvas=" + Name(args.Canvas) + " selected=" + PathOf(args.Hovered));
        }

        public void OnPointerEnter(PointerEventData evt) => Log("enter", evt);
        public void OnPointerDown(PointerEventData evt) => Log("down", evt);
        public void OnPointerUp(PointerEventData evt) => Log("up", evt);
        public void OnPointerClick(PointerEventData evt) => Log("click", evt);

        void Log(string kind, PointerEventData evt)
        {
            var button = GetComponent<Button>();
            var canvas = GetComponentInParent<Canvas>();
            var group = GetComponentInParent<CanvasGroup>();
            string state = button == null ? "no-button"
                : "interactable=" + button.interactable + " listeners=" + button.onClick.GetPersistentEventCount();
            string groupState = group == null ? "" : " group=" + group.name + " alpha=" + group.alpha.ToString("F2") + " blocks=" + group.blocksRaycasts;
            Debug.Log("[Nerdy] UI " + kind + " path=" + PathOf(gameObject) + " canvas=" + Name(canvas != null ? canvas.rootCanvas : null)
                + " pointer=" + (evt != null ? evt.pointerId.ToString() : "?") + " " + state + groupState);
        }

        static string Name(Object o) => o != null ? o.name : "none";

        public static string PathOf(GameObject go)
        {
            if (go == null) return "none";
            var t = go.transform; string path = t.name;
            for (var p = t.parent; p != null; p = p.parent) path = p.name + "/" + path;
            return path;
        }
    }
}
