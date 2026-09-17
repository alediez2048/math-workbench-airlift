# CC-CF-02 — Café workbench: palette, props, plates, boxes, pastry pool, card; builder

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** AFK · **Scope size:** L
A dedicated Corner Café bench in cream, latte, coffee brown and pastels, with its own card, built by one idempotent script.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/00-build/CAFE-GARDEN-CONTRACTS.md
(sections P and CB/GB) and this ticket. You are agent cafe-bench. Model on BuildDockWorkbench.cs (read only).
I'm working on CC-CF-02: BuildCafeWorkbench.cs, CafeTheme, PreviewCafeChapters.cs, CafeWorkbenchWiringTests.
No unity CLI, no git, no hand edits to .unity/.asset YAML. Implement this ticket only; report under 400 words.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: LessonStation contract (CC-PL-02) and LessonTheme (CC-PL-04) available or in progress.

### Ticket Scope

- Phase: CF. Scope size: L. Dependencies: CC-PL-02, CC-PL-04 (theme type); CC-CF-03 adds behaviour on top.
- Owner agent: cafe-bench. Integrator runs the builder once, then tests and renders.
- Reference: Plan "Neighborhood Café" (Payoff, palette); contracts CB/GB builder rules; CC-P0-06.

### Acceptance criteria

- [ ] Root "Cafe workbench" is a child of "Onboarding workbench - world locked", inactive by default, with CafeStation, `cardId` `neighborhood_cafe_division`, `visualRoots = { root }`.
- [ ] Own deck with the Cargo "Workbench" footprint (read from its mesh bounds); cream and latte deck, coffee-brown counter, pastel pink, mint and butter props, espresso machine, menu board, awning, guest table, delivery bike.
- [ ] Every prop uses RoundedBoxMesh meshes (no raw cubes); materials baked as `Cafe*.mat`; theme at `Assets/Airlift/Themes/CafeTheme.asset`.
- [ ] Anything behind the card stays under board height ~0.19 m (sightline test); plates and box marks are not covered by tray pastries.
- [ ] Up to 4 plates, 6 boxes with capacity labels and a pool of 15 grabbable pastries, built like the Dock quarters; their GrabInteractables appended to `TableHandle.pieceInteractables`.
- [ ] "Cafe card" canvas uses the Lesson interface frame (scale 0.001, 920 × 470), AirliftStyle fonts, heading, body, expression line, say-hints.
- [ ] Button row inside a CanvasGroup listed in `fallbackGroups`: Start (briefing), Deal round, Check, Clear, Restart, Next, Back; `UiPressLog` on every button; Back calls `station.Close()` then `NerdyDirector.OnLessonBack` as persistent listeners.
- [ ] Builder is idempotent (second run leaves identical counts), refuses when a scene is dirty, saves the scene and returns a summary string.

### Implementation details

Place props around the edges, keeping the centre clear for plates and boxes. Colliders and snap zones mirror the Dock
bench. The preview script positions the camera at the seated head and renders briefing plus each chapter's layout.

### Key constraints

Cargo objects untouched; `BuildDockWorkbench.cs`, CreateNerdyWelcome and old Cargo builders never rerun. One active
grab interactor per hand. Fonts on AirliftStyle so `CargoTerminalLayoutTests.CaptionsFitAndFontsBound` passes.
Copy ASCII plus "·", "—", "×", "÷". No timers or stars.

### Files to modify

- None outside owned files (TableHandle array is filled by the builder at run time, not by editing TableHandle.cs).

### Files to create

- unity/AgentScripts/BuildCafeWorkbench.cs, unity/AgentScripts/PreviewCafeChapters.cs
- unity/Assets/Airlift/Scripts/Presentation/Cafe/* (helpers), unity/Assets/Airlift/Tests/EditMode/CafeWorkbenchWiringTests.cs
- Builder outputs: Assets/Airlift/Themes/CafeTheme.asset, Cafe*.mat

### Testing requirements

`CafeWorkbenchWiringTests`: hierarchy, inactive root, card frame, fonts bound, fallback group, persistent Back
listeners, UiPressLog, interactables registered, sightline, marks clear, idempotence. Existing CargoTerminalLayoutTests
and DockWorkbenchWiringTests still green. Renders in artifacts reviewed by the integrator.

### Common gotchas

- Transform.Find treats "/" as a path: never name objects like "Plate 1/2".
- The table handle re-enables every piece interactable after a carry: the station re-applies locks in LateUpdate.
- Tall props (espresso machine, menu board, awning) sit beside or behind the sightline band, not behind the card.

### Definition of Done

Code-complete: wiring tests green, renders reviewed. Headset look is accepted in CC-CF-05.
Expected output: A café bench that appears only when the café opens and matches the lesson's palette. No new dependencies.

### Why (fill at completion)

