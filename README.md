# RamayanaPS5

Canonical Ramayana game. Unity 6000.5.4f1 + URP 17.

## State (2026-07-22)

- 58 C# files, 11 Unity scenes, 6 JSON corpora
- 26 characters, 11 voice registers, 36 Bala Kanda moments
- Verse orchestration + combat triggers + HUD stack + UI framework
- Ad-hoc per-day verifiers under `Tools/qa/` (Days 5–20)
- _archive/ramayana-web/ — deprecat

Day 18 iOS Simulator build scaffold shipped, but real xcodebuild is blocked
because Unity iOS Build Support is not installed in this 6000.5.4f1 instance
(`PlaybackEngines/iOSSupport` missing → `ld: framework 'UnityRuntime' not found`).
Install iOS Build Support via Unity Hub, then re-run `bash Tools/qa/build-ios-sim.sh`.

## Docs

- `Documentation/R83_CONSOLIDATION_AUDIT.md` — ELGODS vs RamayanaPS5 parity
- `Documentation/DAY18_IOS_BLOCKER.md` — host-level iOS runtime blocker

## How to validate

```bash
# iOS build scaffold
bash Tools/qa/build-ios-sim.sh  # blocked until iOS module installed

# Day 19 archive verifier
bash Tools/qa/hermes-verify-ramayana-archive-day19.sh
```

## Archive

`_archive/ramayana-web/` is a deprecated vanilla JS/Three.js predecessor.
No development. Browse with `python3 -m http.server 8080` from that folder.
