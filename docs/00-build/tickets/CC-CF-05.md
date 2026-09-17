# CC-CF-05 — Café headset acceptance

**Status:** approved plan, not started · **Type:** HITL · **Scope size:** M
The owner plays all five café chapters on the Quest by voice and by button, and Cargo Crew still works.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/qa/cafe.md, CARGO-CREW-LOCK.md
and this ticket. You are the integrator (main session); the owner runs the headset script.
I'm working on CC-CF-05: café integration build, install and owner headset check (target Thu ~18:00 CDT).
Record actual starting state (git status, last accepted hash) before changes.
No push, purchase, child data or submission. Implement this ticket only.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: CC-CF-01..04 code-complete; CC-PL-03 Cargo recheck accepted and committed.

### Ticket Scope

- Phase: CF. Scope size: M. Dependencies: CC-CF-01, CC-CF-02, CC-CF-03, CC-CF-04, CC-PL-03.
- Owner: integrator builds and installs; owner accepts on the headset. Thu 18:00 is also the slip-decision checkpoint.
- Reference: Plan "Team and schedule", "Verification"; docs/qa/cafe.md; docs/qa/nerdy-welcome.md.

### Acceptance criteria

- [ ] Integration: all existing suites (170 checks), café suites, routing and sync suites, `node --test` green; golden Cargo as agreed.
- [ ] Renders from `PreviewCafeChapters.cs` for briefing and chapters 1–5 reviewed; `PreviewDockChapters.cs` unchanged.
- [ ] Wrapper build, APK scan clean, install with md5 match, dev mint restarted, Unity log captured during the run.
- [ ] Owner completes docs/qa/cafe.md: every chapter by voice and by button, deal one round, chapter 5 fact family.
- [ ] Failure cases pass: wrong-lesson tool refused ("split it"), held-pastry refusal, full-box drop returns to tray, unequal plates give descriptive feedback and stay editable.
- [ ] Recovery passes: Mute and Pause bring back the buttons; headset off pauses; Back mid-payoff then reopen is coherent; carry handle moves the café bench and is refused while a pastry is held.
- [ ] Quick Cargo check in the same build: open Cargo Crew, one chapter by voice, back.
- [ ] Owner verdict, build id, APK SHA-256 and issues recorded in DEV-LOG; commit after acceptance.

### Implementation details

Run builders one at a time: BuildCafeWorkbench, then WireLessonStations, then PatchCatalogCopy. Save and close the
scene between steps. Capture `adb logcat -d -s Unity` into the build's artifacts folder after the run.
Config: Restart the dev mint (`~/.config/nerdy/start-dev-mint.sh`) so café tools are served.

### Key constraints

Cargo Crew never regresses. Adult testers only. If time slips, the owner decides at 18:00 (payoffs first, then garden
chapter 5, then café chapter 4); nothing is cut silently.

### Files to modify

- docs/00-build/DEV-LOG.md, docs/00-build/TICKETS.md, scripts/cargo-milestones.json (café suites)

### Files to create

- artifacts/qa/<build-id>/ evidence (APK, hashes, logs, renders)

### Testing requirements

`bash scripts/verify-cargo.sh --suite <name>` per suite, final run with `--build --approved-dirty-build`; `node --test`;
renders; headset script docs/qa/cafe.md.

### Common gotchas

- RefreshAndCompile, then poll recompile status, or tests run on stale assemblies.
- A dirty-scene Save alert stalls the runner; PlayMode needs the wrapper's reload-and-retry.
- Never rerun CreateNerdyWelcome or the old Cargo builders.

### Definition of Done

Accepted only by the owner on the headset with build hash and evidence in DEV-LOG; commit after acceptance; no push.
Expected output: An installed build where the Neighborhood Café card opens a working five-chapter division lesson. No new dependencies.

### Why (fill at completion)

