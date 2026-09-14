using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Airlift.Editor
{
    // Meta SDK 205's DevAgentBuildProcessor injects a local token at order 1,
    // including when its runtime connection is disabled. Scrub after that hook.
    // The toolkit is not part of the learning product and must not receive secrets.
    public sealed class ProofBuildSafety : IPreprocessBuildWithReport
    {
        public int callbackOrder => 10000;

        public void OnPreprocessBuild(BuildReport report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                "Assets/Resources/DevAgentSettings.asset");
            if (asset == null) return;
            var settings = new SerializedObject(asset);
            Require(settings, "enabled").boolValue = false;
            Require(settings, "serverAddress").stringValue = "127.0.0.1";
            Require(settings, "accessToken").stringValue = "";
            Require(settings, "witClientAccessToken").stringValue = "";
            settings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(asset);
            Debug.Log("Airlift: disabled developer bridge and cleared connection credentials for this build.");
        }

        static SerializedProperty Require(SerializedObject settings, string name)
        {
            return settings.FindProperty(name) ?? throw new BuildFailedException(
                "Meta developer-settings schema changed; review credential stripping before building: " + name);
        }
    }
}
