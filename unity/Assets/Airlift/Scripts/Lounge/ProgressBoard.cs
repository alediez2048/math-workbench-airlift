using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Airlift.Lounge
{
    /// "What you've built": three rows, five checkmarks each, "n of 5". No score, no stars (project pedagogy rules).
    public sealed class ProgressBoard : MonoBehaviour
    {
        [Serializable] public sealed class Row { public string lessonId; public TMP_Text title; public TMP_Text count; public Image[] marks = new Image[0]; }
        public Row[] rows = new Row[0];
        public Color doneColour = new Color(0.62f, 0.59f, 1f), todoColour = new Color(0.23f, 0.25f, 0.42f);

        public void Refresh(LibraryState library)
        {
            var model = ToyRackModel.BoardRows(library);
            foreach (var row in rows)
            {
                BoardRow m = null; foreach (var b in model) if (b.LessonId == row.lessonId) m = b;
                if (m == null) continue;
                if (row.title != null) row.title.text = m.Title;
                if (row.count != null) row.count.text = m.Label;
                for (int i = 0; i < row.marks.Length; i++)
                {
                    if (row.marks[i] == null) continue;
                    bool done = i < m.Marks.Length && m.Marks[i];
                    row.marks[i].color = done ? doneColour : todoColour;
                    var tick = row.marks[i].transform.childCount > 0 ? row.marks[i].transform.GetChild(0).gameObject : null;
                    if (tick != null) tick.SetActive(done);
                }
            }
        }
    }
}
