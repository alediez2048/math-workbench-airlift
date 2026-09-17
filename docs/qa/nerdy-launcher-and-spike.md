# Nerdy launcher (CC-P0-01) and voice spike (CC-P0-03) — headset checks

## Launcher (after the next Cargo build is installed as com.nerdy.vr)

1. Quest home → App Library. Look for "Nerdy" with the green wordmark on an indigo tile.
   If not in the default list, open the filter dropdown and choose "Unknown Sources".
2. Note the exact path that worked. Cold-launch from there (no ADB, no Mac).
3. Report: name shown, icon correct, launch works.

## Voice spike (development build nerdy-spike.apk; Mac mint server must be running)

1. Mac: `~/.config/nerdy/start-dev-mint.sh` (or confirm http://192.168.86.20:8787/health).
2. Wear the headset; launch Nerdy (the spike build). Accept the microphone permission.
3. Panel in front of you shows state, exchange count, last latency and transcripts.
   The guide should greet you within a few seconds without you speaking.
4. Say "Hi Nerdy, how are you?" and wait. Repeat ten short exchanges.
5. Report: did you hear the greeting; did it hear you (your words appear under "You");
   the latency numbers shown; any stalls or garbled audio; did it survive five minutes.
6. Mac side: `adb logcat -s Unity | grep SPIKE` records the same numbers.

### Result 2026-09-16 (owner)

- Owner heard the greeting; three exchanges logged at 415/430/445 ms; transcripts correct
  after the first (first utterance came back empty and the model replied in Portuguese).
- Quest showed a "controllers required" dialog until a controller was woken; one early
  instance died with an ANR behind that dialog.
