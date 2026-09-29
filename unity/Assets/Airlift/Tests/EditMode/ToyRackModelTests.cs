using System.Linq;
using Airlift.Lounge;
using NUnit.Framework;

namespace Airlift.Tests
{
    /// Toy rack (spec 2026-09-29): fifteen slots, one per chapter; unlocked by the library's completed chapters.
    public class ToyRackModelTests
    {
        [Test] public void FifteenSlotsInLessonAndChapterOrderWithTheSpecNames()
        {
            var slots = ToyRackModel.Slots;
            Assert.That(slots.Count, Is.EqualTo(15));
            Assert.That(slots.Take(5).All(s => s.LessonId == "cargo_crew_fractions"), Is.True);
            Assert.That(slots.Skip(5).Take(5).All(s => s.LessonId == "neighborhood_cafe_division"), Is.True);
            Assert.That(slots.Skip(10).All(s => s.LessonId == "community_garden_multiplication"), Is.True);
            Assert.That(slots.Select(s => s.Chapter).ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 }));
            Assert.That(slots[0].ToyName, Is.EqualTo("orange container"));
            Assert.That(slots[1].ToyName, Is.EqualTo("big truck"));
            Assert.That(slots[4].ToyName, Is.EqualTo("dock crane"));
            Assert.That(slots[9].ToyName, Is.EqualTo("coffee pot"));
            Assert.That(slots[14].ToyName, Is.EqualTo("sunflower"));
            Assert.That(slots.Select(s => s.Id).Distinct().Count(), Is.EqualTo(15), "slot ids are unique");
        }

        [Test] public void UnlockedFollowsTheLibraryAndNewMeansUnlockedButNotYetSeen()
        {
            var lib = new LibraryState();
            Assert.That(ToyRackModel.Unlocked(lib), Is.Empty);
            lib.RecordCompleted("cargo_crew_fractions", 1); lib.RecordCompleted("cargo_crew_fractions", 2);
            lib.MarkToySeen("cargo_crew_fractions", 1);
            var unlocked = ToyRackModel.Unlocked(lib).Select(s => s.Id).ToArray();
            Assert.That(unlocked, Is.EqualTo(new[] { "cargo_crew_fractions#1", "cargo_crew_fractions#2" }));
            var fresh = ToyRackModel.NewlyUnlocked(lib).Select(s => s.Id).ToArray();
            Assert.That(fresh, Is.EqualTo(new[] { "cargo_crew_fractions#2" }), "the container was already picked up");
        }

        [Test] public void BoardRowsReadNOfFivePerLessonWithoutAnyScore()
        {
            var lib = new LibraryState();
            lib.RecordCompleted("neighborhood_cafe_division", 1); lib.RecordCompleted("neighborhood_cafe_division", 4);
            var rows = ToyRackModel.BoardRows(lib);
            Assert.That(rows.Count, Is.EqualTo(3));
            Assert.That(rows[0].Title, Is.EqualTo("Cargo Crew")); Assert.That(rows[0].Done, Is.EqualTo(0)); Assert.That(rows[0].Label, Is.EqualTo("0 of 5"));
            Assert.That(rows[1].Title, Is.EqualTo("Neighborhood Café")); Assert.That(rows[1].Done, Is.EqualTo(2)); Assert.That(rows[1].Label, Is.EqualTo("2 of 5"));
            Assert.That(rows[1].Marks, Is.EqualTo(new[] { true, false, false, true, false }));
            Assert.That(rows[2].Title, Is.EqualTo("Community Garden"));
        }

        [Test] public void TheUnlockLineNamesTheToyAndNothingElse()
        {
            Assert.That(ToyRackModel.UnlockLine(ToyRackModel.Slots[1]), Is.EqualTo("Your big truck is on the rack."));
            Assert.That(ToyRackModel.UnlockLine(ToyRackModel.Slots[0]), Is.EqualTo("Your orange container is on the rack."));
        }

        // Owner 2026-09-29: pointing at a locked toy should say what earns it instead of doing nothing.
        [Test] public void ALockedToyExplainsWhatEarnsIt()
        {
            Assert.That(ToyRackModel.Slots[0].ChapterTitle, Is.EqualTo("Big truck"));
            Assert.That(ToyRackModel.Slots[7].ChapterTitle, Is.EqualTo("Box it up"));
            Assert.That(ToyRackModel.LockedLine(ToyRackModel.Slots[0]), Is.EqualTo("Finish Big truck in Cargo Crew to earn the orange container."));
            Assert.That(ToyRackModel.LockedLine(ToyRackModel.Slots[12]), Is.EqualTo("Finish Turn the bed in Community Garden to earn the watering can."));
        }

    }
}
