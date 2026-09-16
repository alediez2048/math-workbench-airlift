using System;
using Airlift.Onboarding;
using Airlift.Presentation;
using Oculus.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Owner headset feedback 2026-09-16: one-hand carry jumped and could not turn the table.
// Replace the wrist-driven OneGrabFreeTransformer with the face-the-player carry rule.
public static class SwapTableCarry
{
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        var handle = d.GetComponent<TableHandle>();
        if (handle == null) throw new InvalidOperationException("Table handle missing.");
        var bar = handle.grabbable.gameObject;
        var old = bar.GetComponent<OneGrabFreeTransformer>();
        if (old == null) throw new InvalidOperationException("Already swapped.");
        UnityEngine.Object.DestroyImmediate(old);
        var carry = bar.AddComponent<TableCarryTransformer>();
        carry.head = d.head;
        handle.grabbable.InjectOptionalOneGrabTransformer(carry);
        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        return "One-hand carry now uses TableCarryTransformer (face the player, grab point stays in hand).";
    }
}
