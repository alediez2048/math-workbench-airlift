using Airlift.Lessons.Cafe;
using NUnit.Framework;

namespace Airlift.Tests
{
    public class CafeModelTests
    {
        const int TwoFriends = 0, TableOfThree = 1, BoxItUp = 2, BiggerOrder = 3, FactFamily = 4;

        static CafeModel At(int index) { var m = new CafeModel(); m.StartChapter(index); return m; }

        static void PlaceRange(CafeModel m, int first, int last, int container)
        {
            for (int i = first; i <= last; i++) Assert.That(m.Place("item-" + i, false, container), Is.True, "item-" + i);
        }

        // ---- start state ----

        [Test] public void StartsAtTwoFriendsWithEverythingOnTheTray()
        {
            var m = new CafeModel();
            Assert.That(m.ChapterIndex, Is.EqualTo(TwoFriends));
            Assert.That(m.Chapter.Id, Is.EqualTo("two_friends"));
            Assert.That(m.StageIndex, Is.EqualTo(0));
            Assert.That(m.Stage.Kind, Is.EqualTo(CafeTargetKind.Plates));
            Assert.That(m.ItemIds, Is.EqualTo(new[] { "item-1", "item-2", "item-3", "item-4", "item-5", "item-6" }));
            Assert.That(m.ContainerCount, Is.EqualTo(2));
            Assert.That(m.LooseCount, Is.EqualTo(6));
            foreach (var id in m.ItemIds) Assert.That(m.ContainerOf(id), Is.EqualTo(-1));
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.AllChaptersComplete, Is.False);
            Assert.That(m.IsLastChapter, Is.False);
            Assert.That(m.Expression, Is.EqualTo(""));
            Assert.That(m.LastFeedback, Is.EqualTo(""));
            Assert.That(m.StageTask, Is.EqualTo(m.Chapter.Task));
        }

        [Test] public void EveryChapterStartsWithStableItemIdsAndItsFirstStage()
        {
            var m = new CafeModel();
            for (int c = 0; c < CafeChapter.All.Count; c++)
            {
                m.StartChapter(c);
                var chapter = CafeChapter.All[c];
                Assert.That(m.ItemIds.Count, Is.EqualTo(chapter.Items), chapter.Id);
                for (int i = 0; i < chapter.Items; i++) Assert.That(m.ItemIds[i], Is.EqualTo("item-" + (i + 1)), chapter.Id);
                Assert.That(m.StageIndex, Is.EqualTo(0), chapter.Id);
                Assert.That(m.ContainerCount, Is.EqualTo(chapter.Stages[0].Containers), chapter.Id);
                Assert.That(m.LooseCount, Is.EqualTo(chapter.Items), chapter.Id);
                for (int k = 0; k < m.ContainerCount; k++) Assert.That(m.CountIn(k), Is.EqualTo(0), chapter.Id);
            }
        }

        [Test] public void StartChapterOutOfRangeThrows()
        {
            var m = new CafeModel();
            Assert.Throws<System.ArgumentOutOfRangeException>(() => m.StartChapter(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => m.StartChapter(5));
        }

        // ---- place and remove ----

        [Test] public void PlaceRefusesHeldUnknownAndBadContainers()
        {
            var m = new CafeModel();
            int g = m.Generation;
            Assert.That(m.Place("item-1", true, 0), Is.False, "held");
            Assert.That(m.Place("item-7", false, 0), Is.False, "unknown id");
            Assert.That(m.Place(null, false, 0), Is.False, "null id");
            Assert.That(m.Place("item-1", false, -1), Is.False, "negative container");
            Assert.That(m.Place("item-1", false, 2), Is.False, "only 2 plates");
            Assert.That(m.Generation, Is.EqualTo(g), "refusals move nothing");
            Assert.That(m.LooseCount, Is.EqualTo(6));
        }

        [Test] public void PlaceMovesBetweenContainersAndBumpsGeneration()
        {
            var m = new CafeModel();
            int g = m.Generation;
            Assert.That(m.Place("item-1", false, 0), Is.True);
            Assert.That(m.Generation, Is.EqualTo(g + 1));
            Assert.That(m.ContainerOf("item-1"), Is.EqualTo(0));
            Assert.That(m.CountIn(0), Is.EqualTo(1));
            Assert.That(m.LooseCount, Is.EqualTo(5));
            Assert.That(m.Place("item-1", false, 0), Is.True, "already there");
            Assert.That(m.Generation, Is.EqualTo(g + 1), "no move, no bump");
            Assert.That(m.Place("item-1", false, 1), Is.True, "plate to plate");
            Assert.That(m.CountIn(0), Is.EqualTo(0));
            Assert.That(m.CountIn(1), Is.EqualTo(1));
            Assert.That(m.Generation, Is.EqualTo(g + 2));
            Assert.That(m.CountIn(2), Is.EqualTo(0), "bad container counts nothing");
            Assert.That(m.ContainerOf("nope"), Is.EqualTo(-1));
        }

        [Test] public void RemoveReturnsToTheTray()
        {
            var m = new CafeModel();
            m.Place("item-2", false, 1);
            int g = m.Generation;
            Assert.That(m.Remove("item-2"), Is.True);
            Assert.That(m.ContainerOf("item-2"), Is.EqualTo(-1));
            Assert.That(m.Generation, Is.EqualTo(g + 1));
            Assert.That(m.Remove("item-2"), Is.False, "already on the tray");
            Assert.That(m.Remove("item-99"), Is.False);
            Assert.That(m.Remove(null), Is.False);
            Assert.That(m.Generation, Is.EqualTo(g + 1));
        }

        [Test] public void FullBoxRefusesAnotherItemAndSaysSo()
        {
            var m = At(BoxItUp);
            PlaceRange(m, 1, 4, 1);
            int g = m.Generation;
            Assert.That(m.Place("item-5", false, 1), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Box 2 is full. It holds 4 cookies."));
            Assert.That(m.ContainerOf("item-5"), Is.EqualTo(-1));
            Assert.That(m.Generation, Is.EqualTo(g));
            Assert.That(m.RoomIn(1), Is.EqualTo(0));
            Assert.That(m.RoomIn(0), Is.EqualTo(4));
            Assert.That(m.Place("item-1", false, 1), Is.True, "an item already in the full box can be dropped back in");
            Assert.That(m.Place("item-1", false, 0), Is.True, "and moved out");
            Assert.That(m.Place("item-5", false, 1), Is.True, "which makes room");
        }

        [Test] public void PlatesHaveNoCapacity()
        {
            var m = At(TableOfThree);
            PlaceRange(m, 1, 12, 0);
            Assert.That(m.CountIn(0), Is.EqualTo(12));
            Assert.That(m.RoomIn(0), Is.EqualTo(0), "RoomIn is for boxes only");
        }

        // ---- deal one round ----

        [Test] public void DealRoundPutsOneLooseItemOnEachPlateInOrder()
        {
            var m = At(TableOfThree);
            m.Place("item-1", false, 2);
            int g = m.Generation;
            Assert.That(m.DealRound(false), Is.True);
            Assert.That(m.Generation, Is.EqualTo(g + 1));
            Assert.That(m.ContainerOf("item-2"), Is.EqualTo(0));
            Assert.That(m.ContainerOf("item-3"), Is.EqualTo(1));
            Assert.That(m.ContainerOf("item-4"), Is.EqualTo(2));
            Assert.That(m.ContainerOf("item-1"), Is.EqualTo(2), "placed items stay put");
            Assert.That(new[] { m.CountIn(0), m.CountIn(1), m.CountIn(2) }, Is.EqualTo(new[] { 1, 1, 2 }));
            Assert.That(m.LooseCount, Is.EqualTo(8));
            Assert.That(m.LastFeedback, Is.EqualTo("Each plate got one more pastry. 8 pastries are still on the tray."));
        }

        [Test] public void DealRoundStopsWhenItemsRunOut()
        {
            var m = At(TableOfThree);
            PlaceRange(m, 1, 10, 0);
            Assert.That(m.DealRound(false), Is.True);
            Assert.That(m.ContainerOf("item-11"), Is.EqualTo(0));
            Assert.That(m.ContainerOf("item-12"), Is.EqualTo(1));
            Assert.That(m.CountIn(2), Is.EqualTo(0));
            Assert.That(m.LooseCount, Is.EqualTo(0));
            Assert.That(m.LastFeedback, Is.EqualTo("Plates 1 and 2 got one more pastry. The tray is empty now."));

            m.Remove("item-12");
            Assert.That(m.DealRound(false), Is.True);
            Assert.That(m.LastFeedback, Is.EqualTo("Plate 1 got one more pastry. The tray is empty now."));
        }

        [Test] public void DealRoundLeavesOneLooseItemFeedbackSingular()
        {
            var m = new CafeModel();
            PlaceRange(m, 1, 3, 0);
            Assert.That(m.DealRound(false), Is.True);
            Assert.That(m.LastFeedback, Is.EqualTo("Each plate got one more croissant. 1 croissant is still on the tray."));
        }

        [Test] public void DealRoundsAloneShareEvenly()
        {
            var m = At(TableOfThree);
            int rounds = 0;
            while (m.DealRound(false)) rounds++;
            Assert.That(rounds, Is.EqualTo(4));
            Assert.That(m.LastFeedback, Is.EqualTo("Every pastry is already on a plate."));
            Assert.That(m.Check(), Is.True);
            Assert.That(m.Expression, Is.EqualTo("12 ÷ 3 = 4"));
        }

        [Test] public void DealRoundRefusals()
        {
            var m = new CafeModel();
            int g = m.Generation;
            Assert.That(m.DealRound(true), Is.False, "held");
            Assert.That(m.LooseCount, Is.EqualTo(6));

            var boxes = At(BoxItUp);
            int bg = boxes.Generation;
            Assert.That(boxes.DealRound(false), Is.False, "boxes are packed, not dealt");
            Assert.That(boxes.LastFeedback, Is.EqualTo("Dealing a round is for plates. Pack the boxes one cookie at a time."));
            Assert.That(boxes.LooseCount, Is.EqualTo(12));
            Assert.That(boxes.Generation, Is.EqualTo(bg));

            PlaceRange(m, 1, 6, 1);
            g = m.Generation;
            Assert.That(m.DealRound(false), Is.False, "nothing loose");
            Assert.That(m.LastFeedback, Is.EqualTo("Every croissant is already on a plate."));
            Assert.That(m.Generation, Is.EqualTo(g));
        }

        // ---- check: plates ----

        [Test] public void PlatesCheckNamesLooseItems()
        {
            var m = new CafeModel();
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("6 croissants are still on the tray."));
            PlaceRange(m, 1, 4, 0);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("2 croissants are still on the tray."));
            m.Place("item-5", false, 1);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("1 croissant is still on the tray."));
            Assert.That(m.ChapterComplete, Is.False);
        }

        [Test] public void UnequalPlatesStayPlacedAndAreListedInOrder()
        {
            var m = At(TableOfThree);
            PlaceRange(m, 1, 4, 0);
            PlaceRange(m, 5, 8, 1);
            PlaceRange(m, 9, 11, 2);
            m.Place("item-12", false, 1);
            int g = m.Generation;
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("The plates are not equal yet: 4, 5 and 3."));
            Assert.That(m.Generation, Is.EqualTo(g), "a wrong check moves nothing");
            Assert.That(m.ContainerOf("item-12"), Is.EqualTo(1), "wrong arrangements stay placed");
            Assert.That(m.Expression, Is.EqualTo(""));

            m.Place("item-12", false, 2);
            Assert.That(m.Check(), Is.True);
            Assert.That(m.ChapterComplete, Is.True);
            Assert.That(m.LastFeedback, Is.EqualTo(m.Chapter.Accepted + " 12 ÷ 3 = 4"));
        }

        [Test] public void TwoPlatesListWithAnd()
        {
            var m = new CafeModel();
            PlaceRange(m, 1, 4, 0);
            PlaceRange(m, 5, 6, 1);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("The plates are not equal yet: 4 and 2."));
        }

        [Test] public void TwoFriendsAccepted()
        {
            var m = new CafeModel();
            PlaceRange(m, 1, 3, 1);
            PlaceRange(m, 4, 6, 0);
            Assert.That(m.Check(), Is.True);
            Assert.That(m.ChapterComplete, Is.True);
            Assert.That(m.Expression, Is.EqualTo("6 ÷ 2 = 3"));
            Assert.That(m.LastFeedback, Is.EqualTo("Both plates go to the window table with 3 croissants each. 6 ÷ 2 = 3"));
        }

        // ---- check: boxes ----

        [Test] public void BoxesCheckNamesLooseItemsFirst()
        {
            var m = At(BoxItUp);
            PlaceRange(m, 1, 4, 0);
            PlaceRange(m, 5, 6, 1);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("6 cookies are still on the tray."), "loose items before part boxes");
        }

        [Test] public void PartBoxSaysHowMuchRoomIsLeft()
        {
            var m = At(BoxItUp);
            PlaceRange(m, 1, 4, 0);
            PlaceRange(m, 5, 7, 1);
            PlaceRange(m, 8, 11, 2);
            Assert.That(m.CountIn(2), Is.EqualTo(4));
            Assert.That(m.LooseCount, Is.EqualTo(1));
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("1 cookie is still on the tray."));
            m.Place("item-12", false, 3);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Box 2 has room for 1 more cookie."));
            Assert.That(m.ContainerOf("item-12"), Is.EqualTo(3), "wrong packing stays placed");
            m.Remove("item-7");
            m.Place("item-7", false, 3);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Box 2 has room for 2 more cookies."));
        }

        [Test] public void ExtraEmptyBoxesAreFineAndAnyFullBoxesCount()
        {
            var m = At(BoxItUp);
            PlaceRange(m, 1, 4, 4);
            PlaceRange(m, 5, 8, 0);
            PlaceRange(m, 9, 12, 2);
            Assert.That(m.CountIn(1), Is.EqualTo(0));
            Assert.That(m.CountIn(3), Is.EqualTo(0));
            Assert.That(m.Check(), Is.True);
            Assert.That(m.ChapterComplete, Is.True);
            Assert.That(m.LastFeedback, Is.EqualTo("3 full boxes of cookies go out on the delivery bike. 12 ÷ 4 = 3"));
        }

        [Test] public void BiggerOrderPacksFifteenIntoThreeBoxes()
        {
            var m = At(BiggerOrder);
            PlaceRange(m, 1, 5, 0);
            PlaceRange(m, 6, 10, 1);
            PlaceRange(m, 11, 14, 2);
            m.Place("item-15", false, 4);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Box 3 has room for 1 more muffin."));
            m.Place("item-15", false, 2);
            Assert.That(m.Check(), Is.True);
            Assert.That(m.Expression, Is.EqualTo("15 ÷ 5 = 3"));
        }

        // ---- chapter 5: two stages ----

        [Test] public void FactFamilyPlatesThenBoxes()
        {
            var m = At(FactFamily);
            Assert.That(m.IsLastChapter, Is.True);
            Assert.That(m.Stage.Kind, Is.EqualTo(CafeTargetKind.Plates));
            Assert.That(m.ContainerCount, Is.EqualTo(4));
            Assert.That(m.StageTask, Is.EqualTo(m.Chapter.Stages[0].Task));
            while (m.DealRound(false)) { }
            m.Place("item-1", false, 3);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("The plates are not equal yet: 2, 3, 3 and 4."));
            m.Place("item-1", false, 0);

            int g = m.Generation;
            Assert.That(m.Check(), Is.True, "plates accepted");
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.AllChaptersComplete, Is.False);
            Assert.That(m.StageIndex, Is.EqualTo(1));
            Assert.That(m.Stage.Kind, Is.EqualTo(CafeTargetKind.Boxes));
            Assert.That(m.ContainerCount, Is.EqualTo(6));
            Assert.That(m.Stage.BoxCapacity, Is.EqualTo(3));
            Assert.That(m.StageTask, Is.EqualTo(m.Chapter.Stages[1].Task));
            Assert.That(m.Generation, Is.GreaterThan(g), "moving to the boxes moves every item");
            Assert.That(m.LooseCount, Is.EqualTo(12), "every muffin is back on the tray");
            Assert.That(m.ItemIds.Count, Is.EqualTo(12), "the same muffins");
            for (int b = 0; b < 6; b++) Assert.That(m.CountIn(b), Is.EqualTo(0));
            Assert.That(m.Expression, Is.EqualTo("12 ÷ 4 = 3"));
            Assert.That(m.LastFeedback, Is.EqualTo(m.Chapter.Stages[0].Accepted + " 12 ÷ 4 = 3"));
            Assert.That(m.NextChapter(), Is.False);
            Assert.That(m.DealRound(false), Is.False, "boxes stage");

            PlaceRange(m, 1, 3, 5);
            PlaceRange(m, 4, 6, 0);
            PlaceRange(m, 7, 9, 2);
            PlaceRange(m, 10, 11, 1);
            Assert.That(m.Place("item-12", false, 5), Is.False, "box 6 is full");
            m.Place("item-12", false, 3);
            Assert.That(m.Check(), Is.False);
            Assert.That(m.LastFeedback, Is.EqualTo("Box 2 has room for 1 more muffin."));
            Assert.That(m.StageIndex, Is.EqualTo(1));
            m.Place("item-12", false, 1);

            Assert.That(m.Check(), Is.True, "boxes accepted");
            Assert.That(m.ChapterComplete, Is.True);
            Assert.That(m.AllChaptersComplete, Is.True);
            Assert.That(m.StageIndex, Is.EqualTo(1));
            Assert.That(m.Expression, Is.EqualTo("12 ÷ 4 = 3 · 12 ÷ 3 = 4 · 3 × 4 = 12"));
            Assert.That(m.LastFeedback, Is.EqualTo(m.Chapter.Accepted + " 12 ÷ 4 = 3 · 12 ÷ 3 = 4 · 3 × 4 = 12"));
            Assert.That(m.NextChapter(), Is.False, "last chapter");
        }

        [Test] public void FactFamilyRestartGoesBackToThePlates()
        {
            var m = At(FactFamily);
            while (m.DealRound(false)) { }
            Assert.That(m.Check(), Is.True);
            m.Place("item-1", false, 0);
            int g = m.Generation;
            m.RestartChapter();
            Assert.That(m.Generation, Is.GreaterThan(g));
            Assert.That(m.StageIndex, Is.EqualTo(0));
            Assert.That(m.Stage.Kind, Is.EqualTo(CafeTargetKind.Plates));
            Assert.That(m.LooseCount, Is.EqualTo(12));
            Assert.That(m.Expression, Is.EqualTo(""));
            Assert.That(m.LastFeedback, Is.EqualTo(""));
        }

        [Test] public void FactFamilyResetTableStaysInTheBoxesStage()
        {
            var m = At(FactFamily);
            while (m.DealRound(false)) { }
            m.Check();
            PlaceRange(m, 1, 3, 0);
            m.ResetTable();
            Assert.That(m.StageIndex, Is.EqualTo(1));
            Assert.That(m.LooseCount, Is.EqualTo(12));
            Assert.That(m.Expression, Is.EqualTo("12 ÷ 4 = 3"), "an accepted stage stays earned");
        }

        // ---- reset, restart, next ----

        [Test] public void ResetTableReturnsEverythingToTheTray()
        {
            var m = At(TableOfThree);
            m.DealRound(false);
            m.Check();
            Assert.That(m.LastFeedback, Is.Not.Empty);
            int g = m.Generation;
            m.ResetTable();
            Assert.That(m.LooseCount, Is.EqualTo(12));
            Assert.That(m.LastFeedback, Is.EqualTo(""));
            Assert.That(m.Generation, Is.EqualTo(g + 1));
            Assert.That(m.ChapterIndex, Is.EqualTo(TableOfThree));
        }

        [Test] public void AcceptedChapterIsServedAndFrozenUntilRestartOrNext()
        {
            var m = new CafeModel();
            while (m.DealRound(false)) { }
            Assert.That(m.Check(), Is.True);
            string accepted = m.LastFeedback;
            int g = m.Generation;
            Assert.That(m.Place("item-1", false, 1), Is.False);
            Assert.That(m.Remove("item-1"), Is.False);
            Assert.That(m.DealRound(false), Is.False);
            m.ResetTable();
            Assert.That(m.Generation, Is.EqualTo(g), "served pastries do not move");
            Assert.That(m.LooseCount, Is.EqualTo(0));
            Assert.That(m.Check(), Is.True, "checking again keeps the verdict");
            Assert.That(m.LastFeedback, Is.EqualTo(accepted));

            m.RestartChapter();
            Assert.That(m.ChapterIndex, Is.EqualTo(TwoFriends));
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.LooseCount, Is.EqualTo(6));
            Assert.That(m.Generation, Is.GreaterThan(g));
            Assert.That(m.Place("item-1", false, 1), Is.True);
        }

        [Test] public void NextChapterOnlyWhenComplete()
        {
            var m = new CafeModel();
            Assert.That(m.NextChapter(), Is.False);
            Assert.That(m.ChapterIndex, Is.EqualTo(TwoFriends));
            while (m.DealRound(false)) { }
            m.Check();
            int g = m.Generation;
            Assert.That(m.NextChapter(), Is.True);
            Assert.That(m.ChapterIndex, Is.EqualTo(TableOfThree));
            Assert.That(m.ChapterComplete, Is.False);
            Assert.That(m.Generation, Is.GreaterThan(g));
            Assert.That(m.LooseCount, Is.EqualTo(12));
            Assert.That(m.Expression, Is.EqualTo(""));
            Assert.That(m.LastFeedback, Is.EqualTo(""));
        }

        [Test] public void WholeLessonRunsInOrderAndEndsComplete()
        {
            var m = new CafeModel();
            for (int c = 0; c < CafeChapter.All.Count; c++)
            {
                Assert.That(m.ChapterIndex, Is.EqualTo(c));
                for (int s = 0; s < m.Chapter.Stages.Length; s++)
                {
                    var stage = m.Stage;
                    int per = stage.Kind == CafeTargetKind.Plates ? m.Chapter.Items / stage.Containers : stage.BoxCapacity;
                    int i = 0;
                    foreach (var id in m.ItemIds) Assert.That(m.Place(id, false, i++ / per), Is.True, m.Chapter.Id);
                    Assert.That(m.Check(), Is.True, m.Chapter.Id + ": " + m.LastFeedback);
                }
                Assert.That(m.ChapterComplete, Is.True);
                Assert.That(m.Expression, Is.EqualTo(m.Chapter.Expression));
                Assert.That(m.AllChaptersComplete, Is.EqualTo(c == CafeChapter.All.Count - 1));
                Assert.That(m.NextChapter(), Is.EqualTo(c < CafeChapter.All.Count - 1));
            }
            Assert.That(m.AllChaptersComplete, Is.True);
        }

        [Test] public void GenerationBumpsOnEveryMoveAndNotOnRefusals()
        {
            var m = new CafeModel();
            int g = m.Generation;
            m.Place("item-1", false, 0); Assert.That(m.Generation, Is.EqualTo(++g));
            m.Remove("item-1"); Assert.That(m.Generation, Is.EqualTo(++g));
            m.DealRound(false); Assert.That(m.Generation, Is.EqualTo(++g));
            m.Check(); Assert.That(m.Generation, Is.EqualTo(g), "a failed check moves nothing");
            m.ResetTable(); Assert.That(m.Generation, Is.EqualTo(++g));
            m.StartChapter(BoxItUp); Assert.That(m.Generation, Is.EqualTo(++g));
            m.RestartChapter(); Assert.That(m.Generation, Is.EqualTo(++g));
        }

        [Test] public void ContainerNamesMatchTheStage()
        {
            var m = new CafeModel();
            Assert.That(m.ContainerName(0), Is.EqualTo("Plate 1"));
            m.StartChapter(BiggerOrder);
            Assert.That(m.ContainerName(4), Is.EqualTo("Box 5"));
        }
    }
}
