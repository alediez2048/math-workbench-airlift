using System.Collections.Generic;
using System.Text;
using Airlift.Lessons;
using Airlift.Lessons.Garden;

namespace Airlift.Welcome
{
    /// What the learner can see and grab on the Sunny Plot table. The guide speaks from this, not from the card.
    /// Convention: rows × columns (a 3 × 4 bed is 3 rows of 4). Row numbers are 1-based here.
    public static class GardenSteps
    {
        public const string BriefingId = "garden_briefing";

        public static GuideStep Briefing() => new GuideStep(BriefingId,
            "The Sunny Plot briefing card only; no seedlings or beds are on the table yet. The first chapter starts when the learner says yes or presses Start.",
            false);

        public const string IntroId = "intro";

        /// A concept intro step ("What is multiplying?"): exactly what GardenStation draws for its visual on the chapter 1
        /// bed. Nothing is grabbable during the intro.
        public static GuideStep Intro(IntroStep step)
        {
            string table;
            switch (step?.Visual)
            {
                case "row":
                    table = "Concept intro. The Sunny Plot bed has room for 3 rows of 4. Only the back row is planted: one row of 4 seedlings. The trays are empty.";
                    break;
                case "rows":
                    table = "Concept intro. The Sunny Plot bed has 3 equal rows of 4 seedlings planted: 12 seedlings. The trays are empty.";
                    break;
                case "times":
                    table = "Concept intro. The Sunny Plot bed has 3 equal rows of 4 seedlings planted, with row numbers 1, 2 and 3 beside the rows: 12 seedlings. The card shows 3 × 4 = 12.";
                    break;
                default:
                    table = "Concept intro. The Sunny Plot bed for chapter 1.";
                    break;
            }
            var steps = ConceptIntros.Garden;
            bool last = steps.Count > 0 && step == steps[steps.Count - 1];
            return new GuideStep(IntroId, table + " Nothing can be grabbed during the intro. Next: say next or press " + (last ? "Start" : "Next") + ".", false);
        }

        public static string StepId(GardenChapter chapter) => "garden_chapter" + chapter.Number + "_" + chapter.Id;

        /// One chapter: bed size, planted rows, tray strips, turn and fence state, and the next action.
        public static GuideStep ForChapter(GardenModel m)
        {
            if (m == null || m.Chapter == null)
                return new GuideStep("garden_chapter", "A Sunny Plot garden bed with rows of seedlings.", false);

            var c = m.Chapter;
            var sb = new StringBuilder();
            sb.Append("Chapter ").Append(c.Number).Append(" · ").Append(c.Title).Append(". ");
            sb.Append(m.Turned ? "Turned bed, now " : "Bed ")
              .Append(m.Rows).Append(" × ").Append(m.Columns).Append(": ")
              .Append(m.Rows).Append(" rows of ").Append(m.Columns).Append(". ");

            if (m.ChapterComplete)
            {
                sb.Append("Bed accepted: ").Append(m.Expression).Append(". The rows grow into flowers and vegetables.");
                return new GuideStep(StepId(c), sb.ToString(), false);
            }

            AppendRows(sb, m);

            var tray = m.TrayStripIds;
            if (tray.Count > 0)
            {
                var sizes = new List<string>();
                foreach (var id in tray) sizes.Add(m.StripLength(id).ToString());
                sb.Append("Tray strips: ").Append(string.Join(", ", sizes)).Append(". ");
            }
            else if (!c.StartsPlanted) sb.Append("Tray empty. ");

            if (c.NeedsTurn)
                sb.Append(m.Turned ? "Turned once; turning again goes back. " : "Not turned yet; the bed can turn a quarter turn. ");

            if (c.HasFence)
            {
                if (m.FenceColumn == 0) sb.Append("No fence yet; it goes after column 1 to ").Append(m.Columns - 1).Append(". ");
                else sb.Append("Fence after column ").Append(m.FenceColumn).Append(": parts ")
                       .Append(m.Rows).Append(" × ").Append(m.FenceColumn).Append(" and ")
                       .Append(m.Rows).Append(" × ").Append(m.Columns - m.FenceColumn).Append(". ");
            }

            sb.Append(c.Task);
            return new GuideStep(StepId(c), sb.ToString(), CanGrabNow(m));
        }

        /// True while the chapter is open and a strip can be planted, a short row can be taken out, or the fence moved.
        public static bool CanGrabNow(GardenModel m) =>
            m != null && !m.ChapterComplete && (m.CanPlantSomething || m.HasShortRow || m.Chapter.HasFence);

        static void AppendRows(StringBuilder sb, GardenModel m)
        {
            int empty = m.EmptyRowCount;
            if (empty == m.Rows) { sb.Append("No rows planted. "); return; }

            bool allFull = empty == 0 && !m.HasShortRow;
            if (allFull)
            {
                sb.Append("All ").Append(m.Rows).Append(" rows planted with strips of ").Append(m.Columns)
                  .Append(" (").Append(m.PlantCount).Append(" seedlings). ");
                return;
            }

            var planted = new List<string>();
            var gaps = new List<string>();
            for (int r = 0; r < m.Rows; r++)
            {
                string id = m.StripInRow(r);
                if (id == null) gaps.Add((r + 1).ToString());
                else planted.Add((r + 1) + " (" + m.StripLength(id) + ")");
            }
            sb.Append("Planted rows (seedlings): ").Append(string.Join(", ", planted)).Append(". ");
            if (gaps.Count > 0) sb.Append(gaps.Count == 1 ? "Empty row: " : "Empty rows: ").Append(string.Join(", ", gaps)).Append(". ");
        }
    }
}
