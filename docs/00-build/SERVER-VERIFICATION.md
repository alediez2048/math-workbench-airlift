# Server and voice-gate verification

Future implementation contracts; these commands have not run during planning.
[Core AI guide](AI-VOICE-GUIDE.md) owns meaning and safety boundaries.

## CC-P1-03 — core output-only AI guide service

Create services/voice-session/package.json, pinned lock, closed shared schemas,
actual HTTP handler, approved explanation catalog, server tests and local fixture
integration server. OpenAI is the preferred candidate; access, model/endpoint
eligibility and spend authorization are unverified. Never embed a provider key in APK.

Baseline pipeline: validated structured lesson context → live model selection of
approved fact/action/variant IDs → semantic validation against that context →
deterministic approved text → verified matching speech asset → captions.
Do not stream arbitrary generated explanation/audio first and validate afterward.
TTS alone or deterministic local cue selection is not live AI-guidance evidence.
Direct Realtime audio is a gated alternative, not the baseline; a transcript alone
cannot establish audio correctness. Use the audio checks in AI-VOICE-GUIDE.md.

Proposed POST /api/guide: maximum 8 KiB, schema version, request/attempt generation,
revision, stage/event IDs, validated quantities, allowed actions, submitted verdict
and help level. No free-form learner text, microphone, personal IDs or room imagery.
Server derives legal fact/action/template combinations from its versioned lesson
catalog; reject out-of-range values, unknown stage IDs and inconsistent snapshots.
Response selects only factIds, actionId, explanationVariantId and evidenceIds.
Reject unsupported or contradictory combinations even if all IDs exist.
Render all quantities/relationships from validated state, not model prose.
Before independent Submit, filter answer/correctness data; Help may expose only
stage-authorized support and must record assistance rather than an independent pass.

One response in flight; a newer revision cancels/discards the old response.
Provisional five-second end-to-end playable-response timeout; then one labeled
local fallback, no automatic retries. Limit request and audio duration plus total
session/global spend in owner-approved configuration before any live spike.
Numerical caps must be set explicitly at that preflight; absent caps fail closed.

Proposed env names: OPENAI_API_KEY (server only), GUIDE_MODEL, SPEECH_MODEL,
GUIDE_ENABLED=false, GUIDE_SESSION_SPEND_LIMIT, GUIDE_GLOBAL_SPEND_LIMIT.
Never put values in documents/logs. Pin evaluated API/model versions in evidence.
Use authenticated adult-only test sessions, expiry/revocation and server-side
quotas. No public unlimited session issuer; IP limiting is not a global budget cap.
No raw request/audio/transcript logs or persistent learner datastore.
Own infrastructure logs and provider retention controls require review.

Future commands:
```sh
/opt/homebrew/opt/node@22/bin/node /opt/homebrew/opt/node@22/lib/node_modules/npm/bin/npm-cli.js --prefix services/voice-session ci
/opt/homebrew/opt/node@22/bin/node /opt/homebrew/opt/node@22/lib/node_modules/npm/bin/npm-cli.js --prefix services/voice-session test
/opt/homebrew/opt/node@22/bin/node /opt/homebrew/opt/node@22/lib/node_modules/npm/bin/npm-cli.js --prefix services/voice-session run test:integration
```

Local tests use provider stubs and make no paid calls. Actual handler cases:
RejectOversize, RejectUnknownStage, RejectInconsistentSnapshot,
DisabledMakesNoCall, UnauthorizedGuideRequestDenied, MissingBudgetDenied,
BudgetCapStopsCalls, ExpiredSessionDenied, RevokedSessionDenied,
FalseRelationWithAllowedIdsRejected, IrrelevantEvidenceRejected,
PreSubmitAnswerNotLeaked, StaleRevisionRejected, TimeoutUsesLabeledFallback,
NoSecretsOrPayloadLogs. HTTP integration tests exercise method/schema/auth,
budget and provider failures; Unity-only mocks are insufficient.
GuideServiceContractTests is the backend suite name; CoreAIGuideTests and
VoiceEligibilityGateTests are Unity suites introduced in P1-03.

P1-03 creates VerifiedSpeechCatalog/SpeechAssetVerifier and a versioned manifest.
For each permitted complete utterance, an adult reviews rights-cleared recorded or
synthesized audio against its exact text; record text/audio hashes, locale,
voice/version, allowed contexts and approval evidence. Runtime checks the manifest,
hashes and stage compatibility before playback. Missing, tampered or unreviewed
assets use labeled fallback; no arbitrary audio generation or clip splicing.
Audio fidelity is established by actual adult listening, not inferred from synthetic
tests or a model transcript. A new quantity/template needs a new reviewed entry.
Cases: UnreviewedAudioNeverPlayed, AudioHashMismatchRejected,
TextHashMismatchRejected, MissingVariantUsesFallback. Fault-injected omitted-negation
clips demonstrate rejection unless explicitly reviewed correctly; synthetic tests
alone cannot certify the generated speech. Dynamic synthesis is outside this baseline.

Adult live acceptance separately demonstrates genuine model selection for actual
grip/place/Help, approved spoken content and matching captions, measured latency/
cost, disconnect/mute/replay and no embedded credentials. Use synthetic fixtures
to test false relations, omitted negation and audio/text mismatch; check actual
fraction pronunciation when P1-04 onward introduces it. Test results do not imply
perfect synthesis or child clearance. No safe provider proof → AI criterion blocked.
Fallback remains usable but cannot close required AI acceptance.

## CC-P3-01 — improve adaptive routing, reuse the core service

Extend the same services/voice-session boundary and its existing tests. No new
Anthropic service or second inference provider. The older RAP service contract is
historical where it conflicts with this revision.
Add bounded evidence-tail selection and pre-labeled synthetic traces, preserving
Submit/reason evidence when a tail overflows. Test stale/irrelevant evidence and
ambiguous traces; ambiguity selects neutral help, never an inferred diagnosis.
Each golden trace must stay within its approved route set over three repeats;
compare to local rules and report differences without efficacy claims.

Closure: validated-retained enhancement, or explicit owner-approved deferral of
the enhancement only. Deferral does not disable or waive P1's core AI guide.
Contest eligibility remains a separate evidence gate; do not infer organizer approval.

## CC-P4-01 — documentary child-use and provider review

May run early after review-scope approval; no APK required.
Record provider, audience, output-only, microphone and child-release permissions
separately, including data inventory and verified retention configuration.
OpenAI under-13 personal-data processing requires its documented ZDR prerequisites;
approval and endpoint/model eligibility are not assumed. Review own logs and
child-safety/disclosure controls too. Check ElevenLabs separately if considered.
A scoped no-go can close this review and blocks affected child/conversation use.
Adult QA is not permission for child release. No external contacts, calls, payment
or child data collection to test eligibility without separate authorization.
P1-03 owns core software gates; P4-02 extends them for optional speech input.

## CC-P4-02 through CC-P4-04 — optional spoken questions

Depend on accepted core, scoped P4-01 go and separate privacy/account/budget approval.
Extend services/voice-session and VoiceEligibilityGateTests rather than creating
the first AI voice backend. Add explicit microphone controls, consent/disclosure
as applicable, transport cancellation, short-lived scoped sessions and immediate
mic shutdown on Back/pause. Provider transport dependency requires review/pinning.

Rerun the server commands above plus actual Quest input/output tests. Additional
handler tests: MissingMicrophonePermissionDenied, UnauthorizedMintDenied,
QuotaExceededDenied, ReplaySessionDenied, ExpiredSessionDenied,
ProviderFailureRedacted and RevokedSessionDenied. No audio/token payload logging.
Measure adult-only speech latency and failure recovery; fixtures alone do not prove
microphone transport. Child testing and public deployment remain unauthorized.
