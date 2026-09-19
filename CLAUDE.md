# Math Workbench: Airlift — instructions for Claude

## CURRENT STATE — September 18, evening (read this first; everything below is history in reverse order)

**Branch `docs-and-landing`** off `lounge-onboarding` (commits 456f8f3 Codex's onboarding repair, 1927bfd the
afternoon's headset fixes). Nothing pushed. The five editor-introduced files (Unity AI packages, build and
quality settings) are still uncommitted on purpose.

**On the headset** (`com.nerdy.vr.lounge`, build of 1927bfd): lounge arrival, *Let's begin*, three questions,
Dee's six-stop catalog tour ending on Cargo Crew, the wall, settings with Age pill and language reconnect,
lessons always in passthrough, three playable lessons. Mic gate: adult only, notice on the bar and caption.
Known red: `CargoVoiceCharacterizationTests` (golden not rewritten since Codex's welcome change).

**Repository layout (this evening):** root planning files moved to `docs/history/`; `docs/README.md` is the
map; `docs/WALKTHROUGH.md` and `docs/media/` are the pictures; the landing page source is `site/` (deployed
to https://nerdy-vr-landing.vercel.app from that folder, `vercel --prod`); `services/guide-proxy/README.md`
documents the voice server; MIT `LICENSE`. The demo MP4 is under `artifacts/demo/` (ignored) and attached
to GitHub release `v0.2-hackathon`. Check relative links before committing docs.

**Rules that still hold:** canvas or preview approval before anything reaches the headset or the live site;
short Unity turns; `export UNITY_PROJECT_PATH=…/unity`; stale-build proof line `[Nerdy] onboarding …`;
`GetComponent ?? AddComponent` never adds; `System.Math` inside `Airlift.*`; Cargo Crew locked; never rerun
`CreateNerdyWelcome.cs`, `CreateFractionChapter.cs`, `ApplyCargoStyle.cs`, `ApplyNerdyStyle.cs`,
`PatchCatalogCards.cs`; key only in `~/.config/nerdy/openai.env` and Vercel; adult testers; commit and push
only when the owner asks.

## CURRENT OVERRIDE — September 18 onboarding repair, local editor review

Latest update: owner confirms the installed tour starts automatically. New consolidated repair scope
is at the TOP of `docs/plans/2026-09-18-onboarding-repair/plan.md`: remove duplicate navigation/exits,
replace companion shortcuts with Play/Stop, teach scenery and conversation controls. Owner authorized
implementation after planning. Source and CargoCrew scene compile; 133 focused checks and the fresh-fixture
runtime tour/three-exit walkthrough pass. Live resume verified the current tour narration. Final Play/Stop
labels have a separate microphone notice. Integrated preview APK built, credential-scanned, installed and
cold-launched; not yet device-accepted. Previous APK/release app and saved data preserved. No commit or push. Earlier acceptance below does not
establish acceptance of this revision.

Owner approved the repair in `docs/plans/2026-09-18-onboarding-repair/plan.md`.
The earlier two-button welcome, removed orb and welcome/questions tour below are historical.
Current local scene: one Let's begin CTA; questions without tour; six catalog-only stops
(cards, filters, page forward, page back, settings + Done, final lesson selection).
Dee's compact circle is restored inside the bar, with explicit Mic control. No automatic
capture from the welcome. Shared lesson arrows are replaced by Exit lesson; native chapter controls remain.
Do not rerun RemoveOrb or the old builder chain. Locked lesson mechanics remain untouched.
Fresh/answered/returning/skipped editor presets are under Nerdy > Preview; desktop overrides
are Play-only. QA evidence and remaining limitations: `docs/qa/onboarding-repair-2026-09-18.md`.
55 focused checks passed; the strict Cargo voice golden remains failing on approved greeting,
mic policy and manual-control visibility differences. Golden was not rewritten. Follow-up speech-sync repair
passed 44 focused checks, with live provider verification of intro/gear/Done. Tour audio now uses isolated,
response-correlated requests and validates the complete transcript before playing; Settings/Done is a
separate substep. Owner-authorized `nerdy-lounge-tour-sync-20260918.apk` built, credential-scanned,
installed over `com.nerdy.vr.lounge` and cold-launched successfully. Data/release app preserved; replay the
tour from Settings if previously completed. Subsequent autostart repair now clears old tour completion
when leaving questions, while preserving ordinary lesson returns. 26 focused checks and real editor
question callbacks passed; `nerdy-lounge-tour-autostart-20260918.apk` built, scanned and installed.
Launch awaits Quest's controllers-required dialog. Build identity and limits are in the repair QA record.
No commit, push or headset acceptance. Preserve other uncommitted package/settings work.

## CURRENT STATE — September 18, 2:15 PM (read this first; older dated sections below are history)

**Branch `lounge-onboarding`** (off `unity-airlift` at d5ff895; `unity-airlift` still holds the submission state and
the release app `com.nerdy.vr` on the Quest is build 124258, untouched). Deadline today 18:00 CDT.

**Owner process rule (2026-09-18):** every new surface is mocked up on the canvas first
(https://claude.ai/artifact/JPTeoWGosWPJqyP2JNACgf) and approved there; nothing goes to the headset before that.
Keep Unity turns short (builder → one render → report); the owner will interrupt long chains.

**What the app does now (all on the headset as `com.nerdy.vr.lounge`, owner testing):**
1. Arrival in the lounge (logo + loading bar on the whiteboard).
2. Welcome card: *Meet Dee, your Nerdy AI assistant*, two pills *Turn on the mic* / *Continue without voice*. No
   OpenAI mention (owner). Dee's bar is compact (`NerdySpace.BarWidth` 900 x 84, Music · gear · Help packed at 12);
   **the orb is deleted** (owner: "no floating sphere anywhere"; `RemoveOrb.cs`).
3. **The host tour** (`docs/superpowers/specs/2026-09-18-host-tour-design.md`, approved): Dee shows the learner
   around, six gated stops, an overlay on the normal screens — welcome pills → the questions' › arrow (lit once all
   three rows are answered; the questions' own Skip pill is dimmed) → first tile → filters → gear (then Done) → ‹ or B.
   Everything but the target dims and cannot be pressed; "N OF 6" + Skip top-right on every screen; Dee says each
   line word for word. Runs once (`LibraryState.RundownSeen`); *Replay the tour* in settings starts at the wall stops.
   **Lessons are untouched** (owner: "onboarding is strictly for how the app works").
4. The wall (24 tiles, filters, pages, scenery), the compact bar with the gear, settings, chapter deep-links.
5. **‹ Back / › Next** circle actions in the board's bottom-right corner on every screen (`BuildNavArrows.cs`,
   `GuideTools.BackButton/NextButton`): ‹ closes settings, skips/finishes the tour, leaves a lesson (never with a
   piece held), backs the questions up; › moves the tour on, turns pages, next chapter once accepted. Dimmed, never
   hidden. B does what ‹ does.
   Switches: `NerdyDirector.OnboardingEnabled` (true), `LessonHintsEnabled` (false: no helper inside lessons).

**Builder order for the welcome side** (after the lesson builders and `PolishCards`): `AddLanguageChoice` →
`BuildLounge` → `BuildSettingsPanel` → `BuildSettingsExtras` → `BuildRundown` (helper only; its card/block are
removed by BuildTour) → `BuildDashboard` → `BuildNavArrows` → `PatchConsentCopy` → `ApplySpatialStandards` →
`FixWelcomeLayout` → `CompactAssistantBar` (last: bar order + orb off) → `BuildTour` → `RemoveOrb`. Then **look**
at `PreviewWelcomeBoard.cs` renders (`artifacts/lounge/welcome-*.png`, incl. `welcome-tour-1/2/3/5/6.png`).

**Three traps that each cost 20+ minutes today:**
- **Stale player builds.** `unity command build` packages whatever the editor last imported. Wait until
  `recompile_status` says `completed` AND the DLL mtime moved, then pause 5 s, then build. Proof on device:
  `[Nerdy] onboarding on/off …` is logged at Start; if that line is missing from logcat the build is stale.
- **A second Unity project open** ("Setup Guide In-Editor Tutorial") makes every CLI call ambiguous: always
  `export UNITY_PROJECT_PATH=/Users/jad/Desktop/math-workbench-airlift/unity`.
- **Play mode** in the editor breaks every builder ("cannot be used during play mode"): `unity command editor_stop`.
  The pipeline also drops for ~30 s after a scene save; poll `recompile_status` until it answers.
- Also: `GetComponent<T>() ?? AddComponent<T>()` never adds (Unity fake null) — use `if (g == null)`.
  `Math.Max` inside `namespace Airlift.*` is `Airlift.Math` — write `System.Math`.
  A card instantiated after Dee's bar covers it and eats the gear press: the bar is always the last sibling
  (`CompactAssistantBar`, and re-asserted in `ShowPhase`).

**Tests for the front door** (registered in `scripts/cargo-milestones.json`): WelcomeFlow, Rundown, RundownWiring,
FrontDoorTool, Dashboard, DashboardWiring, StationOpen, SettingsPanel, ToolNameSync, FrontDoorStyle, LoungeRoom,
LoungeWiring, LoungeStore, SpatialStandard. **Not re-run after the host-tour rewrite** (the owner chose to build and
test on the headset instead): run RundownTests, RundownWiringTests, FrontDoorToolTests, WelcomeFlowTests,
LoungeWiringTests and CargoVoiceCharacterizationTests first thing. Runner: `bash ~/.claude/jobs/<job>/tmp/runsuite.sh`
pattern (delete `unity/Temp/pipeline_test_status.json`, run, poll that file). QA script: `docs/qa/lounge.md`.

**Device workflow:** `bash scripts/lounge-preview.sh` (build + install + launch). Brand-new user:
`adb shell pm clear com.nerdy.vr.lounge` then launch. adb is at `~/Library/Android/sdk/platform-tools/adb`.
Presses and Dee's tool calls are logged (`[Nerdy] UI ray select`, `[Guide] tool <name> phase=…`).

**Non-negotiables (unchanged):** Cargo Crew stays stable beyond the approved exception; never rerun
`CreateNerdyWelcome.cs`, `CreateFractionChapter.cs`, `ApplyCargoStyle.cs`, `ApplyNerdyStyle.cs` or
`PatchCatalogCards.cs`; math is deterministic C#; adult testers only; commit when the owner asks; push only when asked.

**Voice server:** Vercel `nerdy-guide-proxy` (`https://nerdy-guide-proxy.vercel.app/session`, `x-nerdy-client`
header + nonce); key only in its production env; redeployed 2026-09-18 with the four wall tools.

**Open owner items:** headset verdict on the host tour; the six coming-soon names; perf run (release gate); revert
`insecureHttpOption`; launcher icons; shadow-vs-style-guide; BLUETOOTH permission; demo footage; commit is on
`lounge-onboarding` only, nothing pushed.

## September 17, 12:00 AM: Café + Garden plan approved; six-agent team building

Plan: `/Users/jad/.claude/plans/agile-wibbling-nebula.md`; contracts: `docs/00-build/CAFE-GARDEN-CONTRACTS.md`;
tickets CC-PL-*, CC-CF-*, CC-GD-* in `docs/00-build/TICKETS.md`. Cargo Crew stays locked: CargoLessonDirector,
CargoLessonModel, CargoChapter, OnboardingDirector and BuildDockWorkbench must not change; the golden Cargo voice
JSON must stay identical through the platform refactor. Only the main session runs Unity.

## September 16, 10:40 PM: round 2 committed (2740d8a); lock items L-2/L-3/L-6/L-7/L-8 done; planning next

Status and remaining owner items: `docs/00-build/CARGO-CREW-LOCK.md`, `docs/00-build/RELEASE-GATES.md`. Build
223313 on the Quest carries the uncommitted lock changes (pause gate, headset-off pause, readout off). Next:
plan mode for Neighborhood Café (division) and Community Garden (multiplication).

## September 16, 10:15 PM: owner LOCKED Cargo Crew; finish its lock checklist, then plan the next two lessons

Owner accepted the Dock 7 lesson (commit 53647f9) and decided: lock Cargo Crew, complete the remaining work for
it (`docs/00-build/CARGO-CREW-LOCK.md`), then enter planning mode for Neighborhood Café (division) and Community
Garden (multiplication). This supersedes "no third lesson / preview cards do not authorize lessons" below; the
new lessons still need an approved plan before implementation. Round 2 (cranes, load into trucks) is in
integration.

## September 16, 8:30 PM: Phase 1R "Dock 7" built and installed (owner headset test pending)

Owner authorized implementing all three steps of `docs/00-build/PHASE-1R-DOCK-CREW.md` with an agent team
(contracts: `PHASE-1R-CONTRACTS.md`). Build cargo-20260916-202453 (146 checks, 23 suites) is on the Quest:
voice-first story card, 7 new voice tools, five chapters (whole, halves, quarters, 2/4 = 1/2, top-up), vehicles
and dispatch, Pause/Music double-listener bug fixed, `[Nerdy] UI` press logging. Do not rerun
CreateFractionChapter.cs, ApplyCargoStyle.cs or CreateNerdyWelcome.cs (they overwrite Dock 7 copy or HUD
fixes); BuildDockWorkbench.cs and PatchCatalogCopy.cs are idempotent. No commit until owner acceptance.

## September 16, 8:00 PM: owner accepted voice actions; Phase 1R "Dock 7" plan drafted, awaiting approval

Owner on build 190518: voice open and voice demo work; workbench buttons stopped working (not yet
diagnosed); assistant bar placement confirmed by device log. Owner wants a voice-first story card screen,
a dock story, and more chapters. Read `docs/00-build/PHASE-1R-DOCK-CREW.md` and wait for the owner's
approval before implementing CC-D tickets (start with CC-D-01, the button regression).

## September 16, 6:15 PM: Phase 0 "Nerdy welcome" — Pause/Play, music, HUD diagnostics built; owner retest pending

Read `docs/00-build/PHASE-0-NERDY-WELCOME.md` (plan), `docs/00-build/DEV-LOG.md` (newest entry first),
`docs/qa/nerdy-welcome.md` (headset script, steps 6b/9b/9c are new) before touching code.

**Newest since 5:30 PM (all uncommitted, awaiting owner acceptance on the headset):**
- Voice start (`advance_step` → onboarding.Continue) was in build cargo-20260916-173456 (installed 17:42;
  the owner has not run it yet: controllers-required dialog).
- Assistant bar above the workbench: owner did not see it on 172755. Editor drive of the exact runtime path
  renders it correctly above the lesson card (17° above eye level, 0.93 m); no code defect found. The new
  build logs `[Nerdy] HUD …` lines at lesson entry for device evidence; QA step 6b says where to look.
- Pause/Play pill on the bar (Hush + mic off + prompts blocked via `GuidePolicy`/`Say()`), music pill
  (`AmbientMusic`, synthesized 16 s loop, 15 %, ducks under speech and open mic, PlayerPrefs `nerdy.music`),
  `PromptGate` serialises response.create behind the active response (fixes the "active response in
  progress" warning after Hush → Prompt). Suites PromptGateTests/GuidePolicyTests/AmbientMusicTests under
  manifest ticket CC-P0-07. Scene edited by `AgentScripts/UpdateNerdyHud.cs`.
- Tooling: unfocused editor needs `AgentScripts/RefreshAndCompile.cs` before `recompile_status` means
  anything; wrapper via `bash scripts/verify-cargo.sh`; OnboardingFlowTests is the baseline (never pass it
  to `--suite`); PlayMode baseline runs standalone after a script-domain reload.
- Build: cargo-20260916-180603 (SHA-256 0ddf6fe1..., 86 checks, scan passed) installed 18:20 and launched; owner ran it 18:55: voice could not open Cargo Crew and the guide said "grab" during the briefing.
- 19:09 build cargo-20260916-190518 (SHA-256 253ebbac..., 92 checks): `open_lesson` tool + step-grounded guide
  (`GuideSteps`: step / on_table_now / can_grab_now in every lesson_state and tool result; diagnostics readout
  stripped). Mac dev mint restarted with the new tool. Owner retest pending. Quest log buffer set to 8 MB;
  capture Unity lines on the Mac during owner runs (`adb logcat -v time -s Unity:I > file`).

## September 16, 5:30 PM: Phase 0 "Nerdy welcome" is built and under owner iteration

Read `docs/00-build/PHASE-0-NERDY-WELCOME.md` (plan), `docs/00-build/DEV-LOG.md` (newest
entries), `docs/qa/nerdy-welcome.md` (headset script) and the task list before touching code.

**What exists now (all in `unity/Assets/Airlift/Scenes/CargoCrew.unity`, package `com.nerdy.vr`,
display name "Nerdy", version 0.2.0):**
- Consent card (adult testers, mic explanation, logo) → chip-driven welcome (age band, interests,
  goal; mic OFF here) → three Nerdy feature cards (Cargo Crew playable; Café and Garden are
  honest previews) → Cargo workbench with the toy look, carry handle and whole/halves chapter.
- Live voice guide: OpenAI Realtime (`gpt-realtime`) over WSS via `GuideSession`; half-duplex
  mic (never streams while the guide speaks); mic on only in the cards view and the lesson;
  short, prompted-only speech; tools `record_profile`, `end_welcome`, `describe_card`,
  `request_help`, `advance_step` (voice "yes/next" → same as pressing Begin briefing/Continue;
  Unity handler pending at the time of writing). Guide HUD (orb, captions, Again/Mute/Help)
  sits on the welcome panel and moves above the workbench in the lesson.
- Panel handle (lavender bar) carries/resizes the welcome and cards panel like the table handle.
- Nerdy design system: Poppins/Karla (OFL) TMP assets, `NerdyStyle.asset` tokens, generated
  pill/card/gradient sprites, TMP gradient preset `NerdySpectrum`.
- Proxy: `services/guide-proxy` (Vercel function, `node --test` 5/5) mints budgeted ephemeral
  client secrets bound to the server-authored persona/tools. NOT deployed: owner `vercel login`
  pending. Dev mint runs on the Mac (`~/.config/nerdy/start-dev-mint.sh`, LAN :8787);
  `GuideEndpoints.MintUrl` is that LAN URL and `insecureHttpOption=AlwaysAllowed` is a
  documented temporary exception (revert when the https proxy is live).
- Tests: 16 EditMode suites + 1 PlayMode (73 checks at 16:31; later builds 71 with the baseline
  suites evidenced standalone). Wrapper: `scripts/verify-cargo.sh --suite … --build
  --approved-dirty-build`; the PlayMode suite only passes right after a script-domain reload
  (task #12); poll `recompile_status` after `recompile` or editor assemblies stay stale.

**Owner-reported on device (build 171327, 17:20):** flow works end to end; asked for a bigger
consent card, rounded HUD corners, a handle for the panel, the HUD above the workbench, and a
voice-driven lesson start. First four are in the build compiling at 17:28; voice start is next.

**Open:** Vercel deploy (owner login), Library tile shows no logo (task #11), PlayMode runner
flake (task #12), consent heading wording (owner's request was garbled), P0-09 acceptance, then
resume fractions at P1-06. Not authorized: purchases beyond the approved OpenAI key, child data
processing, push, submission. Adult testers only until PRIVACY-GATE.md records a child-use go.
Key location: `~/.config/nerdy/openai.env` (never in repo/APK/docs).

**Commits today:** 2149adf halves loop · 147dbfb toy look · 38e3afd carry handle ·
31d0b1e Phase 0 plan. Everything after 31d0b1e is uncommitted pending owner acceptance.

## September 16 evening: table carry handle ACCEPTED by owner on headset

Owner approved Option 1: a yellow handle on the table's front edge. One hand carries the table
(yaw only, level on release), two hands resize it 0.5x-2x, both via Meta SDK transformers
targeting the station root. Refused while any piece is held. This supersedes the older
"place once then world-lock; recenter only with released pieces" line: the player may carry
and resize the table by the handle; it is still refused while a piece is held.
Owner found one-hand carry jumpy and un-turnable on build 142801; replaced with TableCarryTransformer (grab point stays in hand, table turns to face the player). Installed APK: artifacts/qa/cargo-20260916-143730 (SHA-256 ce1860e3..., 56 checks + scan). Second check felt identical; root cause is two active controller grab interactors per hand in the rig (one squeeze = two grab points). Extras disabled in CargoCrew. Installed APK: artifacts/qa/cargo-20260916-144706 (SHA-256 9f69a09c..., 57 checks + scan). Owner confirmed carry, two-hand rotate/resize and strap grabs on build 144706; committed. Next: quarters and equivalence chapters inside Cargo Crew.

## September 16 late afternoon: toy-look polish pass ACCEPTED by owner on headset

After the halves acceptance the owner asked for a centred measuring pad, removal of the
Raise/Lower/Recenter buttons, and a playful rounded "toy" look. Implemented in CargoCrew
only via generated RoundedBoxMesh assets, retuned materials, lighting, pill buttons and
layout changes; math, colliders and snapping unchanged. Previews: artifacts/toy-look-*.png.
Toy-look APK artifacts/qa/cargo-20260916-140241 (SHA-256 64904407...3a9ea, 48 checks + scan) is installed on the Quest (md5 verified 14:13) and awaiting owner review. Owner accepted the look on the headset (build 140241) and it is committed. Owner's next request before
more fraction chapters: in-app table move and resize (design pending owner approval). See DEV-LOG.

## September 16 afternoon: owner-reported grab + halves loop working; committed

Owner reported on the installed build 18273ac6: practice grab and manipulation works, and
the Cargo Crew halves chapter runs end to end (1/2 + 1/2 = 1 accepted). This is the first
owner-reported working arithmetic slice. Not yet known: whether grip or trigger produced
the grab (the temporary grip-or-trigger override and input readout are still in the build),
so the grip defect is mitigated, not root-caused. Owner authorized the commit of all
current work; no push authorized. Next: remove temporaries only after the readout answer.

## September 16 afternoon (earlier): grab diagnostic + fraction chapter installed

Installed APK is artifacts/qa/cargo-20260916-131831 (SHA-256 18273ac6...2732): startup
placement fix, temporary practice input readout, temporary grip-or-trigger practice grab
override, and the code-complete whole/halves chapter (P1-04/P1-05). Physical check script:
docs/qa/cargo-CC-P1-02.md. Test-runner stalls were a dirty-scene Save alert; the wrapper
now guards against it. Older paragraphs below are historical where they conflict.

## September 16 (morning): owner paused implementation for Claude Code handoff

Read `docs/00-build/HANDOFF-2026-09-16.md` FIRST for current evidence and unresolved
issues. CargoCrew practice grabbing is broken: target hovers but selection never
activates; sampled OVR grip readings are zero, not a confirmed input root cause.
The installed APK is now the INVALID VendorGrabProof diagnostic (platform visible,
cube missing; supplied cube lacks controller GrabInteractable). It is NOT CargoCrew.
No fix/device acceptance. Preserve dirty work. Latest CargoCrew rollback diagnostic:
`artifacts/qa/cargo-20260916-114658/airlift-cargo.apk`. No commit/push performed.
Owner deferred voice; next product objective is functional labeled whole/halves.
Older authorization and acceptance statements below are historical where conflicting.


## September 16 owner priority override

Owner explicitly deferred AI/spoken guidance and associated audio captions, replay,
and mute work. Preserve existing written instructions and fraction labels. Immediate
implementation priority: fix practice grabbing, then deliver the P1-04/P1-05 core
loop: open Cargo Crew, see a stable reachable board, grab a whole labeled 1, split
into two equal pieces labeled 1/2, and manipulate those pieces against a fixed whole.
P1-03/provider work is deferred and is not a dependency of this demo. P1-02.5 and
remaining visual polish must not block this core loop. This overrides historical
sequential/AI-first requirements below; deferred tickets are not accepted or complete.
No new purchases, child-data processing, commits, pushes, or submission authorized.
Physical grab/split acceptance remains required; contest AI requirements remain an
unresolved release consideration, not a reason to block local arithmetic development.

September 15 implementation authorization: owner approved **CC-P1-01 only** using
`docs/00-build/PLAN.md` and its ticket. Headset checks may be batched; mark completed
code awaiting device review honestly. Do not begin later tickets automatically.

September 15 owner update: core AI spoken guidance and the dedicated style guide are
now included in the draft review package: `docs/00-build/AI-VOICE-GUIDE.md` and
`docs/00-build/STYLE-GUIDE.md`. Earlier voice-deferred text below is historical for
this proposed revision. Finish owner review before implementing a ticket.

September 15 planning checkpoint: [Cargo Crew review package](docs/00-build/README.md)
contains the proposed narrated-fractions PRD and local phased tickets. **Implementation
is paused for owner review.** This draft does not silently replace the approved
baseline below. New progress is in [DEV-LOG](docs/00-build/DEV-LOG.md).

This is a five-day Unity/Meta Quest hackathon build. Deadline: Friday,
September 18, 2026 at 11:59 PM CDT. Internal submission target: 6:00 PM.

Read `docs/plans/2026-09-13-unity-vet/plan.md` and its `research.md` completely
before changing project files. They are the authoritative plan and evidence.
`docs/history/UNITY_PLAN.md` and the numbered Markdown files under `docs/history/` are retained design history.
If these concise instructions differ from the vetted plan, the vetted plan wins.

## Platform contract

- Native Unity application for the owned standalone Meta Quest 3S (ADB verified).
- Current compatibility proof: Unity 6000.6.0f1 Apple Silicon, Universal 3D/URP.
  This is a recorded deviation from candidate 6.3 LTS, not a proven release tuple.
- Unity OpenXR plus Unity OpenXR: Meta; do not add the deprecated Oculus XR
  Plugin.
- Use compatible Meta XR All-in-One/Core/Interaction SDK packages and lock their
  versions after the first successful device build.
- Use Meta Building Blocks for passthrough, rig, controllers, grab, ray, and
  haptics. Do not reimplement those systems.
- Controllers are required. Hands, voice, scene understanding, and automatic
  placement are deferred beyond the vetted execution plan.
- Place the workbench relative to the player once, then world-lock it; explicit
  recenter only with released pieces. No locomotion.
- Test every vertical slice on the physical Quest. Simulator success is not
  sufficient.

## Product contract

- P0 is one polished 6–8 minute flagship: The Cut.
- Cargo Grid is authorized only by the Wednesday gate in the vetted plan.
- Owner-approved opening: Arithmetic Lessons with Cargo Crew / Fractions plus
  disabled Coming soon cards for café/division and garden/multiplication.
  Preview cards do not authorize additional implemented lessons.
- No Till, third lesson, browser port, dashboard, accounts, multiplayer, flight
  simulator, runtime mesh slicing, or reward economy.
- “Cutting” swaps exact prebuilt segment prefabs; it does not modify mesh geometry.
- The airplane is narrative context and payoff, not a controllable vehicle.

## Pedagogical invariants

- Mathematical correctness is deterministic C#, never an LLM judgment.
- Every fraction refers to a visible equal whole.
- Equivalent fractions share the same length and number-line endpoint.
- Diagram and notation remain synchronized with the manipulatives.
- `RepresentationPhase` and `DifficultyTier` are separate concepts.
- No timers, lives, streaks, stars, generic praise, or punitive material loss.
- Feedback describes the task or strategy and leaves mistakes repairable.
- Neutral snap slots allow mathematically wrong arrangements; evaluate on Submit.
- Log only observable events. Never infer silent counting or gaze from controller motion.
- Report provisional proficiency, not mastery.

## AI and privacy

- The required AI behavior classifies a derived action trace and selects an
  allowlisted next scaffold.
- A serverless proxy owns the provider key. Never put secrets in the APK or repo.
- Validate responses against a closed schema and fall back locally after five
  seconds.
- Use one model provider. Hypotheses require matching observed evidence; ambiguous
  traces receive neutral support. Compare model routing against local rules.
- Collect no name, image, raw voice, account ID, or persistent device identifier.
- Use synthetic learner traces and entrant-only submission footage.

## Working method

- Setup/basic-interaction checkpoint is complete: safe APK installed; owner
  confirms passthrough, tracked controllers and grabbing. See `docs/qa/device-proof.md`
  for remaining Phase 1 QA; do not claim full acceptance or a completed lesson.
- Current slice is onboarding first: catalog, mission briefing, orientation,
  demonstrated grab/place, required practice and replayable help. See
  `docs/qa/onboarding.md`; arithmetic is not wired yet. Preserve DeviceProof.
- Work in `/Users/jad/Desktop/math-workbench-airlift`, branch `unity-airlift`.
  Read `docs/PROGRESS.md` for current state; do not repeat completed setup.
- Use Unity's official CLI/Pipeline for editor automation. Do not install a
  third-party editor bridge without a fresh authorization/compatibility review.
- Update the progress log after meaningful work and align this file, CLAUDE.md,
  the authoritative plan, and affected product documents when decisions change.
  Label verified, owner-reported, planned, and blocked states explicitly.
- Preserve historical documents as history. Record tests and device evidence,
  not claims that generated files demonstrate a working game.
- Implement vertical slices; do not create several partially working lessons.
- Preserve imported vendor packages and user changes.
- Keep custom architecture small: lesson director, task data, fraction math,
  pieces, snap zones, representation presenter, event log, tutor client, and
  world feedback.
- Write EditMode tests for fraction invariants, progression, local routing, and
  tutor response validation.
- Commit after each headset-tested slice with an outcome-focused message.
- Do not begin production art or stretch work before the device proof succeeds.
- No stretch feature survives while a P0 or P1 defect is open.

## Priority when time contracts

Reliable release APK > correct flagship lesson > visible AI adaptation >
legibility and world payoff > submission evidence > Cargo Grid > hands > voice.
# September 15 continuation

Owner authorized CC-P1-02; baseline APK built and scanned, physical acceptance pending.
Batch baseline device review with cargo-environment review. P1-02 is active; no paid
assets, commit/push or skipping owner font comparison implied.

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **math-workbench-airlift** (6732 symbols, 14273 relationships, 300 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> Index stale? Run `node .gitnexus/run.cjs analyze` from the project root — it auto-selects an available runner. No `.gitnexus/run.cjs` yet? `npx gitnexus analyze` (npm 11 crash → `npm i -g gitnexus`; #1939).

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows. For regression review, compare against the default branch: `detect_changes({scope: "compare", base_ref: "main"})`.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `context({name: "symbolName"})`.

## Never Do

- NEVER edit a function, class, or method without first running `impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `rename` which understands the call graph.
- NEVER commit changes without running `detect_changes()` to check affected scope.

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/math-workbench-airlift/context` | Codebase overview, check index freshness |
| `gitnexus://repo/math-workbench-airlift/clusters` | All functional areas |
| `gitnexus://repo/math-workbench-airlift/processes` | All execution flows |
| `gitnexus://repo/math-workbench-airlift/process/{name}` | Step-by-step execution trace |

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->
