using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Airlift.Lounge
{
    /// CC-FD-09. The settings card's switches and their one record on disk. Every change saves at once and raises
    /// Changed so NerdyDirector can apply it (mic policy, captions, volumes, haptics, helper).
    public sealed class SettingsPanel : MonoBehaviour
    {
        public const string ClearArmedLabel = "Press again to clear", ClearLabel = "Clear saved data", ClearedLabel = "Cleared";
        public static readonly float[] VolumeSteps = { 1f, 0.75f, 0.5f, 0.25f, 0f };

        [Serializable] public sealed class LabelRef { public string key; public TMP_Text label; }
        public List<LabelRef> labels = new List<LabelRef>();
        public TMP_Text versionText;
        public LoungeRoom lounge;
        public float clearArmedSeconds = 5f;

        public SettingsState State { get; private set; } = new SettingsState();
        public LibraryState Library { get; private set; } = new LibraryState();
        public event Action<string> Changed;
        public event Action ReplayRundownRequested;
        public event Action SavedDataCleared;
        float clearArmedUntil = -1f;

        /// NerdyDirector calls this once with what it loaded, before anything is applied.
        public void Bind(SettingsState state, LibraryState library)
        {
            State = state ?? new SettingsState();
            Library = library ?? new LibraryState();
            if (lounge != null) lounge.SceneryChanged += s => { if (State.Scenery != s) { State.Scenery = s; Save("Scenery"); } };
            RefreshLabels();
        }

        public void Save(string key)
        {
            LoungeStoreFiles.Save(State);
            RefreshLabels();
            Changed?.Invoke(key);
        }

        public void SaveLibrary() => LoungeStoreFiles.Save(Library);

        // ---- pills (persistent listeners) ----
        public void Toggle(string key)
        {
            switch (key)
            {
                case "VoiceGuide": State.VoiceGuide = !State.VoiceGuide; break;
                case "Captions": State.Captions = !State.Captions; break;
                case "Haptics": State.Haptics = !State.Haptics; break;
                case "ButtonLabels": State.ButtonLabels = !State.ButtonLabels; break;
                default: return;
            }
            NerdyHaptics.Enabled = State.Haptics;
            NerdyHaptics.Tick();
            Save(key);
        }

        public bool Get(string key)
        {
            switch (key)
            {
                case "VoiceGuide": return State.VoiceGuide;
                case "Captions": return State.Captions;
                case "Haptics": return State.Haptics;
                case "ButtonLabels": return State.ButtonLabels;
                default: return false;
            }
        }

        /// 100 → 75 → 50 → 25 → 0 → 100.
        public void CycleVolume(string bus)
        {
            float v = Volume(bus);
            int i = Array.FindIndex(VolumeSteps, s => Mathf.Abs(s - v) < 0.01f);
            float next = VolumeSteps[(i + 1 + VolumeSteps.Length) % VolumeSteps.Length];
            if (i < 0) next = VolumeSteps[0];
            switch (bus)
            {
                case "Voice": State.VoiceVolume = next; break;
                case "Music": State.MusicVolume = next; break;
                case "Effects": State.EffectsVolume = next; break;
                default: return;
            }
            Save(bus + "Volume");
        }

        public float Volume(string bus)
        {
            switch (bus) { case "Voice": return State.VoiceVolume; case "Music": return State.MusicVolume; case "Effects": return State.EffectsVolume; default: return 1f; }
        }

        public void SetLanguage(string code) { if (State.Language != code) { State.Language = code; Save("Language"); } }

        public void ResetToDefaults()
        {
            var fresh = new SettingsState { Language = State.Language };   // the language was chosen on the consent card; keep it
            State = fresh;
            NerdyHaptics.Enabled = State.Haptics;
            if (lounge != null && lounge.Mode != State.Scenery) lounge.Apply(State.Scenery);
            Save("Reset");
        }

        public void ReplayRundown() => ReplayRundownRequested?.Invoke();

        /// Two presses within a few seconds: the first arms, the second clears. Bookmarks, counts and the profile go.
        public void ClearSavedData()
        {
            if (Time.time > clearArmedUntil) { clearArmedUntil = Time.time + clearArmedSeconds; RefreshLabels(); return; }
            clearArmedUntil = -1f;
            Library.ClearAll();
            SaveLibrary();
            try { Airlift.Welcome.LearnerProfile.Clear(); } catch (Exception) { }
            SavedDataCleared?.Invoke();
            RefreshLabels(ClearedLabel);
        }

        public bool ClearArmed => Time.time <= clearArmedUntil;

        void Update() { if (clearArmedUntil > 0f && Time.time > clearArmedUntil) { clearArmedUntil = -1f; RefreshLabels(); } }

        public static string OnOff(bool on) => on ? "On" : "Off";
        public static string Percent(float v) => Mathf.RoundToInt(v * 100f) + "%";

        void RefreshLabels(string clearText = null)
        {
            foreach (var l in labels)
            {
                if (l == null || l.label == null) continue;
                switch (l.key)
                {
                    case "VoiceGuide": case "Captions": case "Haptics": case "ButtonLabels": l.label.text = OnOff(Get(l.key)); break;
                    case "Voice": case "Music": case "Effects": l.label.text = l.key + " " + Percent(Volume(l.key)); break;
                    case "Clear": l.label.text = clearText ?? (ClearArmed ? ClearArmedLabel : ClearLabel); break;
                }
            }
            if (versionText != null) versionText.text = "Nerdy AI+VR " + Application.version + " · build " + Application.buildGUID.Substring(0, Mathf.Min(8, Application.buildGUID.Length));
        }
    }
}
