# Cargo Crew fraction experience — implementation plan


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

**Mode:** revision · **Size:** XL · **RAP tier:** Deep.
**Status:** draft for owner review; no implementation authorized.
Research extends the existing [RAP run](../plans/2026-09-13-unity-vet/plan.md):
[revision evidence](../plans/2026-09-13-unity-vet/fraction-expansion-research.md).
[Verbatim brief](../plans/2026-09-13-unity-vet/fraction-expansion-brief.md).
Detailed contracts: PRD.md, STORYBOARD.md, systemsdesign.md and TICKETS.md.

## Owner update — core AI voice and visual design

AI voice guidance is now required throughout the lesson, beginning in P1-03—not
an optional Phase 4 extra. OpenAI is the preferred evaluation candidate; recorded
narration without live model selection is fallback only. The core guide selects
reviewed spoken explanations from current lesson context. See AI-VOICE-GUIDE.md for authoritative acceptance and
privacy boundaries. Spoken questions remain separate. P1-02 also owns the new
typography/visual-design review in STYLE-GUIDE.md (VISUAL-DESIGN.md is its brief). Counts remain 10/8/6/4. The owner also requires a recognizable miniature cargo
terminal from entry: P1-02 props, P1-03 role orientation, P1-07 accepted-job parcel
progress and P3-02 final dispatch. See STYLE-GUIDE and F-21.

## Summary

Evolve the working Cargo Crew onboarding into a complete narrated mixed-reality
fraction lesson, one accepted ticket at a time. Reuse Meta interactions, author
exact length pieces and deterministic learning rules, and connect voice/captions/
pointers to state. Start with a complete whole-and-halves loop; add quarters,
equivalence and comparison only after that loop works on the actual Quest.

## Assumptions and approval

Five grouped decisions; the core AI-guide requirement is now owner-confirmed,
while the remaining implementation defaults remain assumptions for review:
1. Provisional Grade 4/approximately age-10 persona; adult-owner QA only.
2. Owner-confirmed core AI voice guide. Proposed provider: OpenAI; actual model,
   account access and transport still require feasibility/privacy/budget checks.
   Learner microphone input is not required for event-aware spoken guidance.
3. “Bigger/smaller” means exact split/merge plus whole-model zoom, not independent
   continuous changes to quantity.
4. Initial whole 0.40 m, zoom 0.8–1.2 and chord window 250 ms; tune only with device evidence.
5. Later phases have 6 and 4 tickets; first phases honor the requested 10 and 8.

Owner review precedes CC-P1-01. Decisions below are the recommended execution
contract **if approved**, not claims that unanswered preferences were locked.
Existing deterministic grading/privacy/safety requirements remain binding.

## Drift and preserved decisions

| Prior design / conversation | Actual current state | Action |
|---|---|---|
| Fraction lesson with adaptive scaffolds | Unwired math drafts; onboarding only | Core AI in P1-03; exact math from P1-04; enhance routing in P3 |
| Recorded-first guide / AI deferred | No audio; owner now requires AI along the lesson | Core event-aware AI output in P1; microphone questions separately gated |
| Quest 3 / 6.3 candidate | Quest 3S / installed 6000.6.0f1 tuple | Keep proven installed foundation |
| Cargo Grid stretch | User now wants Cargo Crew focus | Propose no other implemented lesson |
| Flat docs/progress | Existing root docs plus historical log | Adopt local 00-build review package and next-entry devlog |
| New onboarding physically pending in docs | Owner confirmed catalog, briefing, practice and replay | Update only those checks; full QA still open |

KEPT: URP, stationary MR, controllers, exact prefab partitions, deterministic C#,
neutral independent snapping, no child data, secret stripping, physical-device gates.

## Research that changes the plan

- Installed SDK supports grabs/rays/haptics, but two-grab manipulation is not a
  semantic merge. D3 owns lineage/atomic swaps. Found H.
- Button masks test any button, not both. D4 separately tests both faces with
  edge/rearm guards. Found H; comfort unknown.
- Same-whole representations and generated equivalents align with IES/standards.
  D2/D6 preserve reference/notation and require action/explanation. Found H; no
  product-effectiveness inference.
- ElevenLabs policy restricts under-13-targeted bundles. Narration cannot simply
  be generated now and called offline-safe. D7 uses an authorized recording fallback;
  P4-01 clears or rejects provider-specific use. Found H; application needs review.
- Official Unity CLI exists; named CargoBuild/wrapper do not. P1-01 implements and
  tests that boundary before tickets rely on it. Found H.

## Decisions and alternatives

D1–D10 in systemsdesign.md define exact contracts and confidence/reversibility.
Recommended: author small state models and SDK adapters. Reject custom XR/ECS rewrite.
Recommended: split/join exact prefabs. Reject mesh slicing and geometric resizing
that changes visual quantities without exact labels.
Required: live state-aware AI guidance with captions. OpenAI is the preferred
candidate; access, privacy, output validation and hardware feasibility are early
gates. Local cues without live inference provide fallback, not AI acceptance. The core
live guide may select reviewed speech assets; speech synthesis alone is not AI adaptation.
Recommended: optional chord plus ray fallback; reject mandatory simultaneous dexterity.
Recommended: local draft tickets; reject automatic issue publishing or automatic merges.

## Testing cadence and milestone reviews

1. Each meaningful code change: compile and run affected tests plus baseline regression.
2. Each ticket: its named automated/handler checks, then physical checks for visible,
   audible or manipulated behavior. No new UI/input/audio slice closes on editor tests alone.
3. Milestone checkpoints: P1-03 live guided onboarding; P1-10 complete whole/halves
   chapter; P2-08 complete fraction learning flow; P3-06 release candidate. At each,
   owner starts from the catalog and tests Help/replay, mistakes, interruption and recovery.
4. Record exact APK hash and evidence, review result, then owner-authorized commit/push
   of the working branch. No merge to main is required to test on Quest. Respect
   existing clean-build safeguards or explicitly approve a recorded dirty-build exception.
5. Stop after each ticket for owner acceptance; do not batch unfinished activities.

## Setup and acceptance commands

No installations during planning. Existing branch unity-airlift; dependencies in
techstack.md. Detailed commands and open-editor/dirty-tree handling in TESTING.md.
P1-01 creates the explicitly **planned** wrapper and CargoBuild method.

```sh
bash scripts/verify-cargo.sh --phase 1 --build
bash scripts/verify-cargo.sh --phase 2 --build
bash scripts/verify-cargo.sh --phase 3 --build
bash scripts/verify-cargo.sh --phase 4 --build
git diff --check
```

Commands alone do not close device gates. Each ticket links exact named suites,
APK checksum, physical evidence and owner review. Checkpoint each accepted ticket
with owner approval; preserve rollback artifacts, never destructive reset.
Future server environment names and tests are documented in P1-03 and SERVER-VERIFICATION.md
before authorized setup; no keys or values belong in docs or APK.

## Phases

### Phase 1 — AI-guided whole-and-halves chapter · Size XL · 10 tickets

Depends on owner approval; see CC-P1-01 through CC-P1-10.
Goal: catalog → mission → comfortable controller tutorial → one whole →
two halves → recompose → independent half-length delivery.
Touches existing OnboardingDirector, math/lesson drafts; creates CargoCrew scene,
CargoBuild, verification wrapper, CargoLessonModel/contracts/director,
fraction prefabs, labels/equations, guide cues/audio and recovery adapters.
P1-03 also creates the authenticated guide backend, live output validation and
adult-only capability proof; P1-02 adds typography tokens and font selection.
Named tests: CargoBaselineTests, GuideCueTests, FractionInvariantTests,
SplitTransactionTests, RecomposeWholeTests, IndependentHalfTests,
RecoverySnapshotTests, ChapterOneAcceptance. Full inventory in tickets.
Acceptance: phase-1 command plus owner can play the narrated/muted/offline chapter,
actual split/recompose actions, ten interaction cycles and safe recovery.
Add CoreAIGuideTests and GuideServiceContractTests. P1-10 requires real authorized
live-provider guidance, not fixture/recording playback. An unavailable gate remains
blocked; offline progress is allowed only as an explicitly incomplete preview.
No claim that quarters or the complete lesson exist yet.

### Phase 2 — quarters, equivalents and comparison · Size L · 8 tickets

Depends on Phase 1 acceptance. CC-P2-01 through CC-P2-08.
Goal: four quarters → 3/4 → merge two quarters → equivalent lengths →
comparison/sign/reason → fresh delivery, optional chord and zoom.
Touches CargoLessonModel, stage/cue assets and prefabs; creates MergeStation,
EquivalentTask, ComparisonTask, SplitChordInput and WholeModelZoom.
Tests: QuarterConservationTests, ThreeQuarterTaskTests, MergeTransactionTests,
EquivalentConstructionTests, ComparisonReasonTests, TransferAttemptTests,
ChordInputTests, ZoomInvariantTests, ChapterFlowTests.
Acceptance: phase-2 command plus full actual-device journey, both handed inputs,
one-controller join fallback and truthful independent attempts.
User-requested button shortcut lands here, not silently omitted.

### Phase 3 — complete presentation and release evidence · Size L · 6 tickets

Depends on Phase 2 acceptance. CC-P3-01 through CC-P3-06.
Goal: evaluate retained bounded scaffold AI, finish cargo payoff, harden access/
recovery, measure performance, resolve release rights and package a candidate.
Extends the existing guide service and creates DispatchPresentation and QA/evaluation reports;
touches CargoCrew, guide assets, feedback/recovery and ProofBuildSafety.
Tests: ScaffoldRouterTests, DispatchSummaryTests, AccessibleRecoveryTests,
PerformanceCaptureTests, ReleaseSafetyTests, ReleaseManifestTests.
Acceptance: phase-3 command, authorized AI eval if retained, actual Quest three-run
performance evidence, all core device checks, rights and owner release review.
P3-01 may close as validated-retained or owner-approved-deferred. Either documented
outcome unlocks presentation/recovery. Deferral applies only to the routing enhancement, never core AI guidance;
contest packaging stays gated unless requirements are met or organizers accept
the omission. Private review packaging remains possible. Rights gates can block
distribution; do not present narration as live AI.

### Phase 4 — gated spoken questions and child-release readiness · Size XL · 4 tickets

Integration depends on core acceptance and specific permission gates. P4-01 is
the documentary exception: plan approval only; it may close no-go without an APK.
P4-02 depends on both P3-06 acceptance and P4-01 go plus privacy/budget approval;
P4-03/04 follow sequentially.
P4-01 eligibility may be reviewed earlier because it also gates any ElevenLabs
recorded assets and child-release data paths; it does not authorize microphone implementation.
Goal: explicit child-use/provider go/no-go, then optional adult spoken-question
input, constrained conversation and failure evaluation. This phase does not create
the first AI voice output; that is required in P1. No-go blocks child release or
spoken input as applicable, without relabeling a recorded fallback as core AI.
Creates voice-gate record and VoiceSession/LiveGuideAdapter; extends the existing
server with optional speech-input sessions.
Tests: VoiceEligibilityGateTests, VoiceSessionContractTests, LiveGuideBoundaryTests,
LiveGuideFailureTests. Acceptance: phase-4 command cannot pass without approved
gate evidence; actual authorized adult test, mic/retention/credential controls,
fallback and owner acceptance required. No child production rollout in this plan.

## Deadline and cut line

This is the full product backlog, **not a promise of 28 tickets before September 18**.
Finish and review one slice at a time. After Phase 1 decide whether time permits
Phase 2; do not add microphone conversation under deadline pressure. Core AI voice
is no longer a silent scope cut: missing live-guide proof means its criterion remains
incomplete, or requires the owner's explicit re-scope decision. If only Phase 1 is feasible,
present it honestly as a whole/halves chapter, not the complete fraction curriculum.
A reduced release still requires applicable P3-03 recovery, P3-04 measurement,
P3-05 rights/security and P3-06 packaging checks; owner must approve an explicit
re-scoped release ticket sequence before bypassing normal predecessor dependencies.
Retained contest AI requirements need their own satisfied gate or organizer-approved
scope change; local narration alone is not an asserted substitute.
No automatic submissions, public deployment or changes to deadline-frozen entry.

## Risks and failure modes

| Risk | Mitigation / acceptance |
|---|---|
| AI access/privacy/latency fails | P1-03 early adult proof; recorded fallback keeps play usable but cannot close AI gate |
| SDK release race duplicates parts | D3 stable lineage, no selectors at commit, generations; split/merge cancellation tests |
| Instructions reveal answers | D6 independent neutral lane and feedback parity tests |
| Feature volume consumes deadline | Phase 1 review cut line, owner-approved reduced release, Phase 4 excluded |
| Headset looks good but lessons don't teach | actual actions/explanations, fresh values, intervention logs; no efficacy claims |
| Live guide leaks data or changes answers | P1 output/content gate, no mic in core, approved facts/actions, server credentials and child-use gate |
| Two hands/buttons inaccessible | equivalent ray/sequential-dock path and physical checks |

Most likely: confusing release/selection transitions. Most catastrophic: child-data
or credential exposure. Most underestimated: full Phase 2 integration and Phase 4
voice/device lifecycle. No amount of editor tests substitutes for headset review.

## Requirements trace and review

Every requested interaction maps to F-01–F-21 in requirements.md and a named test
in its ticket. Pointers/narration are P1; quarters/merge/chord/zoom and harder values
are P2; full story/release evidence P3; optional spoken questions and child-release
review P4. AI spoken guidance is a cross-cutting core requirement from P1.
Reviewer findings and resolutions are recorded in REVIEW.md before delivery.

## Handoff prompt

After owner approval only: read this plan and revision research, then CONTEXT.md,
TESTING.md and CC-P1-01. Work in /Users/jad/Desktop/math-workbench-airlift on
unity-airlift. Inspect existing uncommitted work; implement only CC-P1-01.
Use existing official Unity tooling; do not close dirty editor state or install
unapproved packages. Run its named tests and physical checks. Record actual
results and Why in the ticket and DEV-LOG, then stop for owner review.
Do not start the next ticket, publish issues, purchase audio, collect child data,
push/merge or submit unless separately authorized.
