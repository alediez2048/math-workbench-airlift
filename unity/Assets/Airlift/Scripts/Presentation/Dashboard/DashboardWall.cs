using System;
using System.Collections.Generic;
using System.Linq;
using Airlift.Lounge;
using Airlift.Welcome;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Airlift.Presentation.Dashboard
{
    /// CC-FD-07/07b in the scene: 24 tiles, eight per page, a filter bar under them. Order and Continue come from
    /// DashboardCatalog; this only places tiles into slots.
    public sealed class DashboardWall : MonoBehaviour
    {
        public const int Columns = 4, Rows = 2;
        // Four across inside the board's side margins, 28 between cards as the style guide has it.
        public static readonly Vector2 TileSize = new Vector2(280f, 190f);
        public const float ColumnGap = 28f, RowGap = 24f, GridCentreY = 60f;

        public DashboardTileView[] tiles = new DashboardTileView[0];
        public Button featuredButton, newestButton, mostViewedButton, previousButton, nextButton;
        public TMP_Text pageLabel;
        public Image[] filterPills = new Image[3];
        public Sprite filterChosen, filterIdle;

        public DashboardFilter Filter { get; private set; } = DashboardFilter.Featured;
        public int Page { get; private set; }
        public int PageCount => DashboardCatalog.PageCount(ordered.Count);
        public event Action<string, int> TileOpened;   // lessonId, chapter
        public event Action<DashboardFilter> FilterChanged;
        public event Action<int> PageMoved;
        LibraryState library;
        IReadOnlyList<DashboardTile> ordered = new List<DashboardTile>();

        public static Vector2 Slot(int index)
        {
            int col = index % Columns, row = index / Columns;
            float width = Columns * TileSize.x + (Columns - 1) * ColumnGap;
            float x = -width / 2f + TileSize.x / 2f + col * (TileSize.x + ColumnGap);
            float height = Rows * TileSize.y + (Rows - 1) * RowGap;
            float y = GridCentreY + height / 2f - TileSize.y / 2f - row * (TileSize.y + RowGap);
            return new Vector2(x, y);
        }

        public void Refresh(LibraryState lib)
        {
            library = lib;
            ordered = DashboardCatalog.Order(Filter, DashboardCatalog.AllTiles(), library);
            int pages = DashboardCatalog.PageCount(ordered.Count);
            Page = ((Page % pages) + pages) % pages;
            var shown = DashboardCatalog.Page(ordered, Page).Select(t => t.Id).ToList();
            string cont = DashboardCatalog.ContinueTileId(library);
            foreach (var view in tiles)
            {
                if (view == null) continue;
                int slot = shown.IndexOf(view.id);
                view.SetShown(slot >= 0, slot >= 0 ? Slot(slot) : Vector2.zero);
                view.SetRibbon(view.id == cont);
            }
            if (pageLabel != null) pageLabel.text = (Page + 1) + " / " + pages;
            for (int i = 0; i < filterPills.Length; i++)
            {
                var pill = filterPills[i]; if (pill == null) continue;
                bool chosen = i == (int)Filter;
                if (filterChosen != null && filterIdle != null)
                {
                    var sprite = chosen ? filterChosen : filterIdle;
                    pill.sprite = sprite; pill.type = Image.Type.Sliced;
                    // Same capsule rule as the language pills: the ends stay half circles after a sprite swap.
                    float half = Mathf.Max(1f, Mathf.Min(pill.rectTransform.rect.width, pill.rectTransform.rect.height)) / 2f;
                    pill.pixelsPerUnitMultiplier = sprite.border.x / half * (100f / sprite.pixelsPerUnit);
                }
                pill.color = chosen ? Color.white : new Color(1f, 1f, 1f, 0.1f);
            }
        }

        /// Persistent-listener friendly: "featured" | "newest" | "most_viewed".
        public void SetFilter(string name)
        {
            var f = GuideTools.ParseFilter("{\"filter\":\"" + name + "\"}");
            if (f == null) return;
            SetFilter(f.Value);
        }
        public void SetFilter(DashboardFilter filter)
        {
            Filter = filter; Page = 0;
            Refresh(library);
            FilterChanged?.Invoke(filter);
        }
        public void NextPage() { Page++; Refresh(library); NerdyHaptics.Tick(); PageMoved?.Invoke(1); }
        public void PreviousPage() { Page--; Refresh(library); NerdyHaptics.Tick(); PageMoved?.Invoke(-1); }

        /// A tile press: "lessonId#chapter".
        public void PressTile(string tileId)
        {
            var view = tiles.FirstOrDefault(t => t != null && t.id == tileId);
            if (view == null || !view.openable) return;
            TileOpened?.Invoke(view.lessonId, view.chapter);
        }

        /// The tiles on the page, in reading order (left to right, top row first).
        public IEnumerable<DashboardTileView> Visible => tiles.Where(t => t != null && t.gameObject.activeSelf)
            .OrderByDescending(t => Mathf.Round(((RectTransform)t.transform).anchoredPosition.y))
            .ThenBy(t => ((RectTransform)t.transform).anchoredPosition.x);
    }
}
