using TMPro;
using UnityEngine;

namespace Airlift.Lessons
{
    /// The facts of the chapter on the table, for tool results and story lines. default(LessonChapterFacts) when no
    /// chapter is active (Number 0).
    public readonly struct LessonChapterFacts
    {
        public readonly int Number; public readonly string Id, Title, Story, Task, Accepted, Expression, Feedback;
        public readonly bool Complete, IsLast;
        public LessonChapterFacts(int number, string id, string title, string story, string task, string accepted,
                                  string expression, string feedback, bool complete, bool isLast)
        {
            Number = number; Id = id; Title = title; Story = story; Task = task; Accepted = accepted;
            Expression = expression; Feedback = feedback; Complete = complete; IsLast = isLast;
        }
    }

    /// One lesson workbench NerdyDirector can open (Cargo Crew, Neighborhood Café, Community Garden). The director
    /// routes shared voice tools to the open station and lesson tools through LessonToolRouter; math and lesson state
    /// stay inside the station. Every action returns what actually happened, with a short spoken reason.
    public abstract class LessonStation : MonoBehaviour
    {
        public string cardId;                         // LessonCatalog id
        public GameObject[] visualRoots;              // shown only while this station is open
        public CanvasGroup[] fallbackGroups;          // button rows hidden while the live guide listens
        public TMP_Text heading, body;                // this station's card; body is the guide's instruction source

        public abstract string Title { get; }         // "Neighborhood Café"
        public abstract string StoryName { get; }     // "Corner Café"
        public abstract bool IsOpen { get; }
        public abstract bool ChapterActive { get; }   // false during the briefing
        public abstract bool AnyHeld { get; }
        public abstract LessonChapterFacts Chapter { get; }   // default(LessonChapterFacts) when !ChapterActive
        /// The concept-intro step on the card right now, or null outside the intro (ConceptIntros).
        public virtual IntroStep CurrentIntro => null;
        public abstract Airlift.Welcome.GuideStep CurrentStep();
        public virtual string Instruction => body != null ? Airlift.Welcome.GuideSteps.StripDiagnostics(body.text) : "";
        public abstract string[] ToolNames { get; }   // lesson-specific tools this station owns
        public abstract string[] ToolsNow { get; }    // shared + lesson tools that make sense right now

        public abstract void Open();                  // show roots, start at the briefing (Cargo: ChooseCargo)
        public abstract void Close();                 // hide roots, stop animations, pieces back to trays
        public abstract LessonActionResult Advance(); // briefing -> chapter 1; inside a chapter: next chapter
        public abstract LessonActionResult Check();
        public abstract LessonActionResult ResetTable();
        public abstract LessonActionResult NextChapter();
        public abstract LessonActionResult RestartChapter();
        public abstract bool TryLessonTool(string name, Newtonsoft.Json.Linq.JObject args, out LessonActionResult result);

        /// Shows or hides this station's own visual roots (StationVisibility decides which station is shown).
        public void SetVisualsActive(bool on)
        {
            if (visualRoots == null) return;
            foreach (var root in visualRoots)
                if (root != null && !Airlift.Welcome.StationVisibility.IsShared(root.name) && root.activeSelf != on) root.SetActive(on);
        }
    }
}
