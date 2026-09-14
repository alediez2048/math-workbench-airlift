# Workbench - independent review brief

> **Legacy review brief.** It critiques the superseded WebXR proposal. Review
> `UNITY_PLAN.md` and `UNITY_REVIEW_BRIEF.md` for the current Unity plan.

Purpose: give a second model (Codex or any reviewer) everything it needs to critique this
plan without re-deriving it. The plan came out of a research session on 2026-09-14. The
reviewer should attack it, not summarize it.

Read in this order: this file, 00_PLAN.md, 01_PRODUCT_SPEC.md, 03_TECH.md, 02_PEDAGOGY.md,
CLAUDE.md.

---

## Situation

- Solo entrant, Nerdy AI Hackathon (hackathon.nerdy.com), K-5 Math Game track.
- Submissions close Friday 2026-09-18 23:59 CDT. Demo Day 2026-09-25. Five build days.
- Budget: 8-10 focused hours per day, one builder working with an AI coding agent.
- Hardware: one Meta Quest 3. Builder is web-literate, not a Unity or game developer.
- Judging criteria as published: real learner problem solved; educational rigor and
  pedagogical design; actual product shipped and demoed; innovation and execution quality.
- Contest terms: judges need not test entries and may judge on video and description alone.
  Individuals only. IP assigns to Nerdy. AI use allowed with disclosure.

## What was decided, and the reasoning to attack

1. Mixed reality passthrough on the child's real table, not sealed VR.
   Reasoning: Meta's Quest age floor is 10 and only ~44% of six-year-olds fit Quest 3's IPD
   range, so passthrough plus a parent in the room is the honest safety answer, and no
   competitor uses it for early math.

2. Grades 3-5 content, ages 10-11 on headset, 8-9 on a flat web build, K-2 as a tablet-AR
   roadmap. Stated openly in the submission.
   Reasoning: claiming K-5 on a Quest is factually wrong about Meta's own terms.

3. Consumer and membership-retention framing, never classroom or district.
   Reasoning: Nerdy shut down Varsity Tutors for Schools in Q2 2026.

4. Core mechanic is manipulation of quantity, never selection of an answer, with no timers,
   no stars, no leaderboards, no praise, and wrongness shown by the world rather than a label.
   Reasoning: Kluger and DeNisi (38% of feedback interventions reduce performance, via
   self-directed attention); Shute on immediate task-focused elaborated feedback; Kaminski
   and Sloutsky on perceptual richness; Nerdy's own ten browser games are all timed
   answer-selection.

5. Three jobs in one build project: The Till (unitizing via coins), The Countertop (array
   and distributive property as a rectangle you pull apart), The Cut (fraction as a point
   on a strictly linear tape measure).
   Reasoning: ranked by leverage in the K-5 standards; fraction magnitude has longitudinal
   evidence predicting high-school math net of IQ and SES (Siegler 2012).

6. Concreteness fading (concrete, representational, abstract) per job with a scheduled
   advance and mastery defined only at the abstract level.
   Reasoning: Fyfe, McNeil and Borjas 2015; concrete-only was worst on transfer.

7. A push-to-talk voice tutor (Whisper, Claude, ElevenLabs over a serverless route), with a
   text-only tutor behind a flag as the fallback. Not realtime streaming.
   Reasoning: WebRTC audio inside a live XRSession on Horizon Browser is unvalidated; the Web
   Speech API is unreliable on Chromium forks without Google's key.

8. "Tell me how you did that" at the end of each job, with Claude classifying the child's
   strategy into a misconception taxonomy that streams to a 2D web dashboard.
   Reasoning: immersive VR increases presence and reduces learning unless a generative
   activity is added (Makransky, Terkildsen and Mayer 2019); the strategy signal is what
   makes this relevant to Nerdy's Study Plan rather than a novelty.

9. Stack: WebXR with React Three Fiber and @react-three/xr v6, Vite, Vercel. Not Unity, not
   Godot, not Lens Studio.
   Reasoning: seconds-long iteration loop; demo is a URL; bundled Quest 3 emulator for
   judges without a headset; best model training coverage for an AI-assisted build. Lens
   Studio does not target Quest at all.

10. Pinned versions verified on npm 2026-09-14: react 19.2.8 (19.3.0 is outside R3F's peer
    range), three 0.185.1 (0.186.0 released six days ago, untested with the chain),
    @react-three/fiber 9.7.0, @react-three/xr 6.6.30, @react-three/drei 10.7.8. WebGL only,
    never WebGPURenderer on Quest.

11. Schedule: D1 foundation and Job 1; D2 Job 1 finished and Job 2; D3 Job 3, interleaving,
    perf; D4 tutor, dashboard, art, freeze 23:00; D5 harden, record, write, submit 18:00.
    Priority if behind: Till, then Cut, then Countertop.

## What the reviewer should specifically challenge

- Is three jobs in five days realistic for one person, or should it be two done well?
  Where exactly is the schedule lying to itself?
- Hand tracking as the primary input for near-field grabbing on Quest 3 in 2026: is that
  reliable enough to build a demo video around, or should controllers be primary?
- Is passthrough plus plane detection a hidden time sink? What breaks first?
- Any known incompatibility in the pinned stack, especially @react-three/xr 6.6.30 with
  drei 10.7.8 and uikit 1.0.76 on three 0.185.1?
- Is the emulate: { inject: true } claim correct, i.e. will the bundled IWER emulator
  actually activate on a non-localhost Vercel domain and give a laptop judge a usable scene?
- Is the push-to-talk tutor architecture the right risk call, or is text-only the honest
  choice for five days?
- Does the pedagogy hold up? In particular: the ban on all timers versus IES's Strong
  Evidence recommendation for timed fluency activities; the ban on self-explanation prompts
  given Barbieri 2023 versus the "tell me how you did that" mechanic; whether concreteness
  fading across three levels can be meaningfully demonstrated in a five-day build.
- Is the age-band framing (10-11 on headset) a strength or does it kill the K-5 track fit?
- Is naming Nerdy's own games in the video a credibility play or a way to insult the
  people judging you?
- What is the single most likely reason this submission does not make finalist, and what is
  the cheapest change that fixes it?

## Ready-to-paste prompt

You are two reviewers in one: a skeptical senior WebXR engineer who has shipped on Quest 3,
and an experienced K-5 math education researcher who has judged ed-tech competitions. Read
REVIEW_BRIEF.md, then 00_PLAN.md, 01_PRODUCT_SPEC.md, 03_TECH.md, 02_PEDAGOGY.md and
CLAUDE.md in this folder. Do not summarize them. Produce: (1) the five highest-risk
assumptions ranked by expected cost if wrong, each with the evidence you would want and the
cheapest mitigation; (2) every factual or version claim in 03_TECH.md you believe is wrong or
outdated, with what you think is correct; (3) a revised five-day schedule if you think the
current one is unrealistic, cutting scope explicitly; (4) three things you would add that
would most improve the judged outcome for the least effort. Be blunt and specific. Cite when
you disagree with a research claim.
