using System.IO;
using System.Linq;
using System.Text;
using Airlift.Lounge;
using Airlift.Welcome;
using TMPro;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// The welcome board exactly as the app arranges it at runtime: panel placed from the head the way
// PlaceWhenTracked does, consent phase showing, assistant bar on the welcome canvas at its saved position, arrival
// finished, lounge in the Nerdy scenery. Renders it from the seated head and dumps every element's rect and every
// button's wiring, so a layout is judged before it is built, not on the owner's face.
// Run: unity command run_script --file AgentScripts/PreviewWelcomeBoard.cs --entry PreviewWelcomeBoard.Run
public static class PreviewWelcomeBoard
{
    public static string Run()
    {
        const string scenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var n = Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include);
        var room = Object.FindAnyObjectByType<LoungeRoom>(FindObjectsInactive.Include);
        var sb = new StringBuilder();
        string dir = Path.GetFullPath("../artifacts/lounge"); Directory.CreateDirectory(dir);
        try
        {
            // Runtime placement, reproduced: head at seated height, panel in front and below eyes.
            var headPos = new Vector3(0f, 1.25f, 0f);
            var forward = Vector3.forward;
            if (n.welcomeAnchor != null)
                n.welcomeAnchor.SetPositionAndRotation(headPos + forward * n.welcomeDistance - Vector3.up * n.welcomeBelowEyes, Quaternion.LookRotation(forward));

            // Consent phase, as ShowPhase leaves it once the arrival has finished.
            n.consentRoot.SetActive(true);
            n.welcomeRoot.SetActive(false);
            n.catalogRoot.SetActive(false);
            var arrival = n.transform.Find("Arrival"); if (arrival != null) arrival.gameObject.SetActive(false);
            var settings = n.GetComponent<LoungeSettings>(); if (settings != null && settings.panel != null) settings.panel.SetActive(false);
            n.hudRoot.SetActive(true);
            var hud = (RectTransform)n.hudRoot.transform;
            hud.SetParent(n.hudWelcomeCanvas, false);
            hud.anchoredPosition = n.hudWelcomePosition; hud.localRotation = Quaternion.identity; hud.localScale = Vector3.one;
            var ray = n.hudWelcomeCanvas.Find("ISDK_RayCanvasInteraction"); if (ray != null) ray.gameObject.SetActive(true);
            if (room != null) { room.Show(true); room.Apply(Scenery.NerdyLounge); room.ShowBoard(true); }
            var onboarding = Object.FindAnyObjectByType<Airlift.Onboarding.OnboardingDirector>(FindObjectsInactive.Include);
            if (onboarding != null) foreach (Transform c in onboarding.transform) c.gameObject.SetActive(false);
            Canvas.ForceUpdateCanvases();

            n.head.SetPositionAndRotation(headPos, Quaternion.LookRotation((n.welcomeAnchor.position - headPos).normalized));
            Shot(n.head, Path.Combine(dir, "welcome-board.png"));

            // Settings open, same framing.
            if (settings != null) settings.Show(true);
            Canvas.ForceUpdateCanvases();
            Shot(n.head, Path.Combine(dir, "welcome-settings.png"));
            if (settings != null) settings.Show(false);

            // The wall (catalog phase), page 1 of Featured with a Continue ribbon, then the Newest filter.
            n.consentRoot.SetActive(false); n.catalogRoot.SetActive(true);
            var wall = n.catalogRoot.GetComponent<Airlift.Presentation.Dashboard.DashboardWall>();
            if (wall != null)
            {
                var lib = new LibraryState(); lib.RecordOpened("neighborhood_cafe_division", 2);
                wall.Refresh(lib);
                Canvas.ForceUpdateCanvases();
                Shot(n.head, Path.Combine(dir, "welcome-wall.png"));
                wall.SetFilter(DashboardFilter.Newest); wall.NextPage(); Canvas.ForceUpdateCanvases();
                Shot(n.head, Path.Combine(dir, "welcome-wall-newest-p2.png"));
                wall.SetFilter(DashboardFilter.Featured);
                sb.AppendLine("WALL page 1: " + string.Join(" | ", wall.Visible.Select(v => v.id + (v.ribbon != null && v.ribbon.activeSelf ? " [CONTINUE]" : ""))));
                Dump(sb, "wall", (RectTransform)n.catalogRoot.transform);
            }
            else sb.AppendLine("WALL: no DashboardWall on the catalog root (run BuildDashboard)");
            n.catalogRoot.SetActive(false);

            // The tour: stop 3 (filters) and stop 6 (back), pointer on the wall.
            var rundown = n.GetComponent<LoungeRundown>();
            if (rundown != null && rundown.pointerRoot != null)
            {
                n.catalogRoot.SetActive(true);
                if (wall != null) wall.Refresh(new LibraryState());
                rundown.PreviewStep(2); Canvas.ForceUpdateCanvases();
                Shot(n.head, Path.Combine(dir, "welcome-tour-3.png"));
                rundown.PreviewStep(4); Canvas.ForceUpdateCanvases();
                Shot(n.head, Path.Combine(dir, "welcome-tour-5.png"));
                rundown.PreviewStep(5); Canvas.ForceUpdateCanvases();
                Shot(n.head, Path.Combine(dir, "welcome-tour-6.png"));
                n.catalogRoot.SetActive(false); n.consentRoot.SetActive(true);
                rundown.PreviewStep(0); Canvas.ForceUpdateCanvases();
                Shot(n.head, Path.Combine(dir, "welcome-tour-1.png"));
                sb.AppendLine("tour ring stop 1 " + rundown.ring.anchoredPosition + " " + rundown.ring.sizeDelta);
                n.consentRoot.SetActive(false); n.welcomeRoot.SetActive(true);
                rundown.PreviewStep(1); Canvas.ForceUpdateCanvases();
                Shot(n.head, Path.Combine(dir, "welcome-tour-2.png"));
                n.welcomeRoot.SetActive(false);
                rundown.pointerRoot.gameObject.SetActive(false); rundown.header.SetActive(false);
                n.catalogRoot.SetActive(false);
            }
            else sb.AppendLine("TOUR: not built (run BuildTour)");
            n.consentRoot.SetActive(true);

            // World rects of everything the learner can see or press.
            sb.AppendLine("PANEL " + n.welcomeDistance + "m, " + n.welcomeBelowEyes + "m below eyes");
            Dump(sb, "consent", (RectTransform)n.consentRoot.transform);
            Dump(sb, "bar", hud);
            var board = n.transform.Find("Board");
            if (board != null)
            {
                var b = new Bounds(); bool first = true;
                foreach (var r in board.GetComponentsInChildren<Renderer>(true)) { if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds); }
                sb.AppendLine("board world y " + b.min.y.ToString("F3") + " .. " + b.max.y.ToString("F3") + "  x " + b.min.x.ToString("F3") + " .. " + b.max.x.ToString("F3"));
            }
            sb.AppendLine();
            sb.AppendLine("WIRING");
            var rundownCard = n.GetComponent<LoungeRundown>() != null ? n.GetComponent<LoungeRundown>().header : null;
            foreach (var btn in n.GetComponentsInChildren<Button>(true).Where(x => x.transform.IsChildOf(hud) || x.transform.IsChildOf(n.consentRoot.transform) || (settings != null && x.transform.IsChildOf(settings.panel.transform))
                                                                              || (rundownCard != null && x.transform.IsChildOf(rundownCard.transform)) || (x.transform.parent != null && x.transform.parent.name == "Toolbar")))
            {
                var calls = Enumerable.Range(0, btn.onClick.GetPersistentEventCount())
                    .Select(i => (btn.onClick.GetPersistentTarget(i) != null ? btn.onClick.GetPersistentTarget(i).GetType().Name : "null") + "." + btn.onClick.GetPersistentMethodName(i));
                sb.AppendLine("  " + btn.name.PadRight(22) + " -> " + (calls.Any() ? string.Join(", ", calls) : "(nothing)"));
            }
            return sb.ToString();
        }
        finally { EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single); }
    }

    static void Dump(StringBuilder sb, string label, RectTransform root)
    {
        var c = new Vector3[4]; root.GetWorldCorners(c);
        sb.AppendLine(label + " world " + (c[2].x - c[0].x).ToString("F2") + " x " + (c[2].y - c[0].y).ToString("F2") + " m, y " + c[0].y.ToString("F3") + " .. " + c[2].y.ToString("F3"));
        foreach (RectTransform child in root)
        {
            if (!child.gameObject.activeSelf) continue;
            child.GetWorldCorners(c);
            var text = child.GetComponentInChildren<TMP_Text>(true);
            sb.AppendLine("    " + child.name.PadRight(20) + " y " + c[0].y.ToString("F3") + " .. " + c[2].y.ToString("F3")
                          + "  x " + c[0].x.ToString("F3") + " .. " + c[2].x.ToString("F3")
                          + (text != null ? "  '" + Trim(text.text) + "'" : ""));
        }
    }

    static string Trim(string s) => s.Length > 24 ? s.Substring(0, 24) + "…" : s;

    static void ShotFrom(Vector3 from, Vector3 at, string path)
    {
        var go = new GameObject("Preview head") { hideFlags = HideFlags.HideAndDontSave };
        go.transform.SetPositionAndRotation(from, Quaternion.LookRotation((at - from).normalized));
        try { Shot(go.transform, path); } finally { Object.DestroyImmediate(go); }
    }

    static void Shot(Transform head, string path)
    {
        var camObj = new GameObject("Preview cam") { hideFlags = HideFlags.HideAndDontSave }; var cam = camObj.AddComponent<Camera>();
        cam.transform.SetPositionAndRotation(head.position, head.rotation); cam.fieldOfView = 60; cam.nearClipPlane = 0.05f;
        cam.clearFlags = CameraClearFlags.Skybox;
        var rt = new RenderTexture(1600, 1000, 24); var tex = new Texture2D(1600, 1000, TextureFormat.RGB24, false); var prior = RenderTexture.active;
        try { Canvas.ForceUpdateCanvases(); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
        finally { RenderTexture.active = prior; cam.targetTexture = null; Object.DestroyImmediate(tex); Object.DestroyImmediate(rt); Object.DestroyImmediate(camObj); }
    }
}
