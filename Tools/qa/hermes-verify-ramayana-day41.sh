#!/bin/bash
set -u
ROOT="/Users/prabaharan/Aerospace_projects/RamayanaPS5"
cd "$ROOT"
PASS=0; FAIL=0; RESULTS=()
pass()  { RESULTS+=("PASS: $*"); PASS=$((PASS+1)); }
fails() { RESULTS+=("FAIL: $*"); FAIL=$((FAIL+1)); }

python3 - <<'PY'
import json, os
from collections import Counter
root = "/Users/prabaharan/Aerospace_projects/RamayanaPS5"
kandas = {
    "bala-kanda": "Assets/Resources/Ramayana/moments_bala_kanda.json",
    "ayodhya-kanda": "Assets/Resources/Ramayana/moments_ayodhya_kanda.json",
    "aranya-kanda": "Assets/Resources/Ramayana/moments_aranya_kanda.json",
    "kishkindha-kanda": "Assets/Resources/Ramayana/moments_kishkindha_kanda.json",
    "sundara-kanda": "Assets/Resources/Ramayana/moments_sundara_kanda.json",
    "yuddha-kanda": "Assets/Resources/Ramayana/moments_yuddha_kanda.json",
    "uttara-kanda": "Assets/Resources/Ramayana/moments_uttara_kanda.json",
    "return-kanda": "Assets/Resources/Ramayana/moments_return_kanda.json",
}
for kanda, path in kandas.items():
    p = os.path.join(root, path)
    if not os.path.isfile(p):
        print(f"FAIL: {kanda} missing {path}"); continue
    data = json.load(open(p))
    mom = data.get("moments") or []
    print(f"PASS: {kanda} moments={len(mom)}")
    bad = [m for m in mom if not m.get("momentId") or not m.get("voiceCueId")]
    if bad:
        print(f"FAIL: {kanda} has {len(bad)} moments missing momentId/voiceCueId")
    else:
        print(f"PASS: {kanda} all moments have momentId+voiceCueId")
    dup = [x for x, c in Counter([m.get("voiceCueId") for m in mom]).items() if x and c > 1]
    if dup:
        print(f"FAIL: {kanda} duplicate voiceCueIds: {dup[:5]}")
    else:
        print(f"PASS: {kanda} voiceCueIds unique")
PY

python3 - <<'PY'
import json
from pathlib import Path
v = json.loads(Path("Assets/Resources/Ramayana/voices.json").read_text())
cues = v.get("voiceCues", [])
by = {c.get("cueId"): c for c in cues if c.get("cueId")}
print(f"INFO: voices cues={len(cues)} unique={len(by)}")
import os
root = "/Users/prabaharan/Aerospace_projects/RamayanaPS5"
kandas = ["bala-kanda","ayodhya-kanda","aranya-kanda","kishkindha-kanda","sundara-kanda","yuddha-kanda","uttara-kanda","return-kanda"]
missing = []
for kanda in kandas:
    p = f"Assets/Resources/Ramayana/moments_{kanda.replace('-','_')}.json"
    data = json.load(open(p))
    for m in data.get("moments", []):
        cid = m.get("voiceCueId")
        if cid and cid not in by:
            missing.append((kanda, cid))
if missing:
    print(f"FAIL: {len(missing)} moments reference missing voice cueIds")
    for k, cid in missing[:10]:
        print(f"  {k}: {cid}")
else:
    print("PASS: all referenced voiceCueIds exist in voices.json")
PY

grep -q "ResolveMomentsResourceForKanda" Assets/Scripts/Verse/VerseOrchestrator.cs && pass "VerseOrchestrator mentions ResolveMomentsResourceForKanda" || fails "VerseOrchestrator missing ResolveMomentsResourceForKanda"
grep -q "return-kanda" Assets/Scripts/Gameplay/KandaTree.cs && pass "KandaTree has return-kanda" || fails "KandaTree missing return-kanda"
grep -q "uttara-earth-return" Assets/Scripts/UI/KandaLaunchBridge.cs && pass "ActToKandaMap contains uttara-earth-return" || fails "ActToKanaMap missing uttara-earth-return"

printf '%s\n' "${RESULTS[@]}"
echo "---"
echo "$PASS passed, $FAIL failed"
exit $FAIL
