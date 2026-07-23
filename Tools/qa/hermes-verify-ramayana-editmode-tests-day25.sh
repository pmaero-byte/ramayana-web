#!/bin/bash
# Day 25 — EditMode tests verifier
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

FILE="$ROOT/Assets/Tests/EditMode/Day25EditModeTests.cs"
[ -f "$FILE" ] && p "Day25EditModeTests.cs exists" || f "Day25EditModeTests.cs missing"
SRC=$(cat "$FILE" 2>/dev/null || true)
[ -n "$SRC" ] && p "file non-empty" || f "file empty"

# Test method presence
for test in SaveKeys_AllSlotConstantsPresent SaveData_CanBeSerializedAndDeserialized VerseSaveState_RestoreFromSaveData_UpdatesPlayerPrefs VerseSaveState_RestoreFromNull_ReturnsFalse KandaTree_HasEightEntries KandaTree_EntriesAreInCanonicalOrder KandaTree_GetEntry_ReturnsNullForEmptyId KandaTree_GetSceneName_MatchesExistingSceneFile KandaTree_GetNextKanda_ReturnsSequentialOrder KandaTree_EachKanda_HasMomentsJson KandaTree_MomentsJson_HasValidKandaField MomentsBalaKanda_LoadsFromResources MomentsAyodhyaKanda_LoadsFromResources SaveSlotPicker_DefaultSlotKeys_AreValidSaveKeys; do
  echo "$SRC" | grep -q "void $test" && p "test $test exists" || f "test $test missing"
done

# Namespace/reference checks
echo "$SRC" | grep -q "namespace Ramayana.Tests" && p "namespace Ramayana.Tests" || f "missing namespace"
echo "$SRC" | grep -q "using NUnit.Framework" && p "NUnit.Framework referenced" || f "NUnit missing"
echo "$SRC" | grep -q "using Jambudweep.Ramayana.Core" && p "Core namespace referenced" || f "Core missing"
echo "$SRC" | grep -q "using Jambudweep.Ramayana.Data" && p "Data namespace referenced" || f "Data missing"
echo "$SRC" | grep -q "using Jambudweep.Ramayana.Gameplay" && p "Gameplay namespace referenced" || f "Gameplay missing"

# Regression: Day 24 HUD still present
[ -f "$ROOT/Assets/Scripts/UI/SaveSlotPickerHud.cs" ] && p "SaveSlotPickerHud.cs present" || f "SaveSlotPickerHud.cs missing"

# Regression: Day 23 KandaTree still present
[ -f "$ROOT/Assets/Scripts/Gameplay/KandaTree.cs" ] && p "KandaTree.cs present" || f "KandaTree.cs missing"

# Regression: Day 19 verifier still present
[ -f "$ROOT/Tools/qa/hermes-verify-ramayana-archive-day19.sh" ] && p "Day 19 verifier present" || f "Day 19 verifier missing"

# Script syntax
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-kandatree-day23.sh" 2>/dev/null && p "Day 23 verifier bash -n" || f "Day 23 verifier bash -n"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-save-slots-day24.sh" 2>/dev/null && p "Day 24 verifier bash -n" || f "Day 24 verifier bash -n"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 25)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
