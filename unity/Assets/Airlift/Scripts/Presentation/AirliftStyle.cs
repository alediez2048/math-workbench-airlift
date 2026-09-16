using TMPro;
using UnityEngine;
namespace Airlift.Presentation
{
    [CreateAssetMenu(menuName="Airlift/Style")]
    public sealed class AirliftStyle : ScriptableObject
    {
        public TMP_FontAsset headingFont;
        public TMP_FontAsset bodyFont;
        public Color panel = new Color32(24,35,59,255);
        public Color text = new Color32(245,247,251,255);
        public Color primary = new Color32(74,215,188,255);
        public Color muted = new Color32(186,198,216,255);
    }
}
