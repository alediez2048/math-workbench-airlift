using TMPro;
using UnityEngine;

namespace Airlift.Lounge
{
    /// CC-FD-05a. The real Touch Plus controller model, floating beside the board, with the named button lit and a
    /// soft arrow bouncing on it. Used by every rundown stop that names a control, and by the "Show button labels"
    /// setting inside a lesson (a grip hint the first time a step lets the learner grab).
    public sealed class ControllerHelper : MonoBehaviour
    {
        [Tooltip("The controller mesh root (MetaQuestTouchPlus_Right).")]
        public Transform model;
        [Tooltip("Bones of the model, in ControllerButton order: None, Trigger, Grip, B, Thumbstick.")]
        public Transform[] anchors = new Transform[5];
        public Transform glow, arrow;
        public TMP_Text label;
        public float bounce = 0.02f, bounceSpeed = 3.5f, glowPulse = 0.15f;

        public ControllerButton Button { get; private set; } = ControllerButton.None;
        public bool Showing => gameObject.activeSelf;
        float hideAt = float.PositiveInfinity;
        Vector3 arrowBase;

        public void Show(ControllerButton button, string text, float seconds = float.PositiveInfinity)
        {
            Button = button;
            gameObject.SetActive(true);
            var anchor = Anchor(button);
            bool lit = anchor != null && button != ControllerButton.None;
            if (glow != null) { glow.gameObject.SetActive(lit); if (lit) glow.position = anchor.position; }
            if (arrow != null)
            {
                arrow.gameObject.SetActive(lit);
                if (lit) { arrowBase = anchor.position + Vector3.up * 0.05f; arrow.position = arrowBase; }
            }
            if (label != null) { label.text = text ?? ""; label.gameObject.SetActive(!string.IsNullOrEmpty(text)); }
            hideAt = float.IsPositiveInfinity(seconds) ? float.PositiveInfinity : Time.time + seconds;
        }

        public void Hide() { Button = ControllerButton.None; hideAt = float.PositiveInfinity; gameObject.SetActive(false); }

        public Transform Anchor(ControllerButton button)
        {
            int i = (int)button;
            return anchors != null && i >= 0 && i < anchors.Length ? anchors[i] : null;
        }

        void Update()
        {
            if (Time.time >= hideAt) { Hide(); return; }
            float t = Time.time;
            if (arrow != null && arrow.gameObject.activeSelf) arrow.position = arrowBase + Vector3.up * (bounce * (0.5f + 0.5f * Mathf.Sin(t * bounceSpeed * 2f)));
            if (glow != null && glow.gameObject.activeSelf) glow.localScale = Vector3.one * (1f + glowPulse * Mathf.Sin(t * 5f));
        }
    }
}
