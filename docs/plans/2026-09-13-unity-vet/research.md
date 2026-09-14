# RAP research — Unity plan vetting

As of September 13, 2026. Existing design: `../../../UNITY_PLAN.md`.
Research is limited to claims that could change implementation or submission.
Findings are recorded with source, gotcha, and confidence (H/M/L).

## Local drift and learning design — lead

- **Found:** This is a documentation-only directory, not a Git repository. No
  Unity project, installed editor at normal locations, or ADB on PATH was found.
  Source: local `rg --files`, `git rev-parse`, application-directory checks,
  `command -v adb`, September 13. Confidence H for checked locations; installation
  elsewhere and developer-account state remain unverified. A setup estimate is
  not proof of readiness.
- **Found:** `UNITY_PLAN.md:343` invents `endpointChecks` and `pieceCountChecks`
  without an observable interaction. `:299` infers a reason from a wrong answer.
  Confidence H. **Inference:** controller events cannot reveal silent counting
  or reasoning; identical answers can have different causes. Fix: explicit
  compare/overlay/reason events, evidence IDs, and `insufficient_evidence`.
- **Found:** `UNITY_PLAN.md:193` preserves errors while `:260` restores invalid
  releases. Confidence H. Fix: distinguish unsafe/out-of-bounds release from a
  geometrically valid but mathematically wrong arrangement; evaluate on Submit.
- **Found:** `UNITY_PLAN.md:414` says player-relative; no placement lifecycle is
  defined. Confidence H. Fix: initialize from head pose once, then world-lock;
  recenter only explicitly while all objects are released.
- **Found:** `UNITY_PLAN.md:657` can ship P1 math defects on Friday. Confidence H.
  Fix: no unresolved P0/P1 in a submitted enabled path, on any day.
- **Found:** `UNITY_PLAN.md:510` mixes code/editor hours but `:517` supplies exact
  percentages with no measurement. Confidence H. Treat the old allocation as a
  rough planning guess; assign owner/action per phase instead of false precision.

## Unity infrastructure — researcher, verified September 13

- **Finding:** Unity lists 6.3 as the current LTS, supported through December
  2027. Meta's minimum is 6000.0.66f2; OpenXR is the supported direction and the
  Oculus XR Plugin is deprecated. Use a 6.3 LTS patch, then freeze the proven
  editor/package tuple. **Sources:**
  https://unity.com/releases/unity-6/support ;
  https://developers.meta.com/horizon/documentation/unity/unity-project-setup/ ;
  https://developers.meta.com/horizon/documentation/unity/unity-and-openxr-compatibility/
  **Gotcha:** minimum requirements are not a tested matrix for this machine.
  **Confidence:** H documentation; unverified local compatibility.
- **Finding:** Meta's standalone simulator supports ARM Macs; Horizon Link is
  Windows-only. Build/deploy documentation supports ARM macOS, while an older
  paragraph on the requirements page still says x86 only. **Sources:**
  https://developers.meta.com/horizon/documentation/unity/unity-simulate-xrsim/ ;
  https://developers.meta.com/horizon/documentation/unity/unity-env-device-setup/ ;
  https://developers.meta.com/horizon/documentation/unity/unity-development-requirements/
  **Gotcha:** use physical APK evidence to settle this documentation conflict;
  do not depend on headset Play Mode through Link. **Confidence:** H.
- **Finding:** `SnapInteractor`/`SnapInteractable` provide detection and movement
  to poses. Meta still labels Snap experimental. **Sources:**
  https://developers.meta.com/horizon/documentation/unity/unity-isdk-snap-interaction/ ;
  https://developers.meta.com/horizon/documentation/unity/unity-isdk-create-snap-interactions/
  **Gotcha:** test occupied slots, reset, regrab, and contention before reuse;
  retain a simple release-to-slot adapter fallback. **Confidence:** H.
- **Finding:** Compositor passthrough and raw camera frames are distinct.
  `HEADSET_CAMERA` permission is for raw camera access, which this game does not
  use. **Sources:**
  https://developers.meta.com/horizon/documentation/unity/unity-pca-overview/ ;
  https://developers.meta.com/horizon/documentation/unity/unity-passthrough-gs/
  **Gotcha:** test readiness/unavailability and resume, not an invented ordinary
  passthrough permission dialog. **Confidence:** H.
- **Finding:** Building Blocks supply rigs and common interactions. The supplied
  Interactions Rig includes locomotion capabilities that must be disabled here.
  **Source:** https://developers.meta.com/horizon/documentation/unity/unity-building-blocks-overview/
  **Gotcha:** do not add duplicate rigs when blocks add dependencies. **Confidence:** H.

## Contest and reuse — researcher, verified September 13

- **Finding:** Deadline September 18, 11:59 PM Central; judging September 21–23.
  Working experience, description, video up to three minutes, and disclosures are
  required. Repository/live deployment are optional; judges need not test. No
  published weighted rubric. Preserve unrestricted evaluation access and freeze
  the entry after closing. **Source:** https://hackathon.nerdy.com/terms §§1,4–6.
  **Confidence:** H. Native APK compatibility is an inference, M.
- **Finding:** Submission assigns original entry rights broadly; license-back is
  noncommercial. Third-party material has a carve-out, disclosure duties, and
  governing-license constraints. Named reciprocal licenses are excluded.
  Entrant-only footage and synthetic traces avoid relevant data/depiction issues.
  **Source:** https://hackathon.nerdy.com/terms §7. **Confidence:** H text, M application.
- **Finding:** Unity permits runtime distribution within a project subject to
  its terms; Meta grants specified SDK/application rights, not ownership of SDK
  IP. **Sources:** https://unity.com/legal/editor-terms-of-service/software ;
  https://developers.meta.com/horizon/licenses/oculussdk/
  **Gotcha:** inspect exact package licenses and notices; publish owned code and
  manifests, not proprietary caches. Verify Unity tier eligibility and runtime
  notices/recipient agreement before distribution. No blanket legal clearance
  follows from a framework being commonly used. **Confidence:** H texts; M
  applicability to the uninstalled package versions and contest.

## Standards and evidence — researcher and lead

- **Finding:** 4.NF.A.1 includes generating equivalents and explaining the
  change in part count/size. 4.NF.A.2 includes equal wholes, inequality notation,
  and justification. **Source:** https://www.thecorestandards.org/Math/Content/4/NF/
  **Gotcha:** overlay demonstration alone only partially supports these goals.
  Add a generated equivalent and recorded inequality with model-based
  justification. **Confidence:** H.
- **Finding:** Grade 3 comparison is narrower than Grade 4 comparison;
  `3/4` versus `5/8` belongs in the latter. **Source:**
  https://www.thecorestandards.org/Math/Content/3/NF/
  **Gotcha:** one six-step lesson is not six independent assessments. **Confidence:** H.
- **Finding:** IES recommends systematic instruction, mathematical language,
  representations, number lines, word problems, and fluency activities.
  **Source:** https://ies.ed.gov/ncee/wwc/PracticeGuide/26
  **Gotcha:** the guide is not evidence of this VR product's effectiveness.
  Timer omission is a design choice for these conceptual tasks, not a universal
  research conclusion. **Confidence:** H.
- **Inference:** existing transfer is near transfer, not broad transfer or
  retention. New values with faded support provide a more useful session signal.
  Source: task sequence `UNITY_PLAN.md:265–291`. **Confidence:** H design analysis.

## AI and runnable verification — lead

- **Finding:** Haiku `claude-haiku-4-5-20251001` remains listed; structured JSON
  uses `output_config.format`. **Sources:**
  https://platform.claude.com/docs/en/models/overview ;
  https://platform.claude.com/docs/en/build-with-claude/structured-outputs
  **Gotcha:** schema validity does not prove reasoning accuracy, latency, or
  non-refusal. Verify model access with one smoke call; validate again locally.
  **Confidence:** H docs; account access unknown.
- **Finding:** Vercel WAF offers one fixed-window rate-limit rule on Hobby;
  counters are regional. **Source:**
  https://vercel.com/docs/vercel-firewall/vercel-waf/rate-limiting
  **Gotcha:** IP limiting is abuse reduction, not a global spend cap or identity.
  Use a server kill switch, provider spend controls, and bounded payload/output.
  **Confidence:** H.
- **Finding:** Unity supports command-line test runs and `BuildPipeline.BuildPlayer`.
  **Sources:** https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/reference-command-line.html ;
  https://docs.unity3d.com/6000.0/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html
  **Gotcha:** commands require a licensed installed editor, configured project,
  named build method, and the editor process not holding the same project open.
  **Confidence:** H. Planned methods/scripts do not yet exist.

## Unknowns closed by explicit gates

Account verification, Unity license eligibility, exact installed package tuple,
USB authorization, actual Quest performance, live model access, and judges'
willingness to sideload are not established. Phase 1 resolves device/licensing,
Phase 4 resolves model access, Phase 6 records release metrics and delivery. No
further broad research is needed before those concrete checks.
