# Nerdy lounge and onboarding — design

**Date:** 2026-09-17 · **Status:** rewritten 2026-09-17 evening after the owner corrected the architecture;
awaiting owner review · implementation starts after the Friday 2026-09-18 submission · Contracts:
`docs/00-build/FRONT-DOOR-CONTRACTS.md` · Tickets: `CC-FD-*`

The app today opens straight onto a consent card floating in your room. This spec puts an arrival in front of it:
the app opens in the **Nerdy lounge**, a designed room built from the Nerdy style guide, where the logo animates
in, Dee appears, and the onboarding questions we already ask are asked *there*. When onboarding is done the lounge
gives way and the lessons run exactly as they do today.

Structural reference, owner-supplied: `docs/00-build/REFERENCE-THEATRE-ELSEWHERE.md` (transcript plus the
mapping of what we take and what we refuse). Only the structure is borrowed.

**Superseded:** an earlier draft split this into a second Unity scene with a scene router, a persistent core and a
loading screen between two rooms. The owner rejected it on 2026-09-17: *"I dont want two different scenes… its not
two different spaces."* One scene, one place. That deletes the riskiest third of the old plan.

## Owner decisions

| Question | Decision |
|---|---|
| Timing | Build after the Friday 2026-09-18 submission. |
| Shape | **One continuous experience in one scene.** No second scene, no scene router, no trip between rooms. |
| The lounge | **The home.** Arrival, onboarding *and* browsing happen there, every session — it is hidden only while a lesson is running (owner, 2026-09-17). Designed specifically from the Nerdy style guide. |
| Arrival | Nerdy **logo animation** on entry; a **progress bar only when something real is loading**. |
| Dee | Appears **beside the existing onboarding card** — the one that already asks language, interests and goals. **No new assistant**: Dee is the guide we already have. |
| Scenery | The learner picks **AR or VR quickly**, early, and can change it later in settings. |
| What is missing today | A **thorough rundown of the experience** — how it all works — plus an **expanded dashboard and settings**. |
| The wall | The lesson display becomes a **discovery wall** like the reference app: tiles, a toolbar **underneath** with **Featured · Newest · Most viewed**, and a **gear** for settings. |
| More lessons | **Six extra tiles** for other arithmetic concepts, shown as honest *Coming soon* placeholders. |
| Style source | **`NerdyStyle.asset` tokens** / the Live Learning Style Guide (owner, 2026-09-17). |

## Goals

1. Arriving in Nerdy feels like arriving somewhere, not like a card appearing in mid-air.
2. The learner is told, once and clearly, how the whole experience works.
3. Choosing AR or VR takes one press and is never a trap.
4. The dashboard shows everything there is to do and where you left off.
5. Settings are reachable and cover what a learner actually wants to change.

## Non-goals

- No second scene, no scene router, no loading screen between spaces.
- No new assistant character. Dee is the guide that already exists.
- No change to lesson maths, chapters, voice tools or the Cargo Crew lock.
- No accounts, no cloud, no timers, stars or scores. No locomotion.
- The existing onboarding questions are **kept**, not rewritten. They move into the lounge.

## Architecture — one scene

Everything lives in `CargoCrew.unity`, the way the workbenches already do: roots that are shown and hidden, not
scenes that are loaded. The lounge is one more world-locked root.

| Root | Holds |
|---|---|
| `Nerdy lounge` (new) | Room shell, light, logo arrival, Dee's place, the scenery choice, the wall it hangs on; hidden only while a lesson runs |
| `Nerdy welcome` (exists) | The consent / language / interests cards — reparented into the lounge, wording unchanged |
| `Dashboard` (new) | The expanded lesson and chapter tiles, replacing the three flat cards |
| The three workbenches (exist) | Untouched |

Consequences worth stating plainly, because the superseded draft got them wrong:

- **No persistent core.** Nothing has to survive a scene load, because nothing loads. `GuideSession` keeps running
  the way it does today.
- **The concept-intro "once per app run" state keeps working** as-is; the old draft would have broken it.
- **The build ships one scene**, as it does today. No scene-list plumbing, no ordering test.
- **Lesson performance is unchanged**: lounge geometry is disabled the moment a lesson opens, so a lesson costs
  exactly what it costs now. But the room is on screen **every session while browsing**, not just on the first
  run, so its own budget is a shipping requirement rather than a nice-to-have: it is measured as soon as it
  exists, and AR browsing is the fallback if it will not hold.
- `WelcomeFlow` grows new states (`Arrival`, `Scenery`, `Rundown`, `Dashboard`) instead of being replaced.
  `NerdyDirector` gains the lounge; it is not split in two.

## Flow

### First run

1. **Arrival.** The lounge fades up. The Nerdy dots fly in and the logo assembles with the spectrum sweep. A pill
   progress bar appears **only if** something is genuinely still loading — first-frame asset warm-up, the guide
   session connecting, the microphone permission answer — and shows that real progress, never a fake timer.
2. **AR or VR.** Two large pills: *Your room* or *Nerdy lounge*. One press, reversible any time from settings.
3. **Dee.** Dee appears beside the card and gives the greeting she gives today, in the chosen language.
4. **The existing questions.** Consent, voice language, age band, interests, goal — the cards we already have,
   standing in the lounge instead of floating in the dark.
5. **The rundown.** A walk through how the experience works, skippable and replayable.
6. **The wall.** It hangs in the lounge, where lessons are chosen from now on.

### Returning

Arrival → a short welcome back → the lounge with the wall on it, *Continue* tile first. The rundown is replayable
from the gear. The learner is back where they were in two presses.

### Entering a lesson

The lounge and its wall hide, the workbench appears, and the lesson runs exactly as today. Leaving a lesson brings
the lounge and the wall back, in the scenery the learner chose. Nothing loads, so nothing has a progress bar.

## Lounge and Dee

- **VR lounge:** a Nerdy room built from the design system below — seating area, a low table, the wall the
  dashboard hangs on, soft light and a quiet music bed.
- **AR lounge:** the same furniture, glows and accents over passthrough, with no room shell or backdrop.
- Placed once in front of the player, then world-locked, as the welcome panel is today.
- **Dee has no body** (owner, 2026-09-17: *"No need for Dee to have a body right now, this might be too much
  work"*). The assistant we already ship — her voice, her greeting, the orb and the assistant bar — does all of the
  new onboarding. What changes is that she has a *place* in the room beside the card: the existing guide HUD given
  a seat, with an accent glow, so the learner knows where she is speaking from. No mascot, no rig, no lip sync, no
  purchase, nothing to build before the lounge works.

## The rundown — "how this works"

The gap the owner named: nobody has ever explained the experience. Structure follows the reference app
(`docs/00-build/REFERENCE-THEATRE-ELSEWHERE.md`), which teaches each control **where the learner meets it** and
gates every stop on the learner actually pressing the thing.

- Narrated by Dee through the existing exact-words path; captions only when voice is off.
- **Skippable at any moment** by a real, always-visible control — the same button that later means "back to the
  wall" — not a Skip card on the first stop.
- A **helper card** stands in front of the learner showing a controller diagram, highlighting the button being
  named. It is the same diagram the "show button labels" setting governs.
- The continue affordance **pulses until pressed**, so nobody sits waiting for permission.
- Replayable from the gear.

**At the wall (browsing):**
1. **Point and pick** — aim at a tile, press the trigger.
2. **Filters and the gear** — the toolbar under the wall, and where settings and the AR/VR choice live.

**At a workbench (hands-on),** opened for the rundown exactly as a lesson would be:
3. **Grab** — pick a block up and set it down. Generic, never Cargo's locked practice.
4. **Talk to Dee, and the assistant bar** — say something; meet Pause, Mute, Music, Again, Help.
5. **Your table** — the yellow handle moves it, two hands resize it. The most useful thing nobody discovers.
6. **Leaving** — the back control returns to the wall from anywhere, at any time.

## Dashboard — the discovery wall

The three flat cards become a wall, modelled on the reference app's content wall (Theatre Elsewhere: a grid of
tiles with a filter bar beneath it and a settings popover). Only the structure is borrowed; the art, wording and
palette are Nerdy's own, from the style guide below.

### Tiles

| Group | Count | Behaviour |
|---|---|---|
| Lesson heroes | 3 | Cargo Crew, Neighborhood Café, Community Garden — open at the briefing |
| Chapter tiles | 15 | Five per lesson; open that chapter directly |
| Coming soon | 6 | Other arithmetic concepts, visibly not yet available |

Each tile carries art, title, subject badge and estimated minutes. The chapter you were last in wears a
**Continue** ribbon and sorts first. Hover raises the tile, ticks and pulses haptics.

**The six coming-soon tiles** are honest placeholders, in the spirit of the preview cards that shipped before:
greyed, marked *Coming soon*, not openable, and Dee says so plainly if asked rather than pretending. Six proposed worlds, mocked up at
`https://claude.ai/artifact/Sv1f3tnPP6BA84CzDmTbBA` for the owner to confirm or rename. Each is a workplace with a
job, the way Dock 7, the café and the garden are — not a worksheet with a theme:

| Tile | Subject | The job |
|---|---|---|
| **Route 9** | Addition and subtraction | Riders board and leave at each stop; keep the bus's tally right |
| **Ten & Trade** | Place value | Ten loose bolts snap into a rod, ten rods fill a crate; the shelf label must match |
| **Corner Store** | Money | Build the price out of coins, then count the change back |
| **Platform Clock** | Telling time | Move the hands to the departure time so the 3:45 leaves |
| **The Tailor's Bench** | Measuring | Measure the cloth before cutting — the Dock 7 ruler, in whole units and halves |
| **Mile Marker** | Rounding and estimating | Round the distance to the nearest ten and load enough fuel | They are catalog entries with `Playable = false`; nothing else about
them is built, and they never imply a date.

### Toolbar

A bar sits **under** the wall, the way the reference app puts its filter bar under the grid:

`Featured · Newest · Most viewed` + a **scenery selector** and a **gear**, in the corner

| Filter | Order | Source |
|---|---|---|
| Featured | Authored rank: the three lessons, then the suggested next chapter, then coming soon last | Catalog |
| Newest | Release date, newest first | Catalog |
| Most viewed | Open count, then recency | `LibraryStore` |

The gear opens the settings menu below. Beside it, as in the reference app, sits the **scenery selector**: the
AR / VR choice the learner made on arrival, changeable in one press while browsing and remembered as the default.

Subject filters and bookmarks are **not** in this bar: the owner asked for
three filters and a gear, and twenty-four tiles do not need more sorting than that. Both can be added later without
changing the layout.

Chapter titles and numbers are read from the stations, never authored twice, so the wall cannot drift. Tile art is
cropped from the seated-head preview renders and committed under `Assets/`, because `artifacts/` is gitignored; the
six coming-soon tiles use a generated gradient plate instead of art.

## Settings

Opened by the gear on the dashboard toolbar, and from the assistant bar inside a lesson. A basic,
legible menu — the controls a learner actually reaches for, not a preferences tree.

| Setting | Default | Effect |
|---|---|---|
| Scenery | AR | Your room or the Nerdy lounge |
| Voice guide | On | Mic and speech; off leaves captions |
| Voice language | English / Español | Reconnects the guide session; disabled inside a lesson |
| Captions | On | The guide bar caption line |
| Haptic feedback | On | All pulses route through one place |
| Show button labels | On | Floating controller labels |
| Voice / music / effects volume | 100 / 40 / 75% | Three buses |
| Replay the rundown | — | Runs it again |
| Clear saved data | — | Bookmarks, counts, profile; confirm step |
| Reset settings | — | Defaults |
| Version | — | App version and build id |

Saved as one versioned JSON record with defaults that survive a missing or older file. Reachable from the dashboard
and from the assistant bar inside a lesson. Changing the language tears down and reconnects the guide session, so
it is offered outside lessons only, with a warning that Dee restarts.

## Design system compliance

The authoritative source is the owner's **Live Learning Style Guide** (Claude artifact
`https://claude.ai/artifact/5LwzYqPEPoXTJ4PcJv55mr`, read 2026-09-17), implemented as
`NerdyStyle.asset` (`Assets/Airlift/Fonts/NerdyStyle.asset`) and pinned by `NerdyStyleTests`. Every colour,
font, radius, type size and sprite on every new new surface — intro, lounge, wall, tiles, rundown cards,
settings, loading — comes from that asset. No literal colour or font is written into a lounge or dashboard script or
scene object.

Verified against the guide: the asset's surfaces, the six accents, both gradients, the radii (12 / 14 / 20 /
100) and the type ramp (72 / 56 / 36 / 28 / 20 / 16 / 14 / 10) match the published values exactly. What follows
adds the guide's *recipes and restraint rules*, which the token asset alone does not carry.

`docs/00-build/STYLE-GUIDE.md` is the older Cargo-era document (Nunito, navy `#18233B` palette). It stays as
history; where the two disagree, the Live Learning Style Guide wins.

### 2D surfaces

| Element | Recipe from the guide |
|---|---|
| Backdrop | `baseColor` `#202344` plus the **page glow**: a cyan radial at 35% from the upper left and an amber radial at 15% from the right |
| Card / panel | `surface` `#161C2C`, radius 20, **no border and no drop shadow**; 28 px inner padding, 28 px between cards |
| Floating bar (wall filter bar, guide bar, settings header) | The **glass recipe**: glass gradient fill, 1 px `line` border, radius 14, 12 px blur, padding 17.5 × 28 |
| Primary button | `brandGradient` on a full pill, radius 100, padding 12 × 22, Poppins Medium 14 |
| Secondary button | Glass fill with a `line` border; hover shifts the border to lavender |
| Quiet button | Text only in `textMuted`, white on hover |
| Circle action (open tile, next page) | 34 px circle, 1.5 px white outline, transparent fill |
| Badge (subject, "New") | `brandGradient`, radius 20, padding 2 × 10, uppercase 10/24 |
| Eyebrow label | Karla Medium 11, uppercase, letter-spacing 0.14em, in `lavender` |
| Headings | Poppins: display 72/1.05 at weight 400; H1 56/1.0, H2 36/1.1, H3 28/1.15 at weight 500; tracking −0.015 to −0.02em |
| Subhead | Karla Medium 20/1.4 |
| Body / small | Poppins 16/24 and 14/21; a semibold lead-in phrase then plain text |
| Hover / focus | 2 px `cyan` outline, 3 px offset — the guide's focus ring, reused as the VR hover highlight |
| Tile art | Product art over `spectrumGradient` or cyan, fading into `surface`; title bottom-left 28–36, muted line under it |

**Restraint rules, straight from the guide's do and avoid lists:**

- The spectrum gradient carries **one or two elements per screen**, never more: the hero word, or the tile art.
- Accents arrive as **gradients or glows, never flat fills over large areas**.
- **No drop shadows, no hard borders on cards.** Depth comes from blur, glow and gradient.
- Light backgrounds are for mock UI inside a card only. `paper` `#FAF9F5` never fills a panel or the room.
- Every heading is paired with one short muted line under it.

**Conflict to resolve (owner decision, tracked in *Open items*):** the September 2026 card polish added a
hairline stroke and a soft drop shadow to the cards, which the guide's avoid list rules out. The front door
follows the guide — flat cards, no shadow — and the existing lesson cards either keep their polish (documented
exception) or drop the shadow for consistency. This is a look question, so it goes to the owner on the headset
rather than being settled here.

### 3D lounge — the guide extended to materials and light

The guide is a flat-UI document; it has no 3D, material or motion section. These derived rules keep the room in
the same visual language and are what the lounge tests enforce:

- **Room palette:** floor, walls and furniture from `baseColor` and `surface`, with value shifts only. No hue
  the guide does not list.
- **Light as the guide's page glow:** a cool cyan glow entering from one upper side and a warmer amber glow from
  the opposite side, at roughly the guide's 35% / 15% weighting, over a low ambient. This is the room-scale
  translation of the backdrop, and it sets the lounge's mood without adding hues.
- **Materials:** matte and satin, in the rounded toy style already used on the workbenches (`RoundedBoxMesh`).
  No chrome, no mirror, no shadow-casting drama. Frosted glass is reserved for floating bars and panels, exactly
  as the guide reserves it for the nav.
- **Accents:** rings, floor lights, rundown markers and Dee's pointing beam glow rather than paint — emissive with
  bloom, drawn from the accent tokens, with the spectrum reserved for one feature per view.
- **Text in world space:** the same type tokens and minimum cap height as the lesson cards; never smaller.
- **AR mode:** the same furniture, glows and accents; the room shell and backdrop are simply absent so
  passthrough shows through, and the glows become local light around the props.

### Enforcement

`FrontDoorStyleTests` (new, EditMode) walks the lounge and dashboard roots and fails on: a `Graphic`, `TMP_Text` or
renderer colour that is not a `NerdyStyle` token within tolerance; a font outside Poppins and Karla; an
off-token corner radius; a stencil mask; a drop shadow on a lounge or dashboard card; more than two spectrum-gradient
elements visible in one view; or `paper` used as a panel fill. It mirrors `NerdyStyleTests`,
`TypographyAssetTests` and `CardPolishTests`, so the lounge cannot drift the way the early catalog cards did.

## Testing

- **EditMode, written first:** the new `WelcomeFlow` states and guards; dashboard build, filters and Continue;
  bookmarks and counts; settings defaults, round-trip and version upgrade; rundown gating; `Open(int)` per station;
  arrival progress reporting real work.
- **Scene tests** for the lounge and dashboard roots, in the style of the existing wiring tests.
- **Unchanged:** the Cargo golden voice recording and every existing suite. `Open()` with no argument must still
  reproduce the golden script byte for byte.
- Every new suite registered in `scripts/cargo-milestones.json`, or the wrapper skips it.
- **Evidence:** seated-head renders of arrival, lounge in AR and VR, dashboard, settings and each rundown stop,
  reviewed before any build; then the wrapper run, APK, md5 check and a headset QA script (`docs/qa/lounge.md`).
- **Performance:** a Quest 3S budget check as soon as the VR lounge exists — three post-warm-up runs, CPU/GPU p95
  ≤ 13.9 ms. Lessons are measured separately and must not move from where they are today.

## Risks

| Risk | Handling |
|---|---|
| A VR room on top of MSAA 4x and full render scale, on screen every session | Budget check as soon as the room exists, treated as a release gate; the room is disabled during lessons; AR browsing is the fallback |
| The lounge swallowing the onboarding we already have | The existing cards move in unchanged; their tests keep passing |
| The rundown feeling like homework | Skippable at any time, replayable from settings, five short stops |
| Chapter tiles skipping needed grounding | Chapter 1 keeps briefing and concept intro; Cargo keeps its practice step; Continue is offered first |
| Card polish versus the guide's "no shadows" rule | Owner decides on the headset (see the design system section) |

## Build order

1. **Lounge shell** — the room from the tokens, AR/VR choice, the existing onboarding cards reparented into it,
   Dee beside them.
2. **Arrival** — logo animation and a progress bar driven by real startup work.
3. **Rundown** — the five stops and their replay.
4. **Dashboard** — tiles, Continue, filters, bookmarks, `Open(int chapterIndex)`, tile art.
5. **Settings** — the full panel, including scenery and the language reconnect.
6. **Polish, performance, QA, acceptance.**

Each stage is independently playable on the headset and ends in a build the owner checks.

## If time contracts

Cut the VR room first — AR ships, the lounge becomes furniture and glows in your own room, and the wall floats in
passthrough the way the lesson cards do today — then dashboard filters, then the arrival animation. **The rundown and the onboarding questions are not candidates** — explaining the experience
is the point of this work. Cargo Crew never regresses to buy time.

## Open items for the owner

- ~~Supply the Nerdy logo as a vector~~ — **done 2026-09-17**: `unity/Assets/Airlift/Branding/nerdy-logo-green.svg`
  (4001 × 1635, VTracer-traced paths). The arrival animation is authored from its paths.
- Decide whether AR or the VR lounge is the shipped default after seeing both.
- Confirm or rename the six coming-soon worlds (mockups linked above).
- Settle the card-polish conflict: keep the shadow and hairline on lesson cards, or drop them to match the guide.
