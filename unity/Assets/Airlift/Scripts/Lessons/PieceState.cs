using System;
using Airlift.Math;

namespace Airlift.Lessons
{
    public sealed class PieceState
    {
        public string Id { get; }
        public string WholeId { get; }
        public FractionNotation Notation { get; }
        public int Cells { get; }

        public PieceState(string id, string wholeId, int denominator)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Piece ID required.", nameof(id));
            if (string.IsNullOrWhiteSpace(wholeId)) throw new ArgumentException("Whole ID required.", nameof(wholeId));
            if (denominator != 1 && denominator != 2 && denominator != 4 && denominator != 8)
                throw new ArgumentOutOfRangeException(nameof(denominator));
            Id = id;
            WholeId = wholeId;
            Notation = new FractionNotation(1, denominator);
            Cells = PlacementState.CellsPerWhole / denominator;
        }
    }
}
