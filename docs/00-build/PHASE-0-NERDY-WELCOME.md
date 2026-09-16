# Phase 0 — Nerdy welcome: launcher, voice guide, lesson cards

**Status:** draft for owner review · September 16, 2026 · Owner-requested re-scoping.
Implementation of Phase 0 tickets starts only after the owner approves this document.

## Why this phase exists

After accepting the Cargo Crew workbench (toy look, carry handle, whole/halves loop), the
owner stepped back: the app has no findable launcher on the Quest, and it drops the learner
straight into one workbench. The owner wants the foundation first: open a branded **Nerdy**
app from the Quest Library, be welcomed by a **Nerdy AI voice guide** that asks a few
questions **by voice, both ways**, then see **three Nerdy-branded lesson cards**, and only
then enter a lesson's own workbench, where the same guide explains the workbench and
answers lesson questions. Fraction chapters resume after this flow is accepted.

Owner decisions recorded September 16: learner answers by microphone in v1 (chips remain
as the no-mic fallback); provider is **OpenAI**; app identity is **Nerdy**, package
`com.nerdy.vr`; owner supplies the Nerdy logo and the style guide (Live Learning Style
Guide, measured from nerdy.com). This supersedes the September 16 voice deferral.

## Experience

1. **Library.** "Nerdy" with the Nerdy logo appears in Quest Library (sideloaded apps sit
   under the Unknown Sources filter; the ticket verifies the exact route on this OS).
2. **Welcome space.** Passthrough room, an indigo night glow, the Nerdy wordmark on a glass
   bar, and a glowing guide presence (spectrum-gradient orb that pulses while speaking).
   Captions mirror every spoken line. A mic-consent card explains what is sent.
3. **Conversation.** The guide greets, then asks three things and listens: age band,
   interests, and what brings you to Nerdy. Answers are captured as structured tags via
   tool calls, never as stored free text or audio. Pointing at answer chips works at any
   time (no-mic path, accessibility). Profile lives on the device only. No name is asked.
4. **Lesson cards.** Three feature cards: Cargo Crew · Fractions (active), Neighborhood
   Café · Division (preview), Community Garden · Multiplication (preview). Previews do not
   launch lessons (product contract unchanged). The guide can describe any card on request.
5. **Workbench.** Choosing Cargo Crew loads its own workbench root (the accepted toy-look
   station). The guide gives a short tour (handle, strap, pad, ruler), reacts to committed
   actions (grab, place, split, submit verdict), and answers questions within the lesson's
   allowlisted scope. Help re-asks; mute and captions toggles exist. Back returns to cards.

## Architecture

- **Client (Unity, Quest).** `GuideSession` opens a secure WebSocket to the OpenAI Realtime
  API using a short-lived client secret minted by our proxy; streams 24 kHz PCM16 mic audio;
  plays streamed audio; renders transcript deltas as captions; dispatches tool calls
  (`record_profile`, `describe_card`, `request_help`) to C# handlers; pushes `GuideContext`
  (structured, validated lesson state, allowed next actions, submitted verdicts) as
  conversation items. Offline or denied: `LocalGuide` fallback speaks nothing and shows
  scripted captions, clearly labeled as offline.
- **Proxy (serverless, TypeScript; Vercel Functions preferred, Cloudflare Worker acceptable).**
  `POST /session` validates a per-launch nonce, applies a per-session and daily budget,
  returns an ephemeral Realtime client secret bound to a server-authored session config
  (voice, instructions, tools, turn detection, no data-training). The long-lived OpenAI key
  lives only in the proxy environment. No audio or transcripts pass through or persist.
- **Invariants kept.** Mathematical truth stays deterministic C#. The guide is told
  verdicts; it never computes, grades, moves pieces, or advances stages. During
  independent attempts the guide is not told unsubmitted correctness. Event log stays
  observable-only. No names, images, account or persistent device identifiers.

## Privacy and eligibility (must-read)

Two-way voice sends the learner's speech to OpenAI for live transcription. Under the
existing constraints that is allowed only for **adult testers** until the child-use gate
(formerly CC-P4-01, now CC-P0-08) records a go. Age band is captured as a tag, not a date
of birth, and stays on the device. The proxy logs no audio and no transcripts; OpenAI
data-retention and training settings are recorded as evidence. Contest submissions use
entrant-only footage. If the key, Realtime access, or the gate is unavailable, the flow
runs on the local fallback and is labeled offline; it is never presented as live AI.

## Branding

Tokens from the owner's style guide: base #202344, surface #161C2C, line #6C6E87, text
white and 64% white, accents indigo #3C4CDB, lavender #9E97FF, amber #FFC32B, magenta
#FB43DA, orchid #D684FF, cyan #17E2EA; brand gradient 267° indigo→lavender; spectrum
gradient for one or two words per screen; radii 12/14/20/pill; no drop shadows; Poppins
(display/body) and Karla (UI labels), both Google Fonts under the OFL. The Cargo workbench
keeps its accepted toy props; its UI chrome adopts Nerdy tokens. The owner asserts rights
to the Nerdy name and logo; record that assertion in CC-P0-08.

## Tickets (new phase, in order)

| ID | Title | Size | Absorbs |
|---|---|---|---|
| CC-P0-01 | Nerdy app identity and Quest launcher | M | CC-P1-02.5 |
| CC-P0-02 | Nerdy design system in Unity | L | part of P1-02 styling |
| CC-P0-03 | Realtime voice spike on Quest (throwaway) | M | — |
| CC-P0-04 | Guide service: proxy and Unity GuideSession | XL | CC-P1-03 transport |
| CC-P0-05 | Welcome conversation with structured profile capture | L | CC-P4-02 (adult) |
| CC-P0-06 | Nerdy lesson catalog and per-lesson workbenches | M | — |
| CC-P0-07 | Guide inside the Cargo workbench | L | CC-P1-03 meaning, CC-P4-03 |
| CC-P0-08 | Privacy, eligibility and consent | M | CC-P4-01 |
| CC-P0-09 | Accept the Nerdy welcome flow on Quest | M | — |

Reshuffle: P1-02.5 closes into P0-01. P1-03 closes as superseded by P0-04/P0-07. P4-01
moves forward as P0-08. P4-02/P4-03 are delivered for adult testers by P0-05/P0-07; P4-04
(failure evaluation, child readiness) stays in Phase 4. P1-06..P1-10 resume after P0-09,
with P1-10 acceptance now including the guide. Phases 2 and 3 keep their order.

## Deadline reality and cut line

Deadline Friday September 18, 11:59 PM CDT; today is Wednesday. Phase 0 is roughly two
days of focused work with one serious unknown (Realtime audio on Quest, P0-03). Priority:
P0-01 → P0-02 → P0-03 → P0-04 → P0-05 → P0-06 → P0-07 → P0-09, with P0-08 written in
parallel. After P0-09: P3-04..P3-06 release evidence. Quarters and equivalence (Phase 2)
happen only if time remains; otherwise the demo is Library → welcome → cards → halves.

## Needed from the owner

1. Nerdy logo file (square PNG or SVG, ≥1024 px, transparent background if possible).
2. OpenAI API key with Realtime access and a spend cap, placed in the proxy's environment
   by the owner or handed over out-of-band; never in the repo or APK.
3. Hosting for the proxy: Vercel (preferred) or Cloudflare account, or approve me to use a
   free tier on the owner's account.
4. Confirmation of rights to use the Nerdy name and logo in this build.
