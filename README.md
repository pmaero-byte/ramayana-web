# Rāmāyaṇa Web — browser action slice

**No Unity.** Open stack: **vanilla ES modules + Three.js (CDN) + CSS**.

## Play (local)

```bash
cd /Users/prabaharan/Other_projects/ramayana-web
python3 -m http.server 8765
# open http://127.0.0.1:8765/
```

## Controls

| Input | Action |
|-------|--------|
| WASD / arrows | Move |
| Shift | Run |
| Space | Jump |
| Right-mouse / Alt+drag | Orbit camera |
| Auto-bow | Fires at rakshasas in 60° cone |
| ☰ (top-right) | Return to kāṇḍa picker |
| ⛶ (top-right) | Browser fullscreen toggle |
| Esc | Unlock cursor (if locked) |

## Deploy

```bash
# Deploy to hertree embed
bash tools/deploy.sh hertree [/path/to/hertree/public/game/ramayana-web]

# Deploy to GitHub Pages (requires origin remote)
bash tools/deploy.sh gh-pages [user/repo]

# Local server
bash tools/deploy.sh static
```

## Current state (Phase 1–2 complete)

- ✅ Kāṇḍa picker from corpus (8 acts)
- ✅ WASD third-person motion + orbit camera
- ✅ Auto-archer + kinematic arrows
- ✅ Arc / Vyuha / Chakra wave formations
- ✅ Rakshasa chase AI + melee hits
- ✅ Dialogue overlay with portraits
- ✅ Cinematic letterbox
- ✅ On-screen touch joystick + jump
- ✅ Web Audio SFX (bow, hit, wave, cue, victory)
- ✅ 5 HP bar + i-frames + death/respawn
- ✅ Death particle burst
- ✅ Per-act arena mood tint (sky/fog)
- ✅ Fullscreen toggle + menu/restart
- ✅ Lanka arena (ring, pillars, plinth, torches)
- ✅ Player HP bar + melee damage application
- ✅ Act-complete flow → auto-return to picker
- ✅ Ad-hoc verify suite

## Coming (Phase 3+)

- [ ] Save/load slots
- [ ] All 8 act-specific arena geometry
- [ ] Mobile touch polish + wide-screen scaling
- [ ] ELGODS portal embed
- [ ] Reusable hermes-verify surface tests

## Stack

- Three.js r160 (esm.sh CDN)
- Corpus from RamayanaPS5 `corpus_data.json` (8 acts, 50 characters)
- Web Audio API (zero deps)
- Portraits: `assets/portraits/` (Rama, Sita, Hanuman, Lakshmana, Ravana)
- No bundler, no Node runtime required

## Related

- Full narrative web game: `jambudweep/ELGODS/portal` (Next.js + Three)
- Unity prototype (frozen for web-first): `Aerospace_projects/RamayanaPS5`
- Plan: `.hermes/plans/2026-07-18_005110-ramayana-web-open-stack.md`
