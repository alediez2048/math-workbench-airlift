using System;
using System.Linq;
using Airlift.Presentation.Dashboard;
using Airlift.Welcome;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// CC-HF-01. The six coming-soon tiles show an illustration of their world, dimmed like the rest of the tile so
// they never read as playable. Run after BuildDashboard (which recreates the Soon tiles without art). Idempotent.
// Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/ApplyComingSoonArt.cs --entry ApplyComingSoonArt.Run
public static class ApplyComingSoonArt
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string ArtDir = "Assets/Airlift/Art/Tiles";

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var catalog = n.catalogRoot.transform;
        var tile = DashboardWall.TileSize;
        int placed = 0;
        foreach (var world in DashboardCatalog.ComingSoon)
        {
            var soon = catalog.Find("Soon " + world.Id) ?? throw new InvalidOperationException("Soon " + world.Id + " missing: run BuildDashboard first.");
            var art = soon.Find("Art") ?? throw new InvalidOperationException("Soon " + world.Id + "/Art missing.");
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtDir + "/" + world.Id + ".png");
            if (sprite == null) continue;
            var old = art.Find("Photo"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var photo = new GameObject("Photo", typeof(RectTransform), typeof(Image)); photo.transform.SetParent(art, false);
            var pr = (RectTransform)photo.transform; pr.anchoredPosition = new Vector2(0f, -6f); pr.sizeDelta = new Vector2(tile.x - 24f, tile.y / 2f - 12f);
            var img = photo.GetComponent<Image>(); img.sprite = sprite; img.type = Image.Type.Simple; img.preserveAspect = false; img.raycastTarget = false;
            img.color = new Color(1f, 1f, 1f, 0.6f);
            placed++;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return placed + " of " + DashboardCatalog.ComingSoon.Length + " coming-soon tiles have art";
    }
}
