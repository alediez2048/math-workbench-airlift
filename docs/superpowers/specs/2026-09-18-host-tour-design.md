# The host tour — onboarding, second design (2026-09-18)

September 18 owner walkthrough revision: see [onboarding repair plan](../../plans/2026-09-18-onboarding-repair/plan.md) for the proposed catalog-only tour; the design below is retained as history, not evidence that the revision is implemented.

Owner decisions, 2026-09-18 midday, after the first rundown (a text card with six stops) and the first pointing
tour were rejected on the headset ("the story telling is not there, showing the user around controllers and menu
options is not there"):

- **Dee is the host.** She shows the learner around her lounge like a friend showing their place. Warm, no plot.
- **How the app works, not the lessons.** The tour starts on the welcome card, runs through the profile questions
  (they stay) and the wall, and ends there. Lessons are left alone; they teach their own controls. Six stops, about a minute.
- **Gated.** On each stop exactly one thing works; everything else dims and cannot be pressed. Skip is visible
  top-right the whole way. B and ‹ skip too. "Replay the tour" stays in settings.
- **Runs once.** Questions answered and tour seen → welcome card, then the wall, no tour.
- **Voice off:** same tour, captions only; stop 1 drops the mic sentence.

## The six stops

| # | Screen | Ring on | Gate (what the learner presses) | Dee says |
|---|---|---|---|---|
| 1 | Welcome card | the two pills | Turn on the mic, or Continue without voice | Hi, I'm Dee. This is my lounge, and this board is where we start. Turn on the mic if you'd like to talk with me. |
| 2 | Questions | the › arrow | › (after the chips; › is dim until every row has an answer) | Tell me a little about you, then press the arrow down here. It is always in that corner. |
| 3 | Wall | the first lesson tile | › | This is my wall. Every tile is a place we can go. Point the beam and press the trigger to open one. |
| 4 | Wall | Featured · Newest · Most viewed | a filter pill | These sort the wall. Try one. |
| 5 | Wall | the gear | the gear, then Done | The gear is where you set things the way you like. Pause, Again and Mute live there too. |
| 6 | Wall | the ‹ arrow | ‹ or B | Press Back, or B on your controller, to come back here from anywhere. Try it, and then pick a lesson: I'll see you there. |

Stop 6 does not leave the wall: on the wall ‹ is normally dimmed, so during stop 6 it is the gate and does nothing
else; the tour ends on it and the wall is fully lit. Cargo's locked directors are never touched.

## Look

The pointer already built: everything but the target dims to 32 %, a cyan stroke ring with a lavender bouncing arrow
on the target, "3 OF 6" and Skip top-right of the board, Dee's line in her bar and spoken word for word.

## What it reuses

`RundownScript` (stop list, gates), `LoungeRundown` (pointer, dim, header), the ‹ › arrows, `BackButtonWatcher`,
`ControllerHelper`, settings replay. New: targets on the welcome card and the questions; a
`ChipsAnswered` gate. Switch: `NerdyDirector.OnboardingEnabled`
back to true.

## Not in this design

Anything inside a lesson (owner: "leave the lessons alone"), a body for Dee, the hands-on stops of the first rundown,
timers, praise words.
