# Math Workbench: Airlift — vetted execution plan

September 15 revision for owner review: [Cargo Crew implementation plan](../../00-build/PLAN.md)
and [revision research](fraction-expansion-research.md) extend this same RAP run.
The historical approved plan below is preserved; the new package is draft and
implementation is paused until owner review. Do not execute both plans concurrently.

Mode: revision · Size: XL · RAP tier: Deep · September 13, 2026.
Evidence: [research.md](research.md). Request and original brief: [brief.md](brief.md).
This plan supersedes `../../../UNITY_PLAN.md`; that file remains design history.
Status: independent challenge and requirements gap pass completed; fixes applied.

## Summary

Proceed with Unity for a native Quest 3S mixed-reality fraction lesson, subject to
a physical device proof. Reuse Meta's rig, grab, ray, passthrough, and candidate
snap components. Build the learning rules and visible adaptive scaffold in C#
with a small server proxy. The baseline is one coherent lesson; Cargo Grid is
conditional. This is a plan review, not evidence that the game has been built.

## Decisions and assumptions

### September 14 implementation amendments

- Closing checkpoint: safe non-development APK built/installed; owner confirmed
  passthrough, tracked controllers and cube grabbing. This closes setup/basic
  interaction, not the full Phase 1 QA gate. Remaining checks and artifact hash
  are in [device-proof.md](../../qa/device-proof.md).
- Owner prioritized onboarding before arithmetic implementation: Arithmetic
  Lessons opens with three topic cards. Cargo Crew / Fractions opens briefing,
  orientation, a demonstration, guided grab/place practice and replayable help.
  Neighborhood Café / Division and Community Garden / Multiplication are disabled
  Coming soon cards only. This explicitly supersedes the earlier no-menu-preview
  restriction without authorizing two more implemented lessons.
- Onboarding source and a separate Onboarding scene are now authored; 12 pure
  flow tests and editor layout/reference checks pass. Physical onboarding QA
  remains pending; see [onboarding evidence](../../qa/onboarding.md). The arithmetic
  task is intentionally not connected yet. Preserve DeviceProof unchanged.
- Owned headset: **Quest 3S**, with authorized ADB observed; corrects the original
  Quest 3 description. Test on this actual device.
- Use installed **6000.6.0f1 Apple Silicon + URP 17.6.0** for the initial proof.
  Android SDK/NDK/OpenJDK are installed. This supersedes D2's 6.3-only setup
  requirement provisionally, not the requirement to freeze a physically proven tuple.
- Universal 3D project exists in `unity/`. Official Unity CLI/Pipeline is the
  automation route. Basic cube interaction is owner-confirmed; full XR acceptance
  remains open. Onboarding flow tests and Android build pass, not physical QA.
  See [toolchain](../../qa/toolchain.md).
- Owner confirmed cargo theme and requested café/division and garden/area
  alternatives on paper only; see [future concepts](../../future-lessons.md).
  Release scope is unchanged; no full virtual port or hangar is planned.
- Owner authorized Meta XR Core/Interaction SDK license acceptance and import.
  This is not consent to purchases or hackathon submission.
- Update PROGRESS.md, AGENTS.md, CLAUDE.md and affected product documents after
  meaningful work. Preserve earlier research/plans as dated history.

The review-time assumptions below remain historical where amended above.

No new user preference is needed to complete this review. Six defaults are
explicitly **assumed — not in brief**: A1 aircraft-straps theme; A2 age-10 Grade 4
learner; A3 Unity 6.3 LTS release family; A4 Vercel plus Haiku
`claude-haiku-4-5-20251001`; A5 research prototype evaluated using synthetic
traces and the adult entrant; A6 ergonomic/performance targets below. These are
implementation defaults, not user endorsements or proven learning outcomes.

- **LOCKED D1: Unity, URP, native Quest APK** — user preference and reusable Meta
  components. Rejected: automatic IWSDK pivot (changes accepted platform),
  browser port (extra delivery surface). Confidence M until device proof;
  reversibility hard after Phase 2.
- **LOCKED D2: candidate 6.3 LTS, OpenXR, Meta XR** — supported LTS family; select its
  current stable patch in Hub, prove the installed tuple before locking versions, and
  freeze the tested tuple. Rejected: open-ended `6.1+` or upgrading mid-build.
  Confidence H for documented support, M for local compatibility; reversible early.
- **LOCKED D3: one fraction flagship, Cargo Grid conditional** — retain current
  reduced scope. Rejected: three independent lessons or a second challenge.
  Confidence H as scope control; reversible before stretch work.
- **LOCKED D4: controllers, world-locked workbench, prefab partitioning** — reuse
  interaction components and avoid freeform cutting/room capture. Rejected:
  continuous head-following table and required hands. Confidence H; reversible early.
- **LOCKED D5: deterministic correctness, evidence-bounded AI** — a model chooses
  among authored scaffold/prompt pairs using observed events. Rejected: inferring
  silent thoughts or letting model text grade mathematics. Confidence H for
  architecture; M for adaptive usefulness until evaluation; reversible.
- **LOCKED D6: video-first review package, optional APK/source links** — publish
  only licensed material and document native installation. Exact contest facts
  and license qualifications are in research.md. Confidence H facts, M native
  delivery interpretation; submission rights decision is hard to reverse.

## Drift

| Previous plan says | Current evidence | Disposition |
|---|---|---|
| Production-ready architecture | Only Markdown exists; no Git/Unity project | REOPEN: acceptance commands are future deliverables |
| “6.1+” and compatible SDKs | No version tuple has been installed/tested | REOPEN: 6.3 LTS family + recorded tuple in Phase 1 |
| Player-relative table | Attachment/recenter behavior unspecified | REOPEN: place once, then world-lock |
| Count endpoint/piece checks | No UI or sensor produces those fields | REOPEN: explicit events; no latent-behavior counters |
| Wrong drops return; errors inspectable | Conflicting policies | REOPEN: geometry validity separate from correctness |
| Correct-only snap targets | Can reveal answers before reasoning | REOPEN: neutral slots and explicit Submit |
| Custom SnapZone | Meta Snap exists but is experimental | REOPEN: reuse behind tested adapter; simple fallback |
| Permission denial for passthrough | Raw camera permission is a different API | REOPEN: readiness and unavailable-state tests |
| Transfer, mastery-style interpretation | Guided six-step lesson only | REOPEN: novel-value near transfer; no mastery claims |
| Proxy returns whenever complete | Reset/timeout races unspecified | REOPEN: request/attempt IDs and cancellation |
| Friday fixes P0 only | P1 includes wrong mathematics | REOPEN: no P0/P1 in enabled submitted paths |
| Exact code/manual percentages | Unmeasured, mixed categories | REOPEN: task ownership and gates, no precision claim |

KEPT: Unity preference, Meta reuse, fixed whole, linear number line, connected
representations, recoverable mistakes, no timers/lives, intrinsic payoff,
controller baseline, no third lesson, optional Cargo Grid, server-only API key.

## Setup & commands

Project root: `/Users/jad/Desktop/math-workbench-airlift`.
Workspace location updated September 14 at the owner's request; the original
Documents checkout is retained but is no longer the active working copy.
All paths below are relative to this root. Unity project: `unity/`.
Backend: `backend/`. Evidence: `artifacts/` (ignored by Git); concise reports:
`docs/qa/`. Execution branch: `unity-airlift` after checking for an existing repo.

Phase 1 uses **6000.6.0f1 Apple Silicon** for the compatibility proof per the
September 14 amendment, Android Build
Support, bundled SDK/NDK/OpenJDK, and a suitable activated Unity license. Use
Universal 3D and the Quest/Android build profile, ARM64 + IL2CPP. Import Meta
All-in-One once through the official package workflow; keep only needed samples.
Use OpenXR plus Meta support and disable teleport in the supplied rig. Record
editor, SDK packages, OpenXR, device OS, graphics API, and simulator versions in
`docs/qa/toolchain.md`, retaining `ProjectVersion.txt`, manifest, and lock file.

Manual owner tasks: account sign-in/license eligibility, Meta developer
verification, mobile pairing/developer mode, cable connection, accepting USB
debugging, and physical headset/capture checks. Agent tasks: C#, test assemblies,
task assets, thin editor utilities, backend, build scripts, and documentation.
Scene assembly is shared; use a supported Unity integration if already available,
otherwise batch Editor methods and a short explicit Inspector checklist. Do not
assume this chat currently has a Unity editor-control connector.

Create these wrappers in Phase 1; **they do not exist at review time**:

```sh
bash tools/verify.sh preflight
bash tools/verify.sh editmode
bash tools/verify.sh playmode
bash tools/verify.sh build-quest
bash tools/verify.sh device
bash tools/verify.sh release
```

`preflight` resolves the installed editor from `unity/ProjectSettings/ProjectVersion.txt`
and ADB from Unity's configured Android toolchain, reports module/license failures,
and creates `artifacts/`. Test commands run Unity with `-batchmode -projectPath`,
`-runTests -testPlatform EditMode|PlayMode -testResults`, check XML for nonzero test
count and zero failures, and propagate errors. Do not add `-quit` to test runs.
Build uses `-batchmode -quit -buildTarget Android -executeMethod
Airlift.Editor.BuildQuest.Release`, fails on non-success BuildReport, and emits
`artifacts/airlift.apk`. Close the interactive editor before running wrappers
against the same project. `device` uses `adb devices`, explicit serial selection
if needed, `adb install -r artifacts/airlift.apk`, and launch of
`com.jad.airlift`; confirm the visible result on Quest. `release` also checks
manifest permissions, notices, version tuple, APK checksum, and QA report links.

Backend commands after Phase 4 creates package.json and commits its lock file:

```sh
npm --prefix backend ci
npm --prefix backend test
npm --prefix backend run dev
npm --prefix backend run smoke
```

Use Node 22 and Node's built-in tests/fetch; Vercel CLI is a pinned dev dependency.
Environment names: `ANTHROPIC_API_KEY`, `TUTOR_MODEL`, `TUTOR_ENABLED`, `TUTOR_URL`.
Only the HTTPS URL is configured in the app. No credentials in serialized assets,
logs, Git, or APK. Commit verified phases; recover by targeted corrective commits.

## Interaction and lesson contract

The table is placed from the initial horizontal head direction, then stays in
world space. Recenter is deferred while any piece is held. Once all grabs finish,
it cancels pending tutor requests, increments attempt generation, and translates
the whole board while preserving its arrangement. In passthrough unavailability, show a stationary
opaque safe scene with Retry/Exit; the final MR acceptance still requires actual
passthrough. No raw-camera access, room capture, scene anchors, or locomotion.

`FractionValue` is normalized semantic value; `FractionNotation` preserves authored
numerator/denominator and partition count, so six eighths stays labeled 6/8.
`PieceState` stores both, piece ID and whole ID. Each board lane has eight atomic
eighth-width cells: half occupies four contiguous cells, fourth two, eighth one.
`PlacementState` stores ordered pieces and occupied intervals. Place at insertion
boundaries; after placement/removal, pack pieces from zero without gaps, changing
neither count nor quantity. Reject overlap or overflow beyond the whole, never
beyond the requested answer. Comparison lanes have identical whole lengths.
Valid shorter/longer-than-target compositions remain. Unsafe drops return to a
supply tray. `ProposedExpression` is the learner's editable answer, separate from
canonical quantity notation; wrong equations never rewrite the board's value.
Submit freezes a snapshot, evaluates it, logs it, and either advances or leaves
the attempted arrangement for repair. During grabbing, mutation is provisional;
only completed placement/removal/submit events affect lesson state.

Six task assets in `unity/Assets/Airlift/Tasks/`:

| Task asset | Learner action and evidence | Learning claim |
|---|---|---|
| `T01Whole.asset` | Align unit strap with 0–1 ruler; replayable grab tutorial | Establish equal whole |
| `T02Half.asset` | Select equal partition into halves; activate exact segment prefabs; locate 1/2 | 3.NF.A.2 |
| `T03ThreeFourths.asset` | Partition into fourths; assemble 3/4; record 1/4+1/4+1/4=3/4 | 3.NF.A.2; part of 4.NF.B.3 |
| `T04Equivalent.asset` | Subdivide each fourth in two, preserve total; construct 3/4=6/8 using separate numerator/denominator/operator controls; highlight ×2 on numerator and denominator | Supports 4.NF.A.1 |
| `T05Compare.asset` | Predict 3/4 versus 5/8; optionally use overlay/benchmark; construct inequality and choose a model-backed reason before Submit | Supports 4.NF.A.2 |
| `T06NearTransfer.asset` | Fresh 1/2=4/8 construction plus 1/4 versus 3/8 comparison; no answer preview, supports available on request | Near-transfer session evidence |

Partitioning is constrained, equal subdivisions selected from 2/4/8; it is not a
knife simulation. Use Meta constrained grab for the guide if supplied; otherwise
a ray-operated partition control. Invalid mathematical constructions are allowed
for investigation; no snapping/chime indicates correctness before Submit.
Equations use ray-operated numerator, denominator, and operator slots with
independent choices and plausible alternatives. The learner constructs a
statement; direct equation-tile grabbing is cut to reuse the existing UI.
T06 records first attempt, used supports, and
final outcome separately; revisiting it is practice, not a new assessment.

Three learner-visible chapters organize the one flagship: build quantities,
compare equivalents, and apply to a fresh cargo strap. This provides progression
without claiming three independent lessons. Finish with strap attachment, cargo
lights, sound/haptics, and a brief aircraft departure animation. No flight controls.
Target a complete 6–8 minute guided playthrough without a countdown; record actual
duration and allow pause. Replay instructions at every task, reset the task, and
replay the lesson after completion. Meaning must remain clear without color.
No lives, score penalty, shaming, or generic praise.

## Observable AI contract

Types precede UI/backend in Phase 2. `LessonEvent` contains `eventId`, sequence,
taskId, attemptId, eventType, and typed payload. Allowed events:
`PiecePlaced`, `PieceRemoved`, `PartitionSelected`, `OverlayRequested`,
`BenchmarkRequested`, `ComparisonCommitted`, `ReasonSelected`, `Submitted`,
`Reset`. Do not log guessed eye attention, counting, learning style, or emotion.

Request: schemaVersion, random in-memory requestId, taskId, attemptId,
deterministic verdict, immutable `SubmittedAnswer` (physical quantity, authored
expression, comparison, explicit reason), last 24 events, and allowed scaffold IDs.
Retain reason/submit/relevant tool events outside the tail in a bounded six-event
evidence array; long histories cannot evict decisive evidence. Every summary value
references an included event. No free text or
raw input streams. The three authored scaffold/prompt pairs are
`equal_parts`, `common_endpoint`, `benchmark_half`. `ReasonSelected` records
explicit choices such as `more_parts`, `same_endpoint`, `benchmark`, `unsure`.
Tool use is evidence of tool use only. `3/4` versus `5/8` alone cannot identify
denominator confusion because both numerator and denominator differ.

Model response: `hypothesis`, `evidenceIds`,
`scaffoldId`, `promptId`. No numeric confidence or free-generated student feedback.
Server checks that IDs were in the request **and support the hypothesis**; client
repeats enum/ID/evidence checks. Closed routes in `ScaffoldDefinition`:

| Hypothesis | Minimum evidence | Allowed scaffold/prompt |
|---|---|---|
| `symbol_model_mismatch` | T04 physical amount correct; expression inequivalent | `common_endpoint` / `t04_label_same_length` |
| `whole_number_comparison` | T05 incorrect comparison, explicit `more_parts` reason | `equal_parts` / `t05_compare_piece_size` |
| `benchmark_strategy` | T05 incorrect comparison, `benchmark` reason, BenchmarkRequested | `benchmark_half` / `t05_excess_over_half` |
| `insufficient_evidence` | Absent, conflicting, or unsupported evidence | `common_endpoint` / task-specific neutral prompt |

T05 benchmark support compares excess above half: 2/8 versus 1/8. Merely showing
both exceed half cannot resolve the comparison. T05 neutral support aligns 6/8
and 5/8 from zero; T04 aligns 3/4 with 6/8. A supported pair selects an authored
visual step and short text.
Unknown or weak evidence receives neutral common-endpoint support. The model
does not choose correctness, unlock completion, or change mathematical values.

Only an **incorrect** explicit Submit at T04/T05 invokes AI, at most once per
attempt and four requests per session. Correct Submit advances without a request;
wrong T01–T03/T06 or exhausted budget uses local support. Eligible wrong Submit
enters SupportPending, applies one scaffold, and enters Repair; advance is disabled
until a subsequent correct Submit. One request is in flight; reset/recenter/task advance
increments attempt generation and cancels it. At five seconds, local routing wins
once. Late/duplicate/wrong-attempt results cannot mutate the world. Show an
unobtrusive “Guided support” message during fallback; diagnostic review view
distinguishes live model, local fallback, and explicitly labeled fixture playback.

Proxy defaults: POST only; ≤8 KiB body; ≤24 tail events plus ≤6 retained evidence
events; known task/value ranges;
fixed prompt and model; ≤256 output tokens; no tools; provider timeout before the
client deadline; no automatic retries. Apply a Vercel WAF rule for `/api/tutor`,
10 requests per IP per 60 seconds, returning 429. Counters are regional; this is
not a global budget. Before enabling public inference, inspect provider spend
controls and set an owner-chosen ceiling without authorizing new charges. Keep
`TUTOR_ENABLED=false` if usable controls/account access are absent. Do not log
request bodies or identifiers; infrastructure IP logs may still exist and must
not be described as zero data collection. Offline lesson remains complete.

Live model acceptance compares fixed synthetic traces sharing the same final
answer but differing in explicit reason/action evidence. The author prelabels
allowed support sets; the model must stay inside them on three repeats. Ambiguous
traces must abstain. This tests software behavior, not diagnosis accuracy or
educational efficacy. Compare with the local router on the same fixtures. If the
model duplicates fixed rules, call it **model-mediated authored support** and
report no demonstrated model advantage. Do not force artificial differences.
On Quest, verify the differing visible scaffolds for two equal wrong answers with
different explicit evidence, recording actual response/application and timeout
results with synthetic data. Sources and limitations are in research.md.

## Phases

### Phase 1 — device and toolchain proof (Size: M)

Depends on: none. Create `unity/` via Hub; `tools/verify.sh`,
`unity/Assets/Airlift/Editor/BuildQuest.cs`, Editor/test assembly definitions,
`docs/qa/toolchain.md`, `docs/qa/device-proof.md`, `.gitignore`, `README.md`.
Use an existing test framework or install its compatible stable package. Make
one passthrough scene and grabbable cube with Meta Building Blocks; no duplicate
rigs. Check license/account/dev-mode prerequisites before content work.
Create `docs/submission/rights-check.md` now; record the entrant's decision on the
known rights tradeoff before major content work. Unresolved compatibility remains
a submission gate. Check existing provider access and HTTPS early when credentials
exist. Simulator and complete automation wrappers must not delay minimal physical
Build And Run proof; finish wrappers by Phase 2. Capture readable MR content.

Tests: `ProjectSmokeTests.SceneHasSingleRig`; manual `FreshApkLaunch`,
`PassthroughReady`, `ControllerGrabTenTimes`, `PauseResume`, `CaptureTenSeconds`.
Acceptance: all preflight/build/device commands succeed; owner records actual
headset/capture results and exact versions. A four-hour checkpoint from beginning
setup is a stop-loss review, not a promise or an automatic framework switch.
If it fails, record the concrete block; continue only independent planning/math
work and seek direction before abandoning Unity. Do not spend the day on art.
Calendar checkpoint, not a Phase 1 exit dependency: Monday evening requires one
half task with Submit, wrong attempt, repair, and reset on Quest using a thin
slice of Phases 2/3. Phase 1 can pass on the cube proof before this checkpoint.

### Phase 2 — mathematics, state, and event contract (Size: M)

Depends on: 1 for configured project. Create `Scripts/Math/FractionValue.cs`,
`Scripts/Lessons/TaskDefinition.cs`, `LessonDirector.cs`, `PlacementState.cs`,
`Scripts/Tutor/TutorContracts.cs`, `SessionEventLog.cs`, `LocalScaffoldRouter.cs`
under `unity/Assets/Airlift/`. Also create `Scripts/Math/FractionNotation.cs`,
`Scripts/Lessons/PieceState.cs`, `ProposedExpression.cs`, `SubmittedAnswer.cs`,
`RepresentationSettings.cs`, `Scripts/Tutor/ScaffoldDefinition.cs`.
Grouped filenames inherit the preceding directory prefix. Use Unity's compatible
`com.unity.nuget.newtonsoft-json` for DTO serialization, with explicit validation
of required fields, ranges, enums, and extra properties. Create
`contracts/tutor.schema.json` and shared
`contracts/fixtures/`. State graph: Tutorial → Arrange → Submitted → Correct |
SupportPending → Repair → Arrange → Complete. Reset returns to the task's initial
arrangement and increments generation; it never mixes attempts. Node uses pinned
Ajv 8 against the shared schema; C# uses matching explicit contract validators.
Exercise both with identical valid/invalid fixtures.

Tests: `FractionValueTests.EquivalentForms`, `RejectZeroDenominator`,
`ComparisonOrder`; `PlacementStateTests.WrongValidArrangementPersists`,
`OccupiedSlotRejected`, `WholeIdentityPreserved`;
`LessonDirectorTests.ResetInvalidatesAttempt`, `SubmitOnce`,
`CompletionRequiresCorrectSubmission`; `TutorContractTests.UnknownEnumsRejected`,
`SubmittedSnapshotRoundTripsAcrossClientAndServer`.
Add `MixedDenominatorsPreserveLength`, `MultiCellOverlapRejected`,
`SixEighthsNotationSurvivesNormalization`, `WrongEquationDoesNotChangeBoard`,
`EligibleSubmitAppliesSupportBeforeAdvance` to the corresponding test classes.
Acceptance: `bash tools/verify.sh editmode`; fixtures accepted by the same schema
later consumed by backend; no scene or network dependency in pure math tests.

### Phase 3 — complete offline lesson (Size: L)

Depends on: 2. All following Unity paths are under `unity/Assets/Airlift/`.
Create `Scenes/Airlift.unity`, six task assets, fraction/board/UI
prefabs, `Scripts/Interaction/SnapAdapter.cs`, `WorkbenchPlacement.cs`,
`Scripts/Presentation/RepresentationPresenter.cs`, `WorldFeedback.cs`.
First try Meta Snap sample behind adapter. If its contention/reset tests fail,
use existing Meta grab release events plus a nearest-unoccupied-slot transform
snap; preserve the same PlacementState, do not replace input tracking.

World-lock table after placement; initial whole length 0.4 m; seated height
adjustment; one-handed completion; large ray controls outside the grabbing area.
Use handles around small eighth pieces to avoid requiring fingertip precision.
Partition with prebuilt segments; physical/diagram/symbol views derive from the
same canonical state. Finish T01–T06 with local scaffolds and visible payoff.

Tests in `Tests/PlayMode/InteractionTests.cs`: `TwentyGrabRelease`,
`TwoPiecesOneSlot`, `ResetDuringSnap`, `RegrabDuringReturn`,
`TrackingLossRestoresOwnership`, `RecenterKeepsBoardWorldLocked`.
Add `RecenterDuringGrabDefersWithoutDroppingPiece`, `ComparisonLanesShareWhole`,
`SubdivisionPreservesQuantityAndDoublesPartCount`. Inspect SDK hover previews,
attraction, filtering, and haptics: wrong answers receive identical geometric
assistance. Switch a blocked Snap trial to the simple adapter before authoring
other tasks; it cannot delay Monday's task slice.
`LessonFlowTests`: `OfflineStartToFinish`, `WrongThenRepair`,
`EquationMatchesBoard`, `NovelTaskFirstAttemptRecorded`, `NoEarlyCorrectnessCue`.
Acceptance: EditMode/PlayMode commands and device build; manual complete
playthrough, seated reach/readability, no reload/dead end. PlayMode tests simulate
state recovery; physical grab reliability, tracking loss and comfort require
separate owner-run evidence. Physical tests belong
in `docs/qa/offline-lesson.md`; do not mark them passed from simulator results.
Tuesday evening gate: offline flagship complete. Miss it and immediately cut
Cargo Grid, hands/voice, and complex aircraft decoration. Simplify equation UI
before compromising correctness, repair, adaptive proof, or capture.

### Phase 4 — live adaptive support and failure handling (Size: M)

Depends on: 2 and 3. Create `backend/package.json`, `backend/api/tutor.mjs`,
`backend/lib/validate.mjs`, `backend/lib/route.mjs`, `backend/test/tutor.test.mjs`,
`backend/scripts/smoke.mjs`, `backend/vercel.json`, `backend/.env.example`;
`unity/Assets/Airlift/Scripts/Tutor/TutorClient.cs` and `TutorResponseGate.cs`;
`docs/qa/tutor-eval.md`. Use fixed HTTP Messages API and closed schema;
Node built-in tests/fetch plus Ajv avoid an orchestration framework.
Configure HTTPS URL, server secrets, WAF, provider controls, and deployment
access so judges do not meet a hosting login wall.

Tests: backend `RejectOversize`, `RejectUnknownTask`, `RefusalFallsBack`,
`UnknownEvidenceRejected`, `ModelDisabledMakesNoCall`;
Unity `TimeoutWinsOnce`, `LateResponseIgnored`, `ResetWhilePending`,
`SameAnswerDifferentEvidenceRoutesDifferently`, `InsufficientEvidenceAbstains`.
Add `IrrelevantExistingEvidenceRejected`, `ConflictingEvidenceAbstains`,
`CorrectSubmitAdvancesWithoutRequest`, `BudgetExhaustionUsesLocalSupport`.
Acceptance: backend tests and live smoke, Unity tests, physical live/fallback
playthrough; `docs/qa/tutor-eval.md` records model/prompt version and repeated
fixture outcomes. Existing credentials needed; fixture-only behavior cannot
pass the live AI gate. App never blocks completion on this service.
Wednesday also produces a rough full video of the actual lesson; final Thursday
capture replaces footage rather than discovering the capture workflow.

### Phase 5 — conditional Cargo Grid (Size: M)

Depends on: 1–4 completely accepted by Wednesday September 16, 3 PM CDT, with
zero open P0/P1. Otherwise record `SKIPPED` and go directly to Phase 6.
Maximum five hours; if acceptance fails at the limit, remove the enabled path and
record SKIPPED. Create `unity/Assets/Airlift/Tasks/CargoGrid.asset`,
`unity/Assets/Airlift/Scripts/Lessons/ArrayDecomposition.cs` and grid
prefab. Use one 7×6 array split into 7×5 + 7×1, preserve 42 tiles, and record an
assembled equation. Reuse rig/UI/log; no separate AI pipeline or timed fluency claim.
Tests: `ArrayDecompositionTests.TotalPreserved`, `RecombineRestoresArray`,
`EquationMatchesRegions`. Acceptance: all existing tests plus complete Quest
path; no effect on flagship, build size/performance, or final capture.

### Phase 6 — release evidence and submission package (Size: M)

Depends on: 1–4; 5 accepted or skipped. Create `docs/qa/release.md`,
`docs/submission/description.md`, `third-party.md`, `rights-check.md`,
`install.md`, `video-script.md`, `manifest.json` (all six submission files belong
under `docs/submission/`). Preserve vendor notices;
publish source only where redistributable. Asset caches, signing key, and API
secrets stay private. Unity tier eligibility and any recipient agreement must be
resolved before distributing the APK. Do not bundle vendor SDK ownership claims.

Tests: `FreshReleaseInstall`, `ApkChecksumMatchesDownload`,
`NoRawCameraPermission`, `OfflineCompletion`, `LiveAdaptation`,
`PauseResumeFullFlow`, `SeatedReachAndLegibility`, `VideoUnderThreeMinutes`,
`PublicLinksNoLogin`, `FullLessonDurationRecorded`, `MeaningWithoutColor`,
`InstructionReplayAndFinalReplay`, `NonPunitiveFeedbackReview`,
`CorrectMathTriggersAircraftPayoff`. These are manual release checks.
Profile full lesson on Quest at chosen 72 Hz; project target
is p95 CPU/GPU frame time each ≤13.9 ms and no sustained reprojection or thermal
degradation in three consecutive runs. Record actual metrics/build/device state;
this target is an engineering default, not a platform certification.

Acceptance: `bash tools/verify.sh release`; owned-code disclosure and artifact
manifest complete; zero P0/P1 on every enabled path; actual capture reviewed.
Keep APK hash, prompt/model configuration, and endpoint behavior fixed through
judging. Freeze features Thursday September 17 at 6 PM and capture that evening.
Friday is verification and submission, with internal 6 PM target. Actual contest
deadline remains September 18 at 11:59 PM Central. Preserve accessible artifacts
through September 23 and a buffer to September 24, 11:59 PM Central. No automatic
monitoring is configured by this planning task.

Submission assigns original entry rights broadly and grants a noncommercial
license back. `rights-check.md` must record the entrant's informed decision before
submission; ordinary coding authorization does not authorize this assignment.
Native eligibility is inferred, not a sponsor guarantee. The required experience,
description, video, and disclosures take priority over optional download links.
No claim of student efficacy, calibrated diagnosis, or a published judging score.

## Risks, cuts, and requirements trace

| Rank / failure | Consequence | Designed mitigation |
|---|---|---|
| 1: account/toolchain/device setup fails | No product on target hardware | Phase 1 proof before content; no automatic platform pivot |
| 2: reasoning inferred from nonexistent events | Misleading AI demo | Explicit events, abstention and evidence fixtures in 2/4 |
| 3: snap/input/reset races | Stuck lesson or answer leaking | Neutral slots, adapter, generation IDs in 2/3/4 |
| 4: overclaimed learning outcome | Weak pedagogy despite immersion | Novel-value task, inequality/reason evidence, modest claims |
| 5: native delivery/rights assumed | Entry inaccessible or rights surprise | Capture in 1, package/rights review and fixed links in 6 |

Most likely: interaction authoring and device iteration take longer than the old
estimate implied. Most catastrophic: no working submitted experience or
unacceptable submission rights. Most underestimated: Phase 3, where prefabs must
be integrated into mathematically meaningful behavior. No dependency is removed
by claiming agent-generated code can replace physical headset testing.

CORE: spatial math (3), representation connection (3), visible adaptive support
(4), contextual payoff and recorded evidence (3/6). STRETCH: Cargo Grid (5).
Hands/voice are deferred beyond this execution plan. CUT:
third lesson, reading/language entry, browser port, dashboard, accounts, multiplayer,
runtime mesh cutting, flight simulation, required room scanning. If Day 1/2 slips:
cut Cargo Grid first, hands/voice second, decorative aircraft complexity third;
retain fraction correctness, offline completion, live AI proof, and capture.

| User requirement | Phase | Named evidence |
|---|---|---|
| “5 more days” | 1–6 | Wednesday stretch gate; Thursday freeze; VideoUnderThreeMinutes |
| “VR game” / “Oculus Quest 3” | 1/3/6 | FreshApkLaunch; OfflineStartToFinish |
| “fully immersive gaming experience” | 3/6 | SeatedReachAndLegibility; actual world-payoff footage |
| “2–3 arithmetic lessons” | 3/5 | T01–T06 one flagship in three chapters; optional ArrayDecomposition tests; reduced scope explicitly retained |
| “day to day relevant topics” / “airplanes” | 3 | Cargo strap construction and aircraft payoff |
| “pedagogy…best practices and standards” | 2/3 | EquivalentForms; EquationMatchesBoard; NovelTaskFirstAttemptRecorded |
| Existing libraries / Unity preference | 1/3 | Toolchain record; Meta prefab provenance; Snap contention tests |
| Coding versus manual setup | all | Ownership paragraph and per-phase actual evidence |
| AI learning hackathon | 4/6 | SameAnswerDifferentEvidenceRoutesDifferently; LiveAdaptation |
| Reading/language if time allows | Non-goal | No second entry; no time diverted from acceptance |

## Challenge

Independent Challenger and Deep requirements gap pass completed September 13.
Conditional Unity GO; the original draft was not execution-ready. Fixes applied:

- Ambiguous slots → incorrect lengths/overlap → atomic eighth cells, multi-cell
  occupancy, equal lanes, packing, and no target-answer filtering.
- Normalization erases six-eighth notation → separate authored/semantic forms.
- Wrong expression corrupts displayed quantity → separate proposal and board.
- Valid but irrelevant event IDs → minimum evidence rules, abstention,
  counterfactual physical test, and comparison against local rules.
- Response/reset/advance races → explicit incorrect-submit state, generation
  checks, timeout and exhausted-budget paths.
- Both fractions exceed half → compare excess, not just the benchmark itself.
- Cube success hides lesson workload → Monday real-task, Tuesday offline,
  Wednesday live/capture gates and five-hour Cargo limit restored.
- Late setup/rights discovery → early access/license/rights checks; device proof
  before automation polish.
- Mock tests mistaken for headset results → separate physical records and
  readable capture, replay, non-color, and performance checks.
- Conflicting instructions → AGENTS/CLAUDE point here; old plan preserved with
  a pointer. `PlanAuthorityConsistent` checked in this review.

Challenger re-review: HOLDS. Its final schema consistency note was applied:
request and proxy both permit 24 trailing plus six retained evidence events
within the single 8 KiB body limit.

Remaining gates: installation, device performance, provider access, and entrant
rights/license decisions. Vetting does not claim those have passed. Researchers:
gpt-5.6-sol; challenger and gap pass: gpt-6-astra, because RAP's named Claude
models were unavailable in this environment.

## Handoff prompt

Implement `docs/plans/2026-09-13-unity-vet/plan.md` in
`/Users/jad/Desktop/math-workbench-airlift`. Read it and
`research.md` completely; preserve the original design history. Begin Phase 1
with real installed-tool/account/device evidence and create the named wrappers.
Check Git state before initializing and working on `unity-airlift`. Execute
phases in dependency order; record physical acceptance truthfully, commit each
accepted phase, and respect the calendar stretch/freeze gates. All named source
files and tests are planned deliverables, not existing code. Do not silently
replace Unity. If a prerequisite needs the owner's physical action, license,
account, or submission-rights decision, report it and continue independent work.
Do not submit or accept legal terms on the owner's behalf without authorization.
Finish with build hash, tests, actual headset evidence, deviations, and any block.
