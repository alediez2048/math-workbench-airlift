# CC-PL-05 — Three-lesson regression, release build and owner full run

**Status:** approved plan, not started · **Type:** HITL · **Scope size:** M
All three lessons run end to end from one release APK, and Cargo Crew has not regressed.

## Copy-Paste This Into New Agent

```text
Work only in /Users/jad/Desktop/math-workbench-airlift, branch unity-airlift.
Read the plan (/Users/jad/.claude/plans/agile-wibbling-nebula.md), CARGO-CREW-LOCK.md, RELEASE-GATES.md,
docs/qa/nerdy-welcome.md, docs/qa/cafe.md, docs/qa/garden.md and this ticket. You are the integrator.
I'm working on CC-PL-05: three-lesson regression, release build, owner full run (Fri 15:00 CDT).
Record actual starting state (git status, last accepted hashes) before changes.
No push, purchase, child data or submission. Implement this ticket only.
```

## Context Summary for Agent

### What the previous ticket delivered (at start)

Record actual state. Expected: CC-CF-05 and CC-GD-05 accepted and committed; fixes from Friday morning pending.

### Ticket Scope

- Phase: PL. Scope size: M. Dependencies: CC-PL-03, CC-CF-05, CC-GD-05.
- Owner: integrator (main session) runs everything; owner does the full headset run and the submit decision.
- Reference: Plan "Team and schedule", "Verification"; CARGO-CREW-LOCK.md open items.

### Acceptance criteria

- [ ] All existing suites (170 checks), every new suite and `node --test` green on one commit.
- [ ] `CargoVoiceCharacterizationTests` matches the agreed golden; Dock, café and garden preview renders reviewed.
- [ ] Release build through the wrapper, APK scan clean, install md5 verified, Unity log captured during the owner run.
- [ ] Owner full run: consent → welcome → cards → Cargo chapters 1–5 → back → café chapters 1–5 → back → garden chapters 1–5.
- [ ] Cross-lesson checks: opening café after Cargo shows no crates or trucks; opening Cargo after garden shows no seedlings or fence.
- [ ] Wrong-lesson tools refused in each lesson ("split it" in café and garden, "turn the bed" in Cargo, "deal one round" in garden).
- [ ] Recovery: Mute, Pause, headset off and back mid-chapter in each lesson; carry handle with each bench; back mid-payoff.
- [ ] Offline path ("Continue without voice") completes one chapter per lesson with buttons only.
- [ ] Any cut from the slip order (payoffs, garden chapter 5, café chapter 4) was decided with the owner and recorded.

### Implementation details

Fix only defects found here. Log each issue with lesson, chapter, voice or button, expected and seen. Re-run the
failing script section after each fix, then the full suite once more before the final build.

### Key constraints

Cargo Crew never regresses to buy time. No stretch work while a defect is open. Adult testers only until
PRIVACY-GATE.md records a child-use go. Open lock items (O-1 vercel login, O-2 consent heading, O-3 AI decision,
L-4 Library tile logo, L-5 performance) are reported, not silently closed.

### Files to modify

- docs/00-build/DEV-LOG.md, docs/00-build/TICKETS.md, docs/00-build/RELEASE-GATES.md (status only)

### Files to create

- artifacts/qa/<build-id>/ evidence folder produced by the wrapper (APK, hashes, logs)

### Testing requirements

Every suite through `bash scripts/verify-cargo.sh --suite <name>` (new suites added to scripts/cargo-milestones.json),
the final one with `--build --approved-dirty-build`; `node --test`; previews via PreviewDockChapters,
PreviewCafeChapters, PreviewGardenChapters; headset scripts listed above.

### Common gotchas

- PlayMode suite relies on the wrapper's reload-and-retry; do not trust a zero-result run.
- A dirty scene stalls tests with a Save alert.
- Never rerun CreateNerdyWelcome or the old Cargo builders to "refresh" anything.

### Definition of Done

Accepted: owner full run passes, release APK hash and evidence recorded in DEV-LOG, commit after acceptance. No push;
the submit decision stays with the owner at Fri 18:00.
Expected output: One installed APK the owner has played through all three lessons, with evidence and a commit hash. No new dependencies.

### Why (fill at completion)

