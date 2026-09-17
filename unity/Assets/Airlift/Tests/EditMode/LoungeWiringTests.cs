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

        [Test] public void TheSceneryChoiceIsOnTheCardAndWired()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects()
                .SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var buttons = n.consentRoot.GetComponentsInChildren<Button>(true).Where(b => b.name.StartsWith("Scenery ")).ToArray();
            Assert.That(buttons.Length, Is.EqualTo(2), "Your room and Nerdy lounge");
            string[] methods = { "ChooseYourRoom", "ChooseNerdyLounge" };
            foreach (var m in methods)
                Assert.That(buttons.Any(b => Enumerable.Range(0, b.onClick.GetPersistentEventCount())
                    .Any(i => b.onClick.GetPersistentTarget(i) is LoungeRoom && b.onClick.GetPersistentMethodName(i) == m)),
                    Is.True, m + " is wired to a pill");
            Assert.That(n.consentRoot.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text == "Where you learn"), Is.True);
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
    }
}
