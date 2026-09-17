namespace Airlift.Welcome
{
    /// When the guide may listen and speak. Mic only in the cards view and the lesson; never while
    /// muted or paused. Prompts only when live and not paused.
    public static class GuidePolicy
    {
        public static bool MicOn(WelcomePhase phase, bool muted, bool paused) =>
            !muted && !paused && (phase == WelcomePhase.Catalog || phase == WelcomePhase.Lesson);

        public static bool CanPrompt(bool paused, bool live) => live && !paused;

        /// A tool result normally asks the guide to narrate it; while paused the guide stays silent.
        public static bool SpeakAfterToolResult(bool paused) => !paused;

        /// Voice-first workbench: the on-screen buttons hide while the live guide is listening and come back
        /// whenever voice is unavailable (offline, declined, muted, paused, or a phase with the mic off).
        public static bool ShowFallbackButtons(bool live, bool micOn) => !live || !micOn;
    }
}
