#!/usr/bin/env bash
# HERMES: Sequential generation of remaining 5 missing Ramayana portraits
set -e
OUT="/Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/characters"
cd /Users/prabaharan/Aerospace_projects/RamayanaPS5

gen() {
  local key="$1"; local prompt="$2"
  if [[ -f "$OUT/${key}.png" ]]; then
    echo "  → $key: exists, skip"
    return 0
  fi
  echo "→ $key"
  mmx image generate \
    --prompt "$prompt" \
    --aspect-ratio "1:1" \
    --width 512 --height 512 \
    --out "$OUT/${key}.png" \
    --quiet 2>&1 | tail -1
  if [[ -f "$OUT/${key}.png" ]]; then
    echo "   ✓ $(ls -lh $OUT/${key}.png | awk '{print $5}')"
  else
    echo "   ✗ failed"
  fi
}

# Faster: smaller 512x512, no subject-ref
gen "valmiki" "Photorealistic portrait of Sage Valmiki, ancient Indian poet-sage and author of the Ramayana, deep brown weathered skin, flowing white matted hair piled in topknot, gentle wise eyes, white sacred beard, wearing simple ochre bark cloth, rudraksha mala beads around neck, fire-lit forest ashram background, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"
gen "janaka" "Photorealistic portrait of King Janaka of Mithila, Sita's father, elderly Indian philosopher-king with dignified posture, kind contemplative eyes, gray beard trimmed neatly, wearing regal saffron and gold royal attire, jeweled crown with emerald, royal court background, soft golden lighting, painterly cinematic Indian television aesthetic, square portrait, ultra-detailed"
gen "shatrughna" "Photorealistic portrait of Shatrughna, youngest brother of Lord Rama, young Indian warrior prince in early twenties, deep brown skin, intense loyal eyes, short dark hair with tilak mark, wearing royal saffron warrior attire, gold-bordered dhoti, warrior's armband, ready for battle yet gentle smile, painterly cinematic Indian television aesthetic, square portrait, ultra-detailed"
gen "shabari" "Photorealistic portrait of Shabari, the elderly ascetic devotee of Lord Rama, old Indian tribal woman with deep weathered brown skin, joyful blissful smile, white thinning hair tied loosely, wearing simple worn white cotton sari with faded ochre border, humble yet radiant, forest hermitage background, soft golden fire-lit ambient, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"
gen "sugriva" "Photorealistic portrait of Sugriva, the exiled monkey king of Kishkindha and ally of Rama, young Indian-faced monkey king with light golden-brown monkey features, hopeful loyal eyes, wearing gold crown slightly askew from exile, simple forest-king attire, gem-studded armband, contemplative regal yet humble expression, painterly cinematic Indian television aesthetic, square portrait, ultra-detailed"

echo ""
echo "=== Final batch8 status ==="
for c in dasaratha jambavan trijata valmiki janaka shatrughna shabari sugriva; do
  if [[ -f "$OUT/${c}.png" ]]; then
    echo "  ✓ $c: $(ls -lh $OUT/${c}.png | awk '{print $5}')"
  else
    echo "  ✗ $c: MISSING"
  fi
done