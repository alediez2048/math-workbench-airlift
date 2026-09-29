using Airlift.Lounge;
using NUnit.Framework;

namespace Airlift.Tests
{
    /// Toy rack (spec 2026-09-29): a chapter is recorded when the director first sees it complete, once per transition.
    public class CompletionRecorderTests
    {
        [Test] public void RecordsTheChapterOnTheTransitionToComplete()
        {
            Assert.That(CompletionRecorder.Newly(observedChapter: 2, observedComplete: false, chapter: 2, complete: true), Is.EqualTo(2));
        }

        [Test] public void DoesNotRecordWhileIncompleteOrWhileAlreadyObservedComplete()
        {
            Assert.That(CompletionRecorder.Newly(2, false, 2, false), Is.EqualTo(0), "still working");
            Assert.That(CompletionRecorder.Newly(2, true, 2, true), Is.EqualTo(0), "already seen complete; no double record");
            Assert.That(CompletionRecorder.Newly(0, false, 0, false), Is.EqualTo(0), "no chapter");
        }

        [Test] public void AChapterThatIsCompleteWhenFirstSeenStillCounts()
        {
            // Returning to a chapter that was finished earlier (or finishing it while the guide was elsewhere).
            Assert.That(CompletionRecorder.Newly(1, true, 2, true), Is.EqualTo(2));
            Assert.That(CompletionRecorder.Newly(0, false, 3, true), Is.EqualTo(3));
        }
    }
}
