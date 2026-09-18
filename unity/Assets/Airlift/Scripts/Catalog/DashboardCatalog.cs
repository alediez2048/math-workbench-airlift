using System;
using System.Collections.Generic;
using System.Linq;
using Airlift.Lessons;
using Airlift.Lessons.Cafe;
using Airlift.Lessons.Garden;
using Airlift.Lounge;

namespace Airlift.Welcome
{
    public enum TileKind { Lesson, Chapter, ComingSoon }
    public enum DashboardFilter { Featured, Newest, MostViewed }

    /// One tile on the wall. Chapter titles come from the stations' chapter lists, never authored twice.
    public sealed class DashboardTile
    {
        public string Id, LessonId, Title, Subtitle, Subject;
        public TileKind Kind;
        public int Chapter;           // 0 for a lesson hero or a coming-soon world
        public int Minutes;
        public DateTime Released;     // DateTime.MaxValue = not released (coming soon sorts last under Newest)
        public bool Openable => Kind != TileKind.ComingSoon;
        public int FeaturedRank;      // authored order, lower first
    }

    /// The six honest placeholders: workplaces with a job, like Dock 7, the café and the garden. Names are the spec's
    /// proposals until the owner confirms or renames them. Never a date.
    public sealed class ComingSoonWorld
    {
        public string Id, Title, Subject, Job;
    }

    /// CC-FD-07/07b. What the wall shows and in what order. Pure: the scene builder lays it out, LibraryState says
    /// what was opened.
    public static class DashboardCatalog
    {
        public const int TilesPerPage = 8;
        public const int ChapterMinutes = 2, LessonMinutes = 10;
        public static readonly DateTime CargoReleased = new DateTime(2026, 9, 16), CafeGardenReleased = new DateTime(2026, 9, 17);

        public static readonly ComingSoonWorld[] ComingSoon =
        {
            new ComingSoonWorld { Id = "soon_route_9", Title = "Route 9", Subject = "Adding and taking away", Job = "Riders get on and off at each stop. Keep the bus count right." },
            new ComingSoonWorld { Id = "soon_ten_and_trade", Title = "Ten & Trade", Subject = "Place value", Job = "Ten bolts snap into a rod, ten rods fill a crate. The shelf label must match." },
            new ComingSoonWorld { Id = "soon_corner_store", Title = "Corner Store", Subject = "Money", Job = "Build the price out of coins, then count the change back." },
            new ComingSoonWorld { Id = "soon_platform_clock", Title = "Platform Clock", Subject = "Telling time", Job = "Move the hands to the departure time so the train leaves." },
            new ComingSoonWorld { Id = "soon_tailors_bench", Title = "The Tailor's Bench", Subject = "Measuring", Job = "Measure the cloth before cutting, in whole units and halves." },
            new ComingSoonWorld { Id = "soon_mile_marker", Title = "Mile Marker", Subject = "Rounding", Job = "Round the distance to the nearest ten and load enough fuel." },
        };

        public static string TileId(string lessonId, int chapter) => LibraryState.TileId(lessonId, chapter);

        /// (number, title) per chapter, read from the same lists the stations play from.
        public static IReadOnlyList<(int number, string title)> Chapters(string lessonId)
        {
            switch (lessonId)
            {
                case "cargo_crew_fractions": return CargoChapter.All.Select(c => (c.Number, c.Title)).ToList();
                case "neighborhood_cafe_division": return CafeChapter.All.Select(c => (c.Number, c.Title)).ToList();
                case "community_garden_multiplication": return GardenChapter.All.Select(c => (c.Number, c.Title)).ToList();
                default: return new List<(int, string)>();
            }
        }

        static DateTime ReleasedOn(string lessonId) => lessonId == "cargo_crew_fractions" ? CargoReleased : CafeGardenReleased;

        /// 3 heroes, 15 chapters, 6 coming soon — 24 tiles, in authored (Featured) order before any filter.
        public static IReadOnlyList<DashboardTile> AllTiles()
        {
            var tiles = new List<DashboardTile>();
            int rank = 0;
            foreach (var card in LessonCatalog.Cards)
                tiles.Add(new DashboardTile { Id = TileId(card.Id, 0), LessonId = card.Id, Kind = TileKind.Lesson, Chapter = 0, Title = card.Title,
                    Subtitle = card.Description, Subject = card.Subject, Minutes = LessonMinutes, Released = ReleasedOn(card.Id), FeaturedRank = rank++ });
            foreach (var card in LessonCatalog.Cards)
                foreach (var (number, title) in Chapters(card.Id))
                    tiles.Add(new DashboardTile { Id = TileId(card.Id, number), LessonId = card.Id, Kind = TileKind.Chapter, Chapter = number, Title = title,
                        Subtitle = card.Title + " · Chapter " + number, Subject = card.Subject, Minutes = ChapterMinutes, Released = ReleasedOn(card.Id), FeaturedRank = rank++ });
            foreach (var world in ComingSoon)
                tiles.Add(new DashboardTile { Id = world.Id, LessonId = null, Kind = TileKind.ComingSoon, Chapter = 0, Title = world.Title,
                    Subtitle = world.Job, Subject = world.Subject, Minutes = 0, Released = DateTime.MaxValue, FeaturedRank = rank++ });
            return tiles;
        }

        /// The chapter to offer first: where the learner left off, or the next chapter after the one they finished
        /// last. Nothing opened yet → chapter 1 of the first lesson.
        public static string ContinueTileId(LibraryState library)
        {
            var last = library?.Continue;
            if (last == null) return TileId(LessonCatalog.Cards[0].Id, 1);
            int chapter = System.Math.Max(1, last.Value.chapter);
            return TileId(last.Value.lessonId, chapter);
        }

        public static IReadOnlyList<DashboardTile> Order(DashboardFilter filter, IReadOnlyList<DashboardTile> tiles, LibraryState library)
        {
            string cont = ContinueTileId(library);
            switch (filter)
            {
                case DashboardFilter.Newest:
                    return tiles.OrderBy(t => t.Released == DateTime.MaxValue ? 1 : 0)
                                .ThenByDescending(t => t.Released)
                                .ThenBy(t => t.FeaturedRank).ToList();
                case DashboardFilter.MostViewed:
                {
                    int Count(DashboardTile t) => library != null && t.Openable ? library.OpenCount(t.Id) : 0;
                    return tiles.OrderByDescending(Count)
                                .ThenByDescending(t => library != null ? library.Recency(t.Id) : -1)
                                .ThenBy(t => t.FeaturedRank).ToList();
                }
                default:
                    // The three lessons, then the suggested next chapter, then the rest of the chapters, coming soon last.
                    return tiles.OrderBy(t => t.Kind == TileKind.Lesson ? 0 : t.Id == cont ? 1 : t.Kind == TileKind.Chapter ? 2 : 3)
                                .ThenBy(t => t.FeaturedRank).ToList();
            }
        }

        public static int PageCount(int tileCount) => System.Math.Max(1, (tileCount + TilesPerPage - 1) / TilesPerPage);

        public static IReadOnlyList<DashboardTile> Page(IReadOnlyList<DashboardTile> ordered, int page)
        {
            int pages = PageCount(ordered.Count);
            page = ((page % pages) + pages) % pages;
            return ordered.Skip(page * TilesPerPage).Take(TilesPerPage).ToList();
        }

        public static DashboardTile Find(IReadOnlyList<DashboardTile> tiles, string lessonId, int chapter)
            => tiles.FirstOrDefault(t => t.LessonId == lessonId && t.Chapter == chapter);

        /// For Dee: what the wall is, in one APP CONTEXT line. Coming-soon worlds are named so she can say so plainly.
        public static string WallNote()
        {
            return "The wall shows " + LessonCatalog.Cards.Length + " lessons, " + LessonCatalog.Cards.Sum(c => Chapters(c.Id).Count) + " chapter tiles and "
                + ComingSoon.Length + " coming-soon worlds (" + string.Join(", ", ComingSoon.Select(w => w.Title)) + "). "
                + "Coming-soon worlds cannot be opened yet and have no date. Filters: Featured, Newest, Most viewed.";
        }
    }
}
