# Core AI voice guide — owner-requested revision

## September 16 (evening): voice un-deferred; Realtime two-way audio is the baseline

Owner decision: the Nerdy guide is a live OpenAI Realtime voice agent, two-way, for adult
testers (PRIVACY-GATE.md). The proxy owns instructions and tools; the app pushes bounded
context; math verdicts remain deterministic. Sections below that describe a recorded-first or
output-only baseline are historical.

## September 16: deferred from immediate demo

Owner explicitly prioritized working grabbing, labeled whole and labeled halves.
Do not implement provider/speech/caption/replay/mute integration before that loop.
The core-AI statements below describe the prior plan; no child/provider clearance
or contest-compliance conclusion follows from this deferral.

September 15, 2026. **AI voice guidance is a core lesson requirement.**
This supersedes the earlier recorded-first/AI-only-in-Phase-4 recommendation.
Plan changes are authorized; implementation, provider spending and child deployment are not.

## Required experience

A state-aware AI guide accompanies Cargo Crew from briefing through controller
practice, whole/halves, quarters, merge, equivalence, comparison and dispatch.
It explains the next permitted action, reacts to a committed learner action,
and offers relevant help. It does not merely play a fixed recording and call it AI.

The baseline interaction is **AI spoken output driven by structured lesson state**.
Learner microphone input is not necessary: ray Help/Replay can request guidance.
Optional spoken questions/conversation remain a separate capability in Phase 4.
This separation is an implementation recommendation, not a claim the user requested
an always-listening microphone or authorized collection of children's speech.

Preferred provider candidate: **OpenAI API**, not the ChatGPT consumer app.
Baseline: live model selection of constrained guidance, followed by validated text
and matching, reviewed speech assets. Speech synthesis prepares that bounded asset
catalog under separate approval; it is not an unchecked playback-time step. Realtime direct audio is a gated alternative, not assumed safe.
Account access, approved model/configuration, Unity transport and costs remain
unverified. ElevenLabs is an alternative only after its separate eligibility gate.

## What the first milestone must prove

CC-P1-03 introduces an adult-only, authorized live-inference slice:
mission/context → guide speaks → actual grip/place event → guide responds to that
event → ray Help → contextual explanation, with matching captions and local fallback.
CC-P1-04 onward connects authoritative fraction snapshots as those activities land.
CC-P1-10 cannot be accepted as AI-guided using recordings, fixtures or text alone.

Record live-provider evidence separately from fallback/fixture mode, without raw
audio, personal content, credentials or identifying telemetry. If access, safe
output validation or budget is unavailable, mark the AI criterion blocked. Other
offline development may be demonstrated but does not fulfill this requirement.

## Ownership and data flow

1. Unity commits a deterministic action and creates a GuideContext:
   schemaVersion, stageId, attemptGeneration, revision, eventId, validated
   quantities/notation, allowed next-action IDs, submitted verdict and help level.
   No names, voice, room images, account/device IDs or inferred emotions.
2. GuideCoordinator schedules at most one response. Context comes from code,
   never the model. During independent attempts, do not disclose unsubmitted
   correctness or correct target coordinates to guidance.
3. A private authenticated server validates context, enforces session budget and
   holds the provider API key. OpenAI is the first candidate, not an installed SDK.
   Test the constrained text-to-speech pipeline on Unity/Quest; browser samples
   do not establish compatibility.
4. Guide responses may explain approved facts/actions but cannot alter pieces,
   set scores, advance stages or choose arbitrary pointer coordinates.
   Gate every requested pointer/help action against the current stage and revision.
5. The live model returns only factIds, actionId, explanationVariantId and evidenceIds
   from the current server-approved context. No arbitrary prose or numeric values.
   Validate the combination's meaning, not just vocabulary: every selected fact must
   hold in the committed snapshot and support that action/variant. Render final text
   deterministically from reviewed templates and exact snapshot quantities.
   Resolve only an approved audio asset for that exact text; captions use the same text.
   A fixed local cue selector or speech synthesis alone does not pass the live-AI
   criterion: evidence must show live model guidance selection from actual events.
   Direct Realtime audio remains gated until pre-playback audio/text correspondence
   and safety are demonstrated. A model transcript alone does not prove correspondence.
   Include pronunciation/missing-negation/mismatched-audio checks in adult evaluation;
   synthesis correctness is not guaranteed by correct text. Failed or unverified
   audio cases block the affected release path; use labeled local fallback.
   P1-03 owns VerifiedSpeechCatalog and SpeechAssetVerifier. Before runtime, produce
   rights-cleared audio for the finite approved text/quantity/variant combinations
   (OpenAI synthesis is a candidate requiring spending approval). An adult reviewer
   listens to each complete clip against the text, checking fraction pronunciation,
   relations and negation. Record textHash, audioHash, locale, voice/version, rights,
   reviewer approval and allowed template/context IDs in a versioned manifest.
   Runtime recomputes/matches hashes and context, refuses unapproved/missing entries,
   and plays a clearly labeled local fallback instead. No word/number splicing.
   A new template or quantity requires a new reviewed clip before live use.
   The live AI selects relevant explanations from this catalog; recordings alone
   without live model selection still do not pass AI-guided acceptance.
   Dynamic synthesis or direct audio requires a separate concrete validation design
   and owner review; synthetic audio-mismatch tests alone cannot clear it.
6. Captions match the approved spoken content. Back, reset, tracking loss and newer
   state cancel pending/stale audio and tool proposals. A completed sentence never
   satisfies a mathematical task.
7. On timeout, unavailable service or rejected output, play a rights-cleared local
   cue and mark fallback visibly. Game actions remain usable. Fallback is resilience,
   not proof of a working AI guide.

## Pacing, security and cost targets

One short instruction/explanation per meaningful stage/action; no narration on every
controller frame. No correctness feedback before Submit in independent activities.
Stop/Replay/Mute always available. No pressure, persona dependency or inferred emotion.
Provisional timeout: five seconds to an approved playable response, then one local
fallback; no automatic retries. Measure latency including validation/buffering.
Unacceptable latency returns to architecture review, not an unannounced removal of AI.
Session duration/request/audio limits and an owner-approved spend cap must be written
before a paid spike. Defaults disabled; unavailable caps/access fail closed.
No provider secrets or long-lived app credentials in APK. Adult test-session access
is explicitly authorized and revocable; do not deploy an unlimited public issuer.

## Child-use gate applies to every network path

Initial live tests use adult owner/synthetic lesson state only. Do not treat
microphone-off events as automatically anonymous: review the complete data inventory
and infrastructure logs before any child use.
For under-13s or the applicable digital-consent age, OpenAI guidance requires Zero
Data Retention before processing personal data. Approval/configuration, endpoint/model
eligibility, tracing, own backend logs, safety disclosures/filtering, monitoring and
escalation must be verified. ZDR alone is not full legal/safety clearance.
No child account, recording, live test or distribution is authorized by this plan.
P4-01 can run as an early documentary gate; unresolved child eligibility blocks
child release even when adult-only core AI acceptance succeeds.

## Ticket ownership

- P1-03: guide contracts/catalog/coordinator, authenticated voice backend, adult live
  capability proof, output gate, captions/fallback, scope/budget/permission preflight.
- P1-04 through P2-07: attach each new actual lesson state/action and approved facts;
  test that the guide cannot misstate quantities or leak answers.
- P1-09/P3-03: interruption and unsafe/stale-output recovery.
- P1-10/P2-08: live AI end-to-end acceptance for implemented chapters, distinct from
  offline/muted tests. P3-02 includes AI-guided dispatch.
- P3-01: strengthen/evaluate scaffold routing on the same OpenAI guide boundary;
  a deferred enhancement never waives baseline AI-guide acceptance.
- P3-04/05/06: measure voice-on device performance, actual cost/latency, privacy and
  release evidence.
- P4-01: child-use/provider eligibility documentary gate; may occur early.
- P4-02/03/04: separately gated spoken-question input, constrained conversation and
  failure/pilot-readiness review. They do not introduce the first AI voice output.

## Acceptance tests to add

GuideUsesCommittedContext; GuideNeverGradesOrAdvances; WrongPreSubmitAnswerNotLeaked;
StaleVoiceResponseDiscarded; FalseRelationWithAllowedIdsRejected; OmittedNegationRejected; AudioTextMismatchRejected; CaptionMatchesApprovedAudio;
OnlyOneResponseInFlight; TimeoutUsesLabeledFallback; FallbackDoesNotPassLiveGuideGate;
MissingAccountOrBudgetBlocksLiveCalls; UnknownAgeBlocksChildMode;
MicOffInCore; StopBackClosesOptionalMic; UnauthorizedGuideRequestDenied;
BudgetCapStopsCalls; NoSecretsOrPayloadLogs; LiveGuideCoversImplementedStages.

## Verified sources and unknowns

- [Text to speech](https://developers.openai.com/api/docs/guides/text-to-speech):
  approved text-to-audio candidate; require AI-voice disclosure and verify pronunciation.
- [Realtime API](https://developers.openai.com/api/docs/guides/realtime): conversational
  audio/tool capabilities. Found H; Unity integration remains unknown.
- [Server-side controls](https://developers.openai.com/api/docs/guides/voice-server-controls):
  server-held orchestration/control. Found H; our authentication must be implemented.
- [Under-18 guidance](https://developers.openai.com/api/docs/guides/safety-checks/under-18-api-guidance):
  child-data and age-appropriate safeguards. Found H, applicability/configuration
  requires qualified review.
- [Data controls](https://developers.openai.com/api/docs/guides/your-data): ZDR approval
  and endpoint/model caveats. Found H; this account's eligibility is unknown.

No provider calls, Unity changes or paid assets were made during this revision.
