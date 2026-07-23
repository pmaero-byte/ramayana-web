#!/bin/bash
# hermes-verify-ramayana-phase0.sh — Verify Phase 0 of the Ramayana consolidation
#
# Asserts:
# 1. .gitignore present with Library/Logs/UserSettings/Build/ patterns
# 2. Working tree clean (all dirty work committed)
# 3. 8+ commits on top of the pre-existing Day 10 tip (2ac86b2)
# 4. Source surface (Editor scripts, scenes, Feedback components,
#    ScriptableObject assets, Illustrations, generated characters,
#    iOS bridges, ProjectSettings)
# 5. Unity 6000.5.4f1 installed at expected path (compile gate ready)
# 6. Library/.DS_Store/UserSettings/ NOT accidentally tracked
#
# Usage:
#   bash Tools/qa/hermes-verify-ramayana-phase0.sh
#   HERMES_VERIFY_ROOT=<project> bash hermes-verify-ramayana-phase0.sh
#
# Exit codes: 0 = all pass, 1 = any fail.
# Status: ad-hoc only — Unity batchmode compile deferred to Day 14+.

set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
PASS=0; FAIL=0

check() { if [ "$2" = "$3" ]; then echo "  ✅ $1  ($2)"; PASS=$((PASS+1)); else echo "  ❌ $1  expected=$3 actual=$2"; FAIL=$((FAIL+1)); fi; }
ge()    { if [ "$2" -ge "$3" ] 2>/dev/null; then echo "  ✅ $1  ($2 ≥ $3)"; PASS=$((PASS+1)); else echo "  ❌ $1  expected≥$3 actual=$2"; FAIL=$((FAIL+1)); fi; }
file_check() { if [ -f "$2" ]; then echo "  ✅ $1"; PASS=$((PASS+1)); else echo "  ❌ $1  (missing: $2)"; FAIL=$((FAIL+1)); fi; }

echo "== gitignore present =="
file_check ".gitignore" "$ROOT/.gitignore"
GE=$(grep -c 'Library/\|Logs/\|UserSettings/\|Build/' "$ROOT/.gitignore")
ge "Library/Logs/UserSettings/Build/ ignored" "$GE" "3"

echo ""
echo "== working tree clean =="
WT=$(cd "$ROOT" && git status --short | wc -l | tr -d ' ')
check "working tree clean" "$WT" "0"

echo ""
echo "== Day 1-10 commits landed =="
COMMIT_COUNT=$(cd "$ROOT" && git log --oneline 2ac86b2..HEAD | wc -l | tr -d ' ')
ge "new commits on top of 2ac86b2" "$COMMIT_COUNT" "8"

echo ""
echo "== source surface =="
ge "Editor .cs scripts" "$(find "$ROOT/Assets/Editor" -name "*.cs" 2>/dev/null | wc -l | tr -d ' ')" "5"
ge "Scene .unity files" "$(find "$ROOT/Assets/Scenes" -name "*.unity" 2>/dev/null | wc -l | tr -d ' ')" "9"
ge "Feedback .cs components" "$(find "$ROOT/Assets/Scripts/Feedback" -name "*.cs" 2>/dev/null | wc -l | tr -d ' ')" "15"
ge "Scripts subdirs" "$(find "$ROOT/Assets/Scripts" -mindepth 1 -maxdepth 1 -type d 2>/dev/null | wc -l | tr -d ' ')" "9"
ge "Resources/Generated ScriptableObject assets" "$(find "$ROOT/Assets/Resources/Generated" -name "*.asset" 2>/dev/null | wc -l | tr -d ' ')" "40"
ge "Illustration PNGs" "$(find "$ROOT/Assets/Illustrations" -name "*.png" 2>/dev/null | wc -l | tr -d ' ')" "100"
ge "Generated character PNGs" "$(find "$ROOT/Assets/Generated/characters" -name "*.png" 2>/dev/null | wc -l | tr -d ' ')" "8"

echo ""
echo "== iOS bridges (Plugins/iOS) =="
MM=$(find "$ROOT/Assets/Plugins/iOS" -name "*.mm" 2>/dev/null | wc -l | tr -d ' ')
ge "iOS .mm bridges" "$MM" "2"

echo ""
echo "== ProjectSettings + packages =="
PS=$(ls "$ROOT/ProjectSettings"/*.asset "$ROOT/ProjectSettings"/*.json 2>/dev/null | wc -l | tr -d ' ')
ge "ProjectSettings .asset/.json files" "$PS" "22"

echo ""
echo "== Unity install (next compile gate) =="
UNITY="/Users/prabaharan/Unity/Hub/Editor/6000.5.4f1/Unity.app/Contents/MacOS/Unity"
if [ -f "$UNITY" ]; then echo "  ✅ Unity 6000.5.4f1 installed at $UNITY"; PASS=$((PASS+1)); else echo "  ❌ Unity not found at expected path"; FAIL=$((FAIL+1)); fi

echo ""
echo "== Scripts not accidentally committed =="
LIB=$(cd "$ROOT" && git ls-files | grep -c '^Library/\|^\.DS_Store\|^UserSettings/')
check "Library/.DS_Store/UserSettings/ tracked" "$LIB" "0"

echo ""
echo "==================================================="
echo "  $PASS passed, $FAIL failed  (ad-hoc only, Unity compile pending)"
echo "==================================================="
[ "$FAIL" -eq 0 ] && exit 0 || exit 1