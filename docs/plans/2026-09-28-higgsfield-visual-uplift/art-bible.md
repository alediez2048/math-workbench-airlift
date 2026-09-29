# Nerdy art bible (Higgsfield)

## Anchors (append to every prompt)
STYLE: miniature toy diorama, rounded satin-plastic pieces like a premium Hasbro/Lego playset,
soft warm key light from upper left, gentle contact shadows, shallow depth of field, clean
simple silhouettes, matte finish. Palette: orange #FFA84E, mint #58DEBE, yellow #FFD65C,
slate #3A4E76, cream #FFF4E0, background navy #202344.
NEGATIVE: no text, no letters, no numbers, no logos, no people's faces, no neon, no chrome,
no glass reflections, no photoreal grime, no clutter.

## Reference images
- Dock 7: docs/media/09-dock7-halves.jpg, docs/media/10-dock7-quarters.jpg
- Café: docs/media/11-cafe-plates.jpg
- Garden: docs/media/13-garden-sunflowers.jpg
- Lounge: docs/media/05-nerdy-lounge.jpg

## Families
| Id | Output | Aspect at generation | Final |
|---|---|---|---|
| SOON-<id> | coming-soon tile | 16:9, subject centred in the middle band | 584×166 |
| CH-<lessonId>-<n> | chapter tile, current render as reference | 16:9 | 584×166 |
| SKY-BASE | lounge sky | 2:1 equirectangular | 2048×1024 |
| DEE-SHEET | Dee turnaround + 6 expressions | 1:1 | canvas only |
| DETAIL-<name> | tileable surface | 1:1, "seamless tileable texture, top-down, flat light" | 512×512 |

## Prompts
TILE-BASE: "A small {SUBJECT} scene on a rounded navy tabletop, seen from a slightly high
three-quarter angle, the main action in a wide horizontal band across the middle. {STYLE}"

SOON-soon_route_9: SUBJECT = "toy city bus at a bus stop with little round rider tokens getting on"
SOON-soon_ten_and_trade: SUBJECT = "toy workshop shelf with bolts snapping into rods and rods filling a crate"
SOON-soon_corner_store: SUBJECT = "toy corner-store counter with a stack of oversized round coins and a small cash drawer"
SOON-soon_platform_clock: SUBJECT = "toy train platform with a big round station clock and a small train waiting"
SOON-soon_tailors_bench: SUBJECT = "toy tailor's bench with a roll of cloth, a measuring tape laid flat and big scissors"
SOON-soon_mile_marker: SUBJECT = "toy road with a small fuel truck passing a blank roadside marker post, gentle hills"

CH-<lessonId>-<n>: reference image = the current Assets/Airlift/Art/Tiles/<lessonId>-<n>.png
(upscaled source in artifacts/ if present). SUBJECT = "the same objects and arrangement as the
reference, redrawn as an illustrated toy diorama". Keep counts and arrangement identical:
the tile promises what the chapter shows.

SKY-BASE: "360 degree equirectangular panorama, seamless left-right wrap, horizon exactly at
the vertical middle, a calm stylised toy city and harbour skyline far away at dusk, rounded
low-poly buildings, soft cyan glow at the horizon rising to indigo and deep navy overhead,
a few soft clouds, no sun disc. {STYLE}"

DEE-SHEET: "Original friendly mascot 'Dee', a small round floating helper made of soft
satin plastic, a cyan core with a gentle glowing face made of two dots and a smile, tiny
rounded antenna, no arms or simple nub arms; character turnaround front/side/back plus six
expressions (listening, speaking, thinking, pleased, pointing, waving) on plain navy. {STYLE}"

DETAIL-<name>: "seamless tileable {MATERIAL} texture, top-down, flat even light, low
contrast, large soft shapes, toy-like, no text". MATERIAL per Task 9.

## Approved direction
A — satin-plastic toy diorama with the anchors as written (probe 2026-09-28, artifacts/higgsfield/probe/A.png).
Chosen by Claude under the owner's "implement end to end" instruction; shown to the owner on the review page for confirmation.
Model: gpt_image_2_5, variant flare, quality medium, 1k, 16:9 for tiles.
Note: simple pictograms (a bus-stop sign icon) appear; allowed only if they contain no letters or digits.

## Dee (Task 8, awaiting owner pick)
Three directions, each a 4-view turnaround + 6 expressions (listening, speaking, thinking, pleased, pointing, waving), gpt_image_2_5 2k:
- A — cyan satin orb with a navy face screen, antenna bead, nub arms, glow ring (`artifacts/higgsfield/dee/dee-A-orb.png`). Closest to today's cyan circle on the bar.
- B — cream teacup-size robot with an orange belly and mint arms (`dee-B-robot.png`). Most "toy".
- C — cyan-to-indigo drop with a yellow sprout curl and rosy cheeks (`dee-C-drop.png`). Softest, most child-friendly.
Next after the pick: train a Soul Character from the chosen sheet, record its id here, then the placement options (welcome-card portrait; sprite beside the tour's step counter; site and store art).
