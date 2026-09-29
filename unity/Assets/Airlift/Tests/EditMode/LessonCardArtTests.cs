using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Airlift.Tests
{
    /// CC-HF-02. Each lesson card shows its place (dock, café, garden; Higgsfield art) behind a scrim in the card's own
    /// colour, clipped to the rounded backdrop, so text stays as readable as before. Run AgentScripts/ApplyCardArt.cs.
    public class LessonCardArtTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        static readonly (string card, string art)[] Cards = { ("Lesson interface", "dock"), ("Cafe card", "cafe"), ("Garden card", "garden") };

        static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform c in t) { var hit = FindDeep(c, name); if (hit != null) return hit; }
            return null;
        }

        [Test] public void LessonCardsShowTheirPlaceBehindAReadableScrim()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
            try
            {
                var root = scene.GetRootGameObjects().First(g => g.name == "Onboarding workbench - world locked").transform;
                foreach (var (cardName, art) in Cards)
                {
                    var card = FindDeep(root, cardName);
                    Assert.That(card, Is.Not.Null, cardName);
                    var backdrop = card.Find("Backdrop");
                    Assert.That(backdrop.GetComponent<UnityEngine.UI.Mask>(), Is.Not.Null, cardName + " clips the art to its rounded shape: run AgentScripts/ApplyCardArt.cs");
                    var artImg = backdrop.Find("Art")?.GetComponent<UnityEngine.UI.Image>();
                    Assert.That(artImg, Is.Not.Null, cardName + ": run AgentScripts/ApplyCardArt.cs");
                    Assert.That(AssetDatabase.GetAssetPath(artImg.sprite), Is.EqualTo("Assets/Airlift/Art/Generated/Card/Card-" + art + ".png"));
                    var scrim = backdrop.Find("Scrim")?.GetComponent<UnityEngine.UI.Image>();
                    Assert.That(scrim, Is.Not.Null, cardName);
                    Assert.That(scrim.color.a, Is.GreaterThanOrEqualTo(0.55f), cardName + ": text must stay as readable as before");
                    Assert.That(scrim.transform.GetSiblingIndex(), Is.GreaterThan(artImg.transform.GetSiblingIndex()), "scrim above art");
                    Assert.That(artImg.raycastTarget || scrim.raycastTarget, Is.False, "art never eats a press");
                }
            }
            finally { UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
