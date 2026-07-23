#!/bin/bash
# hermes-verify-ramayana-day6.sh
#
# Day 6 ad-hoc verifier for PlayerSceneBootstrap:
#   - The bootstrap component exists and is sealed : MonoBehaviour.
#   - It exposes the four boot option SerializeFields.
#   - It exposes both UnityEvents (onPlayerReady + onSceneBootstrapped).
#   - Its source references each Day 1-4 system type.
#   - The EditMode test surface covers Day 6.

set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
BS="$ROOT/Assets/Scripts/Scene/PlayerSceneBootstrap.cs"
BS_META="$ROOT/Assets/Scripts/Scene/PlayerSceneBootstrap.cs.meta"
TESTS="$ROOT/Assets/Tests/EditMode/RamayanaWireupTests.cs"

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

section "Day 6 — files exist"
file_check "Assets/Scripts/Scene/PlayerSceneBootstrap.cs" \
  "$([ -f "$BS" ] && echo yes || echo no)"
file_check "Assets/Scripts/Scene/PlayerSceneBootstrap.cs.meta" \
  "$([ -f "$BS_META" ] && echo yes || echo no)"

if [ -f "$BS_META" ]; then
  GUID=$(grep -oE 'guid: [a-f0-9]{32}' "$BS_META" | awk '{print $2}')
  check "PlayerSceneBootstrap guid 32 hex" "$([ "${#GUID}" -eq 32 ] && echo yes || echo no)" "yes"
fi

section "Day 6 — PlayerSceneBootstrap surface"
SRC="$([ -f "$BS" ] && cat "$BS" || echo "")"
check "namespace Jambudweep.Ramayana.Scene" \
  "$(echo "$SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Scene\s*$')" "1"
check "class PlayerSceneBootstrap sealed" \
  "$(echo "$SRC" | grep -cE 'public sealed class PlayerSceneBootstrap')" "1"
check "inherits MonoBehaviour" \
  "$(echo "$SRC" | grep -cE ': MonoBehaviour')" "1"
ge "boot-option SerializeFields (≥4)" \
  "$(echo "$SRC" | grep -cE '\[SerializeField[^]]*\]')" "4"
check "autoLoadActId present" \
  "$(echo "$SRC" | grep -cE 'private string autoLoadActId')" "1"
check "autoStartWaves present" \
  "$(echo "$SRC" | grep -cE 'private int autoStartWaves')" "1"
check "mountSaveLoadHud present" \
  "$(echo "$SRC" | grep -cE 'private bool mountSaveLoadHud')" "1"
check "ensureMinimalScene present" \
  "$(echo "$SRC" | grep -cE 'private bool ensureMinimalScene')" "1"
check "onPlayerReady UnityEvent" \
  "$(echo "$SRC" | grep -cE 'public UnityEvent onPlayerReady')" "1"
check "onSceneBootstrapped UnityEvent" \
  "$(echo "$SRC" | grep -cE 'public UnityEvent onSceneBootstrapped')" "1"
check "Awake() present" \
  "$(echo "$SRC" | grep -cE 'void Awake\(\)')" "1"
check "Start() present" \
  "$(echo "$SRC" | grep -cE 'void Start\(\)')" "1"
check "EnsureScene helper" \
  "$(echo "$SRC" | grep -cE 'private void EnsureScene\(\)')" "1"
check "EnsureComponents helper" \
  "$(echo "$SRC" | grep -cE 'private void EnsureComponents\(\)')" "1"
check "BindEvents helper" \
  "$(echo "$SRC" | grep -cE 'private void BindEvents\(\)')" "1"

section "Cross-system wiring (Day 1-4 references)"
for type in StoryMomentPlayer WaveController HudOrchestrator SaveLoadHud ThirdPersonMotionController; do
  ge "$type referenced in source (≥1)" \
    "$(echo "$SRC" | grep -cE "\\b$type\\b")" "1"
done

section "EditMode test surface for Day 6"
TSRC="$([ -f "$TESTS" ] && cat "$TESTS" || echo "")"
ge "Day6 test methods (≥2)" \
  "$(echo "$TSRC" | grep -cE 'Day6_PlayerSceneBootstrap_')" "2"
ge "PlayerSceneBootstrap type referenced in test file (≥1)" \
  "$(echo "$TSRC" | grep -cE 'PlayerSceneBootstrap')" "1"

section "Unity field refs in test"
check "autoLoadActId GetField check" \
  "$(echo "$TSRC" | grep -cE '"autoLoadActId"')" "1"
check "autoStartWaves GetField check" \
  "$(echo "$TSRC" | grep -cE '"autoStartWaves"')" "1"
check "mountSaveLoadHud GetField check" \
  "$(echo "$TSRC" | grep -cE '"mountSaveLoadHud"')" "1"
check "ensureMinimalScene GetField check" \
  "$(echo "$TSRC" | grep -cE '"ensureMinimalScene"')" "1"

section "Summary"
echo "  $PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
