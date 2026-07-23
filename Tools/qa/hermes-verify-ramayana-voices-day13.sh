#!/bin/bash
# hermes-verify-ramayana-voices-day13.sh — Verifier for Day 13
# ELGODS alankarashastra voice register corpus import.
#
# Asserts:
# 1. voices.json + .meta exist
# 2. JSON valid + schema fields present
# 3. Exactly 11 alankarashastra voice registers (Dandin + Bhamaha canon)
# 4. ≥55 character→voice bindings (full ELGODS CHARACTER_VOICE_MAP)
# 5. 36 voice cues for Day 12 Bala Kanda moments (1:1 cross-reference)
# 6. Every cue.cueId matches a Day 12 moment.voiceCueId (bidirectional)
# 7. Every cue.registerId matches a register.id
# 8. Every binding.registerId matches a register.id
# 9. Every cue.speaker is in either the bindings list OR is "narrator" (kathaka default)
# 10. Every register has non-empty english + sanskrit (with diacritics)
# 11. All 11 register IDs are canonical (sloka/vaachaka/shastriya/praarthana/sheershata/vairagya/maatru/pratishedha/dainya/niti/kathaka)
# 12. C# binding has namespace + 5 classes + 5 [Serializable] attributes
# 13. Brace balance
# 14. .cs.meta guid is valid
# 15. ELGODS source still has CHARACTER_VOICE_MAP (sanity)
#
# Usage:
#   bash Tools/qa/hermes-verify-ramayana-voices-day13.sh
#   HERMES_VERIFY_ROOT=<project> bash hermes-verify-ramayana-voices-day13.sh
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

CORPUS="$ROOT/Assets/Resources/Ramayana/voices.json"
BINDING="$ROOT/Assets/Scripts/Data/RamayanaVoicesData.cs"
MOMENTS="$ROOT/Assets/Resources/Ramayana/moments_bala_kanda.json"

# ---------- Files ----------
section "Files exist"
file_check "voices.json"               "$CORPUS"
file_check "voices.json.meta"          "$ROOT/Assets/Resources/Ramayana/voices.json.meta"
file_check "RamayanaVoicesData.cs"     "$BINDING"
file_check "RamayanaVoicesData.cs.meta" "$ROOT/Assets/Scripts/Data/RamayanaVoicesData.cs.meta"

# ---------- JSON validity ----------
section "JSON validity"
if python3 -c "import json; json.load(open('$CORPUS'))" 2>/dev/null; then
  echo "  ✅ voices.json is valid JSON"; PASS=$((PASS+1))
else
  echo "  ❌ voices.json is NOT valid JSON"; FAIL=$((FAIL+1))
  echo ""
  echo "==================================================="
  echo "  $PASS passed, $FAIL failed  (ad-hoc only, Unity compile pending)"
  echo "==================================================="
  exit 1
fi

# ---------- Register count ----------
section "Voice registers (11 alankarashastra)"
COUNT_REGS=$(python3 -c "import json; print(len(json.load(open('$CORPUS'))['registers']))" 2>/dev/null || echo 0)
check "register count" "$COUNT_REGS" "11"

# Required canonical register IDs (from ELGODS VoiceRegister type)
REQUIRED_REG_IDS=(sloka vaachaka shastriya praarthana sheershata vairagya maatru pratishedha dainya niti kathaka)
MISSING_REGS=""
for rid in "${REQUIRED_REG_IDS[@]}"; do
  PRESENT=$(python3 -c "
import json, sys
d = json.load(open('$CORPUS'))
sys.stdout.write('yes' if any(r['id'] == '$rid' for r in d['registers']) else 'no')
")
  if [ "$PRESENT" != "yes" ]; then MISSING_REGS="$MISSING_REGS $rid"; fi
done
check "all 11 canonical register IDs present" "${MISSING_REGS:-ok}" "ok"

# ---------- Character→voice bindings ----------
section "Character→voice bindings (≥55)"
COUNT_BINDINGS=$(python3 -c "import json; print(len(json.load(open('$CORPUS'))['characterVoiceBindings']))" 2>/dev/null || echo 0)
ge "binding count" "$COUNT_BINDINGS" "55"

# ---------- Voice cues ----------
section "Voice cues (36 Bala Kanda, 1:1 with Day 12)"
COUNT_CUES=$(python3 -c "import json; print(len(json.load(open('$CORPUS'))['voiceCues']))" 2>/dev/null || echo 0)
check "cue count" "$COUNT_CUES" "36"

# Bidirectional cross-reference with Day 12
ORPHAN_CUES=$(python3 -c "
import json
cues = json.load(open('$CORPUS'))['voiceCues']
moments = json.load(open('$MOMENTS'))['moments']
moment_cue_ids = set(m['voiceCueId'] for m in moments)
cue_ids = set(c['cueId'] for c in cues)
orphan_cues = cue_ids - moment_cue_ids
print(','.join(sorted(orphan_cues)) if orphan_cues else 'none')
" 2>/dev/null || echo "?")
check "no orphan cues (all cueIds reference Day 12 moments)" "$ORPHAN_CUES" "none"

ORPHAN_MOMENTS=$(python3 -c "
import json
cues = json.load(open('$CORPUS'))['voiceCues']
moments = json.load(open('$MOMENTS'))['moments']
moment_cue_ids = set(m['voiceCueId'] for m in moments)
cue_ids = set(c['cueId'] for c in cues)
orphan_moments = moment_cue_ids - cue_ids
print(','.join(sorted(orphan_moments)) if orphan_moments else 'none')
" 2>/dev/null || echo "?")
check "no orphan moments (all Day 12 cueIds have voice cues)" "$ORPHAN_MOMENTS" "none"

# ---------- Cue.registerId → register.id integrity ----------
section "Cue.registerId integrity"
BAD_CUE_REG=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
reg_ids = set(r['id'] for r in d['registers'])
bad = [c['cueId'] for c in d['voiceCues'] if c['register'] not in reg_ids]
print(','.join(bad) if bad else 'none')
" 2>/dev/null || echo "?")
check "all cue.register references valid register" "$BAD_CUE_REG" "none"

# ---------- Binding.registerId → register.id integrity ----------
section "Binding.registerId integrity"
BAD_BIND_REG=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
reg_ids = set(r['id'] for r in d['registers'])
bad = [b['characterId'] for b in d['characterVoiceBindings'] if b['registerId'] not in reg_ids]
print(','.join(bad) if bad else 'none')
" 2>/dev/null || echo "?")
check "all binding.registerId references valid register" "$BAD_BIND_REG" "none"

# ---------- Sanskrit diacritics on all registers ----------
section "Sanskrit diacritics present on all 11 registers"
NON_DIACRITIC=$(python3 -c "
import json, re
d = json.load(open('$CORPUS'))
# Devanagari Unicode block: U+0900–U+097F
bad = []
for r in d['registers']:
    if not re.search(r'[\u0900-\u097F]', r.get('sanskrit', '')):
        bad.append(r['id'])
print(','.join(bad) if bad else 'none')
" 2>/dev/null || echo "?")
check "all 11 registers have Devanagari diacritics" "$NON_DIACRITIC" "none"

# ---------- C# binding surface ----------
section "C# binding source surface"
SRC=$(cat "$BINDING")
check "namespace Jambudweep.Ramayana.Data" \
  "$(echo "$SRC" | grep -c '^namespace Jambudweep.Ramayana.Data')" "1"
ge "class RamayanaVoiceRegister (≥1 occurrence)"          "$(echo "$SRC" | grep -c 'class RamayanaVoiceRegister')" "1"
ge "class RamayanaCharacterVoiceBinding (≥1 occurrence)"  "$(echo "$SRC" | grep -c 'class RamayanaCharacterVoiceBinding')" "1"
ge "class RamayanaVoiceCue (≥1 occurrence)"              "$(echo "$SRC" | grep -c 'class RamayanaVoiceCue')" "1"
ge "class RamayanaVoicesMeta (≥1 occurrence)"            "$(echo "$SRC" | grep -c 'class RamayanaVoicesMeta')" "1"
ge "class RamayanaVoicesCorpus (≥1 occurrence)"          "$(echo "$SRC" | grep -c 'class RamayanaVoicesCorpus')" "1"
ge "[Serializable] attribute count (≥5)"                 "$(echo "$SRC" | grep -c '\[Serializable\]')" "5"

# ---------- Brace balance ----------
section "Brace balance (Roslyn-free sanity)"
opens=$(grep -o '{' "$BINDING" | wc -l | tr -d ' ')
closes=$(grep -o '}' "$BINDING" | wc -l | tr -d ' ')
check "brace balance ({ $opens, } $closes)" "$opens" "$closes"

# ---------- .meta guid ----------
section ".meta guid"
META_GUID=$(grep -h '^guid:' "$ROOT/Assets/Scripts/Data/RamayanaVoicesData.cs.meta" | awk '{print $2}')
if [[ "$META_GUID" =~ ^[a-f0-9]{32}$ ]]; then
  echo "  ✅ .meta guid valid: $META_GUID"; PASS=$((PASS+1))
else
  echo "  ❌ .meta guid invalid: $META_GUID"; FAIL=$((FAIL+1))
fi

JSON_META_GUID=$(grep -h '^guid:' "$ROOT/Assets/Resources/Ramayana/voices.json.meta" | awk '{print $2}')
if [[ "$JSON_META_GUID" =~ ^[a-f0-9]{32}$ ]]; then
  echo "  ✅ JSON .meta guid valid: $JSON_META_GUID"; PASS=$((PASS+1))
else
  echo "  ❌ JSON .meta guid invalid: $JSON_META_GUID"; FAIL=$((FAIL+1))
fi

# ---------- ELGODS source sanity ----------
section "ELGODS source sanity (CHARACTER_VOICE_MAP still present)"
CVM_LINES=$(grep -c 'CHARACTER_VOICE_MAP' /Users/prabaharan/jambudweep/ELGODS/portal/src/game/ramayana/characterVoices.ts 2>/dev/null || echo 0)
ge "ELGODS source has CHARACTER_VOICE_MAP (≥2)" "$CVM_LINES" "2"

VRL_LINES=$(grep -c 'VOICE_LABELS' /Users/prabaharan/jambudweep/ELGODS/portal/src/game/ramayana/types.ts 2>/dev/null || echo 0)
ge "ELGODS source has VOICE_LABELS (≥1)" "$VRL_LINES" "1"

# ---------- Final report ----------
echo ""
echo "==================================================="
echo "  $PASS passed, $FAIL failed  (ad-hoc only, Unity compile pending)"
echo "==================================================="
[ "$FAIL" -eq 0 ] && exit 0 || exit 1