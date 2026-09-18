#!/usr/bin/env python3
"""Draw the settings gear as a PNG sprite.

The Nerdy fonts have no gear glyph, and a missing character renders as a box, so the gear is a drawn sprite
rather than a text label. Pure stdlib: the shape is rasterised with 4x supersampling and written as an RGBA PNG
by hand, so this needs no image library on the build machine.

    python3 scripts/make_gear_sprite.py unity/Assets/Airlift/Sprites
"""
import math
import struct
import sys
import zlib
from pathlib import Path

SIZE = 128          # final sprite, square
SS = 4              # supersampling factor
TEETH = 8
OUTER = 0.46        # tooth tip, as a fraction of the sprite
ROOT = 0.355        # gear body between the teeth
HOLE = 0.155        # centre hole
TOOTH_SHARE = 0.46  # how much of each tooth period is tooth rather than gap


def coverage(px, py):
    """White where the gear is, transparent elsewhere; alpha carries the anti-aliasing."""
    dx, dy = px - 0.5, py - 0.5
    r = math.hypot(dx, dy)
    if r < 1e-6:
        return 0.0
    if r <= HOLE:
        return 0.0
    angle = math.atan2(dy, dx)
    period = 2.0 * math.pi / TEETH
    phase = (angle % period) / period                      # 0..1 across one tooth period
    on_tooth = abs(phase - 0.5) < TOOTH_SHARE / 2.0
    limit = OUTER if on_tooth else ROOT
    return 1.0 if r <= limit else 0.0


def render():
    rows = []
    for y in range(SIZE):
        row = bytearray()
        for x in range(SIZE):
            hits = 0
            for sy in range(SS):
                for sx in range(SS):
                    px = (x + (sx + 0.5) / SS) / SIZE
                    py = (y + (sy + 0.5) / SS) / SIZE
                    hits += coverage(px, py)
            alpha = int(round(255 * hits / (SS * SS)))
            row += bytes((255, 255, 255, alpha))            # white, tinted by the UI colour at runtime
        rows.append(bytes(row))
    return rows


def write_png(path, rows):
    raw = b"".join(b"\x00" + r for r in rows)               # filter byte 0 per scanline

    def chunk(tag, data):
        body = tag + data
        return struct.pack(">I", len(data)) + body + struct.pack(">I", zlib.crc32(body) & 0xFFFFFFFF)

    png = (b"\x89PNG\r\n\x1a\n"
           + chunk(b"IHDR", struct.pack(">IIBBBBB", SIZE, SIZE, 8, 6, 0, 0, 0))
           + chunk(b"IDAT", zlib.compress(raw, 9))
           + chunk(b"IEND", b""))
    path.write_bytes(png)


if __name__ == "__main__":
    out = Path(sys.argv[1] if len(sys.argv) > 1 else "unity/Assets/Airlift/Sprites")
    out.mkdir(parents=True, exist_ok=True)
    target = out / "NerdyGear.png"
    write_png(target, render())
    print(f"{target} ({target.stat().st_size} bytes, {SIZE}x{SIZE}, {TEETH} teeth)")
