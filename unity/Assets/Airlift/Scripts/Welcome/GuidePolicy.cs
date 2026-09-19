namespace Airlift.Welcome
{
    /// When the guide may listen and speak. Mic only in the cards view and the lesson; never while
    /// muted or paused. Prompts only when live and not paused.
    public static class GuidePolicy
    {
        public static bool MicOn(WelcomePhase phase, bool muted, bool paused) =>
            !muted && !paused && (phase == WelcomePhase.Catalog || phase == WelcomePhase.Lesson || phase == WelcomePhase.Rundown);

        /// The settings card's Voice guide switch: off means Dee neither listens nor speaks, captions stay.
        public static bool MicOn(WelcomePhase phase, bool muted, bool paused, bool voiceGuide) => voiceGuide && MicOn(phase, muted, paused);
        public static bool CanPrompt(bool paused, bool live, bool voiceGuide) => voiceGuide && CanPrompt(paused, live);

        /// Dee speaks from the moment the learner arrives, so her bar is up from the first card: the controls that
        /// stop her — Mute, Pause — must never be further away than her voice. It waits only for the logo arrival.
        public static bool AssistantBarVisible(WelcomePhase phase, bool arriving) => !arriving;

        public static bool CanPrompt(bool paused, bool live) => live && !paused;

        /// A tool result normally asks the guide to narrate it; while paused the guide stays silent.
        public static bool SpeakAfterToolResult(bool paused) => !paused;

        /// Voice-first workbench: the on-screen buttons hide while the live guide is listening and come back
        /// whenever voice is unavailable (offline, declined, muted, paused, or a phase with the mic off).
        public static bool ShowFallbackButtons(bool live, bool micOn) => !live || !micOn;

        /// The welcome canvas keeps its invisible ray surface off during a lesson so it cannot steal presses from the
        /// workbench card; the settings card lives on that canvas, so the surface comes back while the card is open
        /// (owner 2026-09-18: the gear left settings stuck inside a lesson).
        public static bool WelcomeRaySurfaceOn(WelcomePhase phase, bool settingsOpen) => phase != WelcomePhase.Lesson || settingsOpen;

        /// Owner 2026-09-18: with the mic pill and Mic button gone, nothing turned the mic on for a new learner and Dee
        /// could not hear "start the cargo lesson". After the questions the mic comes on by itself for an adult with the
        /// voice guide on and Dee not stopped. Adult testers only: minors and "prefer not to say" stay off.
        /// The voice session is minted and locked in one language (GuideMessages.MintRequestBody), so a change while
        /// Dee is connected, or connecting, has to end and re-mint her; offline or stopped, the next Begin picks it up.
        public static bool ReconnectForLanguage(string current, string next, bool connected, bool paused) =>
            current != next && connected && !paused;

        public const string AgeNotice = "Set your age in settings to talk with Dee";
        public const string AdultsOnlyNotice = "Voice is for adult testers only";
        public const string VoiceOffNotice = "Voice guide is off in settings";
        /// The one line under Dee's Play/Stop control that says why she is not listening. Null while stopped: the
        /// Play notices own that case. Empty when there is nothing to explain.
        public static string MicNotice(string ageBand, bool voiceGuide, bool paused)
        {
            if (paused) return null;
            if (!voiceGuide) return VoiceOffNotice;
            if (string.IsNullOrEmpty(ageBand)) return AgeNotice;
            return ageBand == "adult" ? "" : AdultsOnlyNotice;
        }

        public static bool MicAutoOnAfterQuestions(string ageBand, bool voiceGuide, bool paused) => ageBand == "adult" && voiceGuide && !paused;
    }
}
