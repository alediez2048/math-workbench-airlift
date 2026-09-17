using System.Collections.Generic;
using System.Collections.ObjectModel;
using Airlift.Math;

namespace Airlift.Lessons
{
    /// <summary>
    /// One Dock 7 chapter: story copy, starting crates, the vehicle beds and the load that is accepted.
    /// Pure data. One container length is one whole (8 cells); every fraction refers to it. The chapter's
    /// vehicles back up side by side over that length, so their beds are segments of it from cell 0.
    /// </summary>
    public sealed class CargoChapter
    {
        public int Number;               // 1..5
        public string Id;                // "big_truck" | "two_pickups" | "four_vans" | "same_share" | "top_it_up"
        public string Title;             // "Big truck"
        public string Story;             // one or two sentences of Dock 7 story
        public string Task;              // one sentence: what to do now
        public string VehicleKind;       // "truck" | "pickup" | "van"
        public int VehicleCount;         // 1, 2, 4, 1, 1
        public int[] StartPieces;        // denominators of loose tray pieces at chapter start
        public int[] LockedPieces;       // denominators pre-loaded into the first bed that fits, and locked
        public int SplitTo;              // largest denominator Split can reach; 0 = no splitting
        public FractionValue Target;     // total quantity that is accepted (equals the sum of BedCells)
        public int RequiredDenominator;  // 0 = any; else every unlocked loaded piece must have it
        public bool ShowHalfMark;        // chapter 4 draws the 1/2 mark on the dock-edge strip
        public string Expression;        // shown once accepted
        public string Accepted;          // one-sentence payoff
        public int[] BedCells;           // per vehicle bed, left to right from cell 0; sum <= 8; Length == VehicleCount

        /// Clause after "That fills ..., but " when a loaded piece has the wrong size. Null when unused.
        public string WrongPieces;
        /// Full sentence when the crates hold more than the beds take. Null uses a generic line.
        public string TooMuch;

        static readonly ReadOnlyCollection<CargoChapter> all = new ReadOnlyCollection<CargoChapter>(new[]
        {
            new CargoChapter
            {
                Number = 1, Id = "big_truck", Title = "Big truck",
                Story = "The cranes set a full crate on the dock. A big truck backed up, and its bed fits one whole container.",
                Task = "Load the crate into the truck bed, then check the load.",
                VehicleKind = "truck", VehicleCount = 1,
                StartPieces = new[] { 1 }, LockedPieces = new int[0], SplitTo = 0,
                Target = new FractionValue(1, 1), RequiredDenominator = 0, ShowHalfMark = false,
                Expression = "1",
                Accepted = "The big truck drives off with one whole container.",
                BedCells = new[] { 8 },
            },
            new CargoChapter
            {
                Number = 2, Id = "two_pickups", Title = "Two pickups",
                Story = "Two pickups backed up to the dock. Each pickup bed fits half a container.",
                Task = "Split the crate into two equal chunks, then load one into each pickup.",
                VehicleKind = "pickup", VehicleCount = 2,
                StartPieces = new[] { 1 }, LockedPieces = new int[0], SplitTo = 2,
                Target = new FractionValue(1, 1), RequiredDenominator = 2, ShowHalfMark = false,
                Expression = "1/2 + 1/2 = 1",
                Accepted = "Both pickups drive off with half a container each.",
                BedCells = new[] { 4, 4 },
                WrongPieces = "the pickups need halves. Split the crate first.",
            },
            new CargoChapter
            {
                Number = 3, Id = "four_vans", Title = "Four vans",
                Story = "Four delivery vans backed up to the dock. Each van fits one quarter of a container.",
                Task = "Split each half crate into two equal chunks, then load one into each van.",
                VehicleKind = "van", VehicleCount = 4,
                StartPieces = new[] { 2, 2 }, LockedPieces = new int[0], SplitTo = 4,
                Target = new FractionValue(1, 1), RequiredDenominator = 4, ShowHalfMark = false,
                Expression = "1/4 + 1/4 + 1/4 + 1/4 = 1",
                Accepted = "All four vans drive off with a quarter of a container each.",
                BedCells = new[] { 2, 2, 2, 2 },
                WrongPieces = "the vans need quarters. Split each half crate first.",
            },
            new CargoChapter
            {
                Number = 4, Id = "same_share", Title = "Same share, smaller boxes",
                Story = "One pickup backed up for half a container, but the cranes only unloaded quarter boxes.",
                Task = "Fill the pickup bed with quarter boxes, then check the load.",
                VehicleKind = "pickup", VehicleCount = 1,
                StartPieces = new[] { 4, 4, 4, 4 }, LockedPieces = new int[0], SplitTo = 0,
                Target = new FractionValue(1, 2), RequiredDenominator = 4, ShowHalfMark = true,
                Expression = "2/4 = 1/2",
                Accepted = "The pickup drives off with half a container in quarter boxes.",
                BedCells = new[] { 4 },
                WrongPieces = "this pickup's boxes must be quarters.",
                TooMuch = "That is more than this pickup takes. It needs 1/2 of a container.",
            },
            new CargoChapter
            {
                Number = 5, Id = "top_it_up", Title = "Top it up",
                Story = "This big truck is already half loaded. Top it up with the quarter boxes from the crane.",
                Task = "Add quarter boxes until the truck bed is full, then check the load.",
                VehicleKind = "truck", VehicleCount = 1,
                StartPieces = new[] { 4, 4, 4, 4 }, LockedPieces = new[] { 2 }, SplitTo = 0,
                Target = new FractionValue(1, 1), RequiredDenominator = 4, ShowHalfMark = false,
                Expression = "1/2 + 1/4 + 1/4 = 1",
                Accepted = "The topped-up truck drives off with one whole container.",
                BedCells = new[] { 8 },
                WrongPieces = "the top-up must be quarter boxes.",
            },
        });

        /// The five Dock 7 chapters in order.
        public static IReadOnlyList<CargoChapter> All => all;
    }
}
