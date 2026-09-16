# Cargo Crew stack — installed versus planned

Status: draft execution choices; package versions below are observed manifest state.

| Layer | Use | Status / boundary |
|---|---|---|
| Editor | Unity 6000.6.0f1 Apple Silicon | Installed; do not restart 6.3 migration |
| Rendering | URP 17.6.0 | Existing; mobile budget, no pipeline change |
| XR | OpenXR 1.18.0 + Meta OpenXR 2.6.1 | Existing |
| Interaction | Meta Core / Interaction / OVR 205.0.0 | Existing; reuse rig, grab, ray, haptics |
| Math / lesson state | deterministic C# + ScriptableObjects | Draft fraction classes exist; runtime integration planned |
| UI | existing Unity UI / TMP | Bundled fonts imported; preserve notices |
| Tests | Unity Test Framework 1.8.0, EditMode / PlayMode | 12 onboarding EditMode tests; new suites planned |
| Core AI voice | OpenAI voice API candidate, private server, Unity guide adapter | Required from P1-03; adult live proof and safety/access gates; not installed |
| Fallback narration | Unity AudioSource, licensed local clips | Offline resilience only; cannot satisfy AI-guide acceptance |
| Editor automation | official Unity CLI 1.0.0-beta.9 / Pipeline 0.7.0-exp.1 | Existing; no third-party editor bridge |
| Source control | Git, private GitHub, existing LFS rules | Existing; no new repository |
| Ticket tracker | local docs/00-build/tickets | Draft; no GitHub Issues publication |
| Adaptive scaffold evaluation | Same core OpenAI guide boundary | P3-01 enhancement; no second inference provider |
| ElevenLabs | candidate TTS / optional Agents WebSocket | Blocked by eligibility/rights review; no dependency installed |
| Voice transport | provider-neutral adapter and authenticated server | Core output proof in P1-03; microphone conversation remains P4 |

Official [ElevenLabs libraries](https://elevenlabs.io/docs/eleven-api/resources/libraries)
do not establish an officially supported Unity SDK; Unity/.NET integrations listed
there are community work. Do not label a community client “official.”
A WebSocket API is a protocol possibility, not a measured working Android audio path.

XR reuse is unchanged. P1-03 now includes a voice/backend feasibility gate and any
necessary reviewed/pinned transport dependency; no package installation is authorized now. Version changes require
a concrete compatibility reason, owner approval, manifest/lock update and device regression.
No ECS rewrite: small pure-state models with thin MonoBehaviour adapters fit this scale.
