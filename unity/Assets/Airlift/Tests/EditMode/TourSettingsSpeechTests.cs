using Airlift.Lounge;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    public class TourSettingsSpeechTests
    {
        [Test] public void GearThenDoneThenLessonAreSeparateInstructions()
        {
            var root = new GameObject("settings tour regression");
            try
            {
                var settings = root.AddComponent<LoungeSettings>();
                var panel = new GameObject("settings panel", typeof(RectTransform)); panel.transform.SetParent(root.transform); panel.SetActive(false); settings.panel = panel;
                var done = new GameObject("Done", typeof(RectTransform)); done.transform.SetParent(panel.transform);
                var tour = root.AddComponent<LoungeRundown>(); tour.settings = settings;
                // EditMode does not run MonoBehaviour.Awake for this temporary object.
                typeof(LoungeRundown).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(tour, null);
                string line = null; tour.StepShown += (_, text) => line = text;
                tour.Begin(false, 4);
                Assert.That(line, Does.Contain("gear").And.Not.Contain("Done"));
                settings.Show(true);
                Assert.That(tour.Script.Index, Is.EqualTo(4));
                Assert.That(line, Is.EqualTo(LoungeRundown.SettingsInstruction));
                tour.SetVoice(true);
                Assert.That(line, Is.EqualTo(LoungeRundown.SettingsInstruction), "mic changes must not revert to gear copy while settings are open");
                settings.Close();
                Assert.That(tour.Script.Index, Is.EqualTo(5));
                Assert.That(line, Does.Contain("playable lesson").And.Not.Contain("Done"));
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
