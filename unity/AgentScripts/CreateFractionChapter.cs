using System;
using System.Linq;
using Airlift.Lessons;
using Airlift.Onboarding;
using Airlift.Presentation;
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

// Builds the whole-and-halves chapter objects into CargoCrew once. Reference scenes untouched.
public static class CreateFractionChapter
{
    static Material orange, cream, teal, navy;
    static AirliftStyle style;

    public static string Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Single);
        var d = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        if (d == null) throw new InvalidOperationException("CargoCrew director missing.");
        if (d.transform.Find("Fraction chapter") != null) throw new InvalidOperationException("Fraction chapter exists; refusing to overwrite.");
        style = AssetDatabase.LoadAssetAtPath<AirliftStyle>("Assets/Airlift/Fonts/AirliftStyle.asset");
        if (style == null || style.bodyFont == null) throw new InvalidOperationException("Airlift style/fonts missing.");
        orange = Mat("CargoOrange"); cream = Mat("CargoCream"); teal = Mat("CargoTeal"); navy = Mat("CargoNavy");

        var lesson = d.gameObject.AddComponent<CargoLessonDirector>();
        lesson.stationRoot = d.transform; lesson.heading = d.heading; lesson.body = d.body;
        var chapter = Group("Fraction chapter", d.transform, Vector3.zero);
        lesson.chapterObjects = chapter.gameObject;

        // Ruler: the visible whole from 0 to 1 on the measuring pad.
        var target = d.content.targetPosition;
        var rulerCenter = new Vector3(target.x, 0.031f, target.z);
        var ruler = Group("Ruler 0 to 1", chapter, rulerCenter);
        lesson.ruler = ruler;
        Box("Ruler bar", ruler, Vector3.zero, new Vector3(RulerLayout.WholeLength, 0.004f, 0.012f), cream, false);
        float half = RulerLayout.WholeLength * 0.5f;
        foreach (var (x, text) in new[] { (-half, "0"), (0f, "1/2"), (half, "1") })
        {
            Box("Tick " + text, ruler, new Vector3(x, 0.002f, 0.045f), new Vector3(0.004f, 0.006f, 0.05f), cream, false);
            WorldText("Mark " + text, ruler, new Vector3(x, 0.004f, 0.085f), text, 0.09f, Quaternion.Euler(60, 0, 0));
        }

        // Pieces: exact prebuilt whole and halves. Parent objects stay unscaled so labels keep their shape.
        lesson.whole = Piece("Whole strap", chapter, new Vector3(-0.13f, 0.08f, -0.08f), 8, "whole");
        lesson.halfA = Piece("Half strap A", chapter, new Vector3(-0.21f, 0.08f, -0.08f), 4, "half-1");
        lesson.halfB = Piece("Half strap B", chapter, new Vector3(-0.05f, 0.08f, -0.08f), 4, "half-2");

        // Controls on the existing lesson canvas.
        var ui = d.transform.Find("Lesson interface");
        var buttons = new GameObject("Fraction controls", typeof(RectTransform)); buttons.transform.SetParent(ui, false);
        buttons.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        lesson.chapterButtons = buttons;
        lesson.splitButton = Control(buttons.transform, "Split into halves", new Vector2(-265, -176), new Vector2(250, 60), lesson.Split);
        lesson.submitButton = Control(buttons.transform, "Submit", new Vector2(0, -176), new Vector2(250, 60), lesson.Submit);
        lesson.resetButton = Control(buttons.transform, "Reset pieces", new Vector2(-265, -228), new Vector2(250, 50), lesson.ResetPieces);

        // Flow wiring: Start fractions after practice; Back to lessons also exits the chapter.
        UnityEventTools.AddPersistentListener(d.whenReadyContinue, lesson.Begin);
        var back = ui.GetComponentsInChildren<Button>(true).First(b => b.name == "Back to lessons");
        UnityEventTools.AddPersistentListener(back.onClick, lesson.Exit);
        lesson.hideWhileActive = new[] { d.primary.gameObject, d.help.gameObject, d.strap.gameObject, d.demonstrationStrap.gameObject };

        d.content.ready = "You moved the strap to the measuring station.\n\nIt will represent one whole in the fraction activity.\n\nChoose Start fractions, replay the demo, or return to lessons.";
        EditorUtility.SetDirty(d.content);

        var binding = d.GetComponent<TypographyBindings>();
        binding.bodies = d.GetComponentsInChildren<TMP_Text>(true).Except(binding.headings).ToArray();
        binding.Apply();
        chapter.gameObject.SetActive(false); buttons.SetActive(false);
        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        return "Fraction chapter saved: ruler 0-1, whole labeled 1, two halves labeled 1/2, Split/Submit/Reset, Start fractions hook.";
    }

    static CargoLessonDirector.PieceView Piece(string name, Transform parent, Vector3 tray, int cells, string id)
    {
        float length = RulerLayout.PieceLength(cells);
        var root = new GameObject(name); root.transform.SetParent(parent, false); root.transform.localPosition = tray;
        var collider = root.AddComponent<BoxCollider>(); collider.size = new Vector3(length, 0.055f, 0.06f);
        Box("Body", root.transform, Vector3.zero, new Vector3(length, 0.055f, 0.06f), orange, false);
        if (cells < 8) Box("Cut edge", root.transform, new Vector3(cells == 4 ? 0 : 0, 0, 0), new Vector3(0.003f, 0.057f, 0.062f), cream, false).SetActive(false);
        QuickActionsAPI.AddGrabInteraction(root);
        var grabbable = root.GetComponentInChildren<Grabbable>();
        if (grabbable == null) throw new InvalidOperationException("SDK did not create Grabbable for " + name);
        grabbable.MaxGrabPoints = 1;
        // Stacked notation on the top face, readable from the front.
        var label = Group("Label", root.transform, new Vector3(0, 0.029f, 0)); label.localRotation = Quaternion.Euler(90, 0, 0);
        var view = label.gameObject.AddComponent<FractionNotationView>();
        view.numerator = WorldText("Numerator", label, new Vector3(0, 0.017f, 0), "1", 0.16f, Quaternion.identity);
        view.denominator = WorldText("Denominator", label, new Vector3(0, -0.017f, 0), "2", 0.16f, Quaternion.identity);
        view.fractionBar = Box("Fraction bar", label, new Vector3(0, 0, 0), new Vector3(0.022f, 0.0025f, 0.001f), navy, false);
        view.fractionBar.transform.localRotation = Quaternion.identity;
        return new CargoLessonDirector.PieceView { id = id, piece = root.transform, grabbable = grabbable, label = view, trayPosition = tray };
    }

    static Transform Group(string name, Transform parent, Vector3 p) { var g = new GameObject(name); g.transform.SetParent(parent, false); g.transform.localPosition = p; return g.transform; }
    static GameObject Box(string name, Transform parent, Vector3 p, Vector3 scale, Material m, bool collider)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name; g.transform.SetParent(parent, false);
        g.transform.localPosition = p; g.transform.localScale = scale; g.GetComponent<Renderer>().sharedMaterial = m;
        if (!collider) UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }
    static Material Mat(string name)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/" + name + ".mat");
        if (m == null) throw new InvalidOperationException("Material missing: " + name);
        return m;
    }
    static TMP_Text WorldText(string name, Transform parent, Vector3 p, string text, float size, Quaternion rotation)
    {
        var g = Group(name, parent, p); g.localRotation = rotation;
        var label = g.gameObject.AddComponent<TextMeshPro>(); label.font = style.bodyFont; label.text = text; label.fontSize = size;
        label.color = new Color32(24, 35, 59, 255); label.alignment = TextAlignmentOptions.Center; label.textWrappingMode = TextWrappingModes.NoWrap;
        label.rectTransform.sizeDelta = new Vector2(0.12f, 0.03f);
        return label;
    }
    static Button Control(Transform parent, string text, Vector2 p, Vector2 size, UnityAction action)
    {
        var g = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button)); g.transform.SetParent(parent, false);
        var r = g.GetComponent<RectTransform>(); r.anchoredPosition = p; r.sizeDelta = size;
        g.GetComponent<Image>().color = style.primary;
        var b = g.GetComponent<Button>(); b.targetGraphic = g.GetComponent<Image>();
        var colors = b.colors; colors.normalColor = Color.white; colors.highlightedColor = new Color(0.83f, 1, 0.96f); colors.pressedColor = new Color(0.6f, 0.85f, 0.78f); b.colors = colors;
        UnityEventTools.AddPersistentListener(b.onClick, action);
        var outline = g.AddComponent<Outline>(); outline.effectColor = style.text; outline.effectDistance = new Vector2(3, -3); outline.enabled = false;
        g.AddComponent<FocusPointer>();
        var child = new GameObject("Label", typeof(RectTransform)); child.transform.SetParent(g.transform, false);
        var t = child.AddComponent<TextMeshProUGUI>(); t.font = style.bodyFont; t.text = text; t.fontSize = 23; t.alignment = TextAlignmentOptions.Center;
        t.color = style.panel; t.raycastTarget = false; t.rectTransform.sizeDelta = size - new Vector2(16, 10);
        return b;
    }
}
