using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Airlift.Lounge
{
    /// CC-FD-06. What the learner changed, saved as one versioned record. Defaults have to survive a missing,
    /// broken or older file: a learner who loses their settings to a bad read would rather have the defaults than
    /// an app that will not start.
    public sealed class SettingsState
    {
        public const int CurrentVersion = 1;
        static readonly string[] Languages = { "en", "es" };

        public int Version = CurrentVersion;
        public Scenery Scenery = LoungeScenery.Default;
        public bool VoiceGuide = true, Captions = true, Haptics = true, ButtonLabels = true;
        public string Language = "en";
        public float VoiceVolume = 1f, MusicVolume = 0.4f, EffectsVolume = 0.75f;

        public string ToJson() => new JObject
        {
            ["version"] = CurrentVersion,
            ["Scenery"] = Scenery.ToString(),
            ["VoiceGuide"] = VoiceGuide,
            ["Captions"] = Captions,
            ["Haptics"] = Haptics,
            ["ButtonLabels"] = ButtonLabels,
            ["Language"] = Language,
            ["VoiceVolume"] = VoiceVolume,
            ["MusicVolume"] = MusicVolume,
            ["EffectsVolume"] = EffectsVolume,
        }.ToString(Newtonsoft.Json.Formatting.None);

        /// Every field falls back on its own, so an older file keeps what it had and gains the rest.
        public static SettingsState FromJson(string json)
        {
            var s = new SettingsState();
            if (string.IsNullOrWhiteSpace(json)) return s;
            JObject o;
            try { o = JObject.Parse(json); } catch (Exception) { return s; }

            s.Scenery = Enum.TryParse((string)o["Scenery"] ?? "", out Scenery scenery) ? scenery : s.Scenery;
            s.VoiceGuide = (bool?)o["VoiceGuide"] ?? s.VoiceGuide;
            s.Captions = (bool?)o["Captions"] ?? s.Captions;
            s.Haptics = (bool?)o["Haptics"] ?? s.Haptics;
            s.ButtonLabels = (bool?)o["ButtonLabels"] ?? s.ButtonLabels;
            string language = (string)o["Language"];
            s.Language = Languages.Contains(language) ? language : s.Language;   // never lock the guide to a language it has no persona for
            s.VoiceVolume = Clamp01((float?)o["VoiceVolume"] ?? s.VoiceVolume);
            s.MusicVolume = Clamp01((float?)o["MusicVolume"] ?? s.MusicVolume);
            s.EffectsVolume = Clamp01((float?)o["EffectsVolume"] ?? s.EffectsVolume);
            s.Version = CurrentVersion;
            return s;
        }

        static float Clamp01(float v) => Mathf.Clamp01(v);
    }

    /// Where the learner left off and what they open most. A local file: no identifiers, nothing sent anywhere,
    /// erasable from settings.
    public sealed class LibraryState
    {
        readonly Dictionary<string, int> counts = new Dictionary<string, int>();
        readonly List<string> recency = new List<string>();      // most recent last
        string lastLesson;
        int lastChapter;
        bool hasLast;
        /// CC-FD-05b: the rundown ran (or was skipped) once. Cleared with everything else.
        public bool RundownSeen;

        public static string TileId(string lessonId, int chapter) => lessonId + "#" + chapter;

        public void RecordOpened(string lessonId, int chapter)
        {
            if (string.IsNullOrEmpty(lessonId)) return;
            string id = TileId(lessonId, chapter);
            counts[id] = OpenCount(id) + 1;
            recency.Remove(id);
            recency.Add(id);
            lastLesson = lessonId; lastChapter = chapter; hasLast = true;
        }

        public int OpenCount(string tileId) => counts.TryGetValue(tileId, out int n) ? n : 0;
        /// Higher = opened more recently; -1 = never.
        public int Recency(string tileId) => recency.IndexOf(tileId);

        public (string lessonId, int chapter)? Continue => hasLast ? (lastLesson, lastChapter) : ((string, int)?)null;

        /// Open count first, then whichever was opened more recently — so a tie reads as "what you were just doing".
        public IReadOnlyList<string> MostViewed => counts.Keys
            .OrderByDescending(OpenCount)
            .ThenByDescending(id => recency.IndexOf(id))
            .Select(id => id.Split('#')[0])
            .Distinct()
            .ToList();

        public void ClearAll()
        {
            counts.Clear(); recency.Clear();
            lastLesson = null; lastChapter = 0; hasLast = false;
            RundownSeen = false;
        }

        public string ToJson()
        {
            var opens = new JObject();
            foreach (var pair in counts) opens[pair.Key] = pair.Value;
            var o = new JObject { ["version"] = 1, ["opens"] = opens, ["order"] = new JArray(recency) };
            if (hasLast) o["last"] = new JObject { ["lesson"] = lastLesson, ["chapter"] = lastChapter };
            o["rundownSeen"] = RundownSeen;
            return o.ToString(Newtonsoft.Json.Formatting.None);
        }

        public static LibraryState FromJson(string json)
        {
            var lib = new LibraryState();
            if (string.IsNullOrWhiteSpace(json)) return lib;
            JObject o;
            try { o = JObject.Parse(json); } catch (Exception) { return lib; }

            if (o["opens"] is JObject opens)
                foreach (var pair in opens) lib.counts[pair.Key] = (int?)pair.Value ?? 0;
            if (o["order"] is JArray order)
                foreach (var id in order) { string s = (string)id; if (!string.IsNullOrEmpty(s)) lib.recency.Add(s); }
            if (o["last"] is JObject last)
            {
                lib.lastLesson = (string)last["lesson"];
                lib.lastChapter = (int?)last["chapter"] ?? 0;
                lib.hasLast = !string.IsNullOrEmpty(lib.lastLesson);
            }
            lib.RundownSeen = (bool?)o["rundownSeen"] ?? false;
            return lib;
        }
    }

    /// Both records live as small files in the app's own data folder. A read that fails gives the defaults; a write
    /// that fails is logged and the app carries on with what it has in memory.
    public static class LoungeStoreFiles
    {
        public const string SettingsFile = "nerdy-settings.json", LibraryFile = "nerdy-library.json";
        static string PathFor(string file) => System.IO.Path.Combine(Application.persistentDataPath, file);

        public static SettingsState LoadSettings() => SettingsState.FromJson(ReadOrNull(SettingsFile));
        public static LibraryState LoadLibrary() => LibraryState.FromJson(ReadOrNull(LibraryFile));
        public static void Save(SettingsState settings) { if (settings != null) Write(SettingsFile, settings.ToJson()); }
        public static void Save(LibraryState library) { if (library != null) Write(LibraryFile, library.ToJson()); }

        static string ReadOrNull(string file)
        {
            try { string p = PathFor(file); return System.IO.File.Exists(p) ? System.IO.File.ReadAllText(p) : null; }
            catch (Exception e) { Debug.LogWarning("[Nerdy] could not read " + file + ": " + e.Message); return null; }
        }
        static void Write(string file, string json)
        {
            try { System.IO.File.WriteAllText(PathFor(file), json); }
            catch (Exception e) { Debug.LogWarning("[Nerdy] could not save " + file + ": " + e.Message); }
        }
    }
}
