#!/usr/bin/env bash
# Atmosphere plate batch — 12 sky/time-of-day plates for Ramayana PS5 game
# Style: 1990s Indian hand-painted animation aesthetic (FLUX.2-Klein-4B)
# Output: 1920×1088 (multiples of 64, FLUX.2-Klein-4B requirement)
#
# Command syntax verified:
#   draw-things-cli generate --model flux_2_klein_4b_q6p.ckpt -p "..." --steps 8 --width 1920 --height 1088 --cfg 4.0 -o /path/to/out.png

set -e

DT_CLI="/Users/prabaharan/.local/bin/draw-things-cli"
MODEL="flux_2_klein_4b_q6p.ckpt"
OUT_DIR="/Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/atmosphere"
mkdir -p "$OUT_DIR"

# Common style prefix/suffix (same as Ramayana character batch)
PREFIX="hand-painted illustration in the style of a 1990s Indian animated film"
SUFFIX="warm earth tone palette, golden hour cinematic lighting, dramatic rim light, highly detailed painterly textures, 4K illustration, panoramic landscape, no humans in foreground, no text, no signature, no watermark, no logo"

NEG="photorealistic, 3D render, CGI, modern video game screenshot, anime cel-shading, chibi, pixel art, low resolution, blurry, jpeg artifacts, watermark, text, logo, modern clothing, anachronistic objects, busy composition"

gen() {
    local id="$1"; local prompt="$2"
    local out="$OUT_DIR/${id}.png"
    if [ -f "$out" ]; then
        echo "SKIP: $id (exists)"
        return
    fi
    echo "[$id] generating..."
    "$DT_CLI" generate \
        --model "$MODEL" \
        --prompt "${PREFIX}, ${prompt}, ${SUFFIX}" \
        --negative-prompt "$NEG" \
        --steps 8 \
        --width 1920 --height 1088 \
        --cfg 4.0 \
        -o "$out" 2>&1 | tail -3
    echo "  -> $out ($(du -h "$out" 2>/dev/null | cut -f1))"
}

# ── 12 Atmosphere plates ───────────────────────────────────────

gen "sky_dawn_ayodhya" \
    "Sunrise over ancient Ayodhya city on the Sarayu river, golden pink sky with scattered temple spires silhouetted, mist rising from the river, distant palace domes catching first light, birds in V-formation, atmospheric haze, epic wide shot"

gen "sky_dusk_lanka" \
    "Dusk over the golden city of Lanka, dramatic purple-orange sky, ornate temple towers glowing with internal fire-light, ocean waves in the foreground, Ravana palace silhouetted against burning horizon, ominous but majestic"

gen "sky_night_chitrakuta" \
    "Starry night sky over Chitrakuta hill, full moon rising behind sacred hill, ancient banyan tree in foreground with hanging moss, hermitage smoke rising from a distant ashram, fireflies and constellations, peaceful devotional atmosphere"

gen "sky_monsoon_dandaka" \
    "Monsoon rain over the dense Dandaka forest, dramatic grey-purple storm clouds, lightning strike illuminating ancient trees, mist swirling between trunks, wet leaves glistening, atmospheric rain in shafts of light"

gen "sky_afternoon_kishkindha" \
    "Bright afternoon over rocky Kishkindha monkey kingdom, harsh sunlight on red-rock crags and boulders, blue sky with cumulus clouds, dry forest below, vivid warm tones"

gen "sky_sunset_setu" \
    "Epic sunset over the ocean bridge to Lanka, dramatic orange-red sky reflecting on water, distant Lanka burning on the horizon, painterly panoramic, atmospheric haze"

gen "sky_ashoka_vatika" \
    "Eternal golden afternoon in Ashoka Vatika garden, dense flowering trees, dappled sunlight through leaves, peacocks displaying in middle distance, atmosphere of melancholy and waiting, painterly garden paradise"

gen "sky_janakpur_palace" \
    "Golden morning light over Janakpur palace and city, mist in the valley below the palace, terraced gardens with blooming flowers, ornamental ponds reflecting sky, distant Himalayan foothills"

gen "sky_sarayu_banks" \
    "Sacred Sarayu river at twilight, ghats with oil lamps glowing amber, reflections in still water, floating diyas, atmospheric river mist, painterly devotional calm"

gen "sky_himalayas_dawn" \
    "Dawn over the snow-capped Himalayan peaks, pink alpenglow on white summits, deep blue valleys below with morning mist, sacred Ganges implied in distance, sage hermitages as tiny dots on ridges"

gen "sky_ocean_storm" \
    "Pre-storm sky over the southern ocean, dark indigo clouds building, whitecaps on waves, dramatic horizon, one shaft of golden light breaking through, painterly dramatic ocean"

gen "sky_celestial_heaven" \
    "Mythological celestial realm above the clouds, golden palaces floating among pink clouds, divine chariots as tiny silhouettes, rays of celestial light, painterly devotional grandeur, Hindu mythological cosmology, ethereal"

echo ""
echo "================================================"
echo "DONE. Atmosphere files: $(ls $OUT_DIR/*.png 2>/dev/null | wc -l | tr -d ' ')"
echo "================================================"
