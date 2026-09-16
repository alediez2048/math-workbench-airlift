# Fraction expansion — revision evidence, September 15, 2026

Extends this existing RAP run; previous research remains history. Findings below
changed the proposed docs/00-build plan. Read evidence as facts/inferences/unknowns,
not as authorization to implement or deploy.

## Existing project — found

- Manifest pins Unity XR OpenXR 1.18.0, Meta OpenXR 2.6.1, Meta Core/Interaction/OVR
  205.0.0, URP 17.6.0 and Unity Test Framework 1.8.0. Source:
  unity/Packages/manifest.json. Confidence H.
- FractionValue.cs:13–26 normalizes exact values; FractionNotation.cs:3–17 retains
  unreduced notation. PieceState supports unit 1/2/4/8 denominators; PlacementState.cs:7–54
  rejects duplicates, overflow and wrong WholeId. HalfLessonModel.cs:34–39 restricts
  placement to its current partition, not a full quarter/merge curriculum. Confidence H.
- Only OnboardingFlowTests currently exist. OnboardingDirector has generation
  guarding but no guide/audio or fraction scene integration. No verify-cargo.sh
  or CargoBuild entry point exists. EditorBuildSettings remains a risk for wrong-scene
  builds. Sources: unity/Assets/Airlift/Scripts and Tests; ProjectSettings/EditorBuildSettings.asset.
  Confidence H. Drafting new tests is not a report of passing tests.
- Owner reports actual catalog, mission, grab/place and replay on Quest after
  installation. This is specific acceptance, not all-hand/comfort/performance QA.
  Source: September 15 conversation and docs/qa/onboarding.md. Confidence H as
  owner report; no new physical test performed in this planning turn.

## SDK interaction facts — found in installed 205 sources

- OVRInput.cs:1448–1482 uses any-bit matching for masked Get; face button mapping
  at :52–65 distinguishes A/B, X/Y and Menu. ControllerButtonsMapper.cs:68–117,171–205
  exposes Down/Up/Held, not a chord primitive. Therefore separately query both
  buttons and edge-detect a chord. Confidence H.
- Grabbable.cs:30–69 supports two grab points; :249–278,314–354 manages selection
  end/restart and final release movement. ITransformer.cs:27–35 belongs to one
  grabbable. Two-hand grab of one object is not two-piece merge. Confidence H.
- GrabFreeTransformer.cs:27–76,139–191 supports constrained transforms/scaling;
  spacing-driven scale is continuous. Semantic fractions must not come from scale.
  RayInteractorRayVisual.cs:25–60,96–127 and HapticsRuntime.cs:54–100,115–129 provide
  reusable pointer/haptic behavior. Confidence H.
- Source root: unity/Library/PackageCache/com.meta.xr.sdk.core@*/ and
  com.meta.xr.sdk.interaction@*/ (generated cache, do not commit).
  File references describe inspected installed package source, not invented public APIs.
- Inference: exact authored parent/child swaps plus released-selection guards
  are lower risk than arbitrary mesh slicing. Comfort of A+B/X+Y, two-piece join
  and zoom remains unknown until physical acceptance.

## Learning and multimodal guidance

- [IES fractions practice guide](https://ies.ed.gov/ncee/wwc/docs/practiceguide/fractions_pg_093010.pdf),
  pp20–21, supports equal-sharing and repeated-halving examples, later broader
  fractions. [IES instructional tips](https://ies.ed.gov/ncee/wwc/Docs/referenceresources/50629_Instructional_Tips_Fractions_060721.pdf),
  pp4–6, supports an unchanged original whole and number-line relationships.
  Confidence H. Inference: preserve a ghost whole while partitioning and distinguish
  count of pieces from part size. This does not establish VR efficacy.
- [Grade 4 standards](https://www.thecorestandards.org/Math/Content/4/NF/) require
  generating/explaining equivalents and comparisons referring to the same whole.
  Confidence H. Require construction and model-based reason, not passive overlay.
- [CAST UDL 2.2](https://udlguidelines.cast.org/static/udlg2.2-text-a11y.pdf) supports
  multiple representations/action access. Confidence H. Captions and single-controller
  fallback are design applications, not proof of accessibility certification.
- [Unity AudioSource](https://docs.unity3d.com/6000.0/ScriptReference/AudioSource.html)
  provides playback/stop. Confidence H. A single stopped/replaced narration channel
  avoids overlapping instructions; avoid concurrent PlayOneShot narration. No network
  is required for imported licensed clips.

## ElevenLabs — material policy and integration gates

- [Use policy §3(r)](https://elevenlabs.io/use-policy) restricts making services
  available to under-13s and bundled solutions targeting them. [Privacy policy,
  Children](https://elevenlabs.io/privacy-policy) prohibits submitting under-18 voice
  data. Confidence H on text. **Inference M:** an age-10 lesson using generated
  recordings may also be affected; written permission/qualified review is required.
  Adult testing, payment or parental consent does not itself resolve that inference.
- [Publishing rights FAQ](https://help.elevenlabs.io/hc/en-us/articles/13313564601361-Can-I-publish-the-content-I-generate-on-the-platform)
  distinguishes noncommercial free-tier output and qualifying paid-tier commercial
  rights, with Beta exceptions. Confidence H. These rights do not override use policy.
  Record generation plan/date/source/license only if generation is authorized.
- [Libraries](https://elevenlabs.io/docs/eleven-api/resources/libraries) distinguishes
  official clients from community Unity/.NET work. Confidence H. No claim of an
  officially supported Unity Agents integration follows.
- [Agents WebSocket API](https://elevenlabs.io/docs/eleven-agents/api-reference/eleven-agents/websocket)
  and [authentication](https://elevenlabs.io/docs/eleven-agents/customization/authentication)
  support private-agent signed URLs; documented initiation window 15 minutes,
  not an established-session lifetime cap. Confidence H. Keep API key server-only;
  any stricter one-use/session lifetime policy must be enforced by our backend.
- [Agents terms](https://elevenlabs.io/agents-terms) and
  [disclosures](https://elevenlabs.io/docs/eleven-agents/legal/disclosure-requirement)
  impose user notice/consent responsibilities. [Privacy controls](https://elevenlabs.io/docs/eleven-agents/customization/privacy)
  and [Zero Retention](https://elevenlabs.io/docs/eleven-api/resources/zero-retention-mode)
  are distinct; configuration is not blanket no-retention assurance. Confidence H.
  No child voice capture is authorized; no-go is an acceptable gate outcome.

## Methodology and commands

Found: LabelCheck template includes copy-paste primer, actual starting-state fields,
scope/dependencies, acceptance, implementation, paths, config, tests, references,
gotchas, code/ticket Done, expected output, dependencies and Why mirrored to DEV-LOG.
Source /Users/jad/Desktop/LabelCheck/docs/00-build/TICKET-TEMPLATE.md. Confidence H.
Adapt time budgets to scope sizes, TypeScript/web commands to Unity, and automatic
merge to owner-reviewed checkpoints. User specifically wants local iterative tickets.

Found: installed unity --help, test --help, command --help, build --help expose
EditMode/PlayMode, filtering and explicit execute-method build. Confidence H.
Unknown: proposed wrapper and build class have not been implemented or run.
CC-P1-01 must prove them; editor-lock handling is required, not an assumed passing CLI.

## Remaining unknowns with designed resolution

- Exact age eligibility and provider permission: CC-P4-01, applies before any
  ElevenLabs audio acquisition in P1-03 too; adult-recording fallback is default.
- Input comfort, reach, narration pacing: CC-P1-10 and CC-P2-08 device reviews.
- Live WebSocket microphone/audio lifecycle on Quest: P4-02 only after gates.
- Complete lesson CPU/GPU/resource behavior: P3-04; no performance result yet.
- Owner approval of narrated-first/resizing defaults: review PLAN and PRD before work.
- Deadline feasibility: scope checkpoint, not an invented 28-ticket delivery promise.

## September 15 — AI guide and style revision evidence

- **Finding:** OpenAI supports voice APIs; consumer ChatGPT embedding is not the plan.
  **Sources:** https://developers.openai.com/api/docs/guides/realtime and
  https://developers.openai.com/api/docs/guides/text-to-speech (opened).
  **Gotcha:** Unity integration/account access not verified; live model selection
  followed by approved text/speech is the proposed baseline, not unconstrained audio.
  **Confidence:** H capability, M/unknown implementation.
- **Finding:** Minor safeguards and retention require explicit review.
  **Sources:** https://developers.openai.com/api/docs/guides/safety-checks/under-18-api-guidance
  and https://developers.openai.com/api/docs/guides/your-data (opened).
  **Gotcha:** no verified ZDR approval or child-release clearance. Mic-off does not
  automatically make server telemetry anonymous. **Confidence:** H policy, unknown account.
- **Finding:** Meta guidance favors comfortable spatial placement, physical metaphors
  and real-headset iteration. **Sources:** https://developers.meta.com/horizon/design/mr-design-guideline/
  and https://developers.meta.com/horizon/design/panels/ (opened).
  **Gotcha:** STYLE-GUIDE dimensions/animation values are design hypotheses, not certifications.
  **Confidence:** H source, assumed project tuning.
- **Finding:** Current demo uses default TMP font lookup.
  **Source:** unity/AgentScripts/CreateOnboarding.cs:24–25.
  **Font candidate source:** https://github.com/google/fonts/blob/main/ofl/nunitosans/DESCRIPTION.en_us.html (opened).
  **Gotcha:** no fonts downloaded; compare actual licensed weights before selection.
  Installed Unity TextMeshPro FontAssets.md and FontAssetsFallback.md were read for
  SDF/fallback guidance. **Confidence:** H current code/docs, pending visual approval.
