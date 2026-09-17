using UnityEditor;
// Owner bug 2026-09-17: the consent-card logo vanished. The launcher-icon attempt (L-4) re-imported the logo as a
// Default texture, which drops the sprite the consent card's Logo image references. Restore the Sprite import.
// Run: unity command run_script --file AgentScripts/FixLogoImport.cs --entry FixLogoImport.Run
public static class FixLogoImport
{
    public static string Run()
    {
        const string path = "Assets/Airlift/Branding/nerdy-logo-green.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        var before = importer.textureType;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        return "logo import " + before + " -> " + importer.textureType;
    }
}
