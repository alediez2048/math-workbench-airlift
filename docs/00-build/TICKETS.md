# Cargo Crew — phased ticket backlog

## September 17 — Café and Garden (approved plan, in progress)

Owner approved building Neighborhood Café (division: share, pack, fact family) and Community Garden (multiplication:
plant rows, turn the bed, split the bed), five chapters each, each on its own themed workbench, by the Fri 18:00 CDT
internal target. A shared LessonStation platform routes voice tools to the open lesson; Cargo Crew stays locked and
must reproduce its golden voice output. Plan: `/Users/jad/.claude/plans/agile-wibbling-nebula.md`. Signatures and team
rules: [CAFE-GARDEN-CONTRACTS.md](CAFE-GARDEN-CONTRACTS.md). Headset scripts: [cafe](../qa/cafe.md),
[garden](../qa/garden.md). Code-complete is not accepted; acceptance tickets need the owner on the headset.

| Ticket | Outcome | Owner | Dependency | Status |
|---|---|---|---|---|
| [CC-PL-01](tickets/CC-PL-01.md) | Baseline commit + golden Cargo voice characterization test | platform + integrator | lock L-1..L-3; owner lock check | in progress |
| [CC-PL-02](tickets/CC-PL-02.md) | LessonStation contract, router, visibility, active lesson id | platform | CC-PL-01 | in progress |
| [CC-PL-03](tickets/CC-PL-03.md) | CargoStation adapter; NerdyDirector and TableHandle rewired; Cargo identical | platform | CC-PL-01, CC-PL-02 | in progress |
| [CC-PL-04](tickets/CC-PL-04.md) | Themes, workbench roots, card frame; prefixed tools, tools_now, tool sync test | platform | CC-PL-02, CC-PL-03 | in progress |
| [CC-CF-01](tickets/CC-CF-01.md) | Café engine: plates and boxes, 5 chapters, CafeSteps | cafe-engine | contracts | in progress |
| [CC-CF-02](tickets/CC-CF-02.md) | Café workbench, props, pieces, card; builder; wiring tests | cafe-bench | CC-PL-02, CC-PL-04 | in progress |
| [CC-CF-03](tickets/CC-CF-03.md) | CafeStation: briefing, drops, deal one round, check, payoff | cafe-bench | CC-CF-01, CC-CF-02 | in progress |
| [CC-CF-04](tickets/CC-CF-04.md) | Café voice tools, grounding, catalog facts, Playable on | platform | CC-PL-04, CC-CF-03 | in progress |
| [CC-CF-05](tickets/CC-CF-05.md) | Café headset acceptance | integrator + owner | CC-CF-01..04, CC-PL-03 | not started |
| [CC-GD-01](tickets/CC-GD-01.md) | Garden engine: bed grid, strips, turn, fence, 5 chapters, GardenSteps | garden-engine | contracts | in progress |
| [CC-GD-02](tickets/CC-GD-02.md) | Garden workbench, beds, trees, fence, strip pool, card; builder | garden-bench | CC-PL-02, CC-PL-04 | in progress |
| [CC-GD-03](tickets/CC-GD-03.md) | GardenStation: plant strips, turn, fence, check, growth payoff | garden-bench | CC-GD-01, CC-GD-02 | in progress |
| [CC-GD-04](tickets/CC-GD-04.md) | Garden voice tools, grounding, catalog facts, Playable on | platform | CC-PL-04, CC-GD-03 | in progress |
| [CC-GD-05](tickets/CC-GD-05.md) | Garden headset acceptance | integrator + owner | CC-GD-01..04, CC-CF-05 | not started |
| [CC-PL-05](tickets/CC-PL-05.md) | Three-lesson regression, release build, owner full run | integrator + owner | CC-PL-03, CC-CF-05, CC-GD-05 | not started |

| When (CDT) | Milestone | Owner check |
|---|---|---|
| Wed 23:30–01:00 | Tickets and contracts written; engines and platform agents start | — |
| Thu morning | 2-minute lock check, baseline commit; CC-PL-01..03 integrated | lock check |
| Thu ~12:00 | Cargo unchanged after the rewire | headset recheck of Cargo |
| Thu ~18:00 | Café integrated and installed; slip decision if needed | café headset check |
| Thu ~23:00 | Garden integrated and installed | garden headset check |
| Fri 08:00–14:00 | Fixes, CC-PL-05 regression, release build | — |
| Fri 15:00 | Full run of all three lessons | acceptance |
| Fri 18:00 | Internal target | submit decision |

If time slips, the owner decides at Thu 18:00: simplify payoffs first, then garden chapter 5, then café chapter 4.
Cargo Crew never regresses to buy time.

## September 16 (late night) — Cargo Crew locked by owner

Owner accepted the Dock 7 lesson (53647f9) and locked it. Remaining work and ticket decisions:
[CARGO-CREW-LOCK.md](CARGO-CREW-LOCK.md). Next: plan Neighborhood Café and Community Garden.

## September 16 (night) — Phase 1R "Dock 7" drafted (awaiting owner approval)

Owner accepted voice actions on build 190518 and asked for a voice-first workbench, a dock story and
more chapters. Draft plan and tickets CC-D-01..09: [PHASE-1R-DOCK-CREW.md](PHASE-1R-DOCK-CREW.md).
If approved they replace the P1-06..P2-06 order for the deadline; the old tickets they map to are listed
in the plan's chapter table. Nothing implemented from it yet.

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
| [CC-P1-01](tickets/CC-P1-01.md) | Preserve and reproduce the working onboarding baseline | M | Owner authorized | superseded by owner acceptance of the full build (baseline suites pass in wrapper) |
| [CC-P1-02](tickets/CC-P1-02.md) | Style Cargo Crew and place a comfortable workbench | L | CC-P1-01; owner approved batched review | accepted in practice (toy look, handle, cranes); readout removed (L-3) |
| [CC-P1-02.5](tickets/CC-P1-02.5.md) | Find and launch Cargo Crew from the headset | M | P1-02 usable launch | absorbed by CC-P0-01; Library tile logo still open (L-4) |
| [CC-P1-03](tickets/CC-P1-03.md) | Deliver the first core AI-guided controller loop | XL | CC-P1-02 | absorbed by CC-P0-04 / CC-P0-07 |
| [CC-P1-04](tickets/CC-P1-04.md) | Establish one visible whole and exact lesson state | L | owner override: after grab repair | accepted by owner on headset (Dock 7 chapter 1; 53647f9) |
| [CC-P1-05](tickets/CC-P1-05.md) | Split a whole into two labeled halves | L | CC-P1-04 | accepted by owner on headset (chapter 2 split; 53647f9) |
| [CC-P1-06](tickets/CC-P1-06.md) | Rebuild one whole using two halves | M | CC-P1-05 | accepted by owner on headset (chapter 2 rebuild; 53647f9) |
| [CC-P1-07](tickets/CC-P1-07.md) | Complete a half-length delivery independently | M | CC-P1-06 | accepted by owner on headset (vehicle delivery; 2740d8a) |
| [CC-P1-08](tickets/CC-P1-08.md) | Explain fraction notation with visible and tactile feedback | M | CC-P1-07 | lock D-1: fold into the voice guide (recommended; not built) |
| [CC-P1-09](tickets/CC-P1-09.md) | Recover the whole-halves journey without losing pieces | L | CC-P1-08 | lock L-2: code done, headset check pending |
| [CC-P1-10](tickets/CC-P1-10.md) | Accept the narrated whole-halves chapter on Quest | M | CC-P1-09 | accepted by owner (full Dock 7 journey on headset, 53647f9 / 2740d8a) |

## Phase 2 — Quarters, equivalence and comparison (8 tickets)

| Ticket | Outcome | Size | Dependency | Status |
|---|---|---|---|---|
| [CC-P2-01](tickets/CC-P2-01.md) | Split both halves into four quarters | M | CC-P1-10 | accepted by owner on headset (chapter 3 quarters) |
| [CC-P2-02](tickets/CC-P2-02.md) | Build and name three quarters for a parcel | M | CC-P2-01 | lock D-2: deferred |
| [CC-P2-03](tickets/CC-P2-03.md) | Join two quarter pieces into a half | L | CC-P2-02 | lock D-2: deferred |
| [CC-P2-04](tickets/CC-P2-04.md) | Construct equivalent fractions rather than watch them | L | CC-P2-03 | partly accepted: 2/4 = 1/2 (chapter 4); 6/8 not built |
| [CC-P2-05](tickets/CC-P2-05.md) | Compare unequal fractions and justify the sign | M | CC-P2-04 | lock D-2: deferred |
| [CC-P2-06](tickets/CC-P2-06.md) | Complete fresh delivery checks with faded help | M | CC-P2-05 | lock D-2: deferred |
| [CC-P2-07](tickets/CC-P2-07.md) | Add two-button splitting and truthful inspection zoom | L | CC-P2-06 | lock D-3: superseded by voice "split it" + Split fallback button (recommended) |
| [CC-P2-08](tickets/CC-P2-08.md) | Accept the three-chapter learning flow | M | CC-P2-07 | accepted by owner (five-chapter flow on headset) |

## Phase 3 — Complete experience and release evidence (6 tickets)

| Ticket | Outcome | Size | Dependency | Status |
|---|---|---|---|---|
| [CC-P3-01](tickets/CC-P3-01.md) | Validate bounded adaptive hints without AI grading | L | CC-P2-08 | owner decision O-3 pending (voice guide vs hint router) |
| [CC-P3-02](tickets/CC-P3-02.md) | Finish the cargo story with a truthful dispatch payoff | M | CC-P3-01 | accepted by owner (vehicles drive away with crates) |
| [CC-P3-03](tickets/CC-P3-03.md) | Harden accessible input and interruption recovery | L | CC-P3-02 | lock L-2: partial, headset check pending |
| [CC-P3-04](tickets/CC-P3-04.md) | Measure and meet the Quest 3S performance budget | M | CC-P3-03 | lock L-5: todo (owner wears headset) |
| [CC-P3-05](tickets/CC-P3-05.md) | Clear package, asset and privacy release gates | M | CC-P3-04 | lock L-6: inventory in RELEASE-GATES.md; 3 gates open |
| [CC-P3-06](tickets/CC-P3-06.md) | Package the accepted lesson and capture review evidence | M | CC-P3-05 | todo (after lock; needs footage O-4) |

## Phase 4 — Gated spoken questions and child-release readiness (4 tickets)

| Ticket | Outcome | Size | Dependency | Status |
|---|---|---|---|---|
| [CC-P4-01](tickets/CC-P4-01.md) | Resolve child-use and provider eligibility | M | Plan approval; documentary exception | draft |
| [CC-P4-02](tickets/CC-P4-02.md) | Add optional spoken questions to the core guide | L | CC-P3-06 + CC-P4-01 go + privacy/budget approval | draft |
| [CC-P4-03](tickets/CC-P4-03.md) | Constrain spoken questions to current lesson help | L | CC-P4-02 | draft |
| [CC-P4-04](tickets/CC-P4-04.md) | Evaluate conversation failures and child-release readiness | M | CC-P4-03 | draft |

## Phase 5 — Nerdy lounge and onboarding (12 tickets, after the Friday 2026-09-18 submission)

Design: [lounge spec](../superpowers/specs/2026-09-17-nerdy-front-door-design.md). Signatures, file ownership and
team rules: [FRONT-DOOR-CONTRACTS.md](FRONT-DOOR-CONTRACTS.md). Structural reference:
[REFERENCE-THEATRE-ELSEWHERE.md](REFERENCE-THEATRE-ELSEWHERE.md). **One scene, one place**: the app opens in the
Nerdy lounge, Dee appears beside the onboarding cards we already have, the learner picks AR or VR, gets a rundown
of how the experience works, and lands on an expanded dashboard; lessons run as they do today. The owner rejected
the earlier two-scene draft (no scene router, no persistent core, no loading screen). The onboarding and the
rundown ship in full; the VR room is the first thing cut if time contracts. Cargo Crew stays locked apart from the
agreed `LessonStation.Open(int)` addition. **Nothing here is authorized until the Friday submission is done.**

| Ticket | Outcome | Owner | Dependency | Status |
|---|---|---|---|---|
| CC-FD-01 | Lounge root (the home: arrival, onboarding and browsing) from the style tokens; AR/VR choice; existing onboarding cards reparented into it | lounge-bench | Friday submission | done 2026-09-17 (round room, AR/VR choice, cards reparented; LoungeRoomTests/LoungeWiringTests) |
| CC-FD-02 | Dee's seat in the lounge: the existing guide HUD given a place and an accent glow — no body, no mascot | lounge-bench | CC-FD-01 | done 2026-09-17 (Dee's bar sits inside the board's lower band; no body, owner) |
| CC-FD-03 | Arrival: logo animation from the supplied vector, progress bar driven by real startup work | lounge-bench | CC-FD-01 | done 2026-09-17 (LoungeArrival, bar only while StartupWork is pending) |
| CC-FD-04 | WelcomeFlow gains Arrival/Scenery/Rundown/Dashboard; NerdyDirector drives the lounge | director | CC-FD-01 | done 2026-09-18 (WelcomePhase.Rundown; returning learners skip to the wall; WelcomeFlowTests) |
| CC-FD-05a | Controller helper: real controller model, lit button, bouncing arrow; teaches trigger, grip, B/back and "thumbsticks do nothing"; driven by the button-labels setting, reusable inside lessons | rundown | CC-FD-04 | code complete 2026-09-18 (ControllerHelper on the Touch Plus model, BackButtonWatcher; RundownWiringTests) — headset check of the lit button pending |
| CC-FD-05b | The rundown: six gated stops across wall and workbench, always-visible skip, replay from the gear | rundown | CC-FD-05a | code complete 2026-09-18 (RundownScript six gated stops, skip/replay; RundownTests) — headset acceptance pending |
| CC-FD-06 | SettingsStore and LibraryStore: versioned JSON, defaults, clear data | stores | CC-FD-04 | done 2026-09-17 (SettingsState/LibraryState; on disk via LoungeStoreFiles 2026-09-18) |
| CC-FD-07 | Discovery wall: 3 lesson + 15 chapter + 6 coming-soon tiles (Route 9, Ten & Trade, Corner Store, Platform Clock, Tailor's Bench, Mile Marker), Continue ribbon, tile art | dashboard | CC-FD-06 | code complete 2026-09-18 (24 tiles, Continue ribbon, chapter art under Assets/Airlift/Art/Tiles; DashboardTests/DashboardWiringTests) |
| CC-FD-07b | Toolbar under the wall: Featured / Newest / Most viewed, scenery selector and gear in the corner | dashboard | CC-FD-07 | code complete 2026-09-18 (Featured/Newest/Most viewed, pager, scenery selector; the gear is on the bar right under the row) |
| CC-FD-08 | `LessonStation.Open(int chapterIndex)` in all three stations; golden unchanged | director | CC-FD-07 | done 2026-09-18 (Open(int) in all three stations; StationOpenTests; golden unchanged) |
| CC-FD-09 | Settings panel and the dashboard voice tools; proxy redeploy | stores + director | CC-FD-06, CC-FD-07 | code complete 2026-09-18 (SettingsPanel switches/volumes/replay/clear/reset/version; four wall tools; proxy redeployed) |
| CC-FD-10 | LoungeStyleTests, performance run (room budget is a release gate), QA script, headset acceptance | integrator + owner | CC-FD-01..09 | in progress 2026-09-18 (FrontDoorStyleTests; renders reviewed; docs/qa/lounge.md; perf run and headset acceptance pending) |
## Closing a ticket

Keep the status and completion checklist consistent with the ticket evidence. Record
Why verbatim in DEV-LOG.md. Code-complete is not owner-accepted. A failed gate does
not permit silently skipping its dependency or changing the child audience.
