using System;
using System.Collections.Generic;
using Airlift.Math;

namespace Airlift.Lessons
{
    public enum CargoLessonStage { Whole, Halves, Rebuilt }

    /// <summary>
    /// Deterministic whole-and-halves chapter. Pure C#: no SDK, scene or timing.
    /// One whole (8 cells) is either a single piece labeled 1 or two pieces labeled 1/2.
    /// The ruler (PlacementState) accepts neutral docking; correctness is evaluated on Submit only.
    /// </summary>
    public sealed class CargoLessonModel
    {
        public const string WholeId = "strap-A";
        readonly PlacementState ruler = new PlacementState(WholeId);
        readonly Dictionary<string, PieceState> tray = new Dictionary<string, PieceState>();
        public CargoLessonStage Stage { get; private set; } = CargoLessonStage.Whole;
        public int Generation { get; private set; }
        public FractionValue RulerQuantity => ruler.Quantity;
        public int RulerCount => ruler.Count;
        public IReadOnlyCollection<string> PieceIds => tray.Keys;
        public string LastFeedback { get; private set; } = "";

        public CargoLessonModel()
        {
            tray["whole"] = new PieceState("whole", WholeId, 1);
        }

        public PieceState Piece(string id) => tray.TryGetValue(id, out var piece) ? piece : null;
        public bool IsDocked(string id) { for (int i = 0; i < ruler.Count; i++) if (ruler[i].Id == id) return true; return false; }
        public int DockedIndex(string id) { for (int i = 0; i < ruler.Count; i++) if (ruler[i].Id == id) return i; return -1; }
        public int StartCell(string id) { int i = DockedIndex(id); return i < 0 ? -1 : ruler.StartCell(i); }

        /// Docking is neutral: any piece of this whole may be packed onto the ruler in release order.
        public bool Dock(string id, bool held)
        {
            if (held || !tray.TryGetValue(id, out var piece) || IsDocked(id)) return false;
            return ruler.TryInsert(piece, ruler.Count);
        }

        public bool Undock(string id) => ruler.Remove(id);

        /// Split replaces the single whole by two half pieces exactly once. Held or docked pieces refuse.
        public bool Split(bool held, int expectedGeneration)
        {
            if (Stage != CargoLessonStage.Whole || held || expectedGeneration != Generation) return false;
            if (!tray.ContainsKey("whole")) return false;
            ruler.Clear();
            tray.Clear();
            tray["half-1"] = new PieceState("half-1", WholeId, 2);
            tray["half-2"] = new PieceState("half-2", WholeId, 2);
            Generation++;
            Stage = CargoLessonStage.Halves;
            LastFeedback = "The whole became two equal parts. Each part is one half: 1/2.";
            return true;
        }

        /// Evaluate the ruler after halves exist. Wrong arrangements stay editable.
        public bool Submit()
        {
            if (Stage == CargoLessonStage.Whole)
            {
                bool whole = ruler.Quantity.Equals(new FractionValue(1, 1));
                LastFeedback = whole ? "The strap covers the ruler from 0 to 1. That length is one whole." : "Place the strap along the ruler from 0 to 1, then submit.";
                return whole;
            }
            if (Stage == CargoLessonStage.Rebuilt) return true;
            var quantity = ruler.Quantity;
            if (quantity.Equals(new FractionValue(1, 1)))
            {
                Stage = CargoLessonStage.Rebuilt;
                LastFeedback = "Two halves cover the whole ruler: 1/2 + 1/2 = 1.";
                return true;
            }
            LastFeedback = quantity.Equals(new FractionValue(1, 2))
                ? "One half reaches the middle mark. Add the other half to rebuild the whole."
                : "Dock both halves on the ruler, side by side from 0, then submit.";
            return false;
        }

        public void ResetPieces()
        {
            Generation++;
            ruler.Clear();
        }
    }
}
