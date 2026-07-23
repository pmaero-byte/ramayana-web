#!/usr/bin/env bash
# hermes-verify-ramayana-macos-build.sh
# Verifies Ramayana PS5 → macOS Standalone (universal x86_64 + arm64) build.

set -uo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
APP="$ROOT/Build/macOS/RamayanaPS5.app"

P=0; F=0
ok()   { echo "  ✅ $*"; P=$((P+1)); }
fail() { echo "  ❌ $*"; F=$((F+1)); }

echo "=== Ramayana PS5 macOS Build Verifier ==="

[[ -d "$APP" ]] && ok "RamayanaPS5.app exists ($(du -sh "$APP" | awk '{print $1}'))" || fail "MISSING"
[[ -d "$APP/Contents" ]] && ok "Contents/ exists" || fail "MISSING"
[[ -f "$APP/Contents/MacOS/RamayanaPS5" ]] && ok "MacOS binary exists" || fail "MacOS binary MISSING"
[[ -d "$APP/Contents/MonoBleedingEdge" ]] && ok "MonoBleedingEdge/ exists" || fail "MISSING"

EXEC="$APP/Contents/MacOS/RamayanaPS5"
if [[ -f "$EXEC" ]]; then
    file "$EXEC" | grep -q "Mach-O .* x86_64" && ok "binary contains x86_64" || fail "no x86_64"
    file "$EXEC" | grep -q "Mach-O .* arm64" && ok "binary contains arm64" || fail "no arm64"
fi

if [[ -d "$APP/Contents/Resources/Data" ]]; then
    SHARED=$(find "$APP/Contents/Resources/Data" -name "sharedassets*.assets" 2>/dev/null | wc -l | tr -d ' ')
    LEVELS=$(find "$APP/Contents/Resources/Data" -name "level*" 2>/dev/null | wc -l | tr -d ' ')
    FILE_COUNT=$(find "$APP/Contents/Resources/Data" -type f 2>/dev/null | wc -l | tr -d ' ')
    [[ $SHARED -ge 1 ]] && ok "$SHARED sharedassets*.assets" || fail "0 sharedassets"
    [[ $LEVELS -ge 1 ]] && ok "$LEVELS level files" || fail "0 levels"
    [[ $FILE_COUNT -ge 50 ]] && ok "$FILE_COUNT files" || fail "Only $FILE_COUNT files"
fi

echo
echo "PASS=$P FAIL=$F"
[[ $F -eq 0 ]] && exit 0 || exit 1