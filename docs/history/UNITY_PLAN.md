# Math Workbench: Airlift — authoritative hackathon plan

> Superseded by the [RAP-vetted execution plan](../plans/2026-09-13-unity-vet/plan.md); this original draft is preserved as design history.

Status: approved planning baseline as of September 13, 2026.

Deadline: Friday, September 18, 2026 at 11:59 PM CDT.
Internal submission target: Friday at 6:00 PM CDT.

This document supersedes the original WebXR plan in `00_PLAN.md` through
`03_TECH.md`. Those files remain as research history, not implementation
instructions.

---

## 1. Executive decision

Build a native Meta Quest 3 mixed-reality learning game in Unity.

The submission baseline is one exceptional 6–8 minute fraction experience.
A smaller multiplication experience is permitted only after every baseline
acceptance gate passes. There is no third lesson in the committed scope.

Use Meta XR SDK Building Blocks and prefabs for XR infrastructure. Custom code
is reserved for the mathematics, lesson progression, misconception model,
adaptive response, and meaningful world reactions.

### Platform

- Unity 6.1 or later, Apple Silicon editor
- Universal 3D / URP project
- Standalone Meta Quest 3 Android APK
- Unity OpenXR Plugin plus Unity OpenXR: Meta
- Meta XR All-in-One SDK, with Core and Interaction SDK features
- Controllers are the required input
- Passthrough mixed reality is the required presentation
- Meta XR Simulator is a development aid, not proof of headset correctness
- A native APK, repository, short installation guide, and demo video form the
  review package

### Product

Working title: **Math Workbench: Airlift**.

The learner repairs and loads a small supply aircraft. Mathematical work has a
visible physical consequence: correctly sized cargo straps secure supplies,
array decompositions load the cargo bay, cockpit systems illuminate, and the
aircraft becomes ready for departure.

The airplane theme is a working narrative chosen from the user's preference for
applied, relevant mathematics. It can be reskinned without changing the lesson
architecture.

### Non-negotiable scope

1. Flagship lesson: **The Cut** — fraction magnitude, equivalence, comparison,
   and number-line location.
2. Stretch lesson: **Cargo Grid** — arrays and the distributive property.
3. One visible AI adaptation driven by the learner's action history.
4. No timers, lives, streaks, stars, generic praise, or punitive resource loss.
5. No real child data, child voice, or child footage.
6. No API secrets in the APK.

---

## 2. Outcome and definition of done

The project succeeds when a judge can watch a concise video and understand all
of the following without explanation from the developer:

- The learner manipulates mathematics spatially rather than selecting answers.
- The physical state, diagram, and notation describe the same quantity.
- The system identifies a plausible learner strategy or misconception.
- That diagnosis changes the next scaffold or task.
- Completing the mathematics changes the surrounding world.
- The experience runs reliably on a Quest 3 as a standalone APK.

### Baseline acceptance criteria

- A fresh APK installs, launches, and enters passthrough on the Quest 3.
- The learner completes the entire flagship flow without restarting the app.
- Twenty consecutive controller grabs and releases succeed during a test run.
- All objects return to a valid state after release, reset, or an invalid drop.
- Mathematical correctness comes from deterministic C# logic, not an LLM.
- The concrete, diagrammatic, and symbolic representations stay synchronized.
- At least one action pattern produces a visibly different scaffold.
- AI failure, timeout, or lost network falls back to a local response.
- Text is readable, interaction targets are large, and essential meaning is not
  conveyed by color alone.
- The final build sustains the chosen headset refresh rate during the complete
  experience, not merely in an empty scene.
- The video is under three minutes and the submission is complete by 6:00 PM
  Friday.

### What “fully immersive” means here

It does not mean maximum feature count. It means a coherent spatial loop with
direct manipulation, convincing scale, passthrough, sound, haptics, responsive
objects, and a consequential ending. One polished loop qualifies. Three shallow
minigames do not.

---

## 3. Explicit exclusions

Do not build these before submission:

- A browser or WebXR version
- A general education platform
- Login, learner accounts, cloud persistence, or a database
- Multiplayer
- A teacher dashboard or live cross-device telemetry
- A flight simulator, cockpit simulation, or aerodynamics
- Runtime mesh cutting
- A generic dialogue engine
- Automatic room scanning or required scene understanding
- Hand tracking as a required input
- Voice as a required input
- A reward economy, currency, inventory, achievements, or levels
- A third lesson
- A second hackathon challenge or entry

If a feature is not required by an acceptance criterion, it begins as cut.

---

## 4. Learner and standards target

### Primary learner

A 10-year-old Grade 4 learner who understands basic whole-number operations but
may:

- Treat the larger denominator as the larger fraction
- Attend to the number of pieces instead of the size of each piece
- Fail to recognize equal lengths as equivalent fractions
- Know a multiplication fact without seeing how it decomposes spatially

This target respects the Quest minimum account age while remaining inside the
hackathon's K–5 challenge.

### Standards alignment

The Cut primarily supports:

- CCSS 3.NF.A.2: represent fractions on a number line
- CCSS 3.NF.A.3: explain fraction equivalence and comparison
- CCSS 4.NF.A.1: explain equivalent fractions with visual models
- CCSS 4.NF.A.2: compare fractions using valid reasoning

Cargo Grid supports:

- CCSS 3.OA.B.5: apply properties of operations
- CCSS 3.OA.C.7: fluency grounded in structural understanding
- Preparation for Grade 4 multi-digit multiplication through decomposition

The product will not claim to teach all Grade 4 fractions or certify mastery.
It reports a **provisional proficiency signal** from this session.

References:

- https://www.thecorestandards.org/Math/Content/3/NF/
- https://www.thecorestandards.org/Math/Content/4/NF/
- https://www.thecorestandards.org/Math/Content/3/OA/

---

## 5. Pedagogical contract

### 5.1 Representations

The experience uses a connected sequence rather than forcing a ceremonial
concrete–representational–abstract loop inside every problem:

1. The learner manipulates a unit strip and its segments.
2. A linear number-line overlay makes position and magnitude visible.
3. Fraction notation appears next to the same physical length.
4. Later tasks fade selected physical supports while preserving access to them.

Use these independent fields in code:

- `RepresentationPhase`: `Concrete`, `Diagram`, `Symbolic`
- `DifficultyTier`: `One`, `Two`, `Three`

Never use A/B/C for both concepts.

### 5.2 Mathematical invariants

- Every fraction is relative to a visibly fixed whole.
- Equivalent fractions occupy the same length and position.
- Comparison uses equal wholes.
- The number line remains linear; visual depth never encodes magnitude.
- Symbolic notation never contradicts the spatial state.
- Incorrect placements remain inspectable and repairable.
- The LLM can select a scaffold but cannot declare an answer correct.

### 5.3 Feedback

Feedback describes the task or strategy:

- “Both straps reach the same mark.”
- “Eight pieces made each piece smaller.”
- “That decomposition preserved all 42 cargo spaces.”

Avoid person-level praise such as “You are smart.” Avoid alarms, shaming,
countdowns, lost lives, and consuming a limited material after mistakes.

The world provides intrinsic payoff: a strap locks into place, a cargo indicator
turns on, the engine checks complete, or the aircraft begins its departure
sequence. These effects reward successful mathematical action without obscuring
the learning goal.

### 5.4 Choice and explanation

- The learner may replay instructions, reset a task, or skip an optional verbal
  explanation.
- Request one short explanation after an informative task, not after every item.
- The required experience does not depend on speech recognition.
- If explanation input is absent, the action trace remains sufficient for the
  adaptive demonstration.

### 5.5 Evidence claims

Use modest language in the submission:

- “Designed using connected representations and concreteness fading,” not
  “proven to teach fractions.”
- “Produces a provisional proficiency signal,” not “measures mastery.”
- “Demonstrates adaptive scaffolding,” not “maintains an optimal 85% success
  rate.”
- Spacing, long-term retention, and broad transfer belong in the future-study
  plan, not the prototype claims.

Evidence base:

- IES mathematics intervention guide:
  https://ies.ed.gov/ncee/wwc/PracticeGuide/26
- Concreteness fading review:
  https://eric.ed.gov/?id=EJ1036777
- CAMIL model for immersive learning:
  https://link.springer.com/article/10.1007/s10648-020-09586-2

---

## 6. Flagship experience: The Cut

### Premise

An airfield workbench needs cargo straps of precise fractional lengths. A unit
strip marked `1` establishes the whole. The learner makes, places, overlays, and
compares straps before loading them onto the aircraft.

“Cutting” never performs runtime mesh slicing. At a valid cut mark, an animation
hides the source strip and activates prebuilt segment prefabs with exact
dimensions. This is predictable, testable, and visually convincing.

### Interaction grammar

- Grip to pick up a piece
- Move it directly with the controller
- Release near a valid zone to snap
- Invalid release returns gently to the prior valid pose
- Trigger on the cutting guide to confirm a prepared cut
- Ray interaction operates menus, replay, reset, and exit
- A short haptic pulse confirms a valid snap

### Task sequence

1. **Establish the whole**
   Place the unit strip against the workbench ruler. The number line labels `0`
   and `1`.

2. **Construct one half**
   Set a cut guide at `1/2`, create two equal segments, and place one on the
   number line.

3. **Construct three fourths**
   Partition a new equal whole into fourths and assemble a strap ending at
   `3/4`.

4. **Reveal equivalence**
   Construct `6/8` and overlay it with `3/4`. Equal endpoints and a transparent
   overlay reveal equivalence before the equation appears.

5. **Diagnose comparison reasoning**
   Compare `3/4` and `5/8`. Log whether the learner compares piece counts,
   endpoints, benchmarks, or repeatedly rearranges segments.

6. **Transfer**
   Given a required strap length, choose and arrange equation tiles that match
   the built length. Physical support is reduced but remains available through
   an optional reveal.

7. **World payoff**
   The correct straps secure the cargo. Aircraft lights activate and a short,
   non-playable departure animation closes the experience.

### Misconception routes

| Observed trace | Candidate interpretation | Scaffold |
|---|---|---|
| Selects `5/8` as larger because 8 is larger than 4 | Denominator-as-size | Show equal wholes divided into 4 and 8; highlight piece size |
| Builds the correct count from unequal wholes | Whole not conserved | Lock both strips to the same unit frame |
| Recognizes overlay equality but rejects `3/4 = 6/8` | Symbol–magnitude disconnect | Animate labels onto the unchanged endpoints |
| Repeatedly counts pieces without checking endpoint | Count-focused strategy | Fade internal divisions and emphasize total length |
| Solves immediately and explains via benchmark | Strong strategy evidence | Advance to transfer with less scaffolding |

These are hypotheses, not diagnoses of a child. The interface describes the
observed strategy rather than labeling the learner.

---

## 7. Stretch experience: Cargo Grid

Build this only after the flagship passes every Wednesday gate.

The learner loads supply crates onto a rectangular cargo grid:

1. Fill a `7 × 6` grid.
2. Split it into `7 × 5` and `7 × 1` regions.
3. Physically separate and recombine the regions.
4. Assemble `7 × 6 = (7 × 5) + (7 × 1)` from equation tiles.
5. Watch the cargo load indicator preserve the total of 42 spaces.

Use one grid, one decomposition, and one transfer prompt. Do not implement a
general multiplication curriculum.

---

## 8. AI role

### Required demonstration

The AI receives a compact, derived action history after an informative task. It
returns one structured interpretation and recommended scaffold. The next task
visibly changes as a result.

Example request data:

```json
{
  "task": "compare-3-4-and-5-8",
  "attempts": 2,
  "placements": ["5/8", "3/4"],
  "endpointChecks": 0,
  "pieceCountChecks": 3,
  "usedOverlay": false
}
```

Example response:

```json
{
  "strategy": "denominator-as-size",
  "confidence": 0.86,
  "nextScaffold": "equal-whole-overlay",
  "feedback": "Compare the size of one fourth with one eighth."
}
```

### Guardrails

- C# evaluates mathematical correctness before any network call.
- The model receives no name, voice, image, persistent identifier, or raw child
  data.
- A serverless proxy owns the provider secret; the APK never does.
- Validate the response against a closed schema and allowlisted scaffold IDs.
- Use one provider and one model during the hackathon.
- Timeout after five seconds and use a deterministic local mapping.
- Cache or debounce requests so repeated object movements do not call the model.
- Disclose the provider, model, generated code, and AI-assisted assets.

### Baseline and stretch

Baseline: action-trace classification and adaptive scaffold.

Stretch: optional push-to-talk explanation using one speech service. It has a
strict two-hour implementation limit and may not delay capture or submission.

---

## 9. Unity implementation plan

### 9.1 First installation

Install through Unity Hub:

- Unity 6.1+ Apple Silicon
- Android Build Support
- OpenJDK
- Android SDK & NDK Tools

Create a Universal 3D project. Enable the Meta Quest build profile. Use OpenXR,
not the deprecated Oculus XR Plugin. Install one compatible, stable Meta XR
All-in-One release and commit the Unity package manifest and lock file. Do not
upgrade packages during the build unless blocked by a confirmed defect.

Official setup references:

- https://developers.meta.com/horizon/documentation/unity/unity-development-requirements/
- https://developers.meta.com/horizon/documentation/unity/unity-project-setup/
- https://developers.meta.com/horizon/documentation/unity/unity-and-openxr-compatibility/

### 9.2 Meta Building Blocks

Use prefabricated Meta components for:

- Passthrough
- Camera / Interactions Rig
- Controller tracking
- Grab Interaction
- Ray Interaction
- Haptics

Use a player-relative workbench with a recenter button. Do not require room
scanning, plane detection, anchors, depth occlusion, or MR Utility Kit queries.

Controllers are the shipping input. Hand tracking may be evaluated only after
controller acceptance passes and may never be required to complete a task.

### 9.3 Scene and prefab structure

Keep one production scene until after submission:

```text
Assets/
  Airlift/
    Scenes/Airlift.unity
    Scripts/
      Runtime/
      Math/
      Tutor/
      UI/
    ScriptableObjects/Lessons/
    Prefabs/
      Interaction/
      FractionPieces/
      Workbench/
      Aircraft/
    Audio/
    Materials/
    Tests/EditMode/
```

Do not reorganize imported Meta packages or edit vendor source.

### 9.4 Small custom architecture

- `LessonDirector`: advances through task states
- `LessonDefinition`: ScriptableObject containing ordered task definitions
- `FractionTaskDefinition`: values, representation phase, allowed pieces,
  misconception routes, and next states
- `MathPiece`: immutable mathematical identity plus current visual state
- `SnapZone`: accepts allowlisted pieces and emits placement events
- `FractionMath`: pure normalization, equivalence, comparison, and endpoint logic
- `RepresentationPresenter`: synchronizes objects, number line, and notation
- `WorldFeedback`: audio, haptic, light, and aircraft reactions
- `SessionEventLog`: in-memory append-only action trace
- `TutorClient`: calls the backend and validates structured responses
- `LocalScaffoldRouter`: deterministic fallback

Avoid dependency injection, a general event bus, custom ECS, save migrations, or
a framework for loading arbitrary future lessons.

### 9.5 Data-driven tasks

Author task values as ScriptableObjects so a new problem does not require a new
scene or duplicated scripts. Validate them in editor tooling or EditMode tests.

The generic system should extend only as far as the six flagship tasks and one
Cargo Grid task require. Do not generalize speculatively.

### 9.6 Backend

Use one minimal HTTPS serverless endpoint:

```text
POST /api/tutor
  validate request
  remove unexpected fields
  call model with fixed prompt and closed schema
  validate and allowlist response
  return structured JSON
```

The repository may contain the backend beside the Unity project, but it is
deployed independently. Store secrets only in host environment variables.

### 9.7 Performance

- Select 72 Hz for the baseline and preserve frame time under full lesson load.
- Prefer primitives, simple meshes, atlased materials, and unlit or inexpensive
  URP shaders.
- Use object pooling or pre-created segment objects; do not allocate repeatedly
  during manipulation.
- Avoid real-time shadows unless the complete scene has measured headroom.
- Profile on Quest, because Simulator performance is not representative.
- Final build uses ARM64 and IL2CPP.

---

## 10. Coding versus manual work

Expected 45-hour allocation:

| Work | Hours | Dominant mode |
|---|---:|---|
| Unity, Android, SDK, and Quest setup | 4 | Manual configuration |
| Scene, prefabs, and spatial composition | 6 | Unity Editor |
| Lesson logic and mathematical evaluation | 10 | C# |
| Interaction glue and world responses | 7 | C# + Editor |
| AI proxy and TutorClient | 4 | Code |
| Quest testing and performance fixes | 6 | Device work |
| Visual/audio polish | 3 | Editor |
| Video, disclosures, and submission | 5 | Production |

Approximately 47% is code, 29% is setup/editor authoring, and 24% is testing and
submission. Manual setup is front-loaded. Meta Building Blocks prevent it from
becoming the majority of implementation.

---

## 11. Execution schedule

### Optional preflight — Sunday, September 13, maximum four hours

- Install Unity Hub and the supported Apple Silicon editor.
- Install Android modules.
- Enable Quest developer mode and confirm USB/ADB access.
- Create the URP project and install Meta XR packages.
- Add passthrough, Interactions Rig, and one grabbable cube.
- Build and Run on Quest.

If this preflight is unavailable, it consumes the first four hours of Monday.

### Day 1 — Monday: prove the entire technical path

Morning:

- Finish setup and resolve all Meta Project Setup Tool errors.
- Produce a clean APK from a clean project open.
- Validate passthrough and controller input on Quest.

Afternoon:

- Create the fixed/recenterable workbench.
- Implement one unit strip, one cut-guide interaction, prebuilt half segments,
  one snap zone, and synchronized notation.
- Add reset and complete one beginning-to-end task.
- Commit the first vertical slice.

Day 1 gate:

- APK launches on Quest.
- Passthrough works.
- Ten consecutive grabs work.
- One fraction task completes and resets.

If the gate fails by Monday evening, stop adding product features and resolve the
device path. If the install/build path alone consumed the whole day, reconsider
IWSDK before writing Unity-specific game code.

### Day 2 — Tuesday: complete The Cut

- Implement all six tasks with ScriptableObject data.
- Add number line, equation tiles, equivalence overlay, and representation fade.
- Implement deterministic misconception routes.
- Add task/process feedback, haptics, audio, and aircraft state changes.
- Add EditMode tests for fraction math and progression.
- Test the complete loop repeatedly on Quest.

Day 2 gate:

- A complete 6–8 minute offline lesson works without AI.
- No timer, lives, punitive loss, or dead-end state remains.
- Every symbolic value matches the spatial state.

### Day 3 — Wednesday: make the AI consequential

- Define the minimal session-event schema.
- Deploy the one-route serverless proxy.
- Implement structured classification and allowlisted scaffold selection.
- Add the local fallback and five-second timeout.
- Make at least two traces produce different next scaffolds.
- Run failure tests with the network disabled.
- Begin visual and audio cleanup.

Wednesday expansion gate at 3:00 PM:

Cargo Grid is authorized only if the flagship, AI adaptation, APK build, reset,
and offline fallback all pass. Otherwise Thursday is flagship polish.

### Day 4 — Thursday: conditional lesson two, polish, and capture

If authorized, spend no more than five hours on Cargo Grid. It gets one grid,
one decomposition, one equation, and one payoff.

Then:

- Fix legibility, scale, hand reach, snap tolerances, and audio balance.
- Test fresh install, application pause/resume, recenter, invalid drops, and API
  timeout.
- Finish dependency, asset, API, and AI disclosures.
- Draft final submission copy and video narration.
- Feature freeze by 6:00 PM.
- Capture final raw Quest footage Thursday night.

No voice work begins after noon Thursday.

### Day 5 — Friday: evidence and submission

- Run the final regression matrix on the release APK.
- Fix blockers only.
- Edit a 2:15–2:40 video.
- Produce the APK, checksum, installation steps, repository README, and known
  limitations.
- Verify every download from a clean browser and reinstall the submitted APK.
- Submit by 6:00 PM CDT.
- Preserve the remaining time for upload, form, and rendering failures.

---

## 12. Test plan

### Automated EditMode tests

- Fraction normalization
- Equality such as `3/4 == 6/8`
- Comparison across supported denominators
- Same-whole invariant
- Valid and invalid snap acceptance
- Task transition table
- Local misconception-to-scaffold routing
- Tutor response schema rejection

### Quest manual matrix

- Fresh install and first launch
- Passthrough permission accepted and denied
- Twenty grabs/releases
- Near and edge-of-zone drops
- Reset during every task
- Recenter from two seating positions
- Controller temporarily loses tracking
- App pause and resume
- Wi-Fi disabled before AI task
- AI timeout and malformed response
- Complete playthrough at release performance settings
- Final completion and replay

Log defects by severity:

- P0: cannot launch, enter MR, interact, progress, or complete
- P1: wrong mathematics, broken adaptation, misleading representation, severe
  discomfort, unreadable UI
- P2: visible polish issue with a safe workaround

Thursday fixes P0 and P1. Friday fixes P0 only unless a P1 invalidates the demo.

---

## 13. Submission and compliance

The submission package contains:

- Native Quest APK
- SHA-256 checksum
- Short sideload/install instructions
- Source repository if permitted
- Two-to-three-minute video
- Written product, learning, AI, and technical descriptions
- Third-party dependency and asset inventory with licenses
- AI-use disclosure
- Known limitations and future evaluation plan

Rules source: https://hackathon.nerdy.com/terms

### Privacy

- The entrant is the only person shown in footage.
- Use synthetic session data and a fictional learner trace.
- Do not collect names, account IDs, images, raw audio, or persistent device IDs.
- Keep the action log in memory and discard it when the application closes.
- Future research with minors requires consent, assent, data minimization, and an
  approved protocol; it is not part of this build.

### Licensing

- Record every Unity package, Meta SDK, model, font, sound, texture, and 3D asset.
- Record the exact source and license at the moment an asset enters the project.
- Do not use GPL, LGPL, AGPL, SSPL, unlicensed, ripped, or “editorial use only”
  material.
- Prefer self-created primitives and clearly licensed CC0/MIT/Apache/commercially
  permitted assets.
- Do not attack or reproduce another entrant's or Nerdy's branded game assets.

### Video outline

1. `0:00–0:15` — learner problem and product thesis
2. `0:15–0:35` — Quest passthrough and direct manipulation
3. `0:35–1:15` — build `3/4`, overlay `6/8`, reveal equivalence
4. `1:15–1:45` — misconception trace and visibly changed scaffold
5. `1:45–2:10` — transfer task and aircraft payoff
6. `2:10–2:30` — pedagogy, standards, privacy, and future evaluation

Frame the competitive distinction positively:

> Most digital practice observes the answer. Math Workbench observes the
> learner's strategy because the mathematics exists as something they can build,
> compare, and repair.

---

## 14. Risk register and kill rules

| Risk | Early signal | Response |
|---|---|---|
| Unity setup consumes the first day | No Quest cube by Monday noon | Finish one focused troubleshooting window; then reconsider IWSDK |
| Grab interactions are unreliable | Fewer than 18/20 clean attempts | Increase target size and snap tolerance; controllers only |
| Runtime cutting becomes complex | Mesh or collider defects | Use prefab swapping only |
| AI feels decorative | Same next task regardless of response | Require two visibly different scaffold routes |
| Network latency interrupts flow | Response exceeds five seconds | Use immediate local fallback and log the late response only |
| Content remains below target grade | Tasks stop at naming halves/fourths | Require equivalence, comparison, and symbolic transfer |
| Cargo Grid threatens flagship polish | Any P0/P1 remains Wednesday | Cut Cargo Grid completely |
| Voice threatens capture | Not working by Thursday noon | Delete/disable it from release build |
| Native delivery confuses judges | APK/install path unverified | Provide exact steps, checksum, footage, and clean-download test |

The single most important kill rule is this:

> No stretch feature survives while the flagship has a known P0 or P1 issue.

---

## 15. Immediate next actions

1. Install Unity Hub and Unity 6.1+ Apple Silicon with Android modules.
2. Create the URP project inside this repository or in a clearly named `unity/`
   directory.
3. Initialize Git before importing large assets and add a Unity `.gitignore`.
4. Install and lock the OpenXR and Meta XR packages.
5. Build the passthrough + grabbable-cube proof on Quest.
6. Record the result of the four-hour gate in this document.
7. Only then implement the first fraction task.

Do not create production artwork, tutor prompts, or additional lessons before
the device proof succeeds.
