using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Airlift.Lessons.Cafe
{
    /// <summary>
    /// Deterministic Corner Café chapter engine. Pure C#: no SDK, scene or timing.
    /// Every pastry is a visible item with a fixed id ("item-1".."item-N") that sits on the tray (-1) or in one
    /// container: a plate (sharing) or a box (packing). Placing is neutral; the table is judged on Check only,
    /// and wrong arrangements stay placed and editable. Once a chapter is accepted its pastries are served or
    /// delivered, so nothing moves until the chapter restarts or the next chapter starts.
    /// </summary>
    public sealed class CafeModel
    {
        readonly List<string> ids = new List<string>();
        int[] containerOf = new int[0];

        public CafeChapter Chapter { get; private set; }
        public int ChapterIndex { get; private set; }
        public bool ChapterComplete { get; private set; }
        public bool IsLastChapter => ChapterIndex == CafeChapter.All.Count - 1;
        public bool AllChaptersComplete => IsLastChapter && ChapterComplete;
        public int Generation { get; private set; }
        public int StageIndex { get; private set; }
        public CafeStage Stage => Chapter.Stages[StageIndex];
        public bool IsLastStage => StageIndex == Chapter.Stages.Length - 1;
        /// The sentence to show for what to do now: the stage's own task in a multi-stage chapter.
        public string StageTask => Stage.Task ?? Chapter.Task;
        public IReadOnlyList<string> ItemIds { get; private set; } = new ReadOnlyCollection<string>(new string[0]);
        public int ContainerCount => Stage.Containers;
        public string LastFeedback { get; private set; } = "";

        /// Expression earned so far: "" before any accepted stage, the finished stages' expressions inside a
        /// multi-stage chapter, the whole chapter expression once complete.
        public string Expression
        {
            get
            {
                if (ChapterComplete) return Chapter.Expression;
                var parts = new List<string>();
                for (int s = 0; s < StageIndex; s++) parts.Add(Chapter.Stages[s].Expression ?? "");
                return string.Join(" · ", parts);
            }
        }

        public CafeModel() { StartChapter(0); }

        // ---- queries ----

        /// Container index of an item; -1 when it is on the tray or unknown.
        public int ContainerOf(string itemId)
        {
            int i = IndexOf(itemId);
            return i < 0 ? -1 : containerOf[i];
        }

        public int CountIn(int container)
        {
            if (!ValidContainer(container)) return 0;
            int n = 0;
            foreach (int c in containerOf) if (c == container) n++;
            return n;
        }

        public int LooseCount { get { int n = 0; foreach (int c in containerOf) if (c < 0) n++; return n; } }

        /// Room left in a box; 0 for plates (no limit) or a bad index.
        public int RoomIn(int box) =>
            Stage.Kind == CafeTargetKind.Boxes && ValidContainer(box) ? Stage.BoxCapacity - CountIn(box) : 0;

        /// "Plate 2" / "Box 3".
        public string ContainerName(int container) =>
            (Stage.Kind == CafeTargetKind.Plates ? "Plate " : "Box ") + (container + 1);

        // ---- actions ----

        /// Puts an item onto a plate or into a box, from the tray or from another container. Refuses held, unknown,
        /// a bad container, a full box and a completed chapter. A full box also sets LastFeedback. Placing an item
        /// where it already is returns true and changes nothing.
        public bool Place(string itemId, bool held, int container)
        {
            if (held || ChapterComplete || !ValidContainer(container)) return false;
            int i = IndexOf(itemId);
            if (i < 0) return false;
            if (containerOf[i] == container) return true;
            if (Stage.Kind == CafeTargetKind.Boxes && CountIn(container) >= Stage.BoxCapacity)
            {
                LastFeedback = ContainerName(container) + " is full. It holds " + Stage.BoxCapacity + " " +
                               Chapter.ItemWord(Stage.BoxCapacity) + ".";
                return false;
            }
            containerOf[i] = container;
            Generation++;
            return true;
        }

        /// Puts an item back on the tray. Refuses unknown, already loose and a completed chapter.
        public bool Remove(string itemId)
        {
            if (ChapterComplete) return false;
            int i = IndexOf(itemId);
            if (i < 0 || containerOf[i] < 0) return false;
            containerOf[i] = -1;
            Generation++;
            return true;
        }

        /// Plates stage only: puts one loose item onto each plate, plate 1 first, taking loose items in id order,
        /// and stops when the tray runs out. Refuses held, a completed chapter, a boxes stage (feedback) and an
        /// empty tray (feedback). LastFeedback describes the deal.
        public bool DealRound(bool held)
        {
            if (held || ChapterComplete) return false;
            if (Stage.Kind != CafeTargetKind.Plates)
            {
                LastFeedback = "Dealing a round is for plates. Pack the boxes one " + Chapter.ItemName + " at a time.";
                return false;
            }
            if (LooseCount == 0)
            {
                LastFeedback = "Every " + Chapter.ItemName + " is already on a plate.";
                return false;
            }
            int dealt = 0;
            for (int plate = 0; plate < Stage.Containers; plate++)
            {
                int i = Array.IndexOf(containerOf, -1);
                if (i < 0) break;
                containerOf[i] = plate;
                dealt++;
            }
            Generation++;
            int loose = LooseCount;
            string first = dealt == Stage.Containers
                ? "Each plate got one more " + Chapter.ItemName + "."
                : ListNumbers(Range(1, dealt), "Plate ", "Plates ") + " got one more " + Chapter.ItemName + ".";
            LastFeedback = first + " " + (loose == 0
                ? "The tray is empty now."
                : loose + " " + Chapter.ItemWord(loose) + (loose == 1 ? " is" : " are") + " still on the tray.");
            return true;
        }

        /// Judges the table. Plates: every item placed and all plates equal. Boxes: every item boxed and every used
        /// box exactly full (empty boxes are fine). An accepted intermediate stage moves to the next stage with every
        /// item back on the tray; the last stage completes the chapter. Wrong arrangements stay where they are.
        public bool Check()
        {
            if (ChapterComplete) { LastFeedback = AcceptedFeedback(); return true; }

            int loose = LooseCount;
            if (loose > 0)
            {
                LastFeedback = loose + " " + Chapter.ItemWord(loose) + (loose == 1 ? " is" : " are") + " still on the tray.";
                return false;
            }

            if (Stage.Kind == CafeTargetKind.Plates)
            {
                var counts = new int[Stage.Containers];
                bool equal = true;
                for (int p = 0; p < counts.Length; p++) { counts[p] = CountIn(p); equal &= counts[p] == counts[0]; }
                if (!equal)
                {
                    LastFeedback = "The plates are not equal yet: " + ListNumbers(counts, "", "") + ".";
                    return false;
                }
            }
            else
            {
                for (int b = 0; b < Stage.Containers; b++)
                {
                    int count = CountIn(b), room = Stage.BoxCapacity - count;
                    if (count == 0 || room <= 0) continue;
                    LastFeedback = ContainerName(b) + " has room for " + room + " more " + Chapter.ItemWord(room) + ".";
                    return false;
                }
            }

            if (!IsLastStage)
            {
                string said = (Stage.Accepted ?? Chapter.Accepted) + " " + Stage.Expression;
                StageIndex++;
                for (int i = 0; i < containerOf.Length; i++) containerOf[i] = -1;
                Generation++;
                LastFeedback = said;
                return true;
            }

            ChapterComplete = true;
            LastFeedback = AcceptedFeedback();
            return true;
        }

        /// Every item back on the tray in the current stage. Does nothing once the chapter is accepted
        /// (restart the chapter instead).
        public void ResetTable()
        {
            if (ChapterComplete) return;
            for (int i = 0; i < containerOf.Length; i++) containerOf[i] = -1;
            LastFeedback = "";
            Generation++;
        }

        public void RestartChapter() => StartChapter(ChapterIndex);

        public bool NextChapter()
        {
            if (!ChapterComplete || IsLastChapter) return false;
            StartChapter(ChapterIndex + 1);
            return true;
        }

        public void StartChapter(int index)
        {
            if (index < 0 || index >= CafeChapter.All.Count) throw new ArgumentOutOfRangeException(nameof(index));
            var chapter = CafeChapter.All[index];
            if (chapter.Stages == null || chapter.Stages.Length == 0 || chapter.Items <= 0)
                throw new InvalidOperationException("Chapter " + chapter.Id + " needs items and at least one stage.");
            ChapterIndex = index;
            Chapter = chapter;
            StageIndex = 0;
            ChapterComplete = false;
            LastFeedback = "";
            ids.Clear();
            for (int n = 1; n <= chapter.Items; n++) ids.Add("item-" + n);
            ItemIds = new ReadOnlyCollection<string>(ids.ToArray());
            containerOf = new int[chapter.Items];
            for (int i = 0; i < containerOf.Length; i++) containerOf[i] = -1;
            Generation++;
        }

        // ---- helpers ----

        string AcceptedFeedback() => Chapter.Accepted + " " + Chapter.Expression;

        bool ValidContainer(int container) => container >= 0 && container < Stage.Containers;

        int IndexOf(string itemId) => itemId == null ? -1 : ids.IndexOf(itemId);

        static int[] Range(int from, int count)
        {
            var r = new int[count];
            for (int i = 0; i < count; i++) r[i] = from + i;
            return r;
        }

        /// "4, 4 and 3"; with prefixes: "Plate 1", "Plates 1 and 2".
        static string ListNumbers(int[] numbers, string one, string many)
        {
            var sb = new StringBuilder(numbers.Length == 1 ? one : many);
            for (int i = 0; i < numbers.Length; i++)
            {
                if (i > 0) sb.Append(i == numbers.Length - 1 ? " and " : ", ");
                sb.Append(numbers[i]);
            }
            return sb.ToString();
        }
    }
}
