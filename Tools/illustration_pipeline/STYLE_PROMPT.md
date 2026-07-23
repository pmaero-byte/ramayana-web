# Ramayana / Traders Illustration Style Lock

**Purpose:** Lock the visual style for all 200 illustrations so they look like they came from the same animated film.

**Reference target:** 1990s Indian hand-painted animation aesthetic (Yugo Studios "Ramayana: The Legend of Prince Rama" 1993 + Ramanand Sagar TV series 1987-88).
**Model:** `flux_2_klein_4b_q6p.ckpt` (FLUX.2 Klein 4B, 6-bit quantized, runs locally via draw-things-cli).
**Generation command:** see `generate.sh` in this directory.

---

## Positive Prompt Skeleton

Every illustration uses the same 4-segment structure. Vary only **SEGMENT 2** (the specific subject).

```
SEGMENT 1 (style lock): "hand-painted illustration in the style of a 1990s Indian animated film, "
SEGMENT 2 (subject):    "<CHARACTER OR SCENE DESCRIPTION — see roster>"
SEGMENT 3 (lighting):   "warm earth tone palette, golden hour cinematic lighting, dramatic rim light, "
SEGMENT 4 (technical):  "highly detailed, painterly textures, 4K illustration, isolated subject on simple background"
```

### Why this structure works

- **Segment 1 anchors the style** — by always mentioning "1990s Indian animated film", FLUX locks the painterly aesthetic.
- **Segment 4 includes "isolated subject on simple background"** — gives us clean PNGs with simple backgrounds, ideal for game sprites. Game composite layers them onto procedural ground.
- **Negative prompt below excludes photorealistic 3D, modern game look, anime cel-shading** — the most common drift modes.

---

## Negative Prompt (universal — same for ALL generations)

```
photorealistic, 3D render, CGI, modern video game screenshot, anime cel-shading, chibi,
pixel art, low resolution, blurry, jpeg artifacts, watermark, signature, text, logo,
multiple subjects, busy background, modern clothing, watch, glasses, smartphone, gun,
airplane, car, motor vehicle, anachronistic objects, pale skin, caucasian features
```

---

## Palette Anchor (for character consistency)

These hex values appear in every generation prompt to lock the warm-1990s palette:

| Element | Hex | Used in |
|---|---|---|
| Saffron (sacred) | `#FF9933` | Rama/Sita clothing, temple robes, divine markers |
| Indigo (depth) | `#3B2F8A` | Night skies, deep backgrounds, Krishna iconography |
| Vermilion (force) | `#E34234` | Weapons, blood accents, demon eyes, Ravana |
| Gold (divine) | `#D4A437` | Crowns, jewelry, sun rays, Hanuman's fur trim |
| Forest green | `#2D5A2D` | Trees, Lanka gardens, arrows |
| Earth (ground) | `#6B4423` | Architecture, chariot wheels, scrolls |
| Lotus pink | `#E89EB8` | Sita's clothing accents, flowers |
| Skin (warm) | `#B5754F` | All character skin tones (no pale tones — explicitly excluded) |

**In the positive prompt, append**: `"saffron indigo vermilion gold palette"`

---

## Per-Character Negative Override (additional to universal)

When generating specific characters, ADD these to the negative prompt:

| Character | Additional negatives |
|---|---|
| Rama | crown that obscures eyes, modern armor |
| Sita | provocative, modern dress, jewelry excess |
| Hanuman | humanoid-only (he has a monkey face + tail) |
| Ravana | sympathetic, handsome-only (must show 10 heads or extra arms) |
| Lakshmana | older than 30, bald |
| Bharata | royal garb only (no warrior armor in his default) |

---

## Generation Settings (universal)

| Parameter | Value | Why |
|---|---|---|
| `--width` | 1024 (chars) / 1920 (backgrounds) | Multiple of 64 |
| `--height` | 1024 (chars) / 1080 (backgrounds) | Standard aspect ratios |
| `--steps` | 8 (chars) / 12 (backgrounds) | FLUX.2 Klein: 4-step is draft quality, 8 is good |
| `--cfg` | 3.5 (don't go higher — FLUX.2 Klein drifts at high CFG) |
| `--seed` | NOT set by default — varied per generation | Variation is good |

---

## Seed Lock Strategy (when you like a result and want variants)

Once you find a generation you love (e.g., a specific Rama hero pose), note the seed:
```
draw-things-cli generate --model flux_2_klein_4b_q6p.ckpt \
  --prompt "..." --seed 12345
```
Then for the SAME character in different poses, lock the seed and vary only the pose description:
```
draw-things-cli generate --model flux_2_klein_4b_q6p.ckpt \
  --prompt "...Rama in dynamic battle pose with bow drawn..." --seed 12345
```

---

## File Naming Convention

Each generated PNG goes to `Assets/Illustrations/<category>/<id>_<view>_<seed>.png`

- **categories:** `characters`, `cities`, `props`, `atmosphere`
- **id:** canonical name (e.g., `rama_hero`, `harappa_dawn`)
- **view:** pose angle (`front`, `side`, `back`, `3quarter`, `portrait`, `fullbody`, `battle`)
- **seed:** the FLUX seed used (so it's reproducible)

Example: `Assets/Illustrations/characters/rama_hero_front_12345.png`

---

## Quality Bar Checklist (per illustration)

Before accepting a generation, verify:

- [ ] **Painterly texture** visible (brush-stroke feel, not photoreal)
- [ ] **Warm earth tone palette** dominant (no cold blues/greys)
- [ ] **Subject clearly readable** as the character (no ambiguity)
- [ ] **Cinematic lighting** with at least one strong rim light
- [ ] **Simple background** (game composites onto procedural ground)
- [ ] **PNG dimensions match target** (1024×1024 char, 1920×1080 bg)
- [ ] **No anachronisms** (no guns, planes, watches, modern clothes)
- [ ] **No text/watermarks/UI elements**

If 5+ criteria fail, re-roll with same prompt + new seed.
