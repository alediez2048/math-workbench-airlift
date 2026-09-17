using Airlift.Guide;
using NUnit.Framework;

namespace Airlift.Tests
{
    /// The Realtime API rejects response.create while a response is active ("Conversation already
    /// has an active response in progress"). The gate serialises prompts behind the active one.
    public class PromptGateTests
    {
        [Test] public void RequestSendsImmediatelyWhenIdle()
        {
            var g = new PromptGate(); g.Request("hi", 0f);
            Assert.That(g.TryTake(0f, out var i), Is.True); Assert.That(i, Is.EqualTo("hi"));
            Assert.That(g.ResponseActive, Is.True, "a sent request counts as active until the server reports done");
            Assert.That(g.TryTake(0.1f, out _), Is.False);
        }

        [Test] public void RequestWaitsForTheActiveResponseToFinish()
        {
            var g = new PromptGate(); g.Request("first", 0f); g.TryTake(0f, out _);
            g.Request("second", 0.2f);
            Assert.That(g.TryTake(0.3f, out _), Is.False);
            g.ResponseFinished();
            Assert.That(g.TryTake(0.4f, out var i), Is.True); Assert.That(i, Is.EqualTo("second"));
        }

        [Test] public void HushDropsQueuedRequestsAndWaitsForTheCancelToComplete()
        {
            var g = new PromptGate(); g.Request("first", 0f); g.TryTake(0f, out _);
            g.Request("stale", 0.1f);
            Assert.That(g.Cancel(0.2f), Is.True, "a cancel is needed while a response is active");
            g.Request("fresh", 0.2f);
            Assert.That(g.TryTake(0.25f, out _), Is.False, "wait for the cancel to complete");
            g.ResponseFinished();
            Assert.That(g.TryTake(0.3f, out var i), Is.True); Assert.That(i, Is.EqualTo("fresh"));
            Assert.That(g.TryTake(0.3f, out _), Is.False, "the stale request was dropped");
        }

        [Test] public void AfterHushTheNextPromptWaitsAtMostOneSecondForTheCancel()
        {
            var g = new PromptGate(); g.Request("first", 0f); g.TryTake(0f, out _);
            g.Cancel(0.2f); g.Request("fresh", 0.2f);
            Assert.That(g.TryTake(0.2f + PromptGate.CancelWaitSeconds - 0.01f, out _), Is.False);
            Assert.That(g.TryTake(0.2f + PromptGate.CancelWaitSeconds + 0.01f, out var i), Is.True); Assert.That(i, Is.EqualTo("fresh"));
        }

        [Test] public void HushWhenIdleNeedsNoCancel()
        {
            var g = new PromptGate();
            Assert.That(g.Cancel(0f), Is.False);
            g.Request("next", 0f); Assert.That(g.TryTake(0f, out _), Is.True);
        }

        [Test] public void RequestIsSentAfterTheMaxWaitEvenWithoutDone()
        {
            var g = new PromptGate(); g.Request("first", 0f); g.TryTake(0f, out _);
            g.Request("second", 1f);
            Assert.That(g.TryTake(1f + PromptGate.MaxWaitSeconds - 0.01f, out _), Is.False);
            Assert.That(g.TryTake(1f + PromptGate.MaxWaitSeconds + 0.01f, out var i), Is.True); Assert.That(i, Is.EqualTo("second"));
        }

        [Test] public void ServerReportedResponseAlsoBlocks()
        {
            var g = new PromptGate(); g.ResponseStarted(); g.Request("x", 0f);
            Assert.That(g.TryTake(0f, out _), Is.False);
            g.ResponseFinished(); Assert.That(g.TryTake(0f, out _), Is.True);
        }

        [Test] public void NullInstructionsMeanAPlainResponseCreate()
        {
            var g = new PromptGate(); g.Request(null, 0f);
            Assert.That(g.TryTake(0f, out var i), Is.True); Assert.That(i, Is.Null);
        }
    }
}
