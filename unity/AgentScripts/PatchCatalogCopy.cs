using System;
using System.Linq;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
// Copies LessonCatalog descriptions into the baked card texts (CreateNerdyWelcome bakes them). Saves CargoCrew.
public static class PatchCatalogCopy
{
    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        string report = "";
        foreach (var card in LessonCatalog.Cards)
        {
            var go = n.catalogRoot.transform.Find("Card " + card.Id); if (go == null) throw new InvalidOperationException("missing card " + card.Id);
            var desc = go.Find("Description").GetComponent<TMP_Text>();
            if (desc.text != card.Description) { desc.text = card.Description; EditorUtility.SetDirty(desc); report += card.Id + " updated; "; }
        }
        if (report.Length == 0) return "Card copy already current.";
        EditorSceneManager.MarkSceneDirty(scene); if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("save failed");
        return report + "scene saved.";
    }
}
