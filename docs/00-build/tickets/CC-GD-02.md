# CC-GD-02 — Garden workbench: palette, raised beds, trees, bushes, fence, strip pool, card; builder

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** AFK · **Scope size:** L
A dedicated Sunny Plot bench in grass green, soil brown and sunflower yellow, with its own card, built by one script.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/00-build/CAFE-GARDEN-CONTRACTS.md
(sections P and CB/GB) and this ticket. You are agent garden-bench. Model on BuildDockWorkbench.cs (read only).
I'm working on CC-GD-02: BuildGardenWorkbench.cs, GardenTheme, PreviewGardenChapters.cs, GardenWorkbenchWiringTests.
No unity CLI, no git, no hand edits to .unity/.asset YAML. Implement this ticket only; report under 400 words.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: LessonStation contract (CC-PL-02) and LessonTheme (CC-PL-04) available or in progress.

### Ticket Scope

- Phase: GD. Scope size: L. Dependencies: CC-PL-02, CC-PL-04 (theme type); CC-GD-03 adds behaviour.
- Owner agent: garden-bench. Integrator runs the builder once, then tests and renders.
- Reference: Plan "Community Garden" (Payoff, palette); contracts CB/GB builder rules.

### Acceptance criteria

- [ ] Root "Garden workbench" is a child of "Onboarding workbench - world locked", inactive by default, with GardenStation, `cardId` `community_garden_multiplication`, `visualRoots = { root }`.
- [ ] Own deck with the Cargo "Workbench" footprint; grass-green top, soil-brown raised bed grid, trees and bushes at the edges, wooden fence, sunflowers, watering can, butterfly.
- [ ] Every prop uses RoundedBoxMesh meshes; materials baked as `Garden*.mat`; theme at `Assets/Airlift/Themes/GardenTheme.asset`.
- [ ] The largest bed (8 × 7) and the turned 4 × 3 bed both fit the deck; everything behind the card stays under ~0.19 m; tray strips do not cover bed rows.
- [ ] Strip pool covers chapters 1–2 (lengths 3–6, at most 6 strips) with seedlings spaced one bed column apart; GrabInteractables appended to `TableHandle.pieceInteractables`.
- [ ] Grabbable fence divider sized to the tallest bed, with a rest position off the bed.
- [ ] "Garden card" canvas uses the Lesson interface frame (scale 0.001, 920 × 470), AirliftStyle fonts, heading, body, expression line, say-hints.
- [ ] Button row inside a CanvasGroup listed in `fallbackGroups`: Start (briefing), Turn, Split, Check, Clear, Restart, Next, Back; `UiPressLog` on every button; Back calls `station.Close()` then `NerdyDirector.OnLessonBack` as persistent listeners.
- [ ] Builder is idempotent, refuses when a scene is dirty, saves the scene and returns a summary string.

### Implementation details

Bed cells are generated per chapter by the station from Rows × Columns; the builder provides the bed parent, cell
prefab mesh and row snap zones sized for 8 rows. Trees stay at the back corners, outside the card sightline band.

### Key constraints

Cargo objects untouched; never rerun BuildDockWorkbench, CreateNerdyWelcome or the old Cargo builders. One active grab
interactor per hand. AirliftStyle fonts for `CargoTerminalLayoutTests.CaptionsFitAndFontsBound`. No timers or stars.

### Files to modify

- None outside owned files.

### Files to create

- unity/AgentScripts/BuildGardenWorkbench.cs, unity/AgentScripts/PreviewGardenChapters.cs
- unity/Assets/Airlift/Scripts/Presentation/Garden/* (helpers), unity/Assets/Airlift/Tests/EditMode/GardenWorkbenchWiringTests.cs
- Builder outputs: Assets/Airlift/Themes/GardenTheme.asset, Garden*.mat

### Testing requirements

`GardenWorkbenchWiringTests`: hierarchy, inactive root, card frame, fonts, fallback group, persistent Back listeners,
UiPressLog, interactables registered, sightline for 8 × 7 and turned beds, strips clear of rows, idempotence.
CargoTerminalLayoutTests and DockWorkbenchWiringTests still green. Integrator reviews renders.

### Common gotchas

- Transform.Find treats "/" as a path: name rows "Row 1", never "Row 1/8".
- Trees and sunflowers are tall: keep them out of the band behind the translucent card.
- The table handle re-enables every piece interactable after a carry: the station re-applies locks in LateUpdate.

### Definition of Done

Code-complete: wiring tests green, renders reviewed. Headset look is accepted in CC-GD-05.
Expected output: A garden bench that appears only when the garden opens and fits every bed size. No new dependencies.

### Why (fill at completion)

