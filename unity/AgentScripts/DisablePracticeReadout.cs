using System;
using Airlift.Onboarding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
// Lock item L-3: turn off the temporary controller readout on the practice panel. Idempotent; saves CargoCrew.
public static class DisablePracticeReadout
{
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>(UnityEngine.FindObjectsInactive.Include);
        if (!d.showInputDiagnostics) return "Readout already off.";
        d.showInputDiagnostics = false; EditorUtility.SetDirty(d); EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("save failed");
        return "Practice readout off; scene saved.";
    }
}
