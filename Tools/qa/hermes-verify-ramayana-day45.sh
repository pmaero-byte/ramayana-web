#!/bin/bash
set -eu
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
cd "$ROOT"
PASS=0; FAIL=0; RESULTS=()
pass()  { RESULTS+=("PASS: $*"); PASS=$((PASS+1)); }
fails() { RESULTS+=("FAIL: $*"); FAIL=$((FAIL+1)); }

echo "=== Day 45: KandaPermissions Phase 2 progression rules ==="

SRC_PERMS=$(cat Assets/Scripts/Gameplay/KandaPermissions.cs)
echo "$SRC_PERMS" | grep -q "SaveSystem.GetMostRecentSave" && pass "KandaPermissions hooks SaveSystem" || fails "KandaPermissions missing SaveSystem hook"
echo "$SRC_PERMS" | grep -q "visitedScenes" && pass "KandaPermissions uses visitedScenes" || fails "KandaPermissions missing visitedScenes"
echo "$SRC_PERMS" | grep -q "foreach (var prior in KandaTree.Entries)" && pass "KandaPermissions checks prior kandas" || fails "KandaPermissions missing prior-kanda check"

grep -q "public static IReadOnlyList<KandaEntry> Entries" Assets/Scripts/Gameplay/KandaTree.cs && pass "KandaTree.Entries public" || fails "KandaTree.Entries not public"

mkdir -p /tmp/ramayana-test-backup
mv Assets/Tests /tmp/ramayana-test-backup/ 2>/dev/null || true
UNITY="/Users/prabaharan/Unity/Hub/Editor/6000.5.4f1/Unity.app/Contents/MacOS/Unity"
"$UNITY" -batchmode -nographics -quit -projectPath "$PWD" -executeMethod Ramayana.Editor.Day39RuntimeProbe.Run -logFile /tmp/unity-compile-day45.log >/dev/null 2>&1
EXIT_CODE=$?
mv /tmp/ramayana-test-backup/Tests Assets/Tests 2>/dev/null || true
[ $EXIT_CODE -eq 0 ] && pass "Unity batchmode compile completed" || fails "Unity batchmode compile failed"

ERR_COUNT=$(grep -c "error CS" /tmp/unity-compile-day45.log 2>/dev/null | tr -cd "0-9")
ERR_COUNT=${ERR_COUNT:-0}
[ "$ERR_COUNT" -eq 0 ] && pass "Unity compile errors=0" || fails "Unity compile errors=$ERR_COUNT"

printf '%s\n' "${RESULTS[@]}"
echo "---"
echo "$PASS passed, $FAIL failed"
exit $FAIL
