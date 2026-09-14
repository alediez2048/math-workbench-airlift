using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Meta.XR.BuildingBlocks.Editor;
using Oculus.Interaction.Editor.QuickActions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Editor-only builder run through official Unity Pipeline run_script.
// Meta SDK 205 owns tracking, controller wiring, and grabbing; never patch vendor code.
public static class CreateDeviceProof
{
    const string ScenePath = "Assets/Airlift/Scenes/DeviceProof.unity";

    public static async Task<string> Run()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            throw new InvalidOperationException("DeviceProof already exists; inspect it instead of overwriting.");
        if (SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save the current scene before creating the proof.");
        Folder("Assets", "Airlift");
        Folder("Assets/Airlift", "Scenes");
        Folder("Assets/Airlift", "Materials");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, ScenePath);

        await Install("f0540b20-dfd6-420e-b20d-c270f88dc77e"); // Passthrough + camera dependency.
        await Install("f10154e0-16b2-492f-97d0-6639f69e7df6"); // ISDK controllers + interaction dependency.

        var rig = UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>();
        if (rig == null) throw new InvalidOperationException("Meta camera rig was not installed.");
        var manager = rig.GetComponent<OVRManager>();
        manager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
        manager.isInsightPassthroughEnabled = true;
        foreach (var camera in rig.GetComponentsInChildren<Camera>(true))
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
        }

        var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bench.name = "Proof workbench - virtual surface";
        bench.transform.position = new Vector3(0, 0.78f, 0.65f);
        bench.transform.localScale = new Vector3(0.8f, 0.04f, 0.45f);
        bench.GetComponent<Renderer>().sharedMaterial = Material("Bench", new Color(0.08f, 0.18f, 0.23f));

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Grab this cube";
        cube.transform.position = new Vector3(0, 0.91f, 0.5f);
        cube.transform.localScale = Vector3.one * 0.12f;
        cube.GetComponent<Renderer>().sharedMaterial = Material("Cube", new Color(1f, 0.55f, 0.12f));
        QuickActionsAPI.AddGrabInteraction(cube);

        var light = new GameObject("Proof light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1;
        light.shadows = LightShadows.None;
        light.transform.rotation = Quaternion.Euler(45, -30, 0);
        RenderSettings.ambientLight = Color.gray;
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        return "Saved DeviceProof with Meta passthrough, controller rig, and SDK-grabbable cube. Android configuration and physical verification remain pending.";
    }

    static async Task Install(string id)
    {
        var block = AssetDatabase.FindAssets("t:BlockData")
            .Select(g => AssetDatabase.LoadAssetAtPath<BlockData>(AssetDatabase.GUIDToAssetPath(g)))
            .FirstOrDefault(b => b != null && b.Id == id);
        if (block == null) throw new InvalidOperationException("Missing Meta block: " + id);
        // SDK's own dependency-aware installer is internal in 205. Fail explicitly
        // if its signature changes rather than silently falling back to custom wiring.
        var install = typeof(BlockData).GetMethod("InstallWithDependencies",
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            new[] { typeof(GameObject) }, null);
        if (install == null) throw new MissingMethodException("Meta block installer changed.");
        await (Task)install.Invoke(block, new object[] { null });
    }

    static void Folder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
    }

    static Material Material(string name, Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) throw new InvalidOperationException("URP Lit shader is missing.");
        var material = new Material(shader) { name = name, color = color };
        AssetDatabase.CreateAsset(material, "Assets/Airlift/Materials/" + name + ".mat");
        return material;
    }
}
