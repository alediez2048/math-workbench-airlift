# Community Garden (CC-GD-05) — headset check

Prereqs: dev mint restarted after the garden proxy config (`~/.config/nerdy/start-dev-mint.sh`) on the home Wi-Fi;
Quest on the same network; controllers awake; build id and APK SHA-256 noted. On the Mac, clear the log first
(`adb logcat -c`) and pull it afterwards (`adb logcat -d -s Unity`). Live guide, mic on, not muted or paused,
unless a step says otherwise. Convention: rows × columns (a 3 × 4 bed is 3 rows of 4).

## Open and briefing

1. From the cards, say "open the garden". Any Cargo or café objects disappear; the garden bench appears
   (grass-green deck, soil beds, trees and bushes at the edges, wooden fence). The card tells the Sunny Plot story
   and says "Say yes or press Start". No grab practice.
2. Tap Mute: Start and Back appear. Unmute: they hide again.
3. Say "yes": chapter 1 · First rows opens with a 3 × 4 bed and strips of 4 and 3.

## Chapters by voice

4. **Chapter 1.** Plant two strips of 4 and one strip of 3; say "check the bed". The guide repeats the app's
   feedback (a row has 3 seedlings; the others have 4). Swap the short strip, check: `3 × 4 = 12`; the rows sprout,
   the watering can passes, a butterfly lands. Say "next chapter".
5. **Chapter 2.** Try to drop the strip of 6 into the 4 × 5 bed: it returns to the tray with a short reason.
   Plant four strips of 5, check: `4 × 5 = 20`.
6. **Chapter 3.** Say "check the bed" first: "Turn the bed to see it from the other side." Say "turn the bed":
   the bed rotates a quarter turn and now reads 4 rows of 3. Check: `3 × 4 = 4 × 3 = 12`.
7. **Chapter 4.** Say "split it at six": refused (the bed has 6 columns). Say "split it at three", check: the
   feedback names the parts and the bed stays editable. Say "split it at five", check:
   `7 × 6 = 7 × 5 + 7 × 1 = 42`; the fence opens.
8. **Chapter 5.** Place the fence by hand after any column 1–6, check: the expression matches your choice, e.g.
   `8 × 7 = 8 × 5 + 8 × 2 = 56`. Say "start this chapter over" and try a different column: also accepted.
9. Ask "what do I do now?" in each chapter: the guide talks about rows, columns, strips and the fence with counts,
   never crates, plates or pastries, and never asks you to plant rows that already grew.

## Chapters by button

10. Tap Mute. Replay chapters 1, 3 and 4 with Turn, Split, Check, Clear, Restart and Next plus the fence by hand.
    Each button does what its voice phrase did. Unmuted once, say "clear the bed": planted strips return to the tray.

## Refusals

11. **Wrong lesson.** In the garden say "deal one round", "split the crate" and "load it". Each gets one sentence
    like "That is not part of Community Garden." and nothing changes.
12. **Held.** Hold a strip (then the fence) and say "turn the bed", "check the bed" or "back to the lessons":
    "Let go of the strip first." and nothing changes. Before an accept say "next chapter": the guide says what is left.

## Interruptions and recovery

13. **Pause.** Tap Pause while the guide talks: it stops, the mic closes, buttons appear. Say "turn the bed":
    nothing happens. Tap Play: one sentence about what to do now.
14. **Headset off.** Mid-chapter, take the headset off for 10 seconds and put it back on: the guide stays paused
    until Play; strips and fence are where you left them.
15. **Back mid-animation.** Tap Back during the turn, and again during growth. Reopen: the bed is in a coherent state.
16. **Offline.** Relaunch, choose "Continue without voice", open the garden and finish chapter 1 with buttons only.

## Carry handle

17. Carry the table by the yellow handle and resize it: the beds, strips, fence, trees and card move together, and
    the turned bed still fits. While holding a strip or the fence, grab the handle: refused. After a carry, strips
    and fence still grab and snap normally.

## Other lessons still work

18. Back to the lessons; open Cargo Crew and run chapter 1 by voice; open the café and run chapter 1 by voice. No
    garden props show in either.

Report: any action the guide claimed without the bed changing, doubled speech, a phrase that did not trigger, a
misread "split it at N", props or trees showing through the card, and anything that looked off-palette.
