using System.Collections.Generic;
using System.Linq;
using Airlift.Lessons;
using Airlift.Lessons.Cafe;
using Airlift.Onboarding;
using Airlift.Presentation;
using Airlift.Presentation.Cafe;
using Airlift.Welcome;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Airlift.Tests
{
    /// CC-CF-02 / CC-CF-03: the Corner Café workbench as built by AgentScripts/BuildCafeWorkbench.cs (read-only against
    /// CargoCrew), the pure café layout, and CafeStation behaviour on in-memory objects.
    public class CafeWorkbenchWiringTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }
        static T Find<T>() where T : Component => SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).FirstOrDefault();
        static OnboardingDirector Onboarding() => Find<OnboardingDirector>();
        static Transform CafeRoot() { var d = Onboarding(); foreach (Transform c in d.transform) if (c.name == "Cafe workbench") return c; return null; }
        static CafeStation Station() => CafeRoot()?.GetComponent<CafeStation>();
        static Transform Card(CafeStation s) { foreach (Transform c in s.transform) if (c.name == "Cafe card") return c; return null; }
        static Button[] RowButtons(CafeStation s) => new[] { s.startButton, s.dealButton, s.checkButton, s.clearButton, s.nextButton, s.backButton };
        static int MaxContainers(CafeTargetKind kind) => CafeChapter.All.SelectMany(c => c.Stages).Where(st => st.Kind == kind).Max(st => st.Containers);

        // ---- scene: root, station, card ----
        [Test] public void CafeRootIsAnInactiveStationUnderTheWorldLockedRoot()
        {
            var d = Onboarding(); var root = CafeRoot();
            Assert.That(root, Is.Not.Null, "run AgentScripts/BuildCafeWorkbench.cs");
            Assert.That(d.gameObject.name, Is.EqualTo("Onboarding workbench - world locked"));
            Assert.That(root.gameObject.activeSelf, Is.False, "hidden until the café lesson opens");
            Assert.That(root.localPosition, Is.EqualTo(Vector3.zero)); Assert.That(root.localRotation, Is.EqualTo(Quaternion.identity)); Assert.That(root.localScale, Is.EqualTo(Vector3.one));
            var s = Station(); Assert.That(s, Is.Not.Null);
            Assert.That(s, Is.InstanceOf<LessonStation>());
            Assert.That(s.cardId, Is.EqualTo("neighborhood_cafe_division"));
            Assert.That(s.visualRoots, Is.EqualTo(new[] { root.gameObject }));
            Assert.That(s.space, Is.EqualTo(root));
            Assert.That(s.theme, Is.Not.Null); Assert.That(s.theme.deckMaterial, Is.Not.Null); Assert.That(s.theme.name, Is.EqualTo("CafeTheme"));
            Assert.That(s.payoff, Is.Not.Null); Assert.That(s.payoff.space, Is.EqualTo(root));
            Assert.That(s.Title, Is.EqualTo("Neighborhood Café")); Assert.That(s.StoryName, Is.EqualTo("Corner Café"));
            Assert.That(s.ToolNames, Is.EquivalentTo(new[] { "cafe_deal_round", "cafe_check_order", "cafe_clear_table" }));
        }

        [Test] public void CafeCardUsesTheLessonInterfaceFrame()
        {
            var d = Onboarding(); var s = Station(); var card = Card(s); var frame = d.transform.Find("Lesson interface");
            Assert.That(card, Is.Not.Null);
            Assert.That(card.localPosition, Is.EqualTo(frame.localPosition));
            Assert.That(card.localScale.x, Is.EqualTo(0.001f).Within(1e-6f));
            Assert.That(card.GetComponent<RectTransform>().sizeDelta, Is.EqualTo(new Vector2(920, 470)));
            Assert.That(card.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.WorldSpace));
            Assert.That(card.GetComponentInChildren<Oculus.Interaction.PointableCanvas>(true), Is.Not.Null, "controller rays can press the card");
            foreach (var t in new[] { s.heading, s.body, s.expressionLine, s.sayHints })
            {
                Assert.That(t, Is.Not.Null); Assert.That(t.transform.IsChildOf(card), Is.True, t.name);
            }
            Assert.That(s.body.transform.IsChildOf(d.transform.Find("Lesson interface")), Is.False, "the café has its own card; Cargo's card stays Cargo's");
        }

        [Test] public void CafeTextsUseAirliftFontsAndMeshesAreRounded()
        {
            var d = Onboarding(); var style = d.GetComponent<TypographyBindings>().style; var root = CafeRoot();
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
                Assert.That(t.font == style.headingFont || t.font == style.bodyFont, Is.True, t.name + " uses an AirliftStyle font");
            foreach (var f in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (f.GetComponent<TMP_Text>() != null) continue;
                Assert.That(f.sharedMesh, Is.Not.Null, f.name);
                Assert.That(f.sharedMesh.name, Is.Not.EqualTo("Cube"), f.name + " is a raw cube");
            }
            var s = Station();
            foreach (var c in root.GetComponentsInChildren<Collider>(true))
                Assert.That(s.items.Any(v => c.transform.IsChildOf(v.piece)), Is.True, c.name + ": only pastries have colliders, props never block rays");
        }

        [Test] public void PropsExistAndStayUnderTheCardSightline()
        {
            var d = Onboarding(); var root = CafeRoot(); var card = Card(Station());
            foreach (var name in new[] { "Cafe deck", "Coffee counter", "Espresso machine", "Awning", "Menu board", "Guest table", "Guest chair 1", "Delivery bike", "Pastry tray" })
                Assert.That(root.GetComponentsInChildren<Transform>(true).Any(t => t.name == name), Is.True, name);
            var deck = root.GetComponentsInChildren<Transform>(true).First(t => t.name == "Cafe deck").GetComponent<MeshFilter>().sharedMesh.bounds.size;
            Assert.That(deck, Is.EqualTo(d.transform.Find("Workbench").GetComponent<MeshFilter>().sharedMesh.bounds.size), "Cargo workbench footprint");
            foreach (var f in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (f.transform.IsChildOf(card) || f.GetComponent<TMP_Text>() != null || f.sharedMesh == null) continue;
                var b = f.sharedMesh.bounds; float top = float.MinValue; float maxX = 0f, maxZ = 0f;
                foreach (float x in new[] { b.min.x, b.max.x }) foreach (float y in new[] { b.min.y, b.max.y }) foreach (float z in new[] { b.min.z, b.max.z })
                {
                    var p = d.transform.InverseTransformPoint(f.transform.TransformPoint(new Vector3(x, y, z)));
                    top = Mathf.Max(top, p.y); maxX = Mathf.Max(maxX, Mathf.Abs(p.x)); maxZ = Mathf.Max(maxZ, Mathf.Abs(p.z));
                }
                Assert.That(top, Is.LessThan(CafeLayout.SightlineHeight), Path(f.transform, root) + " stays below the sightline to the lesson card");
                Assert.That(maxX, Is.LessThanOrEqualTo(CafeLayout.DeckHalfX + 0.001f), Path(f.transform, root) + " on the deck (x)");
                Assert.That(maxZ, Is.LessThanOrEqualTo(CafeLayout.DeckHalfZ + 0.005f), Path(f.transform, root) + " on the deck (z)");
            }
            foreach (var t in root.GetComponentsInChildren<TextMeshPro>(true))
                Assert.That(d.transform.InverseTransformPoint(t.transform.position).y, Is.LessThan(CafeLayout.SightlineHeight - 0.02f), Path(t.transform, root));
        }

        static string Path(Transform t, Transform root) { string p = t.name; for (var q = t.parent; q != null && q != root; q = q.parent) p = q.name + "/" + p; return p; }

        [Test] public void DropTargetsAndPastryPoolCoverEveryChapter()
        {
            var d = Onboarding(); var s = Station(); var handle = d.GetComponent<TableHandle>();
            Assert.That(s.plates.Length, Is.GreaterThanOrEqualTo(MaxContainers(CafeTargetKind.Plates)));
            Assert.That(s.boxes.Length, Is.GreaterThanOrEqualTo(MaxContainers(CafeTargetKind.Boxes)));
            Assert.That(s.plateLabels.Length, Is.EqualTo(s.plates.Length)); Assert.That(s.boxLabels.Length, Is.EqualTo(s.boxes.Length));
            for (int i = 0; i < s.boxes.Length; i++)
            {
                Assert.That(s.boxCapacityLabels[i].transform.IsChildOf(s.boxes[i]), Is.True, "box " + (i + 1) + " prints its capacity");
                Assert.That(s.orderUpTags[i].transform.IsChildOf(s.boxes[i]), Is.True, "tag rides with box " + (i + 1));
                Assert.That(s.orderUpTags[i].activeSelf, Is.False);
                Assert.That(s.orderUpTags[i].GetComponentInChildren<TMP_Text>(true).text, Is.EqualTo("ORDER UP"));
            }
            foreach (var label in s.plateLabels.Concat(s.boxLabels)) Assert.That(label.transform.IsChildOf(s.transform), Is.True);
            int maxItems = CafeChapter.All.Max(c => c.Items);
            Assert.That(s.items.Length, Is.GreaterThanOrEqualTo(maxItems));
            Assert.That(s.items.Select(v => v.id).ToArray(), Is.EqualTo(Enumerable.Range(1, s.items.Length).Select(i => "item-" + i).ToArray()));
            Assert.That(s.piecesRoot, Is.Not.Null); Assert.That(s.piecesRoot.activeSelf, Is.False, "no pastry during the briefing");
            foreach (var v in s.items)
            {
                Assert.That(v.piece, Is.Not.Null, v.id); Assert.That(v.grabbable, Is.Not.Null, v.id);
                Assert.That(v.piece.IsChildOf(s.piecesRoot.transform), Is.True, v.id);
                var grab = v.piece.GetComponentInChildren<Oculus.Interaction.GrabInteractable>(true);
                Assert.That(grab, Is.Not.Null, v.id + " grabbable by controller");
                Assert.That(handle.pieceInteractables, Does.Contain(grab), v.id + " paused while the table is carried");
                Assert.That(v.croissant, Is.Not.Null); Assert.That(v.cookie, Is.Not.Null); Assert.That(v.muffin, Is.Not.Null);
                Assert.That(v.croissant.transform.IsChildOf(v.piece) && v.cookie.transform.IsChildOf(v.piece) && v.muffin.transform.IsChildOf(v.piece), Is.True, v.id);
            }
            Assert.That(s.payoff.bike, Is.Not.Null); Assert.That(s.payoff.bike.localPosition, Is.EqualTo(CafeLayout.BikeParked));
            Assert.That(s.payoff.steamPuffs.Length, Is.GreaterThanOrEqualTo(1));
            foreach (var puff in s.payoff.steamPuffs) Assert.That(puff.activeSelf, Is.False);
        }

        // ---- scene: buttons ----
        [Test] public void ButtonRowLivesInOneCanvasGroupAndStaysWired()
        {
            var s = Station(); var n = Find<NerdyDirector>();
            Assert.That(s.fallbackGroups, Is.Not.Null); Assert.That(s.fallbackGroups.Length, Is.EqualTo(1));
            var group = s.fallbackGroups[0]; Assert.That(group.transform.IsChildOf(Card(s)), Is.True);
            foreach (var (button, method) in new[] { (s.startButton, "PressStart"), (s.dealButton, "PressDealRound"), (s.checkButton, "PressCheck"), (s.clearButton, "PressClear"), (s.nextButton, "PressNextChapter"), (s.backButton, "Close") })
            {
                Assert.That(button, Is.Not.Null, method);
                Assert.That(button.transform.IsChildOf(group.transform), Is.True, method);
                Assert.That(button.GetComponentInParent<CanvasGroup>(true), Is.SameAs(group), method);
                Assert.That(button.GetComponent<UiPressLog>(), Is.Not.Null, button.name + " logs presses");
                Assert.That(button.onClick.GetPersistentEventCount(), Is.GreaterThanOrEqualTo(1), method);
                Assert.That(button.onClick.GetPersistentTarget(0), Is.SameAs(s), button.name);
                Assert.That(button.onClick.GetPersistentMethodName(0), Is.EqualTo(method), button.name);
            }
            Assert.That(s.backButton.onClick.GetPersistentEventCount(), Is.EqualTo(2));
            Assert.That(s.backButton.onClick.GetPersistentTarget(1), Is.SameAs(n)); Assert.That(s.backButton.onClick.GetPersistentMethodName(1), Is.EqualTo("OnLessonBack"), "Back closes the café, then returns to the lesson cards");
            foreach (var t in Card(s).GetComponentsInChildren<Button>(true)) Assert.That(t.GetComponent<UiPressLog>(), Is.Not.Null, t.name);
        }

        [Test] public void CardTextFitsForEveryChapterAndRowsStack()
        {
            var s = Station(); var card = Card(s);
            float bodyW = s.body.rectTransform.rect.width, bodyH = s.body.rectTransform.rect.height;
            Assert.That(s.heading.GetPreferredValues(s.Title + " · " + s.StoryName).x, Is.LessThanOrEqualTo(s.heading.rectTransform.rect.width));
            foreach (var resume in new CafeChapter[] { null, CafeChapter.All[4] })
                Assert.That(s.body.GetPreferredValues("<size=80%>" + CafeStation.BriefingBody(resume) + "</size>", bodyW, 10000).y, Is.LessThanOrEqualTo(bodyH), "briefing");
            foreach (var chapter in CafeChapter.All)
            {
                string title = CafeStation.TitleFor(chapter);
                Assert.That(s.heading.GetPreferredValues(title, s.heading.rectTransform.rect.width, 1000).y, Is.LessThanOrEqualTo(s.heading.rectTransform.rect.height), title);
                foreach (var task in chapter.Stages.Select(st => st.Task ?? chapter.Task).Distinct())
                {
                    string text = CafeStation.ComposeBody(chapter.Story, task, LongestFeedback(chapter));
                    // The station shrinks down to 65%; the authored copy must already fit at 80%.
                    Assert.That(s.body.GetPreferredValues("<size=80%>" + text + "</size>", bodyW, 10000).y, Is.LessThanOrEqualTo(bodyH), chapter.Id + ": " + text);
                }
                Assert.That(s.expressionLine.GetPreferredValues(chapter.Expression).x, Is.LessThanOrEqualTo(s.expressionLine.rectTransform.rect.width), chapter.Expression);
            }
            foreach (var briefing in new[] { false, true }) foreach (var plates in new[] { false, true }) foreach (var complete in new[] { false, true }) foreach (var last in new[] { false, true })
            {
                string hints = CafeStation.SayHintsFor(briefing, plates, complete, last);
                Assert.That(s.sayHints.GetPreferredValues(hints).x, Is.LessThanOrEqualTo(s.sayHints.rectTransform.rect.width), hints);
            }
            Rect R(RectTransform r) => new Rect(r.anchoredPosition - r.sizeDelta / 2, r.sizeDelta);
            var body = R(s.body.rectTransform); var expr = R(s.expressionLine.rectTransform); var hintsRect = R(s.sayHints.rectTransform);
            float rowTop = RowButtons(s).Max(b => R(b.GetComponent<RectTransform>()).yMax);
            Assert.That(body.yMin, Is.GreaterThanOrEqualTo(expr.yMax - 1f)); Assert.That(expr.yMin, Is.GreaterThanOrEqualTo(hintsRect.yMax - 1f)); Assert.That(hintsRect.yMin, Is.GreaterThanOrEqualTo(rowTop - 1f));
            var panel = card.GetComponent<RectTransform>().sizeDelta;
            foreach (var b in RowButtons(s))
            {
                var r = R(b.GetComponent<RectTransform>());
                Assert.That(Mathf.Abs(r.xMin) <= panel.x / 2 && Mathf.Abs(r.xMax) <= panel.x / 2 && r.yMin >= -panel.y / 2, Is.True, b.name + " inside the card");
                var label = b.GetComponentInChildren<TMP_Text>(true);
                Assert.That(label.GetPreferredValues(label.text).x, Is.LessThanOrEqualTo(label.rectTransform.rect.width + 1f), b.name + " label fits");
            }
            var row = RowButtons(s).Where(b => b != s.startButton).ToArray();   // Start shares the Deal slot and only shows in the briefing
            for (int i = 0; i < row.Length; i++) for (int j = i + 1; j < row.Length; j++)
                Assert.That(R(row[i].GetComponent<RectTransform>()).Overlaps(R(row[j].GetComponent<RectTransform>())), Is.False, row[i].name + " vs " + row[j].name);
        }

        static string LongestFeedback(CafeChapter chapter)
        {
            var lines = new List<string> { CafeStation.HeldReason, CafeStation.CheckedReason, CafeStation.NotCompleteReason, CafeStation.ClearedReason,
                chapter.Accepted + " " + chapter.Expression,
                chapter.Items + " " + chapter.ItemPlural + " are still on the tray.",
                "The plates are not equal yet: " + string.Join(", ", Enumerable.Repeat(chapter.Items, 3)) + " and " + chapter.Items + ".",
                "Box 6 has room for 4 more " + chapter.ItemPlural + ".",
                "Box 6 is full. It holds 5 " + chapter.ItemPlural + ".",
                "Plates 1, 2 and 3 got one more " + chapter.ItemName + ". " + (chapter.Items - 1) + " " + chapter.ItemPlural + " are still on the tray.",
                "Dealing a round is for plates. Pack the boxes one " + chapter.ItemName + " at a time." };
            foreach (var stage in chapter.Stages) if (stage.Accepted != null) lines.Add(stage.Accepted + " " + stage.Expression);
            return lines.OrderByDescending(l => l.Length).First();
        }

        static Bounds BoardBounds(Transform board, Transform t, out float lowestSightlineMargin)
        {
            var b = new Bounds(); bool has = false; lowestSightlineMargin = float.MaxValue;
            foreach (var f in t.GetComponentsInChildren<MeshFilter>(true))
            {
                if (f.GetComponent<TMP_Text>() != null || f.sharedMesh == null) continue;
                var mb = f.sharedMesh.bounds;
                foreach (float x in new[] { mb.min.x, mb.max.x }) foreach (float y in new[] { mb.min.y, mb.max.y }) foreach (float z in new[] { mb.min.z, mb.max.z })
                {
                    var p = board.InverseTransformPoint(f.transform.TransformPoint(new Vector3(x, y, z)));
                    if (!has) { b = new Bounds(p, Vector3.zero); has = true; } else b.Encapsulate(p);
                    lowestSightlineMargin = Mathf.Min(lowestSightlineMargin, CafeLayout.VisibleUnderCard(p.z) - p.y);
                }
            }
            return b;
        }

        static Transform Named(Transform root, string name) => root.GetComponentsInChildren<Transform>(true).First(t => t.name == name);

        [Test] public void TargetLabelsAreReadableFromTheSeatOnTheFrontEdge()
        {
            var d = Onboarding(); var s = Station();
            void Check(TMP_Text label, CafeTargetKind kind, int index, int count, string text)
            {
                Assert.That(label.text, Is.EqualTo(text));
                float scale = d.transform.InverseTransformVector(label.transform.TransformVector(Vector3.up)).magnitude;
                Assert.That(label.fontSize * scale, Is.GreaterThanOrEqualTo(0.25f), text + " world font size is about 3x the first build (0.09)");
                Assert.That(label.font, Is.SameAs(d.GetComponent<TypographyBindings>().style.headingFont), text + " bold");
                Assert.That(Quaternion.Angle(label.transform.localRotation, Quaternion.Euler(CafeLayout.LabelTilt, 0, 0)), Is.LessThan(0.5f), text + " tilted toward the seat");
                var c = CafeLayout.ContainerCenter(kind, index, count);
                var p = d.transform.InverseTransformPoint(label.transform.position);
                Assert.That(Vector3.Distance(p, CafeLayout.LabelPosition(kind, c)), Is.LessThan(1e-3f), text + " in front of its target");
                float width = label.GetPreferredValues(text).x * scale;
                float spacing = kind == CafeTargetKind.Plates ? CafeLayout.PlateSpacing : CafeLayout.BoxSpacing;
                Assert.That(width, Is.GreaterThan(0.05f), text + " is large");
                Assert.That(width, Is.LessThanOrEqualTo(spacing - 0.02f), text + " never runs into the next label");
            }
            for (int i = 0; i < s.plateLabels.Length; i++) Check(s.plateLabels[i], CafeTargetKind.Plates, i, s.plateLabels.Length, "Plate " + (i + 1));
            for (int i = 0; i < s.boxLabels.Length; i++) Check(s.boxLabels[i], CafeTargetKind.Boxes, i, s.boxLabels.Length, "Box " + (i + 1));
        }

        /// Corners of a text's rect (TMP rect in its own plane) in board space.
        static Vector3[] TextCorners(Transform board, TMP_Text t)
        {
            var size = t.rectTransform.sizeDelta; var corners = new Vector3[4]; int k = 0;
            foreach (float x in new[] { -size.x / 2, size.x / 2 }) foreach (float y in new[] { -size.y / 2, size.y / 2 })
                corners[k++] = board.InverseTransformPoint(t.transform.TransformPoint(new Vector3(x, y, 0)));
            return corners;
        }

        static Bounds Enclose(IEnumerable<Vector3> points) { var list = points.ToList(); var b = new Bounds(list[0], Vector3.zero); foreach (var p in list) b.Encapsulate(p); return b; }

        /// Screen-like rect (yaw, pitch in degrees) of points seen from the seated head looking along +z.
        static Rect FromSeat(IEnumerable<Vector3> points)
        {
            float minYaw = float.MaxValue, maxYaw = float.MinValue, minPitch = float.MaxValue, maxPitch = float.MinValue;
            foreach (var p in points)
            {
                var v = p - CafeLayout.SeatedHead;
                float yaw = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg, pitch = Mathf.Atan2(v.y, new Vector2(v.x, v.z).magnitude) * Mathf.Rad2Deg;
                minYaw = Mathf.Min(minYaw, yaw); maxYaw = Mathf.Max(maxYaw, yaw); minPitch = Mathf.Min(minPitch, pitch); maxPitch = Mathf.Max(maxPitch, pitch);
            }
            return Rect.MinMaxRect(minYaw, minPitch, maxYaw, maxPitch);
        }

        [Test] public void BoxLabelNeverCoversTheCapacityNumber()
        {
            var d = Onboarding(); var s = Station();
            for (int i = 0; i < s.boxes.Length; i++)
            {
                var label = TextCorners(d.transform, s.boxLabels[i]);
                var number = TextCorners(d.transform, s.boxCapacityLabels[i]).ToList();
                var tag = s.boxes[i].GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Capacity tag");
                Assert.That(tag, Is.Not.Null, "box " + (i + 1) + " capacity tag");
                var tagBounds = BoardBounds(d.transform, tag, out _);
                number.Add(tagBounds.min); number.Add(tagBounds.max);
                Assert.That(Enclose(label).Intersects(Enclose(number)), Is.False, "Box " + (i + 1) + ": label and capacity number do not intersect");
                var seenLabel = FromSeat(label); var seenNumber = FromSeat(number);
                Assert.That(seenLabel.Overlaps(seenNumber), Is.False, "Box " + (i + 1) + ": from the seat the label " + seenLabel + " never covers the number " + seenNumber);
                var local = s.boxes[i].InverseTransformPoint(s.boxCapacityLabels[i].transform.position);
                Assert.That(local.y - CafeLayout.CapacityTagSize.y / 2, Is.GreaterThanOrEqualTo(CafeLayout.BoxHeight), "number stands above the pastries on the back rim");
                Assert.That(local.z, Is.GreaterThan(0f), "number on the back of the box");
                Assert.That(s.orderUpTags[i].transform.localPosition.y - 0.015f, Is.GreaterThan(CafeLayout.CapacityTagCenter.y + CafeLayout.CapacityTagSize.y / 2), "ORDER UP floats above the number");
            }
        }

        [Test] public void BikeCargoCrateIsSmallTuckedInAndHoldsTheBoxes()
        {
            var s = Station(); var bike = s.payoff.bike;
            var crate = bike.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Cargo crate");
            Assert.That(crate, Is.Not.Null);
            var b = BoardBounds(bike, crate, out _);
            Assert.That(b.size.x, Is.LessThanOrEqualTo(0.12f)); Assert.That(b.size.z, Is.LessThanOrEqualTo(0.08f));
            Assert.That(crate.GetComponentsInChildren<MeshRenderer>(true).Select(r => r.sharedMaterial.name).Distinct().Count(), Is.GreaterThanOrEqualTo(2), "rimmed crate, not a plain slab");
            var wheels = BoardBounds(bike, bike.GetComponentsInChildren<Transform>(true).First(t => t.name == "Rear wheel"), out _);
            Assert.That(b.min.z, Is.GreaterThanOrEqualTo(wheels.min.z - 0.03f), "crate tucked over the rear wheel");
            Assert.That(Mathf.Abs(b.center.x), Is.LessThan(0.005f));
            Assert.That(s.payoff.rackScale, Is.EqualTo(CafeLayout.RackScale)); Assert.That(s.payoff.rackCenter, Is.EqualTo(CafeLayout.RackCenter));
            int maxFull = CafeChapter.All.Max(c => c.Stages.Where(st => st.Kind == CafeTargetKind.Boxes).Select(st => c.Items / st.BoxCapacity).DefaultIfEmpty(0).Max());
            for (int j = 0; j < maxFull; j++)
            {
                var slot = s.payoff.RackSlot(j);
                Assert.That(Mathf.Abs(slot.x) + CafeLayout.BoxWidth * s.payoff.rackScale / 2, Is.LessThanOrEqualTo(CafeLayout.RackWidth / 2 + 0.005f), "box " + j + " fits the crate width");
                Assert.That(CafeLayout.BoxDepth * s.payoff.rackScale, Is.LessThanOrEqualTo(CafeLayout.RackDepth), "box fits the crate depth");
            }
        }

        [Test] public void PastryTrayIsAServingTrayForTwoRows()
        {
            var d = Onboarding(); var root = CafeRoot();
            var tray = Named(root, "Pastry tray");
            var b = BoardBounds(d.transform, tray, out _);
            Assert.That(b.size.x, Is.LessThanOrEqualTo(0.5f), "no deck-wide slab: tray width " + b.size.x);
            Assert.That(b.size.z, Is.LessThanOrEqualTo(0.16f), "tray depth " + b.size.z);
            Assert.That(b.max.y, Is.LessThan(CafeLayout.TrayTop + 0.006f), "low rim");
            Assert.That(tray.GetComponentsInChildren<MeshRenderer>(true).Select(r => r.sharedMaterial.name).Distinct().Count(), Is.GreaterThanOrEqualTo(2), "pastel trim on the base");
            for (int i = 0; i < CafeLayout.MaxItems; i++)
            {
                var p = CafeLayout.TrayPosition(i);
                Assert.That(p.x > b.min.x + 0.02f && p.x < b.max.x - 0.02f && p.z > b.min.z + 0.02f && p.z < b.max.z - 0.02f, Is.True, "tray slot " + i + " on the tray");
            }
            var s = Station();
            foreach (var label in s.plateLabels.Concat(s.boxLabels))
                Assert.That(b.max.z, Is.LessThan(d.transform.InverseTransformPoint(label.transform.position).z - CafeLayout.LabelDepth / 2 - 0.05f), label.name + " clear of the tray");
        }

        [Test] public void MenuBoardIsVisibleOnTheDeckAndClearOfTheBike()
        {
            var d = Onboarding(); var root = CafeRoot(); var s = Station();
            var board = BoardBounds(d.transform, Named(root, "Menu board"), out float sightlineMargin);
            Assert.That(board.min.x >= -CafeLayout.DeckHalfX && board.max.x <= CafeLayout.DeckHalfX && board.min.z >= -CafeLayout.DeckHalfZ && board.max.z <= CafeLayout.DeckHalfZ + 0.005f, Is.True, "menu board fully on the deck: " + board);
            Assert.That(sightlineMargin, Is.GreaterThan(0.005f), "menu board reads under the lesson card from the seat");
            foreach (var t in Named(root, "Menu board").GetComponentsInChildren<TextMeshPro>(true))
            {
                var p = d.transform.InverseTransformPoint(t.transform.position);
                Assert.That(p.y, Is.LessThan(CafeLayout.VisibleUnderCard(p.z) - 0.01f), t.name + " readable under the card");
            }
            // Bike lane: the parked bike's width, from its rear to past the deck's far edge.
            var bike = BoardBounds(d.transform, s.payoff.bike, out _);
            var lane = new Rect(bike.min.x - 0.02f, bike.min.z, bike.size.x + 0.04f, CafeLayout.DeckHalfZ + 0.2f - bike.min.z);
            Assert.That(lane.Overlaps(new Rect(board.min.x, board.min.z, board.size.x, board.size.z)), Is.False, "menu board clear of the bike's ride-off path");
            foreach (var other in new[] { "Guest table", "Guest chair 1", "Guest chair 2", "Guest chair 3", "Guest chair 4", "Coffee counter" })
            {
                var o = BoardBounds(d.transform, Named(root, other), out _);
                Assert.That(new Rect(o.min.x, o.min.z, o.size.x, o.size.z).Overlaps(new Rect(board.min.x, board.min.z, board.size.x, board.size.z)), Is.False, "menu board clear of " + other);
            }
        }

        // ---- pure layout ----
        [Test] public void LayoutSeparatesTargetsAndKeepsTheTrayClear()
        {
            foreach (var chapter in CafeChapter.All) foreach (var stage in chapter.Stages)
            {
                var kind = stage.Kind; int n = stage.Containers;
                float halfW = kind == CafeTargetKind.Plates ? CafeLayout.PlateDiameter / 2 : CafeLayout.BoxWidth / 2;
                for (int i = 0; i < n; i++)
                {
                    var c = CafeLayout.ContainerCenter(kind, i, n);
                    Assert.That(CafeLayout.NearestContainer(kind, n, c + new Vector3(0, 0.12f, 0)), Is.EqualTo(i), chapter.Id + " release over target " + i);
                    Assert.That(Mathf.Abs(c.x) + halfW, Is.LessThan(CafeLayout.BikeParked.x - 0.08f), chapter.Id + " target " + i + " clear of the bike");
                    if (i > 0) Assert.That(c.x - CafeLayout.ContainerCenter(kind, i - 1, n).x - 2 * halfW, Is.GreaterThanOrEqualTo(kind == CafeTargetKind.Plates ? 0.06f : 0.015f), chapter.Id + " targets clearly separated");
                    if (kind == CafeTargetKind.Boxes)
                        for (int k = 0; k < stage.BoxCapacity; k++)
                        {
                            var p = CafeLayout.SlotPosition(kind, c, k);
                            Assert.That(Mathf.Abs(p.x - c.x) + 0.017f, Is.LessThanOrEqualTo(CafeLayout.BoxWidth / 2 - CafeLayout.BoxWall), chapter.Id + " slot " + k + " inside the box (x)");
                            Assert.That(Mathf.Abs(p.z - c.z) + 0.017f, Is.LessThanOrEqualTo(CafeLayout.BoxDepth / 2 - CafeLayout.BoxWall), chapter.Id + " slot " + k + " inside the box (z)");
                        }
                    var label = CafeLayout.LabelPosition(kind, c);
                    float front = c.z - CafeLayout.FrontHalfDepth(kind);
                    float labelBack = label.z + CafeLayout.LabelDepth / 2, labelFront = label.z - CafeLayout.LabelDepth / 2;
                    Assert.That(labelBack, Is.LessThanOrEqualTo(front).And.GreaterThan(front - 0.02f), chapter.Id + " label " + i + " sits on the target's front edge");
                    Assert.That(Mathf.Abs(label.x - c.x), Is.LessThan(1e-4f));
                    Assert.That(label.y - CafeLayout.LabelHeight / 2 * Mathf.Cos(CafeLayout.LabelTilt * Mathf.Deg2Rad), Is.GreaterThan(CafeLayout.DeckTop), "tilted label clears the deck");
                    for (int t = 0; t < CafeLayout.MaxItems; t++)
                        Assert.That(CafeLayout.TrayPosition(t).z + 0.025f, Is.LessThan(labelFront - 0.05f), "tray pastry " + t + " never covers " + chapter.Id + " label " + i);
                }
                for (int t = 0; t < CafeLayout.MaxItems; t++)
                    Assert.That(CafeLayout.NearestContainer(kind, n, CafeLayout.TrayPosition(t)), Is.EqualTo(-1), "tray slot " + t + " is not a drop target in " + chapter.Id);
                Assert.That(CafeLayout.NearestContainer(kind, n, new Vector3(0, 0.1f, 0.35f)), Is.EqualTo(-1), "the guest table is not a drop target");
            }
            var trays = Enumerable.Range(0, CafeLayout.MaxItems).Select(CafeLayout.TrayPosition).ToList();
            for (int a = 0; a < trays.Count; a++) for (int b = a + 1; b < trays.Count; b++)
                Assert.That(Vector3.Distance(trays[a], trays[b]), Is.GreaterThan(0.05f), "tray slots " + a + " and " + b + " do not overlap");
            // Serving tray: sized for two rows of 15, every tray slot on it with room for the rim.
            Assert.That(CafeLayout.TrayWidth, Is.LessThanOrEqualTo(0.5f)); Assert.That(CafeLayout.TrayDepth, Is.LessThanOrEqualTo(0.16f));
            foreach (var p in trays)
            {
                Assert.That(Mathf.Abs(p.x) + 0.024f, Is.LessThanOrEqualTo(CafeLayout.TrayWidth / 2 - 0.008f), "tray slot on the tray (x)");
                Assert.That(Mathf.Abs(p.z - CafeLayout.TrayCenterZ) + 0.024f, Is.LessThanOrEqualTo(CafeLayout.TrayDepth / 2 - 0.008f), "tray slot on the tray (z)");
            }
            Assert.That(CafeLayout.TrayFrontZ - CafeLayout.TrayDepth, Is.GreaterThan(-CafeLayout.DeckHalfZ - 0.05f), "tray within reach on the deck");
            // The card sightline helper: the card bottom itself is the limit at the card; farther back the limit drops.
            Assert.That(CafeLayout.VisibleUnderCard(CafeLayout.CardZ + 1e-4f), Is.EqualTo(CafeLayout.CardBottomY).Within(1e-3f));
            Assert.That(CafeLayout.VisibleUnderCard(0.39f), Is.LessThan(CafeLayout.CardBottomY));
            Assert.That(CafeLayout.ShapeFor("croissant", 3), Is.EqualTo(CafeItemShape.Croissant));
            Assert.That(CafeLayout.ShapeFor("pastry", 0) != CafeLayout.ShapeFor("pastry", 1), Is.True, "mixed pastries");
        }

        // ---- pure copy rules ----
        [Test] public void ToolsHintsAndCopyFollowTheState()
        {
            var briefing = CafeStation.ToolsFor(true, CafeTargetKind.Plates, false, false);
            Assert.That(briefing, Does.Contain("advance_step").And.Contain("back_to_lessons").And.Not.Contain("cafe_check_order").And.Not.Contain("cafe_deal_round"));
            Assert.That(CafeStation.ToolsFor(false, CafeTargetKind.Plates, false, false), Does.Contain("cafe_deal_round").And.Contain("cafe_check_order").And.Contain("cafe_clear_table").And.Not.Contain("next_chapter"));
            Assert.That(CafeStation.ToolsFor(false, CafeTargetKind.Boxes, false, false), Does.Not.Contain("cafe_deal_round").And.Contain("cafe_check_order"));
            Assert.That(CafeStation.ToolsFor(false, CafeTargetKind.Boxes, true, false), Does.Contain("next_chapter").And.Contain("restart_chapter").And.Not.Contain("cafe_check_order"));
            Assert.That(CafeStation.ToolsFor(false, CafeTargetKind.Boxes, true, true), Does.Not.Contain("next_chapter").And.Contain("back_to_lessons"));
            Assert.That(CafeStation.SayHintsFor(false, true, false, false), Is.EqualTo("Say \"deal a round\" · \"check the order\" · \"clear the table\""));
            Assert.That(CafeStation.SayHintsFor(false, false, false, false), Does.Not.Contain("deal a round"));
            Assert.That(CafeStation.SayHintsFor(false, true, true, false), Does.Contain("next chapter"));
            Assert.That(CafeStation.SayHintsFor(true, true, false, false), Does.Contain("yes"));
            Assert.That(CafeStation.TitleFor(CafeChapter.All[2]), Is.EqualTo("Chapter 3 · Box it up"));
            var ch = CafeChapter.All[0];
            Assert.That(CafeStation.ComposeBody(ch.Story, ch.Task, "2 croissants are still on the tray."), Does.StartWith(ch.Story + "\n" + ch.Task).And.EndWith("still on the tray."));
            string brief = CafeStation.BriefingBody(null);
            Assert.That(brief, Does.StartWith("Welcome to the Corner Café").And.EndWith("Say yes or press Start."));
            Assert.That(CafeStation.BriefingBody(CafeChapter.All[2]), Does.Contain("Chapter 3 · Box it up"));
            foreach (var line in new[] { brief, CafeStation.HeldReason, CafeStation.CheckedReason, CafeStation.ClearedReason })
                foreach (var banned in new[] { "stars", "score", "great job", "points", "good job", "well done" }) Assert.That(line.ToLower(), Does.Not.Contain(banned));
            Assert.That(CafeStation.HeldReason, Is.EqualTo("Let go of the pastry first."));
        }

        // ---- behaviour on in-memory objects (no scene changes) ----
        sealed class Cafe
        {
            public GameObject root; public CafeStation station; public CafePayoff payoff; public Transform bike;
            public Vector3 Local(string id) => root.transform.InverseTransformPoint(station.items.First(v => v.id == id).piece.position);
            public Transform Piece(string id) => station.items.First(v => v.id == id).piece;
        }

        static Cafe MakeCafe()
        {
            var root = new GameObject("test cafe");
            Transform Child(Transform parent, string name) { var t = new GameObject(name).transform; t.SetParent(parent, false); return t; }
            var station = root.AddComponent<CafeStation>();
            station.visualRoots = new[] { root }; station.space = root.transform;
            var pieces = Child(root.transform, "pieces"); station.piecesRoot = pieces.gameObject;
            station.items = Enumerable.Range(1, CafeLayout.MaxItems).Select(i => new CafeStation.ItemView { id = "item-" + i, piece = Child(pieces, "pastry " + i) }).ToArray();
            var targets = Child(root.transform, "targets");
            station.plates = Enumerable.Range(1, CafeLayout.MaxPlates).Select(i => Child(targets, "plate " + i)).ToArray();
            station.boxes = Enumerable.Range(1, CafeLayout.MaxBoxes).Select(i => Child(targets, "box " + i)).ToArray();
            station.orderUpTags = station.boxes.Select(b => { var tag = Child(b, "tag").gameObject; tag.SetActive(false); return tag; }).ToArray();
            var payoff = root.AddComponent<CafePayoff>(); payoff.space = root.transform;
            var bike = Child(root.transform, "bike"); bike.localPosition = CafeLayout.BikeParked;
            payoff.bike = bike; payoff.bikeParked = CafeLayout.BikeParked;
            payoff.steamPuffs = Enumerable.Range(1, 4).Select(i => { var p = Child(root.transform, "puff " + i).gameObject; p.SetActive(false); return p; }).ToArray();
            station.payoff = payoff;
            return new Cafe { root = root, station = station, payoff = payoff, bike = bike };
        }

        static void AssertNear(Vector3 actual, Vector3 expected, string message) => Assert.That(Vector3.Distance(actual, expected), Is.LessThan(1e-4f), message + ": " + actual + " vs " + expected);

        [Test] public void DroppingOnAPlatePlacesItAndAnywhereElseReturnsItToTheTray()
        {
            var cafe = MakeCafe(); var s = cafe.station;
            try
            {
                s.Open();
                Assert.That(s.IsOpen, Is.True); Assert.That(s.ChapterActive, Is.False);
                Assert.That(s.CurrentStep().Id, Is.EqualTo(CafeSteps.BriefingId));
                Assert.That(s.piecesRoot.activeSelf, Is.False, "briefing: nothing to grab");
                Assert.That(s.Chapter.Number, Is.EqualTo(0), "no chapter facts during the briefing");
                Assert.That(s.DropAt("item-1", Vector3.zero), Is.False);

                var start = s.Advance();
                Assert.That(start.Ok, Is.True, start.Reason); Assert.That(start.Reason, Does.StartWith("Chapter 1 · Two friends"));
                Assert.That(s.ChapterActive, Is.True); Assert.That(s.Chapter.Number, Is.EqualTo(1)); Assert.That(s.piecesRoot.activeSelf, Is.True);
                var plate0 = CafeLayout.ContainerCenter(CafeTargetKind.Plates, 0, 2);
                Assert.That(s.plates[0].gameObject.activeSelf && s.plates[1].gameObject.activeSelf, Is.True);
                Assert.That(s.plates[2].gameObject.activeSelf || s.boxes[0].gameObject.activeSelf, Is.False, "only this chapter's two plates");
                AssertNear(s.plates[0].localPosition, plate0, "plate 1 placed for two plates");
                Assert.That(s.items.Count(v => v.piece.gameObject.activeSelf), Is.EqualTo(6), "6 croissants");
                AssertNear(cafe.Local("item-3"), CafeLayout.TrayPosition(2), "waiting on the tray");

                Assert.That(s.DropAt("item-1", plate0 + new Vector3(0.02f, 0.1f, -0.01f)), Is.True, "released over plate 1");
                Assert.That(s.Model.ContainerOf("item-1"), Is.EqualTo(0));
                AssertNear(cafe.Local("item-1"), CafeLayout.SlotPosition(CafeTargetKind.Plates, plate0, 0), "snapped onto plate 1");

                Assert.That(s.DropAt("item-1", new Vector3(0.35f, 0.1f, -0.1f)), Is.False, "released over bare deck");
                Assert.That(s.Model.ContainerOf("item-1"), Is.EqualTo(-1));
                AssertNear(cafe.Local("item-1"), CafeLayout.TrayPosition(0), "back on the tray");
            }
            finally { Object.DestroyImmediate(cafe.root); }
        }

        [Test] public void DealingARoundPutsOnePastryOnEachPlateAndBoxesRefuseIt()
        {
            var cafe = MakeCafe(); var s = cafe.station;
            try
            {
                s.Open(); s.Advance();
                Assert.That(s.ToolsNow, Does.Contain("cafe_deal_round"));
                Assert.That(s.TryLessonTool("cafe_deal_round", null, out var dealt), Is.True);
                Assert.That(dealt.Ok, Is.True, dealt.Reason);
                Assert.That(s.Model.CountIn(0), Is.EqualTo(1)); Assert.That(s.Model.CountIn(1), Is.EqualTo(1)); Assert.That(s.Model.LooseCount, Is.EqualTo(4));
                Assert.That(s.Model.ChapterComplete, Is.False, "dealing never judges the table");

                s.JumpToChapter(2);   // Box it up
                Assert.That(s.ToolsNow, Does.Not.Contain("cafe_deal_round"));
                Assert.That(s.boxes.Take(5).All(b => b.gameObject.activeSelf) && !s.boxes[5].gameObject.activeSelf, Is.True, "5 boxes out");
                Assert.That(s.plates.Any(p => p.gameObject.activeSelf), Is.False);
                Assert.That(s.TryLessonTool("cafe_deal_round", null, out var refused), Is.True);
                Assert.That(refused.Ok, Is.False); Assert.That(refused.Reason, Is.Not.Empty);

                Assert.That(s.TryLessonTool("garden_turn_bed", null, out var wrong), Is.False);
                Assert.That(wrong.Ok, Is.False); Assert.That(wrong.Reason, Is.EqualTo("That is not part of Neighborhood Café."));
            }
            finally { Object.DestroyImmediate(cafe.root); }
        }

        [Test] public void AnAcceptedShareServesThePlatesAndRestartRestoresTheTable()
        {
            var cafe = MakeCafe(); var s = cafe.station;
            try
            {
                s.Open(); s.Advance();
                for (int i = 0; i < 3; i++) Assert.That(s.DealRound().Ok, Is.True);
                var plate0 = s.plates[0].localPosition; var item = cafe.Local("item-1");
                var check = s.TryLessonTool("cafe_check_order", null, out var r) ? r : default;
                Assert.That(check.Ok, Is.True, check.Reason); Assert.That(check.Reason, Does.EndWith("6 ÷ 2 = 3"));
                Assert.That(s.Model.ChapterComplete, Is.True); Assert.That(cafe.payoff.Served, Is.True);
                var delta = new Vector3(0, CafeLayout.GuestTableTop - CafeLayout.DeckTop, CafeLayout.ServeDistance);
                AssertNear(s.plates[0].localPosition, plate0 + delta, "plate 1 slid onto the guest table");
                AssertNear(cafe.Local("item-1"), item + delta, "its croissants rode along");
                Assert.That(cafe.payoff.steamPuffs.All(p => p.activeSelf), Is.True, "steam rises from the cups");
                Assert.That(s.ToolsNow, Does.Contain("next_chapter").And.Not.Contain("cafe_check_order"));
                Assert.That(s.DropAt("item-1", CafeLayout.ContainerCenter(CafeTargetKind.Plates, 1, 2)), Is.False, "served pastries stay served");

                var restart = s.RestartChapter();
                Assert.That(restart.Ok, Is.True);
                Assert.That(s.Model.ChapterComplete, Is.False);
                AssertNear(s.plates[0].localPosition, plate0, "plate back on the counter");
                AssertNear(cafe.Local("item-1"), CafeLayout.TrayPosition(0), "croissants back on the tray");
                Assert.That(cafe.payoff.steamPuffs.Any(p => p.activeSelf), Is.False);
            }
            finally { Object.DestroyImmediate(cafe.root); }
        }

        [Test] public void FullBoxesShowOrderUpAndRideOffOnTheBikeThenNextChapterRestores()
        {
            var cafe = MakeCafe(); var s = cafe.station;
            try
            {
                s.Open(); s.Advance(); s.JumpToChapter(2);   // 12 cookies, 5 boxes of 4
                Vector3 Box(int b) => CafeLayout.ContainerCenter(CafeTargetKind.Boxes, b, 5) + new Vector3(0, 0.08f, 0.01f);
                for (int i = 1; i <= 4; i++) Assert.That(s.DropAt("item-" + i, Box(0)), Is.True, "cookie " + i);
                Assert.That(s.DropAt("item-5", Box(0)), Is.False, "box 1 is full");
                Assert.That(s.Feedback, Does.Contain("full"));
                AssertNear(cafe.Local("item-5"), CafeLayout.TrayPosition(4), "refused cookie back on the tray");
                var early = s.Check();
                Assert.That(early.Ok, Is.False); Assert.That(early.Reason, Does.Contain("still on the tray"));
                for (int i = 5; i <= 8; i++) Assert.That(s.DropAt("item-" + i, Box(1)), Is.True);
                for (int i = 9; i <= 12; i++) Assert.That(s.DropAt("item-" + i, Box(3)), Is.True, "any box will do");
                AssertNear(cafe.Local("item-9"), CafeLayout.SlotPosition(CafeTargetKind.Boxes, CafeLayout.ContainerCenter(CafeTargetKind.Boxes, 3, 5), 0), "packed into box 4");

                var check = s.Check();
                Assert.That(check.Ok, Is.True, check.Reason); Assert.That(s.Model.ChapterComplete, Is.True); Assert.That(cafe.payoff.Shipped, Is.True);
                Assert.That(new[] { 0, 1, 3 }.All(b => s.orderUpTags[b].activeSelf), Is.True, "ORDER UP on every full box");
                Assert.That(s.orderUpTags[2].activeSelf || s.orderUpTags[4].activeSelf, Is.False, "empty boxes stay behind untagged");
                Assert.That(cafe.bike.gameObject.activeSelf, Is.False, "the bike rode off the deck");
                Assert.That(s.boxes[0].gameObject.activeSelf || cafe.Piece("item-1").gameObject.activeSelf, Is.False, "full boxes and their cookies left with it");
                Assert.That(s.boxes[2].gameObject.activeSelf, Is.True);

                var next = s.NextChapter();
                Assert.That(next.Ok, Is.True, next.Reason); Assert.That(s.Chapter.Number, Is.EqualTo(4));
                Assert.That(cafe.bike.gameObject.activeSelf, Is.True); AssertNear(cafe.bike.localPosition, CafeLayout.BikeParked, "bike back at the kerb");
                Assert.That(s.boxes[0].gameObject.activeSelf, Is.True); Assert.That(s.boxes[0].localScale, Is.EqualTo(Vector3.one));
                Assert.That(s.orderUpTags.Any(t => t.activeSelf), Is.False);
                Assert.That(cafe.Piece("item-1").gameObject.activeSelf, Is.True); Assert.That(cafe.Piece("item-1").localScale, Is.EqualTo(Vector3.one));
                AssertNear(cafe.Local("item-15"), CafeLayout.TrayPosition(14), "15 muffins on the tray");
            }
            finally { Object.DestroyImmediate(cafe.root); }
        }

        [Test] public void FactFamilyServesThePlatesThenResetsTheTableForPacking()
        {
            var cafe = MakeCafe(); var s = cafe.station;
            try
            {
                s.Open(); s.Advance(); s.JumpToChapter(4);
                Assert.That(s.plates.All(p => p.gameObject.activeSelf), Is.True, "4 plates");
                for (int i = 0; i < 3; i++) s.DealRound();
                var check = s.Check();
                Assert.That(check.Ok, Is.True, check.Reason);
                Assert.That(s.Model.StageIndex, Is.EqualTo(1)); Assert.That(s.Model.ChapterComplete, Is.False);
                Assert.That(s.plates.Any(p => p.gameObject.activeSelf), Is.False, "plates cleared away");
                Assert.That(s.boxes.All(b => b.gameObject.activeSelf), Is.True, "6 boxes of 3 out for packing");
                for (int i = 0; i < 12; i++) AssertNear(cafe.Local("item-" + (i + 1)), CafeLayout.TrayPosition(i), "muffin " + (i + 1) + " back on the tray");
                Assert.That(s.ToolsNow, Does.Not.Contain("cafe_deal_round"));
                Assert.That(s.Model.Expression, Is.EqualTo("12 ÷ 4 = 3"));
                Assert.That(s.Chapter.Task, Does.Contain("box"), "the card shows the packing task");
            }
            finally { Object.DestroyImmediate(cafe.root); }
        }

        [Test] public void RefusalsAreShortAndSpokenAsIs()
        {
            var cafe = MakeCafe(); var s = cafe.station;
            try
            {
                Assert.That(s.Check().Reason, Is.EqualTo(CafeStation.ClosedReason));
                Assert.That(s.ToolsNow, Is.Empty);
                s.Open();
                Assert.That(s.Check().Reason, Is.EqualTo(CafeStation.BriefingReason));
                Assert.That(s.NextChapter().Ok, Is.False);
                s.Advance();
                s.items[0].held = true;
                Assert.That(s.AnyHeld, Is.True);
                foreach (var result in new[] { s.Check(), s.ResetTable(), s.DealRound(), s.RestartChapter() })
                {
                    Assert.That(result.Ok, Is.False); Assert.That(result.Reason, Is.EqualTo("Let go of the pastry first."));
                }
                s.items[0].held = false;
                Assert.That(s.NextChapter().Reason, Is.EqualTo(CafeStation.NotCompleteReason));
                Assert.That(s.ResetTable().Reason, Is.EqualTo(CafeStation.ClearedReason));
            }
            finally { Object.DestroyImmediate(cafe.root); }
        }

        [Test] public void ClosingPutsPastriesBackAndReturningResumesCoherently()
        {
            var cafe = MakeCafe(); var s = cafe.station;
            try
            {
                s.Open(); s.Advance();
                Assert.That(s.DropAt("item-2", CafeLayout.ContainerCenter(CafeTargetKind.Plates, 1, 2)), Is.True);
                s.Close();
                Assert.That(s.IsOpen, Is.False); Assert.That(cafe.root.activeSelf, Is.False);
                Assert.That(s.Model.ContainerOf("item-2"), Is.EqualTo(-1));
                AssertNear(cafe.Local("item-2"), CafeLayout.TrayPosition(1), "tray restored on close");

                s.Open();
                Assert.That(cafe.root.activeSelf, Is.True); Assert.That(s.ChapterActive, Is.False, "back to the briefing");
                Assert.That(s.Advance().Ok, Is.True); Assert.That(s.Chapter.Number, Is.EqualTo(1));
                for (int i = 0; i < 3; i++) s.DealRound();
                Assert.That(s.Check().Ok, Is.True);
                s.Close();
                Assert.That(cafe.payoff.steamPuffs.Any(p => p.activeSelf), Is.False, "payoff stopped");
                s.Open(); s.Advance();
                Assert.That(s.Chapter.Number, Is.EqualTo(2), "an accepted chapter continues with the next one");
                Assert.That(s.plates.Take(3).All(p => p.gameObject.activeSelf), Is.True);
                AssertNear(s.plates[0].localPosition, CafeLayout.ContainerCenter(CafeTargetKind.Plates, 0, 3), "plates back on the counter");
                Assert.That(s.items.Count(v => v.piece.gameObject.activeSelf), Is.EqualTo(12));
            }
            finally { Object.DestroyImmediate(cafe.root); }
        }
    }
}
