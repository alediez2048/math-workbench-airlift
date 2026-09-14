# Math Workbench: Airlift

Unity / Meta Quest 3S educational game · implementation kickoff · September 14, 2026.

The repository contains documentation, schematic mockups, and a newly created
Universal 3D project in `unity/`. A native test app is installed on Quest 3S;
the owner confirmed passthrough, controller tracking, and grabbing the cube.
It is **not yet a learning game**: onboarding, storytelling and fraction logic
remain to build. See the [milestone evidence and remaining QA](docs/qa/device-proof.md).

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

Review the PRD and visual reference, then implement Phase 1 of the vetted plan:
prove a native passthrough scene and grabbable object on the physical Quest before
production art or stretch content. Account/licensing, USB authorization, physical
comfort, and submission rights need the owner's participation.

Unity 6000.6.0f1 and its Android tools are installed, and the Quest 3S has an
authorized ADB connection. Unity's official Pipeline package is configured;
Meta XR import is complete; the first proof scene is saved and an Android test
build succeeded and was installed after the credential safeguard was verified.
See [toolchain evidence](docs/qa/toolchain.md).
No cloud project, paid service, or contest submission has been created by this kickoff.
