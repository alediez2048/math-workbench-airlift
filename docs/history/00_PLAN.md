# WORKBENCH — Nerdy AI Hackathon execution plan

> **Legacy WebXR plan.** Preserved as research history only. Do not implement
> from this file. The authoritative Unity plan is `UNITY_PLAN.md`.

Deadline: Friday Sep 18 2026, 11:59 PM CDT. Demo Day Sep 25.
Today: Monday Sep 14. Five build days, Mon through Fri. Budget ~45 focused hours.
Submit target: Friday 6:00 PM CDT. Do not use the last six hours.

---

## 1. What you are building, in one paragraph

Workbench is a mixed-reality math manipulative that runs on the child's real kitchen
table through Quest 3 passthrough. The child builds one real object across three jobs,
and each job is a place where arithmetic does actual work: bundling coins to pay for
lumber, tiling a countertop, and measuring a cut. The child makes quantity with their
hands instead of selecting a correct answer. A push-to-talk AI tutor asks the child to
explain how they did it, and Claude classifies the strategy the child used into a
misconception taxonomy that streams to a web dashboard a parent or a Varsity Tutors
tutor can open. The headset is the input device. The mastery model and the web tier are
the product.

Working title: Workbench. Alternates if it collides: Bench, The Build Table, Handbuilt.

---

## 2. Three findings that reshape the submission

### 2.1 Nerdy exited the schools business in Q2 2026

Varsity Tutors for Schools was shut down alongside First Tutors UK. Revenue $43.3M,
down 4% YoY. Memberships 29.1K, down 5%. FY26 guidance cut to $168-175M. Year-end cash
guided to $30-32M. Cohn's stated identity for the company is now "a focused consumer
learning company built around one connected system for learning, tutoring, and progress."

Consequence: never say classroom, district, site license, or school deployment. Pitch the
consumer parent and membership retention. This is the single most common own-goal
available in this track and most entrants will commit it.

### 2.2 Meta's minimum age for Quest is 10, and the hardware does not fit young children

Meta lowered the Quest floor to 10 (preteen accounts, parent-created and supervised).
Quest 3 accommodates IPD 53-75mm with optimal performance at 56-70mm. Against pediatric
IPD norms, the share of children who fit the usable range is roughly: age 6 = 44%,
age 7 = 56%, age 8 = 71%, age 9 = 77%, age 10 = 86%, age 11+ = 92-97%.

Consequence: a Quest product cannot serve K-4 at all, and claiming "K-5 on Quest" is
factually wrong about Meta's own terms. Volunteer this before a judge finds it. The
headset build targets grades 3-5 content with ages 10-11 as the compliant headset band;
ages 8-9 get the same engine through the flat web build; K-2 is a documented tablet-AR
roadmap. Naming your own biggest weakness first, with the data, is the highest-credibility
paragraph in the whole submission.

### 2.3 Nerdy already ships ten free browser math games, and they are all the mechanic you were about to build

varsitytutors.com/practice/games has 28 free games, 10 math. Holo-Math Campaign: "blast
targets with correct answers." Number Ninja: "slash through numbers to solve equations."
Equation Invaders. Every Quest store math app is the same thing: shoot, whack, or slash
the correct answer under time pressure.

Consequence: an arithmetic shooter in VR is Holo-Math Campaign with a $500 hardware
prerequisite. Knowing their catalogue by name and rejecting the mechanic on research
grounds is a large credibility play that almost no entrant will make.

### 2.4 One rule that makes the video the deliverable

The contest terms say judges need not test entries and may evaluate on descriptions and
videos alone. Sponsor "is not obligated to apply any particular criteria or methodology."
So the fear of "judges do not own a Quest 3" is overrated, and the 2-3 minute video is not
marketing, it is the artifact being judged. Budget 20-25% of remaining effort on it.

Other terms worth knowing: individuals only, no teams. Eligibility 18+, US / Argentina /
Colombia / Costa Rica / India. IP assigns irrevocably to Nerdy, you keep a non-exclusive
non-commercial portfolio license. AI use is permitted and encouraged with disclosure, and
submissions "must be work you directed and can explain." Work must be created or
substantially developed during the entry period and you must describe what specifically
was built during it. Nerdy engineers screen everything; executives see only finalists.

---

## 3. The three jobs

One build project, three arithmetic jobs, in order. Each is a real thing adults do, each
hits a top-six hinge concept from the K-5 standards, and each is spatial in a way a mouse
cannot be. Full detail in 01_PRODUCT_SPEC.md.

| # | Job | Real-life situation | Standards | Hinge |
|---|-----|--------------------|-----------|-------|
| 1 | The Till | Count and bundle what you earned, pay for lumber | 1.NBT.B.2.a, 2.NBT.A.1.a, 2.MD.C.8 | Unitizing |
| 2 | The Countertop | Tile a surface, buy tiles in packs | 2.OA.C.4, 3.OA.A.1, 3.MD.C.7.a/c | Multiplicative structure |
| 3 | The Cut | Measure and cut a post to length | 3.NF.A.1, 3.NF.A.2, 3.MD.B.4 | Fraction as a magnitude |

The "power of arithmetic" beat in each, which is the thing you personally cared about:

- Job 1: you do not count 240 pennies. You bundle. The shortcut is place value.
- Job 2: you do not count 42 tiles. You multiply. And when 7x6 will not come, you split
  the rectangle into 7x5 + 7x1 and add. That is the distributive property as a physical
  object.
- Job 3: measure wrong and you waste the board. A fraction is a place on a line, not a
  slice of pizza.

Priority if you fall behind: Job 1 > Job 3 > Job 2. Job 3 is the flagship for rigor
(fraction magnitude is the one K-5 concept with longitudinal evidence predicting
high-school achievement net of IQ, working memory and SES). Job 2 is the cheapest to
build, so it is the one to shorten rather than cut.

---

## 4. Day by day

Each day ends with a deploy. The live URL exists from Day 1 hour 3 and never goes down.

### DAY 1 — Monday Sep 14 (~9h). Foundation and Job 1 playable.

Hour 0-2. THE ONLY MILESTONE THAT MATTERS TODAY: an ugly cube rendering on the actual
headset. Every hour this slips is an hour of the project gone.
- Scaffold with the exact pinned versions in 03_TECH.md section 2. Do not run
  `npm i three@latest` or `react@latest`, both are currently out of range.
- `adb reverse tcp:5173 tcp:5173`, open http://localhost:5173 in Horizon Browser.
- `chrome://inspect/#devices` remote debugging working from your desktop Chrome.
- Deploy an empty page to Vercel. Get the production URL now.

Hour 2-4. The XR shell.
- `createXRStore` with `emulate: { inject: true, syntheticEnvironment: false }` so the
  desktop fallback works on the Vercel domain, not just localhost.
- immersive-ar session with passthrough. Table plane detection via `useXRPlanes('table')`
  with a fallback to a fixed-height virtual table if planes are empty (they are empty for
  the first 2-3 seconds of every session, always).
- Hand tracking on, controllers as fallback. Test both.
- Seated, table-scale, no locomotion. This eliminates nearly all cybersickness and is a
  three-word answer to the motion-sickness objection.

Hour 4-6. The interaction primitives, shared by all three jobs.
- Grab and release a unit object with a pinch.
- Snap-to-slot with a tolerance.
- The symbol panel: a small flat surface in the same field of view that mirrors the
  child's physical action as a number, updating live. This is non-negotiable and it is
  the thing most VR math games skip. Understanding does not travel through the fingertips.
- The three-state world signal: neutral, fits, does not fit. Never the word "Incorrect."

Hour 6-9. Job 1: The Till, greybox.
- Coin tokens. Plain geometry, one flat color per denomination, no textures, no coin art.
  Perceptually rich manipulatives measurably hurt: first-graders using decorated blocks
  made more errors and built towers instead of solving problems.
- Ten pennies dropped into the bundler physically fuse into one dime that thereafter
  behaves as one object. The game refuses to let the child carry eleven ones. That refusal
  is the whole lesson.
- Target price on a tag. Pay it. The register accepts or does not.
- Deploy. End the day with Job 1 playable on the headset.

Day 1 kill switch: if you do not have a cube on the headset by hour 3, stop and debug
the transport (HTTPS, adb, secure context) before writing any more game code.

### DAY 2 — Tuesday Sep 15 (~10h). Job 1 finished, Job 2 playable.

Hour 0-2. Concreteness fading on Job 1. Three levels: coins visible, coins plus a
ten-frame diagram, symbols only with the coins gone. A scheduled advance, not a toggle
the child can avoid. Concrete-only was the worst condition on transfer in the child
experiment; the fade is what beats both endpoints. Build the fade as a reusable component
now, because Jobs 2 and 3 both need it and you do not want to write it three times.

Hour 2-4. Adaptive difficulty and the error response, also shared by all three jobs.
- Target roughly 80-85% success and adapt to hold it. Use it as a tuning heuristic, not a
  law; the 85% result is a gradient-descent finding, not an educational one.
- On a wrong attempt: attempt 1, the world shows the discrepancy. Attempt 2, step back one
  CRA phase. Attempt 3, a faded worked example with the first step already done. Never a
  bottom-out hint that contains the answer.
- Log the error type, not just correct or incorrect. Minimum taxonomy in 01_PRODUCT_SPEC.md.
- No timer, no leaderboard, no stars, no streaks, no "great job." Over 38% of feedback
  interventions in the largest meta-analysis reduced performance, and the mechanism is
  attention moving from the task to the self.

Hour 4-10. Job 2: The Countertop.
- Unit squares tile a rectangle on the real table. Count-all is allowed and is slow on
  purpose. Rows and columns readout appears. The equation panel writes 7 x 6 = 42.
- The split: the store sells tiles in packs of five per row. Grab the vertical seam and
  drag it. The rectangle visibly separates into 7x5 and 7x1, two totals appear, they add
  to 42. That is 3.MD.C.7.c, the highest-value single standard in K-5 for a spatial medium.
- Never teach multiplication as repeated addition only. That plants "multiplication makes
  bigger," which detonates in grade 6.

Day 2 kill switch: at 8:00 PM, if Job 2's split mechanic is not working, ship Job 2
without it as a plain array job and move on. The split is the best beat in the game but
Job 3 outranks it.

Deploy. End the day with Jobs 1 and 2 playable on the headset.

### DAY 3 — Wednesday Sep 16 (~10h). Job 3, then interleaving and performance.

Hour 0-7. Job 3: The Cut.
- A tape measure the child physically pulls out along the post. Strictly one-dimensional,
  strictly linear, no arc, no decoration, no 3D depth on the magnitude axis. Circular
  boards produced zero effect where linear boards produced d = 1.66 to 1.80 in the
  Siegler and Ramani studies. The geometry is the mechanism.
- Level A: whole inches. Level B: the tape shows halves. Level C: quarters and eighths.
- The inverse-relation beat: measure the same post with a foot-stick and then with an
  inch-stick. Smaller unit, larger count. That is 2.MD.A.2 and it is a fraction
  prerequisite hiding in the measurement standards.
- Place the cut mark at 2 3/4. Cut. The post either fits the frame or it does not, and if
  it does not the child sees the gap. That is feedback delivered by the world, not a label.
- Magnitude always maps left to right. Small left, large right, everywhere in the product.

Hour 7-8. Interleaving. Do not let one job be one skill drilled to completion. Mix item
types within a session and re-present missed types in a later session rather than
immediately re-drilling. Interleaved practice produced 61% versus 38% correct at a delayed
test in a preregistered cluster RCT of 787 students.

Hour 8-10. On-device performance pass. Budgets in 03_TECH.md section 4. Deploy.

Day 3 kill switch: at 8:00 PM, if Job 3's fraction tape is not working, ship it at halves
only and drop quarters and eighths. A working halves tape is still the flagship standard.

### DAY 4 — Thursday Sep 17 (~10h). AI tutor, dashboard, art. Freeze at 11 PM.

Hour 0-4. The AI tutor. Architecture in 03_TECH.md section 6.
- Push to talk on the grip button. MediaRecorder captures on hold, POSTs on release.
- Serverless route: Whisper transcribes, Claude reasons with the current job state and
  the last N physical actions as context, ElevenLabs Flash speaks the reply.
- Request microphone permission on the landing page, before entering the session. A
  permission prompt raised inside an immersive session is broken on Quest.
- Text-only tutor behind a flag as the guaranteed fallback. Build the flag first.

Hour 4-6. The generative moment, which is the keystone feature.
At the end of each job the tutor asks: "tell me how you did that." The child explains out
loud. This one mechanic satisfies five things at once, and you should say so in the
submission:
1. The generative learning activity that immersive VR requires. Without one, high
   immersion produces more presence and less learning than the same content on a desktop.
2. Discourse and articulation of strategy, which Abrahamson names as the facilitation
   guideline for embodied design.
3. NCTM effective teaching practices 4, 5 and 8.
4. Standard for Mathematical Practice 3.
5. The mastery signal. Claude classifies the explanation plus the action log into a
   strategy and misconception taxonomy and writes it to the dashboard.

Hour 6-8. The web companion. Next.js page, same repo, no headset required.
- Live mirror of the current job.
- The mastery graph: not "got it right" but "used count-all rather than count-on for 8+5,"
  "bundled without prompting on trial 3," "compared 1/3 and 1/5 by denominator size."
- One line of Nerdy vocabulary: this is a diagnostic event stream for the Study Plan, the
  daily drumbeat of engagement between tutoring sessions.
- A "book a tutor on this skill" button. Non-functional is fine, it shows the loop.

Hour 8-10. Art and audio pass. Flat-shaded low poly, baked vertex color, one directional
light, no shadows, no post-processing, no particles, no background music during a task.
The learning scene should be nearly empty. In XR there is no screen frame keeping clutter
out, so clutter has to be designed out. Kenney and Quaternius CC0 packs, and honestly
most of this scene is primitives you generate.

FEATURE FREEZE at 11:00 PM Thursday. Anything not working then does not ship.

### DAY 5 — Friday Sep 18 (~9h). Harden, record, write, submit.

Hour 0-2. Performance and hardening.
- Test the live URL on: Quest 3, Chrome desktop, Safari desktop, an iPhone. Safari has no
  WebXR at all, so the flat fallback has to not throw, it has to work.
- Panic levers if framerate is short: `foveation: 1`, `frameBufferScaling: 0.8`, delete
  every shadow, swap meshStandardMaterial for meshBasicMaterial.

Hour 2-5. Record and edit the video. Shot list and script in section 6 below.

Hour 5-8. Write the submission. Outline in section 7. This takes longer than you think.
Draft it Thursday night if you can, so Friday is editing rather than writing.

Hour 8-9. Final deploy, verify the URL from a machine you have never used, submit.

Submit at 6:00 PM CDT. Do not use the last six hours.

---

## 5. What ships, checklist

- [ ] Live URL, HTTPS, works in Quest Horizon Browser as immersive-ar with passthrough
- [ ] Same URL works flat in desktop Chrome and Safari with mouse and keyboard
- [ ] Three jobs, each with a CRA fade and a scheduled advance to symbols
- [ ] Push-to-talk voice tutor, with a text fallback flag
- [ ] "Tell me how you did that" at the end of each job
- [ ] Web dashboard with the strategy-level mastery view
- [ ] 2-3 minute demo video, third-person mixed-reality capture where possible
- [ ] Written description with pedagogy citations, AI disclosure, honest limitations,
      and a measurement design
- [ ] Public GitHub repo
- [ ] Submitted by 6:00 PM Friday

---

## 6. The demo video

Structure, target 2:45. The video is what is being judged. Judges may never open the app.

| Time | Beat |
|------|------|
| 0:00-0:15 | Cold open, no logo, no title card. A hand on a real kitchen table pulls ten pennies together and they fuse into a dime. Three seconds of silence. Then: "This is a nine-year-old learning that ten ones become one ten. She is not answering a question. She is making the number." |
| 0:15-0:40 | The problem, shared. "Most K-5 math games are timed answer-selection. Varsity Tutors already ships ten of them. Holo-Math Campaign: blast targets with correct answers. The research on time pressure and working memory says that is the wrong mechanic." Naming their own catalogue is the credibility move. |
| 0:40-1:40 | The demo. One unbroken flow, not a feature tour. Job 1 through to the child explaining out loud. Third-person mixed-reality capture. |
| 1:40-2:05 | The Nerdy payload. Cut to the web dashboard on a laptop. Strategy-level mastery, not a score. "Every action is a diagnostic event feeding the Study Plan, the daily drumbeat of engagement between tutoring sessions." This is the 25 seconds that wins the contest. |
| 2:05-2:30 | The honesty block. "Three objections. Meta's floor is age 10 and only 44% of six-year-olds fit a Quest 3, so the headset band is 10-11 and here is the tablet path to K-2. Quest shipments fell 17% year over year, so the engine is device-independent and the web tier needs no hardware. And I cannot claim learning gains from a five-day build, so here is the study I would run and what would falsify it." |
| 2:30-2:45 | What shipped, what is next, one line on the AI-assisted build. |

Production notes.
- Meta Quest Developer Hub over USB records up to 2160p60 at 40 Mbps. Set it up Day 1,
  not Day 4. In-headset capture is only 1080p at 36 fps variable.
- Recording costs framerate. Profile with capture running.
- Move your head slowly. Fast head turns read as nauseating and cheap on video.
- Color-correct in post. VR capture reads too dark.
- Record voiceover separately with your best microphone. Do not use a headset mic and do
  not use an AI voice for a submission about human-centered learning.
- Capture 20-30 seconds of the flat browser build so the video shows judges they can try it.
- POLICY LANDMINE: Meta's capture policy prohibits footage showing anyone who appears
  under 15 in a headset. Use an adult demonstrator with an on-screen note, or first-person
  capture with a child voiceover. Plan this now, not Thursday.

---

## 7. The written submission outline

Engineers screen it, executives read the finalists. Write for both.

1. The learner problem, in one paragraph, with a real child in it.
2. What it is and what shipped. Link the live URL and say a headset is optional.
3. The pedagogical basis. Six to ten claims, each with a citation. Pull from 02_PEDAGOGY.md.
   Lead with: quantity manipulation over answer selection; concreteness fading; the number
   line's unidimensionality; feedback through the world rather than through a label; and
   the generative-activity requirement for immersive media.
4. What specifically was built during the entry period. The terms require this.
5. Architecture. WebXR so the demo is a URL, the mastery model, the tutor loop.
6. Strategic fit with Nerdy in 2026. Consumer, not schools. Study Plan diagnostic events.
   Retention for the 29.1K membership base. Use their vocabulary, cite their Q2 language.
7. AI use disclosure. Be specific and confident about the workflow. This is a company that
   got more product output from a 30% smaller engineering team using AI. Being articulate
   about AI-assisted building is on-thesis for the role, not a caveat.
8. Honest limitations. Age floor, IPD fit, VR market contraction, novelty effect, no
   efficacy claim.
9. The measurement design. Pre-post on a validated early-numeracy instrument, strategy-use
   coding against the count-all to count-on to decomposition to recall sequence, sample
   size, duration, an active control not business-as-usual, and what result would falsify
   you. Nerdy leadership was publicly criticized this year for shipping ahead of evidence
   when Varsity Tutors for Schools closed. A real measurement plan lands on a nerve.
10. Next steps as a Nerdy roadmap, not a startup pitch.

---

## 8. Standing objections and the answer to each

| Objection | Answer |
|-----------|--------|
| We just exited schools, who buys this | Consumer parents. Retention for the membership base, free-tier acquisition. Never say district. |
| Meta's minimum age is 10 | Volunteer it first with the IPD table. Grades 3-5 content, ages 10-11 on headset, 8-9 on the flat build, K-2 on the tablet roadmap. |
| Is VR safe for a young child | Cite both sides. 197 sessions with 20 second-graders over six weeks: cybersickness rare, safe, highly engaging, but it required supervision, space, and proper fit. Then show the design response: passthrough not occlusion, seated and table-scale, 10-minute session cap, parent in the room by design, no social features. |
| We already ship ten free browser math games | Name them. Then explain why answer-selection under time pressure is the wrong mechanic, and that yours emits a mastery signal theirs cannot. |
| Nobody owns a Quest, this is anti-funnel | Concede fully and cite the 17% shipment decline yourself. The web tier is the funnel, the headset is the premium input device. |
| We have $30M cash and cut engineering 30% | Phase 1 is the web mastery layer into Study Plan. The headset client is additive, not a platform migration. |
| Where is the evidence it teaches anything | Do not claim outcomes. Present the measurement design and what would falsify it. |
| Is this just Prisms for little kids | Prisms is grades 6-12, teacher-led, classroom, $20k a room, occluded VR, and their founder says VR "is not for everyday use." Yours is grades 3-5, parent-mediated, home, consumer, mixed reality, short daily sessions. Cite her approvingly. |
| Judges cannot try it | Live URL, flat fallback, and you know the terms say they need not test it, so you optimized the video as the artifact. |
| Did you build this before the contest | Explicit "built during the entry period" section, precise disclosure of any template or asset reuse. |
| How much did AI write | Disclose enthusiastically and specifically, and be able to explain every system. |

---

## 9. How to run the build with Claude Code

- Point Claude Code at this folder. CLAUDE.md is written for it.
- Work in vertical slices: one job fully working before the next, never three at 60%.
- Test on the headset at the end of every slice, not at the end of the day.
- Commit after every working slice. You will want to roll back on Friday.
- Keep a visible in-scene debug panel (FPS, draw calls, session state) on a controller
  button. You will use it constantly.
