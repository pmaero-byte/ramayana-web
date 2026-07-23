#!/usr/bin/env bash
# Sync the latest city plate PNGs from the RamayanaPS5 illustration pipeline
# into the Traders of Jambudweep public/city_plates/ directory so the in-game
# billboard loader can pick them up.
#
# Run after any generate_traders_cities.sh batch completes. Idempotent.

set -e
SRC="/Users/prabaharan/Aerospace_projects/RamayanaPS5/Assets/Illustrations/cities"
DST="/Users/prabaharan/Aerospace_projects/Hertree/traders-of-jambudweep-3d/public/city_plates"

mkdir -p "$DST"
cp -n "$SRC"/*.png "$DST"/ 2>/dev/null || true
cp "$SRC"/*.png "$DST"/

echo "Source: $(ls "$SRC"/*.png 2>/dev/null | wc -l | tr -d ' ') plates"
echo "Dest:   $(ls "$DST"/*.png 2>/dev/null | wc -l | tr -d ' ') plates"
echo "Synced."