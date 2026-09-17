using System.Collections.Generic;
using System.Linq;
using Airlift.Lessons;
using Airlift.Lessons.Garden;
using Airlift.Onboarding;
using Airlift.Presentation;
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
    /// CC-GD-02 / CC-GD-03: the Sunny Plot workbench built by AgentScripts/BuildGardenWorkbench.cs (read-only against
    /// CargoCrew) and GardenStation behaviour on in-memory objects: drop to plant, turn, fence, check and payoff.
    public class GardenWorkbenchWiringTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        const float CardZ = GardenTableLayout.CardZ, CardSightline = 0.19f;
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }
        static T Find<T>() where T : Component => SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).FirstOrDefault();
        static OnboardingDirector Onboarding() => Find<OnboardingDirector>();
        static GardenStation Station() => Find<GardenStation>();

        // ---- wiring ----
        [Test] public void RootIsAnInactiveChildOfTheWorldLockedRoot()
        {
            var d = Onboarding(); var s = Station();
            Assert.That(s, Is.Not.Null, "GardenStation in CargoCrew");
            Assert.That(s.gameObject.name, Is.EqualTo("Garden workbench"));
            Assert.That(s.transform.parent, Is.EqualTo(d.transform));
            Assert.That(s.gameObject.activeSelf, Is.False, "inactive by default");
            Assert.That(s.transform.localPosition, Is.EqualTo(Vector3.zero)); Assert.That(s.transform.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(s.cardId, Is.EqualTo("community_garden_multiplication"));
            Assert.That(LessonCatalog.Find(s.cardId), Is.Not.Null);
            Assert.That(s.visualRoots, Is.EqualTo(new[] { s.gameObject }));
            Assert.That(s.stationRoot, Is.EqualTo(s.transform));
            Assert.That(s.theme, Is.Not.Null); Assert.That(s.theme.deckMaterial, Is.Not.Null); Assert.That(s.theme.deckMaterial.name, Does.StartWith("Garden"));
            Assert.That(s.Title, Is.EqualTo("Community Garden")); Assert.That(s.StoryName, Is.EqualTo("Sunny Plot"));
            Assert.That(s.ToolNames, Is.EquivalentTo(new[] { "garden_turn_bed", "garden_split_bed", "garden_check_bed", "garden_clear_bed" }));
            var deck = s.transform.Find("Garden deck").GetComponent<MeshFilter>().sharedMesh.bounds.size;
            var cargoDeck = d.transform.Find("Workbench").GetComponent<MeshFilter>().sharedMesh.bounds.size;
            Assert.That(Vector3.Distance(deck, cargoDeck), Is.LessThan(1e-3f), "Cargo Workbench footprint");
        }

        [Test] public void StripsAndFenceAreGrabbablePieces()
        {
            var s = Station();
            Assert.That(s.strips.Length, Is.EqualTo(GardenChapter.All.Max(c => System.Math.Max(c.Rows, c.StripLengths?.Length ?? 0))), "one view per strip in the largest chapter");
            for (int len = 1; len <= GardenBedLayout.MaxColumns; len++)
                Assert.That(s.stripMeshes[len].bounds.size.x, Is.EqualTo(len * s.stripCell - GardenTableLayout.StripBodyInset).Within(1e-4f), "strip body of " + len);
            foreach (var v in s.strips)
            {
                Assert.That(v.piece, Is.Not.Null); Assert.That(v.piece.IsChildOf(s.pieces.transform), Is.True, v.piece.name);
                Assert.That(v.piece.GetComponentInChildren<Oculus.Interaction.GrabInteractable>(true), Is.Not.Null, v.piece.name + " grabbable by controller");
                Assert.That(v.grabbable, Is.Not.Null); Assert.That(v.grabbable.MaxGrabPoints, Is.EqualTo(1));
                Assert.That(v.collider, Is.Not.Null); Assert.That(v.bodyLeft, Is.Not.Null); Assert.That(v.bodyRight, Is.Not.Null);
                Assert.That(v.seedlings.Length, Is.EqualTo(GardenBedLayout.MaxColumns)); Assert.That(v.sprouts.Length, Is.EqualTo(GardenBedLayout.MaxColumns)); Assert.That(v.blooms.Length, Is.EqualTo(GardenBedLayout.MaxColumns));
                foreach (var bloom in v.blooms)
                {
                    Assert.That(bloom.gameObject.activeSelf, Is.False, "blooms hidden until an accepted check");
                    foreach (var kind in new[] { GardenStation.Lettuce, GardenStation.Carrot, GardenStation.Sunflower, GardenStation.Bean })
                        Assert.That(bloom.Cast<Transform>().Any(c => c.name.StartsWith(kind + " ")), Is.True, v.piece.name + " can grow " + kind);
                }
            }
            Assert.That(s.fence, Is.Not.Null); Assert.That(s.fence.IsChildOf(s.pieces.transform), Is.True);
            Assert.That(s.fence.GetComponentInChildren<Oculus.Interaction.GrabInteractable>(true), Is.Not.Null, "fence grabbable by controller");
            Assert.That(s.fenceGrabbable.MaxGrabPoints, Is.EqualTo(1));
            Assert.That(s.pieces.activeSelf, Is.False, "pieces hidden until a chapter starts");
            Assert.That(s.wateringCan, Is.Not.Null); Assert.That(s.butterfly, Is.Not.Null);
            Assert.That(s.partLabels.Length, Is.EqualTo(2)); Assert.That(s.partLabels.All(l => l != null && !l.gameObject.activeSelf), Is.True);
        }

        [Test] public void TableHandleGatesGardenPieces()
        {
            var d = Onboarding(); var s = Station(); var handle = d.GetComponent<TableHandle>();
            Assert.That(handle, Is.Not.Null);
            foreach (var g in s.pieces.GetComponentsInChildren<Oculus.Interaction.GrabInteractable>(true))
                Assert.That(handle.pieceInteractables, Does.Contain(g), g.transform.parent.name);
        }

        [Test] public void FontsBoundAndMeshesRounded()
        {
            var d = Onboarding(); var s = Station(); var binding = d.GetComponent<TypographyBindings>();
            foreach (var t in s.GetComponentsInChildren<TMP_Text>(true))
            {
                Assert.That(t.font == binding.style.headingFont || t.font == binding.style.bodyFont, Is.True, t.name + " uses an Airlift font");
                Assert.That(binding.headings.Contains(t) || binding.bodies.Contains(t), Is.True, t.name + " is in the typography inventory");
            }
            var raw = s.GetComponentsInChildren<MeshFilter>(true).Where(f => f.GetComponent<TMP_Text>() == null && (f.sharedMesh == null || f.sharedMesh.name == "Cube")).Select(f => f.name).ToArray();
            Assert.That(raw, Is.Empty, "raw cubes: " + string.Join(", ", raw));
            var card = s.transform.Find("Garden card");
            foreach (var c in s.GetComponentsInChildren<Collider>(true))
                Assert.That(c.transform.IsChildOf(s.pieces.transform) || c.transform.IsChildOf(card), Is.True, c.name + ": decorative garden props cannot block controller rays");
        }

        [Test] public void PropsStayUnderTheCardSightline()
        {
            // From the seated head (board-local y 0.5, z -0.65) anything taller than ~0.19 m near the back shows through
            // the translucent card. Strips and their blooms (fully grown, scale 1) are measured where they can sit:
            // in the tray and in the back row of every chapter's bed at that chapter's piece scale, against the line
            // from the seated head to the card's bottom edge at their depth.
            var s = Station(); var card = s.transform.Find("Garden card");
            foreach (var f in s.GetComponentsInChildren<MeshFilter>(true))
            {
                if (f.transform.IsChildOf(card) || f.GetComponent<TMP_Text>() != null || f.sharedMesh == null) continue;   // world labels lie flat on the deck
                var strip = s.strips.FirstOrDefault(v => f.transform.IsChildOf(v.piece));
                if (strip == null)
                {
                    Assert.That(TopOf(s.transform, f, s.transform), Is.LessThan(CardSightline), f.name + " (" + f.transform.parent.name + ") stays below the card sightline");
                    continue;
                }
                float top = TopOf(strip.piece, f, strip.piece);   // strip-local, unscaled
                Assert.That(s.trayHeight + top, Is.LessThan(GardenTableLayout.CardSightline(GardenTableLayout.TrayBackZ) - 0.02f), f.name + " in the tray");
                foreach (var chapter in GardenChapter.All)
                {
                    var L = s.bed.LayoutFor(chapter.Rows, chapter.Columns);
                    float scale = L.Cell / s.stripCell;
                    float y = L.Center.y + scale * top, z = L.RowZ(0) + scale * s.stripDepth * 0.5f;
                    Assert.That(y, Is.LessThan(GardenTableLayout.CardSightline(z) - 0.02f), chapter.Id + ": " + f.name + " (" + f.transform.parent.name + ") grown in the back row stays under the card");
                }
            }
            Assert.That(GardenTableLayout.CardSightline(GardenTableLayout.CardZ), Is.EqualTo(GardenTableLayout.CardBottomY).Within(1e-4f), "the sightline meets the card bottom at the card");
            Assert.That(GardenTableLayout.CardSightline(0.37f), Is.LessThan(CardSightline), "near the back the flat 0.19 m rule is the stricter one");
            Assert.That(s.wateringCanHome.y + 0.1f, Is.LessThan(GardenTableLayout.CardSightline(s.wateringCanHome.z + 0.03f)), "watering can visible under the card");
            Assert.That(s.butterflyHome.y, Is.LessThan(CardSightline));
        }

        /// Highest root-local y of a mesh, treating zero scales (hidden blooms) as fully grown.
        static float TopOf(Transform root, MeshFilter f, Transform stop)
        {
            var m = Matrix4x4.identity;
            for (var t = f.transform; t != null && t != stop; t = t.parent)
            {
                var scale = t.localScale; if (scale.x == 0 || scale.y == 0 || scale.z == 0) scale = Vector3.one;
                m = Matrix4x4.TRS(t.localPosition, t.localRotation, scale) * m;
            }
            var b = f.sharedMesh.bounds; float top = float.MinValue;
            foreach (float x in new[] { b.min.x, b.max.x }) foreach (float y in new[] { b.min.y, b.max.y }) foreach (float z in new[] { b.min.z, b.max.z })
                top = Mathf.Max(top, m.MultiplyPoint3x4(new Vector3(x, y, z)).y);
            return top;
        }

        // ---- bed and trays ----
        [Test] public void BedPoolCoversEveryChapterAndTurn()
        {
            var s = Station(); var bed = s.bed;
            Assert.That(bed.stationRoot, Is.EqualTo(s.transform));
            Assert.That(bed.cells.Length, Is.EqualTo(64)); Assert.That(bed.cells, Has.None.Null);
            Assert.That(bed.cells.All(c => c.transform.IsChildOf(bed.turnable)), Is.True, "cells turn with the bed");
            Assert.That(bed.rowLabels.Length, Is.EqualTo(8)); Assert.That(bed.rowLabels, Has.None.Null);
            Assert.That(bed.fenceTicks.Length, Is.EqualTo(7)); Assert.That(bed.fenceTicks, Has.None.Null);
            for (int k = 1; k <= 7; k++) Assert.That(bed.fenceTicks[k - 1].GetComponentInChildren<TMP_Text>(true).text, Is.EqualTo(k.ToString()));
            var first = bed.LayoutFor(GardenChapter.All[0].Rows, GardenChapter.All[0].Columns);
            Assert.That(bed.turnable.localPosition.x, Is.EqualTo(first.Center.x).Within(1e-4f)); Assert.That(bed.turnable.localPosition.z, Is.EqualTo(first.Center.z).Within(1e-4f), "closed state: the turnable sits under the chapter 1 bed centre");
            Assert.That(bed.meshCell, Is.EqualTo(s.stripCell).Within(1e-6f), "soil and wall meshes are built for full-size cells");
            Assert.That(bed.wallThickness, Is.EqualTo(GardenTableLayout.WallThickness).Within(1e-6f));
            for (int n = 1; n <= 8; n++)
            {
                Assert.That(bed.longWalls[n].bounds.size.x, Is.EqualTo(n * bed.meshCell + 2 * bed.wallThickness).Within(1e-4f));
                Assert.That(bed.shortWalls[n].bounds.size.x, Is.EqualTo(n * bed.meshCell).Within(1e-4f));
            }
            foreach (var chapter in GardenChapter.All)
                foreach (var (rows, cols) in new[] { (chapter.Rows, chapter.Columns), (chapter.Columns, chapter.Rows) })
                {
                    Assert.That(rows, Is.LessThanOrEqualTo(GardenBedLayout.MaxRows)); Assert.That(cols, Is.LessThanOrEqualTo(GardenBedLayout.MaxColumns));
                    var L = bed.LayoutFor(rows, cols); var expected = GardenTableLayout.BedFor(rows, cols);
                    Assert.That(Vector3.Distance(L.Center, expected.Center), Is.LessThan(1e-5f), chapter.Id + " scene bed uses the table layout");
                    Assert.That(L.Cell, Is.EqualTo(expected.Cell).Within(1e-6f), chapter.Id + " scene bed cell");
                    Assert.That(L.Back + bed.wallThickness, Is.LessThan(CardZ - 0.02f), chapter.Id + " bed stays in front of the card");
                    Assert.That(L.Cell, Is.GreaterThanOrEqualTo(0.06f), "cells big enough to read seedlings");
                }
        }

        /// Owner 2026-09-17 ("the plants are too little"): seedling strips and every plant on them are twice the first
        /// build in all three dimensions: body, pitch, collider, sprout and every crop they grow into.
        [Test] public void StripsAndPlantsAreTwiceTheFirstBuild()
        {
            const float firstCell = 0.045f;
            var s = Station();
            Assert.That(s.stripCell, Is.EqualTo(2 * firstCell).Within(1e-6f), "seedling pitch on a strip");
            Assert.That(s.stripDepth, Is.EqualTo(2 * 0.04f).Within(1e-6f)); Assert.That(s.stripHeight, Is.EqualTo(2 * 0.04f).Within(1e-6f));
            Assert.That(s.splitGap, Is.EqualTo(GardenTableLayout.SplitGap).Within(1e-6f));
            for (int len = 1; len <= GardenBedLayout.MaxColumns; len++)
                AssertClose(s.stripMeshes[len].bounds.size, 2f * new Vector3(len * firstCell - 0.004f, 0.012f, 0.036f), "strip body of " + len);
            var parts = new (string name, Vector3 position, Vector3 size)[]
            {
                ("Stem", new Vector3(0, 0.008f, 0), new Vector3(0.004f, 0.016f, 0.004f)),
                ("Leaves", new Vector3(0, 0.017f, 0), new Vector3(0.022f, 0.007f, 0.012f)),
                ("Lettuce head", new Vector3(0, 0.012f, 0), new Vector3(0.032f, 0.024f, 0.032f)),
                ("Carrot top", new Vector3(0, 0.022f, 0), new Vector3(0.012f, 0.03f, 0.012f)),
                ("Carrot root", new Vector3(0, 0.004f, 0), new Vector3(0.022f, 0.014f, 0.022f)),
                ("Sunflower stalk", new Vector3(0, 0.025f, 0), new Vector3(0.005f, 0.05f, 0.005f)),
                ("Sunflower petals", new Vector3(0, 0.055f, -0.004f), new Vector3(0.034f, 0.034f, 0.006f)),
                ("Sunflower centre", new Vector3(0, 0.056f, -0.008f), new Vector3(0.015f, 0.015f, 0.008f)),
                ("Bean pole", new Vector3(0.006f, 0.03f, 0), new Vector3(0.004f, 0.06f, 0.004f)),
                ("Bean pods", new Vector3(-0.003f, 0.028f, 0), new Vector3(0.016f, 0.03f, 0.012f)),
            };
            foreach (var v in s.strips)
            {
                Assert.That(v.piece.localScale, Is.EqualTo(Vector3.one), v.piece.name + " full size in the closed state");
                AssertClose(v.collider.size, 2f * new Vector3(GardenBedLayout.MaxColumns * firstCell, 0.04f, 0.04f), v.piece.name + " collider");
                AssertClose(v.bodyLeft.sharedMesh.bounds.size, 2f * new Vector3(GardenBedLayout.MaxColumns * firstCell - 0.004f, 0.012f, 0.036f), v.piece.name + " body");
                for (int k = 0; k < v.seedlings.Length; k++)
                {
                    Assert.That(v.seedlings[k].localPosition.x, Is.EqualTo(2 * GardenBedLayout.SeedlingX(firstCell, k, GardenBedLayout.MaxColumns)).Within(1e-5f), v.piece.name + " seedling pitch");
                    Assert.That(v.seedlings[k].localPosition.y, Is.EqualTo(2 * 0.006f).Within(1e-5f), v.piece.name + " seedling sits on the body");
                    foreach (var (name, position, size) in parts)
                    {
                        var part = v.seedlings[k].GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f => f.name == name);
                        Assert.That(part, Is.Not.Null, v.piece.name + " " + name);
                        AssertClose(part.sharedMesh.bounds.size, 2f * size, v.piece.name + " " + name + " size");
                        AssertClose(part.transform.localPosition, 2f * position, v.piece.name + " " + name + " position");
                    }
                }
            }
            // The fence divider and the payoff grow with the plants.
            var post = s.fence.Find("Post 1").GetComponent<MeshFilter>().sharedMesh.bounds.size;
            AssertClose(post, 2f * new Vector3(0.01f, 0.06f, 0.01f), "fence post");
            Assert.That(s.fence.GetComponent<BoxCollider>().size.z, Is.EqualTo(GardenTableLayout.FenceLength + 0.02f).Within(1e-4f), "the fence spans the deepest bed");
            Assert.That(GardenTableLayout.BedMaxDepth, Is.LessThanOrEqualTo(GardenTableLayout.FenceLength));
            AssertClose(s.wateringCan.Find("Can body").GetComponent<MeshFilter>().sharedMesh.bounds.size, 1.5f * new Vector3(0.06f, 0.05f, 0.04f), "watering can");
            AssertClose(s.butterfly.Find("Wing left").GetComponent<MeshFilter>().sharedMesh.bounds.size, 2f * new Vector3(0.016f, 0.002f, 0.014f), "butterfly wing");
        }

        static void AssertClose(Vector3 actual, Vector3 expected, string what)
        {
            Assert.That(Vector3.Distance(actual, expected), Is.LessThan(2e-4f), what + ": " + actual.ToString("0.0000") + " expected " + expected.ToString("0.0000"));
        }

        /// Every label in front of or beside the bed stays inside the reach the table layout plans for (so the pure fit
        /// test below is honest about the scene), and every prop, the watering can and the trays stay clear of every
        /// chapter's bed, its labels and its opened split.
        [Test] public void LabelsStayInTheirPlannedReachAndPropsClearTheBedAndTrays()
        {
            var s = Station(); var bed = s.bed;
            float Tilted(TMP_Text t) => t.rectTransform.sizeDelta.y * Mathf.Sin(70f * Mathf.Deg2Rad) * 0.5f;
            foreach (var tick in bed.fenceTicks)
            {
                var number = tick.GetComponentInChildren<TMP_Text>(true);
                float reach = bed.wallThickness * 0.5f - number.transform.localPosition.z + Tilted(number);
                Assert.That(reach, Is.LessThanOrEqualTo(GardenTableLayout.BedLabelReach), tick.name + " number reaches " + reach.ToString("0.000") + " m in front of the bed");
            }
            foreach (var label in s.partLabels)
                Assert.That(bed.wallThickness + s.partLabelOffset + Tilted(label), Is.LessThanOrEqualTo(GardenTableLayout.BedLabelReach), label.name + " in front of the bed");
            Assert.That(bed.wallThickness + bed.rowLabelOffset + bed.rowLabels[0].rectTransform.sizeDelta.x * 0.5f, Is.LessThanOrEqualTo(GardenTableLayout.RowLabelReach), "row numbers beside the bed");

            var root = s.transform;
            var blockers = new List<(string name, Bounds b)>();
            foreach (Transform prop in root.Find("Garden props")) blockers.Add(("prop " + prop.name, Footprint(root, prop)));
            blockers.Add(("watering can", Footprint(root, s.wateringCan)));
            foreach (var chapter in GardenChapter.All)
                foreach (var (rows, cols) in chapter.NeedsTurn ? new[] { (chapter.Rows, chapter.Columns), (chapter.Columns, chapter.Rows) } : new[] { (chapter.Rows, chapter.Columns) })
                {
                    var L = bed.LayoutFor(rows, cols);
                    float gap = chapter.HasFence ? s.splitGap : 0f;
                    var area = Rect.MinMaxRect(L.Left - GardenTableLayout.RowLabelReach, L.Front - GardenTableLayout.BedLabelReach, L.Right + gap + bed.wallThickness, L.Back + bed.wallThickness);
                    foreach (var (name, b) in blockers)
                        Assert.That(Overlaps(area, b), Is.False, chapter.Id + " " + rows + " × " + cols + " bed and labels clear of " + name);
                }
            foreach (var trayName in new[] { "Seedling tray left", "Seedling tray right" })
            {
                var tray = Footprint(root, root.Find(trayName));
                var area = Rect.MinMaxRect(tray.min.x, tray.min.z, tray.max.x, tray.max.z);
                foreach (var (name, b) in blockers) Assert.That(Overlaps(area, b), Is.False, trayName + " clear of " + name);
            }
            foreach (var (name, b) in blockers)
            {
                Assert.That(Mathf.Max(Mathf.Abs(b.min.x), Mathf.Abs(b.max.x)), Is.LessThan(GardenTableLayout.DeckHalfX + 0.01f), name + " on the deck");
                Assert.That(b.min.z, Is.GreaterThan(GardenTableLayout.HandleBackZ), name + " clear of the table handle");
            }
        }

        static bool Overlaps(Rect area, Bounds b) => b.min.x < area.xMax && b.max.x > area.xMin && b.min.z < area.yMax && b.max.z > area.yMin;

        /// Owner render review 2026-09-17: the tray slabs dominated the foreground; the same day the strips doubled. The
        /// two trays run front to back beside the bed, each just big enough for the three strips of its side lying
        /// front to back, and the scene trays, tray slots and fence home are the table layout's.
        [Test] public void TraysAreTidyAndHoldTheirStrips()
        {
            var s = Station();
            var trays = new[] { s.transform.Find("Seedling tray left"), s.transform.Find("Seedling tray right") };
            Assert.That(trays, Has.None.Null);
            for (int side = 0; side < 2; side++)
            {
                var f = Footprint(s.transform, trays[side]);
                float sign = side == 0 ? -1f : 1f;
                Assert.That(Mathf.Min(Mathf.Abs(f.min.x), Mathf.Abs(f.max.x)), Is.EqualTo(GardenTableLayout.TrayNearX).Within(0.002f), "tray inner edge");
                Assert.That(f.size.x, Is.EqualTo(GardenTableLayout.TrayWidth).Within(0.002f), "tray width " + f.size.x);
                Assert.That(f.min.z, Is.EqualTo(GardenTableLayout.TrayFrontZ).Within(0.002f)); Assert.That(f.max.z, Is.EqualTo(GardenTableLayout.TrayBackZ).Within(0.002f));
                Assert.That(Mathf.Sign(f.center.x), Is.EqualTo(sign));
                Assert.That(f.max.y, Is.LessThan(s.trayHeight + 0.006f), "tray rims stay below the strip tops so strips are easy to grab");
            }
            Assert.That(trays[0].GetComponentsInChildren<Renderer>(true).Select(r => r.sharedMaterial.name).Distinct().Count(), Is.GreaterThanOrEqualTo(2), "wooden tray with a green trim");
            Assert.That(s.trayHeight, Is.EqualTo(GardenTableLayout.TrayRestY).Within(1e-5f));
            Assert.That(s.trayYaw, Is.EqualTo(GardenTableLayout.TrayYaw).Within(1e-5f));
            for (int i = 0; i < s.strips.Length; i++)
            {
                AssertClose(s.strips[i].trayFront, GardenTableLayout.TrayFront(i), "strip view " + (i + 1) + " tray slot");
                Assert.That(Quaternion.Angle(s.strips[i].piece.localRotation, Quaternion.Euler(0, GardenTableLayout.TrayYaw, 0)), Is.LessThan(0.5f), "strip view " + (i + 1) + " rests front to back in its tray");
            }
            AssertClose(s.fenceHome, GardenTableLayout.FenceHome, "fence home in the right tray");
        }

        /// Pure table layout, every chapter: strips that rest in a tray lie inside it front to back without overlapping,
        /// within reach of the seated learner, never planted by accident; the fence at home lies in the right tray.
        [Test] public void TrayStripsLieInsideTheTraysWithinReach()
        {
            float cell = GardenTableLayout.StripCell;
            foreach (var chapter in GardenChapter.All.Where(c => !c.StartsPlanted))
            {
                Assert.That(GardenTableLayout.BedFor(chapter.Rows, chapter.Columns).Cell, Is.EqualTo(cell).Within(1e-6f), chapter.Id + ": tray chapters are full size");
                for (int i = 0; i < chapter.StripLengths.Length; i++)
                {
                    int len = chapter.StripLengths[i];
                    var p = GardenTableLayout.TrayPosition(GardenTableLayout.TrayFront(i), len, cell);
                    float halfLength = len * cell * 0.5f, halfDepth = GardenTableLayout.StripBodyDepth * 0.5f;
                    string what = chapter.Id + " strip-" + (i + 1) + " (" + len + ")";
                    Assert.That(Mathf.Abs(p.x) - halfDepth, Is.GreaterThanOrEqualTo(GardenTableLayout.TrayNearX + GardenTableLayout.TrayRim), what + " inside its tray (bed side)");
                    Assert.That(Mathf.Abs(p.x) + halfDepth, Is.LessThanOrEqualTo(GardenTableLayout.TrayNearX + GardenTableLayout.TrayWidth - GardenTableLayout.TrayRim), what + " inside its tray (outer side)");
                    Assert.That(p.z - halfLength, Is.GreaterThanOrEqualTo(GardenTableLayout.TrayFrontZ + GardenTableLayout.TrayRim), what + " inside its tray (front rim)");
                    Assert.That(p.z + halfLength, Is.LessThanOrEqualTo(GardenTableLayout.TrayBackZ - GardenTableLayout.TrayRim), what + " inside its tray (back rim)");
                    Assert.That(p.y, Is.EqualTo(GardenTableLayout.TrayRestY));
                    for (int j = 0; j < i; j++)
                    {
                        var q = GardenTableLayout.TrayPosition(GardenTableLayout.TrayFront(j), chapter.StripLengths[j], cell);
                        if (Mathf.Sign(q.x) == Mathf.Sign(p.x)) Assert.That(Mathf.Abs(q.x - p.x), Is.GreaterThanOrEqualTo(GardenTableLayout.StripBodyDepth + 0.01f), what + " does not touch strip-" + (j + 1));
                    }
                    var front = new Vector3(p.x, p.y, p.z - halfLength);
                    Assert.That(front.z, Is.LessThan(-0.3f), what + " grabbable at the front of the table");
                    Assert.That(Vector3.Distance(front, GardenTableLayout.SeatedHead), Is.LessThan(0.8f), what + " front end within seated reach");
                    foreach (var c in GardenChapter.All)
                        foreach (var (rows, cols) in new[] { (c.Rows, c.Columns), (c.Columns, c.Rows) })
                            Assert.That(GardenTableLayout.BedFor(rows, cols).RowAt(p), Is.EqualTo(-1), what + ": a strip resting in the tray is never planted by accident");
                }
                // Every row of a tray chapter is within seated reach for planting.
                var L = GardenTableLayout.BedFor(chapter.Rows, chapter.Columns);
                foreach (float x in new[] { L.Left, L.Right })
                    Assert.That(Vector3.Distance(new Vector3(x, L.Center.y, L.RowZ(0)), GardenTableLayout.SeatedHead), Is.LessThan(0.9f), chapter.Id + " back row within reach");
            }
            var home = GardenTableLayout.FenceHome;
            Assert.That(Mathf.Abs(home.x - (GardenTableLayout.TrayNearX + GardenTableLayout.TrayWidth * 0.5f)) + 0.03f, Is.LessThanOrEqualTo(GardenTableLayout.TrayWidth * 0.5f - GardenTableLayout.TrayRim), "fence home across the right tray");
            Assert.That(home.z - GardenTableLayout.FenceLength * 0.5f, Is.GreaterThanOrEqualTo(GardenTableLayout.TrayFrontZ + GardenTableLayout.TrayRim), "fence home inside the tray (front)");
            Assert.That(home.z + GardenTableLayout.FenceLength * 0.5f, Is.LessThanOrEqualTo(GardenTableLayout.TrayBackZ - GardenTableLayout.TrayRim), "fence home inside the tray (back)");
            Assert.That(home.y, Is.EqualTo(GardenTableLayout.DeckTop + GardenTableLayout.TrayBase).Within(1e-6f), "the fence stands on the tray base");
            foreach (var chapter in GardenChapter.All)
                Assert.That(GardenTableLayout.BedFor(chapter.Rows, chapter.Columns).FenceAt(home), Is.EqualTo(-1), chapter.Id + ": the fence at home is not on the bed");
        }

        /// Pure table layout, every chapter and the turned bed: twice the first build wherever the bed fits; only the
        /// 7 × 6 and 8 × 7 beds shrink their cells (and their pieces) to fit the deck in front of the card. Every bed
        /// stays in front of the card, its fence numbers and part labels clear the table handle, its row numbers clear
        /// the left tray and its opened split clears the right tray.
        [Test] public void EveryChapterBedAndItsTurnFitTheTable()
        {
            float full = GardenTableLayout.StripCell;
            Assert.That(full, Is.EqualTo(2 * 0.045f).Within(1e-6f), "full-size cell is twice the first build");
            foreach (var chapter in GardenChapter.All)
                foreach (var (rows, cols) in chapter.NeedsTurn ? new[] { (chapter.Rows, chapter.Columns), (chapter.Columns, chapter.Rows) } : new[] { (chapter.Rows, chapter.Columns) })
                {
                    var L = GardenTableLayout.BedFor(rows, cols);
                    string what = chapter.Id + " " + rows + " × " + cols;
                    int span = Mathf.Max(rows, cols);
                    if (span * full <= GardenTableLayout.BedMaxDepth + 1e-6f) Assert.That(L.Cell, Is.EqualTo(full).Within(1e-6f), what + " full size");
                    else
                    {
                        Assert.That(span, Is.GreaterThanOrEqualTo(7), what + ": only the biggest beds shrink");
                        Assert.That(L.Cell, Is.EqualTo(GardenTableLayout.BedMaxDepth / span).Within(1e-6f), what + " as large as the deck allows");
                        Assert.That(L.Cell / full, Is.GreaterThanOrEqualTo(0.69f), what + " piece scale " + (L.Cell / full).ToString("0.000"));
                    }
                    Assert.That(L.Front, Is.GreaterThanOrEqualTo(GardenTableLayout.BedFrontZ - 1e-5f), what + " front wall line");
                    Assert.That(L.Back + GardenTableLayout.WallThickness, Is.LessThan(GardenTableLayout.CardZ - 0.02f), what + " stays in front of the card");
                    Assert.That(L.Front - GardenTableLayout.BedLabelReach, Is.GreaterThan(GardenTableLayout.HandleBackZ + 0.02f), what + " fence numbers and part labels clear the table handle");
                    float gap = chapter.HasFence ? GardenTableLayout.SplitGap : 0f;
                    Assert.That(L.Left - GardenTableLayout.RowLabelReach, Is.GreaterThan(-GardenTableLayout.TrayNearX + 0.02f), what + " row numbers clear the left tray");
                    Assert.That(L.Right + gap + GardenTableLayout.WallThickness, Is.LessThan(GardenTableLayout.TrayNearX - 0.02f), what + " opened split clears the right tray");
                    if (chapter.HasFence)
                        Assert.That(GardenStation.PartLabelPosition(L, cols - 1, gap, 1, GardenTableLayout.WallThickness, GardenTableLayout.PartLabelOffset, GardenTableLayout.DeckTop).x + 0.08f,
                            Is.LessThan(GardenTableLayout.TrayNearX), what + " right part label (at most 16 cm wide) clears the right tray");
                    // Strips of this chapter lie on their rows without touching the next row.
                    Assert.That(GardenTableLayout.StripBodyDepth * L.Cell / full, Is.LessThan(L.Cell), what + " strip bodies fit their rows");
                }
            var before = GardenTableLayout.BedFor(3, 4); var after = before.Turned();
            var turned = GardenTableLayout.BedFor(4, 3);
            Assert.That(turned.Cell, Is.EqualTo(before.Cell)); Assert.That(Vector3.Distance(turned.Center, before.Center), Is.LessThan(1e-6f), "a bed and its quarter turn share cell and centre");
            Assert.That(after.Rows, Is.EqualTo(turned.Rows));
            Assert.That(GardenTableLayout.TrayNearX, Is.GreaterThan(GardenTableLayout.HandleHalfX), "trays beside the handle");
            Assert.That(GardenTableLayout.TrayNearX + GardenTableLayout.TrayWidth, Is.LessThan(GardenTableLayout.DeckHalfX - 0.03f), "trays on the deck inside the edging");
            Assert.That(GardenTableLayout.TrayFrontZ, Is.GreaterThan(-GardenTableLayout.DeckHalfZ + 0.02f), "trays on the deck (front)");
            Assert.That(GardenTableLayout.TrayBackZ, Is.LessThan(GardenTableLayout.CardZ - 0.03f), "trays in front of the card");
            // Scales the 7 × 6 and 8 × 7 chapters get (reported to the owner).
            var scales = GardenChapter.All.Select(c => GardenTableLayout.BedFor(c.Rows, c.Columns).Cell / full).ToArray();
            Assert.That(scales.Take(3), Is.All.EqualTo(1f).Within(1e-6f));
            Assert.That(scales[3], Is.EqualTo(0.5f / 7 / 0.09f).Within(1e-4f)); Assert.That(scales[4], Is.EqualTo(0.5f / 8 / 0.09f).Within(1e-4f));
        }

        /// Owner render review 2026-09-17: row numbers, fence spots, part labels and the sign were unreadable from the
        /// seat. TextMeshPro world text: line height is about 0.136 m per unit of font size (Nunito).
        [Test] public void LabelsAreReadableFromTheSeat()
        {
            var s = Station(); var bed = s.bed;
            void Readable(TMP_Text t, string text, float minSize, float minHeight)
            {
                Assert.That(t.fontSize, Is.GreaterThanOrEqualTo(minSize), t.name + " font size");
                var size = t.GetPreferredValues(text);
                Assert.That(size.y, Is.GreaterThanOrEqualTo(minHeight), t.name + " '" + text + "' is " + (size.y * 1000).ToString("0") + " mm tall");
                Assert.That(size.x, Is.LessThanOrEqualTo(t.rectTransform.sizeDelta.x + 0.002f), t.name + " '" + text + "' fits its rect");
            }
            foreach (var label in bed.rowLabels) Readable(label, "8", 0.25f, 0.025f);   // was 0.09 (about 12 mm)
            Assert.That(bed.rowLabelOffset, Is.GreaterThanOrEqualTo(bed.rowLabels[0].rectTransform.sizeDelta.x / 2), "row numbers clear of the left wall");
            foreach (var tick in bed.fenceTicks) Readable(tick.GetComponentInChildren<TMP_Text>(true), "7", 0.2f, 0.02f);   // was 0.07
            foreach (var label in s.partLabels) Readable(label, "8 × 6 = 48", 0.25f, 0.025f);   // was 0.09
            var sign = s.transform.Find("Garden props/Sunny Plot sign");
            Assert.That(sign, Is.Not.Null);
            var signText = sign.GetComponentInChildren<TMP_Text>(true);
            var board = sign.Find("Sign board").GetComponent<MeshFilter>().sharedMesh.bounds.size;
            Assert.That(signText.fontSize, Is.GreaterThanOrEqualTo(0.2f), "sign text size");
            var signSize = signText.GetPreferredValues(signText.text);
            Assert.That(signSize.x, Is.LessThanOrEqualTo(board.x), "sign text fits the board width");
            Assert.That(signSize.y, Is.LessThanOrEqualTo(board.y), "sign text fits the board height");
        }

        [Test] public void PartLabelsSitAtTheFrontWallUnderEachPart()
        {
            float wall = GardenTableLayout.WallThickness, offset = GardenTableLayout.PartLabelOffset, deck = GardenTableLayout.DeckTop, gap = GardenTableLayout.SplitGap;
            foreach (var chapter in GardenChapter.All.Where(c => c.HasFence))
                for (int split = 1; split < chapter.Columns; split++)
                {
                    var L = GardenTableLayout.BedFor(chapter.Rows, chapter.Columns);
                    var left = GardenStation.PartLabelPosition(L, split, gap, 0, wall, offset, deck);
                    var right = GardenStation.PartLabelPosition(L, split, gap, 1, wall, offset, deck);
                    foreach (var p in new[] { left, right })
                    {
                        float fromWall = (L.Front - wall) - p.z;
                        Assert.That(fromWall, Is.GreaterThan(0.01f).And.LessThanOrEqualTo(0.05f), chapter.Id + " split " + split + ": label " + (fromWall * 100).ToString("0.0") + " cm in front of the front wall");
                    }
                    Assert.That(left.x, Is.EqualTo((L.Left + L.BoundaryX(split)) / 2).Within(1e-5f), "left label centred under the left part");
                    Assert.That(right.x, Is.EqualTo((L.BoundaryX(split) + gap + L.Right + gap) / 2).Within(1e-5f), "right label centred under the opened right part");
                    Assert.That(right.x - left.x, Is.GreaterThanOrEqualTo(0.15f), "labels about 12 cm wide never overlap");
                }
        }

        /// Root-local bounds of every mesh under t.
        static Bounds Footprint(Transform root, Transform t)
        {
            bool has = false; var result = new Bounds();
            foreach (var f in t.GetComponentsInChildren<MeshFilter>(true))
            {
                if (f.sharedMesh == null) continue;
                var b = f.sharedMesh.bounds;
                foreach (float x in new[] { b.min.x, b.max.x }) foreach (float y in new[] { b.min.y, b.max.y }) foreach (float z in new[] { b.min.z, b.max.z })
                {
                    var p = root.InverseTransformPoint(f.transform.TransformPoint(new Vector3(x, y, z)));
                    if (!has) { result = new Bounds(p, Vector3.zero); has = true; } else result.Encapsulate(p);
                }
            }
            return result;
        }

        // ---- card ----
        [Test] public void CardSitsInTheLessonInterfaceFrame()
        {
            var d = Onboarding(); var s = Station();
            var frame = d.transform.Find("Lesson interface"); var card = s.transform.Find("Garden card");
            Assert.That(card, Is.Not.Null);
            Assert.That(card.localPosition, Is.EqualTo(frame.localPosition)); Assert.That(card.localScale, Is.EqualTo(frame.localScale));
            Assert.That(card.GetComponent<RectTransform>().sizeDelta, Is.EqualTo(new Vector2(920, 470)));
            Assert.That(card.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.WorldSpace));
            Assert.That(card.GetComponentsInChildren<MonoBehaviour>(true).Any(b => b != null && b.GetType().Name == "PointableCanvas"), Is.True, "controller rays reach the card");
            foreach (var t in new[] { s.heading, s.body, s.expressionLine, s.sayHints }) { Assert.That(t, Is.Not.Null); Assert.That(t.transform.IsChildOf(card), Is.True, t.name); }
        }

        [Test] public void ButtonsLiveInOneFallbackGroupAndStayWired()
        {
            var s = Station(); var n = Find<NerdyDirector>();
            Assert.That(s.fallbackGroups.Length, Is.EqualTo(1));
            var group = s.fallbackGroups[0];
            foreach (var (button, method) in new[] { (s.startButton, "PressStart"), (s.turnButton, "PressTurn"), (s.checkButton, "PressCheck"), (s.clearButton, "PressClear"), (s.nextButton, "PressNext"), (s.backButton, "Close") })
            {
                Assert.That(button, Is.Not.Null, method);
                Assert.That(button.GetComponentInParent<CanvasGroup>(true), Is.SameAs(group), button.name);
                Assert.That(button.GetComponent<UiPressLog>(), Is.Not.Null, button.name + " logs presses");
                Assert.That(button.onClick.GetPersistentTarget(0), Is.SameAs(s), button.name);
                Assert.That(button.onClick.GetPersistentMethodName(0), Is.EqualTo(method), button.name);
            }
            Assert.That(s.backButton.onClick.GetPersistentEventCount(), Is.EqualTo(2));
            Assert.That(s.backButton.onClick.GetPersistentTarget(1), Is.SameAs(n)); Assert.That(s.backButton.onClick.GetPersistentMethodName(1), Is.EqualTo("OnLessonBack"), "Back closes the station, then returns to the lessons");
            Assert.That(s.GetComponentsInChildren<Button>(true).Length, Is.EqualTo(6));
        }

        [Test] public void CardRowsStackWithoutOverlap()
        {
            var s = Station();
            Rect R(RectTransform r) => new Rect(r.anchoredPosition - r.sizeDelta / 2, r.sizeDelta);
            var body = R(s.body.rectTransform); var expr = R(s.expressionLine.rectTransform); var hints = R(s.sayHints.rectTransform);
            var buttons = new[] { s.startButton, s.turnButton, s.checkButton, s.clearButton, s.nextButton, s.backButton };
            float rowTop = buttons.Max(b => R(b.GetComponent<RectTransform>()).yMax);
            Assert.That(body.yMin, Is.GreaterThanOrEqualTo(expr.yMax - 1f)); Assert.That(expr.yMin, Is.GreaterThanOrEqualTo(hints.yMax - 1f)); Assert.That(hints.yMin, Is.GreaterThanOrEqualTo(rowTop - 1f));
            foreach (var b in buttons)
            {
                var r = R(b.GetComponent<RectTransform>());
                Assert.That(Mathf.Abs(r.xMin) <= 460 && Mathf.Abs(r.xMax) <= 460 && r.yMin >= -235, Is.True, b.name + " inside the card");
                var label = b.GetComponentInChildren<TMP_Text>(true);
                Assert.That(label.GetPreferredValues(label.text).x, Is.LessThanOrEqualTo(label.rectTransform.rect.width + 1f), b.name + " label fits");
            }
            // Start shows only in the briefing and Turn bed only inside a chapter, so they share a slot.
            foreach (var row in new[] { new[] { s.turnButton, s.checkButton, s.clearButton, s.nextButton, s.backButton }, new[] { s.startButton, s.backButton } })
                for (int i = 0; i < row.Length; i++) for (int j = i + 1; j < row.Length; j++)
                    Assert.That(R(row[i].GetComponent<RectTransform>()).Overlaps(R(row[j].GetComponent<RectTransform>())), Is.False, row[i].name + " vs " + row[j].name);
        }

        [Test] public void CardTextFitsForEveryChapter()
        {
            var s = Station();
            float bodyW = s.body.rectTransform.rect.width, bodyH = s.body.rectTransform.rect.height;
            Assert.That(s.body.GetPreferredValues("<size=80%>" + GardenStation.BriefingBody + "</size>", bodyW, 10000).y, Is.LessThanOrEqualTo(bodyH), "briefing");
            Assert.That(s.heading.GetPreferredValues(GardenStation.BriefingHeading).x, Is.LessThanOrEqualTo(s.heading.rectTransform.rect.width));
            foreach (var chapter in GardenChapter.All)
            {
                string title = GardenStation.TitleFor(chapter);
                Assert.That(s.heading.GetPreferredValues(title).x, Is.LessThanOrEqualTo(s.heading.rectTransform.rect.width), title);
                string text = GardenStation.ComposeBody(chapter, LongestFeedback(chapter));
                Assert.That(s.body.GetPreferredValues("<size=80%>" + text + "</size>", bodyW, 10000).y, Is.LessThanOrEqualTo(bodyH), chapter.Id + ": " + text);
                var expressions = new List<string> { chapter.Expression };
                for (int k = 1; k < chapter.Columns; k++) expressions.Add(GardenChapter.SplitExpression(chapter.Rows, chapter.Columns, k));
                foreach (var e in expressions) Assert.That(s.expressionLine.GetPreferredValues(e).x, Is.LessThanOrEqualTo(s.expressionLine.rectTransform.rect.width), e);
                foreach (var complete in new[] { false, true }) foreach (var turned in new[] { false, true })
                {
                    string hints = GardenStation.SayHintsFor(chapter, complete, chapter.Number == GardenChapter.All.Count, turned);
                    Assert.That(s.sayHints.GetPreferredValues(hints).x, Is.LessThanOrEqualTo(s.sayHints.rectTransform.rect.width), hints);
                }
            }
        }

        /// The longest line the card can show under a chapter: station refusals and every model verdict reachable by
        /// planting the chapter's strips in any order, turning and moving the fence.
        static string LongestFeedback(GardenChapter chapter)
        {
            var lines = new List<string> { GardenStation.HeldReason, GardenStation.CheckedReason, GardenStation.NoTurnReason, GardenStation.FenceWhereReason,
                GardenStation.NotCompleteReason, GardenStation.BriefingReason, "The seedling strips are back in the tray.", "The bed is back the way it started.",
                "The fence goes between two columns: after column 1 up to after column 7." };
            var m = new GardenModel(); m.StartChapter(chapter.Number - 1);
            lines.Add(Verdict(m));
            if (!chapter.StartsPlanted)
            {
                var ids = m.StripIds.ToList();
                for (int r = 0; r < m.Rows; r++)
                {
                    m.ResetTable();
                    // Plant the shortest strips first so short-row feedback appears, then one empty row.
                    var order = ids.OrderBy(m.StripLength).ToList();
                    for (int k = 0; k < m.Rows && k < order.Count; k++) if (k != r) m.Plant(order[k], false, k);
                    lines.Add(Verdict(m));
                    m.Plant(order[r], false, r); lines.Add(Verdict(m));
                    foreach (var id in ids) { m.Plant(id, false, r); lines.Add(m.LastFeedback); }
                }
            }
            if (chapter.HasFence) for (int k = 0; k <= chapter.Columns; k++) { m.ResetTable(); m.SetFence(k, false); lines.Add(m.LastFeedback); lines.Add(Verdict(m)); }
            if (chapter.NeedsTurn) { m.ResetTable(); lines.Add(Verdict(m)); m.TurnBed(false); lines.Add(m.LastFeedback); lines.Add(Verdict(m)); }
            return lines.Where(l => l != null).OrderByDescending(l => l.Length).First();
        }

        static string Verdict(GardenModel m)
        {
            bool complete = m.ChapterComplete;
            m.Check();
            string text = m.LastFeedback;
            if (!complete && m.ChapterComplete) m.RestartChapter();
            return text;
        }

        // ---- behaviour on in-memory objects (no scene changes) ----
        const float Cell = GardenTableLayout.StripCell;

        internal static GardenStation MakeGarden()
        {
            var root = new GameObject("test garden");
            var bedGo = new GameObject("bed"); bedGo.transform.SetParent(root.transform, false);
            var bed = bedGo.AddComponent<GardenBedView>();
            bed.stationRoot = root.transform;
            var first = bed.LayoutFor(GardenChapter.All[0].Rows, GardenChapter.All[0].Columns);
            bed.turnable = new GameObject("turnable").transform; bed.turnable.SetParent(bedGo.transform, false); bed.turnable.localPosition = new Vector3(first.Center.x, 0, first.Center.z);
            bed.cells = Enumerable.Range(0, 64).Select(i => { var c = new GameObject("cell " + i); c.transform.SetParent(bed.turnable, false); return c; }).ToArray();
            bed.rowLabels = Enumerable.Range(0, 8).Select(i => { var t = new GameObject("row label " + (i + 1)).AddComponent<TextMeshPro>(); t.transform.SetParent(bedGo.transform, false); return (TMP_Text)t; }).ToArray();
            bed.fenceTicks = Enumerable.Range(1, 7).Select(k => { var g = new GameObject("fence spot " + k); g.transform.SetParent(bedGo.transform, false); new GameObject("number").AddComponent<TextMeshPro>().transform.SetParent(g.transform, false); return g; }).ToArray();
            var pieces = new GameObject("pieces"); pieces.transform.SetParent(root.transform, false);
            var station = root.AddComponent<GardenStation>();
            station.stationRoot = root.transform; station.bed = bed; station.pieces = pieces; station.visualRoots = new[] { root };
            station.strips = Enumerable.Range(0, 8).Select(i => MakeStrip(pieces.transform, i)).ToArray();
            station.fence = new GameObject("fence").transform; station.fence.SetParent(pieces.transform, false); station.fence.localPosition = station.fenceHome;
            station.wateringCan = new GameObject("can").transform; station.wateringCan.SetParent(root.transform, false); station.wateringCanHome = GardenTableLayout.CanHome;
            station.butterfly = new GameObject("butterfly").transform; station.butterfly.SetParent(root.transform, false); station.butterflyHome = GardenTableLayout.ButterflyHome;
            station.wateringCan.localPosition = station.wateringCanHome; station.butterfly.localPosition = station.butterflyHome;
            return station;
        }

        static GardenStation.StripView MakeStrip(Transform parent, int i)
        {
            var piece = new GameObject("strip " + (i + 1)).transform; piece.SetParent(parent, false);
            var view = new GardenStation.StripView { piece = piece, trayFront = GardenTableLayout.TrayFront(i) };
            view.bodyLeft = new GameObject("body left").AddComponent<MeshFilter>(); view.bodyLeft.transform.SetParent(piece, false);
            view.bodyRight = new GameObject("body right").AddComponent<MeshFilter>(); view.bodyRight.transform.SetParent(piece, false);
            view.seedlings = new Transform[8]; view.sprouts = new Transform[8]; view.blooms = new Transform[8];
            for (int k = 0; k < 8; k++)
            {
                var s = new GameObject("seedling " + k).transform; s.SetParent(piece, false);
                var sprout = new GameObject("Sprout").transform; sprout.SetParent(s, false);
                var bloom = new GameObject("Bloom").transform; bloom.SetParent(s, false);
                foreach (var part in new[] { "Lettuce head", "Carrot top", "Carrot root", "Sunflower petals", "Bean pods" }) new GameObject(part).transform.SetParent(bloom, false);
                bloom.gameObject.SetActive(false); bloom.localScale = Vector3.zero;
                view.seedlings[k] = s; view.sprouts[k] = sprout; view.blooms[k] = bloom;
            }
            return view;
        }

        /// Open, then Next through the briefing and the concept intro (first time only) to chapter 1.
        internal static void StartChapterOne(GardenStation s)
        {
            s.Open();
            for (int i = 0; i < 10 && !s.ChapterActive; i++) Assert.That(s.Advance().Ok, Is.True, "advance to chapter 1");
            Assert.That(s.Chapter.Number, Is.EqualTo(1));
        }

        static int ActiveStrips(GardenStation s) => s.strips.Count(v => v.piece.gameObject.activeSelf);

        /// Station-local centre of seedling k of a strip lying unturned (planted), with the piece scale applied.
        static Vector3 SeedlingAt(GardenStation.StripView v, int k) => v.piece.localPosition + v.piece.localScale.x * v.seedlings[k].localPosition;

        [Test] public void OpenShowsTheBriefingAndAdvanceEntersChapterOne()
        {
            var s = MakeGarden();
            try
            {
                s.Open();
                Assert.That(s.IsOpen, Is.True); Assert.That(s.InBriefing, Is.True); Assert.That(s.ChapterActive, Is.False);
                Assert.That(s.Chapter.Number, Is.EqualTo(0), "no chapter facts during the briefing");
                Assert.That(s.CurrentStep().Id, Is.EqualTo(GardenSteps.BriefingId));
                Assert.That(s.ToolsNow, Does.Contain("advance_step").And.Not.Contain("garden_check_bed"));
                Assert.That(s.pieces.activeSelf, Is.False, "nothing to grab during the briefing");
                var early = s.Check(); Assert.That(early.Ok, Is.False); Assert.That(early.Reason, Is.EqualTo(GardenStation.BriefingReason));

                for (int i = 0; i < ConceptIntros.Garden.Count; i++) Assert.That(s.Advance().Ok, Is.True, "concept intro step " + (i + 1));
                var start = s.Advance();
                Assert.That(start.Ok, Is.True, start.Reason);
                Assert.That(start.Reason, Is.EqualTo("Chapter 1 · First rows. " + GardenChapter.All[0].Story + " " + GardenChapter.All[0].Task), "after the intro chapter 1 starts exactly as before");
                Assert.That(s.ChapterActive, Is.True); Assert.That(s.Chapter.Number, Is.EqualTo(1));
                Assert.That(s.CurrentStep().Id, Is.EqualTo(GardenSteps.StepId(GardenChapter.All[0])));
                Assert.That(s.pieces.activeSelf, Is.True);
                Assert.That(ActiveStrips(s), Is.EqualTo(5), "strips 4,4,4,3,3 in the tray");
                Assert.That(s.bed.ActiveCellCount(), Is.EqualTo(12), "3 × 4 bed");
                Assert.That(s.fence.gameObject.activeSelf, Is.False, "no fence in chapter 1");
                Assert.That(s.ToolsNow, Does.Contain("garden_check_bed").And.Contain("garden_clear_bed").And.Not.Contain("garden_turn_bed").And.Not.Contain("garden_split_bed").And.Not.Contain("next_chapter"));
                var trayRotation = Quaternion.Euler(0, GardenTableLayout.TrayYaw, 0);
                foreach (var v in s.strips.Take(5))
                {
                    Assert.That(v.piece.localPosition, Is.EqualTo(GardenStation.TrayPosition(v, s.Model.StripLength(v.id), Cell)), v.id + " waits in the tray");
                    Assert.That(Quaternion.Angle(v.piece.localRotation, trayRotation), Is.LessThan(0.01f), v.id + " lies front to back in the tray");
                    Assert.That(v.piece.localScale, Is.EqualTo(Vector3.one), v.id + " full size");
                }
            }
            finally { Object.DestroyImmediate(s.gameObject); }
        }

        [Test] public void DroppingOverARowPlantsAndElsewhereReturnsToTheTray()
        {
            var s = MakeGarden();
            try
            {
                StartChapterOne(s);
                var L = s.BedLayout;
                Assert.That(s.DropAt("strip-1", L.StripPosition(1, 4) + new Vector3(0.02f, 0.03f, 0.015f)), Is.True, "released a little off the row centre");
                Assert.That(s.Model.RowOf("strip-1"), Is.EqualTo(1));
                Assert.That(s.ViewOf("strip-1").piece.localPosition, Is.EqualTo(L.StripPosition(1, 4)), "snapped into row 2 from column 1");
                Assert.That(s.ViewOf("strip-1").piece.localRotation, Is.EqualTo(Quaternion.identity), "planted strips lie along their row");

                Assert.That(s.DropAt("strip-2", L.StripPosition(1, 4)), Is.False, "row 2 is taken");
                Assert.That(s.Feedback, Does.Contain("Row 2"));
                Assert.That(s.ViewOf("strip-2").piece.localPosition, Is.EqualTo(GardenStation.TrayPosition(s.ViewOf("strip-2"), 4, Cell)), "refused strip returns to the tray");

                Assert.That(s.DropAt("strip-1", new Vector3(0.45f, 0.03f, -0.3f)), Is.False, "off the bed");
                Assert.That(s.Model.RowOf("strip-1"), Is.EqualTo(-1), "taken back out of the bed");
                Assert.That(s.ViewOf("strip-1").piece.localPosition, Is.EqualTo(GardenStation.TrayPosition(s.ViewOf("strip-1"), 4, Cell)));

                Assert.That(s.DropAt("strip-4", L.StripPosition(0, 3)), Is.True, "a short strip may be planted");
                Assert.That(s.ViewOf("strip-4").seedlings.Count(t => t.gameObject.activeSelf), Is.EqualTo(3));
                var wrong = s.Check();
                Assert.That(wrong.Ok, Is.False); Assert.That(s.PayoffShown, Is.False);
                Assert.That(s.DropAt("strip-4", L.StripPosition(2, 3)), Is.True, "wrong arrangements stay editable after a check");
                Assert.That(s.Model.RowOf("strip-4"), Is.EqualTo(2));

                var clear = s.ResetTable();
                Assert.That(clear.Ok, Is.True); Assert.That(s.Model.PlantCount, Is.EqualTo(0));
                Assert.That(s.ViewOf("strip-4").piece.localPosition, Is.EqualTo(GardenStation.TrayPosition(s.ViewOf("strip-4"), 3, Cell)));
            }
            finally { Object.DestroyImmediate(s.gameObject); }
        }

        [Test] public void AcceptedCheckGrowsTheRowsAndRestartRestores()
        {
            var s = MakeGarden();
            try
            {
                StartChapterOne(s);
                var L = s.BedLayout;
                for (int r = 0; r < 3; r++) Assert.That(s.DropAt("strip-" + (r + 1), L.StripPosition(r, 4)), Is.True);
                var check = s.Check();
                Assert.That(check.Ok, Is.True, check.Reason);
                Assert.That(check.Reason, Does.Contain("3 × 4 = 12"));
                Assert.That(s.ExpressionText, Is.EqualTo("3 × 4 = 12"));
                Assert.That(s.PayoffShown, Is.True);
                foreach (var id in new[] { "strip-1", "strip-2", "strip-3" })
                {
                    var v = s.ViewOf(id);
                    for (int k = 0; k < 4; k++)
                    {
                        Assert.That(v.blooms[k].gameObject.activeSelf, Is.True, id + " seedling " + k + " grew");
                        Assert.That(v.blooms[k].localScale, Is.EqualTo(Vector3.one));
                        Assert.That(v.blooms[k].Cast<Transform>().Where(c => c.gameObject.activeSelf).All(c => c.name.StartsWith("Lettuce ")), Is.True, "chapter 1 grows lettuce");
                    }
                }
                Assert.That(s.ViewOf("strip-4").blooms.Any(b => b.gameObject.activeSelf), Is.False, "tray strips do not grow");
                Assert.That(s.butterfly.localPosition, Is.Not.EqualTo(s.butterflyHome), "the butterfly landed on the bed");
                Assert.That(s.butterfly.localPosition.y, Is.GreaterThan(L.Center.y + 0.1f), "the butterfly lands on top of the full-size plants");
                Assert.That(s.wateringCan.localPosition, Is.EqualTo(s.wateringCanHome), "the watering can is back home");
                Assert.That(s.DropAt("strip-1", new Vector3(0.45f, 0.03f, -0.3f)), Is.False, "an accepted bed does not change");
                Assert.That(s.Model.RowOf("strip-1"), Is.EqualTo(0));
                Assert.That(s.ResetTable().Ok, Is.False);
                Assert.That(s.ToolsNow, Does.Contain("next_chapter").And.Not.Contain("garden_check_bed"));

                var restart = s.RestartChapter();
                Assert.That(restart.Ok, Is.True);
                Assert.That(s.PayoffShown, Is.False);
                Assert.That(s.strips.SelectMany(v => v.blooms).Any(b => b.gameObject.activeSelf), Is.False, "blooms reset");
                Assert.That(s.butterfly.localPosition, Is.EqualTo(s.butterflyHome));
                Assert.That(s.ViewOf("strip-1").piece.localPosition, Is.EqualTo(GardenStation.TrayPosition(s.ViewOf("strip-1"), 4, Cell)));

                for (int r = 0; r < 3; r++) s.DropAt("strip-" + (r + 1), L.StripPosition(r, 4));
                Assert.That(s.Check().Ok, Is.True);
                var next = s.NextChapter();
                Assert.That(next.Ok, Is.True); Assert.That(s.Chapter.Number, Is.EqualTo(2)); Assert.That(s.PayoffShown, Is.False);
                Assert.That(ActiveStrips(s), Is.EqualTo(6)); Assert.That(s.bed.ActiveCellCount(), Is.EqualTo(20));
            }
            finally { Object.DestroyImmediate(s.gameObject); }
        }

        [Test] public void TurningTheBedSwapsTheGridAndRebindsTheStrips()
        {
            var s = MakeGarden();
            try
            {
                StartChapterOne(s); s.JumpToChapter(2);
                Assert.That(s.Chapter.Id, Is.EqualTo("turn_the_bed"));
                var before = s.BedLayout;
                Assert.That(ActiveStrips(s), Is.EqualTo(3)); Assert.That(s.bed.ActiveCellCount(), Is.EqualTo(12));
                for (int r = 0; r < 3; r++) Assert.That(s.ViewOf("strip-" + (r + 1)).piece.localPosition, Is.EqualTo(before.StripPosition(r, 4)), "planted from the start");
                Assert.That(s.DropAt("strip-1", new Vector3(0.45f, 0.03f, -0.3f)), Is.False, "planted-chapter strips are not tray pieces");
                Assert.That(s.Model.RowOf("strip-1"), Is.EqualTo(0));
                Assert.That(s.ToolsNow, Does.Contain("garden_turn_bed"));
                Assert.That(s.Check().Ok, Is.False, "needs a turn first");

                // Every seedling position before the turn, turned a quarter turn about the bed centre.
                // Seedling centres sit on half-cell marks, so compare within a tolerance instead of rounding to whole
                // millimetres, which splits equal points on float noise.
                var turnedSeedlings = new List<Vector2>();
                foreach (var v in s.strips.Where(v => v.piece.gameObject.activeSelf))
                    for (int k = 0; k < 4; k++)
                    {
                        var p = before.TurnPoint(SeedlingAt(v, k), 90f);
                        turnedSeedlings.Add(new Vector2(p.x, p.z));
                    }

                bool ok = s.TryLessonTool("garden_turn_bed", new JObject(), out var turn);
                Assert.That(ok, Is.True); Assert.That(turn.Ok, Is.True, turn.Reason);
                Assert.That(s.Model.Rows, Is.EqualTo(4)); Assert.That(s.Model.Columns, Is.EqualTo(3));
                Assert.That(s.bed.Layout.Rows, Is.EqualTo(4)); Assert.That(s.bed.ActiveCellCount(), Is.EqualTo(12));
                Assert.That(ActiveStrips(s), Is.EqualTo(4), "regenerated as 4 rows of 3");
                var after = s.BedLayout;
                Assert.That(after.Cell, Is.EqualTo(before.Cell)); Assert.That(after.Center, Is.EqualTo(before.Center), "the turned bed keeps its cell and centre");
                var planted = new List<Vector2>();
                for (int r = 0; r < 4; r++)
                {
                    var v = s.ViewOf("strip-" + (r + 1));
                    Assert.That(v.piece.localPosition, Is.EqualTo(after.StripPosition(r, 3)));
                    Assert.That(v.seedlings.Count(t => t.gameObject.activeSelf), Is.EqualTo(3));
                    for (int k = 0; k < 3; k++) { var p = SeedlingAt(v, k); planted.Add(new Vector2(p.x, p.z)); }
                }
                Assert.That(turnedSeedlings.Count, Is.EqualTo(12)); Assert.That(planted.Count, Is.EqualTo(12));
                foreach (var t in turnedSeedlings)
                    Assert.That(planted.Count(q => Vector2.Distance(q, t) < 1e-4f), Is.EqualTo(1), "turning moves no seedling: a turned seedling at " + t.x.ToString("0.0000") + ", " + t.y.ToString("0.0000") + " is planted exactly once after the turn");
                Assert.That(s.ToolsNow, Does.Not.Contain("garden_turn_bed"));
                Assert.That(s.Check().Ok, Is.True);
                Assert.That(s.ExpressionText, Is.EqualTo("3 × 4 = 4 × 3 = 12"));

                s.RestartChapter();
                Assert.That(s.Model.Rows, Is.EqualTo(3)); Assert.That(ActiveStrips(s), Is.EqualTo(3)); Assert.That(s.bed.Layout.Rows, Is.EqualTo(3));
            }
            finally { Object.DestroyImmediate(s.gameObject); }
        }

        [Test] public void TheFenceSnapsBetweenColumnsAndOpensOnAnAcceptedCheck()
        {
            var s = MakeGarden();
            try
            {
                StartChapterOne(s); s.JumpToChapter(3);   // 7 rows of 6, fence after column 5
                var L = s.BedLayout;
                float scale = L.Cell / s.stripCell;
                Assert.That(ActiveStrips(s), Is.EqualTo(7)); Assert.That(s.bed.ActiveCellCount(), Is.EqualTo(42));
                Assert.That(s.fence.gameObject.activeSelf, Is.True); Assert.That(s.fence.localPosition, Is.EqualTo(s.fenceHome));
                var missing = s.Check(); Assert.That(missing.Ok, Is.False); Assert.That(missing.Reason, Is.EqualTo(GardenModel.NeedsFenceFeedback));

                Assert.That(s.DropFenceAt(new Vector3(L.BoundaryX(4) + 0.012f, 0.05f, L.Center.z + 0.05f)), Is.True, "snaps to the nearest line");
                Assert.That(s.Model.FenceColumn, Is.EqualTo(4));
                Assert.That(s.fence.localPosition.x, Is.EqualTo(L.BoundaryX(4)).Within(1e-5f));
                Assert.That(s.DropFenceAt(new Vector3(0.5f, 0.05f, -0.3f)), Is.False, "off the bed");
                Assert.That(s.Model.FenceColumn, Is.EqualTo(4)); Assert.That(s.fence.localPosition.x, Is.EqualTo(L.BoundaryX(4)).Within(1e-5f), "returns to its line");
                var wrongSpot = s.Check(); Assert.That(wrongSpot.Ok, Is.False); Assert.That(wrongSpot.Reason, Does.Contain("column 5"));

                Assert.That(s.TryLessonTool("garden_split_bed", new JObject { ["columns"] = 9 }, out var tooFar), Is.True); Assert.That(tooFar.Ok, Is.False);
                Assert.That(s.TryLessonTool("garden_split_bed", new JObject(), out var nowhere), Is.True); Assert.That(nowhere.Reason, Is.EqualTo(GardenStation.FenceWhereReason));
                Assert.That(s.TryLessonTool("cafe_deal_round", new JObject(), out _), Is.False, "not a garden tool");
                Assert.That(s.TryLessonTool("garden_split_bed", new JObject { ["columns"] = 5 }, out var split), Is.True); Assert.That(split.Ok, Is.True, split.Reason);
                Assert.That(s.Model.FenceColumn, Is.EqualTo(5));

                Assert.That(s.TryLessonTool("garden_check_bed", null, out var check), Is.True); Assert.That(check.Ok, Is.True, check.Reason);
                Assert.That(s.ExpressionText, Is.EqualTo("7 × 6 = 7 × 5 + 7 × 1 = 42"));
                var v = s.ViewOf("strip-1");
                Assert.That(v.seedlings[4].localPosition.x, Is.EqualTo(GardenBedLayout.SeedlingX(s.stripCell, 4, 6)).Within(1e-5f), "left part stays");
                Assert.That(v.seedlings[5].localPosition.x, Is.EqualTo(GardenBedLayout.SeedlingX(s.stripCell, 5, 6) + s.splitGap / scale).Within(1e-5f), "right part slides apart");
                Assert.That(SeedlingAt(v, 5).x - SeedlingAt(v, 4).x, Is.EqualTo(L.Cell + s.splitGap).Within(1e-5f), "on the table the seedlings open by the split gap");
                Assert.That(v.bodyRight.gameObject.activeSelf, Is.True);
                Assert.That(s.bed.SplitColumn, Is.EqualTo(5)); Assert.That(s.bed.SplitGap, Is.EqualTo(s.splitGap));
                Assert.That(s.bed.Cell(0, 5).transform.localPosition.x - s.bed.Cell(0, 4).transform.localPosition.x, Is.EqualTo(L.Cell + s.splitGap).Within(1e-5f), "the bed opens at the fence");
                Assert.That(v.blooms[0].Cast<Transform>().Where(c => c.gameObject.activeSelf).All(c => c.name.StartsWith("Carrot ")), Is.True);
                Assert.That(v.blooms[5].Cast<Transform>().Where(c => c.gameObject.activeSelf).All(c => c.name.StartsWith("Sunflower ")), Is.True, "a different crop on each side of the fence");

                s.RestartChapter();
                Assert.That(s.Model.FenceColumn, Is.EqualTo(0)); Assert.That(s.fence.localPosition, Is.EqualTo(s.fenceHome));
                Assert.That(s.ViewOf("strip-1").bodyRight.gameObject.activeSelf, Is.False);
                Assert.That(s.ViewOf("strip-1").seedlings[5].localPosition.x, Is.EqualTo(GardenBedLayout.SeedlingX(s.stripCell, 5, 6)).Within(1e-5f), "restart closes the split");
            }
            finally { Object.DestroyImmediate(s.gameObject); }
        }

        [Test] public void HeldPiecesRefuseActionsAndCloseRestoresTheTray()
        {
            var s = MakeGarden();
            try
            {
                StartChapterOne(s);
                var L = s.BedLayout;
                Assert.That(s.DropAt("strip-1", L.StripPosition(0, 4)), Is.True);
                s.ViewOf("strip-2").held = true;
                Assert.That(s.AnyHeld, Is.True);
                foreach (var r in new[] { s.Check(), s.ResetTable(), s.RestartChapter(), s.NextChapter(), s.Advance() })
                {
                    Assert.That(r.Ok, Is.False); Assert.That(r.Reason, Is.EqualTo(GardenStation.HeldReason));
                }
                s.ViewOf("strip-2").held = false;

                s.Close();
                Assert.That(s.IsOpen, Is.False); Assert.That(s.gameObject.activeSelf, Is.False, "visual root hidden");
                Assert.That(s.AnyHeld, Is.False); Assert.That(s.Chapter.Number, Is.EqualTo(0));
                Assert.That(s.Check().Reason, Is.EqualTo(GardenStation.ClosedReason));
                Assert.That(s.Model.PlantCount, Is.EqualTo(0), "strips back in the tray");

                s.Open();
                Assert.That(s.gameObject.activeSelf, Is.True); Assert.That(s.InBriefing, Is.True);
                s.Advance();
                Assert.That(s.Chapter.Number, Is.EqualTo(1), "the intro ran once; the second visit goes straight to the chapter");
                for (int r = 0; r < 3; r++) s.DropAt("strip-" + (r + 1), L.StripPosition(r, 4));
                Assert.That(s.Check().Ok, Is.True);
                s.Close();
                Assert.That(s.PayoffShown, Is.False); Assert.That(s.butterfly.localPosition, Is.EqualTo(s.butterflyHome), "payoff restored on close");
                s.Open(); s.Advance();
                Assert.That(s.Chapter.Number, Is.EqualTo(2), "returning after an accepted bed continues with the next chapter");
            }
            finally { Object.DestroyImmediate(s.gameObject); }
        }

        [Test] public void BedGridSizesFollowEveryChapter()
        {
            var s = MakeGarden();
            try
            {
                StartChapterOne(s);
                for (int i = 0; i < GardenChapter.All.Count; i++)
                {
                    s.JumpToChapter(i);
                    var c = GardenChapter.All[i];
                    var L = s.BedLayout;
                    Assert.That(s.bed.ActiveCellCount(), Is.EqualTo(c.Rows * c.Columns), c.Id);
                    for (int r = 0; r < 8; r++) for (int col = 0; col < 8; col++)
                    {
                        var cell = s.bed.Cell(r, col);
                        Assert.That(cell.activeSelf, Is.EqualTo(r < c.Rows && col < c.Columns), c.Id + " cell " + r + "," + col);
                        if (cell.activeSelf)
                        {
                            var p = s.bed.turnable.localPosition + cell.transform.localPosition;
                            Assert.That(p.x, Is.EqualTo(L.ColumnX(col)).Within(1e-5f)); Assert.That(p.z, Is.EqualTo(L.RowZ(r)).Within(1e-5f));
                            Assert.That(cell.transform.localScale.x, Is.EqualTo(L.Cell / s.bed.meshCell).Within(1e-5f), c.Id + " soil cell scaled to the bed cell");
                        }
                    }
                    Assert.That(ActiveStrips(s), Is.EqualTo(c.StartsPlanted ? c.Rows : c.StripLengths.Length), c.Id);
                    Assert.That(s.fence.gameObject.activeSelf, Is.EqualTo(c.HasFence), c.Id);
                    // Pieces follow the bed: full size on tray chapters, the bed's cell on the 7 × 6 and 8 × 7 beds.
                    float scale = L.Cell / s.stripCell;
                    foreach (var v in s.strips.Where(v => v.piece.gameObject.activeSelf))
                    {
                        Assert.That(v.piece.localScale.x, Is.EqualTo(scale).Within(1e-5f), c.Id + " " + v.id + " piece scale");
                        if (s.Model.RowOf(v.id) >= 0)
                        {
                            int len = s.Model.StripLength(v.id);
                            Assert.That(SeedlingAt(v, len - 1).x - SeedlingAt(v, 0).x, Is.EqualTo((len - 1) * L.Cell).Within(1e-5f), c.Id + " seedlings one cell apart");
                            Assert.That(SeedlingAt(v, 0).x, Is.EqualTo(L.ColumnX(0)).Within(1e-5f), c.Id + " first seedling over column 1");
                        }
                    }
                    if (i >= 3) Assert.That(scale, Is.LessThan(1f), c.Id + " is too big for full-size pieces"); else Assert.That(scale, Is.EqualTo(1f), c.Id + " full size");
                }
            }
            finally { Object.DestroyImmediate(s.gameObject); }
        }

        // ---- pure rules ----
        [Test] public void LayoutMapsReleasesToRowsAndFenceLines()
        {
            var L = GardenTableLayout.BedFor(3, 4); var Center = L.Center;
            Assert.That(L.RowAt(L.StripPosition(0, 4)), Is.EqualTo(0)); Assert.That(L.RowAt(L.StripPosition(2, 3)), Is.EqualTo(2));
            Assert.That(L.RowAt(new Vector3(L.Right + Cell * 0.9f, Center.y, L.RowZ(1))), Is.EqualTo(1), "a long strip held by one end");
            Assert.That(L.RowAt(new Vector3(L.Right + Cell * 1.5f, Center.y, L.RowZ(1))), Is.EqualTo(-1));
            Assert.That(L.RowAt(new Vector3(0, Center.y, L.Back + Cell)), Is.EqualTo(-1)); Assert.That(L.RowAt(new Vector3(0, Center.y + 0.3f, L.RowZ(0))), Is.EqualTo(-1));
            Assert.That(L.FenceAt(new Vector3(L.BoundaryX(0) + 0.001f, Center.y, Center.z)), Is.EqualTo(-1), "the bed edge is not a split");
            Assert.That(L.FenceAt(new Vector3(L.BoundaryX(3) - 0.01f, Center.y, Center.z)), Is.EqualTo(3));
            var turned = L.Turned();
            Assert.That(turned.Rows, Is.EqualTo(4)); Assert.That(turned.Columns, Is.EqualTo(3));
            var p = L.TurnPoint(L.CellCenter(0, 0), 90f);
            bool onGrid = Enumerable.Range(0, 4).Any(r => Enumerable.Range(0, 3).Any(c => Vector3.Distance(turned.CellCenter(r, c), p) < 1e-5f));
            Assert.That(onGrid, Is.True, "a turned cell lands on a cell of the turned grid");

            // Fit: full cells while the longer side fits the depth, the front wall on the front line; the longer side also
            // sets the centre, so a bed and its turn share both.
            var small = GardenBedLayout.Fit(0f, 0.05f, -0.3f, 0.5f, 0.09f, 3, 4);
            Assert.That(small.Cell, Is.EqualTo(0.09f).Within(1e-6f)); Assert.That(small.Center.z, Is.EqualTo(-0.3f + 4 * 0.09f * 0.5f).Within(1e-6f));
            Assert.That(small.Front, Is.EqualTo(-0.3f + 0.09f * 0.5f).Within(1e-6f), "a 3-row bed centred like its 4-row turn");
            var big = GardenBedLayout.Fit(0f, 0.05f, -0.3f, 0.5f, 0.09f, 8, 7);
            Assert.That(big.Cell, Is.EqualTo(0.0625f).Within(1e-6f)); Assert.That(big.Front, Is.EqualTo(-0.3f).Within(1e-6f)); Assert.That(big.Back, Is.EqualTo(0.2f).Within(1e-6f));
            Assert.That(big.Center.y, Is.EqualTo(0.05f));
            var turnedFit = GardenBedLayout.Fit(0f, 0.05f, -0.3f, 0.5f, 0.09f, 4, 3);
            Assert.That(turnedFit.Center, Is.EqualTo(small.Center)); Assert.That(turnedFit.Cell, Is.EqualTo(small.Cell));
        }

        [Test] public void CopyRulesAndCrops()
        {
            var ch = GardenChapter.All;
            Assert.That(GardenStation.TitleFor(ch[2]), Is.EqualTo("Chapter 3 · Turn the bed"));
            foreach (var c in ch) Assert.That(GardenStation.ComposeBody(c, "Row 3 is still empty."), Does.StartWith(c.Story + "\n" + c.Task).And.EndWith("Row 3 is still empty."));
            Assert.That(GardenStation.SayHintsFor(ch[2], false, false, false), Does.Contain("turn the bed").And.Contain("check the bed"));
            Assert.That(GardenStation.SayHintsFor(ch[2], false, false, true), Does.Not.Contain("turn the bed"));
            Assert.That(GardenStation.SayHintsFor(ch[3], false, false, false), Does.Contain("split it at five"));
            Assert.That(GardenStation.SayHintsFor(ch[0], false, false, false), Does.Not.Contain("split").And.Not.Contain("turn"));
            Assert.That(GardenStation.SayHintsFor(ch[0], true, false, false), Does.Contain("next chapter").And.Not.Contain("check the bed"));
            Assert.That(GardenStation.SayHintsFor(ch[4], true, true, false), Does.Not.Contain("next chapter").And.Contain("back to the lessons"));
            Assert.That(GardenStation.BloomKindFor(ch[0], 0, 0, 0), Is.EqualTo(GardenStation.Lettuce), ch[0].Accepted);
            Assert.That(GardenStation.BloomKindFor(ch[1], 2, 3, 0), Is.EqualTo(GardenStation.Carrot), ch[1].Accepted);
            Assert.That(new[] { GardenStation.BloomKindFor(ch[4], 0, 0, 3), GardenStation.BloomKindFor(ch[4], 0, 5, 3) }, Is.EqualTo(new[] { GardenStation.Sunflower, GardenStation.Bean }), ch[4].Accepted);
            foreach (var text in new[] { GardenStation.BriefingBody, GardenStation.HeldReason, GardenStation.CheckedReason, GardenStation.NoTurnReason })
            {
                // Whole words only: "start" is fine, "star" or "stars" is not.
                foreach (var banned in new[] { "stars?", "scores?", "great job", "points?", "well done", "awesome", "perfect" })
                    Assert.That(System.Text.RegularExpressions.Regex.IsMatch(text, @"\b" + banned + @"\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase), Is.False, "no praise or rewards (" + banned + "): " + text);
                Assert.That(text.All(ch2 => ch2 < 128 || "·—×÷".IndexOf(ch2) >= 0), Is.True, "ASCII copy: " + text);
            }
        }
    }
}
