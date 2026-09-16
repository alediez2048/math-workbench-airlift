using System;
using System.Collections.Generic;
using Airlift.Math;

namespace Airlift.Lessons
{
    /// <summary>One fixed whole. Ordered pieces are packed from zero, never filtered by the answer.</summary>
    public sealed class PlacementState
    {
        public const int CellsPerWhole = 8;
        readonly List<PieceState> pieces = new List<PieceState>(CellsPerWhole);
        public string WholeId { get; }
        public int Count => pieces.Count;
        public int OccupiedCells { get; private set; }
        public FractionValue Quantity => new FractionValue(OccupiedCells, CellsPerWhole);
        public PieceState this[int index] => pieces[index];

        public PlacementState(string wholeId)
        {
            if (string.IsNullOrWhiteSpace(wholeId)) throw new ArgumentException("Whole ID required.", nameof(wholeId));
            WholeId = wholeId;
        }

        public bool TryInsert(PieceState piece, int index)
        {
            if (piece == null || index < 0 || index > Count || piece.WholeId != WholeId ||
                OccupiedCells + piece.Cells > CellsPerWhole) return false;
            for (int i = 0; i < Count; i++) if (pieces[i].Id == piece.Id) return false;
            pieces.Insert(index, piece);
            OccupiedCells += piece.Cells;
            return true;
        }

        public bool Remove(string id)
        {
            for (int i = 0; i < Count; i++)
            {
                if (pieces[i].Id != id) continue;
                OccupiedCells -= pieces[i].Cells;
                pieces.RemoveAt(i);
                return true;
            }
            return false;
        }

        public int StartCell(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int start = 0;
            for (int i = 0; i < index; i++) start += pieces[i].Cells;
            return start;
        }

        public void Clear() { pieces.Clear(); OccupiedCells = 0; }
    }
}
