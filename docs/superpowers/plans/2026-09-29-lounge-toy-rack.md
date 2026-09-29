# Plan — the toy rack and progress board (spec: ../specs/2026-09-29-lounge-toy-rack-design.md)

Rules that apply: TDD per step (failing test first), Cargo Crew lesson code untouched, one Unity turn per builder then a
render (`PreviewLounge.cs` / `PreviewWelcomeBoard.cs`), nothing to the headset before the render is approved, commit only
when the owner says. Estimated: six steps, about a day of work with headset checks.

## Step 1 — completion in the library store (pure C#)
- Tests (`LoungeStoreTests`): `RecordCompleted` round-trips through JSON; `IsCompleted` false for unknown; older files
  without `completed` load as none; `ClearAll` empties it; `Continue` unaffected.
- Code: `LibraryState.completed` (Dictionary<string, SortedSet<int>>), `RecordCompleted`, `IsCompleted`,
  `CompletedCount(lessonId)`, JSON `"completed": {"cargo_crew_fractions":[1,2]}`.

## Step 2 — the rack model (pure C#)
- Tests (`ToyRackModelTests`, new): fifteen slots in lesson/chapter order with the names from the spec; `Unlocked`
  follows the library; `NewlyUnlocked` is unlocked minus seen; `MarkSeen` persists in the library (`toysSeen`);
  board rows read "n of 5".
- Code: `Airlift.Lounge.ToyRackModel`, `ToySlot`.

## Step 3 — recording completion in the director
- Test (`GuideToolsTests` or a new `CompletionRecordingTests`): a pure helper `CompletionRecorder.Update(observedComplete,
  chapter)` returns the chapter number to record exactly once per transition to complete.
- Code: `NerdyDirector.NarrateChapterChange` calls it and `Library.RecordCompleted(...)` + `SaveLibrary()`; log
  `[Nerdy] chapter complete lesson=… chapter=…`.

## Step 4 — the rack and board in the scene
- Builder `AgentScripts/AddToyRack.cs` (idempotent): a `Toy rack` under the lounge shell on the wall left of the board
  (wall index chosen from `BuildLounge`'s board angle), three shelves, fifteen slot anchors, a toy per slot from the
  extracted prop helpers (or a placeholder rounded box in the toy's colour), `ToyReturn` + grab per toy, the
  `ProgressBoard` panel above (TMP, Nerdy style tokens). `ToyRack` and `ProgressBoard` components with `Refresh`.
- Wiring test (`LoungeWiringTests`): rack present, fifteen slots, each toy grabbable, board has three rows; the rack is a
  child of the lounge scenery so "Your room" hides it.
- Render: `PreviewLounge.cs` from the arrival spot looking left; owner approves the render before any build.

## Step 5 — unlock moment and return-to-slot
- Tests: `ToyReturn` timing is pure (`ToyReturnRule.ShouldReturn(elapsed)`); the unlock line comes from a pure
  `ToyRackLines.Unlock(toyName)`; `GuidePolicy` unchanged.
- Code: `ShowWall` → `toyRack.Refresh(Library)`, glow for `NewlyUnlocked`, `Say(ToyRackLines.Unlock(...))` once, then
  `MarkSeen` on first grab. Grab release starts the return timer; the toy lerps home.

## Step 6 — reset paths, QA, docs
- Tests: `ClearSavedData` and `RestartFresh` leave the rack empty (store test + wiring/refresh test).
- QA script `docs/qa/lounge.md` section E2 (the five acceptance steps), dev log, CLAUDE.md line, builder order note
  (`AddToyRack` after `ApplyRoomFinish`). Build, install, owner headset check, then commit on the owner's word.

## Risks
- Prop extraction from the station builders may pull lesson-specific dependencies; placeholders keep the step moving and
  the real props follow as a separate step.
- Grabbable toys must not add a second grab interactor per hand (the Sept 16 rig bug); reuse `QuickActionsAPI`.
- Performance: fifteen extra meshes and one text panel in the lounge; check the perf run listed in RELEASE-GATES.
