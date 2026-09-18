using System;
using System.Collections;
using System.Linq;
using Airlift.Lounge;
using Airlift.Welcome;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Runs only in the isolated fresh-learner Play fixture; never changes the saved scene/profile.
public static class CheckControlCleanupRuntime
{
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    public static string Run()
    {
        Require(EditorApplication.isPlaying && SessionState.GetString("Nerdy.PreviewFixture", "") == "fresh", "Use Fresh learner preview first.");
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>();
        Require(n != null && n.Flow.Phase == WelcomePhase.Consent, "Start from welcome.");
        Require(n.consentRoot.activeInHierarchy, "Wait for the startup animation before running the walkthrough.");
        SessionState.SetString("Nerdy.ControlCleanupSmoke", "running");
        n.StartCoroutine(Guard(Walk(n)));
        return "Runtime walkthrough started; result in Nerdy.ControlCleanupSmoke editor session state.";
    }
    static IEnumerator Guard(IEnumerator routine)
    {
        while (true) {
            bool next;
            try { next = routine.MoveNext(); }
            catch (Exception e) { SessionState.SetString("Nerdy.ControlCleanupSmoke", "FAIL: " + e.Message); yield break; }
            if (!next) yield break;
            yield return routine.Current;
        }
    }
    static IEnumerator Walk(NerdyDirector n)
    {
        n.conversationButton.onClick.Invoke();
        Require(n.Paused && n.guide.ConversationStopped && !n.guide.MicActive, "Stop did not stop conversation.");
        n.BeginWelcome(); Require(n.Flow.Phase == WelcomePhase.Welcome, "Questions missing.");
        n.SkipQuestions();
        Require(n.Flow.Phase == WelcomePhase.Catalog && n.rundown.Running && n.rundown.Script.Index == 0, "Tour did not autostart.");
        n.SelectCard("cargo_crew_fractions");
        Require(n.Flow.Phase == WelcomePhase.Catalog, "Early lesson bypassed tour.");
        n.wall.newestButton.onClick.Invoke(); Require(n.rundown.Script.Index == 1, "Filter gate failed.");
        n.wall.nextButton.onClick.Invoke(); Require(n.rundown.Script.Index == 2, "Next page gate failed.");
        n.wall.previousButton.onClick.Invoke(); Require(n.rundown.Script.Index == 3, "Previous page gate failed.");
        var original = n.lounge.Mode; var position = n.welcomeAnchor.position;
        var alternate = original == Scenery.YourRoom ? n.rundown.nerdyLoungeButton : n.rundown.yourRoomButton;
        alternate.onClick.Invoke(); Require(n.rundown.Script.Index == 4 && n.lounge.Mode != original, "Scenery gate failed.");
        Require(Vector3.Distance(position, n.welcomeAnchor.position) < 0.001f, "Scenery moved the board.");
        Require(n.Paused && n.guide.ConversationStopped, "Manual tour resumed voice.");
        n.rundown.gearButton.onClick.Invoke();
        Require(n.loungeSettings.IsOpen && n.rundown.CurrentInstruction == LoungeRundown.SettingsInstruction, "Gear must introduce Done, not finish tour.");
        Require(n.pauseButton.gameObject.activeInHierarchy && n.pauseButton.IsInteractable(), "Settings must retain Play/Stop.");
        n.loungeSettings.panel.transform.Find("Done").GetComponent<Button>().onClick.Invoke();
        Require(!n.loungeSettings.IsOpen && n.rundown.Script.Index == 5, "Done gate failed.");
        n.SelectCard("cargo_crew_fractions");
        yield return null;
        Require(n.Flow.Phase == WelcomePhase.Lesson && !n.rundown.Running && n.Flow.RundownSeen, "Final lesson gate failed.");
        foreach (var id in new[] {"cargo_crew_fractions", "neighborhood_cafe_division", "community_garden_multiplication"})
        {
            if (n.Flow.Phase != WelcomePhase.Lesson) n.SelectCard(id);
            yield return null; // visibility refresh happens in the Director's LateUpdate
            Require(n.Flow.Phase == WelcomePhase.Lesson, "Lesson failed to open: " + id);
            Require(n.exitLessonButton.gameObject.activeInHierarchy, "Missing X exit: " + id);
            Require(!n.navBack.gameObject.activeInHierarchy && !n.navNext.gameObject.activeInHierarchy, "Retired arrows reappeared.");
            var exits = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(b => b.name == "Back to lessons");
            Require(exits.All(b => !b.gameObject.activeInHierarchy), "Legacy exit reappeared.");
            n.exitLessonButton.onClick.Invoke();
            yield return null;
            Require(n.Flow.Phase == WelcomePhase.Catalog && !n.rundown.Running && n.Paused, "X did not preserve catalog/tour/stopped state: " + id);
        }
        n.ReplayRundown(); Require(n.rundown.Running && n.rundown.Script.Index == 0, "Replay failed.");
        n.rundown.skipButton.onClick.Invoke(); Require(!n.rundown.Running, "Skip failed.");
        n.ReplayRundown();
        SessionState.SetString("Nerdy.ControlCleanupSmoke", "PASS: fresh questions, autostart, filter/paging/scenery/gear/Done gates, blocked early entry, all three X exits, replay/skip, silent manual progress. Left at replay intro with Dee stopped.");
    }
}
