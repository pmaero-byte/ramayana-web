#!/bin/bash
# hermes-verify-ramayana-all.sh
#
# Umbrella verifier — runs Days 1, 2, 3, 4, 6, 7 in sequence and reports
# the aggregate pass/fail. Day 5 EditMode tests (RamayanaWireupTests.cs)
# live under Assets/Tests/EditMode and require Unity Test Framework; they
# are NOT covered by this script (run via Unity batchmode separately).
#
# Ad-hoc only. Each day has its own hermes-verify-ramayana-dayN.sh.

set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
DAYS=(1 2 3 4 6 7 8 9 10)
TOTAL_PASS=0
TOTAL_FAIL=0

for d in "${DAYS[@]}"; do
    VERIFIER="$SCRIPT_DIR/hermes-verify-ramayana-day${d}.sh"
    if [ ! -f "$VERIFIER" ]; then
        echo "  ❌ Day $d verifier missing at $VERIFIER"
        TOTAL_FAIL=$((TOTAL_FAIL+1))
        continue
    fi
    echo ""
    echo "================================================================"
    echo "  Day $d"
    echo "================================================================"
    HERMES_VERIFY_ROOT="$ROOT" bash "$VERIFIER"
    d_status=$?
    if [ $d_status -eq 0 ]; then
        # Extract pass/fail counts from the day verifier output.
        PASS_LINE=$(HERMES_VERIFY_ROOT="$ROOT" bash "$VERIFIER" 2>&1 | grep -E "passed, [0-9]+ failed" | tail -1)
        PASS_COUNT=$(echo "$PASS_LINE" | sed -nE 's/.* ([0-9]+) passed.*/\1/p')
        FAIL_COUNT=$(echo "$PASS_LINE" | sed -nE 's/.* ([0-9]+) failed.*/\1/p')
        TOTAL_PASS=$((TOTAL_PASS + ${PASS_COUNT:-0}))
        TOTAL_FAIL=$((TOTAL_FAIL + ${FAIL_COUNT:-0}))
    else
        TOTAL_FAIL=$((TOTAL_FAIL+1))
    fi
done

echo ""
echo "================================================================"
echo "  Aggregate (Days 1-4, 6-10)"
echo "================================================================"
echo "  $TOTAL_PASS passed, $TOTAL_FAIL failed"
[ "$TOTAL_FAIL" -eq 0 ] && exit 0 || exit 1
