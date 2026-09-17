using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Airlift.Guide;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Welcome;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Oculus.Interaction;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Airlift.Tests
{
    /// CC-PL-01 golden characterization: the exact messages Cargo Crew hands the guide (tool results, lesson_state and
    /// other context, response requests with their spoken instructions) for a fixed voice script, plus a small state
    /// snapshot after every step. The multi-lesson rewire (CC-PL-03) must reproduce Golden/cargo-voice.golden.json
    /// byte for byte. Record with NERDY_RECORD_GOLDEN=1 (or delete the file); a recording run is Inconclusive.
    ///
    /// Comparison (integrator ruling, CC-PL-03/04): strict, byte for byte. Cargo Crew sends no tools_now, so neither
    /// `lesson` nor `tools_now` is added to its messages; café and garden carry them.
    ///
    /// Drives NerdyDirector's private tool handler by reflection like AgentScripts/DriveVoiceTools.cs, with the guide in
    /// Live mode and no socket; GuideSession.OutgoingTap captures what would have been sent. Edit mode runs no Start,
    /// Update or coroutines, so the script mirrors them explicitly: Update is ticked after every step, the demo is
    /// finished and the practice crate grabbed/released through OnboardingDirector's own private methods, and the
    /// RuntimeOnly whenReadyContinue listener (CargoLessonDirector.Begin) is invoked when it did not fire.
    public class CargoVoiceCharacterizationTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        static string GoldenPath => Path.Combine(Application.dataPath, "Airlift/Tests/EditMode/Golden/cargo-voice.golden.json");
        static string ActualPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/cargo-voice.actual.json"));

        Scene loaded; bool preloaded;
        [SetUp] public void Open()
        {
            preloaded = SceneManager.GetSceneByPath(ScenePath).isLoaded;
            if (!preloaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        [Test] public void CargoVoiceScriptMatchesTheGoldenRecording()
        {
            // The drive mutates the scene; never do that to a copy someone has open in the editor.
            if (preloaded) Assert.Inconclusive("CargoCrew is already open in the editor; the golden drive only runs on an additively opened copy. Close the scene (or run through the test runner, which opens its own) and rerun.");
            string actual = new Drive(SceneManager.GetSceneByPath(ScenePath)).Run();
            bool record = Environment.GetEnvironmentVariable("NERDY_RECORD_GOLDEN") == "1" || !File.Exists(GoldenPath);
            if (record)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(GoldenPath));
                File.WriteAllText(GoldenPath, actual, new System.Text.UTF8Encoding(false));
                Assert.Inconclusive("Recorded " + GoldenPath + " (" + actual.Length + " chars). Review it, then rerun without NERDY_RECORD_GOLDEN.");
            }
            string golden = File.ReadAllText(GoldenPath);
            if (golden == actual) { if (File.Exists(ActualPath)) File.Delete(ActualPath); return; }
            File.WriteAllText(ActualPath, actual, new System.Text.UTF8Encoding(false));
            Assert.Fail("Cargo voice output differs from the golden recording. " + FirstDifference(golden, actual) + " Full actual output: " + ActualPath);
        }

        static string FirstDifference(string expected, string actual)
        {
            var e = expected.Split('\n'); var a = actual.Split('\n');
            for (int i = 0; i < System.Math.Max(e.Length, a.Length); i++)
            {
                string el = i < e.Length ? e[i] : "<end>", al = i < a.Length ? a[i] : "<end>";
                if (el != al) return "First difference at line " + (i + 1) + ":\n  golden: " + el + "\n  actual: " + al;
            }
            return "Lines match; only line endings differ.";
        }

        sealed class Drive
        {
            readonly Scene scene;
            NerdyDirector n; OnboardingDirector d; CargoLessonDirector lesson;
            readonly List<string> outgoing = new List<string>();
            readonly JArray steps = new JArray();
            int callNumber;

            public Drive(Scene scene) { this.scene = scene; }

            T Find<T>() where T : Component => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).FirstOrDefault();
            Transform Named(string name) => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == name);

            public string Run()
            {
                n = Find<NerdyDirector>(); Assert.That(n, Is.Not.Null, "NerdyDirector");
                d = n.onboarding; Assert.That(d, Is.Not.Null, "onboarding");
                lesson = Find<CargoLessonDirector>(); Assert.That(lesson, Is.Not.Null, "CargoLessonDirector");
                Assert.That(n.guide, Is.Not.Null, "guide");
                // Mirror the play-mode lifecycle the script depends on (Start/Awake do not run in edit mode).
                if (n.cargoLesson == null) n.cargoLesson = lesson;
                typeof(CargoLessonDirector).GetMethod("Awake", Private)?.Invoke(lesson, null);
                typeof(GuideSession).GetField("<Mode>k__BackingField", Private).SetValue(n.guide, GuideMode.Live);
                n.guide.OutgoingTap = outgoing.Add;
                try { Script(); }
                finally { n.guide.OutgoingTap = null; }
                return steps.ToString(Formatting.Indented).Replace("\r\n", "\n") + "\n";
            }

            // ---- the fixed script ----
            void Script()
            {
                Do("consent: no voice", () => n.ConsentNoVoice());
                Do("cards: GoToCatalog", () => n.GoToCatalog());
                Tool("split_cargo"); Tool("advance_step"); Tool("replay_demo"); Tool("back_to_lessons");
                Tool("request_help");
                Tool("open_lesson", "{\"cardId\":\"made_up\"}");
                Tool("open_lesson", "{\"cardId\":\"cargo_crew_fractions\"}");
                Tool("open_lesson", "{\"cardId\":\"cargo_crew_fractions\"}");
                Tool("request_help"); Tool("check_load"); Tool("replay_demo"); Tool("next_chapter");
                Tool("advance_step");                       // briefing -> orientation
                Tool("replay_demo");
                Tool("advance_step");                       // orientation -> demo
                Tool("advance_step"); Tool("replay_demo"); Tool("split_cargo");
                Do("demo finishes", FinishDemo);
                Tool("advance_step"); Tool("split_cargo"); Tool("request_help");
                Do("practice crate grabbed", () => PracticePointer(PointerEventType.Select));
                Tool("back_to_lessons"); Tool("replay_demo"); Tool("advance_step");
                Do("practice crate released on the pad", ReleaseOnPad);
                Tool("request_help"); Tool("restart_chapter");
                Tool("replay_demo");                        // ready -> demo again
                Do("demo finishes", FinishDemo);
                Do("practice crate grabbed", () => PracticePointer(PointerEventType.Select));
                Do("practice crate released on the pad", ReleaseOnPad);
                Tool("advance_step");                       // ready -> Start fractions
                Do("whenReadyContinue listener (Begin) if it did not fire", () => { if (!lesson.IsActive) lesson.Begin(); });

                // Chapter 1: big truck.
                Tool("split_cargo"); Tool("next_chapter"); Tool("check_load"); Tool("reset_cargo"); Tool("request_help");
                Do("drop crates into the beds by hand", DockAll);
                Tool("check_load"); Tool("reset_cargo"); Tool("check_load"); Tool("replay_demo");
                Tool("advance_step");                       // chapter 2

                // Chapter 2: two pickups.
                Tool("check_load"); Tool("split_cargo"); Tool("split_cargo"); Tool("restart_chapter");
                Do("a crate is held", () => lesson.whole.held = true);
                Tool("split_cargo"); Tool("back_to_lessons"); Tool("next_chapter"); Tool("advance_step");
                Do("the crate is released", () => lesson.whole.held = false);
                Tool("split_cargo");
                Do("drop crates into the beds by hand", DockAll);
                Do("Check load button", () => lesson.Submit());
                Do("Next chapter button", () => lesson.NextChapter());   // chapter 3, narrated by Update

                // Chapter 3: four vans. Pause, mute and headset-off on the way.
                Do("Pause", () => n.TogglePause());
                Tool("split_cargo"); Tool("request_help");
                Do("Play", () => n.TogglePause());
                Do("Mute", () => n.ToggleMute());
                Tool("split_cargo");
                Do("Unmute", () => n.ToggleMute());
                Do("headset off", () => n.HandleAppPause(true));
                Do("headset on", () => n.HandleAppPause(false));
                Tool("check_load");
                Do("Play", () => n.TogglePause());
                Do("drop crates into the beds by hand", DockAll);
                Tool("check_load"); Tool("next_chapter");

                // Chapter 4: same share with smaller boxes.
                Tool("split_cargo");
                Do("drop crates into the beds by hand", DockAll);
                Tool("check_load"); Tool("fly_the_plane"); Tool("advance_step");

                // Chapter 5: top it up (last).
                Tool("request_help");
                Do("drop crates into the beds by hand", DockAll);
                Tool("check_load"); Tool("next_chapter"); Tool("advance_step"); Tool("restart_chapter");
                Tool("back_to_lessons");
                Tool("split_cargo"); Tool("back_to_lessons");

                // Re-enter from the cards, then leave from the briefing with the Back button.
                Tool("open_lesson", "{\"cardId\":\"cargo_crew_fractions\"}");
                Tool("advance_step");
                Do("Back button", () => { d.Back(); lesson.Exit(); n.OnLessonBack(); });
                Tool("open_lesson", "{\"cardId\":\"cargo_crew_fractions\"}");
                Tool("back_to_lessons");
            }

            // ---- steps ----
            void Tool(string name, string args = "{}")
            {
                string callId = "call_" + (++callNumber).ToString("00");
                Do("tool " + name + " " + args, () => typeof(NerdyDirector).GetMethod("OnToolCall", Private).Invoke(n, new object[] { name, callId, args }));
            }

            void Do(string label, Action action)
            {
                outgoing.Clear();
                action();
                var afterAction = outgoing.ToList();
                outgoing.Clear();
                typeof(NerdyDirector).GetMethod("Update", Private)?.Invoke(n, null);   // one frame
                var step = new JObject { ["step"] = steps.Count + 1, ["do"] = label, ["out"] = new JArray(afterAction.ToArray()) };
                if (outgoing.Count > 0) step["out_next_frame"] = new JArray(outgoing.ToArray());
                step["state"] = Snapshot();
                steps.Add(step);
                outgoing.Clear();
            }

            JObject Snapshot()
            {
                var groups = new JObject();
                foreach (var name in new[] { "Onboarding buttons", "Fraction controls" })
                {
                    var t = Named(name); var g = t != null ? t.GetComponent<CanvasGroup>() : null;
                    groups[name] = g != null ? (JToken)(g.alpha > 0.5f) : JValue.CreateNull();
                }
                var visuals = new JObject();
                foreach (var name in new[] { "Workbench", "Cargo terminal", "Table handle", "Lesson interface" })
                {
                    var t = Named(name); visuals[name] = t != null ? (JToken)t.gameObject.activeSelf : JValue.CreateNull();
                }
                return new JObject
                {
                    ["phase"] = n.Flow.Phase.ToString(), ["stage"] = d.Stage.ToString(), ["paused"] = n.Paused, ["muted"] = n.Muted,
                    ["mic"] = n.guide.MicEnabled,
                    ["lesson_active"] = lesson.IsActive, ["chapter"] = lesson.IsActive && lesson.Chapter != null ? lesson.Chapter.Number : 0,
                    ["chapter_complete"] = lesson.IsActive && lesson.ChapterComplete,
                    ["caption"] = n.captionText != null ? n.captionText.text : null,
                    ["heading"] = d.heading != null ? d.heading.text : null,
                    ["body"] = d.body != null ? d.body.text : null,
                    ["fallback_buttons_shown"] = groups, ["active"] = visuals,
                    ["hud_parent"] = n.hudRoot != null && n.hudRoot.transform.parent != null ? n.hudRoot.transform.parent.name : null
                };
            }

            // ---- mirrors of play-mode events ----
            object Flow => typeof(OnboardingDirector).GetField("flow", Private).GetValue(d);

            /// The end of OnboardingDirector.ShowDemonstration (the coroutine does not tick in edit mode).
            void FinishDemo()
            {
                typeof(OnboardingDirector).GetField("demonstration", Private).SetValue(d, null);
                Flow.GetType().GetMethod("FinishDemonstration").Invoke(Flow, null);
                typeof(OnboardingDirector).GetMethod("ResetStrap", Private).Invoke(d, null);
                typeof(OnboardingDirector).GetMethod("Refresh", Private).Invoke(d, null);
            }

            void PracticePointer(PointerEventType type)
                => typeof(OnboardingDirector).GetMethod("OnPointer", Private).Invoke(d, new object[] { new PointerEvent(7, type, Pose.identity) });

            /// Unselect over the pad, then run the release check the SDK frame would have run.
            void ReleaseOnPad()
            {
                d.strap.localPosition = d.content.targetPosition;
                PracticePointer(PointerEventType.Unselect);
                int generation = (int)typeof(OnboardingDirector).GetField("generation", Private).GetValue(d);
                var check = (IEnumerator)typeof(OnboardingDirector).GetMethod("CheckRelease", Private).Invoke(d, new object[] { generation });
                while (check.MoveNext()) { }
            }

            /// Every loose crate into the first bed with room, through the real drop path (CargoLessonDirector.DropAt).
            void DockAll()
            {
                var model = (CargoLessonModel)typeof(CargoLessonDirector).GetField("model", Private).GetValue(lesson);
                var rc = lesson.ruler.localPosition;
                foreach (var id in model.PieceIds.ToList())
                {
                    if (model.IsLocked(id) || model.IsDocked(id)) continue;
                    int cells = model.Piece(id).Cells; int bed = -1;
                    for (int b = 0; b < model.BedCount; b++) if (model.BedCapacity(b) - model.BedFill(b) >= cells) { bed = b; break; }
                    if (bed < 0) continue;
                    int start = model.BedStartCell(bed) + model.BedFill(bed);
                    lesson.DropAt(id, new Vector3(RulerLayout.CellsCenterX(rc, start, cells), rc.y, rc.z));
                }
            }
        }
    }
}
