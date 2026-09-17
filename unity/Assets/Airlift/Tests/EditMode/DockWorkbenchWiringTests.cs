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
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }
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

        // ---- container floor ----
        [Test] public void ContainerFloorSkinOnTheRuler()
        {
            var l = Lesson();
            var floor = l.ruler.Find("Container floor"); Assert.That(floor, Is.Not.Null);
            Assert.That(floor.Cast<Transform>().Count(t => t.name.StartsWith("Slot ")), Is.EqualTo(PlacementState.CellsPerWhole));
            Assert.That(floor.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == "ONE CONTAINER"), Is.True);
            Assert.That(l.ruler.Find("Mark 0"), Is.Not.Null); Assert.That(l.ruler.Find("Mark 1"), Is.Not.Null);
            Assert.That(l.halfMarks.Length, Is.EqualTo(2));
            foreach (var mark in l.halfMarks) { Assert.That(mark.name, Does.Contain("1/2")); Assert.That(mark.transform.parent, Is.EqualTo(l.ruler)); }
        }

        // ---- vehicle bay ----
        [Test] public void VehicleBayHasEveryChaptersVehiclesOnTheDeck()
        {
            var d = Onboarding(); var l = Lesson(); var terminal = Find<CargoTerminalView>();
            var bay = l.vehicles; Assert.That(bay, Is.Not.Null);
            Assert.That(bay.transform.IsChildOf(terminal.transform), Is.True);
            Assert.That(bay.CountOf("truck"), Is.EqualTo(1)); Assert.That(bay.CountOf("pickup"), Is.GreaterThanOrEqualTo(2)); Assert.That(bay.CountOf("van"), Is.GreaterThanOrEqualTo(4));
            Assert.That(bay.vehicles.First(v => v.kind == "truck").root, Is.EqualTo(terminal.truck));
            foreach (var chapter in CargoChapter.All) Assert.That(bay.CountOf(chapter.VehicleKind), Is.GreaterThanOrEqualTo(chapter.VehicleCount), chapter.Id);
            var deck = d.transform.Find("Workbench").GetComponent<MeshFilter>().sharedMesh.bounds;
            foreach (var v in bay.vehicles)
            {
                Assert.That(v.root, Is.Not.Null); Assert.That(v.loadedTag, Is.Not.Null, v.root.name);
                Assert.That(v.loadedTag.activeSelf, Is.False, v.root.name + " tag hidden until loaded");
                Assert.That(v.loadedTag.GetComponentInChildren<TMP_Text>(true).text, Is.EqualTo("LOADED"));
                if (v.kind != "truck") Assert.That(v.root.gameObject.activeSelf, Is.False, v.root.name + " parked out of sight until its chapter");
                Vector3 stationLocal = d.transform.InverseTransformPoint(v.root.parent.TransformPoint(v.root.localPosition + Vector3.forward * (v.distance > 0 ? v.distance : bay.rollDistance)));
                Vector3 homeLocal = d.transform.InverseTransformPoint(v.root.parent.TransformPoint(v.root.localPosition));
                Assert.That(stationLocal.z, Is.GreaterThan(homeLocal.z), v.root.name + " rolls away from the learner");
                Assert.That(Mathf.Abs(stationLocal.x) <= deck.extents.x && Mathf.Abs(stationLocal.z) <= deck.extents.z, Is.True, v.root.name + " rolls out but stays on the deck: " + stationLocal);
            }
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
