#!/bin/bash
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

MAINMENU="$ROOT/Assets/Scripts/UI/Screens/MainMenuScreenController.cs"
BRIDGE="$ROOT/Assets/Scripts/UI/KandaLaunchBridge.cs"
PLAYER="$ROOT/Assets/Scripts/Scene/PlayerSceneBootstrap.cs"
KANDA_TREE="$ROOT/Assets/Scripts/UI/KandaTreeSceneBootstrap.cs"
AUDIO="$ROOT/Assets/Scripts/Audio/RagaAudioEngine.cs"
VOICES="$ROOT/Assets/Resources/Ramayana/voices.json"
EBS="$ROOT/ProjectSettings/EditorBuildSettings.asset"

SRC_MAINMENU=$(cat "$MAINMENU")
SRC_BRIDGE=$(cat "$BRIDGE")
SRC_PLAYER=$(cat "$PLAYER")
SRC_KANDA_TREE=$(cat "$KANDA_TREE")
SRC_AUDIO=$(cat "$AUDIO")

# 1. MainMenu resume button + handler
echo "$SRC_MAINMENU" | grep -q 'OnResumeClicked' && p "MainMenu: OnResumeClicked exists" || f "MainMenu: OnResumeClicked missing"
echo "$SRC_MAINMENU" | grep -q 'GetMostRecentSave' && p "MainMenu: GetMostRecentSave wired" || f "MainMenu: GetMostRecentSave missing"
echo "$SRC_MAINMENU" | grep -q 'OnNewGameClicked' && p "MainMenu: OnNewGameClicked exists" || f "MainMenu: OnNewGameClicked missing"
echo "$SRC_MAINMENU" | grep -q 'SceneManager.LoadScene("TitleScreen")' && p "MainMenu: new game -> TitleScreen" || f "MainMenu: TitleScreen path missing"
MATCH_RESUME=$(echo "$SRC_MAINMENU" | grep -c "OnResumeClicked")
[ "$MATCH_RESUME" -eq 1 ] && p "MainMenu: single OnResumeClicked handler" || f "MainMenu: Resume count=$MATCH_RESUME"

# 2. Bridge pure-state carrier
echo "$SRC_BRIDGE" | grep -q 'pendingKanda' && p "bridge: holds pendingKanda" || f "bridge: pendingKanda missing"
echo "$SRC_BRIDGE" | grep -q 'DontDestroyOnLoad' && p "bridge: persists" || f "bridge: missing persistence"
ACTUAL_BRIDGE_EXEC=$(echo "$SRC_BRIDGE" | grep -E '^\s*(StartCoroutine\()?(VerseOrchestrator|RagaAudioEngine|VerseCombatTrigger)' | grep -v '//' | wc -l | tr -d ' ')
[ "$ACTUAL_BRIDGE_EXEC" -eq 0 ] && p "bridge: zero moment-owner executable lines" || f "bridge: $ACTUAL_BRIDGE_EXEC moment-owner lines"
echo "$SRC_BRIDGE" | grep -q 'Select(string actId)' && p "bridge: Select API" || f "bridge: Select missing"

# 3. PlayerSceneBootstrap owns moment+audio+combat
echo "$SRC_PLAYER" | grep -q 'VerseOrchestrator.Instance' && p "PlayerSceneBootstrap: owns VerseOrchestrator" || f "missing VerseOrchestrator"
echo "$SRC_PLAYER" | grep -q 'VerseCombatTrigger.Instance' && p "PlayerSceneBootstrap: owns VerseCombatTrigger" || f "missing VerseCombatTrigger"
echo "$SRC_PLAYER" | grep -q 'KandaLaunchBridge.ConsumePending' && p "PlayerSceneBootstrap: consumes pending" || f "missing pending hook"
ACTUAL_LOADACT=$(echo "$SRC_PLAYER" | grep -E '^\s*_momentPlayer\.LoadAct\(' | grep -v '//' | wc -l | tr -d ' ')
[ "$ACTUAL_LOADACT" -eq 0 ] && p "PlayerSceneBootstrap: no legacy LoadAct" || f "PlayerSceneBootstrap: legacy LoadAct present"

# 4. KandaTreeSceneBootstrap delegates to bridge
echo "$SRC_KANDA_TREE" | grep -q 'KandaLaunchBridge.Select' && p "KandaTreeSceneBootstrap: uses bridge" || f "bridge missing"
! echo "$SRC_KANDA_TREE" | grep -q 'bool launched = KandaTree.TryLoadScene' && p "KandaTreeSceneBootstrap: delegates scene load" || f "still fires TryLoadScene"

# 5. Audio singleton + SFX complete
echo "$SRC_AUDIO" | grep -q 'public static RagaAudioEngine Instance' && p "RagaAudioEngine: singleton" || f "RagaAudioEngine missing Instance"
! echo "$SRC_AUDIO" | grep -q 'TODO: implement procedural SFX' && p "RagaAudioEngine: SFX TODO removed" || f "SFX TODO open"

# 6. Voices corpus still 108 cues
TOTAL=$(python3 -c "import json; print(len(json.load(open('$VOICES')).get('voiceCues', [])))" 2>/dev/null || echo 0)
[ "$TOTAL" -ge 108 ] && p "voices.json: cues=$TOTAL" || f "voices.json: cues=$TOTAL"

# 7. Build settings include all scenes
SCENES=$(grep "Assets/Scenes" "$EBS" | wc -l | tr -d ' ')
[ "$SCENES" -ge 11 ] && p "BuildSettings: scenes=$SCENES" || f "BuildSettings: scenes=$SCENES"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 38 MainMenu resume + bridge/owner contract)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
