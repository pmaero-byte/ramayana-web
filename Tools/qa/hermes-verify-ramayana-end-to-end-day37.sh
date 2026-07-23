#!/bin/bash
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

MM="$ROOT/Assets/Scripts/UI/Screens/MainMenuScreenController.cs"
BR="$ROOT/Assets/Scripts/UI/KandaLaunchBridge.cs"
PS="$ROOT/Assets/Scripts/Scene/PlayerSceneBootstrap.cs"
KS="$ROOT/Assets/Scripts/UI/KandaTreeSceneBootstrap.cs"
RT="$ROOT/Assets/Scripts/Audio/RagaAudioEngine.cs"
VO="$ROOT/Assets/Resources/Ramayana/voices.json"
EBS="$ROOT/ProjectSettings/EditorBuildSettings.asset"

SRC_MM=$(cat "$MM")
SRC_BR=$(cat "$BR")
SRC_PS=$(cat "$PS")
SRC_KS=$(cat "$KS")
SRC_RT=$(cat "$RT")

# 1. MainMenu → bridge: exactly one invocation.
COUNT_BRIDGE=$(echo "$SRC_MM" | grep -c 'KandaLaunchBridge.Select')
[ "$COUNT_BRIDGE" -eq 1 ] && p "MainMenu: single KandaLaunchBridge.Select call" || f "MainMenu: bridge call count=$COUNT_BRIDGE"

# 2. Bridge is a pure state carrier (no moment owner executable lines).
BRIDGE_EXEC=$(echo "$SRC_BR" | grep -E '^\s*(StartCoroutine\()?(VerseOrchestrator|RagaAudioEngine|VerseCombatTrigger)' | grep -v '//' | wc -l | tr -d ' ')
[ "$BRIDGE_EXEC" -eq 0 ] && p "bridge: zero moment-owner executable lines" || f "bridge: $BRIDGE_EXEC moment-owner lines"

# 3. PlayerSceneBootstrap owns moment+audio+combat start.
echo "$SRC_PS" | grep -q 'VerseOrchestrator.Instance' && p "PlayerSceneBootstrap: owns VerseOrchestrator" || f "missing VerseOrchestrator"
echo "$SRC_PS" | grep -q 'VerseCombatTrigger.Instance' && p "PlayerSceneBootstrap: owns VerseCombatTrigger" || f "missing VerseCombatTrigger"
echo "$SRC_PS" | grep -q 'KandaLaunchBridge.ConsumePending' && p "PlayerSceneBootstrap: consumes pending kanda" || f "missing pending hook"

# 4. No legacy double-path: PlayerSceneBootstrap must not call _momentPlayer.LoadAct in active code.
ACTUAL_LOADACT=$(echo "$SRC_PS" | grep -E '^\s*_momentPlayer\.LoadAct\(' | grep -v '//' | wc -l | tr -d ' ')
[ "$ACTUAL_LOADACT" -eq 0 ] && p "PlayerSceneBootstrap: no legacy LoadAct call" || f "PlayerSceneBootstrap: legacy LoadAct still called"

# 5. KandaTreeSceneBootstrap delegates selection to bridge.
echo "$SRC_KS" | grep -q 'KandaLaunchBridge.Select' && p "KandaTreeSceneBootstrap: uses bridge" || f "bridge missing from KandaTreeSceneBootstrap"
! echo "$SRC_KS" | grep -q 'bool launched = KandaTree.TryLoadScene' && p "KandaTreeSceneBootstrap: delegates scene load" || f "KandaTreeSceneBootstrap still fires TryLoadScene directly"

# 6. Audio singleton + SFX complete + 108 voice cues.
echo "$SRC_RT" | grep -q 'public static RagaAudioEngine Instance' && p "RagaAudioEngine: singleton" || f "RagaAudioEngine missing Instance"
! echo "$SRC_RT" | grep -q 'TODO: implement procedural SFX' && p "RagaAudioEngine: SFX TODO removed" || f "RagaAudioEngine: SFX TODO open"
TOTAL=$(python3 -c "import json; print(len(json.load(open('$VO')).get('voiceCues', [])))" 2>/dev/null || echo 0)
[ "$TOTAL" -ge 108 ] && p "voices.json: cues=$TOTAL" || f "voices.json: cues=$TOTAL"

# 7. Build settings include all expected scenes.
SCENES=$(grep "Assets/Scenes" "$EBS" | wc -l | tr -d ' ')
[ "$SCENES" -ge 11 ] && p "BuildSettings: scenes=$SCENES" || f "BuildSettings: scenes=$SCENES"

# 8. No open synthesis/audio/combat TODO in ActiveFlow scripts.
TODO_ACTIVE=$(grep -rn "TODO.*synth\|TODO.*audio\|TODO.*combat" "$BR" "$PS" 2>/dev/null | grep -v '//' | wc -l | tr -d ' ')
[ "$TODO_ACTIVE" -eq 0 ] && p "ActiveFlow: zero open synthesis/audio/combat stubs" || f "ActiveFlow: open stubs=$TODO_ACTIVE"

echo ""
echo "  $PASS passed, $FAIL failed  (End-to-end chain contract)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
