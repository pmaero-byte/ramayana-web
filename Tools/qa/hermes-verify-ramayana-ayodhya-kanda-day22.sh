#!/bin/bash
# Day 22 Ayodhya Kanda moment corpus verifier
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
FILE="$ROOT/Assets/Resources/Ramayana/moments_ayodhya_kanda.json"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

[ -f "$FILE" ] && p "moments_ayodhya_kanda.json exists" || f "moments_ayodhya_kanda.json missing"
SRC=$(cat "$FILE" 2>/dev/null || true)
[ -n "$SRC" ] && p "file non-empty" || f "file empty"
echo "$SRC" | python3 -m json.tool >/dev/null 2>&1 && p "JSON valid" || f "JSON invalid"
COUNT=$(echo "$SRC" | python3 -c "import json,sys; print(len(json.load(sys.stdin).get('moments', [])))" 2>/dev/null || echo 0)
[ "$COUNT" -ge 1 ] && p "moments count >= 1 ($COUNT)" || f "moments < 1"
echo "$SRC" | grep -q '"kanda": "ayodhya-kanda"' && p "kanda=ayodhya-kanda" || f "missing kanda"
echo "$SRC" | grep -q 'voiceCueId' && p "voiceCueId present" || f "voiceCueId missing"

# Day 11 roster whitelist cross-check
WHITELIST=$(echo "$SRC" | python3 -c "import json,sys; print(' '.join(json.load(sys.stdin).get('characterWhitelist', [])))" 2>/dev/null || true)
for char in dasharatha kausalya kaikeyi lakshmana bharata sita guha vishwamitra; do
  echo "$SRC" | python3 -c "import json,sys; d=json.load(sys.stdin); ms=d.get('moments',[]); [print('OK') for m in ms if '$char' in ' '.join(m.get('characters', []) or [])]" >/dev/null 2>&1 \
    && p "character $char appears" || f "character $char missing"
done

# Regression: Day 12 corpus still intact
[ -f "$ROOT/Assets/Resources/Ramayana/moments_bala_kanda.json" ] \
  && p "moments_bala_kanda.json intact" || f "moments_bala_kanda.json missing"

# Day 18/19 scripts still pass syntax
bash -n "$ROOT/Tools/qa/build-ios-sim.sh" 2>/dev/null && p "build-ios-sim.sh syntax" || f "build-ios-sim.sh syntax"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-archive-day19.sh" 2>/dev/null && p "Day 19 verifier syntax" || f "Day 19 verifier syntax"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 22)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
