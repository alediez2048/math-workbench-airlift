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
            Assert.That(buttons, Is.EquivalentTo(new[] { "Allow voice", "No voice" }),
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
            foreach (var r in Room().GetComponentsInChildren<Renderer>(true))
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
    }
}
