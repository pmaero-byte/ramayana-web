#!/bin/bash
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

# New Day 36 bridge file
BR="$ROOT/Assets/Scripts/UI/KandaLaunchBridge.cs"
[ -f "$BR" ] && p "KandaLaunchBridge.cs exists" || f "KandaLaunchBridge.cs missing"
SRC=$(cat "$BR" 2>/dev/null || true)
[ -n "$SRC" ] && p "KandaLaunchBridge non-empty" || f "KandaLaunchBridge empty"
echo "$SRC" | grep -q 'class KandaLaunchBridge' && p "KandaLaunchBridge class" || f "class missing"
echo "$SRC" | grep -q 'public static void Select(' && p "Select API" || f "Select missing"
echo "$SRC" | grep -q 'ConsumePending' && p "ConsumePending" || f "ConsumePending missing"
echo "$SRC" | grep -q 'ResolveKandaId' && p "ResolveKandaId" || f "ResolveKandaId missing"
echo "$SRC" | grep -q 'VerseOrchestrator.Instance' && p "references VerseOrchestrator" || f "VerseOrchestrator missing"
echo "$SRC" | grep -q 'RagaAudioEngine.Instance' && p "references RagaAudioEngine" || f "RagaAudioEngine missing"
echo "$SRC" | grep -q 'VerseCombatTrigger.Instance' && p "references VerseCombatTrigger" || f "VerseCombatTrigger missing"
echo "$SRC" | grep -q 'KandaTree.TryLoadScene' && p "KandaTree.TryLoadScene" || f "TryLoadScene missing"

# MainMenu wires to KandaLaunchBridge
MM="$ROOT/Assets/Scripts/UI/Screens/MainMenuScreenController.cs"
[ -f "$MM" ] && p "MainMenuScreenController exists" || f "MainMenuScreenController missing"
SRC_MM=$(cat "$MM" 2>/dev/null || true)
echo "$SRC_MM" | grep -q 'KandaLaunchBridge.Select' && p "MainMenu calls KandaLaunchBridge.Select" || f "MainMenu missing KandaLaunchBridge bridge"

# PlayerSceneBootstrap consumes pending kanda and calls VerseOrchestrator
PS="$ROOT/Assets/Scripts/Scene/PlayerSceneBootstrap.cs"
[ -f "$PS" ] && p "PlayerSceneBootstrap exists" || f "PlayerSceneBootstrap missing"
SRC_PS=$(cat "$PS" 2>/dev/null || true)
echo "$SRC_PS" | grep -q 'KandaLaunchBridge.ConsumePending' && p "PlayerSceneBootstrap consumes pending kanda" || f "PlayerSceneBootstrap missing bridge hook"
echo "$SRC_PS" | grep -q 'VerseOrchestrator.Instance' && p "PlayerSceneBootstrap calls VerseOrchestrator" || f "PlayerSceneBootstrap missing VerseOrchestrator"
echo "$SRC_PS" | grep -q 'VerseCombatTrigger.Instance' && p "PlayerSceneBootstrap resets combat" || f "PlayerSceneBootstrap missing VerseCombatTrigger"

# RagaAudioEngine.RagaAudioEngine.Instance available
RT="$ROOT/Assets/Scripts/Audio/RagaAudioEngine.cs"
[ -f "$RT" ] && p "RagaAudioEngine.cs exists" || f "RagaAudioEngine.cs missing"
SRC_RT=$(cat "$RT" 2>/dev/null || true)
echo "$SRC_RT" | grep -q 'public static RagaAudioEngine Instance' && p "RagaAudioEngine singleton" || f "RagaAudioEngine missing Instance"

# Voices corpus still 108 cues
VC="$ROOT/Assets/Resources/Ramayana/voices.json"
[ -f "$VC" ] && p "voices.json exists" || f "voices.json missing"
TOTAL=$(python3 -c "import json; print(len(json.load(open('$VC')).get('voiceCues', [])))" 2>/dev/null || echo 0)
[ "$TOTAL" -ge 108 ] && p "voices.json cues >= 108 ($TOTAL)" || f "voices.json cues < 108"

# KandaTreeSceneBootstrap no longer does TryLoadScene directly for user taps
KS="$ROOT/Assets/Scripts/UI/KandaTreeSceneBootstrap.cs"
[ -f "$KS" ] && p "KandaTreeSceneBootstrap exists" || f "KandaTreeSceneBootstrap missing"
SRC_KS=$(cat "$KS" 2>/dev/null || true)
! echo "$SRC_KS" | grep -q 'bool launched = KandaTree.TryLoadScene' && p "KandaTreeSceneBootstrap delegates scene load to bridge" || f "KandaTreeSceneBootstrap still fires TryLoadScene directly"

# Verifier regression
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-scene-transition-day34.sh" 2>/dev/null && p "Day 34 verifier bash -n" || f "Day 34 verifier bash -n"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-voices-corpus-day35.sh" 2>/dev/null && p "Day 35 verifier bash -n" || f "Day 35 verifier bash -n"
bash -n "$ROOT/Tools/qa/hermes-verify-audio-day33.sh" 2>/dev/null && p "Day 33 verifier bash -n" || f "Day 33 verifier bash -n"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 36 mainmenu wiring)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
