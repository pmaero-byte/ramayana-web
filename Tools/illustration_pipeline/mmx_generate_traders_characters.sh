#!/usr/bin/env bash
# Generate 13 Traders of Jambudweep character portraits via mmx (MiniMax image-01)
# Subject-ref consistency across poses is OPTIONAL — first pass just gets a strong
# portrait per character with consistent historical/cultural context.
#
# Style: painterly, cinematic, warm earth tones — same aesthetic as Ramayana PS5
# Era-appropriate costumes, no modern clothing.

set -e

OUT_DIR="/Users/prabaharan/Aerospace_projects/Traders/illustrations/characters"
mkdir -p "$OUT_DIR"

STYLE_SUFFIX=", painterly illustration, dramatic cinematic lighting, warm earth tone palette, golden hour, highly detailed, dark rim light, isolated subject on simple warm beige background, saffron indigo vermilion gold palette, 4K, museum-quality portrait"

NEG_PROMPT="photorealistic, 3D render, CGI, anime, chibi, modern clothing, watch, glasses, smartphone, gun, car, anachronistic, pale skin, caucasian features, busy background, multiple subjects, watermark, signature, text, logo, low quality, blurry"

gen() {
    local id="$1"; local prompt="$2"; local era="$3"
    local out="$OUT_DIR/${id}.jpg"
    if [ -f "$out" ]; then
        echo "SKIP: $id (exists)"
        return
    fi
    echo "[$id] ($era) generating..."
    mmx image generate \
        --prompt "${prompt}${STYLE_SUFFIX}" \
        --negative-prompt "$NEG_PROMPT" \
        --aspect-ratio "1:1" \
        --width 1024 --height 1024 \
        --prompt-optimizer \
        --out "$out" \
        --quiet 2>&1 | grep -E "(saved|error|Error)" || true
    sleep 1  # rate limit politeness
}

# ── 13 Historical Figures ─────────────────────────────────────────

# 1. INDUS ERA — Lothal Merchant (~2000 BCE)
gen "lothal_merchant" \
    "Ancient Indus Valley merchant at the great dock of Lothal, mature bearded man wearing a white cotton dhoti with red border, terracotta and gold beaded necklace, lapis lazuli pendant, strong weathered hands holding a carnelian bead trade ledger, standing beside stacked clay amphorae of oil, golden hour sunlight, dignified smile" \
    "indus"

# 2. INDUS ERA — Priya the Bead-Maker
gen "harappa_craftswoman" \
    "Ancient Indus Valley craftswoman in her bead workshop, young woman with strong hands and intense focus, wearing a simple red cotton sari with gold border, copper bangles, hair in oiled braid, holding a glowing heated carnelian stone with metal tongs over an open kiln fire, sparks rising, perspiration on brow, determined expression" \
    "indus"

# 3. VEDIC ERA — Rishi Vashishtha
gen "vedic_scholar" \
    "Vedic scholar Rishi Vashishtha, elderly sage with flowing white beard, topknot with sacred thread, wearing deer skin and simple ochre robes, holding sacred manuscripts and a wooden staff, sacred fire (dhuni) burning beside him, Himalayan foothills in soft focus, golden dawn light" \
    "vedic"

# 4. MAURYAN ERA — Ashoka's Diplomat (Devanampriya)
gen "ashoka_diplomat" \
    "Mauryan Empire royal diplomat, dignified Indian man in his 40s wearing ornate silk dhoti and uttariyam shoulder cloth in saffron and royal blue, gold torque necklace, pearl earrings, embroidered royal sash, holding a rolled diplomatic scroll, standing in a pillar hall of Pataliputra palace, warm torchlight" \
    "mauryan"

# 5. GUPTA ERA — Aryabhata
gen "gupta_mathematician" \
    "Gupta era mathematician Aryabhata, brilliant young Indian scholar in his 30s, wearing simple white cotton robes with sacred thread, clean-shaven head with tuft, holding a copper astrolabe and mathematical tablet, surrounded by floating astronomical instruments, observatory rooftop at dusk, stars visible" \
    "gupta"

# 6. CHOLA ERA — Karunakara Tondaiman (Naval Admiral)
gen "chola_admiral" \
    "Chola dynasty naval admiral Karunakara Tondaiman, powerful Tamil warrior in his prime, dark bronze skin, wearing ornate Chola-era golden armor with temple motifs, pearl and ruby necklaces, tall conical crown, holding a curved naval sword (val), standing on the deck of a Chola warship, dramatic stormy ocean backdrop, lightning" \
    "chola"

# 7. CHOLA ERA — Raja Raja Nambi (Sculptor)
gen "chola_sculptor" \
    "Chola era master sculptor Raja Raja Nambi, lean elderly Tamil artisan with dust-covered hands, wearing simple white veshti, sacred thread, focused intense gaze, holding a chisel and mallet beside a half-finished bronze Nataraja sculpture, workshop lit by oil lamps, stone dust in air" \
    "chola"

# 8. MUGHAL ERA — Hira Meena (Jeweler)
gen "mughal_jeweler" \
    "Mughal era court jeweler Hira Meena, elegant Indian woman in her 30s, wearing rich emerald green silk anarkali with gold zari embroidery, jhumka earrings, maang tikka, multiple gold bangles, examining a magnificent ruby under loupe with focused precision, jewel merchant stall with diamond necklaces displayed, candlelight" \
    "mughal"

# 9. MUGHAL ERA — Ustad Ahmad Lahori (Architect)
gen "mughal_architect" \
    "Mughal era chief architect Ustad Ahmad Lahori, dignified bearded scholar in his 50s wearing white jama with gold embroidered vest, turban with jeweled sarpech, holding architectural drawings and brass compass, standing beside a half-built marble monument, blue sky with morning light" \
    "mughal"

# 10. BRITISH ERA — Jamsetji Tata
gen "colonial_merchant" \
    "Late 19th century Indian industrialist Jamsetji Tata, distinguished Parsi gentleman with greying beard, wearing Victorian-era black achkan coat with gold buttons, white dhoti underneath, pearl stickpin in cravat, pocket watch chain, standing proudly beside industrial machinery, warm gaslight illumination, dignified expression" \
    "british"

# 11. BRITISH ERA — Rani Lakshmibai
gen "colonial_freedom_fighter" \
    "Rani Lakshmibai of Jhansi, fierce young Indian queen warrior in her 20s, riding bareback on a powerful black horse rearing up, wearing saffron sari battle-dress with gold zari, traditional gold jewelry, sword raised high, fire and battlefield smoke behind, lightning sky, defiant warrior expression" \
    "british"

# 12. MODERN ERA — Arjun Sharma (Tech Founder)
gen "tech_founder" \
    "Contemporary Indian tech entrepreneur Arjun Sharma in his 30s, modern professional wearing a crisp navy blue shirt with rolled sleeves, light stubble, confident approachable smile, holding a tablet, standing in a sunlit Bangalore-style modern office with floor-to-ceiling windows, Indian plants, warm wood accents" \
    "modern"

# 13. MODERN ERA — Dr. M.S. Swaminathan
gen "modern_farmer" \
    "Elderly Indian agricultural scientist Dr. M.S. Swaminathan, gentle grandfatherly face, white hair, wearing simple white cotton kurta and dhoti, holding a stalk of golden wheat with reverence, standing in an experimental wheat field at harvest time, golden afternoon sunlight, peaceful expression" \
    "modern"

echo ""
echo "================================================"
echo "DONE. Generated: $(ls $OUT_DIR/*.jpg 2>/dev/null | wc -l | tr -d ' ') files"
echo "================================================"
