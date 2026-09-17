using System;
using System.IO;
using System.Reflection;
using Airlift.Onboarding;
using Airlift.Welcome;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
// Diagnostic (2026-09-16): drive the Nerdy flow to the Lesson phase in the editor, report where the
// Guide HUD ends up, and render from the head. Reloads the scene from disk afterwards; saves nothing.
public static class InspectLessonHud
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>(FindObjectsInactive.Include);
        var sb = new System.Text.StringBuilder();
        string dir = Path.GetFullPath("../artifacts"); Directory.CreateDirectory(dir);
        try
        {
            sb.AppendLine("onboarding object: " + TPath(d.transform) + " world " + d.transform.position + " rot " + d.transform.eulerAngles + " scale " + d.transform.lossyScale);
            sb.AppendLine("station: " + TPath(d.station.transform) + " local " + d.station.transform.localPosition + " active " + d.station.activeSelf);
            sb.AppendLine("head: " + TPath(n.head) + " world " + n.head.position + " fwd " + n.head.forward);
            var hc = n.hudStationCanvas;
            sb.AppendLine("hudStationCanvas BEFORE: " + TPath(hc) + " local " + hc.localPosition + " anchored " + ((RectTransform)hc).anchoredPosition + " world " + hc.position + " scale " + hc.lossyScale + " activeInHierarchy " + hc.gameObject.activeInHierarchy);
            foreach (Transform c in hc) sb.AppendLine("  child: " + c.name + " active " + c.gameObject.activeSelf);
            sb.AppendLine("stationVisuals: " + string.Join(", ", Array.ConvertAll(n.stationVisuals, g => g == null ? "null" : TPath(g.transform) + "(" + g.activeSelf + ")")));
            // Drive the flow: consent (no voice) -> catalog -> lesson, exactly the runtime path minus the guide.
            n.welcomeAnchor.SetPositionAndRotation(n.head.position + Vector3.ProjectOnPlane(n.head.forward, Vector3.up).normalized * n.welcomeDistance - Vector3.up * n.welcomeBelowEyes, Quaternion.LookRotation(Vector3.ProjectOnPlane(n.head.forward, Vector3.up).normalized));
            var headPos = new Vector3(0, 1.6f, 0); var pose = OnboardingPlacement.BoardPose(headPos, Vector3.forward, d.content.boardDistance, d.content.boardBelowEyes); d.transform.SetPositionAndRotation(pose.position, pose.rotation); n.head.position = headPos;
            n.ConsentNoVoice(); n.GoToCatalog(); n.SelectCard("cargo_crew_fractions");
            sb.AppendLine("phase: " + n.Flow.Phase);
            var h = n.hudRoot.transform;
            sb.AppendLine("hudRoot AFTER: parent " + TPath(h.parent) + " activeInHierarchy " + n.hudRoot.activeInHierarchy + " local " + h.localPosition + " localScale " + h.localScale + " anchored " + ((RectTransform)h).anchoredPosition + " size " + ((RectTransform)h).sizeDelta);
            sb.AppendLine("hudRoot world " + h.position + " lossyScale " + h.lossyScale + " forward " + h.forward);
            sb.AppendLine("hudStationCanvas AFTER: world " + hc.position + " activeInHierarchy " + hc.gameObject.activeInHierarchy + " canvas enabled " + hc.GetComponent<Canvas>().enabled + " renderMode " + hc.GetComponent<Canvas>().renderMode + " cam " + hc.GetComponent<Canvas>().worldCamera);
            // Distance and direction from the head.
            var toHud = h.position - n.head.position;
            sb.AppendLine("head->hud: dist " + toHud.magnitude.ToString("F2") + " elevation " + (Mathf.Asin(toHud.normalized.y) * Mathf.Rad2Deg).ToString("F1") + " deg; hud faces head? dot(hud.forward, toHud)=" + Vector3.Dot(h.forward, toHud.normalized).ToString("F2"));
            // Anything of the station at that height in the way?
            foreach (var r in d.station.GetComponentsInChildren<Renderer>(true)) if (r.bounds.Contains(h.position)) sb.AppendLine("hud centre is INSIDE renderer bounds of " + TPath(r.transform) + " bounds " + r.bounds);
            Canvas.ForceUpdateCanvases();
            Shot(n.head, Path.Combine(dir, "lesson-hud-from-head.png"), 60f);
            n.captionText.text = "Test caption: this is the assistant bar."; n.stateText.text = "live guide";
            Shot(n.head, Path.Combine(dir, "lesson-hud-from-head-captioned.png"), 60f);
        }
        finally { EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single); }
        return sb.ToString();
    }
    static string TPath(Transform t) { if (t == null) return "null"; string s = t.name; while (t.parent != null) { t = t.parent; s = t.name + "/" + s; } return s; }
    static void Shot(Transform head, string path, float fov)
    {
        var camObj = new GameObject("Preview cam") { hideFlags = HideFlags.HideAndDontSave }; var cam = camObj.AddComponent<Camera>();
        cam.transform.SetPositionAndRotation(head.position, head.rotation); cam.fieldOfView = fov; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.58f, 0.64f);
        var rt = new RenderTexture(1600, 1000, 24); var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false); var prior = RenderTexture.active;
        try { cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = prior; cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(camObj); }
    }
}
