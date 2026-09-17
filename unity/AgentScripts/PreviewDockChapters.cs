using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Welcome;
using UnityEditor.SceneManagement;
using UnityEngine;
// Renders Dock 7 chapters from a seated head pose in the editor. Reloads the scene afterwards; saves nothing.
public static class PreviewDockChapters
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var d = n.onboarding; var lesson = UnityEngine.Object.FindAnyObjectByType<CargoLessonDirector>(FindObjectsInactive.Include);
        var sb = new System.Text.StringBuilder(); string dir = Path.GetFullPath("../artifacts/dock7"); Directory.CreateDirectory(dir);
        var bind = BindingFlags.NonPublic | BindingFlags.Instance;
        try
        {
            var headPos = new Vector3(0, 1.25f, 0);
            var pose = OnboardingPlacement.BoardPose(headPos, Vector3.forward, d.content.boardDistance, d.content.boardBelowEyes);
            d.transform.SetPositionAndRotation(pose.position, pose.rotation);
            n.head.SetPositionAndRotation(headPos, Quaternion.LookRotation(new Vector3(0, -0.45f, 1f)));
            n.cargoLesson = lesson;
            typeof(CargoLessonDirector).GetMethod("Awake", bind).Invoke(lesson, null);
            n.ConsentNoVoice(); n.GoToCatalog(); n.SelectCard("cargo_crew_fractions");
            Shot(n.head, Path.Combine(dir, "0-briefing.png")); sb.AppendLine("briefing heading: " + d.heading.text);
            lesson.Begin();
            var model = (CargoLessonModel)typeof(CargoLessonDirector).GetField("model", bind).GetValue(lesson);
            var snap = typeof(CargoLessonDirector).GetMethod("SnapDocked", bind);
            var refresh = typeof(CargoLessonDirector).GetMethod("Refresh", bind);
            for (int i = 0; i < 5; i++)
            {
                lesson.JumpToChapter(i);
                Shot(n.head, Path.Combine(dir, (i + 1) + "a-start.png"));
                if (i == 2) foreach (var t in d.transform.Find("Lesson interface").GetComponentsInChildren<TMPro.TMP_Text>(false))
                    if (!string.IsNullOrEmpty(t.text)) sb.AppendLine("  TMP " + t.name + " font=" + (t.font ? t.font.name : "null") + " style=" + t.fontStyle + " size=" + t.fontSize + " rich=" + t.richText + " pos=" + t.rectTransform.anchoredPosition + " rect=" + t.rectTransform.rect.size + " fallbacks=" + (t.font ? string.Join("/", t.font.fallbackFontAssetTable.ConvertAll(f => f ? f.name : "null")) : "") + " text=" + t.text.Substring(0, System.Math.Min(40, t.text.Length)).Replace("\n", "|"));
                sb.AppendLine("ch" + (i + 1) + " start: heading='" + d.heading.text + "' body='" + d.body.text.Replace("\n", " | ") + "' hints='" + lesson.sayHints.text + "' canSplit=" + lesson.CanSplit);
                if (lesson.CanSplit) { var r = lesson.TrySplit(); sb.AppendLine("  split ok=" + r.Ok + " " + r.Reason); }
                if (i == 1)
                {
                    // Before splitting, the whole crate cannot fit a pickup bed: the real drop path must refuse it.
                    var ruler0 = lesson.ruler.localPosition;
                    bool whole = lesson.DropAt("whole", new Vector3(Airlift.Lessons.RulerLayout.CellsCenterX(ruler0, 0, 8), ruler0.y, ruler0.z));
                    sb.AppendLine("  drop whole into pickups before split: docked=" + whole + " feedback='" + lesson.Feedback + "' model='" + model.LastFeedback + "'");
                }
                if (lesson.CanSplit) { var r = lesson.TrySplit(); sb.AppendLine("  split ok=" + r.Ok + " " + r.Reason); }
                var rc = lesson.ruler.localPosition;
                foreach (var id in model.PieceIds.ToList())
                {
                    if (model.IsLocked(id) || model.IsDocked(id)) continue;
                    int cells = model.Piece(id).Cells; int bed = -1;
                    for (int b = 0; b < model.BedCount; b++) if (model.BedCapacity(b) - model.BedFill(b) >= cells) { bed = b; break; }
                    if (bed < 0) continue;
                    int startCell = model.BedStartCell(bed) + model.BedFill(bed);
                    bool ok = lesson.DropAt(id, new Vector3(Airlift.Lessons.RulerLayout.CellsCenterX(rc, startCell, cells), rc.y, rc.z));
                    sb.AppendLine("  drop " + id + " -> bed " + bed + " ok=" + ok);
                }
                Shot(n.head, Path.Combine(dir, (i + 1) + "b-docked.png"));
                var load = lesson.TryLoad();
                sb.AppendLine("  load ok=" + load.Ok + " reason='" + load.Reason + "' complete=" + lesson.ChapterComplete + " expr='" + lesson.ExpressionText + "' feedback='" + lesson.Feedback + "'");
                Shot(n.head, Path.Combine(dir, (i + 1) + "c-loaded.png"));
            }
        }
        finally { EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single); }
        return sb.ToString();
    }
    static void Shot(Transform head, string path)
    {
        var camObj = new GameObject("Preview cam") { hideFlags = HideFlags.HideAndDontSave }; var cam = camObj.AddComponent<Camera>();
        cam.transform.SetPositionAndRotation(head.position, head.rotation); cam.fieldOfView = 70; cam.nearClipPlane = 0.05f; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.58f, 0.64f);
        var rt = new RenderTexture(1600, 1000, 24); var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false); var prior = RenderTexture.active;
        try { Canvas.ForceUpdateCanvases(); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = prior; cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(camObj); }
    }
}
