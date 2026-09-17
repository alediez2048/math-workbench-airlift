# Privacy gate — Nerdy voice guide (CC-P0-08)

**Status:** draft, September 16, 2026 · Owner review required · Adult testers only.

## Decision record

- **Adult voice testing: GO (owner decision, September 16).** The owner, an adult, tests the
  two-way voice guide. The build enforces `NerdyDirector.adultTesterOnly = true`; the consent
  card states "This preview is for adult testers."
- **Child use: NO-GO until reviewed.** Nothing in this plan authorizes children to use the
  microphone path. A learner who selects an under-18 age band still gets the flow in this
  adult-tester preview, but child release requires a separate review of OpenAI's terms for
  minors, applicable law (COPPA/GDPR-K), and the school/parent consent model.
- **Rights:** the owner asserted on September 16 that they hold rights to use the Nerdy name
  and logo in this build. Poppins and Karla are used under the SIL Open Font License
  (licenses stored beside the font files).

## What leaves the device

| Data | Where it goes | Retained by Nerdy | Notes |
|---|---|---|---|
| Microphone audio (24 kHz PCM) | OpenAI Realtime API over TLS | No | Only while the session is live and unmuted; Mute pauses the mic. |
| Guide audio and transcripts | From OpenAI to the headset | No | Captions are rendered live and not written to disk. |
| Profile tags (age band, up to 5 interest tags, goal) | Stored on the headset (PlayerPrefs) | On device only | Enum-validated; names and free text are dropped by `LearnerProfile.Merge`. Also sent to OpenAI inside the conversation when the guide records them. |
| App context (instruction text, lesson facts, verdict text already shown to the learner) | OpenAI, as system messages | No | Bounded to 600 characters; never unsubmitted correctness; never learner free text. |
| Launch nonce and build version | Nerdy proxy | Logged without the nonce | Used for rate limiting only. No device id, account id, name or image is ever sent. |

The proxy holds the OpenAI key in its environment, mints short-lived client secrets, and
never proxies audio. OpenAI account settings to record before P0-09: data-training opt-out
and retention policy for the Realtime API (owner to capture screenshots/dates).

## Consent copy (in build)

"Your guide talks with you. With the microphone on, what you say is sent to OpenAI for live
transcription and is not stored by Nerdy. Nothing is saved except a few tags about what you
like. This preview is for adult testers." Buttons: "I'm an adult tester · turn on the mic" /
"Continue without voice".

## Contest disclosure (draft)

The Nerdy guide is a live AI voice agent (OpenAI Realtime) that welcomes the learner and
explains the workbench. All mathematical judgments are deterministic app code; the guide is
told results and never grades. Footage uses entrant-only testers.

## Temporary development exceptions (must be closed before release)

- `insecureHttpOption = AlwaysAllowed` so the headset can mint sessions from the Mac over
  LAN HTTP; revert to NotAllowed when `GuideEndpoints.MintUrl` becomes the https proxy.
- Proxy rate limits are best-effort per serverless instance; the hard ceiling is the OpenAI
  project spend cap the owner sets.
