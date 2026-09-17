using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Airlift.Lessons.Cafe
{
    /// Where pastries go in a stage: plates share them out (partitive division), boxes pack them (quotative).
    public enum CafeTargetKind { Plates, Boxes }

    /// One step of a café chapter. Chapters 1-4 have one stage; chapter 5 shares onto plates, then packs boxes.
    public sealed class CafeStage
    {
        public CafeTargetKind Kind;
        public int Containers;           // plates on the counter, or boxes set out (more boxes than needed)
        public int BoxCapacity;          // pastries per full box; 0 for plates

        // Only used by multi-stage chapters; single-stage chapters leave these null and use the chapter's copy.
        public string Task;              // one sentence for this stage
        public string Expression;        // earned when this stage is accepted
        public string Accepted;          // said when an intermediate stage is accepted
    }

    /// <summary>
    /// One Corner Café chapter: story copy, the pastries in play and the plates or boxes they go to. Pure data.
    /// Every quantity is a visible pastry; the division is read from the finished table.
    /// </summary>
    public sealed class CafeChapter
    {
        public int Number;               // 1..5
        public string Id;                // "two_friends" | "table_of_three" | "box_it_up" | "bigger_order" | "fact_family"
        public string Title;             // "Two friends"
        public string Story;             // one or two sentences of café story
        public string Task;              // one sentence: what to do in this chapter
        public string ItemName;          // "croissant"
        public string ItemPlural;        // "croissants"
        public int Items;                // pastries in play for the whole chapter
        public CafeStage[] Stages;       // chapters 1-4: one stage; chapter 5: plates then boxes
        public string Expression;        // the whole chapter's expression, shown once the chapter is accepted
        public string Accepted;          // one-sentence payoff when the last stage is accepted

        /// Story for the lesson briefing card, before chapter 1. CafeStation puts its welcome line ("Welcome to
        /// the Corner Café.") in front, so this never names the café again.
        public const string BriefingStory =
            "Dee is the head barista this morning. Guests share pastries on plates, and orders go out in full boxes.";

        public string ItemWord(int count) => count == 1 ? ItemName : ItemPlural;

        static readonly ReadOnlyCollection<CafeChapter> all = new ReadOnlyCollection<CafeChapter>(new[]
        {
            new CafeChapter
            {
                Number = 1, Id = "two_friends", Title = "Two friends",
                Story = "Two friends sat down at the window table. Dee baked 6 croissants for them to share.",
                Task = "Put all 6 croissants on the 2 plates so both plates match, then check.",
                ItemName = "croissant", ItemPlural = "croissants", Items = 6,
                Stages = new[] { new CafeStage { Kind = CafeTargetKind.Plates, Containers = 2 } },
                Expression = "6 ÷ 2 = 3",
                Accepted = "Both plates go to the window table with 3 croissants each.",
            },
            new CafeChapter
            {
                Number = 2, Id = "table_of_three", Title = "Table of three",
                Story = "Three guests sat down at the big table. Dee has a tray of 12 pastries for them.",
                Task = "Share all 12 pastries onto the 3 plates so every plate matches, dealing one round at a time if you like, then check.",
                ItemName = "pastry", ItemPlural = "pastries", Items = 12,
                Stages = new[] { new CafeStage { Kind = CafeTargetKind.Plates, Containers = 3 } },
                Expression = "12 ÷ 3 = 4",
                Accepted = "Each of the 3 guests gets a plate with 4 pastries.",
            },
            new CafeChapter
            {
                Number = 3, Id = "box_it_up", Title = "Box it up",
                Story = "A phone order came in for 12 cookies, 4 to a box. Dee set out 5 empty boxes, more than the order needs.",
                Task = "Pack all 12 cookies so every box you use is full, then check.",
                ItemName = "cookie", ItemPlural = "cookies", Items = 12,
                Stages = new[] { new CafeStage { Kind = CafeTargetKind.Boxes, Containers = 5, BoxCapacity = 4 } },
                Expression = "12 ÷ 4 = 3",
                Accepted = "3 full boxes of cookies go out on the delivery bike.",
            },
            new CafeChapter
            {
                Number = 4, Id = "bigger_order", Title = "Bigger order",
                Story = "A bigger order came in for 15 muffins, 5 to a box. Dee set out 5 empty boxes on the counter.",
                Task = "Pack all 15 muffins so every box you use is full, then check.",
                ItemName = "muffin", ItemPlural = "muffins", Items = 15,
                Stages = new[] { new CafeStage { Kind = CafeTargetKind.Boxes, Containers = 5, BoxCapacity = 5 } },
                Expression = "15 ÷ 5 = 3",
                Accepted = "3 full boxes of muffins go out on the delivery bike.",
            },
            new CafeChapter
            {
                Number = 5, Id = "fact_family", Title = "Fact family",
                Story = "Dee baked 12 muffins. First 4 guests share them, then the same 12 go out as an order in boxes of 3.",
                Task = "Share the 12 muffins onto 4 plates, then pack the same muffins into full boxes of 3.",
                ItemName = "muffin", ItemPlural = "muffins", Items = 12,
                Stages = new[]
                {
                    new CafeStage
                    {
                        Kind = CafeTargetKind.Plates, Containers = 4,
                        Task = "Share all 12 muffins onto the 4 plates so every plate matches, then check.",
                        Expression = "12 ÷ 4 = 3",
                        Accepted = "Each of the 4 plates held 3 muffins. The muffins are back on the tray for the boxed order.",
                    },
                    new CafeStage
                    {
                        Kind = CafeTargetKind.Boxes, Containers = 6, BoxCapacity = 3,
                        Task = "Pack the same 12 muffins so every box you use is full, then check.",
                        Expression = "12 ÷ 3 = 4 · 3 × 4 = 12",
                    },
                },
                Expression = "12 ÷ 4 = 3 · 12 ÷ 3 = 4 · 3 × 4 = 12",
                Accepted = "4 full boxes of 3 muffins go out on the delivery bike, the same 12 muffins the guests shared.",
            },
        });

        /// The five Corner Café chapters in order.
        public static IReadOnlyList<CafeChapter> All => all;
    }
}
