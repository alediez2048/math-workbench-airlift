#if UNITY_EDITOR
using System;
using Airlift.Welcome;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Airlift.Editor
{
    /// Temporary desktop input and in-memory learner fixtures. Never saved into the Quest scene.
    [InitializeOnLoad]
    public static class OnboardingPreview
    {
        static OnboardingPreview() { EditorApplication.playModeStateChanged += OnPlayMode; }
        [MenuItem("Nerdy/Preview/Fresh learner")]
        public static void Fresh() => StartFixture("fresh");
        [MenuItem("Nerdy/Preview/Answered profile, unseen tour")]
        public static void Answered() => StartFixture("answered");
        [MenuItem("Nerdy/Preview/Returning learner")]
        public static void Returning() => StartFixture("returning");
        [MenuItem("Nerdy/Preview/Skipped tour")]
        public static void Skipped() => StartFixture("skipped");
        static void StartFixture(string fixture)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before choosing a preview fixture.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "CargoCrew") throw new InvalidOperationException("Open CargoCrew first.");
            SessionState.SetString("Nerdy.PreviewFixture", fixture);
            EditorApplication.isPlaying = true;
        }
        static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetString("Nerdy.PreviewFixture", "") != "") EditorApplication.delayCall += EnableMouse;
            if (state == PlayModeStateChange.EnteredEditMode) SessionState.EraseString("Nerdy.PreviewFixture");
        }
        [MenuItem("Nerdy/Preview/Enable desktop mouse (Play only)")]
        public static void EnableMouse()
        {
            if (!EditorApplication.isPlaying) return;
            var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(); if (n == null || n.head == null) return;
            if (n.head.position.y < 0.5f) n.head.parent.position += Vector3.up * 1.25f;
            var es = EventSystem.current;
            if (es != null)
            {
                foreach (var m in es.GetComponents<BaseInputModule>()) m.enabled = false;
                var mouse = es.GetComponent<InputSystemUIInputModule>(); if (mouse == null) mouse = es.gameObject.AddComponent<InputSystemUIInputModule>();
                mouse.AssignDefaultActions(); mouse.enabled = true;
            }
            var camera = n.head.GetComponent<Camera>();
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)) if (canvas.renderMode == RenderMode.WorldSpace) canvas.worldCamera = camera;
            var game = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")); game.Show(); game.Focus();
        }
    }
}
#endif
