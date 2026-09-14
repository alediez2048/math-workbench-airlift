# PRD — Math Workbench: Airlift

Draft v0.1 · September 14, 2026 · working title and art direction.

**Product promise:** make fraction relationships tangible by preparing cargo
straps at a small aircraft workbench. Build a quantity, explain it, repair it,
and see it do useful work in the world.

This document synthesizes the vetted plan; it does not authorize new scope.
[Requirements](requirements.md) define acceptance, [constraints](constraints.md)
define limits, and [tech stack](techstack.md) defines reuse. The
[execution plan](docs/plans/2026-09-13-unity-vet/plan.md) remains authoritative.

## Problem statement

For a learner who sees mathematics as disconnected symbols, being told that 3/4
equals 6/8 can feel arbitrary. They need to see what changes (number and size of
parts), what stays fixed (the whole and total length), and why the relationship
matters. A visually impressive VR worksheet is insufficient: the physical action
must reveal the mathematics, and errors must create useful opportunities to reason.

The creator has five days and a Quest 3S, identified through ADB after initial
planning described it as Quest 3. They want an immersive game, prefer Unity,
and want existing infrastructure to absorb tracking and interaction complexity.

## Solution and audience

**The Cut** is one complete, offline-capable 6–8 minute mixed-reality lesson in
three chapters. A virtual workbench sits within the learner's real surroundings.
Fraction pieces are cargo-strap segments; aligned lengths show equivalence and
comparison; a miniature aircraft makes completion feel consequential.

Primary design persona: a Grade 4 learner, approximately age 10, who can count
and has encountered equal parts but struggles with fractional magnitude. This is
an assumption, not a validated audience. Evaluation in this hackathon uses the
adult entrant and synthetic traces, not children or classroom deployment.

The user is stationary, seated or standing, using controllers. A warm, uncluttered
workshop, substantial manipulatives, spatial feedback, and a visible cargo payoff
provide immersion. Full-room scenery, free movement, and flight simulation are
not needed to fulfill this product promise.

## Experience and learning progression

### Onboarding and purpose (owner approved September 14; not yet implemented)

Introduce the learner's role preparing an aircraft delivery, explain why cargo
needs correctly sized straps, and guide the first grab before asking a math
question. Establish one whole, demonstrate equal halves, then ask the learner
to build one-half. Every chapter introduces its practical purpose and provides
replayable instructions/help. This is part of the flagship, not a separate
lesson or a new onboarding platform. The current cube scene contains none of it.

| Chapter | Tasks | Experience |
|---|---|---|
| Build | T01–T03 | Establish one whole, make equal halves, then assemble three fourths against the same ruler. |
| Compare | T04–T05 | Subdivide fourths into eighths without changing length; construct equivalence; compare 3/4 and 5/8 with an explicit reason. |
| Apply | T06 + payoff | Use new values with reduced initial help, then attach the completed straps and watch the cargo depart. |

Core loop: **inspect the task → manipulate → connect model and symbols → Submit
→ inspect task-specific feedback → repair or advance**. No timer pressures this loop.

In a comparison mistake, the student might choose 3/4 < 5/8 and explicitly say
“more parts means more length.” After Submit, guided support draws attention to
equal-sized units and equal wholes; the arrangement stays visible. A different
explicit benchmark strategy can instead show excess above 1/2: 2/8 versus 1/8.
The model does not claim to know a student's unspoken reasoning.

## User stories

1. As a learner, I want a simple grab tutorial so I can act without first mastering a complex control scheme.
2. As a learner, I want one visible whole so I know what every fraction is a fraction of.
3. As a learner, I want to partition straps into equal pieces so numerator and denominator have physical meaning.
4. As a learner, I want my pieces and number-line endpoint to agree so I can connect length to quantity.
5. As a learner, I want to subdivide without stretching the strap so I can see why equivalent fractions stay equal.
6. As a learner, I want to construct my own equation so I do more than recognize a supplied answer.
7. As a learner, I want equal-whole comparison lanes so a longer whole cannot mislead me.
8. As a learner, I want to commit an inequality and choose my reason so support can respond to evidence I actually supplied.
9. As a learner, I want wrong arrangements to remain visible so I can investigate and repair them.
10. As a learner, I want help that points to part size, endpoints, or a benchmark so I can try a strategy rather than receive a score.
11. As a learner, I want support to remain available offline so a network failure cannot stop the lesson.
12. As a learner, I want replayable instructions, pause, and reset so I can work at my own pace.
13. As a seated or one-controller learner, I want reachable pieces and ray controls so the task does not require two-handed precision.
14. As a learner, I want labels and partitions in addition to color so the meaning remains accessible.
15. As a learner, I want a fresh-value task so I can try applying the relationship without an answer preview.
16. As a learner, I want my straps to secure the cargo so the work has a visible purpose.
17. As the entrant, I want repeatable tests and a reproducible APK so I can safely iterate under a short deadline.
18. As the entrant, I want a clear record of live versus fallback behavior so I can describe the AI honestly.
19. As a judge, I want a short, readable video of the working experience so I can understand the interaction and learning intent without needing a headset.
20. As the entrant, I want a bounded scope and explicit release gates so optional polish cannot consume submission time.

## Pedagogical foundation

Use systematic instruction, precise mathematical language, connected physical/
diagram/symbol representations, and number lines. Introduce support, then reduce
it while keeping help available. These choices are informed by the
[IES mathematics intervention guide](https://ies.ed.gov/ncee/wwc/PracticeGuide/26).
The guide does not establish that this VR app is effective.

Generating equivalence and explaining unchanged length supports 4.NF.A.1;
equal-whole comparison, inequality notation, and justification support 4.NF.A.2.
The tasks address selected elements, not the full standards. See
[Grade 4 fractions](https://www.thecorestandards.org/Math/Content/4/NF/) and
[Grade 3 number-line foundations](https://www.thecorestandards.org/Math/Content/3/NF/).

Manipulation alone is not proof of understanding. T06 adds new values, and records
first attempt, requested support, and final result separately. Report descriptive,
provisional session evidence—not mastery, diagnosis, retention, or a learning gain.
No learning-style categories; no claim that all students learn best in VR.

## Implementation decisions

- Reuse Unity, URP, OpenXR, and Meta components for native Quest delivery.
- Use exact prebuilt partitions, not runtime mesh slicing. Keep eight atomic
  eighth-width cells per lane and preserve whole identity.
- Separate canonical mathematical value from authored notation and the learner's
  proposed expression. Evaluate an immutable snapshot on explicit Submit.
- Keep modules focused: fraction math, placement state, lesson progression,
  representation presentation, interaction adapter, event log, tutor response
  gate, and authored scaffold router.
- AI receives bounded observable evidence and chooses an allowlisted scaffold/
  prompt pair. Local rules guarantee completion if inference fails. Compare both
  on the same traces; report no model advantage if none is demonstrated.
- No learner accounts or persistent learner database. Provider secrets remain
  server-side. Deployment and actual headset evidence are release gates.

## Testing decisions and success measures

Tests verify observable behavior through public interfaces: preserved quantity,
correct comparisons, wrong-answer persistence, progression, and recovery. Do not
couple them to private fields or vendor implementation details. There is no
existing product test suite to extend; the Unity scaffold is only a starting point.

Test pure math, placement/progression, schema validation, evidence routing, timeout
races, and reset separately; then test scene flow and backend integration. Physical
Quest checks cover grab reliability, tracking recovery, legibility, comfort,
performance, passthrough, and actual capture.

Product success is a complete, understandable experience with correct math and
repairable errors. Engineering acceptance includes offline completion, live and
fallback support evidence, no P0/P1 defects, and release reproducibility. Record
actual duration and performance rather than inventing results. Educational
effectiveness remains a future research question.

## Out of scope

No third lesson, reading/language entry, open-ended chat, voice/hand baseline,
room scanning, flight controls, multiplayer, rewards economy, accounts, dashboard,
or browser port. Cargo Grid is a conditional second lesson only if the Wednesday
gate passes; it must reuse the same infrastructure and fit a five-hour cap.

## Further notes and visual references

The owner confirmed the cargo theme on September 14. Keep the experience mixed
reality: a virtual cargo workbench and miniature aircraft in the real room, not a
fully virtual port or hangar. Two alternative future settings are recorded in
[future lesson concepts](docs/future-lessons.md). They are options on paper only.

The [interactive mockups](docs/mockups/index.html) show arrival, fraction assembly,
equivalence, comparison/repair, and completion. Their navigation and design notes
are for reviewers, not planned headset UI. They are schematic design references,
not Unity output or evidence of headset usability. Physical scale is specified in
the plan and must be tested in-headset.

Submission rights, runtime/package licensing, account access, and exact installed
versions remain unresolved gates. This PRD does not authorize accepting terms,
deploying paid services, or submitting on the owner's behalf.
