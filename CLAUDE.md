# Math Workbench: Airlift — instructions for Claude

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
