#!/bin/bash
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
FILE="$ROOT/Assets/Resources/Ramayana/moments_kishkindha_kanda.json"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

[ -f "$FILE" ] && p "moments_kishkindha_kanda.json exists" || f "moments_kishkindha_kanda.json missing"
SRC=$(cat "$FILE" 2>/dev/null || true)
[ -n "$SRC" ] && p "file non-empty" || f "file empty"
echo "$SRC" | python3 -m json.tool >/dev/null 2>&1 && p "JSON valid" || f "JSON invalid"
COUNT=$(echo "$SRC" | python3 -c "import json,sys; print(len(json.load(sys.stdin).get('moments', [])))" 2>/dev/null || echo 0)
[ "$COUNT" -ge 10 ] && p "moments >= 10 ($COUNT)" || f "moments < 10"
echo "$SRC" | grep -q '"kanda": "kishkindha-kanda"' && p "kanda=kishkindha-kanda" || f "missing kanda"
echo "$SRC" | grep -q 'vc_kishkindha_' && p "voiceCueId pattern" || f "voiceCueId missing"
IDS=$(echo "$SRC" | python3 -c "import json,sys; d=json.load(sys.stdin); print(' '.join(m.get('momentId','') for m in d.get('moments',[])))" 2>/dev/null || true)
echo "$IDS" | tr ' ' '\n' | sort -u | wc -l | tr -d ' ' | grep -q '^10$' && p "10 unique momentIds" || f "unique momentIds != 10"
for char in hanuman rama sita lakshmana jambavan sugriva ravana; do
  echo "$SRC" | python3 -c "
import json,sys
d=json.load(sys.stdin)
ms=d.get('moments',[])
hit=any('$char' in ' '.join(m.get('characters', []) or []) for m in ms)
print('OK' if hit else 'MISS')
" 2>/dev/null | grep -q 'OK' && p "character $char appears" || f "character $char missing"
done
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-aranya-kanda-day26.sh" 2>/dev/null && p "Day 26 verifier bash -n" || f "Day 26 verifier bash -n"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-kandatree-day23.sh" 2>/dev/null && p "Day 23 verifier bash -n" || f "Day 23 verifier bash -n"
echo ""
echo "  $PASS passed, $FAIL failed  (Day 27)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
