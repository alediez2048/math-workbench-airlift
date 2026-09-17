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
            // Concept intro frames (2026-09-17): CargoStation runs the intro from Ready, then chapter 1 begins.
            var cargoStation = d.GetComponent<Airlift.Lessons.CargoStation>();
            if (cargoStation != null)
            {
                // Drive onboarding to Ready the way the golden voice test does: briefing, demo, practice release on the pad.
                const BindingFlags priv = BindingFlags.NonPublic | BindingFlags.Instance;
                var flow = typeof(OnboardingDirector).GetField("flow", priv).GetValue(d);
                d.Continue(); d.Continue();
                typeof(OnboardingDirector).GetField("demonstration", priv).SetValue(d, null);
                flow.GetType().GetMethod("FinishDemonstration").Invoke(flow, null);
                typeof(OnboardingDirector).GetMethod("ResetStrap", priv).Invoke(d, null);
                typeof(OnboardingDirector).GetMethod("Refresh", priv).Invoke(d, null);
                Shot(n.head, Path.Combine(dir, "0-practice.png"));
                void Pointer(Oculus.Interaction.PointerEventType type) => typeof(OnboardingDirector).GetMethod("OnPointer", priv).Invoke(d, new object[] { new Oculus.Interaction.PointerEvent(7, type, Pose.identity) });
                Pointer(Oculus.Interaction.PointerEventType.Select);
                d.strap.localPosition = d.content.targetPosition;
                Pointer(Oculus.Interaction.PointerEventType.Unselect);
                int generation = (int)typeof(OnboardingDirector).GetField("generation", priv).GetValue(d);
                var check = (System.Collections.IEnumerator)typeof(OnboardingDirector).GetMethod("CheckRelease", priv).Invoke(d, new object[] { generation });
                while (check.MoveNext()) { }
                Shot(n.head, Path.Combine(dir, "0-ready.png"));
                sb.AppendLine("onboarding stage before intro: " + d.Stage);
                cargoStation.ContinueFromReady();
                for (int intro = 1; cargoStation.CurrentIntro != null && intro < 8; intro++)
                {
                    Shot(n.head, Path.Combine(dir, "0-intro" + intro + ".png"));
                    sb.AppendLine("intro " + intro + " heading='" + d.heading.text + "' body='" + d.body.text.Replace("\n", " | ") + "'");
                    cargoStation.Advance();
                }
            }
            if (!lesson.IsActive) lesson.Begin();
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
                var splitter = UnityEngine.Object.FindAnyObjectByType<Airlift.Presentation.CrateSplitter>(FindObjectsInactive.Include);
                if (splitter != null && lesson.CanSplit && i >= 1)
                {
                    // A crate set on the splitter pad before choosing the size (2026-09-17 splitter frame).
                    var crate = i == 1 ? lesson.whole.piece : lesson.halfA.piece;
                    var saved = crate.localPosition;
                    var pad = splitter.padCenter;
                    crate.localPosition = new Vector3(pad.x, crate.localPosition.y, pad.z);
                    Shot(n.head, Path.Combine(dir, (i + 1) + "s-splitter.png"));
                    Shot(n.head, Path.Combine(dir, (i + 1) + "s-splitter-wide.png"), 95f);
                    crate.localPosition = saved;
                }
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
                if (i < 3)
                {
                    // Departure frames (owner 2026-09-17: turn left, out through the DOCK EXIT), then restore the docked state.
                    var cargo = (System.Collections.Generic.IList<Transform>)typeof(CargoLessonDirector).GetMethod("DockedCargo", bind).Invoke(lesson, null);
                    var saved = cargo.Select(t => (t, t.position, t.rotation)).ToList();
                    var bay = lesson.vehicles;
                    var frames = new[] { bay.tagSeconds + 0.45f, bay.tagSeconds + bay.turnSeconds + 0.35f, bay.DepartureSeconds - 0.9f };
                    for (int f = 0; f < frames.Length; f++) { bay.ShowDepartureAt(cargo, frames[f]); Shot(n.head, Path.Combine(dir, (i + 1) + "d-depart" + f + ".png")); }
                    bay.ShowDepartureAt(cargo, bay.tagSeconds + bay.turnSeconds + 0.35f);
                    var side = SideView(d.transform); Shot(side, Path.Combine(dir, (i + 1) + "e-side.png")); UnityEngine.Object.DestroyImmediate(side.gameObject);
                    bay.ShowChapter(lesson.Chapter);
                    foreach (var (t, p, r) in saved) { t.gameObject.SetActive(true); t.SetPositionAndRotation(p, r); }
                }
                var load = lesson.TryLoad();
                sb.AppendLine("  load ok=" + load.Ok + " reason='" + load.Reason + "' complete=" + lesson.ChapterComplete + " expr='" + lesson.ExpressionText + "' feedback='" + lesson.Feedback + "'");
                Shot(n.head, Path.Combine(dir, (i + 1) + "c-loaded.png"));
            }
        }
        finally { EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single); }
        return sb.ToString();
    }
    static Transform SideView(Transform station)
    {
        var t = new GameObject("Side view") { hideFlags = HideFlags.HideAndDontSave }.transform;
        t.position = station.TransformPoint(new Vector3(-0.25f, 0.32f, -0.62f));
        t.rotation = Quaternion.LookRotation(station.TransformPoint(new Vector3(-0.1f, 0.05f, 0f)) - t.position, station.up);
        return t;
    }

    static void Shot(Transform head, string path) => Shot(head, path, 70f);
    static void Shot(Transform head, string path, float fov)
    {
        var camObj = new GameObject("Preview cam") { hideFlags = HideFlags.HideAndDontSave }; var cam = camObj.AddComponent<Camera>();
        cam.transform.SetPositionAndRotation(head.position, head.rotation); cam.fieldOfView = fov; cam.nearClipPlane = 0.05f; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.58f, 0.64f);
        var rt = new RenderTexture(1600, 1000, 24); var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false); var prior = RenderTexture.active;
        try { Canvas.ForceUpdateCanvases(); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = prior; cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(camObj); }
    }
}
