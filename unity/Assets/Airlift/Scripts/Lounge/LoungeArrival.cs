using System;
using UnityEngine;
using UnityEngine.UI;

namespace Airlift.Lounge
{
    /// CC-FD-03. The first thing the learner sees: Nerdy's dots fly in, the logo assembles, and a pill bar fills
    /// with whatever is genuinely still loading. Then it clears and the board takes over. Pure timing plus alpha,
    /// so it costs nothing and cannot get stuck: it always ends, even if the guide never connects.
    public sealed class LoungeArrival : MonoBehaviour
    {
        [Tooltip("The whole arrival group; hidden the moment the sequence ends.")]
        public CanvasGroup group;
        public RectTransform logo;
        [Tooltip("Dots that converge on the logo before it appears.")]
        public RectTransform[] dots;
        [Tooltip("Fill of the pill progress bar; only shown while real work is pending.")]
        public RectTransform barFill;
        public GameObject barRoot;
        public float barWidth = 360f;

        public const float DotsSeconds = 1.1f, LogoSeconds = 1.3f, HoldSeconds = 0.55f, FadeSeconds = 0.45f;
        public static float TotalSeconds => DotsSeconds + LogoSeconds + HoldSeconds + FadeSeconds;

        /// Raised once, when the arrival is over and the board should appear.
        public event Action Finished;

        float elapsed;
        bool running, finished;
        Func<float> progress = () => 1f;
        Func<bool> showBar = () => false;

        /// The arrival reports real work: pass the lounge's startup tracker, never a timer pretending to load.
        public void Begin(Func<float> realProgress, Func<bool> barWanted)
        {
            progress = realProgress ?? (() => 1f);
            showBar = barWanted ?? (() => false);
            elapsed = 0f; running = true; finished = false;
            if (group != null) { group.alpha = 1f; group.gameObject.SetActive(true); }
            Step(0f);
        }

        void Update()
        {
            if (!running) return;
            elapsed += Time.deltaTime;
            Step(elapsed);
            // The bar may hold the arrival open a moment longer, but never past a sane ceiling: a learner waiting
            // on a dead network still gets into the app.
            bool waiting = showBar() && elapsed < TotalSeconds + 6f;
            if (elapsed >= TotalSeconds && !waiting) Finish();
        }

        void Step(float t)
        {
            // Dots converge on the centre, then hand over to the logo.
            if (dots != null)
            {
                float k = Mathf.Clamp01(t / DotsSeconds);
                float ease = 1f - Mathf.Pow(1f - k, 3f);
                for (int i = 0; i < dots.Length; i++)
                {
                    if (dots[i] == null) continue;
                    float a = i * Mathf.PI * 2f / Mathf.Max(1, dots.Length);
                    float radius = Mathf.Lerp(260f, 0f, ease);
                    dots[i].anchoredPosition = new Vector2(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius * 0.6f);
                    dots[i].localScale = Vector3.one * Mathf.Lerp(1f, 0.2f, ease);
                    var img = dots[i].GetComponent<Graphic>();
                    if (img != null) { var c = img.color; c.a = 1f - ease; img.color = c; }
                }
            }

            if (logo != null)
            {
                float k = Mathf.Clamp01((t - DotsSeconds * 0.75f) / LogoSeconds);
                float ease = 1f - Mathf.Pow(1f - k, 3f);
                logo.localScale = Vector3.one * Mathf.Lerp(0.82f, 1f, ease);
                var img = logo.GetComponent<Graphic>();
                if (img != null) { var c = img.color; c.a = ease; img.color = c; }
            }

            if (barRoot != null) barRoot.SetActive(showBar());
            if (barFill != null)
            {
                float h = barFill.sizeDelta.y;
                // A capsule cannot be narrower than its own ends, so the fill starts as a dot and grows.
                barFill.sizeDelta = new Vector2(Mathf.Max(h, Mathf.Clamp01(progress()) * barWidth), h);
            }

            if (t > TotalSeconds - FadeSeconds && group != null)
                group.alpha = Mathf.Clamp01((TotalSeconds - t) / FadeSeconds);
        }

        public void Finish()
        {
            if (finished) return;
            finished = true; running = false;
            if (group != null) { group.alpha = 0f; group.gameObject.SetActive(false); }
            Finished?.Invoke();
        }
    }
}
