using System;
using System.Collections.Generic;

namespace Airlift.Lessons.Garden
{
    /// <summary>
    /// Deterministic Sunny Plot chapter engine. Pure C#: no SDK, scene or timing.
    /// The bed is Rows × Columns (rows × columns: 3 × 4 is 3 rows of 4). One seedling strip fills at most one row.
    /// Rows are 0-based in the API and 1-based in feedback. Planting is neutral: short strips may be planted and
    /// the bed is judged on Check only, so wrong rows stay planted and editable. Accepting sets ChapterComplete,
    /// which stays true until the chapter restarts; the finished bed no longer changes.
    /// Strip ids are "strip-1".."strip-N", stable per chapter. A planted chapter generates one full strip per row,
    /// planted in row order; a turn regenerates them as rows of the new Columns with the same id scheme.
    /// </summary>
    public sealed class GardenModel
    {
        public const string EmptyBedFeedback = "Nothing is planted yet. Plant a strip in each row, then check the bed.";
        public const string NeedsTurnFeedback = "Turn the bed to see it from the other side.";
        public const string NeedsFenceFeedback = "Put the fence between two columns first.";
        public const string TurnNeedsFullRowsFeedback = "Plant every row with a full strip before turning the bed.";

        readonly Dictionary<string, int> lengths = new Dictionary<string, int>();
        readonly List<string> order = new List<string>();
        string[] rows = new string[0];
        int turns;
        string earned = "";

        public GardenChapter Chapter { get; private set; }
        public int ChapterIndex { get; private set; }
        public bool ChapterComplete { get; private set; }
        public bool IsLastChapter => ChapterIndex == GardenChapter.All.Count - 1;
        public bool AllChaptersComplete => IsLastChapter && ChapterComplete;
        public int Generation { get; private set; }
        public string LastFeedback { get; private set; } = "";
        /// The expression earned so far: "" until the chapter is accepted.
        public string Expression => ChapterComplete ? earned : "";

        /// Current bed size; Rows and Columns swap after each turn.
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        /// True after an odd number of quarter turns.
        public bool Turned => turns % 2 == 1;
        /// After column FenceColumn (1..Columns-1); 0 = no fence.
        public int FenceColumn { get; private set; }

        public GardenModel() { StartChapter(0); }

        // ---- strips and rows ----

        public IReadOnlyList<string> StripIds => order;
        public int StripLength(string id) => id != null && lengths.TryGetValue(id, out int n) ? n : 0;

        public int RowOf(string stripId)
        {
            if (stripId == null) return -1;
            for (int r = 0; r < rows.Length; r++) if (rows[r] == stripId) return r;
            return -1;
        }

        public string StripInRow(int row) => ValidRow(row) ? rows[row] : null;

        /// Seedlings in the bed: the sum of planted strip lengths.
        public int PlantCount
        {
            get { int n = 0; foreach (var id in rows) if (id != null) n += lengths[id]; return n; }
        }

        /// Strips not planted in any row, in id order.
        public IReadOnlyList<string> TrayStripIds
        {
            get { var tray = new List<string>(); foreach (var id in order) if (RowOf(id) < 0) tray.Add(id); return tray; }
        }

        public int EmptyRowCount { get { int n = 0; foreach (var id in rows) if (id == null) n++; return n; } }

        /// True when some tray strip fits some empty row.
        public bool CanPlantSomething
        {
            get
            {
                if (ChapterComplete || EmptyRowCount == 0) return false;
                foreach (var id in order) if (RowOf(id) < 0 && lengths[id] <= Columns) return true;
                return false;
            }
        }

        /// True when some planted row holds fewer seedlings than Columns.
        public bool HasShortRow
        {
            get { foreach (var id in rows) if (id != null && lengths[id] < Columns) return true; return false; }
        }

        // ---- actions ----

        /// Plants a tray strip into an empty row. Refuses held, unknown, already planted, a bad row or a finished
        /// bed silently; refuses an occupied row or a strip longer than the row with a message in LastFeedback.
        public bool Plant(string stripId, bool held, int row)
        {
            if (held || ChapterComplete || !ValidRow(row)) return false;
            if (stripId == null || !lengths.TryGetValue(stripId, out int length) || RowOf(stripId) >= 0) return false;
            if (rows[row] != null)
            {
                LastFeedback = "Row " + (row + 1) + " already has a strip.";
                return false;
            }
            if (length > Columns)
            {
                LastFeedback = "That strip has " + length + " seedlings; a row in this bed has room for " + Columns + ".";
                return false;
            }
            rows[row] = stripId;
            return true;
        }

        /// Takes a planted strip back to the tray.
        public bool Unplant(string stripId)
        {
            if (ChapterComplete) return false;
            int row = RowOf(stripId);
            if (row < 0) return false;
            rows[row] = null;
            return true;
        }

        /// A quarter turn: only in a turning chapter, and only when every row is planted with a full strip.
        /// Swaps Rows and Columns and regenerates the strips as rows of the new Columns, planted in row order.
        /// The number of seedlings does not change.
        public bool TurnBed(bool held)
        {
            if (held || ChapterComplete) return false;
            if (!Chapter.NeedsTurn)
            {
                LastFeedback = "This bed does not turn.";
                return false;
            }
            foreach (var id in rows)
                if (id == null || lengths[id] != Columns) { LastFeedback = TurnNeedsFullRowsFeedback; return false; }
            if (TrayStripIds.Count > 0) { LastFeedback = TurnNeedsFullRowsFeedback; return false; }

            int newRows = Columns, newColumns = Rows;
            Rows = newRows;
            Columns = newColumns;
            PlantFullRows();
            turns++;
            FenceColumn = 0;
            LastFeedback = "The bed turns. It now shows " + Rows + " rows of " + Columns + ".";
            Generation++;
            return true;
        }

        /// Puts the fence after `column` columns (1..Columns-1) in a fence chapter; it can be moved any time before
        /// the bed is accepted.
        public bool SetFence(int column, bool held)
        {
            if (held || ChapterComplete || !Chapter.HasFence) return false;
            if (column < 1 || column > Columns - 1)
            {
                LastFeedback = "The fence goes after column 1 to " + (Columns - 1) + ".";
                return false;
            }
            FenceColumn = column;
            return true;
        }

        /// Judges the bed. Rows first (empty, then short), then the chapter's turn or fence rule.
        public bool Check()
        {
            if (ChapterComplete) { LastFeedback = Chapter.Accepted + " " + earned; return true; }

            bool ok = Judge(out string feedback);
            if (ok)
            {
                earned = Chapter.HasFence && Chapter.RequiredFenceColumn == 0
                    ? GardenChapter.SplitExpression(Rows, Columns, FenceColumn)
                    : Chapter.Expression;
                ChapterComplete = true;
                feedback = Chapter.Accepted + " " + earned;
            }
            LastFeedback = feedback;
            return ok;
        }

        /// Tray chapters: every strip back to the tray. Planted chapters: back to the planted start bed, unturned,
        /// with no fence. A finished bed does not reset; restart the chapter instead.
        public void ResetTable()
        {
            if (ChapterComplete) return;
            ResetBed();
            LastFeedback = "";
            Generation++;
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
            if (index < 0 || index >= GardenChapter.All.Count) throw new ArgumentOutOfRangeException(nameof(index));
            ChapterIndex = index;
            Chapter = GardenChapter.All[index];
            ChapterComplete = false;
            earned = "";
            LastFeedback = "";
            ResetBed();
            Generation++;
        }

        // ---- verdict ----

        bool Judge(out string feedback)
        {
            int planted = 0;
            foreach (var id in rows) if (id != null) planted++;
            if (planted == 0) { feedback = EmptyBedFeedback; return false; }

            for (int r = 0; r < rows.Length; r++)
                if (rows[r] == null) { feedback = "Row " + (r + 1) + " is still empty."; return false; }

            var shortRows = new List<int>();
            for (int r = 0; r < rows.Length; r++) if (lengths[rows[r]] != Columns) shortRows.Add(r);
            if (shortRows.Count > 0) { feedback = ShortRowsFeedback(shortRows); return false; }

            if (Chapter.NeedsTurn && !Turned) { feedback = NeedsTurnFeedback; return false; }

            if (Chapter.HasFence)
            {
                if (FenceColumn == 0) { feedback = NeedsFenceFeedback; return false; }
                if (Chapter.RequiredFenceColumn != 0 && FenceColumn != Chapter.RequiredFenceColumn)
                {
                    feedback = "The fence is after column " + FenceColumn + ", so the parts are " +
                        Rows + " × " + FenceColumn + " and " + Rows + " × " + (Columns - FenceColumn) +
                        ". Put it after column " + Chapter.RequiredFenceColumn + ".";
                    return false;
                }
            }

            feedback = "";
            return true;
        }

        string ShortRowsFeedback(List<int> shortRows)
        {
            if (shortRows.Count == 1 && rows.Length > 1)
                return "Row " + (shortRows[0] + 1) + " has " + Seedlings(lengths[rows[shortRows[0]]]) +
                    "; the other rows have " + Columns + ".";

            bool allSame = true;
            int first = lengths[rows[shortRows[0]]];
            foreach (int r in shortRows) allSame &= lengths[rows[r]] == first;
            if (shortRows.Count == rows.Length && allSame)
                return "Every row has " + Seedlings(first) + "; a full row in this bed has " + Columns + ".";

            var parts = new List<string>();
            foreach (int r in shortRows) parts.Add("row " + (r + 1) + " has " + lengths[rows[r]]);
            string joined = parts.Count == 1
                ? parts[0]
                : string.Join(", ", parts.GetRange(0, parts.Count - 1)) + " and " + parts[parts.Count - 1];
            return char.ToUpperInvariant(joined[0]) + joined.Substring(1) + "; a full row in this bed has " + Columns + ".";
        }

        static string Seedlings(int n) => n + (n == 1 ? " seedling" : " seedlings");

        // ---- helpers ----

        bool ValidRow(int row) => row >= 0 && row < rows.Length;

        void ResetBed()
        {
            turns = 0;
            FenceColumn = 0;
            Rows = Chapter.Rows;
            Columns = Chapter.Columns;
            if (Chapter.StartsPlanted)
            {
                PlantFullRows();
                return;
            }
            lengths.Clear();
            order.Clear();
            var strips = Chapter.StripLengths ?? new int[0];
            for (int i = 0; i < strips.Length; i++)
            {
                if (strips[i] <= 0) throw new InvalidOperationException("Strip lengths must be positive in " + Chapter.Id);
                string id = StripId(i + 1);
                lengths[id] = strips[i];
                order.Add(id);
            }
            rows = new string[Rows];
        }

        /// One full strip per row, "strip-1" in row 1 and so on.
        void PlantFullRows()
        {
            lengths.Clear();
            order.Clear();
            rows = new string[Rows];
            for (int r = 0; r < Rows; r++)
            {
                string id = StripId(r + 1);
                lengths[id] = Columns;
                order.Add(id);
                rows[r] = id;
            }
        }

        public static string StripId(int number) => "strip-" + number;
    }
}
