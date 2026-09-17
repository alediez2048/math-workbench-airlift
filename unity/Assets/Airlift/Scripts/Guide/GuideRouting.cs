namespace Airlift.Guide
{
    public enum GuideMode { Live, Offline }

    /// Pure decision: when the live guide is allowed and when the labeled offline guide runs.
    public static class GuideRouting
    {
        public static GuideMode Decide(bool micPermission, bool minted, bool socketOpen, bool adultTesterOnlySatisfied)
        {
            if (!adultTesterOnlySatisfied) return GuideMode.Offline;
            if (!minted || !socketOpen) return GuideMode.Offline;
            return micPermission ? GuideMode.Live : GuideMode.Live; // no mic still allows spoken output + chips
        }

        public static bool ShouldReconnect(int attemptsSoFar, bool wasEverOpen) => wasEverOpen && attemptsSoFar < 1;
    }
}
