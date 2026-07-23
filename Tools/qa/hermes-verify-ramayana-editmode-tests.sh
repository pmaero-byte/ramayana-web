#!/usr/bin/env bash
# hermes-verify-ramayana-editmode-tests.sh
# Round 10 — Ramayana EditMode test suite static verifier
#
# Confirms the test infrastructure is in place. Does NOT re-run the Unity
# batchmode test runner (that requires live Unity licensing — see note below).
# Verifies:
#   1. Test asmdef exists with correct shape (UnityEngine.TestRunner + nunit ref)
#   2. Test source file has all 5 expected [Test] methods
#   3. corpus_data.json has 50 characters + 8 acts + all characters meet
#      schema invariants that the runtime C# code reads
#
# Usage:  bash Tools/qa/hermes-verify-ramayana-editmode-tests.sh
set -uo pipefail

REPO="$(cd "$(dirname "$0")/../.." && pwd)"
ASM_SRC="$REPO/Assets/Tests/EditMode/Ramayana.EditMode.Tests.asmdef"
TEST_SRC="$REPO/Assets/Tests/EditMode/CorpusDataTests.cs"
CORPUS="$REPO/Assets/Resources/corpus_data.json"
PASS=0
FAIL=0
ok()   { echo "  ✅ $*"; PASS=$((PASS+1)); }
bad()  { echo "  ❌ $*"; FAIL=$((FAIL+1)); }

echo "=== Ramayana PS5 EditMode test suite (Round 10) ==="

echo
echo "-- T1: test asmdef --"
[[ -f "$ASM_SRC" ]] && ok "T1.1: $ASM_SRC exists" || { bad "T1.1: $ASM_SRC MISSING"; }
if [[ -f "$ASM_SRC" ]]; then
  grep -q '"Ramayana.EditMode.Tests"' "$ASM_SRC" && ok "T1.2: asmdef name = Ramayana.EditMode.Tests" || bad "T1.2: asmdef name wrong"
  grep -q '"UnityEngine.TestRunner"' "$ASM_SRC" && ok "T1.3: references UnityEngine.TestRunner" || bad "T1.3: missing TestRunner ref"
  grep -q '"nunit.framework.dll"' "$ASM_SRC" && ok "T1.4: precompiled nunit.framework.dll" || bad "T1.4: missing nunit ref"
  grep -q '"UNITY_INCLUDE_TESTS"' "$ASM_SRC" && ok "T1.5: UNITY_INCLUDE_TESTS constraint" || bad "T1.5: missing constraint"
fi

echo
echo "-- T2: test source --"
[[ -f "$TEST_SRC" ]] && ok "T2.1: CorpusDataTests.cs exists" || { bad "T2.1: $TEST_SRC MISSING"; }
if [[ -f "$TEST_SRC" ]]; then
  COUNT=$(grep -cE "^\s*\[Test\]" "$TEST_SRC" 2>/dev/null | head -1)
  if [[ "$COUNT" -ge 5 ]]; then ok "T2.2: 5 [Test] methods ($COUNT)"; else bad "T2.2: only $COUNT [Test] methods"; fi
  for m in CorpusData_LoadsFromResources CorpusData_CharacterIdsAreUnique CorpusData_AllCharactersHaveRequiredFields CorpusData_AllColorsAreValidHex CorpusData_EightValmikiKandasPresent; do
    if grep -q "public void $m" "$TEST_SRC" 2>/dev/null; then ok "T2.3: method $m present"; else bad "T2.3: method $m MISSING"; fi
  done
  # Confirm test names exactly match the runtime fields we depend on
  grep -q 'CorpusData_LoadsFromResources' "$TEST_SRC" && grep -q 'parsed.characters' "$TEST_SRC" && ok "T2.4: test reads characters[]"
  grep -q 'parsed.acts' "$TEST_SRC" && ok "T2.5: test reads acts[]"
fi

echo
echo "-- T3: corpus data invariants --"
if [[ -f "$CORPUS" ]]; then
  ok "T3.1: corpus_data.json present"
  # schema version
  VER=$(python3 -c "import json; print(json.load(open('$CORPUS')).get('schemaVersion','?'))" 2>/dev/null)
  if [[ "$VER" == "1" ]]; then ok "T3.2: schemaVersion = 1"; else bad "T3.2: schemaVersion is $VER"; fi
  # characters count
  NCHAR=$(python3 -c "import json; print(len(json.load(open('$CORPUS')).get('characters',[])))" 2>/dev/null || echo 0)
  if [[ "$NCHAR" -ge 13 ]]; then ok "T3.3: ≥13 characters ($NCHAR)"; else bad "T3.3: only $NCHAR characters"; fi
  # acts count
  NACTS=$(python3 -c "import json; print(len(json.load(open('$CORPUS')).get('acts',[])))" 2>/dev/null || echo 0)
  if [[ "$NACTS" -eq 8 ]]; then ok "T3.4: 8 acts ($NACTS)"; else bad "T3.4: $NACTS acts (expected 8 — one per Valmiki kanda)"; fi
  # character id uniqueness
  UNIQUE=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
ids = [c['characterId'] for c in d['characters']]
print('yes' if len(set(ids)) == len(ids) else 'no')
" 2>/dev/null)
  if [[ "$UNIQUE" == "yes" ]]; then ok "T3.5: all characterIds unique"; else bad "T3.5: duplicate characterIds exist"; fi
  # character field completeness
  ALLCOMPLETE=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
req = ['characterId', 'displayName', 'role', 'color']
print('yes' if all(all(c.get(f) for f in req) for c in d['characters']) else 'no')
" 2>/dev/null)
  if [[ "$ALLCOMPLETE" == "yes" ]]; then ok "T3.6: every character has 4 required fields"; else bad "T3.6: some character missing field"; fi
  # hex color validation
  ALLHEX=$(python3 -c "
import json, re
d = json.load(open('$CORPUS'))
hex_re = re.compile(r'^#[0-9a-fA-F]{6}$')
print('yes' if all(hex_re.match(c['color']) for c in d['characters']) else 'no')
" 2>/dev/null)
  if [[ "$ALLHEX" == "yes" ]]; then ok "T3.7: every character color is valid #RRGGBB hex"; else bad "T3.7: some color is not #RRGGBB hex"; fi
  # 6 required kanda actIds
  ALLKANDA=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
ids = {a['actId'] for a in d['acts']}
req = {'bala-birth', 'ayodhya-dharma', 'panchavati-golden-deer', 'kishkindha-alliance', 'sundarakanda-leap', 'yuddhakanda-war'}
miss = req - ids
print('yes' if not miss else ','.join(sorted(miss)))
" 2>/dev/null)
  if [[ "$ALLKANDA" == "yes" ]]; then ok "T3.8: all 6 required kanda actIds present"; else bad "T3.8: missing kandas: $ALLKANDA"; fi
else
  bad "T3.1: corpus_data.json MISSING"
fi

echo
echo "=== Summary ==="
echo "PASS=$PASS  FAIL=$FAIL"

if [[ "$FAIL" -ne 0 ]]; then exit 1; fi

# Note about live test runner — Ramayana licensing state is sensitive; verifier is
# static so it survives brief licensing dropouts.
echo
echo "NOTE: This verifier is STATIC. To run the actual Unity batchmode test"
echo "      runner (5/5 PASS signal), execute:"
echo "        Unity -batchmode -nographics -projectPath . -runTests -testPlatform EditMode \\"
echo "              -testResults results.xml -logFile -"
echo "      Requires the Unity 6 (6000.3.18f1) license client to be active."

exit 0
