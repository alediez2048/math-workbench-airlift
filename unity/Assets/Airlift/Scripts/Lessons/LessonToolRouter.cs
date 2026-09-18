using System.Collections.Generic;
using Airlift.Lessons;

namespace Airlift.Welcome
{
    public enum ToolRoute
    {
        Unknown,      // not a tool the app knows
        Welcome,      // record_profile, end_welcome, describe_card, open_lesson: the director handles them
        Shared,       // request_help, advance_step, next_chapter, restart_chapter, back_to_lessons: to the open station
        Lesson,       // a lesson tool owned by the open station
        NoLesson,     // a lesson tool while no lesson is open
        WrongLesson   // a lesson tool that belongs to another lesson
    }

    public readonly struct ToolRouting
    {
        public readonly ToolRoute Route;
        public readonly int StationIndex;    // index of the open station in the list, -1 when none
        public readonly string Reason;       // spoken refusal for NoLesson and WrongLesson, else ""
        public ToolRouting(ToolRoute route, int stationIndex, string reason) { Route = route; StationIndex = stationIndex; Reason = reason ?? ""; }
    }

    /// Pure routing of a guide tool call to the welcome flow, the shared lesson actions or the open lesson station.
    /// Phase and state gates (briefing, held pieces, pause) stay with the director and the station.
    public static class LessonToolRouter
    {
        public static readonly string[] WelcomeTools = { "record_profile", "end_welcome", "describe_card", "open_lesson", "dashboard_open_tile", "dashboard_filter", "open_settings", "replay_rundown" };
        public static readonly string[] SharedTools = { "request_help", "advance_step", "next_chapter", "restart_chapter", "back_to_lessons" };
        public static readonly string[] CargoTools = { "split_cargo", "check_load", "reset_cargo", "replay_demo" };
        public static readonly string[] CafeTools = { "cafe_deal_round", "cafe_check_order", "cafe_clear_table" };
        public static readonly string[] GardenTools = { "garden_turn_bed", "garden_split_bed", "garden_check_bed", "garden_clear_bed" };

        public static string WrongLessonReason(string title) => "That is not part of " + title + ".";

        static bool Contains(string[] names, string name)
        {
            if (names == null || name == null) return false;
            foreach (var n in names) if (n == name) return true;
            return false;
        }

        /// Every lesson tool the app knows, whether or not its station is in the scene yet.
        public static bool IsKnownLessonTool(string name) => Contains(CargoTools, name) || Contains(CafeTools, name) || Contains(GardenTools, name);

        public static int IndexOf(string cardId, IReadOnlyList<string> cardIds)
        {
            if (string.IsNullOrEmpty(cardId) || cardIds == null) return -1;
            for (int i = 0; i < cardIds.Count; i++) if (cardIds[i] == cardId) return i;
            return -1;
        }

        /// Core decision over plain data: each station's card id, title and owned lesson tools, in the same order.
        public static ToolRouting Route(string toolName, string activeLessonId, IReadOnlyList<string> cardIds, IReadOnlyList<string> titles, IReadOnlyList<string[]> toolNames)
        {
            if (Contains(WelcomeTools, toolName)) return new ToolRouting(ToolRoute.Welcome, -1, "");
            int active = IndexOf(activeLessonId, cardIds);
            if (Contains(SharedTools, toolName)) return new ToolRouting(ToolRoute.Shared, active, "");
            bool owned = false;
            if (toolNames != null) foreach (var names in toolNames) if (Contains(names, toolName)) { owned = true; break; }
            if (!owned && !IsKnownLessonTool(toolName)) return new ToolRouting(ToolRoute.Unknown, active, "");
            if (active < 0) return new ToolRouting(ToolRoute.NoLesson, -1, GuideTools.NoLesson);
            if (toolNames != null && active < toolNames.Count && Contains(toolNames[active], toolName)) return new ToolRouting(ToolRoute.Lesson, active, "");
            string title = titles != null && active < titles.Count ? titles[active] : "";
            return new ToolRouting(ToolRoute.WrongLesson, active, WrongLessonReason(title));
        }

        public static ToolRouting Route(string toolName, string activeLessonId, IReadOnlyList<LessonStation> stations)
        {
            var ids = new List<string>(); var titles = new List<string>(); var tools = new List<string[]>();
            if (stations != null)
                foreach (var s in stations)
                {
                    ids.Add(s != null ? s.cardId : null);
                    titles.Add(s != null ? s.Title : "");
                    tools.Add(s != null ? s.ToolNames : null);
                }
            return Route(toolName, activeLessonId, ids, titles, tools);
        }
    }
}
