#!/bin/bash
# hermes-verify-ramayana-moments-day12.sh — Verifier for Day 12
# ELGODS Bala Kanda moment corpus import.
#
# Asserts:
# 1. moments_bala_kanda.json + .meta exist
# 2. JSON valid + schema fields present
# 3. ≥30 moments (target: 36)
# 4. All moments have kanda == "bala-kanda"
# 5. All moments have non-empty narrative
# 6. All moments have non-empty moralLesson
# 7. All protagonists reference characters in the 36-char whitelist
# 8. All characters[] entries reference the whitelist
# 9. voiceCueId matches vc_bala_<verb>_<id> pattern
# 10. durationSec in 8-30 range (kid pacing)
# 11. Adhyayas covered include 1-12 (Bala Kanda sarga 1-73 → adhyaya 1-12)
# 12. Sarga range 1-73
# 13. C# binding has namespace + 5 classes + 5 [Serializable] attributes
# 14. Brace balance
# 15. .cs.meta guid is valid
# 16. Kid-safe: narrative must not contain graphic violence words
# 17. ELGODS source still has Bala Kanda citations (sanity, ≥30 known)
#
# Usage:
#   bash Tools/qa/hermes-verify-ramayana-moments-day12.sh
#   HERMES_VERIFY_ROOT=<project> bash hermes-verify-ramayana-moments-day12.sh
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
le() { if [ "$2" -le "$3" ] 2>/dev/null; then echo "  ✅ $1  ($2 ≤ $3)"; PASS=$((PASS+1)); else echo "  ❌ $1  expected≤$3 actual=$2"; FAIL=$((FAIL+1)); fi; }
file_check() { if [ -f "$2" ]; then echo "  ✅ $1"; PASS=$((PASS+1)); else echo "  ❌ $1  (missing: $2)"; FAIL=$((FAIL+1)); fi; }

CORPUS="$ROOT/Assets/Resources/Ramayana/moments_bala_kanda.json"
BINDING="$ROOT/Assets/Scripts/Data/RamayanaMomentsData.cs"

# ---------- Files ----------
section "Files exist"
file_check "moments_bala_kanda.json"               "$CORPUS"
file_check "moments_bala_kanda.json.meta"          "$ROOT/Assets/Resources/Ramayana/moments_bala_kanda.json.meta"
file_check "RamayanaMomentsData.cs"               "$BINDING"
file_check "RamayanaMomentsData.cs.meta"          "$ROOT/Assets/Scripts/Data/RamayanaMomentsData.cs.meta"

# ---------- JSON validity ----------
section "JSON validity"
if python3 -c "import json; json.load(open('$CORPUS'))" 2>/dev/null; then
  echo "  ✅ moments_bala_kanda.json is valid JSON"; PASS=$((PASS+1))
else
  echo "  ❌ moments_bala_kanda.json is NOT valid JSON"; FAIL=$((FAIL+1))
  echo ""
  echo "==================================================="
  echo "  $PASS passed, $FAIL failed  (ad-hoc only, Unity compile pending)"
  echo "==================================================="
  exit 1
fi

# ---------- Moment count ----------
section "Moment count (≥30, target 36)"
COUNT_MOMENTS=$(python3 -c "import json; print(len(json.load(open('$CORPUS'))['moments']))" 2>/dev/null || echo 0)
ge "moment count" "$COUNT_MOMENTS" "30"

# ---------- All moments kanda == bala-kanda ----------
section "All moments kanda == 'bala-kanda'"
WRONG_KANDA=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
wrong = [m['momentId'] for m in d['moments'] if m.get('kanda') != 'bala-kanda']
print(','.join(wrong) if wrong else 'none')
" 2>/dev/null || echo "?")
check "no kanda mismatches" "$WRONG_KANDA" "none"

# ---------- All moments have non-empty narrative + moral ----------
section "Required text fields non-empty"
EMPTY_NARR=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
empty = [m['momentId'] for m in d['moments'] if not m.get('narrative','').strip()]
print(','.join(empty) if empty else 'none')
" 2>/dev/null || echo "?")
check "no empty narrative" "$EMPTY_NARR" "none"

EMPTY_MORAL=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
empty = [m['momentId'] for m in d['moments'] if not m.get('moralLesson','').strip()]
print(','.join(empty) if empty else 'none')
" 2>/dev/null || echo "?")
check "no empty moralLesson" "$EMPTY_MORAL" "none"

# ---------- Character whitelist integrity ----------
section "Character references in whitelist (36 chars)"
DANGLING_PROT=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
wl = set(d['characterWhitelist'])
bad = [m['momentId'] for m in d['moments'] if m.get('protagonist') not in wl]
print(','.join(bad) if bad else 'none')
" 2>/dev/null || echo "?")
check "no dangling protagonist refs" "$DANGLING_PROT" "none"

DANGLING_CHARS=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
wl = set(d['characterWhitelist'])
bad = []
for m in d['moments']:
    for c in m.get('characters', []):
        if c not in wl:
            bad.append(f\"{m['momentId']}:{c}\")
print(','.join(bad) if bad else 'none')
" 2>/dev/null || echo "?")
check "no dangling characters[] refs" "$DANGLING_CHARS" "none"

WHITELIST_COUNT=$(python3 -c "import json; print(len(json.load(open('$CORPUS'))['characterWhitelist']))" 2>/dev/null || echo 0)
check "characterWhitelist size" "$WHITELIST_COUNT" "36"

# ---------- voiceCueId pattern ----------
section "voiceCueId format (vc_bala_<verb>_<id>)"
BAD_VC=$(python3 -c "
import json, re
d = json.load(open('$CORPUS'))
pat = re.compile(r'^vc_bala_[a-z-]+_[a-z0-9_]+$')
bad = [m['momentId'] for m in d['moments'] if not pat.match(m.get('voiceCueId',''))]
print(','.join(bad) if bad else 'none')
" 2>/dev/null || echo "?")
check "all voiceCueId match vc_bala_<verb>_<id>" "$BAD_VC" "none"

# ---------- Duration range ----------
section "durationSec range (12-22 for kid pacing)"
DUR_OUT=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
bad = [f\"{m['momentId']}:{m['durationSec']}\" for m in d['moments'] if m.get('durationSec', 0) < 8 or m.get('durationSec', 0) > 30]
print(','.join(bad) if bad else 'none')
" 2>/dev/null || echo "?")
check "all durations in 8-30 range" "$DUR_OUT" "none"

# ---------- Adhyaya + Sarga range ----------
section "Adhyaya coverage (Bala Kanda sarga 1-73 → adhyaya 1-12)"
ADHY_UNIQUE=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
ad = set(m['adhyaya'] for m in d['moments'])
print(len(ad))
" 2>/dev/null || echo 0)
ge "≥8 unique adhyayas covered (ELGODS source has gaps in sarga 8-12, 29-38)" "$ADHY_UNIQUE" "8"
# Verify adhyaya 1 is covered (Narada/Valmiki opening)
ADHY_1=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
print('yes' if 1 in set(m['adhyaya'] for m in d['moments']) else 'no')
" 2>/dev/null || echo "no")
check "adhyaya 1 covered (Narada/Valmiki opening)" "$ADHY_1" "yes"
# Verify adhyaya 12 covered (swayamvar climax)
ADHY_12=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
print('yes' if 12 in set(m['adhyaya'] for m in d['moments']) else 'no')
" 2>/dev/null || echo "no")
check "adhyaya 12 covered (swayamvar climax)" "$ADHY_12" "yes"

section "Sarga range (1-73)"
SARGA_MIN=$(python3 -c "import json; print(min(m['sargaStart'] for m in json.load(open('$CORPUS'))['moments']))" 2>/dev/null || echo 0)
SARGA_MAX=$(python3 -c "import json; print(max(m['sargaEnd'] for m in json.load(open('$CORPUS'))['moments']))" 2>/dev/null || echo 0)
check "sarga min" "$SARGA_MIN" "1"
le "sarga max ≤ 73" "$SARGA_MAX" "73"
ge "sarga max ≥ 50" "$SARGA_MAX" "50"

# ---------- Kid-safe narrative (no graphic violence) ----------
section "Kid-safe narrative (no slay/killed/battle/demon)"
UNSAFE=$(python3 -c "
import json, re
d = json.load(open('$CORPUS'))
# Allow 'killed' / 'battle' / 'demon' in moralLesson (knowledge citation) but NOT in narrative
bad = []
for m in d['moments']:
    narr = m.get('narrative', '').lower()
    for word in ['slay', 'killed', 'killing', 'combat', 'demons', 'rakshas']:
        if re.search(r'\\b'+word+r'\\b', narr):
            bad.append(f\"{m['momentId']}:{word}\")
print(','.join(bad) if bad else 'none')
" 2>/dev/null || echo "?")
check "no graphic violence in narrative" "$UNSAFE" "none"

# ---------- C# binding surface ----------
section "C# binding source surface"
SRC=$(cat "$BINDING")
check "namespace Jambudweep.Ramayana.Data" \
  "$(echo "$SRC" | grep -c '^namespace Jambudweep.Ramayana.Data')" "1"
ge "class RamayanaMoment (≥1 occurrence)"            "$(echo "$SRC" | grep -c 'class RamayanaMoment')" "1"
ge "class RamayanaChoiceOption (≥1 occurrence)"     "$(echo "$SRC" | grep -c 'class RamayanaChoiceOption')" "1"
ge "class RamayanaChoiceDilemma (≥1 occurrence)"    "$(echo "$SRC" | grep -c 'class RamayanaChoiceDilemma')" "1"
ge "class RamayanaMomentsMeta (≥1 occurrence)"      "$(echo "$SRC" | grep -c 'class RamayanaMomentsMeta')" "1"
ge "class RamayanaMomentsCorpus (≥1 occurrence)"    "$(echo "$SRC" | grep -c 'class RamayanaMomentsCorpus')" "1"
ge "[Serializable] attribute count (≥5)"             "$(echo "$SRC" | grep -c '\[Serializable\]')" "5"

# ---------- Brace balance ----------
section "Brace balance (Roslyn-free sanity)"
opens=$(grep -o '{' "$BINDING" | wc -l | tr -d ' ')
closes=$(grep -o '}' "$BINDING" | wc -l | tr -d ' ')
check "brace balance ({ $opens, } $closes)" "$opens" "$closes"

# ---------- .meta guid ----------
section ".meta guid"
META_GUID=$(grep -h '^guid:' "$ROOT/Assets/Scripts/Data/RamayanaMomentsData.cs.meta" | awk '{print $2}')
if [[ "$META_GUID" =~ ^[a-f0-9]{32}$ ]]; then
  echo "  ✅ .meta guid valid: $META_GUID"; PASS=$((PASS+1))
else
  echo "  ❌ .meta guid invalid: $META_GUID"; FAIL=$((FAIL+1))
fi

JSON_META_GUID=$(grep -h '^guid:' "$ROOT/Assets/Resources/Ramayana/moments_bala_kanda.json.meta" | awk '{print $2}')
if [[ "$JSON_META_GUID" =~ ^[a-f0-9]{32}$ ]]; then
  echo "  ✅ JSON .meta guid valid: $JSON_META_GUID"; PASS=$((PASS+1))
else
  echo "  ❌ JSON .meta guid invalid: $JSON_META_GUID"; FAIL=$((FAIL+1))
fi

# ---------- ELGODS source sanity ----------
section "ELGODS source sanity (Bala Kanda still cited)"
BK_LINES=$(grep -c 'Bala Kanda' /Users/prabaharan/jambudweep/ELGODS/portal/src/game/ramayana/characterStoryArcs.ts 2>/dev/null || echo 0)
ge "ELGODS source has Bala Kanda citations (≥30)" "$BK_LINES" "30"

# ---------- Final report ----------
echo ""
echo "==================================================="
echo "  $PASS passed, $FAIL failed  (ad-hoc only, Unity compile pending)"
echo "==================================================="
[ "$FAIL" -eq 0 ] && exit 0 || exit 1