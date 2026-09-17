# CC-GD-04 — Garden voice tools, grounding, catalog facts, Playable on

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** AFK · **Scope size:** M
The guide can run the garden by voice with the same actions as the buttons, and the garden card becomes playable.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/00-build/CAFE-GARDEN-CONTRACTS.md
and this ticket. You are agent platform (only you edit GuideSteps, GuideContextBuilder, LessonCatalog, sessionConfig.js).
I'm working on CC-GD-04: garden tool handlers, grounding, catalog copy and facts, Playable flip, copy patch script.
Run GitNexus impact (upstream) on LessonCatalog and GuideContextBuilder first. No unity CLI, no git.
Implement this ticket only; report under 400 words.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: CC-PL-04 schemas live; CC-CF-04 café voice merged; CC-GD-03 GardenStation playable.

### Ticket Scope

- Phase: GD. Scope size: M. Dependencies: CC-PL-04, CC-GD-03 (and CC-CF-04 merged first to avoid conflicts).
- Owner agent: platform.
- Reference: Plan "Shared platform: Voice, Briefing, Enable one lesson at a time"; docs/qa/garden.md.

### Acceptance criteria

- [ ] `garden_turn_bed`, `garden_split_bed {columns: integer 1..6}`, `garden_check_bed`, `garden_clear_bed` reach GardenStation through `TryLessonTool` and call the same methods as the buttons.
- [ ] "Split it at five" places the fence at column 5; an out-of-range or missing column returns the model's refusal and moves nothing.
- [ ] Every garden result and `lesson_state` carries `lesson` and `tools_now`; verdict text is the model's feedback.
- [ ] Wrong-lesson refusal: "deal one round", "split the crate" or "load it" in the garden returns "That is not part of Community Garden." and nothing moves.
- [ ] Grounding: "what do I do now?" names rows, columns, strips or the fence with counts; never crates, trucks, plates or pastries; after growth it does not ask to plant rows that are already grown.
- [ ] Briefing: "yes" or `advance_step` enters chapter 1; no grab practice.
- [ ] `LessonCatalog` garden entry: `Playable = true`, facts describe equal rows, turning and splitting; `describe_card` reads them.
- [ ] Baked card text refreshed by the PatchCatalogCopy-style script; the Garden card no longer says coming soon.
- [ ] Café and Cargo voice results unchanged (golden as agreed; café routing tests still green); `ToolNameSyncTests` green.

### Implementation details

Add a garden story rule to `sessionConfig.js` (head gardener, rows × columns wording, no grading). Parse `columns`
as an integer; non-integer args refuse with a short reason. Grounding comes from `GardenSteps` OnTableNow.
Config: Garden story rule and tool descriptions in `sessionConfig.js`; restart the dev mint after merging.

### Key constraints

Server-authored persona and tools stay in the proxy; key never in repo or APK. Adult testers only. Never rerun
CreateNerdyWelcome. Cargo and café tool names unchanged.

### Files to modify

- unity/Assets/Airlift/Scripts/Welcome/GuideSteps.cs, unity/Assets/Airlift/Scripts/Guide/GuideContextBuilder.cs
- unity/Assets/Airlift/Scripts/Catalog/LessonCatalog.cs, unity/AgentScripts/PatchCatalogCopy.cs
- services/guide-proxy/lib/sessionConfig.js, services/guide-proxy/test/session.test.mjs
- unity/Assets/Airlift/Tests/EditMode/LessonStationRoutingTests.cs, unity/Assets/Airlift/Tests/EditMode/ToolNameSyncTests.cs

### Files to create

- None expected.

### Testing requirements

Routing tests for each garden tool in garden, café, Cargo and cards view; split argument edge cases (0, 6 on 7 × 6, 7,
"five"); grounding per chapter; ToolNameSyncTests; GuideGroundingTests, VoiceActionTests, golden; `node --test`.

### Common gotchas

- Realtime models may send `columns` as a string: accept only whole numbers in range, refuse the rest.
- Flip `Playable` only after CC-GD-03 passes.
- A stale mint serves old tools: restart before headset checks.

### Definition of Done

Code-complete: routing, sync, grounding, golden and proxy tests green. Device acceptance in CC-GD-05.
Expected output: A garden the owner can open and play by voice, with wrong-lesson requests politely refused. No new dependencies.

### Why (fill at completion)

