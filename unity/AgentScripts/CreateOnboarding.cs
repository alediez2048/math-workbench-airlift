using System;
using System.Linq;
using Airlift.Onboarding;
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

public static class CreateOnboarding
{
    const string ScenePath = "Assets/Airlift/Scenes/Onboarding.unity";
    static TMP_FontAsset font;

    public static string Run()
    {
        if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the active scene first.");
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) throw new InvalidOperationException("Onboarding exists; refusing to overwrite.");
        font = TMP_Settings.defaultFontAsset;
        if (font == null) font = AssetDatabase.FindAssets("t:TMP_FontAsset").Select(g => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault(f => f != null);
        if (font == null) throw new InvalidOperationException("Import TextMeshPro Essential Resources before building UI.");
        if (!AssetDatabase.CopyAsset("Assets/Airlift/Scenes/DeviceProof.unity", ScenePath)) throw new InvalidOperationException("Could not copy proof scene.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var oldCube = GameObject.Find("Grab this cube");
        var oldBench = GameObject.Find("Proof workbench - virtual surface");
        if (oldCube == null || oldBench == null) throw new InvalidOperationException("Proof scene changed; inspect before modifying.");
        UnityEngine.Object.DestroyImmediate(oldCube);
        UnityEngine.Object.DestroyImmediate(oldBench);
        var root = new GameObject("Onboarding workbench - world locked");
        root.transform.position = new Vector3(0, 0.8f, 0.65f);
        var director = root.AddComponent<OnboardingDirector>();
        director.head = UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>().centerEyeAnchor;
        if (!AssetDatabase.IsValidFolder("Assets/Airlift/Tasks")) AssetDatabase.CreateFolder("Assets/Airlift", "Tasks");
        director.content = ScriptableObject.CreateInstance<OnboardingContent>();
        AssetDatabase.CreateAsset(director.content, "Assets/Airlift/Tasks/Onboarding.asset");
        var navy = Material("OnboardingNavy", new Color(0.025f, 0.075f, 0.12f));
        var orange = Material("OnboardingOrange", new Color(1f, 0.52f, 0.12f));
        var white = Material("OnboardingWhite", new Color(0.9f, 0.95f, 1f));
        var teal = Material("OnboardingTeal", new Color(0.08f, 0.44f, 0.46f));
        Box("Workbench", root.transform, Vector3.zero, new Vector3(0.8f, 0.035f, 0.5f), navy, true);
        director.station = new GameObject("Measuring station");
        director.station.transform.SetParent(root.transform, false);
        var target = director.content.targetPosition;
        Box("Outlined placement pad", director.station.transform, new Vector3(target.x, 0.024f, target.z), new Vector3(0.36f, 0.008f, 0.13f), white, false);
        Box("Pad inset", director.station.transform, new Vector3(target.x, 0.03f, target.z), new Vector3(0.33f, 0.008f, 0.1f), teal, false);
        director.strap = Box("Cargo strap - practice", root.transform, director.content.trayPosition, new Vector3(0.28f, 0.055f, 0.06f), orange, true).transform;
        QuickActionsAPI.AddGrabInteraction(director.strap.gameObject);
        director.grabbable = director.strap.GetComponentInChildren<Grabbable>();
        if (director.grabbable == null) throw new InvalidOperationException("SDK did not create Grabbable.");
        director.grabbable.MaxGrabPoints = 1;
        director.demonstrationStrap = Box("Striped example - demonstration only", root.transform, director.content.trayPosition, new Vector3(0.28f, 0.055f, 0.06f), orange, false).transform;
        Box("Demo stripe", director.demonstrationStrap, new Vector3(0, 0.51f, 0), new Vector3(0.13f, 0.04f, 1.02f), white, false);

        var ui = new GameObject("Lesson interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        ui.transform.SetParent(root.transform, false);
        ui.transform.localPosition = new Vector3(0, 0.46f, 0.24f);
        ui.transform.localScale = Vector3.one * 0.001f;
        ui.GetComponent<RectTransform>().sizeDelta = new Vector2(920, 470);
        ui.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        ui.GetComponent<Canvas>().worldCamera = director.head.GetComponent<Camera>();
        Panel("Backdrop", ui.transform, Vector2.zero, new Vector2(920, 470), new Color(0.025f, 0.055f, 0.09f, 0.97f));
        director.catalog = Panel("Lesson catalog", ui.transform, Vector2.zero, new Vector2(900, 450), Color.clear);
        Text("Catalog title", director.catalog.transform, "Arithmetic Lessons", new Vector2(0, 174), new Vector2(850, 65), 44);
        Text("Catalog guidance", director.catalog.transform, "Point at Cargo Crew and press the index trigger to begin.", new Vector2(0, 112), new Vector2(840, 48), 25);
        Button("Cargo Crew\nFractions\n\nStart onboarding", director.catalog.transform, new Vector2(-290, -30), new Vector2(270, 220), director.ChooseCargo);
        Button("Neighborhood Café\nDivision\n\nComing soon", director.catalog.transform, new Vector2(0, -30), new Vector2(270, 220), null);
        Button("Community Garden\nMultiplication\n\nComing soon", director.catalog.transform, new Vector2(290, -30), new Vector2(270, 220), null);
        Text("MVP scope", director.catalog.transform, "Onboarding MVP · One guided practice experience available", new Vector2(0, -185), new Vector2(850, 42), 23);
        director.briefing = Panel("Cargo briefing", ui.transform, Vector2.zero, new Vector2(900, 450), Color.clear);
        director.heading = Text("Heading", director.briefing.transform, "Cargo Crew · Fractions", new Vector2(0, 175), new Vector2(850, 65), 40);
        director.body = Text("Instruction", director.briefing.transform, "", new Vector2(0, 14), new Vector2(830, 250), 27);
        director.body.alignment = TextAlignmentOptions.TopLeft;
        director.primary = Button("Begin briefing", director.briefing.transform, new Vector2(-265, -176), new Vector2(250, 60), director.Continue);
        director.primaryLabel = director.primary.GetComponentInChildren<TMP_Text>();
        director.help = Button("Help / replay demo", director.briefing.transform, new Vector2(0, -176), new Vector2(250, 60), director.Help);
        Button("Back to lessons", director.briefing.transform, new Vector2(265, -176), new Vector2(250, 60), director.Back);
        QuickActionsAPI.AddRayCanvasInteraction(ui);
        director.briefing.SetActive(false);
        director.station.SetActive(false);
        director.strap.gameObject.SetActive(false);
        director.demonstrationStrap.gameObject.SetActive(false);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        return "Saved Onboarding scene: 3 cards, cargo briefing, orientation, example animation, practice and honest handoff. DeviceProof unchanged.";
    }

    static GameObject Box(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collider)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name;
        obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        return obj;
    }
    static Material Material(string name, Color color)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, color = color };
        AssetDatabase.CreateAsset(material, "Assets/Airlift/Materials/" + name + ".mat"); return material;
    }
    static GameObject Panel(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.sizeDelta = size; rect.anchoredPosition = position;
        var img = obj.GetComponent<Image>(); img.color = color; img.raycastTarget = false; return obj;
    }
    static TMP_Text Text(string name, Transform parent, string text, Vector2 position, Vector2 size, float fontSize)
    {
        var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
        var label = obj.AddComponent<TextMeshProUGUI>(); label.font = font; label.text = text; label.fontSize = fontSize;
        label.color = Color.white; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
        label.rectTransform.sizeDelta = size; label.rectTransform.anchoredPosition = position; return label;
    }
    static Button Button(string text, Transform parent, Vector2 position, Vector2 size, UnityAction action)
    {
        var obj = Panel(text, parent, position, size, action == null ? new Color(0.12f, 0.15f, 0.19f) : new Color(0.04f, 0.29f, 0.34f));
        obj.GetComponent<Image>().raycastTarget = true;
        var button = obj.AddComponent<Button>(); button.targetGraphic = obj.GetComponent<Image>(); button.interactable = action != null;
        if (action != null) UnityEventTools.AddPersistentListener(button.onClick, action);
        Text("Label", obj.transform, text, Vector2.zero, size - new Vector2(16, 10), size.y > 100 ? 27 : 23);
        return button;
    }
}
