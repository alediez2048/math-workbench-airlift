# Cargo Crew — VR/AR and Unity style guide

**Version:** draft 0.1 · September 15, 2026 · **Owner visual approval pending.**
September 15 execution decision: owner delegated routine visual choices. Selected A,
Nunito Bold headings with Nunito Sans Semibold body/math; final readability reviewed
on headset. This supersedes the font-selection gate below, not device acceptance.
This is the shared visual/interaction reference for future implementation.
It does not change the currently installed demo or authorize font downloads.
Applies to the Unity/Quest 3S mixed-reality game; no browser UI rewrite.

## 1. Creative direction

**Friendly miniature workshop, thoughtfully made.** Clean, warm and tactile;
clear enough for a child without looking babyish. A small air-delivery workbench,
rounded cargo tags, restrained parcel details and a charming aircraft payoff.
Use simple silhouettes and matte surfaces. Learning objects are the visual focus.
Avoid military cockpit clutter, generic sci-fi neon, chrome, rainbow panels,
tiny labels, heavy gradients, excessive glass and full-screen instructions.

September 16 realisation: the owner chose a playful toy direction ("Hasbro, Lego").
Implemented as rounded generated blocks with satin plastic materials, a warm key light with
soft shadows, pill-shaped buttons on a rounded card, sticker-style fraction labels and a
brighter palette (orange 255/168/78, mint 88/222/190, cream, yellow 255/214/92, slate
58/78/118). Board props remain simple original geometry; no purchased assets.

Three priorities: mathematical truth → physical comfort/readability → visual polish.
Polish must reinforce the first two. The current blue/orange proof is an interaction
baseline, not a finished art direction. P1-02 reviews alternatives before adoption.

## Cargo terminal setting — owner-requested revision

The player sees their room with a miniature cargo terminal, not a full virtual port.
From entry to Cargo Crew, include a recognizable delivery truck, at least two
container silhouettes, parcel staging area, measuring/loading platform, destination
tags and parked miniature aircraft. Use simple original or rights-cleared prefabs;
no marketplace purchase is assumed. P1-02 owns this visible setting, not Phase 3.

Keep math at the front within comfortable reach, truck/containers at the sides or
rear without blocking hands, labels or captions. Containers are context, not
fraction units: strap length is measured against the fixed visible reference whole.
Do not imply container size, cargo mass or restraint safety is determined by these
fractions. No vehicle driving, locomotion, real-room scanning or custom physics.

Story sequence: arrival → prepare requested strap length → check the job → move
the completed parcel to staging → final aircraft dispatch. P1-03 teaches the role
using the actual props; P1-07 introduces stage-completion parcel feedback, reused
by later jobs. Guided splits/rebuilds are training, not silently shipped orders.
Independent orders advance only after deterministic Submit acceptance. Repeated
callbacks/replay never duplicate cargo; Reset/Back restore the appropriate stage
view from authoritative progress, not accumulated animation events.

Use text/symbols as well as color for destination and job status. No flashing
beacons or distracting vehicle motion during explanations/manipulation.
Reduced-motion mode uses static completed-state changes. Never move the camera.
Full dispatch remains P3-02; an early chapter ending is labeled as partial progress.

## 2. Typography system

Current code uses TMP_Settings.defaultFontAsset, with a first-found fallback in
CreateOnboarding.cs. Replace that accidental choice with named, explicit style assets.

### Candidate directions for owner review

| Direction | Titles | Instructions/buttons/math | Character |
|---|---|---|---|
| A — friendly workshop (recommended trial) | Nunito Bold | Nunito Sans Medium / Semibold | Warm headings, clean practical reading |
| B — restrained modern | Nunito Sans Bold | Nunito Sans Medium / Semibold | One coherent family, quieter appearance |

These are shortlist proposals, not selected or installed fonts. Google Fonts'
[source description](https://github.com/google/fonts/blob/main/ofl/nunitosans/DESCRIPTION.en_us.html)
distinguishes rounded Nunito from non-rounded Nunito Sans. Inspect exact downloaded
font license/version and retain notices before import. A family name alone is not
a license record or proof of all required glyphs.

### Roles and starting hierarchy

| Token | Weight / treatment | Relative size | Use |
|---|---|---|---|
| type.lessonTitle | Bold, sentence/title case | 1.8 × body | Arithmetic Lessons / Cargo Crew |
| type.sectionTitle | Semibold | 1.3 × body | Current job, chapter heading |
| type.body | Medium, left aligned | 1.0 | One short actionable instruction |
| type.button | Semibold | 1.1 × body | Verb-first actions |
| type.caption | Medium, high contrast backing | ≥ body | Exact approved spoken content |
| type.math | Medium/Semibold, stable digit alignment | 1.3–1.6 × body | Numerator, denominator, equation |
| type.secondary | Medium | ≥0.9 × body | Supporting metadata, never required action |

Body starting target: apparent capital height around 0.4–0.6 degrees at the intended
viewing distance. At 1 m this is approximately 7–10 mm. These are **project trial
values**, not Meta requirements or validated child-legibility thresholds. Measure
glyph cap height, not only Unity fontSize or canvas pixels. Enlarge or reflow for
comfort; never shrink until instructions technically fit.

Use 1.3–1.45 line spacing as a trial, short lines, normal letter spacing and generous
margins. Prefer 1–3 lines per action; show one action at a time. No all-caps paragraphs,
ultra-light, condensed or decorative body text. Avoid centering multi-line prose.
Keep titles and actions visually separate; no giant title consuming the task view.

### Fraction notation

Render arbitrary fractions with a dedicated stacked numerator/bar/denominator view,
not a mixture of font-specific precomposed fraction characters. Digits must be
equally legible; numerator/denominator never tiny superscripts. Keep bar length/
weight consistent, baseline alignment stable and operators clearly separated.
Cover 0–9, +, −, =, <, >, ×, ÷, punctuation, required accented names and controller
icons in the asset inventory. Test 1/I/l and 0/O distinctions and 1/2, 2/4, 3/4,
5/8, 6/8 at real headset size. Preserve unreduced construction notation.

Font appearance never changes the meaning: one half and two quarters align to the
same endpoint; no decorative stretching of a single piece.

## 3. Color tokens

Draft sRGB palette; opaque UI backings provide predictable contrast over passthrough.

| Token | Hex | Role |
|---|---|---|
| color.panel | #18233B | Main navy panel |
| color.surface | #25344D | Nested card / raised surface |
| color.text | #F5F7FB | Main text |
| color.muted | #BAC6D8 | Secondary text, still readable |
| color.primary | #4AD7BC | Primary action / focus accent |
| color.cargo | #FFB86B | Strap/cargo warmth |

Calculated opaque pairs: text on panel ≈14.59:1, muted on panel ≈9.06:1,
panel text on primary ≈8.74:1, panel text on cargo ≈9.18:1.
Recalculate final materials/colors; these calculated ratios are a
starting check, not a headset measurement. Alpha blending and
headset rendering change results, so test actual display conditions.
Use 4.5:1 as a conservative project text-contrast review target, not a claim that
the headset experience is certified accessible.

Use dark text on filled teal/orange buttons, not automatically white.
Do not use red/green or color alone to signify correctness. Error/repair needs
plain-language feedback and geometry, not a flashing red panel. Selection, hover
and success are different states. No correct-answer color cue before Submit.

## 4. Spatial composition

- Stationary, world-locked workbench; the room stays visible. No head-locked menu
  constantly following the eyes and no camera movement/shake.
- Ray-operated instruction panel starts roughly 1 m away, slightly below eye
  level; movable objects stay in the player's adjustable comfortable reach.
  Meta's general placement guidance is a starting point, not permission to put
  directly grabbed pieces beyond a child's reach.
- Center active math work; supply trays flank their own lane; reference whole
  and 0–1 ruler stay visible. Guide captions sit beside/above the workspace, not
  over the fingers or active pieces.
- Keep no more than one primary instruction and one primary action visible.
  Help, mute and Back are stable secondary controls.
- Test seated/standing and left/right controllers. Explicit adjustment/recenter
  only with released pieces. Do not assume physical desk/floor alignment: without
  room sensing, this is a virtual station, not a surface safe to lean on.
- Panels have a calm opaque backing and subtle edge; avoid large opaque walls
  that hide the room. Floating math labels may yaw gently toward the viewer with
  limits; never spin the learning pieces to follow gaze.
- Do not add adaptive scaling to individual fraction geometry. Any view zoom
  scales the whole/reference/ruler together as defined in systemsdesign.md.
- Drop recovery returns accessible material; never require crawling or reaching
  beyond the safe space. Pause interactions when passthrough/tracking is unavailable.

[Meta MR design guidance](https://developers.meta.com/horizon/design/mr-design-guideline/)
supports spatial grounding, early headset iteration, comfortable placement and
limited head-locked UI. These rules adapt it to our controller-only stationary game.

## 5. Components and states

| Component | Required states / behavior |
|---|---|
| Lesson card | default, focused, pressed, disabled; Coming soon explicitly labeled |
| Primary button | large target, distinct hover/press, descriptive verb, focus feedback |
| Fraction piece | supply, hovered, held, docked, placed, split/merge preview; value always clear |
| Measuring lane | neutral valid placement before Submit; whole scale always visible |
| Split/join control | explains required release/dock; optional chord never sole method |
| Caption panel | speaking, paused, fallback; readable without sound |
| Guide status | AI guidance / offline guidance clearly distinguishable without alarming child |
| Help/replay | stable location, repeat current help; not an unexpected whole-lesson restart |
| Completion | lists actual jobs completed; replay/back; no mastery/ability label |

Initial ray button target trial: at least 4 cm high at a 1 m panel distance, with
clear gaps. That is a starting interaction target, not a universal minimum.
Collider/hit area may exceed decorative shape without overlapping adjacent targets.
Direct-grab shapes need generous handles, especially eighth pieces. Do not distort
piece length to make it grabbable; enlarge the interaction handle or selection zone.

Reuse Meta controller/ray/grab components and their input affordances. Don't mix
a “touch-looking” widget with ray-only behavior without a visible explanation.

## 6. Shape, material and icon language

Rounded rectangles, gently beveled manipulatives, matte navy/teal/orange materials.
Keep a clear unlit/readable UI layer; lighting can add depth to parcels, never
erase fraction labels. Subtle grounded contact shading is enough. No bloom around
numbers, excessive transparency layers or realistic metal straps that confuse size.

Use one simple icon family, with text labels for unfamiliar actions. Controller
glyphs must match left versus right and the actual grip/trigger/button mapping.
No decorative icons masquerading as buttons. Do not recolor the entire model
to signal approval; use the result text/equation after Submit.

## 7. Motion and tactile feedback

Motion explains causality: equal split separates visibly; merge closes a seam;
labels update after atomic commit. Show the unchanged reference while parts move.
Provisional durations: focus 120–180 ms, panel reveal 180–250 ms, split/join
350–550 ms. These are authored tuning defaults, not performance claims.
Never move the camera or animate a held piece away from the controller.
No looping pulsing scene, confetti, shake, rapid flashes or forced zoom.

Haptics are brief, optional confirmation of contact/action, with equivalent visual
feedback. Independent wrong-but-valid placement must feel the same as a correct
quantity until Submit. Provide reduced-motion and haptics-off modes; decorative
motion can stop without suppressing essential state-change explanation.

## 8. AI voice, captions and sound

The AI guide is core per AI-VOICE-GUIDE.md. Calm, clear and encouraging, one concrete
step at a time; describe actions and quantities, not inferred emotions or ability.
State the role as AI guidance, not a human teacher. No dependency-building persona,
shaming, countdown or pressure. Let the learner act without nonstop commentary.

Speech, captions and pointers refer to the same approved fact/action and current
state. Do not say “two halves” before the split commits. Replay current instruction,
mute without losing interaction, and label offline fallback honestly.
Optional music is quiet, separately controlled, rights-cleared and subordinate to
speech; never mandatory to recognize a result. Do not mask spoken consonants with
button sounds or stereo tricks. Voice input is separately gated, not required.

## 9. Unity implementation contract

All paths below are future deliverables, not files already created:
- AirliftStyle.asset and AirliftStyle.cs hold font/color/spacing/motion tokens.
- TypographyBindings applies semantic roles to TMP components.
- FractionNotationView owns stacked math layout independently from prose. P1-02
  creates it with static specimens; P1-04/P1-08 reuse it for model-driven labels.
- Shared UI prefabs bind to the style asset; no per-scene arbitrary colors/font search.
- Explicit font assets and weight/material references; no silently first-found fallback.

Use TMP SDF font assets suited to scaled spatial text. Inspect installed Unity UI
package documentation for exact supported settings; do not assume a web font or
variable-font file imports correctly. Prefer verified static weights for the chosen
family. Prebuild required glyphs; define a licensed fallback chain and verify
missing glyphs in the actual Android build. Do not rely on editor font caches.

Reuse material presets, cache component references, update labels on state changes,
pool repetitive pointers/pieces and avoid per-frame allocation or atlas growth.
Test shader/material stereo rendering and both-eye depth; don't fix clipping by
making every label render through every object. Preserve Unity .meta references.
Current target remains 72 Hz on Quest 3S with p95 CPU/GPU each ≤13.9 ms; 90 Hz is
a possible later measured target, not a new requirement silently imposed by a skill.
Style changes must pass the existing performance and safety gates.

Installed primary reference inspected:
unity/Library/PackageCache/com.unity.ugui@23caec89ae27/Documentation~/TextMeshPro/
FontAssets.md and FontAssetsFallback.md. These document SDF atlases and glyph
fallback behavior; cache files are not copied into the repository.

## 10. Required review and acceptance

P1-02 prepares a small comparison scene/contact sheet using actual font assets:
lesson catalog, one instruction/caption, primary/disabled buttons, fractions and
comparison equation. Review A versus B first; the selected direction is then
applied to CargoCrew. No automatic global font replacement in reference scenes.

Named checks: TypographyAssetTests, FractionGlyphCoverageTests, TextLayoutTests,
StyleBindingsResolve, DisabledCardStillReadable, CaptionsDoNotOccludeWork,
NoPerFrameAtlasGrowth and StyleResetDoesNotChangeMath.

On Quest: owner reads at normal viewing distance without leaning, points with
either controller, sees all fractions and captions, changes stance, tests bright/
dim room surroundings, and checks both eyes for clipping/shimmer. Capture actual
device evidence. Desktop prettiness alone does not pass.

Every future ticket: use shared tokens/components, include relevant style checks,
record any deviation and owner approval. P1-10/P2-08 verify stage integration;
P3-03 accessibility, P3-04 performance and P3-05 font/asset rights remain explicit.

## 11. Source and authority notes

The user's 3D Interaction Design skill informs input, layout and feedback; Game
Developer/Unity patterns inform shared assets, state-driven views and performance.
Neither is evidence of measured accessibility or learning efficacy.
[Meta panels](https://developers.meta.com/horizon/design/panels/) supplies component
guidance; adopting a new UI kit is not required or authorized.
Font sources above are references, not evidence of already downloaded assets.

This guide is a draft design reference. Owner approves direction before implementation.
Keep a short dated change log here whenever selected fonts/tokens change.
