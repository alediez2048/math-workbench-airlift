using System;
using System.IO;
using System.Linq;
using Airlift.Onboarding;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CheckOnboarding
{
    public static string Run()
    {
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        if (d == null) throw new InvalidOperationException("Open Onboarding first.");
        var buttons = d.catalog.GetComponentsInChildren<Button>(true);
        if (buttons.Length != 3 || buttons.Count(b => b.interactable) != 1)
            throw new InvalidOperationException("Catalog must have exactly one available lesson.");
        var bodies = new[] { d.content.overview, d.content.orientation, d.content.demonstration, d.content.practice, d.content.retry, d.content.ready };
        var heights = bodies.Select(s => d.body.GetPreferredValues(s, d.body.rectTransform.rect.width, 1000).y).ToArray();
        if (heights.Any(h => h > d.body.rectTransform.rect.height))
            throw new InvalidOperationException("Instruction text overflows: " + string.Join(",", heights));
        if (UnityEngine.Object.FindObjectsByType<OVRCameraRig>().Length != 1)
            throw new InvalidOperationException("Expected a single camera rig.");
        var path = Path.GetFullPath("../artifacts/onboarding-catalog.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var cameraObject = new GameObject("Temporary onboarding preview") { hideFlags = HideFlags.HideAndDontSave };
        var camera = cameraObject.AddComponent<Camera>();
        camera.transform.SetPositionAndRotation(new Vector3(0, 1.16f, -0.65f), Quaternion.identity);
        camera.orthographic = true; camera.orthographicSize = 0.48f;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.12f,0.14f,0.17f);
        var rt = new RenderTexture(1440, 1000, 24);
        var prior = RenderTexture.active;
        var texture = new Texture2D(1440, 1000, TextureFormat.RGB24, false);
        try
        {
            Canvas.ForceUpdateCanvases();
            camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
            texture.ReadPixels(new Rect(0, 0, 1440, 1000), 0, 0); texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = prior; camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(cameraObject);
        }
        return "PASS: single rig; three cards/one enabled; all six instruction pages fit. Preview: " + path
            + "; strap Grabbable: " + d.grabbable.name + "; body heights: " + string.Join(",", heights);
    }
}
