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
    }
}
