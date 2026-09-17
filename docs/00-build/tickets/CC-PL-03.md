# CC-PL-03 — CargoStation adapter; NerdyDirector and TableHandle rewired; Cargo identical

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** HITL · **Scope size:** L
Cargo Crew runs through the new station contract and feels exactly the same on the headset.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/00-build/CAFE-GARDEN-CONTRACTS.md,
CARGO-CREW-LOCK.md and this ticket. You are agent platform.
I'm working on CC-PL-03: CargoStation adapter, NerdyDirector routing, TableHandle held-check across stations.
Run GitNexus impact (upstream) on NerdyDirector and TableHandle first and report the risk level.
No unity CLI, no git. CargoLessonDirector.cs does not change. Implement this ticket only; report under 400 words.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: CC-PL-01 golden recorded and committed; CC-PL-02 contract, router and visibility green.

### Ticket Scope

- Phase: PL. Scope size: L. Dependencies: CC-PL-01, CC-PL-02.
- Owner agent: platform. Integrator runs the wiring script and suites; owner rechecks Cargo on the headset (Thu ~12:00).
- Reference: Plan "Shared platform" (CargoStation, Routing, Dedicated workbenches); docs/qa/nerdy-welcome.md Dock 7 section.

### Acceptance criteria

- [ ] `CargoStation : LessonStation` wraps the untouched OnboardingDirector and CargoLessonDirector; its lesson tools are `split_cargo`, `check_load`, `reset_cargo`, `replay_demo`.
- [ ] Cargo-only logic moves out of NerdyDirector into CargoStation; NerdyDirector routes via `public LessonStation[] stations`, found at Start when unwired, active from `WelcomeFlow.ActiveLessonId`.
- [ ] Fallback button groups come from the stations at runtime; they hide while the guide listens and show on Mute, Pause or offline.
- [ ] `TableHandle` refuses carry and resize while any station reports `AnyHeld` (`public LessonStation[] stations`).
- [ ] `CargoVoiceCharacterizationTests` output identical; all 170 checks and `node --test` green.
- [ ] Recovery unchanged: leaving mid-load or after an accepted load and returning gives a coherent chapter (RecoveryTests, DockWorkbenchWiringTests).
- [ ] Owner headset recheck of Cargo: open by voice, chapters 1–2 by voice and by button, carry handle, back. Nothing differs.

### Implementation details

Move code, do not rewrite it: each NerdyDirector Cargo branch becomes a CargoStation method with the same body. Cargo
objects keep their names; `visualRoots` lists the existing Cargo objects so later stations can hide them.
`unity/AgentScripts/WireLessonStations.cs` adds the component and fills the arrays idempotently and saves the scene.
Integrator renders `PreviewDockChapters.cs` before and after to prove the look is unchanged.

### Key constraints

Cargo locked (no changes to CargoLessonDirector, CargoLessonModel, CargoChapter, OnboardingDirector,
BuildDockWorkbench or Cargo test expectations). One active grab interactor per hand. Never rerun CreateNerdyWelcome
or the old Cargo builders. No hand edits to `.unity` YAML.

### Files to modify

- unity/Assets/Airlift/Scripts/Welcome/NerdyDirector.cs, unity/Assets/Airlift/Scripts/Presentation/TableHandle.cs

### Files to create

- unity/Assets/Airlift/Scripts/Lessons/CargoStation.cs, unity/AgentScripts/WireLessonStations.cs

### Testing requirements

Every suite through `bash scripts/verify-cargo.sh`; golden identical; Dock previews compared; wrapper build
`--build --approved-dirty-build`, APK scan, install with md5 check, Unity log captured during the owner recheck.

### Common gotchas

- The table handle re-enables every piece interactable after a carry: keep re-applying locks in LateUpdate.
- NerdyDirector impact is likely HIGH: report it before editing.
- Transform.Find treats "/" as a path; RefreshAndCompile before trusting recompile status.
- Serialized field renames silently drop scene references: keep existing field names.

### Definition of Done

Code-complete: golden identical, all suites green, previews unchanged. Accepted: owner headset recheck recorded in
DEV-LOG with build hash; commit after acceptance; no push.
Expected output: Cargo Crew plays exactly as accepted, now through a station that café and garden can sit beside. No new dependencies.

### Why (fill at completion)

