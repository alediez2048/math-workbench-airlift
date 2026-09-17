using Airlift.Lessons.Cafe;
using UnityEngine;

namespace Airlift.Presentation.Cafe
{
    /// Pastry shapes in the Corner Café piece pool. Every piece carries all three and shows one per chapter.
    public enum CafeItemShape { Croissant, Cookie, Muffin }

    /// What a concept-intro visual puts on the counter: pastries on the tray, or dealt onto plates, or packed into
    /// boxes. Pure description; CafeStation poses the pieces from it without touching CafeModel.
    public readonly struct CafeIntroScene
    {
        public readonly CafeTargetKind Kind;
        public readonly int Containers, Items, BoxCapacity;
        public readonly bool Placed;          // false: every pastry waits on the tray
        public readonly string ItemName, ItemPlural;
        public CafeIntroScene(CafeTargetKind kind, int containers, int items, int boxCapacity, bool placed, string itemName, string itemPlural)
        {
            Kind = kind; Containers = containers; Items = items; BoxCapacity = boxCapacity; Placed = placed; ItemName = itemName; ItemPlural = itemPlural;
        }
        public int PerContainer => Containers > 0 ? Items / Containers : 0;
    }

    /// Pure geometry of the Corner Café workbench in station-local metres (the café root sits at identity under
    /// the world-locked root, so this is also board-local). The deck is the Cargo "Workbench" footprint:
    /// 1.3 x 0.8 m, top at y 0.0175, learner at -z. Owner 2026-09-17: pastries are 2x the first build, so the front
    /// half holds only what the learner grabs: the pastry tray at the front edge and one row of plates or boxes
    /// behind it. The back half holds the coffee counter (left), the guest table (middle) and the delivery bike
    /// (right). Nothing here decides whether an order is correct.
    public static class CafeLayout
    {
        public const float DeckTop = 0.0175f;
        public const float DeckHalfX = 0.65f, DeckHalfZ = 0.4f;
        public const float SightlineHeight = 0.19f;      // anything behind the card must stay under this

        /// Pastry size against the first build (croissant about 0.047 m wide); every pastry dimension is scaled.
        public const float PieceScale = 2f;
        public const float ItemHalfHeight = 0.011f * PieceScale;   // pastry pivot sits this far above its resting surface
        public const float ItemHalfWidth = 0.0235f * PieceScale;   // widest pastry (croissant) along x
        public const float ItemHalfDepth = 0.018f * PieceScale;    // deepest pastry (cookie) along z
        public const float BoxItemHalfWidth = 0.018f * PieceScale; // cookies and muffins, the only boxed pastries
        public static readonly Vector3 PieceColliderSize = new Vector3(0.045f, 0.034f, 0.045f) * PieceScale;
        public static readonly Vector3 PieceColliderCenter = new Vector3(0f, 0.004f, 0f) * PieceScale;

        public const int MaxPlates = 4, MaxBoxes = 6, MaxItems = 15;

        /// Front edge of every plate and box; the "Plate N" / "Box N" labels sit just in front of it.
        public const float RowFrontZ = -0.1375f;
        /// Seated reach: nothing the learner grabs lies farther back than this.
        public const float ReachBackZ = 0.12f;

        public const float PlateSpacing = 0.27f, PlateDiameter = 0.22f, PlateHeight = 0.016f;
        public const float PlateTolerance = 0.135f;      // horizontal release radius around a plate centre
        public const float PlateSlotDx = 0.095f, PlateSlotDz = 0.08f, PlateLayerHeight = 0.048f;
        public const int PlateSlotsPerLayer = 4;         // 2 x 2, then a new layer on top
        /// Boxes are 2 pastries wide and 3 deep so five boxes stand in one row across the deck.
        public const float BoxSpacing = 0.19f, BoxWidth = 0.17f, BoxDepth = 0.24f, BoxWall = 0.008f, BoxHeight = 0.064f;
        public const float BoxToleranceX = 0.095f, BoxToleranceZ = 0.15f;
        public const float BoxSlotDx = 0.075f, BoxSlotDz = 0.075f;
        /// Box-local capacity tag: stands on the back wall's top rim facing the learner, above the pastries and far
        /// from the "Box N" label on the front edge.
        public static readonly Vector2 CapacityTagSize = new Vector2(0.06f, 0.044f);
        public static readonly Vector3 CapacityTagCenter = new Vector3(0f, BoxHeight + 0.026f, BoxDepth * 0.5f - BoxWall * 0.5f);
        public const float OrderUpTagY = 0.138f;

        /// Delivery bike cargo crate (bike-local, over the rear wheel) and how accepted boxes ride on it.
        public static readonly Vector3 RackCenter = new Vector3(0f, 0.061f, -0.085f);
        public const float RackWidth = 0.11f, RackDepth = 0.07f, RackScale = 0.28f, RackColumnOffset = 0.028f, RackLayerHeight = 0.022f;

        public const float TrayTop = DeckTop + 0.008f;
        public const float TrayFrontZ = -0.33f, TrayBackZ = -0.245f, TraySpacing = 0.1f;
        public const int TrayFrontCount = 8;
        /// Serving tray under the two rows at the front edge, clear of the carry handle.
        public const float TrayWidth = 0.83f, TrayDepth = 0.18f, TrayCenterZ = -0.2875f, TrayHandleReach = 0.018f;

        /// "Plate 1" / "Box 1": readable from the seat, on the target's front edge, tilted toward the head.
        public const float LabelFontSize = 0.32f, LabelTilt = 50f, LabelHeight = 0.05f, LabelWidth = 0.17f;
        public const float LabelCenterY = DeckTop + 0.022f, LabelGap = 0.004f;

        /// Menu board behind the middle of the guest table, between the chairs: under the card, off the bike lane.
        public static readonly Vector3 MenuBoardPosition = new Vector3(0f, DeckTop, 0.388f);

        /// Seated head and card bottom edge in board space (as in the Dock 7 sightline checks). Behind the card a prop
        /// is visible below the card only while it stays under this height at its depth.
        public static readonly Vector3 SeatedHead = new Vector3(0f, 0.5f, -0.65f);
        public const float CardZ = 0.24f, CardBottomY = 0.225f;

        /// Back-half props (station-local centres at deck height and their deck footprints).
        public static readonly Vector3 CounterPosition = new Vector3(-0.53f, DeckTop, 0.26f);
        public static readonly Vector2 CounterSize = new Vector2(0.22f, 0.26f);
        public const float GuestTableTop = 0.05f, GuestTableZ = 0.25f, GuestTableWidth = 0.8f, GuestTableDepth = 0.22f;
        public const float ChairZ = 0.385f;
        public static readonly float[] ChairX = { -0.3f, -0.14f, 0.14f, 0.3f };
        /// Served plates line up on the guest table, a little smaller so every plate count fits between the counter
        /// and the bike.
        public const float ServeSpacing = 0.2f, ServeScale = 0.75f, ServedZ = 0.235f, GuestCupZ = 0.085f;
        public static readonly Vector3 BikeParked = new Vector3(0.53f, DeckTop, 0.25f);
        public static readonly Vector2 BikeHalfSize = new Vector2(0.06f, 0.125f);    // rear crate to front wheel

        /// Carry handle on the table's front edge (Dock 7 station space).
        public static readonly Rect HandleRect = Rect.MinMaxRect(-0.09f, -0.43f, 0.09f, -0.395f);

        /// Station-local centre (at deck height) of container index of count, spread evenly around x = 0.
        public static Vector3 ContainerCenter(CafeTargetKind kind, int index, int count)
        {
            float spacing = kind == CafeTargetKind.Plates ? PlateSpacing : BoxSpacing;
            return new Vector3((index - (count - 1) * 0.5f) * spacing, DeckTop, RowFrontZ + FrontHalfDepth(kind));
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

        /// Resting place of the slot-th pastry in a container centred at center, back row first. Plates hold a 2 x 2
        /// grid and have no limit, so every fifth pastry starts a new layer on top; boxes hold 2 across and 3 deep.
        public static Vector3 SlotPosition(CafeTargetKind kind, Vector3 center, int slot)
        {
            if (slot < 0) slot = 0;
            if (kind == CafeTargetKind.Plates)
            {
                int layer = slot / PlateSlotsPerLayer, i = slot % PlateSlotsPerLayer;
                int col = i % 2, row = i / 2;
                return new Vector3(center.x + (col - 0.5f) * PlateSlotDx, center.y + PlateHeight + ItemHalfHeight + layer * PlateLayerHeight,
                                   center.z + (0.5f - row) * PlateSlotDz);
            }
            int k = slot % 6;
            int c = k % 2, r = k / 2;
            return new Vector3(center.x + (c - 0.5f) * BoxSlotDx, center.y + BoxWall + ItemHalfHeight, center.z + (1 - r) * BoxSlotDz);
        }

        /// Half depth of the front edge of a plate or box from its centre.
        public static float FrontHalfDepth(CafeTargetKind kind) => kind == CafeTargetKind.Plates ? PlateDiameter * 0.5f : BoxDepth * 0.5f;

        /// Depth a tilted label covers on the deck.
        public static float LabelDepth => LabelHeight * Mathf.Sin(LabelTilt * Mathf.Deg2Rad);

        /// Deck label centre just in front of a container's front edge ("Plate 1" / "Box 1").
        public static Vector3 LabelPosition(CafeTargetKind kind, Vector3 center) =>
            new Vector3(center.x, LabelCenterY, center.z - FrontHalfDepth(kind) - LabelGap - LabelDepth * 0.5f);

        // ---- deck footprints (x, z) for fit checks ----
        public static Rect DeckRect => Rect.MinMaxRect(-DeckHalfX, -DeckHalfZ, DeckHalfX, DeckHalfZ);

        public static Rect ContainerRect(CafeTargetKind kind, int index, int count)
        {
            var c = ContainerCenter(kind, index, count);
            float hx = kind == CafeTargetKind.Plates ? PlateDiameter * 0.5f : BoxWidth * 0.5f, hz = FrontHalfDepth(kind);
            return Rect.MinMaxRect(c.x - hx, c.z - hz, c.x + hx, c.z + hz);
        }

        public static Rect LabelRect(CafeTargetKind kind, int index, int count)
        {
            var p = LabelPosition(kind, ContainerCenter(kind, index, count));
            return Rect.MinMaxRect(p.x - LabelWidth * 0.5f, p.z - LabelDepth * 0.5f, p.x + LabelWidth * 0.5f, p.z + LabelDepth * 0.5f);
        }

        /// Tray base plus its side handles.
        public static Rect TrayRect => Rect.MinMaxRect(-TrayWidth * 0.5f - TrayHandleReach, TrayCenterZ - TrayDepth * 0.5f,
                                                       TrayWidth * 0.5f + TrayHandleReach, TrayCenterZ + TrayDepth * 0.5f);
        public static Rect CounterRect => Centered(CounterPosition.x, CounterPosition.z, CounterSize.x * 0.5f, CounterSize.y * 0.5f);
        public static Rect GuestTableRect => Centered(0f, GuestTableZ, GuestTableWidth * 0.5f, GuestTableDepth * 0.5f);
        public static Rect BikeRect => Centered(BikeParked.x, BikeParked.z, BikeHalfSize.x, BikeHalfSize.y);
        public static Rect MenuBoardRect => Rect.MinMaxRect(-0.08f, 0.375f, 0.08f, 0.405f);
        public static Rect ChairRect(int index) => Centered(ChairX[index], ChairZ, 0.024f, 0.018f);

        static Rect Centered(float x, float z, float hx, float hz) => Rect.MinMaxRect(x - hx, z - hz, x + hx, z + hz);

        /// Where the index-th of count accepted plates stands on the guest table (station-local, at the table top).
        public static Vector3 ServedCenter(int index, int count) =>
            new Vector3((index - (count - 1) * 0.5f) * ServeSpacing, GuestTableTop, ServedZ);

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

        // ---- concept intro "What is dividing?" ----
        /// share: 6 croissants on the tray and 2 empty plates; groups: 3 on each of 2 plates; boxes: 2 boxes of 3.
        /// Unknown visuals show an empty counter.
        public static CafeIntroScene IntroScene(string visual)
        {
            switch (visual)
            {
                case "share": return new CafeIntroScene(CafeTargetKind.Plates, 2, 6, 0, false, "croissant", "croissants");
                case "groups": return new CafeIntroScene(CafeTargetKind.Plates, 2, 6, 0, true, "croissant", "croissants");
                case "boxes": return new CafeIntroScene(CafeTargetKind.Boxes, 2, 6, 3, true, "croissant", "croissants");
                default: return new CafeIntroScene(CafeTargetKind.Plates, 0, 0, 0, false, "croissant", "croissants");
            }
        }

        /// Container of the index-th intro pastry (-1 on the tray): plates are dealt one round at a time, boxes are
        /// filled one after the other.
        public static int IntroContainerOf(CafeIntroScene scene, int index, out int slot)
        {
            slot = 0;
            if (!scene.Placed || scene.Containers <= 0 || index < 0 || index >= scene.Items) return -1;
            if (scene.Kind == CafeTargetKind.Plates) { slot = index / scene.Containers; return index % scene.Containers; }
            int per = scene.BoxCapacity > 0 ? scene.BoxCapacity : scene.PerContainer;
            slot = index % per; return index / per;
        }

        /// Station-local resting place of the index-th intro pastry.
        public static Vector3 IntroItemPosition(CafeIntroScene scene, int index)
        {
            int container = IntroContainerOf(scene, index, out int slot);
            return container < 0 ? TrayPosition(index)
                : SlotPosition(scene.Kind, ContainerCenter(scene.Kind, container, scene.Containers), slot);
        }
    }
}
