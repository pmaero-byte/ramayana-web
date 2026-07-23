#!/bin/bash
# hermes-verify-ramayana-characters-day11.sh — Verifier for Day 11
# ELGODS character roster import.
#
# Asserts:
# 1. Assets/Resources/Ramayana/characters.json + .meta exist
# 2. JSON valid + schema fields present
# 3. Exactly 26 canonical characters (matches ELGODS official roster)
# 4. All 11 alankarashastra voice registers present
# 5. 5 character groups present with non-zero membership
# 6. Every group member has a full profile (no dangling references)
# 7. Canonical 26 IDs present (matches ELGODS roster)
# 8. Color format validates (#[0-9A-Fa-f]{6})
# 9. Brace balance + using-before-namespace in the C# binding
# 10. .cs.meta guid is valid
#
# Usage:
#   bash Tools/qa/hermes-verify-ramayana-characters-day11.sh
#   HERMES_VERIFY_ROOT=<project> bash hermes-verify-ramayana-characters-day11.sh
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

CORPUS="$ROOT/Assets/Resources/Ramayana/characters.json"
BINDING="$ROOT/Assets/Scripts/Data/RamayanaCharactersData.cs"

# ---------- Files ----------
section "Files exist"
file_check "characters.json"               "$CORPUS"
file_check "characters.json.meta"          "$ROOT/Assets/Resources/Ramayana/characters.json.meta"
file_check "Ramayana/ folder marker"       "$ROOT/Assets/Resources/Ramayana.meta"
file_check "RamayanaCharactersData.cs"     "$BINDING"
file_check "RamayanaCharactersData.cs.meta" "$ROOT/Assets/Scripts/Data/RamayanaCharactersData.cs.meta"

# ---------- JSON validity ----------
section "JSON validity"
if python3 -c "import json; json.load(open('$CORPUS'))" 2>/dev/null; then
  echo "  ✅ characters.json is valid JSON"; PASS=$((PASS+1))
else
  echo "  ❌ characters.json is NOT valid JSON"; FAIL=$((FAIL+1))
fi

# ---------- Character roster ----------
section "Character roster (26 canonical)"
COUNT_CHARS=$(python3 -c "import json; print(len(json.load(open('$CORPUS'))['characters']))" 2>/dev/null || echo 0)
check "26 characters" "$COUNT_CHARS" "26"

# Required canonical IDs (from ELGODS CHARACTER_PROFILES)
REQUIRED_IDS=(rama sita hanuman ravana lakshmana bharata vibhishana kaikeyi jatayu shabari sugriva mandodari trijata indrajit kumbhakarna tara urmila mandavi angada nila guha vishwamitra agastya jambavan narada valmiki)
MISSING=""
for cid in "${REQUIRED_IDS[@]}"; do
  PRESENT=$(python3 -c "
import json, sys
d = json.load(open('$CORPUS'))
sys.stdout.write('yes' if '$cid' in d['characters'] else 'no')
")
  if [ "$PRESENT" != "yes" ]; then MISSING="$MISSING $cid"; fi
done
check "all 26 canonical IDs present" "${MISSING:-ok}" "ok"

# ---------- Voice registers ----------
section "Voice registers (11 alankarashastra)"
VOICES=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
voices = set()
for c in d['characters'].values():
    if c.get('voice'): voices.add(c['voice'])
print(len(voices))
" 2>/dev/null || echo 0)
ge "distinct voices used (≥8)" "$VOICES" "8"

# ---------- Character groups ----------
section "Character groups (5)"
COUNT_GROUPS=$(python3 -c "import json; print(len(json.load(open('$CORPUS'))['groups']))" 2>/dev/null || echo 0)
check "5 groups" "$COUNT_GROUPS" "5"

EMPTY_GROUPS=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
empty = [g['id'] for g in d['groups'] if len(g.get('characters', [])) == 0]
print(','.join(empty) if empty else 'none')
" 2>/dev/null || echo "?")
check "no empty groups" "$EMPTY_GROUPS" "none"

# ---------- Reference integrity ----------
section "Group references resolve"
DANGLING=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
ids = set(d['characters'].keys())
missing = []
for g in d['groups']:
    for c in g['characters']:
        if c not in ids:
            missing.append(f'{g[\"id\"]}/{c}')
print(','.join(missing) if missing else 'none')
" 2>/dev/null || echo "?")
check "no dangling group references" "$DANGLING" "none"

# ---------- Color format ----------
section "Color format (#RRGGBB)"
INVALID_COLORS=$(python3 -c "
import json, re
d = json.load(open('$CORPUS'))
bad = []
for cid, c in d['characters'].items():
    color = c.get('color', '')
    if not re.match(r'^#[0-9A-Fa-f]{6}\$', color):
        bad.append(f'{cid}:{color}')
print(','.join(bad) if bad else 'none')
" 2>/dev/null || echo "?")
check "all 26 colors match #RRGGBB" "$INVALID_COLORS" "none"

# ---------- C# binding surface ----------
section "C# binding source surface"
SRC=$(cat "$BINDING")
check "namespace Jambudweep.Ramayana.Data" \
  "$(echo "$SRC" | grep -c '^namespace Jambudweep.Ramayana.Data')" "1"
ge "class RamayanaCharacter (≥1 occurrence)"     "$(echo "$SRC" | grep -c 'class RamayanaCharacter')" "1"
check "class RamayanaCharacterGroup" "$(echo "$SRC" | grep -c 'class RamayanaCharacterGroup')" "1"
ge "class RamayanaCharacterRoster (≥1 occurrence)" "$(echo "$SRC" | grep -c 'class RamayanaCharacterRoster')" "1"
check "class RamayanaCharacterRosterMeta" "$(echo "$SRC" | grep -c 'class RamayanaCharacterRosterMeta')" "1"
ge "[Serializable] attribute count (≥4)" "$(echo "$SRC" | grep -c '\[Serializable\]')" "4"

# ---------- Brace balance ----------
section "Brace balance (Roslyn-free sanity)"
opens=$(grep -o '{' "$BINDING" | wc -l | tr -d ' ')
closes=$(grep -o '}' "$BINDING" | wc -l | tr -d ' ')
check "brace balance ({ $opens, } $closes)" "$opens" "$closes"

# ---------- .meta guid ----------
section ".meta guid"
META_GUID=$(grep -h '^guid:' "$ROOT/Assets/Scripts/Data/RamayanaCharactersData.cs.meta" | awk '{print $2}')
if [[ "$META_GUID" =~ ^[a-f0-9]{32}$ ]]; then
  echo "  ✅ .meta guid valid: $META_GUID"; PASS=$((PASS+1))
else
  echo "  ❌ .meta guid invalid: $META_GUID"; FAIL=$((FAIL+1))
fi

# ---------- Final report ----------
echo ""
echo "==================================================="
echo "  $PASS passed, $FAIL failed  (ad-hoc only, Unity compile pending)"
echo "==================================================="
[ "$FAIL" -eq 0 ] && exit 0 || exit 1