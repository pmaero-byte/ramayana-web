#!/usr/bin/env bash
# HERMES: Round 8 second wave — 9 more Ramayana character portraits
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

# Major speakers not yet mapped
gen "vibhishana" "Photorealistic portrait of Vibhishana, the pious younger brother of Ravana who joins Lord Rama, middle-aged Indian man with deep brown skin, kind devout eyes, gray-streaked beard, wearing royal demon-kingdom attire in white and gold (signifying his virtue), crown with single emerald, sacred tilak on forehead, contemplative loyal expression, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"
gen "tara" "Photorealistic portrait of Tara, wife of Vali the monkey king, beautiful Indian woman with deep brown skin, gentle wise eyes, long dark hair in braid with flowers, wearing elegant golden forest-queen attire, soft contemplative expression of intelligence and grace, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"
gen "sampati" "Photorealistic portrait of Sampati, the aged vulture king and elder brother of Jatayu, elderly anthropomorphic vulture with deep wise eyes, weathered plumage, royal forest dignity, gold-beaded sacred thread across chest, regal yet ancient appearance, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"
gen "tataka" "Photorealistic portrait of Tataka, the fierce demoness warrior woman of the Ramayana, fierce beautiful Indian woman with intense battle-ready eyes, wild flowing dark hair, ornate demon-queen battle armor in crimson and black, gold war-jewelry, fearsome yet regal, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"
gen "ahalya" "Photorealistic portrait of Ahalya, the beautiful sage's wife of the Ramayana, divine Indian woman with luminous skin, gentle pure eyes, long dark hair loose flowing, wearing simple white silk sari with gold border, subtle ethereal glow, expression of forgiveness and grace, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"
gen "kabandha" "Photorealistic portrait of Kabandha, the headless demon of the Ramayana, large anthropomorphic Indian warrior without head, massive torso with single fierce eye on chest, wild hair growing from belly-mouth, gold war-bracelets, powerful intimidating yet redeemable presence, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"
gen "urmila" "Photorealistic portrait of Urmila, wife of Lakshmana, gentle Indian princess with deep brown skin, kind patient eyes, long dark braided hair with jasmine flowers, wearing elegant royal princess silk in pale green and gold, royal but humble demeanor, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"
gen "guha" "Photorealistic portrait of Guha, the loyal tribal king of the forest Nishada people who befriends Rama, strong middle-aged Indian chieftain with deep brown weathered skin, loyal trustworthy eyes, short dark hair with feathered crown, wearing simple tribal warrior attire with gold armbands, kind yet fierce, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"
gen "anasuya" "Photorealistic portrait of Anasuya, the devoted wife of Sage Atri and friend of Sita, mature Indian woman with deep brown skin, serene wise eyes, white-streaked hair in simple bun, wearing simple ochre cotton sari of forest hermitage, gentle maternal expression, painterly cinematic Indian aesthetic, square portrait, ultra-detailed"

echo ""
echo "=== Final R8 wave 2 status ==="
for c in vibhishana tara sampati tataka ahalya kabandha urmila guha anasuya; do
  if [[ -f "$OUT/${c}.png" ]]; then
    echo "  ✓ $c: $(ls -lh $OUT/${c}.png | awk '{print $5}')"
  else
    echo "  ✗ $c: MISSING"
  fi
done