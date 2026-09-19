using Airlift.Lounge;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    /// CC-FD-09: the settings card's switches, on a throwaway object (no scene).
    public class SettingsPanelTests
    {
        SettingsPanel panel; GameObject go;
        [SetUp] public void Make() { go = new GameObject("settings (test)"); panel = go.AddComponent<SettingsPanel>(); panel.Bind(new SettingsState(), new LibraryState()); }
        [TearDown] public void Drop() { Object.DestroyImmediate(go); }

        [Test] public void SwitchesToggleAndRaiseChanged()
        {
            string changed = null; panel.Changed += k => changed = k;
            panel.Toggle("Captions");
            Assert.That(panel.State.Captions, Is.False); Assert.That(changed, Is.EqualTo("Captions"));
            panel.Toggle("Haptics");
            Assert.That(NerdyHaptics.Enabled, Is.False, "one switch for every pulse");
            panel.Toggle("Haptics"); Assert.That(NerdyHaptics.Enabled, Is.True);
            panel.Toggle("nonsense"); Assert.That(changed, Is.EqualTo("Haptics"), "unknown keys do nothing");
        }

        [Test] public void VolumesCycleDownInQuartersAndWrap()
        {
            Assert.That(panel.Volume("Voice"), Is.EqualTo(1f));
            panel.CycleVolume("Voice"); Assert.That(panel.Volume("Voice"), Is.EqualTo(0.75f));
            panel.CycleVolume("Voice"); panel.CycleVolume("Voice"); panel.CycleVolume("Voice");
            Assert.That(panel.Volume("Voice"), Is.EqualTo(0f));
            panel.CycleVolume("Voice"); Assert.That(panel.Volume("Voice"), Is.EqualTo(1f), "wraps");
            Assert.That(panel.Volume("Music"), Is.EqualTo(0.4f), "the spec default");
            panel.CycleVolume("Music"); Assert.That(panel.Volume("Music"), Is.EqualTo(1f), "an off-step value restarts the cycle");
        }

        [Test] public void ClearSavedDataNeedsTwoPressesAndWipesTheLibrary()
        {
            panel.Library.RecordOpened("cargo_crew_fractions", 2); panel.Library.RundownSeen = true;
            bool cleared = false; panel.SavedDataCleared += () => cleared = true;
            panel.ClearSavedData();
            Assert.That(panel.ClearArmed, Is.True); Assert.That(cleared, Is.False, "the first press only arms");
            Assert.That(panel.Library.Continue, Is.Not.Null);
            panel.ClearSavedData();
            Assert.That(cleared, Is.True); Assert.That(panel.Library.Continue, Is.Null); Assert.That(panel.Library.RundownSeen, Is.False);
        }

        [Test] public void ResetKeepsTheLanguageChosenOnTheConsentCard()
        {
            panel.SetLanguage("es"); panel.Toggle("VoiceGuide"); panel.CycleVolume("Effects");
            panel.ResetToDefaults();
            Assert.That(panel.State.VoiceGuide, Is.True); Assert.That(panel.State.EffectsVolume, Is.EqualTo(0.75f));
            Assert.That(panel.State.Language, Is.EqualTo("es"));
        }

        [Test] public void LabelsReadOnOffAndPercent()
        {
            Assert.That(SettingsPanel.OnOff(true), Is.EqualTo("On")); Assert.That(SettingsPanel.Percent(0.4f), Is.EqualTo("40%"));
        }
    

        // Owner 2026-09-18: a learner who skipped the questions has no age answer, so Dee may not listen (adult
        // testers only). The settings card gets one Age pill that cycles the bands, adult first, and tells the director.
        [Test] public void AgePillCyclesAdultFirstAndTellsTheDirector()
        {
            string chosen = null; panel.AgeChosen += b => chosen = b;
            Assert.That(panel.AgeBand, Is.EqualTo(""));
            Assert.That(SettingsPanel.AgeLabel(""), Is.EqualTo("Not answered"));
            panel.CycleAge(); Assert.That(panel.AgeBand, Is.EqualTo("adult")); Assert.That(chosen, Is.EqualTo("adult"));
            Assert.That(SettingsPanel.AgeLabel("adult"), Is.EqualTo("Adult"));
            panel.CycleAge(); Assert.That(panel.AgeBand, Is.EqualTo("14_to_17"));
            panel.CycleAge(); panel.CycleAge(); Assert.That(panel.AgeBand, Is.EqualTo("under_10"));
            panel.CycleAge(); Assert.That(panel.AgeBand, Is.EqualTo("prefer_not_to_say"));
            panel.CycleAge(); Assert.That(panel.AgeBand, Is.EqualTo("adult"), "wraps");
            panel.SetAge("10_to_13"); Assert.That(panel.AgeBand, Is.EqualTo("10_to_13")); Assert.That(chosen, Is.EqualTo("adult"), "binding the saved answer is not a choice");
            panel.ClearSavedData(); panel.ClearSavedData();
            Assert.That(panel.AgeBand, Is.EqualTo(""), "the profile went with the saved data");
        }
    }
}
