using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

public static class ConfigureQuestProof
{
    public static string Run()
    {
        const BuildTargetGroup group = BuildTargetGroup.Android;
        XRGeneralSettingsPerBuildTarget settings;
        if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.settingsKey, out settings))
        {
            settings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(settings, "Assets/Airlift/XRSettings.asset");
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey, settings, true);
        }
        if (!settings.HasManagerSettingsForBuildTarget(group)) settings.CreateDefaultManagerSettingsForBuildTarget(group);
        if (!XRPackageMetadataStore.AssignLoader(settings.ManagerSettingsForBuildTarget(group),
            "UnityEngine.XR.OpenXR.OpenXRLoader", group))
            throw new InvalidOperationException("OpenXR loader assignment failed.");
        settings.SettingsForBuildTarget(group).InitManagerOnStart = true;
        FeatureHelpers.RefreshFeatures(group);
        var xr = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
        Enable<MetaQuestFeature>(xr);
        Enable<OculusTouchControllerProfile>(xr);
        Enable<Meta.XR.MetaXRFeature>(xr);
        xr.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
        EditorUtility.SetDirty(xr);
        EditorUtility.SetDirty(settings);
        var config = OVRProjectConfig.CachedProjectConfig;
        config.handTrackingSupport = OVRProjectConfig.HandTrackingSupport.ControllersOnly;
        config.insightPassthroughSupport = OVRProjectConfig.FeatureSupport.Required;
        config.isPassthroughCameraAccessEnabled = false;
        OVRProjectConfig.CommitProjectConfig(config);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.jad.airlift");
        PlayerSettings.productName = "Airlift Device Proof";
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        PlayerSettings.colorSpace = ColorSpace.Linear;
        var mobile = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
        if (mobile == null) throw new InvalidOperationException("Mobile URP asset missing.");
        GraphicsSettings.defaultRenderPipeline = mobile;
        QualitySettings.renderPipeline = mobile;
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Airlift/Scenes/DeviceProof.unity", true) };
        AssetDatabase.SaveAssets();
        return "Android ARM64 IL2CPP, Vulkan, OpenXR/Meta/Touch profiles, passthrough required; controllers only. Build and physical proof pending.";
    }

    static void Enable<T>(OpenXRSettings settings) where T : UnityEngine.XR.OpenXR.Features.OpenXRFeature
    {
        var feature = settings.GetFeature<T>();
        if (feature == null) throw new InvalidOperationException("Missing OpenXR feature " + typeof(T).Name);
        feature.enabled = true;
        EditorUtility.SetDirty(feature);
    }
}
