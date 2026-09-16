# Cargo Crew — phased ticket backlog

## September 16 (evening) — Phase 0 added, voice un-deferred (draft for owner approval)

Owner accepted the Cargo workbench (toy look, carry handle, whole/halves) and re-scoped:
Phase 0 "Nerdy welcome" comes before more fraction work. Nine new tickets CC-P0-01..09
(see PHASE-0-NERDY-WELCOME.md). Absorbed: P1-02.5 → P0-01; P1-03 → P0-04/P0-07;
P4-01 → P0-08; P4-02/P4-03 (adult) → P0-05/P0-07. P4-04 remains. Then P1-06..P1-10,
Phase 2, Phase 3 in order. Accepted so far by owner report: P1-01/P1-02 outcomes,
P1-04, P1-05 and the P1-06 rebuild check on the headset (tickets not formally closed).
Status: **38 tickets; Phase 0 draft awaiting owner approval; none formally accepted.**


## September 16 handoff status

Implementation paused for owner-requested Claude Code handoff; see
[handoff](HANDOFF-2026-09-16.md). P1-01/P1-02 not accepted; practice grab is broken.
P1-03 deferred by owner. Prioritize P1-04/P1-05 core demo after grab repair; launcher
2.5/polish do not block it. No ticket count or completed status inferred from code.


## September 16 owner priority override

Owner explicitly deferred AI/spoken guidance and associated audio captions, replay,
and mute work. Preserve existing written instructions and fraction labels. Immediate
implementation priority: fix practice grabbing, then deliver the P1-04/P1-05 core
loop: open Cargo Crew, see a stable reachable board, grab a whole labeled 1, split
into two equal pieces labeled 1/2, and manipulate those pieces against a fixed whole.
P1-03/provider work is deferred and is not a dependency of this demo. P1-02.5 and
remaining visual polish must not block this core loop. This overrides historical
sequential/AI-first requirements below; deferred tickets are not accepted or complete.
No new purchases, child-data processing, commits, pushes, or submission authorized.
Physical grab/split acceptance remains required; contest AI requirements remain an
unresolved release consideration, not a reason to block local arithmetic development.

Status: **CC-P1-02 authorized and in progress**. 29 tickets; one active, zero accepted.
Owner added intermediate P1-02.5 for independent headset launch on September 15.
Original phase counts remain historical: Phase 1 now has 10 original tickets plus 2.5.
CC-P1-01 awaits batched review; all later tickets remain draft. Owner approved batching headset reviews at meaningful
checkpoints: code-complete/awaiting device review is not accepted. Later work still
requires owner direction; do not silently skip dependencies.
Local Markdown is the tracker. User-supplied phase counts: 10 in Phase 1, 8 in Phase 2;
proposed later counts: 6 and 4. No café/garden implementation tickets.

One ticket at a time in order; predecessor acceptance is the default dependency.
Exception: P4-01 is an early documentary review (plan approval only); P4-02
requires P3-06 plus P4-01 go and privacy/budget approval. P3-01 may close with an
owner-approved deferral of its enhancement only, without waiving core AI guidance
or independently resolving contest eligibility.
Core AI voice now starts in P1-03; Phase 4 is optional spoken-question input and
child-release readiness, not the first AI guide. See PLAN.md for
scope cuts and review decisions. HITL means owner/device review is needed to close.

## Phase 1 — Guided whole-and-halves chapter (10 tickets)

| Ticket | Outcome | Size | Dependency | Status |
|---|---|---|---|---|
| [CC-P1-01](tickets/CC-P1-01.md) | Preserve and reproduce the working onboarding baseline | M | Owner authorized | awaiting batched review |
| [CC-P1-02](tickets/CC-P1-02.md) | Style Cargo Crew and place a comfortable workbench | L | CC-P1-01; owner approved batched review | in-progress |
| [CC-P1-02.5](tickets/CC-P1-02.5.md) | Find and launch Cargo Crew from the headset | M | P1-02 usable launch | draft / owner requested |
| [CC-P1-03](tickets/CC-P1-03.md) | Deliver the first core AI-guided controller loop | XL | CC-P1-02 | draft |
| [CC-P1-04](tickets/CC-P1-04.md) | Establish one visible whole and exact lesson state | L | owner override: after grab repair | code-complete / awaiting device review (2026-09-16) |
| [CC-P1-05](tickets/CC-P1-05.md) | Split a whole into two labeled halves | L | CC-P1-04 | code-complete / awaiting device review (2026-09-16) |
| [CC-P1-06](tickets/CC-P1-06.md) | Rebuild one whole using two halves | M | CC-P1-05 | draft |
| [CC-P1-07](tickets/CC-P1-07.md) | Complete a half-length delivery independently | M | CC-P1-06 | draft |
| [CC-P1-08](tickets/CC-P1-08.md) | Explain fraction notation with visible and tactile feedback | M | CC-P1-07 | draft |
| [CC-P1-09](tickets/CC-P1-09.md) | Recover the whole-halves journey without losing pieces | L | CC-P1-08 | draft |
| [CC-P1-10](tickets/CC-P1-10.md) | Accept the narrated whole-halves chapter on Quest | M | CC-P1-09 | draft |

## Phase 2 — Quarters, equivalence and comparison (8 tickets)

| Ticket | Outcome | Size | Dependency | Status |
|---|---|---|---|---|
| [CC-P2-01](tickets/CC-P2-01.md) | Split both halves into four quarters | M | CC-P1-10 | draft |
| [CC-P2-02](tickets/CC-P2-02.md) | Build and name three quarters for a parcel | M | CC-P2-01 | draft |
| [CC-P2-03](tickets/CC-P2-03.md) | Join two quarter pieces into a half | L | CC-P2-02 | draft |
| [CC-P2-04](tickets/CC-P2-04.md) | Construct equivalent fractions rather than watch them | L | CC-P2-03 | draft |
| [CC-P2-05](tickets/CC-P2-05.md) | Compare unequal fractions and justify the sign | M | CC-P2-04 | draft |
| [CC-P2-06](tickets/CC-P2-06.md) | Complete fresh delivery checks with faded help | M | CC-P2-05 | draft |
| [CC-P2-07](tickets/CC-P2-07.md) | Add two-button splitting and truthful inspection zoom | L | CC-P2-06 | draft |
| [CC-P2-08](tickets/CC-P2-08.md) | Accept the three-chapter learning flow | M | CC-P2-07 | draft |

## Phase 3 — Complete experience and release evidence (6 tickets)

| Ticket | Outcome | Size | Dependency | Status |
|---|---|---|---|---|
| [CC-P3-01](tickets/CC-P3-01.md) | Validate bounded adaptive hints without AI grading | L | CC-P2-08 | draft |
| [CC-P3-02](tickets/CC-P3-02.md) | Finish the cargo story with a truthful dispatch payoff | M | CC-P3-01 | draft |
| [CC-P3-03](tickets/CC-P3-03.md) | Harden accessible input and interruption recovery | L | CC-P3-02 | draft |
| [CC-P3-04](tickets/CC-P3-04.md) | Measure and meet the Quest 3S performance budget | M | CC-P3-03 | draft |
| [CC-P3-05](tickets/CC-P3-05.md) | Clear package, asset and privacy release gates | M | CC-P3-04 | draft |
| [CC-P3-06](tickets/CC-P3-06.md) | Package the accepted lesson and capture review evidence | M | CC-P3-05 | draft |

## Phase 4 — Gated spoken questions and child-release readiness (4 tickets)

| Ticket | Outcome | Size | Dependency | Status |
|---|---|---|---|---|
| [CC-P4-01](tickets/CC-P4-01.md) | Resolve child-use and provider eligibility | M | Plan approval; documentary exception | draft |
| [CC-P4-02](tickets/CC-P4-02.md) | Add optional spoken questions to the core guide | L | CC-P3-06 + CC-P4-01 go + privacy/budget approval | draft |
| [CC-P4-03](tickets/CC-P4-03.md) | Constrain spoken questions to current lesson help | L | CC-P4-02 | draft |
| [CC-P4-04](tickets/CC-P4-04.md) | Evaluate conversation failures and child-release readiness | M | CC-P4-03 | draft |

## Closing a ticket

Keep the status and completion checklist consistent with the ticket evidence. Record
Why verbatim in DEV-LOG.md. Code-complete is not owner-accepted. A failed gate does
not permit silently skipping its dependency or changing the child audience.
