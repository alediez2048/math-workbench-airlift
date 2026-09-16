# Cargo Crew — guided fraction experience PRD

## September 16 demo scope override

Immediate target: choose Cargo Crew, see stable reachable board, grab whole labeled
1, split into equal grabbable halves labeled 1/2. Voice/AI and associated audio UI
are deferred by owner; preserve written instructions and numeric labels. See
[handoff](HANDOFF-2026-09-16.md). No playable fraction implementation yet.

**Revision:** September 15, 2026 · **Status:** draft, owner review required.
This is one fraction lesson with three chapters, not three separate subjects.

## Confirmed revision: AI guide and visual quality

The owner requires an AI voice guide throughout Cargo Crew. This is now core,
not deferred. It receives committed lesson state and explains next steps and
outcomes; Unity grades and advances the lesson. OpenAI is the first candidate to
verify. Recorded audio is resilience only, not fulfillment of the AI requirement.
See AI-VOICE-GUIDE.md. Optional spoken questions remain separately gated.

Typography is part of the designed experience: deliberate title/body/math styles,
spacing and contrast, with font options reviewed before import. See VISUAL-DESIGN.md.

## Outcome

A learner joins a miniature cargo crew, learns the controllers, and prepares
model strap lengths for deliveries. They can see a whole become equal parts,
read each fraction, reconstruct a whole, generate equivalent fractions, and
compare quantities on the same number line. The experience explains what to do,
why it matters, and what their action changed through captions, spoken guidance,
pointers and gentle controller feedback.

The current orange-block onboarding is the foundation, not the completed product.
The setting is a **stationary mixed-reality workbench in the real room**, with
miniature parcels and an aircraft payoff. “Immersive” means coherent story and
interaction, not a fully virtual port, room reconstruction or flight simulator.

## Audience and evidence boundary

Provisional learning persona: Grade 4, approximately age 10, with Grade 3 fraction
foundations revisited. This is an assumption from earlier planning, not a newly
approved age range. Initial QA uses the adult owner and synthetic traces only.
No child deployment or learning-efficacy claim follows from adult usability tests.
Broader child deployment requires a separate age/platform/privacy/consent review.
The current ElevenLabs eligibility problem is described in constraints.md.

## Three-chapter story

1. **Join the crew: a whole and its halves.** Mission, comfortable placement,
   controller demonstration, practice, reference whole, equal split, two halves,
   rebuild one, then prepare a half-length strap without answer-leading ghosts.
2. **Prepare different deliveries: quarters and equivalents.** Split both halves
   into four quarters, build three quarters, join two quarters into a half, and
   generate matching lengths such as one-half/two-quarters and three-fourths/six-eighths.
3. **Check the shipment: compare and explain.** Compare three-fourths/five-eighths
   with equal wholes, choose a comparison symbol and a model-based reason, then
   solve a fresh delivery with faded support. Dispatch concludes the story;
   completion is not a mastery certificate.

See STORYBOARD.md for actual lines, feedback and advance conditions.

## Implementation and testing decisions

Use one deterministic lesson-state module, exact inventory/placement rules,
thin SDK adapters, shared cue/caption guidance and isolated network adapters.
Test observable behavior rather than private implementation details. Existing
OnboardingFlowTests are the prior example; add math, transaction, guide and scene
suites plus physical Quest checks. Server handlers need their own integration tests.
These module and test boundaries are proposed for owner review.

## User stories

- US1: I understand my role and the delivery goal before touching an object.
- US2: I can orient the bench, identify grip/trigger, practice safely and ask for help.
- US3: I can hear or read each instruction, replay it, and mute without getting stuck.
- US4: I see the unchanged whole and labels while splitting into equal halves.
- US5: I can put halves in two locations and see one-half plus one-half equals one.
- US6: I can make four quarters and distinguish part size from number of parts.
- US7: I can bring two compatible pieces together to make a larger exact piece.
- US8: I can inspect a larger/smaller view without labels becoming mathematically false.
- US9: I can construct equivalents and compare fractions using lengths and symbols.
- US10: I can try a new problem, keep a wrong valid arrangement, and repair it after feedback.
- US11: I can recover from dropped pieces, tracking loss, replay, reset and re-entry.
- US12: I experience a complete cargo story with a clear ending.
- US13: As owner, I can verify one ticket, see evidence, and resume work from its devlog.
- US14: As learner, I receive contextual AI spoken guidance throughout the lesson,
  without needing to speak; optional spoken questions require separate safeguards.
- US15: As learner, I see attractive, consistent typography with readable instructions,
  distinct digits and clearly stacked fraction notation.

## Scope and priorities

**Core proposal:** physical controller onboarding; exact prefab-based split and merge;
fraction labels, ruler and symbolic expressions; narrated/captioned activities;
action pointers; haptics; independent attempts; replay/reset; cargo payoff.
The smallest meaningful new milestone is a narrated whole-to-halves learning loop,
not every advanced feature at once.

**Enhancements after that loop:** quarters, equivalence, comparison, controller
button-chord shortcut, synchronized model zoom, accessibility hardening and release evidence.
A bounded AI scaffold selector is retained from the earlier contest plan; it is
not a live conversational tutor and cannot grade mathematics.

**Required core AI:** state-aware spoken guidance from P1-03, captions and safe
fallback, live evidence by P1-10 and for every implemented chapter. Provider access
and safe operation must be proved; no recording-only substitution.
**Separately gated:** microphone-based questions and any child deployment, including
voice-output-only use. No purchases, voice cloning, child recordings or submission.

Café and Garden stay disabled “Coming soon.” No tickets to build them this round.
No freehand mesh cutting, arbitrary unlabeled scaling, full locomotion, multiplayer,
student accounts, persistent profiles, timed rewards or general-purpose chat.

## Success criteria

- Every instruction has an equivalent caption and a usable controller action;
  a muted/offline player can complete the same core tasks.
- Whole length, visible pieces, notation and rational state agree after every action.
- Actual actions, not Next clicks, unlock mathematical stages. No hidden correctness
  signal in snap targets or haptics before Submit during independent tasks.
- A fresh adult tester can follow the journey without extra oral coaching; record
  every needed intervention rather than inventing a success percentage from one test.
- Record first-attempt versus helped success on fresh values locally; report only
  observed performance, not long-term retention or mastery.
- Release candidate passes physical Quest 3S controls, comfort, recovery and
  performance checks; target 72 Hz, p95 CPU and GPU each ≤13.9 ms.
- Each ticket has named tests, a physical/manual check, evidence and a real completion
  rationale. Passing tests alone does not close headset acceptance.

## Approval checkpoint

Review the story, progression, 10/8/6/4 phase counts, the implementation of the
confirmed core AI-guide requirement, typography directions and proposed resizing. Live conversation and free
quantity resizing are not silently substituted with these recommendations.
Approve or revise the package before starting CC-P1-01.

## Cargo setting acceptance — owner clarification

Cargo Crew opens as a miniature mixed-reality terminal with a delivery truck,
containers, loading/measuring platform, destination tags, parcels and parked aircraft.
The role and shipment purpose must be understandable before the first math action.
Cargo progress reflects accepted lesson actions; it is not just a final decorative
animation. See STYLE-GUIDE and F-21. No full-VR environment or vehicle simulation.
