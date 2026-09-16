using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Meta.XR.BuildingBlocks.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreateVendorGrabProof
{
    public static async Task<string> Run()
    {
        const string path = "Assets/Airlift/Scenes/VendorGrabProof.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null) throw new Exception("Proof exists; do not overwrite.");
        for (int i=0;i<SceneManager.sceneCount;i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new Exception("Preserve dirty scenes before diagnostic creation.");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene,path);
        await Install("f0540b20-dfd6-420e-b20d-c270f88dc77e");
        await Install("f10154e0-16b2-492f-97d0-6639f69e7df6");
        var rig=UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>();
        if(rig==null)throw new Exception("Camera rig missing.");
        var manager=rig.GetComponent<OVRManager>();
        manager.trackingOriginType=OVRManager.TrackingOrigin.FloorLevel;
        manager.isInsightPassthroughEnabled=true;
        foreach(var camera in rig.GetComponentsInChildren<Camera>(true))
        {camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;}
        // Stationary MR safety override; no input/grab wiring edits.
        foreach(var component in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(component.GetType().Name=="FirstPersonLocomotor")
            {component.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(component);}
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.meta.xr.sdk.interaction.ovr/Editor/Blocks/InteractableItems/Prefabs/[BB] Grabbable Cube.prefab");
        if(prefab==null)throw new Exception("Vendor cube prefab missing.");
        var cube=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
        cube.transform.position=new Vector3(0,1.0f,0.55f);
        var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name="Static diagnostic platform";floor.transform.position=new Vector3(0,0.8f,0.6f);
        floor.transform.localScale=new Vector3(0.7f,0.04f,0.5f);
        var light=new GameObject("Diagnostic light").AddComponent<Light>();
        light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(45,-30,0);
        RenderSettings.ambientLight=Color.gray;
        if(UnityEngine.Object.FindObjectsByType<OVRCameraRig>(FindObjectsSortMode.None).Length!=1)throw new Exception("Expected one rig.");
        if(cube.GetComponentsInChildren<Oculus.Interaction.Grabbable>(true).Length!=1)throw new Exception("Vendor cube grab component missing.");
        EditorSceneManager.SaveScene(scene,path);
        return "Created separate vendor proof: fresh Meta blocks, supplied cube, stationary locomotion override, no lesson scripts.";
    }
    static async Task Install(string id)
    {
        var block=AssetDatabase.FindAssets("t:BlockData").Select(g=>AssetDatabase.LoadAssetAtPath<BlockData>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault(b=>b!=null&&b.Id==id);
        if(block==null)throw new Exception("Missing vendor block.");
        var method=typeof(BlockData).GetMethod("InstallWithDependencies",BindingFlags.Instance|BindingFlags.NonPublic,null,new[]{typeof(GameObject)},null);
        if(method==null)throw new Exception("Vendor installer signature changed.");
        await (Task)method.Invoke(block,new object[]{null});
    }
}
