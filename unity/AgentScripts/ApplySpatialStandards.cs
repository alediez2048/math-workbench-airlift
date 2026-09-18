using System;
using System.Collections.Generic;
using System.Linq;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Presentation;
using Airlift.Welcome;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Owner 2026-09-17: "we need to standardize spacial dimensons across the experience ... the whiteboard should be at
// the same distance as the workbenches, titles should all be the same size and fonts, dashboards should be the same
// size." Measured first: six panel sizes, 1.85 m vs 0.65 m, headings at 24/36/40/49 mm, and the workbench cards
// still set in Nunito because the Nerdy font rule only ever scanned the welcome panel.
// This applies NerdySpace to every panel: one size, one canvas scale, one distance, one type ramp, Nerdy fonts.
// The owner chose reading distance as the anchor, which moves the lesson card too — an approved exception to the
// Cargo Crew lock, recorded in CARGO-CREW-LOCK.md. Idempotent. Saves CargoCrew and the onboarding asset.
// Run: unity command run_script --file AgentScripts/ApplySpatialStandards.cs --entry ApplySpatialStandards.Run
public static class ApplySpatialStandards
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset")
                    ?? throw new InvalidOperationException("NerdyStyle.asset missing.");
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>(FindObjectsInactive.Include);
        var log = new List<string>();

        // 1. One distance, for the board you read and the card you work at alike.
        n.welcomeDistance = NerdySpace.PanelDistance;
        n.welcomeBelowEyes = NerdySpace.PanelBelowEyes;
        d.content.boardDistance = NerdySpace.PanelDistance;
        d.content.boardBelowEyes = NerdySpace.PanelBelowEyes;
        EditorUtility.SetDirty(n); EditorUtility.SetDirty(d.content);
        log.Add("distance " + NerdySpace.PanelDistance + "m, " + NerdySpace.PanelBelowEyes + "m below eyes");

        // 2. One panel size, at one canvas scale, so a millimetre is a millimetre everywhere.
        foreach (var (name, rect) in Panels(n))
        {
            rect.sizeDelta = new Vector2(NerdySpace.PanelWidth, NerdySpace.PanelHeight);
            float lossy = rect.lossyScale.x;
            if (lossy > 0.000001f && Mathf.Abs(lossy - NerdySpace.PanelScale) > 0.000005f)
            {
                float factor = NerdySpace.PanelScale / lossy;
                rect.localScale = rect.localScale * factor;
                log.Add(name + " scale x" + factor.ToString("F3"));
            }
            EditorUtility.SetDirty(rect);
        }
        log.Add(Panels(n).Count() + " panels at " + NerdySpace.PanelWidth + "x" + NerdySpace.PanelHeight);

        // 2b. The ray-pointable area IS the welcome canvas rect (RayInteractable -> ClippedPlaneSurface clipped to
        //     it). It was still 960x640 after the panels grew, so the gear and Help sat where no ray could land.
        //     Owner 2026-09-17: "gear icon does nothing when i click on it."
        var canvas = (RectTransform)n.hudWelcomeCanvas;
        canvas.sizeDelta = new Vector2(NerdySpace.PanelWidth, NerdySpace.PanelHeight);
        EditorUtility.SetDirty(canvas);
        int clippers = 0;
        foreach (var c in canvas.GetComponentsInChildren<Component>(true))
        {
            if (c == null || !c.GetType().Name.Contains("BoundsClipper")) continue;
            var so = new SerializedObject(c);
            var size = so.FindProperty("_size");
            if (size == null) continue;
            var v = size.vector3Value;
            size.vector3Value = new Vector3(NerdySpace.PanelWidth, NerdySpace.PanelHeight, v.z);
            so.ApplyModifiedPropertiesWithoutUndo(); clippers++;
        }
        log.Add("pointable canvas " + canvas.sizeDelta.x + "x" + canvas.sizeDelta.y + ", " + clippers + " clipper(s) resized");

        // 3. One type ramp and the Nerdy families, everywhere — including the workbench cards the old font rule
        //    never looked at.
        int retyped = 0, resized = 0;
        foreach (var t in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TMP_Text>(true)))
        {
            var font = FontFor(t, style);
            if (font != null && t.font != font) { t.font = font; retyped++; EditorUtility.SetDirty(t); }
            float size = IsHeading(t) ? NerdySpace.Heading
                       : t.name == "Body" || t.name == "Instruction" ? NerdySpace.Body
                       : 0f;
            if (size > 0f && Mathf.Abs(t.fontSize - size) > 0.5f)
            {
                t.fontSize = size; resized++; EditorUtility.SetDirty(t);
            }
        }
        log.Add(retyped + " texts moved to the Nerdy families, " + resized + " texts to the standard ramp");

        // The workbenches bind fonts from AirliftStyle at build time; re-running that binding keeps the asset and
        // the scene saying the same thing.
        int bound = 0;
        foreach (var b in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TypographyBindings>(true)))
        {
            if (b.style == null) continue;
            b.Apply(); bound++; EditorUtility.SetDirty(b);
        }
        log.Add(bound + " typography bindings re-applied");

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        AssetDatabase.SaveAssets();
        return string.Join("; ", log);
    }

    static IEnumerable<(string name, RectTransform rect)> Panels(NerdyDirector n)
    {
        yield return ("consent", (RectTransform)n.consentRoot.transform);
        yield return ("welcome", (RectTransform)n.welcomeRoot.transform);
        yield return ("catalog", (RectTransform)n.catalogRoot.transform);
        foreach (var s in UnityEngine.Object.FindObjectsByType<LessonStation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (s.heading != null && s.heading.transform.parent is RectTransform card)
                yield return (s.cardId, card);
    }

    /// Two faces, and only two: the workbenches bind their text through the older AirliftStyle asset, which holds
    /// exactly one heading font and one body font. Using a third weight here would leave the two assets disagreeing
    /// and the wiring tests flapping between them. Headings take the display face, everything else the body face.
    static TMP_FontAsset FontFor(TMP_Text t, NerdyStyle style) =>
        IsHeading(t) ? style.displayFont : style.bodyFont;

    static bool IsHeading(TMP_Text t) => t.name == "Heading" || t.name == "Title" || t.name == "Catalog title";
}
