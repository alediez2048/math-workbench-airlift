"""Fit Higgsfield outputs to Nerdy's art slots. Pure Pillow; no network."""
import sys

from PIL import Image


def fit_cover(src, dst, width, height, focus_y=0.5):
    """Scale to cover width x height, crop centred horizontally and at focus_y vertically, save opaque RGB."""
    with Image.open(src) as im:
        img = im.convert('RGBA')
    backdrop = Image.new('RGBA', img.size, (32, 35, 68, 255))   # navy base under any transparency
    img = Image.alpha_composite(backdrop, img).convert('RGB')
    scale = max(width / img.width, height / img.height)
    resized = img.resize((max(width, round(img.width * scale)), max(height, round(img.height * scale))), Image.LANCZOS)
    left = (resized.width - width) // 2
    top = round((resized.height - height) * min(1.0, max(0.0, focus_y)))
    resized.crop((left, top, left + width, top + height)).save(dst, 'PNG')


def seam_error(path):
    """Mean absolute RGB difference between the first and last pixel columns."""
    with Image.open(path) as im:
        img = im.convert('RGB')
    w, h = img.size
    total = 0
    for y in range(h):
        a, b = img.getpixel((0, y)), img.getpixel((w - 1, y))
        total += sum(abs(a[i] - b[i]) for i in range(3)) / 3
    return total / h


def make_seamless(src, dst, band=64):
    """Wrap a panorama by crossfading its last `band` columns into its first ones, then restore the width.

    The result R (width w - band) starts with R[x] = lerp(px[w-band+x], px[x], x/band), so R[0] continues from
    R[-1] = px[w-band-1] and R[band-1] runs into px[band]: only neighbouring content is blended, never a mirror.
    """
    with Image.open(src) as im:
        img = im.convert('RGB')
    w, h = img.size
    px = img.load()
    out = Image.new('RGB', (w - band, h))
    opx = out.load()
    for x in range(w - band):
        for y in range(h):
            if x < band:
                t = x / band
                a, b = px[w - band + x, y], px[x, y]
                opx[x, y] = tuple(round(a[i] * (1 - t) + b[i] * t) for i in range(3))
            else:
                opx[x, y] = px[x, y]
    out.resize((w, h), Image.LANCZOS).save(dst, 'PNG')

def detail_map(src, dst, size=512, mean=0.92, spread=0.08):
    """A pale grey detail texture that tiles both ways: multiplies over a flat material colour without changing its hue
    or darkening it by more than `spread` (values span mean ± spread)."""
    with Image.open(src) as im:
        grey = im.convert('L').resize((size, size), Image.LANCZOS)
    values = list(grey.getdata())
    centre = sum(values) / len(values)
    reach = max(1.0, max(abs(v - centre) for v in values))
    scaled = [max(0, min(255, round(255 * (mean + spread * (v - centre) / reach)))) for v in values]
    grey.putdata(scaled)
    tmp = grey.convert('RGB')
    tmp.save(dst, 'PNG')
    make_seamless(dst, dst, band=max(8, size // 8))
    with Image.open(dst) as im:
        turned = im.transpose(Image.Transpose.ROTATE_90)
    turned.save(dst, 'PNG')
    make_seamless(dst, dst, band=max(8, size // 8))
    with Image.open(dst) as im:
        im.transpose(Image.Transpose.ROTATE_270).convert('L').convert('RGB').save(dst, 'PNG')

def tint_to_mean(src, dst, target, width, height, trim=0.04):
    """Trim `trim` of each edge (generated borders), fit to width x height, then scale each channel so the image's
    average colour equals `target` (sRGB 0-1): a painted surface that keeps the approved flat colour on average."""
    with Image.open(src) as im:
        img = im.convert('RGB')
    w, h = img.size
    dx, dy = round(w * trim), round(h * trim)
    img = img.crop((dx, dy, w - dx, h - dy))
    tmp = dst + '.fit.png'
    img.save(tmp, 'PNG')
    fit_cover(tmp, tmp, width, height)
    with Image.open(tmp) as im:
        img = im.convert('RGB')
    import os
    os.remove(tmp)
    for _ in range(3):   # clipping at 255 shifts the mean, so converge in a few passes
        px = list(img.getdata())
        mean = [sum(p[i] for p in px) / len(px) / 255 for i in range(3)]
        gain = [target[i] / max(mean[i], 1e-3) for i in range(3)]
        img.putdata([tuple(max(0, min(255, round(p[i] * gain[i]))) for i in range(3)) for p in px])
    img.save(dst, 'PNG')

if __name__ == '__main__':
    cmd, args = sys.argv[1], sys.argv[2:]
    if cmd == 'fit':
        fit_cover(args[0], args[1], int(args[2]), int(args[3]), float(args[4]) if len(args) > 4 else 0.5)
    elif cmd == 'seam':
        print('%.2f' % seam_error(args[0]))
    elif cmd == 'detail':
        detail_map(args[0], args[1], int(args[2]) if len(args) > 2 else 512)
    elif cmd == 'seamless':
        make_seamless(args[0], args[1], int(args[2]) if len(args) > 2 else 64)
    else:
        sys.exit('usage: art_tools.py fit|seam|seamless|detail ...')
