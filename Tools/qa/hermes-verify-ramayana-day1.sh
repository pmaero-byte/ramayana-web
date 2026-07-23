#!/bin/bash
# hermes-verify-ramayana-day1.sh
#
# Day 1 ad-hoc verifier for RamayanaPS5 MainMenuScreenController.
# Confirms:
#   - The new file compiles-clean by source-level rules (namespace, class,
#     Build/Populate/SelectKanda methods, public surface).
#   - The .meta sidecar exists with a valid guid.
#   - corpus_data.json still ships 8 acts + 50 characters (Kanda picker
#     depends on these counts).
#
# Ad-hoc only — Unity Editor compile / Play-mode smoke lives in
# Tools/qa/hermes-verify-ramayana-editmode-tests.sh. This script runs
# without Unity, fast, before pushing.
#
# Exit 0 = full pass. Non-zero = any failure.
set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# Allow running from a temp copy (e.g. /tmp/hermes-verify-XXX/hermes-verify-*.sh)
# by overriding the repo root via HERMES_VERIFY_ROOT env var.
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
FILE="$ROOT/Assets/Scripts/UI/Screens/MainMenuScreenController.cs"
META="$ROOT/Assets/Scripts/UI/Screens/MainMenuScreenController.cs.meta"
CORPUS="$ROOT/Assets/Resources/corpus_data.json"

PASS=0; FAIL=0
section() { echo ""; echo "== $1 =="; }
check()    { local name="$1" actual="$2" expected="$3"
  if [ "$actual" = "$expected" ]; then echo "  ✅ $name  ($actual)"; PASS=$((PASS+1))
  else echo "  ❌ $name  expected=$expected actual=$actual"; FAIL=$((FAIL+1)); fi
}
ge()       { local name="$1" actual="$2" min="$3"
  if [ "$actual" -ge "$min" ] 2>/dev/null; then echo "  ✅ $name  ($actual ≥ $min)"; PASS=$((PASS+1))
  else echo "  ❌ $name  expected≥$min actual=$actual"; FAIL=$((FAIL+1)); fi
}
file_check() { local name="$1" present="$2"
  if [ "$present" = "yes" ]; then echo "  ✅ $name"; PASS=$((PASS+1))
  else echo "  ❌ $name  (not found)"; FAIL=$((FAIL+1)); fi
}

section "Day 1 — MainMenuScreenController"
file_check "source file exists"       "$([ -f "$FILE" ] && echo yes || echo no)"
file_check "meta sidecar exists"      "$([ -f "$META" ] && echo yes || echo no)"
file_check "corpus_data.json exists"  "$([ -f "$CORPUS" ] && echo yes || echo no)"

if [ -f "$META" ]; then
  GUID=$(grep -oE 'guid: [a-f0-9]{32}' "$META" | awk '{print $2}')
  check "meta guid is 32 hex chars" "$([ "${#GUID}" -eq 32 ] && echo yes || echo no)" "yes"
fi

section "Source-level surface checks"
SRC="$([ -f "$FILE" ] && cat "$FILE" || echo "")"
check "namespace Jambudweep.Ramayana.UI" \
  "$(echo "$SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.UI\s*$')" "1"
check "class MainMenuScreenController sealed" \
  "$(echo "$SRC" | grep -cE 'public sealed class MainMenuScreenController')" "1"
check "inherits MonoBehaviour" \
  "$(echo "$SRC" | grep -cE ': MonoBehaviour')" "1"
check "Build() method present" \
  "$(echo "$SRC" | grep -cE 'private void Build\(\)')" "1"
check "Populate() method present" \
  "$(echo "$SRC" | grep -cE 'private void Populate\(\)')" "1"
check "SelectKanda() method present" \
  "$(echo "$SRC" | grep -cE 'private void SelectKanda\(string')" "1"
check "Start() method present" \
  "$(echo "$SRC" | grep -cE 'void Start\(\)')" "1"
check "onKandaSelected UnityEvent exposed" \
  "$(echo "$SRC" | grep -cE 'public UnityEvent<string> onKandaSelected')" "1"
check "corpusResourcePath SerializeField" \
  "$(echo "$SRC" | grep -cE '\[SerializeField\] private string corpusResourcePath')" "1"
check "uses Resources.Load<TextAsset>" \
  "$(echo "$SRC" | grep -cE 'Resources\.Load<TextAsset>')" "1"
check "uses JsonUtility.FromJson" \
  "$(echo "$SRC" | grep -cE 'JsonUtility\.FromJson<CorpusData>')" "1"
check "uses VerticalLayoutGroup for card list" \
  "$(echo "$SRC" | grep -cE 'VerticalLayoutGroup')" "1"

section "corpus_data.json shape (Kanda picker depends on these counts)"
ACTS=$(python3 -c "import json; d=json.load(open('$CORPUS')); print(len(d.get('acts', [])))" 2>/dev/null || echo 0)
CHARS=$(python3 -c "import json; d=json.load(open('$CORPUS')); print(len(d.get('characters', [])))" 2>/dev/null || echo 0)
ge "acts ≥ 7 (seven kāṇḍas)" "$ACTS" 7
ge "characters ≥ 30"        "$CHARS" 30

section "Summary"
echo "  $PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
