using System.Collections.Generic;

namespace Airlift.Welcome
{
    /// Pure decision of what the world-locked workbench shows: each station's own visuals only while it is the open
    /// lesson, shared lesson visuals (table handle, HUD frame) while any lesson is open.
    public static class StationVisibility
    {
        public static bool StationShows(WelcomePhase phase, string activeLessonId, string stationCardId)
            => phase == WelcomePhase.Lesson && !string.IsNullOrEmpty(activeLessonId) && activeLessonId == stationCardId;

        public static bool SharedShows(WelcomePhase phase) => phase == WelcomePhase.Lesson;

        /// Objects every lesson uses (carry handle, assistant card canvas): never a station's visual root.
        public static readonly string[] SharedObjectNames = { "Table handle", "Guide HUD canvas" };
        public static bool IsShared(string objectName) => System.Array.IndexOf(SharedObjectNames, objectName) >= 0;

        public static bool[] Stations(WelcomePhase phase, string activeLessonId, IReadOnlyList<string> cardIds)
        {
            var shows = new bool[cardIds != null ? cardIds.Count : 0];
            for (int i = 0; i < shows.Length; i++) shows[i] = StationShows(phase, activeLessonId, cardIds[i]);
            return shows;
        }
    }
}
