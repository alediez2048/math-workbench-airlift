# Ticket primer template

Adapted from the owner's LabelCheck TICKET-TEMPLATE.md. Use a concrete ticket file
such as tickets/CC-P1-05.md as the fully filled example. Fill all planning fields
before review; only actual starting-state fields and completion Why remain deferred.
This template is guidance, not authorization to start any draft ticket.

## Copy-Paste This Into New Agent

Read PRD, CONTEXT, systemsdesign, techstack, requirements, constraints, TESTING and
TICKETS under /Users/jad/Desktop/math-workbench-airlift/docs/00-build.
Name the approved ticket, its goal and exact files to inspect. Record what is
actually done at start and what is not. Work only on that ticket; preserve existing
changes; run its tests; stop for review before the next. No automatic push/merge.

## Context Summary for Agent

### What the previous ticket delivered (at start)

Actual files, infrastructure, evidence, branch and uncommitted ownership.

### Ticket Scope

Phase; scope size (no speculative time estimate); dependencies; existing branch;
requirement IDs and user stories; owner approval status.

### Acceptance criteria

Observable user behavior plus named failure/recovery conditions, each testable.

### Implementation details

Specific state, interaction, UI/audio and verification steps for one vertical slice.

### Key constraints

Applicable D-* decisions, math invariants, privacy, accessibility and authority.

### Files to modify

Exact paths; current relevant contents inspected at start; intended action.

### Files to create

Exact planned paths, purpose and tests; distinguish planned versus already existing.

### Config / schema / store updates

Concrete stage/cue contracts and compatibility changes; explicit none if not applicable.

### Testing requirements

Exact commands; named happy/error/edge cases; physical checks; eval if relevant;
evidence destination and devlog update. Passing pure tests is not hardware acceptance.

### Reference

Relevant product/design sections and primary-source evidence.

### Common gotchas

Specific traps for this ticket rather than generic coding advice.

### Definition of Done

Separate code-complete from owner/device-accepted. Record actual checkpoint only
after approval; no automatic push or merge into main.

### Expected output

What the owner can see, do or verify when this ticket is finished.

### Dependencies to install

Existing reuse or explicitly approved version/license; never an implied purchase.

### Why (fill at completion)

Actual scope/technical choices and accepted trade-offs. Mirror verbatim in DEV-LOG.
