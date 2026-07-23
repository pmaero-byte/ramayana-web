#!/usr/bin/env python3
"""
make_mmx_portraits_transparent.py
Convert mmx-generated JPG portraits (with complex backgrounds) to PNG with
alpha channel. Uses GrabCut-inspired approach: dominant background color
detection from corners + edge feathering for clean alpha.

For mmx portraits (photorealistic with detailed backgrounds), the simple
corner-sample approach from make_sprites_transparent.py won't work — those
images have actual scene backgrounds (clouds, ships, etc).

Strategy: use OpenCV GrabCut with a center-biased foreground mask.
Falls back to corner-sampling if cv2 is unavailable.

Usage:
    python3 make_mmx_portraits_transparent.py <input_dir>
"""
import sys
import os
import math
from pathlib import Path
from PIL import Image

try:
    import cv2
    import numpy as np
    HAS_CV2 = True
except ImportError:
    HAS_CV2 = False

TOLERANCE = 50  # Euclidean RGB tolerance for corner-sample fallback


def corner_sample_transparent(img_rgb: Image.Image) -> Image.Image:
    """Simple corner-sample approach for backgrounds with uniform color."""
    w, h = img_rgb.size
    samples = [
        img_rgb.getpixel((5, 5))[:3],
        img_rgb.getpixel((w - 6, 5))[:3],
        img_rgb.getpixel((5, h - 6))[:3],
        img_rgb.getpixel((w - 6, h - 6))[:3],
    ]
    bg_rgb = tuple(sum(s[i] for s in samples) // 4 for i in range(3))

    img = img_rgb.convert('RGBA')
    pixels = img.load()
    for y in range(h):
        for x in range(w):
            r, g, b, a = pixels[x, y]
            d = math.sqrt((r - bg_rgb[0]) ** 2 + (g - bg_rgb[1]) ** 2 + (b - bg_rgb[2]) ** 2)
            if d < TOLERANCE:
                pixels[x, y] = (r, g, b, 0)
    return img


def grabcut_transparent(img_rgb: Image.Image) -> Image.Image:
    """OpenCV GrabCut for complex backgrounds — foreground is center 60%."""
    arr = np.array(img_rgb)
    h, w = arr.shape[:2]

    # Initial mask: 0 (definite bg) around edges, 1 (probable fg) in center
    margin_x = int(w * 0.10)
    margin_y = int(h * 0.05)
    mask = np.zeros((h, w), np.uint8)
    mask[margin_y:h - margin_y, margin_x:w - margin_x] = cv2.GC_PR_FGD
    mask[0:margin_y, :] = cv2.GC_BGD
    mask[h - margin_y:h, :] = cv2.GC_BGD
    mask[:, 0:margin_x] = cv2.GC_BGD
    mask[:, w - margin_x:w] = cv2.GC_BGD

    bgd_model = np.zeros((1, 65), np.float64)
    fgd_model = np.zeros((1, 65), np.float64)

    cv2.grabCut(arr, mask, None, bgd_model, fgd_model, 3, cv2.GC_INIT_WITH_MASK)

    # 0 or 2 = bg, 1 or 3 = fg
    alpha_mask = np.where((mask == cv2.GC_FGD) | (mask == cv2.GC_PR_FGD), 255, 0).astype('uint8')

    # Soft alpha at edges (3px feather)
    alpha_mask = cv2.GaussianBlur(alpha_mask, (5, 5), 0)

    # Apply alpha
    img_rgba = arr.copy()
    img_rgba = np.dstack([img_rgba, alpha_mask])

    return Image.fromarray(img_rgba, mode='RGBA')


def process_file(path: Path):
    img = Image.open(path).convert('RGB')
    if HAS_CV2:
        result = grabcut_transparent(img)
    else:
        result = corner_sample_transparent(img)

    out_path = path.with_suffix('.png')
    result.save(out_path, 'PNG', optimize=True)

    # Delete original JPG
    path.unlink()

    # Report transparency
    w, h = result.size
    pixels = list(result.getdata())
    transparent = sum(1 for px in pixels if px[3] < 30)
    pct = transparent / len(pixels) * 100
    print(f"  {path.name} -> {out_path.name}: {pct:.1f}% transparent")


def main():
    if len(sys.argv) < 2:
        print(f"Usage: {sys.argv[0]} <directory>")
        sys.exit(1)

    in_dir = Path(sys.argv[1])
    if not in_dir.is_dir():
        print(f"Not a directory: {in_dir}")
        sys.exit(1)

    jpg_files = sorted(in_dir.glob('*.jpg')) + sorted(in_dir.glob('*.jpeg'))
    if not jpg_files:
        print(f"No JPG files found in {in_dir}")
        return

    print(f"Processing {len(jpg_files)} JPG files in {in_dir}")
    print(f"Backend: {'OpenCV GrabCut' if HAS_CV2 else 'Corner-sample fallback'}")
    for f in jpg_files:
        process_file(f)

    print(f"\nDone. {len(jpg_files)} files converted to PNG with alpha.")


if __name__ == '__main__':
    main()
