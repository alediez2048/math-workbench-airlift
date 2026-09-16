using System;
using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// TEMPORARY diagnostic/accessibility override for CargoCrew only: the practice grab
// selectors accept grip OR index trigger while the grip-signal defect is open.
// Vendor prefab is untouched; this records a prefab-instance override in the scene.
public static class ApplyGrabSelectorFallback
{
    public static string Run()
    {
        // Reload from disk: the only unsaved editor change (accidental wheel rotation and
        // TMP cache noise) was diffed and archived on 2026-09-16 before discarding.
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        if (scene.isDirty) throw new InvalidOperationException("CargoCrew still dirty after reload; stop.");
        var selectors = UnityEngine.Object.FindObjectsByType<ControllerSelector>(FindObjectsInactive.Include)
            .Where(s => s.gameObject.scene == scene && s.transform.parent != null
                && s.transform.parent.name == "ControllerGrabInteractor").ToArray();
        // Two selectors come from the scene-level Building Block copy and two from the comprehensive rig prefab.
        if (selectors.Length != 4) throw new InvalidOperationException("Expected four controller grab selectors, found " + selectors.Length);
        int changed = 0;
        foreach (var selector in selectors)
        {
            var wanted = ControllerButtonUsage.GripButton | ControllerButtonUsage.TriggerButton;
            if (selector.ControllerButtonUsage == wanted) continue;
            if (selector.ControllerButtonUsage != ControllerButtonUsage.GripButton)
                throw new InvalidOperationException("Unexpected selector usage " + selector.ControllerButtonUsage + " on " + selector.name);
            Undo.RecordObject(selector, "Grab selector fallback");
            selector.ControllerButtonUsage = wanted;
            PrefabUtility.RecordPrefabInstancePropertyModifications(selector);
            EditorUtility.SetDirty(selector);
            changed++;
        }
        if (changed > 0) EditorSceneManager.SaveScene(scene);
        return "CargoCrew grab selectors accept grip or trigger (" + changed + " changed, "
            + selectors.Count(s => s.ControllerButtonUsage == (ControllerButtonUsage.GripButton | ControllerButtonUsage.TriggerButton)) + "/4 overridden; prefab instances: "
            + selectors.Count(s => PrefabUtility.IsPartOfPrefabInstance(s)) + ").";
    }
}
