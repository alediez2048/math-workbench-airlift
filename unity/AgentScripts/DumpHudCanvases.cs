using System.Linq;
using Airlift.Welcome;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class DumpHudCanvases
{
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        string D(Transform t)
        {
            if (t == null) return "null";
            var r = t as RectTransform; var clip = t.GetComponentInChildren<Oculus.Interaction.Surfaces.BoundsClipper>(true);
            var sur = t.GetComponentInChildren<Oculus.Interaction.Surfaces.ClippedPlaneSurface>(true);
            return t.name + " rect=" + (r != null ? r.sizeDelta.ToString() : "-") + " scale=" + t.lossyScale.x.ToString("F4") + " pos=" + t.position.ToString("F2")
                 + " clipper=" + (clip != null ? clip.Size.ToString("F3") : "none") + " surface=" + (sur != null ? "yes" : "no") + " children=" + string.Join(",", t.Cast<Transform>().Select(c => c.name));
        }
        return "welcome: " + D(n.hudWelcomeCanvas) + "\nstation: " + D(n.hudStationCanvas) + "\nbar: " + ((RectTransform)n.hudRoot.transform).sizeDelta + " at " + ((RectTransform)n.hudRoot.transform).anchoredPosition;
    }
}
