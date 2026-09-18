using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Airlift.Welcome
{
    /// Tags only. No name, no birth date, no free text is ever stored.
    [Serializable]
    public sealed class LearnerProfile
    {
        public static readonly string[] AgeBands = { "under_10", "10_to_13", "14_to_17", "adult", "prefer_not_to_say" };
        public static readonly string[] Goals = { "catch_up", "get_ahead", "homework_help", "curious", "teacher_or_parent", "other" };
        public const int MaxInterests = 5;
        public const int MaxInterestLength = 40;
        const string Key = "nerdy.profile.v1";

        public string ageBand = "";
        public List<string> interests = new List<string>();
        public string goal = "";

        /// Every question answered: what NerdyDirector waits for before showing the wall, and what makes a learner "returning".
        public bool IsComplete => !string.IsNullOrEmpty(ageBand) && interests.Count > 0 && !string.IsNullOrEmpty(goal);
        public bool HasAnyAnswer => !string.IsNullOrEmpty(ageBand) || interests.Count > 0 || !string.IsNullOrEmpty(goal);
        public bool IsMinor => ageBand == "under_10" || ageBand == "10_to_13" || ageBand == "14_to_17";

        /// Merges a record_profile tool payload. Unknown enum values and oversized items are rejected;
        /// returns false when nothing valid was present.
        public bool Merge(string argumentsJson)
        {
            JObject o; try { o = JObject.Parse(string.IsNullOrEmpty(argumentsJson) ? "{}" : argumentsJson); } catch { return false; }
            bool any = false;
            var band = (string)o["ageBand"]; if (band != null && AgeBands.Contains(band)) { ageBand = band; any = true; }
            var g = (string)o["goal"]; if (g != null && Goals.Contains(g)) { goal = g; any = true; }
            if (o["interests"] is JArray arr)
            {
                var clean = arr.Select(x => (string)x).Where(x => !string.IsNullOrWhiteSpace(x) && x.Length <= MaxInterestLength)
                    .Select(x => x.Trim().ToLowerInvariant()).Distinct().Take(MaxInterests).ToList();
                if (clean.Count > 0) { interests = clean; any = true; }
            }
            return any;
        }

        public string ToJson() => new JObject { ["ageBand"] = ageBand, ["interests"] = new JArray(interests), ["goal"] = goal }.ToString(Newtonsoft.Json.Formatting.None);
        public static LearnerProfile FromJson(string json) { var p = new LearnerProfile(); p.Merge(json); return p; }
        public void Save() { PlayerPrefs.SetString(Key, ToJson()); PlayerPrefs.Save(); }
        public static LearnerProfile Load() => FromJson(PlayerPrefs.GetString(Key, "{}"));
        public static void Clear() { PlayerPrefs.DeleteKey(Key); }
    }
}
