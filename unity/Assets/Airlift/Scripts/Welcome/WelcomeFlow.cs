namespace Airlift.Welcome
{
    public enum WelcomePhase { Consent, Welcome, Catalog, Lesson }

    /// Pure state machine for the Nerdy entry flow. The guide can only move it forward through
    /// validated tool calls; the learner can always skip to the catalog.
    public sealed class WelcomeFlow
    {
        public WelcomePhase Phase { get; private set; } = WelcomePhase.Consent;
        public bool VoiceConsented { get; private set; }
        public LearnerProfile Profile { get; } = new LearnerProfile();
        /// The LessonCatalog id of the open lesson; null outside the lesson phase.
        public string ActiveLessonId { get; private set; }

        public void Consent(bool allowVoice) { if (Phase != WelcomePhase.Consent) return; VoiceConsented = allowVoice; Phase = WelcomePhase.Welcome; }

        /// record_profile tool payload; true when at least one valid tag was stored.
        public bool RecordProfile(string argumentsJson) => Phase == WelcomePhase.Welcome && Profile.Merge(argumentsJson);

        /// end_welcome tool call or the learner's skip.
        public bool EndWelcome() { if (Phase != WelcomePhase.Welcome) return false; Phase = WelcomePhase.Catalog; return true; }

        public bool OpenLesson(string cardId)
        {
            if (Phase != WelcomePhase.Catalog || !LessonCatalog.IsPlayable(cardId)) return false;
            Phase = WelcomePhase.Lesson; ActiveLessonId = cardId; return true;
        }

        public bool BackToCatalog() { if (Phase != WelcomePhase.Lesson) return false; Phase = WelcomePhase.Catalog; ActiveLessonId = null; return true; }
    }
}
