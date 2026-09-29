using UnityEditor;
using UnityEngine;

// CC-HF-01. Android ASTC overrides and size caps for tile and generated art; tiles import as opaque sprites.
// Idempotent. Run after every art import.
// Run: unity command run_script --file AgentScripts/ApplyArtImportSettings.cs --entry ApplyArtImportSettings.Run
public static class ApplyArtImportSettings
{
    static readonly string[] Roots = { "Assets/Airlift/Art/Tiles", "Assets/Airlift/Art/Generated" };

    public static string Run()
    {
        int changed = 0, seen = 0;
        foreach (var g in AssetDatabase.FindAssets("t:Texture2D", System.Array.FindAll(Roots, AssetDatabase.IsValidFolder)))
        {
            string path = AssetDatabase.GUIDToAssetPath(g); seen++;
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            bool tile = path.Contains("/Art/Tiles/");
            bool dee = path.Contains("/Generated/Dee/");     // a transparent character sprite, drawn small on the bar
            bool card = path.Contains("/Generated/Card/");   // opaque lesson-card art behind a scrim
            int max = tile ? 1024 : dee ? 256 : path.Contains("/Generated/Lounge/") ? 2048 : path.Contains("/Generated/Deck/") || path.Contains("/Generated/Card/") ? 1024 : 512;
            var format = tile || dee ? TextureImporterFormat.ASTC_4x4 : TextureImporterFormat.ASTC_6x6;
            var android = importer.GetPlatformTextureSettings("Android");
            bool dirty = !android.overridden || android.maxTextureSize != max || android.format != format;
            if (tile && (importer.textureType != TextureImporterType.Sprite || importer.alphaIsTransparency)) dirty = true;
            if (dee && (importer.textureType != TextureImporterType.Sprite || !importer.alphaIsTransparency)) dirty = true;
            if (card && importer.textureType != TextureImporterType.Sprite) dirty = true;
            if (!dirty) continue;
            if (tile) { importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = false; importer.mipmapEnabled = true; }
            if (dee) { importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true; importer.mipmapEnabled = true; }
            if (card) { importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = false; importer.mipmapEnabled = true; }
            android.overridden = true; android.maxTextureSize = max; android.format = format; android.compressionQuality = 50;
            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport(); changed++;
        }
        return changed + " of " + seen + " art textures updated";
    }
}
