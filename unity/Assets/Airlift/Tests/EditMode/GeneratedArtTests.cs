using System.Linq;
using Airlift.Presentation.Dashboard;
using Airlift.Welcome;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Airlift.Tests
{
    /// CC-HF-01, scene side. Run AgentScripts/ApplyComingSoonArt.cs (after any BuildDashboard.cs) and
    /// AgentScripts/ApplyLoungePanorama.cs (after any BuildLounge.cs) before this suite.
    public class GeneratedArtTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        static DashboardWall Wall() => SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First().catalogRoot.GetComponent<DashboardWall>();

        [Test] public void ComingSoonTilesShowTheirWorldDimmedAndInert()
        {
            var soon = Wall().tiles.Where(t => !t.openable).ToList();
            Assert.That(soon.Count, Is.EqualTo(DashboardCatalog.ComingSoon.Length));
            foreach (var t in soon)
            {
                var photo = t.transform.Find("Art/Photo");
                Assert.That(photo, Is.Not.Null, t.id + ": run AgentScripts/ApplyComingSoonArt.cs");
                var img = photo.GetComponent<Image>();
                Assert.That(AssetDatabase.GetAssetPath(img.sprite), Is.EqualTo("Assets/Airlift/Art/Tiles/" + t.id + ".png"), t.id);
                Assert.That(img.color.a, Is.LessThanOrEqualTo(0.6f), t.id + " reads as not yet playable");
                Assert.That(img.raycastTarget, Is.False, t.id);
                Assert.That(t.GetComponent<TileHover>(), Is.Null, t.id + " stays inert");
                var badge = t.transform.Find("Badge");
                Assert.That(badge != null && badge.gameObject.activeSelf, Is.True, t.id + " keeps its badge (run AgentScripts/BuildDashboard.cs)");
                Assert.That(badge.GetComponentInChildren<TMPro.TMP_Text>(true).text, Is.EqualTo("COMING SOON"), t.id);
                foreach (var arrow in new[] { "Arrow ring", "Arrow inner", "Arrow" })
                {
                    var a = t.transform.Find(arrow);
                    Assert.That(a == null || !a.gameObject.activeSelf, Is.True, t.id + ": no " + arrow + " on a tile that cannot open");
                }
            }
        }

        [Test] public void DeeFaceSitsOnTheBarOrbUntintedAndInert()
        {
            var n = SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            Assert.That(n.orb, Is.Not.Null, "run AgentScripts/CompactAssistantBar.cs");
            var face = n.orb.transform.Find("Dee face");
            Assert.That(face, Is.Not.Null, "run AgentScripts/ApplyDeeFace.cs (after CompactAssistantBar)");
            var img = face.GetComponent<Image>();
            Assert.That(AssetDatabase.GetAssetPath(img.sprite), Is.EqualTo("Assets/Airlift/Art/Generated/Dee/Dee-icon.png"));
            Assert.That(img.color, Is.EqualTo(Color.white), "the state tint stays on the Orb ring, never on Dee");
            Assert.That(img.raycastTarget, Is.False, "Dee never eats a bar press");
            Assert.That(img.preserveAspect, Is.True);
            var orbSize = ((RectTransform)n.orb.transform).rect.size;
            var faceSize = ((RectTransform)face).rect.size;
            Assert.That(faceSize.x, Is.LessThan(orbSize.x).And.GreaterThan(orbSize.x * 0.6f), "a ring of the Orb shows around Dee");
        }

        const string Panorama = "Assets/Airlift/Art/Generated/Lounge/LoungePanorama.png";

        [Test] public void LoungeSkyUsesThePanoramaWrappingSideways()
        {
            var sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/Lounge/LoungeSky.mat");
            Assert.That(AssetDatabase.GetAssetPath(sky.GetTexture("_MainTex")), Is.EqualTo(Panorama), "run AgentScripts/ApplyLoungePanorama.cs");
            var importer = (TextureImporter)AssetImporter.GetAtPath(Panorama);
            Assert.That(importer.wrapModeU, Is.EqualTo(TextureWrapMode.Repeat));
            Assert.That(importer.wrapModeV, Is.EqualTo(TextureWrapMode.Clamp));
            Assert.That(importer.mipmapEnabled, Is.False, "no mip seam at the wrap");
        }

        [Test] public void WindowPanesAreFaintGlassNotACopyOfTheSky()
        {
            var pane = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/Lounge/LoungeSkyPane.mat");
            const string rerun = ": run AgentScripts/ApplyLoungePanorama.cs";
            Assert.That(pane.mainTexture, Is.Null, "the real skybox shows through the window" + rerun);
            Assert.That(pane.renderQueue, Is.GreaterThanOrEqualTo(3000), "transparent queue" + rerun);
            Assert.That(pane.GetFloat("_Surface"), Is.EqualTo(1f), "URP surface type Transparent" + rerun);
            Assert.That(pane.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"), Is.True, "transparent keyword" + rerun);
            Assert.That(pane.GetTag("RenderType", false), Is.EqualTo("Transparent"), "RenderType tag" + rerun);
            Assert.That(pane.GetShaderPassEnabled("DepthOnly"), Is.False, "no depth prepass, or the pane hides the sky" + rerun);
            Assert.That(pane.GetShaderPassEnabled("ShadowCaster"), Is.False, "glass casts no shadow" + rerun);
            Assert.That(pane.GetColor("_BaseColor").a, Is.LessThanOrEqualTo(0.2f), "faint" + rerun);
        }

        [Test] public void WindowPaneIsSavedTransparentNotOnlyRepairedInMemory()
        {
            // URP re-validates materials when it loads them, so the in-memory checks above can pass while the saved
            // asset (what a build reimports) still says Opaque with a depth pass. Read the file itself.
            string yaml = System.IO.File.ReadAllText("Assets/Airlift/Materials/Lounge/LoungeSkyPane.mat");
            const string rerun = ": run AgentScripts/ApplyLoungePanorama.cs";
            Assert.That(yaml, Does.Contain("RenderType: Transparent"), "saved RenderType tag" + rerun);
            int passes = yaml.IndexOf("disabledShaderPasses:", System.StringComparison.Ordinal);
            Assert.That(passes, Is.GreaterThan(0), "saved pass list" + rerun);
            string passList = yaml.Substring(passes, System.Math.Min(200, yaml.Length - passes));
            Assert.That(passList, Does.Contain("DepthOnly"), "saved without a depth prepass" + rerun);
            Assert.That(passList, Does.Contain("SHADOWCASTER").Or.Contain("ShadowCaster"), "saved without a shadow pass" + rerun);
        }

        static readonly (string mat, string map)[] Details =
        {
            ("Assets/Airlift/Materials/GardenSoil.mat", "soil"), ("Assets/Airlift/Materials/GardenPeat.mat", "soil"),
            ("Assets/Airlift/Materials/GardenWood.mat", "wood"), ("Assets/Airlift/Materials/Lounge/LoungeWood.mat", "wood"),
            ("Assets/Airlift/Materials/Lounge/LoungeRug.mat", "rug"), ("Assets/Airlift/Materials/Lounge/LoungeRugInner.mat", "rug"),
            ("Assets/Airlift/Materials/Lounge/LoungeCouch.mat", "fabric"),
        };

        [Test] public void DetailMapsTintTheirMaterialsWithoutReplacingTheColour()
        {
            foreach (var (matPath, map) in Details)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                Assert.That(AssetDatabase.GetAssetPath(mat.GetTexture("_BaseMap")), Is.EqualTo("Assets/Airlift/Art/Generated/Detail/" + map + ".png"), matPath + ": run AgentScripts/ApplyDetailTextures.cs");
                Assert.That(mat.GetTextureScale("_BaseMap").x, Is.GreaterThanOrEqualTo(1f), matPath);
            }
        }
    }
}
