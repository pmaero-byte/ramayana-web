#!/usr/bin/env bash
# Hermes CTO — Ramayana PS5 macOS Standalone Build Verifier
# Round 18 (R18) — local macOS verification pipeline
set -e
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
APP="$ROOT/Build/macOS/RamayanaPS5.app"

PASS=0; FAIL=0
check() {
  local name="$1"
  local cond="$2"
  if eval "$cond" >/dev/null 2>&1; then
    echo "  [OK] $name"
    PASS=$((PASS+1))
  else
    echo "  [FAIL] $name"
    FAIL=$((FAIL+1))
  fi
}

echo "=== Ramayana PS5 macOS Standalone Build Verifier (R18) ==="
echo "APP = $APP"
echo

echo "--- Project files ---"
check "ProjectSettings_exists"   "test -d '$ROOT/ProjectSettings'"
check "Assets_dir_exists"        "test -d '$ROOT/Assets'"
check "Packages_manifest_exists" "test -f '$ROOT/Packages/manifest.json'"
check "URP_in_packages"          "grep -q 'render-pipelines.universal' '$ROOT/Packages/manifest.json'"

echo
echo "--- Bundle structure ---"
if [ -d "$APP" ]; then
  # Ramayana binary may be named RamayanaPS5 or Ramayana (Unity import may rename it).
  # Use find to handle binary names with spaces.
  BIN=$(find "$APP/Contents/MacOS" -maxdepth 1 -type f 2>/dev/null | head -1)
  check "app_exists"             "test -d '$APP'"
  check "app_binary_exists"      "test -f '$BIN'"
  check "app_has_universal"      "file '$BIN' | grep -q 'universal binary'"
  check "app_has_arm64_slice"    "file '$BIN' | grep -q 'arm64'"
  check "app_has_x86_64_slice"   "file '$BIN' | grep -q 'x86_64'"
  check "app_has_data_dir"       "test -d '$APP/Contents/Resources/Data'"
  check "app_has_info_plist"     "test -f '$APP/Contents/Info.plist'"
  check "app_has_main_module"    "test -f '$APP/Contents/Resources/Data/Managed/Assembly-CSharp.dll'"

  echo
  echo "--- Format checks ---"
  BUNDLE_ID=$(/usr/libexec/PlistBuddy -c "Print :CFBundleIdentifier" "$APP/Contents/Info.plist" 2>/dev/null)
  BUNDLE_NAME=$(/usr/libexec/PlistBuddy -c "Print :CFBundleName" "$APP/Contents/Info.plist" 2>/dev/null)
  BUNDLE_VER=$(/usr/libexec/PlistBuddy -c "Print :CFBundleShortVersionString" "$APP/Contents/Info.plist" 2>/dev/null)
  check "bundle_id_set"          "test -n '$BUNDLE_ID'"
  check "bundle_name_set"        "test -n '$BUNDLE_NAME'"
  check "bundle_version_set"     "test -n '$BUNDLE_VER'"
  APP_SIZE=$(du -sh "$APP" | cut -f1)
  BIN_SIZE=$(stat -f%z "$BIN" 2>/dev/null)
  echo "  Info: $BUNDLE_NAME v$BUNDLE_VER ($BUNDLE_ID), total size $APP_SIZE, binary $BIN_SIZE bytes"
fi

echo
echo "--- StoryPlayer + playable-without-reading ---"
check "StoryPlayer_dir_exists"   "test -d '$ROOT/StoryPlayer'"
check "MainMenu_patched"         "! grep -E 'Press \\[Space\\]' '$ROOT/Assets/Scenes/MainMenu.unity'"
check "TitleTapZone_script"      "test -f '$ROOT/Assets/Scripts/UI/TitleScreenTapZone.cs'"
check "R13_doc_exists"           "test -f '$ROOT/Documentation/QA/ROUND13_RAMAYANA_PLAYABLE_WITHOUT_READING_20260630.md'"

echo
echo "=== Summary ==="
echo "PASS=$PASS  FAIL=$FAIL"
[ $FAIL -eq 0 ] && echo "VERIFIER: GREEN" || echo "VERIFIER: RED"
exit $FAIL