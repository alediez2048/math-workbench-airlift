using System.Linq;
using Airlift.Onboarding;
using Airlift.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Airlift.Tests
{
    public class CargoTerminalLayoutTests
    {
        Scene loaded;
        T Find<T>() where T:Component => SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity")
            .GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).FirstOrDefault();
        [SetUp] public void Open()
        {
            if(!SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").isLoaded)
                loaded=EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity",OpenSceneMode.Additive);
        }
        [TearDown] public void Close(){if(loaded.IsValid())EditorSceneManager.CloseScene(loaded,true);}
        [Test] public void TerminalPresentAndCoherent()
        {
            var terminal=Find<CargoTerminalView>();Assert.That(terminal,Is.Not.Null);
            Assert.That(terminal.HasRequiredProps,Is.True);
            Assert.That(terminal.GetComponentsInChildren<Collider>().Length,Is.Zero,"Decorative cargo cannot block controller rays.");
            var placement=terminal.GetComponentInParent<ComfortPlacement>();Assert.That(placement,Is.Not.Null);
            Assert.That(terminal.transform.IsChildOf(placement.stationRoot),Is.True);
            Assert.That(terminal.measuringPlatform.IsChildOf(placement.stationRoot),Is.True);
        }
        [Test] public void StationarySceneHasNoActiveGravityLocomotor()
        {
            var active=Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
                .Where(b=>b!=null && b.gameObject.scene.path=="Assets/Airlift/Scenes/CargoCrew.unity" && b.GetType().Name=="FirstPersonLocomotor" && b.enabled && b.gameObject.activeInHierarchy);
            Assert.That(active,Is.Empty,"Stationary MR must not simulate player falling.");
        }
        [Test] public void OnlyCargoEnabled()
        {
            var d=Find<OnboardingDirector>();var buttons=d.catalog.GetComponentsInChildren<Button>(true);
            Assert.That(buttons.Length,Is.EqualTo(3));Assert.That(buttons.Count(b=>b.interactable),Is.EqualTo(1));
        }
        [Test] public void CaptionsFitAndFontsBound()
        {
            var d=Find<OnboardingDirector>();var b=d.GetComponent<TypographyBindings>();
            Assert.That(b,Is.Not.Null);
            foreach(var t in d.GetComponentsInChildren<TMP_Text>(true))Assert.That(t.font==b.style.headingFont||t.font==b.style.bodyFont,Is.True,t.name);
            foreach(var text in new[]{d.content.overview,d.content.orientation,d.content.demonstration,d.content.practice,d.content.retry,d.content.ready})
                Assert.That(d.body.GetPreferredValues(text,d.body.rectTransform.rect.width,1000).y,Is.LessThanOrEqualTo(d.body.rectTransform.rect.height));
        }
    }
}
