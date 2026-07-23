#!/bin/bash
# hermes-verify-ramayana-day10.sh
#
# Day 10 ad-hoc verifier — Chakra / Vyuha formation strategies:
#   - FormationStrategy abstract + Arc/Chakra/Vyuha sealed concretes
#   - FormationKind enum
#   - WaveController SetFormation + formationKind + SpawnPoints usage
#   - EditMode Day 10 surface
#
# Ad-hoc only. Unity Editor compile is the only true regression signal.
set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
FORM="$ROOT/Assets/Scripts/Combat/FormationStrategy.cs"
WAVE="$ROOT/Assets/Scripts/Combat/WaveController.cs"
TESTS="$ROOT/Assets/Tests/EditMode/RamayanaWireupTests.cs"

PASS=0; FAIL=0
section() { echo ""; echo "== $1 =="; }
check() { local n="$1" a="$2" e="$3"
  if [ "$a" = "$e" ]; then echo "  ✅ $n  ($a)"; PASS=$((PASS+1))
  else echo "  ❌ $n  expected=$e actual=$a"; FAIL=$((FAIL+1)); fi
}
ge() { local n="$1" a="$2" m="$3"
  if [ "$a" -ge "$m" ] 2>/dev/null; then echo "  ✅ $n  ($a ≥ $m)"; PASS=$((PASS+1))
  else echo "  ❌ $n  expected≥$m actual=$a"; FAIL=$((FAIL+1)); fi
}
file_check() { local n="$1" p="$2"
  if [ "$p" = "yes" ]; then echo "  ✅ $n"; PASS=$((PASS+1))
  else echo "  ❌ $n"; FAIL=$((FAIL+1)); fi
}

section "Day 10 — files"
file_check "FormationStrategy.cs" "$([ -f "$FORM" ] && echo yes || echo no)"
file_check "FormationStrategy.cs.meta" "$([ -f "$FORM.meta" ] && echo yes || echo no)"
file_check "WaveController.cs" "$([ -f "$WAVE" ] && echo yes || echo no)"
if [ -f "$FORM.meta" ]; then
  GUID=$(grep -oE 'guid: [a-f0-9]{32}' "$FORM.meta" | awk '{print $2}')
  check "meta guid 32hex" "$([ ${#GUID} -eq 32 ] && echo yes || echo no)" "yes"
fi

section "FormationStrategy surface"
SRC=$(cat "$FORM" 2>/dev/null || echo "")
check "namespace Combat" "$(echo "$SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Combat\s*$')" "1"
check "enum FormationKind" "$(echo "$SRC" | grep -cE 'public enum FormationKind')" "1"
check "abstract FormationStrategy" "$(echo "$SRC" | grep -cE 'public abstract class FormationStrategy')" "1"
check "ArcFormation sealed" "$(echo "$SRC" | grep -cE 'public sealed class ArcFormation')" "1"
check "ChakraFormation sealed" "$(echo "$SRC" | grep -cE 'public sealed class ChakraFormation')" "1"
check "VyuhaFormation sealed" "$(echo "$SRC" | grep -cE 'public sealed class VyuhaFormation')" "1"
check "SpawnPoints abstract API" "$(echo "$SRC" | grep -cE 'public abstract Vector3\[\] SpawnPoints')" "1"
check "For factory" "$(echo "$SRC" | grep -cE 'public static FormationStrategy For\(FormationKind')" "1"
# No MonoBehaviour — pure strategy objects
check "no MonoBehaviour inheritance on strategy" "$(echo "$SRC" | grep -cE 'FormationStrategy : MonoBehaviour')" "0"

section "WaveController formation wiring"
SRC=$(cat "$WAVE" 2>/dev/null || echo "")
ge "formationKind field (≥1)" "$(echo "$SRC" | grep -cE 'formationKind')" "1"
check "SetFormation API" "$(echo "$SRC" | grep -cE 'public void SetFormation\(FormationKind')" "1"
ge "FormationStrategy.For (≥1)" "$(echo "$SRC" | grep -cE 'FormationStrategy\.For')" "1"
ge "SpawnPoints call (≥1)" "$(echo "$SRC" | grep -cE 'SpawnPoints\(')" "1"
ge "escalates to Chakra (≥1)" "$(echo "$SRC" | grep -cE 'FormationKind\.Chakra')" "1"
ge "escalates to Vyuha (≥1)" "$(echo "$SRC" | grep -cE 'FormationKind\.Vyuha')" "1"
check "still sealed WaveController" "$(echo "$SRC" | grep -cE 'public sealed class WaveController')" "1"
check "StartWaves still present" "$(echo "$SRC" | grep -cE 'public void StartWaves\(int totalWaves = 1\)')" "1"

section "EditMode Day 10"
SRC=$(cat "$TESTS" 2>/dev/null || echo "")
ge "Day10 tests (≥2)" "$(echo "$SRC" | grep -cE 'Day10_')" "2"

section "Summary"
echo "  $PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
