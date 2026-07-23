#!/bin/bash
# hermes-verify-ramayana-day8.sh
#
# Day 8 ad-hoc verifier — Mac GTA playable slice:
#   - MacDesktopInput (orbit for laptop)
#   - CinematicLetterbox (GTA letterbox bars)
#   - MacGtaFeelDirector (dusk fog / sky)
#   - PlayerSceneBootstrap wires camera + archer + Mac input
#   - ThirdPersonMotionController.SetCameraRoot present
#
# Ad-hoc only. Unity Editor compile + macOS .app build are separate gates.
set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
MAC_IN="$ROOT/Assets/Scripts/Platform/MacDesktopInput.cs"
LETTER="$ROOT/Assets/Scripts/Feedback/CinematicLetterbox.cs"
FEEL="$ROOT/Assets/Scripts/Scene/MacGtaFeelDirector.cs"
BS="$ROOT/Assets/Scripts/Scene/PlayerSceneBootstrap.cs"
MOTION="$ROOT/Assets/Scripts/Motion3D/ThirdPersonMotionController.cs"
TESTS="$ROOT/Assets/Tests/EditMode/RamayanaWireupTests.cs"
BUILD_SH="$ROOT/Tools/qa/build-macos.sh"

PASS=0; FAIL=0
section() { echo ""; echo "== $1 =="; }
check() { local n="$1" a="$2" e="$3"
  if [ "$a" = "$e" ]; then echo "  ✅ $n  ($a)"; PASS=$((PASS+1))
  else echo "  ❌ $n  expected=$e actual=$a"; FAIL=$((FAIL+1)); fi
}
ge() { local n="$1" a="$2" m="$3"
  if [ "$a" -ge "$m" ] 2>/dev/null; then echo "  ✅ $n  ($a ≥ $m)"; PASS=$((PASS+1))
  else echo "  ❌ $n  expected≥$m actual=$a"; FAIL=$((FAIL+1)); fi
}
file_check() { local n="$1" p="$2"
  if [ "$p" = "yes" ]; then echo "  ✅ $n"; PASS=$((PASS+1))
  else echo "  ❌ $n"; FAIL=$((FAIL+1)); fi
}

section "Day 8 — files exist"
for f in "$MAC_IN" "$LETTER" "$FEEL" "$BS" "$MOTION"; do
  file_check "$(basename "$f")" "$([ -f "$f" ] && echo yes || echo no)"
done
for m in "$MAC_IN.meta" "$LETTER.meta" "$FEEL.meta"; do
  file_check "$(basename "$m")" "$([ -f "$m" ] && echo yes || echo no)"
  if [ -f "$m" ]; then
    GUID=$(grep -oE 'guid: [a-f0-9]{32}' "$m" | awk '{print $2}')
    check "$(basename "$m") guid 32hex" "$([ ${#GUID} -eq 32 ] && echo yes || echo no)" "yes"
  fi
done

section "MacDesktopInput surface"
SRC=$(cat "$MAC_IN" 2>/dev/null || echo "")
check "namespace Platform" "$(echo "$SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Platform\s*$')" "1"
check "sealed MacDesktopInput" "$(echo "$SRC" | grep -cE 'public sealed class MacDesktopInput')" "1"
ge "AddOrbitInput usage (≥1)" "$(echo "$SRC" | grep -cE 'AddOrbitInput')" "1"
ge "EnsureCreated factory (≥1)" "$(echo "$SRC" | grep -cE 'EnsureCreated')" "1"
ge "Bind API (≥1)" "$(echo "$SRC" | grep -cE 'public void Bind\(')" "1"

section "CinematicLetterbox surface"
SRC=$(cat "$LETTER" 2>/dev/null || echo "")
check "namespace Feedback" "$(echo "$SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Feedback\s*$')" "1"
check "sealed CinematicLetterbox" "$(echo "$SRC" | grep -cE 'public sealed class CinematicLetterbox')" "1"
ge "EnsureCreated (≥1)" "$(echo "$SRC" | grep -cE 'EnsureCreated')" "1"
ge "sortOrder 4700s (≥1)" "$(echo "$SRC" | grep -cE 'sortOrder\s*=\s*47')" "1"
ge "SetVisible API (≥1)" "$(echo "$SRC" | grep -cE 'public void SetVisible')" "1"

section "MacGtaFeelDirector surface"
SRC=$(cat "$FEEL" 2>/dev/null || echo "")
check "namespace Scene" "$(echo "$SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Scene\s*$')" "1"
check "sealed MacGtaFeelDirector" "$(echo "$SRC" | grep -cE 'public sealed class MacGtaFeelDirector')" "1"
ge "Apply API (≥1)" "$(echo "$SRC" | grep -cE 'public void Apply\(')" "1"
ge "fog enabled (≥1)" "$(echo "$SRC" | grep -cE 'RenderSettings\.fog')" "1"

section "Bootstrap GTA wiring"
SRC=$(cat "$BS" 2>/dev/null || echo "")
ge "refs CinematicThirdPersonCamera (≥1)" "$(echo "$SRC" | grep -cE 'CinematicThirdPersonCamera')" "1"
ge "refs ArcherAutoFire (≥1)" "$(echo "$SRC" | grep -cE 'ArcherAutoFire')" "1"
ge "refs MacDesktopInput (≥1)" "$(echo "$SRC" | grep -cE 'MacDesktopInput')" "1"
ge "refs CinematicLetterbox (≥1)" "$(echo "$SRC" | grep -cE 'CinematicLetterbox')" "1"
ge "refs MacGtaFeelDirector (≥1)" "$(echo "$SRC" | grep -cE 'MacGtaFeelDirector')" "1"
ge "SetCameraRoot call (≥1)" "$(echo "$SRC" | grep -cE 'SetCameraRoot')" "1"
ge "mountCinematicCamera flag (≥1)" "$(echo "$SRC" | grep -cE 'mountCinematicCamera')" "1"

section "Motion SetCameraRoot"
SRC=$(cat "$MOTION" 2>/dev/null || echo "")
check "SetCameraRoot API" "$(echo "$SRC" | grep -cE 'public void SetCameraRoot\(Transform')" "1"

section "EditMode Day 8 surface"
SRC=$(cat "$TESTS" 2>/dev/null || echo "")
ge "Day8 tests (≥2)" "$(echo "$SRC" | grep -cE 'Day8_')" "2"

section "Mac build pipeline script"
file_check "build-macos.sh exists" "$([ -f "$BUILD_SH" ] && echo yes || echo no)"
if [ -f "$BUILD_SH" ]; then
  ge "discovers Unity binary (≥1)" "$(grep -cE 'UNITY|Unity\.app|find_unity|Hub/Editor' "$BUILD_SH" || true)" "1"
  ge "BuildMacOS.BuildFromCli (≥1)" "$(grep -cE 'BuildMacOS\.BuildFromCli' "$BUILD_SH" || true)" "1"
fi

section "Summary"
echo "  $PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
