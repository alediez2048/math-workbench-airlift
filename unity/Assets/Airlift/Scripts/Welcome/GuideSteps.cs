using System.Collections.Generic;
using System.Text;
using Airlift.Lessons;
using Airlift.Onboarding;
using Newtonsoft.Json.Linq;

namespace Airlift.Welcome
{
    public readonly struct GuideStep
    {
        public readonly string Id, OnTableNow; public readonly bool CanGrabNow;
        public GuideStep(string id, string onTableNow, bool canGrabNow) { Id = id; OnTableNow = onTableNow; CanGrabNow = canGrabNow; }
    }

    /// What the learner can actually see and grab at each step. The guide speaks from this, not from the
    /// card's lesson overview (owner report 2026-09-16: "grab the strap" during the briefing, nothing to grab).
    public static class GuideSteps
    {
        public const int MaxOnTableLength = 300;   // GuideContextBuilder.LessonStep trims on_table_now to this

        public static GuideStep ForOnboarding(OnboardingStage s)
        {
            switch (s)
            {
                case OnboardingStage.Overview: return new GuideStep("briefing", "The Dock 7 briefing card only; no crate is on the table yet. The next step starts when the learner says yes or presses Begin briefing.", false);
                case OnboardingStage.Orientation: return new GuideStep("orientation", "An example crate rests on the tray to look at; it cannot be grabbed yet. Next: say yes or press Watch demo.", false);
                case OnboardingStage.Demonstration: return new GuideStep("demo", "The example crate is moving onto the loading pad by itself; the learner only watches.", false);
                case OnboardingStage.Practice: return new GuideStep("practice", "The orange practice crate is on the tray and can be grabbed now with the grip, moved above the loading pad, and released. Saying show me the demo replays the demo.", true);
                case OnboardingStage.Ready: return new GuideStep("practice_done", "The practice crate is on the loading pad. Next: say start loading or yes, or press Start fractions.", true);
                default: return new GuideStep("cards", "The lesson cards are showing; no table.", false);
            }
        }

        /// One Dock 7 chapter on the table: the vehicles backed up to the dock with their beds side by side over the
        /// 0 to 1 ruler (CargoChapter.BedCells, 8 cells = one container), any locked crate, and the loose crates at
        /// their current size. A split chapter whose crates can no longer split has been split one level. After an
        /// accepted load the vehicles drive away with the crates, so nothing is grabbable until the next chapter.
        public static GuideStep ForChapter(CargoChapter chapter, bool complete, bool canSplit)
        {
            if (chapter == null) return new GuideStep("chapter", "Vehicles back up to the dock over the 0 to 1 ruler: one whole container.", true);
            var sb = new StringBuilder();
            sb.Append("Chapter ").Append(chapter.Number).Append(" · ").Append(chapter.Title).Append(". ");
            var beds = chapter.BedCells ?? new int[0];
            int vehicles = beds.Length > 0 ? beds.Length : System.Math.Max(1, chapter.VehicleCount);
            string noun = VehicleNoun(chapter.VehicleKind, beds);
            sb.Append(Capitalize(Count(vehicles))).Append(' ').Append(vehicles == 1 ? noun : noun + "s")
              .Append(vehicles == 1 ? " is" : " are").Append(" backed up to the dock, ")
              .Append(vehicles == 1 ? "its bed" : "beds side by side").Append(" over the 0 to 1 ruler (one whole container)");
            if (beds.Length > 0)
            {
                bool same = true; int used = 0;
                foreach (int b in beds) { if (b != beds[0]) same = false; used += b; }
                if (same) sb.Append(vehicles == 1 ? "; the bed holds " : "; each " + noun + " bed holds ").Append(CellsFraction(beds[0]));
                else { var sizes = new List<string>(); foreach (int b in beds) sizes.Add(CellsFraction(b)); sb.Append("; beds hold ").Append(string.Join(", ", sizes)); }
                if (used < WholeCells && used > 0) sb.Append(", and ").Append(CellsFraction(used)).Append(" to 1 is not needed");
            }
            sb.Append(". ");
            if (chapter.LockedPieces != null && chapter.LockedPieces.Length > 0)
                sb.Append("Locked in the bed from 0: ").Append(Crates(chapter.LockedPieces)).Append(". ");
            string into = vehicles == 1 ? "the bed" : "the beds";
            if (complete)
            {
                sb.Append("Load accepted: ").Append(chapter.Expression).Append(". ")
                  .Append(vehicles == 1 ? "The " + noun + " drives" : "The " + noun + "s drive").Append(" away with the crates.");
                return new GuideStep("chapter" + chapter.Number + "_" + chapter.Id, sb.ToString(), false);
            }
            bool split = chapter.SplitTo > 0 && !canSplit;
            var loose = split ? SplitOnce(chapter.StartPieces, chapter.SplitTo) : chapter.StartPieces;
            sb.Append("Crates to load: ").Append(Crates(loose)).Append(". ");
            if (canSplit) sb.Append("They can be split into equal chunks, then loaded into ").Append(into).Append('.');
            else sb.Append("Load crates into ").Append(into).Append(", then check the load.");
            return new GuideStep("chapter" + chapter.Number + "_" + chapter.Id, sb.ToString(), true);
        }

        const int WholeCells = 8;

        static string VehicleNoun(string kind, int[] beds)
        {
            if (kind == "truck") return beds.Length > 0 && beds[0] == WholeCells ? "big truck" : "truck";
            return string.IsNullOrEmpty(kind) ? "vehicle" : kind;
        }

        /// Cells of one 8-cell container as a reduced fraction: 8 -> "1", 4 -> "1/2", 2 -> "1/4".
        static string CellsFraction(int cells)
        {
            if (cells <= 0) return "0";
            int a = cells, b = WholeCells;
            while (b != 0) { int r = a % b; a = b; b = r; }
            int n = cells / a, d = WholeCells / a;
            return d == 1 ? n.ToString() : n + "/" + d;
        }

        static string Capitalize(string word) => string.IsNullOrEmpty(word) ? word : char.ToUpperInvariant(word[0]) + word.Substring(1);

        static int[] SplitOnce(int[] start, int splitTo)
        {
            var result = new List<int>();
            if (start == null) return result.ToArray();
            foreach (int d in start)
            {
                if (d >= splitTo) { result.Add(d); continue; }
                result.Add(d * 2); result.Add(d * 2);
            }
            return result.ToArray();
        }

        /// "two 1/2 crates", "one full crate (1)". Groups equal sizes in first-seen order.
        static string Crates(int[] denominators)
        {
            if (denominators == null || denominators.Length == 0) return "none";
            var order = new List<int>(); var counts = new Dictionary<int, int>();
            foreach (int d in denominators) { if (!counts.ContainsKey(d)) { counts[d] = 0; order.Add(d); } counts[d]++; }
            var parts = new List<string>();
            foreach (int d in order)
            {
                int n = counts[d];
                parts.Add(d == 1 ? Count(n) + (n == 1 ? " full crate (1)" : " full crates (1 each)") : Count(n) + " 1/" + d + (n == 1 ? " crate" : " crates"));
            }
            return string.Join(" and ", parts);
        }

        static string Count(int n)
        {
            switch (n) { case 1: return "one"; case 2: return "two"; case 3: return "three"; case 4: return "four"; case 5: return "five"; case 6: return "six"; case 7: return "seven"; case 8: return "eight"; default: return n.ToString(); }
        }

        /// The practice panel appends a temporary controller readout; it is telemetry, not an instruction.
        public static string StripDiagnostics(string shown)
        {
            if (string.IsNullOrEmpty(shown)) return "";
            int i = shown.IndexOf("\n\n<size=70%>", System.StringComparison.Ordinal);
            return i >= 0 ? shown.Substring(0, i) : shown;
        }
    }

    public enum OpenLessonDecision { Open, NotShowing, ComingSoon, Unknown }

    /// Where a spoken "yes / next" goes.
    public enum AdvanceRoute { Refuse, OnboardingStep, NextChapter }

    /// Whether a voice action may run now, and the one sentence the guide says when it may not.
    public readonly struct VoiceGate
    {
        public readonly bool Ok; public readonly string Reason;
        public VoiceGate(bool ok, string reason) { Ok = ok; Reason = reason ?? ""; }
        public static VoiceGate Allow => new VoiceGate(true, "");
        public static VoiceGate Refuse(string reason) => new VoiceGate(false, reason);
    }

    /// Pure decisions behind the guide's tools. NerdyDirector only gathers state and calls the same methods
    /// the buttons call; every refusal reason here is spoken as-is.
    public static class GuideTools
    {
        public const string LetGo = "Let go of the crate first.";
        /// Tool result while the learner has paused the guide: no action runs.
        public const string PausedResult = "{\"ok\":false,\"reason\":\"The guide is paused. Press Play to continue.\"}";
        public const string NoLesson = "No lesson is open. Say which lesson to open first.";
        public const string CardsAlreadyShowing = "The lesson cards are already showing.";
        public const string ChaptersNotStarted = "The loading chapters start after the briefing and practice.";
        public const string SayStartLoading = "The loading chapters have not started. Say start loading first.";
        public const string DemoOnlyInPractice = "The demo is part of the practice step. Say start this chapter over to reset the crates.";
        public const string DemoAfterBriefing = "The demo comes after the briefing. Say yes to go on.";
        public const string DemoPlaying = "The demo is already playing.";
        public const string FinishPractice = "Move the practice crate onto the loading pad first.";
        public const string NoNextStep = "There is no next step right now.";

        /// Tools that act on the Cargo Crew workbench (open_lesson, request_help and the welcome tools are separate).
        public static readonly string[] LessonToolNames = { "advance_step", "replay_demo", "split_cargo", "check_load", "reset_cargo", "next_chapter", "restart_chapter", "back_to_lessons" };

        public static OpenLessonDecision OpenLesson(WelcomePhase phase, string cardId)
        {
            var card = LessonCatalog.Find(cardId);
            if (card == null) return OpenLessonDecision.Unknown;
            if (phase != WelcomePhase.Catalog) return OpenLessonDecision.NotShowing;
            return card.Playable ? OpenLessonDecision.Open : OpenLessonDecision.ComingSoon;
        }

        public static string CardId(string argumentsJson)
        {
            try { return (string)JObject.Parse(argumentsJson)["cardId"] ?? ""; } catch { return ""; }
        }

        /// "yes / next": the on-card primary button during onboarding, next_chapter inside a chapter.
        public static AdvanceRoute AdvanceStep(WelcomePhase phase, bool chapterActive, bool primaryAvailable, bool holding)
        {
            if (phase != WelcomePhase.Lesson) return AdvanceRoute.Refuse;
            if (chapterActive) return AdvanceRoute.NextChapter;
            return primaryAvailable && !holding ? AdvanceRoute.OnboardingStep : AdvanceRoute.Refuse;
        }

        public static string AdvanceRefusal(WelcomePhase phase, OnboardingStage stage, bool holding)
        {
            if (phase != WelcomePhase.Lesson) return NoLesson;
            if (holding) return LetGo;
            switch (stage)
            {
                case OnboardingStage.Demonstration: return "The demo is still playing. Watch it finish first.";
                case OnboardingStage.Practice: return FinishPractice;
                default: return NoNextStep;
            }
        }

        /// Replay only where the Help button replays it: practice or ready, nothing held, no chapter on the table.
        public static VoiceGate ReplayDemo(WelcomePhase phase, bool chapterActive, OnboardingStage stage, bool holding)
        {
            if (phase != WelcomePhase.Lesson) return VoiceGate.Refuse(NoLesson);
            if (chapterActive) return VoiceGate.Refuse(DemoOnlyInPractice);
            if (holding) return VoiceGate.Refuse(LetGo);
            switch (stage)
            {
                case OnboardingStage.Practice:
                case OnboardingStage.Ready: return VoiceGate.Allow;
                case OnboardingStage.Demonstration: return VoiceGate.Refuse(DemoPlaying);
                case OnboardingStage.Overview:
                case OnboardingStage.Orientation: return VoiceGate.Refuse(DemoAfterBriefing);
                default: return VoiceGate.Refuse(NoLesson);
            }
        }

        /// split_cargo, check_load, reset_cargo, next_chapter, restart_chapter: a chapter must be on the table.
        /// Held crates and chapter rules are refused by CargoLessonDirector.Try* with its own reason.
        public static VoiceGate ChapterTool(WelcomePhase phase, bool chapterActive, OnboardingStage stage)
        {
            if (phase != WelcomePhase.Lesson) return VoiceGate.Refuse(NoLesson);
            if (chapterActive) return VoiceGate.Allow;
            return VoiceGate.Refuse(stage == OnboardingStage.Ready ? SayStartLoading : ChaptersNotStarted);
        }

        /// back_to_lessons does what the Back button does, except while anything is held.
        public static VoiceGate BackToLessons(WelcomePhase phase, bool holding)
        {
            if (phase == WelcomePhase.Catalog) return VoiceGate.Refuse(CardsAlreadyShowing);
            if (phase != WelcomePhase.Lesson) return VoiceGate.Refuse(NoLesson);
            if (holding) return VoiceGate.Refuse(LetGo);
            return VoiceGate.Allow;
        }

        /// The tool output: ok, reason, any extra fields, the step facts, and the chapter facts when a chapter is
        /// on the table. The guide narrates only this, so the verdict and expression come from the app.
        public static string ToolResult(bool ok, string reason, GuideStep step, string instruction,
            CargoChapter chapter = null, bool chapterComplete = false, string expression = null, string feedback = null, JObject extra = null)
        {
            var o = new JObject { ["ok"] = ok, ["reason"] = reason ?? "" };
            if (extra != null) foreach (var p in extra.Properties()) o[p.Name] = p.Value;
            o["step"] = step.Id ?? "";
            o["on_table_now"] = step.OnTableNow ?? "";
            o["can_grab_now"] = step.CanGrabNow;
            o["instruction"] = instruction ?? "";
            if (chapter != null)
            {
                o["chapter"] = chapter.Number;
                o["chapter_title"] = chapter.Title ?? "";
                o["story"] = chapter.Story ?? "";
                o["task"] = chapter.Task ?? "";
                o["chapter_complete"] = chapterComplete;
                o["expression"] = expression ?? "";
                o["feedback"] = feedback ?? "";
            }
            return o.ToString(Newtonsoft.Json.Formatting.None);
        }

        /// Station form: the chapter facts of any lesson (Number 0 = no chapter). When the station lists tools_now
        /// (café, garden) the result also carries lesson (its title) and tools_now; Cargo passes null and its JSON is
        /// byte-identical to the CargoChapter form above.
        public static string ToolResult(bool ok, string reason, GuideStep step, string instruction, LessonChapterFacts chapter, JObject extra, string lesson, string[] toolsNow)
        {
            var o = new JObject { ["ok"] = ok, ["reason"] = reason ?? "" };
            if (extra != null) foreach (var p in extra.Properties()) o[p.Name] = p.Value;
            if (toolsNow != null) o["lesson"] = lesson ?? "";
            o["step"] = step.Id ?? "";
            o["on_table_now"] = step.OnTableNow ?? "";
            o["can_grab_now"] = step.CanGrabNow;
            o["instruction"] = instruction ?? "";
            if (chapter.Number > 0)
            {
                o["chapter"] = chapter.Number;
                o["chapter_title"] = chapter.Title ?? "";
                o["story"] = chapter.Story ?? "";
                o["task"] = chapter.Task ?? "";
                o["chapter_complete"] = chapter.Complete;
                o["expression"] = chapter.Expression ?? "";
                o["feedback"] = chapter.Feedback ?? "";
            }
            if (toolsNow != null) o["tools_now"] = new JArray(toolsNow);
            return o.ToString(Newtonsoft.Json.Formatting.None);
        }

        /// Station form of StoryLine: the new chapter's story, the accepted line once, else null.
        public static string StoryLine(int lastNumber, bool lastComplete, LessonChapterFacts chapter)
        {
            if (chapter.Number <= 0) return null;
            if (chapter.Number != lastNumber) return chapter.Story;
            if (chapter.Complete && !lastComplete) return chapter.Accepted;
            return null;
        }

        /// The Dock 7 line to say when the chapter changed through a button: the new chapter's story when a
        /// different chapter is on the table, the payoff when the current load was just accepted, else null.
        /// lastNumber is 0 when no chapter was on the table.
        public static string StoryLine(int lastNumber, bool lastComplete, CargoChapter chapter, bool complete)
        {
            if (chapter == null) return null;
            if (chapter.Number != lastNumber) return chapter.Story;
            if (complete && !lastComplete) return chapter.Accepted;
            return null;
        }
    }
}
