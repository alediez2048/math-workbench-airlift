# Cargo Crew systems design

Status: **proposed decisions, not approved implementation**. D1–D10 are design
defaults for review. Existing privacy, deterministic grading and safety rules remain.
All new classes/prefabs listed here are proposed, not present today.

## D1 — reuse the working foundation

Keep Unity 6.6, URP, installed OpenXR/Meta packages, controllers and stationary MR.
Create CargoCrew.unity from a reviewed copy of Onboarding, preserving DeviceProof
and Onboarding reference scenes. A CargoLessonDirector composes reusable onboarding
views with a single lesson state owner; do not run two independent directors
against the same buttons. Reject an ECS rewrite/new XR framework: no evidence of need.
Confidence H; reversible moderate.

## D2 — exact model, variable notation

Reuse FractionValue and FractionNotation. Canonical value reduces; display notation
preserves 2/4 or 6/8 to show construction. A task uses eight equal logical cells
and eight cells per material inventory (WholeId). ReferenceUnitId identifies the
shared mathematical unit size; two comparison inventories may share that unit
without sharing pieces. Render whole length initially 0.40 m (assumed — not in brief),
with fixed thickness/width. The old practice block's 0.28 m is not a math unit.
Use a non-interactive ghost whole, labeled “Reference whole — not another piece.”
Length, endpoints, expression and piece labels update from one snapshot.

Extend PieceState/PlacementState only after inspecting existing invariants.
The current unit-only PieceState and HalfLessonModel cannot represent all planned
merge/quarter stages. Replace HalfLessonModel's authority with CargoLessonModel,
reuse its valid logic, and remove obsolete consumers rather than keeping competing
models. Future fractions outside powers of two require a new denominator grid;
do not present this subset as the whole fraction curriculum.
Confidence H; schema choice reversible moderate.

## D3 — commands and atomic transformations

Introduce IGuidePlayer and GuideCueDefinition/GuideCueCatalog ScriptableObject
types in CC-P1-03; CC-P1-04 reuses them. Introduce in CC-P1-04:
- LessonCommand: Grab/Release/Place/Remove/Split/Merge/SelectNotation/
  SelectComparison/SelectReason/Submit/Help/Replay/Reset/Back, commandId,
  expectedRevision, attemptGeneration, target IDs, laneId and typed choice payload.
- LessonSnapshot: stageId, attemptGeneration, revision, referenceUnitId,
  inventories, pieces, placements, selected notation/sign/reason, target values,
  frozen SubmittedAnswer, firstAttemptResult and help-used flag.
- Inventory: WholeId, referenceUnitId, eight source cells and allowed lane IDs.
  Comparison uses W-left and W-right, each conserving one whole independently.
  Labels compare values only when ReferenceUnitId matches; cross-inventory
  placement/merge is rejected. Read-only reference ghosts consume no inventory.
- Piece record: pieceId, wholeId, immutable sourceStartCell, cellWidth,
  parentId, childIds, active. Source interval defines lineage, not current position.
- Placement: pieceId, laneId, placedStartCell; each active piece is in exactly
  one of supply, held, split dock, join dock or lane. Ordered lane pieces pack
  from zero after insertion/removal; reject overflow, not wrong mathematical totals.
- LessonTransition: next snapshot, feedbackId, narrationCueId, pointerCueId.
- LessonStageDefinition: stageId, valid commands, initial snapshot, prompt/cues,
  required mathematical action/result and next-stage ID.

CargoStageCatalog stores LessonStageDefinition assets and validates their references.
SelectNotation stores the learner's proposal separately from canonical labels;
SelectComparison accepts only <, =, >; SelectReason accepts authored enum choices
same_endpoint, farther_endpoint, more_parts, benchmark, unsure. Submit freezes
these explicit choices. Edits invalidate prior submission success. Help is
recorded once per attempt; replay of help cannot manufacture an independent pass.
Commands are deduplicated by commandId and guarded by expectedRevision in addition
to attempt generation. Every accepted mutation increments revision. A delayed old
Split cannot apply after Merge, even within the same attempt; regrab invalidates
the current release candidate and its selector epoch.

Store immutable initial snapshots for Reset. Do not infer quantity from transform
scale or Rigidbody positions. SDK release feeds a placement intent; deterministic
geometry and logical occupancy validate it. No current collider event grades math.

Split is allowed for widths 8→4+4, 4→2+2, 2→1+1, only at the split dock after
removing the piece from any lane and releasing it. Children return to distinct
supply positions. Parent inactive, stable child IDs
active, sum conserved. Merge only matching sibling children from the same whole,
adjacent and released; restore authored parent. A generic equal-valued pair is
not necessarily mergeable: curriculum tasks supply the intended sibling pairs,
and the interface explains this tool compatibility rule: two non-sibling quarters
still equal a half, even though these authored connectors do not join. Recompose accepts other
non-overlapping pieces without semantic merging.

A swap commits once, only after all selectors release; input disabled during a
short authored transition. Generation invalidates stale callbacks. Cancellation
before commit restores prior snapshot; after commit renders final snapshot.
Reset/Back during animation cannot duplicate material. No throw velocities.
Confidence H; rejected runtime mesh slicing and SDK “two-hand grab equals merge.”

## D4 — controller actions

Grip = pick up/hold; index trigger = ray-select UI.
Always provide a large “Split into 2 equal parts” button for a docked selected piece.
CC-P2-07 adds a shortcut: on the same explicitly selected controller, A+B (right)
or X+Y (left), second press within 250 ms (assumed tuning). Both must be held,
edge-detected once; release both to rearm. Show the correct handed controller icon.
Never use Menu/system buttons; never OR a button mask to test “both held.”
Ignore chord while holding, tracking lost, or stage does not permit split.

Two-hand merge: bring compatible pieces near the join station, preview a seam,
release both to commit. One-controller alternative: dock each piece sequentially
and ray-select Join. Same command, math and result. No forced motion gesture.
Confidence H for installed SDK capabilities, M for comfort pending device test.

## D5 — resizing is representation, not arbitrary quantity mutation

Recommended interpretation of “bigger/smaller”: split makes shorter pieces,
merge makes longer ones; optional Inspect Zoom scales **all** representations
(reference, pieces, ruler, spacing, labels) together. Clamp visual scale initially
0.8–1.2, only with released pieces, reset available. Canonical cells, quantities,
answers and relationships never change. Label “Zoom changes view, not amount.”
Reject free individual scaling for this lesson: it can falsify quantity or confuse
length with volume. This interpretation requires owner review.
Confidence H math, M ergonomic limits; easy to cut zoom without breaking lesson.

## D6 — guided versus independent modes

Guided stages may highlight action targets and two destination slots. Independent
stages use a neutral eight-cell lane accepting any valid arrangement, even a
wrong value. Geometry feedback identical regardless of correctness. On Submit,
compare exact values and requested notation/reason; retain repairable work.
No automatic green correct-slot hints, success haptics or narration before Submit.
Stage advance requires actual state predicates, never merely audio completion.
Persist only user settings locally; lesson progress restarts at a clear checkpoint
after process restart, with an explicit Restart/Return choice. No account store.
Confidence H; no mastery inference. See STAGE-FIXTURES.md for per-stage inventories
and legal successful traces; validate every fixture for solvability.

## D7 — core state-aware AI voice guide

Owner-confirmed requirement: live AI spoken guidance belongs in P1 and accompanies
all activities. OpenAI is the preferred evaluation candidate, not cleared deployment.
AI-VOICE-GUIDE.md owns the detailed contract: committed state → server-validated
context → live AI fact/action/variant selection → semantic validation → deterministic
approved text → hash-verified reviewed audio and matching captions. No arbitrary grading,
piece changes or advancement by the guide. Instruction pointers are code-owned.

P1-03 creates GuideCoordinator, OpenAIGuideAdapter, guide service and CoreAIGuideTests,
alongside the local IGuidePlayer/cue types. No microphone in core; ray Help requests
guidance. Real account/budget permission and adult live proof are required.
Recorded-only playback without live model selection is fallback, not core AI;
reviewed speech assets may be selected by the live guide. Missing access blocks AI acceptance.
Use the closed selection/approved-text pipeline, not arbitrary generated prose.
Test meaning against current facts, not vocabulary alone. P1-03 owns the finite
reviewed speech catalog: adult listening establishes audio/text fidelity; runtime
manifest/hash/context checks reject unreviewed assets. Live AI still selects guidance. Direct Realtime audio is
gated until pre-playback safety/correspondence is proven; a transcript alone is not proof.
Cancel stale speech on state changes; local muted/offline play remains available.
Confidence H requirement/capability; M/unknown Quest/account/privacy feasibility.

## D8 — adaptive support on the same AI-guide boundary

Retain closed scaffold selection and synthetic trace evaluation, using the P1
OpenAI guide boundary rather than introducing a second inference provider.
Only deterministic C# grades. P3-01 improves/evaluates adaptive routing; it may be
owner-deferred, but that never defers the already required core AI guide.
Use closed facts/action IDs, generation/revision checks, a five-second timeout and
local fallback. Actual server handlers need tests; client stubs are insufficient.
SERVER-VERIFICATION.md and AI-VOICE-GUIDE.md own the revised service contract.

## D9 — pointers, feedback and comfort

[STYLE-GUIDE.md](STYLE-GUIDE.md) is the canonical proposed presentation reference;
P1-02 owns font comparison, explicit TMP bindings and shared tokens before reuse.

Reuse Meta ray/haptic facilities; show one focus pointer at a time. Hide instructional
rays while not needed. Point to controller icon, selected piece, or task zone;
never require gaze inference. On successful grab/split/merge use a brief optional
pulse plus visual change; haptics never sole feedback. No correctness-coded pulse
on independent placement. Seated/standing bench adjustment and text readability
must be checked on Quest 3S; dimension choices are hypotheses, not certifications.
Recenter only with released pieces, never while manipulating. Lost tracking pauses
commands and guidance until a stable tracked pose; unavailable passthrough shows
a safe explanatory screen instead of encouraging blind reach.
Confidence M until hardware tests.

## D10 — provider/privacy gates and optional spoken questions

No child testing, purchases or submission authorized. P1-03 requires adult-only
live capability/access/budget/output-gate proof. P4-01 reviews child-use eligibility
early when needed, including structured event data and infrastructure logs.
OpenAI under-age personal-data handling requires verified ZDR and applicable
safety/privacy controls; no approval is assumed. ElevenLabs remains independently
gated and is not the default.

P4-02 adds optional explicit microphone input to the existing core guide, not a new
first voice service. P4-03 constrains questions to lesson help, and P4-04 evaluates
failure/pilot-readiness. Keep mic off by default and close it on exit/pause.
No-go blocks the relevant child/conversation scope; it does not turn fallback
recordings into proof of an AI-guided lesson. See AI-VOICE-GUIDE.md.
