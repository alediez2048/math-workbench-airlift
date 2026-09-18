using System.Linq;
using Airlift.Lounge;
using Airlift.Presentation;
using Airlift.Presentation.Dashboard;
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
    /// CC-FD-05a/05b, scene side: the pointing tour. Run AgentScripts/BuildTour.cs (after BuildDashboard,
    /// BuildNavArrows, CompactAssistantBar) before this suite.
    public class RundownWiringTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        static NerdyDirector Director() => SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
        static string Wired(Button b) => string.Join(",", Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Select(i => (b.onClick.GetPersistentTarget(i) != null ? b.onClick.GetPersistentTarget(i).GetType().Name : "null") + "." + b.onClick.GetPersistentMethodName(i)));

        [Test] public void TheTourIsWiredToTheWallTheBarTheArrowsAndSettings()
        {
            var n = Director(); var r = n.rundown;
            Assert.That(r, Is.Not.Null, "run AgentScripts/BuildTour.cs");
            Assert.That(r.wall, Is.EqualTo(n.wall));
            Assert.That(r.pointerRoot, Is.Not.Null); Assert.That(r.ring, Is.Not.Null); Assert.That(r.arrow, Is.Not.Null);
            Assert.That(r.header, Is.Not.Null); Assert.That(r.stepLabel, Is.Not.Null);
            Assert.That(Wired(r.skipButton), Is.EqualTo("LoungeRundown.Skip"), "Skip is a real pill, visible on every stop");
            Assert.That(r.barControls.Select(b => b.name), Is.EquivalentTo(new[] { "Music", "Settings gear", "Help" }));
            Assert.That(r.pagerAndScenery.Length, Is.EqualTo(6), "‹ 1/3 › and the scenery selector");
            Assert.That(r.backArrow, Is.EqualTo(n.navBack)); Assert.That(r.nextArrow, Is.EqualTo(n.navNext)); Assert.That(r.settings, Is.EqualTo(n.loungeSettings));
            Assert.That(r.gearButton, Is.Not.Null); Assert.That(r.gearButton.name, Is.EqualTo("Settings gear"));
            Assert.That(r.consentPills.Select(b => b.name), Is.EquivalentTo(new[] { "Allow voice", "No voice" }), "stop 1 points at the two pills");
            Assert.That(r.header.transform.parent, Is.EqualTo(n.hudWelcomeCanvas), "the header shows on every screen, not only the wall");
        }

        [Test] public void TheCardEraIsGone()
        {
            var n = Director();
            Assert.That(n.hudWelcomeCanvas.Find("Rundown card"), Is.Null, "the text card is replaced by pointing");
            Assert.That(n.transform.Find("Rundown block"), Is.Null, "no hands-on block in the tour-only version");
            Assert.That(n.loungeSettings.hideWhileOpen.All(g => g != null), Is.True);
        }

        [Test] public void ThePointerDrawsLastAndNeverTakesAPress()
        {
            var n = Director(); var pointer = n.rundown.pointerRoot;
            Assert.That(pointer.parent, Is.EqualTo(n.hudWelcomeCanvas), "on the welcome canvas, in canvas units");
            Assert.That(pointer.GetSiblingIndex(), Is.EqualTo(n.hudWelcomeCanvas.childCount - 1), "drawn over the bar so the ring can circle the gear");
            Assert.That(n.rundown.header.transform.GetSiblingIndex(), Is.GreaterThan(n.hudRoot.transform.GetSiblingIndex()), "Skip draws over the cards");
            foreach (var g in pointer.GetComponentsInChildren<Graphic>(true)) Assert.That(g.raycastTarget, Is.False, g.name + " must not swallow the press it points at");
            Assert.That(n.rundown.ring.GetComponent<Image>().color, Is.EqualTo(AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset").cyan), "the guide's focus ring");
            Assert.That(pointer.gameObject.activeSelf, Is.False); Assert.That(n.rundown.header.activeSelf, Is.False);
        }

        [Test] public void EveryStopLandsTheRingOnItsTargetInsideThePanel()
        {
            var n = Director(); var r = n.rundown;
            n.catalogRoot.SetActive(true); r.wall.Refresh(new LibraryState());
            n.hudRoot.SetActive(true); n.consentRoot.SetActive(true);
            for (int i = 0; i < RundownScript.StopCount; i++)
            {
                r.PreviewStep(i);
                Assert.That(r.pointerRoot.gameObject.activeSelf, Is.True, "stop " + (i + 1) + " points at something");
                var ring = r.ring;
                Assert.That(ring.sizeDelta.x, Is.GreaterThan(40f).And.LessThan(NerdySpace.PanelWidth), "stop " + (i + 1) + " ring width " + ring.sizeDelta.x);
                Assert.That(Mathf.Abs(ring.anchoredPosition.x) + ring.sizeDelta.x / 2f, Is.LessThanOrEqualTo(NerdySpace.PanelWidth / 2f + 1f), "stop " + (i + 1) + " ring inside the panel");
                Assert.That(Mathf.Abs(ring.anchoredPosition.y) + ring.sizeDelta.y / 2f, Is.LessThanOrEqualTo(NerdySpace.PanelHeight / 2f + 1f), "stop " + (i + 1) + " ring inside the panel");
                Assert.That(r.stepLabel.text, Is.EqualTo((i + 1) + " OF 6"));
            }
            // Stop 3 circles the first tile; stop 1 the two pills; stop 6 the ‹ arrow.
            r.PreviewStep(2);
            var first = (RectTransform)r.wall.Visible.First().transform;
            Assert.That(r.ring.anchoredPosition.x, Is.EqualTo(first.anchoredPosition.x).Within(2f)); Assert.That(r.ring.sizeDelta.x, Is.EqualTo(first.sizeDelta.x + 2f * r.ringPadding).Within(2f));
            n.catalogRoot.SetActive(false); n.consentRoot.SetActive(true);
            r.PreviewStep(0);
            Assert.That(r.ring.sizeDelta.x, Is.GreaterThan(500f), "both pills inside one ring");
            n.consentRoot.SetActive(false); n.catalogRoot.SetActive(true);
            r.PreviewStep(5);
            Assert.That(r.ring.anchoredPosition.x, Is.GreaterThan(NerdySpace.PanelWidth / 2f - 200f), "the ‹ arrow sits in the bottom-right corner");
        }

        [Test] public void DeesBarDrawsAboveEveryCardSoTheGearCanAlwaysBePressed()
        {
            var n = Director();
            var bar = n.hudRoot.transform; var canvas = bar.parent;
            foreach (Transform card in canvas)
            {
                if (card == bar || card == n.rundown.pointerRoot || card.GetComponent<Image>() == null) continue;
                Assert.That(bar.GetSiblingIndex(), Is.GreaterThan(card.GetSiblingIndex()), card.name + " draws over the bar and blocks its buttons");
            }
            Assert.That(n.loungeSettings.hideWhileOpen, Does.Contain(n.hudRoot));
        }
    }
}
