using Airlift.Presentation;
using TMPro;
using UnityEditor;
// The workbenches bind their fonts through the older AirliftStyle asset, which still pointed at Nunito. Point it at
// the Nerdy families so there is one set of fonts in the app and TypographyBindings.Apply cannot undo the standard.
public static class PointLegacyStyle
{
    public static string Run()
    {
        var nerdy = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        var legacy = AssetDatabase.LoadAssetAtPath<AirliftStyle>("Assets/Airlift/Fonts/AirliftStyle.asset");
        if (nerdy == null || legacy == null) return "style asset missing";
        string before = (legacy.headingFont ? legacy.headingFont.name : "null") + " / " + (legacy.bodyFont ? legacy.bodyFont.name : "null");
        legacy.headingFont = nerdy.displayFont;    // the same display face the standard gives every heading
        legacy.bodyFont = nerdy.bodyFont;
        EditorUtility.SetDirty(legacy);
        AssetDatabase.SaveAssets();
        return "AirliftStyle fonts " + before + " -> " + legacy.headingFont.name + " / " + legacy.bodyFont.name;
    }
}
