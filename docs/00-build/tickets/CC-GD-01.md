# CC-GD-01 — Garden engine: bed grid, seedling strips, turn, fence split, five chapters, GardenSteps

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** AFK · **Scope size:** M
Deterministic multiplication for the Sunny Plot: equal rows, turning the bed, and splitting it with a fence.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/00-build/CAFE-GARDEN-CONTRACTS.md
(section GE) and this ticket. You are agent garden-engine. Model the style on CargoLessonModel.cs and CargoChapter.cs.
I'm working on CC-GD-01: GardenChapter, GardenModel, GardenSteps and their tests.
No unity CLI, no git. Compile offline with Unity's bundled Roslyn; run NUnit logic under bundled mono in scratchpad.
Implement this ticket only; report under 400 words.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: no garden code exists; `GuideStep` and `GuideSteps` exist in Scripts/Welcome.

### Ticket Scope

- Phase: GD. Scope size: M. Dependencies: contracts only. Consumers: CC-GD-03, CC-GD-04.
- Owner agent: garden-engine. Convention everywhere: rows × columns (a 3 × 4 bed is 3 rows of 4).
- Reference: Plan "Community Garden"; contracts section GE.

### Acceptance criteria

- [ ] `GardenChapter.All` matches the contracts table: first_rows 3 × 4, strips 4,4,4,3,3; equal_rows 4 × 5, strips 4,5,5,5,5,6; turn_the_bed 3 × 4 planted; split_the_bed 7 × 6 planted with fence (required column 5); your_own_split 8 × 7 planted with fence (any column 1..6).
- [ ] `Plant` refuses held, unknown, bad row, occupied row and strips longer than `Columns` (state unchanged); shorter strips plant and fail on check.
- [ ] `Check` on chapters 1–2: accepted only when every row holds a full-length strip; feedback like "Row 3 is still empty." or "Row 2 has 3 seedlings; the other rows have 4."
- [ ] `TurnBed` only when every row is planted with full strips; swaps Rows and Columns, keeps strip ids stable and regenerates strips as rows of the new Columns; refuses while held.
- [ ] Chapter 3 check before turning gives "Turn the bed to see it from the other side."; after turning it is accepted with `3 × 4 = 4 × 3 = 12`.
- [ ] `SetFence` accepts 1..Columns-1 only in fence chapters; out-of-range (e.g. 6 on the 7 × 6 bed, 0, 7) refuses; check without a fence gives "Put the fence between two columns first."
- [ ] Chapter 4 accepts only column 5 (`7 × 6 = 7 × 5 + 7 × 1 = 42`); other columns get feedback naming the parts, stay editable.
- [ ] Chapter 5 accepts any valid column and builds the expression at runtime, e.g. column 5 gives `8 × 7 = 8 × 5 + 8 × 2 = 56`, column 1 gives `8 × 7 = 8 × 1 + 8 × 6 = 56`.
- [ ] `ResetTable`, `RestartChapter`, `NextChapter` (refused until complete), `StartChapter(i)` behave like Cargo's; a restart un-turns the bed and removes the fence.
- [ ] `GardenSteps.Briefing()` / `ForChapter(m)`: ids like `garden_chapter4_split_the_bed`, OnTableNow at most 300 chars naming rows, columns, strips or fence.

### Implementation details

Strip ids "strip-1".. stable per chapter and after a turn. Planted chapters generate one full strip per row. Rows
and fence columns are 1-based in feedback. Feedback describes the bed, never the learner.

### Key constraints

Math only on `Check`; no timers, stars or praise. Copy ASCII plus "·", "—", "×", "÷". Use `System.Math` inside
`Airlift.*`. Cargo files locked. Edit only the files below.

### Files to modify

- None.

### Files to create

- unity/Assets/Airlift/Scripts/Lessons/Garden/GardenChapter.cs, unity/Assets/Airlift/Scripts/Lessons/Garden/GardenModel.cs
- unity/Assets/Airlift/Scripts/Welcome/GardenSteps.cs (namespace Airlift.Welcome)
- unity/Assets/Airlift/Tests/EditMode/GardenModelTests.cs, unity/Assets/Airlift/Tests/EditMode/GardenChapterTests.cs

### Testing requirements

Failing tests first; offline mono run, then integrator runs `bash scripts/verify-cargo.sh --suite GardenModelTests`
and `--suite GardenChapterTests`. Cover each accept path, each refusal, both fence edges and the turn swap.

### Common gotchas

- The voice schema allows columns 1..6 for every chapter; the model must still refuse 6 on the 7 × 6 bed.
- After a turn, rows and columns swap: row indices of planted strips must stay consistent.
- `Expression` stays "" until an accept.

### Definition of Done

Code-complete: both suites green in the integrator's run. Device acceptance in CC-GD-05.
Expected output: A tested garden engine the station and the guide can trust for every verdict. No new dependencies.

### Why (fill at completion)

