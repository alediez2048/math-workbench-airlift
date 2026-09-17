using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Owner 2026-09-17: "What is a fraction?" before Dock 7 chapter 1, once per app run, after "Start fractions". Four
    /// steps pose the real lesson crates over the ruler (whole; halves; halves with 1/2 + 1/2 = 1; quarters) without
    /// touching the lesson model. Pure intro state, the station on in-memory objects, and the scene wiring (scene tests
    /// pass after AgentScripts/BuildDockWorkbench.cs runs).
    public class CargoIntroTests
    {
        // ---- pure intro state ----
        [Test] public void IntroStepsRunInOrderOnce()
        {
            var intro = new CargoConceptIntro();
            Assert.That(intro.ShouldRun, Is.True); Assert.That(intro.Running, Is.False); Assert.That(intro.Current, Is.Null);
            Assert.That(intro.Start().Id, Is.EqualTo("whole")); Assert.That(intro.PrimaryLabel, Is.EqualTo("Next"));
            Assert.That(intro.Next().Id, Is.EqualTo("halves"));
            Assert.That(intro.Next().Id, Is.EqualTo("sum"));
            Assert.That(intro.Next().Id, Is.EqualTo("quarters")); Assert.That(intro.IsLast, Is.True); Assert.That(intro.PrimaryLabel, Is.EqualTo("Start"));
            Assert.That(intro.Next(), Is.Null, "Start after the last step ends the intro");
            Assert.That(intro.Running, Is.False); Assert.That(intro.Seen, Is.True); Assert.That(intro.ShouldRun, Is.False, "once per app run");
        }

        [Test] public void CancelledIntroCountsAsNotSeen()
        {
            var intro = new CargoConceptIntro();
            intro.Start(); intro.Next();
            intro.Cancel();
            Assert.That(intro.Running, Is.False); Assert.That(intro.Seen, Is.False); Assert.That(intro.ShouldRun, Is.True);
            Assert.That(intro.Start().Id, Is.EqualTo("whole"), "starts again from the first step");
        }

        [Test] public void PosesUseTheLessonCratesOverTheRuler()
        {
            var ids = new[] { "whole", "half-1", "half-2", "quarter-1", "quarter-2", "quarter-3", "quarter-4" };
            var expected = new Dictionary<string, (string id, int start, int cells, int den)[]>
            {
                ["whole"] = new[] { ("whole", 0, 8, 1) },
                ["halves"] = new[] { ("half-1", 0, 4, 2), ("half-2", 4, 4, 2) },
                ["sum"] = new[] { ("half-1", 0, 4, 2), ("half-2", 4, 4, 2) },
                ["quarters"] = new[] { ("quarter-1", 0, 2, 4), ("quarter-2", 2, 2, 4), ("quarter-3", 4, 2, 4), ("quarter-4", 6, 2, 4) },
            };
            foreach (var step in ConceptIntros.Cargo)
            {
                var poses = CargoConceptIntro.Poses(step.Visual);
                Assert.That(poses.Select(p => (p.Id, p.StartCell, p.Cells, p.Denominator)).ToArray(), Is.EqualTo(expected[step.Visual]), step.Id);
                Assert.That(poses.Sum(p => p.Cells), Is.EqualTo(PlacementState.CellsPerWhole), step.Id + " fills exactly one whole");
                foreach (var p in poses)
                {
                    Assert.That(ids, Does.Contain(p.Id));
                    Assert.That(p.Cells * p.Denominator, Is.EqualTo(PlacementState.CellsPerWhole), p.Id + " label matches its length");
                }
            }
            Assert.That(CargoConceptIntro.Poses("unknown"), Is.Empty);
        }

        [Test] public void GuideStepNamesExactlyWhatIsShown()
        {
            var steps = ConceptIntros.Cargo;
            for (int i = 0; i < steps.Count; i++)
            {
                var g = CargoConceptIntro.StepFor(steps[i], i == steps.Count - 1);
                Assert.That(g.Id, Is.EqualTo("intro")); Assert.That(g.CanGrabNow, Is.False, steps[i].Id);
                Assert.That(g.OnTableNow.Length, Is.LessThanOrEqualTo(GuideSteps.MaxOnTableLength), steps[i].Id);
                Assert.That(g.OnTableNow, Does.Contain("0 to 1 ruler"), steps[i].Id);
            }
            Assert.That(CargoConceptIntro.StepFor(steps[0], false).OnTableNow, Does.Contain("one whole crate").And.Contain("labelled 1").And.Not.Contain("1/2"));
            Assert.That(CargoConceptIntro.StepFor(steps[1], false).OnTableNow, Does.Contain("two half crates").And.Contain("1/2").And.Not.Contain("1/2 + 1/2"));
            Assert.That(CargoConceptIntro.StepFor(steps[2], false).OnTableNow, Does.Contain("two half crates").And.Contain("1/2 + 1/2 = 1"));
            Assert.That(CargoConceptIntro.StepFor(steps[3], true).OnTableNow, Does.Contain("four quarter crates").And.Contain("1/4").And.Contain("Start"));
            Assert.That(CargoConceptIntro.FinishFirstReason, Is.EqualTo("Let's finish the intro first: say next or press Next."));
        }

        // ---- the station on in-memory objects ----
        sealed class Dock
        {
            public GameObject root; public OnboardingDirector d; public CargoLessonDirector l; public CargoStation s;
            public void Destroy() { if (root != null) Object.DestroyImmediate(root); if (d != null && d.content != null) Object.DestroyImmediate(d.content); }
        }

        static TMP_Text Text(Transform parent, string name) { var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); return go.AddComponent<TextMeshProUGUI>(); }
        static Button MakeButton(Transform parent, string name) { var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false); return go.GetComponent<Button>(); }
        static Transform Child(Transform parent, string name, Vector3 p) { var t = new GameObject(name).transform; t.SetParent(parent, false); t.localPosition = p; return t; }

        static Dock MakeDock()
        {
            var k = new Dock { root = new GameObject("test dock") };
            var root = k.root.transform;
            k.d = k.root.AddComponent<OnboardingDirector>();
            k.l = k.root.AddComponent<CargoLessonDirector>();
            k.s = k.root.AddComponent<CargoStation>();
            k.s.onboarding = k.d; k.s.lesson = k.l; k.s.cardId = CargoStation.CardId;

            var d = k.d;
            d.content = ScriptableObject.CreateInstance<OnboardingContent>();
            d.content.trayPosition = new Vector3(-0.13f, 0.0725f, -0.17f); d.content.targetPosition = new Vector3(0, 0.09f, 0.02f);
            d.strap = Child(root, "practice crate", d.content.trayPosition);
            d.demonstrationStrap = Child(root, "demo crate", d.content.trayPosition);
            d.station = Child(root, "pad", Vector3.zero).gameObject;
            d.catalog = Child(root, "catalog", Vector3.zero).gameObject;
            var ui = new GameObject("ui", typeof(RectTransform), typeof(Canvas)).transform; ui.SetParent(root, false);
            d.briefing = new GameObject("briefing", typeof(RectTransform)); d.briefing.transform.SetParent(ui, false);
            d.heading = Text(d.briefing.transform, "Heading"); d.body = Text(d.briefing.transform, "Instruction");
            d.primary = MakeButton(d.briefing.transform, "Primary"); d.primaryLabel = Text(d.primary.transform, "Label");
            d.help = MakeButton(d.briefing.transform, "Help");
            d.whenReadyContinue.AddListener(k.s.ContinueFromReady);   // BuildDockWorkbench wires this persistently

            var l = k.l;
            l.stationRoot = root; l.heading = d.heading; l.body = d.body; l.restHeight = 0.107f;
            l.expressionLine = Text(d.briefing.transform, "Expression line"); l.sayHints = Text(d.briefing.transform, "Say hints");
            l.chapterObjects = Child(root, "Fraction chapter", Vector3.zero).gameObject;
            l.ruler = Child(l.chapterObjects.transform, "Ruler", new Vector3(0, 0.031f, 0.02f));
            l.hideWhileActive = new[] { d.primary.gameObject, d.help.gameObject, d.strap.gameObject, d.demonstrationStrap.gameObject, d.station };
            CargoLessonDirector.PieceView View(string id, Vector3 tray)
            {
                var piece = Child(l.chapterObjects.transform, id, tray);
                return new CargoLessonDirector.PieceView { id = id, piece = piece, trayPosition = tray };
            }
            l.whole = View("whole", new Vector3(-0.13f, 0.0725f, -0.30f));
            l.halfA = View("half-1", new Vector3(-0.21f, 0.0725f, -0.30f));
            l.halfB = View("half-2", new Vector3(-0.05f, 0.0725f, -0.30f));
            l.quarters = Enumerable.Range(1, 4).Select(i => View("quarter-" + i, new Vector3(-0.265f + (i - 1) * 0.09f, 0.0725f, -0.30f))).ToArray();
            foreach (var q in l.quarters) q.piece.gameObject.SetActive(false);
            l.chapterObjects.SetActive(false);
            return k;
        }

        const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

        /// Open Cargo Crew and finish the practice (the demo coroutine and SDK pointer events do not run in edit mode).
        static void OpenToReady(Dock k)
        {
            k.s.Open();
            var flow = (OnboardingFlow)typeof(OnboardingDirector).GetField("flow", Private).GetValue(k.d);
            flow.Continue(); flow.Continue(); flow.FinishDemonstration(); flow.RecordGrab(); flow.RecordRelease(true);
            typeof(OnboardingDirector).GetMethod("Refresh", Private).Invoke(k.d, null);
            Assert.That(k.d.Stage, Is.EqualTo(OnboardingStage.Ready));
        }

        static IEnumerable<CargoLessonDirector.PieceView> Views(CargoLessonDirector l) => new[] { l.whole, l.halfA, l.halfB }.Concat(l.quarters);

        static void AssertPoses(Dock k, string visual)
        {
            var poses = CargoConceptIntro.Poses(visual);
            foreach (var view in Views(k.l))
            {
                var pose = poses.FirstOrDefault(p => p.Id == view.id);
                bool shown = pose.Id != null;
                Assert.That(view.piece.gameObject.activeSelf, Is.EqualTo(shown), visual + ": " + view.id + (shown ? " shown" : " hidden"));
                if (!shown) continue;
                var expected = RulerLayout.SnapPosition(k.l.ruler.localPosition, pose.StartCell, pose.Cells, k.d.content.targetPosition.y);
                Assert.That(Vector3.Distance(view.piece.localPosition, expected), Is.LessThan(1e-5f), visual + ": " + view.id + " over its cells, resting on the measuring pad");
            }
        }

        [Test] public void StationRunsTheIntroBeforeChapterOneOnce()
        {
            var k = MakeDock();
            try
            {
                OpenToReady(k);
                Assert.That(k.s.CurrentIntro, Is.Null, "no intro before Start fractions");
                var steps = ConceptIntros.Cargo;

                var first = k.s.Advance();   // "Start fractions" by voice
                Assert.That(first.Ok, Is.True); Assert.That(first.Reason, Is.EqualTo(steps[0].Say));
                Assert.That(k.s.CurrentIntro, Is.SameAs(steps[0]));
                Assert.That(k.s.ChapterActive, Is.False); Assert.That(k.l.IsActive, Is.False);
                Assert.That(k.d.heading.text, Is.EqualTo(steps[0].Heading)); Assert.That(k.d.body.text, Is.EqualTo(steps[0].Say));
                Assert.That(k.s.Instruction, Is.EqualTo(steps[0].Say), "the guide reads the intro line");
                Assert.That(k.d.primaryLabel.text, Is.EqualTo("Next")); Assert.That(k.d.primary.gameObject.activeSelf, Is.True);
                Assert.That(k.d.help.gameObject.activeSelf, Is.False, "no demo replay during the intro");
                Assert.That(k.d.strap.gameObject.activeSelf, Is.False, "the practice crate steps aside");
                Assert.That(k.l.chapterObjects.activeSelf, Is.True, "ruler and crates visible");
                Assert.That(k.l.expressionLine.text, Is.Empty);
                AssertPoses(k, "whole");
                var guide = k.s.CurrentStep();
                Assert.That(guide.Id, Is.EqualTo("intro")); Assert.That(guide.CanGrabNow, Is.False); Assert.That(guide.OnTableNow, Does.Contain("one whole crate"));

                foreach (var refused in new[] { k.s.Check(), k.s.ResetTable(), k.s.NextChapter(), k.s.RestartChapter() })
                { Assert.That(refused.Ok, Is.False); Assert.That(refused.Reason, Is.EqualTo(CargoConceptIntro.FinishFirstReason)); }
                foreach (var tool in new[] { "split_cargo", "check_load", "reset_cargo", "replay_demo" })
                {
                    Assert.That(k.s.TryLessonTool(tool, new Newtonsoft.Json.Linq.JObject(), out var r), Is.True, tool);
                    Assert.That(r.Ok, Is.False, tool); Assert.That(r.Reason, Is.EqualTo(CargoConceptIntro.FinishFirstReason), tool);
                }
                Assert.That(k.s.CurrentIntro, Is.SameAs(steps[0]), "refusals change nothing");

                k.d.Continue();   // the Next button: OnboardingDirector.whenReadyContinue -> CargoStation.ContinueFromReady
                Assert.That(k.s.CurrentIntro, Is.SameAs(steps[1])); AssertPoses(k, "halves");
                Assert.That(k.d.body.text, Is.EqualTo(steps[1].Say)); Assert.That(k.s.CurrentStep().OnTableNow, Does.Contain("two half crates"));

                var sum = k.s.Advance();
                Assert.That(sum.Ok, Is.True); Assert.That(sum.Reason, Is.EqualTo(steps[2].Say));
                AssertPoses(k, "sum"); Assert.That(k.l.expressionLine.text, Is.EqualTo("1/2 + 1/2 = 1"));

                var quarters = k.s.Advance();
                Assert.That(quarters.Reason, Is.EqualTo(steps[3].Say)); AssertPoses(k, "quarters");
                Assert.That(k.l.expressionLine.text, Is.Empty); Assert.That(k.d.primaryLabel.text, Is.EqualTo("Start"));

                var start = k.s.Advance();   // Start: chapter 1 exactly as before
                Assert.That(start.Ok, Is.True); Assert.That(start.Reason, Is.Empty);
                Assert.That(k.s.CurrentIntro, Is.Null); Assert.That(k.l.IsActive, Is.True); Assert.That(k.l.Chapter.Number, Is.EqualTo(1));
                Assert.That(k.l.whole.piece.gameObject.activeSelf, Is.True); Assert.That(k.l.whole.piece.localPosition, Is.EqualTo(k.l.whole.trayPosition));
                Assert.That(k.l.quarters.All(q => !q.piece.gameObject.activeSelf), Is.True);
                Assert.That(k.d.primary.gameObject.activeSelf, Is.False, "chapter buttons take over");

                // Back to the lessons and in again: straight to the chapter this app run.
                k.s.Close();
                OpenToReady(k);
                var again = k.s.Advance();
                Assert.That(again.Ok, Is.True); Assert.That(k.s.CurrentIntro, Is.Null); Assert.That(k.l.IsActive, Is.True, "the intro runs once per app run");
            }
            finally { k.Destroy(); }
        }

        [Test] public void ClosingDuringTheIntroRestoresTheTableAndTheIntroIsNotSeen()
        {
            var k = MakeDock();
            try
            {
                OpenToReady(k);
                k.s.Advance(); k.s.Advance();   // halves
                Assert.That(k.s.CurrentIntro.Id, Is.EqualTo("halves"));
                k.s.Close();
                Assert.That(k.d.Stage, Is.EqualTo(OnboardingStage.Catalog));
                Assert.That(k.s.CurrentIntro, Is.Null);
                Assert.That(k.l.chapterObjects.activeSelf, Is.False);
                foreach (var view in Views(k.l)) Assert.That(view.piece.localPosition, Is.EqualTo(view.trayPosition), view.id + " back in the tray");
                Assert.That(k.l.quarters.All(q => !q.piece.gameObject.activeSelf), Is.True);

                OpenToReady(k);
                Assert.That(k.s.Advance().Reason, Is.EqualTo(ConceptIntros.Cargo[0].Say), "the intro starts over");

                // The on-card Back button calls OnboardingDirector.Back and CargoLessonDirector.Exit directly.
                k.s.Advance(); k.s.Advance();   // sum
                Assert.That(k.l.expressionLine.text, Is.EqualTo("1/2 + 1/2 = 1"));
                k.d.Back(); k.l.Exit();
                Assert.That(k.s.CurrentIntro, Is.Null, "leaving by the Back button cancels the intro");
                foreach (var view in Views(k.l)) Assert.That(view.piece.localPosition, Is.EqualTo(view.trayPosition), view.id);
                OpenToReady(k);
                Assert.That(k.s.Advance().Reason, Is.EqualTo(ConceptIntros.Cargo[0].Say));
            }
            finally { k.Destroy(); }
        }

        [Test] public void AChapterStartedElsewhereEndsTheIntro()
        {
            var k = MakeDock();
            try
            {
                OpenToReady(k);
                k.s.Advance();
                k.l.Begin();   // e.g. an editor drive; the chapter wins
                Assert.That(k.s.CurrentIntro, Is.Null);
                Assert.That(k.s.ChapterActive, Is.True);
                Assert.That(k.s.CurrentStep().Id, Does.StartWith("chapter1"));
            }
            finally { k.Destroy(); }
        }

        // ---- scene (after BuildDockWorkbench) ----
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene loaded;
        static T Find<T>() where T : Component => SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).FirstOrDefault();
        void OpenScene() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void CloseScene() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); loaded = default; }

        [Test] public void StartFractionsRunsTheIntroThroughTheStation()
        {
            OpenScene();
            var d = Find<OnboardingDirector>(); var station = d.GetComponent<CargoStation>(); var lesson = d.GetComponent<CargoLessonDirector>();
            Assert.That(station, Is.Not.Null);
            var e = d.whenReadyContinue; var calls = Enumerable.Range(0, e.GetPersistentEventCount()).Select(i => (e.GetPersistentTarget(i), e.GetPersistentMethodName(i))).ToArray();
            Assert.That(calls, Does.Contain(((Object)station, nameof(CargoStation.ContinueFromReady))), "Start fractions and Next go through the intro");
            Assert.That(calls.Any(c => c.Item1 == lesson && c.Item2 == nameof(CargoLessonDirector.Begin)), Is.False, "the station starts chapter 1 after the intro");
            Assert.That(calls.Length, Is.EqualTo(1));
        }

        [Test] public void IntroCopyFitsTheLessonCard()
        {
            OpenScene();
            var d = Find<OnboardingDirector>(); var lesson = d.GetComponent<CargoLessonDirector>();
            foreach (var step in ConceptIntros.Cargo)
            {
                Assert.That(d.heading.GetPreferredValues(step.Heading, d.heading.rectTransform.rect.width, 1000).y, Is.LessThanOrEqualTo(d.heading.rectTransform.rect.height), step.Heading);
                Assert.That(d.body.GetPreferredValues(step.Say, d.body.rectTransform.rect.width, 10000).y, Is.LessThanOrEqualTo(d.body.rectTransform.rect.height), step.Say);
                Assert.That(lesson.expressionLine.GetPreferredValues(step.Expression).x, Is.LessThanOrEqualTo(lesson.expressionLine.rectTransform.rect.width), step.Expression);
            }
            foreach (var label in new[] { "Next", "Start" })
                Assert.That(d.primaryLabel.GetPreferredValues(label).x, Is.LessThanOrEqualTo(d.primaryLabel.rectTransform.rect.width), label);
        }
    }
}
