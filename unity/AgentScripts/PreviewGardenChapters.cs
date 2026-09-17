using System;
using System.IO;
using System.Linq;
using Airlift.Lessons.Garden;
using Airlift.Onboarding;
using Airlift.Welcome;
using Newtonsoft.Json.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

// Renders the five Sunny Plot chapters from the seated head in the editor, driving GardenStation through its real
// paths: DropAt for strips, TurnBed, DropFenceAt and the garden_split_bed tool, Check. Opens the lesson through
// NerdyDirector.SelectCard with the catalog card made playable in memory only. Reloads the scene afterwards; saves
// nothing. Output: artifacts/garden/<n>a-start.png, <n>b-planted.png, <n>c-accepted.png plus 0-briefing.png.
// Run: unity command run_script --file AgentScripts/PreviewGardenChapters.cs --entry PreviewGardenChapters.Run
public static class PreviewGardenChapters
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var station = UnityEngine.Object.FindAnyObjectByType<GardenStation>(FindObjectsInactive.Include);
        if (n == null || n.onboarding == null || station == null) throw new InvalidOperationException("Nerdy director, onboarding or Garden workbench missing (run BuildGardenWorkbench first).");
        var d = n.onboarding;
        var card = LessonCatalog.Find(station.cardId);
        bool wasPlayable = card != null && card.Playable;
        var sb = new System.Text.StringBuilder();
        string dir = Path.GetFullPath("../artifacts/garden"); Directory.CreateDirectory(dir);
        try
        {
            if (card != null) card.Playable = true;
            if (n.stations != null && n.stations.Length > 0 && !n.stations.Contains(station)) n.stations = n.stations.Concat(new Airlift.Lessons.LessonStation[] { station }).ToArray();
            var headPos = new Vector3(0, 1.25f, 0);
            var pose = OnboardingPlacement.BoardPose(headPos, Vector3.forward, d.content.boardDistance, d.content.boardBelowEyes);
            d.transform.SetPositionAndRotation(pose.position, pose.rotation);
            n.head.SetPositionAndRotation(headPos, Quaternion.LookRotation(new Vector3(0, -0.45f, 1f)));

            n.ConsentNoVoice(); n.GoToCatalog(); n.SelectCard(station.cardId);
            if (!station.IsOpen) { sb.AppendLine("SelectCard did not open the garden (station not routed yet); opened directly"); station.Open(); }
            sb.AppendLine("briefing: heading='" + station.heading.text + "' step=" + station.CurrentStep().Id + " garden root active=" + station.gameObject.activeInHierarchy);
            Shot(n.head, Path.Combine(dir, "0-briefing.png"));
            var start = station.Advance();
            sb.AppendLine("advance ok=" + start.Ok + " '" + start.Reason + "'");

            var m = station.Model;
            for (int i = 0; i < GardenChapter.All.Count; i++)
            {
                if (m.ChapterIndex != i) station.JumpToChapter(i);
                var c = m.Chapter;
                sb.AppendLine("ch" + c.Number + " " + c.Id + ": heading='" + station.heading.text + "' body='" + station.body.text.Replace("\n", " | ") + "' hints='" + station.sayHints.text + "' tools=" + string.Join(",", station.ToolsNow));
                Shot(n.head, Path.Combine(dir, c.Number + "a-start.png"));

                if (!c.StartsPlanted)
                {
                    var L = station.BedLayout;
                    // One honest mistake first: a short strip in the last row, checked, then taken back to the tray.
                    var shortId = m.StripIds.FirstOrDefault(id => m.StripLength(id) < m.Columns);
                    if (shortId != null)
                    {
                        bool planted = station.DropAt(shortId, L.StripPosition(m.Rows - 1, m.StripLength(shortId)) + new Vector3(0.01f, 0.02f, 0.01f));
                        var early = station.Check();
                        sb.AppendLine("  short strip " + shortId + " planted=" + planted + " check ok=" + early.Ok + " '" + early.Reason + "'");
                        bool back = station.DropAt(shortId, new Vector3(0.45f, 0.05f, -0.3f));
                        sb.AppendLine("  " + shortId + " dropped off the bed: planted=" + back + " row=" + m.RowOf(shortId));
                    }
                    for (int r = 0; r < m.Rows; r++)
                    {
                        var id = m.StripIds.FirstOrDefault(s => m.RowOf(s) < 0 && m.StripLength(s) == m.Columns);
                        if (id == null) { sb.AppendLine("  no full strip left for row " + (r + 1)); break; }
                        bool ok = station.DropAt(id, L.StripPosition(r, m.Columns) + new Vector3(-0.012f, 0.03f, 0.008f));
                        sb.AppendLine("  drop " + id + " -> row " + (r + 1) + " ok=" + ok + (ok ? "" : " '" + station.Feedback + "'"));
                    }
                }
                if (c.NeedsTurn)
                {
                    var turn = station.TurnBed();
                    sb.AppendLine("  turn ok=" + turn.Ok + " '" + turn.Reason + "' bed " + m.Rows + "x" + m.Columns + " strips=" + m.StripIds.Count);
                }
                if (c.HasFence)
                {
                    var L = station.BedLayout;
                    if (c.RequiredFenceColumn > 0)
                    {
                        bool wrong = station.DropFenceAt(new Vector3(L.BoundaryX(c.RequiredFenceColumn - 2) + 0.01f, 0.05f, L.Center.z));
                        var wrongCheck = station.Check();
                        sb.AppendLine("  fence dropped at " + m.FenceColumn + " ok=" + wrong + " check ok=" + wrongCheck.Ok + " '" + wrongCheck.Reason + "'");
                        bool placed = station.DropFenceAt(new Vector3(L.BoundaryX(c.RequiredFenceColumn) - 0.012f, 0.05f, L.Center.z + 0.04f));
                        sb.AppendLine("  fence dropped near line " + c.RequiredFenceColumn + " ok=" + placed + " fence=" + m.FenceColumn);
                    }
                    else
                    {
                        station.TryLessonTool(GardenStation.ToolSplit, new JObject { ["columns"] = 2 }, out var split);
                        sb.AppendLine("  garden_split_bed columns=2 ok=" + split.Ok + " '" + split.Reason + "' fence=" + m.FenceColumn);
                    }
                }
                Shot(n.head, Path.Combine(dir, c.Number + "b-planted.png"));
                station.TryLessonTool(GardenStation.ToolCheck, null, out var check);
                sb.AppendLine("  check ok=" + check.Ok + " '" + check.Reason + "' complete=" + m.ChapterComplete + " expr='" + station.expressionLine.text + "' payoff=" + station.PayoffShown
                    + " step=" + station.CurrentStep().Id);
                Shot(n.head, Path.Combine(dir, c.Number + "c-accepted.png"));
            }
            station.Close();
            sb.AppendLine("closed: open=" + station.IsOpen + " root active=" + station.gameObject.activeSelf);
        }
        finally
        {
            if (card != null) card.Playable = wasPlayable;
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
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
