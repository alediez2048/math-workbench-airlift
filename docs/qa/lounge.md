# Headset QA — the Nerdy lounge front door (CC-FD-01..10)

Adult testers only. Preview app `com.nerdy.vr.lounge` ("Nerdy Lounge (preview)"), installed by
`bash scripts/lounge-preview.sh`. The release app `com.nerdy.vr` is untouched. Voice needs Wi-Fi: the guide mints
from `https://nerdy-guide-proxy.vercel.app/session`.

Write down what you saw next to each step. "Works" is fine; anything else in a sentence.

## A. Arrival and the two-button welcome

| # | Do | Expect |
|---|---|---|
| A1 | Put the headset on, launch the preview app | You are in a round room (Nerdy lounge) or your own room; dots fly in, the Nerdy AI+VR logo assembles on the board. A progress bar shows only while Dee is still connecting |
| A2 | Wait | Dee greets you on sight (voice), the board shows *Meet Dee, your Nerdy AI assistant* with two pills: *Turn on the mic* and *Continue without voice*. Her bar (orb, gear, Help) is inside the board's lower band |
| A3 | Press the gear | The settings card replaces the welcome card: Voice language, Where you learn, Dee (Pause/Again/Mute), Volume, four switches, Replay the rundown, Clear saved data, Reset settings, Done, a version line |
| A4 | Press *Nerdy lounge*, then *Your room*, then *Done* | The room shell comes and goes; passthrough shows in Your room; Done brings the welcome card back, nothing else |
| A5 | Press *Turn on the mic* | The chip questions (age band, interests, goal) as before; the mic is still off here |

## B. The tour (first run, six stops on the wall)

| # | Do | Expect |
|---|---|---|
| B1 | Answer the three questions | The wall appears with *1 OF 6* and a *Skip* pill top-right. Everything dims except the first lesson tile, which has a cyan ring and a bouncing arrow. Dee says the stop. The controller helper to the right of the board lights the **trigger** |
| B2 | Press › (bottom-right) | Stop 2: the CONTINUE chapter tile is ringed |
| B3 | Press › | Stop 3: the three filter pills are ringed; › is dimmed until you press a filter |
| B4 | Press *Newest* | Stop 4 by itself: pages and scenery ringed |
| B5 | Press › | Stop 5: Music · gear · Help on Dee's bar ringed; › dimmed |
| B6 | Press the gear, then Done | Stop 6 by itself: the ‹ arrow is ringed and the helper lights **B** |
| B7 | Press **B** (or ‹) | The tour ends; the wall is bright again; Dee says the lessons are on the wall |
| B8 | (Any stop) press *Skip*, ‹ or B early | The wall at once; Dee says you can replay it from the gear. Pressing a lesson tile mid-tour opens it and ends the tour |

## C. The wall

| # | Do | Expect |
|---|---|---|
| C1 | Look at the wall | *Pick a lesson*, 8 tiles: Cargo Crew, Neighborhood Café, Community Garden, then chapter tiles. Each has a picture of its table, a badge, a title, a muted line, minutes. Under the wall: Featured · Newest · Most viewed, ‹ 1 / 3 ›, Scenery: Your room / Nerdy lounge. The bar with the gear is under that |
| C2 | Hover a tile | It lifts a touch; a short pulse in the controller |
| C3 | Press › twice, ‹ once | Pages 2 and 3, then back; page 3 ends with six greyed *COMING SOON* tiles (Route 9, Ten & Trade, Corner Store, Platform Clock, The Tailor's Bench, Mile Marker) that do not press |
| C4 | Press *Newest*, then *Most viewed*, then *Featured* | The order changes; Featured puts the three lessons first |
| C5 | Press a **chapter** tile (e.g. Cargo Crew · Chapter 3) | The lounge hides, the Dock 7 workbench appears with **chapter 3 on the table**, no briefing. Dee welcomes you and names what is on the table |
| C6 | Press **B** | Back to the wall (same as the Back button). Refused while a crate is in your hand |
| C7 | Press a **lesson** tile (hero) | The briefing card as before (Cargo: briefing → demo → practice → What is a fraction? → chapter 1) |
| C8 | Say "open chapter 2 of the café" / "show me the newest" / "open settings" / "how does this work again" | Dee calls the tool; the app does it (tile opens / filter changes / settings open / rundown replays). Ask to open Route 9: she says it is coming soon, once |
| C9 | Back on the wall after C5 | That chapter wears the amber **CONTINUE** ribbon and sorts first among chapters |

## D. Settings that must do something

| # | Do | Expect |
|---|---|---|
| D1 | Voice guide → Off | Dee stops speaking and listening; captions still update from the app; the on-card buttons come back |
| D2 | Captions → Off | The caption line on Dee's bar disappears; On brings it back |
| D3 | Haptic feedback → Off | Tile hover no longer pulses |
| D4 | Show button labels → Off, open a lesson | No grip hint appears when a step lets you grab; On: the helper shows *Grip* for a few seconds |
| D5 | Voice 100% → 75% → … | Dee's volume steps down; Music and Effects the same |
| D6 | Replay the rundown (from the wall) | Stop 1 again |
| D7 | Clear saved data, twice | Label says *Press again to clear*, then *Cleared*: Continue ribbon goes to Cargo chapter 1, the rundown will run on next launch |
| D8 | Reset settings | Switches and volumes back to defaults; language stays |

## G. The ‹ › arrows (bottom-right corner of every screen)

| # | Do | Expect |
|---|---|---|
| G1 | On the welcome card | Both arrows dimmed (the two pills are the choice) |
| G2 | During the questions, press ‹ | Back to the welcome card; press › instead: straight to the wall |
| G3 | During the rundown, press › | Dimmed until the stop is done, then it is Continue; ‹ skips to the wall |
| G4 | On the wall | ‹ dimmed (home); › turns the page |
| G5 | Settings open, press ‹ | Same as Done |
| G6 | Inside a lesson | ‹ = Back to the wall (refused with a piece in hand); › = next chapter, lit only once the current one is accepted |

## E. Returning

| # | Do | Expect |
|---|---|---|
| E1 | Quit, relaunch | Arrival → the two-button card → after the press, **straight to the wall** with "welcome back" and the Continue ribbon where you left off. No questions, no rundown |

## F. Performance (recorded from the Mac while you sit in the lounge)

`adb logcat -v time -s VrApi` for 60 s in the Nerdy lounge scenery, then 60 s in Your room, then inside Cargo
chapter 1. Budget: app CPU/GPU time p95 ≤ 13.9 ms at 72 Hz, no stale frames. Numbers go in DEV-LOG.
