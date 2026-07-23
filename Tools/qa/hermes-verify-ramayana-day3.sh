#!/bin/bash
# hermes-verify-ramayana-day3.sh
#
# Day 3 ad-hoc verifier for the playable combat trio:
#   - RakshasaTarget (damageable enemy)
#   - WaveController (spawns waves of RakshasaTargets)
#   - ArcherAutoFire (player-side auto-archer)
#
# Confirms source-level surface + cross-references between the three files.
# Ad-hoc only. Unity Editor compile + Play-mode smoke live in the
# hermes-verify-ramayana-editmode-tests.sh suite.
set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
RAK="$ROOT/Assets/Scripts/Combat/RakshasaTarget.cs"
WAVE="$ROOT/Assets/Scripts/Combat/WaveController.cs"
ARCH="$ROOT/Assets/Scripts/Combat/ArcherAutoFire.cs"
RAK_META="$ROOT/Assets/Scripts/Combat/RakshasaTarget.cs.meta"
WAVE_META="$ROOT/Assets/Scripts/Combat/WaveController.cs.meta"
ARCH_META="$ROOT/Assets/Scripts/Combat/ArcherAutoFire.cs.meta"

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

section "Day 3 — playable combat trio"
file_check "RakshasaTarget.cs exists"        "$([ -f "$RAK" ] && echo yes || echo no)"
file_check "WaveController.cs exists"        "$([ -f "$WAVE" ] && echo yes || echo no)"
file_check "ArcherAutoFire.cs exists"        "$([ -f "$ARCH" ] && echo yes || echo no)"
file_check "RakshasaTarget.cs.meta exists"   "$([ -f "$RAK_META" ] && echo yes || echo no)"
file_check "WaveController.cs.meta exists"    "$([ -f "$WAVE_META" ] && echo yes || echo no)"
file_check "ArcherAutoFire.cs.meta exists"    "$([ -f "$ARCH_META" ] && echo yes || echo no)"

for META in "$RAK_META" "$WAVE_META" "$ARCH_META"; do
  if [ -f "$META" ]; then
    GUID=$(grep -oE 'guid: [a-f0-9]{32}' "$META" | awk '{print $2}')
    check "$(basename "$META") guid 32 hex" "$([ "${#GUID}" -eq 32 ] && echo yes || echo no)" "yes"
  fi
done

section "RakshasaTarget surface"
RAK_SRC="$([ -f "$RAK" ] && cat "$RAK" || echo "")"
check "namespace Jambudweep.Ramayana.Combat" \
  "$(echo "$RAK_SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Combat\s*$')" "1"
check "class RakshasaTarget sealed" \
  "$(echo "$RAK_SRC" | grep -cE 'public sealed class RakshasaTarget')" "1"
check "Damage(int) API" \
  "$(echo "$RAK_SRC" | grep -cE 'public void Damage\(int amount\)')" "1"
check "Configure(int) API" \
  "$(echo "$RAK_SRC" | grep -cE 'public void Configure\(int hp\)')" "1"
check "IsDead getter" \
  "$(echo "$RAK_SRC" | grep -cE 'public bool IsDead')" "1"
check "FindAllActive static helper" \
  "$(echo "$RAK_SRC" | grep -cE 'public static System\.Collections\.Generic\.IEnumerable<RakshasaTarget> FindAllActive')" "1"

section "WaveController surface"
WAVE_SRC="$([ -f "$WAVE" ] && cat "$WAVE" || echo "")"
check "namespace Jambudweep.Ramayana.Combat" \
  "$(echo "$WAVE_SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Combat\s*$')" "1"
check "class WaveController sealed" \
  "$(echo "$WAVE_SRC" | grep -cE 'public sealed class WaveController')" "1"
check "StartWaves(totalWaves) API" \
  "$(echo "$WAVE_SRC" | grep -cE 'public void StartWaves\(int totalWaves = 1\)')" "1"
check "StopWaves API" \
  "$(echo "$WAVE_SRC" | grep -cE 'public void StopWaves\(\)')" "1"
check "InstantiateRakshasa helper present" \
  "$(echo "$WAVE_SRC" | grep -cE 'private RakshasaTarget InstantiateRakshasa')" "1"
check "uses GameObject.CreatePrimitive fallback" \
  "$(echo "$WAVE_SRC" | grep -cE 'GameObject\.CreatePrimitive\(PrimitiveType\.Capsule\)')" "1"
check "onWaveStarted UnityEvent" \
  "$(echo "$WAVE_SRC" | grep -cE 'public UnityEvent<int> onWaveStarted')" "1"
check "onAllWavesCompleted UnityEvent" \
  "$(echo "$WAVE_SRC" | grep -cE 'public UnityEvent onAllWavesCompleted')" "1"

section "ArcherAutoFire surface"
ARCH_SRC="$([ -f "$ARCH" ] && cat "$ARCH" || echo "")"
check "namespace Jambudweep.Ramayana.Combat" \
  "$(echo "$ARCH_SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Combat\s*$')" "1"
check "class ArcherAutoFire sealed" \
  "$(echo "$ARCH_SRC" | grep -cE 'public sealed class ArcherAutoFire')" "1"
ge "fireInterval SerializeField (≥1)" \
  "$(echo "$ARCH_SRC" | grep -cE 'fireInterval')" "1"
ge "range SerializeField (≥1)" \
  "$(echo "$ARCH_SRC" | grep -cE 'range\s*=')" "1"
ge "coneAngleDegrees SerializeField (≥1)" \
  "$(echo "$ARCH_SRC" | grep -cE 'coneAngleDegrees')" "1"
check "calls RakshasaTarget.FindAllActive" \
  "$(echo "$ARCH_SRC" | grep -cE 'RakshasaTarget\.FindAllActive')" "1"
# Day 7: damage is applied via ArrowProjectile; archer still deals damagePerShot.
ge "spawns / initializes ArrowProjectile (≥1)" \
  "$(echo "$ARCH_SRC" | grep -cE 'ArrowProjectile')" "1"
ge "uses damagePerShot for projectile damage (≥1)" \
  "$(echo "$ARCH_SRC" | grep -cE 'damagePerShot')" "1"

section "Cross-file integration"
ge "WaveController references RakshasaTarget (≥1)" \
  "$(echo "$WAVE_SRC" | grep -cE 'RakshasaTarget')" "1"
ge "ArcherAutoFire references RakshasaTarget (≥1)" \
  "$(echo "$ARCH_SRC" | grep -cE 'RakshasaTarget')" "1"

section "Summary"
echo "  $PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
