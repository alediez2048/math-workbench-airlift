using Airlift.Welcome;
using UnityEngine;
using UnityEngine.UI;

namespace Airlift.Presentation
{
    public enum DeePose { Idle, Listening, Speaking, Pleased }

    /// Dee as a character beside the assistant bar. Display only: it reads the voice state NerdyDirector already
    /// tracks (playback speaking, mic streaming) and swaps pose, with a gentle float. It never changes guide state.
    [RequireComponent(typeof(Image))]
    public sealed class DeeAvatar : MonoBehaviour
    {
        public NerdyDirector director;
        public Sprite idle, listening, speaking, pleased;
        [Tooltip("Float amplitude in the canvas's units.")] public float bobAmplitude = 6f;

        Image image;
        RectTransform rect;
        Vector2 rest;

        float pleasedUntil = -1f;

        public static DeePose PoseFor(bool speaking, bool listening) => PoseFor(speaking, listening, false);

        /// A moment of celebration wins briefly over the voice state, then Dee goes back to it.
        public static DeePose PoseFor(bool speaking, bool listening, bool pleased) =>
            pleased ? DeePose.Pleased : speaking ? DeePose.Speaking : listening ? DeePose.Listening : DeePose.Idle;

        /// Hold the pleased pose for a moment (SolveCelebration calls this when a check is accepted).
        public void Celebrate(float seconds) => pleasedUntil = Time.unscaledTime + seconds;

        /// A slow, smooth float: about one cycle every four seconds.
        public static float Bob(float time, float amplitude) => amplitude * Mathf.Sin(time * 1.6f);

        void Awake()
        {
            image = GetComponent<Image>();
            rect = (RectTransform)transform;
            rest = rect.anchoredPosition;
        }

        void Update()
        {
            bool isSpeaking = director != null && director.playback != null && director.playback.IsSpeaking;
            bool isListening = director != null && director.guide != null && director.guide.MicStreaming;
            var pose = PoseFor(isSpeaking, isListening, pleased != null && Time.unscaledTime < pleasedUntil);
            var sprite = pose == DeePose.Pleased ? pleased : pose == DeePose.Speaking ? speaking : pose == DeePose.Listening ? listening : idle;
            if (sprite != null && image.sprite != sprite) image.sprite = sprite;
            rect.anchoredPosition = rest + new Vector2(0f, Bob(Time.time, bobAmplitude));
        }
    }
}
