# Math Workbench: Airlift

September 15 planning checkpoint: [Cargo Crew review package](docs/00-build/README.md)
contains the proposed narrated-fractions PRD and local phased tickets. **Implementation
is paused for owner review.** This draft does not silently replace the approved
baseline below. New progress is in [DEV-LOG](docs/00-build/DEV-LOG.md).

Unity / Meta Quest 3S educational game · implementation kickoff · September 14, 2026.

The repository contains documentation, schematic mockups, and a newly created
Universal 3D project in `unity/`. A native test app is installed on Quest 3S;
the owner confirmed passthrough, controller tracking, and grabbing the cube.
It is **not yet a complete learning game**. The new onboarding scene includes a
three-card catalog, cargo briefing, demonstration and grab/place practice;
arithmetic is not connected yet. See [onboarding status](docs/qa/onboarding.md)
and the [earlier device checkpoint](docs/qa/device-proof.md).

## Current documents

For the latest setup status, completed work, blockers, and next-session handoff,
start with the [progress and R&D log](docs/PROGRESS.md).

| Document | Purpose |
|---|---|
| [PRD](prd.md) | Product promise, audience assumptions, learner journey, user stories, pedagogy, and testing decisions |
| [Requirements](requirements.md) | Testable baseline/conditional requirements and definition of done |
| [Constraints](constraints.md) | Scope, deadlines, budget/authority boundaries, unresolved gates |
| [Tech stack](techstack.md) | Reuse versus custom work, architecture, candidate versions, code/manual ownership |
| [Interactive mockups](docs/mockups/index.html) | Five schematic game states and interactive error/repair references |
| [Skills review](docs/skills-review.md) | Installed skill fit, repository findings, cautions, optional editor bridge |

The [vetted execution plan](docs/plans/2026-09-13-unity-vet/plan.md) and its
[research](docs/plans/2026-09-13-unity-vet/research.md) remain authoritative.
The new documents are derived views, not a second competing plan. If a decision
changes, update that plan and the affected requirement/document together.

`UNITY_PLAN.md`, `UNITY_REVIEW_BRIEF.md`, the numbered Markdown files, and the
older review brief are retained history. They are not the current build contract.

## Start here

Active local checkout: `/Users/jad/Desktop/math-workbench-airlift`, branch
`unity-airlift`. Make future edits here; the earlier Documents checkout is retained
as an inactive copy. See the [source-control guide](docs/source-control.md).

The initial cube proof is complete. Next install and test the onboarding APK
using the checks in `docs/qa/onboarding.md`; do not redo the completed toolchain
setup. Account/licensing, USB authorization, physical
comfort, and submission rights need the owner's participation.

Unity 6000.6.0f1 and its Android tools are installed, and the Quest 3S has an
authorized ADB connection. Unity's official Pipeline package is configured;
Meta XR import is complete; the first proof scene is saved and an Android test
build succeeded and was installed after the credential safeguard was verified.
See [toolchain evidence](docs/qa/toolchain.md).
No cloud project, paid service, or contest submission has been created by this kickoff.
