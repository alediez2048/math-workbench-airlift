using UnityEngine;

namespace Airlift.Presentation
{
    /// A lesson workbench's palette: deck, trim and prop colours plus its card colours. Builders bake the colours into
    /// the lesson's own materials; fonts stay on the shared styles.
    [CreateAssetMenu(menuName = "Airlift/Lesson Theme")]
    public sealed class LessonTheme : ScriptableObject
    {
        public Color deck, trim, accent, accentSoft, prop1, prop2, prop3;
        public Color cardPanel, cardHeading, cardBody, buttonFill, buttonLabel;
        public Material deckMaterial, trimMaterial, accentMaterial;
    }
}
