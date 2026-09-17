using UnityEditor;
using UnityEditor.Compilation;
// Import new/changed scripts and request a script compilation (the editor is unfocused, so
// auto-refresh does not run). Poll recompile_status / console_status afterwards.
public static class RefreshAndCompile
{
    public static string Run()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        CompilationPipeline.RequestScriptCompilation();
        return "refreshed; compilation requested";
    }
}
