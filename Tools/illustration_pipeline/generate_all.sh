#!/usr/bin/env bash
# generate_all.sh — Batch-run all 200 illustrations for Traders/Ramayana PS5
#
# Usage:
#   ./generate_all.sh                    # Generate ALL illustrations (200+ images, ~2-3 hours)
#   ./generate_all.sh characters heroes   # Only hero characters (6 × 4 poses = 24 images)
#   ./generate_all.sh characters all      # All 50 characters (~150 images)
#   ./generate_all.sh cities             # All 23 cities
#   ./generate_all.sh atmosphere         # All atmosphere plates
#   DRY_RUN=1 ./generate_all.sh          # Show what would be generated without running
#   PARALLEL=2 ./generate_all.sh         # Run 2 generations in parallel
#
# Requires: draw-things-cli at ~/.local/bin, FLUX.2 Klein 4B model downloaded
# See STYLE_PROMPT.md for prompt structure and quality bar.

set -e

DT_CLI="$HOME/.local/bin/draw-things-cli"
OUT_BASE="$HOME/Aerospace_projects/RamayanaPS5/Assets/Illustrations"
STYLE_FILE="$(dirname "$0")/STYLE_PROMPT.md"
ROSTER_FILE="$(dirname "$0")/character_roster.json"

if [ ! -x "$DT_CLI" ]; then
    echo "ERROR: draw-things-cli not found at $DT_CLI"
    echo "Install Draw Things from App Store, then quit and the CLI appears."
    exit 1
fi

mkdir -p "$OUT_BASE"/{characters,cities,props,atmosphere}

# ── STYLE LOCK — same prefix/suffix on every generation ──
STYLE_PREFIX="hand-painted illustration in the style of a 1990s Indian animated film, "
STYLE_SUFFIX=", warm earth tone palette, golden hour cinematic lighting, dramatic rim light, highly detailed, painterly textures, 4K illustration, isolated subject on simple background, saffron indigo vermilion gold palette"

NEG_PROMPT="photorealistic, 3D render, CGI, modern video game screenshot, anime cel-shading, chibi, pixel art, low resolution, blurry, jpeg artifacts, watermark, signature, text, logo, multiple subjects, busy background, modern clothing, watch, glasses, smartphone, gun, airplane, car, motor vehicle, anachronistic objects, pale skin, caucasian features"

# ── Per-character pose + view prompt extensions ──
# These are the "subject" part of the prompt — Segment 2 from STYLE_PROMPT.md
# Format: "Character Description, [pose]"
CHARACTER_PROMPTS=(
    # RAMA — prince of Ayodhya, blue skin tone, divine bow
    "rama|Prince Rama of Ayodhya, divine warrior, blue-tinted skin tone, golden crown, holding divine bow Kodanda, royal armor with saffron and gold"
    "rama_side|Prince Rama of Ayodhya, divine warrior, blue-tinted skin tone, drawing bow in action stance, focused heroic expression"
    "rama_walk|Prince Rama of Ayodhya walking through forest, divine bow on back, blue-tinted skin tone, royal sandals"
    "rama_full|Prince Rama full body, royal divine armor, sun rising behind him, golden aura"
    "rama_battle|Prince Rama fighting Ravana on Lanka battlefield, divine bow drawn, blue-tinted skin tone"

    # SITA — daughter of Earth, lotus-bearing, gentle
    "sita|Sita princess of Mithila, gentle serene expression, holding sacred lotus flower, golden sari with red border"
    "sita_side|Sita seated beneath ashoka tree, contemplative, golden sari with red border"
    "sita_garden|Sita walking in royal garden, golden sari with veil, surrounded by flowers"
    "sita_full|Sita full body in golden sari, tending sacred fire, divine feminine presence"

    # LAKSHMANA — devoted brother, warrior
    "lakshmana|Lakshmana warrior prince, devoted expression, bow ready, green and gold attire"
    "lakshmana_side|Lakshmana scanning horizon alert, bow drawn, warrior stance"
    "lakshmana_full|Lakshmana full body following Rama, watchful guardian stance, green attire"

    # HANUMAN — monkey warrior, devotee of Rama, golden-orange fur
    "hanuman|Hanuman monkey warrior, golden-orange fur, mace in hand, prayer pose in flight over ocean"
    "hanuman_side|Hanuman leaping across ocean to Lanka, golden-orange fur, mountain peak silhouette"
    "hanuman_full|Hanuman monkey warrior full body with mace gada, golden-orange fur, devotee posture"
    "hanuman_battle|Hanuman lifting entire mountain of Sanjeevani herb, monkey form, triumphant"

    # RAVANA — ten-headed demon king of Lanka, dark purple
    "ravana|Ravana ten-headed demon king, twenty arms holding weapons, dark purple armor, golden crown"
    "ravana_side|Ravana profile with all ten heads visible, dark armor, multiple arms with divine weapons"
    "ravana_full|Ravana full body in dark Lanka palace, ten-headed demon king with twenty arms, dark purple"

    # BHARATA — ascetic prince, holds Rama's sandals
    "bharata|Bharata ascetic prince, royal robes of renunciation, holding Rama's golden sandals reverently"
    "bharata_full|Bharata full body in forest hermitage, ascetic garb, royal sandals placed on throne"
)

# ── Generate one image ──
generate_one() {
    local id="$1"
    local subject="$2"
    local category="${3:-characters}"
    local width="${4:-1024}"
    local height="${5:-1024}"
    local steps="${6:-8}"

    local out_path="$OUT_BASE/$category/${id}.png"

    # Build full prompt
    local full_prompt="${STYLE_PREFIX}${subject}${STYLE_SUFFIX}"

    if [ "${DRY_RUN:-0}" = "1" ]; then
        echo "[DRY] would write $out_path (${width}x${height}, ${steps} steps)"
        return 0
    fi

    echo "→ Generating $id ($category, ${width}x${height}, ${steps} steps)..."

    local start_time=$(date +%s)
    if ! "$DT_CLI" generate \
        --model flux_2_klein_4b_q6p.ckpt \
        --prompt "$full_prompt" \
        --negative-prompt "$NEG_PROMPT" \
        --width "$width" --height "$height" \
        --steps "$steps" --cfg 3.5 \
        --output "$out_path" 2>&1 | tail -3; then
        echo "  ✗ FAILED: $id"
        return 1
    fi

    local end_time=$(date +%s)
    local elapsed=$((end_time - start_time))
    local size=$(stat -f%z "$out_path" 2>/dev/null || echo 0)
    echo "  ✓ $id written ($size bytes, ${elapsed}s)"
}

export -f generate_one
export DT_CLI OUT_BASE STYLE_PREFIX STYLE_SUFFIX NEG_PROMPT

# ── Dispatcher ──
generate_heroes() {
    echo "═══════════════════════════════════════════════"
    echo "  HEROES (6 characters × 3-5 poses = 24 images)"
    echo "═══════════════════════════════════════════════"
    for entry in "${CHARACTER_PROMPTS[@]}"; do
        local id="${entry%%|*}"
        local subject="${entry#*|}"
        generate_one "$id" "$subject" "characters" 1024 1024 8
    done
}

generate_all_characters() {
    echo "═══════════════════════════════════════════════"
    echo "  ALL CHARACTERS (50 × ~3 poses = ~150 images)"
    echo "═══════════════════════════════════════════════"
    generate_heroes  # heroes first
    # Future: add supporting cast, sages, rakshasas, vanaras
    # Use character_roster.json for the full list
    echo "(See character_roster.json — extend this function as you generate more)"
}

generate_cities() {
    echo "═══════════════════════════════════════════════"
    echo "  CITIES (23 city environments, 1920×1080)"
    echo "═══════════════════════════════════════════════"
    # City list — one prompt per city
    CITIES=(
        "harappa_dawn|Harappa Indus Valley city at dawn, granary buildings, riverfront, painted pottery market"
        "mohenjo_daro|Mohenjo-daro Indus city, great bath visible, brick architecture, bustling streets"
        "lothal_port|Lothal Indus port city with dockyard, ships in harbor, tidal lock"
        "taxila_university|Taxila ancient university, students in robes, library building, mountain backdrop"
        "pataliputra_capital|Pataliputra Mauryan capital, royal palace, marketplace, lotus ponds"
        "muziris_trade|Muziris Roman-Indian trade port, ships docked, pepper sacks, amphorae"
        "ayodhya_palace|Ayodhya royal palace, golden spires, Ramayana era grandeur, gardens"
        "lanka_golden|Lanka the golden city of Ravana, dark architecture, demon statues, ocean backdrop"
        "kishkindha_rock|Kishkindha monkey kingdom, rocky cliffs, mountain fortresses"
        "panchavati_forest|Panchavati forest hermitage, river Godavari, five fires lit"
        "chitrakuta_hill|Chitrakuta hill hermitage, Rama and Sita dwelling, mountain landscape"
        "janakpur_palace|Janakpur Mithila palace, Sita's home, royal court"
        "nandigram_ashram|Nandigram Bharata's ashram, hermitage, royal sandals"
        "sarayu_riverbank|Sarayu riverbank, funeral pyres, spiritual atmosphere"
        "sarayu_river|Sarayu river, flowing water, ghats, pilgrims"
        "mithila_court|Mithila royal court, King Janaka presiding, ornate decoration"
        "gandhamadana_peak|Gandhamadana mountain peak, monkeys gathering"
        "ashram_atri|Atri's ashram, ancient sage dwelling, sacred fire"
        "kiskindha_cave|Kishkindha cave entrance, dark interior"
        "ravana_court_throne|Ravana's throne room, ten-headed demon king seated"
        "ashok_vatika|Ashoka Vatika, Sita captive, demon guards, sorrowful atmosphere"
        "lanka_palace_interior|Lanka palace interior, dark grandeur, Ravana court"
        "setu_bridge|Setu bridge to Lanka, ocean, monkey army crossing"
    )

    for entry in "${CITIES[@]}"; do
        local id="${entry%%|*}"
        local subject="${entry#*|}"
        # 1920x1088 = 64-multiples (was 1920x1080; 1080 fails the multiple-of-64 constraint)
        generate_one "$id" "$subject" "cities" 1920 1088 12
    done
}

generate_support() {
    echo "═══════════════════════════════════════════════"
    echo "  SUPPORTING CAST (6 supporting characters × 2 poses = 12 images)"
    echo "═══════════════════════════════════════════════"
    SUPPORTING=(
        "vishwamitra|Sage Vishwamitra, ancient Indian sage with white beard, matted dreadlocks piled high, simple ochre robe, prayer beads rudraksha mala, holding sacred fire stick, intense wise eyes, dark brown weathered skin, painted Indian art"
        "vasishtha|Sage Vasishta, oldest of the Saptarishi seven sages, long white flowing beard, calm divine face, white silk dhoti with gold border, kamandalu water pot, sacred ash on forehead"
        "kaikeyi|Queen Kaikeyi of Ayodhya, royal consort with elaborate gold crown and jewelry, dark brown skin, kohl-lined eyes, deep red silk sari with gold zari border, pearl nose ring, regal posture"
        "manthara|Manthara the hunchbacked maid-servant of Kaikeyi, older woman with deformed back, dark skin, sharp cunning eyes, grey hair in messy bun, simple worn cotton sari, malicious expression"
        "jatayu|Jatayu the vulture king, giant ancient eagle with brown and white feathers, golden crown marking on head, fierce protective eyes, magnificent wings spread, painterly bird warrior"
        "kumbhakarna|Kumbhakarna the giant brother of Ravana, massive demon warrior with four arms, dark blue-grey skin, wild mane of black hair, tusks, holding massive mace gada, gold armor"
    )
    for entry in "${SUPPORTING[@]}"; do
        local id="${entry%%|*}"
        local subject="${entry#*|}"
        generate_one "$id" "$subject" "characters" 1024 1024 8
    done
}

generate_atmosphere() {
    echo "═══════════════════════════════════════════════"
    echo "  ATMOSPHERE (12 plates, 1920×1080)"
    echo "═══════════════════════════════════════════════"
    ATMOSPHERE=(
        "sky_dawn_warm|Pre-dawn sky with warm orange horizon, indigo overhead, painted clouds"
        "sky_noon_bright|Bright midday sky with painted clouds, golden haze"
        "sky_dusk_gold|Golden hour sunset, deep orange to indigo gradient"
        "sky_night_stars|Deep night sky with stars and crescent moon, indigo"
        "sky_storm_dark|Storm clouds gathering, dramatic dark grey to purple"
        "fog_dense_white|Thick morning fog, atmospheric depth"
        "particles_saffron_petals|Saffron flower petals floating on wind"
        "particles_dust_motes|Sun-lit dust motes in temple light"
        "particles_torch_smoke|Torch smoke at night, warm orange wisps"
        "particles_burning_embers|Burning embers floating up from sacred fire"
        "monsoon_rain|Heavy monsoon rain falling, dark clouds"
        "golden_aura_divine|Divine golden aura, sun rays through clouds"
    )

    for entry in "${ATMOSPHERE[@]}"; do
        local id="${entry%%|*}"
        local subject="${entry#*|}"
        # 1920x1088 = 64-multiples (was 1920x1080; 1080 fails the multiple-of-64 constraint)
        generate_one "$id" "$subject" "atmosphere" 1920 1088 10
    done
}

# ── Main ──
case "${1:-all}" in
    heroes|characters/hero)
        generate_heroes
        ;;
    characters|characters/all)
        generate_all_characters
        ;;
    support|supporting)
        generate_support
        ;;
    cities|cities/all)
        generate_cities
        ;;
    atmosphere|atmosphere/all)
        generate_atmosphere
        ;;
    all)
        generate_heroes
        generate_cities
        generate_atmosphere
        ;;
    *)
        echo "Unknown target: $1"
        echo "Usage: $0 [heroes|characters|cities|atmosphere|all]"
        exit 1
        ;;
esac

echo
echo "═══════════════════════════════════════════════"
echo "  Done. Output: $OUT_BASE"
ls -la "$OUT_BASE"/*/*.png 2>/dev/null | wc -l | xargs echo "  Total PNGs:"
echo "═══════════════════════════════════════════════"
