using System.Linq;
using Airlift.Lessons;
using Airlift.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Airlift.Tests
{
    /// Owner 2026-09-17: splitting only worked by voice. The Dock 7 splitter: set a crate on the pad, choose the chunk
    /// size; the chapter's size splits (through CargoLessonDirector.TrySplit), the wrong size gets a hint naming the
    /// size the vehicles need. Pure rules plus the component on in-memory objects (no scene).
    public class SplitterRulesTests
    {
        static CargoChapter Pickups => CargoChapter.All[1];
        static CargoChapter Vans => CargoChapter.All[2];

        [Test] public void NoCrateOnThePadComesFirst()
        {
            var r = SplitterRules.Choose(Pickups.SplitTo, 2, false, true, Pickups.VehicleKind);
            Assert.That(r.Ok, Is.False); Assert.That(r.Reason, Is.EqualTo("Set a crate on the splitter first."));
            Assert.That(SplitterRules.Choose(0, 2, false, false, "truck").Reason, Is.EqualTo(SplitterRules.NoCrateReason));
        }

        [Test] public void ChaptersWithoutSplittingSaySo()
        {
            foreach (var chapter in CargoChapter.All.Where(c => c.SplitTo == 0))
                foreach (int chosen in new[] { 2, 4 })
                {
                    var r = SplitterRules.Choose(chapter.SplitTo, chosen, true, false, chapter.VehicleKind);
                    Assert.That(r.Ok, Is.False, chapter.Id); Assert.That(r.Reason, Is.EqualTo("No splitting in this chapter."), chapter.Id);
                }
        }

        [Test] public void SplitCratesCannotSplitAgain()
        {
            foreach (int chosen in new[] { 2, 4 })
            {
                var r = SplitterRules.Choose(Pickups.SplitTo, chosen, true, false, Pickups.VehicleKind);
                Assert.That(r.Ok, Is.False); Assert.That(r.Reason, Is.EqualTo("Those crates are already split."));
            }
        }

        [Test] public void TheWrongSizeGetsAHintNamingTheNeededSize()
        {
            var halvesNeeded = SplitterRules.Choose(Pickups.SplitTo, 4, true, true, Pickups.VehicleKind);
            Assert.That(halvesNeeded.Ok, Is.False);
            Assert.That(halvesNeeded.Reason, Is.EqualTo("Each pickup takes half a container. Cut into halves."));
            var quartersNeeded = SplitterRules.Choose(Vans.SplitTo, 2, true, true, Vans.VehicleKind);
            Assert.That(quartersNeeded.Ok, Is.False);
            Assert.That(quartersNeeded.Reason, Is.EqualTo("Each van takes a quarter of a container. Cut into quarters."));
        }

        [Test] public void TheChapterSizeIsOk()
        {
            Assert.That(SplitterRules.Choose(Pickups.SplitTo, 2, true, true, Pickups.VehicleKind).Ok, Is.True);
            Assert.That(SplitterRules.Choose(Vans.SplitTo, 4, true, true, Vans.VehicleKind).Ok, Is.True);
        }

        [Test] public void ButtonLabelsNameTheChunk()
        {
            Assert.That(SplitterRules.ButtonLabel(2), Is.EqualTo("Halves · 1/2"));
            Assert.That(SplitterRules.ButtonLabel(4), Is.EqualTo("Quarters · 1/4"));
        }

        [Test] public void CopyIsPlainAndNeverPraises()
        {
            var lines = new[] { SplitterRules.NoCrateReason, SplitterRules.NoSplitReason, SplitterRules.AlreadySplitReason,
                                SplitterRules.Hint(2, "pickup"), SplitterRules.Hint(4, "van") };
            foreach (var line in lines)
            {
                foreach (var banned in new[] { "great", "job", "star", "score", "points", "wrong", "oops" })
                    Assert.That(line.ToLowerInvariant(), Does.Not.Contain(banned), line);
                Assert.That(line.Split(' ').Length, Is.LessThanOrEqualTo(12), "short sentences: " + line);
            }
        }

        [Test] public void OverPadUsesThePadFootprintAndAReachableHeight()
        {
            var centre = new Vector3(0.22f, 0.0255f, -0.315f); var size = new Vector2(0.30f, 0.14f);
            Assert.That(SplitterRules.OverPad(centre, size, centre + new Vector3(0, 0.055f, 0)), Is.True, "resting on the pad");
            Assert.That(SplitterRules.OverPad(centre, size, centre + new Vector3(0.149f, 0.2f, 0.069f)), Is.True, "corner, lifted");
            Assert.That(SplitterRules.OverPad(centre, size, centre + new Vector3(0.151f, 0.055f, 0)), Is.False, "beside the pad");
            Assert.That(SplitterRules.OverPad(centre, size, centre + new Vector3(0, 0.055f, -0.071f)), Is.False, "in front of the pad");
            Assert.That(SplitterRules.OverPad(centre, size, centre + new Vector3(0, SplitterRules.MaxLift + 0.01f, 0)), Is.False, "far above it");
            Assert.That(SplitterRules.OverPad(centre, size, centre + new Vector3(0, -0.05f, 0)), Is.False, "under the table");
        }

        // ---- the component on in-memory objects ----
        static CargoLessonDirector MakeLesson(GameObject station)
        {
            var ruler = new GameObject("ruler").transform; ruler.SetParent(station.transform, false); ruler.localPosition = new Vector3(0, 0.031f, 0.02f);
            var lesson = station.AddComponent<CargoLessonDirector>();
            lesson.stationRoot = station.transform; lesson.ruler = ruler; lesson.restHeight = 0.107f;
            CargoLessonDirector.PieceView View(string id, Vector3 tray)
            {
                var piece = new GameObject(id).transform; piece.SetParent(station.transform, false); piece.localPosition = tray;
                return new CargoLessonDirector.PieceView { id = id, piece = piece, trayPosition = tray };
            }
            lesson.whole = View("whole", new Vector3(-0.13f, 0.0725f, -0.30f));
            lesson.halfA = View("half-1", new Vector3(-0.21f, 0.0725f, -0.30f));
            lesson.halfB = View("half-2", new Vector3(-0.05f, 0.0725f, -0.30f));
            lesson.quarters = Enumerable.Range(1, 4).Select(i => View("quarter-" + i, new Vector3(-0.265f + (i - 1) * 0.09f, 0.0725f, -0.30f))).ToArray();
            return lesson;
        }

        [Test] public void SplitterSplitsOnlyTheChapterSizeWithALooseCrateOnThePad()
        {
            var station = new GameObject("test station");
            try
            {
                var lesson = MakeLesson(station);
                var splitter = station.AddComponent<CrateSplitter>();
                splitter.lesson = lesson; splitter.stationRoot = station.transform;
                splitter.padCenter = new Vector3(0.22f, 0.0255f, -0.315f); splitter.padSize = new Vector2(0.30f, 0.14f);
                var onPad = new Vector3(0.22f, 0.081f, -0.315f);

                Assert.That(splitter.Choose(2).Ok, Is.False, "no chapter on the table");
                lesson.Begin();   // chapter 1: big truck, no splitting
                Assert.That(splitter.CrateOnPad, Is.False, "crates wait in the tray");
                Assert.That(splitter.Choose(2).Reason, Is.EqualTo(SplitterRules.NoCrateReason));
                lesson.whole.piece.localPosition = onPad;
                Assert.That(splitter.CrateOnPad, Is.True);
                Assert.That(splitter.Choose(2).Reason, Is.EqualTo(SplitterRules.NoSplitReason));

                lesson.JumpToChapter(1);   // two pickups
                Assert.That(splitter.CrateOnPad, Is.False, "the chapter starts with the crate back in the tray");
                lesson.whole.piece.localPosition = onPad;
                var hint = splitter.Choose(4);
                Assert.That(hint.Ok, Is.False); Assert.That(hint.Reason, Is.EqualTo("Each pickup takes half a container. Cut into halves."));
                Assert.That(lesson.CanSplit, Is.True, "a hint changes nothing");
                Assert.That(lesson.whole.piece.gameObject.activeSelf, Is.True);

                lesson.whole.held = true;
                Assert.That(splitter.Choose(2).Reason, Is.EqualTo(CargoLessonDirector.HeldReason), "the director still refuses while a crate is held");
                lesson.whole.held = false;

                var split = splitter.Choose(2);
                Assert.That(split.Ok, Is.True, split.Reason);
                Assert.That(split.Reason, Does.StartWith("The crate is split into 2 equal chunks"));
                Assert.That(lesson.CanSplit, Is.False);
                Assert.That(lesson.whole.piece.gameObject.activeSelf, Is.False); Assert.That(lesson.halfA.piece.gameObject.activeSelf && lesson.halfB.piece.gameObject.activeSelf, Is.True);
                Assert.That(lesson.halfA.piece.localPosition, Is.EqualTo(lesson.halfA.trayPosition), "chunks go to the tray");
                lesson.halfA.piece.localPosition = onPad;
                Assert.That(splitter.Choose(2).Reason, Is.EqualTo(SplitterRules.AlreadySplitReason));

                // A crate loaded into a bed is not on the splitter, even if it were moved over the pad by accident.
                Assert.That(lesson.DropAt("half-2", new Vector3(RulerLayout.CellsCenterX(lesson.ruler.localPosition, 4, 4), 0.08f, 0.02f)), Is.True);
                lesson.halfA.piece.localPosition = lesson.halfA.trayPosition;
                lesson.halfB.piece.localPosition = onPad;
                Assert.That(splitter.CrateOnPad, Is.False, "docked crates do not count");

                lesson.JumpToChapter(2);   // four vans, two half crates
                lesson.halfB.piece.localPosition = onPad;
                Assert.That(splitter.Choose(2).Reason, Is.EqualTo("Each van takes a quarter of a container. Cut into quarters."));
                var quarters = splitter.Choose(4);
                Assert.That(quarters.Ok, Is.True, quarters.Reason);
                Assert.That(lesson.quarters.All(q => q.piece.gameObject.activeSelf), Is.True, "four quarter crates");
            }
            finally { Object.DestroyImmediate(station); }
        }
    }
}
