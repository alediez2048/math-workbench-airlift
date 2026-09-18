# Onboarding repair — editor review checkpoint

## Latest implementation — control cleanup (installed; headset acceptance pending)

Owner authorized implementation after the consolidated plan. Source and CargoCrew scene changes are saved.
133 focused EditMode checks pass across ControlCleanup (14), ControlCleanupWiring (2), Rundown (5),
RundownWiring (5), OnboardingRepair (5), TourSettingsSpeech (1), TourAutostart (4), LoungeWiring (11),
Recovery (4), FrontDoorTool (7), TourSpeechRegression (6), GuidePolicy (10), PromptGate (8), Café wiring (25),
and Garden wiring (26). This is focused verification, not the full repository suite; the previously documented
Cargo voice golden differences have not been rewritten or resolved.

- `ApplyControlCleanup` is the final presentation pass. Retired controls retain legacy template/callback
  references beneath inactive wrappers; old lesson refresh cannot revive visible or raycastable exits.
- Stop closes the old voice transport, clears capture/playback/queues and invalidates old event generations.
  Play reseeds current UI/lesson context. Unknown/undisclosed ages do not enable microphone capture.
- The real Play-mode fresh fixture passed `CheckControlCleanupRuntime`: questions → automatic tour;
  filter → next page → previous page → changed scenery → gear → Done → final lesson; early entry blocked;
  all three lesson X exits return to catalog; Replay and Skip work; manual progress stays silent after Stop.
- Initial synchronous harness attempts ran before startup or before LateUpdate, reporting invisible controls.
  Corrected harness waits for visible welcome and yields frames for phase-driven visibility. No production
  visibility patch was needed. A 4-unit companion-control spacing mismatch was corrected and its test rerun.
- Native Unity screenshot confirms the compact Stop + gear bar, filter highlight, and absent corner controls.
  Headset reach/readability and actual controller interaction remain owner acceptance items.
- Live voice resume reached Live with current tour narration verified, same tour step, and microphone off
  for the skipped-age fixture. Stop also preserves written/manual progress.
- Final owner request: labels are simply Play/Stop in both companion and Settings; microphone disclosure is
  adjacent text, not part of the button label. Compiled/applied and 37 affected checks rerun successfully
  (ControlCleanup 14, ControlCleanupWiring 2, Rundown 5, RundownWiring 5, LoungeWiring 11).
- Integrated APK `artifacts/lounge-preview/nerdy-lounge-control-cleanup-20260918.apk` built successfully,
  zero errors and 367 warnings (not a warning-free build). Bounded credential scan passed; SHA-256
  `4115cf7f46e90e61b53e20615cbc14bd39e8e65c72f06027292a647e670c47bb`.
  APK package independently checked as `com.nerdy.vr.lounge`; install -r returned Success and cold launch
  returned Status ok. Real saved profiles preserved; separate release app untouched; prior autostart APK
  retained as rollback. Build identity restored to release settings afterward. No commit/push.
  Headset validation remains pending, including controller presses, physical exit reach, voice Stop/Play,
  actual microphone permission states and caption readability. Use Settings → Replay if saved tour is seen.

## Latest owner headset walkthrough — notes only

**Consolidated plan:** `docs/plans/2026-09-18-onboarding-repair/plan.md` (current revision at top).
The requests below were captured before implementation; see the latest checkpoint above for current status.

- **Tour coverage for the new conversation control:** introduce Dee's **Play / Stop** control before final lesson selection, with matching highlight, spoken explanation, and persistent written guidance. Explain that Stop silences Dee and stops listening; Play resumes the conversation. If the learner stops Dee during this step, keep the written resume instruction and tour controls usable without voice, preserve their place, and do not automatically resume speech or microphone capture. Remove references to the retired Mic / Music / Help controls. Planned only.

- **Requested assistant control simplification (supersedes repairing the three buttons):** remove **Mic off**, **Music on**, and **Help** from the AI companion bar. Replace them with one stateful **Play / Stop conversation** control: Play starts or resumes interaction with Dee; while active, show a square Stop control. Stop must immediately silence current speech, cancel pending responses, stop microphone capture/transmission, and prevent delayed responses from restarting audio. Play must preserve the current lesson/tour position rather than restart onboarding; microphone use remains subject to explicit permission and existing eligibility safeguards. Keep visible state feedback and accessible labels alongside the blue indicator so listening/stopped states are clear. This is conversation control, not a music toggle; preserve separate settings and do not silently change background music. Verify click and controller behavior, rapid stop/resume, permission denial, and stale-response suppression. Planned only; no implementation during this walkthrough.

- **Requested lesson exit simplification:** remove the **Back to lessons** button from **Cargo Crew**, **Community Garden**, and **Neighborhood Café**. Keep the top-right **X — Exit lesson** as the single visible return-to-catalog control. Verify the X works throughout each lesson, returns to Pick a lesson, and preserves the existing exit cleanup/progress behavior; keep activity-specific controls intact. Update any guidance referring to the removed button. Planned only; no app changes during this walkthrough.

- **Requested navigation simplification:** remove the bottom-right **Back / Next** pair and its onboarding steps/instructions. Keep the **left / right arrows directly beneath the dashboard** as the catalog paging controls. Owner reports the two pairs feel duplicative; their underlying behavior has not yet been compared. Update tour progression, narration, highlights, and completion checks so no step depends on the removed pair, including the introductory Next prompt. Retain onboarding coverage of the surviving paging controls. Verify the tour remains completable and lesson navigation is preserved. This supersedes earlier instructions to teach the bottom-right pair. Planned only; no app changes during this walkthrough.

- **Requested tour addition:** introduce **Your room** and **Nerdy Lounge**. Owner reports these controls are absent from onboarding. Add a guided environment-choice step before final lesson selection, with matching narration and highlights explaining each mode. Confirm both controls work and that switching environments preserves the current tour step and progress. This is a missing explanation, not a confirmed broken-button report. Planned only; no implementation during this walkthrough.

- **Confirmed by owner:** catalog tour started automatically on the installed autostart preview.
- **Open, owner-reported:** pressing **Mic off** produced no apparent effect.
- **Open, owner-reported:** pressing **Music on** produced no apparent effect.
- **Open, owner-reported:** pressing **Help** produced no apparent effect (voice transcript rendered the label as “health”; exact label not independently reconfirmed).

Reported during the onboarding walkthrough; precise tour stage, enabled/disabled appearance, and whether
labels changed were not established. No root cause or fix claimed. Expected: available controls produce
their stated action and clear feedback; intentional unavailability must be apparent. Owner explicitly
requested notes only: do not interrupt the walkthrough, change the app, or restart to investigate these reports.

## Verified locally

### Follow-up: automatic tour after questions (installed; owner confirms autostart)

Owner's headset log reported onboarding enabled, incomplete profile, and `rundownSeen=True` at startup.
The pure transition reproduced Welcome → Catalog while retaining that flag, suppressing TourDue.
Changed EndWelcome to clear the flag only when actually leaving questions; ShowCatalog also persists the
library flag as false. Finishing or skipping questions now requests the catalog tour; returning from a lesson
and a returning completed profile retain their previous behavior. Four new regression cases, WelcomeFlow 12,
OnboardingRepair 5 and Rundown 5 all passed (26 total). In the real fresh-learner scene, set the stale seen flags
in memory, answered all three questions through AnswerChip, and observed automatic Catalog / running step 0,
both completion flags false, microphone off. No Replay action. Follow-up APK:
`artifacts/lounge-preview/nerdy-lounge-tour-autostart-20260918.apk`, succeeded in 66 seconds, zero errors.
Credential scan passed; SHA-256 `d6a10f3e5de822b83bd5b2a9c1a83750684015a7a4c7eab1c6d7b8c5d5cf2f50`.
Installed successfully with data retained; preview lastUpdateTime 2026-09-18 15:27:32 (device time).
Launch request returned OK but Quest's controllers-required dialog took focus; owner must wake controllers
and continue before testing. Build settings restored. This supersedes the earlier tour-sync APK below.

OnboardingRepairTests 5/5; RundownTests 5/5; RundownWiringTests 5/5;
WelcomeFlowTests 12/12; GuidePolicyTests 10/10; FrontDoorToolTests 7/7;
LoungeWiringTests 11/11. Total: 55 passing checks.

Running CargoCrew with the fresh-learner editor fixture:

- Actual mouse click on Let's begin opened questions; tour/pointer and microphone capture stayed off.
- Skip questions opened Catalog at step `wall`.
- Scene Button callbacks progressed filters → page-next → page-back → gear.
- Opening Settings alone kept the gear step; closing it reached final lesson selection.
- Opening Cargo Crew at the final step completed the tour.
- In the lesson, Exit was visible; shared Back and Next were hidden. Exit returned to Catalog.
- Replay restarted at `wall`; an early valid Cargo Crew entry was blocked; explicit Skip ended the tour.
- Restored circle and welcome copy visually inspected in Game view. No headset performance claim.

## Known test discrepancy / not release acceptance

**Owner-reported voice drift — repaired locally, headset acceptance pending:** after filters/paging, the highlight points at the gear but Dee says
"Great, you've seen how to move around" and asks for a green Done button to pick a lesson. At capture time
the owner had already advanced to step 6; that later screenshot is not evidence the reported step worked.
The configured gear narration correctly says open Settings first, then Done. The reported wording is not
the scripted line. Unlike the first-step target bug, this needs speech/session diagnosis, not another arrow swap.
Code inspection: guide transcript callbacks unconditionally replace the caption, and audio/transcript events
are not associated with a tour-step identity. A stale response or generated deviation can therefore disagree
with the current highlight. Root cause between stale/in-flight audio, competing prompts, and model paraphrase
is not yet isolated. Next verification must capture requested step text against actual returned transcript/audio,
including rapid advancement; reject obsolete responses and preserve the authoritative current instruction.
Acceptance: gear highlighted → open Settings; only with Settings visible → Done; after Done → choose a lesson.
Do not claim spoken tour alignment based solely on gate/geometry tests.

Regression reproduced through the actual GuideSession event handler: a completed transcript arriving after
Hush still replaced the instruction. It failed before the repair and now passes. ScriptedNarration associates
isolated tour requests with response IDs, invalidates obsolete responses, and buffers at most 25 seconds of
PCM until a completed transcript matches the authoritative line (case/punctuation ignored). Off-script,
cancelled, malformed, oversized, and stale audio is discarded; the written instruction stays available.
Generic conversational media cannot replace the tour, while existing tool-call routing remains enabled.
After the tour, normal lesson conversation resumes. Help/Repeat within the tour repeat its current line.
Settings opening now changes the line and pointer to Done; Done then advances to lesson selection.

Implementation uses [OpenAI's documented out-of-band responses and empty input context](https://developers.openai.com/api/docs/guides/realtime-conversations).
No provider, model, key, microphone opt-in, or locked lesson-math change. Buffered narration adds generation
latency before speech begins; user can still click immediately. Rejection uses written guidance, not a false
claim of successful speech. Six event-level speech checks passed, plus Rundown 5, wiring 5, GuidePolicy 10,
GuideMessages 4, PromptGate 8, and OnboardingRepair 5. Settings substep test passed after fixing its
EditMode fixture to call Awake (44 total). Live adult preview, microphone off: provider transcripts/audio
for the expanded introduction, gear instruction, and Settings/Done instruction passed exact-line validation
before playback. Rapid real button callbacks advanced through filters/paging to gear; obsolete output did
not replace the caption. The gear pointer was visually on the gear. Opening Settings kept step 5 and changed
narration to Done; its real Done callback closed Settings and reached lesson selection. Final lesson-selection
speech was still pending at preview stop; do not claim the full spoken tour accepted.
This verifies transcript matching and playback routing, not subjective sound quality or headset latency.
The long introductory caption is cramped in the compact desktop bar; readability remains an owner review item.

Owner requested a fuller catalog introduction: welcome to the Nerdy AI plus VR math lounge, describe
hands-on fraction/division/multiplication adventures, explain the cards, then direct to highlighted Next.
Updated first-step narration; gate and pointer unchanged. Compilation and structural tests passed; actual audio/caption fit still needs live verification.

Owner also flagged the welcome's mouse reference as inappropriate for VR. Removed it from the spoken
English greeting; retained controller pointing/trigger wording. Added a no-mouse regression assertion.
This copy correction compiled and GuidePolicyTests passed; verify actual greeting audio at the next restart.

Owner walkthrough found a missed visual conflict: first catalog step pointed to Cargo Crew while Dee
requested Next. Live inspection reproduced `FirstTile` with `ContinuePressed`. The first target and wording
are corrected to the highlighted Next arrow, and the scene test now asserts the actual ring center and
interactability against each step's required control. This supersedes the earlier visual-readiness claim;
the owner did not accept the first tour presentation. Follow-up compilation completed without errors;
RundownTests 5/5 and strengthened RundownWiringTests 5/5 passed. Owner retest remains pending.

CargoVoiceCharacterizationTests is still failing against the historical golden. Do not silently regenerate it.
A structured diff found only welcome-caption differences, mic enabled flags, manual fallback-control
visibility, and five outgoing-message array differences attributable solely to audio-buffer-clear messages.
Other recorded lesson state and outgoing messages matched. The old golden assumes automatic mic availability
and hidden manual buttons; the repair intentionally changes those boundaries. A reviewed characterization
update is still needed before calling the entire suite green.

Actual spoken-command recognition, audio timing/quality, headset interaction, performance and final owner
acceptance remain unverified. Owner subsequently authorized the headset build/install. APK:
`artifacts/lounge-preview/nerdy-lounge-tour-sync-20260918.apk`; build succeeded in 94 seconds, zero errors,
367 warnings. Bounded credential scan passed; built IL2CPP metadata contains the new gear copy and narration
guard. SHA-256: `ec258031953b23acda2e336eb0efb22b92c6b1ec296437bc52b40535906db0cb`.
Preview update installed successfully with saved data retained. Package lastUpdateTime: 2026-09-18 15:19:54
(device time); cold launch returned OK. Release app preserved; preview build settings restored to
`com.nerdy.vr` / `Nerdy`. No commit/push. Use Settings → Replay the tour if previously completed.

## Owner walkthrough — Unity Game view

1. Welcome: one Let's begin button; greeting refers to it. Click it.
2. Questions: answer or Skip; no competing tour arrows. Branding says Nerdy AI + VR.
3. Catalog: introduce cards, Next, choose a filter, page right, page left, Settings then Done.
4. Only the final tour step opens a lesson. Check the arrow/caption/speech agree at each step.
5. In the lesson, use Exit lesson to return. Native chapter progression must remain available.
6. From Settings, Replay the tour; try Skip. All normal controls should become usable afterward.
7. Separately, with an adult test profile, explicitly enable Mic and test filter/settings/lesson commands
   at their corresponding steps. Check listening/speaking status; disable mic afterward.

Use Nerdy > Preview > Fresh learner after stopping Play to repeat without clearing the real profile/library.
Do not use Clear saved data during fixture testing: that legacy action is not yet isolated by the preview helper.
The fixtures suppress normal profile/library/settings saves, but are not a complete sandbox for destructive
settings actions or global preferences. Do not save temporary Play-mode camera/input overrides into the scene.
