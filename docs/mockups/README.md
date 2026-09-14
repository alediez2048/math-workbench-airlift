# Airlift visual reference

Draft v0.1 · September 14, 2026.

Open [index.html](index.html). No install, server, network requests, external
fonts, or game runtime are required. This is a standalone interactive design
reference, **not a WebXR port, Unity scene, headset screenshot, or tested game**.

## Five reference states

1. **Arrival:** quiet real-room backdrop, world-locked table, cargo context, one whole.
2. **Build:** three fourths assembled on a same-length ruler; add/remove quarters to inspect wrong valid lengths.
3. **Equivalence:** 3/4 and 6/8 at the same endpoint; edit independent number/operator fields and Submit.
4. **Compare & repair:** choose an inequality and explicit reason; Submit reveals authored support after a wrong attempt. No live model is called.
5. **Departure:** cargo payoff and replay, with no score, lives, or unsupported mastery claim.

Reviewer state navigation, annotations, and browser form controls are outside the
depicted product. Browser selectors stand in for ray-operated fields in Unity.
Each view is independently inspectable; the mockup is not a complete lesson flow.
T06's unprompted assessment state is specified in the PRD, not rendered here.

## Visual decisions

- Warm off-white work surface, deep ink labels, teal strap material, amber task
  emphasis. Color never carries quantity alone: text and partition seams do.
- Stylized translucent room context communicates passthrough without collecting
  a camera image. Do not build the illustrated room as a required virtual hangar.
- Miniature aircraft/cargo sit beyond the working area; essential math stays at
  the center. Decorative objects do not become required reaching targets.
- Equal-width lane rectangles share zero and one. A quarter occupies twice the
  width of an eighth. No perspective distortion is applied to the mathematical
  diagram; the surrounding table is schematic perspective.
- The initial whole length in Unity is 0.4 m, eighth segments 0.05 m. Enlarge grab
  handles independently of represented length. Set bench height/reach in-headset;
  do not convert CSS pixels directly into meters.
- Product copy describes the task and supports repair. Correctness feedback
  occurs only after Submit; neutral placement remains possible.

The 3D interaction-design skill informed constrained placement, separation of
grab and ray-control areas, one-controller use, stable scenery, and multimodal
feedback. Its generic heuristics do not override physical validation or our plan.

## Interaction checks

- Build: add/remove a quarter; Submit an incorrect length, then three fourths.
- Equivalence: change proposed denominator to 4; Submit; restore 6/8 and Submit.
- Comparison: Submit 3/4 < 5/8 with “More parts means more length.” Then switch
  to the benchmark reason and select “Use half benchmark” before Submit to see
  the different repair view. Both supports are authored local fixtures.
- Comparison: “Not sure yet” uses common-endpoint neutral support. Correctness
  requires `>` and an appropriate selected explanation, not just the operator.
- Reset clears the active mockup's edits. State buttons are keyboard accessible.

Generated PNGs capture these mockup states and are labeled design references.
They are not suitable as evidence that the hackathon product functions.

## Captured references

[Arrival](01-arrival.png) · [Build](02-build.png) ·
[Equivalence](03-equivalence.png) · [Equal-parts repair](04-compare.png) ·
[Departure](05-departure.png) · [Benchmark repair](06-benchmark-repair.png) ·
[Mobile review layout](07-mobile-review.png).

Best inspected on a desktop: on a phone the scene is an overview and the notes/
controls reflow underneath. It is not a mobile game interface or a legibility test
for the headset. See [verification.md](verification.md) for the limited checks.
