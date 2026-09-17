# CC-GD-05 — Garden headset acceptance

**Status:** approved plan, not started · **Type:** HITL · **Scope size:** M
The owner plays all five garden chapters on the Quest by voice and by button, and café and Cargo still work.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), docs/qa/garden.md, CARGO-CREW-LOCK.md
and this ticket. You are the integrator (main session); the owner runs the headset script.
I'm working on CC-GD-05: garden integration build, install and owner headset check (target Thu ~23:00 CDT).
Record actual starting state (git status, last accepted hash) before changes.
No push, purchase, child data or submission. Implement this ticket only.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: CC-GD-01..04 code-complete; CC-CF-05 accepted and committed.

### Ticket Scope

- Phase: GD. Scope size: M. Dependencies: CC-GD-01, CC-GD-02, CC-GD-03, CC-GD-04, CC-CF-05.
- Owner: integrator builds and installs; owner accepts on the headset.
- Reference: Plan "Team and schedule", "Verification"; docs/qa/garden.md; docs/qa/cafe.md.

### Acceptance criteria

- [ ] Integration: all existing suites (170 checks), café and garden suites, routing and sync suites, `node --test` green; golden Cargo as agreed.
- [ ] Renders from `PreviewGardenChapters.cs` for briefing, chapters 1–5, mid-turn and after growth reviewed; Dock and café previews unchanged.
- [ ] Wrapper build, APK scan clean, install with md5 match, dev mint restarted, Unity log captured during the run.
- [ ] Owner completes docs/qa/garden.md: every chapter by voice and by button, turn, fence by hand and by "split it at five", own split in chapter 5.
- [ ] Failure cases pass: wrong-lesson tool refused ("deal one round"), held-strip refusal, too-long strip returns to tray, short row gives descriptive feedback and stays editable, wrong fence column in chapter 4 stays editable.
- [ ] Recovery passes: Mute and Pause bring back the buttons; headset off pauses; Back mid-growth then reopen is coherent; carry handle moves the garden bench and is refused while a strip or fence is held.
- [ ] Quick checks in the same build: Cargo Crew one chapter by voice; café one chapter by voice.
- [ ] Owner verdict, build id, APK SHA-256 and issues recorded in DEV-LOG; commit after acceptance.

### Implementation details

Run BuildGardenWorkbench, then WireLessonStations, then PatchCatalogCopy, one at a time with the scene saved between
steps. Capture `adb logcat -d -s Unity` into the build's artifacts folder after the run.
Config: Restart the dev mint (`~/.config/nerdy/start-dev-mint.sh`) so garden tools are served.

### Key constraints

Cargo Crew and the accepted café never regress. Adult testers only. Slip order was decided with the owner at the Thu
18:00 checkpoint (payoffs, then garden chapter 5, then café chapter 4); nothing is cut silently.

### Files to modify

- docs/00-build/DEV-LOG.md, docs/00-build/TICKETS.md, scripts/cargo-milestones.json (garden suites)

### Files to create

- artifacts/qa/<build-id>/ evidence (APK, hashes, logs, renders)

### Testing requirements

`bash scripts/verify-cargo.sh --suite <name>` per suite, final run with `--build --approved-dirty-build`; `node --test`;
renders; headset script docs/qa/garden.md.

### Common gotchas

- RefreshAndCompile, then poll recompile status, or tests run on stale assemblies.
- A dirty-scene Save alert stalls the runner; PlayMode needs the wrapper's reload-and-retry.
- Never rerun CreateNerdyWelcome or the old Cargo builders.

### Definition of Done

Accepted only by the owner on the headset with build hash and evidence in DEV-LOG; commit after acceptance; no push.
Expected output: An installed build where the Community Garden card opens a working five-chapter multiplication lesson. No new dependencies.

### Why (fill at completion)

