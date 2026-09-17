using System;
using System.IO;
using System.Linq;
using System.Text;
using Airlift.Lessons;
using Airlift.Lessons.Cafe;
using Airlift.Onboarding;
using Airlift.Presentation.Cafe;
using Airlift.Welcome;
using UnityEditor.SceneManagement;
using UnityEngine;

// Drives the five Corner Café chapters through the real drop path (CafeStation.DropAt) and the voice tool entry
// (TryLessonTool) in the editor, rendering the briefing and each stage's start, placed and accepted states from the
// seated head into artifacts/cafe/. Reloads the scene afterwards; saves nothing.
// Run: unity command run_script --file AgentScripts/PreviewCafeChapters.cs --entry PreviewCafeChapters.Run
public static class PreviewCafeChapters
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>(FindObjectsInactive.Include);
        var station = UnityEngine.Object.FindAnyObjectByType<CafeStation>(FindObjectsInactive.Include);
        if (n == null || d == null || station == null) throw new InvalidOperationException("Nerdy director, onboarding or CafeStation missing; run BuildCafeWorkbench first.");
        var sb = new StringBuilder(); string dir = Path.GetFullPath("../artifacts/cafe"); Directory.CreateDirectory(dir);
        try
        {
            var headPos = new Vector3(0, 1.25f, 0);
            var pose = OnboardingPlacement.BoardPose(headPos, Vector3.forward, d.content.boardDistance, d.content.boardBelowEyes);
            d.transform.SetPositionAndRotation(pose.position, pose.rotation);
            n.head.SetPositionAndRotation(headPos, Quaternion.LookRotation(new Vector3(0, -0.45f, 1f)));
            n.ConsentNoVoice(); n.GoToCatalog();
            try { n.SelectCard(station.cardId); } catch (Exception e) { sb.AppendLine("SelectCard threw (" + e.GetType().Name + "): " + e.Message); }
            if (!station.IsOpen)
            {
                // Café routing not live yet (catalog card not playable or platform not integrated): show only the café.
                sb.AppendLine("routing: SelectCard did not open the café; opening the station directly");
                foreach (Transform child in d.transform)
                    if (child != station.transform && child.name != "Table handle" && child.name != "Guide HUD canvas") child.gameObject.SetActive(false);
                if (n.catalogRoot != null) n.catalogRoot.SetActive(false);
                station.Open();
            }
            else sb.AppendLine("routing: SelectCard opened the café");
            sb.AppendLine("visible siblings: " + string.Join(", ", d.transform.Cast<Transform>().Where(t => t.gameObject.activeSelf).Select(t => t.name)));
            Shot(n.head, Path.Combine(dir, "0-briefing.png"));
            sb.AppendLine("briefing heading='" + station.heading.text + "' body='" + Flat(station.body.text) + "' hints='" + station.sayHints.text + "' tools=" + string.Join("/", station.ToolsNow));
            var start = station.Advance();
            sb.AppendLine("advance ok=" + start.Ok + " '" + start.Reason + "'");
            // Concept intro frames (2026-09-17): one render per step, then on to chapter 1.
            for (int intro = 1; station.CurrentIntro != null && intro < 8; intro++)
            {
                Shot(n.head, Path.Combine(dir, "0-intro" + intro + ".png"));
                sb.AppendLine("intro " + intro + " heading='" + station.heading.text + "' body='" + station.body.text.Replace("\n", " | ") + "'");
                station.Advance();
            }

            for (int i = 0; i < CafeChapter.All.Count; i++)
            {
                station.JumpToChapter(i);
                var m = station.Model;
                for (int stage = 0; stage < m.Chapter.Stages.Length; stage++)
                {
                    string tag = (i + 1) + (m.Chapter.Stages.Length > 1 ? "s" + (stage + 1) : "");
                    Shot(n.head, Path.Combine(dir, tag + "a-start.png"));
                    sb.AppendLine("ch" + tag + " start: heading='" + station.heading.text + "' body='" + Flat(station.body.text) + "' hints='" + station.sayHints.text + "' step='" + station.CurrentStep().Id + "' tools=" + string.Join("/", station.ToolsNow));
                    var kind = m.Stage.Kind; int count = m.ContainerCount;
                    if (kind == CafeTargetKind.Plates)
                    {
                        // One deal by voice tool, the rest by releasing pastries over the plates.
                        station.TryLessonTool("cafe_deal_round", null, out var dealt);
                        sb.AppendLine("  deal ok=" + dealt.Ok + " '" + dealt.Reason + "'");
                        int plate = 0;
                        foreach (var id in m.ItemIds.ToList())
                        {
                            if (m.ContainerOf(id) >= 0) continue;
                            bool ok = station.DropAt(id, CafeLayout.ContainerCenter(kind, plate, count) + new Vector3(0.01f, 0.1f, 0f));
                            if (!ok) sb.AppendLine("  drop " + id + " -> plate " + (plate + 1) + " refused '" + station.Feedback + "'");
                            plate = (plate + 1) % count;
                        }
                    }
                    else
                    {
                        int cap = m.Stage.BoxCapacity, box = 0;
                        // Show a refusal once: one pastry too many for box 1.
                        foreach (var id in m.ItemIds.ToList())
                        {
                            while (box < count && m.CountIn(box) >= cap) box++;
                            if (box >= count) break;
                            station.DropAt(id, CafeLayout.ContainerCenter(kind, box, count) + new Vector3(0f, 0.1f, 0.01f));
                        }
                        var spare = m.ItemIds.FirstOrDefault(id => m.ContainerOf(id) < 0);
                        if (spare != null) sb.AppendLine("  unplaced " + spare);
                        var over = station.DropAt(m.ItemIds[0], CafeLayout.ContainerCenter(kind, 1, count));
                        sb.AppendLine("  move item-1 into full box 2: placed=" + over + " feedback='" + station.Feedback + "'");
                        if (!over) station.DropAt(m.ItemIds[0], CafeLayout.ContainerCenter(kind, 0, count));
                    }
                    sb.AppendLine("  counts: " + string.Join(",", Enumerable.Range(0, count).Select(m.CountIn)) + " loose=" + m.LooseCount);
                    Shot(n.head, Path.Combine(dir, tag + "b-placed.png"));
                    bool lastStage = stage == m.Chapter.Stages.Length - 1;
                    if (kind == CafeTargetKind.Boxes && station.payoff != null) station.payoff.holdOnRack = true;   // render the loaded bike
                    station.TryLessonTool("cafe_check_order", null, out var check);
                    sb.AppendLine("  check ok=" + check.Ok + " '" + check.Reason + "' complete=" + m.ChapterComplete + " stage=" + m.StageIndex + " expr='" + station.expressionLine.text + "'");
                    Shot(n.head, Path.Combine(dir, tag + "c-accepted.png"));
                    if (station.payoff != null) station.payoff.holdOnRack = false;
                    if (!check.Ok) { sb.AppendLine("  STOP: not accepted"); break; }
                    if (lastStage) sb.AppendLine("  facts: " + station.Chapter.Number + " complete=" + station.Chapter.Complete + " last=" + station.Chapter.IsLast + " tools=" + string.Join("/", station.ToolsNow));
                }
            }
            var restart = station.RestartChapter();
            sb.AppendLine("restart ok=" + restart.Ok + " '" + restart.Reason + "'");
            Shot(n.head, Path.Combine(dir, "6-restarted.png"));
            station.Close();
            sb.AppendLine("closed: open=" + station.IsOpen + " root active=" + station.gameObject.activeSelf);
        }
        finally { EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single); }
        return sb.ToString();
    }

    static string Flat(string s) => (s ?? "").Replace("\n", " | ");

    static void Shot(Transform head, string path)
    {
        var camObj = new GameObject("Preview cam") { hideFlags = HideFlags.HideAndDontSave }; var cam = camObj.AddComponent<Camera>();
        cam.transform.SetPositionAndRotation(head.position, head.rotation); cam.fieldOfView = 70; cam.nearClipPlane = 0.05f; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.58f, 0.64f);
        var rt = new RenderTexture(1600, 1000, 24); var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false); var prior = RenderTexture.active;
        try { Canvas.ForceUpdateCanvases(); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = prior; cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(camObj); }
    }
}
