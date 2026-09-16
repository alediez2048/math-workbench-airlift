using System;
using System.Linq;
using Airlift.Lessons;
using Airlift.Onboarding;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Preview-driven follow-up to ApplyToyLook: ruler marks to the player-facing edge so
// docked pieces cannot hide them, pieces resting on the surface instead of floating,
// and a visible cream cut face on each half so two docked halves still read as two.
public static class RefineToyLook
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const float TrayHeight = 0.047f;   // strap bottom just above the deck top (0.0175)
    const float RestHeight = 0.063f;   // strap bottom just above the pad inset top (0.034)

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        var lesson = d.GetComponent<CargoLessonDirector>();
        if (lesson.restHeight < 0.07f) throw new InvalidOperationException("Refinement already applied; refusing to run twice.");

        // Ruler marks and ticks toward the player (negative z), on the pad's front edge.
        foreach (Transform child in lesson.ruler)
        {
            if (child.name.StartsWith("Tick")) child.localPosition = new Vector3(child.localPosition.x, 0.002f, -0.038f);
            if (child.name.StartsWith("Mark"))
            {
                child.localPosition = new Vector3(child.localPosition.x, 0.006f, -0.078f);
                child.localRotation = Quaternion.Euler(60, 0, 0);
                var text = child.GetComponent<TMP_Text>(); text.fontSize = 0.11f; text.rectTransform.sizeDelta = new Vector2(0.12f, 0.035f);
            }
        }

        // Heights: tray row on the deck, docked pieces on the pad.
        d.content.trayPosition = new Vector3(d.content.trayPosition.x, TrayHeight, d.content.trayPosition.z);
        d.content.targetPosition = new Vector3(d.content.targetPosition.x, RestHeight, d.content.targetPosition.z);
        EditorUtility.SetDirty(d.content);
        d.strap.localPosition = d.content.trayPosition; d.demonstrationStrap.localPosition = d.content.trayPosition;
        lesson.restHeight = RestHeight;
        foreach (var view in new[] { lesson.whole, lesson.halfA, lesson.halfB })
        {
            view.trayPosition = new Vector3(view.trayPosition.x, TrayHeight, view.trayPosition.z);
            view.piece.localPosition = view.trayPosition;
        }

        // Visible cut faces: a thin cream cap inside each half's exact length.
        float halfLength = RulerLayout.PieceLength(4);
        Cap(lesson.halfA.piece, +1, halfLength); Cap(lesson.halfB.piece, -1, halfLength);

        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        return "Ruler marks/ticks moved to the front edge; tray height " + TrayHeight + ", rest height " + RestHeight + "; cut caps on both halves.";
    }

    static void Cap(Transform piece, int side, float length)
    {
        var cap = piece.Find("Cut edge");
        if (cap == null) throw new InvalidOperationException("Cut edge missing on " + piece.name);
        cap.localPosition = new Vector3(side * (length * 0.5f - 0.0016f), 0, 0);
        cap.gameObject.SetActive(true);
    }
}
