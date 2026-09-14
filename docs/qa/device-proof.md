# Setup and first headset interaction — checkpoint closed

September 14, 2026. Build/install verified; passthrough, controller tracking and
successful grabbing confirmed by the owner. Full Phase 1 QA remains incomplete.

## Editor evidence

- Official Pipeline `editor_status`: ready, not compiling, correct Desktop project.
- `CreateDeviceProof.Run`: compiled and executed successfully; saved scene.
- `ConfigureQuestProof.Run`: executed successfully; configured Android/OpenXR.
- Scene: one Meta camera/interaction rig, passthrough, controllers, virtual bench,
  SDK-grabbable 12 cm cube; no locomotion controller added.
- Geometry uses an initial fixed floor-relative bench height of 0.78 m. It is a
  proof fixture, not the final placement/recenter/accessibility implementation.
- Product fraction logic, progression, tests, and adaptive tutoring are not built.

## Build status and safety

First development build succeeded but was not installed or distributed: Meta
SDK 205's DevAgentBuildProcessor injects a local token even when disabled.
Local settings/crash dumps are excluded from Git. `ProofBuildSafety` uses an
order-10000 callback to disable the bridge and clear tokens after vendor injection.

The subsequent non-development build `build_99cee7a052f6` succeeded: zero errors,
nine warnings, 197.256 seconds. The log confirms the safeguard ran and Unity
Pipeline is disabled in Player builds. A streamed comparison across the unpacked
APK found no occurrence of the local developer token. This is a targeted check,
not a comprehensive security audit.

Installed artifact: `artifacts/airlift-safe.apk` (approximately 68 MiB, ignored by Git).
SHA-256: `26ddbc41b03deec8fddebf985826530c72fe568921ee1e6578b2870f193600f8`.
Bundled ADB reported fresh install `Success`; launch of
`com.jad.airlift/com.unity3d.player.UnityPlayerGameActivity` succeeded.
USB temporarily disconnected/reverted to unauthorized; reconnect and owner
approval restored access. No device identifier is stored in this record.

Open warnings to review before release: single GameActivity entry selection,
MR splash background, input-polling latency, Meta/OpenXR requested API version
difference. Remaining warnings concern disabled Pipeline and large TextMeshPro
generated compilation units. Successful compilation is not release clearance.

## Owner checks after a safe build succeeds

- [x] Fresh installation and launch on Quest 3S (ADB verified).
- [x] Owner reports real room, blue workbench, orange cube and moving controllers visible.
- [x] Owner reports successful cube grab after initially having trouble finding the interaction.
- [ ] Explicit stereo/comfort assessment.
- [ ] Ten grabs/releases; cube follows and releases without duplicate ownership.
- [ ] Bench/cube reachable and legible without stepping toward unseen obstacles.
- [ ] Pause/resume works.
- [ ] Ten-second readable capture with no private surroundings or other people.

Record failures, observed behavior, final APK SHA-256, build duration and exact
versions here. A saved scene or successful compilation does not check these boxes.

## Milestone boundary and next work

Owner requested closing this setup/basic-interaction checkpoint before proceeding.
This does not mark all Phase 1 acceptance criteria complete. Ten grabs, reach,
pause/resume and capture remain carry-forward checks before full device-proof signoff.
The approved next product slice is cargo-role briefing, a guided grab interaction,
one visible whole, equal halves, and building one-half. Include replayable help
and chapter purpose. Storytelling, onboarding and arithmetic behavior are not
implemented in this checkpoint; the cube is an infrastructure test only.
