# Constraints — Math Workbench: Airlift

September 15 planning checkpoint: [Cargo Crew review package](../00-build/README.md)
contains the proposed narrated-fractions PRD and local phased tickets. **Implementation
is paused for owner review.** This draft does not silently replace the approved
baseline below. New progress is in [DEV-LOG](../00-build/DEV-LOG.md).

Draft v0.1 · September 14, 2026. Corrected filename for the requested
“constraings.md.” Derived from the [authoritative plan](../plans/2026-09-13-unity-vet/plan.md).

## Hard boundaries

| Area | Constraint | Consequence |
|---|---|---|
| Delivery | Deadline September 18, 2026, 11:59 PM Central; internal target 6 PM. | Work backward from a functioning release and captured evidence. |
| Platform | User prefers Unity; owned headset verified as standalone Quest 3S. | Native APK, no parallel WebXR/SceneView/Snapchat implementation. No silent platform pivot. |
| Scope | One fraction flagship in three chapters; one gated multiplication lesson at most. | Reuse one scene, rig, task engine, representation model, and support pipeline. |
| Interaction | Controllers; stationary, world-locked bench; explicit recenter. | No required hands, voice, locomotion, spatial room scan, or camera-frame access. |
| Math | Fixed equal whole; quantity conservation; deterministic evaluation. | No LLM grading, freeform mesh cutting, answer-aware snapping, or visual proportions that contradict numbers. |
| Learning | Errors stay inspectable and repairable. | No countdown, lives, loss of material, stars, streaks, shaming, or generic praise. |
| AI | Only evidence-supported routing to authored scaffolds. | No open-ended tutoring chat, emotional/learning-style inference, or generated learner-facing explanations. |
| Privacy | Synthetic learner traces and entrant-only capture for this build. | No recruitment of children, identifying student data, video analysis, or persistent profiles. |
| Quality | Zero open P0/P1 in every enabled submission path. | Remove failing stretch paths; do not waive incorrect math to meet the deadline. |

Deadline and evaluation-access requirements come from the [official rules](https://hackathon.nerdy.com/terms),
rechecked September 14. Design restrictions above are our project decisions, not
claims that the contest mandates Unity, this age group, or a live tutoring model.

## Calendar and stop-loss gates

All times are America/Chicago (CDT). Gates are targets, not completed milestones.

| When | Required evidence / response |
|---|---|
| First four setup hours | Review actual account/toolchain/device progress. If blocked, report the concrete cause; do not automatically switch frameworks. |
| Monday Sep 14 evening | One real half-task on Quest: Submit, wrong attempt, repair, reset. Minimal cube proof can pass Phase 1 earlier. |
| Tuesday Sep 15 evening | Entire offline flagship playable. If missed, cut Cargo Grid and decorative complexity; hands/voice remain deferred. |
| Wednesday Sep 16, 3 PM | Cargo Grid allowed only if Phases 1–4 are accepted and zero P0/P1 remain. Give it at most five hours; otherwise mark SKIPPED. |
| Wednesday | Record rough footage of the complete actual lesson and adaptive behavior. |
| Thursday Sep 17, 6 PM | Feature freeze; final capture. |
| Friday Sep 18, 6 PM | Internal submission target; remaining time is contingency. |
| Through Sep 23; buffer Sep 24, 11:59 PM | Preserve accessible, fixed evaluation artifacts. No automatic monitoring is configured. |

## Assumptions, not verified facts

- **Learner:** age-10 Grade 4 learner who can count and has encountered equal parts.
  This is a design persona, not evidence of age suitability or a tested cohort.
- **Theme:** aircraft cargo straps confirmed by the owner; detailed art remains
  a draft. Café and garden are future concepts with disabled preview cards only,
  not additional implemented lessons. Onboarding precedes arithmetic in the MVP.
- **Toolchain:** installed 6000.6.0f1 Apple Silicon is the provisional proof
  candidate, a recorded deviation from 6.3 LTS. No exact
  patch/package combination is accepted until it works on this physical Quest.
- **Backend:** Vercel and Claude Haiku 4.5 are proposed defaults, subject to account
  access, current service availability, and owner-approved limits.
- **Ergonomics:** a 0.4 m unit whole and enlarged piece handles are starting values.
  A browser mockup cannot establish reach, text legibility, or comfort.
- **Duration/performance:** 6–8 minutes and 72 Hz are design targets, not measurements.
- **Readiness:** Unity scaffold, Android tools, and authorized ADB are verified;
  Meta team/account setup is owner-reported. Provider access and release licensing
  remain unresolved. Safe APK build/install and owner-confirmed passthrough,
  tracking and grab have passed basic checks; full physical QA remains open.

## Human, automation, and budget boundaries

The owner must handle account sign-in, license eligibility, developer verification,
phone/headset pairing, USB approval, headset wearing/testing, and the final rights
decision. Code generation can create C#, data, tests, editor utilities, and the
proxy. Scene assembly is shared. No editor-control connection is currently proven.

No precise “percent coding versus manual” is defensible before the first slice.
Track actual work instead. Use official Unity CLI/Pipeline; a third-party bridge
is not a prerequisite and requires fresh review before adoption.

No new purchases, charges, public inference, or submission are authorized by this
document. Keep inference disabled if credentials or usable cost controls are absent.
Rate limiting reduces abuse; it does not guarantee a total spending cap.

## Reuse and rights gates

Use vendor infrastructure without claiming ownership of it. Preserve notices,
record exact licenses/versions, and do not redistribute package caches or secrets.
Native APK eligibility is an interpretation of the rules, not a sponsor guarantee.

The contest contains a broad assignment of original entry rights and a
noncommercial license-back, plus third-party and disclosure conditions. The owner
must review and record an informed decision before submission. Unity tier/runtime
terms and exact Meta package licenses also require review; this is not legal clearance.
See [rule §7](https://hackathon.nerdy.com/terms) and the [prior evidence](../plans/2026-09-13-unity-vet/research.md).

The GSD Skill Creator repository has additional license considerations; consult
[skills-review.md](../skills-review.md) before copying any of its content into
deliverables. A skill installed as a development reference is not automatically
a runtime dependency or permission to redistribute it.

## Explicit non-goals

No second reading/language entry, third implemented math lesson, browser port, mobile AR Lens,
flight simulator, multiplayer, accounts, classroom dashboard, database, rewards
economy, open-world environment, automatic room placement, or new orchestration framework.
No empirical efficacy, mastery, long-term retention, or superiority-to-2D claim.
