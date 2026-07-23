#!/bin/bash
# hermes-verify-ramayana-hud-pills-day15.sh — Verifier for Day 15
# 4 HUD pill MonoBehaviours: TimeOfDayPill, GuidedModePill,
# DharmaLedgerMini, QuestPill. (StreakPill + KandaProgressPill were
# intentionally skipped to avoid duplication with VerseStreakHUD and
# DayDotStrip respectively.)
#
# Asserts:
# 1. All 4 .cs files + .meta sidecars exist
# 2. namespace Jambudweep.Ramayana.Feedback on each
# 3. class X : MonoBehaviour (sealed) on each
# 4. Instance singleton + EnsureCreated() factory pattern on each
# 5. Public SetXxx() setter OR FindFirstObjectByType<StoryEngine> wiring
# 6. .meta guid valid (32 hex)
# 7. Brace balance
# 8. No duplication with VerseStreakHUD (no "streak" field on any pill)
# 9. No duplication with DayDotStrip / VersesProgressHUD (no dot arrays)
# 10. No duplication with EmbassyRecapHUD (no _line1/_line2/_line3 fields)
# 11. Each pill references UnityEngine.UI (Canvas + Image + Text pattern)
# 12. Reference StoryEngine (3 of 4 pills) or self-contained (TimeOfDayPill)
# 13. Each pill compiles cleanly (Unity batchmode later)
#
# Usage:
#   bash Tools/qa/hermes-verify-ramayana-hud-pills-day15.sh
#   HERMES_VERIFY_ROOT=<project> bash hermes-verify-ramayana-hud-pills-day15.sh
#
# Exit codes: 0 = all pass, 1 = any fail.
# Status: ad-hoc only — Unity batchmode compile pending.

set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
PASS=0; FAIL=0

section() { echo ""; echo "== $1 =="; }
check() { if [ "$2" = "$3" ]; then echo "  ✅ $1  ($2)"; PASS=$((PASS+1)); else echo "  ❌ $1  expected=$3 actual=$2"; FAIL=$((FAIL+1)); fi; }
ge() { if [ "$2" -ge "$3" ] 2>/dev/null; then echo "  ✅ $1  ($2 ≥ $3)"; PASS=$((PASS+1)); else echo "  ❌ $1  expected≥$3 actual=$2"; FAIL=$((FAIL+1)); fi; }
file_check() { if [ -f "$2" ]; then echo "  ✅ $1"; PASS=$((PASS+1)); else echo "  ❌ $1  (missing: $2)"; FAIL=$((FAIL+1)); fi; }

PILLS=(TimeOfDayPill GuidedModePill DharmaLedgerMini QuestPill)
PILL_DIR="$ROOT/Assets/Scripts/Feedback"

# ---------- Files ----------
section "Files exist (4 pills + 4 metas)"
for pill in "${PILLS[@]}"; do
  file_check "$pill.cs"      "$PILL_DIR/$pill.cs"
  file_check "$pill.cs.meta" "$PILL_DIR/$pill.cs.meta"
done

# ---------- Source surface per pill ----------
for pill in "${PILLS[@]}"; do
  section "Surface: $pill"
  SRC=$(cat "$PILL_DIR/$pill.cs")
  check "namespace Jambudweep.Ramayana.Feedback" \
    "$(echo "$SRC" | grep -c '^namespace Jambudweep.Ramayana.Feedback')" "1"
  check "public sealed class $pill" \
    "$(echo "$SRC" | grep -c "public sealed class $pill")" "1"
  ge "MonoBehaviour (≥1 ref)" \
    "$(echo "$SRC" | grep -c 'MonoBehaviour')" "1"
  check "public static $pill Instance" \
    "$(echo "$SRC" | grep -c "public static $pill Instance")" "1"
  check "public static void EnsureCreated()" \
    "$(echo "$SRC" | grep -c 'public static void EnsureCreated()')" "1"
  ge "private void Build()" \
    "$(echo "$SRC" | grep -c 'private void Build()')" "1"
  ge "UnityEngine.UI referenced (≥1)" \
    "$(echo "$SRC" | grep -c 'UnityEngine.UI')" "1"
  ge "Canvas referenced (≥1)" \
    "$(echo "$SRC" | grep -c 'Canvas')" "1"
  ge "Image referenced (≥1)" \
    "$(echo "$SRC" | grep -c 'AddComponent<Image>')" "1"
  ge "Text referenced (≥1)" \
    "$(echo "$SRC" | grep -c 'AddComponent<Text>')" "1"
  opens=$(grep -o '{' "$PILL_DIR/$pill.cs" | wc -l | tr -d ' ')
  closes=$(grep -o '}' "$PILL_DIR/$pill.cs" | wc -l | tr -d ' ')
  check "brace balance ({ $opens, } $closes)" "$opens" "$closes"
done

# ---------- Pill-specific asserts ----------
section "TimeOfDayPill — references TimeOfDayBand constant"
SRC=$(cat "$PILL_DIR/TimeOfDayPill.cs")
ge "TimeOfDayBand referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'TimeOfDayBand')" "3"
ge "4 bands declared (Sunrise/Midday/Sunset/Night)" \
  "$(echo "$SRC" | grep -cE 'Sunrise|Midday|Sunset|Night')" "8"

section "GuidedModePill — references StoryEngine + StoryMode"
SRC=$(cat "$PILL_DIR/GuidedModePill.cs")
ge "Jambudweep.Ramayana.Story referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'Jambudweep.Ramayana.Story')" "1"
ge "StoryMode enum referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'StoryMode')" "5"
ge "FindFirstObjectByType<StoryEngine> (≥1)" \
  "$(echo "$SRC" | grep -c 'FindFirstObjectByType<StoryEngine>')" "1"
ge "12 StoryMode labels covered (≥10)" \
  "$(echo "$SRC" | grep -cE 'case StoryMode')" "10"

section "DharmaLedgerMini — references StoryEngine + dharma score"
SRC=$(cat "$PILL_DIR/DharmaLedgerMini.cs")
ge "Jambudweep.Ramayana.Story referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'Jambudweep.Ramayana.Story')" "1"
ge "dharmaScore referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'dharmaScore')" "1"
ge "FindFirstObjectByType<StoryEngine> (≥1)" \
  "$(echo "$SRC" | grep -c 'FindFirstObjectByType<StoryEngine>')" "1"

section "QuestPill — references StoryEngine + StoryMomentPlayer"
SRC=$(cat "$PILL_DIR/QuestPill.cs")
ge "Jambudweep.Ramayana.Story referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'Jambudweep.Ramayana.Story')" "1"
ge "StoryMomentPlayer referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'StoryMomentPlayer')" "3"
ge "FindFirstObjectByType<StoryEngine> (≥1)" \
  "$(echo "$SRC" | grep -c 'FindFirstObjectByType<StoryEngine>')" "1"
ge "completedObjectiveIds referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'completedObjectiveIds')" "1"
ge "CurrentObjectiveId referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'CurrentObjectiveId')" "1"

# ---------- .meta guid ----------
section ".meta guid valid"
for pill in "${PILLS[@]}"; do
  META_GUID=$(grep -h '^guid:' "$PILL_DIR/$pill.cs.meta" | awk '{print $2}')
  if [[ "$META_GUID" =~ ^[a-f0-9]{32}$ ]]; then
    echo "  ✅ $pill guid valid: $META_GUID"; PASS=$((PASS+1))
  else
    echo "  ❌ $pill guid invalid: $META_GUID"; FAIL=$((FAIL+1))
  fi
done

# ---------- No duplication with existing pills ----------
section "No duplication with existing HUD surface"
for pill in "${PILLS[@]}"; do
  SRC=$(cat "$PILL_DIR/$pill.cs")
  # Streak should only appear in VerseStreakHUD, not in any new pill
  STREAK_CNT=$(echo "$SRC" | grep -cE '_streak\b|StreakPill\b')
  if [ "$STREAK_CNT" -eq 0 ]; then
    echo "  ✅ $pill no streak field"; PASS=$((PASS+1))
  else
    echo "  ❌ $pill has streak duplication (count=$STREAK_CNT)"; FAIL=$((FAIL+1))
  fi
  # _line1/_line2/_line3 should only appear in EmbassyRecapHUD, not in new pills
  LINE_CNT=$(echo "$SRC" | grep -cE '_line1|_line2|_line3')
  if [ "$LINE_CNT" -eq 0 ]; then
    echo "  ✅ $pill no EmbassyRecapHUD-style multi-line recap"; PASS=$((PASS+1))
  else
    echo "  ❌ $pill has recap duplication (count=$LINE_CNT)"; FAIL=$((FAIL+1))
  fi
  # No 18-element dot arrays (DayDotStrip signature)
  DOTS_CNT=$(echo "$SRC" | grep -cE '_dots\s*=\s*new Image\[18\]')
  if [ "$DOTS_CNT" -eq 0 ]; then
    echo "  ✅ $pill no dot-strip duplication"; PASS=$((PASS+1))
  else
    echo "  ❌ $pill has dot-strip duplication (count=$DOTS_CNT)"; FAIL=$((FAIL+1))
  fi
done

# ---------- Final report ----------
echo ""
echo "==================================================="
echo "  $PASS passed, $FAIL failed  (ad-hoc only, Unity compile pending)"
echo "==================================================="
[ "$FAIL" -eq 0 ] && exit 0 || exit 1