#!/usr/bin/env bash
# hermes-verify-ramayana-linux-build.sh
# Verifies Ramayana PS5 → Linux Standalone64 build artifacts.

set -uo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
EXE="$ROOT/Build/Linux64/RamayanaPS5.x86_64"
LIB="$ROOT/Build/Linux64/UnityPlayer.so"
DATA="$ROOT/Build/Linux64/RamayanaPS5_Data"

P=0; F=0
ok()   { echo "  ✅ $*"; P=$((P+1)); }
fail() { echo "  ❌ $*"; F=$((F+1)); }

echo "=== Ramayana PS5 Linux Build Verifier ==="

[[ -f "$EXE" ]] && ok "RamayanaPS5.x86_64 exists ($(stat -f%z "$EXE") bytes)" || fail "MISSING"
[[ -f "$LIB" ]] && ok "UnityPlayer.so exists ($(stat -f%z "$LIB") bytes)" || fail "MISSING"
[[ -d "$DATA" ]] && ok "RamayanaPS5_Data/ exists" || fail "MISSING"

if [[ -f "$EXE" ]]; then
    file "$EXE" | grep -q "ELF 64-bit LSB executable, x86-64" && ok "ELF 64-bit x86-64" || fail "wrong format"
    file "$EXE" | grep -q "dynamically linked" && ok "dynamically linked" || fail "not dynamically linked"
fi
if [[ -f "$LIB" ]]; then
    file "$LIB" | grep -q "ELF 64-bit LSB shared object" && ok "UnityPlayer.so is ELF shared object" || fail "wrong format"
fi

if [[ -d "$DATA" ]]; then
    SHARED=$(find "$DATA" -name "sharedassets*.assets" 2>/dev/null | wc -l | tr -d ' ')
    LEVELS=$(find "$DATA" -name "level*" 2>/dev/null | wc -l | tr -d ' ')
    FILE_COUNT=$(find "$DATA" -type f 2>/dev/null | wc -l | tr -d ' ')
    [[ $SHARED -ge 1 ]] && ok "$SHARED sharedassets" || fail "0"
    [[ $LEVELS -ge 1 ]] && ok "$LEVELS level files" || fail "0"
    [[ $FILE_COUNT -ge 50 ]] && ok "$FILE_COUNT files" || fail "Only $FILE_COUNT"
    [[ -f "$DATA/resources.assets" ]] && ok "resources.assets (Linux main data)" || fail "missing"
    [[ -f "$DATA/globalgamemanagers" ]] && ok "globalgamemanagers present" || fail "missing"
fi

echo
echo "PASS=$P FAIL=$F"
[[ $F -eq 0 ]] && exit 0 || exit 1