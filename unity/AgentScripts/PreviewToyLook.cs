using System;
using System.IO;
using Airlift.Lessons;
using Airlift.Onboarding;
using UnityEditor;
using UnityEngine;
// Desktop-only renders of the practice stage and the halves stage after the toy look; nothing is saved.
public static class PreviewToyLook
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != scenePath)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Active scene is dirty; review before switching.");
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
        }
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        var lesson = d != null ? d.GetComponent<CargoLessonDirector>() : null;
        if (lesson == null) throw new InvalidOperationException("Chapter missing.");
        string dir = Path.GetFullPath("../artifacts");
        d.station.SetActive(true); d.briefing.SetActive(true); d.catalog.SetActive(false);
        try
        {
            // Practice: strap in the tray, chapter hidden.
            d.strap.gameObject.SetActive(true); d.demonstrationStrap.gameObject.SetActive(false);
            lesson.chapterObjects.SetActive(false); lesson.chapterButtons.SetActive(false);
            Shot(d.transform, Path.Combine(dir, "toy-look-practice.png"), new Vector3(0, 0.55f, -0.6f), new Vector3(0, 0.08f, 0.05f), 50);
            // Halves: whole in the tray, halves docked on the ruler.
            d.strap.gameObject.SetActive(false);
            lesson.chapterObjects.SetActive(true); lesson.chapterButtons.SetActive(true);
            lesson.whole.piece.gameObject.SetActive(true); lesson.halfA.piece.gameObject.SetActive(true); lesson.halfB.piece.gameObject.SetActive(true);
            lesson.whole.label.ShowWhole(1); lesson.halfA.label.Show(1, 2); lesson.halfB.label.Show(1, 2);
            lesson.halfA.piece.localPosition = RulerLayout.SnapPosition(lesson.ruler.localPosition, 0, 4, lesson.restHeight);
            lesson.halfB.piece.localPosition = RulerLayout.SnapPosition(lesson.ruler.localPosition, 4, 4, lesson.restHeight);
            Shot(d.transform, Path.Combine(dir, "toy-look-fractions.png"), new Vector3(0, 0.55f, -0.6f), new Vector3(0, 0.08f, 0.05f), 50);
            Shot(d.transform, Path.Combine(dir, "toy-look-closeup.png"), new Vector3(0.02f, 0.34f, -0.52f), new Vector3(0, 0.05f, 0.0f), 36);
        }
        finally
        {
            lesson.halfA.piece.localPosition = lesson.halfA.trayPosition; lesson.halfB.piece.localPosition = lesson.halfB.trayPosition;
            lesson.chapterObjects.SetActive(false); lesson.chapterButtons.SetActive(false);
            d.station.SetActive(false); d.briefing.SetActive(false); d.catalog.SetActive(true);
            d.strap.gameObject.SetActive(false); d.demonstrationStrap.gameObject.SetActive(false);
        }
        return "Previews written to " + dir + ": toy-look-practice.png, toy-look-fractions.png, toy-look-closeup.png";
    }

    static void Shot(Transform root, string path, Vector3 from, Vector3 at, float fov)
    {
        var camObj = new GameObject("Preview cam") { hideFlags = HideFlags.HideAndDontSave };
        var cam = camObj.AddComponent<Camera>();
        cam.transform.position = root.TransformPoint(from); cam.transform.LookAt(root.TransformPoint(at));
        cam.fieldOfView = fov; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.58f, 0.64f);
        var rt = new RenderTexture(1600, 1000, 24); var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false); var prior = RenderTexture.active;
        try { Canvas.ForceUpdateCanvases(); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = prior; cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(camObj); }
    }
}
