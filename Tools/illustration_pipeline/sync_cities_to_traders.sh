#!/usr/bin/env bash
# HERMES: Sync city plates from RamayanaPS5 illustration pipeline to the Traders game
# Source:      /Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/cities/
# Destination: /Users/prabaharan/Aerospace_projects/Hertree/traders-of-jambudweep-3d/public/city_plates/
#
# Skips files that are already up to date.

set -uo pipefail

SRC="/Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/cities"
DST="/Users/prabaharan/Aerospace_projects/Hertree/traders-of-jambudweep-3d/public/city_plates"

mkdir -p "$DST"

if [ ! -d "$SRC" ]; then
  echo "ERROR: source dir does not exist: $SRC" >&2
  exit 1
fi

synced=0
skipped=0

for f in "$SRC"/*.png; do
  [ -e "$f" ] || continue
  fname=$(basename "$f")
  dst_path="$DST/$fname"

  if [ -f "$dst_path" ] && [ "$dst_path" -nt "$f" ]; then
    skipped=$((skipped + 1))
    continue
  fi

  if cp "$f" "$dst_path"; then
    synced=$((synced + 1))
    size=$(du -h "$dst_path" | cut -f1)
    echo "  ✓ $fname ($size)"
  fi
done

echo "----------------------------------------"
echo "Synced: $synced | Skipped: $skipped | Total in $DST: $(ls "$DST" 2>/dev/null | wc -l | tr -d ' ')"