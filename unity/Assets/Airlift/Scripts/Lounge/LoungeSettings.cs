using System.Collections.Generic;
using UnityEngine;

namespace Airlift.Lounge
{
    /// CC-FD-09, brought forward. The welcome board asks one question — voice or no voice — and everything else
    /// (language, scenery, and the settings that follow) lives behind the gear on the assistant bar.
    /// Owner 2026-09-17: "lets leave only two buttons visible on the dashboard ... everything else should be inside
    /// a gear icon next to the ai assistant."
    public sealed class LoungeSettings : MonoBehaviour
    {
        [Tooltip("The settings card itself; closed until the learner opens it.")]
        public GameObject panel;
        [Tooltip("Hidden while settings are open, so the two cards never overlap.")]
        public GameObject[] hideWhileOpen;

        // Only what was showing when settings opened comes back when they close. Re-activating every root on
        // close was the bug that made Done open the lesson cards on top of the welcome board.
        readonly List<GameObject> wasShowing = new List<GameObject>();

        public bool IsOpen => panel != null && panel.activeSelf;

        void Awake() { if (panel != null) panel.SetActive(false); }

        /// Wired to the gear on the assistant bar.
        public void Toggle() => Show(!IsOpen);
        public void Close() => Show(false);

        public void Show(bool on)
        {
            if (panel == null || on == IsOpen) return;
            if (on)
            {
                wasShowing.Clear();
                if (hideWhileOpen != null)
                    foreach (var go in hideWhileOpen)
                        if (go != null && go != panel && go.activeSelf) { wasShowing.Add(go); go.SetActive(false); }
                panel.SetActive(true);
            }
            else
            {
                panel.SetActive(false);
                foreach (var go in wasShowing) if (go != null) go.SetActive(true);
                wasShowing.Clear();
            }
        }
    }
}
