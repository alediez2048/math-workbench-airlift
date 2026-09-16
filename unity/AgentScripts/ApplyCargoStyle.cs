using System;
using System.Linq;
using Airlift.Onboarding;
using Airlift.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ApplyCargoStyle
{
    const string Glyphs="0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz .,;:!?'-/()[]+−=<>×÷·é—–“”’";
    public static string Run()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.path!="Assets/Airlift/Scenes/CargoCrew.unity" || scene.isDirty)throw new InvalidOperationException("Open clean CargoCrew first.");
        var d=UnityEngine.Object.FindAnyObjectByType<OnboardingDirector>();
        const string contentPath="Assets/Airlift/Tasks/CargoOnboarding.asset";
        var content=AssetDatabase.LoadAssetAtPath<OnboardingContent>(contentPath);
        if(content==null){content=UnityEngine.Object.Instantiate(d.content);AssetDatabase.CreateAsset(content,contentPath);}
        content.overview="Join the Cargo Crew. Help prepare parcels for an aircraft delivery.\n\nFirst, practise moving a strap to the measuring pad. Fraction activities come next; this build is onboarding only.";
        content.orientation="The orange piece is a strap. The outlined pad is the measuring station.\n\nWatch the demo, then try it yourself. Adjust the station below if needed. Never lean on the virtual surface.";
        content.demonstration="Watch the example strap move from the tray to the pad.\n\nNext, you will try the same movement. This is controller practice, not a fraction question.";
        content.practice="Touch the orange strap with a controller.\n\nHold the grip under your middle finger. Move the strap above the pad, then release.\n\nNeed help? Replay the demo.";
        content.retry="Try again: hold the grip while moving, then release above the pad. You can replay the demo.";
        content.ready="You moved the strap to the measuring station.\n\nIt will represent one whole in the fraction activities. Those activities are not in this build yet.\n\nReplay, or return to lessons.";
        d.content=content;EditorUtility.SetDirty(content);
        var stylePath="Assets/Airlift/Fonts/AirliftStyle.asset";
        var style=AssetDatabase.LoadAssetAtPath<AirliftStyle>(stylePath);
        if(style==null){style=ScriptableObject.CreateInstance<AirliftStyle>();AssetDatabase.CreateAsset(style,stylePath);}
        style.headingFont=Font("Nunito-Bold");style.bodyFont=Font("NunitoSans-Semibold");
        var binding=d.GetComponent<TypographyBindings>()??d.gameObject.AddComponent<TypographyBindings>();binding.style=style;
        var labels=d.GetComponentsInChildren<TMP_Text>(true);
        binding.headings=labels.Where(t=>t.name=="Catalog title"||t.name=="Heading").ToArray();
        binding.bodies=labels.Except(binding.headings).ToArray();binding.Apply();
        foreach(var label in labels){label.fontStyle=FontStyles.Normal;label.color=style.text;}
        foreach(var button in d.GetComponentsInChildren<Button>(true))
        {
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(0.83f,1,0.96f);colors.pressedColor=new Color(0.6f,0.85f,0.78f);colors.disabledColor=Color.white;button.colors=colors;
            var fill=button.GetComponent<Image>();if(fill!=null)fill.color=button.interactable?style.primary:style.panel;
            foreach(var label in button.GetComponentsInChildren<TMP_Text>())label.color=button.interactable?style.panel:style.muted;
            var outline=button.GetComponent<Outline>()??button.gameObject.AddComponent<Outline>();outline.effectColor=style.text;outline.effectDistance=new Vector2(3,-3);outline.enabled=false;
            if(button.interactable && button.GetComponent<FocusPointer>()==null)button.gameObject.AddComponent<FocusPointer>();
        }
        var ui=d.transform.Find("Lesson interface");
        var placement=d.GetComponent<ComfortPlacement>();
        if(placement.statusText==null)
        {
            var status=new GameObject("Placement guidance",typeof(RectTransform));status.transform.SetParent(ui,false);
            var label=status.AddComponent<TextMeshProUGUI>();label.font=style.bodyFont;label.fontSize=21;label.color=style.text;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;
            label.text="Virtual surface — do not lean on it.";label.rectTransform.sizeDelta=new Vector2(900,45);label.rectTransform.anchoredPosition=new Vector2(0,-335);placement.statusText=label;
        }
        // Keep the new label in the explicit binding inventory.
        binding.bodies=d.GetComponentsInChildren<TMP_Text>(true).Except(binding.headings).ToArray();binding.Apply();
        EditorUtility.SetDirty(style);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "Static Nunito/Nunito Sans SDF fonts applied explicitly; button focus and placement feedback added.";
    }
    static TMP_FontAsset Font(string name)
    {
        string path="Assets/Airlift/Fonts/"+name+" SDF.asset";
        var existing=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);if(existing!=null)return existing;
        var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/Airlift/Fonts/"+name+".ttf");
        if(source==null)throw new InvalidOperationException("Font source not imported: "+name);
        var font=TMP_FontAsset.CreateFontAsset(source);font.name=name+" SDF";
        if(!font.TryAddCharacters(Glyphs,out string missing))throw new InvalidOperationException("Missing required glyphs: "+missing);
        font.atlasPopulationMode=AtlasPopulationMode.Static;
        font.fallbackFontAssetTable=new System.Collections.Generic.List<TMP_FontAsset>();
        AssetDatabase.CreateAsset(font,path);
        AssetDatabase.AddObjectToAsset(font.material,font);
        foreach(var atlas in font.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,font);
        EditorUtility.SetDirty(font);return font;
    }
}
