using System;
using UnityEditor;
using UnityEngine;

public static class ImportTextResources
{
    public static string Run()
    {
        if (Resources.Load<TMPro.TMP_Settings>("TMP Settings") != null)
            return "Text resources already available.";
        var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMPro.TMP_Text).Assembly);
        if (package == null) throw new InvalidOperationException("Cannot locate Unity UI package.");
        AssetDatabase.ImportPackage(package.resolvedPath + "/Package Resources/TMP Essential Resources.unitypackage", false);
        return "Requested import of Unity bundled TMP Essential Resources.";
    }
}
