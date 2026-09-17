using System;
using System.IO;
using Airlift.Welcome;
using UnityEditor;
using UnityEngine;
// Desktop renders of the consent, welcome and catalog views; nothing saved.
public static class PreviewNerdyWelcome
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != scenePath) UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        if (n == null) throw new InvalidOperationException("NerdyDirector missing.");
        string dir = Path.GetFullPath("../artifacts");
        var root = n.welcomeAnchor; root.SetPositionAndRotation(new Vector3(0, 1.3f, 1.15f), Quaternion.identity);
        foreach (var go in n.stationVisuals) go.SetActive(false);
        try
        {
            Show(n, true, false, false, false); Shot(root, Path.Combine(dir, "nerdy-consent.png"));
            Show(n, false, true, false, true); n.captionText.text = "Hi there! I'm Nerdy. Roughly how old are you?"; n.stateText.text = "live guide"; Shot(root, Path.Combine(dir, "nerdy-welcome.png"));
            Show(n, false, false, true, true); n.captionText.text = "Here are your lessons. Cargo Crew is ready to play."; Shot(root, Path.Combine(dir, "nerdy-catalog.png"));
        }
        finally { Show(n, true, false, false, false); n.captionText.text = ""; n.stateText.text = ""; foreach (var go in n.stationVisuals) go.SetActive(true); }
        return "Previews: nerdy-consent.png, nerdy-welcome.png, nerdy-catalog.png";
    }
    static void Show(NerdyDirector n, bool consent, bool welcome, bool catalog, bool hud) { n.consentRoot.SetActive(consent); n.welcomeRoot.SetActive(welcome); n.catalogRoot.SetActive(catalog); n.hudRoot.SetActive(hud); }
    static void Shot(Transform root, string path)
    {
        var camObj = new GameObject("Preview cam") { hideFlags = HideFlags.HideAndDontSave }; var cam = camObj.AddComponent<Camera>();
        cam.transform.position = root.TransformPoint(new Vector3(0, 0.02f, -1.05f)); cam.transform.LookAt(root.TransformPoint(new Vector3(0, 0.02f, 0)));
        cam.fieldOfView = 42; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.58f, 0.64f);
        var rt = new RenderTexture(1600, 1000, 24); var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false); var prior = RenderTexture.active;
        try { Canvas.ForceUpdateCanvases(); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = prior; cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(camObj); }
    }
}
