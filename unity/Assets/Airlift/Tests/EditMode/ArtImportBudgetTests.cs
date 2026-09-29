using NUnit.Framework;
using UnityEditor;

namespace Airlift.Tests
{
    /// CC-HF-01. Generated and tile art ships compressed for Quest. Run AgentScripts/ApplyArtImportSettings.cs after any import.
    public class ArtImportBudgetTests
    {
        static readonly string[] Roots = { "Assets/Airlift/Art/Tiles", "Assets/Airlift/Art/Generated" };

        static (int max, TextureImporterFormat format) Budget(string path) =>
            path.Contains("/Art/Tiles/") ? (1024, TextureImporterFormat.ASTC_4x4)
            : path.Contains("/Generated/Lounge/") ? (2048, TextureImporterFormat.ASTC_6x6)
            : path.Contains("/Generated/Dee/") ? (256, TextureImporterFormat.ASTC_4x4)
            : path.Contains("/Generated/Deck/") ? (1024, TextureImporterFormat.ASTC_6x6)
            : path.Contains("/Generated/Card/") ? (1024, TextureImporterFormat.ASTC_6x6)
            : (512, TextureImporterFormat.ASTC_6x6);

        [Test] public void EveryArtTextureHasAnAndroidAstcOverrideWithinItsCap()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", System.Array.FindAll(Roots, AssetDatabase.IsValidFolder));
            Assert.That(guids, Is.Not.Empty, "the 15 chapter tiles at least");
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                var android = importer.GetPlatformTextureSettings("Android");
                var (max, format) = Budget(path);
                Assert.That(android.overridden, Is.True, path + ": run AgentScripts/ApplyArtImportSettings.cs");
                Assert.That(android.format, Is.EqualTo(format), path);
                Assert.That(android.maxTextureSize, Is.LessThanOrEqualTo(max), path);
            }
        }

        [Test] public void DeeImportsAsATransparentSprite()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Airlift/Art/Generated/Dee" });
            Assert.That(guids, Is.Not.Empty, "Dee-icon.png at least");
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), path + ": run AgentScripts/ApplyArtImportSettings.cs");
                Assert.That(importer.alphaIsTransparency, Is.True, path + " keeps its transparent edges");
            }
        }

        [Test] public void TileArtImportsAsOpaqueSprites()
        {
            foreach (var g in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Airlift/Art/Tiles" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), path);
                Assert.That(importer.alphaIsTransparency, Is.False, path);
            }
        }
    }
}
