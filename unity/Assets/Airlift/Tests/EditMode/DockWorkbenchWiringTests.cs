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
        // Owner 2026-09-17: crates twice as tall and deep (length stays exact to the ruler); beds deepen to match.
        static class BuildGeometry
        {
            public const float CrateHeight = 0.11f, CrateDepth = 0.12f, BedTop = 0.052f, DeckTop = 0.0175f;
            public const float BedDepthHalf = 0.065f, Rear = -0.0725f;
            public const float RestHeight = BedTop + CrateHeight / 2, TrayHeight = DeckTop + CrateHeight / 2;
        }
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

        /// Owner 2026-09-17: "the blocks ... about 100% bigger". Every crate is twice as tall and deep with labels twice
        /// as large; length stays exactly its cells, trays sit on the deck and loaded crates rest on the bed floor.
        [Test] public void CratesAreChunkyWithLargeLabels()
        {
            var l = Lesson();
            foreach (var v in Views(l))
            {
                float length = RulerLayout.PieceLength(v.id == "whole" ? 8 : v.id.StartsWith("half") ? 4 : 2);
                var box = v.piece.GetComponent<BoxCollider>();
                Assert.That(box.size.x, Is.EqualTo(length).Within(1e-4f), v.id + " length stays exact to the ruler");
                Assert.That(box.size.y, Is.EqualTo(BuildGeometry.CrateHeight).Within(1e-4f), v.id + " grab box height");
                Assert.That(box.size.z, Is.EqualTo(BuildGeometry.CrateDepth).Within(1e-4f), v.id + " grab box depth");
                var body = v.piece.Find("Body").GetComponent<MeshFilter>().sharedMesh.bounds.size;
                Assert.That(body.x, Is.EqualTo(length).Within(1e-4f)); Assert.That(body.y, Is.EqualTo(BuildGeometry.CrateHeight).Within(1e-4f), v.id); Assert.That(body.z, Is.EqualTo(BuildGeometry.CrateDepth).Within(1e-4f), v.id);
                Assert.That(v.trayPosition.y, Is.EqualTo(BuildGeometry.TrayHeight).Within(1e-4f), v.id + " sits on the deck in the tray");
                Assert.That(v.label.numerator.fontSize, Is.GreaterThanOrEqualTo(0.3f), v.id + " label twice as large");
                Assert.That(v.label.transform.localPosition.y, Is.GreaterThan(BuildGeometry.CrateHeight / 2), v.id + " label on the top face");
                foreach (var t in v.piece.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("Cut edge") || t.name == "Strap"))
                    Assert.That(t.GetComponent<MeshFilter>().sharedMesh.bounds.size.y, Is.GreaterThan(BuildGeometry.CrateHeight), v.id + "/" + t.name + " covers the crate height");
            }
        }

        /// Owner 2026-09-17: the practice and demonstration crates were smaller than the lesson crates. Both are the chunky
        /// size now (28 x 11 x 12 cm) with their stripe and grab kept, resting on the deck in the tray and on the pad.
        [Test] public void PracticeCratesAreChunkyAndRestOnTheDeckAndPad()
        {
            var d = Onboarding(); var root = d.transform;
            var size = new Vector3(RulerLayout.WholeLength, BuildGeometry.CrateHeight, BuildGeometry.CrateDepth);
            foreach (var crate in new[] { d.strap, d.demonstrationStrap })
            {
                var mesh = crate.GetComponent<MeshFilter>().sharedMesh.bounds.size;
                Assert.That(Vector3.Distance(mesh, size), Is.LessThan(1e-4f), crate.name + " body is the chunky crate size, was " + mesh.ToString("F3"));
                Assert.That(crate.localScale, Is.EqualTo(Vector3.one), crate.name);
                Assert.That(crate.localPosition, Is.EqualTo(d.content.trayPosition), crate.name + " waits in the tray");
            }
            var box = d.strap.GetComponent<BoxCollider>();
            Assert.That(Vector3.Distance(box.size, size), Is.LessThan(1e-4f), "practice grab box matches the crate");
            Assert.That(d.grabbable, Is.Not.Null); Assert.That(d.strap.GetComponentInChildren<Oculus.Interaction.GrabInteractable>(true), Is.Not.Null, "still grabbable by controller");
            var stripe = d.demonstrationStrap.Find("Demo stripe"); Assert.That(stripe, Is.Not.Null, "the demo crate keeps its stripe");
            var stripeBounds = stripe.GetComponent<MeshFilter>().sharedMesh.bounds;
            Assert.That(stripe.localPosition.y + stripeBounds.min.y, Is.GreaterThanOrEqualTo(BuildGeometry.CrateHeight / 2 - 1e-3f), "stripe on the top face");
            Assert.That(stripeBounds.size.z, Is.GreaterThanOrEqualTo(BuildGeometry.CrateDepth), "stripe wraps the full depth");

            Assert.That(d.content.trayPosition.y, Is.EqualTo(BuildGeometry.TrayHeight).Within(1e-4f), "tray crate sits on the deck");
            var inset = d.station.GetComponentsInChildren<MeshFilter>(true).First(f => f.name == "Pad inset");
            float padTop = StationBounds(root, inset).max.y;
            float bottom = d.content.targetPosition.y - BuildGeometry.CrateHeight / 2;
            Assert.That(bottom, Is.InRange(padTop, padTop + 0.003f), "placed crate rests on the pad inset");
        }

        // ---- Dock 7 splitter ----
        static CrateSplitter Splitter() => Find<CrateSplitter>();

        [Test] public void SplitterIsWiredToTheLessonWithTwoAlwaysVisibleButtons()
        {
            var d = Onboarding(); var l = Lesson(); var n = Find<NerdyDirector>(); var s = Splitter();
            Assert.That(s, Is.Not.Null, "Dock splitter");
            Assert.That(s.lesson, Is.SameAs(l)); Assert.That(s.stationRoot, Is.SameAs(d.transform));
            Assert.That(s.transform.IsChildOf(l.chapterObjects.transform), Is.True, "shown with the chapter on the table");
            Assert.That(s.visuals, Is.Not.Null); Assert.That(s.visuals.transform.IsChildOf(s.transform), Is.True); Assert.That(s.visuals.activeSelf, Is.True);
            Assert.That(s.gameObject.activeSelf, Is.True);
            foreach (var (button, label, method) in new[] { (s.halvesButton, "Halves · 1/2", nameof(CrateSplitter.ChooseHalves)), (s.quartersButton, "Quarters · 1/4", nameof(CrateSplitter.ChooseQuarters)) })
            {
                Assert.That(button, Is.Not.Null, label);
                Assert.That(button.transform.IsChildOf(s.visuals.transform), Is.True, label);
                Assert.That(button.GetComponentInChildren<TMP_Text>(true).text, Is.EqualTo(label));
                bool wired = Enumerable.Range(0, button.onClick.GetPersistentEventCount()).Any(i => button.onClick.GetPersistentTarget(i) == s && button.onClick.GetPersistentMethodName(i) == method);
                Assert.That(wired, Is.True, label + " -> " + method);
                Assert.That(button.GetComponent<UiPressLog>(), Is.Not.Null, label + " logs presses");
                foreach (var group in n.fallbackButtons) Assert.That(button.transform.IsChildOf(group.transform), Is.False, label + " stays visible while the guide listens");
                var label3d = button.GetComponentInChildren<TMP_Text>(true);
                Assert.That(label3d.GetPreferredValues(label).x, Is.LessThanOrEqualTo(label3d.rectTransform.rect.width + 1f), label + " fits");
            }
            Assert.That(s.feedback, Is.Not.Null); Assert.That(s.feedback.transform.IsChildOf(s.visuals.transform), Is.True);
            Assert.That(s.visuals.GetComponentsInChildren<Oculus.Interaction.PointableCanvas>(true).Any(), Is.True, "controller rays can press the buttons");
            var canvas = s.halvesButton.GetComponentInParent<Canvas>(true); Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.WorldSpace));
            Assert.That(canvas.GetComponent<GraphicRaycaster>(), Is.Not.Null);
        }

        [Test] public void SplitterFeedbackFitsForEveryLine()
        {
            var s = Splitter(); var f = s.feedback;
            var lines = new List<string> { SplitterRules.NoCrateReason, SplitterRules.NoSplitReason, SplitterRules.AlreadySplitReason, CargoLessonDirector.HeldReason, CargoLessonDirector.ClosedReason };
            foreach (var chapter in CargoChapter.All.Where(c => c.SplitTo > 0))
            {
                lines.Add(SplitterRules.Hint(chapter.SplitTo, chapter.VehicleKind));
                var m = new CargoLessonModel(); m.StartChapter(chapter.Number - 1);
                Assert.That(m.Split(false, m.Generation), Is.True); lines.Add(m.LastFeedback);
            }
            foreach (var line in lines)
                Assert.That(f.GetPreferredValues(line, f.rectTransform.rect.width, 1000).y, Is.LessThanOrEqualTo(f.rectTransform.rect.height + 1f), line);
        }

        /// Front-right of the dock, reachable seated: the pad fits a whole crate and the whole splitter clears the crate
        /// tray, the exit lane and every departing vehicle, the staging platform, the table handle, the deck edges and the
        /// sightline to the lesson card.
        [Test] public void SplitterFitsAWholeCrateAndClearsTheDock()
        {
            var d = Onboarding(); var l = Lesson(); var s = Splitter(); var root = d.transform; var terminal = Find<CargoTerminalView>();
            Assert.That(s.padSize.x, Is.GreaterThanOrEqualTo(RulerLayout.WholeLength + 0.01f), "a whole crate fits the pad's width");
            Assert.That(s.padSize.y, Is.GreaterThanOrEqualTo(BuildGeometry.CrateDepth + 0.01f), "and its depth");
            var pad = s.visuals.GetComponentsInChildren<MeshFilter>(true).First(f => f.name == "Splitter pad");
            var padBounds = StationBounds(root, pad);
            Assert.That(padBounds.size.x, Is.EqualTo(s.padSize.x).Within(2e-3f)); Assert.That(padBounds.size.z, Is.EqualTo(s.padSize.y).Within(2e-3f));
            Assert.That(padBounds.center.x, Is.EqualTo(s.padCenter.x).Within(1e-3f)); Assert.That(padBounds.center.z, Is.EqualTo(s.padCenter.z).Within(1e-3f));
            Assert.That(s.padCenter.y, Is.EqualTo(padBounds.max.y).Within(3e-3f), "padCenter is the top surface");

            var parts = s.visuals.GetComponentsInChildren<MeshFilter>(true).Select(f => (f.name, b: StationBounds(root, f))).ToList();
            foreach (var rect in s.visuals.GetComponentsInChildren<Canvas>(true).Select(c => c.GetComponent<RectTransform>()))
            {
                var corners = new Vector3[4]; rect.GetWorldCorners(corners);
                var b = new Bounds(root.InverseTransformPoint(corners[0]), Vector3.zero); foreach (var c in corners) b.Encapsulate(root.InverseTransformPoint(c));
                parts.Add(("canvas " + rect.name, b));
            }
            Assert.That(parts.Count, Is.GreaterThan(2));
            var deck = root.Find("Workbench").GetComponent<MeshFilter>().sharedMesh.bounds;
            var obstacles = new List<(string name, Bounds b)>();
            foreach (var view in Views(l)) { var box = view.piece.GetComponent<BoxCollider>(); obstacles.Add(("tray " + view.id, new Bounds(view.trayPosition + box.center, box.size))); }
            if (terminal.staging != null) foreach (var f in terminal.staging.GetComponentsInChildren<MeshFilter>(true)) obstacles.Add(("staging/" + f.name, StationBounds(root, f)));
            foreach (var crane in terminal.cranes) foreach (var f in crane.GetComponentsInChildren<MeshFilter>(true)) obstacles.Add((crane.name + "/" + f.name, StationBounds(root, f)));
            var handle = d.GetComponent<TableHandle>();
            foreach (var f in handle.grabbable.transform.GetComponentsInChildren<MeshFilter>(true)) obstacles.Add(("handle/" + f.name, StationBounds(root, f)));
            foreach (var f in l.ruler.GetComponentsInChildren<MeshFilter>(true)) obstacles.Add(("ruler/" + f.name, StationBounds(root, f)));
            var exit = terminal.transform.Find("Dock exit"); foreach (var f in exit.GetComponentsInChildren<MeshFilter>(true)) obstacles.Add(("gate/" + f.name, StationBounds(root, f)));
            foreach (var tag in new[] { "STAGING tag", "ARRIVALS tag" })
            {
                var rt = terminal.transform.Find(tag) as RectTransform; if (rt == null) continue;
                var tagCorners = new Vector3[4]; rt.GetWorldCorners(tagCorners);
                var tb = new Bounds(root.InverseTransformPoint(tagCorners[0]), Vector3.zero); foreach (var c in tagCorners) tb.Encapsulate(root.InverseTransformPoint(c));
                obstacles.Add((tag, tb));
            }
            foreach (var part in parts)
            {
                Assert.That(part.b.max.y, Is.LessThan(0.19f), part.name + " below the sightline to the lesson card");
                Assert.That(Mathf.Abs(part.b.center.x) + part.b.extents.x <= deck.extents.x && Mathf.Abs(part.b.center.z) + part.b.extents.z <= deck.extents.z, Is.True, part.name + " on the deck: " + part.b);
                Assert.That(part.b.max.z < -0.195f || part.b.min.z > 0.09f || part.b.min.x >= 0.14f, Is.True, part.name + " clear of the exit lane band");
                foreach (var o in obstacles) Assert.That(Flat(part.b).Intersects(Flat(o.b)), Is.False, part.name + " overlaps " + o.name);
            }

            // Every chapter's vehicles, parked and all along their departure, never touch the splitter.
            var bay = l.vehicles;
            var saved = bay.vehicles.Select(v => (v.root.localPosition, v.root.localRotation, v.root.gameObject.activeSelf)).ToList();
            try
            {
                foreach (var chapter in CargoChapter.All)
                {
                    bay.ShowChapter(chapter);
                    var current = bay.Current.ToList(); var parked = current.Select(v => v.root.localPosition).ToList();
                    for (float t = 0f; t <= bay.DepartureSeconds; t += 0.05f)
                        for (int i = 0; i < current.Count; i++)
                        {
                            var pose = bay.DeparturePose(current[i], parked[i], t); if (pose.gone) continue;
                            current[i].root.localPosition = pose.position; current[i].root.localRotation = Quaternion.Euler(0, pose.yaw, 0);
                            var vb = new Bounds(); bool has = false;
                            foreach (var f in current[i].root.GetComponentsInChildren<MeshFilter>(true)) { var fb = StationBounds(root, f); if (!has) { vb = fb; has = true; } else vb.Encapsulate(fb); }
                            foreach (var part in parts) Assert.That(Flat(part.b).Intersects(Flat(vb)), Is.False, chapter.Id + ": " + current[i].root.name + " hits " + part.name + " at t=" + t.ToString("0.00"));
                        }
                }
            }
            finally
            {
                bay.ResetBay();
                for (int i = 0; i < saved.Count; i++) { var tr = bay.vehicles[i].root; tr.localPosition = saved[i].Item1; tr.localRotation = saved[i].Item2; tr.gameObject.SetActive(saved[i].Item3); }
            }
        }

        /// Footprint on the deck (y collapsed) so a tall part never slips past a low one in the check.
        static Bounds Flat(Bounds b) => new Bounds(new Vector3(b.center.x, 0, b.center.z), new Vector3(b.size.x, 1, b.size.z));

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
            Assert.That(l.restHeight, Is.EqualTo(BuildGeometry.RestHeight).Within(1e-3f), "crates rest on the bed floor");
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
                Assert.That(bay.parkHeight + floor.localPosition.y + floorBounds.extents.y, Is.EqualTo(l.restHeight - BuildGeometry.CrateHeight / 2).Within(1e-3f), v.root.name + " bed floor carries the crates");
                Assert.That(floorBounds.extents.z * 2, Is.GreaterThan(BuildGeometry.CrateDepth), v.root.name + " bed holds a crate's depth");
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
                Assert.That(park.z + BuildGeometry.Rear, Is.GreaterThan(l.whole.trayPosition.z + BuildGeometry.CrateDepth / 2), v.root.name + " rear stays clear of the tray row");
                Assert.That(bay.exitX - BuildGeometry.Rear, Is.LessThan(-deck.extents.x), v.root.name + " leaves the deck on the left before it disappears");
            }
            Assert.That(terminal.truck.gameObject.activeSelf, Is.True, "idle truck parked outside the lesson");
            foreach (var v in bay.vehicles.Where(v => v.kind != "truck")) Assert.That(v.root.gameObject.activeSelf, Is.False, v.root.name);
            Assert.That(bay.unusedCells.Length, Is.EqualTo(PlacementState.CellsPerWhole));
            foreach (var c in bay.unusedCells) { Assert.That(c.transform.IsChildOf(l.ruler), Is.True); Assert.That(c.activeSelf, Is.False); }
            Assert.That(bay.notNeededLabel, Is.Not.Null);
        }

        /// Owner 2026-09-17: "the trucks don't really look like trucks". Every vehicle has a cab with windows, lights,
        /// mirrors, a plate and wheels, is longer than it is wide, and stays low enough to read under the lesson card.
        [Test] public void VehiclesLookLikeTrucks()
        {
            var d = Onboarding(); var bay = Lesson().vehicles;
            foreach (var v in bay.vehicles)
            {
                var names = v.root.GetComponentsInChildren<Transform>(true).Select(t => t.name).ToList();
                foreach (var part in new[] { "Cab", "Windshield", "Rear window", "Headlight left", "Headlight right", "Tail light left", "Tail light right",
                                             "Side mirror left", "Side mirror right", "License plate", "Front bumper", "Rear bumper" })
                    Assert.That(names, Does.Contain(part), v.root.name + " has a " + part);
                Assert.That(names.Count(n => n == "Wheel"), Is.GreaterThanOrEqualTo(v.kind == "truck" ? 6 : 4), v.root.name + " wheels");
                if (v.kind != "van") { Assert.That(names, Does.Contain("Hood"), v.root.name); Assert.That(names, Does.Contain("Grille"), v.root.name); }
                if (v.kind == "truck") Assert.That(names, Does.Contain("Exhaust stack"));
                float w = RulerLayout.PieceLength(v.bedCells);
                float minZ = float.MaxValue, maxZ = float.MinValue, maxY = float.MinValue;
                foreach (var f in v.root.GetComponentsInChildren<MeshFilter>(true))
                {
                    var b = f.sharedMesh.bounds;
                    foreach (float x in new[] { b.min.x, b.max.x }) foreach (float y in new[] { b.min.y, b.max.y }) foreach (float z in new[] { b.min.z, b.max.z })
                    {
                        var p = v.root.InverseTransformPoint(f.transform.TransformPoint(new Vector3(x, y, z)));
                        minZ = Mathf.Min(minZ, p.z); maxZ = Mathf.Max(maxZ, p.z); maxY = Mathf.Max(maxY, p.y);
                    }
                }
                Assert.That(minZ, Is.GreaterThanOrEqualTo(BuildGeometry.Rear - 0.0005f), v.root.name + " rear bumper stays clear of the tray row");
                Assert.That(v.noseZ, Is.EqualTo(maxZ).Within(0.004f), v.root.name + " noseZ matches the model");
                Assert.That(maxZ - minZ, Is.GreaterThanOrEqualTo(v.kind == "truck" ? 1.0f * w : 1.6f * w), v.root.name + " is longer than it is wide");
                Assert.That(bay.parkHeight + maxY, Is.LessThan(0.19f), v.root.name + " stays below the sightline to the lesson card");
            }
        }

        /// Loaded vehicles turn left into a lane and leave through the DOCK EXIT gate. Along the lane every vehicle clears
        /// the cranes, containers, gate posts and the crate tray; the gate stands between the beds and the deck's left edge.
        [Test] public void DockExitLaneIsClearForEveryVehicle()
        {
            var d = Onboarding(); var l = Lesson(); var terminal = Find<CargoTerminalView>(); var bay = l.vehicles;
            var root = d.transform;
            var exit = terminal.transform.Find("Dock exit"); Assert.That(exit, Is.Not.Null, "dock exit gate");
            Assert.That(exit.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == "DOCK EXIT"), Is.True);
            var deck = root.Find("Workbench").GetComponent<MeshFilter>().sharedMesh.bounds;
            float gateX = root.InverseTransformPoint(exit.position).x;
            Assert.That(gateX, Is.LessThan(-RulerLayout.WholeLength / 2 - 0.1f)); Assert.That(gateX, Is.GreaterThan(-deck.extents.x));
            foreach (var r in exit.GetComponentsInChildren<MeshFilter>(true))
                Assert.That(StationBounds(root, r).max.y, Is.LessThan(0.19f), "gate/" + r.name + " below the card sightline");

            var obstacles = new List<(string name, Bounds b)>();
            foreach (var crane in terminal.cranes) foreach (var f in crane.GetComponentsInChildren<MeshFilter>(true)) obstacles.Add((crane.name + "/" + f.name, StationBounds(root, f)));
            foreach (var c in terminal.containers) foreach (var f in c.GetComponentsInChildren<MeshFilter>(true)) obstacles.Add((c.name + "/" + f.name, StationBounds(root, f)));
            foreach (var f in exit.GetComponentsInChildren<MeshFilter>(true)) if (f.name.StartsWith("Post")) obstacles.Add(("gate/" + f.name, StationBounds(root, f)));
            foreach (var view in Views(l))
            {
                var box = view.piece.GetComponent<BoxCollider>(); Assert.That(box, Is.Not.Null, view.id);
                obstacles.Add(("tray " + view.id, new Bounds(view.trayPosition + box.center, box.size)));
            }

            var saved = bay.vehicles.Select(v => (v.root.localPosition, v.root.localRotation, v.root.gameObject.activeSelf)).ToList();
            try
            {
                foreach (var v in bay.vehicles)
                {
                    Vector3 parked = bay.ParkPosition(0, v.bedCells);
                    float laneStart = bay.DeparturePose(v, parked, bay.turnSeconds).position.x;
                    for (float x = laneStart; x > bay.exitX; x -= 0.01f)
                    {
                        v.root.localPosition = new Vector3(x, bay.parkHeight, bay.laneZ); v.root.localRotation = Quaternion.Euler(0, -90f, 0);
                        foreach (var f in v.root.GetComponentsInChildren<MeshFilter>(true))
                        {
                            var vb = StationBounds(root, f);
                            foreach (var o in obstacles)
                                Assert.That(vb.Intersects(o.b), Is.False, v.root.name + "/" + f.name + " hits " + o.name + " at x=" + x.ToString("0.00"));
                        }
                    }
                }
            }
            finally { for (int i = 0; i < saved.Count; i++) { var t = bay.vehicles[i].root; t.localPosition = saved[i].Item1; t.localRotation = saved[i].Item2; t.gameObject.SetActive(saved[i].Item3); } }

            var idle = terminal.truck; Assert.That(idle.gameObject.activeSelf, Is.True);
            var practice = d.station.GetComponentsInChildren<MeshFilter>(true).Select(f => StationBounds(root, f)).ToList();
            foreach (var f in idle.GetComponentsInChildren<MeshFilter>(true))
            {
                var vb = StationBounds(root, f);
                foreach (var o in obstacles.Where(o => !o.name.StartsWith("tray"))) Assert.That(vb.Intersects(o.b), Is.False, "idle truck/" + f.name + " hits " + o.name);
                foreach (var p in practice) Assert.That(vb.Intersects(p), Is.False, "idle truck/" + f.name + " sits on the practice pad");
                Assert.That(Mathf.Abs(vb.center.x) + vb.extents.x <= deck.extents.x && Mathf.Abs(vb.center.z) + vb.extents.z <= deck.extents.z, Is.True, "idle truck/" + f.name + " on the deck");
            }
        }

        /// Station-local axis-aligned bounds of one mesh (works for inactive objects, unlike Renderer.bounds).
        static Bounds StationBounds(Transform root, MeshFilter f)
        {
            var m = f.sharedMesh.bounds; var b = new Bounds(); bool has = false;
            foreach (float x in new[] { m.min.x, m.max.x }) foreach (float y in new[] { m.min.y, m.max.y }) foreach (float z in new[] { m.min.z, m.max.z })
            {
                var p = root.InverseTransformPoint(f.transform.TransformPoint(new Vector3(x, y, z)));
                if (!has) { b = new Bounds(p, Vector3.zero); has = true; } else b.Encapsulate(p);
            }
            return b;
        }

        // ---- behaviour on in-memory objects (no scene changes) ----
        static (GameObject station, Transform ruler, VehicleBay bay) MakeDock()
        {
            var station = new GameObject("test station");
            var ruler = new GameObject("ruler").transform; ruler.SetParent(station.transform, false); ruler.localPosition = new Vector3(0, 0.031f, 0.02f);
            var bay = new GameObject("bay").AddComponent<VehicleBay>(); bay.transform.SetParent(station.transform, false);
            bay.ruler = ruler;
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
                Vector3 inBed = crate.localPosition - parked;
                bay.DriveAway(new List<Transform> { crate });
                Assert.That(bay.DrivenAway, Is.True);
                Assert.That(pickup.root.localPosition.x, Is.LessThanOrEqualTo(bay.exitX + 1e-4f), "vehicle drove left out through the dock exit");
                Assert.That(pickup.root.localPosition.z, Is.EqualTo(bay.laneZ).Within(1e-4f), "along the exit lane");
                Assert.That(Vector3.Distance(pickup.root.localRotation * Vector3.forward, Vector3.left), Is.LessThan(1e-3f), "facing the exit");
                Assert.That(Vector3.Distance(pickup.root.InverseTransformPoint(crate.position), inBed), Is.LessThan(1e-4f), "the crate rides along in the bed");
                Assert.That(pickup.root.gameObject.activeSelf || crate.gameObject.activeSelf, Is.False, "gone once off the deck");

                bay.ShowChapter(CargoChapter.All[3]);
                Assert.That(bay.DrivenAway, Is.False);
                Assert.That(bay.Current[0].root.gameObject.activeSelf, Is.True); Assert.That(bay.Current[0].root.localPosition, Is.EqualTo(parked), "restored for a restart");
                Assert.That(bay.Current[0].root.localRotation, Is.EqualTo(Quaternion.identity), "parked facing away from the learner again");
                Assert.That(bay.Current[0].loadedTag.activeSelf, Is.False);
            }
            finally { Object.DestroyImmediate(station); }
        }

        /// Owner 2026-09-17: loaded vehicles turn left and drive out through the dock exit, leftmost first, and never
        /// run into each other on the way (vehicle footprints as rectangles: bed width by rear bumper to nose).
        [Test] public void DepartureTurnsLeftIntoTheLaneAndLeavesInOrderWithoutCollisions()
        {
            var (station, ruler, bay) = MakeDock();
            try
            {
                foreach (var v in bay.vehicles) v.noseZ = v.kind == "truck" ? 0.32f : v.kind == "pickup" ? 0.22f : 0.15f;
                foreach (var chapter in CargoChapter.All)
                {
                    bay.ShowChapter(chapter);
                    var vehicles = bay.Current.ToList();
                    var parked = vehicles.Select(v => v.root.localPosition).ToList();
                    for (int i = 0; i < vehicles.Count; i++)
                    {
                        var start = bay.DeparturePose(vehicles[i], parked[i], 0f);
                        Assert.That(Vector3.Distance(start.position, parked[i]), Is.LessThan(1e-5f), chapter.Id + " starts parked");
                        Assert.That(start.yaw, Is.EqualTo(0f).Within(1e-3f)); Assert.That(start.gone, Is.False);
                        var turned = bay.DeparturePose(vehicles[i], parked[i], bay.DepartureStart(vehicles[i]) + bay.turnSeconds);
                        Assert.That(turned.position.z, Is.EqualTo(bay.laneZ).Within(1e-4f), chapter.Id + " turned into the lane");
                        Assert.That(turned.yaw, Is.EqualTo(-90f).Within(1e-3f), "a left turn");
                        var later = bay.DeparturePose(vehicles[i], parked[i], bay.DepartureStart(vehicles[i]) + bay.turnSeconds + 0.4f);
                        Assert.That(later.position.x, Is.LessThan(turned.position.x), "drives left toward the exit");
                        var end = bay.DeparturePose(vehicles[i], parked[i], bay.DepartureSeconds + 0.01f);
                        Assert.That(end.gone, Is.True); Assert.That(end.position.x, Is.LessThanOrEqualTo(bay.exitX + 1e-4f));
                        if (i > 0) Assert.That(bay.DepartureStart(vehicles[i]), Is.GreaterThan(bay.DepartureStart(vehicles[i - 1])), "leftmost leaves first");
                    }
                    for (float t = 0f; t <= bay.DepartureSeconds; t += 0.02f)
                        for (int a = 0; a < vehicles.Count; a++)
                            for (int b = a + 1; b < vehicles.Count; b++)
                            {
                                var pa = bay.DeparturePose(vehicles[a], parked[a], t); var pb = bay.DeparturePose(vehicles[b], parked[b], t);
                                if (pa.gone || pb.gone) continue;
                                Assert.That(Overlap(Rect(vehicles[a], pa), Rect(vehicles[b], pb)), Is.False,
                                    chapter.Id + ": " + vehicles[a].root.name + " and " + vehicles[b].root.name + " collide at t=" + t.ToString("0.00"));
                            }
                }
            }
            finally { Object.DestroyImmediate(station); }
        }

        static Vector2[] Rect(VehicleBay.Vehicle v, VehicleBay.Pose pose)
        {
            float half = RulerLayout.PieceLength(v.bedCells) / 2 - 0.001f;   // neighbours park touching; shrink 1 mm
            var q = Quaternion.Euler(0, pose.yaw, 0);
            return new[] { new Vector3(-half, 0, BuildGeometry.Rear + 0.001f), new Vector3(half, 0, BuildGeometry.Rear + 0.001f), new Vector3(half, 0, v.noseZ - 0.001f), new Vector3(-half, 0, v.noseZ - 0.001f) }
                .Select(c => { var w = pose.position + q * c; return new Vector2(w.x, w.z); }).ToArray();
        }

        static bool Overlap(Vector2[] a, Vector2[] b)
        {
            foreach (var poly in new[] { a, b })
                for (int i = 0; i < 4; i++)
                {
                    var edge = poly[(i + 1) % 4] - poly[i]; var axis = new Vector2(-edge.y, edge.x);
                    float aMin = a.Min(p => Vector2.Dot(p, axis)), aMax = a.Max(p => Vector2.Dot(p, axis));
                    float bMin = b.Min(p => Vector2.Dot(p, axis)), bMax = b.Max(p => Vector2.Dot(p, axis));
                    if (aMax <= bMin || bMax <= aMin) return false;
                }
            return true;
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
                Assert.That(truck.localPosition.x, Is.LessThanOrEqualTo(bay.exitX + 1e-4f), "accepted: the truck drives out through the dock exit");
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

        /// Lock item L-2: leaving the lesson mid-load or right after an accepted load and coming back gives a coherent
        /// chapter: crates in the tray, this chapter's vehicles parked, nothing left driving away.
        [Test] public void LeavingAndReturningRestoresACoherentChapter()
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
                lesson.whole = View("whole", new Vector3(-0.13f, 0.047f, -0.225f));
                lesson.halfA = View("half-1", new Vector3(-0.21f, 0.047f, -0.225f));
                lesson.halfB = View("half-2", new Vector3(-0.05f, 0.047f, -0.225f));
                lesson.quarters = Enumerable.Range(1, 4).Select(i => View("quarter-" + i, new Vector3(-0.265f + (i - 1) * 0.09f, 0.047f, -0.225f))).ToArray();

                // Mid-load: crate docked, not checked. Leave, come back.
                lesson.Begin();
                Assert.That(lesson.DropAt("whole", new Vector3(0.01f, 0.08f, 0.03f)), Is.True);
                lesson.Exit();
                lesson.Begin();
                Assert.That(lesson.Chapter.Number, Is.EqualTo(1), "same chapter");
                Assert.That(lesson.ChapterComplete, Is.False);
                Assert.That(lesson.whole.piece.gameObject.activeSelf, Is.True);
                Assert.That(lesson.whole.piece.localPosition, Is.EqualTo(lesson.whole.trayPosition), "crate back in the tray");
                Assert.That(bay.DrivenAway, Is.False); Assert.That(bay.Current.Count, Is.EqualTo(1)); Assert.That(bay.Current[0].root.gameObject.activeSelf, Is.True);

                // Accepted load drove away: leave, come back to the next chapter with its own vehicles.
                Assert.That(lesson.DropAt("whole", new Vector3(0.01f, 0.08f, 0.03f)), Is.True);
                Assert.That(lesson.TryLoad().Ok, Is.True); Assert.That(bay.DrivenAway, Is.True);
                lesson.Exit();
                lesson.Begin();
                Assert.That(lesson.Chapter.Number, Is.EqualTo(2), "returning after an accepted load continues with the next chapter");
                Assert.That(bay.DrivenAway, Is.False);
                Assert.That(bay.Current.Count, Is.EqualTo(2)); Assert.That(bay.Current.All(v => v.root.gameObject.activeSelf), Is.True, "both pickups parked and visible");
                Assert.That(lesson.whole.piece.gameObject.activeSelf, Is.True, "chapter 2 starts from one whole crate in the tray");
                Assert.That(lesson.whole.piece.localPosition, Is.EqualTo(lesson.whole.trayPosition));
                Assert.That(lesson.TryReset().Ok, Is.True, "reset works in the resumed chapter");
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
