#!/usr/bin/env bash
# Wait for cities batch to finish (PID 95684), then run props batch.
# This script runs detached so the cities batch can complete first.

set -e

LOG=/tmp/hermes-after-cities.log
CITIES_PID=95684
PROPS_SCRIPT="/Users/prabaharan/Aerospace_projects/RamayanaPS5/Tools/illustration_pipeline/generate_props.sh"

# Wait for cities batch process
while kill -0 "$CITIES_PID" 2>/dev/null; do
    sleep 30
done

echo "[$(date)] Cities batch done (pid $CITIES_PID exited). Starting props..." >> "$LOG"
bash "$PROPS_SCRIPT" >> "$LOG" 2>&1
echo "[$(date)] Props batch done." >> "$LOG"