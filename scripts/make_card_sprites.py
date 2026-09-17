"""Nerdy card polish sprites (owner 2026-09-17). Supersampled anti-aliased shapes, written into Assets/Airlift/Sprites.
Import settings (sprite borders, pixels per unit, mipmaps) are applied by AgentScripts/PolishCards.cs."""
import sys
from PIL import Image, ImageDraw, ImageFilter

OUT = sys.argv[1]
SS = 4  # supersampling factor

def rounded(size, radius, inset=0, fill=255, top_only=False):
    w, h = size
    big = Image.new('L', (w * SS, h * SS), 0)
    d = ImageDraw.Draw(big)
    box = [inset * SS, inset * SS, (w - inset) * SS - 1, (h - inset) * SS - 1]
    if top_only:
        d.rounded_rectangle(box, radius * SS, fill=fill, corners=(True, True, False, False))
    else:
        d.rounded_rectangle(box, radius * SS, fill=fill)
    return big.resize((w, h), Image.LANCZOS)

def white_with_alpha(alpha):
    img = Image.new('RGBA', alpha.size, (255, 255, 255, 0)); img.putalpha(alpha); return img

# Card: 512 px, corner 112 px (imported at 400 ppu -> 28 canvas units).
white_with_alpha(rounded((512, 512), 112)).save(OUT + '/NerdyCard.png')
# Pill: 258 x 258, corner 128 px, 2 px stretch centre (imported at 100 ppu; ppu multiplier per element makes a capsule).
white_with_alpha(rounded((258, 258), 128)).save(OUT + '/NerdyPill.png')
# Stroke: 6 px ring (1.5 canvas units) on the card outline.
outer = rounded((512, 512), 112); inner = rounded((512, 512), 106, inset=6)
ring = Image.eval(Image.merge('LA', (outer, inner)).split()[0], lambda v: v)
ring = Image.frombytes('L', outer.size, bytes(max(0, a - b) for a, b in zip(outer.tobytes(), inner.tobytes())))
white_with_alpha(ring).save(OUT + '/NerdyStroke.png')
# Shadow: 256 px, card shape inset 48 px, blurred; border 96 px at 200 ppu -> 48 units (24 blur + 24 shape).
shape = rounded((256, 256), 56, inset=48)
shadow = shape.filter(ImageFilter.GaussianBlur(14))
Image.merge('RGBA', (Image.new('L', shadow.size, 0),) * 3 + (shadow,)).save(OUT + '/NerdyShadow.png')
# Catalog art: the spectrum gradient with rounded top corners matching the card (280x140 units at 4 px per unit).
spectrum = Image.open(OUT + '/NerdySpectrumGradient.png').convert('RGBA').resize((1120, 560), Image.BICUBIC)
mask = rounded((1120, 560), 112, top_only=True)
spectrum.putalpha(Image.frombytes('L', mask.size, bytes(min(a, m) for a, m in zip(spectrum.split()[3].tobytes(), mask.tobytes()))))
spectrum.save(OUT + '/NerdyCardArt.png')
# Fade: vertical alpha ramp, transparent at the top to opaque at the bottom (smoothstep), tinted by the card colour.
fade = Image.new('RGBA', (8, 128), (255, 255, 255, 0))
for y in range(128):
    t = y / 127.0; a = int(255 * (t * t * (3 - 2 * t)))
    for x in range(8): fade.putpixel((x, y), (255, 255, 255, a))   # image row 0 is the top: transparent there
fade.save(OUT + '/NerdyFade.png')
# Gradient pill: the brand gradient baked into a capsule, 1024 x 130 with 64 px caps (sliced: the centre stretches the
# gradient, the caps keep round ends), so gradient buttons and badges need no stencil mask.
brand = Image.open(OUT + '/NerdyBrandGradient.png').convert('RGBA').resize((1024, 130), Image.BICUBIC)
cap = rounded((1024, 130), 64)
brand.putalpha(cap)
brand.save(OUT + '/NerdyPillGradient.png')
print('sprites written')
