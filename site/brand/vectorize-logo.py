"""Trace the supplied Nerdy AI + VR wordmark into transparent SVG paths."""

from pathlib import Path
import json
from PIL import Image, ImageOps

source = Path('/Users/jad/Downloads/4F32D244-597C-4F2B-840C-EAD4B869D7BE.png')
output_dir = Path(__file__).parent
image = Image.open(source).convert('RGB')
mask = Image.new('L', image.size)
src = image.load()
dst = mask.load()

for y in range(image.height):
    for x in range(image.width):
        r, g, b = src[x, y]
        dst[x, y] = 255 if (0.2126 * r + 0.7152 * g + 0.0722 * b) > 64 else 0

bounds = mask.getbbox()
if not bounds:
    raise RuntimeError('No logo artwork detected')
margin = 8
left, top, right, bottom = bounds
mask = mask.crop((max(0, left - margin), max(0, top - margin), min(image.width, right + margin), min(image.height, bottom + margin)))
width, height = mask.size
mask = ImageOps.invert(mask)
binary_path = output_dir / 'trace-mask.png'
mask.save(binary_path)
(output_dir / 'trace-meta.json').write_text(json.dumps({'width': width, 'height': height}))
print(f'{binary_path}: {width}x{height}')
