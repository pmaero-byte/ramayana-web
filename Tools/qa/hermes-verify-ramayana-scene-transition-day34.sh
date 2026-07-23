#!/bin/bash
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

GB="$ROOT/Assets/Scripts/Core/GameBootstrap.cs"
[ -f "$GB" ] && p "GameBootstrap.cs exists" || f "GameBootstrap.cs missing"
SRC_GB=$(cat "$GB" 2>/dev/null || true)
echo "$SRC_GB" | grep -q 'SceneManager.LoadScene' && p "GameBootstrap LoadScene" || f "GameBootstrap missing LoadScene"
echo "$SRC_GB" | grep -q 'TitleScreen' && p "GameBootstrap TitleScene" || f "GameBootstrap missing TitleScreen"
echo "$SRC_GB" | grep -q 'mostRecent' && p "GameBootstrap resume logic" || f "GameBootstrap missing resume logic"
echo "$SRC_GB" | grep -q 'TODO: scene transition' && f "GameBootstrap has open TODO" || p "GameBootstrap no open scene TODO"

TT="$ROOT/Assets/Scripts/UI/TitleScreenTapZone.cs"
[ -f "$TT" ] && p "TitleScreenTapZone.cs exists" || f "TitleScreenTapZone.cs missing"
SRC_TT=$(cat "$TT" 2>/dev/null || true)
echo "$SRC_TT" | grep -q 'SceneManager' && p "TitleScreenTapZone LoadScene" || f "TitleScreenTapZone missing LoadScene"
echo "$SRC_TT" | grep -q 'MainMenu' && p "TitleScreenTapZone target MainMenu" || f "TitleScreenTapZone missing MainMenu"
echo "$SRC_TT" | grep -q 'namespace Jambudweep.Ramayana.UI' && p "TitleScreenTapZone namespace" || f "TitleScreenTapZone wrong namespace"

bash -n "$ROOT/Tools/qa/hermes-verify-audio-day33.sh" 2>/dev/null && p "Day 33 audio verifier bash -n" || f "Day 33 audio verifier bash -n"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-save-slots-day24.sh" 2>/dev/null && p "Day 24 save verifier bash -n" || f "Day 24 save verifier bash -n"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-editmode-tests-day25.sh" 2>/dev/null && p "Day 25 editmode verifier bash -n" || f "Day 25 editmode verifier bash -n"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-yuddha-kanda-day29.sh" 2>/dev/null && p "Day 29 corpus verifier bash -n" || f "Day 29 corpus verifier bash -n"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-return-kanda-day31.sh" 2>/dev/null && p "Day 31 corpus verifier bash -n" || f "Day 31 corpus verifier bash -n"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 34 scene transition)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
