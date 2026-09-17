using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Welcome;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
// Card-edge polish evidence: renders the consent, catalog and lesson cards at Quest 3S pixel density (~19 px/deg) with
// MSAA off (the current device setting) and 4x, and lists every panel Image (sprite, type, pixels-per-unit multiplier).
// Reloads the scene afterwards; saves nothing.
public static class PreviewCardEdges
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var d = n.onboarding; var sb = new System.Text.StringBuilder(); string dir = Path.GetFullPath("../artifacts/polish"); Directory.CreateDirectory(dir);
        foreach (var img in n.GetComponentsInChildren<Image>(true).Concat(d.transform.Find("Lesson interface").GetComponentsInChildren<Image>(true)))
        {
            var rt = img.rectTransform; if (rt.rect.width < 150 && rt.rect.height < 150) continue;
            sb.AppendLine(PathOf(img.transform) + " sprite=" + (img.sprite ? img.sprite.name : "none") + " type=" + img.type + " ppuMul=" + img.pixelsPerUnitMultiplier + " size=" + rt.rect.size + " color=" + img.color + " scale=" + img.transform.lossyScale.x.ToString("0.0000"));
        }
        try
        {
            var headPos = new Vector3(0, 1.25f, 0);
            var pose = OnboardingPlacement.BoardPose(headPos, Vector3.forward, d.content.boardDistance, d.content.boardBelowEyes);
            d.transform.SetPositionAndRotation(pose.position, pose.rotation);
            var root = n.welcomeAnchor; root.SetPositionAndRotation(new Vector3(0, 1.25f, 1.0f), Quaternion.identity);
            var head = new GameObject("head") { hideFlags = HideFlags.HideAndDontSave }.transform; head.position = headPos;
            n.ConsentNoVoice(); n.consentRoot.SetActive(true); n.welcomeRoot.SetActive(false);
            n.hudRoot.SetActive(false); head.LookAt(n.consentRoot.transform.position); Shots(head, dir, "1-consent"); n.hudRoot.SetActive(true);
            n.captionText.text = GuideIntro.SpokenSpanish;
            n.GoToCatalog(); head.LookAt(n.catalogRoot.transform.position); Shots(head, dir, "2-catalog");
            n.SelectCard("cargo_crew_fractions");
            var lesson = UnityEngine.Object.FindAnyObjectByType<CargoLessonDirector>(FindObjectsInactive.Include);
            typeof(CargoLessonDirector).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(lesson, null);
            lesson.Begin(); lesson.JumpToChapter(1);
            head.LookAt(d.transform.Find("Lesson interface").position); Shots(head, dir, "3-lesson");
            UnityEngine.Object.DestroyImmediate(head.gameObject);
        }
        finally { EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single); }
        return sb.ToString();
    }
    static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
    static void Shots(Transform head, string dir, string name)
    {
        foreach (int msaa in new[] { 1, 4 }) Shot(head, System.IO.Path.Combine(dir, name + "-msaa" + msaa + ".png"), msaa);
    }
    static void Shot(Transform head, string path, int msaa)
    {
        const int w = 1100, h = 1000;   // 58 deg vertical FOV over 1000 px ~ Quest 3S density
        var camObj = new GameObject("Preview cam") { hideFlags = HideFlags.HideAndDontSave }; var cam = camObj.AddComponent<Camera>();
        cam.transform.SetPositionAndRotation(head.position, head.rotation); cam.fieldOfView = 58; cam.nearClipPlane = 0.05f; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.58f, 0.64f);
        var rt = new RenderTexture(w, h, 24) { antiAliasing = msaa }; var tex = new Texture2D(w, h, TextureFormat.RGB24, false); var prior = RenderTexture.active;
        try { Canvas.ForceUpdateCanvases(); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = prior; cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(camObj); }
    }
}
