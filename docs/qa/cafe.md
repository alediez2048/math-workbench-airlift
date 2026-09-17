# Neighborhood Café (CC-CF-05) — headset check

Prereqs: dev mint restarted after the café proxy config (`~/.config/nerdy/start-dev-mint.sh`) on the home Wi-Fi;
Quest on the same network; controllers awake; build id and APK SHA-256 noted. On the Mac, clear the log first
(`adb logcat -c`) and pull it afterwards (`adb logcat -d -s Unity`). Live guide, mic on, not muted or paused,
unless a step says otherwise.

## Open and briefing

1. From the cards, say "open the café". The Cargo table, crates and trucks disappear; the café bench appears
   (cream deck, coffee counter, espresso machine, menu board, awning). The card tells the Corner Café story and
   says "Say yes or press Start". No grab practice is offered.
2. Buttons hide while the guide listens. Tap Mute: Start and Back appear. Unmute: they hide again.
3. Say "yes": chapter 1 · Two friends opens with 6 croissants and 2 plates.

## Chapters by voice

4. **Chapter 1.** Put 4 croissants on plate 1 and 2 on plate 2; say "is this right". The guide repeats the app's
   feedback (plates not equal) and does not invent a verdict. Move one across; say "check the order": accepted,
   `6 ÷ 2 = 3`, the plates slide to the guest table and steam rises. Say "next chapter".
5. **Chapter 2.** Say "deal one round" twice: one pastry lands on each of the 3 plates per round. Place the rest
   by hand, check: `12 ÷ 3 = 4`. Say "next chapter".
6. **Chapter 3.** Pack 12 cookies into boxes of 4. Try to drop a fifth cookie into a full box: it returns to the tray.
   Leave one box with 3, check: "Box N has room for 1 more cookie." Fix it, check: `12 ÷ 4 = 3`, full boxes get
   "ORDER UP" and leave on the bike. Two empty boxes stay behind and did not block acceptance.
7. **Chapter 4.** Say "deal one round": the guide says there are no plates in this chapter. Pack 15 muffins into
   boxes of 5, check: `15 ÷ 5 = 3`.
8. **Chapter 5.** Share 12 muffins onto 4 plates, check: the muffins return to the tray and 6 boxes of 3 appear.
   Pack them, check: the card shows `12 ÷ 4 = 3 · 12 ÷ 3 = 4 · 3 × 4 = 12`.
9. Ask "what do I do now?" in each chapter: the guide talks about plates, boxes and pastries with counts, never
   crates, trucks or seedlings, and never asks you to grab pastries that already left.

## Chapters by button

10. Tap Mute. Replay chapters 1 and 3 with Deal round, Check, Clear, Restart and Next only. Each button does what
    its voice phrase did. Say "clear the table" while unmuted once: loaded pastries return to the tray.

## Refusals

11. **Wrong lesson.** In the café say "split it", "load it" and "turn the bed". Each gets one sentence like
    "That is not part of Neighborhood Café." and nothing on the table changes.
12. **Held.** Hold a pastry and say "deal one round", "check the order" or "back to the lessons": "Let go of the
    pastry first." and nothing changes. Before an accept say "next chapter": the guide says what is left.

## Interruptions and recovery

13. **Pause.** Tap Pause while the guide talks: it stops, the mic closes, buttons appear. Say "deal one round":
    nothing happens. Tap Play: one sentence about what to do now.
14. **Headset off.** Mid-chapter, take the headset off for 10 seconds and put it back on: the guide stays paused
    until Play; the pastries are where you left them.
15. **Back mid-payoff.** Accept a box order and tap Back while the bike is leaving. Reopen the café: the chapter is
    coherent (no pastries stuck on the bike or the guest table).
16. **Offline.** Relaunch, choose "Continue without voice", open the café and finish chapter 1 with buttons only.

## Carry handle

17. Carry the table by the yellow handle and resize it with two hands: the café bench, plates, boxes and card move
    together. While holding a pastry, grab the handle: refused. After a carry, pastries still grab and drop normally.

## Cargo still works

18. Back to the lessons, open Cargo Crew, say "yes" and run chapter 1 by voice: no café props show; nothing differs.

Report: any action the guide claimed without the table changing, doubled speech, a phrase that did not trigger,
wrong feedback wording, props showing through the card, and anything that looked off-palette.
