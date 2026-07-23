#!/bin/bash
# Day 24 — 3-slot save/load system verifier
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

# --- New files exist ---
[ -f "$ROOT/Assets/Scripts/UI/SaveSlotPickerHud.cs" ] \
  && p "SaveSlotPickerHud.cs exists" || f "SaveSlotPickerHud.cs missing"
[ -f "$ROOT/Assets/Scripts/Feedback/VerseSaveState.cs" ] \
  && p "VerseSaveState.cs exists" || f "VerseSaveState.cs missing"

# --- SaveSlotPicker references SaveKeys ---
grep -q "Data.SaveKeys" "$ROOT/Assets/Scripts/UI/SaveSlotPickerHud.cs" \
  && p "SaveSlotPicker references Data.SaveKeys" || f "SaveSlotPicker missing Data.SaveKeys"

# --- VerseSaveState restore method exists ---
grep -q "RestoreFromSaveData" "$ROOT/Assets/Scripts/Feedback/VerseSaveState.cs" \
  && p "VerseSaveState.RestoreFromSaveData exists" || f "VerseSaveState missing RestoreFromSaveData"

# --- SaveSlotPicker has 4 slot keys wired ---
COUNT=$(grep -c 'slotKeys = new string' "$ROOT/Assets/Scripts/UI/SaveSlotPickerHud.cs" || echo 0)
[ "$COUNT" = "1" ] && p "slotKeys array declared ($COUNT)" || f "slotKeys array count == 1 ($COUNT)"

# --- Refers to 3 slots + autosave in slotKeys array (array line) ---
SLOTS_LINE=$(grep -A5 'slotKeys = new string' "$ROOT/Assets/Scripts/UI/SaveSlotPickerHud.cs" | head -6 || true)
echo "$SLOTS_LINE" | grep -q 'Slot1' \
  && p "Slot1 present in SaveSlotPicker slots" || f "Slot1 missing"
echo "$SLOTS_LINE" | grep -q 'Slot2' \
  && p "Slot2 present in SaveSlotPicker slots" || f "Slot2 missing"
echo "$SLOTS_LINE" | grep -q 'Slot3' \
  && p "Slot3 present in SaveSlotPicker slots" || f "Slot3 missing"
echo "$SLOTS_LINE" | grep -q 'AutoSave' \
  && p "AutoSave present in SaveSlotPicker slots" || f "AutoSave missing"

# --- SaveLoadHud untouched (Day 4) ---
SAVELOAD_LINES=$(wc -l < "$ROOT/Assets/Scripts/UI/SaveLoadHud.cs" | tr -d ' ')
[ "$SAVELOAD_LINES" = "231" ] && p "SaveLoadHud untouched (231 lines)" || f "SaveLoadHud modified (expected 231, got $SAVELOAD_LINES)"

# --- Scene files unchanged ---
SAVE_SCENE_HASH=$(md5 "$ROOT/Assets/Scenes/MainMenu.unity" | awk '{print $4}')
echo "  INFO: MainMenu.unity md5=$SAVE_SCENE_HASH"

# --- Regression: Day 23 KandaTree committed ---
[ -f "$ROOT/Assets/Scripts/Gameplay/KandaTree.cs" ] \
  && p "KandaTree.cs still present" || f "KandaTree.cs missing"
[ -f "$ROOT/Assets/Scripts/Gameplay/KandaPermissions.cs" ] \
  && p "KandaPermissions.cs still present" || f "KandaPermissions.cs missing"

# --- Script syntax (.sh only; C# checked by Unity compile later) ---
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-kandatree-day23.sh" 2>/dev/null \
  && p "Day 23 verifier bash -n" || f "Day 23 verifier bash -n"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-save-slots-day24.sh" 2>/dev/null \
  && p "Day 24 verifier bash -n" || f "Day 24 verifier bash -n"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 24)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
