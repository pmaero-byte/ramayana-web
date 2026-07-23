#!/bin/bash
# Day 26 — Aranya Kanda moment corpus verifier
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
FILE="$ROOT/Assets/Resources/Ramayana/moments_aranya_kanda.json"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

[ -f "$FILE" ] && p "moments_aranya_kanda.json exists" || f "moments_aranya_kanda.json missing"
SRC=$(cat "$FILE" 2>/dev/null || true)
[ -n "$SRC" ] && p "file non-empty" || f "file empty"
echo "$SRC" | python3 -m json.tool >/dev/null 2>&1 && p "JSON valid" || f "JSON invalid"
COUNT=$(echo "$SRC" | python3 -c "import json,sys; print(len(json.load(sys.stdin).get('moments', [])))" 2>/dev/null || echo 0)
[ "$COUNT" -ge 10 ] && p "moments count >= 10 ($COUNT)" || f "moments < 10"
echo "$SRC" | grep -q '"kanda": "aranya-kanda"' && p "kanda=aranya-kanda" || f "missing kanda"
echo "$SRC" | grep -q 'vc_aranya_' && p "voiceCueId pattern present" || f "voiceCueId missing"

# Unique momentIds
IDS=$(echo "$SRC" | python3 -c "import json,sys; d=json.load(sys.stdin); print(' '.join(m.get('momentId','') for m in d.get('moments',[])))" 2>/dev/null || true)
TOTAL=$(echo "$IDS" | wc -w | tr -d ' ')
UNIQ=$(echo "$IDS" | tr ' ' '\n' | sort -u | wc -l | tr -d ' ')
[ "$TOTAL" = "$UNIQ" ] && p "momentIds unique ($UNIQ)" || f "duplicate momentIds"

# Day 11 roster cross-check
for char in rama sita lakshmana bharata shabari jatayu ravana; do
  echo "$SRC" | python3 -c "
import json,sys
d=json.load(sys.stdin)
ms=d.get('moments',[])
hit=any('$char' in ' '.join(m.get('characters', []) or []) for m in ms)
print('OK' if hit else 'MISS')
" 2>/dev/null | grep -q 'OK' && p "character $char appears" || f "character $char missing"
done

# Regression: Day 22 corpus intact
[ -f "$ROOT/Assets/Resources/Ramayana/moments_ayodhya_kanda.json" ] && p "moments_ayodhya_kanda.json intact" || f "moments_ayodhya_kanda.json missing"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-kandatree-day23.sh" 2>/dev/null && p "Day 23 verifier bash -n" || f "Day 23 verifier bash -n"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-save-slots-day24.sh" 2>/dev/null && p "Day 24 verifier bash -n" || f "Day 24 verifier bash -n"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 26)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
