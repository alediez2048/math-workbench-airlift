using System.Linq;
using Airlift.Lessons;
using Airlift.Lessons.Garden;
using Airlift.Presentation.Garden;
using Airlift.Welcome;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Airlift.Tests
{
    /// Owner 2026-09-17: before chapter 1 of Community Garden a short concept intro, "What is multiplying?" (row, rows,
    /// times), runs once per app run after the briefing. The garden shows planted rows of 4 on the chapter 1 bed
    /// without touching GardenModel; nothing can be grabbed; every other action asks to finish the intro first.
    public class GardenIntroTests
    {
        static readonly string[] IntroTools = { "advance_step", "request_help", "back_to_lessons" };

        static GardenStation MakeGarden() => GardenWorkbenchWiringTests.MakeGarden();

        static int ActiveStrips(GardenStation s) => s.strips.Count(v => v.piece.gameObject.activeSelf);

        /// Card texts and a Start button with a label, like the builder's card.
        static void AddCard(GardenStation s)
        {
            TMP_Text Text(string name) { var t = new GameObject(name).AddComponent<TextMeshPro>(); t.transform.SetParent(s.transform, false); t.rectTransform.sizeDelta = Vector2.zero; return t; }
            s.heading = Text("heading"); s.body = Text("body"); s.expressionLine = Text("expression"); s.sayHints = Text("hints");
            var start = new GameObject("Start").AddComponent<Button>(); start.transform.SetParent(s.transform, false);
            var label = new GameObject("Label").AddComponent<TextMeshPro>(); label.transform.SetParent(start.transform, false); label.text = "Start";
            s.startButton = start;
        }

        static string StartLabel(GardenStation s) => s.startButton.GetComponentInChildren<TMP_Text>(true).text;

        [Test] public void TheFirstStartRunsTheIntroBeforeChapterOne()
        {
            var s = MakeGarden();
            try
            {
                AddCard(s);
                s.Open();
                Assert.That(s.CurrentIntro, Is.Null, "no intro during the briefing");
                Assert.That(StartLabel(s), Is.EqualTo("Start"));
                var steps = ConceptIntros.Garden;
                Assert.That(steps.Select(x => x.Id), Is.EqualTo(new[] { "row", "rows", "times" }));
                int[] plantedRows = { 1, 3, 3 };
                for (int i = 0; i < steps.Count; i++)
                {
                    var result = s.Advance();
                    var step = steps[i];
                    Assert.That(result.Ok, Is.True, step.Id);
                    Assert.That(result.Reason, Is.EqualTo(step.Say), "Advance returns the new step's words");
                    Assert.That(s.CurrentIntro, Is.SameAs(step));
                    Assert.That(s.InIntro, Is.True); Assert.That(s.InBriefing, Is.False); Assert.That(s.ChapterActive, Is.False);
                    Assert.That(s.Chapter.Number, Is.EqualTo(0), "no chapter facts during the intro");
                    Assert.That(s.ToolsNow, Is.EquivalentTo(IntroTools), step.Id + " tools");

                    var guide = s.CurrentStep();
                    Assert.That(guide.Id, Is.EqualTo("intro")); Assert.That(guide.CanGrabNow, Is.False);
                    Assert.That(guide.OnTableNow.Length, Is.LessThanOrEqualTo(GuideSteps.MaxOnTableLength));

                    Assert.That(s.heading.text, Is.EqualTo(step.Heading)); Assert.That(s.body.text, Is.EqualTo(step.Say));
                    Assert.That(s.expressionLine.text, Is.EqualTo(step.Expression));
                    Assert.That(StartLabel(s), Is.EqualTo(i == steps.Count - 1 ? "Start" : "Next"));
                    Assert.That(s.sayHints.text, Does.Contain("next"));

                    // The picture: rows of 4 on the chapter 1 bed, back row first; the model is untouched.
                    Assert.That(s.pieces.activeSelf, Is.True);
                    Assert.That(ActiveStrips(s), Is.EqualTo(plantedRows[i]), step.Id + " planted rows");
                    Assert.That(s.bed.ActiveCellCount(), Is.EqualTo(12), "the 3 × 4 bed of chapter 1");
                    var L = s.BedLayout;
                    for (int r = 0; r < plantedRows[i]; r++)
                    {
                        var v = s.strips[r];
                        Assert.That(v.piece.localPosition, Is.EqualTo(L.StripPosition(r, 4)), step.Id + " row " + (r + 1));
                        Assert.That(v.piece.localRotation, Is.EqualTo(Quaternion.identity));
                        Assert.That(v.seedlings.Count(t => t.gameObject.activeSelf), Is.EqualTo(4), step.Id + " rows of 4");
                    }
                    Assert.That(s.fence.gameObject.activeSelf, Is.False);
                    Assert.That(s.bed.rowLabels.Take(3).All(t => t.enabled), Is.EqualTo(step.Id == "times"), step.Id + ": row numbers only on the times step");
                    Assert.That(s.Model.PlantCount, Is.EqualTo(0)); Assert.That(s.Model.ChapterIndex, Is.EqualTo(0));
                }
                Assert.That(GardenSteps.Intro(steps[0]).OnTableNow, Does.Contain("one row of 4").IgnoreCase.Or.Contain("one strip of 4").IgnoreCase);
                Assert.That(GardenSteps.Intro(steps[1]).OnTableNow, Does.Contain("3").And.Contain("12"));
                Assert.That(GardenSteps.Intro(steps[2]).OnTableNow, Does.Contain("row numbers").And.Contain("3 × 4 = 12"));

                var start = s.Advance();
                Assert.That(start.Ok, Is.True);
                Assert.That(start.Reason, Is.EqualTo(GardenStation.TitleFor(GardenChapter.All[0]) + ". " + GardenChapter.All[0].Story + " " + GardenChapter.All[0].Task), "Start after the intro starts chapter 1 as before");
                Assert.That(s.CurrentIntro, Is.Null); Assert.That(s.InIntro, Is.False); Assert.That(s.ChapterActive, Is.True);
                Assert.That(s.Chapter.Number, Is.EqualTo(1));
                Assert.That(ActiveStrips(s), Is.EqualTo(5), "chapter 1 tray strips");
                Assert.That(s.bed.rowLabels.Take(3).All(t => t.enabled), Is.True, "row numbers back on");
                Assert.That(s.heading.text, Is.EqualTo(GardenStation.TitleFor(GardenChapter.All[0])));
            }
            finally { Object.DestroyImmediate(s.gameObject); }
        }

        [Test] public void EveryOtherActionDuringTheIntroAsksToFinishIt()
        {
            var s = MakeGarden();
            try
            {
                s.Open(); s.Advance();
                Assert.That(s.InIntro, Is.True);
                foreach (var r in new[] { s.Check(), s.ResetTable(), s.NextChapter(), s.RestartChapter(), s.TurnBed(), s.SplitAt(3) })
                {
                    Assert.That(r.Ok, Is.False); Assert.That(r.Reason, Is.EqualTo(GardenStation.IntroReason));
                }
                Assert.That(GardenStation.IntroReason, Is.EqualTo("Let's finish the intro first: say next or press Next."));
                foreach (var tool in new[] { "garden_check_bed", "garden_turn_bed", "garden_clear_bed" })
                {
                    Assert.That(s.TryLessonTool(tool, new JObject(), out var result), Is.True);
                    Assert.That(result.Reason, Is.EqualTo(GardenStation.IntroReason), tool);
                }
                Assert.That(s.TryLessonTool("garden_split_bed", new JObject { ["columns"] = 2 }, out var split), Is.True);
                Assert.That(split.Reason, Is.EqualTo(GardenStation.IntroReason));
                Assert.That(s.DropAt("strip-1", s.BedLayout.StripPosition(1, 4)), Is.False, "nothing is planted during the intro");
                Assert.That(s.DropFenceAt(new Vector3(s.BedLayout.BoundaryX(2), 0.05f, s.BedLayout.Center.z)), Is.False);
                Assert.That(s.CurrentIntro.Id, Is.EqualTo("row"), "refusals do not move the intro");
                Assert.That(s.AnyHeld, Is.False);
                Assert.That(s.Model.PlantCount, Is.EqualTo(0));
            }
            finally { Object.DestroyImmediate(s.gameObject); }
        }

        [Test] public void TheIntroRunsOncePerRunAndCloseDuringItRestoresTheTable()
        {
            var s = MakeGarden();
            try
            {
                s.Open(); s.Advance(); s.Advance();
                Assert.That(s.CurrentIntro.Id, Is.EqualTo("rows"));
                s.Close();
                Assert.That(s.IsOpen, Is.False); Assert.That(s.CurrentIntro, Is.Null); Assert.That(s.IntroSeen, Is.False, "a closed intro counts as not seen");
                Assert.That(s.pieces.activeSelf, Is.False, "intro rows cleared");
                Assert.That(s.bed.rowLabels.All(t => t.enabled), Is.True, "row numbers restored");
                Assert.That(s.Model.PlantCount, Is.EqualTo(0)); Assert.That(s.Model.ChapterIndex, Is.EqualTo(0));

                s.Open();
                Assert.That(s.InBriefing, Is.True);
                var again = s.Advance();
                Assert.That(s.CurrentIntro.Id, Is.EqualTo("row"), "the intro starts over after a close"); Assert.That(again.Reason, Is.EqualTo(ConceptIntros.Garden[0].Say));
                s.Advance(); s.Advance(); s.Advance();
                Assert.That(s.Chapter.Number, Is.EqualTo(1)); Assert.That(s.IntroSeen, Is.True);

                s.Close(); s.Open();
                var direct = s.Advance();
                Assert.That(s.CurrentIntro, Is.Null, "the second visit skips the intro");
                Assert.That(s.ChapterActive, Is.True); Assert.That(direct.Reason, Does.StartWith("Chapter 1 · First rows."));
            }
            finally { Object.DestroyImmediate(s.gameObject); }
        }

        [Test] public void JumpingToAChapterLeavesTheIntro()
        {
            var s = MakeGarden();
            try
            {
                s.Open(); s.Advance();
                s.JumpToChapter(3);
                Assert.That(s.InIntro, Is.False); Assert.That(s.ChapterActive, Is.True); Assert.That(s.Chapter.Number, Is.EqualTo(4));
                s.JumpToIntro(2);
                Assert.That(s.CurrentIntro.Id, Is.EqualTo("times"), "previews can show an intro step"); Assert.That(s.ChapterActive, Is.False);
                Assert.That(ActiveStrips(s), Is.EqualTo(3));
                Assert.That(s.strips.Take(3).All(v => v.piece.localScale == Vector3.one), Is.True, "intro rows are full size");
            }
            finally { Object.DestroyImmediate(s.gameObject); }
        }

        [Test] public void IntroButtonLabelAndStepText()
        {
            Assert.That(GardenStation.IntroButtonLabel(0, 3), Is.EqualTo("Next"));
            Assert.That(GardenStation.IntroButtonLabel(1, 3), Is.EqualTo("Next"));
            Assert.That(GardenStation.IntroButtonLabel(2, 3), Is.EqualTo("Start"));
            Assert.That(GardenStation.IntroPlantedRows("row"), Is.EqualTo(1));
            Assert.That(GardenStation.IntroPlantedRows("rows"), Is.EqualTo(3));
            Assert.That(GardenStation.IntroPlantedRows("times"), Is.EqualTo(3));
            foreach (var step in ConceptIntros.Garden)
            {
                var g = GardenSteps.Intro(step);
                Assert.That(g.Id, Is.EqualTo(GardenSteps.IntroId)); Assert.That(g.Id, Is.EqualTo("intro"));
                Assert.That(g.CanGrabNow, Is.False);
                Assert.That(g.OnTableNow, Does.Contain("cannot be grabbed").Or.Contain("Nothing can be grabbed"), step.Id);
                Assert.That(g.OnTableNow, Does.Contain(step == ConceptIntros.Garden.Last() ? "press Start" : "press Next"), step.Id);
                Assert.That(g.OnTableNow.Length, Is.LessThanOrEqualTo(GuideSteps.MaxOnTableLength), step.Id);
                Assert.That(g.OnTableNow.All(c => c < 128 || "·—×÷".IndexOf(c) >= 0), Is.True, "ASCII: " + g.OnTableNow);
            }
            Assert.That(GardenStation.IntroSayHints.All(c => c < 128 || "·".IndexOf(c) >= 0), Is.True);
        }

        // ---- scene (after BuildGardenWorkbench) ----
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";

        [Test] public void IntroCopyFitsTheGardenCard()
        {
            Scene loaded = default;
            if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var s = SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<GardenStation>(true)).First();
                float bodyW = s.body.rectTransform.rect.width, bodyH = s.body.rectTransform.rect.height;
                foreach (var step in ConceptIntros.Garden)
                {
                    Assert.That(s.heading.GetPreferredValues(step.Heading).x, Is.LessThanOrEqualTo(s.heading.rectTransform.rect.width), step.Heading);
                    Assert.That(s.body.GetPreferredValues("<size=80%>" + step.Say + "</size>", bodyW, 10000).y, Is.LessThanOrEqualTo(bodyH), step.Say);
                    Assert.That(s.expressionLine.GetPreferredValues(step.Expression).x, Is.LessThanOrEqualTo(s.expressionLine.rectTransform.rect.width), step.Expression);
                }
                Assert.That(s.sayHints.GetPreferredValues(GardenStation.IntroSayHints).x, Is.LessThanOrEqualTo(s.sayHints.rectTransform.rect.width));
                var label = s.startButton.GetComponentInChildren<TMP_Text>(true);
                Assert.That(label, Is.Not.Null, "Start button label");
                foreach (var text in new[] { "Next", "Start" }) Assert.That(label.GetPreferredValues(text).x, Is.LessThanOrEqualTo(label.rectTransform.rect.width + 1f), text);
            }
            finally { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }
        }
    }
}
