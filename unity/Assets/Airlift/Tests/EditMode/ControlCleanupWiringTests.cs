using System.Linq;
using Airlift.Welcome;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Airlift.Tests
{
    public class ControlCleanupWiringTests
    {
        const string Path = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene opened;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(Path).isLoaded) opened = EditorSceneManager.OpenScene(Path, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (opened.IsValid()) EditorSceneManager.CloseScene(opened, true); }
        static NerdyDirector Director() => SceneManager.GetSceneByPath(Path).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).Single();

        [Test] public void CompanionHasOneConversationControlAndRetiredShortcuts()
        {
            var n = Director();
            Assert.That(n.conversationButton, Is.Not.Null);
            Assert.That(n.conversationButton.gameObject.activeSelf, Is.True);
            Assert.That(n.conversationLabel, Is.Not.Null);
            Assert.That(n.conversationButton.onClick.GetPersistentEventCount(), Is.Zero, "only the Director runtime subscription should toggle it");
            foreach (var b in new[] {n.micButton, n.musicButton, n.helpButton}) {
                Assert.That(b, Is.Not.Null);
                Assert.That(b.transform.parent.name, Does.StartWith("Retired control"));
                Assert.That(b.transform.parent.gameObject.activeSelf, Is.False);
            }
            Assert.That(n.pauseButton.gameObject.activeSelf, Is.True, "Settings has its own Stop while it hides the companion bar");
        }

        [Test] public void AllThreeLegacyLessonExitsStayHiddenEvenIfRefreshEnablesThem()
        {
            var buttons = SceneManager.GetSceneByPath(Path).GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Button>(true))
                .Where(b => b.name == "Back to lessons" || b.GetComponentsInChildren<TMP_Text>(true).Any(t => t.text.Trim().ToLowerInvariant() == "back to lessons")).ToArray();
            Assert.That(buttons.Length, Is.EqualTo(3));
            foreach (var b in buttons) {
                bool previous = b.gameObject.activeSelf;
                try {
                    b.gameObject.SetActive(true);
                    Assert.That(b.transform.parent.name, Does.StartWith("Retired control"));
                    Assert.That(b.transform.parent.gameObject.activeSelf, Is.False);
                    Assert.That(b.gameObject.activeInHierarchy, Is.False, b.name + " must never intercept clicks");
                } finally { b.gameObject.SetActive(previous); }
            }
        }
    }
}
