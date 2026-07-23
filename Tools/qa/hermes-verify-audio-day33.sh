#!/bin/bash
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

FILE="$ROOT/Assets/Scripts/Audio/FallbackRagaSynth.cs"
[ -f "$FILE" ] && p "FallbackRagaSynth.cs exists" || f "FallbackRagaSynth.cs missing"
SRC=$(cat "$FILE" 2>/dev/null || true)
[ -n "$SRC" ] && p "file non-empty" || f "file empty"
echo "$SRC" | grep -q 'class FallbackRagaSynth' && p "class FallbackRagaSynth present" || f "class FallbackRagaSynth missing"
echo "$SRC" | grep -q 'GetOrCreate(' && p "GetOrCreate API" || f "GetOrCreate missing"
echo "$SRC" | grep -q 'AudioClip.Create' && p "AudioClip.Create" || f "AudioClip.Create missing"
echo "$SRC" | grep -q 'class CueHash' && p "CueHash" || f "CueHash missing"
echo "$SRC" | grep -q 'class RagaPreset' && p "RagaPreset" || f "RagaPreset missing"
echo "$SRC" | grep -q 'static FallbackRagaSynth Instance' && p "Instance" || f "Instance missing"
echo "$SRC" | grep -q 'Dictionary<string, AudioClip>' && p "cache" || f "cache missing"
echo "$SRC" | grep -q 'namespace Jambudweep.Ramayana.Audio' && p "namespace" || f "namespace"

RT="$ROOT/Assets/Scripts/Audio/RagaAudioEngine.cs"
[ -f "$RT" ] && p "RagaAudioEngine.cs exists" || f "RagaAudioEngine.cs missing"
RT_SRC=$(cat "$RT" 2>/dev/null || true)
echo "$RT_SRC" | grep -q 'TODO: implement procedural SFX' && f "RagaAudioEngine still has SFX TODO" || p "RagaAudioEngine no open SFX TODO"
echo "$RT_SRC" | grep -q 'case SoundEffectType.Collect:' && p "SFX Collect covered" || f "SFX Collect missing"
echo "$RT_SRC" | grep -q 'case SoundEffectType.VictoryFanfare:' && p "SFX VictoryFanfare covered" || f "SFX VictoryFanfare missing"
echo "$RT_SRC" | grep -q 'case SoundEffectType.BossTelegraph:' && p "SFX BossTelegraph covered" || f "SFX BossTelegraph missing"
echo "$RT_SRC" | grep -q 'case SoundEffectType.Arrow:' && p "SFX Arrow covered" || f "SFX Arrow missing"
echo "$RT_SRC" | grep -q 'AudioClip.Create' && p "AudioClip.Create in SFX" || f "AudioClip.Create missing in SFX"
echo "$RT_SRC" | grep -q 'Mathf.Clamp(' && p "output clamping present" || f "output clamping missing"
echo "$RT_SRC" | grep -q 'FadePair' && p "RagaAudioEngine FadePair present" || f "RagaAudioEngine missing FadePair"
echo "$RT_SRC" | grep -q 'CrossfadeToRaga' && p "CrossfadeToRaga present" || f "CrossfadeToRaga missing"

SRC1=$(cat "$ROOT/Assets/Resources/Ramayana/moments_ayodhya_kanda.json" 2>/dev/null || true)
echo "$SRC1" | grep -q 'vc_ayodhya_' && p "ayodhya corpus voice cue IDs present" || f "ayodhya corpus missing voice cue IDs"
[ -f "$ROOT/Assets/Resources/Ramayana/moments_aranya_kanda.json" ] && p "moments_aranya_kanda.json intact" || f "moments_aranya_kanda.json missing"

[ -f "$ROOT/Assets/Scripts/Combat/VerseCombatTrigger.cs" ] && p "VerseCombatTrigger.cs present" || f "VerseCombatTrigger.cs missing"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 33 audio)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
