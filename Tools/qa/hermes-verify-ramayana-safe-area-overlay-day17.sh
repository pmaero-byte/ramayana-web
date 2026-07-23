#!/bin/bash
# hermes-verify-ramayana-safe-area-overlay-day17.sh — Verifier for Day 17
# SafeAreaOverlay — notch/safe-area aware UI overlay.
#
# Asserts:
# 1. SafeAreaOverlay.cs + .meta exist
# 2. namespace Jambudweep.Ramayana.UI
# 3. class SafeAreaOverlay : MonoBehaviour (sealed)
# 4. Instance singleton + EnsureCreated() factory
# 5. References UnityEngine.Screen.safeArea (the API contract)
# 6. References UnityEngine.UI (Canvas/Image/RectTransform pattern)
# 7. Runtime UI: Canvas + Image + RectTransform built at runtime
# 8. Public API: RegisterExternal / UnregisterExternal / SetShowNotchBars
# 9. notchBar + homeBar decorative visuals (top + bottom unsafe area)
# 10. Brace balance
# 11. .meta guid valid
# 12. No duplication with CinematicThirdPersonCamera (camera, not overlay)
# 13. No duplication with CinematicLetterbox (bars, not safe-area)
# 14. Day 1-10 UI files UNTOUCHED
# 15. Unity batchmode compile clean (tested separately)
#
# Usage:
#   bash Tools/qa/hermes-verify-ramayana-safe-area-overlay-day17.sh
#   HERMES_VERIFY_ROOT=<project> bash hermes-verify-ramayana-safe-area-overlay-day17.sh
#
# Exit codes: 0 = all pass, 1 = any fail.
# Status: ad-hoc only — Unity batchmode compile pending.

set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
PASS=0; FAIL=0

section() { echo ""; echo "== $1 =="; }
check() { if [ "$2" = "$3" ]; then echo "  ✅ $1  ($2)"; PASS=$((PASS+1)); else echo "  ❌ $1  expected=$3 actual=$2"; FAIL=$((FAIL+1)); fi; }
ge() { if [ "$2" -ge "$3" ] 2>/dev/null; then echo "  ✅ $1  ($2 ≥ $3)"; PASS=$((PASS+1)); else echo "  ❌ $1  expected≥$3 actual=$2"; FAIL=$((FAIL+1)); fi; }
file_check() { if [ -f "$2" ]; then echo "  ✅ $1"; PASS=$((PASS+1)); else echo "  ❌ $1  (missing: $2)"; FAIL=$((FAIL+1)); fi; }

OVR="$ROOT/Assets/Scripts/UI/SafeAreaOverlay.cs"
SRC=$(cat "$OVR")

# ---------- Files ----------
section "Files exist"
file_check "SafeAreaOverlay.cs"      "$OVR"
file_check "SafeAreaOverlay.cs.meta" "$ROOT/Assets/Scripts/UI/SafeAreaOverlay.cs.meta"

# ---------- Source surface ----------
section "Source surface"
check "namespace Jambudweep.Ramayana.UI" \
  "$(echo "$SRC" | grep -c '^namespace Jambudweep.Ramayana.UI')" "1"
check "public sealed class SafeAreaOverlay" \
  "$(echo "$SRC" | grep -c 'public sealed class SafeAreaOverlay')" "1"
ge "MonoBehaviour (≥1 ref)" \
  "$(echo "$SRC" | grep -c 'MonoBehaviour')" "1"
check "public static SafeAreaOverlay Instance" \
  "$(echo "$SRC" | grep -c 'public static SafeAreaOverlay Instance')" "1"
check "public static EnsureCreated()" \
  "$(echo "$SRC" | grep -c 'public static SafeAreaOverlay EnsureCreated()')" "1"
check "private void Build()" \
  "$(echo "$SRC" | grep -c 'private void Build()')" "1"
check "void Awake()" \
  "$(echo "$SRC" | grep -c 'void Awake()')" "1"

# ---------- Safe-area API contract ----------
section "Safe-area API contract"
ge "Screen.safeArea referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'Screen.safeArea')" "1"
check "ApplySafeArea method declared" \
  "$(echo "$SRC" | grep -c 'private void ApplySafeArea')" "1"
check "SafeArea property exposed" \
  "$(echo "$SRC" | grep -cE 'public Rect SafeArea')" "1"
ge "Screen.width referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'Screen.width')" "1"
ge "Screen.height referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'Screen.height')" "1"

# ---------- Runtime UI components ----------
section "Runtime UI (Canvas + Image + RectTransform)"
ge "using UnityEngine.UI" \
  "$(echo "$SRC" | grep -c 'using UnityEngine.UI;')" "1"
ge "Canvas added (≥1)" \
  "$(echo "$SRC" | grep -cE 'AddComponent<Canvas>')" "1"
ge "Image added (≥2)" \
  "$(echo "$SRC" | grep -cE 'AddComponent<Image>')" "2"
ge "RectTransform added (≥3)" \
  "$(echo "$SRC" | grep -cE 'AddComponent<RectTransform>')" "3"
ge "CanvasScaler referenced (≥3)" \
  "$(echo "$SRC" | grep -c 'CanvasScaler')" "3"

# ---------- Notch + home bars ----------
section "Notch + home bars (decorative visual)"
ge "NotchBar string referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'NotchBar')" "1"
check "HomeBar GameObject declared once" \
  "$(echo "$SRC" | grep -c 'HomeBar')" "1"
ge "_notchBar RectTransform (≥1)" \
  "$(echo "$SRC" | grep -c '_notchBar')" "1"
ge "_homeBar RectTransform (≥1)" \
  "$(echo "$SRC" | grep -c '_homeBar')" "1"

# ---------- External listener API ----------
section "Public API (external HUD registration)"
check "RegisterExternal(RectTransform rt)" \
  "$(echo "$SRC" | grep -c 'public void RegisterExternal')" "1"
check "UnregisterExternal(RectTransform rt)" \
  "$(echo "$SRC" | grep -c 'public void UnregisterExternal')" "1"
check "SetShowNotchBars(bool show)" \
  "$(echo "$SRC" | grep -c 'public void SetShowNotchBars')" "1"
ge "_externalListeners list (≥2)" \
  "$(echo "$SRC" | grep -c '_externalListeners')" "2"

# ---------- Brace balance ----------
section "Brace balance (Roslyn-free sanity)"
opens=$(grep -o '{' "$OVR" | wc -l | tr -d ' ')
closes=$(grep -o '}' "$OVR" | wc -l | tr -d ' ')
check "brace balance ({ $opens, } $closes)" "$opens" "$closes"

# ---------- .meta guid ----------
section ".meta guid"
META_GUID=$(grep -h '^guid:' "$ROOT/Assets/Scripts/UI/SafeAreaOverlay.cs.meta" | awk '{print $2}')
if [[ "$META_GUID" =~ ^[a-f0-9]{32}$ ]]; then
  echo "  ✅ .meta guid valid: $META_GUID"; PASS=$((PASS+1))
else
  echo "  ❌ .meta guid invalid: $META_GUID"; FAIL=$((FAIL+1))
fi

# ---------- No duplication with existing surface ----------
section "No duplication with existing cinematic + UI surface"
check "SafeAreaOverlay does NOT touch CinematicThirdPersonCamera" \
  "$(echo "$SRC" | grep -c 'CinematicThirdPersonCamera')" "0"
check "SafeAreaOverlay does NOT touch CinematicLetterbox" \
  "$(echo "$SRC" | grep -c 'CinematicLetterbox')" "0"
check "SafeAreaOverlay does NOT touch DialogueOverlay" \
  "$(echo "$SRC" | grep -c 'DialogueOverlay')" "0"

# ---------- Day 1-10 files UNTOUCHED ----------
section "Day 1-10 UI/Motion3D files UNTOUCHED"
for f in Assets/Scripts/Motion3D/CinematicThirdPersonCamera.cs \
         Assets/Scripts/Feedback/CinematicLetterbox.cs \
         Assets/Scripts/UI/DialogueOverlay.cs \
         Assets/Scripts/UI/PortraitResolver.cs \
         Assets/Scripts/UI/SaveLoadHud.cs \
         Assets/Scripts/UI/TitleScreenTapZone.cs; do
  DIFF_LINES=$(cd "$ROOT" && git diff --name-only HEAD -- "$f" | wc -l | tr -d ' ')
  if [ "$DIFF_LINES" = "0" ]; then
    echo "  ✅ $f untouched"; PASS=$((PASS+1))
  else
    echo "  ❌ $f modified (diff lines=$DIFF_LINES)"; FAIL=$((FAIL+1))
  fi
done

# ---------- ELGODS source sanity (no equivalent) ----------
section "ELGODS source sanity (no SafeArea equivalent in browser game)"
ELGODS_SAFE=$(grep -c 'safeArea\|SafeArea\|safe-area' /Users/prabaharan/jambudweep/ELGODS/portal/src/game/ramayana/*.ts 2>/dev/null | awk -F: '{s+=$2}END{print s+0}')
echo "  ELGODS safeArea references in game/*.ts: $ELGODS_SAFE (browser games don't need it)"

# ---------- Final report ----------
echo ""
echo "==================================================="
echo "  $PASS passed, $FAIL failed  (ad-hoc only, Unity compile pending)"
echo "==================================================="
[ "$FAIL" -eq 0 ] && exit 0 || exit 1