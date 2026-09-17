using UnityEditor;
using UnityEditor.Build;
// Swaps the package id/name so a preview APK installs BESIDE the release app, and puts them back afterwards.
// Two fast calls around the pipeline's own `build` command, which run_script's 30 s limit cannot host.
// Run: unity command run_script --file AgentScripts/LoungePreviewSettings.cs --entry LoungePreviewSettings.Apply
//      unity command run_script --file AgentScripts/LoungePreviewSettings.cs --entry LoungePreviewSettings.Restore
public static class LoungePreviewSettings
{
    const string PreviewId = "com.nerdy.vr.lounge", PreviewName = "Nerdy Lounge (preview)";
    const string ReleaseId = "com.nerdy.vr", ReleaseName = "Nerdy";

    public static string Apply() => Set(PreviewId, PreviewName);
    public static string Restore() => Set(ReleaseId, ReleaseName);

    static string Set(string id, string name)
    {
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, id);
        PlayerSettings.productName = name;
        AssetDatabase.SaveAssets();
        return "applicationIdentifier=" + PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)
               + " productName=" + PlayerSettings.productName;
    }
}
