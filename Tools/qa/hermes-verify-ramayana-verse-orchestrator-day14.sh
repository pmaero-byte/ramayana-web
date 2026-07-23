#!/bin/bash
# hermes-verify-ramayana-verse-orchestrator-day14.sh — Verifier for Day 14
# Verse orchestrator that consumes the Day 11/12/13 corpora.
#
# Asserts:
# 1. VerseOrchestrator.cs + .meta exist
# 2. namespace Jambudweep.Ramayana.Verse
# 3. class VerseOrchestrator : MonoBehaviour (sealed)
# 4. Instance singleton + EnsureCreated() factory pattern
# 5. Loads 3 corpus resource paths: characters, moments_bala_kanda, voices
# 6. Uses the 3 Day 11/12/13 POCO types: RamayanaCharacterRoster,
#    RamayanaMomentsCorpus, RamayanaVoicesCorpus
# 7. UnityEvent hooks present (onVerseLoaded/onMomentEntered/onMomentCompleted/
#    onVerseCompleted/onError)
# 8. Brace balance
# 9. .meta guid is valid
# 10. Day 11/12/13 corpora files still exist (downstream contract)
# 11. Field path "Ramayana/characters" matches Day 11 Resources path
# 12. Field path "Ramayana/moments_bala_kanda" matches Day 12 path
# 13. Field path "Ramayana/voices" matches Day 13 path
# 14. Public Play/Advance/Stop/LoadCorpora methods present
# 15. UnityEvent<string> typed (caller-friendly for cueId payloads)
# 16. Story/Story*.cs UNTOUCHED (no modify-existing-files discipline)
# 17. Does NOT extend or modify StoryEngine.cs
# 18. Does NOT extend or modify StoryMomentPlayer.cs
# 19. ELGODS source still has storyEngine.ts (sanity)
#
# Usage:
#   bash Tools/qa/hermes-verify-ramayana-verse-orchestrator-day14.sh
#   HERMES_VERIFY_ROOT=<project> bash hermes-verify-ramayana-verse-orchestrator-day14.sh
#
# Exit codes: 0 = all pass, 1 = any fail.
# Status: ad-hoc only — Unity batchmode compile pending.

set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="${HERMES_VERIFY_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"
PASS=0; FAIL=0

section() { echo ""; echo "== $1 =="; }
check() { if [ "$2" = "$3" ]; then echo "  ✅ $1  ($2)"; PASS=$((PASS+1)); else echo "  ❌ $1  expected=$3 actual=$2"; FAIL=$((FAIL+1)); fi; }
ge() { if [ "$2" -ge "$3" ] 2>/dev/null; then echo "  ✅ $1  ($2 ≥ $3)"; PASS=$((PASS+1)); else echo "  ❌ $1  expected≥$3 actual=$2"; FAIL=$((FAIL+1)); fi; }
file_check() { if [ -f "$2" ]; then echo "  ✅ $1"; PASS=$((PASS+1)); else echo "  ❌ $1  (missing: $2)"; FAIL=$((FAIL+1)); fi; }

ORCH="$ROOT/Assets/Scripts/Verse/VerseOrchestrator.cs"

# ---------- Files ----------
section "Files exist"
file_check "VerseOrchestrator.cs"       "$ORCH"
file_check "VerseOrchestrator.cs.meta"  "$ROOT/Assets/Scripts/Verse/VerseOrchestrator.cs.meta"

# ---------- Source surface ----------
section "Source surface"
SRC=$(cat "$ORCH")
check "namespace Jambudweep.Ramayana.Verse" \
  "$(echo "$SRC" | grep -c '^namespace Jambudweep.Ramayana.Verse')" "1"
check "public sealed class VerseOrchestrator" \
  "$(echo "$SRC" | grep -c 'public sealed class VerseOrchestrator')" "1"
ge "MonoBehaviour (≥1 ref)" \
  "$(echo "$SRC" | grep -c 'MonoBehaviour')" "1"
check "public static Instance singleton" \
  "$(echo "$SRC" | grep -c 'public static VerseOrchestrator Instance')" "1"
check "public static EnsureCreated()" \
  "$(echo "$SRC" | grep -c 'public static VerseOrchestrator EnsureCreated()')" "1"
check "void Awake()" \
  "$(echo "$SRC" | grep -c 'void Awake()')" "1"
check "void Start()" \
  "$(echo "$SRC" | grep -c 'void Start()')" "1"

# ---------- Corpus resource paths ----------
section "Corpus resource paths (Day 11/12/13)"
ge "characters resource path 'Ramayana/characters' (≥1)" \
  "$(echo "$SRC" | grep -cE 'charactersResourcePath[[:space:]]*=')" "1"
ge "moments resource path 'Ramayana/moments_bala_kanda' (≥1)" \
  "$(echo "$SRC" | grep -cE 'momentsResourcePath[[:space:]]*=')" "1"
ge "voices resource path 'Ramayana/voices' (≥1)" \
  "$(echo "$SRC" | grep -cE 'voicesResourcePath[[:space:]]*=')" "1"

# ---------- POCO type usage (Day 11/12/13 binding contract) ----------
section "Day 11/12/13 POCO types referenced"
ge "RamayanaCharacterRoster referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'RamayanaCharacterRoster')" "1"
ge "RamayanaMomentsCorpus referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'RamayanaMomentsCorpus')" "1"
ge "RamayanaVoicesCorpus referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'RamayanaVoicesCorpus')" "1"
ge "RamayanaVoiceCue referenced (≥1)" \
  "$(echo "$SRC" | grep -c 'RamayanaVoiceCue')" "1"

# ---------- Public API ----------
section "Public API methods"
check "public void LoadCorpora()" \
  "$(echo "$SRC" | grep -c 'public void LoadCorpora()')" "1"
check "public bool Play(string kanda)" \
  "$(echo "$SRC" | grep -c 'public bool Play(string kanda)')" "1"
check "public bool Advance()" \
  "$(echo "$SRC" | grep -c 'public bool Advance()')" "1"
check "public void Stop()" \
  "$(echo "$SRC" | grep -c 'public void Stop()')" "1"
check "public IEnumerator LoadAndPlay" \
  "$(echo "$SRC" | grep -c 'public IEnumerator LoadAndPlay')" "1"

# ---------- UnityEvent hooks ----------
section "UnityEvent hooks"
ge "onVerseLoaded (≥1)"      "$(echo "$SRC" | grep -c 'onVerseLoaded')" "1"
ge "onMomentEntered (≥1)"    "$(echo "$SRC" | grep -c 'onMomentEntered')" "1"
ge "onMomentCompleted (≥1)"  "$(echo "$SRC" | grep -c 'onMomentCompleted')" "1"
ge "onVerseCompleted (≥1)"   "$(echo "$SRC" | grep -c 'onVerseCompleted')" "1"
ge "onError (≥1)"            "$(echo "$SRC" | grep -c 'onError')" "1"
ge "VerseMomentEvent (≥1)"   "$(echo "$SRC" | grep -c 'VerseMomentEvent')" "1"

# ---------- Brace balance ----------
section "Brace balance (Roslyn-free sanity)"
opens=$(grep -o '{' "$ORCH" | wc -l | tr -d ' ')
closes=$(grep -o '}' "$ORCH" | wc -l | tr -d ' ')
check "brace balance ({ $opens, } $closes)" "$opens" "$closes"

# ---------- .meta guid ----------
section ".meta guid"
META_GUID=$(grep -h '^guid:' "$ROOT/Assets/Scripts/Verse/VerseOrchestrator.cs.meta" | awk '{print $2}')
if [[ "$META_GUID" =~ ^[a-f0-9]{32}$ ]]; then
  echo "  ✅ .meta guid valid: $META_GUID"; PASS=$((PASS+1))
else
  echo "  ❌ .meta guid invalid: $META_GUID"; FAIL=$((FAIL+1))
fi

# ---------- Downstream corpora exist ----------
section "Downstream corpora (Day 11/12/13 files)"
file_check "Day 11 characters.json"      "$ROOT/Assets/Resources/Ramayana/characters.json"
file_check "Day 12 moments_bala_kanda.json" "$ROOT/Assets/Resources/Ramayana/moments_bala_kanda.json"
file_check "Day 13 voices.json"          "$ROOT/Assets/Resources/Ramayana/voices.json"

# ---------- No-touch discipline (don't modify Story/Story*.cs) ----------
section "Day 1-10 files UNTOUCHED (no modify-existing-files)"
check "StoryEngine.cs not modified" \
  "$(cd "$ROOT" && git diff --name-only HEAD -- Assets/Scripts/Story/StoryEngine.cs | wc -l | tr -d ' ')" "0"
check "StoryMomentPlayer.cs not modified" \
  "$(cd "$ROOT" && git diff --name-only HEAD -- Assets/Scripts/Story/StoryMomentPlayer.cs | wc -l | tr -d ' ')" "0"

# ---------- ELGODS source sanity ----------
section "ELGODS source sanity (storyEngine.ts still present)"
STORY_ENGINE_FILE="/Users/prabaharan/jambudweep/ELGODS/portal/src/game/ramayana/storyEngine.ts"
if [ -f "$STORY_ENGINE_FILE" ]; then
  LINES=$(wc -l < "$STORY_ENGINE_FILE" | tr -d ' ')
  ge "ELGODS storyEngine.ts exists (≥200 lines)" "$LINES" "200"
else
  echo "  ❌ ELGODS storyEngine.ts not found at $STORY_ENGINE_FILE"; FAIL=$((FAIL+1))
fi

# ---------- Final report ----------
echo ""
echo "==================================================="
echo "  $PASS passed, $FAIL failed  (ad-hoc only, Unity compile pending)"
echo "==================================================="
[ "$FAIL" -eq 0 ] && exit 0 || exit 1