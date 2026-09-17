using System.Linq;
using Airlift.Guide;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
// The guide proxy was deployed to Vercel on 2026-09-17, so the scene must mint over https instead of the Mac's
// LAN alias (which dies on every reboot). GuideSession.mintUrl is serialised in the scene, so the constant in
// GuideEndpoints is only documentation until this bakes it in. Idempotent; safe to rerun after the URL changes.
// Run: unity command run_script --file AgentScripts/SetMintUrl.cs --entry SetMintUrl.Run
public static class SetMintUrl
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var sessions = scene.GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<GuideSession>(true)).ToArray();
        if (sessions.Length == 0) return "no GuideSession in " + scenePath;

        int changed = 0;
        foreach (var s in sessions)
        {
            if (s.mintUrl == GuideEndpoints.MintUrl) continue;
            Undo.RecordObject(s, "Set mint url");
            s.mintUrl = GuideEndpoints.MintUrl;
            EditorUtility.SetDirty(s);
            changed++;
        }
        if (changed > 0) EditorSceneManager.SaveScene(scene);
        return "GuideSession count " + sessions.Length + ", changed " + changed + " -> " + GuideEndpoints.MintUrl;
    }
}
