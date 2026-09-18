# Onboarding repair — consolidated headset-feedback revision

**Mode:** revision · **Size:** L · **Tier:** Standard  
**Status:** Implemented; compilation, 133 focused checks, fresh-fixture runtime tour/three-exit walkthrough, and live current-line narration resume pass. Final owner refinement uses Play/Stop only, with a separate microphone notice. Integrated Quest preview built, scanned, installed and cold-launched; device acceptance remains pending. The earlier repair below is historical evidence, not acceptance of this revision.  
**Working baseline:** `lounge-onboarding`, HEAD `111049a` plus existing uncommitted repair, scene, package and settings changes. The latest installed autostart preview is preserved.  
**Evidence:** `docs/qa/onboarding-repair-2026-09-18.md`; [research.md](research.md).

## Summary

Simplify the existing experience without rebuilding it: one set of catalog paging arrows, one lesson exit, and one clearly labeled Play/Stop conversation control for Dee. Teach the remaining controls in a catalog-only tour whose spoken instruction, caption, highlight and accepted action always agree. Preserve the owner-confirmed automatic tour after questions and all existing math mechanics. Implement small internal increments, verify in the Editor, then deliver one integrated Quest review build.

## Latest brief and scope

The owner requested: remove bottom-right Back/Next and their onboarding references; retain the left/right arrows beneath the dashboard; remove Back to lessons in Cargo Crew, Community Garden and Neighborhood Café because the top-right X exists; replace Mic off, Music on and Help in the companion bar with Play/resume and square Stop; teach that control and Your room/Nerdy Lounge during onboarding. The earlier three unresponsive-button reports are retained as evidence, but repairing their old UI is superseded by replacement. These reports do not establish their root cause.

## Locked decisions

1. **One catalog paging pair:** keep the arrows directly beneath the dashboard. Remove the shared bottom-right Back/Next objects and tour dependencies on them, not merely their labels. Reject hiding them while leaving invisible hit targets. Preserve the profile's own navigation and Settings Done; do not equate shared navigation with every arrow in the app. User decision; confidence H; reversible.
2. **One exit per lesson:** remove redundant Back to lessons presentation in all three lessons; keep a labeled top-right X routed through the existing safe exit. Preserve release-held-object checks, cleanup, saved progress, controller B behavior and native chapter progression. Reject rewriting locked lesson mechanics to achieve a visual cleanup. User decision; confidence H; reversible.
3. **One Dee conversation toggle:** Play starts/resumes; square Stop stops. Keep the gear and compact blue state indicator. Remove companion-bar Mic/Music/Help only, not the Settings features or lesson-specific help/replay. Reject a visual-only pause that leaves listening or late speech active. User decision; confidence H; reversible.
4. **Stop is an effective session boundary:** immediately silence local playback, stop capture/transmission, cancel pending narration/conversation work, and reject stale speech, transcripts and tool actions. Play resumes from current UI/lesson state, not an old queued instruction. Existing permission, explicit consent and adult-test eligibility remain mandatory. Permission denial/offline/ineligible states must be clear and manually usable. Safety-preserving implementation requirement; confidence H.
5. **Catalog-only guided tour:** welcome with Let's begin → questions or Skip → Pick a lesson and automatic tour. Keep filters, surviving paging, Settings/Done, explicit Skip, Replay, and final lesson selection. Add environment choices and Dee control coverage. No additional compulsory microphone activation. User decisions; confidence H.
6. **No new framework or visual redesign:** preserve board size, reach, fonts, compact bar, provider, lesson math and blue indicator. A genuinely new surface would trigger the existing mockup approval rule; these are changes to existing controls. No gender/profile schema change, production deployment, commit/push, or saved-data reset.

## Proposed tour and control behavior

Implemented ordering has six gated stops, with Settings/Done as a substep of the fifth stop; the seven teaching segments below remain covered:

1. **Welcome to the math lounge + filters:** retain the richer lounge introduction, explain lesson cards and Featured/Newest/Most viewed, and highlight the filters. Trying one filter advances. Remove the old introductory “press Next” gate entirely; do not add a replacement generic Next button.
2. **More lessons:** highlight the existing right paging arrow; advance only after the page actually changes.
3. **Return a page:** highlight the existing left paging arrow; advance only after the page actually changes back. Do not count one click twice across a transition. With one page, explain that all lessons fit and bypass paging practice without waiting on a disabled target.
4. **Your room / Nerdy Lounge:** explain both modes and highlight the environment controls, targeting the alternate mode. Advance on a successful mode selection (not merely a click); preserve board position and tour progress. Once this stop finishes, the learner may restore their preferred mode without advancing another gate. If a mode is unavailable, show why and continue on the available mode rather than trapping the tour.
5. **Dee's Play/Stop:** explain the current action and state; stop/resume practice is optional, not a consent trap. Persistent written guidance remains visible when stopped. Finish this teaching segment by pointing to the existing Settings gear and instructing the learner to open it. That actual gear-open event moves into Settings; no new Continue button. While tour narration is active, Stop stays usable at every step, not just this segment.
6. **Settings then Done:** only after Settings opens, highlight Done and describe Settings including replay/audio options actually available. Done closes Settings and advances. Do not narrate Done while highlighting the gear.
7. **Choose a lesson:** only now permit a playable card or verified supported voice action to finish the tour and open the lesson. Explain that X returns here. Unavailable/coming-soon cards do not complete the tour.

At every step: one required action and matching instruction/target; Skip remains available. Spoken command examples appear only when genuinely available; manual operation works throughout. Stop must suppress tour speech too; manual advancement while stopped updates captions/highlights without speaking. Play during the tour explains only the current step. Stop is not Skip and does not mark the tour seen/completed. Existing blue indicator and text reflect real listening/speaking/stopped/unavailable state.

## Assumptions — four reversible defaults

- Merge lounge introduction with filter practice rather than adding another navigation button (assumed — not in brief).
- Conversation stopped state is session-scoped, retained across UI/lesson transitions, with existing safe startup behavior on a new app launch; no new persistence schema (assumed — not in brief).
- Control practice never requires Play/microphone activation: learner can open Settings and continue silently (assumed — not in brief).
- Remove the Music shortcut only; preserve current music preference and its Settings control rather than silently changing background audio (assumed — not in brief).

No additional owner decisions are needed to finish this plan. Provider/child-use permission remains unchanged, not implicitly granted.

## Drift from the earlier repair

| Earlier plan / current evidence | Revised requirement |
|---|---|
| Intro waits for bottom-right Next; catalog also has page arrows | Merge intro/filter stop; retain only real catalog paging arrows |
| Explicit Mic plus Music/Help shortcuts | One Play/Stop with clear state and permission handling |
| Six-stop tour lacks environment and conversation controls | Cover both before final lesson selection; derive count from steps |
| X exists but lesson cards still offer Back to lessons (owner report) | Remove redundant exit presentation in all three lessons |
| Tour-after-questions was skipped by old seen flag | KEPT: autostart repair, now confirmed by owner on headset |
| Tour audio previously drifted / arrived late | KEPT: current-step transcript verification and cancellation guards; extend to Stop |

## Setup and commands

All paths below are relative to `/Users/jad/Desktop/math-workbench-airlift`. One operator owns Unity; do not run simultaneous editor changes with Claude. First `git status --short --branch`; preserve all unrelated dirty files. Use GitNexus impact before symbol edits and report HIGH/CRITICAL risk. No commit/push requested.

```sh
/opt/homebrew/bin/unity command editor_status --project-path /Users/jad/Desktop/math-workbench-airlift/unity --timeout 8 --format json
/opt/homebrew/bin/unity command recompile_status --project-path /Users/jad/Desktop/math-workbench-airlift/unity --timeout 8 --format json
/opt/homebrew/bin/unity command run_tests EditMode RundownTests testName false true 300 --project-path /Users/jad/Desktop/math-workbench-airlift/unity --timeout 8 --format json
/opt/homebrew/bin/unity command test_status --project-path /Users/jad/Desktop/math-workbench-airlift/unity --timeout 8 --format json
```

Use `scripts/verify_cargo.py:run_tests` for completion polling and each named suite below; require nonzero discovered tests and zero failures. A dispatched command is not a passing test. `Nerdy > Preview` fresh/answered/returning/skipped fixtures are isolated, Play-only desktop checks. Stop Play before persistent edits. No SDK/package installs or provider changes are needed; reuse configured service without printing secrets.

For the final approved preview build, use `LoungePreviewSettings.Apply`, explicit CargoCrew scene and a new artifact name, then `LoungePreviewSettings.Restore` in a guaranteed cleanup path. Wait for actual compile/build completion, run `verify_cargo.scan_apk`, record hash and package identity, then install with `adb install -r` and verify installed identity before launch. Do not blindly run `scripts/lounge-preview.sh`: it deletes its existing APK and treats file existence as build success. Preserve `nerdy-lounge-tour-autostart-20260918.apk` as rollback and the separate release app. Installation/restart is a review checkpoint, not part of this planning turn.

## Implementation phases

### Phase A — Baseline and contracts (Size: S)

**Depends on:** none. **Touches:** QA record and isolated `unity/Assets/Airlift/Editor/OnboardingPreview.cs` fixtures only if needed.

- Record current bindings for shared navigation, catalog paging, question controls, three lesson exits, environment selectors and companion controls. Identify actual intended behavior without asking the owner to repeat the entire walkthrough.
- Capture focused baseline results and current golden differences. No rewriting golden data to obtain a green run.
- Define the new tour gates and conversation state contract before changing consumers. Do not preserve six-stop indexes in tests/builders.

**Tests:** existing `TourAutostartTests`, `RundownTests`, `RundownWiringTests`, `TourSpeechRegressionTests`, `TourSettingsSpeechTests`, `OnboardingRepairTests`. **Acceptance:** repeatable editor fresh/returning fixtures with preserved real saves; baseline failures explicitly recorded.

### Phase B — Reliable Play/Stop (Size: M)

**Depends on:** A. **Touches:** `unity/Assets/Airlift/Scripts/Welcome/NerdyDirector.cs`, `GuidePolicy.cs`; `Scripts/Guide/GuideSession.cs`, `ScriptedNarration.cs`, `MicStreamer.cs` only as required; `unity/AgentScripts/CompactAssistantBar.cs`; `CargoCrew.unity` serialized bindings.

- Centralize stopped/active state; route clicks and voice policy through it. Stop is synchronous locally even if network cancellation fails. Clear pending capture/output and invalidate the old interaction generation so late events/tool calls cannot execute. Preserve existing exact-tour-narration safeguards.
- Resume at most one connection/session path; reuse valid context or reseed from current state after reconnect. Never replay queued old actions. Handle rapid toggles and connection failure without a fake listening indicator.
- Replace only the three companion shortcuts with one labeled Play/Stop control. Permission prompt is requested only through eligible explicit activation; denied/unknown-age/minor paths remain manually usable. Existing automatic narration may remain at startup, but startup never enables capture.
- Keep the gear, captions and orb. Clearly distinguish speaking-only from listening; do not show Play as though Dee were silent while she is narrating. Settings Voice guide remains respected and must not silently be re-enabled by Play; explain if it blocks playback.
- Reuse existing `TogglePause` semantics. Implementation strengthens `GuideSession.End()` into a real session boundary: clears playback/capture/queues, disposes the old connection and invalidates its event generation. Resume reseeds current context; old server conversation history is not preserved. Keep Stop accessible through the existing Settings pause control while Settings hides the bar. Legacy callback/template references are retained beneath inactive presentation wrappers, rather than deleting locked lesson dependencies; run `ApplyControlCleanup` last after any supported presentation rebuild. No inactive wrapper can receive raycasts, even if legacy refresh re-enables its child.

**Tests:** add `ConversationControlTests`: `StopSilencesAndEndsCapture`, `StopRejectsLateAudioAndTools`, `ResumeUsesCurrentTourStep`, `RepeatedPlayDoesNotDuplicateSession`, `PermissionDeniedStaysManual`, `StopSurvivesScreenTransitions`, `SettingsVoiceOffIsRespected`; extend `GuidePolicyTests`, `TourSpeechRegressionTests` and bar wiring checks. **Acceptance:** real editor run can interrupt narration immediately, continue manually in silence and resume the current step; simulated delayed events cannot reactivate speech or navigation. Verify with the named test command pattern above, not just screenshots.

### Phase C — Navigation cleanup (Size: M)

**Depends on:** A. **Touches:** `NerdyDirector.cs`, `unity/AgentScripts/BuildNavArrows.cs`, supported narrow scene patching, `CargoCrew.unity`; existing shared lesson presentation bindings.

- Remove shared bottom-right controls and their raycast blockers/listeners while retaining real dashboard paging, question navigation, Settings Done and native lesson progression. Keep internal routing methods needed by other inputs.
- Remove Back to lessons from Cargo/Café/Garden presentation; bind top-right X through the existing safe exit. If locked lesson code re-enables an exit object, adapt presentation ownership narrowly; do not delete lesson callback references or rewrite math logic.
- Audit chapter transitions before removal; no lesson may become a dead end. Controller B retains existing safe semantics.

**Tests:** add/extend `OnboardingRepairTests` with `OnlyCatalogPagingPairVisible`, `QuestionsStillCompletable`, `SettingsDoneStillWorks`, `AllLessonsUseSingleExit`, `ExitWithHeldPieceRequestsRelease`, `ChapterProgressionStillReachable`, `CatalogControlsRestoreAfterExit`. Parameterize all three lessons. **Acceptance:** complete questions, page catalog, enter/exit each lesson, and reach the next native activity using existing controls in the actual serialized scene.

### Phase D — Complete synchronized tour (Size: M)

**Depends on:** B and C. **Touches:** `Scripts/Lounge/RundownScript.cs`, `LoungeRundown.cs`, `NerdyDirector.cs`, environment selector binding, `Scripts/Presentation/Dashboard/DashboardWall.cs` only if needed, `unity/AgentScripts/BuildTour.cs`, tour tests and scene references.

- Implement the seven teaching segments above, with explicit event gates, actual target references and derived counter values. Environment and assistant stops survive layout/state changes.
- Keep automatic start after answered/skipped questions; preserve normal return-from-lesson seen state. Replay uses the same new sequence; no forced data migration or re-questioning of completed profiles.
- Include valid voice/manual alternatives without requiring a mic. Stop always reachable while tour audio plays; no gate locks the control it instructs the learner to press. All retired-control references removed from captions and scripted/generated instruction context.

**Tests:** `RundownTests`/`RundownWiringTests`: `IntroDoesNotRequireRemovedNext`, `PagingAdvancesExactlyOnce`, `EnvironmentSelectionPreservesTour`, `UnavailableEnvironmentDoesNotTrapTour`, `DeeStopDoesNotSkipTour`, `SilentTourRemainsCompletable`, `GearThenDoneTargetsMatchSpeech`, `FinalLessonOpensOnlyAtFinalGate`, `ReplayUsesNewOrder`, `SinglePageCatalogDoesNotDeadlock`; run `FrontDoorToolTests`, `TourAutostartTests`, `TourSpeechRegressionTests`, `TourSettingsSpeechTests` as regressions. **Acceptance:** entire manual-only and eligible voice-enabled editor walkthroughs match each spoken line to visible caption, highlight and actual action; rapid interactions cannot resurrect prior instructions.

### Phase E — Integrated editor and Quest acceptance (Size: M)

**Depends on:** B–D. **Touches:** QA records and build evidence, no additional features.

- Run phases' tests plus `WelcomeFlowTests`, `LoungeWiringTests`, `DashboardTests`, `DashboardWiringTests`, `LoungeRoomTests`, `FrontDoorToolTests`, lesson model/wiring suites and `CargoVoiceCharacterizationTests`. Report existing golden drift separately, review any new differences, and do not silently accept a failing suite.
- Visually verify compact layout, both environment modes, controller target accessibility, no phantom buttons, truthful orb state and readable silent instructions. Desktop success is not headset acceptance.
- Deliver one integrated preview APK after editor checks; verify build identity, credentials scan and install result; preserve release app and saved data. Owner headset script: finish/skip questions → tour autostarts → filters → page right/left → environment choice → stop/resume Dee → Settings/Done → lesson; check X and native progression in all three lessons. Also perform one silent/denied-permission path and Replay.
- Update QA, DEV-LOG, CLAUDE and AGENTS with exact verified/remaining status. Stop at review if a gate fails; no claim of completion based on test counts alone.

**Acceptance:** owner confirms the integrated flow on Quest and no regressions in grabbing, math/labels, lesson progression or station placement. Preserve rollback APK and record any remaining unrelated limitations.

## Risks and challenge review

Independent review found two blockers, resolved in the implementation contract: first Play must disclose microphone activation (button text **Play — mic on** for eligible adults, **Play — narration** otherwise, with explanatory persistent text); only an explicit `ageBand == "adult"`, tester flag and consent permit capture. Startup narration uses Stop with a narration-only state, never implying listening. Later Play resumes consented interaction; denied permission remains narration-only. Unknown/skipped/prefer-not-to-say ages are ineligible for capture. Tests cover these states. Preview installation/restart is only under applicable owner device authorization and after checks; release remains untouched.

- Most likely: a removed Next button remains a tour dependency. Mitigation: merge introduction with filters; serialized-scene gate/target tests.
- Most serious: Stop silences output but capture or stale voice tool calls continue. Mitigation: explicit stopped state, session-generation invalidation and negative-event tests before UI cleanup.
- Most underestimated: tour input locks can disable Play or environment toggles. Mitigation: test the actual filtered interactability and silent path, not pure step order alone.
- New requirements do not authorize enlarging the board, rebuilding lessons, changing voice providers, adding a profile field or resetting saves.

## Requirements trace

| Owner requirement | Phase | Named acceptance test |
|---|---|---|
| Remove bottom-right Back/Next and tour mentions | C, D | OnlyCatalogPagingPairVisible; IntroDoesNotRequireRemovedNext |
| Keep dashboard left/right arrows | C, D | PagingAdvancesExactlyOnce |
| Remove Back to lessons in all three lessons; keep X | C | AllLessonsUseSingleExit; ChapterProgressionStillReachable |
| Replace companion Mic/Music/Help with Play/Stop | B | StopSilencesAndEndsCapture; bar wiring checks |
| Stop/resume conversation without losing place | B, D | StopRejectsLateAudioAndTools; ResumeUsesCurrentTourStep |
| Teach Your room/Nerdy Lounge | D | EnvironmentSelectionPreservesTour |
| Teach conversation control | D | DeeStopDoesNotSkipTour; SilentTourRemainsCompletable |
| Preserve automatic tour after questions | D, E | TourAutostartTests |

## Handoff prompt

Implement this consolidated revision only after the owner authorizes implementation. Read current AGENTS/CLAUDE, this revision and research. Work on `lounge-onboarding`, preserving the dirty baseline and locked lesson mechanics. One operator controls Unity. Execute A through E with symbol impact review, focused regressions and real scene checks; do not ask the owner to rebuild/test every individual edit. Do not commit, push, reset saved data, replace the release app, upgrade packages, or introduce new UI surfaces without the corresponding authorization. When assumptions conflict with source behavior, record the conflict and choose the smallest safe adaptation; stop for scope/permission changes. Report implemented versus editor-verified versus headset-accepted distinctly, including golden drift. No implementation was performed by this planning pass.

---

# Historical repair plan — prior implemented iteration

The consolidated revision above supersedes conflicting control/tour instructions below. This section is preserved as history and evidence, not a second active execution plan.

**Mode:** revision · **Size:** M · **Tier:** Quick  
**Status:** Implemented; follow-up speech-sync repair passed 44 focused checks and live intro/gear/Done transcript validation. Owner-authorized preview APK installed and launched; headset acceptance pending. See `docs/qa/onboarding-repair-2026-09-18.md` for build identity, limits and the remaining golden-test discrepancy.  
**Baseline:** `lounge-onboarding`, `111049a` (host-tour checkpoint). Preserve remaining package/settings edits.  
**Research:** [research.md](research.md)

## Summary and brief

Repair the existing front door, not the lessons. The owner requested: “let's do the full walkthrough ... then we'll fix in one ... chunk” and “the only ... onboarding tour ... should start at the pick a lesson ... portion.” Deliver one integrated review build, using small verified internal changes rather than asking the owner to test every edit.

## Decisions

- **LOCKED:** Welcome has one **Let's begin** button; remove the two mic-choice CTAs. Its greeting describes that button, not questions that are not yet visible.
- **LOCKED:** Welcome → profile questions → Pick a lesson. No guided-tour overlay on welcome/questions. The latest catalog-only decision supersedes the earlier requested welcome pointer.
- **LOCKED:** On the catalog: introduce cards without opening → try a filter → practice page Next and Back → open Settings and return → choose a playable lesson to finish. Preserve Featured/Newest/Most viewed, navigation, gear, Skip and Replay.
- **LOCKED:** One highlighted target/action at a time, with matching speech and captions. An early card press must not silently end the tour; explicit Skip exits it.
- **LOCKED:** No automatic microphone capture from Let's begin. OS permission is not the app's listening choice. Retain an explicit mic/listening control and visible state; do not broaden permissions or send profile data to new services.
- **KEPT:** Existing lessons, compact layout, fonts, scene, voice provider and offline path. No board enlargement, new framework, dependency upgrade or new onboarding screens.
- **LOCKED — later owner addition:** Restore Dee's compact blue-glowing circular indicator inside the assistant bar. This supersedes the earlier remove-orb instruction, not the compact-layout requirement. Keep it anchored to the bar, never a separate floating sphere. Its feedback must reflect actual idle, listening, speaking, muted and unavailable states; it must never imply microphone capture is active when it is not. Reuse the prior appearance and available state bindings; no new voice functionality or automatic mic activation.
- **LOCKED — voice discoverability addition:** Teach both controller/mouse selection and the existing voice commands during the catalog tour. Show short examples such as “Open settings” and “Open Cargo Crew” beside the corresponding action. With explicit mic opt-in, either modality advances the same validated gate; without it, pointing completes the entire tour. Never require speech to proceed, and never treat recognized text alone as a successful action.
- **LOCKED — lesson navigation addition:** Inside a lesson, replace the shared Back/Next arrow pair with one **× / Exit lesson** control returning to Pick a lesson. This is a narrowly approved shared-navigation exception, not authorization to redesign or rewrite the locked lessons. Keep catalog paging arrows and settings navigation unchanged. Preserve lesson-owned progression controls; inspect whether any chapter currently relies on the shared Next arrow before hiding it so no activity becomes a dead end.

## Profile refinement — recommendation for owner review, not authorization

The owner asked to explore a more useful voice-or-manual profile, mentioning age, gender and interests. Existing `LearnerProfile` already stores an age band, interests and a goal; it has no gender field. Recommend retaining coarse age-band choices (including prefer not to say), optional interests, and a learning goal only where there is an actual downstream use; otherwise omit the question. Do not add gender: it has no defined learning requirement and must not be used to infer interests or ability. Do not replace age-band safety checks with a grade/skill preference.

Make the profile short and skippable, but do not let skipping imply adult status or microphone eligibility. This is a proposed refinement, not a locked schema change. Keep it out of the repair implementation until approved. The current adult-test-only restriction remains. Voice answers on the profile screen are also a separate gate: current mic policy intentionally excludes that phase, so this must not be enabled simply by adding example text. Any future approval must preserve consent/minor/privacy restrictions, show recognized choices for correction, validate existing fields and retain manual fallback. No new provider, personal-data collection, automatic listening or child-use clearance is authorized here.

## Assumptions — three reversible defaults

1. Complete profiles are not requested again; returning learners keep their saved choices (assumed — not in brief).
2. Explicit Skip suppresses future automatic tours but is recorded separately from completing every step (assumed — not in brief).
3. Existing seen-tour saves are preserved, not silently migrated or erased; use Replay to review the revision (assumed — not in brief).

## Drift

| Previous design / actual code | Revision |
|---|---|
| Six stops include welcome and questions | Tour starts only after reaching the catalog |
| First catalog stop invites opening; `OpenTile` calls `Skip` | Opening a valid lesson is the final gate only |
| Replay starts at hardcoded index 2 | Replay starts at the first catalog-only step |
| Questions and welcome share misleading answer-oriented greeting | Separate phase-specific copy, consistent Nerdy AI + VR identity |
| QA document describes an older tour | Rewrite QA against this flow; keep history labeled |

## Setup and guardrails

One agent owns Unity; Claude stays paused during implementation. Confirm branch/status before edits. Run GitNexus upstream impact before modifying symbols and report high-risk results. No commit/push, production deployment, saved-data wipe or headset replacement until requested. Do not rerun the locked legacy builders listed in CLAUDE.md. Stop Play mode before scene edits/builders; never save temporary desktop input/camera overrides into the Quest scene.

Run commands from the Unity directory with `UNITY_PROJECT_PATH` pointing to this exact project. Use `unity command editor_status --format json` before editor operations. A timeout does not prove the UI is frozen: if menus respond, use Unity's native UI rather than repeatedly queuing CLI calls.

Focused test invocation (repeat for the named suites): `unity command run_tests EditMode RundownTests testName false true 300 --format json`. Wait for actual completion/results; require nonzero discovered tests and zero failures, not merely successful dispatch. Existing completion polling is in `scripts/verify_cargo.py:69`. After the editor loop passes, `bash scripts/lounge-preview.sh` builds the separate preview package; first confirm recompile completed and the runtime DLL is fresh. Do not treat the script's APK existence check as proof of build/installation success: inspect the build result, install result and launched build identity.

## Phase 1 — Reproducible local walkthrough (Size: S)

**Touches:** existing editor preview utilities, QA evidence; add a small editor-only session helper only if required. No scene redesign.

- Establish responsive CargoCrew Game view, desktop mouse clicks and correct framing without altering runtime scene defaults.
- Add isolated editor fixtures for fresh profile, answered profile with unseen tour, completed tour, and skipped tour. Snapshot/restore test-owned state; no blanket PlayerPrefs or app-data deletion.
- Preserve a read-only snapshot of reported failures and identify actual active tour step, seen status and target visibility.

**Acceptance:** repeat fresh start and returning start in Game view without reinstalling an APK, without clearing real progress, and without repeated manual camera/input repair. Record actual timings, no promised latency.

## Phase 2 — Welcome and profile boundary (Size: M; depends on 1)

**Touches:** `NerdyDirector.cs`, `WelcomeFlow.cs`, `GuideIntro.cs`, `PatchConsentCopy.cs`, narrow `FixWelcomeLayout.cs` changes if necessary, CargoCrew welcome objects. Runtime scripts live under `unity/Assets/Airlift/Scripts/Welcome`; builders under `unity/AgentScripts`.

- Render one Let's begin CTA, matching greeting/captions and Nerdy AI + VR branding on both cards. Reuse established visual components; no new-surface mockup is needed unless layout scope expands.
- Begin goes to unanswered questions, or directly to catalog for an already-complete profile. Guard double clicks and stale voice callbacks.
- No pointer/tour state or competing navigation narration during questions. Normal Continue/Back remain; current Skip behavior stays explicit.
- Separate narration from listening; retain explicit opt-in access to the mic outside the removed CTAs. Guide/network failure cannot trap the learner.
- Restore the prior circular blue indicator and its reference/state binding. Update `unity/AgentScripts/CompactAssistantBar.cs` so it no longer disables the approved orb, and remove `RemoveOrb` from the active build sequence (preserve the historical script, do not execute it). Review its other effects separately: restoring the orb does not authorize re-enabling the controller-helper sphere. Keep captions/buttons readable and the orb nonblocking to ray/mouse input.

**Tests:** extend `WelcomeFlowTests` with `BeginDoesNotEnableMicrophone`, `QuestionsDoNotStartTour`, `CompleteProfileSkipsQuestions`, `RepeatedBeginAdvancesOnce`; add wiring/copy checks for one CTA and correct branding. Keep `GuidePolicyTests` green.
**Acceptance:** welcome → questions → catalog works with voice available or unavailable; no overlapping instructions or invisible click blockers.
**Orb acceptance:** visible in the compact bar; changes only with verified guide/mic state; quiet when muted/unavailable, with state text for clarity; no overlap or intercepted clicks; survives the supported scene rebuild. Add `OrbReflectsGuideStateWithoutEnablingMic` and a bar-wiring assertion; visually verify idle, speaking and opt-in listening, plus mute/disconnect.

## Phase 3 — Catalog-only action-gated tour (Size: M; depends on 2)

**Touches:** `RundownScript.cs`, `LoungeRundown.cs` under Scripts/Lounge; `NerdyDirector.cs`; welcome `GuideTools` routing; `BuildTour.cs`, `BuildNavArrows.cs`; affected wiring/tests and CargoCrew serialized references.

- Introduce cards, continue; try any one filter; explicitly page forward then back; open gear and close settings; finally choose a playable lesson. Do not imply all three filters must be clicked.
- Define page-navigation gates separately from tour-advance gates so a Next click cannot both turn a page and skip a stop. Refresh pointer after filtering/paging/layout.
- Before the final step, block lesson entry consistently for mouse, controller and voice tool routes. Explicit Skip restores normal access. Coming-soon tiles never complete the final gate.
- Explain voice alongside each relevant manual action, not as a separate compulsory tour. Verify supported phrasing against the existing tool allowlist. If a learner asks to open a lesson before the final gate, explain how to Skip first; never silently abandon the tour. After Skip or on the final step, the existing direct-open command works normally. Offline or mic-off state must not invite speech as though it were available.
- Final opening advances once, enters the chosen lesson, and records completion only after entry succeeds. Settings closing returns to the correct tour step. Back/B behavior must match the current instruction rather than silently skipping when Back is the required practice action.
- Replay starts the same catalog flow even with seen state set; no profile replay or duplicate listeners. Version the diagnostic state/copy, not the learner's answers.
- When a lesson is active, show only the shared × / Exit lesson control instead of shared Back/Next. Route it through the existing safe return-to-catalog action (also used by controller B); if a piece is held, refuse exit with a clear release-first cue. Restore catalog controls on return and preserve saved progress according to existing behavior. Use a generous hit target and the text label, not an unexplained tiny glyph. Verify chapter progression still works without the shared Next arrow before applying the scene change; if a lesson lacks its own progression affordance, report that specific lock conflict rather than silently removing access.

**Tests:** replace obsolete six-stop assertions in `RundownTests`; add `TourBeginsOnlyOnCatalog`, `EarlyLessonOpenDoesNotEndTour`, `PageNextAndBackHaveDistinctGates`, `SettingsRoundTripResumesTour`, `FinalPlayableLessonCompletesOnce`, `ComingSoonDoesNotComplete`, `ReplayWorksWithSeenState`, `SkipUndimsAllControls`. Extend `RundownWiringTests` for every pointer target and `FrontDoorToolTests` for voice-route parity.
**Acceptance:** one uninterrupted Game-view walkthrough sees every agreed control before entering a lesson; Skip and Replay also work; muted/offline guidance stays usable.
**Dual-mode acceptance:** one manual-only pass and one explicitly opted-in adult voice pass reach the same states; voice settings/filter/lesson actions advance only after execution succeeds, duplicate requests advance once, unsupported requests do not advance, and denied/unavailable mic still permits completion. Add `VoiceAndManualActionsShareTourGates`, `VoiceCommandFailureDoesNotAdvance` and `MicOffTourRemainsCompletable` to the relevant routing suites.
**Lesson-exit acceptance:** `LessonShowsExitInsteadOfSharedArrows`, `ExitReturnsToCatalog`, `ExitWhileHoldingRequestsRelease`, `CatalogPagingRestoredAfterExit` and `ChapterProgressionStillReachable` cover all three lessons using the shared presentation/routing boundary. Exit is not app shutdown, saved-data deletion or a lesson reset.

## Phase 4 — Integrated acceptance and handoff (Size: S; depends on 3)

Run the focused suites plus `LoungeWiringTests`, `WelcomeFlowTests`, `GuidePolicyTests`, `FrontDoorToolTests` and `CargoVoiceCharacterizationTests`. Drive the real serialized scene, not only pure state-machine tests. Test fresh, returning, skipped and replay sessions, including rapid repeated input and stale narration after a transition. Retest lesson entry/return without modifying lesson internals.

Update `docs/qa/lounge.md`, CLAUDE.md and DEV-LOG with exact build, tests and remaining issues. Then make one preview APK for an owner headset walkthrough: controller pointing, arrows/readability, audible sequence, gear/page gates and final lesson entry. Desktop success is not headset acceptance. Preserve the separate release app and rollback artifact. Do not mark complete until the owner confirms the integrated flow; no commit unless requested.

## Challenge — self-review

- “Latest build means fresh onboarding” → saved seen state hides the tour → isolated fresh/returning fixtures and explicit Replay acceptance added.
- “Removing mic buttons is cosmetic” → starting the lesson unintentionally enables listening → Begin must leave capture off; explicit mic control and policy tests retained.
- “Reordering text repairs progression” → shared Next/Back or voice routes still skip gates → separate navigation events, serialized-scene checks and all entry-route tests added.

## Handoff

Implement this plan on `lounge-onboarding` only after owner approval. Read current CLAUDE.md, this plan and research. Preserve unrelated edits, locked lessons and release app. Execute phases in order with one Unity operator; use no parallel editor control. Show impact before code edits, reproduce each failure before fixing, and record actual tests and device evidence. If a new visual surface is required, obtain the required mockup approval first. Do not commit, push, reset learner data or deploy production services. Report fixes, test results, owner-QA needs and deviations; do not claim an attempted editor command succeeded without verification.
