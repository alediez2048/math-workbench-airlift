# Project progress and R&D log

## Current status — September 16, 2026

Owner paused work for Claude Code handoff. Read [current handoff](00-build/HANDOFF-2026-09-16.md).
Cargo visuals and stationary placement were owner-observed; practice grab fails.
Installed APK is VendorGrabProof, not CargoCrew; cube invisible and prefab lacks
controller-grab support, so that comparison is invalid. No fix or ticket acceptance.
Voice deferred in favor of functional whole/halves. Older sections below are history.

September 15 handoff: new progress is recorded in [docs/00-build/DEV-LOG.md](00-build/DEV-LOG.md).
This file retains earlier history. Owner has now confirmed catalog, briefing,
grab/place and replay on the headset; see the specific [updated QA](qa/onboarding.md).
Fraction implementation is paused for review of [the new plan](00-build/PLAN.md).
Earlier statements below about undecided tickets or all onboarding QA pending
describe their original checkpoint, not the latest status.

Last updated: September 15, 2026 (America/Chicago).

Active checkout: `/Users/jad/Desktop/math-workbench-airlift`.
Working branch: `unity-airlift`. Use this Desktop copy for all future edits.

This is the operational checkpoint and handoff record, not a replacement for the
[vetted plan](plans/2026-09-13-unity-vet/plan.md). The setup/basic-interaction
checkpoint is closed at the owner's request: safe APK built and installed;
owner confirmed passthrough, controllers, and successful cube grabbing. The
lesson is not built and full Phase 1 QA remains incomplete.

## Resume here

1. Read [device-proof evidence](qa/device-proof.md); do not repeat completed setup.
2. Finish carry-forward Phase 1 checks: ten grabs/releases, comfort/reach,
   pause/resume, and a ten-second capture. Review recorded build warnings.
3. Onboarding-first slice is now authored: three-card catalog, cargo-role briefing,
   demonstration and required grab/place practice. See [onboarding QA](qa/onboarding.md)
   for build status and the next physical checks. Arithmetic is not connected yet.
4. Owner authorized this implementation after the setup checkpoint. Two future
   topic cards are disabled previews only. Formal ticket setup remains undecided.
5. September 16 afternoon: owner reports practice grabbing and the whole/halves
   fraction chapter working on the Quest (build 18273ac6). See
   [DEV-LOG](00-build/DEV-LOG.md) newest entry and [cargo QA](qa/cargo-CC-P1-02.md).
   Grip-vs-trigger readout still unreported; temporaries remain in the build.

## Workstream checkpoint

| Workstream | Status | Evidence and next action |
|---|---|---|
| Research and planning | Draft/vetting complete; execution gates open | [Plan](plans/2026-09-13-unity-vet/plan.md), [research](plans/2026-09-13-unity-vet/research.md). Native Unity, one fraction flagship, conditional Cargo Grid. Not proof of feasibility on this device. |
| Product documentation | Drafts complete | [PRD](../prd.md), [requirements](../requirements.md), [constraints](../constraints.md), [tech stack](../techstack.md). Keep aligned with the authoritative plan. |
| Mockup development | Reference package complete | Five interactive views, seven PNG references, [recorded browser checks](mockups/verification.md). No Unity interactions or live AI implemented. |
| Skills/reuse research | Official automation route selected | [Skills review](skills-review.md). Unity CLI/Pipeline configured; third-party bridge suggestion superseded. 3D interaction skill used for reach/comfort heuristics, not physical validation. |
| Source control | Verified complete for initial baseline | Private [repository](https://github.com/alediez2048/math-workbench-airlift); `main` and `unity-airlift` pushed at `367a7f485350b0e36cf88eced5e73d94e5dcf0dc`. 35 initial files. Ignore rules, LFS filters and local hook configured. Working branch: `unity-airlift`. |
| Terminal dependencies | Verified installed | Git 2.50.1, Git LFS 3.7.1, authenticated GitHub CLI, Node 22.23.2. Node 22 is side-by-side at `/opt/homebrew/opt/node@22/bin/node`; default Node and shell configuration unchanged. |
| Unity setup | Editor and scaffold verified | Hub 3.20.1; `6000.6.0f1` Apple Silicon. Universal 3D project created at `unity/`; URP 17.6.0. Provisional 6.6 deviation recorded in plan. Authentication is not a determination of license eligibility. |
| Meta Quest Developer Hub | Download verified; onboarding owner-reported | 6.5.0 ZIP checksum verified. Owner reports authentication, phone pairing, developer team/account verification and Developer Mode complete. App version not independently confirmed. |
| Android toolchain | Verified installed for 6.6 | Android Build Support, SDK/NDK Tools, OpenJDK 17.0.18; bundled ADB 1.0.41 / platform-tools 36.0.0. Owner handled module license/onboarding in Hub. |
| Physical device proof | Basic interaction confirmed; full QA pending | Safe APK built/installed; owner confirms passthrough, controllers and grabbing. Ten-grab, pause/resume, reach and capture checks remain open. |
| Game/backend implementation | Grab and whole/halves chapter owner-reported working on device (Sept 16) | 12/12 onboarding flow tests pass; layout/reference check passes; Android build succeeded with zero errors and ten warnings. September 15 install and launch command succeeded after USB authorization recovery. Fraction drafts remain unwired/untested. No backend or tutor endpoint. See [QA](qa/onboarding.md). |
| Rights, release and submission | Open | Account authentication does not resolve Unity eligibility, asset redistribution, contest rights, or submission consent. No entry submitted and no new paid service authorized. |

## Dated activity log

### September 13 — planning record

- Vetted the Unity execution plan and documented research, assumptions, scope
  cuts, acceptance gates and adversarial review in the dated plan folder.
- Retained earlier numbered documents and `UNITY_PLAN.md` as design history.

### September 14 — documentation and mockups

- Created the four root product documents, skills review and browser-based
  schematic mockups. Verification results are recorded in the mockup folder;
  these are prior recorded results, not new tests run during this log update.
- Mockups demonstrate authored interactions and local support fixtures only;
  they must not be presented as actual headset footage or live inference.

### September 14 — installation and source control

- Installed Git LFS and Unity Hub; left license/account onboarding to the owner.
- Installed Node 22 side-by-side through Homebrew. Homebrew upgraded shared
  dependencies during installation. Untrusted-tap settings were not changed.
- Downloaded the official Meta Quest Developer Hub archive to
  `/Users/jad/Downloads/AirliftSetup/Meta-Quest-Developer-Hub-6.5.0.zip`.
  Verified SHA-256:
  `268e2a63f6c6be3160f6854424eac2b319eb41873a6cfe0ae20904f192f370c4`.
  Revealed the archive in Finder; the owner handled subsequent onboarding.
- Created the private GitHub repository, committed the 35-file baseline, and
  verified matching remote hashes for `main` and `unity-airlift`.
- Owner reported Unity and Meta authentication and completed editor installation.
  A local inspection found `/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app`,
  and confirmed Android modules were absent/unselected. This supersedes earlier
  statements that editor completion and Android installation were merely unknown.
- No duplicate editor installation, automatic downgrade, SDK import, headset
  deployment, or editor-control bridge was performed.

### September 14 — documentation checkpoint

- Added this log and a README entry so progress can be resumed without relying
  on the chat transcript. Formal ticket-tracker setup awaits location confirmation.
- Saved the log in commit `ec4456e` and pushed `unity-airlift` before cloning the
  repository into `/Users/jad/Desktop/math-workbench-airlift` at the owner's
  request. Verified the clone contains that commit and installed its local LFS
  hook. The original Documents checkout is retained, not deleted or moved.
- Updated the active plan's workspace paths and README in the Desktop clone.
  This changes the working location only, not the product or toolchain decisions.

### September 14 — additional download attempt

- Installed Unity's official CLI `1.0.0-beta.9` via its Homebrew cask, following
  [Unity's documentation](https://docs.unity.com/en-us/unity-cli/use-unity-cli).
  No editor-control bridge or project package was configured.
- A dry run resolved the 6.3 stream to `6000.3.24f1`, arm64. Editor plus Android
  and child modules totals 8,794,993,129 download bytes (approximately 8.8 GB).
- Attempted the combined install without `--accept-eula`. It exited with code 6:
  module licenses require acceptance. No agreements were accepted by the agent.
- Started the editor-only installation using
  `unity install 6000.3.24f1 -a arm64 --non-interactive --no-banner`.
  The CLI entered its download phase. This is an in-progress operation, not a
  completed installation or build proof. Existing 6.6 was not removed or changed.
- Android tools are still pending owner license review. After the editor finishes,
  use Hub's module workflow for that editor, or resume the official CLI module
  workflow only after explicit approval of the applicable agreements.

### September 14 — device readiness and implementation kickoff

- Owner completed Android module installation in Hub. Installed modules, bundled
  Java and ADB were checked; earlier Android-blocked entries are historical.
- Owner completed headset setup, phone pairing, Meta developer team creation,
  account verification, Developer Mode and USB approval. Authorized ADB was
  observed; hardware reports **Quest 3S**, not Quest 3. No device ID is stored here.
- Selected the already installed Unity 6000.6.0f1 for the first compatibility
  proof, explicitly noting deviation from candidate 6.3 LTS. No editor removed.
- Created `unity/` using official CLI, Universal 3D, arm64, `--no-cloud`, without
  nested version control. Configured Pipeline 0.7.0-exp.1. A local editor endpoint
  was briefly reachable, but subsequent command discovery found no instance;
  editor was reopened as a separate macOS app instance. It remained running and
  its package-manager log confirmed Core and Interaction downloads. Do not count
  this as a verified automation smoke test or a diagnosed startup failure.
- Owner explicitly authorized accepting the Meta SDK license by importing Core
  and Interaction. Added pinned package requests and the official Meta registry
  to the manifest; resolution and compilation still need verification.
- Cargo theme confirmed. Café/equal-sharing and garden/multiplication-area are
  [paper-only future concepts](future-lessons.md), not additional build commitments.
- Updated active product docs, both agent instruction files, plan amendments,
  skills review and [toolchain checkpoint](qa/toolchain.md). Earlier planning
  documents and dated research remain historical rather than being rewritten.

### September 14 — first custom code and test scene

- Added versioned `CreateDeviceProof.cs` and `ConfigureQuestProof.cs` under
  `unity/AgentScripts/`; these are development tools, not runtime lesson code.
- Generated and saved Meta passthrough, controller interaction rig, stationary
  workbench, 12 cm cube and SDK grab interaction. Used the interaction-design
  skill for reachable placement; actual comfort and reach remain unverified.
- Configured ARM64 IL2CPP/Vulkan, mobile URP, OpenXR/Meta/Touch profiles,
  controllers-only and required passthrough, without camera-frame access.
- Queued the first development APK build through official Unity Pipeline.
  Build completion, installation, grabs and capture are not yet demonstrated.

### September 14 — safe APK and owner-confirmed interaction

- Safe non-development build completed with zero errors and nine warnings in
  197.256 seconds. Credential safeguard execution and absence of the local token
  from unpacked APK contents were verified; exact artifact hash is in device-proof.md.
- Installed through ADB and launched successfully after owner reauthorized USB.
  Owner confirmed seeing the room, tracked controllers and successfully grabbing
  the orange cube. Earlier failed grab attempt was resolved during the same test.
- Initial scene/setup/docs were backed up in `0fa9c80`. Closing checkpoint includes
  the build safeguard, evidence, generated build-state changes and ignore rules.
  SDK-generated test metadata and action-binding artifacts were removed by build
  hooks; keep them ignored rather than treating them as product source.
- Owner approved purposeful cargo storytelling and guided onboarding with replayable
  help; these are the next product work, not features already built.
- Checkpoint closed is setup/basic interaction only; remaining QA is explicit.

### September 14 — onboarding-first implementation

- Owner requested three separate topic cards before entering a lesson, and
  explicitly prioritized context, demonstration and guided practice over jumping
  directly into arithmetic. Cargo Crew is the only available onboarding path;
  café/division and garden/multiplication are disabled Coming soon previews.
- Authored a pure progression model, editable content, Meta-event director,
  scene-generation/check scripts and 12 passing EditMode tests. Imported Unity's
  bundled TMP resources after the first scene-generation attempt found them missing.
- Created Onboarding as a separate scene, preserving DeviceProof. Catalog render
  inspected and all instruction pages fit their text region. Non-development
  build `build_e54958879ec6` succeeded in 75.613 seconds, zero errors and ten
  warnings. APK hash and warnings are in qa/onboarding.md; known local developer
  token scan passed. ADB lists no headset, so install/physical QA remain pending.
- Updated agent instructions, plan amendment, PRD, requirements, constraints,
  stack and future concepts to reflect the approved menu/onboarding scope.
- Changes are not yet committed; previous committed/pushed checkpoint is fd7895f.

### September 15 — connection recovered; onboarding deployed

- Owner reported Developer Mode unexpectedly off, then an enablement error.
  Headset browser internet access and no pending update were owner-confirmed.
  Developer Mode was restored but USB remained unauthorized without a prompt.
- Reconnects, host ADB restart and headset restart did not resolve it. Owner
  updated Developer Hub; its Bluetooth discovery failed despite Mac Bluetooth
  and app Bluetooth permission being enabled. No factory reset or key deletion.
- Owner toggled Developer Mode off/on with USB attached, then saw and approved
  USB debugging. ADB authorization was verified; onboarding APK installation and
  launch command succeeded. Visible UI and interaction acceptance remain pending.
- Changes remain uncommitted pending the requested physical slice check.

### September 16 — grab fixed by owner report; first fraction chapter runs on device

- Claude Code root-caused the Unity test-runner stalls to a dirty-scene Save alert
  and fixed startup placement to wait for a tracked head (four new tests).
- Built and installed cargo-20260916-131831 (SHA-256 18273ac6...2732): 38 checks
  and credential scan passed. Contains a temporary practice input readout and a
  temporary grip-or-trigger practice grab override plus the code-complete
  whole/halves chapter (CargoLessonModel, RulerLayout, CargoLessonDirector).
- Owner initially saw nothing: the app was not running (Quest home shell, no
  crash). Launched over ADB; focus, tracking, passthrough and controllers OK.
- **Owner-reported:** grabbing and manipulating the practice strap works; the
  Cargo Crew halves chapter completes with 1/2 + 1/2 = 1. Grip-vs-trigger
  numbers not reported, so the grip root cause stays open; override retained.
- Owner authorized commit of all work in progress. No push.

## Ongoing update policy

- Update this log after each meaningful milestone, failure or decision; do not
  overwrite dated history. Keep the checkpoint table current.
- Distinguish **verified**, **owner-reported**, **planned**, and **blocked**.
  Record commands/results or artifact links; never count a download as a completed
  installation or a mockup test as headset acceptance.
- Proposed ticket contents: stable ID, workstream, status, owner, dependencies,
  scope, acceptance checklist, dated evidence, blockers and next action.
- Keep requirements in the PRD/requirements and decisions in the authoritative
  plan. Tickets and this log link to them rather than invent competing scope.
- Commit milestone documentation; push when authorized to back it up off-device.
  Keep credentials, raw personal data and signing material out of tickets/logs.

## Next-session handoff

Read this file, `AGENTS.md`, the vetted plan and its research, then check Git
status before acting. Do not reinitialize Git or repeat completed downloads.
Use the installed 6.6 proof candidate and inspect current editor/package state.
Do not block on formal ticket setup or re-download completed Android modules.
Pause for new licenses, account decisions, purchases and physical headset actions.
Complete remaining physical QA before full device-proof signoff. The next product
slice is getting the authored onboarding onto the headset and checking its flow,
then the first fraction interaction. Do not start additional lessons. Preserve
the working proof scene as a regression reference. See qa/onboarding.md.
