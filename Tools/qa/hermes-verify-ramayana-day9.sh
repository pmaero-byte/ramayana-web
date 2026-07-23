#!/bin/bash
# hermes-verify-ramayana-day9.sh
#
# Day 9 ad-hoc verifier — character portraits for GTA dialogue panels:
#   - PortraitResolver with ResolvePortrait / Resolve API
#   - StoryMomentPlayer.ResolvePortrait + non-null portrait args to Show()
#   - DialogueOverlay.EnsureCreated runtime factory
#   - Resources/portraits has key epic faces
#   - Bootstrap wires DialogueOverlay + PortraitResolver
#
# Ad-hoc only. Unity Editor compile is the only true regression signal.
set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
RESOLVER="$ROOT/Assets/Scripts/UI/PortraitResolver.cs"
OVERLAY="$ROOT/Assets/Scripts/UI/DialogueOverlay.cs"
PLAYER="$ROOT/Assets/Scripts/Story/StoryMomentPlayer.cs"
BS="$ROOT/Assets/Scripts/Scene/PlayerSceneBootstrap.cs"
PORT_DIR="$ROOT/Assets/Resources/portraits"
TESTS="$ROOT/Assets/Tests/EditMode/RamayanaWireupTests.cs"

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

section "Day 9 — files exist"
file_check "PortraitResolver.cs" "$([ -f "$RESOLVER" ] && echo yes || echo no)"
file_check "PortraitResolver.cs.meta" "$([ -f "$RESOLVER.meta" ] && echo yes || echo no)"
file_check "DialogueOverlay.cs" "$([ -f "$OVERLAY" ] && echo yes || echo no)"
file_check "StoryMomentPlayer.cs" "$([ -f "$PLAYER" ] && echo yes || echo no)"
file_check "portraits folder" "$([ -d "$PORT_DIR" ] && echo yes || echo no)"
if [ -f "$RESOLVER.meta" ]; then
  GUID=$(grep -oE 'guid: [a-f0-9]{32}' "$RESOLVER.meta" | awk '{print $2}')
  check "PortraitResolver meta guid 32hex" "$([ ${#GUID} -eq 32 ] && echo yes || echo no)" "yes"
fi

section "PortraitResolver surface"
SRC=$(cat "$RESOLVER" 2>/dev/null || echo "")
check "namespace UI" "$(echo "$SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.UI\s*$')" "1"
check "sealed PortraitResolver" "$(echo "$SRC" | grep -cE 'public sealed class PortraitResolver')" "1"
ge "EnsureCreated (≥1)" "$(echo "$SRC" | grep -cE 'EnsureCreated')" "1"
check "static Resolve API" "$(echo "$SRC" | grep -cE 'public static Sprite Resolve\(')" "1"
ge "ResolvePortrait instance API (≥1)" "$(echo "$SRC" | grep -cE 'ResolvePortrait\(')" "1"
ge "Resources.Load portraits (≥1)" "$(echo "$SRC" | grep -cE 'Resources\.Load')" "1"
ge "fallback plate (≥1)" "$(echo "$SRC" | grep -cE 'FallbackPlate|fallback')" "1"

section "DialogueOverlay EnsureCreated"
SRC=$(cat "$OVERLAY" 2>/dev/null || echo "")
check "EnsureCreated factory" "$(echo "$SRC" | grep -cE 'public static DialogueOverlay EnsureCreated')" "1"
ge "BuildRuntime (≥1)" "$(echo "$SRC" | grep -cE 'BuildRuntime')" "1"
ge "portraitImage usage (≥1)" "$(echo "$SRC" | grep -cE 'portraitImage')" "1"
ge "sortOrder 4720s (≥1)" "$(echo "$SRC" | grep -cE 'sortOrder\s*=\s*47')" "1"

section "StoryMomentPlayer portrait plumbing"
SRC=$(cat "$PLAYER" 2>/dev/null || echo "")
check "public ResolvePortrait" "$(echo "$SRC" | grep -cE 'public Sprite ResolvePortrait\(')" "1"
ge "calls PortraitResolver (≥1)" "$(echo "$SRC" | grep -cE 'PortraitResolver')" "1"
# Show must pass a portrait arg that is not the literal null twice in a row for both paths —
# at least one ResolvePortrait in Show calls.
ge "Show with ResolvePortrait (≥1)" "$(echo "$SRC" | grep -cE 'ResolvePortrait\(')" "1"
# Day 2 used null, null for sanskrit+portrait — Day 9 must not leave both Show calls with bare null portrait.
check "no bare null portrait pair in Show" "$(echo "$SRC" | grep -cE 'Show\([^;]*null, null,')" "0"

section "Resources/portraits assets"
for id in rama sita hanuman lakshmana ravana bharata default; do
  file_check "portraits/$id.png" "$([ -f "$PORT_DIR/$id.png" ] && echo yes || echo no)"
done
PNG_COUNT=$(find "$PORT_DIR" -maxdepth 1 -name '*.png' 2>/dev/null | wc -l | tr -d ' ')
ge "portrait png count ≥6" "$PNG_COUNT" "6"

section "Bootstrap Day 9 wiring"
SRC=$(cat "$BS" 2>/dev/null || echo "")
ge "DialogueOverlay.EnsureCreated (≥1)" "$(echo "$SRC" | grep -cE 'DialogueOverlay\.EnsureCreated')" "1"
ge "PortraitResolver.EnsureCreated (≥1)" "$(echo "$SRC" | grep -cE 'PortraitResolver\.EnsureCreated')" "1"

section "EditMode Day 9 surface"
SRC=$(cat "$TESTS" 2>/dev/null || echo "")
ge "Day9 tests (≥2)" "$(echo "$SRC" | grep -cE 'Day9_')" "2"

section "Summary"
echo "  $PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
