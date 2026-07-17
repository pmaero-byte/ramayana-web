# Rāmāyaṇa Web — browser action slice

**No Unity.** Open stack: **vanilla ES modules + Three.js (CDN) + CSS**.

## Play

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
| Auto | Bow fires at rakshasas in cone |

## Stack

- Three.js r160 (esm.sh)
- Corpus from RamayanaPS5 `corpus_data.json` (8 acts, 50 characters)
- Portraits under `assets/portraits/`

## Related

- Full narrative web game: `jambudweep/ELGODS/portal` (Next.js + Three)
- Unity prototype (frozen for web-first): `Aerospace_projects/RamayanaPS5`
- Plan: `.hermes/plans/2026-07-18_005110-ramayana-web-open-stack.md`
