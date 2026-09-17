using System;
using System.IO;
using System.Linq;
using System.Text;
using Airlift.Lounge;
using Airlift.Onboarding;
using Airlift.Welcome;
using UnityEditor.SceneManagement;
using UnityEngine;

// CC-FD-01. Renders the lounge from the seated head in both sceneries, with the consent card in place, into
// artifacts/lounge/. Also looks left at Dee's seat and back at the seating, so the room is judged as a room and
// not as one framing. Reloads the scene afterwards; saves nothing.
// Run: unity command run_script --file AgentScripts/PreviewLounge.cs --entry PreviewLounge.Run
public static class PreviewLounge
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>(FindObjectsInactive.Include);
        var room = UnityEngine.Object.FindAnyObjectByType<LoungeRoom>(FindObjectsInactive.Include);
        if (n == null || d == null || room == null) throw new InvalidOperationException("Run BuildLounge first.");

        var sb = new StringBuilder();
        string dir = Path.GetFullPath("../artifacts/lounge"); Directory.CreateDirectory(dir);
        try
        {
            var headPos = new Vector3(0f, 1.25f, 0f);
            var pose = OnboardingPlacement.BoardPose(headPos, Vector3.forward, d.content.boardDistance, d.content.boardBelowEyes);
            d.transform.SetPositionAndRotation(pose.position, pose.rotation);

            // The arrival is the lounge and the card, nothing else: hide the workbench so the room is judged as
            // the learner first meets it. CC-FD-04 does this at runtime; here it is just the framing.
            foreach (Transform child in d.transform)
                if (child.name != "Lesson interface" && child.name != "Table handle" && child.name != "Guide HUD canvas")
                    child.gameObject.SetActive(false);

            foreach (var scenery in new[] { Scenery.NerdyLounge, Scenery.YourRoom })
            {
                room.Apply(scenery);
                string tag = scenery == Scenery.NerdyLounge ? "lounge" : "yourroom";
                sb.AppendLine(tag + ": shell=" + room.shell.activeSelf + " furniture=" + room.furniture.activeSelf
                              + " passthrough=" + (room.passthrough != null && room.passthrough.enabled));

                Look(n.head, headPos, new Vector3(0f, -0.28f, 1f));
                Shot(n.head, Path.Combine(dir, tag + "-1-ahead.png"));
                Look(n.head, headPos, new Vector3(-0.85f, -0.22f, 1f));
                Shot(n.head, Path.Combine(dir, tag + "-2-dee.png"));
                Look(n.head, headPos, new Vector3(0.9f, -0.1f, 0.6f));
                Shot(n.head, Path.Combine(dir, tag + "-3-right.png"));
                Look(n.head, headPos, new Vector3(0f, -0.1f, -1f));
                Shot(n.head, Path.Combine(dir, tag + "-4-behind.png"));
            }

            room.Apply(LoungeScenery.Default);
            var seat = room.transform.Find("Furniture/Dee seat");
            sb.AppendLine("Dee seat at " + seat.position.ToString("F2"));
            sb.AppendLine("renderers " + room.GetComponentsInChildren<Renderer>(true).Length
                          + ", lights " + room.GetComponentsInChildren<Light>(true).Length);
            return sb.ToString() + "renders in artifacts/lounge/";
        }
        finally { EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single); }
    }

    static void Look(Transform head, Vector3 at, Vector3 forward) =>
        head.SetPositionAndRotation(at, Quaternion.LookRotation(forward.normalized));

    static void Shot(Transform head, string path)
    {
        var camObj = new GameObject("Preview cam") { hideFlags = HideFlags.HideAndDontSave }; var cam = camObj.AddComponent<Camera>();
        cam.transform.SetPositionAndRotation(head.position, head.rotation); cam.fieldOfView = 70; cam.nearClipPlane = 0.05f;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.06f, 0.07f, 0.1f);
        var rt = new RenderTexture(1600, 1000, 24); var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false); var prior = RenderTexture.active;
        try { Canvas.ForceUpdateCanvases(); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = prior; cam.targetTexture = null; UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(camObj); }
    }
}
