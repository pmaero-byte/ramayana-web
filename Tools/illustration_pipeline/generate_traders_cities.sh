#!/usr/bin/env bash
# Traders-of-Jambudweep city plates batch
# Style: 1990s Indian hand-painted animation aesthetic (FLUX.2-Klein-4B)
# Output: 1920×1088 (multiples of 64, FLUX.2-Klein-4B requirement)
#
# Cities include both ancient Indian ones + international trade hubs.
# All 25 cities (some already generated as part of Ramayana batch).

set -e

DT_CLI="/Users/prabaharan/.local/bin/draw-things-cli"
MODEL="flux_2_klein_4b_q6p.ckpt"
OUT_DIR="/Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/cities"
mkdir -p "$OUT_DIR"

# Style prefix/suffix (same as Ramayana character batch — 1990s Indian animation)
PREFIX="hand-painted illustration in the style of a 1990s Indian animated film"
SUFFIX="warm earth tone palette, golden hour cinematic lighting, dramatic rim light, highly detailed painterly textures, 4K illustration, panoramic landscape, isolated on simple warm atmospheric haze, no humans in foreground, no text, no signature, no watermark"

NEG="photorealistic, 3D render, CGI, modern video game screenshot, anime cel-shading, chibi, pixel art, low resolution, blurry, jpeg artifacts, watermark, text, logo, modern clothing, anachronistic objects, busy composition, multiple focal points"

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

# ── 25 Traders cities (most already exist; only the missing ones will run) ─────

# INDUS VALLEY (5) - some already generated
gen "mehrgarh" \
    "Mehrgarh ancient Neolithic settlement, mud brick houses on terraced hillside, early agriculture, pottery kilns smoking, golden sunset, Balochistan mountains, panoramic"

gen "daimabad" \
    "Daimabad bronze-age site, chalcolithic village with thatched huts, central fire pit, terracotta pottery, surrounded by Deccan plateau hills at dawn, painterly"

# MAURYAN ERA (2)
gen "sanchi" \
    "Sanchi great stupa at golden hour, ancient Buddhist monument with carved toranas, sandstone in warm pink-orange light, hilltop setting with forested plains below, panoramic"

gen "nalanda" \
    "Ancient Nalanda university monastery at sunset, multi-storied red brick cells, students walking between buildings, central stupa, mango groves, panoramic painterly"

# GUPTA ERA (1)
gen "ajanta" \
    "Ajanta caves carved into horseshoe cliff, golden sunset illuminating cave entrances, lush green valley below with river, ancient Buddhist monastery, panoramic"

# CHOLA ERA (1)
gen "ariyankuppam" \
    "Arikamedu Roman-Indian trading port at dawn, stone warehouses with amphorae, Tamil-Roman merchants on dock, French-style colonial watchtower in distance, Bay of Bengal, panoramic"

# FOREIGN TRADE CITIES (10) — most have never been generated for any game
gen "mesopotamia" \
    "Ancient Mesopotamian city Ur with ziggurat towering over mud-brick houses, palm trees, two rivers converging, golden hour light, biblical atmosphere, panoramic"

gen "dilmun" \
    "Ancient Dilmun island of Bahrain in Persian Gulf, palm-fringed oasis, pearl-diving boats on turquoise water, simple stone buildings, golden sunset, panoramic"

gen "oman_magan" \
    "Ancient Magan kingdom in Oman, copper smelting furnaces smoking on rocky coastline, frankincense trees, Bronze Age ships in harbor, harsh desert sun, panoramic"

gen "rome" \
    "Ancient Rome imperial city panorama, Colosseum in foreground, forum temples beyond, Tiber river, marble columns, golden sunset sky, epic panoramic, no specific people"

gen "alexandria" \
    "Ancient Alexandria Egypt with the great Lighthouse Pharos towering over harbor, library buildings, Mediterranean ships, scholars walking on marble streets, golden afternoon, panoramic"

gen "persepolis" \
    "Persepolis ceremonial capital of Persia, towering stone columns and staircases with carved reliefs, apadana terrace, Persian guards as silhouettes, dramatic sunset, panoramic"

gen "changan" \
    "Ancient Tang dynasty Chinese capital Chang'an at twilight, pagoda towers, wide imperial avenues, bustling market rooftops, distant mountains, atmospheric haze, panoramic"

gen "kathmandu" \
    "Ancient Kathmandu valley with wooden Newari temples, prayer flags on hilltops, snow-capped Himalayas in distance, golden afternoon light, painterly Himalayan kingdom, panoramic"

gen "venice" \
    "Medieval Venice trading port, Byzantine-style domes and bell towers, gondolas in lagoon, ornate marble palaces, soft golden light on water, panoramic"

gen "aden" \
    "Medieval Aden port city, dramatic volcanic crater backdrop, stone warehouses and minarets, frankincense and spice ships in harbor, Arabian Sea, hot afternoon, panoramic"

# LATER CITIES (4)
gen "kashgar" \
    "Silk Road oasis city Kashgar at dawn, mud-brick mosques and bazaar, camel caravans resting in courtyard, Pamir mountains beyond, atmospheric Central Asian architecture, panoramic"

gen "kilwa" \
    "Medieval Kilwa Kisiwani island trading port off East African coast, coral stone buildings and Great Mosque, Indian Ocean dhows, palm trees, hot afternoon light, panoramic"

gen "great_zimbabwe" \
    "Great Zimbabwe stone-walled city, massive curving dry-stone walls, conical tower, golden granite hills, savanna grass, dramatic African sunset, panoramic"

gen "tenochtitlan" \
    "Aztec Tenochtitlan capital on lake, stepped pyramid temples, canoes on canal, tropical jungle mountains, dramatic sunset sky, panoramic"

gen "kandy" \
    "Ancient Kandy Sri Lanka with golden-roofed Temple of the Tooth, surrounded by misty green hills and tropical forest, sacred lake, dawn light, panoramic"

echo ""
echo "================================================"
echo "DONE. Cities in $OUT_DIR: $(ls $OUT_DIR/*.png 2>/dev/null | wc -l | tr -d ' ')"
echo "================================================"
