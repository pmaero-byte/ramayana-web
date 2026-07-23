#!/bin/bash
# hermes-verify-ramayana-verse-combat-trigger-day16.sh — Verifier for Day 16
# VerseCombatTrigger bridges VerseOrchestrator.onMomentCompleted →
# WaveController.StartWaves, driven by a configurable rule map.
#
# Asserts:
# 1. VerseCombatTrigger.cs + .meta exist
# 2. namespace Jambudweep.Ramayana.Combat
# 3. class VerseCombatTrigger : MonoBehaviour (sealed)
# 4. Instance singleton + EnsureCreated() factory
# 5. References WaveController (Day 1-10) + VerseOrchestrator (Day 14)
# 6. References FormationKind enum (Arc/Chakra/Vyuha)
# 7. Subscribe to VerseOrchestrator.onMomentCompleted (UnityEvent<string>)
# 8. Call WaveController.SetFormation() + StartWaves()
# 9. Rule model: cueId + totalWaves + formation + graceSeconds + oneShot
# 10. UnityEvent hooks: onTriggerFired / onTriggerSkipped / onError
# 11. Brace balance
# 12. .meta guid valid
# 13. Day 1-10 WaveController.cs UNTOUCHED
# 14. Day 14 VerseOrchestrator.cs UNTOUCHED
# 15. C# compiles cleanly (Unity batchmode later)
#
# Usage:
#   bash Tools/qa/hermes-verify-ramayana-verse-combat-trigger-day16.sh
#   HERMES_VERIFY_ROOT=<project> bash hermes-verify-ramayana-verse-combat-trigger-day16.sh
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

TRIG="$ROOT/Assets/Scripts/Combat/VerseCombatTrigger.cs"
SRC=$(cat "$TRIG")

# ---------- Files ----------
section "Files exist"
file_check "VerseCombatTrigger.cs"      "$TRIG"
file_check "VerseCombatTrigger.cs.meta" "$ROOT/Assets/Scripts/Combat/VerseCombatTrigger.cs.meta"

# ---------- Source surface ----------
section "Source surface"
check "namespace Jambudweep.Ramayana.Combat" \
  "$(echo "$SRC" | grep -c '^namespace Jambudweep.Ramayana.Combat')" "1"
check "public sealed class VerseCombatTrigger" \
  "$(echo "$SRC" | grep -c 'public sealed class VerseCombatTrigger')" "1"
ge "MonoBehaviour (≥1 ref)" \
  "$(echo "$SRC" | grep -c 'MonoBehaviour')" "1"
check "public static VerseCombatTrigger Instance" \
  "$(echo "$SRC" | grep -c 'public static VerseCombatTrigger Instance')" "1"
check "public static EnsureCreated()" \
  "$(echo "$SRC" | grep -c 'public static VerseCombatTrigger EnsureCreated()')" "1"
check "public static EnsureCreatedWithRules(List)" \
  "$(echo "$SRC" | grep -c 'public static VerseCombatTrigger EnsureCreatedWithRules')" "1"

# ---------- Cross-system wiring ----------
section "Cross-system wiring"
ge "WaveController referenced (≥2)" \
  "$(echo "$SRC" | grep -c 'WaveController')" "2"
ge "VerseOrchestrator referenced (≥2)" \
  "$(echo "$SRC" | grep -c 'VerseOrchestrator')" "2"
ge "Verse.VerseOrchestrator namespace imported (≥1)" \
  "$(echo "$SRC" | grep -c 'Jambudweep.Ramayana.Verse')" "1"
ge "FormationKind type referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'FormationKind')" "1"
check "default formation = FormationKind.Arc" \
  "$(echo "$SRC" | grep -cE 'formation = FormationKind\.Arc')" "1"

# Verify FormationKind enum actually has Arc/Chakra/Vyuha in the source enum file
FORM_STRAT_ENUM=$(python3 -c "
import re
src = open('$ROOT/Assets/Scripts/Combat/FormationStrategy.cs').read()
m = re.search(r'enum FormationKind\s*\{([^}]+)\}', src, re.DOTALL)
if not m:
    print('missing')
else:
    vals = []
    for v in re.findall(r'\\b(Arc|Chakra|Vyuha)\\b', m.group(1)):
        vals.append(v)
    print(','.join(sorted(set(vals))) if vals else 'missing')
" 2>/dev/null || echo "missing")
check "FormationKind enum has Arc/Chakra/Vyuha (FormationStrategy.cs)" "$FORM_STRAT_ENUM" "Arc,Chakra,Vyuha"

# ---------- Event wiring ----------
section "Event wiring (verse → combat)"
check "VerseOrchestrator.onMomentCompleted subscribed" \
  "$(echo "$SRC" | grep -c 'verseOrchestrator.onMomentCompleted.AddListener')" "1"
check "WaveController.SetFormation called" \
  "$(echo "$SRC" | grep -c 'waveController.SetFormation')" "1"
check "WaveController.StartWaves called" \
  "$(echo "$SRC" | grep -c 'waveController.StartWaves')" "1"
check "HandleMomentCompleted method declared" \
  "$(echo "$SRC" | grep -c 'private void HandleMomentCompleted')" "1"

# ---------- Rule model ----------
section "Rule model fields"
check "VerseCombatTriggerRule class declared" \
  "$(echo "$SRC" | grep -c 'public class VerseCombatTriggerRule')" "1"
ge "cueId field (≥1)"     "$(echo "$SRC" | grep -cE 'public string cueId')" "1"
ge "totalWaves field (≥1)" "$(echo "$SRC" | grep -cE 'public int totalWaves')" "1"
ge "formation field (≥1)"  "$(echo "$SRC" | grep -cE 'public FormationKind formation')" "1"
ge "graceSeconds field (≥1)" "$(echo "$SRC" | grep -cE 'public float graceSeconds')" "1"
ge "oneShot field (≥1)"    "$(echo "$SRC" | grep -cE 'public bool oneShot')" "1"

# ---------- UnityEvent hooks ----------
section "UnityEvent hooks"
ge "onTriggerFired (≥1)"   "$(echo "$SRC" | grep -c 'onTriggerFired')" "1"
ge "onTriggerSkipped (≥1)" "$(echo "$SRC" | grep -c 'onTriggerSkipped')" "1"
ge "onError (≥1)"          "$(echo "$SRC" | grep -c 'onError')" "1"
check "VerseCombatTriggerEvent typed" \
  "$(echo "$SRC" | grep -c 'class VerseCombatTriggerEvent')" "1"

# ---------- Brace balance ----------
section "Brace balance (Roslyn-free sanity)"
opens=$(grep -o '{' "$TRIG" | wc -l | tr -d ' ')
closes=$(grep -o '}' "$TRIG" | wc -l | tr -d ' ')
check "brace balance ({ $opens, } $closes)" "$opens" "$closes"

# ---------- .meta guid ----------
section ".meta guid"
META_GUID=$(grep -h '^guid:' "$ROOT/Assets/Scripts/Combat/VerseCombatTrigger.cs.meta" | awk '{print $2}')
if [[ "$META_GUID" =~ ^[a-f0-9]{32}$ ]]; then
  echo "  ✅ .meta guid valid: $META_GUID"; PASS=$((PASS+1))
else
  echo "  ❌ .meta guid invalid: $META_GUID"; FAIL=$((FAIL+1))
fi

# ---------- No-touch discipline ----------
section "Day 1-10 + Day 14 files UNTOUCHED"
check "WaveController.cs not modified" \
  "$(cd "$ROOT" && git diff --name-only HEAD -- Assets/Scripts/Combat/WaveController.cs | wc -l | tr -d ' ')" "0"
check "VerseOrchestrator.cs not modified" \
  "$(cd "$ROOT" && git diff --name-only HEAD -- Assets/Scripts/Verse/VerseOrchestrator.cs | wc -l | tr -d ' ')" "0"
check "FormationStrategy.cs not modified" \
  "$(cd "$ROOT" && git diff --name-only HEAD -- Assets/Scripts/Combat/FormationStrategy.cs | wc -l | tr -d ' ')" "0"

# ---------- Day 12 moment verbs exist for combat triggers ----------
section "Day 12 face-challenge moments present (combat triggers)"
FACE_CHAL=$(python3 -c "
import json
d = json.load(open('$ROOT/Assets/Resources/Ramayana/moments_bala_kanda.json'))
fc = [m for m in d['moments'] if m['verb'] == 'face-challenge']
print(len(fc))
" 2>/dev/null || echo 0)
ge "Day 12 face-challenge moments (≥2)" "$FACE_CHAL" "2"

# ---------- Final report ----------
echo ""
echo "==================================================="
echo "  $PASS passed, $FAIL failed  (ad-hoc only, Unity compile pending)"
echo "==================================================="
[ "$FAIL" -eq 0 ] && exit 0 || exit 1