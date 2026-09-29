namespace Airlift.Lounge
{
    /// Toy rack (spec 2026-09-29). The director already watches each chapter's Complete flag to narrate it; this turns
    /// that observation into "record chapter n now" exactly once per transition, so the library never depends on the
    /// lesson code (Cargo Crew stays locked). Returns 0 when there is nothing to record.
    public static class CompletionRecorder
    {
        public static int Newly(int observedChapter, bool observedComplete, int chapter, bool complete)
        {
            if (chapter <= 0 || !complete) return 0;
            bool alreadySeenComplete = observedChapter == chapter && observedComplete;
            return alreadySeenComplete ? 0 : chapter;
        }
    }
}
