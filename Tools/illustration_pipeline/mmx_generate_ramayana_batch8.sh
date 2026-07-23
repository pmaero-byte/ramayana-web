#!/usr/bin/env bash
# HERMES: Generate 8 missing Ramayana character portraits via mmx
# Subjects: Dasharatha (King of Ayodhya), Jambavan (Bear king), Trijata (demon nurse),
#           Valmiki (poet-sage), Janaka (King of Mithila), Shatrughna (Rama's brother),
#           Shabari (ascetic woman), Sugriva (monkey king ally)
# All generated photorealistic + Indian features + dark skin + cultural markers
set -e
OUT="/Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/characters"
mkdir -p "$OUT"
echo "=== HERMES Ramayana batch8: 8 missing major speakers ==="

# HERMES: Helper that pulls a portrait seed (use rama.png for visual cohesion).
# 8 speakers total. Each ~5-15s. Total ~80s.

gen() {
  local key="$1"; local prompt="$2"
  echo "→ $key"
  mmx image generate \
    --prompt "$prompt" \
    --subject-ref "type=character,image=$OUT/rama.png" \
    --aspect-ratio "1:1" \
    --width 768 --height 768 \
    --out "$OUT/${key}.png" \
    --quiet 2>&1 | tail -1
  if [[ -f "$OUT/${key}.png" ]]; then
    echo "   ✓ saved $(ls -lh $OUT/${key}.png | awk '{print $5}')"
  else
    echo "   ✗ failed"
  fi
}

# 1. Dasharatha — King of Ayodhya, regal patriarch, white beard, gold crown
gen "dasaratha" "Photorealistic portrait of King Dasharatha of Ayodhya, elderly Indian king with dignified white beard and mustache, kind wise eyes, deep brown skin, wearing elaborate gold crown with ruby inlays, royal maroon silk robes with gold thread, royal jewelry, throne room lighting, soft golden ambient, painterly cinematic style, 1990s Indian television drama aesthetic, full color, square portrait, ultra-detailed face"

# 2. Jambavan — Bear king, wise ancient, golden-brown fur, wise eyes
gen "jambavan" "Photorealistic portrait of Jambavan, ancient wise bear king of the Ramayana, mature anthropomorphic bear with golden-brown fur, deep wise eyes, silver-streaked mane, gold-beaded necklaces, sacred thread across chest, royal forest guardian demeanor, warm umber lighting, painterly cinematic Indian television aesthetic, square portrait, ultra-detailed"

# 3. Trijata — Demon nurse, gentle loyal demoness, soft features
gen "trijata" "Photorealistic portrait of Trijata, the gentle demoness nurse of Lanka from the Ramayana, young Indian woman with deep brown skin, kind compassionate eyes, dark wavy hair with jasmine flowers, wearing elegant crimson and gold demon-kingdom silk, pearl necklace, subtle mystical aura, soft ambient glow, painterly cinematic style, square portrait, ultra-detailed face"

# 4. Valmiki — Poet-sage, ancient rishi with matted hair
gen "valmiki" "Photorealistic portrait of Sage Valmiki, ancient Indian poet-sage and author of the Ramayana, deep brown weathered skin, flowing white matted hair piled in topknot, gentle wise eyes, white sacred beard, wearing simple ochre bark cloth, rudraksha mala beads around neck, fire-lit forest ashram background, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"

# 5. Janaka — King of Mithila, philosopher-king, regal
gen "janaka" "Photorealistic portrait of King Janaka of Mithila, Sita's father, elderly Indian philosopher-king with dignified posture, kind contemplative eyes, gray beard trimmed neatly, wearing regal saffron and gold royal attire, jeweled crown with emerald, royal court background, soft golden lighting, painterly cinematic Indian television aesthetic, square portrait, ultra-detailed"

# 6. Shatrughna — Rama's youngest brother, warrior
gen "shatrughna" "Photorealistic portrait of Shatrughna, youngest brother of Lord Rama, young Indian warrior prince in early twenties, deep brown skin, intense loyal eyes, short dark hair with tilak mark, wearing royal saffron warrior attire, gold-bordered dhoti, warrior's armband, ready for battle yet gentle smile, painterly cinematic Indian television aesthetic, square portrait, ultra-detailed"

# 7. Shabari — Ascetic woman, elderly devotee of Rama, smile of bliss
gen "shabari" "Photorealistic portrait of Shabari, the elderly ascetic devotee of Lord Rama, old Indian tribal woman with deep weathered brown skin, joyful blissful smile, white thinning hair tied loosely, wearing simple worn white cotton sari with faded ochre border, humble yet radiant, forest hermitage background, soft golden fire-lit ambient, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"

# 8. Sugriva — Monkey king ally, exiled, hopeful
gen "sugriva" "Photorealistic portrait of Sugriva, the exiled monkey king of Kishkindha and ally of Rama, young Indian-faced monkey king with light golden-brown monkey features, hopeful loyal eyes, wearing gold crown slightly askew from exile, simple forest-king attire, gem-studded armband, contemplative regal yet humble expression, painterly cinematic Indian television aesthetic, square portrait, ultra-detailed"

echo ""
echo "=== Generation complete ==="
ls -lh "$OUT"/{dasaratha,jambavan,trijata,valmiki,janaka,shatrughna,shabari,sugriva}.png 2>/dev/null