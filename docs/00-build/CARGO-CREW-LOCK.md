# Cargo Crew lock checklist (owner decision 2026-09-16 night)

**Owner decision:** lock the Cargo Crew / Dock 7 lesson, finish the remaining work that keeps it in good shape,
then move to planning the next two lessons (Neighborhood Café · division, Community Garden · multiplication).
This supersedes the older product-contract lines "no third lesson" and "preview cards do not authorize
additional implemented lessons": the owner now authorizes planning both. Implementation of those lessons still
starts only after their plan is approved.

**Locked means:** no new learning activities or visual redesigns in Cargo Crew. Only defects, hardening,
release gates and the items below. Baseline: owner-accepted commit 53647f9 plus the cranes-and-trucks round
once the owner accepts it on the headset.

## Already accepted on the headset (bookkeeping only)

| Ticket | Covered by |
|---|---|
| CC-P0-01, 02, 05, 06, 07 | Nerdy launcher, design system, consent + welcome, lesson cards, guide in the workbench |
| CC-P0-04 (Unity side) | GuideSession, prompt gate, voice tools; proxy deploy still open (O-1) |
| CC-P1-04, 05, 06, 07 | Chapters 1–2 (whole, halves, rebuild, first delivery) |
| CC-P2-01, CC-P2-04 (2/4 = 1/2 part) | Chapters 3–4 |
| CC-P3-02 | Vehicles leave with LOADED tags, dispatch recap |
| CC-D-01..08 | Voice-first story card, voice actions, Dock 7 story, chapter engine, five chapters |

## Lock work I can do without the owner

| ID | Work | Ticket | Status |
|---|---|---|---|
| L-1 | Integrate, verify and install cranes + load-into-trucks; owner accepts on headset; commit | CC-D-10 (round 2) | built and installed 22:17 (cargo-20260916-221238), owner headset check pending |
| L-2 | Recovery and interruptions: Back and resume mid-chapter, reset/restart/next during the drive-away, Pause during a voice action, voice action while a crate is held, tracking loss gates grabs, one-controller path; tests for each | CC-P1-09, CC-P3-03 | todo |
| L-3 | Remove temporaries once L-1 is accepted: practice-panel controller readout, grip-or-trigger fallback decision, keep `[Nerdy] UI` logging behind a dev flag | CC-P1-02 leftovers | todo (see D-4) |
| L-4 | Library tile logo (launcher shows no icon) | task #11 / CC-P0-01 | open, cause not found: the APK's adaptive icon already uses the Nerdy logo, and Unity 6.6 supports only adaptive Android icons (the Unity-logo mdpi PNG in the APK is a pre-API-26 fallback the Quest does not use). A default-icon change was tried, had no effect on the APK, and was reverted. Next: confirm what the Library tile shows for other sideloaded apps (it may never draw icons for unknown sources) before more changes |
| L-5 | Measure Quest 3S performance: three post-warm-up full runs, CPU/GPU p95 ≤ 13.9 ms, stable over repeated chapters | CC-P3-04 | todo (owner wears headset) |
| L-6 | Release gates for this lesson: asset and license inventory (fonts OFL, generated meshes, logo rights), Android permission/network inventory, APK secret scan, no dev HTTP exception | CC-P3-05 | todo (HTTP part needs O-1) |
| L-7 | PlayMode runner flake: make the wrapper pass without a manual domain reload | task #12 | done: the wrapper ran the baseline EditMode suite before PlayMode; PlayMode suites now run first with one automatic reload-and-retry on zero results (wrapper unit tests 7/7). Proven 22:12: CargoBaselineTestsSceneTests passed inside the full wrapper run with no manual reload and no retry |
| L-8 | Ticket bookkeeping: mark accepted, superseded and deferred tickets in TICKETS.md; QA script matches the locked flow | — | todo |

## Needs the owner

| ID | What | Why it matters |
|---|---|---|
| O-1 | `vercel login` on the Mac | Deploy the guide proxy over HTTPS and remove the LAN HTTP exception; otherwise voice only works next to the Mac |
| O-2 | Consent card heading wording | Earlier request was garbled |
| O-3 | Is the live voice guide the project's "AI adaptation" feature, or is the hint router (CC-P3-01) still required? | Changes whether P3-01 is built or superseded |
| O-4 | Demo footage on the headset when submitting | CC-P0-09 / CC-P3-06 evidence |

## Decisions: remaining learning-activity tickets (recommendation in bold)

| ID | Ticket | Recommendation |
|---|---|---|
| D-1 | CC-P1-08 notation meaning (choose 1/2 vs 2/1 for the model) | **Fold into the guide**: the guide can ask it by voice using approved facts; no new UI. Small. |
| D-2 | CC-P2-02 three quarters, CC-P2-03 join quarters, CC-P2-05 compare 3/4 vs 5/8, CC-P2-06 fresh delivery | **Defer** until after the next two lessons exist; they are new activities, and the lesson is locked. |
| D-3 | CC-P2-07 two-button split chord and zoom | **Supersede**: voice "split it" and the Split fallback button cover it. |
| D-4 | Temporary grab diagnostics (readout on the practice panel, grip-or-trigger fallback) | **Remove the readout; keep grip-or-trigger** because the owner's grabbing works with it and the root cause was never isolated. |

## After the lock: planning mode for the next two lessons

Enter plan mode for Neighborhood Café (division) and Community Garden (multiplication): story, representation,
chapters, how much of the Dock 7 engine, voice tools and workbench they reuse, and a realistic cut line against
the Friday 2026-09-18 deadline (internal 18:00 CDT).
