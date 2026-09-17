using System.Collections.Generic;
using System.Collections.ObjectModel;
using Airlift.Math;

namespace Airlift.Lessons
{
    /// <summary>
    /// One Dock 7 chapter: story copy, starting crates and the floor load that is accepted.
    /// Pure data. One container is one whole (8 cells); every fraction refers to it.
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
        public int[] LockedPieces;       // denominators pre-loaded on the floor and locked
        public int SplitTo;              // largest denominator Split can reach; 0 = no splitting
        public FractionValue Target;     // floor quantity that is accepted
        public int RequiredDenominator;  // 0 = any; else every unlocked loaded piece must have it
        public bool ShowHalfMark;        // chapter 4 draws the 1/2 mark on the container floor
        public string Expression;        // shown once accepted
        public string Accepted;          // one-sentence payoff

        /// Clause after "That fills ..., but " when a loaded piece has the wrong size. Null when unused.
        public string WrongPieces;
        /// Full sentence when the floor holds more than Target. Null uses a generic line.
        public string TooMuch;

        static readonly ReadOnlyCollection<CargoChapter> all = new ReadOnlyCollection<CargoChapter>(new[]
        {
            new CargoChapter
            {
                Number = 1, Id = "big_truck", Title = "Big truck",
                Story = "A big truck backed into Dock 7. It takes one full container of cargo.",
                Task = "Load the crate along the container floor from 0 to 1, then check the load.",
                VehicleKind = "truck", VehicleCount = 1,
                StartPieces = new[] { 1 }, LockedPieces = new int[0], SplitTo = 0,
                Target = new FractionValue(1, 1), RequiredDenominator = 0, ShowHalfMark = false,
                Expression = "1",
                Accepted = "The big truck is loaded with one full container.",
            },
            new CargoChapter
            {
                Number = 2, Id = "two_pickups", Title = "Two pickups",
                Story = "Two pickups pulled in. Each pickup fits half a container.",
                Task = "Split the crate into two equal chunks, then load both.",
                VehicleKind = "pickup", VehicleCount = 2,
                StartPieces = new[] { 1 }, LockedPieces = new int[0], SplitTo = 2,
                Target = new FractionValue(1, 1), RequiredDenominator = 2, ShowHalfMark = false,
                Expression = "1/2 + 1/2 = 1",
                Accepted = "Both pickups are loaded.",
                WrongPieces = "the pickups need halves. Split the crate first.",
            },
            new CargoChapter
            {
                Number = 3, Id = "four_vans", Title = "Four vans",
                Story = "Four delivery vans pulled in. Each van fits one quarter of a container.",
                Task = "Split each half crate into two equal chunks, then load all four.",
                VehicleKind = "van", VehicleCount = 4,
                StartPieces = new[] { 2, 2 }, LockedPieces = new int[0], SplitTo = 4,
                Target = new FractionValue(1, 1), RequiredDenominator = 4, ShowHalfMark = false,
                Expression = "1/4 + 1/4 + 1/4 + 1/4 = 1",
                Accepted = "All four vans are loaded.",
                WrongPieces = "the vans need quarters. Split each half crate first.",
            },
            new CargoChapter
            {
                Number = 4, Id = "same_share", Title = "Same share, smaller boxes",
                Story = "A pickup wants half a container, but only quarter boxes are left.",
                Task = "Load quarter boxes up to the half mark, then check the load.",
                VehicleKind = "pickup", VehicleCount = 1,
                StartPieces = new[] { 4, 4, 4, 4 }, LockedPieces = new int[0], SplitTo = 0,
                Target = new FractionValue(1, 2), RequiredDenominator = 4, ShowHalfMark = true,
                Expression = "2/4 = 1/2",
                Accepted = "The pickup is loaded with half a container.",
                WrongPieces = "this pickup's boxes must be quarters.",
                TooMuch = "That is more than this pickup takes. It needs 1/2 of a container.",
            },
            new CargoChapter
            {
                Number = 5, Id = "top_it_up", Title = "Top it up",
                Story = "This container is already half full. Fill it with quarter boxes.",
                Task = "Add quarter boxes until the container is full, then check the load.",
                VehicleKind = "truck", VehicleCount = 1,
                StartPieces = new[] { 4, 4, 4, 4 }, LockedPieces = new[] { 2 }, SplitTo = 0,
                Target = new FractionValue(1, 1), RequiredDenominator = 4, ShowHalfMark = false,
                Expression = "1/2 + 1/4 + 1/4 = 1",
                Accepted = "The container is full and ready to ship.",
                WrongPieces = "the top-up must be quarter boxes.",
            },
        });

        /// The five Dock 7 chapters in order.
        public static IReadOnlyList<CargoChapter> All => all;
    }
}
