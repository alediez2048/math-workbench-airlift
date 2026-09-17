using UnityEngine;

namespace Airlift.Presentation.Garden
{
    /// Pure station-local geometry of the Sunny Plot table in metres (the garden root sits at identity under the
    /// world-locked root, so this is also board-local). The deck is the Cargo "Workbench" footprint: 1.3 x 0.8 m, top at
    /// y 0.0175, learner at -z, lesson card at z 0.24 with its bottom edge at y 0.225, table handle on the front edge.
    /// Owner 2026-09-17 ("the plants are too little"): seedling strips and their plants are twice the first build. The
    /// bed sits in the middle with its front wall on BedFrontZ; the two seedling trays run front to back beside it and
    /// hold the strips lying front to back. Only the 7 × 6 and 8 × 7 beds are too deep for full-size cells in front of
    /// the card, so their cells (and the pieces on them) shrink to fit BedMaxDepth. Nothing here decides correctness.
    public static class GardenTableLayout
    {
        public const float DeckTop = 0.0175f, DeckHalfX = 0.65f, DeckHalfZ = 0.4f;

        /// Seated head and card bottom edge in board space (as in the Dock 7 and café sightline checks).
        public static readonly Vector3 SeatedHead = new Vector3(0f, 0.5f, -0.65f);
        public const float CardZ = 0.24f, CardBottomY = 0.225f;
        /// Table handle on the front edge: x ±0.09, z -0.43 to -0.395.
        public const float HandleHalfX = 0.09f, HandleBackZ = -0.395f;

        // ---- seedling strips (twice the first build: 45 mm pitch, 12 mm body, 36 mm deep) ----
        public const float StripCell = 0.09f;
        public const float StripBodyHeight = 0.024f, StripBodyDepth = 0.072f, StripBodyInset = 0.008f;
        public const float StripColliderSize = 0.08f;
        public const float SeedlingY = 0.012f;
        /// Strip-local top of the tallest grown crop (the sunflower), at piece scale 1.
        public const float PlantTop = 0.16f;

        // ---- bed ----
        public const float SoilHeight = 0.018f, SoilTop = DeckTop + SoilHeight;
        public const float PlantedRestY = SoilTop + StripBodyHeight * 0.5f;
        public const float WallThickness = 0.016f, WallHeight = 0.048f;
        /// Inside of the front wall of the deepest bed; every bed grows back from here.
        public const float BedFrontZ = -0.30f;
        /// Deepest bed (inside the walls): the back wall stays 2 cm in front of the card.
        public const float BedMaxDepth = 0.50f;
        /// How far fence spot numbers and part labels reach in front of the inside of the front wall.
        public const float BedLabelReach = 0.07f;
        /// How far row numbers reach left of the inside of the left wall.
        public const float RowLabelReach = 0.072f;
        public const float RowLabelOffset = 0.035f, PartLabelOffset = 0.032f, SpotNumberOffset = 0.04f;
        /// Gap the bed and the strips open by at an accepted fence.
        public const float SplitGap = 0.05f;

        /// The bed for rows × columns on this table (see GardenBedLayout.Fit).
        public static GardenBedLayout BedFor(int rows, int columns) =>
            GardenBedLayout.Fit(0f, PlantedRestY, BedFrontZ, BedMaxDepth, StripCell, rows, columns);

        /// Uniform scale of a strip piece on a bed: 1 on full-size beds, below 1 on the 7 × 6 and 8 × 7 beds.
        public static float PieceScale(GardenBedLayout layout) => layout.Cell / StripCell;

        // ---- seedling trays: beside the bed, front to back, three strips each ----
        public const int TraySlots = 3;
        public const float TrayNearX = 0.325f, TrayWidth = 0.27f, TrayFrontZ = -0.375f, TrayBackZ = 0.19f;
        public const float TrayBase = 0.008f, TrayRim = 0.008f, TraySlotPitch = 0.085f;
        public const float TrayCenterX = TrayNearX + TrayWidth * 0.5f, TrayCenterZ = (TrayFrontZ + TrayBackZ) * 0.5f;
        public const float TrayDepth = TrayBackZ - TrayFrontZ;
        public const float TrayRestY = DeckTop + TrayBase + StripBodyHeight * 0.5f;
        /// A strip in a tray lies front to back: yaw -90 turns strip-local +x (seedling 1 to 8) toward +z.
        public const float TrayYaw = -90f;
        const float TrayClearance = 0.006f;

        /// Front end (seedling 1 edge) of strip view i's tray slot: even views on the left tray, odd on the right, slot
        /// 1 next to the bed. Views 6 and 7 exist only for planted chapters and share the outer slot.
        public static Vector3 TrayFront(int view)
        {
            float sign = view % 2 == 0 ? -1f : 1f;
            int slot = Mathf.Min(view / 2, TraySlots - 1);
            float x = TrayNearX + TrayRim + TrayClearance + StripBodyDepth * 0.5f + slot * TraySlotPitch;
            return new Vector3(sign * x, TrayRestY, TrayFrontZ + TrayRim + TrayClearance);
        }

        /// Root of a strip of `length` seedlings resting in the slot that starts at trayFront (strip roots are centred).
        public static Vector3 TrayPosition(Vector3 trayFront, int length, float cell) => trayFront + new Vector3(0f, 0f, length * cell * 0.5f);

        // ---- fence divider: spans the deepest bed; waits lying in the right tray ----
        public const float FenceLength = 0.54f, FenceHeight = 0.12f;
        public static readonly Vector3 FenceHome = new Vector3(TrayCenterX, DeckTop + TrayBase, TrayCenterZ);

        // ---- props and payoff (behind the bed and the trays) ----
        public static readonly Vector3 CanHome = new Vector3(0.42f, DeckTop, 0.285f);
        public static readonly Vector3 ButterflyHome = new Vector3(0.3f, DeckTop + 0.07f, 0.34f);
        public static readonly Vector3 TreeLeft = new Vector3(-0.56f, DeckTop, 0.3f), TreeRight = new Vector3(0.56f, DeckTop, 0.3f);
        public static readonly Vector3[] Bushes =
        {
            new Vector3(-0.6f, DeckTop, 0.25f), new Vector3(0.6f, DeckTop, 0.25f), new Vector3(-0.3f, DeckTop, 0.34f), new Vector3(0.3f, DeckTop, 0.34f),
        };
        public static readonly Vector3[] Sunflowers =
        {
            new Vector3(-0.19f, DeckTop, 0.33f), new Vector3(0.2f, DeckTop, 0.33f), new Vector3(0.12f, DeckTop, 0.34f),
        };
        public static readonly Vector3 Sign = new Vector3(-0.36f, DeckTop, 0.33f);
        public static readonly Vector3 BackFence = new Vector3(0f, DeckTop, 0.37f);

        /// Highest point the seated head sees at depth z without covering (in front) or hiding behind (behind) the
        /// lesson card: the line from the head through the card's bottom edge.
        public static float CardSightline(float z) =>
            SeatedHead.y + (CardBottomY - SeatedHead.y) * (z - SeatedHead.z) / (CardZ - SeatedHead.z);
    }
}
