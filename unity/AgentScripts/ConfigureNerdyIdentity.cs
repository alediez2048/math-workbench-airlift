using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Android;
using UnityEngine;

// CC-P0-01: app identity "Nerdy", package com.nerdy.vr, owner logo as launcher icon.
public static class ConfigureNerdyIdentity
{
    const string Branding = "Assets/Airlift/Branding/";
    public static string Run()
    {
        foreach (var file in new[] { "icon-adaptive-bg.png", "icon-adaptive-fg.png", "icon-legacy.png", "icon-store.png", "nerdy-logo-green.png" })
        {
            string path = Branding + file;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new InvalidOperationException("Missing branding asset " + path);
            importer.textureType = TextureImporterType.Default; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.npotScale = TextureImporterNPOTScale.None; importer.isReadable = false;
            importer.SaveAndReimport();
        }
        var bg = AssetDatabase.LoadAssetAtPath<Texture2D>(Branding + "icon-adaptive-bg.png");
        var fg = AssetDatabase.LoadAssetAtPath<Texture2D>(Branding + "icon-adaptive-fg.png");
        var legacy = AssetDatabase.LoadAssetAtPath<Texture2D>(Branding + "icon-legacy.png");
        if (bg == null || fg == null || legacy == null) throw new InvalidOperationException("Icon textures did not import.");

        PlayerSettings.productName = "Nerdy";
        PlayerSettings.companyName = "Nerdy";
        PlayerSettings.bundleVersion = "0.2.0";
        PlayerSettings.Android.bundleVersionCode = 2;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.nerdy.vr");
        PlayerSettings.Android.forceInternetPermission = true;

        var target = NamedBuildTarget.Android;
        var adaptive = PlayerSettings.GetPlatformIcons(target, AndroidPlatformIconKind.Adaptive);
        foreach (var icon in adaptive) icon.SetTextures(bg, fg);
        PlayerSettings.SetPlatformIcons(target, AndroidPlatformIconKind.Adaptive, adaptive);
        AssetDatabase.SaveAssets();
        return "Identity: Nerdy / com.nerdy.vr / 0.2.0; icons set: adaptive " + adaptive.Length + ".";
    }
}
