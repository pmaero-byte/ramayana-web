#!/usr/bin/env bash
# HERMES Round 8: Generate 7 missing Ramayana character plates via mmx
# Missing: angada, indrajit, kausalya, mandodari, shurpanakha, sumitra, vali
# Also regenerates 'rich' versions of major chars with stronger cultural detail

set -uo pipefail

OUT_DIR="/Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/characters"
mkdir -p "$OUT_DIR"

STYLE_SUFFIX=", painterly illustration in the style of 1993 Ramanand Sagar Ramayana TV series, dramatic cinematic lighting, warm earth tone palette, golden hour, highly detailed, dark rim light, isolated subject on simple warm beige background, saffron indigo vermilion gold palette, 4K, museum-quality portrait"

NEG_PROMPT="photorealistic, 3D render, CGI, anime, chibi, modern clothing, watch, glasses, smartphone, gun, car, anachronistic, pale skin, caucasian features, busy background, multiple subjects, watermark, signature, text, logo, low quality, blurry"

gen() {
    local id="$1"; local prompt="$2"
    local out="$OUT_DIR/${id}.png"
    if [ -f "$out" ] && [ -s "$out" ]; then
        echo "SKIP: $id (exists, $(du -h "$out" | cut -f1))"
        return
    fi
    echo "[$id] generating..."
    mmx image generate \
        --prompt "${prompt}${STYLE_SUFFIX}" \
        --negative-prompt "$NEG_PROMPT" \
        --aspect-ratio "1:1" \
        --width 1024 --height 1024 \
        --prompt-optimizer \
        --out "$out" \
        2>&1 | grep -E "(saved|error|Error|Generated)" | head -3 || true
    sleep 2
}

# ── 7 Missing Characters ─────────────────────────────────────────────

# 1. ANGADA — son of Vali, monkey warrior, sent as Rama's envoy to Ravana
gen "angada" \
    "Angry young monkey warrior Angada, son of Vali, muscular Vaanar prince with determined fierce expression, golden-brown fur, wearing ornate gold chest plate and tiger-skin loincloth with saffron border, holding a massive iron mace across his shoulders, red tilak on forehead, golden sunset background"

# 2. INDRAJIT — Ravana's son, most powerful warrior, invincible in magical warfare
gen "indrajit" \
    "Fearsome demon prince Indrajit (Meghnad), Ravana's eldest son, dark indigo skin, glowing red eyes, wearing black armor inlaid with rubies, golden crown with 9 serpent hoods, holding the legendary bow Vaishnavastra, magical energy swirling around him, dark dramatic background with red lightning"

# 3. KAUSALYA — Rama's mother, queen of Ayodhya, personification of maternal virtue
gen "kausalya" \
    "Queen Kausalya of Ayodhya, gentle dignified elderly Indian woman with silver-streaked hair in elaborate braid, wearing a saffron silk sari with heavy gold zari border, gold nose ring and ear ornaments, large bindi, modest kind smile, sitting on a wooden throne with carved lotus motifs, warm golden palace interior"

# 4. MANDODARI — Ravana's noble wife, often considered the most virtuous of Lanka
gen "mandodari" \
    "Queen Mandodari of Lanka, dignified Indian woman with dark skin and sorrowful wise eyes, wearing red and gold silk sari with intricate peacock embroidery, gold waist belt with ruby stones, jasmine flowers in braided hair, gold forehead ornaments, lotus in hand, soft moonlight background"

# 5. SHURPANAKHA — Ravana's sister, the catalyst of the war
gen "shurpanakha" \
    "Shurpanakha, Ravana's fierce Rakshasi sister, dark indigo skin with bright golden eyes, fang-tipped smile, wild flowing hair, wearing bone jewelry and a leopard-skin sash, sharp claws, holding a curved demon blade, dramatic jungle of Dandaka forest at dusk"

# 6. SUMITRA — Lakshmana and Shatrughna's mother, wise counselor queen
gen "sumitra" \
    "Queen Sumitra of Ayodhya, contemplative middle-aged Indian woman with calm serene expression, wearing a deep blue silk sari with silver zari border, simple gold jewelry and sacred rudraksha beads, hair tied back with jasmine, soft warm palace chamber with oil lamp"

# 7. VALI — Monkey King of Kishkindha, mighty warrior, killed by Rama
gen "vali" \
    "King Vali of Kishkindha, mighty Vaanar monarch with thick golden-brown fur and powerful muscular build, magnificent golden crown with jewels, ornate gold chest armor, holding a massive iron mace at his side, regal bearing but also prideful expression, dramatic sunset over mountain peaks"

echo ""
echo "============================================="
echo "DONE. Characters in $OUT_DIR:"
ls -1 "$OUT_DIR" | wc -l
echo "============================================="