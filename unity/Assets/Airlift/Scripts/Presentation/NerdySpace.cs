namespace Airlift.Presentation
{
    /// The spatial half of the design system. NerdyStyle says what a surface looks like; NerdySpace says where it
    /// stands and how big it is, so a heading is the same height in millimetres wherever the learner meets it.
    ///
    /// Owner 2026-09-17, after measuring six panel sizes, two distances and four physical heading heights in one
    /// app: "we need to standardize spacial dimensons across the experience". The anchor is reading distance —
    /// every panel is one size, at one distance, whether it is the lounge board or a lesson card.
    ///
    /// Canvas units are millimetres at PanelScale: a 40 unit heading is 40 mm tall in the world. Keep it that way,
    /// and the numbers in this file can be read as physical sizes.
    public static class NerdySpace
    {
        /// Head to panel, in metres. Far enough to read a full board without turning your head.
        public const float PanelDistance = 1.2f;
        /// Panel centre below eye level, in metres: a slight downward gaze, the way a desk sits. Moved a metre
        /// back and up on 2026-09-17 and reverted the same evening — at 2.2 m the board was too far to read.
        public const float PanelBelowEyes = 0.25f;

        /// Every panel is this size, in canvas units, at PanelScale. Grown 20% on 2026-09-17: the owner found the
        /// board cramming its content ("is too small"), and a panel that fits its content is worth the extra area.
        public const float PanelWidth = 1440f, PanelHeight = 840f;
        public const float PanelScale = 0.001f;

        /// The same panel in metres, for placing 3D frames and boards around it.
        public const float PanelWidthMetres = PanelWidth * PanelScale;
        public const float PanelHeightMetres = PanelHeight * PanelScale;

        /// The type ramp in canvas units, i.e. millimetres. Headings match across every surface. Trimmed on
        /// 2026-09-17 with the board's second growth: the owner wanted room around the content, and a bigger board
        /// full of the same large type is just a bigger crowd.
        public const float Heading = 34f, Subhead = 21f, Body = 17f, Label = 14f, Micro = 12f;

        /// Space between things, in canvas units.
        public const float Gutter = 28f, RowGap = 20f, EdgePadding = 36f;

        /// Controls a hand has to hit: 40 mm is a comfortable pill height at PanelScale.
        public const float PillHeight = 40f, PillGap = 26f;
        /// Dee's bar: compact (owner 2026-09-18: "not having so much space between elements"). Orb, caption, then
        /// the controls packed at BarGap, on a bar that is well under the panel's width.
        public const float BarWidth = 900f, BarHeight = 84f, BarGap = 12f, BarPadding = 20f;

        /// A frame or board mounted behind a panel clears it by this much on each side, in metres.
        public const float BoardMargin = 0.06f;
    }
}
