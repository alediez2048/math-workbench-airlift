# Phase 1R — Voice-first Dock Crew: story and expanded fractions (draft for owner approval)

Status: **draft, not approved, nothing implemented from this document.** Written 2026-09-16 20:00
from the owner's headset report on build cargo-20260916-190518. Deadline context: internal target
Friday 2026-09-18 18:00 CDT, hard deadline 23:59.

## 1. What the owner asked for

- **Voice works.** "Start the Cargo lesson" opens it; "show me the demo" runs the demo.
- **Buttons on the workbench screen stopped working.** The owner would rather the screen show basic
  instructions and story, and do everything by voice: restart the demo, move to the next lesson, ask
  questions about the lesson.
- **No story.** The owner proposes dock workers choosing the right amount of cargo per container by
  splitting merchandise into chunks, and asked Claude to choose the storytelling.
- **Expand the lesson.** Today there is one block split into halves. Add chapters: halves into
  fourths ("the truck can now fit four pieces"), then adding cargo so it fits.

## 2. Chosen story: "Dock 7"

**Setting.** The learner joins the crew at Dock 7 of a busy harbor. A cargo ship has unloaded goods in
full crates. Vehicles line up at the dock, and each vehicle's cargo space is a share of one container.
The learner is the **load planner**: they split crates into equal chunks so every vehicle leaves with
exactly the right amount. Nerdy is their dock partner and speaks the story; there is no extra character.

**Why this story.**
- It keeps the one representation that already works on the headset: a length on a 0-to-1 ruler. The
  ruler becomes the **container floor**, so "one whole" is always a visible, fixed container. That keeps
  the invariant that every fraction refers to a visible equal whole.
- Splitting becomes a reason, not an instruction: smaller vehicles need smaller equal chunks. Each
  chapter introduces a new vehicle, which motivates the next denominator.
- Adding fractions has a natural job: topping up a partly loaded container.
- It reuses what is built: truck, containers, staging pads and parcels on the terminal, the orange
  piece (re-labelled a crate), the snap-to-ruler docking, and a model that already measures a whole in
  8 cells (halves are 4 cells, quarters 2, eighths 1).

**Rejected alternatives.** Airport air-freight weights would make weight the whole, which a table
cannot show as a visible length. Pizza/food sharing loses the cargo world already built. Warehouse
shelves are close to Dock 7 but have weaker "vehicle leaves" payoffs.

**Language.** "Crate" replaces "strap". "Load" replaces "Submit". Every fraction is said as a share of
"one container". The guide never claims real loading or safety advice.

## 3. Step 1 — Voice-first workbench

### 3.1 The screen becomes a story card

The lesson panel shows, top to bottom:
1. **Chapter title**, e.g. "Chapter 3 · Four vans".
2. **Story line**, one or two sentences, e.g. "Four delivery vans pulled in. Each van fits one quarter of
   a container."
3. **Task line**, e.g. "Split each half crate into two equal chunks, then load all four."
4. **Expression line** from the model once earned, e.g. `1/4 + 1/4 + 1/4 + 1/4 = 1`.
5. **Say-hints** (not buttons), e.g. `"split it" · "load it" · "show me again" · "next chapter"`.

### 3.2 Voice actions

Every voice action calls the same C# method the button calls today. Math and verdicts stay in C#; the
guide narrates the returned result and never states a verdict the app did not return.

| Learner says (examples) | Tool | App action | Allowed when |
|---|---|---|---|
| "open Cargo Crew" | `open_lesson` (built) | open card | cards showing |
| "yes", "start", "next step" | `advance_step` (built) | briefing → demo → practice | a step button would be active |
| "show me the demo again", "restart the demo" | `replay_demo` | OnboardingDirector.Help | practice or ready, nothing held |
| "split it", "cut it in half" | `split_cargo` | CargoLessonDirector.Split | chapter allows a split, nothing held |
| "load it", "check my load", "is this right" | `check_load` | CargoLessonDirector.Submit | chapter active, nothing held |
| "put everything back", "reset" | `reset_cargo` | ResetPieces | chapter active, nothing held |
| "next chapter", "next lesson" | `next_chapter` | start next chapter | current chapter loaded correctly |
| "start this chapter over" | `restart_chapter` | reset chapter to its start | chapter active |
| "back to the lessons" | `back_to_lessons` | OnLessonBack | lesson open, nothing held |
| any question | none, or `request_help` (built) | answer from lesson_state facts | always |

Refusals return a reason the guide says in one sentence ("Let go of the crate first").

### 3.3 Concern and decision: keep a fallback

Removing every button would make the lesson unusable whenever voice is unavailable: "Continue
without voice", mic declined, Wi-Fi or mint server down, Mute, Pause, or a noisy room. **Decision:**
the buttons stay in the scene but are **hidden while the live guide is listening**, and reappear
automatically when the guide is offline, muted or paused. One rule in `GuidePolicy` decides this and
is unit-tested. The owner's voice-first look is the default experience.

### 3.4 The button regression comes first

The owner reports the on-screen buttons stopped working on build 190518. The fallback depends on them,
so this is diagnosed before anything else, not hidden.
- Evidence so far: the assistant bar now sits where intended (headset log 19:46: 1.30 m from the head,
  13 degrees up, facing the learner). Its ray surface is sized to the bar (0.92 by 0.10 m), not
  oversized. The bar and the lesson card share the same plane, 2 cm apart.
- Unknown: which buttons failed (the lesson card's Begin/Split/Submit, or the bar's Pause/Again/Mute/
  Help), and whether presses reach Unity at all.
- Method: log every UI pointer select with the hit object and canvas, reproduce on the headset, then fix
  the cause. Candidates to rule in or out: the two ray surfaces on one plane, the re-parented HUD
  graphics, and the new pill layout.

## 4. Step 2 — Story layer

- **Briefing** (screen + guide): "Welcome to Dock 7. Ships bring full crates. Trucks, pickups and vans
  each take an equal share of a container. You're the load planner."
- **Practice** keeps the controller practice, re-labelled: "Move a crate onto the loading pad."
- **Container floor skin** on the ruler: container outline, 8 faint slot marks, "ONE CONTAINER" label,
  0 and 1 at the ends, and half/quarter ticks that appear once that chapter introduces them.
- **Vehicle bays** on the terminal: the existing truck plus small toy pickups and vans (rounded-box
  meshes like the current props). Only the current chapter's vehicles are shown.
- **Payoff per chapter:** when the app accepts a load, the matching vehicle gets a "LOADED" tag and
  rolls a short distance out of its bay. No stars, timers or scores.
- **Guide context:** each chapter's story, vehicles and target go into `lesson_state`, extending the
  step grounding built tonight (`on_table_now`, `can_grab_now`).

## 5. Step 3 — Expanded chapters

All chapters use one container (8 cells). Docking stays neutral; a wrong load stays on the floor and is
repairable.

| # | Chapter (story) | Starts with | Learner does | Accepted when | Expression shown | Maps to old ticket |
|---|---|---|---|---|---|---|
| 1 | **Big truck**: "The truck takes a full container." | one crate, 1 | load it along the floor | floor = 1 | `1` | P1-04 |
| 2 | **Two pickups**: "Each pickup fits half a container." | one crate | split into 2, load both | floor = 1, two 1/2 pieces | `1/2 + 1/2 = 1` | P1-05, P1-06 |
| 3 | **Four vans**: "The truck can now fit four pieces." | two 1/2 crates | split each half, load all four | floor = 1, four 1/4 pieces | `1/4 + 1/4 + 1/4 + 1/4 = 1` | P2-01 |
| 4 | **Same share, smaller boxes**: "A pickup wants half a container, but only quarter boxes are left." | four 1/4 crates, half-mark on floor | load chunks up to the half mark | floor = 1/2 using quarters | `2/4 = 1/2` | P2-03, P2-04 |
| 5 | **Top it up**: "This container is already half full. Fill it with quarter boxes." | locked 1/2 already on the floor, four 1/4 in tray | add quarters until full | floor = 1 | `1/2 + 1/4 + 1/4 = 1` | P2-02 |
| — | **Dispatch** | all chapters done | watch vehicles leave; hear a one-line recap of what was actually completed | — | — | P3-02 |

Wrong-load feedback describes the load, never the learner, e.g. "That fills three quarters of the
container. One more quarter fits." Chapter 4 accepts only quarters, and if a 1/2 crate is used it says
"That is half, but this pickup's boxes must be quarters."

## 6. Architecture changes (kept small)

- **Chapter data:** `CargoChapter` records (id, title, story line, task line, starting pieces, locked
  pieces, which pieces may split, target quantity, allowed denominators, vehicles). Pure C#.
- **Model:** `CargoLessonModel` generalises from fixed whole/halves to a chapter: split any unlocked,
  undocked piece with an even cell count into two equal children; evaluate the floor against the
  chapter target on Load only. The existing 8-cell `PlacementState` and `FractionValue` already support
  quarters and eighths.
- **Director:** a pool of piece views (1 whole, 2 halves, 4 quarters) activated per chapter, replacing
  the three fixed views. Snap and grab code is unchanged.
- **Guide:** new tools in `services/guide-proxy/lib/sessionConfig.js` and handlers in `NerdyDirector`,
  each returning the step result JSON already used by `open_lesson`.
- **Screen:** `CargoLessonDirector.Refresh` writes the story card fields; the button row is shown
  only when the fallback rule says so.

## 7. Tickets, order and cut line

| Ticket | Outcome | Size | Needs | Priority |
|---|---|---|---|---|
| CC-D-01 | Workbench buttons respond again (diagnosed on headset) | S | — | must |
| CC-D-02 | Voice actions for demo, split, load, reset, chapter and back; buttons only when voice is unavailable | M | D-01 | must |
| CC-D-03 | Dock 7 story: briefing, crate language, container floor skin, story card screen | M | D-02 | must |
| CC-D-04 | Chapter data and generalised model with tests (chapters 1–2 ported, no behaviour change) | L | D-03 | must |
| CC-D-05 | Chapter 3: four vans, quarters | M | D-04 | must |
| CC-D-06 | Chapter 4: same share, smaller boxes (2/4 = 1/2) | M | D-05 | should |
| CC-D-07 | Chapter 5: top it up (1/2 + 1/4 + 1/4 = 1) | M | D-06 | could |
| CC-D-08 | Vehicle bays, LOADED payoff and dispatch recap | M | D-05 | could |
| CC-D-09 | Owner headset acceptance of the whole journey, docs, commit on acceptance | M | all built | must |

**Proposed schedule.** Wednesday night: D-01. Thursday: D-02 through D-05 with one headset check at
midday (after D-03) and one in the evening (after D-05). Friday morning: D-06, then D-07 and D-08 only
if D-06 is accepted by noon. Friday 15:00: D-09 acceptance run. 18:00: internal target.
**Cut line:** chapters 1–3 with the story and voice actions ship even if 4, 5 and dispatch do not.

Each ticket follows the existing method: failing EditMode test first, editor drive of the path,
wrapper build, install with md5 check, owner headset check. No commit until the owner accepts; no push.

## 8. Tests to add

- Chapter model: splits allowed/refused per chapter, targets per chapter, locked piece cannot move or
  split, wrong loads stay editable, quarter-only rule in chapter 4.
- Voice tools: each tool refuses in the wrong state and returns the same step JSON as buttons.
- Fallback rule: buttons visible exactly when the guide is offline, muted or paused.
- Wiring: story card fields exist, piece pool has 1 + 2 + 4 views, vehicles per chapter exist.
- Proxy: tool list and grounding rules (`node --test`).

## 9. Risks

- **Voice misfires** on background speech. Mitigation: every tool is state-gated and the guide narrates
  what happened, so a wrong action is visible and reversible (reset, restart chapter).
- **Model skips a tool** and only talks. Mitigation: proxy rule "never say an action happened unless the
  tool returned ok true", plus Help and fallback buttons.
- **Time.** Chapter generalisation (D-04) is the largest risk; it lands before any new chapter so
  chapters 1–2 cannot regress unnoticed.
- **Performance.** Seven piece views and a few toy vehicles are small; check frame rate at the D-05
  headset check.

## 10. Questions for the owner (defaults apply if unanswered)

1. **Which buttons stopped working:** the lesson card (Begin briefing, Split, Submit) or the assistant
   bar (Pause, Again, Mute, Help)? Default: diagnose both.
2. **Fallback buttons when voice is unavailable:** keep them? Default: yes, hidden while the guide listens.
3. **"Next chapter" before finishing the current one:** allow it for demos? Default: no; the guide says
   what is left.
4. **Story name "Dock 7" and vehicles truck → pickups → vans:** OK? Default: yes.
