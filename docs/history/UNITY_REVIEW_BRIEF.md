# Math Workbench: Airlift — independent review brief

> The completed RAP review and current plan live in [docs/plans/2026-09-13-unity-vet](../plans/2026-09-13-unity-vet/plan.md).

Review date: September 13, 2026.

Read `UNITY_PLAN.md`, `AGENTS.md`, and this file. The numbered `00_` through
`03_` documents are preserved WebXR research and are not current instructions.

## Situation

- One developer
- Apple Silicon Mac; Unity was not installed when this plan was written
- Meta Quest 3 available
- Five build days before the Friday, September 18 deadline
- Native Unity/Quest APK rather than WebXR
- Required baseline: one complete fraction lesson plus one visible AI adaptation
- Conditional stretch: one multiplication-array lesson

## Review standard

Attack the plan rather than summarizing it. Prefer a smaller, reliable,
mathematically honest product over a broader proposal. Distinguish fatal
submission risks from ordinary polish work.

Verify current claims only against primary sources: Nerdy's official rules,
Meta developer documentation, Unity documentation, standards publishers, and
original research or evidence syntheses.

## Decisions already made

- Unity 6.1+ URP, OpenXR, and Meta XR SDK Building Blocks
- Standalone Quest 3 APK
- Passthrough, seated/standing workbench, no locomotion
- Controllers required; hands optional
- No runtime mesh cutting; swap exact prefabs
- One flagship lesson: The Cut
- Cargo Grid exists only after the Wednesday expansion gate
- Mathematical correctness is deterministic C#
- AI interprets an action trace and chooses an allowlisted scaffold
- No voice, dashboard, accounts, database, browser port, or third lesson in P0
- No real child data or footage

Challenge a decision only when evidence suggests that retaining it is riskier
than changing it inside the remaining schedule.

## Questions the review must answer

1. Can a developer with this machine reach the first Quest APK gate inside four
   hours, and what exact failure should trigger the IWSDK fallback?
2. Are the chosen Unity, OpenXR, Meta SDK, Android, and macOS requirements
   current and mutually compatible?
3. Which planned behavior is accidentally reimplementing a Meta Building Block,
   Interaction SDK prefab, Unity package, or official sample?
4. Is each fraction task mathematically valid for an equal whole and aligned to
   the named Grade 3/4 standards?
5. Does the AI change the learning experience causally, or could it be removed
   without anyone noticing?
6. Is any personal data, third-party material, package license, or submission
   behavior noncompliant with the official rules?
7. Does the Thursday feature-freeze rule leave enough time for device capture,
   APK verification, and a sub-three-minute video?
8. What is the single highest-value deletion still available?

## Required response

Return:

1. Five risks ranked by expected cost if wrong
2. Incorrect or outdated technical claims
3. Missing acceptance tests
4. Any pedagogy claim stronger than its evidence
5. A go/no-go judgment for Unity
6. The first three cuts to make if Day 1 or Day 2 slips

Be blunt. Do not propose more features as the solution to a scope problem.
