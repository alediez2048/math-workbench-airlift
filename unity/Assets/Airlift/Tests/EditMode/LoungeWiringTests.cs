using System.Linq;
using Airlift.Lounge;
using Airlift.Presentation;
using Airlift.Welcome;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Airlift.Tests
{
    /// CC-FD-01, scene side. Run AgentScripts/BuildLounge.cs before this suite.
    public class LoungeWiringTests
    {
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").isLoaded) loaded = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        LoungeRoom Room() => SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<LoungeRoom>(true)).FirstOrDefault();

        [Test] public void TheLoungeIsInTheSceneWithItsShellAndFurniture()
        {
            var room = Room();
            Assert.That(room, Is.Not.Null, "run AgentScripts/BuildLounge.cs");
            Assert.That(room.shell, Is.Not.Null, "walls and floor, so Your room can drop them");
            Assert.That(room.furniture, Is.Not.Null, "furniture and glows stay in both sceneries");
            Assert.That(room.transform.parent, Is.Null, "world-locked root, like the workbench and the welcome panel");
        }

        [Test] public void TheDirectorHoldsTheLoungeSoItCanHideForALesson()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            Assert.That(n.lounge, Is.Not.Null, "NerdyDirector.ShowPhase hides the room while a lesson runs");
            Assert.That(n.lounge, Is.EqualTo(Room()));
        }

        // Dee's pedestal was removed with the rest of the props (owner, 2026-09-17: carpet and whiteboard only).
        // She speaks from the assistant bar on the board; if she is given a place in the room again, test it here.

        [Test] public void TheBoardFramesThePanelAndTheHandleSitsUnderIt()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var board = n.transform.Find("Board");
            Assert.That(board, Is.Not.Null, "run AgentScripts/BuildLounge.cs");

            var face = board.Find("Board face");
            float expected = Airlift.Presentation.NerdySpace.PanelWidthMetres + Airlift.Presentation.NerdySpace.BoardMargin * 2f;
            Assert.That(face.GetComponent<Renderer>().bounds.size.x, Is.EqualTo(expected).Within(0.03f),
                "the board frames the standard panel rather than dwarfing it");

            var handle = n.transform.Find("Panel handle");
            float boardBottom = board.Find("Board frame bottom").localPosition.y;
            Assert.That(handle.localPosition.y, Is.LessThan(boardBottom),
                "the handle hangs below the board, not across the middle of it");
        }

        // Owner 2026-09-17, revised: the assistant belongs "at the bottom of the whiteboard, but inside the
        // whiteboard" — under the content, above the bottom edge, not hanging off the board.
        [Test] public void TheAssistantBarSitsInsideTheBoardBelowTheContent()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var hud = (RectTransform)n.hudRoot.transform;
            hud.SetParent(n.hudWelcomeCanvas, false);
            hud.anchoredPosition = n.hudWelcomePosition;
            Canvas.ForceUpdateCanvases();

            var bar = new Vector3[4]; hud.GetWorldCorners(bar);
            float barTop = bar.Max(c => c.y), barBottom = bar.Min(c => c.y);

            var consent = (RectTransform)n.consentRoot.transform;
            var card = new Vector3[4]; consent.GetWorldCorners(card);
            float cardBottom = card.Min(c => c.y);

            float contentBottom = float.MaxValue;
            var corners = new Vector3[4];
            foreach (RectTransform child in consent)
            {
                if (child.sizeDelta.y <= 0f) continue;
                child.GetWorldCorners(corners);
                contentBottom = Mathf.Min(contentBottom, corners.Min(c => c.y));
            }

            Assert.That(barTop, Is.LessThanOrEqualTo(contentBottom + 0.005f), "the bar overlaps the card's content");
            Assert.That(barBottom, Is.GreaterThanOrEqualTo(cardBottom - 0.005f), "the bar hangs off the bottom of the board");
        }

        // The board's content is spaced through the board with even margins, and nothing touches anything.
        // Owner 2026-09-17, after several rounds of too-tight and too-spread: "properly spaced out inside of the
        // whiteboard". Judged against the rendered board, then pinned here.
        [Test] public void TheCardContentIsSpacedThroughTheBoardWithoutOverlap()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var consent = (RectTransform)n.consentRoot.transform;
            var items = consent.Cast<Transform>().Select(c => c as RectTransform)
                .Where(r => r != null && r.gameObject.activeSelf && r.sizeDelta.y > 0f && r.name != "Stroke")
                .OrderByDescending(r => r.anchoredPosition.y).ToList();

            for (int i = 1; i < items.Count; i++)
            {
                float upperBottom = items[i - 1].anchoredPosition.y - items[i - 1].sizeDelta.y / 2f;
                float lowerTop = items[i].anchoredPosition.y + items[i].sizeDelta.y / 2f;
                bool sideBySide = Mathf.Abs(items[i].anchoredPosition.y - items[i - 1].anchoredPosition.y) < 1f;
                if (!sideBySide)
                    Assert.That(lowerTop, Is.LessThanOrEqualTo(upperBottom + 0.5f), items[i].name + " overlaps " + items[i - 1].name);
            }

            float half = consent.sizeDelta.y / 2f;
            float top = items.Max(r => r.anchoredPosition.y + r.sizeDelta.y / 2f);
            float bottom = items.Min(r => r.anchoredPosition.y - r.sizeDelta.y / 2f);
            Assert.That(half - top, Is.GreaterThanOrEqualTo(half * 0.12f), "content runs into the top edge");
            Assert.That(half - top, Is.LessThanOrEqualTo(half * 0.5f), "content is a small block lost in the board");
        }

        // The room must not stand where the lesson happens: the table, its pieces and the cards own that volume.
        [Test] public void NothingInTheLoungeStandsInTheWorkbenchVolume()
        {
            // The toy rack's toys are the lessons' own objects (the orange container, the pastries): their colours are
            // the lessons', not the room's palette (spec 2026-09-29), so the rack is left out of this check.
            foreach (var r in Room().GetComponentsInChildren<Renderer>(true))
            {
                var b = r.bounds;
                bool overlapsTable = b.min.x < 0.9f && b.max.x > -0.9f
                                  && b.min.z < 1.25f && b.max.z > 0.1f
                                  && b.min.y < 1.6f && b.max.y > 0.55f;
                Assert.That(overlapsTable, Is.False, r.name + " stands in the workbench volume");
            }
        }

        // Owner 2026-09-17: the welcome board asks one question. Language and scenery moved behind the gear.
        [Test] public void TheWelcomeBoardKeepsOnlyTheTwoAnswers()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var buttons = n.consentRoot.GetComponentsInChildren<Button>(true).Select(b => b.name).ToArray();
            Assert.That(buttons, Is.EquivalentTo(new[] { "Allow voice", "Skip onboarding" }),   // owner 2026-09-28: Let's begin + Skip onboarding
                "the board should offer voice or no voice and nothing else");
        }

        [Test] public void TheOptionsLiveBehindTheGearOnTheAssistantBar()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var settings = n.GetComponent<LoungeSettings>();
            Assert.That(settings, Is.Not.Null, "run AgentScripts/BuildSettingsPanel.cs");
            Assert.That(settings.panel, Is.Not.Null);
            Assert.That(settings.panel.activeSelf, Is.False, "settings start closed");

            var pills = settings.panel.GetComponentsInChildren<Button>(true).Select(b => b.name).ToArray();
            foreach (var expected in new[] { "Language en", "Language es", "Scenery your room", "Scenery nerdy lounge" })
                Assert.That(pills, Does.Contain(expected), expected + " belongs in the settings card");

            var gear = n.hudRoot.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "Settings gear");
            Assert.That(gear, Is.Not.Null, "the gear sits on the assistant bar");
            bool wired = Enumerable.Range(0, gear.onClick.GetPersistentEventCount())
                .Any(i => gear.onClick.GetPersistentTarget(i) is LoungeSettings && gear.onClick.GetPersistentMethodName(i) == "Toggle");
            Assert.That(wired, Is.True, "the gear opens the settings card");
        }

        // Owner 2026-09-17: the settings control overlapped its neighbour, and a word is not a gear.
        [Test] public void TheBarControlsNeverOverlapAndTheGearIsAnIcon()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var bar = (RectTransform)n.hudRoot.transform;
            var controls = bar.Cast<Transform>().Select(t2 => t2 as RectTransform)
                .Where(r => r != null && r.GetComponent<Button>() != null)   // Music sits in the control row now (compact bar)
                .OrderBy(r => r.anchoredPosition.x).ToArray();

            for (int i = 1; i < controls.Length; i++)
            {
                float leftEdge = controls[i].anchoredPosition.x - controls[i].sizeDelta.x / 2f;
                float priorRight = controls[i - 1].anchoredPosition.x + controls[i - 1].sizeDelta.x / 2f;
                Assert.That(leftEdge, Is.GreaterThanOrEqualTo(priorRight - 0.5f),
                    controls[i].name + " overlaps " + controls[i - 1].name);
            }
            float half = bar.sizeDelta.x / 2f;
            foreach (var r in controls)
                Assert.That(Mathf.Abs(r.anchoredPosition.x) + r.sizeDelta.x / 2f, Is.LessThanOrEqualTo(half),
                    r.name + " hangs off the bar");
            // Owner 2026-09-18: "very compact, not so much space between elements". Neighbouring controls sit one
            // BarGap apart and the bar itself is the compact standard width.
            Assert.That(bar.sizeDelta.x, Is.EqualTo(NerdySpace.BarWidth).Within(0.5f), "compact bar width");
            for (int i = 1; i < controls.Length; i++)
            {
                float gap = (controls[i].anchoredPosition.x - controls[i].sizeDelta.x / 2f) - (controls[i - 1].anchoredPosition.x + controls[i - 1].sizeDelta.x / 2f);
                Assert.That(gap, Is.EqualTo(NerdySpace.BarGap).Within(1f), controls[i].name + " gap from " + controls[i - 1].name);
            }

            // The left half is text: orb, caption, transcript. No button may stand where those draw.
            foreach (var textName in new[] { "Caption", "You said", "State", "Orb" })
            {
                var text = bar.Find(textName) as RectTransform;
                if (text == null) continue;
                float textRight = text.anchoredPosition.x + text.sizeDelta.x / 2f;
                foreach (var r in controls)
                {
                    float left = r.anchoredPosition.x - r.sizeDelta.x / 2f;
                    Assert.That(left, Is.GreaterThanOrEqualTo(textRight - 0.5f), r.name + " stands on the " + textName + " text");
                }
            }

            var gear = controls.FirstOrDefault(r => r.name == "Settings gear");
            Assert.That(gear, Is.Not.Null);
            var icon = gear.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.name == "Gear icon");
            Assert.That(icon, Is.Not.Null, "the gear is drawn, not spelled");
            Assert.That(icon.sprite, Is.Not.Null);
            Assert.That(icon.sprite.name, Does.StartWith("NerdyGear"), "Unity may suffix the sprite name on import");
            Assert.That(gear.GetComponentsInChildren<TMP_Text>(true), Is.Empty, "no leftover text label on the gear");
        }

        [Test] public void TheRoomIsBuiltFromTheStyleTokens()
        {
            var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
            Color[] allowed = { style.baseColor, style.surface, style.line, style.indigo, style.lavender, style.amber, style.magenta, style.orchid, style.cyan };
            // The guide allows the room to shift a token's VALUE (lighter walls, darker floor) but never its hue:
            // "no hue the guide does not list". So compare hue and saturation, not brightness.
            // The toy rack's toys are the lessons' own objects (the orange container, the pastries): their colours are
            // the lessons', not the room's palette (spec 2026-09-29), so the rack is left out of this check.
            foreach (var r in Room().GetComponentsInChildren<Renderer>(true).Where(r => r.GetComponentInParent<ToyRack>(true) == null))
            {
                // A textured surface takes its colour from the texture — and the only one here is the sky gradient,
                // which is generated from these same tokens. Its material tint is plain white by design.
                if (r.sharedMaterial.mainTexture != null) continue;
                var c = r.sharedMaterial.color;
                Color.RGBToHSV(c, out float h, out float s, out _);
                bool known = allowed.Any(a =>
                {
                    Color.RGBToHSV(a, out float ah, out float as_, out _);
                    float dh = Mathf.Abs(Mathf.DeltaAngle(h * 360f, ah * 360f)) / 360f;
                    return (s < 0.06f && as_ < 0.06f) || (dh < 0.03f && Mathf.Abs(s - as_) < 0.2f);
                });
                Assert.That(known, Is.True, r.name + " uses " + ColorUtility.ToHtmlStringRGB(c) + ", whose hue is not a NerdyStyle token");
            }
            foreach (var l in Room().GetComponentsInChildren<Light>(true))
            {
                Color.RGBToHSV(l.color, out float lh, out float ls, out _);
                bool known = ls < 0.06f || allowed.Any(a =>
                {
                    Color.RGBToHSV(a, out float ah, out float as_, out _);
                    return Mathf.Abs(Mathf.DeltaAngle(lh * 360f, ah * 360f)) / 360f < 0.04f && Mathf.Abs(ls - as_) < 0.25f;
                });
                Assert.That(known, Is.True, l.name + " glows in a hue the guide does not list");
            }
        }

        /// Owner 2026-09-18 revision: retire corner arrows; preserve real catalog paging and lesson exit.
        [Test] public void RetiredCornerArrowsCannotReappearButCatalogPagingAndExitRemainWired()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            Assert.That(n.navBack, Is.Not.Null, "run AgentScripts/BuildNavArrows.cs"); Assert.That(n.navNext, Is.Not.Null);
            foreach (var b in new[] { n.navBack, n.navNext })
            {
                string wired = string.Join(",", Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Select(k => b.onClick.GetPersistentMethodName(k)));
                Assert.That(wired, Is.EqualTo(b == n.navBack ? "PressBack" : "PressNext"), b.name);
                Assert.That(b.transform.parent.name, Does.StartWith("Retired control"));
                Assert.That(b.transform.parent.gameObject.activeSelf, Is.False, "even old refresh code cannot expose a hit target");
            }
            Assert.That(n.wall.previousButton, Is.Not.Null); Assert.That(n.wall.nextButton, Is.Not.Null);
            Assert.That(n.wall.previousButton.transform.parent.name, Does.Not.StartWith("Retired control"));
            Assert.That(n.wall.nextButton.transform.parent.name, Does.Not.StartWith("Retired control"));
            Assert.That(n.exitLessonButton, Is.Not.Null);
        }
    

        // Owner 2026-09-18: the age answer can be given from the settings card too, so a learner who skipped the
        // questions can still let Dee listen. One pill, wired to CycleAge, with a live label.
        [Test] public void TheSettingsCardHasAnAgePillWiredToTheDirector()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var settings = n.GetComponent<LoungeSettings>(); Assert.That(settings, Is.Not.Null);
            var row = settings.panel.transform.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(r => r.name == "Age row");
            Assert.That(row, Is.Not.Null, "run AgentScripts/AddAgeRow.cs");
            var pill = row.GetComponentInChildren<Button>(true); Assert.That(pill, Is.Not.Null);
            var sp = n.settingsPanel; Assert.That(sp, Is.Not.Null);
            bool wired = Enumerable.Range(0, pill.onClick.GetPersistentEventCount()).Any(k => pill.onClick.GetPersistentTarget(k) == sp && pill.onClick.GetPersistentMethodName(k) == "CycleAge");
            Assert.That(wired, Is.True, "the Age pill calls SettingsPanel.CycleAge");
            Assert.That(sp.labels.Any(l => l.key == "Age" && l.label != null && l.label.transform.IsChildOf(pill.transform)), Is.True, "the pill shows the current answer");
            Assert.That(n.conversationNotice, Is.Not.Null); Assert.That(n.conversationNotice.rectTransform.sizeDelta.x, Is.GreaterThanOrEqualTo(300f), "room for the age notice");
        }

        // Owner 2026-09-28: a tester switch on the settings card; On = the next launch opens on the wall.
        [Test] public void TheSettingsCardHasASkipOnboardingSwitch()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var settings = n.GetComponent<LoungeSettings>(); Assert.That(settings, Is.Not.Null);
            var row = settings.panel.transform.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(r => r.name == "Skip onboarding row");
            Assert.That(row, Is.Not.Null, "run AgentScripts/AddSkipOnboardingRow.cs");
            var pill = row.GetComponentInChildren<Button>(true); Assert.That(pill, Is.Not.Null);
            var sp = n.settingsPanel;
            bool wired = Enumerable.Range(0, pill.onClick.GetPersistentEventCount()).Any(k => pill.onClick.GetPersistentTarget(k) == sp && pill.onClick.GetPersistentMethodName(k) == "Toggle");
            Assert.That(wired, Is.True, "the pill calls SettingsPanel.Toggle");
            Assert.That(sp.labels.Any(l => l.key == "SkipOnboarding" && l.label != null && l.label.transform.IsChildOf(pill.transform)), Is.True, "the pill reads On/Off");
            var age = settings.panel.transform.GetComponentsInChildren<RectTransform>(true).First(r => r.name == "Age row");
            var actions = settings.panel.transform.GetComponentsInChildren<RectTransform>(true).First(r => r.name == "Actions row");
            Assert.That(age.anchoredPosition.y - NerdySpace.PillHeight / 2f, Is.GreaterThan(actions.anchoredPosition.y + NerdySpace.PillHeight / 2f), "the Age row stays clear of the actions row");
        }


        // Owner 2026-09-28: "the skip onboarding button should be right next to Let's begin".
        [Test] public void TheWelcomeCardHasSkipOnboardingBesideLetsBegin()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var consent = n.consentRoot.transform;
            var begin = consent.Find("Allow voice") as RectTransform; Assert.That(begin, Is.Not.Null);
            var skip = consent.Find("Skip onboarding") as RectTransform; Assert.That(skip, Is.Not.Null, "run AgentScripts/AddSkipOnboardingPill.cs");
            Assert.That(skip.gameObject.activeSelf, Is.True);
            Assert.That(skip.GetComponentInChildren<TMP_Text>(true).text, Is.EqualTo("Skip onboarding"));
            Assert.That(begin.GetComponentInChildren<TMP_Text>(true).text, Is.EqualTo("Let's begin"));
            Assert.That(Mathf.Abs(skip.anchoredPosition.y - begin.anchoredPosition.y), Is.LessThan(1f), "same row");
            Assert.That(skip.anchoredPosition.x, Is.GreaterThan(begin.anchoredPosition.x + begin.sizeDelta.x / 2f), "to the right of Let's begin, not overlapping");
            var pill = skip.GetComponent<Button>();
            bool wired = Enumerable.Range(0, pill.onClick.GetPersistentEventCount()).Any(k => pill.onClick.GetPersistentTarget(k) == n && pill.onClick.GetPersistentMethodName(k) == "SkipOnboardingNow");
            Assert.That(wired, Is.True, "the pill calls NerdyDirector.SkipOnboardingNow");
        }


        // Owner 2026-09-28: a Restart fresh pill in the settings actions row, two presses, wired to SettingsPanel.RestartFresh.
        [Test] public void TheSettingsCardHasARestartFreshPill()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var settings = n.GetComponent<LoungeSettings>();
            var pill = settings.panel.transform.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "Restart fresh");
            Assert.That(pill, Is.Not.Null, "run AgentScripts/AddRestartPill.cs");
            var sp = n.settingsPanel;
            bool wired = Enumerable.Range(0, pill.onClick.GetPersistentEventCount()).Any(k => pill.onClick.GetPersistentTarget(k) == sp && pill.onClick.GetPersistentMethodName(k) == "RestartFresh");
            Assert.That(wired, Is.True);
            Assert.That(sp.labels.Any(l => l.key == "Restart" && l.label != null && l.label.transform.IsChildOf(pill.transform)), Is.True, "the label shows the armed state");
            var row = pill.transform.parent as RectTransform;
            var pills = row.GetComponentsInChildren<Button>(true).Select(b => (RectTransform)b.transform).OrderBy(r => r.anchoredPosition.x).ToArray();
            for (int i = 1; i < pills.Length; i++) Assert.That(pills[i].anchoredPosition.x - pills[i].sizeDelta.x / 2f, Is.GreaterThanOrEqualTo(pills[i - 1].anchoredPosition.x + pills[i - 1].sizeDelta.x / 2f), "actions row pills do not overlap");
        }


        // Toy rack (spec 2026-09-29): fifteen slots, one grabbable toy each, a progress board, all lounge furniture.
        [Test] public void TheLoungeHasAToyRackWithFifteenToysAndAProgressBoard()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            Assert.That(n.toyRack, Is.Not.Null, "run AgentScripts/AddToyRack.cs");
            Assert.That(n.toyRack.slots.Length, Is.EqualTo(15));
            Assert.That(n.toyRack.slots.Select(s => s.id).ToArray(), Is.EqualTo(ToyRackModel.Slots.Select(s => s.Id).ToArray()), "slot order matches the model");
            foreach (var slot in n.toyRack.slots)
            {
                Assert.That(slot.toy, Is.Not.Null, slot.id + " has a toy");
                Assert.That(slot.toy.GetComponentInChildren<Oculus.Interaction.Grabbable>(true), Is.Not.Null, slot.id + " is grabbable");
                // Device log 2026-09-29: a cone (distance) grab on the toy outranks the ray in the hand's interactor group and
                // switches the ray off while it points at the toy: no cursor, no trigger. The ray alone takes a toy from afar.
                Assert.That(slot.toy.GetComponentInChildren<Oculus.Interaction.DistanceGrabInteractable>(true), Is.Null, slot.id + " has no cone grab (it would silence the ray)");
                Assert.That(slot.toy.GetComponentInChildren<Oculus.Interaction.HandGrab.DistanceHandGrabInteractable>(true), Is.Null, slot.id + " has no hand cone grab either");
                var rayProvider = new SerializedObject(slot.toy.GetComponentInChildren<Oculus.Interaction.RayInteractable>(true)).FindProperty("_movementProvider").objectReferenceValue;
                Assert.That(rayProvider, Is.InstanceOf<Oculus.Interaction.MoveTowardsTargetProvider>(), slot.id + " comes to the hand on the trigger, from where the learner stands");
                Assert.That(slot.toy.GetComponent<ToyPlace>(), Is.Not.Null, slot.id + " keeps its place");
                var grabbable = slot.toy.GetComponentInChildren<Oculus.Interaction.Grabbable>(true);
                Assert.That(grabbable.MaxGrabPoints, Is.Not.EqualTo(1), slot.id + " takes two hands");
                Assert.That(slot.toy.GetComponentInChildren<Oculus.Interaction.GrabFreeTransformer>(true), Is.Not.Null, slot.id + " resizes with two hands");
                Assert.That(slot.toy.GetComponentInChildren<Oculus.Interaction.GrabInteractable>(true), Is.Not.Null, slot.id + " can be grabbed up close by the second hand");
                Assert.That(slot.toy.GetComponentInChildren<Oculus.Interaction.RayInteractable>(true), Is.Not.Null, slot.id + " can be grabbed with the trigger from a distance");
                Assert.That(slot.toy.GetComponent<ToyLockedHint>(), Is.Not.Null, slot.id + " explains itself while locked");
                Assert.That(slot.renderers.All(r => r.sharedMaterial == slot.lockedMaterial), Is.True, slot.id + " is grey in the saved scene: nothing is earned yet");
                Assert.That(slot.toy.GetComponentInChildren<Oculus.Interaction.Grabbable>(true).enabled, Is.False, slot.id + " cannot be grabbed while locked");
                Assert.That(slot.toy.transform.localScale.x, Is.EqualTo(slot.toy.GetComponent<ToyPlace>().homeScale).Within(0.01f), slot.id + " sits at its built size");
                Assert.That(slot.toy.GetComponent<ToyPlace>().homeScale, Is.GreaterThan(1.5f), slot.id + " is built big");
                Assert.That(slot.toy.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0), slot.id + " is visible");
            }
            Assert.That(n.progressBoard, Is.Not.Null); Assert.That(n.progressBoard.rows.Length, Is.EqualTo(3));
            Assert.That(n.progressBoard.rows.All(r => r.marks.Length == 5 && r.title != null && r.count != null), Is.True);
            var room = n.lounge; Assert.That(room, Is.Not.Null);
            Assert.That(n.toyRack.transform.IsChildOf(room.furniture.transform), Is.True, "hidden with the furniture in Your room");
            var distance = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Oculus.Interaction.DistanceGrabInteractor>(true)).ToArray();
            Assert.That(distance.Length, Is.GreaterThanOrEqualTo(2), "the rig keeps its distance-grab interactor per controller (lessons may use it)");
        }


        // Owner 2026-09-29: "a mute/unmute button next to the pause button".
        [Test] public void DeesBarHasMuteBesidePlayStop()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            Assert.That(n.conversationButton, Is.Not.Null);
            var bar = n.conversationButton.transform.parent;
            var mute = bar.Find("Mute"); Assert.That(mute, Is.Not.Null, "run AgentScripts/AddBarMute.cs");
            Assert.That(mute.gameObject.activeSelf, Is.True);
            var pill = mute.GetComponent<Button>();
            bool wired = Enumerable.Range(0, pill.onClick.GetPersistentEventCount()).Any(k => pill.onClick.GetPersistentTarget(k) == n && pill.onClick.GetPersistentMethodName(k) == "ToggleMute");
            Assert.That(wired, Is.True, "the pill calls NerdyDirector.ToggleMute");
            Assert.That(n.barMuteLabel, Is.Not.Null); Assert.That(n.barMuteLabel.transform.IsChildOf(mute), Is.True); Assert.That(n.barMuteLabel.text, Is.EqualTo("Mute"));
            var m = (RectTransform)mute; var c = (RectTransform)n.conversationButton.transform;
            Assert.That(Mathf.Abs(m.anchoredPosition.y - c.anchoredPosition.y), Is.LessThan(1f), "same row as Play/Stop");
            float gap = Mathf.Abs(m.anchoredPosition.x - c.anchoredPosition.x) - (m.sizeDelta.x + c.sizeDelta.x) / 2f;
            Assert.That(gap, Is.GreaterThanOrEqualTo(0f).And.LessThan(30f), "right next to it, not overlapping");
        }

        [Test] public void TheSettingsCardHasAnExitAppPill()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var settings = n.GetComponent<LoungeSettings>();
            var pill = settings.panel.transform.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name == "Exit app");
            Assert.That(pill, Is.Not.Null, "run AgentScripts/AddExitPill.cs");
            var sp = n.settingsPanel;
            bool wired = Enumerable.Range(0, pill.onClick.GetPersistentEventCount()).Any(k => pill.onClick.GetPersistentTarget(k) == sp && pill.onClick.GetPersistentMethodName(k) == "ExitApp");
            Assert.That(wired, Is.True);
            Assert.That(sp.labels.Any(l => l.key == "Exit" && l.label != null && l.label.transform.IsChildOf(pill.transform)), Is.True, "the label shows the armed state");
            var row = pill.transform.parent as RectTransform;
            var pills = row.GetComponentsInChildren<Button>(true).Select(b => (RectTransform)b.transform).OrderBy(r => r.anchoredPosition.x).ToArray();
            Assert.That(pills.Length, Is.EqualTo(5));
            for (int i = 1; i < pills.Length; i++) Assert.That(pills[i].anchoredPosition.x - pills[i].sizeDelta.x / 2f, Is.GreaterThanOrEqualTo(pills[i - 1].anchoredPosition.x + pills[i - 1].sizeDelta.x / 2f), "actions row pills do not overlap");
        }

    }
}
