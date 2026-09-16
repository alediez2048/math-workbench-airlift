# Verification contract

## September 16 evidence caveat

Read [current handoff](HANDOFF-2026-09-16.md) before testing. Twenty-five passing
Cargo checks did not detect the physical grab failure. The isolated vendor cube
control is invalid (controller GrabInteractable absent; cube not visible). Neither
build/install success nor candidate hover proves selection. No repeated owner QA
until a diagnostic discriminates input, SDK selection and visible motion.

No tests or builds are run as part of authoring this plan. Commands below are
the proposed implementation protocol, not claims of new passing results.

## Existing tools and proposed wrapper

CLI help was inspected September 15: unity test supports --mode/--filter/--output;
unity build supports --target/--execute-method/--output-path. Use the official
Pipeline command discovery when driving an already-open editor. Never launch a
second editor against a locked project or close one with unsaved user changes.

CC-P1-01 creates scripts/verify-cargo.sh and the build entry point
unity/Assets/Airlift/Editor/CargoBuild.cs (Airlift.Editor.CargoBuild.Build).
These paths **do not exist yet**. The wrapper contract is:

```sh
bash scripts/verify-cargo.sh --suite CargoBaselineTests --build
bash scripts/verify-cargo.sh --phase 1 --build
bash scripts/verify-cargo.sh --phase 2 --build
bash scripts/verify-cargo.sh --phase 3 --build
bash scripts/verify-cargo.sh --phase 4 --build
```

A suite option accepts comma-separated suites and runs each plus baseline regression;
phase runs all required suites through that phase. CC-P1-01 creates
scripts/cargo-milestones.json listing required suites, scenes and cue IDs by ticket.
P1-01/02 need existing written onboarding only; P1-03 introduces live AI guide
contracts and fallback cues. Its live acceptance requires authorized provider proof,
not fixture playback. Later chapters add state-aware guide coverage with their own cues.
Future tests/assets are not required prematurely; current required omissions fail.
--build builds CargoCrew.unity explicitly, never the
currently open scene or stale EditorBuildSettings list. Return nonzero for
compilation, missing/zero tests, failed/skipped required tests, unsafe package,
missing required audio, or build failure. Phase 4 additionally requires signed
policy gate evidence and contract tests; it cannot pass while blocked.
Write machine-readable and readable evidence to artifacts/qa/, ignored in Git;
commit a scrubbed summary in docs/qa/cargo-CC-ID.md.

Underlying tools, after saving and safely releasing the editor lock:
```sh
unity test ./unity --mode EditMode --output ./artifacts/qa/editmode.xml
unity test ./unity --mode PlayMode --output ./artifacts/qa/playmode.xml
unity build ./unity --target Android --execute-method Airlift.Editor.CargoBuild.Build --output-path ./artifacts/airlift-cargo.apk --no-tail
git diff --check
```

If the open-editor route is used, discover the installed Pipeline command schemas
and map the same operations; record exact commands in evidence. Do not guess payloads.
Wrapper must report an actionable editor-lock condition without changing user state.
Do not weaken the dirty-tree build guard: record/approve a checkpoint first or
explicitly request a development dirty-build exception and record its diff hash.
No npm/pnpm commands substitute for compiling C#. The global node binary currently
fails to load a Homebrew simdjson library; do not repair it as part of this plan.
Use installed Node 22 at /opt/homebrew/opt/node@22/bin/node for documentation/server
checks, and its npm CLI path as shown in SERVER-VERIFICATION.md.

## Non-Unity verification

See SERVER-VERIFICATION.md for actual server-handler tests, commands and documentary
gate acceptance. Client stubs cannot prove backend auth or budget controls.
P4-01 is a document-only go/no-go review: no Unity build or headset check required.
It may close with no-go; affected child/conversational scope stays blocked. Core
adult AI-output feasibility now belongs to P1-03 and has its own early gates.

## Cadence

Owner-approved refinement: batch physical review around cargo environment/controls,
AI-guided onboarding and complete whole/halves checkpoints. Automated checks remain
per-ticket; affected tickets can be code-complete/awaiting device review, never
accepted without the relevant batch results. Stop for an interaction regression
before building dependent behavior. CC-P1-01 alone is currently authorized.


Compile and run affected automated tests after meaningful code changes. At ticket
closure run the named complete suite and relevant handler tests. Every visible,
audible or interaction change requires actual Quest checks before owner acceptance.
At P1-03, P1-10, P2-08 and P3-06, replay the entire available journey from the
catalog including mistakes, Help/replay, offline/mute and interruption. Pure
documentary gates do not require redundant APKs. Checkpoints and pushes are
owner-authorized; main-branch merge is not a condition of headset preview.

## Layers and physical acceptance

- EditMode: exact arithmetic, guards, snapshots, idempotency and progression.
- PlayMode: scene wiring, labels, input adapter, AudioSource interruption,
  pointer cues, prefab replacement and reset.
- Device: build/install/launch on the actual Quest 3S, then both controller paths,
  seated/standing reach, 10 grabs, 10 split/merge cycles when available,
  tracking interruption, pause/resume, offline/muted completion and Help/Back.
- A build is not accepted until its SHA-256, manifest tuple, test XML and physical
  outcome are linked. Capture only owner-approved footage with private room details
  excluded. Do not commit APKs or unredacted dumps by default.
- Performance: three complete runs after warm-up on Quest 3S, record CPU/GPU
  p95 separately, dropped frames, thermal state and memory trend. Target 72 Hz
  and each p95 ≤13.9 ms. Memory must stabilize across repeated reset/replay;
  report measured values, not invented device thresholds.
- Test with live AI, sound off and service offline as separate modes; no audio-end
  callback advances learning. CoreAIGuideTests include stale/rejected fact selections,
  false relationships, omitted negation, audio/text mismatch and unsafe-output non-playback and fallback-not-counted-as-live. Test typography
  samples at actual headset scale, not just desktop screenshots.
- Mathematical independent checks submit wrong-but-valid arrangements and verify
  they stay editable, then submit corrected ones. No location/haptic answer leak.

## Rollback

Preserve DeviceProof.unity and existing airlift-safe.apk and airlift-onboarding.apk.
Build CargoCrew separately; retain a per-accepted-ticket checkpoint and artifact
hash. Roll back using a reviewed revert or prior artifact, never reset --hard.
Keep developer credential stripping enabled; scan the rebuilt artifact before install.

## Cargo setting regression

P1-02 adds CargoTerminalLayoutTests: TerminalVisibleOnEntry,
PropsDoNotOccludeMath, RecenterMovesTerminalTogether. P1-07 adds
ParcelJobPresentationTests: AcceptedJobStagesParcel, WrongSubmitKeepsParcel,
DuplicateEventDoesNotDuplicateCargo, ResetRestoresStageView. Later job tickets
rerun these alongside their math suites. P1-10/P2-08 require owner walkthrough
from entry to the actual chapter endpoint; P3-02 adds full dispatch and reduced-motion
checks. No premature dependencies on future props/animations in baseline P1-01.
