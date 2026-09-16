# Planning review and validation

September 15, 2026. RAP revision / XL / Deep. Planning only.

## Final AI/style revision — September 15

Planning synchronization is complete; ready for owner approval of the first ticket.
No implementation ticket has started.

- Core AI begins P1-03; optional spoken questions remain Phase 4. Backend contracts,
  ticket sizes, tests and presentation ownership now agree.
- A challenger recheck found no material blocker in the constrained selection contract.
  The gap reviewer additionally required explicit audio verification and ownership
  of FractionNotationView; both fixes were applied and passed its bounded recheck.
- P1-03 owns a reviewed, hash-identified speech catalog and runtime verifier; live AI
  selects context-relevant guidance. Unreviewed audio cannot play; recordings alone
  do not count as AI. Dynamic synthesis/direct Realtime audio is separately gated.
- P1-02 owns FractionNotationView static specimens; P1-04/P1-08 reuse it.
- STYLE-GUIDE values are proposed trials, not measured accessibility guarantees.
- Structural validation: 51 Markdown files, 28 tickets in exact 10/8/6/4 counts;
  required headings, draft status, title/size agreement and local file links pass.
  git diff --check passed. No Unity build/runtime tests or provider inference calls performed.
- Start-readiness means P1-01 can begin after approval. It does not establish provider
  access, spending permission, child-release clearance or lesson runtime acceptance.
- Change-level tests, ticket device checks and full-journey milestone reviews are
  specified in PLAN and TESTING. Preserve the current uncommitted work at kickoff.

## Earlier independent reviews and applied changes

Two independent read-only passes challenged the draft and traced the user brief.
Available-model fallback was used because RAP's named Claude models were not
available in this session. Findings were applied rather than treating a first
draft as vetted.

| Finding | Applied resolution |
|---|---|
| Parallel fraction lanes lacked material ownership | D2/D3 separate ReferenceUnitId, per-source WholeId and per-lane placements; STAGE-FIXTURES defines conserved legal traces |
| Source positions confused with movable positions | Immutable source intervals separate from placedStartCell; removal/dock rules explicit |
| Missing learner choices and duplicate-event guards | Typed notation/sign/reason/Help commands, frozen submissions, commandId/expectedRevision and regrab cancellation |
| Merge could imply non-sibling quarters do not equal a half | Explain connector limitation; preserve free recomposition and test it |
| Guide contract introduced after its consumer | P1-03 owns guide/cue types; P1-04 owns stage catalog and reuses them |
| Early tickets required future audio/tests | Per-ticket milestone manifest, baseline written-only checks, P1-03 cue acquisition checklist |
| Eligibility ticket depended on final release and required APK | P4-01 documentary exception, no-go valid, no Unity/device test; enforcement in P4-02 |
| Optional AI blocked essential later work | Retained/deferred closure; contest gate separated from private-review/presentation work |
| Server controls tested only via Unity stubs | Actual-handler commands and HTTP/auth/budget/evidence tests in SERVER-VERIFICATION |
| Full story ending arrived after learning-flow acceptance | P2-08 explicitly temporary exit; P3-02 final narrated dispatch acceptance |
| Canonical devlog absent during early review | DEV-LOG and shared handoff created with actual owner-reported evidence |
| P1 fixture checks required later commands | P1 schema/whole trace only; each activity owns its trace; P2-08 runs the combined sweep |
| Equivalent fixtures lacked explicit active supplies | Both equivalent tasks now list active parents/children, source trays, placements and Submit |

## Verification

Challenger recheck reported no unresolved material blockers in the revised contracts.
Gap recheck's final two fixture findings were applied as recorded above.

Final structural check passed: 48 Markdown files in 00-build; 28 ticket primers
with exact 10/8/6/4 counts, required template headings, draft status and user-story
references. All local Markdown file links resolve. git diff --check passed.
Requirement coverage and dependency exceptions were reviewed against F-01–F-19.
Validation first failed to launch the unrelated global Node binary because of a
missing Homebrew simdjson library; rerun using existing Node 22.23.2 passed.
No global runtime repair was attempted. PLAN.md is 1,586 words, with detail kept
in the storyboard, design contracts and tickets rather than inflating the overview.
No Unity tests, builds, installs, new audio generation or external ticket publication
were performed in this planning task.

Owner design review remains required; independent review is not owner approval.
High-impact defaults are listed in PLAN.md. Runtime input comfort, provider
permissions and actual lesson performance remain implementation gates.
