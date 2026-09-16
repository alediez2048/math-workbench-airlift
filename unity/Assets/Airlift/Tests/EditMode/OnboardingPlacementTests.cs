using Airlift.Onboarding;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    public class OnboardingPlacementTests
    {
        [Test] public void UntrackedHeadAtOriginIsNotReady()
        {
            Assert.That(OnboardingPlacement.IsHeadTracked(Vector3.zero), Is.False);
            Assert.That(OnboardingPlacement.IsHeadTracked(new Vector3(0, float.NaN, 0)), Is.False);
        }
        [Test] public void SeatedAndStandingHeadsAreReady()
        {
            Assert.That(OnboardingPlacement.IsHeadTracked(new Vector3(0, 1.1f, 0)), Is.True);
            Assert.That(OnboardingPlacement.IsHeadTracked(new Vector3(0.2f, 1.7f, -0.3f)), Is.True);
        }
        [Test] public void BoardSitsBelowEyesInFrontOfHeadAndNeverOnFloor()
        {
            var pose = OnboardingPlacement.BoardPose(new Vector3(0, 1.2f, 0), new Vector3(0, -0.4f, 1), 0.65f, 0.5f);
            Assert.That(pose.position.y, Is.EqualTo(0.7f).Within(1e-4f));
            Assert.That(pose.position.z, Is.EqualTo(0.65f).Within(1e-4f));
            var low = OnboardingPlacement.BoardPose(new Vector3(0, 0.6f, 0), Vector3.forward, 0.65f, 0.5f);
            Assert.That(low.position.y, Is.EqualTo(OnboardingPlacement.MinimumBoardHeight).Within(1e-4f));
        }
        [Test] public void DegenerateForwardFallsBackToWorldForward()
        {
            var pose = OnboardingPlacement.BoardPose(new Vector3(0, 1.2f, 0), Vector3.up, 0.65f, 0.5f);
            Assert.That(pose.position.z, Is.EqualTo(0.65f).Within(1e-4f));
        }
    }
}
