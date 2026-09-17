# CC-PL-04 — Themes, dedicated workbench roots, card frame; prefixed tools and tools_now

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** AFK · **Scope size:** M
Each lesson can own a themed bench and card, and the voice guide only calls tools that fit the open lesson.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/00-build/CAFE-GARDEN-CONTRACTS.md
and this ticket. You are agent platform.
I'm working on CC-PL-04: LessonTheme, workbench roots and card frame, prefixed lesson tools, lesson + tools_now,
proxy rule, ToolNameSyncTests. Run GitNexus impact (upstream) on GuideContextBuilder and GuideTools first.
No unity CLI, no git. Implement this ticket only; report under 400 words.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: CC-PL-03 CargoStation live, golden identical, owner Cargo recheck done or pending.

### Ticket Scope

- Phase: PL. Scope size: M. Dependencies: CC-PL-02 (code), CC-PL-03 (integration).
- Owner agent: platform. Consumers: cafe-bench, garden-bench (theme and frame), CC-CF-04 and CC-GD-04 (tools).
- Reference: Plan "Shared platform" (Themes, Voice, Briefing); contracts section P.

### Acceptance criteria

- [ ] `LessonTheme` ScriptableObject matches the contract (deck, trim, accent, props, card and button colours, three materials).
- [ ] Opening a non-Cargo station hides the Cargo deck, props, pieces and card while the world-locked root, carry handle, placement and HUD canvas keep working; back restores Cargo.
- [ ] Card frame values ("Lesson interface" local position, scale 0.001, 920 × 470) are exposed in one place benches read.
- [ ] Tool schemas exist for `cafe_deal_round`, `cafe_check_order`, `cafe_clear_table`, `garden_turn_bed`, `garden_split_bed {columns: integer 1..6}`, `garden_check_bed`, `garden_clear_bed`; Cargo names unchanged.
- [ ] `lesson_state` and every tool result carry `lesson` and `tools_now`; in the cards view `tools_now` holds no lesson tools.
- [ ] Proxy rule "inside a lesson call only tools in tools_now" and per-lesson story rules are in `sessionConfig.js`; the proxy test asserts them.
- [ ] `ToolNameSyncTests` fails when a C# tool name is missing from `sessionConfig.js` or the reverse (checked by renaming one locally).
- [ ] Golden Cargo output differs only by the two added keys; the characterization test documents how it compares.

### Implementation details

Extend the GuideTools/GuideContextBuilder overloads rather than changing existing signatures. Tools_now comes from the
active station's `ToolsNow`. The sync test reads `services/guide-proxy/lib/sessionConfig.js` as text and extracts names.
Themes are baked by the bench builders into `Cafe*.mat` / `Garden*.mat`; fonts stay on AirliftStyle.
Config: New tool schemas and the tools_now rule in `sessionConfig.js`; restart the dev mint (`~/.config/nerdy/start-dev-mint.sh`).

### Key constraints

Server-authored persona and tools stay in the proxy; no key in repo or APK. The guide never grades, moves pieces or
advances by itself: every tool calls the same station method as a button. Adult testers only. Cargo locked.

### Files to modify

- unity/Assets/Airlift/Scripts/Welcome/GuideSteps.cs (GuideTools), unity/Assets/Airlift/Scripts/Guide/GuideContextBuilder.cs
- unity/Assets/Airlift/Scripts/Welcome/NerdyDirector.cs, services/guide-proxy/lib/sessionConfig.js, services/guide-proxy/test/session.test.mjs
- unity/Assets/Airlift/Tests/EditMode/CargoVoiceCharacterizationTests.cs (comparison of the two new keys only)

### Files to create

- unity/Assets/Airlift/Scripts/Presentation/LessonTheme.cs, unity/Assets/Airlift/Tests/EditMode/ToolNameSyncTests.cs

### Testing requirements

ToolNameSyncTests, LessonStationRoutingTests, GuideGroundingTests, VoiceActionTests, golden; `node --test` in
services/guide-proxy; full suite before integration. RefreshAndCompile, then poll.

### Common gotchas

- Adding keys changes Cargo JSON: agree the golden comparison with the integrator before merging.
- The mint reads tools only at start: a stale mint serves old schemas.
- Hiding Cargo roots must not hide the shared handle or HUD canvas if they sit under a Cargo object: check parents.

### Definition of Done

Code-complete: sync, routing, grounding and golden suites green; `node --test` green. No device check on its own.
Expected output: The proxy and Unity agree on every tool name, and a café tool can never run inside Cargo Crew. No new dependencies.

### Why (fill at completion)

