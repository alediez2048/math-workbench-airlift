namespace Airlift.Onboarding
{
    public enum OnboardingStage { Catalog, Overview, Orientation, Demonstration, Practice, Ready }

    /// <summary>Onboarding only: no arithmetic grading or automatic lesson completion.</summary>
    public sealed class OnboardingFlow
    {
        public OnboardingStage Stage { get; private set; }
        public bool HasPracticedGrab { get; private set; }
        public bool OpenLesson(int index)
        {
            if (index != 0 || Stage != OnboardingStage.Catalog) return false;
            Stage = OnboardingStage.Overview;
            return true;
        }
        public bool Continue()
        {
            if (Stage == OnboardingStage.Overview) Stage = OnboardingStage.Orientation;
            else if (Stage == OnboardingStage.Orientation) Stage = OnboardingStage.Demonstration;
            else return false;
            return true;
        }
        public bool FinishDemonstration()
        {
            if (Stage != OnboardingStage.Demonstration) return false;
            Stage = OnboardingStage.Practice;
            HasPracticedGrab = false;
            return true;
        }
        public void RecordGrab()
        {
            if (Stage == OnboardingStage.Practice) HasPracticedGrab = true;
        }
        public bool RecordRelease(bool inPracticeZone)
        {
            if (Stage != OnboardingStage.Practice || !HasPracticedGrab || !inPracticeZone) return false;
            Stage = OnboardingStage.Ready;
            return true;
        }
        public bool ReplayDemonstration(bool holding)
        {
            if (holding || (Stage != OnboardingStage.Practice && Stage != OnboardingStage.Ready)) return false;
            HasPracticedGrab = false;
            Stage = OnboardingStage.Demonstration;
            return true;
        }
        public bool BackToCatalog(bool holding)
        {
            if (holding) return false;
            Stage = OnboardingStage.Catalog;
            HasPracticedGrab = false;
            return true;
        }
    }
}
