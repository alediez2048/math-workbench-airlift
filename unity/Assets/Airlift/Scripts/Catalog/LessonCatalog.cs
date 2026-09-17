using System.Linq;

namespace Airlift.Welcome
{
    public sealed class LessonCard
    {
        public string Id, Title, Subject, Description;
        public bool Playable;
        public string[] Facts;
    }

    /// The three owner-approved cards. Cargo Crew tells the Dock 7 story (PHASE-1R-DOCK-CREW.md). Only Cargo Crew is an implemented lesson; the others are
    /// honest previews and never launch anything.
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
            new LessonCard { Id = "neighborhood_cafe_division", Title = "Neighborhood Café", Subject = "Division", Playable = false,
                Description = "Coming soon: share orders fairly at a busy café.",
                Facts = new[] { "Neighborhood Café is a division lesson that is not available yet." } },
            new LessonCard { Id = "community_garden_multiplication", Title = "Community Garden", Subject = "Multiplication", Playable = false,
                Description = "Coming soon: plant rows and columns to grow multiplication.",
                Facts = new[] { "Community Garden is a multiplication lesson that is not available yet." } }
        };

        public static LessonCard Find(string id) => Cards.FirstOrDefault(c => c.Id == id);
        public static bool IsPlayable(string id) => Find(id)?.Playable == true;

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
