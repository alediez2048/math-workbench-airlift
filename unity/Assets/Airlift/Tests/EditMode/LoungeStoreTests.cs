using Airlift.Lounge;
using NUnit.Framework;

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
    }
}
