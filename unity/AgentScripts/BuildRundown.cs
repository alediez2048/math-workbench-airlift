using System;
using System.Linq;
using Airlift.Lounge;
using Airlift.Presentation;
using Airlift.Welcome;
using Oculus.Interaction;
using Oculus.Interaction.Editor.QuickActions;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// CC-FD-05a/05b. The rundown card (a copy of the consent card, stripped to eyebrow / heading / body / Continue / Skip),
// the controller helper (the real Touch Plus model from the Meta SDK with a lit button and a bouncing arrow), the
// generic block for the grab stop, and the B-button watcher. Everything is wired into NerdyDirector by reference.
// Idempotent. Run after BuildSettingsPanel, before ApplySpatialStandards. Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/BuildRundown.cs --entry BuildRundown.Run
public static class BuildRundown
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string MatDir = "Assets/Airlift/Materials/Lounge";
    const string ControllerFbx = "Packages/com.meta.xr.sdk.core/Meshes/MetaQuestTouchPlus/MetaQuestTouchPlus_Right.fbx";
    const string ConeObj = "Packages/com.meta.xr.sdk.core/Meshes/Cone.obj";
    static NerdyStyle style;

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        style = AssetDatabase.LoadAssetAtPath<NerdyStyle>("Assets/Airlift/Fonts/NerdyStyle.asset");
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var settings = n.GetComponent<LoungeSettings>() ?? throw new InvalidOperationException("Run BuildSettingsPanel first.");
        var consent = (RectTransform)n.consentRoot.transform;

        var rundown = n.GetComponent<LoungeRundown>() ?? n.gameObject.AddComponent<LoungeRundown>();
        var card = BuildCard(n, consent, rundown);
        var helper = BuildHelper(n);
        var block = BuildBlock(n);
        var back = n.GetComponent<BackButtonWatcher>() ?? n.gameObject.AddComponent<BackButtonWatcher>();

        rundown.card = card;
        rundown.helper = helper;
        rundown.block = block;
        rundown.blockGrabbable = block.GetComponentInChildren<Grabbable>(true);
        var handle = n.transform.Find("Panel handle");
        rundown.handleGrabbable = handle != null ? handle.GetComponent<Grabbable>() : null;
        rundown.settings = settings;
        n.rundown = rundown; n.helper = helper; n.backButton = back; n.loungeSettings = settings;
        settings.hideWhileOpen = (settings.hideWhileOpen ?? new GameObject[0]).Where(g => g != null && g != card).Append(card).ToArray();

        card.SetActive(false); block.SetActive(false); helper.gameObject.SetActive(false);
        foreach (var o in new UnityEngine.Object[] { n, rundown, settings, back }) EditorUtility.SetDirty(o);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return "Rundown card, controller helper (anchors: " + string.Join(", ", helper.anchors.Select(a => a != null ? a.name : "-")) + "), block and B watcher wired"
             + (rundown.handleGrabbable != null ? "; handle gate wired" : "; NO panel handle found");
    }

    // ---- the card ----------------------------------------------------------------------------------------------
    static GameObject BuildCard(NerdyDirector n, RectTransform consent, LoungeRundown rundown)
    {
        var previous = consent.parent.Find("Rundown card");
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var card = UnityEngine.Object.Instantiate(n.consentRoot, consent.parent);
        card.name = "Rundown card";
        var rect = (RectTransform)card.transform;
        rect.localPosition = consent.localPosition; rect.localRotation = consent.localRotation; rect.localScale = consent.localScale; rect.sizeDelta = consent.sizeDelta;
        foreach (Transform child in rect.Cast<Transform>().ToArray())
        {
            switch (child.name)
            {
                case "Heading": case "Body": case "Stroke": break;
                case "Allow voice": child.name = "Continue"; break;
                case "No voice": child.name = "Skip"; break;
                default: UnityEngine.Object.DestroyImmediate(child.gameObject); break;
            }
        }
        var heading = rect.GetComponentsInChildren<TMP_Text>(true).First(t => t.name == "Heading");
        var body = rect.GetComponentsInChildren<TMP_Text>(true).First(t => t.name == "Body");
        // One evenly spaced block, as the welcome board: eyebrow, heading, body, two pills.
        var eyebrow = UnityEngine.Object.Instantiate(heading.gameObject, rect).GetComponent<TMP_Text>();
        eyebrow.name = "Eyebrow"; eyebrow.font = style.altFont; eyebrow.fontSize = NerdySpace.Label; eyebrow.color = style.lavender; eyebrow.characterSpacing = 10; eyebrow.text = "1 OF 6";
        eyebrow.rectTransform.anchoredPosition = new Vector2(0f, 250f); eyebrow.rectTransform.sizeDelta = new Vector2(900f, 30f);
        eyebrow.enableAutoSizing = false; eyebrow.fontStyle = FontStyles.UpperCase;
        heading.rectTransform.anchoredPosition = new Vector2(0f, 185f); heading.text = "Point and press";
        body.rectTransform.anchoredPosition = new Vector2(0f, 70f); body.rectTransform.sizeDelta = new Vector2(900f, 150f); body.text = RundownScript.Steps[0].Say;
        body.alignment = TextAlignmentOptions.Center;

        var cont = rect.Find("Continue").GetComponent<Button>(); var skip = rect.Find("Skip").GetComponent<Button>();
        Place(cont, new Vector2(-150f, -110f), "Continue"); Place(skip, new Vector2(170f, -110f), "Skip");
        Rewire(cont, rundown, "PressContinue"); Rewire(skip, rundown, "Skip");
        rundown.eyebrow = eyebrow; rundown.heading = heading; rundown.body = body; rundown.continueButton = cont; rundown.skipButton = skip;
        return card;
    }

    static void Place(Button b, Vector2 pos, string text)
    {
        var r = (RectTransform)b.transform; r.anchoredPosition = pos;
        var label = b.GetComponentInChildren<TMP_Text>(true); if (label != null) label.text = text;
    }

    static void Rewire(Button b, UnityEngine.Object target, string method)
    {
        for (int k = b.onClick.GetPersistentEventCount() - 1; k >= 0; k--) UnityEventTools.RemovePersistentListener(b.onClick, k);
        UnityEventTools.AddPersistentListener(b.onClick, (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, method));
    }

    // ---- the helper -------------------------------------------------------------------------------------------
    /// The Meta SDK's Touch Plus model, floating to the right of the board at hand height and turned a little
    /// towards the learner. Its bones give the exact button positions, so nothing is guessed.
    static ControllerHelper BuildHelper(NerdyDirector n)
    {
        var old = n.transform.Find("Controller helper"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var root = new GameObject("Controller helper"); root.transform.SetParent(n.transform, false);
        // Just outside the board's right edge, a little forward of it, turned towards the learner: in view without
        // turning the head, never over the card.
        root.transform.localPosition = new Vector3(NerdySpace.PanelWidthMetres / 2f + 0.16f, -0.12f, -0.35f);
        root.transform.localRotation = Quaternion.Euler(-10f, -30f, 0f);
        var helper = root.AddComponent<ControllerHelper>();

        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(ControllerFbx) ?? throw new InvalidOperationException("Touch Plus model not found at " + ControllerFbx);
        var model = (GameObject)PrefabUtility.InstantiatePrefab(fbx); model.name = "Touch Plus right";
        model.transform.SetParent(root.transform, false);
        model.transform.localPosition = Vector3.zero; model.transform.localRotation = Quaternion.Euler(-60f, 0f, 0f); model.transform.localScale = Vector3.one;
        // The FBX's own phong materials are not URP; the helper is a matte prop in the room's palette. Its size is
        // normalised from its rendered bounds so a stray import scale can never fill the learner's view.
        var bodyMat = Mat("RundownController", Color.Lerp(style.surface, Color.white, 0.22f), 0.75f);
        foreach (var r in model.GetComponentsInChildren<Renderer>(true))
        {
            r.sharedMaterials = Enumerable.Repeat(bodyMat, Mathf.Max(1, r.sharedMaterials.Length)).ToArray();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
        }
        var anim = model.GetComponentInChildren<Animator>(true); if (anim != null) anim.enabled = false;
        NormaliseSize(model.transform, 0.13f);
        helper.model = model.transform;

        var bones = model.GetComponentsInChildren<Transform>(true);
        Transform Bone(params string[] names) => names.Select(name => bones.FirstOrDefault(b => b.name == name)).FirstOrDefault(b => b != null);
        helper.anchors = new Transform[5];
        helper.anchors[(int)ControllerButton.Trigger] = Bone("right_b_trigger_front", "b_trigger_front");
        helper.anchors[(int)ControllerButton.Grip] = Bone("right_b_trigger_grip", "b_trigger_grip");
        helper.anchors[(int)ControllerButton.B] = Bone("b_button_b", "right_b_button_b");
        helper.anchors[(int)ControllerButton.Thumbstick] = Bone("right_b_thumbstick", "b_thumbstick");

        var glowMat = Mat("RundownGlow", style.cyan, 0.4f, emissive: true);
        var arrowMat = Mat("RundownArrow", style.lavender, 0.5f, emissive: true);
        var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere); glow.name = "Button glow";
        UnityEngine.Object.DestroyImmediate(glow.GetComponent<Collider>());
        glow.transform.SetParent(root.transform, false); glow.transform.localScale = Vector3.one * 0.022f;
        Finish(glow, glowMat);
        helper.glow = glow.transform;

        var arrow = new GameObject("Arrow", typeof(MeshFilter), typeof(MeshRenderer)); arrow.transform.SetParent(root.transform, false);
        var cone = AssetDatabase.LoadAssetAtPath<Mesh>(ConeObj);
        if (cone == null) cone = AssetDatabase.LoadAllAssetsAtPath(ConeObj).OfType<Mesh>().FirstOrDefault();
        arrow.GetComponent<MeshFilter>().sharedMesh = cone != null ? cone : RoundedBoxMesh.Build(new Vector3(0.02f, 0.03f, 0.02f), 0.004f);
        arrow.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);   // point down at the button
        Finish(arrow, arrowMat);
        NormaliseSize(arrow.transform, 0.03f);
        helper.arrow = arrow.transform;

        var labelGo = new GameObject("Button label", typeof(RectTransform)); labelGo.transform.SetParent(root.transform, false);
        var label = labelGo.AddComponent<TextMeshPro>();
        label.font = style.displayFont; label.fontSize = 1.6f; label.color = style.text; label.alignment = TextAlignmentOptions.Center; label.text = "Trigger";
        label.rectTransform.sizeDelta = new Vector2(1.2f, 0.3f); label.rectTransform.localScale = Vector3.one * 0.1f;
        label.rectTransform.localPosition = new Vector3(0f, 0.15f, 0f);
        helper.label = label;
        return helper;
    }

    // ---- the block ---------------------------------------------------------------------------------------------
    /// A plain rounded block on a small lit pad, within reach in front of the board. Generic on purpose: never
    /// Cargo's locked practice crate.
    static GameObject BuildBlock(NerdyDirector n)
    {
        var old = n.transform.Find("Rundown block"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var root = new GameObject("Rundown block"); root.transform.SetParent(n.transform, false);
        root.transform.localPosition = new Vector3(0.25f, -0.32f, -0.5f);

        var padMat = Mat("RundownPad", style.cyan, 0.6f, emissive: true);
        var pad = new GameObject("Block pad", typeof(MeshFilter), typeof(MeshRenderer)); pad.transform.SetParent(root.transform, false);
        pad.GetComponent<MeshFilter>().sharedMesh = RoundedBoxMesh.Build(new Vector3(0.16f, 0.006f, 0.16f), 0.003f);
        pad.transform.localPosition = new Vector3(0f, -0.045f, 0f);
        Finish(pad, padMat);

        var blockMat = Mat("RundownBlock", style.amber, 0.7f);
        var block = new GameObject("Block", typeof(MeshFilter), typeof(MeshRenderer)); block.transform.SetParent(root.transform, false);
        block.GetComponent<MeshFilter>().sharedMesh = RoundedBoxMesh.Build(new Vector3(0.08f, 0.08f, 0.08f), 0.014f);
        Finish(block, blockMat);
        var collider = block.AddComponent<BoxCollider>(); collider.size = new Vector3(0.09f, 0.09f, 0.09f);
        QuickActionsAPI.AddGrabInteraction(block);
        var grabbable = block.GetComponentInChildren<Grabbable>();
        if (grabbable == null) throw new InvalidOperationException("SDK did not create Grabbable for the rundown block");
        grabbable.MaxGrabPoints = 1;
        return root;
    }

    /// Scales a prop so its longest rendered side is `metres`, whatever the source file's units were.
    static void NormaliseSize(Transform root, float metres)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;
        var b = renderers[0].bounds; foreach (var r in renderers) b.Encapsulate(r.bounds);
        float longest = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
        if (longest > 1e-5f) root.localScale *= metres / longest;
    }

    // ---- helpers (each AgentScript compiles alone) --------------------------------------------------------------
    static void Finish(GameObject go, Material material)
    {
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = material;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
    }

    static Material Mat(string name, Color colour, float roughness, bool emissive = false)
    {
        string path = MatDir + "/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
        mat.shader = shader; mat.color = colour;
        mat.SetFloat("_Smoothness", Mathf.Clamp01(1f - roughness)); mat.SetFloat("_Metallic", 0f);
        if (emissive) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", colour * 2.2f); }
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
