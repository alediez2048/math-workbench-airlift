using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Airlift.Editor
{
    // Development build of the voice spike scene only. Cleartext HTTP is allowed in development builds
    // so it can reach a local dev mint; release builds still require HTTPS. Restores the setting afterwards.
    public static class SpikeBuild
    {
        public static string Run()
        {
            var prior = PlayerSettings.insecureHttpOption;
            try
            {
                PlayerSettings.insecureHttpOption = InsecureHttpOption.DevelopmentOnly;
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
