using System;
using System.Collections.Generic;
using System.Linq;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Presentation;
using Airlift.Welcome;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// CC-PL-03: wires the lesson stations in CargoCrew. Idempotent. Adds CargoStation to the Cargo station root (the
// OnboardingDirector object) with its visual roots (Workbench, Cargo terminal, Lesson interface), card text and
// button groups; fills NerdyDirector.stations and TableHandle.stations with every LessonStation in the scene
// (Cargo first, then catalog order). Leaves NerdyDirector.stationVisuals and fallbackButtons as they are (the
// Table handle stays shared; existing wiring tests pin both). Rerun after a café or garden builder adds its station.
// Run: unity command run_script --file AgentScripts/WireLessonStations.cs --entry WireLessonStations.Run
public static class WireLessonStations
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>(FindObjectsInactive.Include);
        var lesson = d != null ? d.GetComponent<CargoLessonDirector>() : null;
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        if (d == null || lesson == null || n == null) throw new InvalidOperationException("CargoCrew onboarding, lesson or Nerdy director missing.");
        var report = new List<string>();

        // 1. Cargo station on the Cargo station root.
        var cargo = d.GetComponent<CargoStation>();
        if (cargo == null) { cargo = d.gameObject.AddComponent<CargoStation>(); report.Add("CargoStation added"); }
        cargo.cardId = CargoStation.CardId;
        cargo.onboarding = d; cargo.lesson = lesson;
        cargo.heading = d.heading; cargo.body = d.body;
        var roots = new[] { "Workbench", "Cargo terminal", "Lesson interface" }.Select(name => Child(d.transform, name)).ToArray();
        if (roots.Any(r => r == null)) throw new InvalidOperationException("Cargo visual roots missing: " + string.Join(", ", new[] { "Workbench", "Cargo terminal", "Lesson interface" }.Where((name, i) => roots[i] == null)));
        cargo.visualRoots = roots.Select(r => r.gameObject).ToArray();
        cargo.fallbackGroups = (n.fallbackButtons ?? new CanvasGroup[0]).Where(g => g != null && g.transform.IsChildOf(d.transform)).ToArray();
        EditorUtility.SetDirty(cargo);
        report.Add("CargoStation: " + cargo.visualRoots.Length + " visual roots, " + cargo.fallbackGroups.Length + " button groups");

        // 2. Every station in the scene, Cargo first then catalog order.
        var stations = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LessonStation>(true))
            .OrderBy(s => LessonCatalog.IndexOf(s.cardId)).ToArray();
        n.stations = stations;
        EditorUtility.SetDirty(n);
        var handle = d.GetComponent<TableHandle>();
        if (handle != null) { handle.stations = stations; EditorUtility.SetDirty(handle); }
        report.Add("stations: " + string.Join(", ", stations.Select(s => s.cardId + " on " + s.gameObject.name)) + (handle != null ? "; table handle wired" : "; no table handle"));

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "WireLessonStations: " + string.Join(" | ", report);
    }

    static Transform Child(Transform parent, string name) { foreach (Transform c in parent) if (c.name == name) return c; return null; }
}
