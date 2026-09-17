using Airlift.Lessons.Cafe;
using UnityEngine;

namespace Airlift.Presentation.Cafe
{
    /// Pastry shapes in the Corner Café piece pool. Every piece carries all three and shows one per chapter.
    public enum CafeItemShape { Croissant, Cookie, Muffin }

    /// Pure geometry of the Corner Café workbench in station-local metres (the café root sits at identity under
    /// the world-locked root, so this is also board-local). The deck is the Cargo "Workbench" footprint:
    /// 1.3 x 0.8 m, top at y 0.0175, learner at -z. Plates or boxes stand in one row across the middle; the
    /// pastry tray is two rows in front; the guest table is behind the row; the delivery bike parks on the right.
    /// Nothing here decides whether an order is correct.
    public static class CafeLayout
    {
        public const float DeckTop = 0.0175f;
        public const float DeckHalfX = 0.65f, DeckHalfZ = 0.4f;
        public const float SightlineHeight = 0.19f;      // anything behind the card must stay under this

        public const float RowZ = 0.03f;                 // centre line of the plates or boxes
        public const int MaxPlates = 4, MaxBoxes = 6, MaxItems = 15;

        public const float PlateSpacing = 0.22f, PlateDiameter = 0.15f, PlateHeight = 0.012f;
        public const float PlateTolerance = 0.11f;       // horizontal release radius around a plate centre
        public const float BoxSpacing = 0.15f, BoxWidth = 0.13f, BoxDepth = 0.1f, BoxWall = 0.004f, BoxHeight = 0.032f;
        public const float BoxToleranceX = 0.075f, BoxToleranceZ = 0.09f;
        /// Box-local capacity tag: stands on the back wall's top rim facing the learner, above the pastries and far
        /// from the "Box N" label on the front edge.
        public static readonly Vector3 CapacityTagCenter = new Vector3(0f, BoxHeight + 0.015f, BoxDepth * 0.5f - BoxWall * 0.5f);
        public static readonly Vector2 CapacityTagSize = new Vector2(0.036f, 0.028f);
        public const float OrderUpTagY = 0.092f;

        /// Delivery bike cargo crate (bike-local, over the rear wheel) and how accepted boxes ride on it.
        public static readonly Vector3 RackCenter = new Vector3(0f, 0.061f, -0.085f);
        public const float RackWidth = 0.11f, RackDepth = 0.07f, RackScale = 0.4f, RackColumnOffset = 0.028f, RackLayerHeight = 0.02f;

        public const float ItemHalfHeight = 0.011f;      // pastry pivot sits this far above its resting surface
        public const float TrayTop = DeckTop + 0.008f;
        public const float TrayFrontZ = -0.30f, TrayBackZ = -0.245f, TraySpacing = 0.052f;
        public const int TrayFrontCount = 8;
        /// Serving tray under the two rows: sized for 15 pastries, well short of the 1.3 m deck.
        public const float TrayWidth = 0.45f, TrayDepth = 0.13f, TrayCenterZ = (TrayFrontZ + TrayBackZ) * 0.5f;

        /// "Plate 1" / "Box 1": readable from the seat, on the target's front edge, tilted toward the head.
        public const float LabelFontSize = 0.27f, LabelTilt = 50f, LabelHeight = 0.04f, LabelWidth = 0.13f;
        public const float LabelCenterY = DeckTop + 0.018f, LabelGap = 0.004f;

        /// Menu board behind the middle of the guest table, between the chairs: under the card, off the bike lane.
        public static readonly Vector3 MenuBoardPosition = new Vector3(0f, DeckTop, 0.388f);

        /// Seated head and card bottom edge in board space (as in the Dock 7 sightline checks). Behind the card a prop
        /// is visible below the card only while it stays under this height at its depth.
        public static readonly Vector3 SeatedHead = new Vector3(0f, 0.5f, -0.65f);
        public const float CardZ = 0.24f, CardBottomY = 0.225f;

        public const float GuestTableTop = 0.05f;
        public const float ServeDistance = 0.24f;        // plates slide +z onto the guest table
        public static readonly Vector3 BikeParked = new Vector3(0.53f, DeckTop, -0.1f);

        /// Station-local centre (at deck height) of container index of count, spread evenly around x = 0.
        public static Vector3 ContainerCenter(CafeTargetKind kind, int index, int count)
        {
            float spacing = kind == CafeTargetKind.Plates ? PlateSpacing : BoxSpacing;
            return new Vector3((index - (count - 1) * 0.5f) * spacing, DeckTop, RowZ);
        }

        /// The plate or box under a release point, or -1 when the point is not over any of them.
        public static int NearestContainer(CafeTargetKind kind, int count, Vector3 stationLocal)
        {
            int best = -1; float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var c = ContainerCenter(kind, i, count);
                float dx = stationLocal.x - c.x, dz = stationLocal.z - c.z;
                bool inside = kind == CafeTargetKind.Plates
                    ? dx * dx + dz * dz <= PlateTolerance * PlateTolerance
                    : Mathf.Abs(dx) <= BoxToleranceX && Mathf.Abs(dz) <= BoxToleranceZ;
                float d = dx * dx + dz * dz;
                if (inside && d < bestDistance) { best = i; bestDistance = d; }
            }
            return best;
        }

        /// Resting place of the index-th pastry on the tray: 8 in the front row, 7 behind.
        public static Vector3 TrayPosition(int index)
        {
            float y = TrayTop + ItemHalfHeight;
            if (index < TrayFrontCount) return new Vector3((index - (TrayFrontCount - 1) * 0.5f) * TraySpacing, y, TrayFrontZ);
            int back = index - TrayFrontCount, backCount = MaxItems - TrayFrontCount;
            return new Vector3((back - (backCount - 1) * 0.5f) * TraySpacing, y, TrayBackZ);
        }

        /// Resting place of the slot-th pastry in a container centred at center: a 3 x 2 grid, back row first.
        /// Plates have no limit, so every sixth pastry starts a new layer on top.
        public static Vector3 SlotPosition(CafeTargetKind kind, Vector3 center, int slot)
        {
            if (slot < 0) slot = 0;
            bool plate = kind == CafeTargetKind.Plates;
            int layer = plate ? slot / 6 : 0, i = slot % 6;
            int col = i % 3, row = i / 3;
            float dx = plate ? 0.042f : 0.037f, dz = 0.04f;
            float floor = plate ? PlateHeight : BoxWall;
            return new Vector3(center.x + (col - 1) * dx, center.y + floor + ItemHalfHeight + layer * 0.024f, center.z + (0.5f - row) * dz);
        }

        /// Half depth of the front edge of a plate or box from its centre.
        public static float FrontHalfDepth(CafeTargetKind kind) => kind == CafeTargetKind.Plates ? PlateDiameter * 0.5f : BoxDepth * 0.5f;

        /// Depth a tilted label covers on the deck.
        public static float LabelDepth => LabelHeight * Mathf.Sin(LabelTilt * Mathf.Deg2Rad);

        /// Deck label centre just in front of a container's front edge ("Plate 1" / "Box 1").
        public static Vector3 LabelPosition(CafeTargetKind kind, Vector3 center) =>
            new Vector3(center.x, LabelCenterY, center.z - FrontHalfDepth(kind) - LabelGap - LabelDepth * 0.5f);

        /// Highest point behind the card (at depth z > CardZ) that the seated head still sees under the card.
        public static float VisibleUnderCard(float z)
        {
            if (z <= CardZ) return float.MaxValue;
            float t = (z - SeatedHead.z) / (CardZ - SeatedHead.z);
            return SeatedHead.y + (CardBottomY - SeatedHead.y) * t;
        }

        /// Pastry shape for a chapter's item word; a mixed "pastry" chapter cycles through all three.
        public static CafeItemShape ShapeFor(string itemName, int index)
        {
            string n = (itemName ?? "").ToLowerInvariant();
            if (n.Contains("croissant")) return CafeItemShape.Croissant;
            if (n.Contains("cookie")) return CafeItemShape.Cookie;
            if (n.Contains("muffin")) return CafeItemShape.Muffin;
            return (CafeItemShape)(((index % 3) + 3) % 3);
        }
    }
}
