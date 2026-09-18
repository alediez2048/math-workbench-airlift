# Cargo Crew lock checklist (owner decision 2026-09-16 night)

**Owner decision:** lock the Cargo Crew / Dock 7 lesson, finish the remaining work that keeps it in good shape,
then move to planning the next two lessons (Neighborhood Café · division, Community Garden · multiplication).
This supersedes the older product-contract lines "no third lesson" and "preview cards do not authorize
additional implemented lessons": the owner now authorizes planning both. Implementation of those lessons still
starts only after their plan is approved.

**Owner-requested change 2026-09-17 (lock exception):** truck shapes and the drive-away route (load in the middle,
turn left, leave through a DOCK EXIT gate). Math, chapters, card copy and voice tools are unchanged; the golden Cargo
voice recording still passes (only the shared no-voice welcome caption changed, for Dee).

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
| L-1 | Integrate, verify and install cranes + load-into-trucks; owner accepts on headset; commit | CC-D-10 (round 2) | done: owner "looks good, keep going"; committed 2740d8a |
| L-2 | Recovery and interruptions | CC-P1-09, CC-P3-03 | code done, headset check pending: while paused a voice action runs nothing and the guide stays silent; taking the headset off pauses the guide until Play; leaving mid-load or after an accepted load and returning gives a coherent chapter (RecoveryTests 4/4 confirmed RED first; DockWorkbenchWiringTests leave-and-return test passed on first run, a regression guard). Reviewed in code and already coherent: next/restart/back during the drive-away stop it and restore crates; voice actions while a crate is held refuse with "Let go of the crate first"; controller tracking loss releases through the SDK cancel path. One-controller play is possible by design (ray + voice + either hand grabs); not yet checked on device |
| L-3 | Remove temporaries | CC-P1-02 leftovers | done: practice-panel controller readout off (test confirmed RED first); grip-or-trigger fallback kept (D-4); `[Nerdy] UI` press logging kept, low volume |
| L-4 | Library tile logo (launcher shows no icon) | task #11 / CC-P0-01 | explained 2026-09-17: Nerdy is the only sideloaded app on the Quest, and Horizon OS lists sideloaded apps only under the Library's Unknown Sources filter, without Store artwork; a normal tile needs a Meta release-channel upload (owner decision). Side effect found: the icon attempt had re-imported the logo as a Default texture and blanked the consent-card logo (fixed). Earlier notes: the APK's adaptive icon already uses the Nerdy logo, and Unity 6.6 supports only adaptive Android icons (the Unity-logo mdpi PNG in the APK is a pre-API-26 fallback the Quest does not use). A default-icon change was tried, had no effect on the APK, and was reverted. Next: confirm what the Library tile shows for other sideloaded apps (it may never draw icons for unknown sources) before more changes |
| L-5 | Measure Quest 3S performance: three post-warm-up full runs, CPU/GPU p95 ≤ 13.9 ms, stable over repeated chapters | CC-P3-04 | todo (owner wears headset) |
| L-6 | Release gates for this lesson | CC-P3-05 | inventory written: RELEASE-GATES.md. Open: dev HTTP mint (O-1), BLUETOOTH permission still in the APK (store only), owner confirmation of vendor SDK and OpenAI terms |
| L-7 | PlayMode runner flake: make the wrapper pass without a manual domain reload | task #12 | done: the wrapper ran the baseline EditMode suite before PlayMode; PlayMode suites now run first with one automatic reload-and-retry on zero results (wrapper unit tests 7/7). Proven 22:12: CargoBaselineTestsSceneTests passed inside the full wrapper run with no manual reload and no retry |
| L-8 | Ticket bookkeeping | — | done: 25 ticket rows in TICKETS.md now show accepted, lock item, deferred or superseded status; D-1..D-4 marked as recommendations |

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

## Lock exception L-9 (owner, 2026-09-17 evening): the lesson card follows the spatial standard

The owner chose reading distance as the anchor for `NerdySpace`, so the lesson card above the workbench moved
from 0.65 m / 920x470 to 1.2 m / 1440x840 with every other panel, and its text is Poppins like the rest of the app.
`CargoLessonDirector`, `CargoLessonModel`, `CargoChapter`, `OnboardingDirector` and `BuildDockWorkbench` are
untouched; `CargoVoiceCharacterizationTests` still reproduces the golden script byte for byte. Applied by
`AgentScripts/ApplySpatialStandards.cs`; pinned by `SpatialStandardTests`.
