# Proposed skill/workflow configuration — owner review

The setup-matt-pocock-skills process requires confirmation before installing active
agent tracker/triage configuration. This is the proposed configuration only;
no active docs/agents files or tracker-routing block was installed.

- Tracker: local Markdown, docs/00-build/tickets, indexed by TICKETS.md.
  This follows the supplied methodology rather than GitHub Issues or .scratch.
- PRD: docs/00-build/PRD.md; no external publication.
- Planning status: draft / awaiting owner review. Proposed triage vocabulary:
  needs-triage, needs-info, ready-for-agent, ready-for-human, wontfix.
- Execution status is distinct: draft, ready, in-progress, code-complete, accepted,
  blocked. A ready-for-agent implementation still needs owner/device acceptance.
- Domain: one context, docs/00-build/CONTEXT.md; decisions currently D1–D10 in
  systemsdesign.md. Create separate ADRs only when an approved decision changes.
- Consumer rule: read context, decision and ticket before work; one ticket at a
  time, no automatic issue publishing, purchases, commits or merges.
- Proposed agent block after approval: “Issue tracker: local docs/00-build/tickets.
  Domain/decisions: docs/00-build/CONTEXT.md and systemsdesign.md.
  Read docs/00-build/CONTEXT.md for statuses and review gates.”

This preserves the skill's confirmation requirement while letting the user review
the requested PRD/ticket drafts now. to-prd and to-issues publication steps remain
paused until approval. Their decomposition/testing guidance informed these drafts.
