namespace Airlift.Welcome
{
    /// Consent → Welcome (chip questions) → Rundown (how this works, first run only) → Catalog (the wall) → Lesson.
    /// A returning learner — profile answered, rundown seen — goes from consent straight to the wall.
    public enum WelcomePhase { Consent, Welcome, Rundown, Catalog, Lesson }

    /// Pure state machine for the Nerdy entry flow. The guide can only move it forward through
    /// validated tool calls; the learner can always skip to the catalog.
    public sealed class WelcomeFlow
    {
        public WelcomePhase Phase { get; private set; } = WelcomePhase.Consent;
        public bool VoiceConsented { get; private set; }
        public LearnerProfile Profile { get; } = new LearnerProfile();
        /// The LessonCatalog id of the open lesson; null outside the lesson phase.
        public string ActiveLessonId { get; private set; }
        /// CC-FD-05b: the rundown runs once, then only on request. Set from LibraryState before consent.
        public bool RundownSeen { get; set; }
        /// True when the consent answer skipped the questions and the rundown: the wall is next, with a welcome back.
        public bool Returning { get; private set; }
        /// Owner 2026-09-18: "get rid of the entire onboarding steps for now". True = the welcome card goes
        /// straight to the wall: no questions, no rundown. The code for both stays for when it comes back.
        public bool SkipOnboarding { get; set; }

        public void Consent(bool allowVoice)
        {
            if (Phase != WelcomePhase.Consent) return;
            VoiceConsented = allowVoice;
            Returning = Profile.IsComplete && RundownSeen;
            Phase = Returning || SkipOnboarding ? WelcomePhase.Catalog : WelcomePhase.Welcome;
        }

        /// The ‹ arrow during the questions: back to the welcome card, where the mic choice can be made again.
        public bool BackToConsent()
        {
            if (Phase != WelcomePhase.Welcome) return false;
            Phase = WelcomePhase.Consent; VoiceConsented = false; return true;
        }

        /// record_profile tool payload; true when at least one valid tag was stored.
        public bool RecordProfile(string argumentsJson) => Phase == WelcomePhase.Welcome && Profile.Merge(argumentsJson);

        /// end_welcome tool call or the learner's skip: the wall. (The host tour is an overlay on these phases and
        /// never a phase of its own; WelcomePhase.Rundown stays in the enum unused.)
        public bool EndWelcome()
        {
            if (Phase != WelcomePhase.Welcome) return false;
            Phase = WelcomePhase.Catalog;
            return true;
        }

        /// The tour ended — finished or skipped, both count as seen.
        public void MarkRundownSeen() => RundownSeen = true;

        /// A replay from the gear is only offered while browsing, never over a lesson.
        public bool CanReplayRundown => Phase == WelcomePhase.Catalog;

        public bool OpenLesson(string cardId)
        {
            if (Phase != WelcomePhase.Catalog || !LessonCatalog.IsPlayable(cardId)) return false;
            Phase = WelcomePhase.Lesson; ActiveLessonId = cardId; return true;
        }

        public bool BackToCatalog() { if (Phase != WelcomePhase.Lesson) return false; Phase = WelcomePhase.Catalog; ActiveLessonId = null; return true; }
    }
}
