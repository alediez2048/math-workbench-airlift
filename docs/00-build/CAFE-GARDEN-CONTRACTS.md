# Café + Garden — shared code contracts (2026-09-17)

Approved plan: `/Users/jad/.claude/plans/agile-wibbling-nebula.md` (copy of the lesson tables below). Tickets:
`TICKETS.md` → "Café and Garden". Five implementers work in parallel on **disjoint files**; the main session
(integrator) is the only one that runs Unity. Code against these signatures exactly. If one is impossible, keep the
signature, implement the nearest honest behaviour, and report it.

## Rules for every implementer

- Edit only the files your brief owns. Read anything.
- **No `unity` CLI at all** (no recompile, run_script, run_tests, build, refresh, scene open/save). No hand edits to
  `.unity` or `.asset` YAML unless your brief says so. You may compile offline with Unity's bundled Roslyn and run
  plain NUnit logic under Unity's bundled mono in your scratchpad, as the Dock 7 engine agent did.
- No git commit, push, stash, checkout or reset. Never revert other people's changes.
- **Cargo Crew is locked.** Do not change `CargoLessonDirector.cs`, `CargoLessonModel.cs`, `CargoChapter.cs`,
  `OnboardingDirector.cs`, `BuildDockWorkbench.cs` or any Cargo test's expectations.
- C#: Unity 6000.6, C# 9. Inside `namespace Airlift.*`, `Math` is the `Airlift.Math` namespace: use `Mathf` or
  `System.Math`. Newtonsoft.Json is available. Tests: NUnit EditMode in `unity/Assets/Airlift/Tests/EditMode`.
- On-screen copy: ASCII plus "·", "—", "×", "÷". No emoji. Card text uses **AirliftStyle** fonts (Nunito Bold heading,
  Nunito Sans Semibold body) because `CargoTerminalLayoutTests.CaptionsFitAndFontsBound` scans every TMP_Text under
  the world-locked root.
- Pedagogy: math is deterministic C# and evaluated only on check; every quantity refers to visible objects; wrong
  arrangements stay editable; feedback describes the table, never the learner; no timers, stars, scores or praise.
- Known gotchas from Dock 7: `Transform.Find` treats "/" as a path separator (never name objects "Row 1/2");
  anything behind the translucent card must stay under board height ~0.19 m or it shows through; tray pieces must not
  cover floor marks; each vehicle-like prop needs rounded meshes (no raw cubes); keep one active grab interactor per
  hand; the table handle re-enables every piece interactable after a carry, so re-apply locks in `LateUpdate`.
- Before editing an existing class, run GitNexus `impact` (repo `math-workbench-airlift`, upstream) and report risk.
- Final report under 400 words: files, public API, tests, integrator run order, assumptions, risks.

## P. Platform (agent: platform) — `Airlift.Lessons`, `Airlift.Welcome`, `Airlift.Presentation`

```csharp
// Scripts/Lessons/LessonStation.cs
public readonly struct LessonChapterFacts
{
    public readonly int Number; public readonly string Id, Title, Story, Task, Accepted, Expression, Feedback;
    public readonly bool Complete, IsLast;
    public LessonChapterFacts(int number, string id, string title, string story, string task, string accepted,
                              string expression, string feedback, bool complete, bool isLast);
}

public abstract class LessonStation : MonoBehaviour
{
    public string cardId;                         // LessonCatalog id
    public GameObject[] visualRoots;              // shown only while this station is open
    public CanvasGroup[] fallbackGroups;          // button rows hidden while the live guide listens
    public TMP_Text heading, body;                // this station's card; body is the guide's instruction source

    public abstract string Title { get; }         // "Neighborhood Café"
    public abstract string StoryName { get; }     // "Corner Café"
    public abstract bool IsOpen { get; }
    public abstract bool ChapterActive { get; }   // false during the briefing
    public abstract bool AnyHeld { get; }
    public abstract LessonChapterFacts Chapter { get; }   // default(LessonChapterFacts) when !ChapterActive
    public abstract Airlift.Welcome.GuideStep CurrentStep();
    public virtual string Instruction => body != null ? Airlift.Welcome.GuideSteps.StripDiagnostics(body.text) : "";
    public abstract string[] ToolNames { get; }   // lesson-specific tools this station owns
    public abstract string[] ToolsNow { get; }    // shared + lesson tools that make sense right now

    public abstract void Open();                  // show roots, start at the briefing (Cargo: ChooseCargo)
    public abstract void Close();                 // hide roots, stop animations, pieces back to trays
    public abstract LessonActionResult Advance(); // briefing → chapter 1; inside a chapter: next chapter
    public abstract LessonActionResult Check();
    public abstract LessonActionResult ResetTable();
    public abstract LessonActionResult NextChapter();
    public abstract LessonActionResult RestartChapter();
    public abstract bool TryLessonTool(string name, Newtonsoft.Json.Linq.JObject args, out LessonActionResult result);
}
// LessonActionResult already exists (CargoLessonDirector.cs); reuse it.

// Scripts/Presentation/LessonTheme.cs
[CreateAssetMenu(menuName = "Airlift/Lesson Theme")]
public sealed class LessonTheme : ScriptableObject
{
    public Color deck, trim, accent, accentSoft, prop1, prop2, prop3;
    public Color cardPanel, cardHeading, cardBody, buttonFill, buttonLabel;
    public Material deckMaterial, trimMaterial, accentMaterial;
}
```

Platform also owns: `CargoStation : LessonStation` (wraps OnboardingDirector + CargoLessonDirector; its lesson tools
are `split_cargo`, `check_load`, `reset_cargo`, `replay_demo`), `WelcomeFlow.ActiveLessonId`, pure
`LessonToolRouter` and `StationVisibility`, `NerdyDirector` routing (`public LessonStation[] stations`, found at Start
when unwired), `TableHandle` held-check across stations (`public LessonStation[] stations`), `GuideTools` /
`GuideContextBuilder` overloads carrying `lesson` and `tools_now`, `LessonCatalog` (copy and `Playable` flips),
`sessionConfig.js`, `AgentScripts/WireLessonStations.cs`, and tests `LessonStationRoutingTests`,
`CargoVoiceCharacterizationTests`, `ToolNameSyncTests`.

**Shared tools** (routed to the active station): `request_help`, `advance_step`, `next_chapter`,
`restart_chapter`, `back_to_lessons`. **Lesson tools**: Cargo names unchanged; `cafe_deal_round`, `cafe_check_order`,
`cafe_clear_table`; `garden_turn_bed`, `garden_split_bed {columns: integer 1..6}`, `garden_check_bed`,
`garden_clear_bed`. A lesson tool called in the wrong lesson returns ok false, reason "That is not part of <Title>."

## CE. Café engine (agent: cafe-engine) — `Airlift.Lessons.Cafe`

Files: `Scripts/Lessons/Cafe/CafeChapter.cs`, `Scripts/Lessons/Cafe/CafeModel.cs`,
`Scripts/Welcome/CafeSteps.cs` (namespace `Airlift.Welcome`), tests `CafeModelTests`, `CafeChapterTests`.

```csharp
public enum CafeTargetKind { Plates, Boxes }
public sealed class CafeStage { public CafeTargetKind Kind; public int Containers; public int BoxCapacity; }
public sealed class CafeChapter
{
    public int Number; public string Id, Title, Story, Task, ItemName, ItemPlural;   // "croissant", "croissants"
    public int Items;                      // pastries in play for the whole chapter
    public CafeStage[] Stages;             // chapters 1-4: one stage; chapter 5: plates then boxes
    public string Expression, Accepted;
    public static IReadOnlyList<CafeChapter> All { get; }
}
public sealed class CafeModel
{
    public CafeModel();                                  // chapter 1 started
    public CafeChapter Chapter { get; } public int ChapterIndex { get; } public bool ChapterComplete { get; }
    public bool IsLastChapter { get; } public bool AllChaptersComplete { get; } public int Generation { get; }
    public int StageIndex { get; } public CafeStage Stage { get; }
    public IReadOnlyList<string> ItemIds { get; }        // "item-1".."item-N", stable per chapter
    public int ContainerCount { get; }                   // plates or boxes in the current stage
    public int ContainerOf(string itemId);               // -1 = tray
    public int CountIn(int container);
    public int LooseCount { get; }
    public bool Place(string itemId, bool held, int container); // refuses held/unknown/bad container/full box
    public bool Remove(string itemId);                   // back to the tray
    public bool DealRound(bool held);                    // plates stage only: one loose item onto each plate, in
                                                         // order, while items remain; refuses boxes stage / none loose
    public bool Check();                                 // plates: all placed and equal; boxes: all placed and every
                                                         // used box full. Chapter 5 stage 1 accepted → stage 2 (items
                                                         // back to the tray), stage 2 accepted → chapter complete
    public void ResetTable(); public void RestartChapter(); public bool NextChapter(); public void StartChapter(int i);
    public string LastFeedback { get; } public string Expression { get; }   // earned so far ("" before)
}
// CafeSteps (Airlift.Welcome): static GuideStep Briefing(); static GuideStep ForChapter(CafeModel m) — ids like
// "cafe_chapter3_box_it_up", OnTableNow <= 300 chars naming plates/boxes and counts, CanGrabNow true in chapters.
```

| # | Id | Items | Stages | Expression |
|---|---|---|---|---|
| 1 | two_friends | 6 croissants | Plates ×2 | `6 ÷ 2 = 3` |
| 2 | table_of_three | 12 pastries | Plates ×3 | `12 ÷ 3 = 4` |
| 3 | box_it_up | 12 cookies | Boxes ×5, capacity 4 | `12 ÷ 4 = 3` |
| 4 | bigger_order | 15 muffins | Boxes ×5, capacity 5 | `15 ÷ 5 = 3` |
| 5 | fact_family | 12 muffins | Plates ×4, then Boxes ×6 capacity 3 | `12 ÷ 4 = 3 · 12 ÷ 3 = 4 · 3 × 4 = 12` |

Feedback examples: "2 croissants are still on the tray." · "The plates are not equal yet: 4, 4 and 3." ·
"Box 2 has room for 1 more cookie." · accepted: `Accepted + " " + Expression`.

## GE. Garden engine (agent: garden-engine) — `Airlift.Lessons.Garden`

Files: `Scripts/Lessons/Garden/GardenChapter.cs`, `Scripts/Lessons/Garden/GardenModel.cs`,
`Scripts/Welcome/GardenSteps.cs` (namespace `Airlift.Welcome`), tests `GardenModelTests`, `GardenChapterTests`.
Convention: **rows × columns**.

```csharp
public sealed class GardenChapter
{
    public int Number; public string Id, Title, Story, Task;
    public int Rows, Columns;              // bed at chapter start
    public int[] StripLengths;             // strips in the tray (chapters 1-2); planted strips are generated for 3-5
    public bool StartsPlanted, NeedsTurn, HasFence;
    public int RequiredFenceColumn;        // chapter 4: 5; chapter 5: 0 = any column 1..Columns-1
    public string Expression, Accepted;    // chapter 5 Expression is built at runtime from the fence
    public static IReadOnlyList<GardenChapter> All { get; }
}
public sealed class GardenModel
{
    public GardenModel();
    public GardenChapter Chapter { get; } public int ChapterIndex { get; } public bool ChapterComplete { get; }
    public bool IsLastChapter { get; } public bool AllChaptersComplete { get; } public int Generation { get; }
    public int Rows { get; } public int Columns { get; }  // current, swapped after a turn
    public bool Turned { get; }
    public IReadOnlyList<string> StripIds { get; }        // "strip-1".., stable per chapter (and after a turn)
    public int StripLength(string id);
    public int RowOf(string stripId);                     // -1 = tray
    public string StripInRow(int row);                    // null when empty
    public int PlantCount { get; }
    public bool Plant(string stripId, bool held, int row);// refuses held/unknown/bad row/occupied/longer than Columns;
                                                          // shorter strips are allowed and fail on check
    public bool Unplant(string stripId);
    public bool TurnBed(bool held);                       // only when every row is planted with full strips; swaps
                                                          // Rows/Columns and regenerates strips as rows of Columns
    public int FenceColumn { get; }                       // 0 = no fence
    public bool SetFence(int column, bool held);          // chapters with a fence; 1..Columns-1
    public bool Check();
    public void ResetTable(); public void RestartChapter(); public bool NextChapter(); public void StartChapter(int i);
    public string LastFeedback { get; } public string Expression { get; }
}
// GardenSteps (Airlift.Welcome): static GuideStep Briefing(); static GuideStep ForChapter(GardenModel m).
```

| # | Id | Bed | Tray / start | Accepted when | Expression |
|---|---|---|---|---|---|
| 1 | first_rows | 3 × 4 | strips 4,4,4,3,3 | 3 rows of 4 | `3 × 4 = 12` |
| 2 | equal_rows | 4 × 5 | strips 4,5,5,5,5,6 | 4 rows of 5 | `4 × 5 = 20` |
| 3 | turn_the_bed | 3 × 4 planted | — | Turned (reads 4 × 3) | `3 × 4 = 4 × 3 = 12` |
| 4 | split_the_bed | 7 × 6 planted, fence | — | fence at column 5 | `7 × 6 = 7 × 5 + 7 × 1 = 42` |
| 5 | your_own_split | 8 × 7 planted, fence | — | any fence 1..6 | e.g. `8 × 7 = 8 × 5 + 8 × 2 = 56` |

Feedback examples: "Row 3 is still empty." · "Row 2 has 3 seedlings; the other rows have 4." · "Turn the bed to see it
from the other side." · "Put the fence between two columns first."

## CB / GB. Workbenches (agents: cafe-bench, garden-bench)

Each bench owns: its station (`Scripts/Lessons/Cafe/CafeStation.cs` or `Scripts/Lessons/Garden/GardenStation.cs`,
implementing `LessonStation`), any presentation helpers under `Scripts/Presentation/Cafe/` or `/Garden/`, its builder
`AgentScripts/BuildCafeWorkbench.cs` / `BuildGardenWorkbench.cs`, its theme asset created by the builder at
`Assets/Airlift/Themes/CafeTheme.asset` / `GardenTheme.asset`, a preview script `AgentScripts/PreviewCafeChapters.cs`
/ `PreviewGardenChapters.cs`, and tests `CafeWorkbenchWiringTests` / `GardenWorkbenchWiringTests`.

Builder rules:
- Root object "Cafe workbench" / "Garden workbench", child of "Onboarding workbench - world locked", **inactive by
  default**; station component on the root with `cardId` `neighborhood_cafe_division` /
  `community_garden_multiplication`, `visualRoots = { root }`.
- Own deck with the Cargo "Workbench" footprint (read its mesh bounds), own props in the theme palette (café: cream
  and latte deck, coffee-brown counter, pastel pink/mint/butter props, espresso machine, menu board, awning, guest
  table, delivery bike; garden: grass-green deck, soil-brown raised beds, trees, bushes, wooden fence, sunflowers,
  watering can). Rounded meshes via RoundedBoxMesh only.
- Own card canvas "Cafe card" / "Garden card" with the Lesson interface frame (read "Lesson interface": local position,
  scale 0.001, size 920 × 470), AirliftStyle fonts, heading, body, expression line, say-hints, a button row inside a
  CanvasGroup (in `fallbackGroups`), Back button calling `station.Close()` then `NerdyDirector.OnLessonBack` as
  persistent listeners, `UiPressLog` on every button.
- Grabbable pieces built like `CreateFractionChapter.cs` / `BuildDockWorkbench.cs` quarters; append their
  GrabInteractables to `TableHandle.pieceInteractables`.
- Idempotent (replace named objects), refuse when scenes are dirty, save the scene, return a summary string.

Station behaviour: `Open` shows the briefing (story + "Say yes or press Start"), `Advance` enters chapter 1; drops map
the release point to a plate/box (café) or bed row (garden); lesson tools as listed in P; payoff on accepted checks
(café: plates slide to the guest table, full boxes get "ORDER UP" and leave on the bike; garden: rows grow into
flowers and vegetables, the fence opens); `Close` stops animations and restores trays; `CurrentStep` uses
`CafeSteps` / `GardenSteps`.
