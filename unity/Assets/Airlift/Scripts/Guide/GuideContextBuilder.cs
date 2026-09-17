using Newtonsoft.Json.Linq;

namespace Airlift.Guide
{
    /// Builds the only text the app ever pushes into the conversation as context: app-authored
    /// instruction and verdict strings, never learner free text, never unsubmitted correctness.
    public static class GuideContextBuilder
    {
        public const int MaxLength = 600;

        public static string Lesson(string stageLabel, string instructionShown)
        {
            var o = new JObject { ["kind"] = "lesson_state", ["stage"] = Trim(stageLabel, 60), ["instruction"] = Trim(instructionShown, MaxLength),
                ["rules"] = "Explain the instruction in your own words when asked. Do not judge correctness unless the instruction itself states a submitted result." };
            return "APP CONTEXT " + o.ToString(Newtonsoft.Json.Formatting.None);
        }

        /// The current step, what is on the table right now and whether anything can be grabbed. Speech about
        /// what to do must come from this, never from the lesson overview facts.
        public static string LessonStep(string lesson, string stepId, string onTableNow, bool canGrabNow, string instructionShown)
        {
            var o = new JObject { ["kind"] = "lesson_state", ["lesson"] = Trim(lesson, 60), ["step"] = Trim(stepId, 40), ["on_table_now"] = Trim(onTableNow, 300),
                ["can_grab_now"] = canGrabNow, ["instruction"] = Trim(instructionShown, MaxLength),
                ["rules"] = "Only describe what is on the table now and what the instruction says. Do not judge correctness unless the instruction itself states a submitted result." };
            return "APP CONTEXT " + o.ToString(Newtonsoft.Json.Formatting.None);
        }

        public static string Entered(string cardTitle, string[] facts)
            => "APP CONTEXT " + new JObject { ["kind"] = "entered_lesson", ["lesson"] = Trim(cardTitle, 60), ["facts"] = new JArray(facts),
                ["note"] = "Lesson overview for answering questions, not the current step. The latest lesson_state says what is on the table now." }.ToString(Newtonsoft.Json.Formatting.None);

        public static string Catalog() => "APP CONTEXT {\"kind\":\"catalog\",\"note\":\"Three lesson cards are in front of the learner. Only Cargo Crew (fractions) can be opened; the other two are previews.\"}";

        static string Trim(string s, int max) => string.IsNullOrEmpty(s) ? "" : (s.Length <= max ? s : s.Substring(0, max));
    }
}
