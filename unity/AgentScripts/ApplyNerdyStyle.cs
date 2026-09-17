using System;
using System.IO;
using Airlift.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;

// CC-P0-02: import Poppins/Karla (OFL) as static TMP fonts, generate token sprites, create the
// NerdyStyle asset. Scenes are not restyled here; the welcome/catalog builders consume the asset.
public static class ApplyNerdyStyle
{
    const string FontDir = "Assets/Airlift/Fonts/Nerdy/";
    const string SpriteDir = "Assets/Airlift/Sprites/";
    const string Glyphs = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz .,;:!?'-/()[]+−=<>×÷·é—–“”’&%@#";

    public static string Run()
    {
        string stylePath = "Assets/Airlift/Fonts/NerdyStyle.asset";
        var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>(stylePath);
        if (style == null) { style = ScriptableObject.CreateInstance<NerdyStyle>(); AssetDatabase.CreateAsset(style, stylePath); }
        style.displayFont = Font("Poppins-Medium"); style.displayItalic = Font("Poppins-MediumItalic");
        style.bodyFont = Font("Poppins-Regular"); style.bodySemibold = Font("Poppins-SemiBold");
        style.altFont = Font("Karla-Medium"); style.altBold = Font("Karla-Bold");
        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Application.dataPath), SpriteDir));
        style.pill = RoundedSprite("NerdyPill", 128, 60);
        style.card = RoundedSprite("NerdyCard", 96, 20);
        style.small = RoundedSprite("NerdySmall", 64, 12);
        style.brandGradient = GradientSprite("NerdyBrandGradient", new[] { (0f, NerdyStyle.Hex("#3C4CDB")), (1f, NerdyStyle.Hex("#9E97FF")) });
        style.spectrumGradient = GradientSprite("NerdySpectrumGradient", new[] { (0.08f, NerdyStyle.Hex("#FFC32B")), (0.42f, NerdyStyle.Hex("#FB43DA")), (0.76f, NerdyStyle.Hex("#D684FF")), (0.97f, NerdyStyle.Hex("#17E2EA")) });
        style.glass = GradientSprite("NerdyGlass", new[] { (0f, new Color(79 / 255f, 77 / 255f, 93 / 255f, 0.2f)), (1f, new Color(1f, 1f, 1f, 0.08f)) });
        EditorUtility.SetDirty(style); AssetDatabase.SaveAssets();
        return "NerdyStyle asset: 6 fonts, 6 sprites, tokens.";
    }

    static TMP_FontAsset Font(string name)
    {
        string path = FontDir + name + " SDF.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (existing != null) return existing;
        var source = AssetDatabase.LoadAssetAtPath<Font>(FontDir + name + ".ttf");
        if (source == null) throw new InvalidOperationException("Font not imported: " + name);
        var font = TMP_FontAsset.CreateFontAsset(source, 72, 8, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        font.name = name + " SDF";
        if (!font.TryAddCharacters(Glyphs, out string missing)) throw new InvalidOperationException(name + " missing glyphs: " + missing);
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        font.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
        AssetDatabase.CreateAsset(font, path);
        AssetDatabase.AddObjectToAsset(font.material, font);
        foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, font);
        EditorUtility.SetDirty(font); return font;
    }

    static Sprite RoundedSprite(string name, int size, int corner)
    {
        string path = SpriteDir + name + ".png";
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path); if (existing != null) return existing;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            float cx = Mathf.Clamp(x + 0.5f, corner, size - corner), cy = Mathf.Clamp(y + 0.5f, corner, size - corner);
            float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
            tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(corner - dist + 0.5f)));
        }
        return Save(tex, path, new Vector4(corner, corner, corner, corner));
    }

    static Sprite GradientSprite(string name, (float t, Color c)[] stops)
    {
        string path = SpriteDir + name + ".png";
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path); if (existing != null) return existing;
        const int w = 256, h = 8; var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int x = 0; x < w; x++)
        {
            float t = x / (float)(w - 1); Color c = stops[0].c;
            for (int i = 0; i < stops.Length - 1; i++)
                if (t >= stops[i].t && t <= stops[i + 1].t) { c = Color.Lerp(stops[i].c, stops[i + 1].c, Mathf.InverseLerp(stops[i].t, stops[i + 1].t, t)); break; }
            if (t < stops[0].t) c = stops[0].c; if (t > stops[stops.Length - 1].t) c = stops[stops.Length - 1].c;
            for (int y = 0; y < h; y++) tex.SetPixel(x, y, c);
        }
        return Save(tex, path, Vector4.zero);
    }

    static Sprite Save(Texture2D tex, string path, Vector4 border)
    {
        tex.Apply(); File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(Application.dataPath), path), tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.spriteBorder = border;
        importer.spritePixelsPerUnit = 100; importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? throw new InvalidOperationException("Sprite import failed: " + path);
    }
}
