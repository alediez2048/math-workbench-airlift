using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
public static class CreateNerdyGradientPreset
{
    public static string Run()
    {
        string dir = "Assets/TextMesh Pro/Resources/" + TMP_Settings.defaultColorGradientPresetsPath.TrimEnd('/');
        Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Application.dataPath), dir));
        string path = dir + "/NerdySpectrum.asset";
        var g = AssetDatabase.LoadAssetAtPath<TMP_ColorGradient>(path);
        if (g == null) { g = ScriptableObject.CreateInstance<TMP_ColorGradient>(); AssetDatabase.CreateAsset(g, path); }
        g.colorMode = ColorMode.FourCornersGradient;
        Color amber = new Color32(255, 195, 43, 255), magenta = new Color32(251, 67, 218, 255), orchid = new Color32(214, 132, 255, 255), cyan = new Color32(23, 226, 234, 255);
        g.topLeft = amber; g.bottomLeft = magenta; g.topRight = cyan; g.bottomRight = orchid;
        EditorUtility.SetDirty(g); AssetDatabase.SaveAssets();
        return "Gradient preset at " + path;
    }
}
