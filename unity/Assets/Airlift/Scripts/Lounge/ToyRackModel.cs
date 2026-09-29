using System.Collections.Generic;
using System.Linq;

namespace Airlift.Lounge
{
    /// One slot of the toy rack: a chapter's toy, the object that chapter was about (spec 2026-09-29).
    public sealed class ToySlot
    {
        public readonly string LessonId; public readonly int Chapter; public readonly string ToyName; public readonly string Colour; public readonly string ChapterTitle;
        public string Id => LibraryState.TileId(LessonId, Chapter);
        public ToySlot(string lessonId, int chapter, string toyName, string colour, string chapterTitle = "") { LessonId = lessonId; Chapter = chapter; ToyName = toyName; Colour = colour; ChapterTitle = chapterTitle; }
    }

    /// A progress-board row: chapters completed per lesson as checkmarks and "n of 5". Never a score.
    public sealed class BoardRow
    {
        public readonly string LessonId, Title; public readonly bool[] Marks;
        public int Done => Marks.Count(m => m);
        public string Label => Done + " of " + Marks.Length;
        public BoardRow(string lessonId, string title, bool[] marks) { LessonId = lessonId; Title = title; Marks = marks; }
    }

    /// Pure rules for the lounge toy rack and its progress board. No Unity types: the scene side only reads these.
    public static class ToyRackModel
    {
        public const string Cargo = "cargo_crew_fractions", Cafe = "neighborhood_cafe_division", Garden = "community_garden_multiplication";
        static readonly (string id, string title)[] Lessons = { (Cargo, "Cargo Crew"), (Cafe, "Neighborhood Café"), (Garden, "Community Garden") };

        public static readonly IReadOnlyList<ToySlot> Slots = new[]
        {
            new ToySlot(Cargo, 1, "orange container", "#F28C28", "Big truck"),
            new ToySlot(Cargo, 2, "big truck", "#3C4CDB", "Two pickups"),
            new ToySlot(Cargo, 3, "pickup", "#17E2EA", "Four vans"),
            new ToySlot(Cargo, 4, "van", "#FB43DA", "Same share, smaller boxes"),
            new ToySlot(Cargo, 5, "dock crane", "#FFC32B", "Top it up"),
            new ToySlot(Cafe, 1, "plate of two pastries", "#E8E4F5", "Two friends"),
            new ToySlot(Cafe, 2, "café table", "#9E97FF", "Table of three"),
            new ToySlot(Cafe, 3, "pastry box", "#FB43DA", "Box it up"),
            new ToySlot(Cafe, 4, "tray of pastries", "#D28A4A", "Bigger order"),
            new ToySlot(Cafe, 5, "coffee pot", "#17E2EA", "Fact family"),
            new ToySlot(Garden, 1, "seed tray", "#7A5230", "First rows"),
            new ToySlot(Garden, 2, "planter bed", "#4CC24A", "Equal rows"),
            new ToySlot(Garden, 3, "watering can", "#FFC32B", "Turn the bed"),
            new ToySlot(Garden, 4, "fence piece", "#E8E4F5", "Split the bed"),
            new ToySlot(Garden, 5, "sunflower", "#FFC32B", "Your own split"),
        };

        public static ToySlot Find(string lessonId, int chapter) => Slots.FirstOrDefault(s => s.LessonId == lessonId && s.Chapter == chapter);
        public static IReadOnlyList<ToySlot> Unlocked(LibraryState library) => Slots.Where(s => library != null && library.IsCompleted(s.LessonId, s.Chapter)).ToList();
        /// Unlocked but not yet picked up: these glow on the rack and Dee mentions them once.
        public static IReadOnlyList<ToySlot> NewlyUnlocked(LibraryState library) => Unlocked(library).Where(s => !library.IsToySeen(s.LessonId, s.Chapter)).ToList();
        public static IReadOnlyList<BoardRow> BoardRows(LibraryState library) => Lessons
            .Select(l => new BoardRow(l.id, l.title, Enumerable.Range(1, 5).Select(c => library != null && library.IsCompleted(l.id, c)).ToArray()))
            .ToList();
        /// Pointing at a locked toy: what earns it, in one line.
        public static string LockedLine(ToySlot slot)
        {
            string lesson = "the lesson"; foreach (var l in Lessons) if (l.id == slot.LessonId) lesson = l.title;
            return "Finish " + slot.ChapterTitle + " in " + lesson + " to earn the " + slot.ToyName + ".";
        }
        /// Said once, in the lounge, after the chapter that earned it. No praise words, just the fact.
        public static string UnlockLine(ToySlot slot) => "Your " + slot.ToyName + " is on the rack.";
    }
}
