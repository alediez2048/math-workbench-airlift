# Concept intros, Dock 7 splitter, bigger café/garden pieces — contracts (2026-09-17 afternoon)

Owner requests after build 112858 ("starting to look incredibly good"):
1. More voice onboarding once a lesson starts: elementary learners may need a basic intro to what fractions (and
   division, multiplication) are. **Decision: a short concept intro in all three lessons, before chapter 1.**
2. Dock 7: the practice/demo crate is smaller than the lesson crates. **Make it the chunky size (28 x 11 x 12 cm).**
3. Dock 7: splitting only works by voice. **Decision: a splitter station: set a crate on it, choose the chunk size
   (halves or quarters); the right size splits, the wrong size gets a hint. Always visible, even while Dee listens.**
4. Café pastries and garden plants are too small. **Decision: 2x the pieces, keep every chapter's numbers.**

Four implementers work in parallel on **disjoint files**; the main session (integrator) is the only one that runs
Unity, builders, tests and renders.

## Rules for every implementer

- Edit only the files your brief owns. Read anything.
- **No `unity` CLI at all** (no recompile, run_script, run_tests, build). No hand edits to `.unity`/`.asset` YAML.
  You may compile offline with Unity's bundled Roslyn against `unity/Library/ScriptAssemblies/*.dll` and the Unity
  managed DLLs, and run pure NUnit logic under Unity's bundled mono in your scratchpad.
- No git commit, push, stash, checkout or reset. Never revert other people's changes.
- C# 9. Inside `namespace Airlift.*`, `Math` is the `Airlift.Math` namespace: use `Mathf` or `System.Math`.
- Tests first: write the failing EditMode test, then the code. Scene-dependent tests are fine (the integrator runs them
  after running your builder); say which builder must run first.
- Cargo Crew: the owner authorized these changes. The golden `CargoVoiceCharacterizationTests` will change only by
  the new intro steps and the splitter; the integrator re-records after a line-by-line diff. Do not edit the golden.
- Copy: short sentences a 7-10 year old understands; no timers, stars, scores or praise words ("great job");
  math is deterministic C#; Dee never judges correctness.
- Report at the end: files changed, tests added (and which you saw fail first, offline), builders to run in order,
  anything you could not do.

## Shared API (platform agent owns; others code against it now)

`unity/Assets/Airlift/Scripts/Lessons/ConceptIntro.cs`, namespace `Airlift.Lessons`:

```csharp
public sealed class IntroStep
{
    public string Id;          // "whole", "halves", ... unique within a lesson
    public string Heading;     // card heading, e.g. "What is a fraction? · 1 of 4"
    public string Say;         // what Dee says word for word and what the card body shows
    public string Expression;  // optional expression line ("1/2 + 1/2 = 1"), "" when none
    public string Visual;      // station-specific visual id (same as Id unless noted)
}

public static class ConceptIntros
{
    public static readonly IReadOnlyList<IntroStep> Cargo;   // fractions
    public static readonly IReadOnlyList<IntroStep> Cafe;    // division
    public static readonly IReadOnlyList<IntroStep> Garden;  // multiplication
    public static IReadOnlyList<IntroStep> For(string cardId); // empty list for unknown ids
}
```

`LessonStation` additions (platform agent adds; lesson agents override):

```csharp
/// The concept-intro step on the card right now, or null outside the intro.
public virtual IntroStep CurrentIntro => null;
```

Intro content (platform agent writes these exact lines; lesson agents build visuals for the Visual ids):

- **Cargo (fractions)**, before chapter 1, after onboarding's "Start fractions":
  1. `whole` "What is a fraction? · 1 of 4" — "Before we load, let's learn fractions. This crate is one whole container. We write one whole as 1." Visual: the whole crate over the ruler, labelled 1.
  2. `halves` "What is a fraction? · 2 of 4" — "Cut the whole into 2 equal parts. Each part is one half, written 1 over 2. The bottom number tells how many equal parts. The top number tells how many parts you have." Visual: two half crates over the ruler, labelled 1/2.
  3. `sum` "What is a fraction? · 3 of 4" — "Put the two halves together and they fill the whole container again. One half plus one half makes one." Expression "1/2 + 1/2 = 1". Visual: same halves, expression shown.
  4. `quarters` "What is a fraction? · 4 of 4" — "Cut it into 4 equal parts and each part is one quarter, written 1 over 4. More equal parts means smaller parts. Now let's load the trucks." Visual: four quarter crates over the ruler, labelled 1/4.
- **Café (division)**, after the café briefing:
  1. `share` "What is dividing? · 1 of 3" — "Dividing means sharing into equal groups. Here are 6 croissants and 2 plates." Visual: 6 croissants on the tray, 2 plates.
  2. `groups` "What is dividing? · 2 of 3" — "Give one to each plate, again and again, until none are left. Now each plate has 3." Expression "6 ÷ 2 = 3". Visual: 3 croissants on each of 2 plates.
  3. `boxes` "What is dividing? · 3 of 3" — "We can also divide by packing boxes of the same size and counting the boxes. Let's open the café." Visual: 6 croissants in 2 boxes of 3 (or plates cleared if boxes are not ready; say so in the report).
- **Garden (multiplication)**, after the garden briefing:
  1. `row` "What is multiplying? · 1 of 3" — "Multiplying counts equal rows quickly. Here is one row of 4 seedlings." Visual: a 1 × 4 row planted.
  2. `rows` "What is multiplying? · 2 of 3" — "Three equal rows of 4 make 4, 8, 12 plants." Visual: 3 × 4 planted.
  3. `times` "What is multiplying? · 3 of 3" — "We write 3 rows of 4 as 3 times 4, which equals 12. Rows first, then how many in each row. Let's plant." Expression "3 × 4 = 12". Visual: 3 × 4 planted with row labels.

## Station behaviour (each lesson agent, in its station)

- The intro runs **once per app run per lesson**, the first time the learner moves from the briefing (Cargo: from
  onboarding Ready / "Start fractions") toward chapter 1. Returning to the lesson later goes straight to the chapter.
- During the intro: `ChapterActive` is false; `CurrentIntro` is the step; the card heading/body/expression show the
  step's Heading/Say/Expression; the primary button reads **Next** (last step: **Start**); pieces cannot be grabbed;
  `CurrentStep()` returns a `GuideStep` with step "intro", `on_table_now` naming exactly what the visual shows and
  `can_grab_now` false; `ToolsNow` (café/garden) = advance_step, request_help, back_to_lessons.
- `Advance()` moves to the next intro step and returns `new LessonActionResult(true, nextStep.Say)`; after the last
  step it starts chapter 1 exactly as before and returns what it returned before. Every other action during the
  intro refuses with "Let's finish the intro first: say next or press Next."
- `Close()` during the intro restores the table (visual poses undone) and the intro counts as not seen.

## NerdyDirector narration (platform agent)

- Track the open station's `CurrentIntro?.Id`. When it changes to a new step not produced by a voice tool, say it:
  `Say(GuideIntro-style exact words prompt with step.Say)`.
- Voice path: `SubmitLessonResult` marks the current intro step observed and, when an intro step is showing, adds
  `["say_exactly"] = step.Say` to the tool result so Dee reads it word for word once (no double narration).
- Proxy `sessionConfig.js`: "If a tool result contains say_exactly, say it word for word, then stop." plus a node
  test; `GuideSteps`/grounding tests for the intro step if needed.

## Dock 7 splitter (Cargo agent)

- Pure rules `unity/Assets/Airlift/Scripts/Lessons/SplitterRules.cs` (namespace `Airlift.Lessons`):
  `public static LessonActionResult Choose(int chapterSplitTo, int chosenDenominator, bool crateOnPad, bool canSplit, string vehicleNoun)`
  — no crate on the pad: "Set a crate on the splitter first."; chapter has no split (SplitTo 0): "No splitting in
  this chapter."; !canSplit: "Those crates are already split."; chosen != SplitTo: a hint naming the needed size
  ("Each pickup takes half a container. Cut into halves." / "Each van takes a quarter of a container. Cut into
  quarters."); otherwise Ok (the component then calls `CargoLessonDirector.TrySplit()` and returns its result).
- `unity/Assets/Airlift/Scripts/Presentation/CrateSplitter.cs` MonoBehaviour: pad bounds in station space; "crate on
  pad" = any lesson piece (unlocked, not docked, active) whose centre lies over the pad; two world-space pill buttons
  "Halves · 1/2" and "Quarters · 1/4" (UiPressLog), always visible while a chapter is active (never in fallback
  groups), feedback text on the splitter. Voice `split_cargo` keeps working unchanged.
- Placement: front-right of the dock, reachable seated, clear of the crate tray, the exit lane (station z > -0.195
  band), the staging platform, the table handle and the card sightline (< 0.19 m); a whole crate (0.28 x 0.12) fits
  the pad. Builder: in `BuildDockWorkbench.cs` (the integrator reruns it, then WireLessonStations, PolishCards,
  AddLanguageChoice).
- Practice crate (`OnboardingDirector.strap`) and demonstration crate (`demonstrationStrap`) resized in place to the
  chunky crate size (reuse `ChunkyCrate`-style resizing; keep their grab setup, stripes and labels); their tray and
  pad heights follow (`OnboardingContent.trayPosition/targetPosition` y, practice pad placement radius if needed).

## Café and garden 2x pieces (café agent, garden agent)

- Every grabbable piece and its visual is 2x in all three dimensions (café pastries; garden seedling strips and the
  plants on them); containers, trays, plates, boxes, beds, fences, labels and spacing grow so everything still fits the
  1.3 x 0.8 m table, stays reachable seated, clear of the card sightline and the table handle, with the same chapter
  numbers. Update `CafeLayout` / `GardenBedLayout` constants, the builders and the wiring/layout tests.
