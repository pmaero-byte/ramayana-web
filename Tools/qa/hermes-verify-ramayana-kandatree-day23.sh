#!/bin/bash
# Day 23 — KandaTree navigation system verifier
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

KANDATREE="$ROOT/Assets/Scripts/Gameplay/KandaTree.cs"
VERSE="$ROOT/Assets/Scripts/Verse/VerseOrchestrator.cs"
MAINMENU="$ROOT/Assets/Scripts/UI/Screens/MainMenuScreenController.cs"
BOOTSTRAP="$ROOT/Assets/Scripts/UI/KandaTreeSceneBootstrap.cs"
RESOURCES="$ROOT/Assets/Resources/Ramayana"
SCENES="$ROOT/Assets/Scenes"

# --- KandaTree.cs exists ---
[ -f "$KANDATREE" ] && p "KandaTree.cs exists" || f "KandaTree.cs missing"

# --- Registry size = 8 kandas ---
COUNT=$(grep -cE '^\s*new KandaEntry$' "$KANDATREE" 2>/dev/null || echo 0)
[ "$COUNT" = "8" ] && p "registry has 8 kandas ($COUNT)" || f "registry != 8 ($COUNT)"

# --- Each registered kanda has corresponding scene file ---
for id in bala-kanda ayodhya-kanda aranya-kanda kishkindha-kanda sundara-kanda yuddha-kanda uttara-kanda return-kanda; do
  SCENE=$(grep -A10 "kandaId = \"$id\"" "$KANDATREE" | grep 'sceneName' | sed -E 's/.*"([^"]+)".*/\1/' || true)
  if [ -n "$SCENE" ] && [ -f "$SCENES/${SCENE}.unity" ]; then
    p "scene exists for $id: $SCENE.unity"
  else
    f "scene missing for $id (resolved=$SCENE)"
  fi
done

# --- Each registered kanda has moments JSON file (stem only) ---
for id in bala-kanda ayodhya-kanda aranya-kanda kishkindha-kanda sundara-kanda yuddha-kanda uttara-kanda return-kanda; do
  FILE=$(grep -A15 "kandaId = \"$id\"" "$KANDATREE" | grep 'corpusFileName' | head -1 | sed -E 's/.*"([^"]+)".*/\1/' | tr -d ' ' || true)
  if [ -n "$FILE" ] && [ -f "$RESOURCES/${FILE}.json" ]; then
    p "moments JSON exists for $id: ${FILE}.json"
  else
    f "moments JSON missing for $id (resolved=$FILE)"
  fi
done

# --- Optional permissions stub present ---
[ -f "$ROOT/Assets/Scripts/Gameplay/KandaPermissions.cs" ] \
  && p "KandaPermissions.cs stub exists" || f "KandaPermissions.cs missing"

# --- VerseOrchestrator references KandaTree ---
grep -q 'Gameplay.KandaTree' "$VERSE" && p "VerseOrchestrator references KandaTree" || f "VerseOrchestrator missing KandaTree ref"

# --- MainMenu untouched by Day 23 (scope discipline) ---
MAINMENU_LINES=$(wc -l < "$MAINMENU" | tr -d ' ')
if [ "$MAINMENU_LINES" = "244" ]; then
  p "MainMenuScreenController unchanged (244 lines)"
else
  f "MainMenuScreenController modified (expected 244, got $MAINMENU_LINES)"
fi

# --- KandaTreeSceneBootstrap.cs exists ---
[ -f "$BOOTSTRAP" ] && p "KandaTreeSceneBootstrap.cs exists" || f "KandaTreeSceneBootstrap.cs missing"

# --- KandaTreeSceneBootstrap wire-up paths ---
grep -q 'TryLoadScene' "$BOOTSTRAP" && p "Bootstrap wires TryLoadScene" || f "Bootstrap missing TryLoadScene"
grep -q 'KandaTree.GetEntry' "$BOOTSTRAP" && p "Bootstrap uses KandaTree.GetEntry" || f "Bootstrap missing GetEntry"
grep -q 'SelectKanda' "$BOOTSTRAP" && p "Bootstrap exposes SelectKanda" || f "Bootstrap missing SelectKanda"

# --- KandaTree verifier bash-n clean ---
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-ayodhya-kanda-day22.sh" 2>/dev/null \
  && p "Day 22 verifier bash -n" \
  || f "Day 22 verifier bash -n"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 23)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
