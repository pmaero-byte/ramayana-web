#!/usr/bin/env bash
# Story Moment illustrations — 10 key narrative scenes for Ramayana PS5 game
# Generated via mmx (MiniMax image-01) — fast, quota-favorable, photorealistic
# Will be used as in-game dialogue backgrounds, gallery unlocks, loading screens.

set -e

OUT_DIR="/Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/story_moments"
mkdir -p "$OUT_DIR"

STYLE_SUFFIX=", painterly illustration, dramatic cinematic lighting, warm earth tone palette, golden hour, highly detailed, dark rim light, atmospheric haze, 4K, no watermark, no signature, no text, no logo, isolated scene"

NEG="photorealistic, 3D render, CGI, anime cel-shading, chibi, modern clothing, watch, glasses, smartphone, gun, car, anachronistic, pale skin, caucasian features, busy composition, multiple focal points, low quality, blurry"

gen() {
    local id="$1"; local prompt="$2"
    local out="$OUT_DIR/${id}.jpg"
    if [ -f "$out" ]; then
        echo "SKIP: $id (exists)"
        return
    fi
    echo "[$id] generating..."
    mmx image generate \
        --prompt "${prompt}${STYLE_SUFFIX}" \
        --negative-prompt "$NEG" \
        --aspect-ratio "16:9" \
        --width 1920 --height 1080 \
        --prompt-optimizer \
        --out "$out" \
        --quiet 2>&1 | grep -E "(saved|error|Error)" || true
    sleep 2
}

# ── 10 KEY STORY MOMENTS ─────────────────────────────────────

gen "01_sita_swayamvar" \
    "Sita Swayamvar ceremony in Mithila court, the divine bow of Shiva placed in the center, King Janaka on golden throne, princes attempting to lift the bow, sage Vyasa witnessing, ornate palace hall with marble pillars and oil lamps"

gen "02_rama_breaks_bow" \
    "The dramatic moment Prince Rama lifts and strings the divine bow of Shiva in Mithila court, the bow snapping with explosive force, all courtiers and princes gasping in shock, King Janaka rising from throne, dramatic backlight"

gen "03_forest_exile" \
    "Prince Rama, Sita and Lakshmana beginning their 14-year forest exile, walking down a dusty road from Ayodhya, palace spires visible behind them in haze, the city receding, dawn light, emotional farewell scene"

gen "04_sita_in_ashoka_vatika" \
    "Sita captive in Ashoka Vatika garden in Lanka, sitting beneath a flowering ashoka tree, rakshasa guards lurking in shadows at edges, golden twilight filtering through leaves, melancholy but defiant expression"

gen "05_hanuman_meets_rama" \
    "First meeting of Hanuman and Rama in Kishkindha forest, Hanuman in monkey form bowing before Rama in sage-like garb, Lakshmana standing behind, dramatic golden forest light, sacred moment"

gen "06_setu_bandhan" \
    "Epic construction of Rama's bridge to Lanka, vanara army of millions carrying rocks and building across the ocean, divine help from sea god, panoramic vista, sunset sky reflecting on water, painterly"

gen "07_rama_meets_ravana_battle" \
    "Climactic battle between Rama and Ravana on Lanka battlefield, Rama in divine armor with bow drawn, ten-headed Ravana on chariot wielding celestial weapons, lightning sky, dramatic painterly composition"

gen "08_rama_coronation" \
    "The glorious return and coronation of Rama in Ayodhya after 14 years, Rama and Sita on golden throne, thousands of citizens and vanaras celebrating, diyas and flower garlands everywhere, divine golden light"

gen "09_sita_agnipariksha" \
    "Sita's trial by fire, walking into flames to prove her purity, goddess Agni emerging from the fire holding her, audience of gods and sages watching from clouds, dramatic divine light"

gen "10_rama_darshana" \
    "The eternal cosmic Rama darshana vision, Rama revealed as divine cosmic form with multiple arms holding weapons, surrounded by celestial beings and gods bowing, cosmic golden light, devotional Hindu spiritual art"

echo ""
echo "================================================"
echo "DONE. Story moments: $(ls $OUT_DIR/*.jpg 2>/dev/null | wc -l | tr -d ' ')"
echo "================================================"
