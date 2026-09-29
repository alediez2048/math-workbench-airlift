# Higgsfield generation log
Cap: 300 images (Tasks 1–9), 10 videos (Task 10). Account: Lite plan, 743 credits at start (2026-09-28).
Default model: gpt_image_2_5 (flare, quality medium, 1k) = 0.5 credits per image.

| Date | Tool / model | Prompt id | Credits | Output (artifacts/higgsfield/…) | Verdict |
|---|---|---|---|---|---|
| 2026-09-28 | gpt_image_2_5 medium 1k | probe A (anchors) | 0.5 | probe/A.png | picked: matches the in-app satin-plastic toy look |
| 2026-09-28 | gpt_image_2_5 medium 1k | probe B (flat vector) | 0.5 | probe/B.png | not picked: flatter than the 3D props |
| 2026-09-28 | gpt_image_2_5 medium 1k | probe C (clay) | 0.5 | probe/C.png | not picked: clay texture departs from satin plastic |
| 2026-09-28 | gpt_image_2_5 medium 1k | SOON-soon_route_9 ×2 | 1.0 | soon/soon_route_9-{1,2}.png | picked 2 |
| 2026-09-28 | gpt_image_2_5 medium 1k | SOON-soon_ten_and_trade ×2 | 1.0 | soon/soon_ten_and_trade-{1,2}.png | picked 1 |
| 2026-09-28 | gpt_image_2_5 medium 1k | SOON-soon_platform_clock ×2 | 1.0 | soon/soon_platform_clock-{1,2}.png | picked 1 (focus 0.2) |
| 2026-09-28 | gpt_image_2_5 medium 1k | SOON-soon_tailors_bench ×2 | 1.0 | soon/soon_tailors_bench-{1,2}.png | picked 1 |
| 2026-09-28 | gpt_image_2_5 medium 1k | SOON-soon_mile_marker ×2 | 1.0 | soon/soon_mile_marker-{1,2}.png | picked 1 |
| 2026-09-28 | gpt_image_2_5 medium 1k | SOON-soon_corner_store ×1 (1 rate-limited, not charged) | 0.5 | soon/soon_corner_store-2.png | picked (focus 0.35) |
| 2026-09-28 | gpt_image_2_5 medium 1k + reference crop | CH-<lesson>-<n> ×15 (1 each) | 7.5 | chapters/<lessonId>-<n>-1.png | 13 faithful; cafe-4 (12 muffins, needs 15) and garden-4 (6 rows, needs 7) rejected; garden-5 rechecked |
| 2026-09-28 | gpt_image_2_5 medium 2k 21:9 | SKY-BASE variants ×4 | 4.0 | sky/sky-{1..4}.png | picked 3 (city + airport + cargo plane, horizon at middle); seam 8.27 → 0.00 after make_seamless 96 |
| 2026-09-28 | gpt_image_2_5 high 1k + reference crop | CH cafe-4, garden-4, garden-5 reruns ×2 each | ~6 | chapters/*-{4,5}-{2,3}.png | picked cafe-4-2 (3×5=15), garden-4-3 (7×6), garden-5-2 (8×7); others miscounted |
| 2026-09-28 | gpt_image_2_5 medium 2k | DEE-SHEET A/B/C ×1 each | 3.0 | dee/dee-{A-orb,B-robot,C-drop}.png | awaiting owner pick |
| 2026-09-28 | gpt_image_2_5 high 2k | site illustration + OG source | ~4 | site/illustration-coming-soon.png, site/og-source.png | illustration used (labelled); OG not used |

**Totals (2026-09-28):** 51 generations submitted (1 rate-limited, not charged). Balance 743 → 707 credits (36 spent). Cap: 300 images, 0 video used.
| 2026-09-28 | gpt_image_2_5 high 1k transparent + dee-B reference | Dee expressions ×6 + icon | ~7 | dee/dee-{listening,speaking,thinking,pleased,pointing,waving,icon}.png | all on-model; owner picked B |
| 2026-09-28 | gpt_image_2_5 medium 1k | DETAIL soil/wood/rug/fabric ×1 | 2.0 | detail/*-src.png | processed by art_tools.detail_map |
| 2026-09-28 | kling3_0 std 5 s, sound off, start=end frame | coming-soon loop | 6.25 | site/coming-soon-loop-src.mp4 | used on the site (video 1 of 10) |
| 2026-09-28 | gpt_image_2_5 high 2k 21:9 | studio reflection environment ×2 | 2.0 | env/studio-{1,2}.png | picked 1 (cool lavender softboxes) |
| 2026-09-28 | gpt_image_2_5 high 2k | deck surfaces dock/cafe/garden ×1 | ~3 | deck/*-src.png | |
| 2026-09-28 | gpt_image_2_5 high 2k 21:9 | lesson card headers dock/cafe/garden | ~3 | header/*.png | |
| 2026-09-28 | gpt_image_2_5 high 1k transparent | Dee idle pose | 1 | dee/dee-idle.png | used |
| 2026-09-28 | gpt_image_2_5 medium 1k | ROOM wall (fluted acoustic panel) + floor (grey herringbone) ×1 each | 1.0 | room/{wall,floor}-src.png | both used; art_tools.detail_map (wall 0.84±0.16, floor 0.62±0.38), token-tinted in ApplyRoomFinish |
| 2026-09-28 | (reuse, no credits) | Dee pleased pose from the expression set | 0 | dee/dee-pleased.png | used for the solve celebration (PIL 512 px) |
