# Cargo Crew — first-lesson story and interaction script

Draft authored teaching script. Lines below are grounding examples and fallback
copy, not existing audio. Core AI guidance must preserve their learning intent and
correct facts while responding to actual lesson state; see AI-VOICE-GUIDE.md. Captions convey the same instruction. Never narrate a result before the
corresponding successful state transition. Use “strap piece,” not “cube,” once
length becomes the mathematical representation.

## Stage flow

| Stage / ticket | Player sees and hears | Action / pointer | Advance condition and repair |
|---|---|---|---|
| Catalog / P1-02 | Arithmetic Lessons. Cargo Crew / Fractions. Café and Garden Coming soon. “Join the cargo crew. Learn how parts make a whole.” | Ray pointer and trigger icon on Cargo Crew | Explicit selection; no disabled-card activation |
| Mission / P1-03 | Miniature terminal: parked truck, containers, destination tags, parcels, loading/measuring platform and aircraft. “We’re preparing a model delivery. Packages need different strap lengths. You’ll make, compare, and check those lengths.” | Begin button; no object demands yet | Begin; replay or Back always available |
| Comfort / P1-02 | Bench height/recenter controls. “Put the workbench where you can reach comfortably. Keep your feet in place.” | One location highlight; no forced reach | Player confirms placement; adjust later only with released pieces |
| Demonstrate / P1-03 | Ghost strap moves tray to measuring pad. “Hold the grip button under your middle finger. Move the strap to the pad, then let go.” | Handed grip icon then pad outline | Demo ends; Your Turn is explicit, not mistaken for autonomous user action |
| Practice / P1-03 | Real strap and neutral pad. “Your turn. Pick up the strap and place it on the measuring pad.” | Piece → pad; no competing pointers | Real grab then release in pad. Miss: “Try releasing over the outlined pad.” No penalty |
| Whole / P1-04 | Whole strap labeled 1; ruler 0 to 1; reference ghost separate. “This strap is one whole. All our pieces will be measured against this same length.” | Whole then endpoints; learner places whole and selects 1 | Submit confirms actual placement and notation. Retry explains whole reference |
| Halves / P1-05 | Selected whole at split station. “Split the whole into two equal parts. Each part is one-half of this whole.” | Split button; later optional A+B / X+Y chord tutorial | One split transaction, two labels 1/2, old whole outline remains. Cancel leaves one coherent state |
| Rebuild / P1-06 | Two guided positions spanning reference. “Put one half here and the other half beside it. Together, two halves make one whole.” | Highlight first then second slot | Actual two placements, Submit, expression 1/2 + 1/2 = 1; no Next-only bypass |
| First delivery / P1-07 | Truck-delivered parcel bears a destination tag and requests 1/2; neutral ruler lane. “This package needs half the whole length. Prepare the strap, then check your work.” | Task lane, not answer endpoint | Submit exact half moves the parcel to the ready staging area; wrong-but-valid whole remains: “That reaches one whole. Compare it with the half-length request.” |
| Meaning / P1-08 | Current split and numeral parts emphasized. “Two tells us the whole was split into two equal parts. One tells us how many of those parts we’re using.” | Label numerator/denominator, not a new task | Learner chooses model-matching 1/2 from 1/2, 2/1, 1/1; feedback describes part count |
| Quarters / P2-01 | Whole reference, two halves ready. “Split each half into two equal parts. Now the whole has four equal parts. Each is one-quarter.” | First eligible half then second | Two guarded splits; four unique active pieces 1/4; total remains 1 |
| Three quarters / P2-02 | Parcel requests 3/4; neutral lane and four quarters. “Use three of the four equal parts. How much of the whole have you used?” | Task zone; no correct-length ghost | Place three, choose 3/4, Submit; show 1/4 + 1/4 + 1/4 = 3/4, remaining quarter stays in supply |
| Join / P2-03 | Two sibling quarters with matching seam marks; join station. “Bring these two quarter pieces together. Release them at the join station. Two quarters make one-half.” | Both pieces then seam; alternative sequential dock + Join | Both selectors released, atomic merge; labels 2/4 = 1/2; invalid pair explained, never silently consumed |
| Equivalents / P2-04 | Two equal-whole lanes: 1/2 and constructed 2/4, then 3/4 and constructed 6/8. “Different numbers of smaller parts can cover the same length.” | Split eligible pieces; neutral comparison ruler | Player generates and submits equivalents and selects same-endpoint reason; animation alone does not pass |
| Comparison / P2-05 | Equal reference lanes with 3/4 and 5/8. “Which is longer? Place a comparison sign between them and use the ruler to explain.” | Sign/reason controls, no answer highlight | Submit 3/4 > 5/8 and endpoint-based reason; wrong sign stays editable with descriptive feedback |
| Fresh delivery / P2-06 | New values: make 4/8 matching 1/2; compare 1/4 and 3/8. “Check this shipment yourself. Help is here if you want it.” | Help and Submit only, no answer ghost | Record first response and help use; require 1/2 = 4/8 and 1/4 < 3/8, allow repair; this is near transfer |
| Dispatch / P3-02 | Accepted parcels move from staging past the containers/loading platform to the miniature aircraft, checklist of actual completed jobs. “You made equal parts, rebuilt a whole, and compared lengths. The model delivery is ready.” | Replay chapter / Back to Lessons | No mastery badge; final summary matches actual completed chapter coverage |

## Interaction rules around the script

- Guidance is one action at a time. Give action time; no countdown or silence interpreted
  as failure. A replay button never restarts the whole lesson unexpectedly.
- Before independent tasks, fade endpoint ghosts and correct-answer slot counts.
  Pointers identify controls, not correct quantities.
- For a wrong quarter answer: “You used two quarters. The request is three quarters.
  Compare the number of equal pieces.” Preserve all pieces.
- For unequal-looking placement: “The pieces still need to line up from zero.”
  Distinguish motor alignment from mathematical errors.
- Never claim real aircraft restraint safety. This is a model-length learning scenario,
  not actual cargo engineering or certified loading instruction.
- Baseline AI guidance receives structured actions, not learner speech. Ray Help
  triggers contextual AI explanation; optional spoken questions remain separately gated.

## Cue asset contract

One cue ID per line/action: catalog.invite, mission.brief, comfort.place,
practice.demo, practice.try, whole.reference, halves.split, halves.rebuild,
halves.delivery, halves.notation, quarters.split, quarters.build,
quarters.join, equivalent.half, equivalent.threequarters, compare.prompt,
transfer.prompt, dispatch.summary. Include separate error/recovery cues.
The manifest stores caption, clip, speaker-rights record and pointer target.
QA verifies each exists, has nonempty captions, and resolves to the correct stage.
Audio naming uses cue IDs; script version/hash prevents silently mismatched captions.

## Chapters versus phases

Chapter 1 becomes playable through Phase 1's ten tickets. Chapters 2–3 become playable
through Phase 2's eight. Phase 3 hardens and completes the presentation/release.
Phase 4 does not add a fourth lesson; it adds separately gated spoken-question
input/child-release review. The core AI spoken guide already accompanies P1–P3.

## Cargo continuity

P1-02 introduces recognizable static terminal props; P1-03 orients the learner to
their purpose. P1-07 creates the parcel-progress presentation, reused by P2-02 and
P2-06. P1-10/P2-08 review the visible chapter journey. P3-02 completes dispatch,
not the first appearance of the cargo theme. Decorative objects neither grade
answers nor change the fixed mathematical reference whole.
