using UnityEngine;

namespace Airlift.Presentation.Dashboard
{
    /// One tile object on the wall. The builder makes 24 and the wall shows eight at a time.
    public sealed class DashboardTileView : MonoBehaviour
    {
        public string id, lessonId;
        public int chapter;
        public bool openable = true;
        [Tooltip("The Continue ribbon, shown on exactly one tile.")]
        public GameObject ribbon;
        [Tooltip("The shadow that shows and hides with the tile (a sibling drawn behind it, as PolishCards makes it).")]
        public GameObject shadow;
        public float shadowDrop = 6f;

        public void SetShown(bool on, Vector2 slot)
        {
            if (gameObject.activeSelf != on) gameObject.SetActive(on);
            if (shadow != null && shadow.activeSelf != on) shadow.SetActive(on);
            if (!on) return;
            ((RectTransform)transform).anchoredPosition = slot;
            if (shadow != null && shadow.transform is RectTransform sr) sr.anchoredPosition = slot + new Vector2(0f, -shadowDrop);
        }

        public void SetRibbon(bool on) { if (ribbon != null && ribbon.activeSelf != on) ribbon.SetActive(on); }
    }
}
