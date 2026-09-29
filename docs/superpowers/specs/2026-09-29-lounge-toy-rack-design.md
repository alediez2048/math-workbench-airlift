# The toy rack and progress board — design

**Owner ask (2026-09-29):** "add a rack of toys from the lessons and a scoreboard to the Nerdy lounge. Every time a kid
completes a lesson he gets access to a new toy on the rack: complete lesson 1 of Dock 7 and he gets an orange container
he can grab from the rack and play with; lesson 2, a truck; and so on for all the lessons, like Super Smash Bros. Melee
after every adventure." Mockup approved on the canvas board *Proposed · toy rack and progress board*
(https://claude.ai/artifact/JPTeoWGosWPJqyP2JNACgf).

## What it is

A rack on the lounge wall to the left of the whiteboard with **fifteen slots, one per chapter** (Cargo Crew 5, Café 5,
Garden 5), and a **progress board** above it. Completing a chapter unlocks that chapter's toy: it appears in its slot,
grabbable, and stays for good. Everything lives in the lounge; lessons are untouched.

| Lesson | Chapter | Toy (the object the chapter is about) |
|---|---|---|
| Cargo Crew | Big truck | orange container |
| Cargo Crew | Two pickups | big truck |
| Cargo Crew | Four vans | pickup |
| Cargo Crew | Same share, smaller boxes | van |
| Cargo Crew | Top it up | dock crane |
| Café | Two friends | plate of two pastries |
| Café | Table of three | café table |
| Café | Box it up | pastry box |
| Café | Bigger order | tray of pastries |
| Café | Fact family | coffee pot |
| Garden | First rows | seed tray |
| Garden | Equal rows | planter bed |
| Garden | Turn the bed | watering can |
| Garden | Split the bed | fence piece |
| Garden | Your own split | sunflower |

## Behaviour

- **Locked** toys are grey silhouettes in their slot: visible, not grabbable, nothing hidden.
- **Unlock** happens the moment the app records a chapter as complete (the same signal Dee already narrates). On the
  next return to the lounge the new toy has a soft glow until first grabbed, and Dee says one line ("Your big truck is on
  the rack"). No confetti, no score, no sound sting.
- **Grab and play:** unlocked toys use the same Meta grab interaction as lesson pieces. Released anywhere, a toy floats
  back to its slot after 4 s. Toys never leave the lounge; they are hidden during a lesson like the rest of the room.
- **Progress board:** three rows (Cargo Crew, Café, Garden), five checkmarks each, "n of 5". No points, stars, streaks
  or timers (project pedagogy rules). The heading is "What you've built".
- **Persistence:** completed chapters are part of the library store (`nerdy-library.json`). Clear saved data and
  Restart fresh empty the rack. Completing a chapter again changes nothing.
- **Scenery:** the rack exists in the Nerdy lounge scenery only; in "Your room" (passthrough) it is not shown.

## Architecture (small, testable)

- `LibraryState` gains `completed` (lessonId → set of chapter numbers), `RecordCompleted`, `IsCompleted`,
  `CompletedCount`, round-tripped in `ToJson`/`FromJson`; `ClearAll` empties it.
- `ToyRackModel` (pure C#): the fifteen `ToySlot`s (lesson, chapter, name, spoken name), `Unlocked(library)`,
  `NewlyUnlocked(sinceSeen)` and `SeenSet` for the glow, board rows. No Unity types.
- `NerdyDirector` records completion where it already observes `chapter.Complete` per chapter number
  (`NarrateChapterChange`), calling `Library.RecordCompleted(ActiveLessonId, chapter.Number)` and `SaveLibrary()`.
  On `ShowWall` it refreshes the rack and speaks the unlock line through `Say` (localized like every prompt).
- `ToyRack` (MonoBehaviour, lounge child): slots with a `ToyReturn` component per toy (float back to slot), applies
  locked/unlocked/new states, exposes `Refresh(LibraryState)`.
- `ProgressBoard` (MonoBehaviour): three rows of TMP text and checkmark images, `Refresh(LibraryState)`.
- Toys are built by `AgentScripts/AddToyRack.cs` from the existing prop builders: Dock 7 vehicles through
  `BuildDockWorkbench.BuildVehicle` (extracted to a shared helper), Café and Garden props through their station builders'
  helpers (extracted the same way). Placeholder rounded boxes with the toy's colour are acceptable for a first headset
  check where a helper does not extract cleanly; the mockup marks these as placeholders.
- Grab: `QuickActionsAPI.AddGrabInteraction` per toy in the builder (as the rundown block does), one grab interactor per
  hand as already enforced in the rig.

## Out of scope

Lessons and their directors (Cargo Crew is locked), any reward beyond the toy itself, toys inside lessons, sharing or
export, any numeric score.

## Acceptance (headset, adult tester)

1. Fresh learner: rack shows fifteen grey silhouettes, board reads 0 of 5 three times.
2. Complete Dock 7 chapter 1, exit to the lounge: container in colour with a glow, Dee's one line, board 1 of 5.
3. Grab the container, drop it across the room: it floats back within 5 s.
4. Relaunch: container still unlocked, no glow after it was grabbed once.
5. Restart fresh: rack back to fifteen silhouettes.
