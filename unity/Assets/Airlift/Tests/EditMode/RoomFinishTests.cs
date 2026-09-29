using System.Linq;
using Airlift.Lounge;
using Airlift.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Airlift.Tests
{
    /// CC-HF-03. The Nerdy lounge reads as a finished room: fluted acoustic wall panels, a herringbone floor (both
    /// generated, grey, tinted by the tokens so the palette is unchanged) and a darker lower panel with a lavender rail
    /// on every solid wall. Same room, same size. Run AgentScripts/ApplyRoomFinish.cs after BuildLounge.
    public class RoomFinishTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        const string Rerun = ": run AgentScripts/ApplyRoomFinish.cs";

        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        static LoungeRoom Room() => SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<LoungeRoom>(true)).First();

        static void AssertTokenHue(Color c, string what)
        {
            var s = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
            Color[] allowed = { s.baseColor, s.surface, s.line, s.indigo, s.lavender };
            Color.RGBToHSV(c, out float h, out float sat, out _);
            bool known = allowed.Any(a =>
            {
                Color.RGBToHSV(a, out float ah, out float asat, out _);
                return Mathf.Abs(Mathf.DeltaAngle(h * 360f, ah * 360f)) < 11f && Mathf.Abs(sat - asat) < 0.2f;
            });
            Assert.That(known, Is.True, what + " is tinted " + ColorUtility.ToHtmlStringRGB(c) + ", not a token hue");
        }

        [Test] public void SolidWallsAreFlutedPanelsAndTheFloorIsHerringbone()
        {
            var shell = Room().shell.transform;
            var walls = Enumerable.Range(0, 12).Select(i => shell.Find("Wall " + i)).Where(t => t != null).ToArray();
            Assert.That(walls.Length, Is.GreaterThanOrEqualTo(6), "run AgentScripts/BuildLounge.cs");
            foreach (var (t, file) in walls.Select(w => (w, "Wall.png")).Append((shell.Find("Floor"), "Floor.png")))
            {
                var m = t.GetComponent<MeshRenderer>().sharedMaterial;
                var tex = m.GetTexture("_BaseMap");
                Assert.That(tex, Is.Not.Null, t.name + " is a flat colour" + Rerun);
                Assert.That(AssetDatabase.GetAssetPath(tex), Is.EqualTo("Assets/Airlift/Art/Generated/Room/" + file), t.name);
                Assert.That(m.GetTextureScale("_BaseMap").x, Is.GreaterThan(1.5f), t.name + ": tiled at a real-world scale, not one stretched swatch");
                AssertTokenHue(m.GetColor("_BaseColor"), t.name);
            }
        }

        [Test] public void EverySolidWallHasALowerPanelAndARail()
        {
            var shell = Room().shell.transform;
            foreach (var wall in Enumerable.Range(0, 12).Select(i => shell.Find("Wall " + i)).Where(t => t != null))
            {
                string i = wall.name.Substring(5);
                var panel = shell.Find("Wainscot " + i); var rail = shell.Find("Rail " + i);
                Assert.That(panel, Is.Not.Null, wall.name + Rerun);
                Assert.That(rail, Is.Not.Null, wall.name + Rerun);
                Assert.That(Vector3.Dot(panel.position - wall.position, wall.forward), Is.LessThan(0f), "on the inside of the wall");
                AssertTokenHue(panel.GetComponent<MeshRenderer>().sharedMaterial.color, panel.name);
                AssertTokenHue(rail.GetComponent<MeshRenderer>().sharedMaterial.color, rail.name);
            }
        }

        [Test] public void RoomTexturesFitTheQuestBudget()
        {
            foreach (var (file, cap) in new[] { ("Wall.png", 512), ("Floor.png", 512) })
            {
                var imp = (TextureImporter)AssetImporter.GetAtPath("Assets/Airlift/Art/Generated/Room/" + file);
                Assert.That(imp, Is.Not.Null, file + Rerun);
                var android = imp.GetPlatformTextureSettings("Android");
                Assert.That(android.overridden, Is.True, file);
                Assert.That(android.maxTextureSize, Is.LessThanOrEqualTo(cap), file);
                Assert.That(imp.wrapMode, Is.EqualTo(TextureWrapMode.Repeat), file + " tiles");
            }
        }
    }
}
