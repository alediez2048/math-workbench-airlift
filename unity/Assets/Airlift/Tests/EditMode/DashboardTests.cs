using System;
using System.Linq;
using Airlift.Lounge;
using Airlift.Welcome;
using NUnit.Framework;

namespace Airlift.Tests
{
    /// CC-FD-07/07b, the pure side: what the wall shows and in what order.
    public class DashboardTests
    {
        [Test] public void TwentyFourTilesThreeHeroesFifteenChaptersSixComingSoon()
        {
            var tiles = DashboardCatalog.AllTiles();
            Assert.That(tiles.Count, Is.EqualTo(24));
            Assert.That(tiles.Count(t => t.Kind == TileKind.Lesson), Is.EqualTo(3));
            Assert.That(tiles.Count(t => t.Kind == TileKind.Chapter), Is.EqualTo(15));
            Assert.That(tiles.Count(t => t.Kind == TileKind.ComingSoon), Is.EqualTo(6));
            Assert.That(tiles.Select(t => t.Id).Distinct().Count(), Is.EqualTo(24), "ids are unique");
            Assert.That(tiles.Where(t => t.Kind == TileKind.ComingSoon).All(t => !t.Openable && t.Minutes == 0), Is.True);
            Assert.That(DashboardCatalog.ComingSoon.Select(w => w.Title), Is.EqualTo(new[] { "Route 9", "Ten & Trade", "Corner Store", "Platform Clock", "The Tailor's Bench", "Mile Marker" }));
        }

        [Test] public void ChapterTitlesComeFromTheStationsNotASecondList()
        {
            Assert.That(DashboardCatalog.Chapters("cargo_crew_fractions").Select(c => c.title), Is.EqualTo(Airlift.Lessons.CargoChapter.All.Select(c => c.Title)));
            Assert.That(DashboardCatalog.Chapters("neighborhood_cafe_division").Select(c => c.number), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
            Assert.That(DashboardCatalog.Chapters("community_garden_multiplication").Count, Is.EqualTo(5));
            Assert.That(DashboardCatalog.Chapters("soon_route_9"), Is.Empty);
            var tile = DashboardCatalog.Find(DashboardCatalog.AllTiles(), "cargo_crew_fractions", 3);
            Assert.That(tile.Title, Is.EqualTo(Airlift.Lessons.CargoChapter.All[2].Title));
            Assert.That(tile.Subtitle, Does.Contain("Chapter 3"));
        }

        [Test] public void FeaturedIsLessonsThenTheSuggestedChapterThenChaptersThenComingSoon()
        {
            var lib = new LibraryState(); lib.RecordOpened("neighborhood_cafe_division", 2);
            var order = DashboardCatalog.Order(DashboardFilter.Featured, DashboardCatalog.AllTiles(), lib);
            Assert.That(order.Take(3).Select(t => t.Kind), Is.All.EqualTo(TileKind.Lesson));
            Assert.That(order[3].Id, Is.EqualTo("neighborhood_cafe_division#2"), "where they left off wears the Continue ribbon and sorts first among chapters");
            Assert.That(order.Skip(4).Take(14).Select(t => t.Kind), Is.All.EqualTo(TileKind.Chapter));
            Assert.That(order.Skip(18).Select(t => t.Kind), Is.All.EqualTo(TileKind.ComingSoon), "coming soon last");
        }

        [Test] public void ContinueIsWhereTheyLeftOffOrChapterOneOfTheFirstLesson()
        {
            Assert.That(DashboardCatalog.ContinueTileId(new LibraryState()), Is.EqualTo("cargo_crew_fractions#1"));
            Assert.That(DashboardCatalog.ContinueTileId(null), Is.EqualTo("cargo_crew_fractions#1"));
            var lib = new LibraryState(); lib.RecordOpened("community_garden_multiplication", 0);
            Assert.That(DashboardCatalog.ContinueTileId(lib), Is.EqualTo("community_garden_multiplication#1"), "a hero open points at its first chapter");
            lib.RecordOpened("community_garden_multiplication", 4);
            Assert.That(DashboardCatalog.ContinueTileId(lib), Is.EqualTo("community_garden_multiplication#4"));
        }

        [Test] public void NewestIsByReleaseDateWithComingSoonLast()
        {
            var order = DashboardCatalog.Order(DashboardFilter.Newest, DashboardCatalog.AllTiles(), new LibraryState());
            Assert.That(order.First().LessonId, Is.Not.EqualTo("cargo_crew_fractions"), "café and garden shipped a day after Cargo");
            Assert.That(order.Take(12).All(t => t.Released == DashboardCatalog.CafeGardenReleased), Is.True);
            Assert.That(order.Skip(12).Take(6).All(t => t.LessonId == "cargo_crew_fractions"), Is.True);
            Assert.That(order.Skip(18).All(t => t.Kind == TileKind.ComingSoon), Is.True);
        }

        [Test] public void MostViewedIsOpenCountThenRecencyThenFeaturedOrder()
        {
            var lib = new LibraryState();
            lib.RecordOpened("cargo_crew_fractions", 2); lib.RecordOpened("cargo_crew_fractions", 2);
            lib.RecordOpened("community_garden_multiplication", 1); lib.RecordOpened("neighborhood_cafe_division", 3);
            var order = DashboardCatalog.Order(DashboardFilter.MostViewed, DashboardCatalog.AllTiles(), lib);
            Assert.That(order[0].Id, Is.EqualTo("cargo_crew_fractions#2"), "two opens");
            Assert.That(order[1].Id, Is.EqualTo("neighborhood_cafe_division#3"), "one open, more recent");
            Assert.That(order[2].Id, Is.EqualTo("community_garden_multiplication#1"));
            Assert.That(order[3].Kind, Is.EqualTo(TileKind.Lesson), "unopened tiles follow in featured order");
            Assert.That(order.Last().Kind, Is.EqualTo(TileKind.ComingSoon));
        }

        [Test] public void PagesHoldEightTilesAndWrapAround()
        {
            var all = DashboardCatalog.Order(DashboardFilter.Featured, DashboardCatalog.AllTiles(), null);
            Assert.That(DashboardCatalog.PageCount(all.Count), Is.EqualTo(3));
            Assert.That(DashboardCatalog.Page(all, 0).Count, Is.EqualTo(8));
            Assert.That(DashboardCatalog.Page(all, 2).Count, Is.EqualTo(8));
            Assert.That(DashboardCatalog.Page(all, 3).Select(t => t.Id), Is.EqualTo(DashboardCatalog.Page(all, 0).Select(t => t.Id)), "past the last page wraps to the first");
            Assert.That(DashboardCatalog.Page(all, -1).Select(t => t.Id), Is.EqualTo(DashboardCatalog.Page(all, 2).Select(t => t.Id)));
        }

        [Test] public void TheWallNoteNamesTheComingSoonWorldsHonestly()
        {
            string note = DashboardCatalog.WallNote();
            Assert.That(note, Does.Contain("6 coming-soon worlds").And.Contain("Route 9").And.Contain("cannot be opened yet").And.Contain("no date"));
        }
    }
}
