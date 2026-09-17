# CC-CF-03 — CafeStation: briefing, chapters, drops, deal one round, check, payoff

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** AFK · **Scope size:** L
The café bench plays: pastries dropped on plates or into boxes, checked by the engine, with a served-order payoff.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/00-build/CAFE-GARDEN-CONTRACTS.md
and this ticket. You are agent cafe-bench. Read CargoLessonDirector.cs for patterns (do not edit it).
I'm working on CC-CF-03: CafeStation implementing LessonStation over CafeModel.
No unity CLI, no git. Implement this ticket only; report under 400 words.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: CC-CF-01 engine green; CC-CF-02 bench built; CC-PL-02 contract available.

### Ticket Scope

- Phase: CF. Scope size: L. Dependencies: CC-CF-01, CC-CF-02, CC-PL-02. Consumer: CC-CF-04.
- Owner agent: cafe-bench.
- Reference: Plan "Neighborhood Café"; contracts CB/GB station behaviour; docs/qa/cafe.md.

### Acceptance criteria

- [ ] `Open` shows the root and the briefing (story + "Say yes or press Start"); `ChapterActive` false; `Advance` enters chapter 1.
- [ ] Releasing a pastry over a plate or box calls `CafeModel.Place`; a refused drop (full box, bad target) or a drop off every target returns it to the tray and shows the model's feedback.
- [ ] Deal round (button and `cafe_deal_round`) runs `DealRound` and animates the pastries onto plates; on a boxes stage it refuses with a one-line reason.
- [ ] Check (button and `cafe_check_order`) shows feedback; wrong arrangements stay editable; accepted shows `Accepted + " " + Expression`.
- [ ] Payoff on accept: served plates slide to the guest table with steam puffs; full boxes get an "ORDER UP" tag and leave on the delivery bike.
- [ ] Chapter 5: the accepted plates stage returns muffins to the tray and swaps plates for 6 boxes of 3; the second accept completes the lesson.
- [ ] Clear (`cafe_clear_table`), Restart, Next (refused until complete) and Back work mid-chapter and mid-payoff; Close stops animations and restores trays.
- [ ] Every table-changing action while a pastry is held refuses with "Let go of the pastry first." and changes nothing.
- [ ] Leave mid-chapter or after an accept, reopen: the chapter state is coherent (no pastries stuck on the bike or guest table).
- [ ] After a table-handle carry, pieces that should be locked stay locked; `AnyHeld` blocks the carry.

### Implementation details

`CurrentStep` returns `CafeSteps.Briefing()` or `CafeSteps.ForChapter(model)`. `ToolNames` = the three café tools;
`ToolsNow` = shared tools plus only the café tools that make sense (no deal round on a boxes stage, none in briefing).
Visuals sync from model state after every action (single `Sync()`), so voice and buttons share one path.

### Key constraints

Math only in CafeModel; the station never decides a verdict. No timers, stars or praise. Payoff is truthful: only
accepted plates and boxes move. Cargo locked. One active grab interactor per hand.

### Files to modify

- unity/AgentScripts/BuildCafeWorkbench.cs (wire station references)

### Files to create

- unity/Assets/Airlift/Scripts/Lessons/Cafe/CafeStation.cs, unity/Assets/Airlift/Scripts/Presentation/Cafe/* (payoff helpers)

### Testing requirements

Extend `CafeWorkbenchWiringTests`: open → briefing → each chapter solved by model-driven drops → check → payoff state;
refused drop returns to tray; held refusal; close mid-payoff then reopen; deal round refused on boxes. Integrator
runs `PreviewCafeChapters.cs` for every chapter and after each payoff; RecoveryTests and DockWorkbenchWiringTests green.

### Common gotchas

- Map drops by release point, not by trigger overlap, or a pastry brushing two plates counts twice.
- The table handle re-enables interactables after a carry: re-apply locks in LateUpdate.
- Close during the bike animation must return boxes and pastries before hiding the root.

### Definition of Done

Code-complete: wiring tests and renders green. Device acceptance in CC-CF-05.
Expected output: A playable café by buttons, ready for voice tools. No new dependencies.

### Why (fill at completion)

