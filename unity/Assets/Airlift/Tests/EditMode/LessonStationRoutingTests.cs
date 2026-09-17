using System.Collections.Generic;
using System.Linq;
using Airlift.Lessons;
using Airlift.Lessons.Testing;
using Airlift.Welcome;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    /// CC-PL-02: shared voice tools go to the open lesson, lesson tools only to the lesson that owns them, and each
    /// station's visuals show only while it is the open lesson.
    public class LessonStationRoutingTests
    {
        const string Cargo = "cargo_crew_fractions", Cafe = "neighborhood_cafe_division", Garden = "community_garden_multiplication";
        readonly List<GameObject> made = new List<GameObject>();

        [TearDown] public void Clean() { foreach (var go in made) if (go != null) Object.DestroyImmediate(go); made.Clear(); }

        FakeLessonStation Station(string cardId, string title, string[] tools)
        {
            var go = new GameObject(title + " (test)"); made.Add(go);
            var s = go.AddComponent<FakeLessonStation>();
            s.cardId = cardId; s.title = title; s.toolNames = tools;
            return s;
        }

        LessonStation[] Three() => new LessonStation[]
        {
            Station(Cargo, "Cargo Crew", LessonToolRouter.CargoTools),
            Station(Cafe, "Neighborhood Café", LessonToolRouter.CafeTools),
            Station(Garden, "Community Garden", LessonToolRouter.GardenTools)
        };

        [Test] public void WelcomeToolsNeverReachAStation()
        {
            var stations = Three();
            foreach (var tool in LessonToolRouter.WelcomeTools)
                foreach (var active in new[] { null, Cargo, Cafe })
                    Assert.That(LessonToolRouter.Route(tool, active, stations).Route, Is.EqualTo(ToolRoute.Welcome), tool + " in " + active);
        }

        [Test] public void SharedToolsGoToTheOpenStation()
        {
            var stations = Three();
            foreach (var tool in LessonToolRouter.SharedTools)
            {
                var inCafe = LessonToolRouter.Route(tool, Cafe, stations);
                Assert.That(inCafe.Route, Is.EqualTo(ToolRoute.Shared), tool);
                Assert.That(inCafe.StationIndex, Is.EqualTo(1), tool);
                Assert.That(inCafe.Reason, Is.Empty);
                var noLesson = LessonToolRouter.Route(tool, null, stations);
                Assert.That(noLesson.Route, Is.EqualTo(ToolRoute.Shared), tool + ": the director applies the cards-view gates");
                Assert.That(noLesson.StationIndex, Is.EqualTo(-1), tool);
            }
        }

        [Test] public void LessonToolsRunOnlyInTheirOwnLesson()
        {
            var stations = Three();
            Assert.That(LessonToolRouter.Route("split_cargo", Cargo, stations).Route, Is.EqualTo(ToolRoute.Lesson));
            Assert.That(LessonToolRouter.Route("split_cargo", Cargo, stations).StationIndex, Is.EqualTo(0));
            var g = LessonToolRouter.Route("garden_split_bed", Garden, stations);
            Assert.That(g.Route, Is.EqualTo(ToolRoute.Lesson)); Assert.That(g.StationIndex, Is.EqualTo(2));

            var wrong = LessonToolRouter.Route("split_cargo", Cafe, stations);
            Assert.That(wrong.Route, Is.EqualTo(ToolRoute.WrongLesson));
            Assert.That(wrong.Reason, Is.EqualTo("That is not part of Neighborhood Café."));
            Assert.That(LessonToolRouter.Route("cafe_deal_round", Garden, stations).Reason, Is.EqualTo("That is not part of Community Garden."));
            Assert.That(LessonToolRouter.Route("garden_turn_bed", Cargo, stations).Reason, Is.EqualTo("That is not part of Cargo Crew."));
        }

        [Test] public void AKnownLessonToolIsRefusedEvenBeforeItsStationExists()
        {
            var cargoOnly = new LessonStation[] { Station(Cargo, "Cargo Crew", LessonToolRouter.CargoTools) };
            var r = LessonToolRouter.Route("cafe_deal_round", Cargo, cargoOnly);
            Assert.That(r.Route, Is.EqualTo(ToolRoute.WrongLesson));
            Assert.That(r.Reason, Is.EqualTo("That is not part of Cargo Crew."));
        }

        [Test] public void LessonToolsWithNoLessonOpenGiveTheExistingCargoRefusal()
        {
            var stations = Three();
            foreach (var tool in new[] { "split_cargo", "cafe_check_order", "garden_clear_bed" })
            {
                var r = LessonToolRouter.Route(tool, null, stations);
                Assert.That(r.Route, Is.EqualTo(ToolRoute.NoLesson), tool);
                Assert.That(r.Reason, Is.EqualTo(GuideTools.NoLesson), tool);
            }
            Assert.That(LessonToolRouter.Route("split_cargo", "unknown_card", stations).Route, Is.EqualTo(ToolRoute.NoLesson));
        }

        [Test] public void UnknownToolsAreUnknownEverywhere()
        {
            var stations = Three();
            foreach (var active in new[] { null, Cargo })
            {
                Assert.That(LessonToolRouter.Route("fly_the_plane", active, stations).Route, Is.EqualTo(ToolRoute.Unknown));
                Assert.That(LessonToolRouter.Route("", active, stations).Route, Is.EqualTo(ToolRoute.Unknown));
                Assert.That(LessonToolRouter.Route(null, active, stations).Route, Is.EqualTo(ToolRoute.Unknown));
            }
        }

        [Test] public void AStationOwnedToolOutsideTheKnownListsStillRoutes()
        {
            var stations = new LessonStation[] { Station("x", "Extra", new[] { "extra_tool" }) };
            Assert.That(LessonToolRouter.Route("extra_tool", "x", stations).Route, Is.EqualTo(ToolRoute.Lesson));
            Assert.That(LessonToolRouter.Route("extra_tool", null, stations).Route, Is.EqualTo(ToolRoute.NoLesson));
        }

        [Test] public void ToolListsAreDisjointAndMatchTheContract()
        {
            var seen = new HashSet<string>();
            foreach (var list in new[] { LessonToolRouter.WelcomeTools, LessonToolRouter.SharedTools, LessonToolRouter.CargoTools, LessonToolRouter.CafeTools, LessonToolRouter.GardenTools })
                foreach (var name in list) Assert.That(seen.Add(name), Is.True, name + " listed twice");
            Assert.That(LessonToolRouter.SharedTools, Is.EqualTo(new[] { "request_help", "advance_step", "next_chapter", "restart_chapter", "back_to_lessons" }));
            Assert.That(LessonToolRouter.CargoTools, Is.EquivalentTo(new[] { "split_cargo", "check_load", "reset_cargo", "replay_demo" }));
            Assert.That(LessonToolRouter.CafeTools, Is.EquivalentTo(new[] { "cafe_deal_round", "cafe_check_order", "cafe_clear_table" }));
            Assert.That(LessonToolRouter.GardenTools, Is.EquivalentTo(new[] { "garden_turn_bed", "garden_split_bed", "garden_check_bed", "garden_clear_bed" }));
        }

        [Test] public void WrongLessonReasonIsOneShortSpokenSentence()
        {
            foreach (var title in new[] { "Cargo Crew", "Neighborhood Café", "Community Garden" })
            {
                string r = LessonToolRouter.WrongLessonReason(title);
                Assert.That(r, Does.EndWith(".")); Assert.That(r.Length, Is.LessThan(110));
            }
        }

        [Test] public void OnlyTheOpenStationShows()
        {
            var ids = new[] { Cargo, Cafe, Garden };
            Assert.That(StationVisibility.Stations(WelcomePhase.Lesson, Cafe, ids), Is.EqualTo(new[] { false, true, false }));
            Assert.That(StationVisibility.Stations(WelcomePhase.Lesson, Cargo, ids), Is.EqualTo(new[] { true, false, false }));
            foreach (var phase in new[] { WelcomePhase.Consent, WelcomePhase.Welcome, WelcomePhase.Catalog })
            {
                Assert.That(StationVisibility.Stations(phase, Cafe, ids), Is.EqualTo(new[] { false, false, false }), phase.ToString());
                Assert.That(StationVisibility.SharedShows(phase), Is.False, phase.ToString());
            }
            Assert.That(StationVisibility.Stations(WelcomePhase.Lesson, null, ids), Is.EqualTo(new[] { false, false, false }));
            Assert.That(StationVisibility.SharedShows(WelcomePhase.Lesson), Is.True, "table handle shows in every lesson");
            Assert.That(StationVisibility.StationShows(WelcomePhase.Lesson, "", ""), Is.False, "an unwired station never shows");
        }

        [Test] public void ActiveLessonIdIsSetOnOpenAndClearedOnBack()
        {
            var flow = new WelcomeFlow();
            Assert.That(flow.ActiveLessonId, Is.Null);
            flow.Consent(false); flow.EndWelcome();
            Assert.That(flow.OpenLesson("made_up_card"), Is.False); Assert.That(flow.ActiveLessonId, Is.Null, "an unknown card sets nothing");
            Assert.That(flow.OpenLesson(Cargo), Is.True);
            Assert.That(flow.ActiveLessonId, Is.EqualTo(Cargo));
            Assert.That(flow.OpenLesson(Cargo), Is.False, "already in a lesson");
            Assert.That(flow.ActiveLessonId, Is.EqualTo(Cargo));
            Assert.That(flow.BackToCatalog(), Is.True);
            Assert.That(flow.ActiveLessonId, Is.Null);
            Assert.That(flow.BackToCatalog(), Is.False);
            Assert.That(flow.ActiveLessonId, Is.Null);
        }

        [Test] public void LessonToolsAreRefusedInEveryOtherLessonInBothDirections()
        {
            var stations = Three();
            var owners = new[] { (Cargo, "Cargo Crew", LessonToolRouter.CargoTools), (Cafe, "Neighborhood Café", LessonToolRouter.CafeTools), (Garden, "Community Garden", LessonToolRouter.GardenTools) };
            foreach (var open in owners)
                foreach (var owner in owners)
                    foreach (var tool in owner.Item3)
                    {
                        var r = LessonToolRouter.Route(tool, open.Item1, stations);
                        if (open.Item1 == owner.Item1) { Assert.That(r.Route, Is.EqualTo(ToolRoute.Lesson), tool + " in its own lesson"); continue; }
                        Assert.That(r.Route, Is.EqualTo(ToolRoute.WrongLesson), tool + " inside " + open.Item2);
                        Assert.That(r.Reason, Is.EqualTo("That is not part of " + open.Item2 + "."), tool + " inside " + open.Item2);
                    }
        }

        [Test] public void CargoSendsNoToolsNowSoItsJsonStaysAccepted()
        {
            var go = new GameObject("cargo station (test)"); made.Add(go);
            var cargo = go.AddComponent<CargoStation>();
            Assert.That(cargo.ToolsNow, Is.Null);
            Assert.That(cargo.ToolNames, Is.EquivalentTo(LessonToolRouter.CargoTools));
            var step = new GuideStep("briefing", "table", false);
            string plain = GuideTools.ToolResult(true, "", step, "Join the crew.");
            Assert.That(GuideTools.ToolResult(true, "", step, "Join the crew.", default(LessonChapterFacts), null, "Cargo Crew", null), Is.EqualTo(plain));
            var chapter = CargoChapter.All[1];
            var facts = new LessonChapterFacts(chapter.Number, chapter.Id, chapter.Title, chapter.Story, chapter.Task, chapter.Accepted, "1/2 + 1/2 = 1", "Both pickups are loaded.", true, false);
            Assert.That(GuideTools.ToolResult(false, "r", step, "i", facts, null, "Cargo Crew", null),
                Is.EqualTo(GuideTools.ToolResult(false, "r", step, "i", chapter, true, "1/2 + 1/2 = 1", "Both pickups are loaded.")));
            Assert.That(Airlift.Guide.GuideContextBuilder.LessonStep("Cargo Crew", "s", "t", true, "i", null), Is.EqualTo(Airlift.Guide.GuideContextBuilder.LessonStep("Cargo Crew", "s", "t", true, "i")));
            var cafe = Newtonsoft.Json.Linq.JObject.Parse(GuideTools.ToolResult(true, "", step, "i", default(LessonChapterFacts), null, "Neighborhood Café", new[] { "request_help", "cafe_deal_round" }));
            Assert.That((string)cafe["lesson"], Is.EqualTo("Neighborhood Café"));
            Assert.That(cafe["tools_now"].ToObject<string[]>(), Is.EqualTo(new[] { "request_help", "cafe_deal_round" }));
            Assert.That(Airlift.Guide.GuideContextBuilder.LessonStep("Neighborhood Café", "s", "t", true, "i", new[] { "request_help" }), Does.Contain("\"tools_now\":[\"request_help\"]"));
        }

        [Test] public void SharedObjectsAreNeverAStationRoot()
        {
            Assert.That(StationVisibility.IsShared("Table handle"), Is.True);
            Assert.That(StationVisibility.IsShared("Guide HUD canvas"), Is.True);
            Assert.That(StationVisibility.IsShared("Workbench"), Is.False);
            var station = Station(Cafe, "Neighborhood Café", LessonToolRouter.CafeTools);
            var handle = new GameObject("Table handle"); made.Add(handle);
            var own = new GameObject("Cafe workbench"); made.Add(own);
            station.visualRoots = new[] { own, handle };
            station.SetVisualsActive(false);
            Assert.That(own.activeSelf, Is.False);
            Assert.That(handle.activeSelf, Is.True, "a shared object listed by mistake is never hidden by a station");
        }

        [Test] public void FakeStationActionsAreRecorded()
        {
            var s = Station(Cafe, "Neighborhood Café", LessonToolRouter.CafeTools);
            s.Open(); Assert.That(s.IsOpen, Is.True);
            Assert.That(s.TryLessonTool("cafe_deal_round", null, out var r), Is.True); Assert.That(r.Ok, Is.True);
            Assert.That(s.TryLessonTool("split_cargo", null, out _), Is.False);
            Assert.That(s.Chapter.Number, Is.EqualTo(0), "no chapter during the briefing");
            s.Close();
            Assert.That(s.calls, Is.EqualTo(new[] { "Open", "cafe_deal_round", "Close" }));
        }
    }
}

namespace Airlift.Tests
{
    /// Integrator decision (CC-PL-04): the table handle and the Guide HUD canvas are shared by every lesson and stay
    /// visible while a non-Cargo station is open; Cargo's own roots hide. Runs on an additively opened CargoCrew copy.
    public class LessonStationSceneTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        const System.Reflection.BindingFlags Private = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        UnityEngine.SceneManagement.Scene loaded; bool preloaded;

        [SetUp] public void Open()
        {
            preloaded = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(ScenePath).isLoaded;
            if (!preloaded) loaded = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
        }
        [TearDown] public void Close() { if (loaded.IsValid()) UnityEditor.SceneManagement.EditorSceneManager.CloseScene(loaded, true); }

        static void SetFlow(WelcomeFlow flow, WelcomePhase phase, string activeId)
        {
            typeof(WelcomeFlow).GetField("<Phase>k__BackingField", Private).SetValue(flow, phase);
            typeof(WelcomeFlow).GetField("<ActiveLessonId>k__BackingField", Private).SetValue(flow, activeId);
        }

        [Test] public void HandleAndHudStayVisibleWhileAnotherLessonIsOpen()
        {
            if (preloaded) Assert.Inconclusive("CargoCrew is already open in the editor; this test only mutates an additively opened copy.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(ScenePath);
            var n = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var d = n.onboarding;
            var resolved = (LessonStation[])typeof(NerdyDirector).GetProperty("Stations", Private).GetValue(n);
            var cargo = resolved.OfType<CargoStation>().Single();
            Assert.That(cargo.visualRoots.Select(r => r.name), Is.EquivalentTo(new[] { "Workbench", "Cargo terminal", "Lesson interface" }));
            foreach (var s in resolved) foreach (var root in s.visualRoots ?? new GameObject[0])
                Assert.That(StationVisibility.IsShared(root.name), Is.False, s.cardId + " lists shared " + root.name);

            var otherRoot = new GameObject("Other workbench (test)"); otherRoot.transform.SetParent(d.transform, false); otherRoot.SetActive(false);
            var other = otherRoot.AddComponent<FakeLessonStation>(); other.cardId = "test_other_lesson"; other.title = "Other Lesson";
            other.visualRoots = new[] { otherRoot };
            n.stations = resolved.Concat(new LessonStation[] { other }).ToArray();

            var handle = n.stationVisuals.First(g => g != null && g.name == "Table handle");
            var hudCanvas = n.hudStationCanvas.gameObject;
            var showPhase = typeof(NerdyDirector).GetMethod("ShowPhase", Private);
            n.Flow.Consent(false); n.Flow.EndWelcome();

            SetFlow(n.Flow, WelcomePhase.Lesson, other.cardId); showPhase.Invoke(n, null);
            Assert.That(otherRoot.activeSelf, Is.True, "the open lesson's bench shows");
            foreach (var root in cargo.visualRoots) Assert.That(root.activeSelf, Is.False, root.name + " hides outside Cargo");
            Assert.That(handle.activeInHierarchy, Is.True, "table handle stays for every lesson");
            Assert.That(hudCanvas.activeInHierarchy, Is.True, "Guide HUD canvas stays for every lesson");
            Assert.That(n.hudRoot.activeInHierarchy, Is.True); Assert.That(n.hudRoot.transform.parent, Is.EqualTo(n.hudStationCanvas));

            SetFlow(n.Flow, WelcomePhase.Lesson, CargoStation.CardId); showPhase.Invoke(n, null);
            Assert.That(otherRoot.activeSelf, Is.False, "switching lessons hides the previous bench");
            foreach (var root in cargo.visualRoots) Assert.That(root.activeSelf, Is.True, root.name + " shows in Cargo");
            Assert.That(handle.activeInHierarchy, Is.True); Assert.That(hudCanvas.activeInHierarchy, Is.True);

            SetFlow(n.Flow, WelcomePhase.Catalog, null); showPhase.Invoke(n, null);
            Assert.That(otherRoot.activeSelf, Is.False); foreach (var root in cargo.visualRoots) Assert.That(root.activeSelf, Is.False);
            Assert.That(handle.activeSelf, Is.False, "no table in the cards view");
            Assert.That(hudCanvas.activeSelf, Is.True, "the HUD canvas itself is never toggled by a station");
        }

        /// CC-CF-04 (after AgentScripts/PatchCatalogCards.cs): every baked card matches LessonCatalog: a playable card
        /// shows its subject badge on the gradient and the catalog description; a preview shows COMING SOON.
        [Test] public void CatalogCardsMatchTheCatalog()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(ScenePath);
            var n = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            foreach (var card in LessonCatalog.Cards)
            {
                var go = n.catalogRoot.transform.Find("Card " + card.Id); Assert.That(go, Is.Not.Null, card.Id);
                Assert.That(go.Find("Description").GetComponent<TMPro.TMP_Text>().text, Is.EqualTo(card.Description), card.Id);
                var badge = go.Find("Badge");
                Assert.That(badge.GetComponentInChildren<TMPro.TMP_Text>(true).text, Is.EqualTo(card.Playable ? card.Subject.ToUpper() : "COMING SOON"), card.Id);
                Assert.That(badge.Find("Gradient") != null, Is.EqualTo(card.Playable), card.Id + " gradient badge only when playable");
                var badgeRect = badge.GetComponent<RectTransform>(); var badgeLabel = badge.GetComponentInChildren<TMPro.TMP_Text>(true);
                Assert.That(badgeLabel.GetPreferredValues(badgeLabel.text).x, Is.LessThanOrEqualTo(badgeRect.sizeDelta.x - 8f), card.Id + " badge text fits its pill");
                if (card.Playable) Assert.That(badgeRect.anchoredPosition.x - badgeRect.sizeDelta.x / 2f, Is.EqualTo(-125f).Within(0.5f), card.Id + " playable badges share a left edge");
                Assert.That(badge.GetComponent<UnityEngine.UI.Outline>() == null, Is.EqualTo(card.Playable), card.Id + " outlined badge only for a preview");
                Assert.That(go.Find("Art").GetComponent<UnityEngine.UI.Image>().color.a, Is.EqualTo(card.Playable ? 0.9f : 0.35f).Within(0.01f), card.Id + " art opacity");
                Assert.That(go.GetComponent<UnityEngine.UI.Image>().color.a, Is.EqualTo(card.Playable ? 1f : 0.75f).Within(0.01f), card.Id + " panel opacity");
                var button = go.GetComponent<UnityEngine.UI.Button>();
                bool selects = Enumerable.Range(0, button.onClick.GetPersistentEventCount()).Any(i => button.onClick.GetPersistentTarget(i) == n && button.onClick.GetPersistentMethodName(i) == "SelectCard");
                Assert.That(selects, Is.True, card.Id + " card calls SelectCard");
            }
        }

        /// CC-CF-04: pointing at the café card and saying "open Neighborhood Café" both open the café bench; Back and
        /// "back to the lessons" return to the cards; Cargo tools are refused inside the café.
        [Test] public void CafeOpensByCardAndVoiceAndBackReturnsToTheCards()
            => OpensByCardAndVoiceAndBackReturnsToTheCards("neighborhood_cafe_division", "Neighborhood Café", "Corner Café", "split_cargo");

        /// CC-GD-04: the same for Community Garden (Sunny Plot); a café tool is refused inside the garden.
        [Test] public void GardenOpensByCardAndVoiceAndBackReturnsToTheCards()
            => OpensByCardAndVoiceAndBackReturnsToTheCards("community_garden_multiplication", "Community Garden", "Sunny Plot", "cafe_deal_round");

        void OpensByCardAndVoiceAndBackReturnsToTheCards(string cardId, string title, string storyName, string foreignTool)
        {
            if (preloaded) Assert.Inconclusive("CargoCrew is already open in the editor; this test only mutates an additively opened copy.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(ScenePath);
            var n = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var stations = (LessonStation[])typeof(NerdyDirector).GetProperty("Stations", Private).GetValue(n);
            var station = stations.FirstOrDefault(s => s.cardId == cardId);
            if (station == null) Assert.Inconclusive(title + " bench is not in CargoCrew yet (run its builder and WireLessonStations).");
            Assert.That(LessonCatalog.IsPlayable(cardId), Is.True, title + " card is playable");
            Assert.That(station.Title, Is.EqualTo(title)); Assert.That(station.StoryName, Is.EqualTo(storyName));
            var others = stations.Where(s => s != station).ToArray();
            var handle = n.stationVisuals.First(g => g != null && g.name == "Table handle");
            var sent = new List<string>(); n.guide.OutgoingTap = sent.Add;
            var tool = typeof(NerdyDirector).GetMethod("OnToolCall", Private);
            Newtonsoft.Json.Linq.JObject LastResult() => Newtonsoft.Json.Linq.JObject.Parse((string)Newtonsoft.Json.Linq.JObject.Parse(sent.Last(m => m.Contains("function_call_output")))["item"]["output"]);
            try
            {
                n.ConsentNoVoice(); n.GoToCatalog();
                n.SelectCard(cardId);
                Assert.That(n.Flow.Phase, Is.EqualTo(WelcomePhase.Lesson), "pointing at the " + title + " card opens it");
                Assert.That(n.Flow.ActiveLessonId, Is.EqualTo(cardId));
                Assert.That(station.IsOpen, Is.True);
                Assert.That(station.visualRoots.All(r => r.activeSelf), Is.True, title + " bench shows");
                foreach (var other in others) Assert.That((other.visualRoots ?? new GameObject[0]).Any(r => r != null && r.activeSelf), Is.False, other.cardId + " bench hides");
                Assert.That(handle.activeInHierarchy, Is.True, "table handle stays");
                Assert.That(n.hudStationCanvas.gameObject.activeInHierarchy, Is.True, "Guide HUD canvas stays");

                sent.Clear(); tool.Invoke(n, new object[] { foreignTool, "c1", "{}" });
                var refusal = LastResult();
                Assert.That((bool)refusal["ok"], Is.False);
                Assert.That((string)refusal["reason"], Is.EqualTo("That is not part of " + title + "."));
                Assert.That((string)refusal["lesson"], Is.EqualTo(title));
                Assert.That(refusal["tools_now"], Is.Not.Null);
                Assert.That(station.IsOpen, Is.True, "a refused tool changes nothing");

                station.Close(); n.OnLessonBack();   // the bench's Back button persistent listeners, in order
                Assert.That(n.Flow.Phase, Is.EqualTo(WelcomePhase.Catalog)); Assert.That(n.Flow.ActiveLessonId, Is.Null);
                Assert.That(station.IsOpen, Is.False); Assert.That(station.visualRoots.Any(r => r.activeSelf), Is.False);
                Assert.That(handle.activeSelf, Is.False);

                sent.Clear(); tool.Invoke(n, new object[] { "open_lesson", "c2", "{\"cardId\":\"" + cardId + "\"}" });
                var opened = LastResult();
                Assert.That((bool)opened["ok"], Is.True, "open by voice: " + opened);
                Assert.That((string)opened["say"], Does.Contain(storyName));
                Assert.That(n.Flow.ActiveLessonId, Is.EqualTo(cardId)); Assert.That(station.IsOpen, Is.True);

                tool.Invoke(n, new object[] { "back_to_lessons", "c3", "{}" });
                Assert.That(n.Flow.Phase, Is.EqualTo(WelcomePhase.Catalog), "back to the lessons by voice");
                Assert.That(station.IsOpen, Is.False); Assert.That(station.visualRoots.Any(r => r.activeSelf), Is.False);
            }
            finally { n.guide.OutgoingTap = null; }
        }
    }
}
