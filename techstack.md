# Technology stack — Math Workbench: Airlift

Draft v0.1 · September 14, 2026 · choices are planned, not installed/validated.
The [vetted execution plan](docs/plans/2026-09-13-unity-vet/plan.md) owns exact phase
order and contracts. This document explains what to reuse and what to build.

## Selected stack

| Layer | Selection | Version / verification policy |
|---|---|---|
| Engine | Unity 6.3 LTS, Apple Silicon editor | Select stable patch in Hub; prove on device, then freeze. |
| Rendering | Universal 3D / URP | Use editor-compatible version; simple opaque materials, limited effects. |
| XR platform | Unity OpenXR + Unity OpenXR: Meta | Pin compatible package tuple; no deprecated Oculus XR Plugin. |
| XR interactions | Meta XR All-in-One/Core/Interaction and Building Blocks | Reuse rig, passthrough, controllers, grab, ray, haptics; no duplicate rigs. |
| Snapping | Meta Snap behind a thin adapter | Experimental: trial contention/reset first; fallback to existing grab-release events and nearest valid slot. |
| Application | C#, task assets, prefabs, world-space UI | Small deterministic lesson model; avoid a general-purpose game framework. |
| Target | Standalone Android Quest 3 APK | ARM64, IL2CPP; editor-bundled SDK/NDK/OpenJDK. Record proven graphics API. |
| Tutor transport | HTTPS POST via Unity client → small Vercel proxy | One request in flight; no provider secret in Unity. |
| Model | Claude Haiku 4.5, `claude-haiku-4-5-20251001` | Candidate; verify account access, freeze model/prompt configuration. |
| Proxy | Node 22, built-in fetch/test, pinned Ajv 8 | Fixed request/response schema; no agent orchestration library. |
| Unity JSON | Compatible `com.unity.nuget.newtonsoft-json` | Explicit validators matching shared schema fixtures. |
| Testing | Unity Test Framework EditMode/PlayMode + Node tests + physical Quest QA | Each layer proves different things; simulation never passes physical acceptance. |
| Build harness | Thin Editor build method, shell wrapper, bundled ADB | Planned deliverables; not present/runnable yet. |

Source checks: [Unity LTS](https://unity.com/releases/unity-6/support),
[Meta OpenXR compatibility](https://developers.meta.com/horizon/documentation/unity/unity-and-openxr-compatibility/),
[Building Blocks](https://developers.meta.com/horizon/documentation/unity/unity-building-blocks-overview/),
[Meta Snap](https://developers.meta.com/horizon/documentation/unity/unity-isdk-snap-interaction/).
Documentation support is not a tested local matrix. Record the actual tuple in
`docs/qa/toolchain.md` and preserve Unity's manifest, package lock, and project version.

## Reuse versus custom work

| Reuse the existing system | Write only the product-specific layer |
|---|---|
| Tracking, input, hand/controller pose, grab ownership | What quantity a piece represents and which lane interval it occupies |
| Ray interaction and standard world-space controls | Partition choices, equation fields, explicit Submit, lesson progression |
| Passthrough compositor and XR rig | Initial placement, safe unavailable state, explicit recenter policy |
| Standard materials, meshes, transforms, audio/haptic APIs | Equal fraction geometry, synchronized labels, short cargo payoff |
| Unity testing/build APIs and ADB | Repeatable project commands and concise evidence records |
| Hosted inference API and HTTPS hosting | Evidence validation, allowlisted scaffold selection, local fallback |

This does **not** reinvent VR infrastructure. The custom work is the educational
game: correct quantities, deliberate error states, pedagogy, state recovery, and
the link between a student's action and a visible explanation.

## Minimal architecture

```text
Meta grab / ray events
        ↓
PlacementState + ProposedExpression → SubmittedAnswer (immutable)
        ↓                                  ↓
RepresentationPresenter            Deterministic evaluator
(pieces, line, notation)             ├─ correct → next task / payoff
                                    └─ wrong → support → repair
                                                ↑
                          LocalScaffoldRouter ← TutorResponseGate
                                                ↑
                             validated HTTPS proxy → model
```

- **Math:** `FractionValue` normalizes quantities; `FractionNotation` preserves
  authored forms such as 6/8. Never use floats as the source of fraction truth.
- **Placement:** eight atomic cells per unit lane; halves occupy four, fourths
  two, eighths one. Pack from zero after committed edits; preserve whole ID,
  quantity, and piece count. Geometric validity is not answer correctness.
- **Lessons:** `LessonDirector` owns progression; task assets define target,
  allowed actions, representations, and authored prompts. Representation phase
  and difficulty remain separate.
- **Presentation:** derive pieces, labels, and endpoints from canonical state;
  proposed learner statements cannot rewrite it.
- **Support:** event log, immutable submitted snapshot, local router, client,
  response gate, and scaffold definitions. Reset/recenter invalidates requests.

## Tutor boundary

Eligible trigger: incorrect explicit Submit at T04/T05, once per attempt and at
most four calls per session. Correct submissions never call the model.

Request: schema version, ephemeral request/attempt/task IDs, deterministic verdict,
submitted quantity/expression/comparison/reason, up to 24 trailing plus six retained
evidence events, and allowed scaffold IDs. Entire body ≤8 KiB. No free text or raw
controller streams. Summaries must reference included evidence.

Response: only `hypothesis`, `evidenceIds`, `scaffoldId`, `promptId`. Validate both
shape and whether the cited events support that route on server and client. Never
render model prose. Scaffold choices are `equal_parts`, `common_endpoint`, and
`benchmark_half`; the exact hypothesis/evidence pairs live in the plan.

Five-second client deadline; local fallback applies once; late or stale answers
are ignored. Fixed prompt/model, ≤256 output tokens, no tools/retries; WAF rule
10 requests/IP/60 seconds, owner-set provider spending controls, `TUTOR_ENABLED`
kill switch. Keep `ANTHROPIC_API_KEY` server-side; the app receives only `TUTOR_URL`.

## Planned layout and verification

```text
unity/Assets/Airlift/   Scenes, Tasks, Prefabs, Scripts, Tests, Editor
backend/              api, validation/routing, tests, smoke script
contracts/            tutor.schema.json and shared valid/invalid fixtures
tools/verify.sh        preflight | editmode | playmode | build-quest | device | release
docs/qa/              toolchain, device proof, lesson, tutor evaluation, release
docs/submission/      description, rights, notices, install, video, artifact manifest
artifacts/            ignored local test/build/capture outputs
docs/mockups/         standalone design reference; never bundled as game UI
```

The future wrappers must fail on missing modules, failed builds, zero tests, or
failed tests. APK build output is `artifacts/airlift.apk`; package ID is
`com.jad.airlift`. Test math/state without scene or network dependencies; then
test Unity flow and actual controller behavior separately.

## Optional development accelerator, not a runtime dependency

[MCP for Unity](https://github.com/CoplayDev/unity-mcp) is a third-party MIT editor
bridge worth a bounded trial **after device proof**, not a requirement. It documents
scene/asset editing, tests, builds, and Codex support. No active Unity tool was found
in this session. A proposed 30-minute trial must demonstrate scene inspection,
one disposable object edit, console reading, and a test run before adoption;
pin a reviewed release and keep it local. Otherwise use the planned Editor utilities.
Do not install/configure it as part of this documentation task.

SceneView, A-Frame, Three.js, Lens Studio, and Snapchat are not additions to this
stack. They would create a different runtime/delivery path. See the
[skills review](docs/skills-review.md) for selective reuse of design guidance.
