using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Airlift.Editor
{
    // CC-P0-03 THROWAWAY build: development build of the spike scene only, cleartext HTTP allowed
    // for the LAN mint server. Never used for release; restores the HTTP setting afterwards.
    public static class SpikeBuild
    {
        public static string Run()
        {
            var prior = PlayerSettings.insecureHttpOption;
            try
            {
                PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
                string dir = Path.GetFullPath("../artifacts/spike"); Directory.CreateDirectory(dir);
                string output = Path.Combine(dir, "nerdy-spike.apk");
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { "Assets/Airlift/Scenes/RealtimeSpike.unity" }, target = BuildTarget.Android,
                    locationPathName = output, options = BuildOptions.Development
                });
                if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Spike build failed: " + report.summary.totalErrors + " errors");
                return "Spike APK: " + output + " (" + report.summary.totalWarnings + " warnings)";
            }
            finally { PlayerSettings.insecureHttpOption = prior; AssetDatabase.SaveAssets(); }
        }
    }
}
