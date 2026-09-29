using System.Linq;
using Airlift.Presentation;
using Airlift.Welcome;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Airlift.Tests
{
    /// CC-HF-02. Dee is a character beside the assistant bar, not a dot: its pose follows the voice state it reads
    /// (never writes), and it floats gently. Run AgentScripts/ApplyDeeAvatar.cs after CompactAssistantBar.
    public class DeeAvatarTests
    {
        [Test] public void SpeakingWinsOverListeningWhichWinsOverIdle()
        {
            Assert.That(DeeAvatar.PoseFor(speaking: false, listening: false), Is.EqualTo(DeePose.Idle));
            Assert.That(DeeAvatar.PoseFor(speaking: false, listening: true), Is.EqualTo(DeePose.Listening));
            Assert.That(DeeAvatar.PoseFor(speaking: true, listening: true), Is.EqualTo(DeePose.Speaking));
            Assert.That(DeeAvatar.PoseFor(speaking: true, listening: false), Is.EqualTo(DeePose.Speaking));
        }

        [Test] public void TheFloatStaysSmallAndSmooth()
        {
            for (float t = 0f; t < 10f; t += 0.037f)
            {
                Assert.That(Mathf.Abs(DeeAvatar.Bob(t, 6f)), Is.LessThanOrEqualTo(6f));
                Assert.That(Mathf.Abs(DeeAvatar.Bob(t + 0.02f, 6f) - DeeAvatar.Bob(t, 6f)), Is.LessThan(0.5f), "no jumps");
            }
        }

        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        [Test] public void DeeStandsBesideTheBarWithAllThreePosesAndNeverEatsAPress()
        {
            var n = SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First();
            var dee = n.hudRoot.GetComponentInChildren<DeeAvatar>(true);
            Assert.That(dee, Is.Not.Null, "run AgentScripts/ApplyDeeAvatar.cs (after CompactAssistantBar)");
            Assert.That(dee.director, Is.SameAs(n));
            foreach (var s in new[] { dee.idle, dee.listening, dee.speaking })
                Assert.That(AssetDatabase.GetAssetPath(s), Does.StartWith("Assets/Airlift/Art/Generated/Dee/Dee-"));
            var img = dee.GetComponent<Image>();
            Assert.That(img.raycastTarget, Is.False);
            Assert.That(img.preserveAspect, Is.True);
        }
    }
}
