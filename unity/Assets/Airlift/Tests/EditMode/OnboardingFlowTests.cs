using Airlift.Onboarding;
using NUnit.Framework;

public sealed class OnboardingFlowTests
{
    [Test] public void CatalogIsFirst() => Assert.AreEqual(OnboardingStage.Catalog, new OnboardingFlow().Stage);
    [TestCase(1)] [TestCase(2)] [TestCase(-1)] public void ComingSoonCannotOpen(int index)
    { var flow = new OnboardingFlow(); Assert.IsFalse(flow.OpenLesson(index)); Assert.AreEqual(OnboardingStage.Catalog, flow.Stage); }
    [Test] public void PurposeAndOrientationPrecedePractice()
    { var f = new OnboardingFlow(); f.OpenLesson(0); Assert.AreEqual(OnboardingStage.Overview, f.Stage); f.Continue(); Assert.AreEqual(OnboardingStage.Orientation, f.Stage); f.Continue(); Assert.AreEqual(OnboardingStage.Demonstration, f.Stage); }
    [Test] public void ReleaseWithoutGrabDoesNotComplete()
    { var f = Practice(); Assert.IsFalse(f.RecordRelease(true)); }
    [Test] public void WrongPlacementRemainsPractice()
    { var f = Practice(); f.RecordGrab(); Assert.IsFalse(f.RecordRelease(false)); Assert.AreEqual(OnboardingStage.Practice, f.Stage); }
    [Test] public void GrabThenPlaceCompletesOnboardingOnly()
    { var f = Practice(); f.RecordGrab(); Assert.IsTrue(f.RecordRelease(true)); Assert.AreEqual(OnboardingStage.Ready, f.Stage); Assert.IsFalse(f.Continue()); }
    [Test] public void NoButtonCanSkipPractice()
    { var f = Practice(); Assert.IsFalse(f.Continue()); Assert.AreEqual(OnboardingStage.Practice, f.Stage); }
    [Test] public void ReplayClearsPracticeEvidence()
    { var f = Practice(); f.RecordGrab(); f.ReplayDemonstration(false); f.FinishDemonstration(); Assert.IsFalse(f.RecordRelease(true)); }
    [Test] public void HeldObjectBlocksNavigation()
    { var f = Practice(); Assert.IsFalse(f.BackToCatalog(true)); Assert.IsFalse(f.ReplayDemonstration(true)); Assert.AreEqual(OnboardingStage.Practice, f.Stage); }
    [Test] public void BackReturnsToMenu()
    { var f = Practice(); Assert.IsTrue(f.BackToCatalog(false)); Assert.AreEqual(OnboardingStage.Catalog, f.Stage); }
    static OnboardingFlow Practice()
    { var f = new OnboardingFlow(); f.OpenLesson(0); f.Continue(); f.Continue(); f.FinishDemonstration(); return f; }
}
