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
        [Test] public void StationControlsRemoved()
        {
            var d=Find<OnboardingDirector>();
            var names=d.GetComponentsInChildren<Button>(true).Select(b=>b.name).ToArray();
            Assert.That(names.Intersect(new[]{"Raise station","Lower station","Recenter"}),Is.Empty);
            Assert.That(d.content.orientation,Does.Not.Contain("Adjust the station"));
        }
        [Test] public void MeasuringPadCenteredOnBoard()
        {
            var d=Find<OnboardingDirector>();var lesson=d.GetComponent<Airlift.Lessons.CargoLessonDirector>();
            Assert.That(d.content.targetPosition.x,Is.EqualTo(0f).Within(1e-4f));
            Assert.That(Mathf.Abs(d.content.targetPosition.z),Is.LessThanOrEqualTo(0.05f));
            Assert.That(lesson.ruler.localPosition.x,Is.EqualTo(0f).Within(1e-4f));
            Assert.That(lesson.ruler.localPosition.z,Is.EqualTo(d.content.targetPosition.z).Within(1e-4f));
            // Tray pieces start clear of the pad so the release check cannot fire from the tray.
            foreach(var view in new[]{lesson.whole,lesson.halfA,lesson.halfB})
                Assert.That(Vector3.Distance(view.trayPosition,d.content.targetPosition),Is.GreaterThan(d.content.placementRadius),view.id);
            Assert.That(Vector3.Distance(d.content.trayPosition,d.content.targetPosition),Is.GreaterThan(d.content.placementRadius));
        }
        [Test] public void BoardObjectsAreRoundedNotRawCubes()
        {
            var d=Find<OnboardingDirector>();
            var raw=d.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.sharedMesh==null||f.sharedMesh.name=="Cube").Select(f=>f.name).ToArray();
            Assert.That(raw,Is.Empty,"raw primitive cubes remain: "+string.Join(", ",raw));
        }
        [Test] public void TableHandleWiredToStationRoot()
        {
            var d=Find<OnboardingDirector>();var handle=d.GetComponent<TableHandle>();
            Assert.That(handle,Is.Not.Null,"TableHandle component");
            Assert.That(handle.stationRoot,Is.EqualTo(d.transform));
            Assert.That(handle.grabbable,Is.Not.Null);Assert.That(handle.grabbable.Transform,Is.EqualTo(d.transform),"handle moves the whole table");
            Assert.That(handle.grabbable.MaxGrabPoints,Is.EqualTo(2),"two hands resize");
            var bar=handle.grabbable.transform;
            Assert.That(bar.GetComponent<TableCarryTransformer>(),Is.Not.Null,"one-hand carry rule");Assert.That(bar.GetComponent<TableCarryTransformer>().head,Is.EqualTo(d.head));Assert.That(bar.GetComponent<Oculus.Interaction.OneGrabFreeTransformer>(),Is.Null,"wrist-driven transformer removed");
            Assert.That(bar.GetComponent<Oculus.Interaction.TwoGrabPlaneTransformer>(),Is.Not.Null);
            Assert.That(bar.GetComponent<Collider>(),Is.Not.Null);
            Assert.That(handle.handleInteractable,Is.Not.Null);Assert.That(handle.handleInteractable.transform.IsChildOf(bar),Is.True);
            Assert.That(handle.pieceInteractables.Length,Is.GreaterThanOrEqualTo(4),"practice strap + whole + two halves");
            Assert.That(handle.pieceInteractables,Has.None.EqualTo(handle.handleInteractable));
            Assert.That(handle.placement,Is.Not.Null);Assert.That(handle.lesson,Is.Not.Null);
            Assert.That(d.content.orientation.ToLower(),Does.Contain("handle"));
        }
        [Test] public void ExactlyOneActiveGrabInteractorPerHand()
        {
            var active=Object.FindObjectsByType<Oculus.Interaction.GrabInteractor>(FindObjectsInactive.Include)
                .Where(g=>g.gameObject.scene.path=="Assets/Airlift/Scenes/CargoCrew.unity" && g.gameObject.activeInHierarchy).ToArray();
            Assert.That(active.Length,Is.EqualTo(2),"one squeeze must yield one grab point: "+string.Join(", ",active.Select(a=>a.transform.parent.parent.name+"/"+a.name)));
        }
    }
}
