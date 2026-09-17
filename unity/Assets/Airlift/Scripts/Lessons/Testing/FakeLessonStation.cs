#if UNITY_EDITOR
using System.Collections.Generic;
using Airlift.Lessons;
using Airlift.Welcome;
using Newtonsoft.Json.Linq;

namespace Airlift.Lessons.Testing
{
    /// Editor-only test double: a LessonStation with no scene objects, for routing and visibility tests. It lives in the
    /// runtime assembly because Unity cannot attach a MonoBehaviour from the editor-only test assembly; the
    /// UNITY_EDITOR guard keeps it out of the player. No scene may reference it.
    public sealed class FakeLessonStation : LessonStation
    {
        public string title = "Fake Lesson", storyName = "Fake Story";
        public string[] toolNames = new string[0];
        public bool open, chapterActive, held;
        public readonly List<string> calls = new List<string>();

        public override string Title => title;
        public override string StoryName => storyName;
        public override bool IsOpen => open;
        public override bool ChapterActive => chapterActive;
        public override bool AnyHeld => held;
        public override LessonChapterFacts Chapter => chapterActive ? new LessonChapterFacts(1, "first", "First", "Story.", "Task.", "Done.", "", "", false, false) : default;
        public override GuideStep CurrentStep() => new GuideStep(chapterActive ? "fake_chapter1" : "fake_briefing", "A fake table.", chapterActive);
        public override string[] ToolNames => toolNames;
        public override string[] ToolsNow => LessonToolRouter.SharedTools;

        public override void Open() { open = true; calls.Add("Open"); }
        public override void Close() { open = false; calls.Add("Close"); }
        LessonActionResult Record(string name) { calls.Add(name); return new LessonActionResult(true, name); }
        public override LessonActionResult Advance() => Record("Advance");
        public override LessonActionResult Check() => Record("Check");
        public override LessonActionResult ResetTable() => Record("ResetTable");
        public override LessonActionResult NextChapter() => Record("NextChapter");
        public override LessonActionResult RestartChapter() => Record("RestartChapter");
        public override bool TryLessonTool(string name, JObject args, out LessonActionResult result)
        {
            result = default;
            if (System.Array.IndexOf(toolNames, name) < 0) return false;
            result = Record(name); return true;
        }
    }
}
#endif
