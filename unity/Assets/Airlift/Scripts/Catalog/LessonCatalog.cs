using System.Linq;

namespace Airlift.Welcome
{
    public sealed class LessonCard
    {
        public string Id, Title, Subject, Description;
        public bool Playable;
        public string[] Facts;
    }

    /// The three owner-approved cards, all playable: Cargo Crew tells the Dock 7 story (PHASE-1R-DOCK-CREW.md), Neighborhood
    /// Café the Corner Café story (CC-CF-04) and Community Garden the Sunny Plot story (CC-GD-04).
    public static class LessonCatalog
    {
        public static readonly LessonCard[] Cards =
        {
            new LessonCard { Id = "cargo_crew_fractions", Title = "Cargo Crew", Subject = "Fractions", Playable = true,
                Description = "Dock 7: split crates into equal chunks so every vehicle leaves loaded.",
                Facts = new[] {
                    "Cargo Crew is a fractions lesson set at Dock 7, a busy harbor, on a toy cargo table.",
                    "The learner is the load planner: cranes unload full crates from the ship, and the learner splits crates into equal chunks so each vehicle gets its share of one container.",
                    "Trucks, pickups and vans back up to the dock in front of the learner. Their beds sit side by side over a ruler from 0 to 1 that stands for one whole container, so equal chunks line up.",
                    "Crates load straight into the beds. After the app accepts a load, the vehicles drive away with their crates.",
                    "Five chapters: Big truck (one full container), Two pickups (halves), Four vans (quarters), Same share with smaller boxes (2/4 = 1/2), and Top it up (1/2 + 1/4 + 1/4 = 1).",
                    "It works by voice: split it, load it, put everything back, next chapter, start this chapter over, show me the demo, back to the lessons.",
                    "The yellow handle at the front moves the whole table; two hands resize it." } },
            new LessonCard { Id = "neighborhood_cafe_division", Title = "Neighborhood Café", Subject = "Division", Playable = true,
                Description = "Corner Café: share pastries on plates and pack orders into full boxes.",
                Facts = new[] {
                    "Neighborhood Café is a division lesson set at the Corner Café, a small neighborhood café on a busy morning, on a toy café table.",
                    "The learner helps run the café and Dee is the head barista: pastries come out of the oven, guests share them on plates, and orders go out in boxes.",
                    "Sharing onto plates: every pastry goes on a plate and every plate holds the same number. Packing into boxes: every pastry goes in a box and every box used is full; the number of boxes is the answer.",
                    "Five chapters: Two friends (6 ÷ 2 = 3), Table of three (12 ÷ 3 = 4), Box it up (12 ÷ 4 = 3), Bigger order (15 ÷ 5 = 3), and Fact family, where the same 12 muffins are shared onto 4 plates and packed into boxes of 3 (12 ÷ 4 = 3, 12 ÷ 3 = 4, 3 × 4 = 12).",
                    "The app checks an order only when asked. Served plates slide to the guest table and full boxes go out on the delivery bike.",
                    "It works by voice: deal a round, check the order, clear the table, next chapter, start this chapter over, back to the lessons.",
                    "The yellow handle at the front moves the whole table; two hands resize it." } },
            new LessonCard { Id = "community_garden_multiplication", Title = "Community Garden", Subject = "Multiplication", Playable = true,
                Description = "Sunny Plot: plant equal rows, turn the bed and split it with a fence.",
                Facts = new[] {
                    "Community Garden is a multiplication lesson set at Sunny Plot, the neighborhood's shared garden, on a toy garden table.",
                    "The learner plants the garden and Dee is the head gardener: seedlings come in strips, and each strip plants one whole row of a bed.",
                    "Beds are always read as rows × columns: a 3 × 4 bed is 3 rows of 4. A bed is accepted when every row is planted with the same number of seedlings.",
                    "Five chapters: First rows (3 × 4 = 12), Equal rows (4 × 5 = 20), Turn the bed (3 × 4 = 4 × 3 = 12), Split the bed with a fence (7 × 6 = 7 × 5 + 7 × 1 = 42), and Your own split, where the learner chooses where the fence goes in an 8 × 7 bed.",
                    "The app checks a bed only when asked. Accepted rows sprout into lettuce, carrots, sunflowers and beans.",
                    "It works by voice: turn the bed, split it at five, check the bed, clear the bed, next chapter, start this chapter over, back to the lessons.",
                    "The yellow handle at the front moves the whole table; two hands resize it." } }
        };

        public static LessonCard Find(string id) => Cards.FirstOrDefault(c => c.Id == id);
        public static bool IsPlayable(string id) => Find(id)?.Playable == true;
        public static int IndexOf(string id) { for (int i = 0; i < Cards.Length; i++) if (Cards[i].Id == id) return i; return Cards.Length; }

        /// "Cargo Crew is ready", "Cargo Crew and Neighborhood Café are ready": the playable lessons, for spoken lines.
        public static string ReadyPhrase()
        {
            var titles = Cards.Where(c => c.Playable).Select(c => c.Title).ToArray();
            if (titles.Length == 0) return "no lesson is ready yet";
            return JoinAnd(titles) + (titles.Length == 1 ? " is ready" : " are ready");
        }

        /// The catalog context note: which cards can be opened and which are previews.
        public static string CatalogNote()
        {
            var playable = Cards.Where(c => c.Playable).Select(c => c.Title + " (" + c.Subject.ToLowerInvariant() + ")").ToArray();
            var previews = Cards.Where(c => !c.Playable).Select(c => c.Title).ToArray();
            string count = Cards.Length == 3 ? "Three" : Cards.Length.ToString();
            string opened = previews.Length == 0 ? "All of them can be opened: " + JoinAnd(playable) + "."
                : playable.Length == 1 && previews.Length == 2 ? "Only " + playable[0] + " can be opened; the other two are previews."
                : playable.Length == 0 ? "None can be opened yet; they are previews."
                : JoinAnd(playable) + " can be opened; " + JoinAnd(previews) + (previews.Length == 1 ? " is a preview." : " are previews.");
            return count + " lesson cards are in front of the learner. " + opened;
        }

        static string JoinAnd(string[] items) => items.Length <= 1 ? string.Join("", items) : string.Join(", ", items, 0, items.Length - 1) + " and " + items[items.Length - 1];

        /// Tool output for describe_card: facts only, from code.
        public static string DescribeJson(string id)
        {
            var c = Find(id);
            if (c == null) return "{\"error\":\"unknown card\"}";
            var facts = new Newtonsoft.Json.Linq.JArray(c.Facts);
            return new Newtonsoft.Json.Linq.JObject { ["title"] = c.Title, ["subject"] = c.Subject, ["playable"] = c.Playable, ["facts"] = facts }.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}
