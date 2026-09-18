using System.Linq;
using Airlift.Lounge;
using Airlift.Presentation;
using Airlift.Presentation.Dashboard;
using Airlift.Welcome;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Airlift.Tests
{
    /// CC-FD-07/07b, scene side. Run AgentScripts/BuildDashboard.cs before this suite.
    public class DashboardWiringTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        static NerdyDirector Director() => SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
        static DashboardWall Wall() { var w = Director().catalogRoot.GetComponent<DashboardWall>(); Assert.That(w, Is.Not.Null, "run AgentScripts/BuildDashboard.cs"); return w; }
        static string Wired(Button b) => string.Join(",", Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Select(i => (b.onClick.GetPersistentTarget(i) != null ? b.onClick.GetPersistentTarget(i).GetType().Name : "null") + "." + b.onClick.GetPersistentMethodName(i)));

        [Test] public void TwentyFourTilesMatchTheCatalogAndTheHeroesAreTheOriginalCards()
        {
            var wall = Wall();
            Assert.That(wall.tiles.Length, Is.EqualTo(24));
            Assert.That(wall.tiles.Select(t => t.id), Is.EquivalentTo(DashboardCatalog.AllTiles().Select(t => t.Id)));
            foreach (var card in LessonCatalog.Cards)
            {
                var hero = wall.tiles.First(t => t.id == DashboardCatalog.TileId(card.Id, 0));
                Assert.That(hero.name, Is.EqualTo("Card " + card.Id), "the hero is the same object the card tests know");
                Assert.That(hero.transform.Find("Description").GetComponent<TMP_Text>().text, Is.EqualTo(card.Description));
            }
            foreach (var t in wall.tiles.Where(t => t.chapter > 0))
                Assert.That(t.transform.Find("Title").GetComponent<TMP_Text>().text, Is.EqualTo(DashboardCatalog.Chapters(t.lessonId).First(c => c.number == t.chapter).title), t.id);
        }

        [Test] public void ComingSoonTilesAreVisiblyNotOpenable()
        {
            var wall = Wall();
            foreach (var world in DashboardCatalog.ComingSoon)
            {
                var tile = wall.tiles.First(t => t.id == world.Id);
                Assert.That(tile.openable, Is.False);
                Assert.That(tile.GetComponent<Button>().interactable, Is.False, world.Title + " cannot be pressed");
                Assert.That(tile.transform.Find("Badge").GetComponentInChildren<TMP_Text>(true).text, Is.EqualTo("COMING SOON"));
                Assert.That(tile.transform.Find("Badge").GetComponent<Outline>(), Is.Not.Null, "outlined, not the gradient badge");
                Assert.That(tile.GetComponent<Image>().color.a, Is.EqualTo(0.75f).Within(0.01f), "greyed");
                Assert.That(tile.transform.Find("Title").GetComponent<TMP_Text>().text, Is.EqualTo(world.Title));
                Assert.That(tile.transform.Find("Minutes").GetComponent<TMP_Text>().text, Does.Not.Contain("min"), "no minutes for something that does not exist");
            }
        }

        [Test] public void EveryOpenableTilePressesTheWallWithItsOwnId()
        {
            var wall = Wall();
            foreach (var t in wall.tiles.Where(t => t.openable))
            {
                var b = t.GetComponent<Button>();
                Assert.That(Wired(b), Is.EqualTo("DashboardWall.PressTile"), t.id);
                Assert.That(b.onClick.GetPersistentEventCount(), Is.EqualTo(1), t.id + " one listener, not the old SelectCard plus the new one");
            }
        }

        [Test] public void APageIsEightTilesInAFourByTwoGridInsideThePanelAboveTheToolbar()
        {
            var wall = Wall();
            wall.Refresh(new LibraryState());
            var shown = wall.Visible.ToList();
            Assert.That(shown.Count, Is.EqualTo(DashboardCatalog.TilesPerPage));
            var toolbar = (RectTransform)Director().catalogRoot.transform.Find("Toolbar");
            Assert.That(toolbar, Is.Not.Null, "the bar under the wall");
            float toolbarTop = toolbar.anchoredPosition.y + toolbar.sizeDelta.y / 2f;
            var rects = shown.Select(t => (RectTransform)t.transform).ToList();
            foreach (var r in rects)
            {
                Assert.That(r.sizeDelta, Is.EqualTo(DashboardWall.TileSize));
                Assert.That(r.anchoredPosition.y - r.sizeDelta.y / 2f, Is.GreaterThan(toolbarTop), r.name + " sits above the toolbar");
                Assert.That(Mathf.Abs(r.anchoredPosition.x) + r.sizeDelta.x / 2f, Is.LessThanOrEqualTo(NerdySpace.PanelWidth / 2f - NerdySpace.PanelWidth * 0.08f), r.name + " inside the side margins");
            }
            for (int i = 0; i < rects.Count; i++) for (int j = i + 1; j < rects.Count; j++)
            {
                var a = rects[i]; var b = rects[j];
                bool overlap = Mathf.Abs(a.anchoredPosition.x - b.anchoredPosition.x) < a.sizeDelta.x - 0.5f && Mathf.Abs(a.anchoredPosition.y - b.anchoredPosition.y) < a.sizeDelta.y - 0.5f;
                Assert.That(overlap, Is.False, a.name + " overlaps " + b.name);
            }
            Assert.That(rects.Select(r => Mathf.Round(r.anchoredPosition.y)).Distinct().Count(), Is.EqualTo(2), "two rows");
            Assert.That(rects.Select(r => Mathf.Round(r.anchoredPosition.x)).Distinct().Count(), Is.EqualTo(4), "four columns");
        }

        [Test] public void TheToolbarHasThreeFiltersAPagerAndTheScenerySelector()
        {
            var wall = Wall(); var n = Director();
            Assert.That(Wired(wall.featuredButton), Is.EqualTo("DashboardWall.SetFilter"));
            Assert.That(Wired(wall.newestButton), Is.EqualTo("DashboardWall.SetFilter"));
            Assert.That(Wired(wall.mostViewedButton), Is.EqualTo("DashboardWall.SetFilter"));
            Assert.That(Wired(wall.nextButton), Is.EqualTo("DashboardWall.NextPage")); Assert.That(Wired(wall.previousButton), Is.EqualTo("DashboardWall.PreviousPage"));
            var toolbar = n.catalogRoot.transform.Find("Toolbar");
            Assert.That(Wired(toolbar.Find("Your room").GetComponent<Button>()), Is.EqualTo("LoungeRoom.ChooseYourRoom"));
            Assert.That(Wired(toolbar.Find("Nerdy lounge").GetComponent<Button>()), Is.EqualTo("LoungeRoom.ChooseNerdyLounge"));
            var gear = n.hudRoot.transform.Find("Settings gear");
            Assert.That(gear, Is.Not.Null, "the gear sits on the assistant bar right under this row");
            Assert.That(n.wall, Is.EqualTo(wall), "NerdyDirector refreshes the wall when the catalog shows");
            foreach (var b in toolbar.GetComponentsInChildren<Button>(true))
            {
                var r = (RectTransform)b.transform;
                Assert.That(r.sizeDelta.y, Is.EqualTo(NerdySpace.PillHeight).Within(0.5f), b.name + " standard pill height");
            }
        }

        [Test] public void TheContinueRibbonSitsOnExactlyOneTileAndFiltersReorder()
        {
            var wall = Wall();
            var lib = new LibraryState(); lib.RecordOpened("community_garden_multiplication", 3);
            wall.Refresh(lib);
            var ribboned = wall.tiles.Where(t => t.ribbon != null && t.ribbon.activeSelf).ToList();
            Assert.That(ribboned.Select(t => t.id), Is.EqualTo(new[] { "community_garden_multiplication#3" }));
            Assert.That(ribboned[0].gameObject.activeSelf, Is.True, "Continue is on page 1 of Featured");
            wall.SetFilter(DashboardFilter.Newest);
            Assert.That(wall.Visible.First().lessonId, Is.Not.EqualTo("cargo_crew_fractions"));
            wall.SetFilter(DashboardFilter.Featured);
            Assert.That(wall.Visible.Take(3).All(t => t.chapter == 0), Is.True, "heroes first");
            wall.NextPage(); wall.NextPage(); wall.NextPage();
            Assert.That(wall.Page, Is.EqualTo(0), "three pages wrap");
        }

        [Test] public void ChapterTilesCarryArtCommittedUnderAssets()
        {
            var wall = Wall();
            int withArt = wall.tiles.Count(t => t.openable && t.transform.Find("Art/Photo") != null);
            Assert.That(withArt, Is.EqualTo(18), "3 heroes + 15 chapters show their table");
            foreach (var t in wall.tiles.Where(t => t.openable))
            {
                var photo = t.transform.Find("Art/Photo").GetComponent<Image>();
                Assert.That(AssetDatabase.GetAssetPath(photo.sprite), Does.StartWith("Assets/Airlift/Art/Tiles/"), t.id + " art lives in Assets, not artifacts");
            }
        }
    }
}
