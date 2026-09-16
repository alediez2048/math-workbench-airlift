using UnityEditor;
public static class ReloadAfterStalledTests
{
    public static string Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new System.InvalidOperationException("Exit play mode first.");
        EditorUtility.RequestScriptReload();
        return "Requested script-domain reload after test timeout; no scene saved or editor restarted.";
    }
}
