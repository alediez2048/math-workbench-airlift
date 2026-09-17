using System;
using System.Collections.Generic;
using Airlift.Math;

namespace Airlift.Lessons
{
    /// <summary>
    /// Deterministic Dock 7 chapter engine. Pure C#: no SDK, scene or timing.
    /// One container (8 cells) is the fixed whole. Docking is neutral; the load is evaluated on Submit
    /// only, and wrong loads stay docked and editable.
    /// Piece ids are fixed so views bind by id: whole; half-1, half-2; quarter-1..4 (eighth-1..8).
    /// </summary>
    public sealed class CargoLessonModel
    {
        public const string WholeId = "strap-A";
        public const string EmptyFloorFeedback = "The container floor is empty. Load crates from 0, then check.";

        readonly PlacementState ruler = new PlacementState(WholeId);
        readonly Dictionary<string, PieceState> pieces = new Dictionary<string, PieceState>();
        readonly List<string> order = new List<string>();
        readonly HashSet<string> locked = new HashSet<string>();

        public CargoChapter Chapter { get; private set; }
        public int ChapterIndex { get; private set; }
        public bool ChapterComplete { get; private set; }
        public bool IsLastChapter => ChapterIndex == CargoChapter.All.Count - 1;
        public bool AllChaptersComplete => IsLastChapter && ChapterComplete;
        public int Generation { get; private set; }
        public FractionValue RulerQuantity => ruler.Quantity;
        public int RulerCount => ruler.Count;
        public IReadOnlyCollection<string> PieceIds => order;
        public string LastFeedback { get; private set; } = "";
        public string Expression => ChapterComplete ? Chapter.Expression : "";

        public CargoLessonModel() { StartChapter(0); }

        public PieceState Piece(string id) => id != null && pieces.TryGetValue(id, out var piece) ? piece : null;
        public bool IsDocked(string id) => DockedIndex(id) >= 0;
        public int DockedIndex(string id) { for (int i = 0; i < ruler.Count; i++) if (ruler[i].Id == id) return i; return -1; }
        public int StartCell(string id) { int i = DockedIndex(id); return i < 0 ? -1 : ruler.StartCell(i); }
        public bool IsLocked(string id) => id != null && locked.Contains(id);

        /// True when this chapter allows splitting and some unlocked piece is still larger than SplitTo pieces.
        public bool CanSplit
        {
            get
            {
                if (Chapter.SplitTo <= 0) return false;
                foreach (var id in order)
                    if (!locked.Contains(id) && pieces[id].Notation.Denominator < Chapter.SplitTo) return true;
                return false;
            }
        }

        /// Docking is neutral: any unlocked piece may be packed onto the floor in release order.
        public bool Dock(string id, bool held)
        {
            if (held || IsLocked(id)) return false;
            var piece = Piece(id);
            if (piece == null || IsDocked(id)) return false;
            return ruler.TryInsert(piece, ruler.Count);
        }

        public bool Undock(string id)
        {
            if (id == null || IsLocked(id)) return false;
            return ruler.Remove(id);
        }

        /// Undocks unlocked pieces, then splits every unlocked piece with denominator below SplitTo
        /// into its two fixed child ids. Total cells are conserved.
        public bool Split(bool held, int expectedGeneration)
        {
            if (held || expectedGeneration != Generation || !CanSplit) return false;

            var next = new List<string>(order.Count * 2);
            var created = new List<PieceState>();
            foreach (var id in order)
            {
                var piece = pieces[id];
                int den = piece.Notation.Denominator;
                if (locked.Contains(id) || den >= Chapter.SplitTo) { next.Add(id); continue; }
                int index = PieceIndex(id);
                for (int k = 0; k < 2; k++)
                {
                    var child = new PieceState(PieceId(den * 2, index * 2 - 1 + k), WholeId, den * 2);
                    created.Add(child);
                    next.Add(child.Id);
                }
            }
            var seen = new HashSet<string>();
            foreach (var id in next) if (!seen.Add(id)) return false;   // lineage collision: refuse, change nothing

            RemoveUnlockedFromFloor();
            var kept = new Dictionary<string, PieceState>();
            foreach (var id in next) if (pieces.TryGetValue(id, out var p)) kept[id] = p;
            pieces.Clear();
            foreach (var pair in kept) pieces[pair.Key] = pair.Value;
            foreach (var child in created) pieces[child.Id] = child;
            order.Clear();
            order.AddRange(next);
            Generation++;

            int childDen = created[0].Notation.Denominator;
            bool same = true;
            foreach (var child in created) same &= child.Notation.Denominator == childDen;
            string subject = created.Count == 2 ? "The crate is" : "The crates are";
            LastFeedback = subject + " split into " + created.Count + " equal chunks." +
                (same ? " Each chunk is 1/" + childDen + " of a container." : "");
            return true;
        }

        /// Evaluates the floor against the chapter. Accepting sets ChapterComplete, which stays true until the
        /// chapter restarts. Wrong loads stay docked.
        public bool Submit()
        {
            var lockedDens = new List<int>();
            var loadedDens = new List<int>();
            for (int i = 0; i < ruler.Count; i++)
            {
                int den = ruler[i].Notation.Denominator;
                if (locked.Contains(ruler[i].Id)) lockedDens.Add(den); else loadedDens.Add(den);
            }
            bool ok = Evaluate(Chapter, lockedDens, loadedDens, out var feedback);
            LastFeedback = feedback;
            if (ok) ChapterComplete = true;
            return ok;
        }

        /// Pure verdict for a floor: locked pieces count toward the quantity but not the denominator rule.
        public static bool Evaluate(CargoChapter chapter, IReadOnlyList<int> lockedDenominators,
            IReadOnlyList<int> loadedDenominators, out string feedback)
        {
            if (chapter == null) throw new ArgumentNullException(nameof(chapter));
            int cells = 0;
            if (lockedDenominators != null) foreach (int d in lockedDenominators) cells += CellsFor(d);
            if (loadedDenominators != null) foreach (int d in loadedDenominators) cells += CellsFor(d);
            if (cells == 0) { feedback = EmptyFloorFeedback; return false; }

            var quantity = new FractionValue(cells, PlacementState.CellsPerWhole);
            if (chapter.RequiredDenominator != 0 && loadedDenominators != null)
            {
                foreach (int d in loadedDenominators)
                {
                    if (d == chapter.RequiredDenominator) continue;
                    string fills = quantity.Equals(new FractionValue(1, 1))
                        ? "That fills the container"
                        : "That fills " + Describe(quantity) + " of the container";
                    feedback = fills + ", but " + (chapter.WrongPieces ??
                        "these crates must be 1/" + chapter.RequiredDenominator + " chunks.");
                    return false;
                }
            }

            int targetCells = TargetCells(chapter.Target);
            if (cells > targetCells)
            {
                feedback = chapter.TooMuch ??
                    "That is more than this load takes. It needs " + Describe(chapter.Target) + " of a container.";
                return false;
            }
            if (cells < targetCells)
            {
                feedback = "That fills " + Describe(quantity) + " of the container. " +
                    Describe(new FractionValue(targetCells - cells, PlacementState.CellsPerWhole)) + " more fits.";
                return false;
            }
            feedback = chapter.Accepted + " " + chapter.Expression;
            return true;
        }

        /// Undocks every unlocked piece; split level and locked pieces stay.
        public void ResetPieces()
        {
            RemoveUnlockedFromFloor();
            Generation++;
            LastFeedback = "";
        }

        public void RestartChapter() => StartChapter(ChapterIndex);

        public bool NextChapter()
        {
            if (!ChapterComplete || IsLastChapter) return false;
            StartChapter(ChapterIndex + 1);
            return true;
        }

        public void StartChapter(int index)
        {
            if (index < 0 || index >= CargoChapter.All.Count) throw new ArgumentOutOfRangeException(nameof(index));
            ChapterIndex = index;
            Chapter = CargoChapter.All[index];
            ChapterComplete = false;
            LastFeedback = "";
            ruler.Clear();
            pieces.Clear();
            order.Clear();
            locked.Clear();

            var counters = new Dictionary<int, int>();
            foreach (int den in Chapter.LockedPieces ?? new int[0])
            {
                var piece = AddPiece(den, counters);
                locked.Add(piece.Id);
                if (!ruler.TryInsert(piece, ruler.Count))
                    throw new InvalidOperationException("Locked pieces overfill the container in " + Chapter.Id);
            }
            foreach (int den in Chapter.StartPieces ?? new int[0]) AddPiece(den, counters);
            Generation++;
        }

        /// Fixed id for the index-th (1-based) piece of a denominator: "whole", "half-2", "quarter-3", "eighth-8".
        public static string PieceId(int denominator, int index)
        {
            switch (denominator)
            {
                case 1: return "whole";
                case 2: return "half-" + index;
                case 4: return "quarter-" + index;
                case 8: return "eighth-" + index;
                default: throw new ArgumentOutOfRangeException(nameof(denominator));
            }
        }

        PieceState AddPiece(int den, Dictionary<int, int> counters)
        {
            counters.TryGetValue(den, out int n);
            counters[den] = ++n;
            var piece = new PieceState(PieceId(den, n), WholeId, den);
            if (pieces.ContainsKey(piece.Id))
                throw new InvalidOperationException("Duplicate piece id " + piece.Id + " in " + Chapter.Id);
            pieces[piece.Id] = piece;
            order.Add(piece.Id);
            return piece;
        }

        void RemoveUnlockedFromFloor()
        {
            for (int i = ruler.Count - 1; i >= 0; i--)
                if (!locked.Contains(ruler[i].Id)) ruler.Remove(ruler[i].Id);
        }

        static int PieceIndex(string id)
        {
            if (id == "whole") return 1;
            int dash = id.LastIndexOf('-');
            return dash >= 0 && int.TryParse(id.Substring(dash + 1), out int n) ? n : 1;
        }

        static int CellsFor(int denominator)
        {
            if (denominator != 1 && denominator != 2 && denominator != 4 && denominator != 8)
                throw new ArgumentOutOfRangeException(nameof(denominator));
            return PlacementState.CellsPerWhole / denominator;
        }

        static int TargetCells(FractionValue target) =>
            target.Numerator * PlacementState.CellsPerWhole / target.Denominator;

        static string Describe(FractionValue value) =>
            value.Denominator == 1 ? value.Numerator.ToString() : value.ToString();
    }
}
