# CC-GD-03 — GardenStation: plant strips, turn animation, fence placement, check, growth payoff

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** AFK · **Scope size:** L
The garden bench plays: strips dropped into rows, the bed turned, a fence placed, checked by the engine, then it grows.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/00-build/CAFE-GARDEN-CONTRACTS.md
and this ticket. You are agent garden-bench. Read CargoLessonDirector.cs for patterns (do not edit it).
I'm working on CC-GD-03: GardenStation implementing LessonStation over GardenModel.
No unity CLI, no git. Implement this ticket only; report under 400 words.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: CC-GD-01 engine green; CC-GD-02 bench built; CC-PL-02 contract available.

### Ticket Scope

- Phase: GD. Scope size: L. Dependencies: CC-GD-01, CC-GD-02, CC-PL-02. Consumer: CC-GD-04.
- Owner agent: garden-bench.
- Reference: Plan "Community Garden"; contracts CB/GB station behaviour; docs/qa/garden.md.

### Acceptance criteria

- [ ] `Open` shows the root and the briefing (story + "Say yes or press Start"); `Advance` enters chapter 1 with a 3 × 4 bed.
- [ ] Releasing a strip over a row calls `GardenModel.Plant`; a refused drop (too long, occupied, off the bed) returns it to the tray with the model's feedback.
- [ ] Short strips plant visibly short and fail on Check (`garden_check_bed`) with row feedback; the learner can pull them out and replant.
- [ ] Turn (button and `garden_turn_bed`) animates a quarter turn; afterwards the bed reads 4 × 3 and the counts are unchanged; refused with a one-line reason when rows are not full.
- [ ] Fence: placing it by hand snaps to the nearest column boundary; releasing it off the bed returns it to rest with no fence; `garden_split_bed {columns}` places it at that column.
- [ ] Check shows both parts and the total once accepted; chapter 4 rejects columns other than 5 with descriptive feedback.
- [ ] Payoff on accept: rows sprout flowers and vegetables (scale-up growth), the watering can passes, a butterfly lands; the fence opens.
- [ ] Clear (`garden_clear_bed`), Restart, Next (refused until complete) and Back work mid-turn and mid-growth; Close stops animations and restores trays.
- [ ] Every bed-changing action while a strip or the fence is held refuses with "Let go of the strip first." and changes nothing.
- [ ] Leave mid-chapter or after an accept, reopen: the bed state is coherent; after a table-handle carry, locks are re-applied and `AnyHeld` blocks the carry.

### Implementation details

`CurrentStep` uses `GardenSteps`. `ToolNames` = the four garden tools; `ToolsNow` = shared tools plus only what fits
(turn in chapter 3, split in chapters 4–5, check and clear in chapters). Visuals sync from model state after every
action, so voice and buttons share one path. Closing mid-turn snaps to the model's final state first.

### Key constraints

Math only in GardenModel; the station never decides a verdict. Rows × columns everywhere. No timers, stars or praise.
Payoff is truthful: only accepted rows grow. Cargo locked. One active grab interactor per hand.

### Files to modify

- unity/AgentScripts/BuildGardenWorkbench.cs (wire station references)

### Files to create

- unity/Assets/Airlift/Scripts/Lessons/Garden/GardenStation.cs, unity/Assets/Airlift/Scripts/Presentation/Garden/* (turn, growth)

### Testing requirements

Extend `GardenWorkbenchWiringTests`: open → briefing → chapters 1–2 by model-driven drops → check; turn; fence at 5
and at a wrong column; chapter 5 any column; held refusal; close mid-turn then reopen. Integrator runs
`PreviewGardenChapters.cs` for every chapter, mid-turn and after growth; RecoveryTests and DockWorkbenchWiringTests green.

### Common gotchas

- Map drops by release point to the row centre, not trigger overlap.
- The turned bed changes footprint: re-run the sightline check on the 4 × 3 layout.
- The table handle re-enables interactables after a carry: re-apply locks (planted strips, fence at rest) in LateUpdate.

### Definition of Done

Code-complete: wiring tests and renders green. Device acceptance in CC-GD-05.
Expected output: A playable garden by buttons and hands, ready for voice tools. No new dependencies.

### Why (fill at completion)

