using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Airlift.Lessons.Garden
{
    /// <summary>
    /// One Sunny Plot chapter: story copy, the bed at chapter start, the seedling strips in the tray and what is
    /// accepted. Pure data. Convention everywhere is rows × columns: a 3 × 4 bed is 3 rows of 4 seedlings, and
    /// one seedling strip fills one row.
    /// </summary>
    public sealed class GardenChapter
    {
        public int Number;               // 1..5
        public string Id;                // "first_rows" | "equal_rows" | "turn_the_bed" | "split_the_bed" | "your_own_split"
        public string Title;             // "First rows"
        public string Story;             // one or two sentences of Sunny Plot story
        public string Task;              // one sentence: what to do now
        public int Rows, Columns;        // bed at chapter start
        public int[] StripLengths;       // strips in the tray (chapters 1-2); planted strips are generated for 3-5
        public bool StartsPlanted;       // every row starts planted with a full strip
        public bool NeedsTurn;           // accepted only after a quarter turn
        public bool HasFence;            // a fence can be put between two columns
        public int RequiredFenceColumn;  // chapter 4: 5; chapter 5: 0 = any column 1..Columns-1
        public string Expression;        // shown once accepted; chapter 5 is built at runtime from the fence ("")
        public string Accepted;          // one-sentence payoff

        static readonly ReadOnlyCollection<GardenChapter> all = new ReadOnlyCollection<GardenChapter>(new[]
        {
            new GardenChapter
            {
                Number = 1, Id = "first_rows", Title = "First rows",
                Story = "Welcome to Sunny Plot. This bed has 3 rows, and each row has room for 4 seedlings.",
                Task = "Plant a strip of 4 seedlings in each row, then check the bed.",
                Rows = 3, Columns = 4, StripLengths = new[] { 4, 4, 4, 3, 3 },
                StartsPlanted = false, NeedsTurn = false, HasFence = false, RequiredFenceColumn = 0,
                Expression = "3 × 4 = 12",
                Accepted = "Your 3 rows of 4 sprout into lettuce.",
            },
            new GardenChapter
            {
                Number = 2, Id = "equal_rows", Title = "Equal rows",
                Story = "This bed is bigger: 4 rows, with room for 5 seedlings in each row. The tray has strips of different lengths.",
                Task = "Plant a strip that fills each row, then check the bed.",
                Rows = 4, Columns = 5, StripLengths = new[] { 4, 5, 5, 5, 5, 6 },
                StartsPlanted = false, NeedsTurn = false, HasFence = false, RequiredFenceColumn = 0,
                Expression = "4 × 5 = 20",
                Accepted = "Your 4 rows of 5 sprout into carrots.",
            },
            new GardenChapter
            {
                Number = 3, Id = "turn_the_bed", Title = "Turn the bed",
                Story = "Your first bed is planted: 3 rows of 4. Neighbors on the other path see it from the side.",
                Task = "Turn the bed a quarter turn, then check it.",
                Rows = 3, Columns = 4, StripLengths = new int[0],
                StartsPlanted = true, NeedsTurn = true, HasFence = false, RequiredFenceColumn = 0,
                Expression = "3 × 4 = 4 × 3 = 12",
                Accepted = "From the side path the bed reads 4 rows of 3, with the same seedlings.",
            },
            new GardenChapter
            {
                Number = 4, Id = "split_the_bed", Title = "Split the bed",
                Story = "This bed has 7 rows of 6, too many to count one by one. A fence can split it into two smaller parts.",
                Task = "Put the fence after column 5, then check the bed.",
                Rows = 7, Columns = 6, StripLengths = new int[0],
                StartsPlanted = true, NeedsTurn = false, HasFence = true, RequiredFenceColumn = 5,
                Expression = "7 × 6 = 7 × 5 + 7 × 1 = 42",
                Accepted = "The fence opens on both parts: 7 rows of 5 and 7 rows of 1.",
            },
            new GardenChapter
            {
                Number = 5, Id = "your_own_split", Title = "Your own split",
                Story = "This bed has 8 rows of 7. This time you choose where the fence goes.",
                Task = "Put the fence between any two columns, then check the bed.",
                Rows = 8, Columns = 7, StripLengths = new int[0],
                StartsPlanted = true, NeedsTurn = false, HasFence = true, RequiredFenceColumn = 0,
                Expression = "",
                Accepted = "The fence opens and both parts bloom into sunflowers and beans.",
            },
        });

        /// The five Sunny Plot chapters in order.
        public static IReadOnlyList<GardenChapter> All => all;

        /// "7 × 6 = 7 × 5 + 7 × 1 = 42" for a fence after `fence` columns.
        public static string SplitExpression(int rows, int columns, int fence) =>
            rows + " × " + columns + " = " + rows + " × " + fence + " + " + rows + " × " + (columns - fence) +
            " = " + rows * columns;
    }
}
