#!/usr/bin/env bash
# Props batch — items, weapons, vehicles for Ramayana PS5 game
# Generated via draw-things-cli (free, unlimited, painterly 1990s style)
# Used as inventory icons, prop sprites, quest items

set -e

DT_CLI="/Users/prabaharan/.local/bin/draw-things-cli"
MODEL="flux_2_klein_4b_q6p.ckpt"
OUT_DIR="/Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/props"
mkdir -p "$OUT_DIR"

PREFIX="hand-painted illustration in the style of a 1990s Indian animated film"
SUFFIX="warm earth tone palette, golden hour cinematic lighting, painterly textures, highly detailed, isolated object on simple warm beige background, item icon, no humans, no text, no signature, no watermark"

NEG="photorealistic, 3D render, CGI, modern video game screenshot, anime cel-shading, chibi, pixel art, low resolution, blurry, jpeg artifacts, watermark, text, logo, busy composition"

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
        --width 768 --height 768 \
        --cfg 4.0 \
        -o "$out" 2>&1 | tail -2
    echo "  -> $out ($(du -h "$out" 2>/dev/null | cut -f1))"
}

# ── 15 PROPS ──────────────────────────────────────────────

gen "bow_kodanda" "divine bow of Rama, ornate golden longbow with intricate carvings, sacred thread wrapped around, divine aura glow"

gen "arrow_brahmastra" "single glowing golden divine arrow, magical aura, golden tip, peacock feather fletching, celestial weapon"

gen "mace_gada" "Hanuman's celestial mace, heavy iron gada with golden knob, ornate handle, divine weapon of vanara warriors"

gen "chariot_rama" "ornate golden war chariot with horses implied, carved wooden frame with red silk curtains, divine wheels, royal vehicle"

gen "crown_rama" "golden royal crown of Ayodhya with embedded rubies and emeralds, tall conical crown, divine princely regalia"

gen "jewelry_set_sita" "ornate Indian bridal jewelry set, gold necklace, earrings, bangles, nose ring, maang tikka, rubies and pearls"

gen "garland_mala" "sacred flower garland of jasmine and roses, traditional Indian wedding garland (jaimala), fragrant devotional"

gen "diyas_oil_lamps" "row of traditional Indian clay oil lamps (diyas) glowing golden, festival of lights atmosphere"

gen "scroll_manuscript" "ancient palm-leaf manuscript bound with golden thread, sacred Hindu text, ornate cover, scholarly"

gen "lotus_throne" "ornate lotus-shaped golden throne, divine seat for deities, painted in red and gold, sacred object"

gen "rudraksha_mala" "sacred prayer bead necklace of rudraksha seeds, brown beads, Hindu spiritual object, traditional"

gen "tilak_paste" "traditional Hindu tilak paste container, small brass pot with sandalwood paste, religious ceremonial object"

gen "kalasha" "sacred copper kalasha pot with mango leaves and coconut on top, Hindu ritual vessel, ornate engravings"

gen "tulsi_plant" "sacred tulsi plant in ornate brass planter, holy basil, Hindu devotional home plant, traditional"

gen "sitar" "classical Indian sitar instrument, ornate carved wood with mother-of-pearl inlay, large gourd resonator, silk strings"

echo ""
echo "================================================"
echo "DONE. Props: $(ls $OUT_DIR/*.png 2>/dev/null | wc -l | tr -d ' ')"
echo "================================================"
