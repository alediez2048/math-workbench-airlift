# Cargo Crew development log

## 2026-09-16 — Table handle: carry and resize the whole table (OWNER ACCEPTED on headset)

- Owner ask after accepting the toy look: move the table elsewhere and make it bigger or
  smaller from inside the app (only the Meta-button system recenter existed). Owner chose
  Option 1, a carry handle, over buttons or an adjust mode.
- **Handle:** yellow rounded bar on the deck's front edge (Table handle, z -0.412). Its
  Grabbable targets the station root, MaxGrabPoints 2. One hand: SDK OneGrabFreeTransformer
  with X/Z rotation locked (carry + yaw). Two hands: SDK TwoGrabPlaneTransformer on the table
  plane (planar move, yaw, uniform scale 0.5x-2x). Interactable created by QuickActionsAPI.
- **Gate/settle:** new runtime TableHandle on the director: disables the handle interactable
  while the practice strap or any fraction piece is held; disables the four piece
  interactables while the handle is held; on release levels the table (yaw only), clamps
  height to ComfortPlacement's 0.35-1.4 m band and clamps scale. Pure rules in
  TableAdjustRules (5 tests). CargoLessonDirector gains AnyPieceHeld.
- Orientation text now ends "Move the table by its yellow handle; two hands resize it."
- Layout test TableHandleWiredToStationRoot added (CargoTerminalLayoutTests now 8).
- Contract change: CLAUDE.md "place once, then world-lock; explicit recenter only with
  released pieces" becomes "player may carry/resize by the handle; refused while a piece
  is held". Meta-button recenter remains the fallback.
- **Build:** artifacts/qa/cargo-20260916-142801/airlift-cargo.apk, SHA-256 96818c56...; 54 checks across 12 suites; credential scan passed; 0 errors / 10 warnings. Installed on the Quest 14:31 (md5 345dd783 verified on device) and launched for the owner's check.
- **Owner headset check (build 142801):** handle visible and works, but one-hand carrying
  "jumps to a different place" and the table cannot be turned. Cause: OneGrabFreeTransformer
  applies the full wrist rotation to the root and then flattens pitch/roll, so with the
  handle 0.41 m from the pivot a small wrist tilt swings the centre by ~0.2 m, and wrist yaw
  is too limited to turn the table. Fix: new TableCarryTransformer (one hand): the grabbed
  point stays in the hand and the table yaws to keep its front edge toward the head, eased
  by slerp; wrist tilt/twist ignored. Pure rule TableAdjustRules.CarryPose (2 more tests,
  7 total). Two-hand plane transformer unchanged (free rotation + resize).
- **Build:** artifacts/qa/cargo-20260916-143730/airlift-cargo.apk, SHA-256 ce1860e3...; 56 checks across 12 suites; scan passed; 0 errors. Installed on the Quest (md5 8de8b9ab verified on device) for the owner's carry check.
- **Owner headset check (build 143730): "feels exactly the same".** Verified the installed
  APK (md5 8de8b9ab) and the scene contain the carry transformer; the device log showed
  TwoGrabPlaneTransformer running during a one-hand carry. **Root cause:** CargoCrew has two
  active ControllerGrabInteractors per hand (OVRComprehensiveInteractionRig's plus a
  standalone "[BuildingBlock] Controller Interactions" block; Onboarding has the same pair).
  With MaxGrabPoints 2 on the handle, one squeeze produced two coincident grab points, so the
  two-hand transformer ran (planar translate only, degenerate rotation/scale, jumps on
  release). The practice strap (MaxGrabPoints 1) hid this. Fix: disable the two extra
  interactors (AgentScripts/DisableDuplicateGrabInteractors.cs); test
  ExactlyOneActiveGrabInteractorPerHand added (CargoTerminalLayoutTests now 9). Note for the
  open grip defect: competing interactors is a candidate explanation worth re-checking.
- **Build:** artifacts/qa/cargo-20260916-144706/airlift-cargo.apk, SHA-256 9f69a09c...; 57 checks across 12 suites; scan passed; 0 errors. Installed on the Quest (md5 6929c4fe verified on device) for the owner's check.
- **Owner-reported (headset, build 144706):** "it works now... exactly what I was looking
  for." One-hand carry, two-hand rotate/resize and strap grabs all confirmed. Accepted and
  committed; no push.

### Why

Direct manipulation matches how the straps already work and needs no buttons or modes,
which the owner had just asked to remove. Targeting the station root keeps the card,
props and pieces together, and because all lesson geometry is station-local, resizing
changes nothing mathematical. The held-piece gate preserves the plan's rule that the
table never moves under a piece in the hand.

## 2026-09-16 — Toy look, centred pad, station controls removed (OWNER ACCEPTED on headset)

- Owner asks after the halves acceptance: (1) centre the measuring pad on the board,
  (2) remove Raise/Lower/Recenter station buttons, (3) a playful, polished look
  ("Hasbro, Lego") instead of sharp blocky primitives. Design approved in chat.
- **Layout:** pad and ruler now at the deck centre (x 0, z 0.02); tray row moved forward
  (z -0.17) so tray pieces sit outside the placement radius. Pieces rest on the surfaces
  (tray y 0.047, docked y 0.063) instead of floating 3 cm above them.
- **Controls:** three station buttons deleted from the lesson canvas; orientation text no
  longer says "Adjust the station below". Startup placement is unchanged. The system
  Meta-button recenter still works.
- **Look:** new runtime `RoundedBoxMesh` (pure, 7 EditMode tests incl. an orientation test
  against Unity's own cube) replaces every raw cube on the board with an exact-size rounded
  block; halves keep one flat cream cut face so docked halves still read as two pieces.
  Materials retuned to satin plastic in a warmer palette plus CargoYellow; warm key light,
  soft shadows, trilight ambient; pill buttons and rounded card via a generated 9-slice
  sprite; fraction labels on cream sticker plates; ruler marks moved to the pad's front rim
  in navy so docked pieces cannot hide them. 24 mesh assets under Assets/Airlift/Meshes.
- Editor scripts: ApplyToyLook, RefineToyLook, PolishRulerMarks, RefreshRoundedMeshes,
  PreviewToyLook. Layout tests added: StationControlsRemoved, MeasuringPadCenteredOnBoard,
  BoardObjectsAreRoundedNotRawCubes (CargoTerminalLayoutTests now 7).
- Desktop previews: artifacts/toy-look-practice.png, toy-look-fractions.png,
  toy-look-closeup.png. Not headset evidence. Build/device status: see below.
- **Build:** artifacts/qa/cargo-20260916-140241/airlift-cargo.apk, SHA-256 6490440791b4a08c92bf1265830c4973700fe16a53e3ea78d42cac40ec43a9ea; 48 checks passed (11 suites); credential scan passed; 0 errors / 10 warnings. Installed on the Quest for owner review (installed 14:13 after an ADB server restart; md5 142a655e verified on the device; launched).
- **Owner-reported (headset, build 140241):** "absolutely brilliant", loves the new look and
  feel, reads as Fisher-Price/Hasbro/Lego, smoother and more polished. Accepted.
- Owner's next ask before further fraction chapters: move the table to another location and
  make it bigger/smaller from inside the app (today only the Meta-button system recenter).
  Design pending owner approval; see the next entry when it exists.
- Onboarding scene untouched (regression reference). Committed after acceptance; no push.

### Why

The owner's playful-toy direction is a polish pass on top of an accepted core loop, so it
had to preserve every mathematical invariant: piece lengths, colliders and ruler snapping
are unchanged and only meshes, materials, light and layout moved. Rounded meshes are
generated, not purchased, keeping the no-new-assets constraint. Centring the pad and
lowering pieces onto the surfaces make the whole/halves comparison the visual focus.

## 2026-09-16 — Owner-reported: grabbing works, halves loop completed; committed

- Build under test: artifacts/qa/cargo-20260916-131831/airlift-cargo.apk, SHA-256
  18273ac61a1621d4f642dbf938660556a458d30d53a936c12d74f7bd09662732.
- Owner first saw nothing in the headset; ADB showed the Quest at its home shell with the
  app not running and no crash logged since install. Launched via ADB; VR focus, tracking,
  passthrough and both Touch Plus controllers came up cleanly.
- **Owner reported:** practice strap grab and manipulation works; the fraction chapter runs
  end to end (Start fractions, whole labeled 1, Split into halves, dock, Submit reads
  1/2 + 1/2 = 1). Owner calls it a simple early-stage lesson and accepted the progress.
- **Not established:** which input produced the grab. The build's temporary
  grip-or-trigger override and the practice-panel readout are still active, and the
  owner did not report the OVR/XR/SDK grip and trigger numbers from script steps 5-9.
  Treat the grip defect as mitigated, not root-caused; keep the override until answered.
- **Not yet checked:** board start height, release-on-pad completion text, Reset pieces,
  Back to lessons, both-hand matrix, label legibility at distance.
- Owner authorized committing all current work on `unity-airlift`. No push.
  `.gitnexus/` stays local via .git/info/exclude; the GitNexus blocks in CLAUDE.md and
  AGENTS.md are committed with the rest of the instructions.

### Why

The core loop the owner prioritized on September 16 (grab a whole labeled 1, split into
two 1/2 pieces, rebuild against a fixed whole) now works on the physical Quest by owner
report, which is the acceptance evidence the plan requires. Committing preserves the
tested state before the temporaries are removed and the next chapter starts.

## 2026-09-16 — Whole-and-halves chapter built, installed, awaiting device check

- Owner priority two (P1-04/P1-05 core loop) implemented as code-complete, not accepted:
  after practice, "Start fractions" shows a 0-to-1 ruler on the measuring pad, a whole
  strap labeled 1, Split into two exact half pieces labeled 1/2 (stacked notation via
  FractionNotationView), neutral docking on release, Submit evaluates the ruler
  deterministically (whole = 1; both halves = 1/2 + 1/2 = 1), Reset pieces, Back exits.
- New: Scripts/Lessons/CargoLessonModel.cs (pure, 6 tests), RulerLayout.cs (pure, 3 tests),
  CargoLessonDirector.cs (SDK adapter), AgentScripts/CreateFractionChapter.cs,
  FixFractionChapterLook.cs, PreviewFractionChapter.cs; OnboardingDirector gained a
  Ready-stage "Start fractions" primary button (UnityEvent). Desktop preview:
  artifacts/fraction-chapter-preview.png (geometry only, not headset evidence).
- APK artifacts/qa/cargo-20260916-131831/airlift-cargo.apk, SHA-256
  18273ac61a1621d4f642dbf938660556a458d30d53a936c12d74f7bd09662732; 38 checks passed
  (adds CargoLessonModelTests 6, RulerLayoutTests 3); scan passed; 0 errors / 10 warnings.
  Installed on the Quest (md5 verified). This build still contains the temporary practice
  input readout and grip-or-trigger override from the earlier entry today.
- Not verified on device: grabbing at all (root defect still open), docking feel, label
  legibility, Split/Submit flow. Known nit: CargoLessonDirector does not unsubscribe its
  pointer lambdas on destroy (single-scene app; fix in the next build).
- No commit/push.

### Why

The chapter reuses the practice strap's exact size as the whole and prebuilt halves so
"cutting" is a prefab swap with conserved cells, never geometry editing. The ruler is the
visible whole, docking is neutral and evaluated only on Submit, and every piece belongs to
one WholeId so pieces from another whole cannot mix. This keeps the math deterministic and
lets the owner test the full grab-split-rebuild loop in one headset session.

## 2026-09-16 — Grab diagnostic build installed; test-runner hang root-caused

- Owner handed implementation to Claude Code with priority: fix practice grabbing,
  then the labeled whole/halves loop. Voice/AI remains deferred.
- Static investigation exhausted before any fix: CargoCrew and Onboarding scenes are
  object-for-object identical except styling, cargo props, ComfortPlacement and the
  locomotor override; Android manifests and OpenXR runtime action bindings are
  byte-identical across the working Onboarding APK and the broken CargoCrew APKs;
  Quest OS (v207, built Aug 26) and controller firmware unchanged. The vendor selector
  chain (ControllerGrabInteractor -> ControllerSelector usage 16 GripButton ->
  FromOVRControllerDataSource -> OVRInput) carries no scene override. Grip cause is
  still unconfirmed; the index trigger demonstrably reaches OVRInput because the ray
  navigated the catalog.
- Built and installed artifacts/qa/cargo-20260916-130202/airlift-cargo.apk,
  SHA-256 44bee55ef6362885852a2e4b73bbc202b8e6f59acddef0594a1aa9ab8ae04d0b
  (installed md5 verified equal). 29 checks passed (12 onboarding, 3 placement,
  1 typography, 1 glyph, 4 layout, 4 new OnboardingPlacementTests, 3 baseline,
  1 PlayMode scene); bounded credential scan passed; 0 errors / 16 warnings, triage pending.
  This replaces the invalid VendorGrabProof APK on the device.
- Changes in this build: (1) startup placement waits for a tracked head instead of
  sampling the origin, which the editor log proved placed the board at 0.35 m;
  (2) TEMPORARY on-panel input readout during practice (OVRInput, Unity XR and SDK
  selector grip/trigger values plus grab interactor state), logged as
  [DEBUG-cargo-input] for 120 s; (3) TEMPORARY CargoCrew-only scene override so the four
  practice grab selectors accept grip OR trigger (AgentScripts/ApplyGrabSelectorFallback.cs).
  Old [DEBUG-cargo-placement] probes removed. Reference scenes untouched (hashes pass).
- Test-runner "deadline exceeded" stall root-caused with a process sample: the Unity
  Test Framework raised a native Scene(s) Have Been Modified alert because TextMesh Pro
  dirties CargoCrew on open; the editor main thread sat in NSAlert runModal. Dismissed
  with Save. scripts/verify_cargo.py now refuses play mode and runs
  AgentScripts/PrepareCleanScenes.cs (copies dirty state to artifacts/qa/dirty-scenes,
  reloads from disk) before tests. Wrapper tests: 5 passed.
- An accidental editor-only rotation of a truck Wheel was found in the unsaved scene
  state and discarded; the unsaved state is archived outside the repo.
- Not done: physical grab confirmation, warning triage, P1-02 remaining gaps (Reset,
  notation specimens, prefab extraction), free station repositioning, fraction loop.
  No commit/push.

### Why

Every static comparison between the working and failing builds came back equal, so the
only remaining lever was runtime evidence that the owner can read without ADB. The
readout shows at which layer the grip signal disappears, and the trigger fallback keeps
practice usable if the grip path is the defect. Placement now waits for tracking because
the log showed the head at the origin at Start, which is the exact floor-height symptom.

## 2026-09-16 — Owner requested pause and Claude Code handoff

- Wrote HANDOFF-2026-09-16.md with facts, hypotheses, installed artifact, known
  errors, invalid vendor experiment and safe resume recommendations.
- Updated CLAUDE.md, AGENTS.md, shared handoff, progress, README, backlog,
  current ticket/voice status, PRD/requirements and QA notices. Historical records
  retained with prominent superseding status, not silently rewritten as success.
- Vendor APK built, credential scan passed, installed and launched. Owner sees
  platform but no cube. Prefab inspection found hand-grab support but no controller
  GrabInteractable; experiment invalid. Cube visibility cause not established.
- Cargo runtime diagnostic observed Hover/candidate=true, selected=false, and
  sampled grips zero. No confirmed cause or fix; no current ticket acceptance.
- Implementation paused; no new commit/push. Documentation diff check passed.

## 2026-09-16 — Isolated vendor-grab control experiment

- Owner authorized a separate vendor test scene/build. Created VendorGrabProof
  using fresh Meta passthrough/controller Building Blocks and the supplied
  [BB] Grabbable Cube prefab. No lesson scripts, custom grab wiring, or vendor edits.
- Explicit stationary-MR override disables FirstPersonLocomotor; floor-level
  tracking, fixed cube/platform placement and transparent cameras are configured.
- Builder checks one camera rig and one supplied Grabbable. Reference Onboarding
  and DeviceProof scene hashes remain unchanged. CargoCrew scene untouched.
- This is a diagnostic control, not ticket acceptance. Build/install/device outcome
  must be recorded separately. No commit/push.

## 2026-09-16 — Core arithmetic prioritized; grab diagnostic added

- Diagnostic verification initially timed out in editor play mode. Exited that
  session and requested script-domain reload; repeat run passed all 25 checks.
- Diagnostic APK: artifacts/qa/cargo-20260916-114658/airlift-cargo.apk.
  Build and bounded credential scan passed; ADB replacement install succeeded.
  This adds diagnostic observation only, not a verified grab fix. Owner reproduction
  is required within the bounded two-minute diagnostic sampling window after launch.

- Owner deferred voice/AI integration and supporting audio controls. Prioritize
  functional grabbing and the labeled whole-to-halves loop before launcher/polish.
- Owner confirms stable station and visible cargo props; practice grabbing fails
  despite controller contact. No acceptance inferred from visuals.
- Added bounded temporary DEBUG-cargo-grab runtime samples for grip input,
  target activation/colliders, interactor state, selection and contact distance.
  Compilation passed; verification/build active, not installed or a verified fix.
- Startup height and free repositioning remain open; no commit or push.

## 2026-09-15 — Disappearing station traced to gravity locomotor

- Owner repeatedly reproduced apparent table ascent. Device samples show fixed
  station with rapidly falling virtual head; not a moving table or button event.
- Found active FirstPersonLocomotor inherited from Meta comprehensive rig; added
  failing stationary-rig check, then disabled component in CargoCrew scene override.
- No vendor package/reference scene edits. Regression/build running; physical
  confirmation and removal of temporary diagnostic probes still pending.

## 2026-09-15 — Owner-requested intermediate ticket 2.5

- Added CC-P1-02.5 for recognizable app branding and independent headset launch.
- Includes verification of sideloaded-app library/dashboard limitations; no store
  upload, third-party launcher or distribution change is implied.
- Backlog now 29 tickets; disappearing-station bug remains the active P1-02 priority.

## 2026-09-15 — P1-02 visual checkpoint APK ready

- Full verification/build run completed successfully: 24 Unity checks, bounded APK
  credential scan; 5 verifier tests and diff check pass. No skipped-test override.
- artifacts/qa/cargo-20260915-182240/airlift-cargo.apk, SHA-256
  71aa75bebcf44bf481b3e5171534862c1ef3b38762e776ee32f524c05e7dd919.
- Build: 0 errors / 10 warnings, pending warning triage. No commit/push.
- Quest connected but unauthorized: owner USB approval needed before installation.
- Visual checkpoint ready, not full ticket acceptance. Remaining notation/prefab/reset/
  focus details and device profiling remain explicit in P1-02 QA.

## 2026-09-15 — P1-02 typography integrated

- Owner delegated routine design choices; selected A (Nunito Bold / Nunito Sans
  Semibold). Static font sources and OFL notices are recorded in the project.
- Explicit SDF bindings, focus outlines and placement feedback applied to CargoCrew.
- New-font overflow was reproduced and repaired through shorter CargoCrew-only
  instructions, not smaller text. Reference content/scenes preserved.
- Compilation and six-page text-fit inspection pass; regression/build in progress.
- Ticket remains incomplete: notation specimens, prefab/reset/focus details and
  device/profiling checks still pending. See P1-02 QA record.

## 2026-09-15 — P1-02 initial cargo environment and placement

- Owner authorized next ticket with baseline physical review batched at this checkpoint.
- CargoCrew now contains original truck/containers/staging/aircraft/destination labels
  and three station controls. Reference scene hashes unchanged. No new packages.
- Compile passed; 3 ComfortPlacement tests passed; catalog and existing text-fit
  inspection passed. Fixed oversized destination labels found in desktop preview.
- Actual-font browser comparison prepared; both candidate font loads verified. Unity
  font integration awaits owner choice. No new APK/device check; ticket in progress.
- Full remaining scope and evidence: docs/qa/cargo-CC-P1-02.md.

### Why

Original static cargo silhouettes establish context without new packages or runtime
spawning. Moving the shared station root keeps props and learning materials aligned;
held-object movement is refused. Font selection remains an explicit owner review.

## 2026-09-15 — Baseline APK built; bounded scan passes

- Approved CargoCrew APK succeeded with 0 errors / 8 warnings; warning triage pending.
- Artifact: artifacts/qa/cargo-20260915-180248/airlift-cargo.apk.
  SHA-256: 0c77ee9e1bfbbc13083d5ca38d6d3ebfba54643cbc4444184f078932db3e7e58.
- Raw scan falsely combined adjacent IL2CPP language literals. Corrected scanner to
  respect installed v108 metadata string boundaries, preserving actual token detection.
- Five verifier tests pass; existing APK separately rescanned successfully. The original
  wrapper exited at the false positive; do not describe it as a clean uninterrupted run.
- 16 Unity checks passed before build. No new APK installed, no device acceptance,
  no commit/push. See docs/qa/cargo-CC-P1-01.md for recovery and artifact lineage.

### Why

The test-runner domain refresh restored execution without skipping tests. Parsing
IL2CPP string boundaries prevents false secret matches across unrelated literals
while retaining checks on actual strings. Device review remains a separate checkpoint.

## 2026-09-15 — Test-runner recovery; approved baseline APK building

- Owner approved local build from uncommitted work; no commit/push required or made.
- Reproduced zero PlayMode results despite successful list_tests and Pipeline selector
  discovery. Exact-name and explicit-test switches did not fix execution.
- Official script-domain reload restored the full verification sequence: 12 onboarding,
  3 baseline, 1 scene tests passed in artifacts/qa/cargo-20260915-180248.
- Root cause within Unity/Pipeline remains unresolved; domain refresh is a verified
  recovery only. No vendor patches. Temporary diagnostic script removed after use.
- Build status verified as building. APK completion, credential scan and device review
  remain pending; no new fraction gameplay or next-ticket completion is claimed.

### Why

This distinguishes a recovered test runner from a proven permanent fix. Zero tests
still fail closed; no checks were skipped to start the explicitly approved local build.

## 2026-09-15 — CC-P1-01 implementation, build authorization pending

- Owner authorized only Phase 1 ticket 1 and batching physical review at meaningful checkpoints.
- Created non-overwriting CreateCargoCrew builder, CargoCrew scene copy, CargoBuild entry,
  milestone manifest, Python/CLI verification wrapper and baseline EditMode/PlayMode tests.
- Existing reference scene hashes unchanged; no new packages or gameplay changes.
- Verified: compilation completed without errors; 12 onboarding + 3 baseline EditMode +
  1 PlayMode tests passed; 4 verifier tests passed; git diff --check passed.
- Evidence: artifacts/qa/cargo-20260915-173318 (JSON and generated test XML).
- First new-suite attempt correctly failed on zero discovered tests before explicit import/
  compilation. Recompile and rerun passed; not counted as an initial passing run.
- Build blocked by uncommitted work as designed. No APK created/installed; no commit/push.
- Status remains in-progress, not code-complete or accepted. Build polling/actual APK scan
  are implemented but not exercised on a new real build yet; scan coverage is bounded.
- Next: owner chooses a documented local development-build exception or reviewed Git
  checkpoint; do not bypass guard or start ticket 2. Physical checks remain batched.

### Why

The baseline is copied rather than rebuilt so its known interaction behavior and reference scenes remain intact. Explicit scene selection and fail-closed test/build checks prevent a successful command from disguising missing tests or the wrong scene. Device acceptance remains pending, and the uncommitted-work guard requires owner direction before an APK build.

Newest entries first. This is the live progress log from September 15 onward.
Earlier history remains in [PROGRESS.md](../PROGRESS.md). Never mark planned work done.

## 2026-09-15 — Cargo setting made explicit in tickets

- Owner authorized documentation/ticket updates for a recognizable cargo experience.
- Retained stationary mixed reality; no full-VR port, driving or new XR framework.
- P1-02 owns truck, containers, loading/measuring platform, destinations and parked
  aircraft; P1-03 introduces their purpose. P1-07 owns accepted-job parcel progress,
  later activities reuse it, and P3-02 completes dispatch.
- Added F-21 and named layout/progress/reset checks; updated story, style and plan.
- Phase counts remain 10/8/6/4. No game code, models, imports, builds or commits.

### Why

The setting should explain the learner's job before arithmetic begins and visibly
respond to completed work. Bringing recognizable cargo props forward while keeping
progress tied to deterministic lesson acceptance makes the real-world context
part of the activity without adding a vehicle simulator or obscuring fraction meaning.

## 2026-09-15 — Core AI guide, style guide and testing cadence revision

- **Status:** planning changes authorized; implementation remains paused for owner review.
- **Owner decisions:** AI voice guidance is core throughout Cargo Crew; spoken questions
  are separate. A dedicated VR/AR/Unity style guide is required. Only Cargo Crew is in scope.
- **Authored:** STYLE-GUIDE.md, VISUAL-DESIGN.md and AI-VOICE-GUIDE.md; synchronized
  PRD, plan, requirements, systems design, server contract, ticket index and primers.
- **Design:** Nunito/Nunito Sans are comparison candidates, not selected/imported fonts.
  Shared tokens, fraction readability, accessible input and actual headset review are specified.
  The 3D Interaction Design skill informed spatial comfort/input; Game Developer informed
  reusable Unity assets and state-driven presentation, not invented validation claims.
- **AI:** live constrained model selection starts P1-03, followed by validated text and
  matching reviewed speech assets; recordings are fallback only. Direct audio and child use retain explicit gates.
  Provider/account/transport/budget readiness is not verified.
- **Review:** challenger identified that allowed vocabulary/transcripts alone cannot
  guarantee correct spoken math; contract now uses approved fact/action/template IDs
  with semantic checks. P1-03 owns a finite reviewed speech catalog: adult listening
  plus runtime hash/context checks, not a transcript-only safety claim. The live model
  selects explanations; unreviewed new audio uses labeled fallback.
- **Cadence:** change-level tests, ticket-level device checks, full-journey milestone
  review, then owner-authorized checkpoint. No merge to main is needed for headset preview.
- **Unchanged:** 28 draft tickets (10/8/6/4), no new implementation, provider calls,
  audio/font imports, Unity build, headset test, commit or push during this revision.
- **Next:** final review evidence in REVIEW.md; present readiness and first ticket
  for owner implementation approval. This entry supersedes the older recorded-first proposal.

### Why

The owner requires a coherent designed learning experience with AI spoken guidance,
not an arithmetic scene with optional narration. Moving its first verifiable slice
early exposes provider and device risks before building the entire lesson. Shared
style rules and small accepted tickets preserve consistency while leaving runtime
claims dependent on real tests and owner review.

## 2026-09-15 — Fraction planning revision (not an implementation ticket)

- **Status:** draft for owner review; no ticket started.
- **Branch/workspace:** unity-airlift, /Users/jad/Desktop/math-workbench-airlift.
- **Latest existing commit:** fd7895f; onboarding code and documentation currently
  include uncommitted work. This planning task did not commit or push anything.
- **What was authored:** local 00-build PRD, requirements, constraints, stack,
  systems design, story/cue script, implementation plan, template, shared
  Claude/Codex handoff and 28 full draft ticket primers (10/8/6/4).
- **Methodology:** adapted LabelCheck's structure and completion rationale pattern.
  Its files were not changed; its web commands and automatic merge rules were not adopted.
- **Research:** existing Unity/Meta source inspected for button chords, two-piece
  merge and scaling; primary pedagogy and ElevenLabs policy/API references retained
  under the existing RAP run. No package or provider integration performed.
- **Important gate:** ElevenLabs child-targeted-use restrictions require permission
  review; local authorized adult-recorded narration is the proposed fallback.
- **Verification:** planning-file structural/link/count checks only, recorded in
  REVIEW.md. No new Unity compile, test, build, installation or headset test in
  this planning task. All ticket test suites beyond prior onboarding are proposed.
- **Scope decisions awaiting owner review:** narrated-first versus live guide;
  split/merge and synchronized zoom versus free resizing; 10/8/6/4 backlog and
  a realistic first-chapter cut line. Café and Garden remain unimplemented.
- **Next:** review PRD → STORYBOARD → PLAN → Phase 1 tickets together. Do not begin
  CC-P1-01 until approved.

### Why

The next deliverable is a reviewable learner journey and executable ticket sequence,
not another isolated interaction. The plan preserves the working headset foundation,
makes mathematical meaning and narration explicit, and separates full product scope
from a small accepted first chapter. Provider-neutral audio avoids making the lesson
depend on unresolved child-targeted service permissions. Template completion fields
remain factual-at-start/at-completion rather than inventing future progress.

## 2026-09-15 — Owner-reported onboarding acceptance checkpoint

- **Artifact:** artifacts/airlift-onboarding.apk.
- **SHA-256:** 9559d0bda634e89a81a5a88b7c0d487efc9c0c9baa99210d5fede766462d7edf.
- **Previously recorded build:** build_e54958879ec6, zero errors/10 warnings;
  12/12 OnboardingFlowTests and layout/reference check passed.
- **Connection:** after unsuccessful reconnect/restart attempts, toggling Developer
  Mode off/on with USB connected caused a debugging prompt. Owner approved; ADB
  became authorized; install and launch succeeded. Cause of lost authorization
  was not established; do not present the successful recovery as root-cause proof.
- **Owner confirmed:** Arithmetic Lessons catalog and three cards, Cargo Crew
  mission/briefing, successful grab/place practice and replay.
- **Still unverified:** both-controller complete matrix, seated/standing reach,
  repeated interaction counts, Back round trip, pause/tracking recovery, capture
  and measured performance. No actual fraction activity or voice guide exists.
- **Outcome:** onboarding is a working foundation, not the finished lesson.

## Entry template for accepted work

Date / ticket / branch / starting commit:
Status: in-progress, code-complete, accepted or blocked:
Actual changes and observed outcome:
Automated commands/results and failures:
Physical checks, interventions and artifact/hash:
Deviations and owner approval:
Why: paste the ticket's completed Why paragraph verbatim.
Checkpoint commit/push status (only what actually happened):
Next ticket/action and remaining gates:
