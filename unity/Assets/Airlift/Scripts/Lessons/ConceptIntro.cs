using System.Collections.Generic;

namespace Airlift.Lessons
{
    /// One step of a lesson's concept intro (owner 2026-09-17: elementary learners hear what fractions, dividing and
    /// multiplying are before chapter 1). The card shows Heading/Say/Expression; Dee says Say word for word.
    public sealed class IntroStep
    {
        public string Id;          // "whole", "halves", ... unique within a lesson
        public string Heading;     // card heading, e.g. "What is a fraction? · 1 of 4"
        public string Say;         // what Dee says word for word and what the card body shows
        public string Expression;  // optional expression line ("1/2 + 1/2 = 1"), "" when none
        public string Visual;      // station-specific visual id (same as Id unless noted)
    }

    /// The intro lines of each lesson, from docs/00-build/INTRO-SPLITTER-CONTRACTS.md. Stations own the visuals.
    public static class ConceptIntros
    {
        public static readonly IReadOnlyList<IntroStep> Cargo = Steps("What is a fraction?",
            ("whole", "Before we load, let's learn fractions. This crate is one whole container. We write one whole as 1.", ""),
            ("halves", "Cut the whole into 2 equal parts. Each part is one half, written 1 over 2. The bottom number tells how many equal parts. The top number tells how many parts you have.", ""),
            ("sum", "Put the two halves together and they fill the whole container again. One half plus one half makes one.", "1/2 + 1/2 = 1"),
            ("quarters", "Cut it into 4 equal parts and each part is one quarter, written 1 over 4. More equal parts means smaller parts. Now let's load the trucks.", ""));

        public static readonly IReadOnlyList<IntroStep> Cafe = Steps("What is dividing?",
            ("share", "Dividing means sharing into equal groups. Here are 6 croissants and 2 plates.", ""),
            ("groups", "Give one to each plate, again and again, until none are left. Now each plate has 3.", "6 ÷ 2 = 3"),
            ("boxes", "We can also divide by packing boxes of the same size and counting the boxes. Let's open the café.", ""));

        public static readonly IReadOnlyList<IntroStep> Garden = Steps("What is multiplying?",
            ("row", "Multiplying counts equal rows quickly. Here is one row of 4 seedlings.", ""),
            ("rows", "Three equal rows of 4 make 4, 8, 12 plants.", ""),
            ("times", "We write 3 rows of 4 as 3 times 4, which equals 12. Rows first, then how many in each row. Let's plant.", "3 × 4 = 12"));

        static readonly IReadOnlyList<IntroStep> None = new IntroStep[0];

        /// The intro for a LessonCatalog card id; empty for unknown ids.
        public static IReadOnlyList<IntroStep> For(string cardId)
        {
            switch (cardId)
            {
                case "cargo_crew_fractions": return Cargo;
                case "neighborhood_cafe_division": return Cafe;
                case "community_garden_multiplication": return Garden;
                default: return None;
            }
        }

        static IReadOnlyList<IntroStep> Steps(string question, params (string id, string say, string expression)[] lines)
        {
            var steps = new IntroStep[lines.Length];
            for (int i = 0; i < lines.Length; i++)
                steps[i] = new IntroStep { Id = lines[i].id, Heading = question + " · " + (i + 1) + " of " + lines.Length,
                                           Say = lines[i].say, Expression = lines[i].expression, Visual = lines[i].id };
            return System.Array.AsReadOnly(steps);
        }
    }
}
