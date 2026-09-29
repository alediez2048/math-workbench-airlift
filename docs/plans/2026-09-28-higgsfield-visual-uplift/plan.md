# Higgsfield Visual Uplift Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Use the Higgsfield MCP to replace Nerdy's placeholder and screenshot art with a single illustrated toy-diorama art set: 24 wall tiles, a painted lounge sky, Dee as a character, optional detail textures and site media. The 3D toy props, the lesson mechanics and the Quest frame budget stay as they are.

**Architecture:** Higgsfield generates 2D images only. Each image passes through a local fitting script (crop, resize, seam check), then an owner approval on the canvas, before it lands under `Assets/Airlift/Art/`. Small, idempotent AgentScripts wire the approved art into the existing scene. They never rerun the old builders. An EditMode suite pins the import budget (Android ASTC, size caps) and the wiring. Each phase ends with a preview render, and the whole set ends with a headset and performance check.

**Tech Stack:** Higgsfield MCP (`https://mcp.higgsfield.ai/mcp`) or Higgsfield CLI; Python 3 + Pillow (already used by `scripts/make_card_sprites.py`); Unity 6000.6.0f1 / URP 17.6, Unity CLI `run_script`; NUnit EditMode tests through `scripts/verify-cargo.sh`; Vercel for `site/`.

**Spec:** The research reply of 2026-09-28 in this session, summarised in the "Research summary" section below. There is no separate spec file; this section is the spec.

## Research summary (the spec)

- **The Higgsfield MCP** is a hosted MCP with 30+ image and video models. Images go up to 4K; GPT Image 2 is the model noted for readable text. Soul Character keeps one character consistent across generations. Video clips run up to 15 s. Everything is billed in plan credits: a few credits per image, tens to hundreds per video. The `model` field is a hint and the router may override it.
- **3D (GLB, PBR, rigging)** is reached through the Blender add-on and its Bridge MCP. It is **out of scope** here: generated meshes default to about 30k triangles, the props are the app's strongest visual, Cargo Crew is locked, and the licence line says "no third-party art".
- **The app today:** there are no textures or imported models. Of the 24 wall tiles, 18 reuse 15 cropped editor screenshots (`Assets/Airlift/Art/Tiles/<lessonId>-<chapter>.png`, 584×166), and the 6 coming-soon tiles have no art. The lounge sky is an 8×256 gradient (`Materials/Lounge/LoungeSkyGradient.png` on `LoungeSky.mat`, `Skybox/Panoramic`, latitude-longitude mapping). The window panes (`LoungeSkyPane.mat`) show that same texture. Texture imports have no Android override. The performance gate (L-5) is still todo.
- **Value ranking:** (1) tile art, (2) Dee character, (3) lounge panorama, (4) subtle detail textures, (5) site media. Do not use generated 3D.

## Global Constraints

- Canvas approval first: every generated image is shown on the canvas (https://claude.ai/artifact/JPTeoWGosWPJqyP2JNACgf) and approved by the owner before it enters `unity/Assets/` or `site/`.
- Short Unity turns: one script, one render, one report. Before any Unity CLI call run `export UNITY_PROJECT_PATH=/Users/jad/Desktop/math-workbench-airlift/unity`. Run `unity command editor_stop` if the editor is in Play mode.
- Never rerun `CreateNerdyWelcome.cs`, `CreateFractionChapter.cs`, `ApplyCargoStyle.cs`, `ApplyNerdyStyle.cs`, `PatchCatalogCards.cs`, `RemoveOrb.cs` or the old builder chain. Rerunning `BuildDashboard.cs` or `BuildLounge.cs` is also avoided: the new AgentScripts patch on top of their output.
- Cargo Crew lesson visuals (Dock 7 props, materials, lesson HUD) are **not touched**. The Cargo Crew *catalog tiles* change only after an explicit owner exception, which Task 5 asks for.
- Generated images contain **no text, letters, digits or logos**. Titles and badges stay TMP text. This also avoids AI gibberish lettering.
- Palette, copied from `docs/00-build/STYLE-GUIDE.md` §1 and `NerdyStyle.asset`: orange `#FFA84E`, mint `#58DEBE`, yellow `#FFD65C`, slate `#3A4E76`, cream `#FFF4E0`, navy base `#202344`. Look: rounded satin-plastic toy diorama ("Hasbro, Lego"), warm key light, soft shadows, simple silhouettes. No neon, chrome, heavy gradients or glass.
- Texture budget on Quest: tiles ≤ 1024 px with ASTC 4×4; the lounge panorama ≤ 2048 px with ASTC 6×6; detail textures ≤ 512 px with ASTC 6×6. The frame budget is 72 Hz with CPU and GPU p95 ≤ 13.9 ms (`requirements.md` F-16).
- Honesty on the site: generated art is always labelled as illustration and never presented as a headset capture or Unity render.
- People: no real-person likeness. Dee is an original character. The audience is children, so no mature, scary or branded content.
- Credits: a hard cap of **300 image generations and 0 video** for Tasks 1–9; Task 10 may spend up to 10 video generations. Log every generation in `docs/plans/2026-09-28-higgsfield-visual-uplift/generation-log.md`.
- Secrets: the Higgsfield login stays in the MCP/CLI auth store. It never goes in the repo, the APK or the docs.
- Git: commit and push only when the owner asks. The "Checkpoint" steps below stage files and write the message, and commit only on the owner's word. Before any commit run `detect_changes()`, as CLAUDE.md requires.
- Stale-build proof on device: `[Nerdy] onboarding …` appears in logcat.

## Review Focus

1. **A builder rerun wipes the art.** If someone reruns `BuildDashboard.cs` or `BuildLounge.cs`, the coming-soon photos and the panorama wiring disappear silently. Expected: the `GeneratedArtTests` suite fails and names the script to rerun. (Tests in Tasks 4 and 6.)
2. **Coming-soon tiles look playable once they have art.** Expected: the art is visibly dimmer than playable art (alpha ≤ 0.6), the COMING SOON badge stays, there is no hover and no arrow. (Test in Task 4.)
3. **A panorama with a seam or stretched poles.** Generated 2:1 images rarely wrap. Expected: the left and right edges match within tolerance before import, and the horizon sits in the middle band. (Tests in Task 3, check in Task 6.)
4. **A generated image with transparency or the wrong size reaches Unity.** Expected: the fitting script always outputs opaque RGB at the exact target size, whatever the input (RGBA, portrait, tiny). (Tests in Task 3.)
5. **Art imported without an Android override** ships at 2048 automatic format and costs memory. Expected: every texture under `Art/Tiles` and `Art/Generated` has an ASTC override within its cap. (Test in Task 2.)

---

## File structure

| Path | Responsibility |
|---|---|
| `docs/plans/2026-09-28-higgsfield-visual-uplift/art-bible.md` | Style anchors, the prompt kit per asset family, the reference images, and the approved direction |
| `docs/plans/2026-09-28-higgsfield-visual-uplift/generation-log.md` | Every generation: date, tool/model, prompt id, credits, output file, verdict |
| `scripts/art_tools.py` | Pure functions: `fit_cover`, `seam_error`, `make_seamless`, plus a CLI |
| `scripts/test_art_tools.py` | unittest for `art_tools.py` |
| `artifacts/higgsfield/<family>/` (ignored) | Raw downloads and candidates, never imported |
| `unity/AgentScripts/ApplyArtImportSettings.cs` | Sprite type + Android ASTC override + caps for `Art/Tiles` and `Art/Generated` |
| `unity/AgentScripts/ApplyComingSoonArt.cs` | Adds a dimmed `Art/Photo` to the six `Soon <id>` tiles |
| `unity/AgentScripts/ApplyLoungePanorama.cs` | Points `LoungeSky.mat` at the panorama and turns the panes into faint glass |
| `unity/AgentScripts/ApplyDetailTextures.cs` | (Task 9, optional) assigns 512 px detail maps to the named materials |
| `unity/Assets/Airlift/Art/Tiles/*.png` | 15 chapter PNGs replaced in place, plus 6 new `soon_*.png` |
| `unity/Assets/Airlift/Art/Generated/Lounge/LoungePanorama.png` | 2048×1024 equirectangular sky |
| `unity/Assets/Airlift/Art/Generated/Detail/*.png` | (Task 9) tileable detail maps |
| `unity/Assets/Airlift/Tests/EditMode/ArtImportBudgetTests.cs` | Import-budget test |
| `unity/Assets/Airlift/Tests/EditMode/GeneratedArtTests.cs` | Wiring tests for coming-soon art, panorama and detail maps |
| `scripts/cargo-milestones.json` | New ticket `CC-HF-01` listing both suites |
| `docs/00-build/RELEASE-GATES.md`, `CLAUDE.md`, `docs/qa/higgsfield-art-2026-09-28.md` | Licence row, builder order, QA evidence |

---

### Task 1: Connect Higgsfield, write the art bible, run a style probe

No Unity work. The deliverable is an approved visual direction on the canvas.

**Files:**
- Create: `docs/plans/2026-09-28-higgsfield-visual-uplift/art-bible.md`
- Create: `docs/plans/2026-09-28-higgsfield-visual-uplift/generation-log.md`

**Interfaces:**
- Produces: prompt ids `TILE-BASE`, `SOON-<id>`, `CH-<lessonId>-<n>`, `SKY-BASE`, `DEE-SHEET` and `DETAIL-<name>`, used by Tasks 4–10. The chosen probe (A, B or C) is recorded in `art-bible.md` under "Approved direction".

- [ ] **Step 1: Connect the MCP (after the owner has Higgsfield access)**

```bash
claude mcp add --transport http higgsfield https://mcp.higgsfield.ai/mcp
claude mcp list | grep higgsfield
```
Expected: `higgsfield: https://mcp.higgsfield.ai/mcp (HTTP) - ✓ Connected`. Sign-in happens in the browser. If the MCP will not connect, the fallback is `npm i -g @higgsfield/cli && higgsfield auth login`.

- [ ] **Step 2: Ask the owner to confirm the commercial-use terms for the plan tier and the models to be used, and record the answer.** The tiles ship in a public GitHub repo and a release APK.

- [ ] **Step 3: Write `art-bible.md`**

```markdown
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
```

- [ ] **Step 4: Write the header of `generation-log.md`**

```markdown
# Higgsfield generation log
Cap: 300 images (Tasks 1–9), 10 videos (Task 10). Running total at the bottom of the table.

| Date | Tool / model | Prompt id | Credits | Output (artifacts/higgsfield/…) | Verdict |
|---|---|---|---|---|---|
```

- [ ] **Step 5: Style probe.** Generate `SOON-soon_route_9` three ways: A with the anchors as written; B with "flat vector illustration" in place of "diorama"; C with "clay stop-motion" in place of "satin plastic". Save the results to `artifacts/higgsfield/probe/{A,B,C}.png` and log them.

- [ ] **Step 6: Canvas approval.** Add a "Higgsfield probe" section to the canvas artifact with the three images beside the current wall screenshot (`docs/media/04-wall-tour.jpg`). Ask the owner to pick A, B or C, or to redirect. Record the choice in `art-bible.md` under `## Approved direction`. **Stop until the owner answers.**

---

### Task 2: Import budget guard (Android ASTC + caps)

**Files:**
- Create: `unity/AgentScripts/ApplyArtImportSettings.cs`
- Create: `unity/Assets/Airlift/Tests/EditMode/ArtImportBudgetTests.cs`
- Modify: `scripts/cargo-milestones.json` (add ticket `CC-HF-01`)

**Interfaces:**
- Produces: `ApplyArtImportSettings.Run()`, which later tasks run after every art import. Caps by folder: `/Art/Tiles/` → 1024 + ASTC_4x4; `/Art/Generated/Lounge/` → 2048 + ASTC_6x6; `/Art/Generated/Detail/` → 512 + ASTC_6x6.

- [ ] **Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEditor;

namespace Airlift.Tests
{
    /// CC-HF-01. Generated and tile art ships compressed for Quest. Run AgentScripts/ApplyArtImportSettings.cs after any import.
    public class ArtImportBudgetTests
    {
        static readonly string[] Roots = { "Assets/Airlift/Art/Tiles", "Assets/Airlift/Art/Generated" };

        static (int max, TextureImporterFormat format) Budget(string path) =>
            path.Contains("/Art/Tiles/") ? (1024, TextureImporterFormat.ASTC_4x4)
            : path.Contains("/Generated/Lounge/") ? (2048, TextureImporterFormat.ASTC_6x6)
            : (512, TextureImporterFormat.ASTC_6x6);

        [Test] public void EveryArtTextureHasAnAndroidAstcOverrideWithinItsCap()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", System.Array.FindAll(Roots, AssetDatabase.IsValidFolder));
            Assert.That(guids, Is.Not.Empty, "the 15 chapter tiles at least");
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                var android = importer.GetPlatformTextureSettings("Android");
                var (max, format) = Budget(path);
                Assert.That(android.overridden, Is.True, path + ": run AgentScripts/ApplyArtImportSettings.cs");
                Assert.That(android.format, Is.EqualTo(format), path);
                Assert.That(android.maxTextureSize, Is.LessThanOrEqualTo(max), path);
            }
        }

        [Test] public void TileArtImportsAsOpaqueSprites()
        {
            foreach (var g in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Airlift/Art/Tiles" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), path);
                Assert.That(importer.alphaIsTransparency, Is.False, path);
            }
        }
    }
}
```

- [ ] **Step 2: Register the suite.** `verify-cargo.sh --suite` refuses any filter that is not in the manifest. `--phase` runs include every ticket key that sorts at or before the phase end, and `CC-HF` sorts before `CC-P`. So register only the suite that exists now. Task 4 adds `GeneratedArtTests`. In `scripts/cargo-milestones.json`, add under `"tickets"`:

```json
    "CC-HF-01": {
      "suites": [
        { "mode": "editor", "filter": "ArtImportBudgetTests" }
      ],
      "requiredAssets": [],
      "requiredCueIds": []
    },
```

- [ ] **Step 3: Run it and confirm it fails**

```bash
export UNITY_PROJECT_PATH=/Users/jad/Desktop/math-workbench-airlift/unity
unity command run_script --file AgentScripts/RefreshAndCompile.cs --entry RefreshAndCompile.Run
bash scripts/verify-cargo.sh --suite ArtImportBudgetTests
```
Expected: FAIL on the first tile, `…cargo_crew_fractions-1.png: run AgentScripts/ApplyArtImportSettings.cs`. The suite `GeneratedArtTests` does not exist yet, so run only this one suite.

- [ ] **Step 4: Write the script**

```csharp
using UnityEditor;
using UnityEngine;

// CC-HF-01. Android ASTC overrides and size caps for tile and generated art; tiles import as opaque sprites.
// Idempotent. Run after every art import.
// Run: unity command run_script --file AgentScripts/ApplyArtImportSettings.cs --entry ApplyArtImportSettings.Run
public static class ApplyArtImportSettings
{
    static readonly string[] Roots = { "Assets/Airlift/Art/Tiles", "Assets/Airlift/Art/Generated" };

    public static string Run()
    {
        int changed = 0, seen = 0;
        foreach (var g in AssetDatabase.FindAssets("t:Texture2D", System.Array.FindAll(Roots, AssetDatabase.IsValidFolder)))
        {
            string path = AssetDatabase.GUIDToAssetPath(g); seen++;
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            bool tile = path.Contains("/Art/Tiles/");
            int max = tile ? 1024 : path.Contains("/Generated/Lounge/") ? 2048 : 512;
            var format = tile ? TextureImporterFormat.ASTC_4x4 : TextureImporterFormat.ASTC_6x6;
            var android = importer.GetPlatformTextureSettings("Android");
            bool dirty = !android.overridden || android.maxTextureSize != max || android.format != format;
            if (tile && (importer.textureType != TextureImporterType.Sprite || importer.alphaIsTransparency)) dirty = true;
            if (!dirty) continue;
            if (tile) { importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = false; importer.mipmapEnabled = true; }
            android.overridden = true; android.maxTextureSize = max; android.format = format; android.compressionQuality = 50;
            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport(); changed++;
        }
        return changed + " of " + seen + " art textures updated";
    }
}
```

- [ ] **Step 5: Run the script, then the test**

```bash
unity command run_script --file AgentScripts/ApplyArtImportSettings.cs --entry ApplyArtImportSettings.Run
bash scripts/verify-cargo.sh --suite ArtImportBudgetTests
bash scripts/verify-cargo.sh --suite DashboardWiringTests
```
Expected: `15 of 15 art textures updated`, then both suites PASS. `DashboardWiringTests` proves the tiles still resolve under `Assets/Airlift/Art/Tiles/`.

- [ ] **Step 6: Performance baseline.** If the headset is available, build and install the *current* scene with `bash scripts/lounge-preview.sh` and record a L-5-style baseline: three post-warm-up runs of the wall and the lounge, CPU and GPU p95 from the OVR Metrics Tool, saved in `docs/qa/higgsfield-art-2026-09-28.md` under "Baseline". If the headset is not available, record "baseline not captured" and continue; Task 7 then compares against the 13.9 ms budget only.

- [ ] **Step 7: Checkpoint.** Stage `ApplyArtImportSettings.cs`, `ArtImportBudgetTests.cs`, the `.meta` files, the 15 changed `Art/Tiles/*.png.meta` files and `cargo-milestones.json`. Suggested message: `Compress tile art for Quest (ASTC overrides, CC-HF-01)`. Commit only if the owner asks.

---

### Task 3: Art fitting tools (crop, resize, seam)

**Files:**
- Create: `scripts/art_tools.py`
- Create: `scripts/test_art_tools.py`

**Interfaces:**
- Produces:
  - `fit_cover(src: str, dst: str, width: int, height: int, focus_y: float = 0.5) -> None` writes an opaque RGB PNG of exactly `width×height`.
  - `seam_error(path: str) -> float` gives the mean absolute RGB difference (0–255) between the leftmost and rightmost pixel columns.
  - `make_seamless(src: str, dst: str, band: int = 64) -> None` cross-fades the outer `band` px on each side so the image wraps.
  - CLI: `python3 scripts/art_tools.py fit SRC DST W H [FOCUS_Y]`, `python3 scripts/art_tools.py seam PATH`, `python3 scripts/art_tools.py seamless SRC DST [BAND]`.

- [ ] **Step 1: Write the failing tests**

```python
import os
import tempfile
import unittest

from PIL import Image

import art_tools


class FitCoverTests(unittest.TestCase):
    def setUp(self):
        self.dir = tempfile.mkdtemp()

    def path(self, name):
        return os.path.join(self.dir, name)

    def test_wide_target_from_square_rgba_is_exact_size_and_opaque(self):
        Image.new('RGBA', (1024, 1024), (255, 168, 78, 0)).save(self.path('src.png'))
        art_tools.fit_cover(self.path('src.png'), self.path('out.png'), 584, 166)
        out = Image.open(self.path('out.png'))
        self.assertEqual(out.size, (584, 166))
        self.assertEqual(out.mode, 'RGB')

    def test_portrait_and_tiny_sources_still_fill_the_frame(self):
        Image.new('RGB', (90, 300), (88, 222, 190)).save(self.path('tall.png'))
        art_tools.fit_cover(self.path('tall.png'), self.path('out.png'), 584, 166)
        self.assertEqual(Image.open(self.path('out.png')).size, (584, 166))

    def test_focus_y_picks_the_band(self):
        src = Image.new('RGB', (1600, 900), (0, 0, 0))
        src.paste((255, 255, 255), (0, 0, 1600, 300))   # white top third
        src.save(self.path('src.png'))
        art_tools.fit_cover(self.path('src.png'), self.path('top.png'), 584, 166, focus_y=0.0)
        art_tools.fit_cover(self.path('src.png'), self.path('bottom.png'), 584, 166, focus_y=1.0)
        self.assertGreater(Image.open(self.path('top.png')).getpixel((292, 10))[0], 200)
        self.assertLess(Image.open(self.path('bottom.png')).getpixel((292, 150))[0], 50)


class SeamTests(unittest.TestCase):
    def setUp(self):
        self.dir = tempfile.mkdtemp()

    def test_seam_error_is_zero_for_matching_edges_and_high_for_mismatch(self):
        same = os.path.join(self.dir, 'same.png')
        Image.new('RGB', (256, 128), (32, 35, 68)).save(same)
        self.assertEqual(art_tools.seam_error(same), 0.0)
        split = Image.new('RGB', (256, 128), (0, 0, 0))
        split.paste((255, 255, 255), (128, 0, 256, 128))
        path = os.path.join(self.dir, 'split.png')
        split.save(path)
        self.assertGreater(art_tools.seam_error(path), 200)

    def test_make_seamless_brings_the_seam_under_tolerance_and_keeps_size(self):
        split = Image.new('RGB', (512, 256), (0, 0, 0))
        split.paste((255, 255, 255), (256, 0, 512, 256))
        src = os.path.join(self.dir, 'split.png')
        dst = os.path.join(self.dir, 'wrapped.png')
        split.save(src)
        art_tools.make_seamless(src, dst, band=64)
        self.assertEqual(Image.open(dst).size, (512, 256))
        self.assertLess(art_tools.seam_error(dst), 8.0)


if __name__ == '__main__':
    unittest.main()
```

- [ ] **Step 2: Run them and confirm they fail**

Run: `cd scripts && python3 -m unittest test_art_tools -v`
Expected: `ModuleNotFoundError: No module named 'art_tools'`

- [ ] **Step 3: Implement**

```python
"""Fit Higgsfield outputs to Nerdy's art slots. Pure Pillow; no network."""
import sys

from PIL import Image


def fit_cover(src, dst, width, height, focus_y=0.5):
    """Scale to cover width x height, crop centred horizontally and at focus_y vertically, save opaque RGB."""
    img = Image.open(src).convert('RGBA')
    backdrop = Image.new('RGBA', img.size, (32, 35, 68, 255))   # navy base under any transparency
    img = Image.alpha_composite(backdrop, img).convert('RGB')
    scale = max(width / img.width, height / img.height)
    resized = img.resize((max(width, round(img.width * scale)), max(height, round(img.height * scale))), Image.LANCZOS)
    left = (resized.width - width) // 2
    top = round((resized.height - height) * min(1.0, max(0.0, focus_y)))
    resized.crop((left, top, left + width, top + height)).save(dst, 'PNG')


def seam_error(path):
    """Mean absolute RGB difference between the first and last pixel columns."""
    img = Image.open(path).convert('RGB')
    w, h = img.size
    total = 0
    for y in range(h):
        a, b = img.getpixel((0, y)), img.getpixel((w - 1, y))
        total += sum(abs(a[i] - b[i]) for i in range(3)) / 3
    return total / h


def make_seamless(src, dst, band=64):
    """Cross-fade each edge towards the opposite edge's band so the panorama wraps."""
    img = Image.open(src).convert('RGB')
    w, h = img.size
    out = img.copy()
    px, opx = img.load(), out.load()
    for x in range(band):
        t = 0.5 * (1 - x / band)        # 0.5 at the very edge, 0 at the band's inner end
        for y in range(h):
            left, right = px[x, y], px[w - 1 - x, y]
            opx[x, y] = tuple(round(left[i] * (1 - t) + right[i] * t) for i in range(3))
            opx[w - 1 - x, y] = tuple(round(right[i] * (1 - t) + left[i] * t) for i in range(3))
    out.save(dst, 'PNG')


if __name__ == '__main__':
    cmd, args = sys.argv[1], sys.argv[2:]
    if cmd == 'fit':
        fit_cover(args[0], args[1], int(args[2]), int(args[3]), float(args[4]) if len(args) > 4 else 0.5)
    elif cmd == 'seam':
        print('%.2f' % seam_error(args[0]))
    elif cmd == 'seamless':
        make_seamless(args[0], args[1], int(args[2]) if len(args) > 2 else 64)
    else:
        sys.exit('usage: art_tools.py fit|seam|seamless ...')
```

- [ ] **Step 4: Run the tests and confirm they pass**

Run: `cd scripts && python3 -m unittest test_art_tools -v`
Expected: 5 tests, OK. Also run `python3 -m unittest test_verify_cargo` to confirm nothing else in `scripts/` broke.

- [ ] **Step 5: Checkpoint.** Stage `scripts/art_tools.py` and `scripts/test_art_tools.py`. Suggested message: `Art fitting tools for generated tiles and panoramas`. Commit only on request.

---

### Task 4: Coming-soon tile art (6 worlds)

**Files:**
- Create: `unity/Assets/Airlift/Art/Tiles/soon_route_9.png`, `soon_ten_and_trade.png`, `soon_corner_store.png`, `soon_platform_clock.png`, `soon_tailors_bench.png`, `soon_mile_marker.png`
- Create: `unity/AgentScripts/ApplyComingSoonArt.cs`
- Create: `unity/Assets/Airlift/Tests/EditMode/GeneratedArtTests.cs`

**Interfaces:**
- Consumes: `ApplyArtImportSettings.Run()` (Task 2); `art_tools.py fit` (Task 3); `DashboardCatalog.ComingSoon[].Id` (`Airlift.Welcome`); `DashboardWall.TileSize` (`Airlift.Presentation.Dashboard`, 292×190); the scene objects `Soon <id>/Art` made by `BuildDashboard.cs`.
- Produces: `ApplyComingSoonArt.Run()`, which must run after any `BuildDashboard.cs` rerun. `GeneratedArtTests` is the suite later tasks append to.

- [ ] **Step 1: Generate.** For each of the six `SOON-*` prompts, generate four candidates in the approved direction at 16:9 and save them to `artifacts/higgsfield/soon/<id>-{1..4}.png`. Log all 24 generations.

- [ ] **Step 2: Fit every candidate** so the owner judges it at the size it will ship:

```bash
for f in artifacts/higgsfield/soon/*.png; do python3 scripts/art_tools.py fit "$f" "${f%.png}-tile.png" 584 166 0.5; done
```

- [ ] **Step 3: Canvas approval.** Add a "Coming soon art" section with each world's four tile-sized candidates at 50 % opacity on the navy tile, plus the world title. The owner picks one per world or asks for a rerun. Check each pick for stray letters or digits; reject any that has them. **Stop until approved.**

- [ ] **Step 4: Copy the picks into Unity**

```bash
cp artifacts/higgsfield/soon/soon_route_9-<pick>-tile.png unity/Assets/Airlift/Art/Tiles/soon_route_9.png
# … repeat for the other five ids with their picked numbers
unity command run_script --file AgentScripts/RefreshAndCompile.cs --entry RefreshAndCompile.Run
unity command run_script --file AgentScripts/ApplyArtImportSettings.cs --entry ApplyArtImportSettings.Run
```
Expected: `6 of 21 art textures updated`.

- [ ] **Step 5: Write the failing test**

```csharp
using System.Linq;
using Airlift.Presentation.Dashboard;
using Airlift.Welcome;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Airlift.Tests
{
    /// CC-HF-01, scene side. Run AgentScripts/ApplyComingSoonArt.cs (after any BuildDashboard.cs) and
    /// AgentScripts/ApplyLoungePanorama.cs (after any BuildLounge.cs) before this suite.
    public class GeneratedArtTests
    {
        const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
        Scene loaded;
        [SetUp] public void Open() { if (!SceneManager.GetSceneByPath(ScenePath).isLoaded) loaded = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive); }
        [TearDown] public void Close() { if (loaded.IsValid()) EditorSceneManager.CloseScene(loaded, true); }

        static DashboardWall Wall() => SceneManager.GetSceneByPath(ScenePath).GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<NerdyDirector>(true)).First().catalogRoot.GetComponent<DashboardWall>();

        [Test] public void ComingSoonTilesShowTheirWorldDimmedAndInert()
        {
            var soon = Wall().tiles.Where(t => !t.openable).ToList();
            Assert.That(soon.Count, Is.EqualTo(DashboardCatalog.ComingSoon.Length));
            foreach (var t in soon)
            {
                var photo = t.transform.Find("Art/Photo");
                Assert.That(photo, Is.Not.Null, t.id + ": run AgentScripts/ApplyComingSoonArt.cs");
                var img = photo.GetComponent<Image>();
                Assert.That(AssetDatabase.GetAssetPath(img.sprite), Is.EqualTo("Assets/Airlift/Art/Tiles/" + t.id + ".png"), t.id);
                Assert.That(img.color.a, Is.LessThanOrEqualTo(0.6f), t.id + " reads as not yet playable");
                Assert.That(img.raycastTarget, Is.False, t.id);
                Assert.That(t.GetComponent<TileHover>(), Is.Null, t.id + " stays inert");
            }
        }
    }
}
```

- [ ] **Step 6: Register the suite, then run it and confirm it fails.** Append `{ "mode": "editor", "filter": "GeneratedArtTests" }` to the `CC-HF-01` suites in `scripts/cargo-milestones.json`, then:

```bash
unity command run_script --file AgentScripts/RefreshAndCompile.cs --entry RefreshAndCompile.Run
bash scripts/verify-cargo.sh --suite GeneratedArtTests
```
Expected: FAIL with `soon_route_9: run AgentScripts/ApplyComingSoonArt.cs`.

- [ ] **Step 7: Write the script**

```csharp
using System;
using System.Linq;
using Airlift.Presentation.Dashboard;
using Airlift.Welcome;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// CC-HF-01. The six coming-soon tiles show an illustration of their world, dimmed like the rest of the tile so
// they never read as playable. Run after BuildDashboard (which recreates the Soon tiles without art). Idempotent.
// Saves CargoCrew.
// Run: unity command run_script --file AgentScripts/ApplyComingSoonArt.cs --entry ApplyComingSoonArt.Run
public static class ApplyComingSoonArt
{
    const string ScenePath = "Assets/Airlift/Scenes/CargoCrew.unity";
    const string ArtDir = "Assets/Airlift/Art/Tiles";

    public static string Run()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Review dirty scenes first.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var n = UnityEngine.Object.FindAnyObjectByType<NerdyDirector>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("NerdyDirector missing.");
        var catalog = n.catalogRoot.transform;
        var tile = DashboardWall.TileSize;
        int placed = 0;
        foreach (var world in DashboardCatalog.ComingSoon)
        {
            var soon = catalog.Find("Soon " + world.Id) ?? throw new InvalidOperationException("Soon " + world.Id + " missing: run BuildDashboard first.");
            var art = soon.Find("Art") ?? throw new InvalidOperationException("Soon " + world.Id + "/Art missing.");
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtDir + "/" + world.Id + ".png");
            if (sprite == null) continue;
            var old = art.Find("Photo"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var photo = new GameObject("Photo", typeof(RectTransform), typeof(Image)); photo.transform.SetParent(art, false);
            var pr = (RectTransform)photo.transform; pr.anchoredPosition = new Vector2(0f, -6f); pr.sizeDelta = new Vector2(tile.x - 24f, tile.y / 2f - 12f);
            var img = photo.GetComponent<Image>(); img.sprite = sprite; img.type = Image.Type.Simple; img.preserveAspect = false; img.raycastTarget = false;
            img.color = new Color(1f, 1f, 1f, 0.6f);
            placed++;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        return placed + " of " + DashboardCatalog.ComingSoon.Length + " coming-soon tiles have art";
    }
}
```
The photo's rect and anchor match `BuildDashboard.Photo()` (`unity/AgentScripts/BuildDashboard.cs:229-231`), so the coming-soon art sits exactly where the chapter art sits.

- [ ] **Step 8: Run the script, then the tests**

```bash
unity command run_script --file AgentScripts/ApplyComingSoonArt.cs --entry ApplyComingSoonArt.Run
bash scripts/verify-cargo.sh --suite GeneratedArtTests
bash scripts/verify-cargo.sh --suite DashboardWiringTests
bash scripts/verify-cargo.sh --suite ArtImportBudgetTests
```
Expected: `6 of 6 coming-soon tiles have art`, then all three suites PASS. `DashboardWiringTests` still counts 18 openable tiles with art.

- [ ] **Step 9: One render.** `PreviewWelcomeBoard.cs` renders Featured page 1 (`welcome-wall.png`) and Newest page 2 (`welcome-wall-newest-p2.png`), and its report lists the tile ids on each. If no `soon_` id appears on a rendered page, add a third shot after `unity/AgentScripts/PreviewWelcomeBoard.cs:70`:

```csharp
                wall.SetFilter(DashboardFilter.Featured); wall.NextPage(); wall.NextPage(); Canvas.ForceUpdateCanvases();
                Shot(n.head, Path.Combine(dir, "welcome-wall-p3.png"));
                sb.AppendLine("WALL featured page 3: " + string.Join(" | ", wall.Visible.Select(v => v.id)));
```
Then run `unity command run_script --file AgentScripts/PreviewWelcomeBoard.cs --entry PreviewWelcomeBoard.Run` and put the render that shows the coming-soon tiles on the canvas. **Stop for the owner's look.**

- [ ] **Step 10: Checkpoint.** Stage the six PNGs and metas, `ApplyComingSoonArt.cs`, `GeneratedArtTests.cs`, `CargoCrew.unity`. Suggested message: `Illustrated coming-soon tiles (Higgsfield, owner-approved)`. Commit only on request.

---

### Task 5: Chapter and hero tile art (15 images, replaced in place)

`BuildDashboard.Photo()` crops a render only when `Art/Tiles/<lessonId>-<n>.png` is missing (`BuildDashboard.cs:212-216`). Overwriting the file at the same path therefore changes the art without any scene or builder change, and the three hero tiles pick up chapter 1's art automatically.

**Files:**
- Modify (replace contents): `unity/Assets/Airlift/Art/Tiles/{cargo_crew_fractions,neighborhood_cafe_division,community_garden_multiplication}-{1..5}.png`

**Interfaces:**
- Consumes: `art_tools.py fit` (Task 3), `ApplyArtImportSettings.Run()` (Task 2).

- [ ] **Step 1: Ask for the Cargo Crew exception.** Cargo Crew is locked. The five `cargo_crew_fractions-*.png` tiles are catalog art, not lesson visuals, but ask the owner explicitly anyway. If the answer is no, do Café and Garden only (10 images).

- [ ] **Step 2: Generate.** For each tile, use `CH-<lessonId>-<n>` with the current PNG as the reference image. If `artifacts/dock7|cafe|garden/<n>a-start.png` exists, use that higher-resolution render as the reference instead. Make three candidates per tile, save them to `artifacts/higgsfield/chapters/<lessonId>-<n>-{1..3}.png`, and log them.

- [ ] **Step 3: Fit every candidate**

```bash
for f in artifacts/higgsfield/chapters/*-[123].png; do python3 scripts/art_tools.py fit "$f" "${f%.png}-tile.png" 584 166 0.55; done
```

- [ ] **Step 4: Faithfulness check before the canvas.** For each candidate, compare it against the current tile. The object counts and arrangement must match what the chapter really shows: the same number of crates or halves or quarters, plates, and rows × columns. A tile that shows 4 crates for a halves chapter misleads the learner. Reject mismatches and regenerate, still within the cap.

- [ ] **Step 5: Canvas approval.** Show the old and new tiles side by side in wall order (15 rows). **Stop until approved.**

- [ ] **Step 6: Replace in place, then verify**

```bash
cp artifacts/higgsfield/chapters/community_garden_multiplication-3-<pick>-tile.png unity/Assets/Airlift/Art/Tiles/community_garden_multiplication-3.png
# … one cp per approved tile
unity command run_script --file AgentScripts/RefreshAndCompile.cs --entry RefreshAndCompile.Run
unity command run_script --file AgentScripts/ApplyArtImportSettings.cs --entry ApplyArtImportSettings.Run
bash scripts/verify-cargo.sh --suite DashboardWiringTests
bash scripts/verify-cargo.sh --suite ArtImportBudgetTests
unity command run_script --file AgentScripts/PreviewWelcomeBoard.cs --entry PreviewWelcomeBoard.Run
```
Expected: `0 of 21 art textures updated`, because the metas are unchanged and the overrides survive a content replace. Both suites PASS. Put the page 1–3 renders on the canvas for the owner's look.

- [ ] **Step 7: Checkpoint.** Stage the replaced PNGs. Suggested message: `Illustrated chapter tiles (Higgsfield, owner-approved)`. Commit only on request.

---

### Task 6: Lounge panorama sky

**Files:**
- Create: `unity/Assets/Airlift/Art/Generated/Lounge/LoungePanorama.png` (2048×1024)
- Create: `unity/AgentScripts/ApplyLoungePanorama.cs`
- Modify: `unity/Assets/Airlift/Tests/EditMode/GeneratedArtTests.cs` (append two tests)
- Modify (by script): `unity/Assets/Airlift/Materials/Lounge/LoungeSky.mat`, `LoungeSkyPane.mat`

**Interfaces:**
- Consumes: `art_tools.py seam|seamless|fit` (Task 3); `LoungeSky.mat` (`Skybox/Panoramic`, `_Mapping` = 1, latitude-longitude) and `LoungeSkyPane.mat`, both made by `BuildLounge.cs`; `LoungeRoom` sets `RenderSettings.skybox` only in lounge mode (`LoungeRoom.cs:100`).
- Produces: `ApplyLoungePanorama.Run()`, which must run after any `BuildLounge.cs` rerun.

Today each window pane shows the *whole* gradient texture. With a real panorama that would repeat a squashed copy of the city in every window. The panes therefore become faint glass, and the true skybox shows through them with the right perspective.

- [ ] **Step 1: Generate.** Make four `SKY-BASE` candidates at 2:1 and save them to `artifacts/higgsfield/sky/sky-{1..4}.png`. Log them.

- [ ] **Step 2: Fit and wrap each one**

```bash
for i in 1 2 3 4; do
  python3 scripts/art_tools.py fit artifacts/higgsfield/sky/sky-$i.png artifacts/higgsfield/sky/sky-$i-2k.png 2048 1024 0.5
  python3 scripts/art_tools.py seam artifacts/higgsfield/sky/sky-$i-2k.png
  python3 scripts/art_tools.py seamless artifacts/higgsfield/sky/sky-$i-2k.png artifacts/higgsfield/sky/sky-$i-wrap.png 96
  python3 scripts/art_tools.py seam artifacts/higgsfield/sky/sky-$i-wrap.png
done
```
Expected: the second `seam` number is below 8. Discard any candidate where the cross-fade leaves a visible ghost at the join (look at the image offset by half its width). Also discard any candidate whose horizon is not near the middle row, because the room's windows sit at eye height.

- [ ] **Step 3: Canvas approval.** Show each wrapped candidate flat, plus the current `artifacts/lounge/` render for comparison. **Stop until approved.**

- [ ] **Step 4: Import the pick**

```bash
mkdir -p unity/Assets/Airlift/Art/Generated/Lounge
cp artifacts/higgsfield/sky/sky-<pick>-wrap.png unity/Assets/Airlift/Art/Generated/Lounge/LoungePanorama.png
unity command run_script --file AgentScripts/RefreshAndCompile.cs --entry RefreshAndCompile.Run
unity command run_script --file AgentScripts/ApplyArtImportSettings.cs --entry ApplyArtImportSettings.Run
```

- [ ] **Step 5: Append the failing tests to `GeneratedArtTests`**

```csharp
        const string Panorama = "Assets/Airlift/Art/Generated/Lounge/LoungePanorama.png";

        [Test] public void LoungeSkyUsesThePanoramaWrappingSideways()
        {
            var sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/Lounge/LoungeSky.mat");
            Assert.That(AssetDatabase.GetAssetPath(sky.GetTexture("_MainTex")), Is.EqualTo(Panorama), "run AgentScripts/ApplyLoungePanorama.cs");
            var importer = (TextureImporter)AssetImporter.GetAtPath(Panorama);
            Assert.That(importer.wrapModeU, Is.EqualTo(TextureWrapMode.Repeat));
            Assert.That(importer.wrapModeV, Is.EqualTo(TextureWrapMode.Clamp));
            Assert.That(importer.mipmapEnabled, Is.False, "no mip seam at the wrap");
        }

        [Test] public void WindowPanesAreFaintGlassNotACopyOfTheSky()
        {
            var pane = AssetDatabase.LoadAssetAtPath<Material>("Assets/Airlift/Materials/Lounge/LoungeSkyPane.mat");
            Assert.That(pane.mainTexture, Is.Null, "the real skybox shows through the window");
            Assert.That(pane.renderQueue, Is.GreaterThanOrEqualTo(3000), "transparent");
            Assert.That(pane.GetColor("_BaseColor").a, Is.LessThanOrEqualTo(0.2f));
        }
```

- [ ] **Step 6: Run them and confirm they fail**

Run: `bash scripts/verify-cargo.sh --suite GeneratedArtTests`
Expected: two FAILs, starting with `run AgentScripts/ApplyLoungePanorama.cs`. The coming-soon test still passes.

- [ ] **Step 7: Write the script**

```csharp
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// CC-HF-01. The lounge sky becomes the approved Higgsfield panorama; the window panes become faint glass so the
// real skybox shows through them. Run after BuildLounge (which regenerates the gradient). Idempotent.
// Run: unity command run_script --file AgentScripts/ApplyLoungePanorama.cs --entry ApplyLoungePanorama.Run
public static class ApplyLoungePanorama
{
    const string Panorama = "Assets/Airlift/Art/Generated/Lounge/LoungePanorama.png";
    const string MatDir = "Assets/Airlift/Materials/Lounge";

    public static string Run()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(Panorama) ?? throw new InvalidOperationException(Panorama + " missing.");
        if (importer.mipmapEnabled || importer.wrapModeU != TextureWrapMode.Repeat || importer.wrapModeV != TextureWrapMode.Clamp)
        { importer.mipmapEnabled = false; importer.wrapModeU = TextureWrapMode.Repeat; importer.wrapModeV = TextureWrapMode.Clamp; importer.SaveAndReimport(); }
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Panorama);

        var sky = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/LoungeSky.mat") ?? throw new InvalidOperationException("Run BuildLounge first.");
        sky.SetTexture("_MainTex", tex);
        sky.SetFloat("_Mapping", 1f);            // latitude-longitude
        sky.SetFloat("_ImageType", 0f);          // 360 degrees
        EditorUtility.SetDirty(sky);

        var pane = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/LoungeSkyPane.mat") ?? throw new InvalidOperationException("Run BuildLounge first.");
        pane.shader = Shader.Find("Universal Render Pipeline/Unlit");
        pane.mainTexture = null;
        pane.SetColor("_BaseColor", new Color(0.78f, 0.9f, 1f, 0.12f));
        pane.SetFloat("_Surface", 1f); pane.SetFloat("_Blend", 0f);
        pane.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); pane.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        pane.SetFloat("_ZWrite", 0f);
        pane.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        pane.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(pane);

        AssetDatabase.SaveAssets();
        return "lounge sky uses " + Panorama + "; panes are faint glass";
    }
}
```

- [ ] **Step 8: Run the script, the tests and the lounge tests**

```bash
unity command run_script --file AgentScripts/ApplyLoungePanorama.cs --entry ApplyLoungePanorama.Run
bash scripts/verify-cargo.sh --suite GeneratedArtTests
bash scripts/verify-cargo.sh --suite ArtImportBudgetTests
bash scripts/verify-cargo.sh --suite LoungeWiringTests
bash scripts/verify-cargo.sh --suite LoungeRoomTests
```
Expected: all PASS. If a lounge suite asserts the old gradient texture, **stop and report** the assertion. Do not edit that test without the owner's word.

- [ ] **Step 9: One render.** Run `unity command run_script --file AgentScripts/PreviewLounge.cs --entry PreviewLounge.Run` and put the `artifacts/lounge/` renders (front, left at Dee's seat, back) on the canvas. Check the horizon height through the windows and look for a visible seam behind the couch. **Stop for the owner's look.**

- [ ] **Step 10: Checkpoint.** Stage the panorama and its meta, the two materials, `ApplyLoungePanorama.cs` and `GeneratedArtTests.cs`. Suggested message: `Painted lounge sky and glass windows (Higgsfield, owner-approved)`. Commit only on request.

---

### Task 7: Headset check and performance gate

**Files:**
- Modify: `docs/qa/higgsfield-art-2026-09-28.md`

- [ ] **Step 1: Run the full suite for this ticket**

```bash
bash scripts/verify-cargo.sh --suite ArtImportBudgetTests
bash scripts/verify-cargo.sh --suite GeneratedArtTests
bash scripts/verify-cargo.sh --suite DashboardWiringTests
bash scripts/verify-cargo.sh --suite DashboardTests
```
Expected: all PASS. Record the counts in the QA file.

- [ ] **Step 2: Build and install, avoiding a stale build.** Wait until `recompile_status` says `completed` and the DLL modification time has moved, pause 5 s, then run `bash scripts/lounge-preview.sh`. Confirm that `[Nerdy] onboarding` appears in `~/Library/Android/sdk/platform-tools/adb logcat -v time -s Unity:I`.

- [ ] **Step 3: Owner walkthrough (adult tester).** Page through all three wall pages in "Your room", switch to "Nerdy Lounge", look out every window, and open one Café and one Garden lesson to confirm the lessons are unchanged. Ask the owner for a verdict on each tile page and on the sky.

- [ ] **Step 4: Performance.** Do three post-warm-up runs on the wall and in the lounge, using the same method as the Task 2 baseline. Pass means CPU and GPU p95 ≤ 13.9 ms, and no worse than the baseline by more than 0.5 ms when a baseline exists. If it fails, lower the panorama cap to 1024 in both `ApplyArtImportSettings` and `ArtImportBudgetTests`, rerun, and measure again.

- [ ] **Step 5: Record** the build identity, test counts, the performance table and the owner's verdicts in `docs/qa/higgsfield-art-2026-09-28.md`, with each state labelled verified, owner-reported or pending.

---

### Task 8: Dee character exploration (canvas only)

No scene change in this plan. Placing Dee in the onboarding flow touches the recently repaired tour, so it needs its own approved plan.

**Files:**
- Modify: `docs/plans/2026-09-28-higgsfield-visual-uplift/art-bible.md` (append a "Dee" section)

- [ ] **Step 1: Generate `DEE-SHEET`**: three design directions, two sheets each, saved to `artifacts/higgsfield/dee/`. Log them.
- [ ] **Step 2: Canvas approval** of one direction. **Stop until approved.**
- [ ] **Step 3: Train a Soul Character** from the approved sheet (a one-time credit cost) and record its character id in `art-bible.md`.
- [ ] **Step 4: Consistency check.** Generate Dee in the six expressions with the Soul id. All six must read as the same character at 128 px. Put them on the canvas.
- [ ] **Step 5: Write up placement options for the owner** in `art-bible.md`: a welcome-card portrait, a sprite beside the tour's step counter, and site and store art. For each, note the files it would touch, for example `BuildTour.cs` output and `CompactAssistantBar.cs` ordering. The owner decides whether a follow-up plan is written.

---

### Task 9 (optional, only with performance headroom from Task 7): Subtle detail textures

`RoundedBoxMesh` UVs run 0–1 per face (`RoundedBoxMesh.cs:56,98`), so a texture stretches with the box's aspect. Only low-contrast, large-shape textures survive that. This task is a spike: if the render shows stretching, drop it.

**Files:**
- Create: `unity/Assets/Airlift/Art/Generated/Detail/{soil,wood,rug,fabric}.png` (512×512)
- Create: `unity/AgentScripts/ApplyDetailTextures.cs`
- Modify: `unity/Assets/Airlift/Tests/EditMode/GeneratedArtTests.cs`

**Interfaces:**
- Consumes: `ApplyArtImportSettings.Run()`, `art_tools.py fit`.
- Produces: the material → map pairs `GardenSoil`, `GardenPeat` → soil; `GardenWood`, `LoungeWood` → wood; `LoungeRug`, `LoungeRugInner` → rug; `LoungeCouch` → fabric. None of these are Cargo materials.

- [ ] **Step 1: Generate** `DETAIL-soil|wood|rug|fabric` (MATERIAL = "rich soft soil", "pale smooth toy wood grain", "woven wool rug", "soft felt upholstery"), two each. Fit them with `python3 scripts/art_tools.py fit SRC DST 512 512`, check `seam` along the horizontal edges (and the vertical edges on a copy rotated 90°), and run `seamless` if needed.
- [ ] **Step 2: Canvas approval. Stop until approved.**
- [ ] **Step 3: Append the failing test**

```csharp
        static readonly (string mat, string map)[] Details =
        {
            ("Assets/Airlift/Materials/GardenSoil.mat", "soil"), ("Assets/Airlift/Materials/GardenPeat.mat", "soil"),
            ("Assets/Airlift/Materials/GardenWood.mat", "wood"), ("Assets/Airlift/Materials/Lounge/LoungeWood.mat", "wood"),
            ("Assets/Airlift/Materials/Lounge/LoungeRug.mat", "rug"), ("Assets/Airlift/Materials/Lounge/LoungeRugInner.mat", "rug"),
            ("Assets/Airlift/Materials/Lounge/LoungeCouch.mat", "fabric"),
        };

        [Test] public void DetailMapsTintTheirMaterialsWithoutReplacingTheColour()
        {
            foreach (var (matPath, map) in Details)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                Assert.That(AssetDatabase.GetAssetPath(mat.GetTexture("_BaseMap")), Is.EqualTo("Assets/Airlift/Art/Generated/Detail/" + map + ".png"), matPath + ": run AgentScripts/ApplyDetailTextures.cs");
                Assert.That(mat.GetTextureScale("_BaseMap").x, Is.GreaterThanOrEqualTo(1f), matPath);
            }
        }
```

- [ ] **Step 4: Run it and confirm it fails** (`bash scripts/verify-cargo.sh --suite GeneratedArtTests`).
- [ ] **Step 5: Write the script**

```csharp
using UnityEditor;
using UnityEngine;

// CC-HF-01 (optional). Subtle 512 px detail maps multiply over the existing flat colours; _BaseColor is untouched.
// Idempotent. Run: unity command run_script --file AgentScripts/ApplyDetailTextures.cs --entry ApplyDetailTextures.Run
public static class ApplyDetailTextures
{
    static readonly (string mat, string map, float tiling)[] Details =
    {
        ("Assets/Airlift/Materials/GardenSoil.mat", "soil", 2f), ("Assets/Airlift/Materials/GardenPeat.mat", "soil", 2f),
        ("Assets/Airlift/Materials/GardenWood.mat", "wood", 1f), ("Assets/Airlift/Materials/Lounge/LoungeWood.mat", "wood", 1f),
        ("Assets/Airlift/Materials/Lounge/LoungeRug.mat", "rug", 3f), ("Assets/Airlift/Materials/Lounge/LoungeRugInner.mat", "rug", 3f),
        ("Assets/Airlift/Materials/Lounge/LoungeCouch.mat", "fabric", 2f),
    };

    public static string Run()
    {
        int n = 0;
        foreach (var (matPath, map, tiling) in Details)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Airlift/Art/Generated/Detail/" + map + ".png");
            if (mat == null || tex == null) continue;
            mat.SetTexture("_BaseMap", tex); mat.SetTextureScale("_BaseMap", new Vector2(tiling, tiling));
            EditorUtility.SetDirty(mat); n++;
        }
        AssetDatabase.SaveAssets();
        return n + " of " + Details.Length + " materials carry a detail map";
    }
}
```
The detail PNGs must be near-white (mean luminance ≥ 200). URP Lit multiplies `_BaseMap` by `_BaseColor`, so a dark map would darken the approved palette. If a pick is darker than that, lift it with `python3 -c "from PIL import Image,ImageEnhance;im=Image.open('P');ImageEnhance.Brightness(im).enhance(1.6).save('P')"` before import.

- [ ] **Step 6: Import, apply, test, render**

```bash
unity command run_script --file AgentScripts/RefreshAndCompile.cs --entry RefreshAndCompile.Run
unity command run_script --file AgentScripts/ApplyArtImportSettings.cs --entry ApplyArtImportSettings.Run
unity command run_script --file AgentScripts/ApplyDetailTextures.cs --entry ApplyDetailTextures.Run
bash scripts/verify-cargo.sh --suite GeneratedArtTests
bash scripts/verify-cargo.sh --suite ArtImportBudgetTests
unity command run_script --file AgentScripts/PreviewGardenChapters.cs --entry PreviewGardenChapters.Run
unity command run_script --file AgentScripts/PreviewLounge.cs --entry PreviewLounge.Run
```
Put the renders on the canvas. If the maps look stretched or noisy, revert with `git checkout -- <materials>`, delete `Art/Generated/Detail/`, and remove the test. **Stop for the owner's look**, then repeat Task 7 steps 2–5.

- [ ] **Step 7: Checkpoint** (commit only on request).

---

### Task 10: Landing site and marketing media

**Files:**
- Create: `site/dist/assets/illustration-*.jpg`, `site/dist/assets/og-card.jpg`
- Modify: `site/dist/index.html`

- [ ] **Step 1: Generate** (1) a hero illustration at 16:9 that pairs the approved tile style with Dee, (2) an Open Graph card at 1200×630 with **no text in the image** (the page title supplies it), and (3) optionally one 10 s video loop of the lounge sky or a coming-soon world. Log them, including the video cap.
- [ ] **Step 2: Canvas approval. Stop until approved.**
- [ ] **Step 3: Insert as illustration, never as capture.** Keep the current headset hero image. Add the illustration in a new figure below the "Coming soon" copy, with this caption:

```html
<figure class="illustration"><img src="/assets/illustration-coming-soon.jpg" width="1600" height="900" alt="Illustration of upcoming Nerdy worlds: a toy bus stop, a train platform clock and a corner store." loading="lazy"><figcaption><span class="preview-label">Illustration · not a headset capture</span></figcaption></figure>
```
Add `<meta property="og:image" content="https://nerdy-vr-landing.vercel.app/assets/og-card.jpg">` to `<head>`.
- [ ] **Step 4: Preview deploy.** Run `cd site && vercel` (a preview, **not** `--prod`) and send the preview URL to the owner. `vercel --prod` runs only on the owner's word.

---

### Task 11: Documentation and gates

**Files:**
- Modify: `docs/00-build/RELEASE-GATES.md` (Assets and licenses table)
- Modify: `CLAUDE.md`, `AGENTS.md` (builder order, a short dated section)
- Modify: `docs/plans/2026-09-28-higgsfield-visual-uplift/generation-log.md` (final totals)

- [ ] **Step 1: Add the licence rows**

```markdown
| Tile art, coming-soon art, lounge panorama, detail maps (`Art/Tiles`, `Art/Generated`) | generated with Higgsfield (models per generation-log.md); commercial use per owner-confirmed plan terms (date) | ok once owner confirmation is recorded |
| Landing-site illustrations and OG card | generated with Higgsfield; labelled "Illustration" on the site | ok once owner confirmation is recorded |
```
Change the existing props row to: `All 3D props … | generated in-project by RoundedBoxMesh scripts | ok, no third-party 3D art`.

- [ ] **Step 2: Builder order.** Append to the welcome-side builder order in CLAUDE.md: `… BuildDashboard → ApplyComingSoonArt; BuildLounge → ApplyLoungePanorama; after any art import → ApplyArtImportSettings (→ ApplyDetailTextures if Task 9 shipped)`. Mirror the change in AGENTS.md.

- [ ] **Step 3: Check the relative links** in every changed doc, as the September 18 rule requires:

```bash
grep -oE '\]\(([^)#]+)' docs/qa/higgsfield-art-2026-09-28.md docs/plans/2026-09-28-higgsfield-visual-uplift/*.md | sed 's/.*](//' | while read p; do [ -e "$p" ] || [ -e "docs/qa/$p" ] || [ -e "docs/plans/2026-09-28-higgsfield-visual-uplift/$p" ] || echo "broken: $p"; done
```
Expected: no output.

- [ ] **Step 4: Run `detect_changes()`** (GitNexus) and confirm the change touches only the AgentScripts, tests, art, the two lounge materials, the scene's tile photos and the docs. Then ask the owner whether to commit.

---

## Phase gates at a glance

| Gate | After | Owner decides |
|---|---|---|
| G1 | Task 1 | Style direction (A, B or C) and commercial terms |
| G2 | Task 4 | Six coming-soon picks, then the page-3 render |
| G3 | Task 5 | Cargo exception; 15 tile picks; wall renders |
| G4 | Task 6 | Sky pick; lounge renders |
| G5 | Task 7 | Headset verdict; performance pass |
| G6 | Tasks 8–10 | Dee direction; detail maps (optional); site preview → prod |
