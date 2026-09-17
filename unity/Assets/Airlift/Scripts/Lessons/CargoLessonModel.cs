using System;
using System.Collections.Generic;
using Airlift.Math;

namespace Airlift.Lessons
{
    /// <summary>
    /// Deterministic Dock 7 chapter engine. Pure C#: no SDK, scene or timing.
    /// One container length (8 cells) is the fixed whole. The chapter's vehicles back up side by side over it,
    /// and each vehicle bed is a segment of those cells starting at cell 0. Crates pack from the start of the
    /// bed they are dropped in. Docking is neutral; the load is judged on Submit only, and wrong loads stay
    /// docked and editable.
    /// Piece ids are fixed so views bind by id: whole; half-1, half-2; quarter-1..4 (eighth-1..8).
    /// </summary>
    public sealed class CargoLessonModel
    {
        public const string WholeId = "strap-A";
        public const string EmptyFloorFeedback = "Nothing is loaded yet. Put crates into the vehicle beds, then check.";

        readonly Dictionary<string, PieceState> pieces = new Dictionary<string, PieceState>();
        readonly List<string> order = new List<string>();
        readonly HashSet<string> locked = new HashSet<string>();
        List<PieceState>[] beds = new List<PieceState>[0];
        int[] bedStart = new int[0];

        public CargoChapter Chapter { get; private set; }
        public int ChapterIndex { get; private set; }
        public bool ChapterComplete { get; private set; }
        public bool IsLastChapter => ChapterIndex == CargoChapter.All.Count - 1;
        public bool AllChaptersComplete => IsLastChapter && ChapterComplete;
        public int Generation { get; private set; }
        public FractionValue RulerQuantity => new FractionValue(LoadedCells, PlacementState.CellsPerWhole);
        public int RulerCount { get { int n = 0; foreach (var bed in beds) n += bed.Count; return n; } }
        public IReadOnlyCollection<string> PieceIds => order;
        public string LastFeedback { get; private set; } = "";
        public string Expression => ChapterComplete ? Chapter.Expression : "";

        public CargoLessonModel() { StartChapter(0); }

        // ---- pieces ----

        public PieceState Piece(string id) => id != null && pieces.TryGetValue(id, out var piece) ? piece : null;
        public bool IsDocked(string id) => BedOf(id) >= 0;
        public bool IsLocked(string id) => id != null && locked.Contains(id);

        /// Left-to-right index among all docked crates, across beds; -1 when not docked.
        public int DockedIndex(string id)
        {
            int index = 0;
            foreach (var bed in beds)
                foreach (var piece in bed) { if (piece.Id == id) return index; index++; }
            return -1;
        }

        /// Global cell where a docked crate starts: bed start plus the crates packed before it in that bed.
        public int StartCell(string id)
        {
            for (int b = 0; b < beds.Length; b++)
            {
                int cell = bedStart[b];
                foreach (var piece in beds[b]) { if (piece.Id == id) return cell; cell += piece.Cells; }
            }
            return -1;
        }

        // ---- beds ----

        public int BedCount => beds.Length;
        public int BedStartCell(int bed) => ValidBed(bed) ? bedStart[bed] : -1;
        public int BedCapacity(int bed) => ValidBed(bed) ? Chapter.BedCells[bed] : 0;
        public int BedFill(int bed) { if (!ValidBed(bed)) return 0; int n = 0; foreach (var p in beds[bed]) n += p.Cells; return n; }

        /// Bed covering a global cell; -1 when the cell is outside this chapter's beds.
        public int BedAtCell(int cell)
        {
            for (int b = 0; b < beds.Length; b++)
                if (cell >= bedStart[b] && cell < bedStart[b] + Chapter.BedCells[b]) return b;
            return -1;
        }

        public int BedOf(string id)
        {
            if (id == null) return -1;
            for (int b = 0; b < beds.Length; b++)
                foreach (var piece in beds[b]) if (piece.Id == id) return b;
            return -1;
        }

        /// Name used in feedback: "The pickup" for a lone vehicle, "Van 3" when there are several.
        public static string VehicleName(CargoChapter chapter, int bed)
        {
            string noun = VehicleNoun(chapter);
            int count = chapter.BedCells != null ? chapter.BedCells.Length : chapter.VehicleCount;
            return count <= 1 ? "The " + noun : char.ToUpperInvariant(noun[0]) + noun.Substring(1) + " " + (bed + 1);
        }

        // ---- actions ----

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

        /// Packs the crate at the end of that bed's load. Refuses held, unknown, locked, already docked or a bad
        /// bed silently; refuses a crate longer than the bed's free space with a size message in LastFeedback.
        public bool Dock(string id, bool held, int bed)
        {
            if (held || IsLocked(id) || !ValidBed(bed)) return false;
            var piece = Piece(id);
            if (piece == null || IsDocked(id)) return false;
            int free = BedCapacity(bed) - BedFill(bed);
            if (piece.Cells > free)
            {
                LastFeedback = TooLongFeedback(bed, piece.Cells, free);
                return false;
            }
            beds[bed].Add(piece);
            return true;
        }

        /// Docks into the first bed, left to right, with room for the whole crate.
        public bool Dock(string id, bool held)
        {
            if (held || IsLocked(id)) return false;
            var piece = Piece(id);
            if (piece == null || IsDocked(id)) return false;
            for (int b = 0; b < beds.Length; b++)
                if (BedCapacity(b) - BedFill(b) >= piece.Cells) return Dock(id, false, b);
            int widest = 0;
            for (int b = 0; b < beds.Length; b++) widest = System.Math.Max(widest, BedCapacity(b));
            if (piece.Cells > widest || beds.Length == 1)
                LastFeedback = TooLongFeedback(0, piece.Cells, BedCapacity(0) - BedFill(0));
            else
                LastFeedback = "No " + VehicleNoun(Chapter) + " has room left for that crate.";
            return false;
        }

        public bool Undock(string id)
        {
            if (id == null || IsLocked(id)) return false;
            foreach (var bed in beds)
                for (int i = 0; i < bed.Count; i++)
                    if (bed[i].Id == id) { bed.RemoveAt(i); return true; }
            return false;
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

            RemoveUnlockedFromBeds();
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

        /// Accepts only when every bed is exactly full and the denominator rule holds. Accepting sets
        /// ChapterComplete, which stays true until the chapter restarts. Wrong loads stay docked.
        public bool Submit()
        {
            var fills = new int[beds.Length];
            var loadedDens = new List<int>();
            for (int b = 0; b < beds.Length; b++)
                foreach (var piece in beds[b])
                {
                    fills[b] += piece.Cells;
                    if (!locked.Contains(piece.Id)) loadedDens.Add(piece.Notation.Denominator);
                }
            bool ok = Judge(Chapter, fills, loadedDens, 0, out var feedback);
            LastFeedback = feedback;
            if (ok) ChapterComplete = true;
            return ok;
        }

        /// Pure verdict for a list of crates: locked then loaded crates are packed first-fit into the chapter's
        /// beds (as Dock(id, held) would). Locked crates count toward the fill but not the denominator rule.
        public static bool Evaluate(CargoChapter chapter, IReadOnlyList<int> lockedDenominators,
            IReadOnlyList<int> loadedDenominators, out string feedback)
        {
            if (chapter == null) throw new ArgumentNullException(nameof(chapter));
            var capacity = BedsOf(chapter);
            var fills = new int[capacity.Length];
            int overflow = 0;
            var loaded = new List<int>();
            if (lockedDenominators != null)
                foreach (int d in lockedDenominators) overflow += Pack(capacity, fills, CellsFor(d));
            if (loadedDenominators != null)
                foreach (int d in loadedDenominators) { loaded.Add(d); overflow += Pack(capacity, fills, CellsFor(d)); }
            return Judge(chapter, fills, loaded, overflow, out feedback);
        }

        /// Undocks every unlocked piece; split level and locked pieces stay.
        public void ResetPieces()
        {
            RemoveUnlockedFromBeds();
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
            pieces.Clear();
            order.Clear();
            locked.Clear();

            var capacity = BedsOf(Chapter);
            beds = new List<PieceState>[capacity.Length];
            bedStart = new int[capacity.Length];
            for (int b = 0, cell = 0; b < capacity.Length; cell += capacity[b], b++)
            {
                beds[b] = new List<PieceState>();
                bedStart[b] = cell;
            }

            var counters = new Dictionary<int, int>();
            foreach (int den in Chapter.LockedPieces ?? new int[0])
            {
                var piece = AddPiece(den, counters);
                locked.Add(piece.Id);
                int bed = -1;
                for (int b = 0; b < beds.Length && bed < 0; b++)
                    if (capacity[b] - BedFill(b) >= piece.Cells) bed = b;
                if (bed < 0) throw new InvalidOperationException("Locked pieces do not fit the beds in " + Chapter.Id);
                beds[bed].Add(piece);
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

        // ---- verdict ----

        static bool Judge(CargoChapter chapter, int[] fills, List<int> loadedDens, int overflowCells, out string feedback)
        {
            var capacity = BedsOf(chapter);
            int cells = overflowCells;
            foreach (int f in fills) cells += f;
            if (cells == 0) { feedback = EmptyFloorFeedback; return false; }

            if (chapter.RequiredDenominator != 0)
            {
                foreach (int d in loadedDens)
                {
                    if (d == chapter.RequiredDenominator) continue;
                    var quantity = new FractionValue(cells, PlacementState.CellsPerWhole);
                    string fills1 = quantity.Equals(new FractionValue(1, 1))
                        ? "That fills a whole container"
                        : "That fills " + Describe(quantity) + " of a container";
                    feedback = fills1 + ", but " + (chapter.WrongPieces ??
                        "these crates must be 1/" + chapter.RequiredDenominator + " chunks.");
                    return false;
                }
            }

            if (overflowCells > 0)
            {
                feedback = chapter.TooMuch ??
                    "That is more than these vehicles take. They need " + Describe(chapter.Target) + " of a container.";
                return false;
            }

            for (int b = 0; b < capacity.Length; b++)
            {
                int free = capacity[b] - fills[b];
                if (free <= 0) continue;
                string name = VehicleName(chapter, b);
                feedback = fills[b] == 0
                    ? name + " is still empty."
                    : name + " has room for " + Describe(new FractionValue(free, PlacementState.CellsPerWhole)) + " of a container more.";
                return false;
            }

            feedback = chapter.Accepted + " " + chapter.Expression;
            return true;
        }

        string TooLongFeedback(int bed, int crateCells, int free)
        {
            string noun = VehicleNoun(Chapter);
            if (crateCells > BedCapacity(bed))
                return "That crate is too long for this " + noun + "." + (CanSplit ? " Split it first." : "");
            if (free <= 0) return "This " + noun + " is already full.";
            return "That crate is too long for the room left in this " + noun + ". It has room for " +
                Describe(new FractionValue(free, PlacementState.CellsPerWhole)) + " of a container.";
        }

        static int Pack(int[] capacity, int[] fills, int cells)
        {
            for (int b = 0; b < capacity.Length; b++)
                if (capacity[b] - fills[b] >= cells) { fills[b] += cells; return 0; }
            return cells;
        }

        static int[] BedsOf(CargoChapter chapter)
        {
            var cells = chapter.BedCells != null && chapter.BedCells.Length > 0
                ? chapter.BedCells
                : new[] { PlacementState.CellsPerWhole };
            int sum = 0;
            foreach (int c in cells)
            {
                if (c <= 0) throw new InvalidOperationException("Bed sizes must be positive in " + chapter.Id);
                sum += c;
            }
            if (sum > PlacementState.CellsPerWhole)
                throw new InvalidOperationException("Beds exceed one container in " + chapter.Id);
            return cells;
        }

        static string VehicleNoun(CargoChapter chapter) =>
            chapter.VehicleKind == "truck" ? "big truck" : string.IsNullOrEmpty(chapter.VehicleKind) ? "vehicle" : chapter.VehicleKind;

        // ---- helpers ----

        bool ValidBed(int bed) => bed >= 0 && bed < beds.Length;

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

        void RemoveUnlockedFromBeds()
        {
            foreach (var bed in beds)
                for (int i = bed.Count - 1; i >= 0; i--)
                    if (!locked.Contains(bed[i].Id)) bed.RemoveAt(i);
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

        int LoadedCells { get { int n = 0; for (int b = 0; b < beds.Length; b++) n += BedFill(b); return n; } }

        static string Describe(FractionValue value) =>
            value.Denominator == 1 ? value.Numerator.ToString() : value.ToString();
    }
}
