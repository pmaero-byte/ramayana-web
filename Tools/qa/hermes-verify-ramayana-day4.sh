#!/bin/bash
# hermes-verify-ramayana-day4.sh
#
# Day 4 ad-hoc verifier for HUD polish + save/load:
#   - HudOrchestrator wires StoryMomentPlayer + WaveController events
#     into existing KandaPortraitHUD / VerseStreakHUD / DayDotStrip /
#     SanskritTitle singletons.
#   - SaveLoadHud exposes Save / Load / Slot cycler buttons that call
#     into the existing static SaveSystem (Core namespace).
#
# Ad-hoc only. Unity Editor compile + Play-mode smoke live in the
# hermes-verify-ramayana-editmode-tests.sh suite.
set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
HUD="$ROOT/Assets/Scripts/Feedback/HudOrchestrator.cs"
HUD_META="$ROOT/Assets/Scripts/Feedback/HudOrchestrator.cs.meta"
SAVE="$ROOT/Assets/Scripts/UI/SaveLoadHud.cs"
SAVE_META="$ROOT/Assets/Scripts/UI/SaveLoadHud.cs.meta"
KANDA="$ROOT/Assets/Scripts/Feedback/KandaPortraitHUD.cs"
STREAK="$ROOT/Assets/Scripts/Feedback/VerseStreakHUD.cs"
DOT="$ROOT/Assets/Scripts/Feedback/DayDotStrip.cs"
SAYA="$ROOT/Assets/Scripts/Feedback/SanskritTitle.cs"
CORE="$ROOT/Assets/Scripts/Core/SaveSystem.cs"

PASS=0; FAIL=0
section() { echo ""; echo "== $1 =="; }
check()    { local name="$1" actual="$2" expected="$3"
  if [ "$actual" = "$expected" ]; then echo "  ✅ $name  ($actual)"; PASS=$((PASS+1))
  else echo "  ❌ $name  expected=$expected actual=$actual"; FAIL=$((FAIL+1)); fi
}
ge() { local name="$1" actual="$2" min="$3"
  if [ "$actual" -ge "$min" ] 2>/dev/null; then echo "  ✅ $name  ($actual ≥ $min)"; PASS=$((PASS+1))
  else echo "  ❌ $name  expected≥$min actual=$actual"; FAIL=$((FAIL+1)); fi
}
file_check() { local name="$1" present="$2"
  if [ "$present" = "yes" ]; then echo "  ✅ $name"; PASS=$((PASS+1))
  else echo "  ❌ $name  (not found)"; FAIL=$((FAIL+1)); fi
}

section "Day 4 — files exist"
file_check "HudOrchestrator.cs"           "$([ -f "$HUD" ] && echo yes || echo no)"
file_check "HudOrchestrator.cs.meta"      "$([ -f "$HUD_META" ] && echo yes || echo no)"
file_check "SaveLoadHud.cs"               "$([ -f "$SAVE" ] && echo yes || echo no)"
file_check "SaveLoadHud.cs.meta"          "$([ -f "$SAVE_META" ] && echo yes || echo no)"
file_check "KandaPortraitHUD.cs"          "$([ -f "$KANDA" ] && echo yes || echo no)"
file_check "VerseStreakHUD.cs"            "$([ -f "$STREAK" ] && echo yes || echo no)"
file_check "DayDotStrip.cs"               "$([ -f "$DOT" ] && echo yes || echo no)"
file_check "SanskritTitle.cs"             "$([ -f "$SAYA" ] && echo yes || echo no)"
file_check "SaveSystem.cs"                "$([ -f "$CORE" ] && echo yes || echo no)"

if [ -f "$HUD_META" ]; then
  GUID=$(grep -oE 'guid: [a-f0-9]{32}' "$HUD_META" | awk '{print $2}')
  check "HudOrchestrator guid 32 hex" "$([ "${#GUID}" -eq 32 ] && echo yes || echo no)" "yes"
fi
if [ -f "$SAVE_META" ]; then
  GUID=$(grep -oE 'guid: [a-f0-9]{32}' "$SAVE_META" | awk '{print $2}')
  check "SaveLoadHud guid 32 hex" "$([ "${#GUID}" -eq 32 ] && echo yes || echo no)" "yes"
fi

section "HudOrchestrator surface"
HUD_SRC="$([ -f "$HUD" ] && cat "$HUD" || echo "")"
check "namespace Jambudweep.Ramayana.Feedback" \
  "$(echo "$HUD_SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.Feedback\s*$')" "1"
check "class HudOrchestrator sealed" \
  "$(echo "$HUD_SRC" | grep -cE 'public sealed class HudOrchestrator')" "1"
check "Instance singleton" \
  "$(echo "$HUD_SRC" | grep -cE 'public static HudOrchestrator Instance')" "1"
check "EnsureCreated factory" \
  "$(echo "$HUD_SRC" | grep -cE 'public static void EnsureCreated')" "1"
check "wires StoryMomentPlayer.onObjectiveEntered" \
  "$(echo "$HUD_SRC" | grep -cE 'momentPlayer\.onObjectiveEntered\.AddListener')" "1"
check "wires StoryMomentPlayer.onActCompleted" \
  "$(echo "$HUD_SRC" | grep -cE 'momentPlayer\.onActCompleted\.AddListener')" "1"
check "wires WaveController.onWaveCompleted" \
  "$(echo "$HUD_SRC" | grep -cE 'waveController\.onWaveCompleted\.AddListener')" "1"
check "wires WaveController.onAllWavesCompleted" \
  "$(echo "$HUD_SRC" | grep -cE 'waveController\.onAllWavesCompleted\.AddListener')" "1"
ge "actIdsInDayOrder entries (≥7)" \
  "$(echo "$HUD_SRC" | grep -cE '"(bala-birth|ayodhya-dharma|panchavati-golden-deer|kishkindha-alliance|sundarakanda-leap|yuddhakanda-war|return-ayodhya|uttara-earth-return)"')" "7"
check "drives KandaPortraitHUD.Show" \
  "$(echo "$HUD_SRC" | grep -cE 'KandaPortraitHUD\.Instance\.Show')" "1"
ge "drives VerseStreakHUD.OnSuccess (≥1)" \
  "$(echo "$HUD_SRC" | grep -cE 'VerseStreakHUD\.Instance\.OnSuccess')" "1"
ge "drives DayDotStrip.SetDay (≥1)" \
  "$(echo "$HUD_SRC" | grep -cE 'DayDotStrip\.Instance\.SetDay')" "1"
ge "drives SanskritTitle.Show (≥1)" \
  "$(echo "$HUD_SRC" | grep -cE 'SanskritTitle\.Instance\.Show')" "1"

section "SaveLoadHud surface"
SAVE_SRC="$([ -f "$SAVE" ] && cat "$SAVE" || echo "")"
check "namespace Jambudweep.Ramayana.UI" \
  "$(echo "$SAVE_SRC" | grep -cE '^namespace Jambudweep\.Ramayana\.UI\s*$')" "1"
check "class SaveLoadHud sealed" \
  "$(echo "$SAVE_SRC" | grep -cE 'public sealed class SaveLoadHud')" "1"
check "public Save() API" \
  "$(echo "$SAVE_SRC" | grep -cE 'public void Save\(\)')" "1"
check "public Load() API" \
  "$(echo "$SAVE_SRC" | grep -cE 'public void Load\(\)')" "1"
check "public DeleteSave() API" \
  "$(echo "$SAVE_SRC" | grep -cE 'public void DeleteSave\(\)')" "1"
check "public SetSlot() API" \
  "$(echo "$SAVE_SRC" | grep -cE 'public void SetSlot\(string')" "1"
check "calls SaveSystem.Save" \
  "$(echo "$SAVE_SRC" | grep -cE 'SaveSystem\.Save\(slotKey')" "1"
check "calls SaveSystem.Load" \
  "$(echo "$SAVE_SRC" | grep -cE 'SaveSystem\.Load\(slotKey')" "1"
check "calls SaveSystem.DeleteSlot" \
  "$(echo "$SAVE_SRC" | grep -cE 'SaveSystem\.DeleteSlot\(slotKey')" "1"
check "Builds SaveData from StoryEngine" \
  "$(echo "$SAVE_SRC" | grep -cE 'new SaveData')" "1"
check "Builds UI with 3 buttons (Save/Load/Slot)" \
  "$(echo "$SAVE_SRC" | grep -cE 'BuildButton\(bar\.transform')" "3"
ge "Uses StoryEngine state for save (≥1)" \
  "$(echo "$SAVE_SRC" | grep -cE 'FindFirstObjectByType<Story\.StoryEngine>')" "1"

section "Existing HUDs have the surface HudOrchestrator needs"
check "KandaPortraitHUD.Show(int,int)" \
  "$(grep -cE 'public void Show\(int kanda, int sarga\)' "$KANDA")" "1"
check "VerseStreakHUD.OnSuccess()" \
  "$(grep -cE 'public void OnSuccess' "$STREAK")" "1"
check "DayDotStrip.SetDay(int,int)" \
  "$(grep -cE 'public void SetDay\(int day, int versesHeard\)' "$DOT")" "1"
check "SanskritTitle.Show(int)" \
  "$(grep -cE 'public void Show\(int day\)' "$SAYA")" "1"

section "Summary"
echo "  $PASS passed, $FAIL failed"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
