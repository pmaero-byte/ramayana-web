# ramayana-web — ARCHIVED

**Status:** DEPRECATED — archived 2026-07-22  
**Moved to:** `RamayanaPS5/_archive/ramayana-web/`  
**Canonical project:** `RamayanaPS5` (Unity 6 + URP)  
**Source repo:** `/Users/prabaharan/Other_projects/ramayana-web/`

## Why archived

This vanilla JS/Three.js prototype was the predecessor to the canonical Unity
consolidation. It shipped 26 characters, ~40 Bala Kanda story moments, and
the Kalari combat loop. Development moved to RamayanaPS5 so this codebase
is now read-only.

Day 18 note: RamayanaPS5 currently ships an iOS Simulator build scaffold, but
the real iOS linker gate is blocked because this host’s Unity 6000.5.4f1
installation is missing iOS Build Support (PlaybackEngines/iOSSupport).
The Day 18 scaffolding is complete; install iOS Build Support via Unity Hub
to enable a real xcodebuild circuit.

## How to browse

Open `index.html` from this folder in any browser, or serve locally:

```bash
cd /Users/prabaharan/Aerospace_projects/RamayanaPS5/_archive/ramayana-web
python3 -m http.server 8080
# then open http://localhost:8080
```

## Files

- `index.html`, `css/`, `assets/` — browser shell
- `js/core/` — i18n, state, input, audio, touch
- `js/story/moments.json` — Day 12 Bala Kanda corpus
- `js/combat/` — formation, wave, rakshasa, cover systems
- `js/world/` — scene/camera/player
- `js/ui/dialogue.js` — dialogue system

## Commit history

Last live development commit in `Other_projects/ramayana-web` is preserved;
this archive was copied 2026-07-22 during RamayanaPS5 consolidation Day 19.
