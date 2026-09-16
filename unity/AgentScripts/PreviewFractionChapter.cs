using System;
using System.IO;
using Airlift.Lessons;
using Airlift.Onboarding;
using UnityEditor;
using UnityEngine;
// Desktop-only geometry preview of the fraction chapter; nothing is saved.
public static class PreviewFractionChapter
{
    public static string Run()
    {
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        var lesson = d.GetComponent<CargoLessonDirector>();
        if (lesson == null) throw new InvalidOperationException("Chapter missing.");
        d.station.SetActive(true); d.briefing.SetActive(true); d.catalog.SetActive(false);
        lesson.chapterObjects.SetActive(true); lesson.chapterButtons.SetActive(true);
        d.strap.gameObject.SetActive(false); d.demonstrationStrap.gameObject.SetActive(false);
        lesson.whole.label.ShowWhole(1); lesson.halfA.label.Show(1, 2); lesson.halfB.label.Show(1, 2);
        // Show halves docked on the ruler and the whole in the tray for the preview only.
        lesson.halfA.piece.localPosition = RulerLayout.SnapPosition(lesson.ruler.localPosition, 0, 4, lesson.restHeight);
        lesson.halfB.piece.localPosition = RulerLayout.SnapPosition(lesson.ruler.localPosition, 4, 4, lesson.restHeight);
        string path = Path.GetFullPath("../artifacts/fraction-chapter-preview.png");
        var camObj = new GameObject("Preview cam") { hideFlags = HideFlags.HideAndDontSave };
        var cam = camObj.AddComponent<Camera>();
        var root = d.transform;
        cam.transform.position = root.TransformPoint(new Vector3(0, 0.55f, -0.55f));
        cam.transform.LookAt(root.TransformPoint(new Vector3(0, 0.1f, 0.05f)));
        cam.fieldOfView = 50; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.12f, 0.14f, 0.17f);
        var rt = new RenderTexture(1600, 1000, 24); var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false); var prior = RenderTexture.active;
        try { Canvas.ForceUpdateCanvases(); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally
        {
            RenderTexture.active = prior; cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(camObj);
            // Restore authored state without saving.
            lesson.halfA.piece.localPosition = lesson.halfA.trayPosition; lesson.halfB.piece.localPosition = lesson.halfB.trayPosition;
            lesson.chapterObjects.SetActive(false); lesson.chapterButtons.SetActive(false);
            d.station.SetActive(false); d.briefing.SetActive(false); d.catalog.SetActive(true);
        }
        return "Preview written: " + path;
    }
}
