#!/usr/bin/env python3
"""
make_sprites_transparent.py
Convert the painterly character PNGs from opaque RGB to RGBA with
transparent backgrounds. Uses flood-fill from the 4 corners — the
generated backgrounds are uniform warm earth tones, so the corner
pixels reliably indicate the background color.

For each pixel, if it's within tolerance (Euclidean RGB distance) of
the corner color, make it transparent. Otherwise keep it.

Usage:
    python3 make_sprites_transparent.py
"""
import sys
import math
from pathlib import Path
from PIL import Image

ILL_DIR = Path('/Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/characters')
TOLERANCE = 80  # Euclidean RGB distance tolerance (was 35 Manhattan; bumped to 80 Euclidean for v2 batch)

def make_transparent(png_path: Path):
    img = Image.open(png_path).convert('RGBA')
    w, h = img.size

    # Sample background color from the 4 corners (5px inset to avoid foot shadow)
    samples = [
        img.getpixel((5, 5))[:3],
        img.getpixel((w - 6, 5))[:3],
        img.getpixel((5, h - 6))[:3],
        img.getpixel((w - 6, h - 6))[:3],
    ]
    bg_rgb = (
        sum(s[0] for s in samples) // 4,
        sum(s[1] for s in samples) // 4,
        sum(s[2] for s in samples) // 4,
    )

    pixels = img.load()
    made_transparent = 0
    for y in range(h):
        for x in range(w):
            r, g, b, a = pixels[x, y]
            # Euclidean RGB distance from corner average
            d = math.sqrt((r - bg_rgb[0]) ** 2 + (g - bg_rgb[1]) ** 2 + (b - bg_rgb[2]) ** 2)
            if d < TOLERANCE:
                pixels[x, y] = (r, g, b, 0)
                made_transparent += 1

    img.save(png_path)
    total = w * h
    pct = 100 * made_transparent / total
    print(f"  {png_path.name}: bg={bg_rgb} → {made_transparent}/{total} pixels ({pct:.1f}%) transparent")

def main():
    pngs = sorted(ILL_DIR.glob('*.png'))
    print(f"Processing {len(pngs)} character PNGs (Euclidean tolerance={TOLERANCE})...")
    for p in pngs:
        make_transparent(p)
    print(f"Done. {len(pngs)} files updated.")

if __name__ == '__main__':
    main()