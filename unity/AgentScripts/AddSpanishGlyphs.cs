using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

// Owner 2026-09-17: Dee can speak Spanish, so the Nerdy caption and label fonts need Spanish letters. Adds them to the
// static SDF atlases in place (same assets, materials and references): switch to dynamic with the source font, add the
// characters, switch back to static. Idempotent.
// Run: unity command run_script --file AgentScripts/AddSpanishGlyphs.cs --entry AddSpanishGlyphs.Run
public static class AddSpanishGlyphs
{
    const string Spanish = "áéíóúüñÁÉÍÓÚÜÑ¡¿";

    public static string Run()
    {
        var report = new List<string>();
        foreach (var name in new[] { "Poppins-Regular", "Poppins-Medium", "Poppins-MediumItalic", "Poppins-SemiBold", "Karla-Medium", "Karla-Bold" })
        {
            string path = "Assets/Airlift/Fonts/Nerdy/" + name + " SDF.asset";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path) ?? throw new InvalidOperationException("Font missing: " + path);
            if (font.HasCharacters(Spanish)) { report.Add(name + " already complete"); continue; }
            var so = new SerializedObject(font);
            var source = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(so.FindProperty("m_SourceFontFileGUID").stringValue))
                ?? throw new InvalidOperationException("Source font missing for " + name);
            so.FindProperty("m_SourceFontFile").objectReferenceValue = source;
            so.FindProperty("m_AtlasPopulationMode").intValue = (int)AtlasPopulationMode.Dynamic;
            so.ApplyModifiedPropertiesWithoutUndo();
            font.ReadFontAssetDefinition();
            bool added = font.TryAddCharacters(Spanish, out string missing);
            so.Update();
            so.FindProperty("m_AtlasPopulationMode").intValue = (int)AtlasPopulationMode.Static;
            so.FindProperty("m_SourceFontFile").objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(font);
            foreach (var atlas in font.atlasTextures) if (atlas != null) EditorUtility.SetDirty(atlas);
            report.Add(name + (added ? " +" + Spanish.Length : " missing " + missing));
        }
        AssetDatabase.SaveAssets();
        return string.Join("; ", report);
    }
}
