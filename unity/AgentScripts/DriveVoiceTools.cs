using System.Reflection;
using Airlift.Onboarding;
using Airlift.Welcome;
using UnityEditor.SceneManagement;
using UnityEngine;
// Diagnostic: calls NerdyDirector's tool handler as the Realtime guide would (no socket), then reloads the scene.
public static class DriveVoiceTools
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var n = Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var d = n.onboarding; var sb = new System.Text.StringBuilder();
        var tool = typeof(NerdyDirector).GetMethod("OnToolCall", BindingFlags.NonPublic | BindingFlags.Instance);
        var step = typeof(NerdyDirector).GetMethod("CurrentStep", BindingFlags.NonPublic | BindingFlags.Instance);
        try
        {
            // Start() does not run in edit mode: mirror the one line of it this path needs.
            n.cargoLesson = Object.FindAnyObjectByType<Airlift.Lessons.CargoLessonDirector>(FindObjectsInactive.Include);
            n.ConsentNoVoice(); n.GoToCatalog();
            sb.AppendLine("phase before: " + n.Flow.Phase);
            tool.Invoke(n, new object[] { "open_lesson", "c1", "{\"cardId\":\"neighborhood_cafe_division\"}" });
            sb.AppendLine("after open cafe: " + n.Flow.Phase + " caption='" + n.captionText.text + "'");
            tool.Invoke(n, new object[] { "open_lesson", "c2", "{\"cardId\":\"cargo_crew_fractions\"}" });
            var s = (GuideStep)step.Invoke(n, null);
            sb.AppendLine("after open cargo: " + n.Flow.Phase + " stage=" + d.Stage + " step=" + s.Id + " canGrab=" + s.CanGrabNow + " stationActive=" + d.station.activeSelf);
            tool.Invoke(n, new object[] { "advance_step", "c3", "{}" });
            s = (GuideStep)step.Invoke(n, null);
            sb.AppendLine("after yes: stage=" + d.Stage + " step=" + s.Id + " canGrab=" + s.CanGrabNow);
            tool.Invoke(n, new object[] { "open_lesson", "c4", "{\"cardId\":\"cargo_crew_fractions\"}" });
            sb.AppendLine("open again inside lesson: " + n.Flow.Phase + " stage=" + d.Stage);
        }
        finally { EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single); }
        return sb.ToString();
    }
}
