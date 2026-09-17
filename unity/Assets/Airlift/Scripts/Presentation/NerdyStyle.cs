using TMPro;
using UnityEngine;

namespace Airlift.Presentation
{
    /// Nerdy design tokens from the owner's Live Learning Style Guide (September 2026).
    /// Colors are exact; type sizes are canvas units at the 1 px = 1 unit lesson canvas scale.
    [CreateAssetMenu(menuName = "Airlift/Nerdy Style")]
    public sealed class NerdyStyle : ScriptableObject
    {
        [Header("Type")]
        public TMP_FontAsset displayFont;      // Poppins Medium
        public TMP_FontAsset displayItalic;    // Poppins Medium Italic (one emphasized word)
        public TMP_FontAsset bodyFont;         // Poppins Regular
        public TMP_FontAsset bodySemibold;     // Poppins SemiBold (lead-ins)
        public TMP_FontAsset altFont;          // Karla Medium (UI labels, eyebrows)
        public TMP_FontAsset altBold;          // Karla Bold

        [Header("Surfaces and text")]
        public Color baseColor = Hex("#202344");
        public Color surface = Hex("#161C2C");
        public Color line = Hex("#6C6E87");
        public Color text = Color.white;
        public Color textMuted = new Color(1f, 1f, 1f, 0.64f);
        public Color paper = Hex("#FAF9F5");

        [Header("Accents")]
        public Color indigo = Hex("#3C4CDB");
        public Color lavender = Hex("#9E97FF");
        public Color amber = Hex("#FFC32B");
        public Color magenta = Hex("#FB43DA");
        public Color orchid = Hex("#D684FF");
        public Color cyan = Hex("#17E2EA");

        [Header("Sprites (9-slice)")]
        public Sprite pill;          // radius 100 pill
        public Sprite card;          // radius 20
        public Sprite small;         // radius 12
        public Sprite brandGradient; // 267deg indigo -> lavender
        public Sprite spectrumGradient; // amber 8%, magenta 42%, orchid 76%, cyan 97%
        public Sprite glass;         // frosted bar fill

        public const float RadiusSmall = 12f, RadiusNav = 14f, RadiusCard = 20f, RadiusPill = 100f;
        public const float SizeDisplay = 72f, SizeH1 = 56f, SizeH2 = 36f, SizeH3 = 28f, SizeSubhead = 20f, SizeBody = 16f, SizeSmall = 14f, SizeMicro = 10f;

        public static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }
    }
}
