# Release gates — Cargo Crew lock (CC-P3-05 / lock item L-6)

Inventory taken 2026-09-16 night from build cargo-20260916-221238 (commit 2740d8a) and the repo. Facts only; a
gate is **open** until the fix or owner decision is recorded here.

## APK permissions (aapt2 dump permissions)

| Permission | Source | Needed | Status |
|---|---|---|---|
| INTERNET | forceInternetPermission (guide) | yes | ok |
| RECORD_AUDIO | Unity Microphone (guide mic) | yes, adult voice consent | ok; runtime prompt after consent |
| MODIFY_AUDIO_SETTINGS | Unity audio/microphone | yes | ok |
| BLUETOOTH | Unity adds it when the Microphone class is used | no | **open**: the OpenXR Meta manifest step is meant to strip it ("will cause projects to fail Meta cert"), but it is still in this APK. Matters only for Meta store submission; investigate the branch in ModifyAndroidManifestMeta that skipped the removal |
| com.nerdy.vr.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION | Android 13+ default for dynamic receivers | yes (system) | ok |

## Network

| Endpoint | Used for | Status |
|---|---|---|
| `http://192.168.86.20:8787/session` | dev mint on the owner's Mac (LAN, cleartext) | **open**: temporary. Needs owner `vercel login`, deploy `services/guide-proxy`, set `GuideEndpoints.MintUrl` to https, revert `insecureHttpOption` (currently 2 = AlwaysAllowed) |
| `wss://api.openai.com/v1/realtime` | live guide session with an ephemeral client secret | ok; the provider key never ships (secret scan passes each build) |
| `https://api.openai.com/v1/realtime/client_secrets` | called only by the proxy / dev mint, never by the app | ok |

## Secrets

- Wrapper credential scan of APK strings and IL2CPP metadata passes on every build (last: 221238).
- Provider key lives only in `~/.config/nerdy/openai.env` on the Mac; the repo contains one fake key string in the
  proxy test (`sk-live-should-not-leak`), which is intentional.

## Assets and licenses

| Asset | License / rights | Status |
|---|---|---|
| Poppins (Regular, Medium, MediumItalic, SemiBold) | SIL OFL, `Fonts/Nerdy/OFL-Poppins.txt` | ok |
| Karla (Regular, Medium, Bold) | SIL OFL, `Fonts/Nerdy/OFL-Karla.txt` | ok |
| Nunito, Nunito Sans | SIL OFL, `Fonts/Nunito-OFL.txt`, `Fonts/NunitoSans-OFL.txt` | ok |
| LiberationSans SDF (TMP default) | SIL OFL via TextMesh Pro | ok |
| Nerdy wordmark and name | owner confirmed rights (2026-09-16) | ok |
| All 3D props (crates, trucks, pickups, vans, cranes, containers, table) | generated in-project by RoundedBoxMesh scripts | ok, no third-party art |
| Ambient music | synthesized at runtime by AmbientMusic | ok, no audio asset |
| Meta XR Core / Interaction SDK 205, Unity OpenXR 1.18, Meta OpenXR 2.6.1 | vendor licenses (Meta Platforms SDK license, Unity Companion License) | **open**: owner to confirm acceptable for the contest submission |
| OpenAI Realtime (gpt-realtime, voice "marin") | OpenAI terms; adult testers only | **open**: owner confirms contest eligibility and adult-only scope (PRIVACY-GATE.md) |

## Data and privacy

- Adult testers only; consent card before any microphone use; no names, images or raw audio stored by Nerdy;
  learner profile is tags only (LearnerProfile, PlayerPrefs). Child use stays blocked (PRIVACY-GATE.md).
- Live audio goes to OpenAI for transcription and speech during the session (disclosed on the consent card).

## Summary of open gates

1. Dev HTTP mint and `insecureHttpOption` (owner `vercel login`).
2. BLUETOOTH permission still present (store submission only).
3. Owner confirmation of vendor SDK and OpenAI terms for the contest.
