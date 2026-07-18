# Rāmāyaṇa Web — browser action slice

**No Unity.** Open stack: **vanilla ES modules + Three.js (CDN) + CSS**.

## Play (local)

```bash
cd /Users/prabaharan/Other_projects/ramayana-web
python3 -m http.server 8765
# open http://127.0.0.1:8765/
```

## Play (embedded)

```bash
http://127.0.0.1:8765/embed.html          # iframe shell, autoplay + fullscreen
http://127.0.0.1:8765/embed.html?locale=sa  # start in Sanskṛit
```

The embed shell listens for `postMessage({ type: 'rama-set-locale', locale: 'sa' | 'en' })`
to swap languages at runtime from a parent frame (Hertree card, etc.).

## Controls

| Input | Action |
|-------|--------|
| WASD / arrows | Move |
| Shift | Run |
| Space | Jump |
| Right-mouse / Alt+drag | Orbit camera |
| Auto-bow | Fires at rakshasas in 60° cone |
| अ/En (top-right) | Toggle Sanskṛit / English |
| 📂 / 💾 (top-right) | Save slots / autosave |
| ☰ (top-right) | Return to kāṇḍa picker |
| ⛶ (top-right) | Browser fullscreen toggle |
| F / M / Esc | Fullscreen / menu / close slots |

## Localization

- `js/core/i18n.js` — `t(key, vars)`, `translateCorpus(text, 'char'|'act')`
- `js/core/locales/en.js` — English defaults
- `js/core/locales/sa.js` — Sanskṛit (देवनागरी) with corpus name translations
- Persisted in `localStorage.ramayana_web_locale`
- Toggle via अ/En button or `window.RamaWeb.setLocale('sa')`

## Verify (ad-hoc only — NOT suite-green)

```bash
node .hermes/verify-boot.js   # 28-file boot surface check
node .hermes/verify-test.mjs  # 13 ESM unit smoke for formation + i18n
```

Both expect exit 0.

## Stack

- Three.js r160 (esm.sh CDN)
- Corpus from RamayanaPS5 `corpus_data.json` (8 acts, 50 characters)
- Web Audio API (zero deps) — bow / hit / wave / cue / victory + tanpura drone
- localStorage for 5 save slots + autosave + high-score + locale
- Portraits: `assets/portraits/` (Rama, Sita, Hanuman, Lakshmana, Ravana)
- No bundler, no Node runtime required at runtime

## Features shipped (Phase 1–2 complete)

### Combat
- ✅ WASD third-person motion + cinematic orbit camera
- ✅ Auto-archer + kinematic arrows with ember trail
- ✅ Rakshasa chase AI + melee hits
- ✅ Arc / Vyuha / Chakra wave formations
- ✅ Hit flash + bob + ground shadow
- ✅ Death particle burst

### Story
- ✅ Kāṇḍa picker (8 acts from corpus)
- ✅ StoryMomentPlayer walking `objectives[]`
- ✅ Dialogue overlay with portraits
- ✅ Sanskṛit diacritics on titles, cues, completedLines
- ✅ Per-act arena geometry (sun + fog + ring + pillars + plinth + torches)

### UX
- ✅ Cinematic letterbox bars
- ✅ 5 HP bar with i-frames + death/respawn loop
- ✅ 5 save slots + autosave + continue button
- ✅ Character-select grid (4 playable avatars)
- ✅ Sanskṛit ↔ English toggle
- ✅ On-screen touch joystick + jump (mobile + iPad)
- ✅ High-score tracking
- ✅ Fullscreen + safe-area insets + landscape media
- ✅ iframe embed shell + postMessage locale handshake

### Architecture
- ✅ Pure ES modules — no bundler, no Unity, no Node runtime
- ✅ KISS/DRY throughout — sealed-style factory pattern, EnsureCreated spirit
- ✅ Ad-hoc verification surface (.hermes/verify-boot.js, verify-test.mjs)
- ✅ deploy.sh targets: gh-pages, hertree, local

## Deploy

```bash
bash tools/deploy.sh hertree [/path/to/hertree/public/game/ramayana-web]
bash tools/deploy.sh gh-pages [user/repo]
bash tools/deploy.sh static  # local python3 http.server :8765
```

## Related

- Full narrative web game: `jambudweep/ELGODS/portal` (Next.js + Three)
- Unity prototype (frozen for web-first): `Aerospace_projects/RamayanaPS5`
- Plan: `.hermes/plans/2026-07-18_005110-ramayana-web-open-stack.md`
