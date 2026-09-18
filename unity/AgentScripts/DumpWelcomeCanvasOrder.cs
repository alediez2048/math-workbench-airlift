using System.Linq;
using Airlift.Welcome;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public static class DumpWelcomeCanvasOrder
{
    public static string Run()
    {
        EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var canvas = n.hudWelcomeCanvas;
        return string.Join(" | ", canvas.Cast<Transform>().Select((t, i) => i + ":" + t.name + (t.GetComponent<Image>() != null && t.GetComponent<Image>().raycastTarget ? "(rc)" : "")));
    }
}
