using System;
using Airlift.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// CC-HF-01. The lounge sky becomes the approved Higgsfield panorama; the window panes become faint glass so the
// real skybox shows through them. Run after BuildLounge (which regenerates the gradient). Idempotent.
// Run: unity command run_script --file AgentScripts/ApplyLoungePanorama.cs --entry ApplyLoungePanorama.Run
public static class ApplyLoungePanorama
{
    const string Panorama = "Assets/Airlift/Art/Generated/Lounge/LoungePanorama.png";
    const string MatDir = "Assets/Airlift/Materials/Lounge";

    public static string Run()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(Panorama) ?? throw new InvalidOperationException(Panorama + " missing.");
        if (importer.mipmapEnabled || importer.wrapModeU != TextureWrapMode.Repeat || importer.wrapModeV != TextureWrapMode.Clamp)
        { importer.mipmapEnabled = false; importer.wrapModeU = TextureWrapMode.Repeat; importer.wrapModeV = TextureWrapMode.Clamp; importer.SaveAndReimport(); }
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Panorama);

        var sky = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/LoungeSky.mat") ?? throw new InvalidOperationException("Run BuildLounge first.");
        sky.SetTexture("_MainTex", tex);
        sky.SetFloat("_Mapping", 1f);            // latitude-longitude
        sky.SetFloat("_ImageType", 0f);          // 360 degrees
        EditorUtility.SetDirty(sky);

        var pane = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/LoungeSkyPane.mat") ?? throw new InvalidOperationException("Run BuildLounge first.");
        pane.shader = Shader.Find("Universal Render Pipeline/Unlit");
        pane.mainTexture = null;
        var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset") ?? throw new InvalidOperationException("NerdyStyle missing.");
        pane.SetColor("_BaseColor", new Color(style.cyan.r, style.cyan.g, style.cyan.b, 0.12f));   // a token hue (LoungeWiringTests)
        pane.SetFloat("_Surface", 1f); pane.SetFloat("_Blend", 0f);
        pane.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); pane.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        pane.SetFloat("_ZWrite", 0f);
        pane.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        pane.renderQueue = (int)RenderQueue.Transparent;
        pane.SetOverrideTag("RenderType", "Transparent");
        pane.SetShaderPassEnabled("DepthOnly", false);      // a depth prepass would hide the skybox behind the glass
        pane.SetShaderPassEnabled("ShadowCaster", false);
        EditorUtility.SetDirty(pane);

        AssetDatabase.SaveAssets();
        return "lounge sky uses " + Panorama + "; panes are faint glass";
    }
}
