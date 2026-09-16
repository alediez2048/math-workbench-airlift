using System;
using Airlift.Onboarding;
using Airlift.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildCargoTerminal
{
    static Material navy, teal, orange, cream, rubber;
    public static string Run()
    {
        for (int i=0;i<SceneManager.sceneCount;i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity");
        var director = UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        if (director == null) throw new InvalidOperationException("Baseline director missing.");
        if (director.transform.Find("Cargo terminal") != null) throw new InvalidOperationException("Terminal exists; do not overwrite owner edits.");
        navy = Mat("CargoNavy", new Color32(37,52,77,255));
        teal = Mat("CargoTeal", new Color32(74,215,188,255));
        orange = Mat("CargoOrange", new Color32(255,184,107,255));
        cream = Mat("CargoCream", new Color32(245,247,251,255));
        rubber = Mat("CargoRubber", new Color32(24,35,59,255));
        var root = Group("Cargo terminal", director.transform, Vector3.zero);
        var view = root.gameObject.AddComponent<CargoTerminalView>();
        var deck = director.transform.Find("Workbench");
        deck.localScale = new Vector3(1.3f,0.035f,0.8f);
        deck.GetComponent<Renderer>().sharedMaterial = navy;
        view.measuringPlatform = director.station.transform;
        view.containers = new[] {Container(root,"Container A",new Vector3(-0.43f,0.035f,0.24f),teal),
            Container(root,"Container B",new Vector3(0.43f,0.035f,0.24f),orange)};
        view.staging = Group("Parcel staging",root,new Vector3(0.49f,0.025f,-0.09f));
        Box("Staging pad",view.staging,Vector3.zero,new Vector3(0.22f,0.012f,0.26f),cream);
        for(int i=0;i<2;i++)
        {
            var p = Group("Parcel "+(i+1),view.staging,new Vector3(0,0.047f,-0.055f+i*0.11f));
            Box("Carton",p,Vector3.zero,new Vector3(0.115f,0.075f,0.085f),orange);
            Box("Tape",p,new Vector3(0,0.039f,0),new Vector3(0.023f,0.003f,0.085f),cream);
        }
        view.truck = Group("Delivery truck",root,new Vector3(-0.49f,0.055f,-0.07f));
        Box("Chassis",view.truck,Vector3.zero,new Vector3(0.16f,0.025f,0.27f),rubber);
        Box("Cab",view.truck,new Vector3(0,0.045f,-0.09f),new Vector3(0.15f,0.085f,0.09f),teal);
        Box("Windscreen",view.truck,new Vector3(0,0.058f,-0.137f),new Vector3(0.115f,0.035f,0.004f),navy);
        Box("Cargo box",view.truck,new Vector3(0,0.06f,0.055f),new Vector3(0.155f,0.115f,0.18f),cream);
        foreach(float x in new[]{-0.085f,0.085f}) foreach(float z in new[]{-0.085f,0.085f})
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder); wheel.name="Wheel";
            wheel.transform.SetParent(view.truck,false); wheel.transform.localPosition=new Vector3(x,-0.015f,z);
            wheel.transform.localRotation=Quaternion.Euler(0,0,90); wheel.transform.localScale=new Vector3(0.055f,0.013f,0.055f);
            wheel.GetComponent<Renderer>().sharedMaterial=rubber; UnityEngine.Object.DestroyImmediate(wheel.GetComponent<Collider>());
        }
        view.aircraft=Group("Parked aircraft",root,new Vector3(0,0.065f,0.32f));
        var fuselage=GameObject.CreatePrimitive(PrimitiveType.Sphere); fuselage.name="Fuselage";
        fuselage.transform.SetParent(view.aircraft,false); fuselage.transform.localScale=new Vector3(0.065f,0.065f,0.24f);
        fuselage.GetComponent<Renderer>().sharedMaterial=cream; UnityEngine.Object.DestroyImmediate(fuselage.GetComponent<Collider>());
        Box("Wings",view.aircraft,Vector3.zero,new Vector3(0.3f,0.012f,0.065f),teal);
        Box("Tailplane",view.aircraft,new Vector3(0,0.018f,0.085f),new Vector3(0.12f,0.009f,0.035f),teal);
        Box("Tail fin",view.aircraft,new Vector3(0,0.035f,0.09f),new Vector3(0.009f,0.065f,0.045f),orange);
        var font=director.heading.font; // Explicit existing font for geometry review only; font decision still pending.
        Tag(root,"ARRIVALS",new Vector3(-0.49f,0.028f,-0.25f),font);
        Tag(root,"STAGING",new Vector3(0.49f,0.028f,-0.25f),font);
        Tag(root,"AIR DELIVERY",new Vector3(0,0.028f,0.19f),font);
        var placement=director.gameObject.AddComponent<ComfortPlacement>();
        placement.stationRoot=director.transform; placement.head=director.head; placement.grabbable=director.grabbable;
        placement.distance=director.content.boardDistance; placement.belowEyes=director.content.boardBelowEyes;
        var ui=director.transform.Find("Lesson interface");
        Control(ui,"Lower station",new Vector2(-275,-285),font,placement.Lower);
        Control(ui,"Recenter",new Vector2(0,-285),font,placement.Recenter);
        Control(ui,"Raise station",new Vector2(275,-285),font,placement.Raise);
        // Components are generated once in the editor, not every frame on the device.
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return "Cargo terminal saved with original static props and three station controls. Font selection and device acceptance pending.";
    }
    static Transform Group(string name,Transform parent,Vector3 p) {var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=p;return g.transform;}
    static GameObject Box(string name,Transform parent,Vector3 p,Vector3 scale,Material m)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;
        g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static Transform Container(Transform root,string name,Vector3 p,Material material)
    {
        var g=Group(name,root,p);Box("Shell",g,new Vector3(0,0.055f,0),new Vector3(0.28f,0.11f,0.16f),material);
        for(int i=0;i<7;i++) Box("Rib",g,new Vector3(-0.12f+i*0.04f,0.055f,-0.082f),new Vector3(0.006f,0.10f,0.006f),cream);
        return g;
    }
    static Material Mat(string name,Color color)
    {
        var path="Assets/Airlift/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m!=null)return m;m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};m.SetFloat("_Smoothness",0.1f);AssetDatabase.CreateAsset(m,path);return m;
    }
    static void Tag(Transform parent,string text,Vector3 p,TMP_FontAsset font)
    {
        var g=Group(text+" tag",parent,p);g.localRotation=Quaternion.Euler(60,0,0);
        var label=g.gameObject.AddComponent<TextMeshPro>();label.font=font;label.text=text;label.fontSize=0.14f;label.color=Color.white;
        label.textWrappingMode=TextWrappingModes.NoWrap;
        label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=new Vector2(0.24f,0.035f);
    }
    static void Control(Transform parent,string text,Vector2 p,TMP_FontAsset font,UnityAction action)
    {
        var g=new GameObject(text,typeof(RectTransform),typeof(Image),typeof(Button));g.transform.SetParent(parent,false);
        var r=g.GetComponent<RectTransform>();r.anchoredPosition=p;r.sizeDelta=new Vector2(250,55);
        g.GetComponent<Image>().color=new Color32(74,215,188,255);var b=g.GetComponent<Button>();b.targetGraphic=g.GetComponent<Image>();UnityEventTools.AddPersistentListener(b.onClick,action);
        var child=new GameObject("Label",typeof(RectTransform));child.transform.SetParent(g.transform,false);var t=child.AddComponent<TextMeshProUGUI>();
        t.font=font;t.text=text;t.fontSize=24;t.alignment=TextAlignmentOptions.Center;t.color=new Color32(24,35,59,255);t.raycastTarget=false;t.rectTransform.sizeDelta=new Vector2(240,50);
    }
}
