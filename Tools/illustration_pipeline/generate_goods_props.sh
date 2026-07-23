#!/usr/bin/env bash
# HERMES: Generate 15 goods props (trader items) using local FLUX.2-Klein-4B
# Output: Assets/Illustrations/props/{NN}_{slug}.png
# Style: 1990s Indian hand-painted animation, painterly, single object, transparent bg

set -uo pipefail
DRAW=/Users/prabaharan/.local/bin/draw-things-cli
OUT=/Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/props
# HERMES: use the exact downloaded model id (run `draw-things-cli models list --downloaded-only` to confirm)
MODEL=flux_2_klein_4b_q6p.ckpt

mkdir -p "$OUT"

# Format: "slug|positive|negative"
ITEMS=(
  "terracotta_horse|An ancient painted terracotta horse figurine, Indus Valley style, earth-tone ceramic with geometric black and red painted patterns, museum artifact on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, plastic, cartoon, 3d render, photograph, text, watermark"
  "silk_saree|A folded Indian silk saree in deep saffron and maroon with gold zari border, intricate paisley weave, draped elegantly on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, polyester, photograph, model, mannequin, text, watermark"
  "pearl_necklace|A strand of luminous Indian ocean pearls with gold clasp, ancient Bharuch pearl necklace, draped on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern jewelry, plastic beads, photograph, model, text, watermark"
  "cardamom_pods|A small pile of fresh green cardamom pods with brown seeds visible, Kerala spice, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern packaging, jar, photograph, text, watermark"
  "iron_sword|An ancient Indian wootz steel sword with ornate silver handle, Damascus-patterned blade, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, plastic, toy, photograph, text, watermark"
  "cinnabar_ore|A chunk of red cinnabar ore with crystalline surface, mercury-bearing mineral, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, polished, photograph, jewelry, text, watermark"
  "cotton_bale|A traditional Indian cotton bale wrapped in jute sacking, soft white fibers visible at edges, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, plastic wrap, machinery, photograph, text, watermark"
  "ivory_bangle|A carved ivory bangle with delicate floral engravings, antique Indian bracelet, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, plastic, gold, photograph, text, watermark"
  "clay_urn|An ancient Indus Valley black-on-red painted clay urn with geometric patterns and peaked rim, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, plastic, photograph, text, watermark"
  "lead_ingot|A rectangular ancient lead ingot with stamped merchant mark, Roman-era Indian export ingot, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, machine cut, photograph, text, watermark"
  "iron_pickaxe|A simple ancient iron pickaxe with wooden handle, mining tool, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, plastic, power tool, photograph, text, watermark"
  "jade_pendant|A polished green jade pendant carved with lotus motif, Indian Mughal-era jewelry, gold filigree bail, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, plastic, costume jewelry, photograph, text, watermark"
  "muslin_scarf|A length of finely woven Dhaka muslin, so fine it can pass through a ring, white with delicate woven border, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, polyester, photograph, mannequin, text, watermark"
  "dried_pepper|A pile of dried black peppercorns from Malabar coast, dark brown wrinkled berries, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern packaging, jar, photograph, text, watermark"
  "sewing_needle|An ancient bone sewing needle with carved eye, Indus Valley craft tool, on plain background, 1990s Indian hand-painted animation aesthetic, soft watercolor edges, painterly texture, single centered object|modern, metal, plastic, machine, photograph, text, watermark"
)

for entry in "${ITEMS[@]}"; do
  IFS='|' read -r slug positive negative <<< "$entry"
  OUTFILE="$OUT/${slug}.png"

  if [ -f "$OUTFILE" ] && [ -s "$OUTFILE" ]; then
    echo "[skip] $slug already exists"
    continue
  fi

  echo "[$(date +%H:%M:%S)] [$slug] generating..."

  "$DRAW" generate \
    --model "$MODEL" \
    --prompt "$positive" \
    --negative-prompt "$negative" \
    --steps 8 \
    --cfg 1.0 \
    --width 1024 \
    --height 1024 \
    --seed $((RANDOM + 17)) \
    -o "$OUTFILE" 2>&1 | tail -8

  if [ -f "$OUTFILE" ]; then
    SIZE=$(du -h "$OUTFILE" | cut -f1)
    echo "  -> $OUTFILE ($SIZE)"
  else
    echo "  !! FAILED: $slug"
  fi
done

echo "============================================="
echo "DONE. Goods props in $OUT: $(ls "$OUT" | wc -l)"
echo "============================================="