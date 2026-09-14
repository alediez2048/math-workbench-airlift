# Requirements — Math Workbench: Airlift

Draft v0.1 · September 14, 2026 · implementation not started in this directory.

This is the acceptance contract, not a progress report. It derives from the
[vetted execution plan](docs/plans/2026-09-13-unity-vet/plan.md), which remains
authoritative. Product intent lives in [prd.md](prd.md), boundaries in
[constraints.md](constraints.md), and implementation choices in [techstack.md](techstack.md).
All requirements below are **unverified** until named evidence is recorded.

## Release scope

**Must** means required for the submitted baseline. **Conditional** means enabled
only after the stretch gate. Priority is not defect severity: an incorrect
mathematical result is a release blocker even inside an optional feature.

The baseline is one complete 6–8 minute fraction lesson, **The Cut**, organized
into three chapters. It is not three independent lessons. **Cargo Grid** is the
only conditional second lesson. No third lesson or second hackathon challenge.

## Functional requirements

| ID | Priority | Requirement | Acceptance evidence |
|---|---|---|---|
| FR-01 | Must | Run a native standalone Quest 3 APK with passthrough, a single rig, and controllers. | Fresh install, launch, passthrough, ten grabs, pause/resume, readable 10-second capture on physical Quest. |
| FR-02 | Must | Place the workbench once from player pose, then world-lock it. Offer explicit recenter and seated height adjustment. | Turning/moving the head does not drag the board. Recenter while holding waits for release, preserves arrangement, and invalidates pending support. |
| FR-03 | Must | Provide six ordered task assets T01–T06, task instruction replay, reset, pause, and end-of-lesson replay. | Complete every task offline without reload or dead end; replay/reset does not mix attempts. |
| FR-04 | Must | Establish a visible fixed whole, equal partitions, and a connected 0–1 number line. | Halves/fourths/eighths are exact; all comparison lanes have the same whole length. |
| FR-05 | Must | Permit neutral placement of both correct and incorrect quantities within a whole. | Wrong shorter/longer arrangements persist; only overlap, unsafe drops, or overflow beyond the whole are rejected. Hover/snap/haptics do not reveal correctness. |
| FR-06 | Must | Keep physical quantity, number-line endpoint, and canonical notation synchronized; keep the student's proposed equation separate. | Six eighths keeps its authored 6/8 notation although its normalized value equals 3/4. An incorrect equation cannot change the board. |
| FR-07 | Must | Evaluate only an explicit Submit using deterministic rules. | Correct submission advances; incorrect submission keeps the arrangement repairable; duplicate Submit does not advance twice. |
| FR-08 | Must | Let learners construct equivalence and comparison statements through independently editable numerator, denominator, and operator fields. | T04 accepts a generated 3/4 = 6/8 statement; T05 records inequality plus an explicit model-backed reason. No precomposed answer-only tiles. |
| FR-09 | Must | Apply evidence-bounded AI support after eligible incorrect T04/T05 submissions. | Two synthetic traces with the same wrong answer but different explicit reasons receive the expected visible scaffolds; ambiguous evidence receives neutral support. Record actual live calls. |
| FR-10 | Must | Keep the entire lesson usable without inference or connectivity. | Disabled model, timeout, refusal, malformed output, and exhausted budget all apply local authored support. Correct answers never wait on a model. |
| FR-11 | Must | Prevent stale or duplicate model responses from changing the world. | One request in flight, one application per attempt; timeout wins at five seconds; reset/recenter/advance invalidates old responses. |
| FR-12 | Must | Record only observable, bounded session evidence and distinguish first attempt from supported completion. | T06 records initial answer, support requests, and final result separately. Replays are practice, not new assessment. |
| FR-13 | Must | End with a clear mathematical consequence in the world. | Correct cargo-strap work triggers attachment, cargo lights, sound/haptic feedback, and a short departure animation. No flight controls or score economy. |
| FR-14 | Conditional | Add Cargo Grid using one 7×6 array, split into 7×5 + 7×1. | Total remains 42; recombination/equation agree; existing release paths still pass; Wednesday gate and five-hour limit respected. |

## Task-level learning acceptance

| Task / chapter | Required learner action | Required observation |
|---|---|---|
| T01 Whole / Build | Align one unit strap to the 0–1 ruler. | Whole identity and grab/release understood operationally. |
| T02 Half / Build | Partition into two equal pieces and place 1/2. | Physical endpoint and notation agree. |
| T03 Three fourths / Build | Assemble three fourth-pieces. | 1/4 + 1/4 + 1/4 = 3/4 is connected to the model. |
| T04 Equivalent / Compare | Split each fourth into two and construct an equivalent statement. | Same length, doubled part count; ×2 on numerator and denominator. |
| T05 Compare / Compare | Compare 3/4 and 5/8; commit an inequality and a reason. | Equal wholes, explicit reasoning evidence, repair after wrong submission. |
| T06 Near transfer / Apply | Construct 1/2 = 4/8; compare 1/4 and 3/8 with reduced initial support. | No answer preview; separate first-attempt/support/final evidence. |

Alignment is to selected portions of 3.NF.A.2, 4.NF.A.1, and 4.NF.A.2, not full
coverage of a grade or standard. See the [standards and evidence](docs/plans/2026-09-13-unity-vet/research.md).

## Nonfunctional requirements

| ID | Requirement | Pass condition |
|---|---|---|
| NFR-01 | Comfort and access | No artificial locomotion/camera shake; one-controller completion; seated reach; labels/shapes preserve meaning without color; mute does not remove instructions. Verify physically. |
| NFR-02 | Performance | Profile the entire lesson at the chosen 72 Hz; project target p95 CPU and GPU frame time each ≤13.9 ms, no sustained reprojection or thermal decline in three consecutive runs. Record actual metrics. |
| NFR-03 | Mathematical reliability | Automated tests cover quantity conservation, mixed denominations, notation, overlap, equal wholes, wrong-answer persistence, and reset/progression. |
| NFR-04 | Security and cost | No secrets in app/repository/logs. HTTPS proxy, closed schemas, ≤8 KiB request, bounded event/output counts, no automatic retries, rate limiting, owner-set provider controls, kill switch. |
| NFR-05 | Data minimization | No learner names, images, raw voice, account IDs, persistent device IDs, or inferred gaze/thought. Use synthetic traces and entrant-only footage. Infrastructure logs are a separate disclosure. |
| NFR-06 | Reproducibility | Freeze the physically proven editor/package tuple, commit lock files and shared contract fixtures, keep APK checksum and test reports. |
| NFR-07 | Honest evidence | Separate live model, local fallback, and fixture playback; compare model routing against local rules; do not claim diagnosis, efficacy, retention, or mastery from this session. |
| NFR-08 | Submission readiness | Working experience, English description, functioning video ≤3 minutes, required disclosures, accepted rights/license decisions, and accessible evaluation artifacts. |

## Definition of done

- Automated EditMode, PlayMode, backend, and shared-schema tests pass with nonzero test counts.
- Physical fresh-release installation and full offline/live/fallback playthroughs pass.
- All enabled paths have zero P0/P1 defects. Treat crashes/dead ends, wrong math,
  unsafe required interaction, secret leakage, and broken adaptation claims as blockers.
- Release evidence records versions, device state, timings, limitations, and APK hash.
- Submission footage shows the actual game, not these mockups; optional work is accepted or disabled.

Evidence destinations and exact test names are defined in the execution plan.
The mockups are design review aids and cannot satisfy device or learning acceptance.
