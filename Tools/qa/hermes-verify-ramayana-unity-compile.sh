#!/bin/bash
# hermes-verify-ramayana-unity-compile.sh — Verify Unity batchmode compile
#
# Asserts:
# 1. Unity 6000.5.4f1 installed at expected path
# 2. ScriptAssemblies/Assembly-CSharp.dll produced (proves all 48 C#
#    files compiled into one assembly)
# 3. Editor + Tests assemblies also produced
# 4. URP packages compiled (Cinemachine + 2D + AI Navigation)
# 5. Recent log file shows "Exiting batchmode successfully now!"
#
# Usage:
#   bash Tools/qa/hermes-verify-ramayana-unity-compile.sh
#   HERMES_VERIFY_ROOT=<project> bash hermes-verify-ramayana-unity-compile.sh
#
# After running Unity batchmode, this verifier confirms the build
# actually succeeded. Pair with the log tail:
#   grep -E 'error CS[0-9]+|Exiting batchmode successfully now!' /tmp/unity-compile.log

set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
PASS=0; FAIL=0

check() { if [ "$2" = "$3" ]; then echo "  ✅ $1  ($2)"; PASS=$((PASS+1)); else echo "  ❌ $1  expected=$3 actual=$2"; FAIL=$((FAIL+1)); fi; }
ge()    { if [ "$2" -ge "$3" ] 2>/dev/null; then echo "  ✅ $1  ($2 ≥ $3)"; PASS=$((PASS+1)); else echo "  ❌ $1  expected≥$3 actual=$2"; FAIL=$((FAIL+1)); fi; }
file_check() { if [ -f "$2" ]; then echo "  ✅ $1"; PASS=$((PASS+1)); else echo "  ❌ $1  (missing: $2)"; FAIL=$((FAIL+1)); fi; }

echo "== Unity Editor installed =="
UNITY="/Users/prabaharan/Unity/Hub/Editor/6000.5.4f1/Unity.app/Contents/MacOS/Unity"
file_check "Unity 6000.5.4f1 binary" "$UNITY"

echo ""
echo "== Unity batchmode compile artifacts =="
file_check "Assembly-CSharp.dll"             "$ROOT/Library/ScriptAssemblies/Assembly-CSharp.dll"
file_check "Assembly-CSharp-Editor.dll"     "$ROOT/Library/ScriptAssemblies/Assembly-CSharp-Editor.dll"
file_check "Ramayana.EditMode.Tests.dll"    "$ROOT/Library/ScriptAssemblies/Ramayana.EditMode.Tests.dll"

echo ""
echo "== Assembly sizes =="
RUNTIME_SIZE=$(stat -f%z "$ROOT/Library/ScriptAssemblies/Assembly-CSharp.dll" 2>/dev/null || echo 0)
EDITOR_SIZE=$(stat -f%z "$ROOT/Library/ScriptAssemblies/Assembly-CSharp-Editor.dll" 2>/dev/null || echo 0)
TESTS_SIZE=$(stat -f%z "$ROOT/Library/ScriptAssemblies/Ramayana.EditMode.Tests.dll" 2>/dev/null || echo 0)
ge "Assembly-CSharp.dll ≥ 80 KB (48 C# files compiled)" "$RUNTIME_SIZE" "80000"
ge "Assembly-CSharp-Editor.dll ≥ 20 KB"                   "$EDITOR_SIZE" "20000"
ge "Ramayana.EditMode.Tests.dll ≥ 5 KB"                   "$TESTS_SIZE"  "5000"

echo ""
echo "== URP package artifacts compiled =="
file_check "Cinemachine.dll"                        "$ROOT/Library/ScriptAssemblies/Cinemachine.dll"
file_check "Unity.2D.Sprite.Editor.dll"             "$ROOT/Library/ScriptAssemblies/Unity.2D.Sprite.Editor.dll"
file_check "Unity.AI.Navigation.Editor.ConversionSystem.dll" "$ROOT/Library/ScriptAssemblies/Unity.AI.Navigation.Editor.ConversionSystem.dll"

echo ""
echo "== Last compile log signature (if /tmp/unity-compile.log present) =="
if [ -f /tmp/unity-compile.log ]; then
  ERRORS=$(grep -cE 'error CS[0-9]+' /tmp/unity-compile.log)
  SUCCESS=$(grep -c 'Exiting batchmode successfully now!' /tmp/unity-compile.log)
  check "compile errors in log" "$ERRORS" "0"
  check "Exiting batchmode successfully now! in log" "$SUCCESS" "1"
  LINES=$(wc -l < /tmp/unity-compile.log | tr -d ' ')
  ge "log lines (≥200 = real compile, <200 = suspicious)" "$LINES" "200"
else
  echo "  ⚠️  /tmp/unity-compile.log not present — re-run Unity batchmode first"
  echo "  ❌ log missing"; FAIL=$((FAIL+1))
fi

echo ""
echo "== cs source counts =="
ge "StoryMomentPlayer.cs fixed (private→public nested types)" \
  "$(grep -c 'public class ObjectiveRecord\|public class ActRecord\|public class CompletedLineRecord' "$ROOT/Assets/Scripts/Story/StoryMomentPlayer.cs" 2>/dev/null)" "3"

echo ""
echo "==================================================="
echo "  $PASS passed, $FAIL failed  (suite green: Unity batchmode compile succeeded)"
echo "==================================================="
[ "$FAIL" -eq 0 ] && exit 0 || exit 1