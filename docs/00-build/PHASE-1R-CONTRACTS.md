# Phase 1R — shared code contracts for the agent team (2026-09-16 night)

Plan: `PHASE-1R-DOCK-CREW.md`. Three implementers work in parallel on **disjoint files**; the
integrator (main session) compiles, runs builder scripts, tests, builds and installs. Code against
these signatures exactly. If a contract is impossible, keep the signature, implement the nearest honest
behaviour, and report it.

## Rules for every implementer

- Edit only the files your brief owns. Read anything.
- Do **not** run any `unity` CLI command (no recompile, run_script, run_tests, build, refresh, scene open
  or save). The editor is shared; the integrator runs Unity. Do not hand-edit `.unity` scene files.
- No git commit, push, stash or checkout. Do not delete or revert other people's changes.
- C#: Unity 6000.6, C# 9. Inside `namespace Airlift.*`, `Math` resolves to the `Airlift.Math` namespace:
  use `Mathf` or `System.Math`. Newtonsoft.Json is available to runtime and tests. Tests are NUnit
  EditMode in `unity/Assets/Airlift/Tests/EditMode` (assembly references Airlift.Runtime, TMP, ugui,
  Oculus.Interaction). Every new `.cs` under `Assets` needs no `.meta` by hand (Unity creates it).
- On-screen copy: plain ASCII plus the characters already used in the project ("·", "—"). No emoji.
- Pedagogy invariants: math is deterministic C#; every fraction refers to the visible container (one
  whole = 8 cells); docking is neutral and wrong loads stay editable; feedback describes the load, never
  the learner; no timers, stars, scores or generic praise.
- Before editing an existing class or method, run GitNexus `impact` (repo `math-workbench-airlift`,
  direction upstream) and include the risk level in your report.
- Write the EditMode tests you would want first. You cannot run them; reason carefully about compile
  errors (namespaces, usings, signatures) because the integrator will compile everything at once.
- Final report: files changed, public API added or changed, tests added, anything the integrator must
  run or wire, assumptions, and known risks. Keep it under 400 words.

## A. Chapter engine (pure C#) — `Airlift.Lessons`

```csharp
public sealed class CargoChapter
{
    public int Number;               // 1..5
    public string Id;                // "big_truck" | "two_pickups" | "four_vans" | "same_share" | "top_it_up"
    public string Title;             // "Big truck"
    public string Story;             // one or two sentences of Dock 7 story
    public string Task;              // one sentence: what to do now
    public string VehicleKind;       // "truck" | "pickup" | "van"
    public int VehicleCount;         // 1, 2, 4, 1, 1
    public int[] StartPieces;        // denominators of loose tray pieces at chapter start
    public int[] LockedPieces;       // denominators pre-loaded on the floor and locked (chapter 5: {2})
    public int SplitTo;              // largest denominator Split can reach; 0 = no splitting this chapter
    public FractionValue Target;     // floor quantity that is accepted
    public int RequiredDenominator;  // 0 = any; else every unlocked loaded piece must have it
    public bool ShowHalfMark;        // chapter 4 draws the 1/2 mark on the container floor
    public string Expression;        // shown once accepted, e.g. "1/2 + 1/2 = 1"
    public string Accepted;          // one-sentence payoff, e.g. "Both pickups are loaded."
    public static IReadOnlyList<CargoChapter> All { get; }   // the five chapters in order
}
```

| # | Id | Start | Locked | SplitTo | Target | RequiredDen | Expression |
|---|---|---|---|---|---|---|---|
| 1 | big_truck | {1} | {} | 0 | 1 | 0 | `1` |
| 2 | two_pickups | {1} | {} | 2 | 1 | 2 | `1/2 + 1/2 = 1` |
| 3 | four_vans | {2,2} | {} | 4 | 1 | 4 | `1/4 + 1/4 + 1/4 + 1/4 = 1` |
| 4 | same_share | {4,4,4,4} | {} | 0 | 1/2 | 4 | `2/4 = 1/2` |
| 5 | top_it_up | {4,4,4,4} | {2} | 0 | 1 | 4 | `1/2 + 1/4 + 1/4 = 1` |

**Piece ids are fixed** so views bind by id: `whole`; `half-1`, `half-2`; `quarter-1` … `quarter-4`.
Splitting `whole` gives `half-1`,`half-2`; `half-1` gives `quarter-1`,`quarter-2`; `half-2` gives
`quarter-3`,`quarter-4`. Chapter 5's locked piece is `half-1`; its loose quarters are `quarter-1..4`.

```csharp
public sealed class CargoLessonModel
{
    public const string WholeId = "strap-A";
    public CargoLessonModel();                          // chapter 1 started
    public CargoChapter Chapter { get; }
    public int ChapterIndex { get; }                    // 0-based
    public bool ChapterComplete { get; }
    public bool IsLastChapter { get; }
    public bool AllChaptersComplete { get; }
    public int Generation { get; }                      // bumps on split, reset, restart, chapter start
    public FractionValue RulerQuantity { get; }         // includes locked pieces
    public int RulerCount { get; }
    public IReadOnlyCollection<string> PieceIds { get; }   // every current piece: loose, docked, locked
    public PieceState Piece(string id);
    public bool IsDocked(string id);
    public int DockedIndex(string id);
    public int StartCell(string id);
    public bool IsLocked(string id);
    public string LastFeedback { get; }
    public string Expression { get; }                   // Chapter.Expression when complete, else ""
    public bool CanSplit { get; }                       // chapter allows it and some unlocked piece can split
    public bool Dock(string id, bool held);             // refuses held, unknown, locked, already docked, overfull
    public bool Undock(string id);                      // refuses locked
    public bool Split(bool held, int expectedGeneration);  // undocks unlocked pieces, splits every unlocked
                                                        // piece with denominator < SplitTo one level
    public bool Submit();                               // evaluate floor vs chapter; sets ChapterComplete
    public void ResetPieces();                          // undock all unlocked pieces; keep split level
    public void RestartChapter();                       // back to this chapter's start pieces
    public bool NextChapter();                          // only when ChapterComplete and not last
    public void StartChapter(int index);                // tests and dev
}
```

Submit feedback (reduce fractions for display, e.g. 6/8 → 3/4):
- empty floor: "The container floor is empty. Load crates from 0, then check."
- wrong denominator: e.g. "That fills the container, but the pickups need halves. Split the crate first."
- under target: "That fills 3/4 of the container. 1/4 more fits."
- over target (chapter 4): "That is more than this pickup takes. It needs 1/2 of a container."
- accepted: `Chapter.Accepted + " " + Chapter.Expression`.
`HalfLessonModel` stays untouched.

## B. Workbench (runtime + scene builder) — `Airlift.Lessons` / `Airlift.Presentation`

```csharp
public readonly struct LessonActionResult
{
    public readonly bool Ok; public readonly string Reason;
    public LessonActionResult(bool ok, string reason) { Ok = ok; Reason = reason; }
}

// CargoLessonDirector keeps: Begin(), Exit(), AnyPieceHeld, IsActive, fields whole/halfA/halfB, ruler,
// stationRoot, heading, body, hideWhileActive. REMOVES the old `Stage` property (CargoLessonStage is gone).
public CargoChapter Chapter { get; }
public bool ChapterComplete { get; }
public bool AllChaptersComplete { get; }
public string ExpressionText { get; }
public string Feedback { get; }
public LessonActionResult TrySplit();
public LessonActionResult TryLoad();           // Submit
public LessonActionResult TryReset();
public LessonActionResult TryNextChapter();
public LessonActionResult TryRestartChapter();
public void Split(); public void Submit(); public void ResetPieces(); public void NextChapter(); public void RestartChapter();
public PieceView[] quarters;                   // 4 views, ids quarter-1..4
public TMP_Text expressionLine, sayHints;      // story card extras (heading = chapter title, body = story + task + feedback)
public CanvasGroup chapterButtonGroup;         // Split / Load / Reset / Next / Back row
public VehicleBay vehicles;
```

`body.text` must always contain the chapter's story line and task (the guide reads it). Refusal reasons
are short and spoken as-is, e.g. "Let go of the crate first.", "There is nothing to split in this
chapter.", "Load this container correctly first.", "That was the last chapter."

`VehicleBay` (MonoBehaviour): `ShowChapter(CargoChapter c)`, `MarkLoaded()`, `DispatchAll()`.

`OnboardingDirector` button row gets a `CanvasGroup` created by the builder; the builder wires
`NerdyDirector.fallbackButtons = { onboarding row group, chapterButtonGroup }`.

## C. Voice — `Airlift.Welcome` / guide proxy

```csharp
// GuidePolicy
public static bool ShowFallbackButtons(bool live, bool micOn) => !live || !micOn;
// NerdyDirector
public CanvasGroup[] fallbackButtons;          // alpha 0/1, interactable, blocksRaycasts follow the rule
// GuideSteps
public static GuideStep ForChapter(CargoChapter chapter, bool complete, bool canSplit);
```

Proxy tools added: `replay_demo`, `split_cargo`, `check_load`, `reset_cargo`, `next_chapter`,
`restart_chapter`, `back_to_lessons` (all parameterless). `advance_step` moves the onboarding steps, and
inside a chapter behaves like `next_chapter`. Every handler returns the step result JSON with `ok` and
`reason`, and the guide narrates only what the tool returned.
