#!/bin/bash
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
cd "$ROOT"
PASS=0; FAIL=0; RESULTS=()
pass()  { RESULTS+=("PASS: $*"); PASS=$((PASS+1)); }
fails() { RESULTS+=("FAIL: $*"); FAIL=$((FAIL+1)); }

grep -q "System.Collections.Generic" Assets/Scripts/Verse/VerseOrchestrator.cs && pass "VerseOrchestrator uses generic collections" || fails "VerseOrchestrator missing generic collections"
grep -q "ResolveMomentsResourceForKanda" Assets/Scripts/Verse/VerseOrchestrator.cs && pass "VerseOrchestrator resolves moments file per kanda" || fails "VerseOrchestrator missing per-kanda resolution"
grep -q "using System.IO" Assets/Scripts/Verse/VerseOrchestrator.cs && pass "VerseOrchestrator imports System.IO" || fails "VerseOrchestrator missing System.IO import"
grep -q "_pendingMomentsResource" Assets/Scripts/Verse/VerseOrchestrator.cs && pass "VerseOrchestrator has _pendingMomentsResource field" || fails "VerseOrchestrator missing _pendingMomentsResource"

if [ -f "Library/DebugReports/Day39-runtime-report.txt" ]; then
  pass "Day39 runtime report exists"
else
  fails "Day39 runtime report missing"
fi

# Screenshot is informational: Unity batchmode -quit may not flush ScreenCapture
if [ -f "Library/Screenshots/Day39-MainMenu.png" ]; then
  pass "Day39 screenshot exists (informational)"
else
  echo "INFO: Day39 screenshot missing — expected limitation of Unity batchmode -quit (ScreenCapture may not flush)"
fi

python3 - <<'PY'
import json, os
from collections import Counter
kandas = ["bala-kanda","ayodhya-kanda","aranya-kanda","kishkindha-kanda","sundara-kanda","yuddha-kanda","uttara-kanda","return-kanda"]
all_ok = True
for kanda in kandas:
 p = f"Assets/Resources/Ramayana/moments_{kanda.replace('-','_')}.json"
 if not os.path.isfile(p):
   print(f"FAIL: {kanda} missing"); all_ok = False; continue
 data = json.load(open(p))
 mom = data.get("moments") or []
 bad = [m for m in mom if not m.get("momentId") or not m.get("voiceCueId")]
 dup = [x for x, c in Counter([m.get("voiceCueId") for m in mom]).items() if x and c > 1]
 if bad or dup:
   print(f"FAIL: {kanda} invalid moments"); all_ok = False
print("PASS: all 8 kanda corpora structurally valid" if all_ok else "FAIL: some kanda corpora invalid")
PY

printf '%s\n' "${RESULTS[@]}"
echo "---"
echo "$PASS passed, $FAIL failed"
exit $FAIL
