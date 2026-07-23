#!/bin/bash
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

BR="$ROOT/Assets/Scripts/UI/KandaLaunchBridge.cs"
PS="$ROOT/Assets/Scripts/Scene/PlayerSceneBootstrap.cs"
MM="$ROOT/Assets/Scripts/UI/Screens/MainMenuScreenController.cs"
KS="$ROOT/Assets/Scripts/UI/KandaTreeSceneBootstrap.cs"

SRC_BR=$(cat "$BR")
SRC_PS=$(cat "$PS")

# 1. Bridge: state carrier only (no moment ownership in executable code).
echo "$SRC_BR" | grep -q 'pendingKanda' && p "bridge holds pendingKanda" || f "pendingKanda missing"
echo "$SRC_BR" | grep -q 'DontDestroyOnLoad' && p "bridge persists across scene loads" || f "bridge missing persistence"
ACTUAL_VO=$(echo "$SRC_BR" | grep -E '^\s*(this\.)?(StartCoroutine\()?VerseOrchestrator' | grep -v '//')
[ -z "$ACTUAL_VO" ] && p "bridge does not own VerseOrchestrator" || f "bridge over-reaches into VerseOrchestrator"
ACTUAL_RA=$(echo "$SRC_BR" | grep -E '^\s*(this\.)?RagaAudioEngine\.Instance' | grep -v '//')
[ -z "$ACTUAL_RA" ] && p "bridge does not own RagaAudioEngine" || f "bridge over-reaches into RagaAudioEngine"
ACTUAL_VC=$(echo "$SRC_BR" | grep -E '^\s*(this\.)?VerseCombatTrigger\.Instance' | grep -v '//')
[ -z "$ACTUAL_VC" ] && p "bridge does not own VerseCombatTrigger" || f "bridge over-reaches into VerseCombatTrigger"
echo "$SRC_BR" | grep -q 'Select(string actId)' && p "bridge API accepts actId" || f "Select actId parameter missing"

# 2. PlayerSceneBootstrap: owns the moment+audio+combat chain.
echo "$SRC_PS" | grep -q 'VerseOrchestrator.Instance' && p "PlayerSceneBootstrap owns VerseOrchestrator" || f "missing VerseOrchestrator"
echo "$SRC_PS" | grep -q 'VerseCombatTrigger.Instance' && p "PlayerSceneBootstrap owns VerseCombatTrigger" || f "missing VerseCombatTrigger"
echo "$SRC_PS" | grep -q 'ConsumePending' && p "PlayerSceneBootstrap consumes pending kanda" || f "missing pending hook"
ACTUAL_LOADACT=$(echo "$SRC_PS" | grep -E '^\s*_momentPlayer\.LoadAct\(' | grep -v '//')
[ -z "$ACTUAL_LOADACT" ] && p "PlayerSceneBootstrap does not call legacy LoadAct" || f "PlayerSceneBootstrap still calls legacy LoadAct"

# 3. MainMenu delegates to bridge.
SRC_MM=$(cat "$MM")
echo "$SRC_MM" | grep -q 'KandaLaunchBridge.Select' && p "MainMenu delegates to bridge" || f "MainMenu missing bridge"

# 4. KandaTreeSceneBootstrap delegates to bridge.
SRC_KS=$(cat "$KS")
echo "$SRC_KS" | grep -q 'KandaLaunchBridge.Select' && p "KandaTreeSceneBootstrap owns bridge" || f "bridge missing"

# 5. StoryMomentPlayer exists as fallback object but not driven by default path.
SMP="$ROOT/Assets/Scripts/Story/StoryMomentPlayer.cs"
[ -f "$SMP" ] && p "StoryMomentPlayer exists (fallback available)" || f "StoryMomentPlayer missing"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 36 bridge/owner contract)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
