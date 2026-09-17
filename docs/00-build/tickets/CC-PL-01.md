# CC-PL-01 — Baseline commit and golden Cargo voice characterization

**Status:** approved plan, in progress (agent started 2026-09-17) · **Type:** HITL · **Scope size:** S
Lock today's accepted Cargo Crew in a commit and a byte-for-byte voice test before any routing changes.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read /Users/jad/.claude/plans/agile-wibbling-nebula.md, docs/00-build/CAFE-GARDEN-CONTRACTS.md,
docs/00-build/CARGO-CREW-LOCK.md and this ticket. You are agent platform (test author).
I'm working on CC-PL-01: baseline commit + golden Cargo voice characterization test.
Record actual starting state (git status, installed build, uncommitted ownership) before changes.
No unity CLI, no git; the main session runs Unity, records the golden output and commits.
Implement this ticket only; report under 400 words (files, API, tests, run order, risks).
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state here. Expected: lock work L-2/L-3/L-7 (pause gate, headset-off pause, readout off,
RecoveryTests; build 223313) uncommitted on top of 2740d8a.

### Ticket Scope

- Phase: Café and Garden, platform (PL). Scope size: S. No speculative time estimate.
- Dependencies: CARGO-CREW-LOCK L-1..L-3; owner 2-minute lock check. Blocks integration of CC-PL-02..04.
- Owner agent: platform writes the test; integrator captures the golden text, runs suites, commits.
- Reference: Plan "Shared platform" (Baseline first, Golden Cargo voice test); CARGO-CREW-LOCK.md L-2; docs/qa/nerdy-welcome.md.

### Acceptance criteria

- [ ] Owner lock check passes: Pause while the guide talks silences it until Play; headset off pauses the guide, on + Play resumes.
- [ ] Baseline commit on unity-airlift holds the lock changes; hash recorded in DEV-LOG and TICKETS.md. No push.
- [ ] All existing suites (170 checks) and `node --test` in services/guide-proxy are green on that commit.
- [ ] `CargoVoiceCharacterizationTests` drives the whole Cargo script (open, yes, replay demo, start, every chapter's split, load, check, reset, next, restart, held-crate refusal, back) and compares each tool result and `lesson_state` JSON with the golden text byte for byte.
- [ ] Failure case: a one-character change to a Cargo tool result (tried locally, reverted) turns the test red with the failing step named.
- [ ] Two consecutive runs give identical output (no timestamps, generation counters or float noise in the golden).

### Implementation details

Model the drive on `unity/AgentScripts/DriveVoiceTools.cs`: open CargoCrew, find NerdyDirector, invoke its tool
handler by reflection, serialize with Newtonsoft `Formatting.None`. Normalize only provably non-deterministic
fields and list them in the test header. Integrator: run once to capture, paste the golden, run twice green.

### Key constraints

Cargo Crew is locked: no edits to CargoLessonDirector, CargoLessonModel, CargoChapter, OnboardingDirector,
BuildDockWorkbench or any Cargo test expectation. Deterministic math in C#. Adult testers only. Never rerun
CreateNerdyWelcome or the old Cargo builders. Commit only after the owner check; never push.

### Files to modify

- docs/00-build/DEV-LOG.md, docs/00-build/TICKETS.md (baseline hash; integrator)

### Files to create

- unity/Assets/Airlift/Tests/EditMode/CargoVoiceCharacterizationTests.cs (golden inline or a sibling TextAsset; note which)

### Testing requirements

`bash scripts/verify-cargo.sh --suite CargoVoiceCharacterizationTests`, then every suite and `node --test`.
RefreshAndCompile, then poll recompile status before trusting any result. Evidence in DEV-LOG.

### Common gotchas

- A dirty-scene Save alert stalls the runner: leave the scene clean after the drive.
- Refuse-while-held cases must be driven through the model's held flag, not physics, or output varies.
- CC-PL-04 adds `lesson` and `tools_now` to every result: agree then how the golden compares (see that ticket).

### Definition of Done

Code-complete: test green twice in the integrator's run. Accepted: owner lock check passed, baseline hash
recorded, 170 checks + `node --test` green. No push.
Expected output: A commit the owner can return to, and a red test the moment Cargo voice behaviour drifts. No new dependencies.

### Why (fill at completion)

