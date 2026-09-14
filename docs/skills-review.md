# Skills and development-tool review

September 14, 2026 · recommendation only; no new installs or integrations performed.

**Implementation update:** the review below is historical. Unity's official
CLI 1.0.0-beta.9 and Pipeline 0.7.0-exp.1 are now configured. Use that route
instead of the proposed third-party MCP trial. The 3D interaction-design skill
is being used for reach and comfort heuristics; physical validation is pending.
No new skill framework was installed. See [toolchain](qa/toolchain.md).

## Recommendation

Keep a small set: **3d-interaction-design** for spatial interaction review, **RAP**
for material plan changes, and **TDD** for math/state/tutor contracts during the
build. The existing vetted plan already supplies the main execution discipline.
Use official Unity/Meta documentation for actual APIs. Do not introduce a second
engine or an agent orchestration system to make this five-day build easier.

A **skill** supplies instructions. An **SDK** supplies reusable game/runtime code.
An **editor bridge** supplies tools that can operate Unity. A **test/build harness**
makes verification repeatable. These are complementary, not interchangeable.

## Installed and visible in this session

| Skill | Fit / recommended use | Limitation or adjustment |
|---|---|---|
| `3d-interaction-design` | Use now and during physical interaction review: constrained manipulation, reach, visible selection, multimodal feedback. | Its generic 90 Hz minimum is not our release contract; retain the vetted 72 Hz engineering target and measure on Quest. Generic target sizes are starting heuristics, not child-accessibility validation. |
| `rap` | Use when a material decision reopens; useful adversarial review. | Already used to vet this plan. Do not repeat broad planning instead of device work. |
| `tdd` | Use during implementation for fractions, placement, progression, timeout races, and schema boundaries; one behavioral slice at a time. | Repo workflow setup is not configured. Respect its setup step when adopting it; C# test selection still needs Unity-specific judgment. |
| `to-prd` | Its problem/story/decision/test structure informed the local PRD. | User requested a file, so no issue publication or tracker configuration was performed. |
| `sceneview-web` | Keep for a separate browser-based project. | Filament/Kotlin-JS/WebXR is not Unity. It does not furnish Meta Unity components or editor automation. |
| `aframe-webxr` | Keep for a future web prototype only. | HTML/A-Frame would be another implementation surface; not used for these schematic mockups or native APK. |
| `spatial-developer` | General inspiration only. | Local examples focus on WebXR/React and SwiftUI/RealityKit. Do not paste those APIs into a Unity task. |

Local check confirmed the three newly added relevant entries are symlinks to the
SceneView, freshtechbro, and Tibsfox checkouts. `spatial-developer` is from a
different repository (`daffy0208/ai-dev-standards`). A checkout containing many
skills does not mean every skill is installed or active. No callable Unity editor
tool appeared in the tool inventory checked for this review.

## The three repositories you supplied

### SceneView

[SceneView](https://github.com/sceneview/sceneview) provides Android/Compose,
Apple/SwiftUI, and web rendering paths, plus AI-facing documentation and tools.
Its repository describes the web platform as alpha. Useful technology, but not a
Unity XR plugin. **Decision: defer for this build.** Importing it would not reduce
our C# lesson work and would add a second rendering/toolchain path.

The installed `sceneview-web` skill also warns that the web haptic helper wraps
the browser Vibration API. It is not a substitute for Meta controller haptics.
Repo Apache-2.0 licensing does not eliminate asset or dependency license checks.
[Skill source](https://github.com/sceneview/sceneview/blob/main/agents/sceneview-web/SKILL.md).

### freshtechbro/claudedesignskills

The [catalog](https://github.com/freshtechbro/claudedesignskills) is primarily web
3D/animation: Three.js, A-Frame, React, and related authoring workflows.
**Decision: no new runtime skills from this collection for the baseline.**

`blender-web-pipeline` is a selective later reference for reducing decorative
asset complexity. Its actual pipeline targets glTF/web delivery, not a verified
Unity/URP import path. Do not inherit its generic polygon budgets or batch export
scripts without checking the installed Blender/Unity versions. Use Unity
primitives first; consider this only if aircraft art becomes an accepted bottleneck.
[Skill](https://github.com/freshtechbro/claudedesignskills/blob/main/.claude/skills/blender-web-pipeline/SKILL.md).

The checked root license is MIT; imported packages, models, and subfolders can
carry separate terms. Do not install all bundles for a single asset task.

### Tibsfox/gsd-skill-creator

[GSD Skill Creator](https://github.com/Tibsfox/gsd-skill-creator) is a broad skill
creation/coordination system, not a Quest interaction SDK. **Decision: do not
adopt its orchestration stack during the hackathon.**

Two additional reference candidates exist in its spatial-computing examples:

- [`immersive-environment-design`](https://github.com/Tibsfox/gsd-skill-creator/blob/main/examples/skills/spatial-computing/immersive-environment-design/SKILL.md): useful for scale, focal hierarchy, calm onboarding, and readable scenery. Candidate only, not installed here as a skill.
- [`embodied-computing-and-constructionism`](https://github.com/Tibsfox/gsd-skill-creator/blob/main/examples/skills/spatial-computing/embodied-computing-and-constructionism/SKILL.md): useful error-repair/artifact prompts, but **do not adopt its claim that an artifact makes a final test unnecessary or demonstrates mastery**. Our near-transfer task and cautious claims remain authoritative.

The [root license](https://github.com/Tibsfox/gsd-skill-creator/blob/main/LICENSE)
is BSL 1.1, with no Additional Use Grant and a change to GPL-3.0 dated 2030-03-11.
Do not assume permissive redistribution. No separate license was found inside
the inspected spatial-computing examples directory. Before copying text/code or
bundling it, establish the exact applicable terms and compatibility. Merely
consulting general design ideas is distinct from incorporating repository material;
this review is not a legal determination. No repository material was copied into
the game's runtime or these mockup assets.

## More useful missing capability: Unity editor access

The third-party [MCP for Unity](https://github.com/CoplayDev/unity-mcp) documents
scene/asset operations, script editing, tests, profiling, and builds, with Unity
2021.3–6.x support and an MIT license. It is an editor bridge, not a skill, and is
not affiliated with Unity Technologies.

**Conditional recommendation:** after the first physical APK proof, allow a
30-minute trial of a pinned, reviewed release. Pass only if we can inspect the
correct project, add/remove a disposable cube, read compilation errors, and run
a test. Keep the connection local, inspect permissions, and exclude development
tooling from runtime where appropriate. Failure means returning to batch Editor
utilities and a short Inspector checklist—not spending the day repairing a bridge.

No install command was executed and no working connection is claimed.

## Suggested use by phase

1. Device proof: official Unity/Meta setup docs; no new skill framework.
2. Math/state: TDD after its repo setup is agreed; preserve the current domain contract.
3. Lesson: 3D interaction review, then physical Quest evidence; optional environment reference.
4. Tutor: deterministic fixtures, local-router comparison, timeout/race tests.
5. Polish: selective asset guidance only after release-path gates hold.
6. Submission: inspect real footage, licenses, and disclosures; no mockup-as-product claims.

## Provenance and limits

Reviewed local skill files, repository catalogs/licenses, and upstream editor-bridge
documentation. This is a focused applicability audit, not an exhaustive security
or code audit. Repository documentation is a claim until tested locally.

Local checkout revisions inspected:

| Repository | Local commit |
|---|---|
| SceneView | `00b6182d6f864e0d71d6c63a4122ed019a1eb29d` |
| claudedesignskills | `1da73febff0c3e1dfefc07f8a5ef8f7d1dfdb6cd` |
| gsd-skill-creator | `e179dfe911f3b3c2ff8bcd90ecd6ee4738648d17` |

Do not upgrade installed skills or copy their scripts just because upstream has
changed. Review the exact version when a concrete build task needs it.
