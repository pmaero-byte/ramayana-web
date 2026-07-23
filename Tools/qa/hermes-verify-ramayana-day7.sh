#!/bin/bash
# hermes-verify-ramayana-day7.sh
#
# Day 7 ad-hoc verifier for ArrowProjectile + BowCooldown + ArcherAutoFire
# projectile plumbing:
#   - ArrowProjectile has Initialize() API, Awake/Update lifecycle, damages
#     RakshasaTarget, and does NOT use UnityEngine.Physics (KISS kinematic).
#   - BowCooldown exposes CanFire() + Consume().
#   - ArcherAutoFire spawns ArrowProjectile instead of instant Damage().
#   - EditMode wireup test covers Day 7 surface.
#
# Ad-hoc only. Unity Editor compile is the only true regression signal.
set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
ARROW="$ROOT/Assets/Scripts/Combat/ArrowProjectile.cs"
BOW="$ROOT/Assets/Scripts/Combat/BowCooldown.cs"
ARCH="$ROOT/Assets/Scripts/Combat/ArcherAutoFire.cs"
ARROW_META="$ROOT/Assets/Scripts/Combat/ArrowProjectile.cs.meta"
BOW_META="$ROOT/Assets/Scripts/Combat/BowCooldown.cs.meta"
TESTS="$ROOT/Assets/Tests/EditMode/RamayanaWireupTests.cs"

PASS=0; FAIL=0
section() { echo ""; echo "== $1 =="; }
check()    { local name="$1" actual="$2" expected="$3"
  if [ "$actual" = "$expected" ]; then echo "  ✅ $name  ($actual)"; PASS=$((PASS+1))
  else echo "  ❌ $name  expected=$expected actual=$actual"; FAIL=$((FAIL+1)); fi
}
file_check() { local name="$1" present="$2"
  if [ "$present" = "yes" ]; then echo "  ✅ $name"; PASS=$((PASS+1))
  else echo "  ❌ $name  (not found)"; FAIL=$((FAIL+1)); fi
}
ge() { local name="$1" actual="$2" min="$3"
  if [ "$actual" -ge "$min" ] 2>/dev/null; then echo "  ✅ $name  ($actual ≥ $min)"; PASS=$((PASS+1))
  else echo "  ❌ $name  expected≥$min actual=$actual"; FAIL=$((FAIL+1)); fi
}

section "Day 7 — files exist"
file_check "ArrowProjectile.cs exists"      "$([ -f "$ARROW" ] && echo yes || echo no)"
file_check "BowCooldown.cs exists"          "$([ -f "$BOW" ] && echo yes || echo no)"
file_check "ArrowProjectile.cs.meta exists" "$([ -f "$ARROW_META" ] && echo yes || echo no)"
file_check "BowCooldown.cs.meta exists"     "$([ -f "$BOW_META" ] && echo yes || echo no)"
file_check "ArcherAutoFire.cs still exists" "$([ -f "$ARCH" ] && echo yes || echo no)"

for META in "$ARROW_META" "$BOW_META"; do
  if [ -f "$META" ]; then
    GUID=$(grep -oE 'guid: [a-f0-9]{32}' "$META" | awk '{print $2}')
    check "$(basename "$META") guid 32 hex" "$([ "${#GUID}" -eq 32 ] && echo yes || echo no)" "yes"
  fi
done

section "ArrowProjectile surface"
ARROW_SRC="$([ -f "$ARROW" ] && cat "$ARROW" || echo "")"
check "namespace Jambudweep.Ramayana.Combat" \
  "$(echo "$ARROW_SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Combat\s*$')" "1"
check "class ArrowProjectile sealed" \
  "$(echo "$ARROW_SRC" | grep -cE 'public sealed class ArrowProjectile')" "1"
check "Initialize API" \
  "$(echo "$ARROW_SRC" | grep -cE 'public void Initialize\(Vector3 origin, Vector3 direction, float speed, int damage, float maxLifetime\)')" "1"
ge "Awake lifecycle (≥1)" \
  "$(echo "$ARROW_SRC" | grep -cE 'void Awake\(')" "1"
ge "Update lifecycle (≥1)" \
  "$(echo "$ARROW_SRC" | grep -cE 'void Update\(')" "1"
ge "calls RakshasaTarget.Damage (≥1)" \
  "$(echo "$ARROW_SRC" | grep -cE 'target\.Damage\(')" "1"
check "no UnityEngine.Physics API (code lines)" \
  "$(echo "$ARROW_SRC" | grep -vE '^\s*//' | grep -cE 'UnityEngine\.Physics|Physics\.(Raycast|SphereCast|Overlap|Linecast)' || true)" "0"
ge "CreateProcedural factory (≥1)" \
  "$(echo "$ARROW_SRC" | grep -cE 'CreateProcedural')" "1"
ge "OnTriggerEnter present (≥1)" \
  "$(echo "$ARROW_SRC" | grep -cE 'void OnTriggerEnter')" "1"

section "BowCooldown surface"
BOW_SRC="$([ -f "$BOW" ] && cat "$BOW" || echo "")"
check "namespace Jambudweep.Ramayana.Combat" \
  "$(echo "$BOW_SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Combat\s*$')" "1"
check "class BowCooldown sealed" \
  "$(echo "$BOW_SRC" | grep -cE 'public sealed class BowCooldown')" "1"
check "CanFire() API" \
  "$(echo "$BOW_SRC" | grep -cE 'public bool CanFire\(')" "1"
ge "Consume() API (≥1)" \
  "$(echo "$BOW_SRC" | grep -cE 'public void Consume\(')" "1"

section "ArcherAutoFire projectile plumbing"
ARCH_SRC="$([ -f "$ARCH" ] && cat "$ARCH" || echo "")"
check "class ArcherAutoFire sealed" \
  "$(echo "$ARCH_SRC" | grep -cE 'public sealed class ArcherAutoFire')" "1"
ge "references ArrowProjectile (≥1)" \
  "$(echo "$ARCH_SRC" | grep -cE 'ArrowProjectile')" "1"
ge "references BowCooldown (≥1)" \
  "$(echo "$ARCH_SRC" | grep -cE 'BowCooldown')" "1"
ge "arrowPrefab SerializeField (≥1)" \
  "$(echo "$ARCH_SRC" | grep -cE 'arrowPrefab')" "1"
ge "calls Initialize on arrow (≥1)" \
  "$(echo "$ARCH_SRC" | grep -cE '\.Initialize\(')" "1"
ge "delegates fire rate to CanFire/Consume (≥1)" \
  "$(echo "$ARCH_SRC" | grep -cE 'CanFire\(|Consume\(')" "1"

section "EditMode Day 7 surface"
TESTS_SRC="$([ -f "$TESTS" ] && cat "$TESTS" || echo "")"
ge "Day7_ArrowProjectile_ExposesInitializeApi test (≥1)" \
  "$(echo "$TESTS_SRC" | grep -cE 'Day7_ArrowProjectile_ExposesInitializeApi')" "1"
ge "Day7_BowCooldown_ExposesCanFireApi test (≥1)" \
  "$(echo "$TESTS_SRC" | grep -cE 'Day7_BowCooldown_ExposesCanFireApi')" "1"

section "Summary"
echo "  $PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
