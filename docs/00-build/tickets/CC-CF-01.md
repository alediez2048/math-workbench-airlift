# CC-CF-01 — Café engine: plates and boxes models, five chapters, CafeSteps

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** AFK · **Scope size:** M
Deterministic division for the Corner Café: fair sharing onto plates, packing into full boxes, five chapters.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/00-build/CAFE-GARDEN-CONTRACTS.md
(section CE) and this ticket. You are agent cafe-engine. Model the style on CargoLessonModel.cs and CargoChapter.cs.
I'm working on CC-CF-01: CafeChapter, CafeModel, CafeSteps and their tests.
No unity CLI, no git. Compile offline with Unity's bundled Roslyn; run NUnit logic under bundled mono in scratchpad.
Implement this ticket only; report under 400 words.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: no café code exists; `GuideStep` and `GuideSteps` exist in Scripts/Welcome.

### Ticket Scope

- Phase: CF. Scope size: M. Dependencies: contracts only (no Unity scene). Consumers: CC-CF-03, CC-CF-04.
- Owner agent: cafe-engine.
- Reference: Plan "Neighborhood Café"; contracts section CE.

### Acceptance criteria

- [ ] `CafeChapter.All` holds five chapters exactly as the contracts table: two_friends 6 croissants on 2 plates; table_of_three 12 pastries on 3 plates; box_it_up 12 cookies, 5 boxes of 4; bigger_order 15 muffins, 5 boxes of 5; fact_family 12 muffins on 4 plates, then 6 boxes of 3.
- [ ] `Place` refuses held, unknown, bad container and full-box drops and leaves state unchanged; `Remove` returns an item to the tray.
- [ ] `DealRound` puts one loose item on each plate in order while items remain; refuses on a boxes stage, while held, or with nothing loose.
- [ ] `Check` on plates: accepted only when every item is placed and every plate holds the same count; otherwise feedback names what is left, e.g. "2 croissants are still on the tray." or "The plates are not equal yet: 4, 4 and 3."
- [ ] `Check` on boxes: accepted only when every item is boxed and every used box is full; empty spare boxes are fine; feedback like "Box 2 has room for 1 more cookie."
- [ ] Chapter 5: accepted plates stage moves to the boxes stage with items back on the tray; accepted boxes stage completes the chapter; `Expression` builds up to `12 ÷ 4 = 3 · 12 ÷ 3 = 4 · 3 × 4 = 12`.
- [ ] Accepted feedback is `Accepted + " " + Expression`; `Expression` is "" before any acceptance.
- [ ] Wrong arrangements stay editable after a failed check; `ResetTable`, `RestartChapter`, `NextChapter` (refused until complete) and `StartChapter(i)` behave like Cargo's; `Generation` follows CargoLessonModel's meaning.
- [ ] `CafeSteps.Briefing()` and `CafeSteps.ForChapter(m)`: ids like `cafe_chapter3_box_it_up`, OnTableNow at most 300 chars naming plates or boxes and counts, CanGrabNow true in chapters.

### Implementation details

Item ids "item-1".."item-N" stable per chapter. Containers are 0-based internally; feedback says "Plate 1", "Box 2".
Feedback describes the table, never the learner, with correct singular and plural item names.

### Key constraints

Math evaluated only on `Check`; no timers, stars, scores or praise. On-screen copy ASCII plus "·", "—", "×", "÷".
`Math` inside `Airlift.*` is `Airlift.Math`: use `System.Math`. Cargo files locked. Edit only the files below.

### Files to modify

- None.

### Files to create

- unity/Assets/Airlift/Scripts/Lessons/Cafe/CafeChapter.cs, unity/Assets/Airlift/Scripts/Lessons/Cafe/CafeModel.cs
- unity/Assets/Airlift/Scripts/Welcome/CafeSteps.cs (namespace Airlift.Welcome)
- unity/Assets/Airlift/Tests/EditMode/CafeModelTests.cs, unity/Assets/Airlift/Tests/EditMode/CafeChapterTests.cs

### Testing requirements

Failing tests first. Offline mono run in scratchpad, then integrator runs `bash scripts/verify-cargo.sh --suite
CafeModelTests` and `--suite CafeChapterTests`. Cover every chapter's accept path, each refusal and each feedback line.

### Common gotchas

- Chapter 3 has spare boxes: an empty box must not fail the check.
- Chapter 5 stage change must clear placements, not just move the stage index.
- Held pieces: every mutating method takes `held` and refuses without side effects.

### Definition of Done

Code-complete: both suites green in the integrator's run. No device check for this ticket (covered by CC-CF-05).
Expected output: A tested café engine the station and the guide can trust for every verdict. No new dependencies.

### Why (fill at completion)

