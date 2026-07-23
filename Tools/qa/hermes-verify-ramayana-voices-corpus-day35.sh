#!/bin/bash
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
PASS=0; FAIL=0
p(){ echo "  PASS: $*"; PASS=$((PASS+1)); }
f(){ echo "  FAIL: $*"; FAIL=$((FAIL+1)); }

VC="$ROOT/Assets/Resources/Ramayana/voices.json"
[ -f "$VC" ] && p "voices.json exists" || f "voices.json missing"
SRC=$(cat "$VC" 2>/dev/null || true)
[ -n "$SRC" ] && p "file non-empty" || f "file empty"
python3 -c "import json; d=json.loads(open('$VC').read()); print(len(d['voiceCues']))" 2>/dev/null >/tmp/voices_count.txt
TOTAL=$(cat /tmp/voices_count.txt)
[ "$TOTAL" -ge 108 ] && p "voiceCues >= 108 ($TOTAL)" || f "voiceCues < 108"
KANDAS=$(python3 -c "
import json
d=json.loads(open('$VC').read())
ks=sorted({c['kanda'] for c in d['voiceCues']})
print(' '.join(ks))
")
echo "$KANDAS" | tr ' ' '\n' | wc -l | tr -d ' ' | grep -q '^8$' && p "all 8 kandas present" || f "missing kandas"
for kanda in bala-kanda ayodhya-kanda aranya-kanda kishkindha-kanda sundara-kanda yuddha-kanda uttara-kanda return-kanda; do
  echo "$SRC" | grep -q "\"kanda\": \"$kanda\"" && p "$kanda cues present" || f "$kanda cues missing"
done
python3 -c "
import json,glob,os
root='$ROOT/Assets/Resources/Ramayana'
d=json.loads(open(os.path.join(root,'voices.json')).read())
vc={c['cueId'] for c in d['voiceCues']}
miss=[]
for fp in sorted(glob.glob(root+'/moments_*.json')):
    m=json.loads(open(fp).read())
    for x in m.get('moments',[]):
        if x.get('voiceCueId') not in vc:
            miss.append(x['voiceCueId'])
print('MISSING' if miss else 'OK')
" 2>/dev/null | grep -q OK && p "every momentId has a voice cue" || f "some moments lack voice cues"

# Verifier regression
bash -n "$ROOT/Tools/qa/hermes-verify-audio-day33.sh" 2>/dev/null && p "Day 33 audio verifier bash -n" || f "Day 33 audio verifier bash -n"
bash -n "$ROOT/Tools/qa/hermes-verify-ramayana-scene-transition-day34.sh" 2>/dev/null && p "Day 34 scene verifier bash -n" || f "Day 34 scene verifier bash -n"

echo ""
echo "  $PASS passed, $FAIL failed  (Day 35 voices corpus)"
[ "$FAIL" -eq 0 ] && exit 0 || exit 1
