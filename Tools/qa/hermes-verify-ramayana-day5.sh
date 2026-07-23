#!/bin/bash
# hermes-verify-ramayana-day5.sh
#
# Day 5 ad-hoc verifier for QA hardening:
#   - RamayanaWireupTests.cs exists + covers all 7 Day 1-4 types.
#   - hermes-verify-ramayana-all.sh umbrella exists.
#   - Existing EditMode test asmdef is intact and references TestRunner.
#   - Cross-day integration signatures are reflected in the test file.
#
# Ad-hoc only. The EditMode tests themselves are run via Unity Test
# Framework in batchmode; that path is documented in
# Tools/qa/hermes-verify-ramayana-editmode-tests.sh.

set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
TESTS="$ROOT/Assets/Tests/EditMode/RamayanaWireupTests.cs"
TESTS_META="$ROOT/Assets/Tests/EditMode/RamayanaWireupTests.cs.meta"
ASMDEF="$ROOT/Assets/Tests/EditMode/Ramayana.EditMode.Tests.asmdef"
UMBRELLA="$ROOT/Tools/qa/hermes-verify-ramayana-all.sh"
CORPUS_TESTS="$ROOT/Assets/Tests/EditMode/CorpusDataTests.cs"

PASS=0; FAIL=0
section() { echo ""; echo "== $1 =="; }
check()    { local n="$1" actual="$2" expected="$3"
  if [ "$actual" = "$expected" ]; then echo "  ✅ $n  ($actual)"; PASS=$((PASS+1))
  else echo "  ❌ $n  expected=$expected actual=$actual"; FAIL=$((FAIL+1)); fi
}
ge() { local n="$1" actual="$2" min="$3"
  if [ "$actual" -ge "$min" ] 2>/dev/null; then echo "  ✅ $n  ($actual ≥ $min)"; PASS=$((PASS+1))
  else echo "  ❌ $n  expected≥$min actual=$actual"; FAIL=$((FAIL+1)); fi
}
file_check() { local n="$1" p="$2"
  if [ "$p" = "yes" ]; then echo "  ✅ $n"; PASS=$((PASS+1))
  else echo "  ❌ $n  (not found)"; FAIL=$((FAIL+1)); fi
}

section "Day 5 — files exist"
file_check "RamayanaWireupTests.cs"            "$([ -f "$TESTS" ] && echo yes || echo no)"
file_check "RamayanaWireupTests.cs.meta"       "$([ -f "$TESTS_META" ] && echo yes || echo no)"
file_check "Ramayana.EditMode.Tests.asmdef"    "$([ -f "$ASMDEF" ] && echo yes || echo no)"
file_check "hermes-verify-ramayana-all.sh"     "$([ -f "$UMBRELLA" ] && echo yes || echo no)"
file_check "CorpusDataTests.cs (pre-existing)" "$([ -f "$CORPUS_TESTS" ] && echo yes || echo no)"

if [ -f "$TESTS_META" ]; then
  GUID=$(grep -oE 'guid: [a-f0-9]{32}' "$TESTS_META" | awk '{print $2}')
  check "RamayanaWireupTests guid 32 hex" "$([ "${#GUID}" -eq 32 ] && echo yes || echo no)" "yes"
fi

section "RamayanaWireupTests.cs surface"
TSRC="$([ -f "$TESTS" ] && cat "$TESTS" || echo "")"
check "namespace Ramayana.Tests" \
  "$(echo "$TSRC" | grep -cE '^namespace Ramayana\.Tests\s*$')" "1"
check "class RamayanaWireupTests" \
  "$(echo "$TSRC" | grep -cE 'public class RamayanaWireupTests')" "1"
ge "uses NUnit.Framework (≥1)" \
  "$(echo "$TSRC" | grep -cE 'using NUnit\.Framework')" "1"
ge "[Test] attribute uses (≥8)" \
  "$(echo "$TSRC" | grep -cE '\[Test\]')" "8"
ge "Type.GetType lookups (≥7)" \
  "$(echo "$TSRC" | grep -cE 'Type\.GetType\(')" "7"

section "Test method names cover Day 1-4"
ge "Day 1 tests (MainMenu) (≥1)" \
  "$(echo "$TSRC" | grep -cE 'Day1_MainMenuScreenController_')" "1"
ge "Day 2 tests (StoryMoment) (≥2)" \
  "$(echo "$TSRC" | grep -cE 'Day2_StoryMomentPlayer_')" "2"
ge "Day 3 tests (Combat) (≥3)" \
  "$(echo "$TSRC" | grep -cE 'Day3_(RakshasaTarget|WaveController|ArcherAutoFire)_')" "3"
ge "Day 4 tests (HUD + Save) (≥2)" \
  "$(echo "$TSRC" | grep -cE 'Day4_(HudOrchestrator|SaveLoadHud)_')" "2"
check "Cross-day invariant test" \
  "$(echo "$TSRC" | grep -cE 'Cross_AllNewTypes_AreSealedMonoBehaviours')" "1"

section "Existing EditMode asmdef intact"
ASM=$(cat "$ASMDEF" 2>/dev/null)
check "asmdef name Ramayana.EditMode.Tests" \
  "$(echo "$ASM" | grep -cE '"name": "Ramayana\.EditMode\.Tests"')" "1"
check "references UnityEngine.TestRunner" \
  "$(echo "$ASM" | grep -cE 'UnityEngine\.TestRunner')" "1"
check "references UnityEditor.TestRunner" \
  "$(echo "$ASM" | grep -cE 'UnityEditor\.TestRunner')" "1"
check "nunit.framework.dll precompiled ref" \
  "$(echo "$ASM" | grep -cE 'nunit\.framework\.dll')" "1"
check "includePlatforms: Editor (multi-line JSON)" \
  "$(echo "$ASM" | tr -d '\n' | grep -cE '"includePlatforms":[^]]*"Editor"')" "1"
check "defineConstraints: UNITY_INCLUDE_TESTS (multi-line JSON)" \
  "$(echo "$ASM" | tr -d '\n' | grep -cE '"defineConstraints":[^]]*"UNITY_INCLUDE_TESTS"')" "1"

section "umbrella hermes-verify-ramayana-all.sh surface"
USRC=$(cat "$UMBRELLA" 2>/dev/null)
check "executable bit set" \
  "$([ -x "$UMBRELLA" ] && echo yes || echo no)" "yes"
ge "iterates Days 1-4 (umbrella includes '1 2 3 4' substring)" \
  "$(echo "$USRC" | grep -cE 'DAYS=\([^)]*\b1\b \b2\b \b3\b \b4\b')" "1"
ge "calls each day verifier via HERMES_VERIFY_ROOT (≥1)" \
  "$(echo "$USRC" | grep -cE 'HERMES_VERIFY_ROOT=')" "1"
ge "reports aggregate TOTAL_PASS + TOTAL_FAIL (≥2)" \
  "$(echo "$USRC" | grep -cE 'TOTAL_(PASS|FAIL)')" "2"

section "Cross-day invariant — types Day 1-4 exist in Day 5 test"
for type in \
    "Jambudweep.Ramayana.UI.MainMenuScreenController" \
    "Jambudweep.Ramayana.Story.StoryMomentPlayer" \
    "Jambudweep.Ramayana.Combat.RakshasaTarget" \
    "Jambudweep.Ramayana.Combat.WaveController" \
    "Jambudweep.Ramayana.Combat.ArcherAutoFire" \
    "Jambudweep.Ramayana.Feedback.HudOrchestrator" \
    "Jambudweep.Ramayana.UI.SaveLoadHud"; do
  ge "  $type referenced in RamayanaWireupTests (≥1)" \
    "$(echo "$TSRC" | grep -cE "$(echo "$type" | sed 's/\./\\./g')")" "1"
done

section "Summary"
echo "  $PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
