# Onboarding MVP — implementation and acceptance

## September 16 regression notice

Historical owner success below applies to earlier builds only. Current CargoCrew
practice grabbing FAILS; no current acceptance implied. See
[handoff](../00-build/HANDOFF-2026-09-16.md) for installed diagnostic, evidence and
rollback artifact. Do not treat editor tests as physical interaction proof.

September 14, 2026. Scene: `unity/Assets/Airlift/Scenes/Onboarding.unity`.
Status: authored, editor-checked and Android build verified. Installed and launch
command succeeded September 15; owner confirmed catalog, briefing, grab/place
completion and replay. Remaining physical acceptance is still pending.
This is not the completed arithmetic lesson or hackathon release.

## Included

- Arithmetic Lessons catalog: Cargo Crew / Fractions is available. Neighborhood
  Café / Division and Community Garden / Multiplication are disabled Coming soon
  cards, per the owner's explicit scope update.
- Cargo mission and purpose before interaction instructions; a striped example
  demonstrates tray-to-pad movement before the player tries it.
- Meta controller grab and ray-operated UI; practice requires a grab followed by
  release near the outlined pad. Missed practice placements return to the tray
  with descriptive retry instructions, no penalty. This motor tutorial behavior
  is not mathematical answer filtering.
- Replay demonstration and return to the lesson menu. Navigation while holding
  asks the player to release first. Completion explicitly says arithmetic is not
  included yet; no false lesson-unlock or proficiency claim.
- Editable ScriptableObject content; precreated demonstration object, no repeated
  Instantiate/Destroy loop. No narration, music, network or AI calls.

## Verified evidence

- C# compilation passed. Twelve EditMode `OnboardingFlowTests` passed (12/12,
  zero failed/skipped, latest run 0.4 seconds). These are state tests, not device
  input or tracking tests.
- `CheckOnboarding.Run` verified one rig, three catalog buttons with exactly one
  enabled, and six instruction pages fitting the 250-unit text area. Measured
  preferred heights: 216.45, 216.45, 216.45, 185.41, 92.26, 216.45.
- Catalog rendered through a temporary editor camera and visually inspected:
  `artifacts/onboarding-catalog.png`. This is an editor render, not headset footage.
- TMP Essential Resources imported from the installed official Unity UI package.
  Preserve its Liberation Sans OFL notice and package resource notices.
- DeviceProof scene remains unchanged from the accepted cube checkpoint.
- Game Developer skill informed the small state machine and editable data;
  3D Interaction Design informed stationary placement and large ray buttons.
  Neither substitutes for device testing or evidence of learning effectiveness.

## Build / install

- Requested non-development Android build `build_e54958879ec6`, output
  `artifacts/airlift-onboarding.apk`, Onboarding scene only, options `[]`.
- Build succeeded in 75.613 seconds, zero errors and ten warnings; APK approximately
  69 MiB. SHA-256:
  `9559d0bda634e89a81a5a88b7c0d487efc9c0c9baa99210d5fede766462d7edf`.
- Known local Meta developer token was absent from unpacked APK contents. The
  existing `ProofBuildSafety` preprocessor remains included. This targeted check
  is not a comprehensive security audit.
- ADB listed no devices after the build; no installation or launch attempted.
- September 15: after repeated unauthorized connections, the owner toggled
  Developer Mode off/on with USB connected and saw the debugging prompt. Owner
  approved access; ADB then reported an authorized device. `adb install -r`
  succeeded and the app launch command succeeded. Subsequently the owner confirmed
  catalog visibility, Cargo Crew briefing, successful grab/place and replay.
  This is owner-reported evidence, not a full controller/comfort/recovery matrix.
- Build warnings remain: Meta GameActivity configuration and MR splash background,
  recommended latency mode, two OpenXR API patch notices, deliberately disabled
  Pipeline runtime, TMP shader pragma deprecation and three IL2CPP large-method
  notices. Review compatibility/splash items before release; do not count a
  successful build as resolving those warnings.
- Preserve `artifacts/airlift-safe.apk` as the earlier cube proof rollback build.

## Physical acceptance — partial owner confirmation

September 15: catalog/three cards, Cargo Crew briefing, grab/place completion and
replay were owner-confirmed. Items below remain detailed follow-up checks;
Back-to-Lessons, both-controller coverage, comfort and performance are not inferred.

1. Fresh launch shows the catalog at a readable, comfortable distance, with
   passthrough and both controllers. No duplicate camera or unwanted locomotion.
2. Either controller can point/trigger Cargo Crew. Neither Coming soon card opens.
3. The mission, goal and button progression make sense without oral coaching.
4. Demonstration is visible on the bench; the player can replay it.
5. Grab/place with each controller, wrong placement then retry, regrab during
   release, Back and Help while holding, repeated menu visits: no dead end.
6. Verify seated/standing reach, legibility, pause/resume and tracking-loss
   recovery. Initial placement is world-locked; height adjustment/recenter are
   not implemented in this slice and remain release requirements.
7. Record readable 10-second headset footage and actual CPU/GPU timings. Project
   target remains 72 Hz, p95 CPU/GPU each ≤13.9 ms; not measured yet. No performance
   acceptance is inferred from the editor preview or pure tests.

Known scope: primitive strap/workbench, written storytelling, no aircraft model
or animated delivery payoff yet. Fraction model drafts compile but are untested
and unused by this onboarding scene. Full Phase 1 QA remains open separately.
