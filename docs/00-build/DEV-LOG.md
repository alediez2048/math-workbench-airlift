# Cargo Crew development log

## 2026-09-17 evening — Guide proxy deployed to Vercel; the scene mints over https

Owner ran `vercel login` (account alediez2048), which unblocked the last piece of the AI/privacy contract that was
still running on the Mac. `services/guide-proxy` is deployed as project **nerdy-guide-proxy**, aliased at
`https://nerdy-guide-proxy.vercel.app`, with `OPENAI_API_KEY` stored encrypted in the production environment only
(read straight from `~/.config/nerdy/openai.env`; never printed, never in the repo or APK).

- **Route parity:** `vercel.json` rewrites `/session` to the `api/session` function, so the dev mint and production
  differ only by host. `.vercel/` is gitignored.
- **Proven live:** GET 405, missing `x-nerdy-client` 403, bad nonce 400, `language: "fr"` 400; real mints in English
  and Spanish return `gpt-realtime` and a secret that expires in 10 minutes. `node --test` 14/14 before deploying.
- **Scene:** `GuideSession.mintUrl` is serialised in CargoCrew.unity, so the `GuideEndpoints` constant was only
  documentation. Added `AgentScripts/SetMintUrl.cs` (idempotent) to bake it, and pinned it in
  NerdyWelcomeWiringTests: the scene URL must equal `GuideEndpoints.MintUrl` and start with `https://`. The
  assertion was watched failing against the old LAN URL before the bake.
- **Deliberately not done yet:** `insecureHttpOption` stays AlwaysAllowed for one build, so the Mac dev mint remains
  a fallback if https misbehaves on the device. Revert it to NotAllowed after the owner hears Dee over the deployed
  proxy on the headset. The installed build 124258 still mints from the Mac; the https path reaches the Quest only
  with the next build.


## 2026-09-17 afternoon — Concept intros, Dock 7 splitter, chunky practice crate, 2x café/garden pieces

Owner after build 112858 ("starting to look incredibly good"): more voice onboarding at lesson start (what
fractions are); the practice/demo crate did not match the lesson crates; a non-voice way to split loads; café and
garden pieces "3x". Owner decisions: intros in all three lessons; splitter station; **2x pieces keeping every
chapter's numbers** (3x did not fit the 1.3 x 0.8 m table). Contracts: docs/00-build/INTRO-SPLITTER-CONTRACTS.md.
Four agents on disjoint files (platform, cargo, café, garden); the main session integrated.

- **Intros:** ConceptIntros (Cargo 4 steps: whole, halves, sum, quarters; Café 3; Garden 3) shown once per app run
  before chapter 1; card heading/body/expression; Next/Start; nothing grabbable; other actions refused. Dee says each
  step word for word: button path via NerdyDirector (IntroNarration.ShouldNarrate), voice path via say_exactly in the
  tool result (proxy rule, node 14/14). Integration fixes (tests first): the tool reason is emptied when say_exactly
  carries the line, and say_exactly is sent only when the voice tool moved to a new step (a refusal during the intro
  no longer repeats the step).
- **Splitter:** pad 0.30 x 0.14 at front right with a console "Halves · 1/2" / "Quarters · 1/4"; SplitterRules
  (crate on pad, chapter split size, hint naming the needed size) then CargoLessonDirector.TrySplit; always visible;
  voice split unchanged. STAGING tag moved onto the staging platform edge.
- **Practice/demo crates** resized to 28 x 11 x 12 cm; tray/pad heights written to CargoOnboarding.asset.
- **Café 2x:** pastries 2x; boxes 2 x 3 pastries (one row), plates 0.22; props moved to the back half; served plates
  line up on the guest table; "Box N"/"Plate N" labels leave with their container (known cosmetic bug fixed).
- **Garden 2x:** strips and plants 2x for chapters 1-3 and the intro; the 7x6 and 8x7 beds use smaller cells (1.59x
  and 1.39x of the first build) because a 2x 8-row bed does not fit in front of the card; trays run front-to-back
  beside the bed; props moved.
- **Golden:** CargoVoiceCharacterizationTests script walks the intro (advance x4 plus two refused tools); re-recorded
  after proving 35 steps before and 58 after are identical (step numbers and call ids normalised) and only the intro
  block differs.
- **Evidence:** all builders rerun in order (Dock, Café, Garden, WireLessonStations, PolishCards, AddLanguageChoice);
  every affected suite green in the editor; renders artifacts/dock7/0-intro1..4, 0-practice, 2s/3s-splitter(-wide),
  artifacts/cafe/0-intro1..3 and chapters, artifacts/garden/0-intro1..3 and chapters.
- **Open:** grabbing disabled during the intro and the Next button path only verifiable in play mode/headset;
  splitter feedback text is small (about 1 cm); the owner should judge the smaller 7x6/8x7 garden beds.

## 2026-09-17 — Chunky crates, polished cards, voice language lock, Dee in the café story

Owner requests after build 104622: Dock 7 crates about twice as big; "hone in on the edges of the entire card
experience ... much more polished from beginning to end"; the voice guide kept switching from English to Spanish
("select a language ... and stick to only that language"). Owner also reported the mic not hearing them, then
"microphone is back to working" before diagnosis finished.

- **Chunky crates (owner chose "chunkier": 2x height and depth, length exact to the ruler):** crates 11 x 12 cm
  with labels twice as large (ChunkyCrate in BuildDockWorkbench resizes the existing whole/half crates in place and
  every rebuilt quarter); rest height BedTop + 5.5 cm; beds 13 cm deep; vehicles lengthened and cabs raised above the
  load; dock-edge strip moved forward; tray at z -0.30 (renders showed the taller crates hiding the 0, 1/4, 1/2
  marks at -0.26); DOCK EXIT beam raised to clear loaded crates; slide leads the turn and the exit moved out so
  the longer vehicles still never collide and fully leave the deck. DockWorkbenchWiringTests RED then green.
  Build cargo-20260917-110246 (325 checks, SHA-256 bea0cb99...) installed, md5 verified.
- **Card polish (evidence: artifacts/polish before-*/after-*, corners-zoom.png at Quest 3S pixel density):** causes
  were MSAA off with render scale 0.8 on the Quest pipeline asset (stair-stepped card corners), 64-96 px corner
  sprites, pill sprites whose corners exceeded the pill height (ovals), stencil masks (cannot anti-alias) on the
  catalog cards, badges, Allow voice and the assistant bar, square-cornered catalog art, a caption spilling over
  the bar's top edge, and lesson cards in a different flatter style. Fixes: MSAA 4x + render scale 1.0;
  scripts/make_card_sprites.py (supersampled card, capsule pill, gradient capsule, hairline stroke, soft shadow,
  rounded-top art, vertical fade); AgentScripts/PolishCards.cs (idempotent; ran twice to prove it) retires
  RoundedRect, sizes every pill as a true capsule, bakes gradient pills and removes all masks, adds stroke and
  shadow (CardShadowLink keeps a shadow visible exactly with its card, ExecuteAlways so previews match the device)
  to consent, welcome, catalog, the three lesson cards, and a stroke to the assistant bar; caption auto-sizes
  inside the bar. CardPolishTests 9 (8 RED first, masks found by the new test). Performance: MSAA 4x and full
  render scale cost GPU time; the Quest performance run (L-5) is still owed.
- **Voice language lock:** consent card row "Voice language · English · Español" (AddLanguageChoice.cs) saved in
  PlayerPrefs nerdy.language; the mint request carries language; proxy validates en|es and builds instructions
  with "Speak only <language> for the whole session ... Never switch languages" (replacing "English only, unless
  the learner clearly speaks another language first", the cause of the switching) and transcription language
  en|es; dev mint reads the body. Dee greets with the Spanish introduction in Spanish. Spanish letters added to
  the six Nerdy SDF atlases in place (AddSpanishGlyphs.cs). Proxy node --test 13/13 (RED first); GuideLanguageTests
  (font coverage RED first) and NerdyWelcomeWiringTests.ConsentCardChoosesTheVoiceLanguage. Dev mint restarted:
  builds without the row mint English-locked sessions. Card and lesson text stay English.
- **Dee in stories:** café briefing/chapter stories and café/garden catalog facts said "Nerdy" for the barista and
  gardener; now Dee (GuideLanguageTests.StoriesCallTheGuideDeeNotNerdy RED first; Cargo golden unchanged).
- **Mic report:** device state at the time: RECORD_AUDIO granted, AudioRecord active and not silenced for
  com.nerdy.vr, session minted and live; the mic is off by design on consent and the welcome chips. No defect
  found; the owner reported it working again. The next build logs "[Guide] mode/state/mic" transitions.

## 2026-09-17 — Owner bugs on onboarding and Dock 7: logo, Dee, trucks, Library

Owner report (headset, build 095611): (1) no Nerdy app in the Meta Library; (2) Nerdy logo gone from the first
screen; (3) the voice guide must present "Welcome to Nerdy AI+VR, my name is Dee and I will be your AI assistant
throughout your elementary math journey" (the story is important); (4) Cargo Crew is great, but trucks should load
in the middle and drive left to a dock exit, and look like trucks. Item 4 is an owner-requested change to the
locked lesson (vehicles and their departure only; math, chapters, copy and voice tools unchanged).

- **Library (1), not an app defect:** the APK is installed, launchable, and carries LAUNCHER plus
  com.oculus.intent.category.VR. It is the only sideloaded app on the Quest (installer null; every other app came
  from the Store via com.oculus.ocms). Horizon OS lists sideloaded apps only under the Library's Unknown Sources
  filter. A normal Library tile (and its artwork) needs a Meta developer release channel upload.
- **Logo (2), root cause:** the L-4 launcher-icon attempt re-imported Branding/nerdy-logo-green.png as a Default
  texture (commit 2740d8a, textureType 8 -> 0), which removes the sprite the consent card's Logo image references.
  NerdyWelcomeWiringTests.ConsentCardShowsTheNerdyLogo confirmed RED ("Expected Sprite, was Default"), then
  AgentScripts/FixLogoImport.cs restored the Sprite import: green.
- **Dee (3):** proxy persona is now "You are Dee, the AI assistant of Nerdy AI+VR" with exact-words handling
  (node test RED then green, 11/11); GuideIntro holds the spoken introduction, the greeting prompt and the no-voice
  caption; NerdyDirector uses them (GuidePolicyTests). The golden Cargo recording changed only by the no-voice
  caption: 62 lines, proven identical after substituting the old caption for the new one, then re-recorded.
  Dev mint restarted with the Dee persona. "elemental" in the request is read as "elementary".
- **Trucks (4):** new bodies in BuildDockWorkbench: semi (flatbed trailer + tractor with sleeper cab, hood, grille,
  headlights, exhaust stacks, marker lights, mirrors, 8 wheels), pickup (bed sides, tailgate, cab, hood, grille,
  roof light) and cab-over van; every vehicle has tail lights, plate and bumpers (new CargoRed material). Beds stay
  exactly the chapter's cell widths over the ruler. VehicleBay departure: LOADED tags, then leftmost first each
  vehicle turns left into the exit lane (station z -0.05, between the crate tray and the left crane base), drives
  under a new DOCK EXIT gate (x -0.40, striped posts, sign, lane chevrons) and disappears past the deck's left edge;
  crates ride in their beds. The idle truck now waits at the back of the dock. Tests first: departure poses and a
  no-collision sweep for every chapter (in memory), truck anatomy/length/height, and a lane sweep against cranes,
  containers, gate posts and the crate tray (scene) — scene tests RED before the builder ran, green after.
  Renders artifacts/dock7/*d-depart*.png and *e-side.png checked: arrows first pointed right and trailer wheels poked
  through the flatbed; both fixed before the build.

## 2026-09-17 — Three lessons integrated: platform, Café and Garden installed for the owner check

- **Golden first:** CargoVoiceCharacterizationTests recorded 95 Cargo voice steps before any routing change
  (deterministic on rerun). Platform refactor (LessonStation, CargoStation adapter, router, visibility, TableHandle
  across stations, lesson-prefixed tools) passed the strict golden and every Cargo suite unchanged. Two planned
  catalog re-records (café playable: 12 lines; garden playable: 8 lines) were each proven line-by-line to differ
  only by the catalog phrases before re-recording.
- **Integration fixes:** FakeLessonStation lived in the editor-only test assembly, so Unity refused AddComponent
  (moved to runtime behind #if UNITY_EDITOR); garden copy test matched "start" as "star" (whole-word match now);
  garden turn test rounded half-millimetre centres (tolerance match now); render-driven visual fixes (plate/box and
  row/part labels 3x, tidy trays, menu board placement, capacity tag off the label, bike crate, garden sign).
- **Cargo behaviour changes to recheck:** table carry refused while the practice crate is held; the guide may
  decline tools not in the card state; the Cargo card badge lost a thin outline (PatchCatalogCards).
- **Evidence:** editor drives of all 15 chapters through real drop/deal/turn/fence paths with renders in
  artifacts/dock7, artifacts/cafe, artifacts/garden. Wrapper: 319 checks across 36 suites, scan passed, 0 errors;
  APK cargo-20260917-095611 (SHA-256 b7c04284...), md5 a7044d66 verified on the Quest, launched 10:04. Proxy
  node --test 10/10; dev mint serving 20 tools (Mac alias 192.168.86.20 after a DHCP change).
- **Known cosmetic:** after café boxes ride away, "Box N" labels stay on the empty spots.

## 2026-09-17 — Café and Garden plan approved; six-agent team started

- Owner decisions: both lessons, five chapters each, by Friday 18:00; Café teaches share → pack → fact family;
  Garden teaches plant rows → turn the bed → split the bed; every lesson gets its own dedicated, custom workbench
  and colour scheme (café pastels/browns/coffee; garden greenery/trees/bushes); dedicated tickets from the template.
- Plan: /Users/jad/.claude/plans/agile-wibbling-nebula.md. Contracts: CAFE-GARDEN-CONTRACTS.md. Platform design
  reviewed against the code: LessonStation base + CargoStation adapter (CargoLessonDirector unchanged), router and
  visibility, workbench roots under the shared world-locked root, LessonTheme, lesson-prefixed voice tools with
  tools_now, golden Cargo voice characterization test before any routing change.
- Agents: platform (CC-PL-01..04, pauses for the golden recording), cafe-engine (CC-CF-01), garden-engine
  (CC-GD-01), cafe-bench (CC-CF-02/03), garden-bench (CC-GD-02/03), tickets (15 primers, TICKETS.md section, QA
  scripts). Main session integrates and is the only one running Unity.
- Pending owner: 2-minute check of build 223313 (Pause while the guide talks; headset off and on) so the lock work
  can be committed as the baseline.

## 2026-09-16 — Round 2 accepted and committed (2740d8a); lock work L-2, L-3, L-6, L-7, L-8

- Owner on build 221238: "looks good, keep going". Committed 2740d8a (detect_changes high: the round 2 model,
  director, layout and wrapper changes only; secret scan clean). Not pushed.
- **L-2 recovery (code done, headset check pending):** a voice tool call arriving while paused runs nothing and
  submits its result without requesting speech (new GuideSession.SubmitToolResult overload; the existing method is
  unchanged because impact analysis rated it CRITICAL: every voice action flows through it). Taking the headset
  off (OnApplicationPause) pauses the guide until Play; a learner's own pause is never undone. RecoveryTests 4/4,
  confirmed RED first. Leave-and-return test (mid-load and after an accepted load) passed on first run.
- **L-3:** practice-panel controller readout off (test RED then GREEN); grip-or-trigger fallback kept.
- **L-6:** RELEASE-GATES.md inventory from the APK: INTERNET, RECORD_AUDIO, MODIFY_AUDIO_SETTINGS and an
  unexpected BLUETOOTH permission (Unity adds it for Microphone; the OpenXR Meta step meant to strip it did not);
  endpoints; OFL font licenses present; all props generated in-project; open gates listed.
- **L-7:** proven again: baseline PlayMode suite passed inside the full wrapper run.
- **L-8:** 25 ticket rows in TICKETS.md updated.
- **L-4 correction:** Library tile logo still open (adaptive icon already Nerdy; default-icon attempt reverted).
- Build cargo-20260916-223313 (SHA-256 a77db7b4..., 170 checks, 26 suites, scan passed), md5 6fc7f816 verified on
  the Quest, launched 22:37. Uncommitted until the owner checks pause-while-talking and headset-off pause.
- Next: planning mode for Neighborhood Café (division) and Community Garden (multiplication).

## 2026-09-16 — Round 2 built: cranes, crates loaded straight into trucks that drive away (owner check pending)

- **Team round 2:** engine (BedCells {8},{4,4},{2,2,2,2},{4},{8}; Dock(id, held, bed); per-bed acceptance with
  vehicle-named feedback; 37 tests), workbench (vehicles back up to the dock edge with beds exactly over the
  0-to-1 ruler, dock-edge strip with 0/1/4/1/2/3/4/1 marks and ONE CONTAINER, "not needed" cells for chapter 4,
  DropAt picks the bed under the crate, DriveAway carries crates off the deck; aircraft, old truck and container
  floor removed; two cranes), voice (on_table_now describes vehicles and beds; proxy story adds cranes and
  trucks backed up to the dock; catalog facts).
- **Integration fixes:** a workbench test used Transform.Find for "Mark 1/4" ("/" is a path separator) and
  failed although the scene was right; test now matches child names. Renders showed tall crane jibs through the
  translucent lesson card and tray crates covering the dock-edge marks: cranes made compact and moved in front of
  the containers at the deck sides with jibs over the table edges; tray moved to z -0.225. Tests added first
  (crane height under the card sightline, crane in front of containers and clear of props, tray clear of the
  strip), confirmed RED, then GREEN.
- **Launcher icon (L-4) correction:** a default-icon change and its test passed but did not change the APK;
  Unity 6.6 packages only adaptive Android icons, which already use the Nerdy logo. Reverted; L-4 stays open.
- **Runner flake (L-7) fixed:** PlayMode suites now run before the baseline EditMode suite; the full wrapper
  passed CargoBaselineTestsSceneTests with no manual reload.
- **Evidence:** editor drive of all chapters through the real drop path (every crate into its bed, every load
  accepted with the right expression); renders artifacts/dock7/. Wrapper: 165 checks across 25 suites, scan
  passed, 0 errors; APK cargo-20260916-221238 (SHA-256 16e977c3...), md5 60e660e9 verified on the Quest, launched
  22:17; dev mint restarted with round 2 instructions. Not verified: drive-away timing in play, 7 cm vans on the
  headset, grabbing into beds on device.

## 2026-09-16 — Owner decision: lock Cargo Crew, then plan Café and Garden

- Owner: "We still have two more lessons to complete. Lock this lesson in, complete the remaining tickets for it
  to be in good shape and move to planning mode for the next two lessons."
- Wrote CARGO-CREW-LOCK.md: accepted-ticket bookkeeping, lock work L-1..L-8 (round 2, recovery/interruptions,
  temporaries, launcher logo, performance, release gates, runner flake, bookkeeping), owner items O-1..O-4
  (vercel login, consent heading, AI-requirement decision, footage) and recommendations for the remaining
  learning-activity tickets (fold P1-08 into the guide, defer P2-02/03/05/06, supersede P2-07).

## 2026-09-16 — OWNER ACCEPTED the Dock 7 voice-guided lesson on the Quest; committed 53647f9

- **Owner-reported (~20:50) on build cargo-20260916-202453:** ran the entire flow; "this is 100% exactly what I
  wanted from the beginning ... a fully immersive experience" with the voice assistant throughout.
- **Commit:** 53647f9 (215 files; not pushed). detect_changes rated the diff critical because it spans the lesson
  model, director and onboarding flows, which is exactly the accepted scope; secret scan hit only the proxy test's
  fake key string.
- **Next (owner request, round 2):** replace the aircraft with a couple of cranes; load crates directly into
  trucks in front instead of the central container floor, trucks drive away once loaded. Design: vehicles back
  up to the dock edge with beds side by side over the 0-to-1 ruler so the whole container stays visible
  (contract "Round 2" in PHASE-1R-CONTRACTS.md). Same three-agent team; integration by the main session.

## 2026-09-16 — Phase 1R "Dock 7" built by an agent team (owner authorized "implement all 3 steps")

- **Team:** three implementers on disjoint files against docs/00-build/PHASE-1R-CONTRACTS.md, no Unity access;
  main session integrated. Engine: CargoChapter (5 chapters) + chapter-driven CargoLessonModel (22 + 6 tests).
  Workbench: CargoLessonDirector reworked (piece pool incl. 4 quarters, locked half, story card, Try* actions),
  VehicleBay (truck, 2 pickups, 4 vans, LOADED tag, roll-out, dispatch recap), UiPressLog, builder
  AgentScripts/BuildDockWorkbench.cs, DockWorkbenchWiringTests (17). Voice: 7 new tools (replay_demo,
  split_cargo, check_load, reset_cargo, next_chapter, restart_chapter, back_to_lessons), handlers, fallback
  buttons (`GuidePolicy.ShowFallbackButtons`: hidden while the live guide listens), chapter grounding,
  proactive one-line story on button-driven chapter changes, Dock 7 onboarding/card copy, VoiceActionTests (10).
- **Button regression cause found (workbench agent, confirmed):** UpdateNerdyHud.cs (my earlier HUD change) wired
  Pause and Music persistently while NerdyDirector.Start also added runtime listeners, so each press toggled
  twice. Builder removed the persistent copies; a wiring test forbids persistent NerdyDirector listeners on HUD
  buttons. Lesson card buttons: no static cause found; the welcome canvas ray surface is now disabled during
  the lesson (possible press thief after carrying the table), and UiPressLog logs `[Nerdy] UI` enter/down/click.
- **Integration fixes:** director CanSplit used by the voice grounding; onboarding refusals/heading in crate
  language ("Dock 7 · Crew training"); baked Cargo Crew card text patched (AgentScripts/PatchCatalogCopy.cs);
  vehicles roll away from the learner (were rolling toward them), pinned by the wiring test.
- **Evidence:** first Unity compile clean; editor drive of all five chapters (AgentScripts/PreviewDockChapters.cs)
  split/load/accept with expressions 1, 1/2 + 1/2 = 1, 1/4 x4 = 1, 2/4 = 1/2, 1/2 + 1/4 + 1/4 = 1; renders in
  artifacts/dock7/. Proxy node --test 8/8; PlayMode baseline standalone 1/1 and CargoBaselineTests 3/3 after
  reload. Wrapper: 146 checks across 23 suites, scan passed, 0 errors. APK cargo-20260916-202453 (SHA-256
  abe100e7...), md5 da8fe54a verified on the Quest, launched 20:29; dev mint serving 13 tools. Scene backup
  before the builder: scratchpad scene-backup/CargoCrew-before-dock7.unity.
- **Not verified:** anything on the headset (voice reliability for the new tools, grabbing quarter crates,
  vehicle roll in play, fallback buttons toggling with Mute/Pause, text fit on device). Owner test next.

## 2026-09-16 — Owner check of 190518: voice actions work; buttons stopped; story and chapters requested

- **Owner-reported (~19:50):** "start the Cargo lesson" by voice opens it and "show me the demo" runs the
  demo. The buttons on the workbench screen stopped working. Owner wants the screen to show basic
  instructions and story, with actions by voice (restart demo, next lesson, questions), a dock-worker
  story about choosing the right amount of cargo per container, and more chapters (halves into
  fourths, adding cargo so it fits).
- **Device evidence (Mac capture of the run):** `[Nerdy] HUD at lesson entry` shows the assistant bar under
  the station canvas, active, 1.30 m from the head, 13 degrees up, facing the learner: the bar placement
  defect from 172755 is resolved on device. Two "[Guide] Cancellation failed: no active response found"
  warnings (Hush with nothing playing; harmless, the gate frees itself). No exceptions from app code.
  Button regression not yet diagnosed: the log has no pointer events; ray surfaces are sized to their
  canvases (bar 920x104, card 920x470 canvas units), so an oversized surface is ruled out.
- **Plan drafted:** PHASE-1R-DOCK-CREW.md ("Dock 7" story, voice-first story card with fallback buttons
  while voice is unavailable, five chapters: whole, halves, quarters, 2/4 = 1/2, 1/2 + 1/4 + 1/4 = 1,
  dispatch; tickets CC-D-01..09 with a chapters-1-to-3 cut line). Awaiting owner approval; nothing
  implemented from it.

## 2026-09-16 — Owner check of 180603: voice could not open Cargo Crew; guide said "grab" with nothing to grab

- **Owner-reported (18:58):** asking the guide to start Cargo Crew from the three cards did nothing; after
  opening it by hand, the guide told them to grab the cargo strap while the lesson showed no orange strap.
  The Quest log buffer (256 KB) had already rotated the app lines out; buffer raised to 8 MB and a filtered
  Unity capture now runs on the Mac during owner runs.
- **Root causes (from code, reproduced in the editor):** (1) no tool could open a lesson; the guide's only
  tools were profile, welcome end, card description, help and advance_step, so a spoken "open Cargo Crew"
  had no action path. (2) On entry the app pushed the card's lesson overview facts ("you grab an orange
  strap…") and prompted the welcome before the step context existed, so the guide described the fractions
  chapter during the briefing step, where no strap is shown. (3) During practice the temporary controller
  readout changed the instruction text every frame and each change was pushed to the guide as context.
- **Fixes (build cargo-20260916-190518):** new `open_lesson` tool (cardId enum) handled by
  `GuideTools.OpenLesson`: opens only the playable card while the cards show, exactly like pointing at it,
  and returns the first step for narration; previews answer "coming soon". `GuideSteps` names each step,
  what is on the table and whether anything can be grabbed; every lesson_state context and tool result
  (advance_step, request_help, open_lesson) carries `step`, `on_table_now`, `can_grab_now` and the
  instruction without the diagnostics readout. Card facts are labelled "not the current step"; the step
  context is pushed before the welcome prompt. Proxy instructions: call open_lesson for open/start/play;
  never tell the learner to grab unless can_grab_now is true. Mac dev mint restarted 19:02 with the new tool.
- **Evidence:** proxy `node --test` 6/6; GuideGroundingTests 6/6 (confirmed RED on the missing API first);
  editor drive of the handler: cafe → "coming soon", cargo → Lesson/briefing/canGrab false, "yes" →
  orientation/canGrab false, a second open inside the lesson is refused. PlayMode baseline standalone after
  reload 1/1, CargoBaselineTests 3/3. Wrapper: 92 checks across 20 suites, scan passed, SHA-256 253ebbac...,
  md5 bfc34d05 verified on the Quest; installed and launched 19:09 (controllers-required dialog showing).
- **Still unknown:** whether the model reliably calls open_lesson/advance_step by voice; the owner's run of
  this build decides it. HUD-above-workbench and Pause/music checks from 180603 were not reported yet.

## 2026-09-16 — Voice-start build installed; HUD-above-workbench investigated; Pause/Play + music built

- **Installed 17:42:** voice-start build cargo-20260916-173456 (SHA-256 b4cd19c9..., md5 b98ebe89 verified on
  the Quest, 71 checks) and launched; the Quest showed the "controllers required" dialog (controllers asleep),
  so the owner's run of it is still pending. The device logcat had been cleared at 17:37, so no Unity log
  from the owner's 172755 check survived.
- **Assistant bar above the workbench (owner: not seen on 172755).** Editor reproduction with the exact runtime
  path (AgentScripts/InspectLessonHud.cs: Consent → Catalog → SelectCard, board placed with
  OnboardingPlacement.BoardPose for a 1.6 m head): hudRoot re-parents under `Onboarding workbench - world
  locked/Guide HUD canvas`, active, world scale 0.001, 0.93 m from the head, 17° above eye level, facing the
  head (dot 0.96), and it renders directly above the lesson card (artifacts/lesson-hud-from-head-captioned.png).
  The lesson card itself proves on device that RectTransform anchored positions under a plain Transform are
  honoured (its serialised m_LocalPosition.y is 0 while it renders at 0.46), so the y=0 vs 0.77 mismatch in
  the scene file is not the cause. No code defect found; root cause still open pending device evidence.
  Added: runtime log lines `[Nerdy] HUD at lesson entry` / `1.5 s later` (parent path, world pose, distance,
  elevation, canvas state), explicit local rotation/scale reset on re-parent, and the station canvas's
  serialised local position now matches its anchored position. QA script 6b tells the owner where to look and
  what to report.
- **Pause/Play (owner ask):** `TogglePause` on the bar: Pause = Hush + mic closed + every prompt blocked
  (all spoken requests now go through one `Say()` that checks `GuidePolicy.CanPrompt`); Play restores the
  phase's mic policy (`GuidePolicy.MicOn`: mic only in cards/lesson, never muted or paused) and the guide
  says one sentence about the current step. GuidePolicyTests 3/3.
- **"Active response in progress" warning:** GuideSession now routes every response.create through
  `PromptGate`: a prompt waits for the active response's `response.done` (a cancelled one reports done within
  ~100 ms; caps 1 s after a Hush, 6 s otherwise), Hush drops stale queued prompts and only sends
  response.cancel when a response is active, and a "no active response" error frees the gate. Tool results
  and user text use the same gate. PromptGateTests 8/8.
- **Task #14 background music:** `AmbientMusic` synthesises a 16 s mono loop at Awake (Cmaj7·Am7·Fmaj7·G6
  pads with raised-cosine edges so the seam is silent, seeded pentatonic plucks, peak-normalised to 0.6); base
  volume 0.15, ducks to 30 % while the guide speaks or the mic is streaming (`GuideSession.MicStreaming`),
  fades at 0.6/s, HUD pill "Music on/off", PlayerPrefs `nerdy.music` (on by default). AmbientMusicTests 3/3.
- **HUD layout:** five pills now fit the 920 px bar: Pause · Again · Mute · Help on the right (88 px), the
  music pill under the orb, caption 440 px (artifacts/nerdy-catalog.png). NerdyWelcomeWiringTests 3/3
  (new test checks the controls, the ride-along canvas height and that every pill stays inside the bar).
  Scene edit via AgentScripts/UpdateNerdyHud.cs (idempotent).
- **Tooling:** the unfocused editor did not import new scripts on `recompile` (assemblies stayed at 17:24
  while `recompile_status` said completed); AgentScripts/RefreshAndCompile.cs (AssetDatabase.Refresh +
  RequestScriptCompilation) then polling fixed it. The wrapper file has no execute bit (`bash
  scripts/verify-cargo.sh`). PlayMode baseline run standalone right after a script-domain reload:
  CargoBaselineTestsSceneTests 1/1, CargoBaselineTests 3/3 (18:06).
- **Build:** artifacts/qa/cargo-20260916-180603/airlift-cargo.apk, SHA-256 0ddf6fe1..., md5 814b560e verified on the Quest, 86 checks across 19 suites in the wrapper (baseline 12 + 18 ticket suites; PlayMode/EditMode baseline evidenced standalone 1/1 + 3/3), bounded credential scan passed, 0 errors / 11 warnings. Installed and launched 18:20; the Quest is sitting at the controllers-required dialog until the owner picks up the controllers.
- **Still pending from the owner:** `vercel login` (then deploy services/guide-proxy, `vercel env` for
  OPENAI_API_KEY, https MintUrl, revert insecureHttpOption), consent heading wording, Library tile icon
  (task #11). No commit until the owner accepts on the headset; no push; adult testers only.

## 2026-09-16 — Owner check of the fix build (171327): flow works; polish round + voice start

- **Owner-reported (17:20):** consent → chips → cards → Cargo Crew works and "looks pretty good
  based on what we wanted"; chips give confirmation; cards have the look and Nerdy palette.
  Asks: consent card ~50% bigger; assistant card's right edge square; a handle to move the
  welcome/cards panel like the table; the assistant card should sit above the workbench and
  move with it; and the lesson should start by voice ("Do you want to get started?" → yes).
- **Build cargo-20260916-172755 (SHA-256 1d6826d5..., 71 checks):** consent scaled 1.45x; HUD
  masked to its rounded card; lavender panel handle (TableCarryTransformer + TwoGrabPlane,
  0.6x-1.8x) on the welcome root; second world-space "Guide HUD canvas" above the workbench
  and the HUD re-parents there in the lesson; mic now on in the cards view too (off only
  during the chip questions). Installed and launched 17:38 for the owner.
- **Voice start (P0-07b):** proxy gains `advance_step` and instructions to ask "would you like
  to get started" on lesson entry; Unity handler runs onboarding.Continue() only when the
  on-card primary button is active and interactable (same action as pressing it) and returns
  the new instruction for a one-sentence narration. Build in progress (started 17:40).
- **Owner check of build 172755 (17:42):** panel-side fixes fine; in the lesson the assistant
  bar did NOT appear above the workbench (no exceptions in logcat; wiring test passes, so the
  re-parented HUD is likely mispositioned/hidden at runtime; investigate in the editor by
  driving Flow to Lesson and reading hudRoot's world pose). Owner also asked for Pause/Play on
  the bar (task) and background music (task #14). Log warning: "Conversation already has an
  active response in progress" when Hush() is immediately followed by Prompt(); wait for the
  cancel before prompting.

## 2026-09-16 — Owner check of the first welcome build (163119): voice loop, dead chips; fixes built

- **Owner-reported:** found "Nerdy" via Library search and launched it (logcat confirms the
  launch came from the Quest launcher: P0-01 launch criterion met); the tile showed no Nerdy
  logo (follow-up task). Consent card with logo looked right. After consent the guide connected
  and spoke, but "it doesn't stop talking"; the "I enjoy" / "I'm here to" chips and Skip
  appeared to do nothing. Owner asked to tame the guide and suggested restricting voice to the
  lesson. Copy requests: welcome heading → "Hello to Nerdy AI"; consent heading wording to be
  confirmed (transcription garbled).
- **Root cause (voice loop):** the Quest microphone hears the Quest speaker; server VAD
  transcribed the guide's own speech as the learner and the model kept answering itself. Chip
  and Skip actions only sent user text into that loop, so nothing visible happened.
- **Fixes (build in progress):** GuideSession is half-duplex (mic never streams while the guide
  is speaking or for 0.6 s after; input buffer cleared on each response; Hush() cancels a
  response and drops queued audio). Onboarding is chip-driven with the mic off: chips write the
  profile deterministically, the guide only acknowledges in ≤5 words on app prompt, all three
  answers auto-advance to the cards, Skip always advances. The mic opens only inside the lesson
  (and closes on Mute). Proxy/dev-mint instructions rewritten: the guide never asks the welcome
  questions itself and speaks only when prompted; one sentence unless the app asks for two.
  dev-mint now uses the shared session config. Welcome copy updated.
- **Runner flake:** the PlayMode suite CargoBaselineTestsSceneTests returned total 0 inside the
  wrapper three times (16:57, 16:59, 17:02) yet passed 1/1 when run first after a script-domain
  reload at 17:03. Wrapper run for the fix build excludes it (documented; standalone pass is
  the evidence); follow-up task: run it first in the manifest or reload before PlayMode.
- **Tooling finding 17:12:** the Pipeline `recompile` command returned "compiling" but the
  editor assemblies were not rebuilt for a stretch (EditMode test DLL stayed at 16:29 while
  PrivacyGateTests.cs was added at ~16:40, so the runner reported total 0 for it). Polling
  `recompile_status` until completed rebuilt the assemblies. Player builds compile from
  source, so the APKs were unaffected; editor test runs in that window used stale DLLs.
  Standalone evidence after reload: CargoBaselineTestsSceneTests 1/1, CargoBaselineTests 3/3.
- **Fix build:** artifacts/qa/cargo-20260916-171327/airlift-cargo.apk, SHA-256 19a2a4c3..., 71
  checks across 16 suites (baseline scene/edit suites evidenced standalone), scan passed.
  Installed on the Quest (md5 3d1bc1d8 verified) and launched for the owner's retest.

## 2026-09-16 — Phase 0 build-out: P0-01 installed, P0-02/04/05/06 code-complete, P0-07 first cut

- **P0-01:** cargo-20260916-155124 (SHA-256 f144a620..., 59 checks) built as "Nerdy"
  `com.nerdy.vr` with the adaptive wordmark icon (aapt: label 'Nerdy'); installed (md5
  verified), old `com.jad.airlift` uninstalled. Owner Library route check pending.
- **P0-02:** Poppins (Regular/Medium/SemiBold/MediumItalic) and Karla (Regular/Medium/Bold)
  fetched from Google Fonts with OFL licenses; static TMP SDF assets; NerdyStyle tokens asset;
  generated 9-slice pill/card/small sprites, brand/spectrum/glass gradients; TMP gradient
  preset NerdySpectrum. NerdyStyleTests 3/3.
- **P0-04:** services/guide-proxy (Vercel function, plain ESM): server-authored session config
  (persona, English-only, no surroundings, no names, tools record_profile/end_welcome/
  describe_card/request_help, VAD threshold 0.6, transcription en), request validation,
  best-effort limiter, no secrets in responses/logs; `node --test` 5/5; real mint through the
  handler returns 200. Unity: GuideSession (mint → WSS → serialized sends → receive loop →
  main-thread events, tool results, context push, one reconnect, offline fallback),
  GuideMessages (Newtonsoft), GuideRouting, MicStreamer, AudioPlayback. GuideMessagesTests
  4/4. Not deployed yet (owner Vercel login pending); dev mint over LAN HTTP with
  insecureHttpOption=AlwaysAllowed as a documented temporary dev exception.
- **P0-05/06/07 (first cut):** NerdyDirector orchestrates consent → welcome → catalog →
  lesson; LearnerProfile stores tags only (enum-validated, ≤5 interests, names dropped);
  WelcomeFlow pure state machine; LessonCatalog (3 cards, 1 playable, facts for
  describe_card); GuideContextBuilder pushes bounded app-authored context (instruction text
  is the event source; no unsubmitted verdicts, no learner free text). CreateNerdyWelcome
  builds consent card, 16 answer chips + skip, three feature cards, guide HUD (orb, captions,
  Mute/Help/Again), hides the table during welcome/catalog and hooks Back to the catalog.
  WelcomeFlowTests 5/5; NerdyWelcomeWiringTests 2/2; PrivacyGateTests added.
- **Build:** artifacts/qa/cargo-20260916-163119/airlift-cargo.apk, SHA-256 d04f8820..., 73
  checks across 17 suites, scan passed, 0 errors. Installed on the Quest for the owner's
  welcome-flow check (docs/qa/nerdy-welcome.md). Voice needs the Mac mint server on the LAN
  until the proxy is deployed.

## 2026-09-16 — CC-P0-01 identity code-complete; CC-P0-03 voice spike in progress

- Owner approved the Phase 0 plan (commit 31d0b1e), supplied the Nerdy wordmark
  (4001x1635 PNG with alpha), confirmed rights, and provided an OpenAI key (stored only in
  ~/.config/nerdy/openai.env, mode 600; owner will rotate it later).
- **P0-01:** ConfigureNerdyIdentity sets productName/companyName "Nerdy", Android package
  `com.nerdy.vr`, version 0.2.0 (code 2), forceInternetPermission, and the six adaptive
  launcher icon slots (background #202344, wordmark foreground at 250 px inside the safe
  zone; legacy/round kinds are obsolete in Unity 6.6). AppIdentityTests 2/2. Device check
  (Library route, cold launch) pending the next Cargo build.
- **P0-03 facts so far:** the key works; `POST /v1/realtime/client_secrets` with model
  `gpt-realtime` returns an ephemeral `ek_…` secret (HTTP 200); the legacy
  `/v1/realtime/sessions` endpoint is gone (404). Throwaway LAN mint server
  services/guide-proxy/dev-mint.mjs runs on the Mac (192.168.86.20:8787); Quest is on
  192.168.86.34. Throwaway RealtimeSpike scene (CargoCrew copy, lesson hidden, head panel)
  and RealtimeSpike.cs (ClientWebSocket, mic PCM16 24 kHz streaming, PCM playback via
  OnAudioFilterRead, transcripts, [SPIKE] latency logs). SpikeBuild makes a development
  build with cleartext HTTP allowed for the LAN mint only, restoring the setting after.
- **P0-03 result: GO.** Development build nerdy-spike.apk (md5 392c2125) on the Quest 3S,
  adult tester (owner), Wi-Fi, LAN mint server on the Mac. Two-way voice works end to end:
  mic permission prompt → ephemeral secret → WSS to wss://api.openai.com/v1/realtime?model=
  gpt-realtime via System.Net.WebSockets.ClientWebSocket (IL2CPP, .NET Standard 2.1) →
  greeting heard by the owner → three spoken exchanges. Latency from server speech_stopped
  to first output audio delta: 415, 430, 445 ms. GA event names confirmed
  (response.output_audio.delta, response.output_audio_transcript.done,
  conversation.item.input_audio_transcription.completed). Input transcription via
  gpt-4o-mini-transcribe works ("Hey, I speak English.", "I want to access the nerdy lesson.").
- Findings to carry into P0-04: (1) sending the `OpenAI-Beta: realtime=v1` header closes the
  socket with beta_api_shape_disabled; omit it. (2) Concurrent ClientWebSocket.SendAsync is
  unsafe; the client now serializes sends through one queue. (3) The first utterance was
  transcribed empty and the model answered in Portuguese while hallucinating a room; session
  instructions must pin the language and forbid describing surroundings; consider a higher
  VAD threshold / prefix padding. (4) One ANR ("user request after error") killed an early
  instance while it sat behind the Quest "controllers required" dialog; keep the connect
  path off the main thread and add a watchdog. (5) Quest shows a controllers-required launch
  dialog when controllers are asleep; document in the QA script. Transport decision:
  WebSocket with ClientWebSocket, no extra package; WebRTC not needed at this latency.

## 2026-09-16 — Owner re-scoping: Phase 0 "Nerdy welcome" (plan drafted, awaiting approval)

- After accepting the workbench, the owner asked for the foundation first: findable Nerdy
  launcher with logo, two-way voice welcome by a Nerdy AI guide (age band, interests, goal),
  three Nerdy-branded lesson cards, per-lesson workbenches, guide inside the Cargo workbench.
  Owner chose: two-way voice (adult testers), OpenAI, identity "Nerdy" / `com.nerdy.vr`.
- Written: PHASE-0-NERDY-WELCOME.md (flow, architecture, privacy gate, branding from the
  owner's Live Learning Style Guide, ticket table, cut line) and tickets CC-P0-01..09.
  PLAN.md, TICKETS.md, CLAUDE.md, AGENTS.md updated; absorbed tickets annotated.
- Needed from owner: logo file, OpenAI key with Realtime access and a spend cap (out of
  band), proxy hosting choice, rights confirmation for the Nerdy name/logo, and approval
  of the plan. No implementation started.

## 2026-09-16 — Table handle: carry and resize the whole table (OWNER ACCEPTED on headset)

- Owner ask after accepting the toy look: move the table elsewhere and make it bigger or
  smaller from inside the app (only the Meta-button system recenter existed). Owner chose
  Option 1, a carry handle, over buttons or an adjust mode.
- **Handle:** yellow rounded bar on the deck's front edge (Table handle, z -0.412). Its
  Grabbable targets the station root, MaxGrabPoints 2. One hand: SDK OneGrabFreeTransformer
  with X/Z rotation locked (carry + yaw). Two hands: SDK TwoGrabPlaneTransformer on the table
  plane (planar move, yaw, uniform scale 0.5x-2x). Interactable created by QuickActionsAPI.
- **Gate/settle:** new runtime TableHandle on the director: disables the handle interactable
  while the practice strap or any fraction piece is held; disables the four piece
  interactables while the handle is held; on release levels the table (yaw only), clamps
  height to ComfortPlacement's 0.35-1.4 m band and clamps scale. Pure rules in
  TableAdjustRules (5 tests). CargoLessonDirector gains AnyPieceHeld.
- Orientation text now ends "Move the table by its yellow handle; two hands resize it."
- Layout test TableHandleWiredToStationRoot added (CargoTerminalLayoutTests now 8).
- Contract change: CLAUDE.md "place once, then world-lock; explicit recenter only with
  released pieces" becomes "player may carry/resize by the handle; refused while a piece
  is held". Meta-button recenter remains the fallback.
- **Build:** artifacts/qa/cargo-20260916-142801/airlift-cargo.apk, SHA-256 96818c56...; 54 checks across 12 suites; credential scan passed; 0 errors / 10 warnings. Installed on the Quest 14:31 (md5 345dd783 verified on device) and launched for the owner's check.
- **Owner headset check (build 142801):** handle visible and works, but one-hand carrying
  "jumps to a different place" and the table cannot be turned. Cause: OneGrabFreeTransformer
  applies the full wrist rotation to the root and then flattens pitch/roll, so with the
  handle 0.41 m from the pivot a small wrist tilt swings the centre by ~0.2 m, and wrist yaw
  is too limited to turn the table. Fix: new TableCarryTransformer (one hand): the grabbed
  point stays in the hand and the table yaws to keep its front edge toward the head, eased
  by slerp; wrist tilt/twist ignored. Pure rule TableAdjustRules.CarryPose (2 more tests,
  7 total). Two-hand plane transformer unchanged (free rotation + resize).
- **Build:** artifacts/qa/cargo-20260916-143730/airlift-cargo.apk, SHA-256 ce1860e3...; 56 checks across 12 suites; scan passed; 0 errors. Installed on the Quest (md5 8de8b9ab verified on device) for the owner's carry check.
- **Owner headset check (build 143730): "feels exactly the same".** Verified the installed
  APK (md5 8de8b9ab) and the scene contain the carry transformer; the device log showed
  TwoGrabPlaneTransformer running during a one-hand carry. **Root cause:** CargoCrew has two
  active ControllerGrabInteractors per hand (OVRComprehensiveInteractionRig's plus a
  standalone "[BuildingBlock] Controller Interactions" block; Onboarding has the same pair).
  With MaxGrabPoints 2 on the handle, one squeeze produced two coincident grab points, so the
  two-hand transformer ran (planar translate only, degenerate rotation/scale, jumps on
  release). The practice strap (MaxGrabPoints 1) hid this. Fix: disable the two extra
  interactors (AgentScripts/DisableDuplicateGrabInteractors.cs); test
  ExactlyOneActiveGrabInteractorPerHand added (CargoTerminalLayoutTests now 9). Note for the
  open grip defect: competing interactors is a candidate explanation worth re-checking.
- **Build:** artifacts/qa/cargo-20260916-144706/airlift-cargo.apk, SHA-256 9f69a09c...; 57 checks across 12 suites; scan passed; 0 errors. Installed on the Quest (md5 6929c4fe verified on device) for the owner's check.
- **Owner-reported (headset, build 144706):** "it works now... exactly what I was looking
  for." One-hand carry, two-hand rotate/resize and strap grabs all confirmed. Accepted and
  committed; no push.

### Why

Direct manipulation matches how the straps already work and needs no buttons or modes,
which the owner had just asked to remove. Targeting the station root keeps the card,
props and pieces together, and because all lesson geometry is station-local, resizing
changes nothing mathematical. The held-piece gate preserves the plan's rule that the
table never moves under a piece in the hand.

## 2026-09-16 — Toy look, centred pad, station controls removed (OWNER ACCEPTED on headset)

- Owner asks after the halves acceptance: (1) centre the measuring pad on the board,
  (2) remove Raise/Lower/Recenter station buttons, (3) a playful, polished look
  ("Hasbro, Lego") instead of sharp blocky primitives. Design approved in chat.
- **Layout:** pad and ruler now at the deck centre (x 0, z 0.02); tray row moved forward
  (z -0.17) so tray pieces sit outside the placement radius. Pieces rest on the surfaces
  (tray y 0.047, docked y 0.063) instead of floating 3 cm above them.
- **Controls:** three station buttons deleted from the lesson canvas; orientation text no
  longer says "Adjust the station below". Startup placement is unchanged. The system
  Meta-button recenter still works.
- **Look:** new runtime `RoundedBoxMesh` (pure, 7 EditMode tests incl. an orientation test
  against Unity's own cube) replaces every raw cube on the board with an exact-size rounded
  block; halves keep one flat cream cut face so docked halves still read as two pieces.
  Materials retuned to satin plastic in a warmer palette plus CargoYellow; warm key light,
  soft shadows, trilight ambient; pill buttons and rounded card via a generated 9-slice
  sprite; fraction labels on cream sticker plates; ruler marks moved to the pad's front rim
  in navy so docked pieces cannot hide them. 24 mesh assets under Assets/Airlift/Meshes.
- Editor scripts: ApplyToyLook, RefineToyLook, PolishRulerMarks, RefreshRoundedMeshes,
  PreviewToyLook. Layout tests added: StationControlsRemoved, MeasuringPadCenteredOnBoard,
  BoardObjectsAreRoundedNotRawCubes (CargoTerminalLayoutTests now 7).
- Desktop previews: artifacts/toy-look-practice.png, toy-look-fractions.png,
  toy-look-closeup.png. Not headset evidence. Build/device status: see below.
- **Build:** artifacts/qa/cargo-20260916-140241/airlift-cargo.apk, SHA-256 6490440791b4a08c92bf1265830c4973700fe16a53e3ea78d42cac40ec43a9ea; 48 checks passed (11 suites); credential scan passed; 0 errors / 10 warnings. Installed on the Quest for owner review (installed 14:13 after an ADB server restart; md5 142a655e verified on the device; launched).
- **Owner-reported (headset, build 140241):** "absolutely brilliant", loves the new look and
  feel, reads as Fisher-Price/Hasbro/Lego, smoother and more polished. Accepted.
- Owner's next ask before further fraction chapters: move the table to another location and
  make it bigger/smaller from inside the app (today only the Meta-button system recenter).
  Design pending owner approval; see the next entry when it exists.
- Onboarding scene untouched (regression reference). Committed after acceptance; no push.

### Why

The owner's playful-toy direction is a polish pass on top of an accepted core loop, so it
had to preserve every mathematical invariant: piece lengths, colliders and ruler snapping
are unchanged and only meshes, materials, light and layout moved. Rounded meshes are
generated, not purchased, keeping the no-new-assets constraint. Centring the pad and
lowering pieces onto the surfaces make the whole/halves comparison the visual focus.

## 2026-09-16 — Owner-reported: grabbing works, halves loop completed; committed

- Build under test: artifacts/qa/cargo-20260916-131831/airlift-cargo.apk, SHA-256
  18273ac61a1621d4f642dbf938660556a458d30d53a936c12d74f7bd09662732.
- Owner first saw nothing in the headset; ADB showed the Quest at its home shell with the
  app not running and no crash logged since install. Launched via ADB; VR focus, tracking,
  passthrough and both Touch Plus controllers came up cleanly.
- **Owner reported:** practice strap grab and manipulation works; the fraction chapter runs
  end to end (Start fractions, whole labeled 1, Split into halves, dock, Submit reads
  1/2 + 1/2 = 1). Owner calls it a simple early-stage lesson and accepted the progress.
- **Not established:** which input produced the grab. The build's temporary
  grip-or-trigger override and the practice-panel readout are still active, and the
  owner did not report the OVR/XR/SDK grip and trigger numbers from script steps 5-9.
  Treat the grip defect as mitigated, not root-caused; keep the override until answered.
- **Not yet checked:** board start height, release-on-pad completion text, Reset pieces,
  Back to lessons, both-hand matrix, label legibility at distance.
- Owner authorized committing all current work on `unity-airlift`. No push.
  `.gitnexus/` stays local via .git/info/exclude; the GitNexus blocks in CLAUDE.md and
  AGENTS.md are committed with the rest of the instructions.

### Why

The core loop the owner prioritized on September 16 (grab a whole labeled 1, split into
two 1/2 pieces, rebuild against a fixed whole) now works on the physical Quest by owner
report, which is the acceptance evidence the plan requires. Committing preserves the
tested state before the temporaries are removed and the next chapter starts.

## 2026-09-16 — Whole-and-halves chapter built, installed, awaiting device check

- Owner priority two (P1-04/P1-05 core loop) implemented as code-complete, not accepted:
  after practice, "Start fractions" shows a 0-to-1 ruler on the measuring pad, a whole
  strap labeled 1, Split into two exact half pieces labeled 1/2 (stacked notation via
  FractionNotationView), neutral docking on release, Submit evaluates the ruler
  deterministically (whole = 1; both halves = 1/2 + 1/2 = 1), Reset pieces, Back exits.
- New: Scripts/Lessons/CargoLessonModel.cs (pure, 6 tests), RulerLayout.cs (pure, 3 tests),
  CargoLessonDirector.cs (SDK adapter), AgentScripts/CreateFractionChapter.cs,
  FixFractionChapterLook.cs, PreviewFractionChapter.cs; OnboardingDirector gained a
  Ready-stage "Start fractions" primary button (UnityEvent). Desktop preview:
  artifacts/fraction-chapter-preview.png (geometry only, not headset evidence).
- APK artifacts/qa/cargo-20260916-131831/airlift-cargo.apk, SHA-256
  18273ac61a1621d4f642dbf938660556a458d30d53a936c12d74f7bd09662732; 38 checks passed
  (adds CargoLessonModelTests 6, RulerLayoutTests 3); scan passed; 0 errors / 10 warnings.
  Installed on the Quest (md5 verified). This build still contains the temporary practice
  input readout and grip-or-trigger override from the earlier entry today.
- Not verified on device: grabbing at all (root defect still open), docking feel, label
  legibility, Split/Submit flow. Known nit: CargoLessonDirector does not unsubscribe its
  pointer lambdas on destroy (single-scene app; fix in the next build).
- No commit/push.

### Why

The chapter reuses the practice strap's exact size as the whole and prebuilt halves so
"cutting" is a prefab swap with conserved cells, never geometry editing. The ruler is the
visible whole, docking is neutral and evaluated only on Submit, and every piece belongs to
one WholeId so pieces from another whole cannot mix. This keeps the math deterministic and
lets the owner test the full grab-split-rebuild loop in one headset session.

## 2026-09-16 — Grab diagnostic build installed; test-runner hang root-caused

- Owner handed implementation to Claude Code with priority: fix practice grabbing,
  then the labeled whole/halves loop. Voice/AI remains deferred.
- Static investigation exhausted before any fix: CargoCrew and Onboarding scenes are
  object-for-object identical except styling, cargo props, ComfortPlacement and the
  locomotor override; Android manifests and OpenXR runtime action bindings are
  byte-identical across the working Onboarding APK and the broken CargoCrew APKs;
  Quest OS (v207, built Aug 26) and controller firmware unchanged. The vendor selector
  chain (ControllerGrabInteractor -> ControllerSelector usage 16 GripButton ->
  FromOVRControllerDataSource -> OVRInput) carries no scene override. Grip cause is
  still unconfirmed; the index trigger demonstrably reaches OVRInput because the ray
  navigated the catalog.
- Built and installed artifacts/qa/cargo-20260916-130202/airlift-cargo.apk,
  SHA-256 44bee55ef6362885852a2e4b73bbc202b8e6f59acddef0594a1aa9ab8ae04d0b
  (installed md5 verified equal). 29 checks passed (12 onboarding, 3 placement,
  1 typography, 1 glyph, 4 layout, 4 new OnboardingPlacementTests, 3 baseline,
  1 PlayMode scene); bounded credential scan passed; 0 errors / 16 warnings, triage pending.
  This replaces the invalid VendorGrabProof APK on the device.
- Changes in this build: (1) startup placement waits for a tracked head instead of
  sampling the origin, which the editor log proved placed the board at 0.35 m;
  (2) TEMPORARY on-panel input readout during practice (OVRInput, Unity XR and SDK
  selector grip/trigger values plus grab interactor state), logged as
  [DEBUG-cargo-input] for 120 s; (3) TEMPORARY CargoCrew-only scene override so the four
  practice grab selectors accept grip OR trigger (AgentScripts/ApplyGrabSelectorFallback.cs).
  Old [DEBUG-cargo-placement] probes removed. Reference scenes untouched (hashes pass).
- Test-runner "deadline exceeded" stall root-caused with a process sample: the Unity
  Test Framework raised a native Scene(s) Have Been Modified alert because TextMesh Pro
  dirties CargoCrew on open; the editor main thread sat in NSAlert runModal. Dismissed
  with Save. scripts/verify_cargo.py now refuses play mode and runs
  AgentScripts/PrepareCleanScenes.cs (copies dirty state to artifacts/qa/dirty-scenes,
  reloads from disk) before tests. Wrapper tests: 5 passed.
- An accidental editor-only rotation of a truck Wheel was found in the unsaved scene
  state and discarded; the unsaved state is archived outside the repo.
- Not done: physical grab confirmation, warning triage, P1-02 remaining gaps (Reset,
  notation specimens, prefab extraction), free station repositioning, fraction loop.
  No commit/push.

### Why

Every static comparison between the working and failing builds came back equal, so the
only remaining lever was runtime evidence that the owner can read without ADB. The
readout shows at which layer the grip signal disappears, and the trigger fallback keeps
practice usable if the grip path is the defect. Placement now waits for tracking because
the log showed the head at the origin at Start, which is the exact floor-height symptom.

## 2026-09-16 — Owner requested pause and Claude Code handoff

- Wrote HANDOFF-2026-09-16.md with facts, hypotheses, installed artifact, known
  errors, invalid vendor experiment and safe resume recommendations.
- Updated CLAUDE.md, AGENTS.md, shared handoff, progress, README, backlog,
  current ticket/voice status, PRD/requirements and QA notices. Historical records
  retained with prominent superseding status, not silently rewritten as success.
- Vendor APK built, credential scan passed, installed and launched. Owner sees
  platform but no cube. Prefab inspection found hand-grab support but no controller
  GrabInteractable; experiment invalid. Cube visibility cause not established.
- Cargo runtime diagnostic observed Hover/candidate=true, selected=false, and
  sampled grips zero. No confirmed cause or fix; no current ticket acceptance.
- Implementation paused; no new commit/push. Documentation diff check passed.

## 2026-09-16 — Isolated vendor-grab control experiment

- Owner authorized a separate vendor test scene/build. Created VendorGrabProof
  using fresh Meta passthrough/controller Building Blocks and the supplied
  [BB] Grabbable Cube prefab. No lesson scripts, custom grab wiring, or vendor edits.
- Explicit stationary-MR override disables FirstPersonLocomotor; floor-level
  tracking, fixed cube/platform placement and transparent cameras are configured.
- Builder checks one camera rig and one supplied Grabbable. Reference Onboarding
  and DeviceProof scene hashes remain unchanged. CargoCrew scene untouched.
- This is a diagnostic control, not ticket acceptance. Build/install/device outcome
  must be recorded separately. No commit/push.

## 2026-09-16 — Core arithmetic prioritized; grab diagnostic added

- Diagnostic verification initially timed out in editor play mode. Exited that
  session and requested script-domain reload; repeat run passed all 25 checks.
- Diagnostic APK: artifacts/qa/cargo-20260916-114658/airlift-cargo.apk.
  Build and bounded credential scan passed; ADB replacement install succeeded.
  This adds diagnostic observation only, not a verified grab fix. Owner reproduction
  is required within the bounded two-minute diagnostic sampling window after launch.

- Owner deferred voice/AI integration and supporting audio controls. Prioritize
  functional grabbing and the labeled whole-to-halves loop before launcher/polish.
- Owner confirms stable station and visible cargo props; practice grabbing fails
  despite controller contact. No acceptance inferred from visuals.
- Added bounded temporary DEBUG-cargo-grab runtime samples for grip input,
  target activation/colliders, interactor state, selection and contact distance.
  Compilation passed; verification/build active, not installed or a verified fix.
- Startup height and free repositioning remain open; no commit or push.

## 2026-09-15 — Disappearing station traced to gravity locomotor

- Owner repeatedly reproduced apparent table ascent. Device samples show fixed
  station with rapidly falling virtual head; not a moving table or button event.
- Found active FirstPersonLocomotor inherited from Meta comprehensive rig; added
  failing stationary-rig check, then disabled component in CargoCrew scene override.
- No vendor package/reference scene edits. Regression/build running; physical
  confirmation and removal of temporary diagnostic probes still pending.

## 2026-09-15 — Owner-requested intermediate ticket 2.5

- Added CC-P1-02.5 for recognizable app branding and independent headset launch.
- Includes verification of sideloaded-app library/dashboard limitations; no store
  upload, third-party launcher or distribution change is implied.
- Backlog now 29 tickets; disappearing-station bug remains the active P1-02 priority.

## 2026-09-15 — P1-02 visual checkpoint APK ready

- Full verification/build run completed successfully: 24 Unity checks, bounded APK
  credential scan; 5 verifier tests and diff check pass. No skipped-test override.
- artifacts/qa/cargo-20260915-182240/airlift-cargo.apk, SHA-256
  71aa75bebcf44bf481b3e5171534862c1ef3b38762e776ee32f524c05e7dd919.
- Build: 0 errors / 10 warnings, pending warning triage. No commit/push.
- Quest connected but unauthorized: owner USB approval needed before installation.
- Visual checkpoint ready, not full ticket acceptance. Remaining notation/prefab/reset/
  focus details and device profiling remain explicit in P1-02 QA.

## 2026-09-15 — P1-02 typography integrated

- Owner delegated routine design choices; selected A (Nunito Bold / Nunito Sans
  Semibold). Static font sources and OFL notices are recorded in the project.
- Explicit SDF bindings, focus outlines and placement feedback applied to CargoCrew.
- New-font overflow was reproduced and repaired through shorter CargoCrew-only
  instructions, not smaller text. Reference content/scenes preserved.
- Compilation and six-page text-fit inspection pass; regression/build in progress.
- Ticket remains incomplete: notation specimens, prefab/reset/focus details and
  device/profiling checks still pending. See P1-02 QA record.

## 2026-09-15 — P1-02 initial cargo environment and placement

- Owner authorized next ticket with baseline physical review batched at this checkpoint.
- CargoCrew now contains original truck/containers/staging/aircraft/destination labels
  and three station controls. Reference scene hashes unchanged. No new packages.
- Compile passed; 3 ComfortPlacement tests passed; catalog and existing text-fit
  inspection passed. Fixed oversized destination labels found in desktop preview.
- Actual-font browser comparison prepared; both candidate font loads verified. Unity
  font integration awaits owner choice. No new APK/device check; ticket in progress.
- Full remaining scope and evidence: docs/qa/cargo-CC-P1-02.md.

### Why

Original static cargo silhouettes establish context without new packages or runtime
spawning. Moving the shared station root keeps props and learning materials aligned;
held-object movement is refused. Font selection remains an explicit owner review.

## 2026-09-15 — Baseline APK built; bounded scan passes

- Approved CargoCrew APK succeeded with 0 errors / 8 warnings; warning triage pending.
- Artifact: artifacts/qa/cargo-20260915-180248/airlift-cargo.apk.
  SHA-256: 0c77ee9e1bfbbc13083d5ca38d6d3ebfba54643cbc4444184f078932db3e7e58.
- Raw scan falsely combined adjacent IL2CPP language literals. Corrected scanner to
  respect installed v108 metadata string boundaries, preserving actual token detection.
- Five verifier tests pass; existing APK separately rescanned successfully. The original
  wrapper exited at the false positive; do not describe it as a clean uninterrupted run.
- 16 Unity checks passed before build. No new APK installed, no device acceptance,
  no commit/push. See docs/qa/cargo-CC-P1-01.md for recovery and artifact lineage.

### Why

The test-runner domain refresh restored execution without skipping tests. Parsing
IL2CPP string boundaries prevents false secret matches across unrelated literals
while retaining checks on actual strings. Device review remains a separate checkpoint.

## 2026-09-15 — Test-runner recovery; approved baseline APK building

- Owner approved local build from uncommitted work; no commit/push required or made.
- Reproduced zero PlayMode results despite successful list_tests and Pipeline selector
  discovery. Exact-name and explicit-test switches did not fix execution.
- Official script-domain reload restored the full verification sequence: 12 onboarding,
  3 baseline, 1 scene tests passed in artifacts/qa/cargo-20260915-180248.
- Root cause within Unity/Pipeline remains unresolved; domain refresh is a verified
  recovery only. No vendor patches. Temporary diagnostic script removed after use.
- Build status verified as building. APK completion, credential scan and device review
  remain pending; no new fraction gameplay or next-ticket completion is claimed.

### Why

This distinguishes a recovered test runner from a proven permanent fix. Zero tests
still fail closed; no checks were skipped to start the explicitly approved local build.

## 2026-09-15 — CC-P1-01 implementation, build authorization pending

- Owner authorized only Phase 1 ticket 1 and batching physical review at meaningful checkpoints.
- Created non-overwriting CreateCargoCrew builder, CargoCrew scene copy, CargoBuild entry,
  milestone manifest, Python/CLI verification wrapper and baseline EditMode/PlayMode tests.
- Existing reference scene hashes unchanged; no new packages or gameplay changes.
- Verified: compilation completed without errors; 12 onboarding + 3 baseline EditMode +
  1 PlayMode tests passed; 4 verifier tests passed; git diff --check passed.
- Evidence: artifacts/qa/cargo-20260915-173318 (JSON and generated test XML).
- First new-suite attempt correctly failed on zero discovered tests before explicit import/
  compilation. Recompile and rerun passed; not counted as an initial passing run.
- Build blocked by uncommitted work as designed. No APK created/installed; no commit/push.
- Status remains in-progress, not code-complete or accepted. Build polling/actual APK scan
  are implemented but not exercised on a new real build yet; scan coverage is bounded.
- Next: owner chooses a documented local development-build exception or reviewed Git
  checkpoint; do not bypass guard or start ticket 2. Physical checks remain batched.

### Why

The baseline is copied rather than rebuilt so its known interaction behavior and reference scenes remain intact. Explicit scene selection and fail-closed test/build checks prevent a successful command from disguising missing tests or the wrong scene. Device acceptance remains pending, and the uncommitted-work guard requires owner direction before an APK build.

Newest entries first. This is the live progress log from September 15 onward.
Earlier history remains in [PROGRESS.md](../PROGRESS.md). Never mark planned work done.

## 2026-09-15 — Cargo setting made explicit in tickets

- Owner authorized documentation/ticket updates for a recognizable cargo experience.
- Retained stationary mixed reality; no full-VR port, driving or new XR framework.
- P1-02 owns truck, containers, loading/measuring platform, destinations and parked
  aircraft; P1-03 introduces their purpose. P1-07 owns accepted-job parcel progress,
  later activities reuse it, and P3-02 completes dispatch.
- Added F-21 and named layout/progress/reset checks; updated story, style and plan.
- Phase counts remain 10/8/6/4. No game code, models, imports, builds or commits.

### Why

The setting should explain the learner's job before arithmetic begins and visibly
respond to completed work. Bringing recognizable cargo props forward while keeping
progress tied to deterministic lesson acceptance makes the real-world context
part of the activity without adding a vehicle simulator or obscuring fraction meaning.

## 2026-09-15 — Core AI guide, style guide and testing cadence revision

- **Status:** planning changes authorized; implementation remains paused for owner review.
- **Owner decisions:** AI voice guidance is core throughout Cargo Crew; spoken questions
  are separate. A dedicated VR/AR/Unity style guide is required. Only Cargo Crew is in scope.
- **Authored:** STYLE-GUIDE.md, VISUAL-DESIGN.md and AI-VOICE-GUIDE.md; synchronized
  PRD, plan, requirements, systems design, server contract, ticket index and primers.
- **Design:** Nunito/Nunito Sans are comparison candidates, not selected/imported fonts.
  Shared tokens, fraction readability, accessible input and actual headset review are specified.
  The 3D Interaction Design skill informed spatial comfort/input; Game Developer informed
  reusable Unity assets and state-driven presentation, not invented validation claims.
- **AI:** live constrained model selection starts P1-03, followed by validated text and
  matching reviewed speech assets; recordings are fallback only. Direct audio and child use retain explicit gates.
  Provider/account/transport/budget readiness is not verified.
- **Review:** challenger identified that allowed vocabulary/transcripts alone cannot
  guarantee correct spoken math; contract now uses approved fact/action/template IDs
  with semantic checks. P1-03 owns a finite reviewed speech catalog: adult listening
  plus runtime hash/context checks, not a transcript-only safety claim. The live model
  selects explanations; unreviewed new audio uses labeled fallback.
- **Cadence:** change-level tests, ticket-level device checks, full-journey milestone
  review, then owner-authorized checkpoint. No merge to main is needed for headset preview.
- **Unchanged:** 28 draft tickets (10/8/6/4), no new implementation, provider calls,
  audio/font imports, Unity build, headset test, commit or push during this revision.
- **Next:** final review evidence in REVIEW.md; present readiness and first ticket
  for owner implementation approval. This entry supersedes the older recorded-first proposal.

### Why

The owner requires a coherent designed learning experience with AI spoken guidance,
not an arithmetic scene with optional narration. Moving its first verifiable slice
early exposes provider and device risks before building the entire lesson. Shared
style rules and small accepted tickets preserve consistency while leaving runtime
claims dependent on real tests and owner review.

## 2026-09-15 — Fraction planning revision (not an implementation ticket)

- **Status:** draft for owner review; no ticket started.
- **Branch/workspace:** unity-airlift, /Users/jad/Desktop/math-workbench-airlift.
- **Latest existing commit:** fd7895f; onboarding code and documentation currently
  include uncommitted work. This planning task did not commit or push anything.
- **What was authored:** local 00-build PRD, requirements, constraints, stack,
  systems design, story/cue script, implementation plan, template, shared
  Claude/Codex handoff and 28 full draft ticket primers (10/8/6/4).
- **Methodology:** adapted LabelCheck's structure and completion rationale pattern.
  Its files were not changed; its web commands and automatic merge rules were not adopted.
- **Research:** existing Unity/Meta source inspected for button chords, two-piece
  merge and scaling; primary pedagogy and ElevenLabs policy/API references retained
  under the existing RAP run. No package or provider integration performed.
- **Important gate:** ElevenLabs child-targeted-use restrictions require permission
  review; local authorized adult-recorded narration is the proposed fallback.
- **Verification:** planning-file structural/link/count checks only, recorded in
  REVIEW.md. No new Unity compile, test, build, installation or headset test in
  this planning task. All ticket test suites beyond prior onboarding are proposed.
- **Scope decisions awaiting owner review:** narrated-first versus live guide;
  split/merge and synchronized zoom versus free resizing; 10/8/6/4 backlog and
  a realistic first-chapter cut line. Café and Garden remain unimplemented.
- **Next:** review PRD → STORYBOARD → PLAN → Phase 1 tickets together. Do not begin
  CC-P1-01 until approved.

### Why

The next deliverable is a reviewable learner journey and executable ticket sequence,
not another isolated interaction. The plan preserves the working headset foundation,
makes mathematical meaning and narration explicit, and separates full product scope
from a small accepted first chapter. Provider-neutral audio avoids making the lesson
depend on unresolved child-targeted service permissions. Template completion fields
remain factual-at-start/at-completion rather than inventing future progress.

## 2026-09-15 — Owner-reported onboarding acceptance checkpoint

- **Artifact:** artifacts/airlift-onboarding.apk.
- **SHA-256:** 9559d0bda634e89a81a5a88b7c0d487efc9c0c9baa99210d5fede766462d7edf.
- **Previously recorded build:** build_e54958879ec6, zero errors/10 warnings;
  12/12 OnboardingFlowTests and layout/reference check passed.
- **Connection:** after unsuccessful reconnect/restart attempts, toggling Developer
  Mode off/on with USB connected caused a debugging prompt. Owner approved; ADB
  became authorized; install and launch succeeded. Cause of lost authorization
  was not established; do not present the successful recovery as root-cause proof.
- **Owner confirmed:** Arithmetic Lessons catalog and three cards, Cargo Crew
  mission/briefing, successful grab/place practice and replay.
- **Still unverified:** both-controller complete matrix, seated/standing reach,
  repeated interaction counts, Back round trip, pause/tracking recovery, capture
  and measured performance. No actual fraction activity or voice guide exists.
- **Outcome:** onboarding is a working foundation, not the finished lesson.

## Entry template for accepted work

Date / ticket / branch / starting commit:
Status: in-progress, code-complete, accepted or blocked:
Actual changes and observed outcome:
Automated commands/results and failures:
Physical checks, interventions and artifact/hash:
Deviations and owner approval:
Why: paste the ticket's completed Why paragraph verbatim.
Checkpoint commit/push status (only what actually happened):
Next ticket/action and remaining gates:
