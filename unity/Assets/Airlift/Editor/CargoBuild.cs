using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace Airlift.Editor
{
    public static class CargoBuild
    {
        public const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        public static string[] Scenes() => new[] { ScenePath };

        public static void Build()
        {
            if (!File.Exists(ScenePath)) throw new BuildFailedException("Create CargoCrew first.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new BuildFailedException("Unsaved scene changes: save/review before building.");
            string directory = Path.GetFullPath("../artifacts");
            Directory.CreateDirectory(directory);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = Scenes(), target = BuildTarget.Android,
                locationPathName = Path.Combine(directory, "airlift-cargo.apk"),
                options = BuildOptions.DetailedBuildReport
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("CargoCrew Android build did not succeed.");
        }
    }
}
