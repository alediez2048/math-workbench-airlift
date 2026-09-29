using Airlift.Lounge;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    /// CC-FD-06. What the learner changed and where they left off, saved on the headset. No identifiers, nothing
    /// sent anywhere, erasable from settings (PRIVACY-GATE.md). These are the rules that decide what the wall shows.
    public class LoungeStoreTests
    {
        [Test] public void SettingsStartAtTheDefaultsTheSpecNames()
        {
            var s = new SettingsState();
            Assert.That(s.Scenery, Is.EqualTo(Scenery.YourRoom), "the learner's own room until they choose");
            Assert.That(s.VoiceGuide, Is.True);
            Assert.That(s.Captions, Is.True);
            Assert.That(s.Haptics, Is.True);
            Assert.That(s.ButtonLabels, Is.True);
            Assert.That(s.Language, Is.EqualTo("en"));
            Assert.That(s.VoiceVolume, Is.EqualTo(1f));
            Assert.That(s.MusicVolume, Is.EqualTo(0.4f));
            Assert.That(s.EffectsVolume, Is.EqualTo(0.75f));
        }

        [Test] public void SettingsSurviveARoundTrip()
        {
            var s = new SettingsState { Scenery = Scenery.NerdyLounge, Captions = false, Language = "es", MusicVolume = 0.1f };
            var back = SettingsState.FromJson(s.ToJson());
            Assert.That(back.Scenery, Is.EqualTo(Scenery.NerdyLounge));
            Assert.That(back.Captions, Is.False);
            Assert.That(back.Language, Is.EqualTo("es"));
            Assert.That(back.MusicVolume, Is.EqualTo(0.1f).Within(0.001f));
        }

        [Test] public void AMissingOrBrokenFileGivesTheDefaultsRatherThanNothing()
        {
            Assert.That(SettingsState.FromJson(null).VoiceGuide, Is.True);
            Assert.That(SettingsState.FromJson("").Language, Is.EqualTo("en"));
            Assert.That(SettingsState.FromJson("{ not json").Scenery, Is.EqualTo(Scenery.YourRoom));
        }

        // An older file must not wipe what the learner set; unknown keys fill in from the defaults.
        [Test] public void AnOlderFileKeepsWhatItHadAndDefaultsTheRest()
        {
            var back = SettingsState.FromJson("{\"version\":0,\"Language\":\"es\",\"MusicVolume\":0.2}");
            Assert.That(back.Language, Is.EqualTo("es"));
            Assert.That(back.MusicVolume, Is.EqualTo(0.2f).Within(0.001f));
            Assert.That(back.EffectsVolume, Is.EqualTo(0.75f), "a key the old file never had");
            Assert.That(back.Version, Is.EqualTo(SettingsState.CurrentVersion), "upgraded on read");
        }

        [Test] public void AnUnknownLanguageFallsBackRatherThanLockingTheGuide()
        {
            Assert.That(SettingsState.FromJson("{\"Language\":\"fr\"}").Language, Is.EqualTo("en"));
        }

        [Test] public void ContinueIsTheChapterTheLearnerWasLastIn()
        {
            var lib = new LibraryState();
            Assert.That(lib.Continue, Is.Null, "nothing to continue before the first lesson");
            lib.RecordOpened("cargo_crew_fractions", 0);
            lib.RecordOpened("community_garden_multiplication", 2);
            Assert.That(lib.Continue.Value.lessonId, Is.EqualTo("community_garden_multiplication"));
            Assert.That(lib.Continue.Value.chapter, Is.EqualTo(2));
        }

        [Test] public void MostViewedCountsOpensAndBreaksTiesOnRecency()
        {
            var lib = new LibraryState();
            lib.RecordOpened("a", 0); lib.RecordOpened("a", 0); lib.RecordOpened("a", 0);
            lib.RecordOpened("b", 0);
            lib.RecordOpened("c", 0); lib.RecordOpened("c", 0);
            Assert.That(lib.MostViewed, Is.EqualTo(new[] { "a", "c", "b" }));

            lib.RecordOpened("b", 0);          // now tied with c, but opened more recently
            Assert.That(lib.MostViewed, Is.EqualTo(new[] { "a", "b", "c" }));
        }

        [Test] public void OpenCountsAreKeyedPerChapterNotPerLesson()
        {
            var lib = new LibraryState();
            lib.RecordOpened("cargo_crew_fractions", 0);
            lib.RecordOpened("cargo_crew_fractions", 3);
            Assert.That(lib.OpenCount("cargo_crew_fractions#0"), Is.EqualTo(1));
            Assert.That(lib.OpenCount("cargo_crew_fractions#3"), Is.EqualTo(1));
        }

        [Test] public void ClearingLeavesNothingBehind()
        {
            var lib = new LibraryState();
            lib.RecordOpened("a", 1);
            lib.ClearAll();
            Assert.That(lib.Continue, Is.Null);
            Assert.That(lib.MostViewed, Is.Empty);
            Assert.That(lib.OpenCount("a#1"), Is.EqualTo(0));
        }

        [Test] public void TheLibrarySurvivesARoundTripAndCarriesNoIdentifiers()
        {
            var lib = new LibraryState();
            lib.RecordOpened("cargo_crew_fractions", 2);
            string json = lib.ToJson();
            Assert.That(json, Does.Not.Contain("device").IgnoreCase);
            Assert.That(json, Does.Not.Contain("user").IgnoreCase);
            var back = LibraryState.FromJson(json);
            Assert.That(back.Continue.Value.chapter, Is.EqualTo(2));
            Assert.That(back.OpenCount("cargo_crew_fractions#2"), Is.EqualTo(1));
        }

        [Test] public void TheRundownSeenFlagRoundTripsAndClearsWithEverythingElse()
        {
            var lib = new LibraryState(); lib.RecordOpened("cargo_crew_fractions", 2);
            Assert.That(lib.RundownSeen, Is.False, "first run");
            lib.RundownSeen = true;
            var back = LibraryState.FromJson(lib.ToJson());
            Assert.That(back.RundownSeen, Is.True);
            Assert.That(back.Recency(LibraryState.TileId("cargo_crew_fractions", 2)), Is.GreaterThanOrEqualTo(0));
            Assert.That(back.Recency("never#1"), Is.EqualTo(-1));
            back.ClearAll();
            Assert.That(back.RundownSeen, Is.False, "clear saved data means the rundown plays again");
            Assert.That(LibraryState.FromJson("{\"version\":1}").RundownSeen, Is.False, "an older file without the flag");
        }

        [Test] public void SkipOnboardingDefaultsOffAndSurvivesARoundTrip()
        {
            Assert.That(new SettingsState().SkipOnboarding, Is.False, "every learner gets the welcome unless a tester turns it off");
            var back = SettingsState.FromJson(new SettingsState { SkipOnboarding = true }.ToJson());
            Assert.That(back.SkipOnboarding, Is.True);
            Assert.That(SettingsState.FromJson("{\"version\":1}").SkipOnboarding, Is.False, "an older file without the flag");
        }


        [Test] public void DeleteAllLeavesOnlyDefaultsBehind()
        {
            LoungeStoreFiles.Save(new SettingsState { Language = "es", SkipOnboarding = true });
            var lib = new LibraryState(); lib.RecordOpened("cargo_crew_fractions", 2); lib.RundownSeen = true; LoungeStoreFiles.Save(lib);
            LoungeStoreFiles.DeleteAll();
            Assert.That(LoungeStoreFiles.LoadSettings().SkipOnboarding, Is.False);
            Assert.That(LoungeStoreFiles.LoadSettings().Language, Is.EqualTo("en"));
            Assert.That(LoungeStoreFiles.LoadLibrary().Continue, Is.Null);
            Assert.That(LoungeStoreFiles.LoadLibrary().RundownSeen, Is.False);
        }


        // Toy rack (spec 2026-09-29): completed chapters and seen toys live with the library.
        [Test] public void CompletedChaptersRoundTripAndNeverDoubleCount()
        {
            var lib = new LibraryState();
            Assert.That(lib.IsCompleted("cargo_crew_fractions", 1), Is.False);
            Assert.That(lib.CompletedCount("cargo_crew_fractions"), Is.EqualTo(0));
            lib.RecordCompleted("cargo_crew_fractions", 1); lib.RecordCompleted("cargo_crew_fractions", 1); lib.RecordCompleted("cargo_crew_fractions", 3);
            lib.RecordCompleted("", 2); lib.RecordCompleted("neighborhood_cafe_division", 0);
            var back = LibraryState.FromJson(lib.ToJson());
            Assert.That(back.IsCompleted("cargo_crew_fractions", 1), Is.True);
            Assert.That(back.IsCompleted("cargo_crew_fractions", 2), Is.False);
            Assert.That(back.IsCompleted("cargo_crew_fractions", 3), Is.True);
            Assert.That(back.CompletedCount("cargo_crew_fractions"), Is.EqualTo(2), "chapter 1 twice counts once");
            Assert.That(back.CompletedCount("neighborhood_cafe_division"), Is.EqualTo(0), "chapter 0 and empty ids are ignored");
            Assert.That(back.Continue, Is.Null, "completion is not an open");
        }

        [Test] public void ToysSeenRoundTripAndClearWithEverythingElse()
        {
            var lib = new LibraryState();
            lib.RecordCompleted("community_garden_multiplication", 2); lib.MarkToySeen("community_garden_multiplication", 2);
            var back = LibraryState.FromJson(lib.ToJson());
            Assert.That(back.IsToySeen("community_garden_multiplication", 2), Is.True);
            Assert.That(back.IsToySeen("community_garden_multiplication", 1), Is.False);
            back.ClearAll();
            Assert.That(back.CompletedCount("community_garden_multiplication"), Is.EqualTo(0));
            Assert.That(back.IsToySeen("community_garden_multiplication", 2), Is.False);
            Assert.That(LibraryState.FromJson("{\"version\":1}").CompletedCount("cargo_crew_fractions"), Is.EqualTo(0), "an older file without the key");
        }


        // Owner 2026-09-29: "leave them around the lounge and keep the memory of the location of the toys".
        [Test] public void ToyPosesRoundTripAndClearWithEverythingElse()
        {
            var lib = new LibraryState(); lib.RecordCompleted("cargo_crew_fractions", 1);   // only an earned toy can be moved
            Assert.That(lib.ToyPose("cargo_crew_fractions#1"), Is.Null, "never moved: it sits on the rack");
            lib.RecordToyPose("cargo_crew_fractions#1", new Vector3(1f, 0.5f, -2f), new Quaternion(0f, 0.7071f, 0f, 0.7071f), 2.5f);
            var back = LibraryState.FromJson(lib.ToJson());
            var pose = back.ToyPose("cargo_crew_fractions#1");
            Assert.That(pose, Is.Not.Null);
            Assert.That(pose.Value.position, Is.EqualTo(new Vector3(1f, 0.5f, -2f)));
            Assert.That(pose.Value.rotation.y, Is.EqualTo(0.7071f).Within(0.001f));
            Assert.That(pose.Value.scale, Is.EqualTo(2.5f).Within(0.001f));
            back.ClearAll();
            Assert.That(back.ToyPose("cargo_crew_fractions#1"), Is.Null);
        }

        // Older builds let every toy be grabbed, so a file from them marks locked toys seen and places them; and their
        // places were measured from the room, useless now that the rack moves to whichever wall is beside the board.
        [Test] public void LockedToysKeepNoSeenFlagOrRememberedPlaceAfterLoad()
        {
            string json = "{\"version\":1,\"completed\":[\"cargo_crew_fractions#1\"],\"toysSeen\":[\"cargo_crew_fractions#1\",\"neighborhood_cafe_division#1\"],"
                + "\"poseFrame\":\"rack\",\"toyPoses\":{\"cargo_crew_fractions#1\":{\"p\":[0.5,0.2,0.3],\"r\":[0,0,0,1],\"s\":1},"
                + "\"neighborhood_cafe_division#1\":{\"p\":[0.9,1.15,0],\"r\":[0,0,0,1],\"s\":1}}}";
            var lib = LibraryState.FromJson(json);
            Assert.That(lib.IsToySeen("cargo_crew_fractions", 1), Is.True);
            Assert.That(lib.ToyPose("cargo_crew_fractions#1"), Is.Not.Null);
            Assert.That(lib.IsToySeen("neighborhood_cafe_division", 1), Is.False, "a locked toy was never picked up, so its unlock line still comes");
            Assert.That(lib.ToyPose("neighborhood_cafe_division#1"), Is.Null, "a locked toy sits in its slot");
        }

        [Test] public void PlacesSavedInRoomSpaceByOlderBuildsAreForgotten()
        {
            string old = "{\"version\":1,\"completed\":[\"cargo_crew_fractions#1\"],\"toyPoses\":{\"cargo_crew_fractions#1\":{\"p\":[-0.92,1.7,2.68],\"r\":[0,1,0,0],\"s\":1}}}";
            Assert.That(LibraryState.FromJson(old).ToyPose("cargo_crew_fractions#1"), Is.Null, "no poseFrame: the toy goes back to its slot");
            var lib = new LibraryState(); lib.RecordCompleted("cargo_crew_fractions", 1);
            lib.RecordToyPose("cargo_crew_fractions#1", Vector3.one, Quaternion.identity, 1f);
            Assert.That(LibraryState.FromJson(lib.ToJson()).ToyPose("cargo_crew_fractions#1"), Is.Not.Null, "today's files say the frame is the rack");
        }

    }
}
