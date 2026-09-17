using System.Linq;
using Airlift.Lessons;
using Airlift.Lessons.Cafe;
using Airlift.Onboarding;
using Airlift.Presentation.Cafe;
using Airlift.Welcome;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Airlift.Tests
{
    /// Concept intro "What is dividing?" (docs/00-build/INTRO-SPLITTER-CONTRACTS.md): three steps after the café
    /// briefing, once per app run, before chapter 1. Visuals pose the pastries without touching CafeModel.
    public class CafeIntroTests
    {
        static readonly string[] IntroTools = { "advance_step", "request_help", "back_to_lessons" };

        sealed class Cafe
        {
            public GameObject root; public CafeStation station;
            public Vector3 Local(int index) => root.transform.InverseTransformPoint(station.items[index].piece.position);
        }

        static Cafe MakeCafe()
        {
            var root = new GameObject("test cafe intro");
            Transform Child(Transform parent, string name) { var t = new GameObject(name).transform; t.SetParent(parent, false); return t; }
            GameObject Shape(Transform parent, string name) => Child(parent, name).gameObject;
            var station = root.AddComponent<CafeStation>();
            station.cardId = CafeStation.CardId; station.visualRoots = new[] { root }; station.space = root.transform;
            var pieces = Child(root.transform, "pieces"); station.piecesRoot = pieces.gameObject;
            station.items = Enumerable.Range(1, CafeLayout.MaxItems).Select(i =>
            {
                var piece = Child(pieces, "pastry " + i);
                return new CafeStation.ItemView { id = "item-" + i, piece = piece, croissant = Shape(piece, "Croissant"), cookie = Shape(piece, "Cookie"), muffin = Shape(piece, "Muffin") };
            }).ToArray();
            var targets = Child(root.transform, "targets");
            station.plates = Enumerable.Range(1, CafeLayout.MaxPlates).Select(i => Child(targets, "plate " + i)).ToArray();
            station.boxes = Enumerable.Range(1, CafeLayout.MaxBoxes).Select(i => Child(targets, "box " + i)).ToArray();
            station.orderUpTags = station.boxes.Select(b => { var tag = Child(b, "tag").gameObject; tag.SetActive(false); return tag; }).ToArray();
            var labels = Child(root.transform, "labels");
            station.plateLabels = Enumerable.Range(1, CafeLayout.MaxPlates).Select(i => (TMP_Text)Child(labels, "plate label " + i).gameObject.AddComponent<TextMeshPro>()).ToArray();
            station.boxLabels = Enumerable.Range(1, CafeLayout.MaxBoxes).Select(i => (TMP_Text)Child(labels, "box label " + i).gameObject.AddComponent<TextMeshPro>()).ToArray();
            station.boxCapacityLabels = station.boxes.Select(b => (TMP_Text)Child(b, "capacity").gameObject.AddComponent<TextMeshPro>()).ToArray();
            return new Cafe { root = root, station = station };
        }

        static void AssertNear(Vector3 actual, Vector3 expected, string message) => Assert.That(Vector3.Distance(actual, expected), Is.LessThan(1e-4f), message + ": " + actual + " vs " + expected);

        /// The pieces, plates and boxes on the counter match the intro scene exactly.
        static void AssertScene(Cafe cafe, CafeIntroScene scene, string visual)
        {
            var s = cafe.station;
            Assert.That(s.piecesRoot.activeSelf, Is.True, visual + ": the pastries are visible");
            for (int i = 0; i < s.items.Length; i++)
            {
                var v = s.items[i];
                Assert.That(v.piece.gameObject.activeSelf, Is.EqualTo(i < scene.Items), visual + " " + v.id);
                if (i >= scene.Items) continue;
                Assert.That(v.croissant.activeSelf && !v.cookie.activeSelf && !v.muffin.activeSelf, Is.True, visual + " " + v.id + " is a croissant");
                AssertNear(cafe.Local(i), CafeLayout.IntroItemPosition(scene, i), visual + " " + v.id);
            }
            for (int p = 0; p < s.plates.Length; p++)
            {
                bool on = scene.Kind == CafeTargetKind.Plates && p < scene.Containers;
                Assert.That(s.plates[p].gameObject.activeSelf, Is.EqualTo(on), visual + " plate " + (p + 1));
                Assert.That(s.plateLabels[p].gameObject.activeSelf, Is.EqualTo(on), visual + " plate label " + (p + 1));
                if (on) AssertNear(s.plates[p].localPosition, CafeLayout.ContainerCenter(CafeTargetKind.Plates, p, scene.Containers), visual + " plate " + (p + 1));
            }
            for (int b = 0; b < s.boxes.Length; b++)
            {
                bool on = scene.Kind == CafeTargetKind.Boxes && b < scene.Containers;
                Assert.That(s.boxes[b].gameObject.activeSelf, Is.EqualTo(on), visual + " box " + (b + 1));
                Assert.That(s.boxLabels[b].gameObject.activeSelf, Is.EqualTo(on), visual + " box label " + (b + 1));
                if (!on) continue;
                AssertNear(s.boxes[b].localPosition, CafeLayout.ContainerCenter(CafeTargetKind.Boxes, b, scene.Containers), visual + " box " + (b + 1));
                Assert.That(s.boxCapacityLabels[b].text, Is.EqualTo(scene.BoxCapacity.ToString()), visual + " box capacity");
            }
        }

        static void AssertModelUntouched(CafeStation s)
        {
            Assert.That(s.Model.ChapterIndex, Is.EqualTo(0)); Assert.That(s.Model.StageIndex, Is.EqualTo(0));
            Assert.That(s.Model.LooseCount, Is.EqualTo(6), "the intro never places anything in the model");
            Assert.That(s.Model.ChapterComplete, Is.False);
        }

        // ---- pure ----
        [Test] public void IntroScenesShowSharingThenGroupsThenBoxes()
        {
            var share = CafeLayout.IntroScene("share");
            Assert.That((share.Kind, share.Containers, share.Items, share.Placed), Is.EqualTo((CafeTargetKind.Plates, 2, 6, false)));
            for (int i = 0; i < 6; i++) AssertNear(CafeLayout.IntroItemPosition(share, i), CafeLayout.TrayPosition(i), "share: croissant " + (i + 1) + " on the tray");

            var groups = CafeLayout.IntroScene("groups");
            Assert.That((groups.Kind, groups.Containers, groups.Items, groups.Placed), Is.EqualTo((CafeTargetKind.Plates, 2, 6, true)));
            var perPlate = new int[2];
            for (int i = 0; i < 6; i++)
            {
                int plate = CafeLayout.IntroContainerOf(groups, i, out int slot);
                Assert.That(plate, Is.EqualTo(i % 2), "dealt one to each plate, again and again");
                Assert.That(slot, Is.EqualTo(perPlate[plate]++));
                AssertNear(CafeLayout.IntroItemPosition(groups, i), CafeLayout.SlotPosition(CafeTargetKind.Plates, CafeLayout.ContainerCenter(CafeTargetKind.Plates, plate, 2), slot), "groups: croissant " + (i + 1));
            }
            Assert.That(perPlate, Is.EqualTo(new[] { 3, 3 }), "6 ÷ 2 = 3");

            var boxes = CafeLayout.IntroScene("boxes");
            Assert.That((boxes.Kind, boxes.Containers, boxes.Items, boxes.BoxCapacity, boxes.Placed), Is.EqualTo((CafeTargetKind.Boxes, 2, 6, 3, true)));
            var perBox = new int[2];
            for (int i = 0; i < 6; i++) perBox[CafeLayout.IntroContainerOf(boxes, i, out _)]++;
            Assert.That(perBox, Is.EqualTo(new[] { 3, 3 }), "6 croissants in 2 full boxes of 3");

            Assert.That(CafeLayout.IntroScene("nope").Items, Is.EqualTo(0));
            Assert.That(ConceptIntros.Cafe.Select(st => CafeLayout.IntroScene(st.Visual).Items), Is.All.EqualTo(6), "every café intro visual has a scene");
        }

        [Test] public void IntroStepsAreGroundedAndNeverGrabbable()
        {
            var steps = ConceptIntros.Cafe;
            for (int i = 0; i < steps.Count; i++)
            {
                bool last = i == steps.Count - 1;
                var g = CafeSteps.Intro(steps[i], last);
                Assert.That(g.Id, Is.EqualTo("intro"));
                Assert.That(g.CanGrabNow, Is.False, steps[i].Id);
                Assert.That(g.OnTableNow.Length, Is.LessThanOrEqualTo(GuideSteps.MaxOnTableLength));
                Assert.That(g.OnTableNow, Does.Contain(last ? "Start" : "Next"), steps[i].Id);
            }
            Assert.That(CafeSteps.Intro(steps[0], false).OnTableNow, Does.Contain("6 croissants on the tray").And.Contain("2 empty plates"));
            Assert.That(CafeSteps.Intro(steps[1], false).OnTableNow, Does.Contain("2 plates").And.Contain("3 croissants on each").And.Contain("tray is empty"));
            Assert.That(CafeSteps.Intro(steps[2], true).OnTableNow, Does.Contain("2 boxes").And.Contain("3 croissants in each").And.Not.Contain("plates"));
            Assert.That(CafeStation.IntroHintsFor(false), Is.EqualTo("Say \"next\""));
            Assert.That(CafeStation.IntroHintsFor(true), Is.EqualTo("Say \"start\""));
        }

        // ---- station on in-memory objects ----
        [Test] public void IntroRunsOnceBeforeChapterOneAndPosesTheCounter()
        {
            var cafe = MakeCafe(); var s = cafe.station;
            try
            {
                Assert.That(s.CurrentIntro, Is.Null, "closed");
                s.Open();
                Assert.That(s.CurrentIntro, Is.Null, "briefing first");
                var steps = ConceptIntros.Cafe;
                for (int i = 0; i < steps.Count; i++)
                {
                    var r = s.Advance();
                    Assert.That(r.Ok, Is.True, r.Reason);
                    Assert.That(r.Reason, Is.EqualTo(steps[i].Say), "Advance returns the step's words");
                    Assert.That(s.CurrentIntro, Is.SameAs(steps[i]));
                    Assert.That(s.ChapterActive, Is.False); Assert.That(s.Chapter.Number, Is.EqualTo(0));
                    var step = s.CurrentStep();
                    Assert.That(step.Id, Is.EqualTo("intro")); Assert.That(step.CanGrabNow, Is.False);
                    Assert.That(step.OnTableNow, Is.EqualTo(CafeSteps.Intro(steps[i], i == steps.Count - 1).OnTableNow));
                    Assert.That(s.ToolsNow, Is.EquivalentTo(IntroTools));
                    AssertScene(cafe, CafeLayout.IntroScene(steps[i].Visual), steps[i].Id);
                    AssertModelUntouched(s);
                }
                var start = s.Advance();
                Assert.That(start.Ok, Is.True, start.Reason);
                Assert.That(start.Reason, Does.StartWith("Chapter 1 · Two friends"), "Start after the last step opens chapter 1 as before");
                Assert.That(s.CurrentIntro, Is.Null); Assert.That(s.ChapterActive, Is.True); Assert.That(s.Chapter.Number, Is.EqualTo(1));
                Assert.That(s.ToolsNow, Does.Contain("cafe_deal_round"));
                Assert.That(s.boxes.Any(b => b.gameObject.activeSelf) || s.boxLabels.Any(l => l.gameObject.activeSelf), Is.False, "intro boxes cleared away");
                for (int p = 0; p < 2; p++) AssertNear(s.plates[p].localPosition, CafeLayout.ContainerCenter(CafeTargetKind.Plates, p, 2), "chapter 1 plate " + (p + 1));
                for (int i = 0; i < 6; i++) AssertNear(cafe.Local(i), CafeLayout.TrayPosition(i), "croissant " + (i + 1) + " back on the tray");
                Assert.That(s.Model.LooseCount, Is.EqualTo(6));

                s.Close(); s.Open();
                var again = s.Advance();
                Assert.That(s.CurrentIntro, Is.Null, "once per app run");
                Assert.That(again.Reason, Does.StartWith("Chapter 1 · Two friends"));
            }
            finally { Object.DestroyImmediate(cafe.root); }
        }

        [Test] public void EveryOtherActionWaitsForTheIntro()
        {
            var cafe = MakeCafe(); var s = cafe.station;
            try
            {
                s.Open(); s.Advance(); s.Advance();   // "groups": croissants posed on the plates
                foreach (var r in new[] { s.Check(), s.DealRound(), s.ResetTable(), s.NextChapter(), s.RestartChapter() })
                {
                    Assert.That(r.Ok, Is.False); Assert.That(r.Reason, Is.EqualTo(CafeStation.IntroReason));
                }
                Assert.That(CafeStation.IntroReason, Is.EqualTo("Let's finish the intro first: say next or press Next."));
                foreach (var tool in new[] { "cafe_deal_round", "cafe_check_order", "cafe_clear_table" })
                {
                    Assert.That(s.TryLessonTool(tool, null, out var r), Is.True, tool);
                    Assert.That(r.Ok, Is.False); Assert.That(r.Reason, Is.EqualTo(CafeStation.IntroReason), tool);
                }
                Assert.That(s.DropAt("item-1", CafeLayout.ContainerCenter(CafeTargetKind.Plates, 1, 2)), Is.False, "pastries cannot be placed");
                Assert.That(s.CurrentIntro.Id, Is.EqualTo("groups"), "refusals never move the intro");
                AssertScene(cafe, CafeLayout.IntroScene("groups"), "groups after refusals");
                AssertModelUntouched(s);
            }
            finally { Object.DestroyImmediate(cafe.root); }
        }

        [Test] public void ClosingDuringTheIntroRestoresTheTableAndTheIntroRunsAgain()
        {
            var cafe = MakeCafe(); var s = cafe.station;
            try
            {
                s.Open(); s.Advance(); s.Advance(); s.Advance();   // "boxes"
                Assert.That(s.CurrentIntro.Id, Is.EqualTo("boxes"));
                s.Close();
                Assert.That(s.CurrentIntro, Is.Null); Assert.That(s.IsOpen, Is.False);
                Assert.That(s.boxes.Any(b => b.gameObject.activeSelf) || s.boxLabels.Any(l => l.gameObject.activeSelf), Is.False, "intro boxes put away");
                for (int p = 0; p < 2; p++) AssertNear(s.plates[p].localPosition, CafeLayout.ContainerCenter(CafeTargetKind.Plates, p, 2), "plate " + (p + 1) + " back for chapter 1");
                for (int i = 0; i < 6; i++) AssertNear(cafe.Local(i), CafeLayout.TrayPosition(i), "croissant " + (i + 1) + " back on the tray");
                AssertModelUntouched(s);

                s.Open();
                Assert.That(s.CurrentIntro, Is.Null, "reopening shows the briefing");
                var first = s.Advance();
                Assert.That(s.CurrentIntro, Is.Not.Null, "an unfinished intro counts as not seen");
                Assert.That(s.CurrentIntro.Id, Is.EqualTo("share")); Assert.That(first.Reason, Is.EqualTo(ConceptIntros.Cafe[0].Say));
            }
            finally { Object.DestroyImmediate(cafe.root); }
        }

        [Test] public void JumpingToAChapterLeavesTheIntro()
        {
            var cafe = MakeCafe(); var s = cafe.station;
            try
            {
                s.Open(); s.Advance();
                s.JumpToChapter(2);
                Assert.That(s.CurrentIntro, Is.Null); Assert.That(s.ChapterActive, Is.True); Assert.That(s.Chapter.Number, Is.EqualTo(3));
                Assert.That(s.plates.Any(p => p.gameObject.activeSelf), Is.False);
            }
            finally { Object.DestroyImmediate(cafe.root); }
        }

        // ---- scene card (run AgentScripts/BuildCafeWorkbench.cs first) ----
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene loaded;
        [SetUp] public void OpenScene() { loaded = default; }
        [TearDown] public void CloseScene() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        [Test] public void CardShowsTheIntroWordsAndNextThenStart()
        {
            if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            var s = SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<CafeStation>(true)).FirstOrDefault();
            Assert.That(s, Is.Not.Null, "run AgentScripts/BuildCafeWorkbench.cs");
            var startLabel = s.startButton.GetComponentInChildren<TMP_Text>(true);
            try
            {
                s.Open();
                Assert.That(startLabel.text, Is.EqualTo("Start"), "briefing");
                var steps = ConceptIntros.Cafe;
                for (int i = 0; i < steps.Count; i++)
                {
                    s.Advance();
                    bool last = i == steps.Count - 1;
                    Assert.That(s.heading.text, Is.EqualTo(steps[i].Heading));
                    Assert.That(s.body.text, Is.EqualTo(steps[i].Say));
                    Assert.That(s.expressionLine.text, Is.EqualTo(steps[i].Expression));
                    Assert.That(s.sayHints.text, Is.EqualTo(CafeStation.IntroHintsFor(last)));
                    Assert.That(s.startButton.gameObject.activeSelf, Is.True);
                    Assert.That(startLabel.text, Is.EqualTo(last ? "Start" : "Next"), steps[i].Id);
                    foreach (var b in new[] { s.dealButton, s.checkButton, s.clearButton, s.nextButton }) Assert.That(b.gameObject.activeSelf, Is.False, b.name);
                    Assert.That(s.backButton.gameObject.activeSelf, Is.True);
                    Assert.That(s.heading.GetPreferredValues(steps[i].Heading).x, Is.LessThanOrEqualTo(s.heading.rectTransform.rect.width), "heading fits");
                    Assert.That(s.body.GetPreferredValues("<size=80%>" + steps[i].Say + "</size>", s.body.rectTransform.rect.width, 10000).y, Is.LessThanOrEqualTo(s.body.rectTransform.rect.height), "body fits");
                }
                s.Advance();
                Assert.That(s.ChapterActive, Is.True);
                Assert.That(startLabel.text, Is.EqualTo("Start"), "label restored for the next briefing");
            }
            finally { s.Close(); }
        }
    }
}
