using System;
using System.Linq;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Presentation;
using Oculus.Interaction;
using Oculus.Interaction.Editor.QuickActions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Owner request 2026-09-16: move and resize the whole table from inside the app.
// Adds a yellow carry handle on the front edge whose Grabbable targets the station root:
// one hand carries it (yaw only), two hands resize it in the table plane (0.5x to 2x).
public static class AddTableHandle
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        var lesson = d.GetComponent<CargoLessonDirector>();
        var placement = d.GetComponent<ComfortPlacement>();
        if (d.transform.Find("Table handle") != null) throw new InvalidOperationException("Handle exists; refusing to run twice.");
        var yellow = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/CargoYellow.mat");
        var root = d.transform;

        // Geometry: a chunky rounded bar overlapping the deck's front edge by 5 mm.
        var size = new Vector3(0.18f, 0.035f, 0.035f);
        string meshPath = "Assets/Airlift/Meshes/RoundedBox-0_18x0_035x0_035-r0_012.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (mesh == null) { mesh = RoundedBoxMesh.Build(size, 0.012f); AssetDatabase.CreateAsset(mesh, meshPath); }
        var bar = new GameObject("Table handle");
        bar.transform.SetParent(root, false); bar.transform.localPosition = new Vector3(0f, 0f, -0.412f);
        bar.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = bar.AddComponent<MeshRenderer>(); renderer.sharedMaterial = yellow; renderer.receiveShadows = true;
        var collider = bar.AddComponent<BoxCollider>(); collider.size = new Vector3(0.2f, 0.05f, 0.05f);
        var body = bar.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;

        // Grabbable that moves the station root instead of the bar itself.
        var grabbable = bar.AddComponent<Grabbable>();
        grabbable.InjectOptionalTargetTransform(root);
        grabbable.MaxGrabPoints = 2;
        var one = bar.AddComponent<OneGrabFreeTransformer>();
        var so = new SerializedObject(one);
        foreach (var axis in new[] { "XAxis", "ZAxis" })
        {
            var prop = so.FindProperty("_rotationConstraints." + axis);
            prop.FindPropertyRelative("ConstrainAxis").boolValue = true;
            prop.FindPropertyRelative("AxisRange.Min").floatValue = 0f;
            prop.FindPropertyRelative("AxisRange.Max").floatValue = 0f;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        var two = bar.AddComponent<TwoGrabPlaneTransformer>();
        two.InjectOptionalPlaneTransform(root);
        two.InjectOptionalConstraints(new TwoGrabPlaneTransformer.TwoGrabPlaneConstraints
        {
            MinScale = new FloatConstraint { Constrain = true, Value = TableAdjustRules.MinScale },
            MaxScale = new FloatConstraint { Constrain = true, Value = TableAdjustRules.MaxScale },
            MinY = new FloatConstraint { Constrain = false }, MaxY = new FloatConstraint { Constrain = false }
        });
        grabbable.InjectOptionalOneGrabTransformer(one);
        grabbable.InjectOptionalTwoGrabTransformer(two);
        QuickActionsAPI.AddGrabInteraction(bar);
        var handleInteractable = bar.GetComponentInChildren<GrabInteractable>();
        if (handleInteractable == null) throw new InvalidOperationException("SDK did not create a GrabInteractable for the handle.");

        // Gate component on the director object.
        var handle = d.gameObject.AddComponent<TableHandle>();
        handle.grabbable = grabbable; handle.stationRoot = root; handle.placement = placement; handle.lesson = lesson;
        handle.handleInteractable = handleInteractable; handle.statusText = placement != null ? placement.statusText : null;
        handle.pieceInteractables = root.GetComponentsInChildren<GrabInteractable>(true).Where(g => g != handleInteractable).ToArray();

        d.content.orientation = d.content.orientation.TrimEnd() + " Move the table by its yellow handle; two hands resize it.";
        EditorUtility.SetDirty(d.content);
        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        return "Table handle added: one-hand carry (yaw only), two-hand plane resize " + TableAdjustRules.MinScale + "x-" + TableAdjustRules.MaxScale + "x; " + handle.pieceInteractables.Length + " piece interactables gated.";
    }
}
