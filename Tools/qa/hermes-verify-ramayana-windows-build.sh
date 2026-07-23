#!/usr/bin/env bash
# hermes-verify-ramayana-windows-build.sh
# Verifies Ramayana PS5 → Windows Standalone64 build.

set -uo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
EXE="$ROOT/Build/Windows64/RamayanaPS5.exe"
PLAYER="$ROOT/Build/Windows64/UnityPlayer.dll"
DATA="$ROOT/Build/Windows64/RamayanaPS5_Data"

P=0; F=0
ok()   { echo "  ✅ $*"; P=$((P+1)); }
fail() { echo "  ❌ $*"; F=$((F+1)); }

echo "=== Ramayana PS5 Windows Build Verifier ==="

[[ -f "$EXE" ]] && ok "RamayanaPS5.exe exists ($(stat -f%z "$EXE") bytes)" || fail "MISSING"
[[ -f "$PLAYER" ]] && ok "UnityPlayer.dll exists ($(stat -f%z "$PLAYER") bytes)" || fail "MISSING"
[[ -d "$DATA" ]] && ok "RamayanaPS5_Data/ exists" || fail "MISSING"
[[ -d "$ROOT/Build/Windows64/MonoBleedingEdge" ]] && ok "MonoBleedingEdge/ present" || fail "MISSING"

if [[ -f "$EXE" ]]; then
    file "$EXE" | grep -q "PE32+ executable (GUI) x86-64" && ok "PE32+ x86-64 Windows" || fail "wrong format"
fi

if [[ -d "$DATA" ]]; then
    SHARED=$(find "$DATA" -name "sharedassets*.assets" 2>/dev/null | wc -l | tr -d ' ')
    LEVELS=$(find "$DATA" -name "level*" 2>/dev/null | wc -l | tr -d ' ')
    FILE_COUNT=$(find "$DATA" -type f 2>/dev/null | wc -l | tr -d ' ')
    [[ $SHARED -ge 1 ]] && ok "$SHARED sharedassets*.assets" || fail "0 sharedassets"
    [[ $LEVELS -ge 1 ]] && ok "$LEVELS level files" || fail "0 level files"
    [[ $FILE_COUNT -ge 50 ]] && ok "$FILE_COUNT files in Data/" || fail "Only $FILE_COUNT files"
fi

echo
echo "PASS=$P FAIL=$F"
[[ $F -eq 0 ]] && exit 0 || exit 1