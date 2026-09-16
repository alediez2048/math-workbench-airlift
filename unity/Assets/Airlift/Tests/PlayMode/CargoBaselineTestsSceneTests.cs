using System.Collections;
using System.Linq;
using Airlift.Onboarding;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Airlift.Tests
{
    public class CargoBaselineTestsSceneTests
    {
        [UnityTest] public IEnumerator SingleRigAndCatalog()
        {
            #if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Airlift/Scenes/CargoCrew.unity", new LoadSceneParameters(LoadSceneMode.Single));
            #else
            yield return SceneManager.LoadSceneAsync("CargoCrew");
            #endif
            yield return null;
            Assert.That(Object.FindObjectsByType<OVRCameraRig>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            var directors = Object.FindObjectsByType<OnboardingDirector>(FindObjectsSortMode.None);
            Assert.That(directors.Length, Is.EqualTo(1));
            var director = directors[0];
            var buttons = director.catalog.GetComponentsInChildren<Button>(true);
            Assert.That(buttons.Length, Is.EqualTo(3));
            Assert.That(buttons.Count(b => b.interactable), Is.EqualTo(1));
            director.ChooseCargo();
            Assert.That(director.briefing.activeSelf, Is.True);
            director.Back();
            Assert.That(director.catalog.activeSelf, Is.True);
        }
    }
}
