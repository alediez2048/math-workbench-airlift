using TMPro;
using UnityEngine;
namespace Airlift.Presentation
{
    public sealed class TypographyBindings : MonoBehaviour
    {
        public AirliftStyle style;
        public TMP_Text[] headings;
        public TMP_Text[] bodies;
        public void Apply()
        {
            if (style == null || style.headingFont == null || style.bodyFont == null)
                throw new System.InvalidOperationException("Explicit Airlift fonts required.");
            foreach(var label in headings) if(label != null) label.font=style.headingFont;
            foreach(var label in bodies) if(label != null) label.font=style.bodyFont;
        }
    }
}
