using System.Collections.Generic;

namespace Airlift.Guide
{
    /// Serialises response.create requests behind the active response. The Realtime API rejects a
    /// new response while one is in progress ("Conversation already has an active response"), and
    /// Hush() → Prompt() back to back used to trip exactly that. Pure; time is passed in.
    public sealed class PromptGate
    {
        public const float MaxWaitSeconds = 6f;      // a normal response finishes long before this
        public const float CancelWaitSeconds = 1f;   // a cancelled response reports done within ~100 ms
        public bool ResponseActive { get; private set; }
        readonly Queue<Pending> queue = new Queue<Pending>();
        bool awaitingCancel;

        sealed class Pending { public string Instructions; public float At; }

        public void Request(string instructions, float now) { queue.Enqueue(new Pending { Instructions = instructions, At = now }); }
        public void ResponseStarted() { ResponseActive = true; }
        public void ResponseFinished() { ResponseActive = false; awaitingCancel = false; }

        /// Hush: drops stale requests. Returns true when a response is active and must be cancelled.
        public bool Cancel(float now)
        {
            queue.Clear();
            if (!ResponseActive) return false;
            awaitingCancel = true; return true;
        }

        public bool TryTake(float now, out string instructions)
        {
            instructions = null;
            if (queue.Count == 0) return false;
            var head = queue.Peek();
            float limit = awaitingCancel ? CancelWaitSeconds : MaxWaitSeconds;
            if (ResponseActive && now - head.At < limit) return false;
            queue.Dequeue(); instructions = head.Instructions; ResponseActive = true; awaitingCancel = false; return true;
        }
    }
}
