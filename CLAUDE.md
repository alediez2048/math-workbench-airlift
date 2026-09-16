# Math Workbench: Airlift — instructions for Claude

## September 16 afternoon: owner-reported grab + halves loop working; committed

Owner reported on the installed build 18273ac6: practice grab and manipulation works, and
the Cargo Crew halves chapter runs end to end (1/2 + 1/2 = 1 accepted). This is the first
owner-reported working arithmetic slice. Not yet known: whether grip or trigger produced
the grab (the temporary grip-or-trigger override and input readout are still in the build),
so the grip defect is mitigated, not root-caused. Owner authorized the commit of all
current work; no push authorized. Next: remove temporaries only after the readout answer.

## September 16 afternoon (earlier): grab diagnostic + fraction chapter installed

Installed APK is artifacts/qa/cargo-20260916-131831 (SHA-256 18273ac6...2732): startup
placement fix, temporary practice input readout, temporary grip-or-trigger practice grab
override, and the code-complete whole/halves chapter (P1-04/P1-05). Physical check script:
docs/qa/cargo-CC-P1-02.md. Test-runner stalls were a dirty-scene Save alert; the wrapper
now guards against it. Older paragraphs below are historical where they conflict.

## September 16 (morning): owner paused implementation for Claude Code handoff

Read `docs/00-build/HANDOFF-2026-09-16.md` FIRST for current evidence and unresolved
issues. CargoCrew practice grabbing is broken: target hovers but selection never
activates; sampled OVR grip readings are zero, not a confirmed input root cause.
The installed APK is now the INVALID VendorGrabProof diagnostic (platform visible,
cube missing; supplied cube lacks controller GrabInteractable). It is NOT CargoCrew.
No fix/device acceptance. Preserve dirty work. Latest CargoCrew rollback diagnostic:
`artifacts/qa/cargo-20260916-114658/airlift-cargo.apk`. No commit/push performed.
Owner deferred voice; next product objective is functional labeled whole/halves.
Older authorization and acceptance statements below are historical where conflicting.


## September 16 owner priority override

Owner explicitly deferred AI/spoken guidance and associated audio captions, replay,
and mute work. Preserve existing written instructions and fraction labels. Immediate
implementation priority: fix practice grabbing, then deliver the P1-04/P1-05 core
loop: open Cargo Crew, see a stable reachable board, grab a whole labeled 1, split
into two equal pieces labeled 1/2, and manipulate those pieces against a fixed whole.
P1-03/provider work is deferred and is not a dependency of this demo. P1-02.5 and
remaining visual polish must not block this core loop. This overrides historical
sequential/AI-first requirements below; deferred tickets are not accepted or complete.
No new purchases, child-data processing, commits, pushes, or submission authorized.
Physical grab/split acceptance remains required; contest AI requirements remain an
unresolved release consideration, not a reason to block local arithmetic development.

September 15 implementation authorization: owner approved **CC-P1-01 only** using
`docs/00-build/PLAN.md` and its ticket. Headset checks may be batched; mark completed
code awaiting device review honestly. Do not begin later tickets automatically.

September 15 owner update: core AI spoken guidance and the dedicated style guide are
now included in the draft review package: `docs/00-build/AI-VOICE-GUIDE.md` and
`docs/00-build/STYLE-GUIDE.md`. Earlier voice-deferred text below is historical for
this proposed revision. Finish owner review before implementing a ticket.

September 15 planning checkpoint: [Cargo Crew review package](docs/00-build/README.md)
contains the proposed narrated-fractions PRD and local phased tickets. **Implementation
is paused for owner review.** This draft does not silently replace the approved
baseline below. New progress is in [DEV-LOG](docs/00-build/DEV-LOG.md).

This is a five-day Unity/Meta Quest hackathon build. Deadline: Friday,
September 18, 2026 at 11:59 PM CDT. Internal submission target: 6:00 PM.

Read `docs/plans/2026-09-13-unity-vet/plan.md` and its `research.md` completely
before changing project files. They are the authoritative plan and evidence.
`UNITY_PLAN.md` and the numbered Markdown files are retained design history.
If these concise instructions differ from the vetted plan, the vetted plan wins.

## Platform contract

- Native Unity application for the owned standalone Meta Quest 3S (ADB verified).
- Current compatibility proof: Unity 6000.6.0f1 Apple Silicon, Universal 3D/URP.
  This is a recorded deviation from candidate 6.3 LTS, not a proven release tuple.
- Unity OpenXR plus Unity OpenXR: Meta; do not add the deprecated Oculus XR
  Plugin.
- Use compatible Meta XR All-in-One/Core/Interaction SDK packages and lock their
  versions after the first successful device build.
- Use Meta Building Blocks for passthrough, rig, controllers, grab, ray, and
  haptics. Do not reimplement those systems.
- Controllers are required. Hands, voice, scene understanding, and automatic
  placement are deferred beyond the vetted execution plan.
- Place the workbench relative to the player once, then world-lock it; explicit
  recenter only with released pieces. No locomotion.
- Test every vertical slice on the physical Quest. Simulator success is not
  sufficient.

## Product contract

- P0 is one polished 6–8 minute flagship: The Cut.
- Cargo Grid is authorized only by the Wednesday gate in the vetted plan.
- Owner-approved opening: Arithmetic Lessons with Cargo Crew / Fractions plus
  disabled Coming soon cards for café/division and garden/multiplication.
  Preview cards do not authorize additional implemented lessons.
- No Till, third lesson, browser port, dashboard, accounts, multiplayer, flight
  simulator, runtime mesh slicing, or reward economy.
- “Cutting” swaps exact prebuilt segment prefabs; it does not modify mesh geometry.
- The airplane is narrative context and payoff, not a controllable vehicle.

## Pedagogical invariants

- Mathematical correctness is deterministic C#, never an LLM judgment.
- Every fraction refers to a visible equal whole.
- Equivalent fractions share the same length and number-line endpoint.
- Diagram and notation remain synchronized with the manipulatives.
- `RepresentationPhase` and `DifficultyTier` are separate concepts.
- No timers, lives, streaks, stars, generic praise, or punitive material loss.
- Feedback describes the task or strategy and leaves mistakes repairable.
- Neutral snap slots allow mathematically wrong arrangements; evaluate on Submit.
- Log only observable events. Never infer silent counting or gaze from controller motion.
- Report provisional proficiency, not mastery.

## AI and privacy

- The required AI behavior classifies a derived action trace and selects an
  allowlisted next scaffold.
- A serverless proxy owns the provider key. Never put secrets in the APK or repo.
- Validate responses against a closed schema and fall back locally after five
  seconds.
- Use one model provider. Hypotheses require matching observed evidence; ambiguous
  traces receive neutral support. Compare model routing against local rules.
- Collect no name, image, raw voice, account ID, or persistent device identifier.
- Use synthetic learner traces and entrant-only submission footage.

## Working method

- Setup/basic-interaction checkpoint is complete: safe APK installed; owner
  confirms passthrough, tracked controllers and grabbing. See `docs/qa/device-proof.md`
  for remaining Phase 1 QA; do not claim full acceptance or a completed lesson.
- Current slice is onboarding first: catalog, mission briefing, orientation,
  demonstrated grab/place, required practice and replayable help. See
  `docs/qa/onboarding.md`; arithmetic is not wired yet. Preserve DeviceProof.
- Work in `/Users/jad/Desktop/math-workbench-airlift`, branch `unity-airlift`.
  Read `docs/PROGRESS.md` for current state; do not repeat completed setup.
- Use Unity's official CLI/Pipeline for editor automation. Do not install a
  third-party editor bridge without a fresh authorization/compatibility review.
- Update the progress log after meaningful work and align this file, CLAUDE.md,
  the authoritative plan, and affected product documents when decisions change.
  Label verified, owner-reported, planned, and blocked states explicitly.
- Preserve historical documents as history. Record tests and device evidence,
  not claims that generated files demonstrate a working game.
- Implement vertical slices; do not create several partially working lessons.
- Preserve imported vendor packages and user changes.
- Keep custom architecture small: lesson director, task data, fraction math,
  pieces, snap zones, representation presenter, event log, tutor client, and
  world feedback.
- Write EditMode tests for fraction invariants, progression, local routing, and
  tutor response validation.
- Commit after each headset-tested slice with an outcome-focused message.
- Do not begin production art or stretch work before the device proof succeeds.
- No stretch feature survives while a P0 or P1 defect is open.

## Priority when time contracts

Reliable release APK > correct flagship lesson > visible AI adaptation >
legibility and world payoff > submission evidence > Cargo Grid > hands > voice.
# September 15 continuation

Owner authorized CC-P1-02; baseline APK built and scanned, physical acceptance pending.
Batch baseline device review with cargo-environment review. P1-02 is active; no paid
assets, commit/push or skipping owner font comparison implied.

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **math-workbench-airlift** (1745 symbols, 2394 relationships, 28 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> Index stale? Run `node .gitnexus/run.cjs analyze` from the project root — it auto-selects an available runner. No `.gitnexus/run.cjs` yet? `npx gitnexus analyze` (npm 11 crash → `npm i -g gitnexus`; #1939).

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows. For regression review, compare against the default branch: `detect_changes({scope: "compare", base_ref: "main"})`.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `context({name: "symbolName"})`.

## Never Do

- NEVER edit a function, class, or method without first running `impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `rename` which understands the call graph.
- NEVER commit changes without running `detect_changes()` to check affected scope.

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/math-workbench-airlift/context` | Codebase overview, check index freshness |
| `gitnexus://repo/math-workbench-airlift/clusters` | All functional areas |
| `gitnexus://repo/math-workbench-airlift/processes` | All execution flows |
| `gitnexus://repo/math-workbench-airlift/process/{name}` | Step-by-step execution trace |

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->
