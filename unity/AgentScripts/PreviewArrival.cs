using System.IO;
using Airlift.Lounge;
using Airlift.Welcome;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// The arrival as the learner sees it two seconds in: the logo assembled on the board, the pill bar part way
// (Dee still connecting), no card yet. Renders artifacts/lounge/welcome-arrival.png from the seated head.
// Run: unity command run_script --file AgentScripts/PreviewArrival.cs --entry PreviewArrival.Run
public static class PreviewArrival
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var n = Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var room = Object.FindAnyObjectByType<LoungeRoom>(FindObjectsInactive.Include);
        string dir = Path.GetFullPath("../artifacts/lounge"); Directory.CreateDirectory(dir);
        try
        {
            var headPos = new Vector3(0f, 1.25f, 0f);
            n.welcomeAnchor.SetPositionAndRotation(headPos + Vector3.forward * n.welcomeDistance - Vector3.up * n.welcomeBelowEyes, Quaternion.LookRotation(Vector3.forward));
            n.consentRoot.SetActive(false); n.welcomeRoot.SetActive(false); n.catalogRoot.SetActive(false); n.hudRoot.SetActive(false);
            var settings = n.GetComponent<LoungeSettings>(); if (settings != null && settings.panel != null) settings.panel.SetActive(false);
            var rundown = n.GetComponent<LoungeRundown>(); if (rundown != null && rundown.header != null) rundown.header.SetActive(false);
            if (room != null) { room.Show(true); room.Apply(Scenery.NerdyLounge); room.ShowBoard(true); }
            var onboarding = Object.FindAnyObjectByType<Airlift.Onboarding.OnboardingDirector>(FindObjectsInactive.Include);
            if (onboarding != null) foreach (Transform c in onboarding.transform) c.gameObject.SetActive(false);

            var arrival = n.arrival ?? room.arrival;
            arrival.gameObject.SetActive(true);
            if (arrival.group != null) arrival.group.alpha = 1f;
            if (arrival.dots != null) foreach (var d in arrival.dots) if (d != null) d.gameObject.SetActive(false);
            if (arrival.logo != null) { arrival.logo.localScale = Vector3.one; var g = arrival.logo.GetComponent<Graphic>(); if (g != null) g.color = new Color(g.color.r, g.color.g, g.color.b, 1f); }
            if (arrival.barRoot != null) arrival.barRoot.SetActive(true);
            if (arrival.barFill != null) arrival.barFill.sizeDelta = new Vector2(0.62f * arrival.barWidth, arrival.barFill.sizeDelta.y);
            Canvas.ForceUpdateCanvases();
            n.head.SetPositionAndRotation(headPos, Quaternion.LookRotation((n.welcomeAnchor.position - headPos).normalized));
            Shot(n.head, Path.Combine(dir, "welcome-arrival.png"));
            return "rendered welcome-arrival.png (logo up, bar at 62%)";
        }
        finally { EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single); }
    }

    static void Shot(Transform head, string path)
    {
        var camObj = new GameObject("Preview cam") { hideFlags = HideFlags.HideAndDontSave }; var cam = camObj.AddComponent<Camera>();
        cam.transform.SetPositionAndRotation(head.position, head.rotation); cam.fieldOfView = 60; cam.nearClipPlane = 0.05f; cam.clearFlags = CameraClearFlags.Skybox;
        var rt = new RenderTexture(1600, 1000, 24); var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false); var prior = RenderTexture.active;
        try { cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = prior; cam.targetTexture = null; Object.DestroyImmediate(tex); Object.DestroyImmediate(rt); Object.DestroyImmediate(camObj); }
    }
}
