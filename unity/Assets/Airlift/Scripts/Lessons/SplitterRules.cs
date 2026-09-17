using UnityEngine;

namespace Airlift.Lessons
{
    /// Dock 7 splitter (owner 2026-09-17: splitting only worked by voice). The learner sets a crate on the splitter pad
    /// and chooses a chunk size. Pure decision: only the chapter's size may split, through CargoLessonDirector.TrySplit;
    /// any other choice gets a short hint naming the size the vehicles need. It never judges the learner.
    public static class SplitterRules
    {
        public const string NoCrateReason = "Set a crate on the splitter first.";
        public const string NoSplitReason = "No splitting in this chapter.";
        public const string AlreadySplitReason = "Those crates are already split.";
        /// Highest a released crate may float above the pad and still count as set on it (crates stay where released).
        public const float MaxLift = 0.35f;

        /// Ok means "split now" (the caller calls CargoLessonDirector.TrySplit and reports its result).
        public static LessonActionResult Choose(int chapterSplitTo, int chosenDenominator, bool crateOnPad, bool canSplit, string vehicleNoun)
        {
            if (!crateOnPad) return new LessonActionResult(false, NoCrateReason);
            if (chapterSplitTo <= 0) return new LessonActionResult(false, NoSplitReason);
            if (!canSplit) return new LessonActionResult(false, AlreadySplitReason);
            if (chosenDenominator != chapterSplitTo) return new LessonActionResult(false, Hint(chapterSplitTo, vehicleNoun));
            return new LessonActionResult(true, "");
        }

        /// "Each pickup takes half a container. Cut into halves."
        public static string Hint(int splitTo, string vehicleNoun)
        {
            string noun = string.IsNullOrEmpty(vehicleNoun) ? "vehicle" : vehicleNoun;
            switch (splitTo)
            {
                case 2: return "Each " + noun + " takes half a container. Cut into halves.";
                case 4: return "Each " + noun + " takes a quarter of a container. Cut into quarters.";
                default: return "Each " + noun + " takes 1/" + splitTo + " of a container. Cut into " + splitTo + " equal chunks.";
            }
        }

        public static string ButtonLabel(int denominator) => denominator == 2 ? "Halves · 1/2" : denominator == 4 ? "Quarters · 1/4" : "1/" + denominator;

        /// Station-local point over the pad: inside its footprint and between just under its top and MaxLift above it.
        /// padCenter is the centre of the pad's top surface; padSize is its x and z size.
        public static bool OverPad(Vector3 padCenter, Vector2 padSize, Vector3 point)
        {
            return Mathf.Abs(point.x - padCenter.x) <= padSize.x * 0.5f
                && Mathf.Abs(point.z - padCenter.z) <= padSize.y * 0.5f
                && point.y >= padCenter.y - 0.01f
                && point.y <= padCenter.y + MaxLift;
        }
    }
}
