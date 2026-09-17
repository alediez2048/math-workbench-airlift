using System.Collections.Generic;
using System.Linq;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Presentation;
using Airlift.Welcome;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Airlift.Tests
{
    /// Phase 1R Dock 7 workbench: piece pool, story card, button groups, container floor, vehicle bay and
    /// press diagnostics as built by AgentScripts/BuildDockWorkbench.cs. Read-only against CargoCrew.
    public class DockWorkbenchWiringTests
    {
        static class BuildGeometry { public const float BedDepthHalf = 0.0375f; }
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }
        // Transform.Find treats "/" as a path separator, so names like "Mark 1/4" must be matched directly.
        static Transform Child(Transform parent, string name) { foreach (Transform c in parent) if (c.name == name) return c; return null; }
        static T Find<T>() where T : Component => SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).FirstOrDefault();
        static OnboardingDirector Onboarding() => Find<OnboardingDirector>();
        static CargoLessonDirector Lesson() => Onboarding().GetComponent<CargoLessonDirector>();
        static IEnumerable<CargoLessonDirector.PieceView> Views(CargoLessonDirector l) => new[] { l.whole, l.halfA, l.halfB }.Concat(l.quarters ?? new CargoLessonDirector.PieceView[0]);

        // ---- piece pool ----
        [Test] public void PiecePoolIsBoundByContractIds()
        {
            var l = Lesson(); Assert.That(l, Is.Not.Null);
            Assert.That(Views(l).Select(v => v.id).ToArray(), Is.EqualTo(new[] { "whole", "half-1", "half-2", "quarter-1", "quarter-2", "quarter-3", "quarter-4" }));
            foreach (var v in Views(l))
            {
                Assert.That(v.piece, Is.Not.Null, v.id); Assert.That(v.grabbable, Is.Not.Null, v.id); Assert.That(v.label, Is.Not.Null, v.id);
                Assert.That(v.piece.GetComponentInChildren<Oculus.Interaction.GrabInteractable>(true), Is.Not.Null, v.id + " grabbable by controller");
                Assert.That(v.piece.IsChildOf(l.chapterObjects.transform), Is.True, v.id);
            }
            foreach (var q in l.quarters)
                Assert.That(q.piece.GetComponent<BoxCollider>().size.x, Is.EqualTo(RulerLayout.PieceLength(2)).Within(1e-4f), q.id + " is 2 cells long");
        }

        [Test] public void EveryModelPieceHasAViewInEveryChapterAndSplit()
        {
            var ids = new HashSet<string>(Views(Lesson()).Select(v => v.id));
            for (int i = 0; i < CargoChapter.All.Count; i++)
            {
                var m = new CargoLessonModel(); m.StartChapter(i);
                foreach (var id in m.PieceIds) Assert.That(ids, Does.Contain(id), CargoChapter.All[i].Id);
                int guard = 0;
                while (m.CanSplit && guard++ < 4)
                {
                    Assert.That(m.Split(false, m.Generation), Is.True);
                    foreach (var id in m.PieceIds) Assert.That(ids, Does.Contain(id), CargoChapter.All[i].Id + " after split");
                }
            }
        }

        [Test] public void TrayPositionsStayClearOfPadAndDockZone()
        {
            var d = Onboarding(); var l = Lesson();
            foreach (var v in Views(l))
            {
                Assert.That(Vector3.Distance(v.trayPosition, d.content.targetPosition), Is.GreaterThan(d.content.placementRadius), v.id);
                Assert.That(RulerLayout.InDockZone(l.ruler.localPosition, v.trayPosition), Is.False, v.id + " cannot dock from the tray");
            }
        }

        [Test] public void HalvesCarryAHiddenLockedMark()
        {
            var l = Lesson();
            foreach (var v in new[] { l.halfA, l.halfB })
            {
                Assert.That(v.lockedMark, Is.Not.Null, v.id);
                Assert.That(v.lockedMark.transform.IsChildOf(v.piece), Is.True, v.id);
                Assert.That(v.lockedMark.activeSelf, Is.False, v.id + " unlocked by default");
            }
            var locked = CargoChapter.All.SelectMany(c => { var m = new CargoLessonModel(); m.StartChapter(c.Number - 1); return m.PieceIds.Where(m.IsLocked); }).Distinct().ToArray();
            foreach (var id in locked) Assert.That(Views(l).First(v => v.id == id).lockedMark, Is.Not.Null, id + " can show that it is locked");
        }

        [Test] public void TableHandlePausesQuarterGrabs()
        {
            var d = Onboarding(); var l = Lesson(); var handle = d.GetComponent<TableHandle>();
            foreach (var q in l.quarters)
                Assert.That(handle.pieceInteractables, Does.Contain(q.piece.GetComponentInChildren<Oculus.Interaction.GrabInteractable>(true)), q.id);
        }

        // ---- story card ----
        [Test] public void StoryCardFieldsExistOnTheLessonPanel()
        {
            var d = Onboarding(); var l = Lesson(); var ui = d.transform.Find("Lesson interface");
            foreach (var t in new[] { l.heading, l.body, l.expressionLine, l.sayHints })
            {
                Assert.That(t, Is.Not.Null);
                Assert.That(t.transform.IsChildOf(ui), Is.True, t.name);
            }
            Assert.That(l.heading, Is.SameAs(d.heading)); Assert.That(l.body, Is.SameAs(d.body), "the guide reads the same body text");
        }

        [Test] public void StoryCardTextFitsForEveryChapter()
        {
            var l = Lesson();
            float bodyW = l.body.rectTransform.rect.width, bodyH = l.body.rectTransform.rect.height;
            var recap = CargoLessonDirector.RecapLine(CargoChapter.All.ToList());
            foreach (var chapter in CargoChapter.All)
            {
                string title = CargoLessonDirector.TitleFor(chapter);
                Assert.That(l.heading.GetPreferredValues(title, l.heading.rectTransform.rect.width, 1000).y, Is.LessThanOrEqualTo(l.heading.rectTransform.rect.height), title);
                string feedback = LongestFeedback(chapter);
                string text = CargoLessonDirector.ComposeBody(chapter, feedback, chapter.Number == CargoChapter.All.Count ? recap : "");
                // The director shrinks down to 65%; the authored copy must already fit at 80%.
                Assert.That(l.body.GetPreferredValues("<size=80%>" + text + "</size>", bodyW, 10000).y, Is.LessThanOrEqualTo(bodyH), chapter.Id + ": " + text);
                Assert.That(l.expressionLine.GetPreferredValues(chapter.Expression).x, Is.LessThanOrEqualTo(l.expressionLine.rectTransform.rect.width), chapter.Expression);
                foreach (var complete in new[] { false, true }) foreach (var canSplit in new[] { false, true })
                {
                    string hints = CargoLessonDirector.SayHintsFor(chapter, complete, canSplit, chapter.Number == CargoChapter.All.Count);
                    Assert.That(l.sayHints.GetPreferredValues(hints).x, Is.LessThanOrEqualTo(l.sayHints.rectTransform.rect.width), hints);
                }
            }
        }

        static string LongestFeedback(CargoChapter chapter)
        {
            var floors = new[] { new int[0], new[] { 4 }, new[] { 2, 4 }, new[] { 4, 4, 4 }, new[] { 1 }, new[] { 2, 2 }, new[] { 4, 4, 4, 4 }, new[] { 4, 4 }, new[] { 2 } };
            var lines = new List<string> { CargoLessonDirector.HeldReason, CargoLessonDirector.SplitDoneReason, CargoLessonDirector.NoSplitReason, CargoLessonDirector.NotCompleteReason, CargoLessonDirector.LoadedReason };
            foreach (var floor in floors)
            {
                CargoLessonModel.Evaluate(chapter, chapter.LockedPieces, floor, out var feedback);
                lines.Add(feedback);
            }
            return lines.OrderByDescending(s => s.Length).First();
        }

        [Test] public void CardRowsStackWithoutOverlap()
        {
            var d = Onboarding(); var l = Lesson();
            Rect R(RectTransform r) => new Rect(r.anchoredPosition - r.sizeDelta / 2, r.sizeDelta);
            var body = R(l.body.rectTransform); var expr = R(l.expressionLine.rectTransform); var hints = R(l.sayHints.rectTransform);
            var buttons = l.chapterButtons.GetComponentsInChildren<Button>(true).Concat(d.briefing.GetComponentsInChildren<Button>(true)).Distinct().ToArray();
            float rowTop = buttons.Max(b => R(b.GetComponent<RectTransform>()).yMax);
            Assert.That(body.yMin, Is.GreaterThanOrEqualTo(expr.yMax - 1f), "body above the expression line");
            Assert.That(expr.yMin, Is.GreaterThanOrEqualTo(hints.yMax - 1f), "expression above the say-hints");
            Assert.That(hints.yMin, Is.GreaterThanOrEqualTo(rowTop - 1f), "say-hints above the button row");
            var panel = d.transform.Find("Lesson interface").GetComponent<RectTransform>().sizeDelta;
            foreach (var b in buttons)
            {
                var r = R(b.GetComponent<RectTransform>());
                Assert.That(Mathf.Abs(r.xMin) <= panel.x / 2 && Mathf.Abs(r.xMax) <= panel.x / 2 && r.yMin >= -panel.y / 2, Is.True, b.name + " inside the panel");
                var label = b.GetComponentInChildren<TMP_Text>(true);
                if (label != null) Assert.That(label.GetPreferredValues(label.text).x, Is.LessThanOrEqualTo(label.rectTransform.rect.width + 1f), b.name + " label fits");
            }
            var chapterRow = l.chapterButtons.GetComponentsInChildren<Button>(true).Concat(new[] { buttons.First(b => b.name == "Back to lessons") }).ToArray();
            for (int i = 0; i < chapterRow.Length; i++) for (int j = i + 1; j < chapterRow.Length; j++)
                Assert.That(R(chapterRow[i].GetComponent<RectTransform>()).Overlaps(R(chapterRow[j].GetComponent<RectTransform>())), Is.False, chapterRow[i].name + " vs " + chapterRow[j].name);
        }

        // ---- buttons ----
        [Test] public void ChapterButtonsLiveInOneCanvasGroupAndStayWired()
        {
            var l = Lesson();
            Assert.That(l.chapterButtonGroup, Is.Not.Null);
            Assert.That(l.chapterButtonGroup.gameObject, Is.SameAs(l.chapterButtons));
            foreach (var (button, method) in new[] { (l.splitButton, "Split"), (l.submitButton, "Submit"), (l.resetButton, "ResetPieces"), (l.nextButton, "NextChapter") })
            {
                Assert.That(button, Is.Not.Null, method);
                Assert.That(button.transform.IsChildOf(l.chapterButtonGroup.transform), Is.True, method);
                bool wired = Enumerable.Range(0, button.onClick.GetPersistentEventCount()).Any(i => button.onClick.GetPersistentTarget(i) == l && button.onClick.GetPersistentMethodName(i) == method);
                Assert.That(wired, Is.True, button.name + " -> " + method);
            }
        }

        [Test] public void OnboardingRowHasItsOwnGroupAndNerdyFallbackUsesBoth()
        {
            var d = Onboarding(); var l = Lesson(); var n = Find<NerdyDirector>();
            var back = d.transform.Find("Lesson interface").GetComponentsInChildren<Button>(true).First(b => b.name == "Back to lessons");
            var group = d.primary.GetComponentInParent<CanvasGroup>(true);
            Assert.That(group, Is.Not.Null); Assert.That(group, Is.Not.SameAs(l.chapterButtonGroup));
            Assert.That(d.help.GetComponentInParent<CanvasGroup>(true), Is.SameAs(group));
            Assert.That(back.GetComponentInParent<CanvasGroup>(true), Is.SameAs(group));
            var targets = Enumerable.Range(0, back.onClick.GetPersistentEventCount()).Select(i => back.onClick.GetPersistentTarget(i)).ToArray();
            Assert.That(targets.OfType<OnboardingDirector>().Any() && targets.OfType<CargoLessonDirector>().Any() && targets.OfType<NerdyDirector>().Any(), Is.True, "Back still exits onboarding, the chapter and the lesson");
            Assert.That(n.fallbackButtons, Is.Not.Null);
            Assert.That(n.fallbackButtons, Does.Contain(group)); Assert.That(n.fallbackButtons, Does.Contain(l.chapterButtonGroup));
        }

        [Test] public void EveryLessonAndHudButtonLogsPresses()
        {
            var d = Onboarding(); var n = Find<NerdyDirector>();
            var buttons = d.transform.Find("Lesson interface").GetComponentsInChildren<Button>(true).Concat(n.hudRoot.GetComponentsInChildren<Button>(true));
            foreach (var b in buttons) Assert.That(b.GetComponent<UiPressLog>(), Is.Not.Null, b.name);
        }

        [Test] public void HudButtonsHaveNoPersistentNerdyListeners()
        {
            // NerdyDirector.Start adds every bar listener at runtime; a persistent copy made Pause and Music
            // toggle twice per press on build 190518, so nothing visibly changed.
            var n = Find<NerdyDirector>();
            foreach (var button in new[] { n.pauseButton, n.musicButton, n.muteButton, n.helpButton, n.repeatButton })
            {
                Assert.That(button, Is.Not.Null);
                var targets = Enumerable.Range(0, button.onClick.GetPersistentEventCount()).Select(i => button.onClick.GetPersistentTarget(i) + "." + button.onClick.GetPersistentMethodName(i));
                bool persistent = Enumerable.Range(0, button.onClick.GetPersistentEventCount()).Any(i => button.onClick.GetPersistentTarget(i) is NerdyDirector);
                Assert.That(persistent, Is.False, button.name + " must fire once per press: " + string.Join(", ", targets));
            }
        }

        // ---- dock edge (Round 2: the vehicle beds are the container floor) ----
        [Test] public void DockEdgeStripKeepsTheContainerMarks()
        {
            var d = Onboarding(); var l = Lesson();
            Assert.That(l.ruler.Find("Container floor"), Is.Null, "the old central container floor is gone");
            var edge = l.ruler.Find("Dock edge"); Assert.That(edge, Is.Not.Null);
            string[] marks = { "0", "1/4", "1/2", "3/4", "1" };
            for (int k = 0; k < marks.Length; k++)
            {
                var mark = Child(edge, "Mark " + marks[k]); Assert.That(mark, Is.Not.Null, marks[k]);
                Assert.That(mark.GetComponent<TMP_Text>().text, Is.EqualTo(marks[k]));
                var tick = Child(edge, "Tick " + marks[k]); Assert.That(tick, Is.Not.Null, marks[k]);
                Assert.That(tick.localPosition.x, Is.EqualTo(-RulerLayout.WholeLength / 2 + k * RulerLayout.PieceLength(2)).Within(1e-4f), "tick " + marks[k] + " sits on its cell boundary");
            }
            Assert.That(edge.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == "ONE CONTAINER"), Is.True);
            var strip = edge.Find("Edge strip"); Assert.That(strip, Is.Not.Null);
            float stripBack = l.ruler.localPosition.z + strip.localPosition.z + strip.GetComponent<MeshFilter>().sharedMesh.bounds.extents.z;
            Assert.That(stripBack, Is.LessThan(l.ruler.localPosition.z - BuildGeometry.BedDepthHalf), "strip stays in front of the beds");
            Assert.That(l.halfMarks.Length, Is.EqualTo(1)); Assert.That(l.halfMarks[0].transform.IsChildOf(edge), Is.True); Assert.That(l.halfMarks[0].activeSelf, Is.False);
            Assert.That(l.restHeight, Is.EqualTo(0.0795f).Within(1e-3f), "crates rest on the bed floor");
            Assert.That(l.hideWhileActive, Does.Contain(d.station), "the practice pad hides under the beds in the lesson; onboarding re-shows it");
        }

        // ---- vehicle bay ----
        [Test] public void VehicleBedsAreExactCellWidthsForEveryChapter()
        {
            var d = Onboarding(); var l = Lesson(); var terminal = Find<CargoTerminalView>();
            var bay = l.vehicles; Assert.That(bay, Is.Not.Null);
            Assert.That(bay.transform.IsChildOf(terminal.transform), Is.True);
            Assert.That(bay.ruler, Is.EqualTo(l.ruler));
            Assert.That(bay.CountOf("truck", 8), Is.EqualTo(1)); Assert.That(bay.CountOf("pickup", 4), Is.GreaterThanOrEqualTo(2)); Assert.That(bay.CountOf("van", 2), Is.GreaterThanOrEqualTo(4));
            Assert.That(bay.vehicles.First(v => v.kind == "truck").root, Is.EqualTo(terminal.truck));
            foreach (var chapter in CargoChapter.All)
            {
                Assert.That(chapter.BedCells.Length, Is.EqualTo(chapter.VehicleCount), chapter.Id);
                Assert.That(chapter.BedCells.Sum(), Is.LessThanOrEqualTo(PlacementState.CellsPerWhole), chapter.Id);
                foreach (var size in chapter.BedCells.Distinct())
                    Assert.That(bay.CountOf(chapter.VehicleKind, size), Is.GreaterThanOrEqualTo(chapter.BedCells.Count(c => c == size)), chapter.Id + " needs " + size + "-cell " + chapter.VehicleKind + "s");
            }
            var deck = d.transform.Find("Workbench").GetComponent<MeshFilter>().sharedMesh.bounds;
            foreach (var v in bay.vehicles)
            {
                float w = RulerLayout.PieceLength(v.bedCells);
                var floor = v.root.Find("Bed floor"); Assert.That(floor, Is.Not.Null, v.root.name);
                var floorBounds = floor.GetComponent<MeshFilter>().sharedMesh.bounds;
                Assert.That(floorBounds.size.x, Is.EqualTo(w).Within(1e-4f), v.root.name + " bed is " + v.bedCells + " cells wide");
                Assert.That(Mathf.Abs(floor.localPosition.x), Is.LessThan(1e-4f), v.root.name + " bed centred on the root");
                Assert.That(bay.parkHeight + floor.localPosition.y + floorBounds.extents.y, Is.EqualTo(l.restHeight - 0.0275f).Within(1e-3f), v.root.name + " bed floor carries the crates");
                Assert.That(floorBounds.extents.z * 2, Is.GreaterThan(0.06f), v.root.name + " bed holds a crate's depth");
                foreach (var f in v.root.GetComponentsInChildren<MeshFilter>(true))
                {
                    var b = f.sharedMesh.bounds; float maxX = 0f;
                    foreach (float x in new[] { b.min.x, b.max.x }) foreach (float y in new[] { b.min.y, b.max.y }) foreach (float z in new[] { b.min.z, b.max.z })
                        maxX = Mathf.Max(maxX, Mathf.Abs(v.root.InverseTransformPoint(f.transform.TransformPoint(new Vector3(x, y, z))).x));
                    Assert.That(maxX, Is.LessThanOrEqualTo(w / 2 + 1e-3f), v.root.name + "/" + f.name + " stays inside its cells so neighbours never overlap");
                }
                Assert.That(v.loadedTag, Is.Not.Null, v.root.name);
                Assert.That(v.loadedTag.activeSelf, Is.False, v.root.name + " tag hidden until loaded");
                Assert.That(v.loadedTag.GetComponentInChildren<TMP_Text>(true).text, Is.EqualTo("LOADED"));
                Vector3 park = d.transform.InverseTransformPoint(bay.transform.TransformPoint(bay.ParkPosition(0, v.bedCells)));
                Assert.That(park.x, Is.EqualTo(RulerLayout.CellsCenterX(l.ruler.localPosition, 0, v.bedCells)).Within(1e-4f));
                Assert.That(park.z, Is.EqualTo(l.ruler.localPosition.z).Within(1e-4f), v.root.name + " bed over the ruler");
                Assert.That(park.z - 0.045f, Is.GreaterThan(l.whole.trayPosition.z + 0.03f), v.root.name + " rear stays clear of the tray row");
                Assert.That(park.z - 0.045f + bay.driveDistance, Is.GreaterThan(deck.extents.z), v.root.name + " drives fully off the deck");
            }
            Assert.That(terminal.truck.gameObject.activeSelf, Is.True, "idle truck parked outside the lesson");
            foreach (var v in bay.vehicles.Where(v => v.kind != "truck")) Assert.That(v.root.gameObject.activeSelf, Is.False, v.root.name);
            Assert.That(bay.unusedCells.Length, Is.EqualTo(PlacementState.CellsPerWhole));
            foreach (var c in bay.unusedCells) { Assert.That(c.transform.IsChildOf(l.ruler), Is.True); Assert.That(c.activeSelf, Is.False); }
            Assert.That(bay.notNeededLabel, Is.Not.Null);
        }

        // ---- behaviour on in-memory objects (no scene changes) ----
        static (GameObject station, Transform ruler, VehicleBay bay) MakeDock()
        {
            var station = new GameObject("test station");
            var ruler = new GameObject("ruler").transform; ruler.SetParent(station.transform, false); ruler.localPosition = new Vector3(0, 0.031f, 0.02f);
            var bay = new GameObject("bay").AddComponent<VehicleBay>(); bay.transform.SetParent(station.transform, false);
            bay.ruler = ruler; bay.driveSeconds = 0f;
            var list = new List<VehicleBay.Vehicle>();
            void Add(string kind, int cells, string name)
            {
                var root = new GameObject(name).transform; root.SetParent(bay.transform, false);
                var tag = new GameObject("Loaded tag"); tag.transform.SetParent(root, false); tag.SetActive(false);
                list.Add(new VehicleBay.Vehicle { kind = kind, bedCells = cells, root = root, loadedTag = tag });
            }
            Add("truck", 8, "truck"); Add("pickup", 4, "pickup 1"); Add("pickup", 4, "pickup 2");
            for (int i = 1; i <= 4; i++) Add("van", 2, "van " + i);
            bay.vehicles = list.ToArray();
            bay.unusedCells = Enumerable.Range(0, 8).Select(i => { var c = new GameObject("cell " + i); c.transform.SetParent(ruler, false); return c; }).ToArray();
            return (station, ruler, bay);
        }

        [Test] public void BayParksBedsOverTheirCellsAndDrivesAwayWithCargo()
        {
            var (station, ruler, bay) = MakeDock();
            try
            {
                bay.ShowChapter(CargoChapter.All[2]);   // four vans {2,2,2,2}
                Assert.That(bay.Current.Count, Is.EqualTo(4));
                for (int i = 0; i < 4; i++)
                {
                    var van = bay.Current[i].root;
                    Assert.That(van.gameObject.activeSelf, Is.True);
                    Assert.That(van.localPosition.x, Is.EqualTo(RulerLayout.CellsCenterX(ruler.localPosition, 2 * i, 2)).Within(1e-4f), "van " + (i + 1) + " over cells " + 2 * i + "-" + (2 * i + 2));
                    Assert.That(van.localPosition.z, Is.EqualTo(ruler.localPosition.z).Within(1e-4f));
                }
                Assert.That(bay.vehicles.Where(v => v.kind != "van").All(v => !v.root.gameObject.activeSelf), Is.True, "only this chapter's vehicles");
                Assert.That(bay.unusedCells.Any(c => c.activeSelf), Is.False, "all cells used");

                bay.ShowChapter(CargoChapter.All[3]);   // one pickup {4}; cells 4-8 not needed
                Assert.That(bay.Current.Count, Is.EqualTo(1));
                Assert.That(bay.Current[0].root.localPosition.x, Is.EqualTo(RulerLayout.CellsCenterX(ruler.localPosition, 0, 4)).Within(1e-4f));
                for (int c = 0; c < 8; c++) Assert.That(bay.unusedCells[c].activeSelf, Is.EqualTo(c >= 4), "cell " + c);

                var crate = new GameObject("crate").transform; crate.SetParent(station.transform, false); crate.localPosition = new Vector3(-0.07f, 0.08f, 0.02f);
                var pickup = bay.Current[0]; Vector3 parked = pickup.root.localPosition;
                bay.DriveAway(new List<Transform> { crate });
                Assert.That(bay.DrivenAway, Is.True);
                Assert.That(pickup.root.localPosition.z - parked.z, Is.EqualTo(bay.driveDistance).Within(1e-4f), "vehicle drives +z");
                Assert.That(crate.localPosition.z - 0.02f, Is.EqualTo(bay.driveDistance).Within(1e-4f), "the crate rides along");
                Assert.That(crate.localPosition.x, Is.EqualTo(-0.07f).Within(1e-4f));
                Assert.That(pickup.root.gameObject.activeSelf || crate.gameObject.activeSelf, Is.False, "gone once off the deck");

                bay.ShowChapter(CargoChapter.All[3]);
                Assert.That(bay.DrivenAway, Is.False);
                Assert.That(bay.Current[0].root.gameObject.activeSelf, Is.True); Assert.That(bay.Current[0].root.localPosition, Is.EqualTo(parked), "restored for a restart");
                Assert.That(bay.Current[0].loadedTag.activeSelf, Is.False);
            }
            finally { Object.DestroyImmediate(station); }
        }

        [Test] public void DroppingIntoBedsLoadsAndAnAcceptedLoadDrivesAway()
        {
            var (station, ruler, bay) = MakeDock();
            try
            {
                var lesson = station.AddComponent<CargoLessonDirector>();
                lesson.stationRoot = station.transform; lesson.ruler = ruler; lesson.vehicles = bay; lesson.restHeight = 0.0795f;
                CargoLessonDirector.PieceView View(string id, Vector3 tray)
                {
                    var piece = new GameObject(id).transform; piece.SetParent(station.transform, false);
                    return new CargoLessonDirector.PieceView { id = id, piece = piece, trayPosition = tray };
                }
                lesson.whole = View("whole", new Vector3(-0.13f, 0.047f, -0.17f));
                lesson.halfA = View("half-1", new Vector3(-0.21f, 0.047f, -0.17f));
                lesson.halfB = View("half-2", new Vector3(-0.05f, 0.047f, -0.17f));
                lesson.quarters = Enumerable.Range(1, 4).Select(i => View("quarter-" + i, new Vector3(-0.265f + (i - 1) * 0.09f, 0.047f, -0.17f))).ToArray();
                lesson.Begin();   // chapter 1: big truck

                var truck = bay.Current.Single().root; Vector3 parked = truck.localPosition;
                Assert.That(lesson.DropAt("whole", new Vector3(0.3f, 0.08f, -0.17f)), Is.False, "off the dock: no load");
                Assert.That(lesson.DropAt("whole", new Vector3(0.01f, 0.08f, 0.03f)), Is.True, "over the truck bed");
                Assert.That(lesson.whole.piece.localPosition.x, Is.EqualTo(RulerLayout.CellsCenterX(ruler.localPosition, 0, 8)).Within(1e-4f), "snapped into the bed from cell 0");
                Assert.That(lesson.whole.piece.localPosition.y, Is.EqualTo(0.0795f).Within(1e-4f));

                var load = lesson.TryLoad();
                Assert.That(load.Ok, Is.True, load.Reason);
                Assert.That(truck.localPosition.z - parked.z, Is.EqualTo(bay.driveDistance).Within(1e-4f), "accepted: the truck drives away");
                Assert.That(lesson.whole.piece.gameObject.activeSelf, Is.False, "the crate left with the truck");

                var restart = lesson.TryRestartChapter();
                Assert.That(restart.Ok, Is.True);
                Assert.That(truck.gameObject.activeSelf, Is.True); Assert.That(truck.localPosition, Is.EqualTo(parked));
                Assert.That(lesson.whole.piece.gameObject.activeSelf, Is.True); Assert.That(lesson.whole.piece.localPosition, Is.EqualTo(lesson.whole.trayPosition));

                lesson.JumpToChapter(1);   // two pickups: the whole crate is too long for one pickup bed
                Assert.That(bay.Current.Count, Is.EqualTo(2));
                Assert.That(lesson.DropAt("whole", new Vector3(0.1f, 0.08f, 0.02f)), Is.False);
                Assert.That(lesson.Feedback, Is.Not.Empty, "the refusal says why");
                Assert.That(lesson.whole.piece.localPosition, Is.EqualTo(lesson.whole.trayPosition), "refused crate returns to the tray");
                Assert.That(lesson.TryNextChapter().Ok, Is.False, "not loaded yet");
            }
            finally { Object.DestroyImmediate(station); }
        }

        // ---- pure copy rules ----
        [Test] public void BodyStartsWithStoryAndTask()
        {
            foreach (var chapter in CargoChapter.All)
            {
                string text = CargoLessonDirector.ComposeBody(chapter, "That fills 3/4 of the container. 1/4 more fits.", "");
                Assert.That(text, Does.StartWith(chapter.Story + "\n" + chapter.Task));
                Assert.That(text, Does.EndWith("1/4 more fits."));
            }
            Assert.That(CargoLessonDirector.TitleFor(CargoChapter.All[2]), Is.EqualTo("Chapter 3 · Four vans"));
        }

        [Test] public void SayHintsFollowTheState()
        {
            var big = CargoChapter.All[0]; var pickups = CargoChapter.All[1]; var last = CargoChapter.All[CargoChapter.All.Count - 1];
            Assert.That(CargoLessonDirector.SayHintsFor(big, false, false, false), Does.Not.Contain("split it").And.Contain("load it"));
            Assert.That(CargoLessonDirector.SayHintsFor(pickups, false, true, false), Does.StartWith("Say \"split it\""));
            Assert.That(CargoLessonDirector.SayHintsFor(pickups, false, false, false), Does.Not.Contain("split it"));
            Assert.That(CargoLessonDirector.SayHintsFor(pickups, true, false, false), Does.Contain("next chapter").And.Not.Contain("load it"));
            Assert.That(CargoLessonDirector.SayHintsFor(last, true, false, true), Does.Not.Contain("next chapter").And.Contain("back to the lessons"));
        }

        [Test] public void RecapNamesOnlyCompletedChapters()
        {
            Assert.That(CargoLessonDirector.RecapLine(new List<CargoChapter>()), Is.Empty);
            string recap = CargoLessonDirector.RecapLine(new List<CargoChapter> { CargoChapter.All[0], CargoChapter.All[2] });
            Assert.That(recap, Does.Contain("Big truck").And.Contain("Four vans").And.Not.Contain("Two pickups"));
            foreach (var banned in new[] { "star", "score", "great job", "points" }) Assert.That(recap.ToLower(), Does.Not.Contain(banned));
        }
    }
}
