# Higgsfield art uplift — QA record (2026-09-28)

Plan: [plan.md](../plans/2026-09-28-higgsfield-visual-uplift/plan.md). States are labelled verified, owner-reported or pending.

## Baseline
- Performance baseline: **not captured** (owner not in the headset during the automated run). Task 7 compares against the 13.9 ms budget only.

## Automated checks
- Task 2 (verified): `verify-cargo.sh --suite ArtImportBudgetTests,DashboardWiringTests` → OnboardingFlowTests 12, ArtImportBudgetTests 2, DashboardWiringTests 7, all pass. `ApplyArtImportSettings` → "15 of 15 art textures updated".
- Task 4 (verified): `ApplyComingSoonArt` → "6 of 6 coming-soon tiles have art"; suites OnboardingFlow 12, ArtImportBudget 2, GeneratedArt 1, Dashboard 8, DashboardWiring 7 pass. Scene diff: 6 new Photo objects only. Render: `artifacts/lounge/welcome-wall-p3.png` (owner look pending).
- Task 6 (verified): `ApplyLoungePanorama` → sky uses `Art/Generated/Lounge/LoungePanorama.png` (sky-3, seam 8.27 → 0.00), panes are NerdyStyle-cyan glass at 12 %. Suites OnboardingFlow 12, ArtImportBudget 2, GeneratedArt 3, LoungeRoom 8, LoungeWiring 12 pass. Renders `artifacts/lounge/lounge-{1..4}-*.png` show the city through the windows. Note for the headset: 2048 px across 360° is ~5.7 px/degree, soft up close; raise to 4096 only if Task 7's perf allows (pending).
- Task 5 (verified): 10 Café/Garden chapter tiles replaced in place (counts checked on zoomed full images: cafe 6/12/12/15/12 pieces with 2/3/5/5/4 plates-or-boxes; garden beds 3 rows, 4 rows, 3×4, 7×6, 8×7). Cargo Crew tiles generated and faithful but **not imported** (lock exception pending). `ApplyArtImportSettings` → 0 of 22 (overrides survive content replace). Suites OnboardingFlow 12, ArtImportBudget 2, DashboardWiring 7 pass. Render `artifacts/lounge/welcome-wall.png`.

## Headset and performance (Task 7)
- **Pending owner.** Build/install needs owner approval of the art first (CLAUDE.md rule); performance runs need the owner in the headset. Procedure: plan Task 7 steps 2–5.

## Optional detail textures (Task 9)
- Not started: gated on Task 7 performance headroom.
- Final review (fresh reviewer): fixed the panorama seam blend (mirror ghost → neighbour crossfade, seam 0.40), extended the coming-soon test (badge, no arrows; proved by a temporary break), made ApplyLoungePanorama set the transparent tag and passes itself. After fixes: OnboardingFlow 12, ArtImportBudget 2, GeneratedArt 4, LoungeRoom 8, LoungeWiring 12, Dashboard 8, DashboardWiring 7, art_tools + verify_cargo 13 — all pass (verified).

## Phase 1 — Dee (owner picked B · Robot)
- Verified: Dee face on the assistant bar Orb (`ApplyDeeFace`, run after `CompactAssistantBar`); Orb keeps its state tint and pulse as a halo. Suites ArtImportBudget 3, GeneratedArt 5, LoungeWiring 13, DashboardWiring 7 pass.
- Site: Dee avatar on the conversation card; Vercel preview (login-protected) https://nerdy-vr-landing-9exdgi3te-alediez2048s-projects.vercel.app. Production not deployed.
- Pending: headset build (another session's unreviewed onboarding changes share the tree), Cargo Crew tiles (explicit lock exception).

## Phase 2 — detail textures (Task 9)
- Verified: soil (GardenSoil, GardenPeat), wood (GardenWood, LoungeWood), rug (LoungeRug, LoungeRugInner), fabric (LoungeCouch); 512 px grey maps from `art_tools.detail_map`, ASTC 6x6. GeneratedArt 6, ArtImportBudget 3, Lounge 8+13, Garden 10+26+26+6 pass. Renders show faint grain/weave, no stretching. Perf impact to be measured in Task 7.

## Phase 3 — site video loop
- Verified locally (page 200, video 200 video/mp4): `site/dist/assets/coming-soon-loop.mp4`, 5 s Kling 3.0 loop from the approved illustration (start = end frame), muted, 213 KB; paused for prefers-reduced-motion. Vercel preview https://nerdy-vr-landing-171fcsoxm-alediez2048s-projects.vercel.app; production not deployed.

## Headset build from the branch (2026-09-28 17:43)
- Verified: branch-only APK (`design/higgsfield-uplift` @ 7054c9a; other sessions' onboarding work stashed during the build, scene file unchanged before/after, editor loaded the branch scene with no skip-onboarding row) installed on Quest 3S 3487C10J3706GW as `com.nerdy.vr.lounge`, lastUpdateTime 17:43:43; startup proof `[Nerdy] onboarding on …` at 17:43:51. Stash restored byte-for-byte afterwards.
- Pending: owner review on the headset; three post-warm-up performance runs. Voice cannot mint (Vercel proxy still holds the old OpenAI key).
