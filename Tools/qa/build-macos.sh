#!/usr/bin/env bash
# Ramayana PS5 → macOS Standalone build pipeline
# Day 8 — discovers any installed Unity 6 Editor (Hub secondary path or Applications).
#
# Usage:
#   bash Tools/qa/build-macos.sh
#
# Output:
#   Build/macOS/RamayanaPS5.app
#   Bundle ID: com.jambudweepgames.ramayanaps5
#
# Requires a Unity Editor install. Unity Hub alone is not enough.

set -euo pipefail

PROJECT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
LOG="/tmp/ramayana-macos-build.log"
export TMPDIR="${TMPDIR:-/tmp}"
mkdir -p "$TMPDIR"

find_unity() {
  local candidates=(
    "/Users/prabaharan/Unity/Hub/Editor/6000.5.2f1/Unity.app/Contents/MacOS/Unity"
    "/Users/prabaharan/Unity/Hub/Editor/6000.3.18f1/Unity.app/Contents/MacOS/Unity"
    "/Applications/Unity/Hub/Editor/6000.5.2f1/Unity.app/Contents/MacOS/Unity"
  )
  local c
  for c in "${candidates[@]}"; do
    if [ -x "$c" ]; then echo "$c"; return 0; fi
  done
  # Any Unity 6 under Hub secondary path
  if [ -d "/Users/prabaharan/Unity/Hub/Editor" ]; then
    local found
    found=$(find /Users/prabaharan/Unity/Hub/Editor -path '*/Unity.app/Contents/MacOS/Unity' -type f 2>/dev/null | sort -r | head -1)
    if [ -n "${found:-}" ] && [ -x "$found" ]; then echo "$found"; return 0; fi
  fi
  return 1
}

UNITY="${UNITY_PATH:-}"
if [ -z "$UNITY" ] || [ ! -x "$UNITY" ]; then
  UNITY=$(find_unity || true)
fi

if [ -z "${UNITY:-}" ] || [ ! -x "$UNITY" ]; then
  echo "[ERROR] No Unity Editor binary found."
  echo "  Install Unity 6000.5.2f1 (matching ProjectVersion.txt) via Unity Hub:"
  echo "    secondary path: /Users/prabaharan/Unity/Hub/Editor"
  echo "  Or set UNITY_PATH=/path/to/Unity.app/Contents/MacOS/Unity"
  exit 2
fi

echo "=== [1/2] Ramayana → macOS Standalone ==="
echo "Unity: $UNITY"
cd "$PROJECT"
"$UNITY" \
  -batchmode -quit -nographics \
  -projectPath "$PROJECT" \
  -executeMethod Ramayana.Editor.BuildMacOS.BuildFromCli \
  -logFile "$LOG" \
  -buildTarget StandaloneOSX
RC=$?
echo "Unity exit: $RC"

echo
echo "=== [2/2] Verify .app ==="
APP="$PROJECT/Build/macOS/RamayanaPS5.app"
if [ -d "$APP" ]; then
  /usr/libexec/PlistBuddy -c "Print :CFBundleIdentifier" "$APP/Contents/Info.plist" 2>&1 | head -1
  /usr/libexec/PlistBuddy -c "Print :CFBundleName" "$APP/Contents/Info.plist" 2>&1 | head -1
  echo "Total size: $(du -sh "$APP" | cut -f1)"
  BIN=$(find "$APP/Contents/MacOS" -type f -maxdepth 1 2>/dev/null | head -1)
  if [ -n "$BIN" ]; then file "$BIN" | head -1; fi
else
  echo "[ERROR] $APP not built — see $LOG"
  exit 1
fi

echo
echo "=== Build DONE — see $LOG ==="
