# Cargo environment checkpoint — in progress

## September 16 (evening) — toy-look build: owner-reported PASS on look and feel

- CURRENT installed APK: artifacts/qa/cargo-20260916-140241/airlift-cargo.apk, SHA-256 6490440791b4a08c92bf1265830c4973700fe16a53e3ea78d42cac40ec43a9ea; 48 checks + scan passed. Supersedes 131831 (installed 14:13 after an ADB server restart; md5 142a655e verified on the device; launched).
- Changes: pad/ruler centred, tray forward, station buttons gone, rounded blocks, satin
  materials, shadows, pill buttons, sticker labels, navy ruler marks on the front rim,
  cream cut faces on halves, pieces resting on surfaces. Math/colliders unchanged.
- Owner check: 1. Board appears at desk height, pad in the middle of the board. 2. No
  Raise/Lower/Recenter buttons; card and buttons rounded. 3. Practice grab still works
  (report grip vs trigger if possible). 4. Start fractions: read 0 / 1/2 / 1 on the pad's
  front rim; whole docks and snaps; Split shows the cream cut faces; Submit reads
  1/2 + 1/2 = 1. 5. Look and feel: does it read as playful/toy-like? What still feels
  blocky or flat? 6. Any piece clipping into the deck or pad when released.

## September 16 (afternoon, later) — owner-reported PASS on grab and halves loop

- Build 18273ac6 (below). Owner: practice strap grabs and moves; Start fractions, Split
  into halves, dock and Submit produce "1/2 + 1/2 = 1". Simple but working.
- Steps 5-9 numbers (OVR/XR/SDK grip and trigger) NOT reported; unknown whether the grab
  used grip or the temporary trigger override. Steps 2, 10, 12 (snap), Reset and Back not
  explicitly reported. Grip defect: mitigated, not root-caused. Keep override until answered.
- Note: on the first attempt the owner saw nothing because the app was not running
  (Quest home shell); launched over ADB. Add "confirm Airlift is the foreground app" to
  step 1 for future sessions.

## September 16 (afternoon) — grab diagnostic build installed, physical check pending

- CURRENT installed APK: artifacts/qa/cargo-20260916-131831/airlift-cargo.apk, SHA-256
  18273ac61a1621d4f642dbf938660556a458d30d53a936c12d74f7bd09662732 (md5 f2ac8bf2f6ace99fd1f9e317c714bbb0
  verified on device). 38 checks + scan pass. Supersedes 130202 (29 checks), which had the
  same diagnostics without the fraction chapter.
- Contains: tracked-head startup placement fix; temporary practice-panel input readout;
  temporary grip-or-trigger practice grab override (CargoCrew only). See DEV-LOG.
- Still unverified on device: board height at start, grab with grip, grab with trigger,
  release-on-pad completion. Owner script:
  1. Wear headset, both controllers awake; launch Airlift from Library > Unknown sources.
  2. Note where the board appears (desk height expected, not floor).
  3. Cargo Crew > Begin briefing > Watch demo > wait for "Your turn".
  4. Read the small readout lines: OVR (g=grip, t=trigger per hand), XR, SDK, GRAB.
  5. Hold LEFT grip 3 s away from the strap; report OVR L g, XR L g, SDK grip.
  6. Same for RIGHT grip. 7. Hold LEFT trigger 3 s; report OVR L t, XR L t, SDK trig.
  8. Touch the strap, hold grip: does GRAB show SEL and does the strap move?
  9. If not, touch the strap and hold TRIGGER: same questions.
  10. Release above the pad: does the text change to "You moved the strap..."?
  Optional: run `adb logcat -s Unity | grep DEBUG-cargo-input` on the Mac during steps 5-9.
  11. If practice completed: choose Start fractions. Is the whole strap (labeled 1) in the
      tray and the 0 / 1/2 / 1 ruler on the pad readable?
  12. Grab the whole, lay it along the ruler, release: does it snap onto the ruler? Submit.
  13. Choose Split into halves: two 1/2 pieces appear in the tray? Dock both on the ruler,
      Submit: text should read "1/2 + 1/2 = 1". Try Reset pieces and Back to lessons.

## September 16 physical evidence — NOT accepted

See [complete handoff](../00-build/HANDOFF-2026-09-16.md).
Owner confirms gravity fix keeps station still and cargo props look correct.
Board initially too low; system recenter helps. Free movement is not implemented.
Practice strap fails to grab after demo; actual practice instructions confirmed.
Diagnostic build passed 25 checks and scan, but still fails physically: active
collider/target, Hover/candidate=true, no selection, sampled grip=0.00. Cause unknown.
Vendor control APK replaced CargoCrew: platform visible, cube missing, controller
GrabInteractable absent on supplied cube. Invalid comparison, not a tested fallback.
Reset control/notation specimens and other ticket gaps remain. Older entries follow.

September 15, 2026. Owner authorized continuation and batched baseline/device review.

## Device failure — blocks acceptance

- Fix build artifacts/qa/cargo-20260915-183902/airlift-cargo.apk passed 25 Unity
  checks and bounded scan; install returned Success and cold launch requested.
  Await physical observation and diagnostic samples before declaring resolved.

- Diagnostic samples reproduced stationary root (y=0.35) with head y falling from
  approximately -3 to -176 over six seconds; no adjustment call occurred. Owner
  reported apparent table ascent during the same run.
- Found enabled FirstPersonLocomotor in Meta comprehensive rig. Its Update applies
  gravity and LastUpdate moves the player origin. Stationary-scene guard failed
  before fix. Disabled this component via CargoCrew prefab-instance override only.
- Added StationarySceneHasNoActiveGravityLocomotor regression and scoped editor
  scene checks to CargoCrew. Build/test run in progress. Device confirmation pending;
  temporary pose probes remain in this diagnostic fix build until confirmed.

- Installation and launch succeeded but owner reports the table rises out of view,
  leaving no usable catalog/onboarding. Repeated cold restarts did not recover it.
- Owner explicitly requested diagnosis/fix. Do not count Android launch success or
  passing editor tests as device acceptance. Stop further lesson work until resolved.
- Root station has no Rigidbody; custom movement is startup placement and explicit
  ComfortPlacement actions. Logs show tracking acquired seconds after launch.
- Ranked hypotheses: initial pose/tracking-origin timing; unintended placement
  callbacks; rendering/visibility. Diagnostic build adds six timed root/head snapshots
  and adjustment-call logging, no gameplay changes. Remove probes after resolution.

## Typography and controls continuation

- Owner toggled Developer Mode and approved Always allow USB debugging. ADB reported
  device authorized; install -r returned Success for the visual-checkpoint APK.
  Launch requested. Owner-visible behavior and physical acceptance still pending.

- Visual checkpoint APK built successfully: artifacts/qa/cargo-20260915-182240/airlift-cargo.apk.
  SHA-256 71aa75bebcf44bf481b3e5171534862c1ef3b38762e776ee32f524c05e7dd919.
  0 errors / 10 warnings (triage pending); full wrapper passed 24 Unity tests and
  bounded credential scan. Five verifier tests and git diff --check also pass.
- Quest reports unauthorized at handoff. APK not installed. This is a visual-review
  checkpoint, not completion of all P1-02 acceptance criteria listed below.

- Owner delegated routine design decisions; selected direction A without further font
  approval prompts. Imported static Nunito Bold and Nunito Sans Semibold with OFL
  notices and source/hash records in Assets/Airlift/Fonts/SOURCES.md.
- Applied explicit static SDF assets, shared AirliftStyle/TypographyBindings, button
  focus outlines and visible feedback when placement is blocked/changed.
- Initial new-font text-fit check failed (257.8 units versus 250 available). Shortened
  the CargoCrew-specific content asset without reducing font size or touching the
  reference Onboarding asset. All six pages now fit (largest 220.97 units).
- FractionNotationView code exists but static specimen prefab/layout tests remain
  pending. No fraction lesson gameplay is implied.
- Current regression/build run underway; no P1-02 APK or headset acceptance yet.
- Remaining ticket work includes prefab extraction, notation specimens/tests, reset
  control, full single-pointer focus behavior, physical/profiling verification.

- Added original primitive-built truck, two ribbed containers, parcel staging, parked
  aircraft and destination labels to CargoCrew only. No marketplace assets acquired.
- Added ComfortPlacement with Raise/Lower/Recenter controls. Held-state guard and
  coherent-root movement have 3 passing EditMode tests, including invalid position.
- Compilation passed. Existing scene inspection passes: one rig, three lesson cards,
  only Cargo Crew enabled, six current instruction texts fit.
- Desktop image inspected; caught and corrected oversized world-space destination text.
  Preview: artifacts/onboarding-catalog.png. This is not headset evidence.
- DeviceProof and Onboarding SHA-256 remain their pinned baseline values.
- Typography comparison: docs/qa/font-comparison.html, actual remotely loaded Nunito
  and Nunito Sans fonts verified in browser. Google Fonts source OFL notices reviewed;
  no fonts imported into Unity yet. Desktop direction review only, not static-TMP/glyph QA.
- Pending: owner font choice, static fonts/license copies and explicit TMP bindings,
  style asset, stacked fraction component, pointer/status feedback, prefab extraction,
  layout/integration tests, full regression, new APK and physical comfort/readability.
- No device install, commit or push. Do not mark this ticket complete.

## Why

Original static cargo silhouettes establish context without new packages or runtime
spawning. Moving the shared station root keeps props and learning materials aligned;
held-object movement is refused. Font selection remains an explicit owner review.
