using System.Linq;
using Airlift.Welcome;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Airlift.Tests
{
    public class PrivacyGateTests
    {
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").isLoaded) loaded = EditorSceneManager.OpenScene("Assets/Airlift/Scenes/CargoCrew.unity", OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        [Test] public void AdultTesterGateAndConsentCopyAreInTheBuild()
        {
            var n = SceneManager.GetSceneByPath("Assets/Airlift/Scenes/CargoCrew.unity").GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            Assert.That(n.adultTesterOnly, Is.True, "adult testers only until CC-P0-08 records a child-use go");
            var copy = string.Join(" ", n.consentRoot.GetComponentsInChildren<TMP_Text>(true).Select(t => t.text));
            Assert.That(copy, Does.Contain("adult testers"));
            Assert.That(copy, Does.Contain("OpenAI"));
            Assert.That(copy, Does.Contain("not stored"));
        }

        [Test] public void ProfileNeverStoresNamesOrFreeText()
        {
            var p = new LearnerProfile();
            p.Merge("{\"name\":\"Sam\",\"email\":\"s@x.y\",\"ageBand\":\"adult\",\"interests\":[\"my address is 1 Main St\"]}");
            string json = p.ToJson();
            Assert.That(json, Does.Not.Contain("Sam")); Assert.That(json, Does.Not.Contain("s@x.y"));
            Assert.That(p.interests.Count, Is.EqualTo(1), "free-form interest is kept only as a short tag");
            Assert.That(p.interests[0].Length, Is.LessThanOrEqualTo(LearnerProfile.MaxInterestLength));
        }
    }
}
