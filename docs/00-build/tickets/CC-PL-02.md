# CC-PL-02 — LessonStation contract, tool router, visibility and active lesson id

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** AFK · **Scope size:** M
One abstract station contract lets the app hold three lessons, route voice tools to the open one and show only its bench.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read /Users/jad/.claude/plans/agile-wibbling-nebula.md, docs/00-build/CAFE-GARDEN-CONTRACTS.md (section P)
and this ticket. You are agent platform.
I'm working on CC-PL-02: LessonStation contract, LessonToolRouter, StationVisibility, WelcomeFlow.ActiveLessonId.
Record actual starting state before changes. Run GitNexus impact (upstream) on WelcomeFlow before editing.
No unity CLI, no git. Compile offline with Unity's bundled Roslyn; NUnit logic under bundled mono.
Implement this ticket only; report under 400 words.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: CC-PL-01 test written; NerdyDirector still hard-wired to `cargoLesson` + `onboarding`.

### Ticket Scope

- Phase: PL. Scope size: M. Dependencies: CC-PL-01 (integration only; coding can start in parallel).
- Owner agent: platform. Consumers: cafe-bench, garden-bench (code against this contract now).
- Reference: Plan "Shared platform" (LessonStation, Routing); contracts section P.

### Acceptance criteria

- [ ] `LessonStation` and `LessonChapterFacts` match the contract signatures exactly; `LessonActionResult` is reused.
- [ ] `WelcomeFlow.ActiveLessonId` is set when a playable card opens, cleared on back; a preview card never sets it.
- [ ] `LessonToolRouter.Route`: shared tools (`request_help`, `advance_step`, `next_chapter`, `restart_chapter`, `back_to_lessons`) go to the active station.
- [ ] A lesson tool owned by the active station goes to its `TryLessonTool`; one owned by another station returns ok false with "That is not part of <Title>." and calls nothing.
- [ ] With no active station (cards view) every lesson tool is refused with no side effects; unknown tools keep today's handling.
- [ ] `StationVisibility`: cards view shows no station roots; an active lesson shows exactly its own roots; switching lessons hides the previous roots.
- [ ] `LessonStationRoutingTests` cover all of the above with fake stations in the test assembly (no scene).

### Implementation details

Pure static router and visibility functions over `IReadOnlyList<LessonStation>` plus the active id, so the tests need
no scene. The router adds no held-piece rule: stations own "let go first" refusals. Tool ownership comes from each
station's `ToolNames`; shared names are one constant list the proxy sync test (CC-PL-04) also reads.

### Key constraints

Cargo tool names unchanged. Cargo files locked (see contracts). No timers or stars. Refusal copy describes the table or
lesson, never the learner. ASCII plus "·", "—", "×", "÷" on screen.

### Files to modify

- unity/Assets/Airlift/Scripts/Welcome/WelcomeFlow.cs (ActiveLessonId)

### Files to create

- unity/Assets/Airlift/Scripts/Lessons/LessonStation.cs (LessonStation, LessonChapterFacts)
- unity/Assets/Airlift/Scripts/Lessons/LessonToolRouter.cs, unity/Assets/Airlift/Scripts/Lessons/StationVisibility.cs
- unity/Assets/Airlift/Tests/EditMode/LessonStationRoutingTests.cs

### Testing requirements

Failing tests first. `bash scripts/verify-cargo.sh --suite LessonStationRoutingTests`; WelcomeFlowTests still green;
`CargoVoiceCharacterizationTests` identical. RefreshAndCompile, then poll before trusting results.

### Common gotchas

- `Math` inside `namespace Airlift.*` is `Airlift.Math`: use `Mathf` or `System.Math`.
- Fake stations derive from MonoBehaviour: create them on a GameObject and destroy in TearDown.
- Do not wire NerdyDirector here; that is CC-PL-03.

### Definition of Done

Code-complete: routing suite green in the integrator's run, golden unchanged. No device check for this ticket.
Expected output: A contract the café and garden agents compile against, and tests proving wrong-lesson tools are refused. No new dependencies.

### Why (fill at completion)

