# Nerdy AI + VR — Math Workbench: Airlift

A mixed-reality math lounge for Meta Quest 3 and 3S, built in Unity for the Nerdy AI Hackathon
(September 13 to 18, 2026). You arrive in a lounge, Dee, a live voice guide, shows you around, and
three hands-on lessons wait on the wall: fractions at a harbor dock, division in a café, multiplication
in a garden. The math is checked by deterministic code; the AI explains, never grades.

![Arrival in the lounge: the Nerdy AI+VR logo assembles on the board and Dee's welcome card appears](docs/media/demo.gif)

**Watch the 80-second demo:** [nerdy-ai-vr-demo.mp4](https://github.com/alediez2048/math-workbench-airlift/releases/download/v0.2-hackathon/nerdy-ai-vr-demo.mp4)
(Quest 3S recording, passthrough, adult tester) · **Landing page:** [nerdy-vr-landing.vercel.app](https://nerdy-vr-landing.vercel.app)
· **Walkthrough with pictures:** [docs/WALKTHROUGH.md](docs/WALKTHROUGH.md)

## What you do

| | |
|---|---|
| ![Meet Dee](docs/media/02-meet-dee.jpg) | **Arrive.** The board in front of you lights up with the logo, then *Meet Dee, your Nerdy AI + VR guide*. One press on *Let's begin*. |
| ![The wall](docs/media/04-wall-tour.jpg) | **Take the tour.** Dee walks you through the lesson wall in six stops: filters, pages, the Nerdy lounge scenery, settings, and your first lesson. Everything she is not talking about dims until you press the highlighted control. |
| ![Cargo Crew](docs/media/10-dock7-quarters.jpg) | **Cargo Crew · Fractions.** You are the load planner at Dock 7. Split one container into halves and quarters with the splitter, load trucks, pickups and vans, and see 2/4 = 1/2 on the ruler under the beds. Five chapters. |
| ![Neighborhood Café](docs/media/11-cafe-plates.jpg) | **Neighborhood Café · Division.** Deal pastries onto plates a round at a time, pack orders into full boxes, and check the order when you think it is fair. 6 ÷ 2 through a fact family with 12 muffins. |
| ![Community Garden](docs/media/13-garden-sunflowers.jpg) | **Community Garden · Multiplication.** Plant seedling strips row by row, turn the bed to see 3 × 4 become 4 × 3, and split a bed with a fence: 7 × 6 = 7 × 5 + 7 × 1. Accepted rows sprout. |
| ![Settings](docs/media/06-settings.jpg) | **Make it yours.** Your room (passthrough) or the Nerdy lounge, English or Spanish, captions, volumes, Play/Stop for Dee, replay the tour, and an Age pill that gates the microphone. |

## How it works

- **Dee is a live voice guide.** The headset talks to OpenAI's Realtime API over a WebSocket. A small Vercel
  function ([services/guide-proxy](services/guide-proxy)) mints short-lived, budgeted client secrets and owns
  the provider key; the APK never holds it. Dee's persona and her tools are authored on the server.
- **Dee acts through tools, and the app decides.** "Dee, open Cargo Crew", "split it", "deal a round",
  "turn the bed", "next chapter", "open settings" each become a tool call the app validates before anything
  moves. Every tool has a button twin, so the app works with the microphone off.
- **The math is deterministic C#.** Fractions, quotients and products are checked by code against a visible
  whole; the AI reads the result out and helps, it never judges correctness. Wrong arrangements stay editable.
- **Privacy by construction.** No accounts, no names, no images, no persistent device identifiers, no raw audio
  stored. The preview is for adult testers: the microphone stays off until the learner answers *adult* on the
  age question, and Stop cuts speech and listening at once.
- **Built with** Unity 6000.6.0f1 (URP), Unity OpenXR + Meta XR SDK (passthrough, controllers, ray and grab),
  TextMeshPro, and a design system with Poppins and Karla.

## Build and run

You need Unity 6000.6.0f1 with the Android modules, a Quest 3 or 3S in developer mode, and `adb`.

```bash
git clone git@github.com:alediez2048/math-workbench-airlift.git
cd math-workbench-airlift
# open unity/ in Unity 6000.6.0f1 once so it imports (about 10 minutes the first time)

# preview build: builds, installs beside any release build, and launches on the headset
bash scripts/lounge-preview.sh

# start as a brand-new learner
adb shell pm clear com.nerdy.vr.lounge
adb shell monkey -p com.nerdy.vr.lounge -c android.intent.category.LAUNCHER 1
```

Voice needs Wi-Fi on the headset and a deployed guide proxy. Without it, Dee is offline and the app runs
on buttons and captions. To run your own proxy, see [services/guide-proxy/README.md](services/guide-proxy/README.md);
the OpenAI key lives only in that function's environment.

Tests are Unity EditMode suites under `unity/Assets/Airlift/Tests`. The Unity Pipeline CLI runs them:

```bash
export UNITY_PROJECT_PATH=$PWD/unity
unity command run_tests editor LoungeRoomTests testName false true 300
```

## Repository map

| Path | What is there |
|---|---|
| [unity/](unity) | The Unity project. Scripts under `Assets/Airlift/Scripts` (Lounge, Welcome, Guide, Presentation, Catalog, Lessons), scene builders under `AgentScripts`, tests under `Assets/Airlift/Tests` |
| [services/guide-proxy/](services/guide-proxy) | The Vercel function that mints Realtime sessions and holds Dee's persona and tools |
| [site/](site) | The landing page, deployed to Vercel |
| [docs/WALKTHROUGH.md](docs/WALKTHROUGH.md) | The experience step by step, with pictures and Dee's voice commands |
| [docs/00-build/](docs/00-build) | Build docs: dev log, plans, contracts, tickets, style guide, privacy gate, release gates |
| [docs/qa/](docs/qa) | Headset test scripts and evidence |
| [docs/plans/](docs/plans) and [docs/superpowers/specs/](docs/superpowers/specs) | Approved plans and design specs |
| [docs/media/](docs/media) | Headset stills and the demo GIF |
| [docs/history/](docs/history) | The first days' planning documents, unchanged |
| [CLAUDE.md](CLAUDE.md) | Working instructions and the current state for the coding agents that built this |

Start with [docs/README.md](docs/README.md) for the documentation map.

## Status

Built in five days by one person with Claude Code and Codex. Three playable lessons, the lounge, the tour,
settings and the voice guide are on the headset and were tested by the owner on September 18. Not an
app-store release; not an official Nerdy or Meta product. Learning outcomes have not been evaluated, and a
child-facing release would need its own privacy and safety review ([docs/00-build/PRIVACY-GATE.md](docs/00-build/PRIVACY-GATE.md)).

## License

[MIT](LICENSE). Fonts are under the SIL Open Font License; see `unity/Assets/Airlift/Fonts`.
