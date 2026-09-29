using System.IO;
using Airlift.Lounge;
using UnityEditor.SceneManagement;
using UnityEngine;
// Close-ups of the lounge finish (floor, wall panel and rail) in Nerdy lounge mode. Never saves.
// Run: unity command run_script --file AgentScripts/PreviewRoomClose.cs --entry PreviewRoomClose.Run
public static class PreviewRoomClose
{
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var room = Object.FindAnyObjectByType<LoungeRoom>(FindObjectsInactive.Include);
        room.Show(true); room.Apply(Scenery.NerdyLounge);
        var onboarding = Object.FindAnyObjectByType<Airlift.Onboarding.OnboardingDirector>(FindObjectsInactive.Include);
        if (onboarding != null) foreach (Transform c in onboarding.transform) c.gameObject.SetActive(false);
        string dir = Path.GetFullPath("../artifacts/lounge"); Directory.CreateDirectory(dir);
        Shot(new Vector3(0f, 1.3f, -0.5f), new Vector3(0.8f, 0f, -2.2f), Path.Combine(dir, "room-floor.png"));
        Shot(new Vector3(0f, 1.25f, 0f), new Vector3(-1.4f, 1.0f, -2.6f), Path.Combine(dir, "room-wall.png"));
        return "room close-ups in artifacts/lounge/room-{floor,wall}.png";
    }

    static void Shot(Vector3 from, Vector3 at, string path)
    {
        var go = new GameObject("cam") { hideFlags = HideFlags.HideAndDontSave }; var cam = go.AddComponent<Camera>();
        cam.transform.SetPositionAndRotation(from, Quaternion.LookRotation(at - from)); cam.fieldOfView = 70; cam.nearClipPlane = 0.05f;
        cam.clearFlags = RenderSettings.skybox != null ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
        var rt = new RenderTexture(1200, 800, 24); var tex = new Texture2D(1200, 800, TextureFormat.RGB24, false);
        try { cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1200, 800), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = null; cam.targetTexture = null; Object.DestroyImmediate(tex); Object.DestroyImmediate(rt); Object.DestroyImmediate(go); }
    }
}
