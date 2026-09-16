# Execution context

Read PRD.md, PLAN.md, requirements.md, constraints.md, systemsdesign.md, TESTING.md
and the selected ticket before work. This package is draft until owner approval.

## Owner-authorized kickoff

CC-P1-01 is authorized. Physical checks may be batched at meaningful checkpoints;
code-complete/awaiting device review remains distinct from accepted. No automatic
implementation of later tickets or Git operations is implied.

## One-ticket workflow

1. Confirm approval and cleanly identify existing changes; do not assume the working
   tree is clean. Record actual starting state and previous ticket evidence.
2. Select exactly one ready ticket from TICKETS.md. No parallel implementation.
3. Inspect listed files; preserve existing code. Use Game Developer for Unity
   implementation and 3D Interaction Design for controls/comfort. Skills are not
   evidence that an interaction is accessible or educationally effective.
4. Implement the smallest complete user-visible slice; run named automated tests,
   rebuild the exact scene and conduct the relevant headset checks.
5. Record code-complete separately from owner/device-accepted. Stop for missing
   authority or new provider/data scope; do not silently broaden work.
6. Fill the ticket's Why and mirror that paragraph verbatim in DEV-LOG.md. Record
   commit/hash, artifact, tests, failures, deviations and next action.
7. Present outcome to owner. Commit/push only with approval; no automatic merge
   into main. Move to the next ticket after its review.

## Status vocabulary

draft → ready (owner-approved, dependencies accepted) → in-progress →
code-complete → accepted. blocked records cause and owner action.
No speculative completed checkboxes. HITL = owner/device interaction is required
to close; AFK describes implementation capability, not permission to skip review.
All Unity feature tickets require a physical verification gate.

## Canonical language

Whole: immutable reference length for a task, identified by WholeId.
Piece: conserved interval of that whole, not a freely stretchable mesh.
Split: one parent becomes two equal children, conserving occupied eighth-cells.
Merge: two compatible sibling pieces become their authored parent.
Recompose: place separate pieces side-by-side; not necessarily merge.
Zoom: scale the entire representation together; quantity does not change.
Chapter: a stage group inside Cargo Crew, not a separate subject card.
Attempt: one independent response before Submit; help use is recorded explicitly.
Voice guide: required live AI spoken output driven by lesson state. Recorded cues
are fallback; microphone questions/conversation are a separately gated capability.

## Documentation transitions

DEV-LOG.md owns new progress; ../PROGRESS.md remains dated history with a pointer.
Existing root AGENTS/CLAUDE and prior RAP plan remain the approved baseline until
this revision is reviewed. After approval, update those pointers in one checkpoint.
Never copy LabelCheck's web commands or automatic push/merge instructions.

## Presentation and AI source of truth

[Style guide](STYLE-GUIDE.md) defines the proposed shared visual/spatial design;
[AI guide](AI-VOICE-GUIDE.md) defines required live, constrained spoken guidance.
Both are planning artifacts, not installed fonts or implemented services.
