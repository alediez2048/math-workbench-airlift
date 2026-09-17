# Nerdy welcome flow (CC-P0-05/06/07) — headset check

Prereqs: Mac mint server running (`~/.config/nerdy/start-dev-mint.sh`) on the home Wi-Fi until
the https proxy is deployed; Quest on the same network; controllers awake.

1. Launch Nerdy from Library. You should see the consent card only (no table yet).
2. Choose "I'm an adult tester · turn on the mic". Accept the Quest mic prompt.
   The HUD appears; state goes permission → minting → connecting → ready → live guide.
3. The guide greets you and asks your rough age. Answer by voice OR tap a chip.
   Watch the caption line and "You said" line. Answer interests and goal.
4. After the third answer the guide says the lessons are appearing; the three cards show.
   (If it stalls, tap "Skip to lessons".)
5. Point at Neighborhood Café: guide says it is coming soon; nothing opens.
5b. Voice open: with the cards showing, say "open Cargo Crew". The table and briefing should appear exactly
    as when pointing at the card, and the guide should welcome you without asking you to grab anything
    (no strap is shown during the briefing). Say "open the café": it should say coming soon.
6. Point at Cargo Crew: the table appears with the briefing; the guide welcomes you to the
   workbench and asks "would you like to get started?". Say "yes": the briefing should advance
   exactly as if you pressed Begin briefing (voice start, P0-07b).
6b. **Assistant bar check (open defect):** the bar should now sit directly above the lesson
   card, riding with the table (carry the table by the yellow handle: the bar follows). If you
   do not see it, look up about 15 degrees from the lesson card, then report where it is (still
   on the welcome panel? nowhere?). The build logs `[Nerdy] HUD at lesson entry` and
   `[Nerdy] HUD 1.5 s later` with the bar's parent, world position, distance and elevation from
   your head; the Mac pulls these with `adb logcat -d -s Unity | grep "\[Nerdy\] HUD"`.
7. Tap Help on the HUD: the guide explains the current instruction. Ask "what do I do now?"
8. Run the halves loop; after Submit, the guide should react to the shown result.
9. Tap Mute: guide audio stops and mic pauses; Unmute restores.
9b. Tap Pause: the guide stops mid-sentence, the mic closes, the caption reads "Guide paused",
    and nothing is spoken until you tap Play (label flips Pause ↔ Play). After Play the guide
    says one sentence about what to do now and listening resumes.
9c. Music: a soft synthesized loop plays at about 15 percent from the consent card onward; it
    dips while the guide speaks and while the mic is streaming. Tap "Music on" → "Music off"
    to stop it; the choice is remembered on the next launch (on by default).
10. Back to lessons: the table hides and the cards return; the bar returns to the panel.
11. Report: greeting heard, transcripts correct, any Portuguese/wrong-language reply, any
    jump, any stall, latency feel, what looked off-brand, whether the bar was above the
    workbench, and any repeat of the "active response in progress" log warning.

Offline path: choose "Continue without voice": chips write the profile locally, Skip goes to
the cards, captions show app text, HUD state reads "offline guide".

## Dock 7 voice actions (Phase 1R, CC-D-02/D-03)

Prereq: restart the dev mint after pulling the new proxy config (`~/.config/nerdy/start-dev-mint.sh`);
the session tools and rules are read when the mint starts. Live guide, mic on, not muted or paused.

1. **Buttons hide while listening.** Open Cargo Crew by voice. The Begin briefing / Back row and, later,
   the chapter row (Split / Load / Reset / Next / Back) should be invisible and not clickable. Tap Mute:
   they reappear. Unmute: they hide. Pause/Play: same. "Continue without voice": always visible.
2. **Briefing copy.** The card reads "Welcome to Dock 7 ..." and says crate, never strap. Say "yes"
   twice: orientation, then the demo plays.
3. **Replay.** During practice say "show me the demo again": the demo replays and the guide says so.
   While holding the crate say it again: the guide says "Let go of the crate first." and nothing moves.
4. **Start.** Put the crate on the pad; say "start loading". Chapter 1 · Big truck opens; the guide tells
   the story once (not twice).
5. **Chapter actions.** Say "is this right" with an empty bed: the guide repeats the app's feedback
   and does not invent a verdict. Load the crate into the big truck's bed, say "load it": accepted with `1`,
   and the truck turns left and drives out through the DOCK EXIT gate with the crate. Say "next chapter": two pickups back up to the dock and the
   guide tells their story. Say "split it": two 1/2 crates appear. Say "put everything back": loaded crates
   come back out of the beds. Say "start this chapter over": one full crate again.
5b. **Grounding (Round 2).** Ask "what do I do now?" in each chapter. The guide should talk about the
   vehicles backed up to the dock, their beds and the crates to load, and mention cranes rather than
   aircraft. In chapter 4 it should say the rest of the container is not needed. After an accepted load it
   must not tell you to grab crates that just drove away.
6. **Refusals.** Hold a crate and say "split it" or "back to the lessons": one-sentence refusal, nothing
   changes. Before a load is accepted say "next chapter": the guide says what is left.
7. **Buttons still narrate.** Mute, press Load with the beds correctly filled, unmute, press Next: the guide says one
   Dock 7 line for the new chapter (only while not paused).
8. **Back.** Say "back to the lessons": the cards return; the guide says so in one sentence.
9. Report any action the guide claimed without the table changing, any doubled speech, and any
   phrase that did not trigger its action.

## 2026-09-17 additions (build after 110246)

1. **Consent card.** Logo shows. "Voice language" row: English (highlighted) and Español. Tap Español: it
   highlights; tap English again. Choose one, then turn on the mic.
2. **Dee.** English: Dee says "Welcome to Nerdy AI plus VR! My name is Dee ..." Español: the same introduction in
   Spanish. In the lessons Dee stays in the chosen language even if you say something in the other language.
   (Cards and lesson text stay English.)
3. **Card edges.** Consent, welcome, catalog cards, assistant bar and each lesson card: smooth rounded corners, a
   thin light border, a soft shadow; buttons and badges are round-ended pills; catalog art has rounded top corners
   and fades into the card; the assistant caption stays inside the bar.
4. **Dock 7.** Crates twice as tall and deep with large labels; trucks look like trucks, load in the middle, turn
   left and leave through the DOCK EXIT gate one at a time; the ruler marks stay visible past the crate tray.
5. **Café.** The story says Dee is the head barista.
