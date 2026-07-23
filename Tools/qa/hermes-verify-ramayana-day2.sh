#!/bin/bash
# hermes-verify-ramayana-day2.sh
#
# Day 2 ad-hoc verifier for StoryMomentPlayer.
# Confirms:
#   - File + meta exist with valid guid.
#   - Required surface (namespace, sealed class, MonoBehaviour, public API,
#     objective walker, overlay wiring, storyEngine wiring).
#   - Corpus still has objectives populated for the act we'd boot first.
#
# Ad-hoc only. Unity Editor compile + Play-mode smoke live in the
# hermes-verify-ramayana-editmode-tests.sh suite.
set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
FILE="$ROOT/Assets/Scripts/Story/StoryMomentPlayer.cs"
META="$ROOT/Assets/Scripts/Story/StoryMomentPlayer.cs.meta"
CORPUS="$ROOT/Assets/Resources/corpus_data.json"

PASS=0; FAIL=0
section() { echo ""; echo "== $1 =="; }
check()    { local name="$1" actual="$2" expected="$3"
  if [ "$actual" = "$expected" ]; then echo "  ✅ $name  ($actual)"; PASS=$((PASS+1))
  else echo "  ❌ $name  expected=$expected actual=$actual"; FAIL=$((FAIL+1)); fi
}
file_check() { local name="$1" present="$2"
  if [ "$present" = "yes" ]; then echo "  ✅ $name"; PASS=$((PASS+1))
  else echo "  ❌ $name  (not found)"; FAIL=$((FAIL+1)); fi
}

section "Day 2 — StoryMomentPlayer"
file_check "source file exists"      "$([ -f "$FILE" ] && echo yes || echo no)"
file_check "meta sidecar exists"     "$([ -f "$META" ] && echo yes || echo no)"
file_check "corpus_data.json exists" "$([ -f "$CORPUS" ] && echo yes || echo no)"

if [ -f "$META" ]; then
  GUID=$(grep -oE 'guid: [a-f0-9]{32}' "$META" | awk '{print $2}')
  check "meta guid is 32 hex chars" "$([ "${#GUID}" -eq 32 ] && echo yes || echo no)" "yes"
fi

section "Source-level surface"
SRC="$([ -f "$FILE" ] && cat "$FILE" || echo "")"
check "namespace Jambudweep.Ramayana.Story" \
  "$(echo "$SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Story\s*$')" "1"
check "class StoryMomentPlayer sealed" \
  "$(echo "$SRC" | grep -cE 'public sealed class StoryMomentPlayer')" "1"
check "inherits MonoBehaviour" \
  "$(echo "$SRC" | grep -cE ': MonoBehaviour')" "1"
check "LoadAct(actId) public API" \
  "$(echo "$SRC" | grep -cE 'public bool LoadAct\(string actId\)')" "1"
check "AdvanceObjective() public API" \
  "$(echo "$SRC" | grep -cE 'public bool AdvanceObjective\(\)')" "1"
check "CompleteCurrentObjective() public API" \
  "$(echo "$SRC" | grep -cE 'public void CompleteCurrentObjective\(\)')" "1"
check "CurrentObjective getter" \
  "$(echo "$SRC" | grep -cE 'public ObjectiveRecord CurrentObjective')" "1"
check "StoryEngine reference" \
  "$(echo "$SRC" | grep -cE 'private StoryEngine storyEngine')" "1"
check "DialogueOverlay reference" \
  "$(echo "$SRC" | grep -cE 'private DialogueOverlay overlay')" "1"
check "uses Resources.Load<TextAsset>" \
  "$(echo "$SRC" | grep -cE 'Resources\.Load<TextAsset>')" "1"
check "uses JsonUtility.FromJson<CorpusData>" \
  "$(echo "$SRC" | grep -cE 'JsonUtility\.FromJson<CorpusData>')" "1"
check "calls StoryEngine.BeginAct" \
  "$(echo "$SRC" | grep -cE 'storyEngine\?.BeginAct')" "1"
check "calls StoryEngine.CompleteObjective" \
  "$(echo "$SRC" | grep -cE 'storyEngine\?.CompleteObjective')" "1"
check "fires onObjectiveEntered UnityEvent" \
  "$(echo "$SRC" | grep -cE 'onObjectiveEntered\?.Invoke')" "1"
check "fires onObjectiveCompleted UnityEvent" \
  "$(echo "$SRC" | grep -cE 'onObjectiveCompleted\?.Invoke')" "1"
check "fires onActCompleted UnityEvent" \
  "$(echo "$SRC" | grep -cE 'onActCompleted\?.Invoke')" "1"

section "corpus_data.json shape"
BALA_OBJECTIVES=$(python3 -c "
import json
d = json.load(open('$CORPUS'))
for a in d.get('acts', []):
    if a.get('actId') == 'bala-birth':
        print(len(a.get('objectives', []))); break
" 2>/dev/null || echo 0)
echo "  bala-birth objectives: $BALA_OBJECTIVES"
[ "$BALA_OBJECTIVES" -ge 4 ] && echo "  ✅ bala-birth has ≥4 objectives" && PASS=$((PASS+1)) \
  || { echo "  ❌ bala-birth has <4 objectives"; FAIL=$((FAIL+1)); }

section "Summary"
echo "  $PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
