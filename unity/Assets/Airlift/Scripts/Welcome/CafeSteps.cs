using System.Text;
using Airlift.Lessons;
using Airlift.Lessons.Cafe;
using Airlift.Presentation.Cafe;

namespace Airlift.Welcome
{
    /// What the learner can see and grab at the Corner Café counter. The guide speaks from this, so every count
    /// here is read from the model, never from the card copy.
    public static class CafeSteps
    {
        public const string BriefingId = "cafe_briefing";

        public static GuideStep Briefing() => new GuideStep(BriefingId,
            "The Corner Café briefing card only; no pastry is on the counter yet. The first chapter starts when the learner says yes or presses Start.",
            false);

        /// A concept-intro step ("What is dividing?"): exactly what the intro visual poses on the counter. Nothing can
        /// be grabbed; the learner moves on with Next (Start on the last step).
        public static GuideStep Intro(IntroStep step, bool lastStep)
        {
            var scene = CafeLayout.IntroScene(step != null ? step.Visual : null);
            var sb = new StringBuilder("Intro card: ");
            int n = scene.Containers, per = scene.PerContainer;
            string items = scene.Items + " " + (scene.Items == 1 ? scene.ItemName : scene.ItemPlural);
            string perWord = per == 1 ? scene.ItemName : scene.ItemPlural;
            if (scene.Items == 0 || n == 0) sb.Append("the counter is empty. ");
            else if (!scene.Placed)
                sb.Append(items).Append(" on the tray and ").Append(n).Append(" empty ").Append(Container(scene.Kind, n)).Append(" on the counter. ");
            else if (scene.Kind == CafeTargetKind.Plates)
                sb.Append(n).Append(' ').Append(Container(scene.Kind, n)).Append(" on the counter with ").Append(per).Append(' ').Append(perWord).Append(" on each; the tray is empty. ");
            else
                sb.Append(n).Append(' ').Append(Container(scene.Kind, n)).Append(" on the counter with ").Append(per).Append(' ').Append(perWord).Append(" in each; the tray is empty. ");
            sb.Append("Nothing can be grabbed yet. Next: say ").Append(lastStep ? "start or press Start." : "next or press Next.");
            return new GuideStep("intro", Fit(sb), false);
        }

        static string Container(CafeTargetKind kind, int n) => kind == CafeTargetKind.Plates ? (n == 1 ? "plate" : "plates") : (n == 1 ? "box" : "boxes");

        /// One chapter on the counter: the plates or boxes with the count on each, the loose pastries on the tray,
        /// and the earned expression. After an accepted chapter the plates are served or the boxes leave on the
        /// delivery bike, so nothing is grabbable until the next chapter.
        public static GuideStep ForChapter(CafeModel m)
        {
            if (m == null || m.Chapter == null)
                return new GuideStep("cafe_chapter", "Pastries on the tray and plates or boxes on the counter.", true);

            var chapter = m.Chapter;
            string id = "cafe_chapter" + chapter.Number + "_" + chapter.Id;
            var stage = m.Stage;
            bool plates = stage.Kind == CafeTargetKind.Plates;
            var sb = new StringBuilder();
            sb.Append("Chapter ").Append(chapter.Number).Append(" · ").Append(chapter.Title).Append(". ");
            if (chapter.Stages.Length > 1)
                sb.Append("Part ").Append(m.StageIndex + 1).Append(" of ").Append(chapter.Stages.Length).Append(". ");

            if (m.ChapterComplete)
            {
                sb.Append("Order accepted: ").Append(chapter.Expression).Append(". ")
                  .Append(plates ? "The plates go to the guests." : "The full boxes leave on the delivery bike.");
                return new GuideStep(id, Fit(sb), false);
            }

            int n = m.ContainerCount;
            sb.Append(n).Append(plates ? (n == 1 ? " plate" : " plates") : (n == 1 ? " box" : " boxes"));
            if (!plates) sb.Append(" (each holds ").Append(stage.BoxCapacity).Append(')');
            sb.Append(" on the counter, holding ");
            for (int c = 0; c < n; c++)
            {
                if (c > 0) sb.Append(c == n - 1 ? " and " : ", ");
                sb.Append(m.CountIn(c));
            }
            sb.Append(". ");

            int loose = m.LooseCount;
            sb.Append(loose).Append(' ').Append(chapter.ItemWord(loose)).Append(" loose on the tray. ");
            string earned = m.Expression;
            if (earned.Length > 0) sb.Append("Done so far: ").Append(earned).Append(". ");
            sb.Append(plates
                ? "Share them so every plate matches (deal one round helps), then check."
                : "Pack them so every used box is full, then check.");
            return new GuideStep(id, Fit(sb), true);
        }

        static string Fit(StringBuilder sb)
        {
            string s = sb.ToString();
            return s.Length <= GuideSteps.MaxOnTableLength ? s : s.Substring(0, GuideSteps.MaxOnTableLength);
        }
    }
}
