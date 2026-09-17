# CC-CF-04 — Café voice tools, grounding, catalog facts, Playable on

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** AFK · **Scope size:** M
The guide can run the café by voice with the same actions as the buttons, and the café card becomes playable.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/00-build/CAFE-GARDEN-CONTRACTS.md
and this ticket. You are agent platform (only you edit GuideSteps, GuideContextBuilder, LessonCatalog, sessionConfig.js).
I'm working on CC-CF-04: café tool handlers, grounding, catalog copy and facts, Playable flip, copy patch script.
Run GitNexus impact (upstream) on LessonCatalog and GuideContextBuilder first. No unity CLI, no git.
Implement this ticket only; report under 400 words.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: CC-PL-04 schemas and tools_now live; CC-CF-03 CafeStation playable by buttons.

### Ticket Scope

- Phase: CF. Scope size: M. Dependencies: CC-PL-04, CC-CF-03. Consumer: CC-CF-05.
- Owner agent: platform.
- Reference: Plan "Shared platform: Voice, Briefing, Enable one lesson at a time"; docs/qa/cafe.md.

### Acceptance criteria

- [ ] `cafe_deal_round`, `cafe_check_order`, `cafe_clear_table` reach CafeStation through `TryLessonTool` and call the same methods as the buttons.
- [ ] Every café result and `lesson_state` carries `lesson` and `tools_now`; the guide's verdict text is the model's feedback, never its own.
- [ ] Wrong-lesson refusal: "split it", "load it" or "turn the bed" in the café returns "That is not part of Neighborhood Café." and nothing moves.
- [ ] Grounding: "what do I do now?" names plates or boxes, counts and pastries; never crates, trucks, cranes or seedlings; after an accept it does not ask to grab pastries that left.
- [ ] Briefing: "yes" or `advance_step` enters chapter 1; no grab practice is offered.
- [ ] `LessonCatalog` café entry: `Playable = true`, facts describe sharing and packing; `describe_card` reads them.
- [ ] Baked card text refreshed by a PatchCatalogCopy-style script; the Café card no longer says coming soon.
- [ ] Golden Cargo output unchanged (as agreed in CC-PL-04); `ToolNameSyncTests` green.

### Implementation details

Add a café story rule to `sessionConfig.js` (head barista, fair shares, full boxes; no grading). Grounding text comes
from `CafeSteps` OnTableNow. Tool-call while held returns the station's "Let go of the pastry first." unchanged.
Config: Café story rule and tool descriptions in `sessionConfig.js`; restart the dev mint after merging.

### Key constraints

Server-authored persona and tools stay in the proxy; key never in repo or APK. Adult testers only. Mic only in cards
view and lessons. Never rerun CreateNerdyWelcome. Cargo tool names unchanged.

### Files to modify

- unity/Assets/Airlift/Scripts/Welcome/GuideSteps.cs, unity/Assets/Airlift/Scripts/Guide/GuideContextBuilder.cs
- unity/Assets/Airlift/Scripts/Catalog/LessonCatalog.cs, unity/AgentScripts/PatchCatalogCopy.cs
- services/guide-proxy/lib/sessionConfig.js, services/guide-proxy/test/session.test.mjs
- unity/Assets/Airlift/Tests/EditMode/LessonStationRoutingTests.cs, unity/Assets/Airlift/Tests/EditMode/ToolNameSyncTests.cs

### Files to create

- None expected.

### Testing requirements

Routing tests for each café tool in café, Cargo and cards view; grounding assertions per chapter; ToolNameSyncTests;
GuideGroundingTests and VoiceActionTests still green; golden; `node --test`. Integrator drives the tools by
reflection like `DriveVoiceTools.cs` and captures renders.

### Common gotchas

- Flip `Playable` only after CC-CF-03 passes: a playable card with a broken station strands the learner.
- A stale mint serves old tools: restart it before headset checks.
- WelcomeFlowTests or catalog tests may assert "one playable card": update only non-Cargo expectations, and say so.

### Definition of Done

Code-complete: routing, sync, grounding, golden and proxy tests green. Device acceptance in CC-CF-05.
Expected output: A café the owner can open and play by voice, with wrong-lesson requests politely refused. No new dependencies.

### Why (fill at completion)

