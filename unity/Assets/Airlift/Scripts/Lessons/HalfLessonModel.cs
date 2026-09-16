using Airlift.Math;

namespace Airlift.Lessons
{
    public enum HalfLessonPhase { Briefing, GrabPractice, ArrangeWhole, Partition, ArrangeHalf, Repair, Complete }
    public enum SubmissionResult { Unavailable, CorrectWhole, NeedsRepair, CorrectHalf }

    /// <summary>First T01/T02 slice only. No SDK, scene, network, timing or inferred learner behavior.</summary>
    public sealed class HalfLessonModel
    {
        readonly PlacementState placement;
        public HalfLessonPhase Phase { get; private set; } = HalfLessonPhase.Briefing;
        public int AttemptGeneration { get; private set; }
        public int PieceCount => placement.Count;
        public FractionValue Quantity => placement.Quantity;
        public string WholeId => placement.WholeId;
        public int PartitionCount { get; private set; } = 1;

        public HalfLessonModel(string wholeId) { placement = new PlacementState(wholeId); }
        public bool Begin()
        {
            if (Phase != HalfLessonPhase.Briefing) return false;
            Phase = HalfLessonPhase.GrabPractice;
            return true;
        }

        public bool RecordPracticeGrabAndRelease()
        {
            if (Phase != HalfLessonPhase.GrabPractice) return false;
            Phase = HalfLessonPhase.ArrangeWhole;
            return true;
        }

        public bool Place(PieceState piece)
        {
            if (!CanArrange || piece == null || piece.Notation.Denominator != PartitionCount) return false;
            bool placed = placement.TryInsert(piece, placement.Count);
            if (placed && Phase == HalfLessonPhase.Repair) Phase = HalfLessonPhase.ArrangeHalf;
            return placed;
        }

        public bool Remove(string id)
        {
            if (!CanArrange) return false;
            bool removed = placement.Remove(id);
            if (removed && Phase == HalfLessonPhase.Repair) Phase = HalfLessonPhase.ArrangeHalf;
            return removed;
        }

        public SubmissionResult Submit()
        {
            if (!CanArrange) return SubmissionResult.Unavailable;
            if (Phase == HalfLessonPhase.ArrangeWhole)
            {
                if (!Quantity.Equals(new FractionValue(1, 1))) return SubmissionResult.NeedsRepair;
                Phase = HalfLessonPhase.Partition;
                return SubmissionResult.CorrectWhole;
            }
            if (!Quantity.Equals(new FractionValue(1, 2)))
            {
                Phase = HalfLessonPhase.Repair;
                return SubmissionResult.NeedsRepair;
            }
            Phase = HalfLessonPhase.Complete;
            return SubmissionResult.CorrectHalf;
        }

        public bool PartitionIntoHalves()
        {
            if (Phase != HalfLessonPhase.Partition) return false;
            // Presentation swaps the whole for two prebuilt halves in the tray.
            // Keep their common whole identity; the answer lane begins empty.
            placement.Clear();
            PartitionCount = 2;
            Phase = HalfLessonPhase.ArrangeHalf;
            return true;
        }

        public void ResetTask()
        {
            AttemptGeneration++;
            placement.Clear();
            if (PartitionCount == 2) Phase = HalfLessonPhase.ArrangeHalf;
            else if (Phase != HalfLessonPhase.Briefing && Phase != HalfLessonPhase.GrabPractice)
                Phase = HalfLessonPhase.ArrangeWhole;
        }

        public void Replay()
        {
            AttemptGeneration++;
            placement.Clear();
            PartitionCount = 1;
            Phase = HalfLessonPhase.Briefing;
        }

        bool CanArrange => Phase == HalfLessonPhase.ArrangeWhole ||
            Phase == HalfLessonPhase.ArrangeHalf || Phase == HalfLessonPhase.Repair;
    }
}
