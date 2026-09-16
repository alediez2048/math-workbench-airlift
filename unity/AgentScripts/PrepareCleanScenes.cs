using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Pre-test guard: the Unity Test Framework raises a native "Scene(s) Have Been Modified"
// alert when an open scene is dirty, which blocks the editor main thread and hangs the
// verification wrapper (observed 2026-09-16: TextMesh Pro re-caches font data on scene
// open, dirtying CargoCrew). Preserve a copy of any dirty state as evidence, then reload
// the scene from disk so the run cannot hang. Nothing is saved over the project scene.
public static class PrepareCleanScenes
{
    public static string Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit play mode first.");
        string report = "";
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isDirty) continue;
            string dir = Path.GetFullPath("../artifacts/qa/dirty-scenes");
            Directory.CreateDirectory(dir);
            string copy = Path.Combine(dir, scene.name + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity");
            string temp = "Assets/Airlift/Scenes/__dirty_copy_tmp.unity";
            if (!EditorSceneManager.SaveScene(scene, temp, true)) throw new InvalidOperationException("Could not copy dirty scene " + scene.name);
            File.Copy(temp, copy, true);
            AssetDatabase.DeleteAsset(temp);
            if (string.IsNullOrEmpty(scene.path)) throw new InvalidOperationException("Untitled dirty scene open; save or close it before testing. Copy: " + copy);
            report += "WARNING dirty scene " + scene.name + " reloaded from disk; unsaved state copied to " + copy + ". ";
        }
        if (report.Length > 0)
        {
            var active = SceneManager.GetActiveScene();
            EditorSceneManager.OpenScene(active.path, OpenSceneMode.Single);
        }
        return report.Length > 0 ? report : "All open scenes clean.";
    }
}
