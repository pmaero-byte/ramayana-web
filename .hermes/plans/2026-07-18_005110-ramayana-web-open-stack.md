# Rāmāyaṇa Web Game — Implementation Plan

> **For Hermes:** Implement task-by-task. Web-first, **no Unity**. Open stacks only.

**Goal:** Ship a browser-playable third-person Rāmāyaṇa action/story slice (Yuddha first) with zero Unity dependency, using open MIT/BSD stacks.

**Architecture:** Static vanilla ES modules + Three.js (CDN). Single `index.html`, no bundler required for v1. Port RamayanaPS5 Day 1–10 *systems* (story walker, combat, formations, portraits, HUD) into JS. Corpus = `data/corpus_data.json` (50 characters, 8 acts).

**Tech Stack (all open / free):**
| Layer | Stack | Why |
|-------|--------|-----|
| 3D | [Three.js r160+](https://threejs.org) via esm.sh CDN | Same family as Traders / ELGODS; MIT |
| App shell | Vanilla JS ES modules + CSS | No Node required to play; `python3 -m http.server` |
| Audio (later) | Web Audio API | No assets required for procedural tanpura |
| Input | Keyboard + pointer + optional Gamepad API | Mac laptop first |
| Data | JSON corpus (already Valmiki-shaped) | Port from RamayanaPS5 |
| Host | Static files (GitHub Pages / any CDN) | Zero backend |
| Optional later | Vite build, Capacitor iOS wrap | Only after browser slice is fun |

**Relationship to existing projects:**
| Project | Role |
|---------|------|
| **This repo `Other_projects/ramayana-web`** | **Canonical web-first action slice** (new) |
| `Aerospace_projects/RamayanaPS5` | Unity experiment — freeze for web-first; optional later export of art |
| `jambudweep/ELGODS/portal` | Full narrative Three.js + Next.js Ramayana — keep for encyclopedia/story depth; **do not block** this lighter action game |
| `traders-of-jambudweep` | Pattern reference (vanilla + Three + hermes-verify) |

**Non-goals (v1):** Photoreal GTA, multiplayer, Unity bridge, paid SDKs, Node build step.

---

## Product vision (honest)

Browser third-person **kāṇḍa combat + dialogue** loop:
1. Title → pick kāṇḍa (Yuddha default)
2. Walk (WASD) + orbit camera (RMB / Alt-drag)
3. Auto-archer + visible arrows vs rakshasa waves (Arc → Vyuha → Chakra)
4. Cinematic letterbox + portrait dialogue from corpus cues
5. Wave clear → next objective → act complete

GTA *feel* here means: **camera weight, letterbox, combat pressure, face plates** — not Rockstar open world.

---

## Phase map

| Phase | Deliverable | Exit criteria |
|-------|-------------|-----------------|
| **0** | Repo scaffold + plan + corpus | `python3 -m http.server` loads black canvas + title |
| **1** | Playable Yuddha vertical slice | Walk, shoot, 3 waves, dialogue portraits |
| **2** | Kāṇḍa menu + all 8 acts load | Act picker from corpus |
| **3** | Audio + juice | Web Audio hit/cue; camera kick; particles |
| **4** | Polish + deploy | Mobile touch, GitHub Pages, hermes-verify suite |
| **5** | Optional ELGODS embed | iframe or shared corpus only |

---

## File layout (target)

```
Other_projects/ramayana-web/
  index.html
  README.md
  css/game.css
  data/corpus_data.json
  assets/portraits/{rama,sita,hanuman,lakshmana,ravana}.png
  js/
    main.js                 # boot
    core/state.js           # game state
    core/loop.js            # rAF
    core/input.js           # WASD + mouse orbit
    world/scene.js          # Three scene, ground, fog, light
    world/player.js         # capsule + CharacterController-ish
    world/camera.js         # cinematic third-person
    combat/rakshasa.js
    combat/wave.js
    combat/arrow.js
    combat/formation.js     # Arc / Chakra / Vyuha
    story/moments.js        # act/objective walker
    ui/hud.js
    ui/dialogue.js
    ui/letterbox.js
    ui/title.js
  .hermes/verify-boot.js
```

---

### Task 0: Scaffold + README

**Objective:** Empty playable shell loads in browser.

**Files:**
- Create: `index.html`, `css/game.css`, `js/main.js`, `README.md`

**Verify:** `python3 -m http.server 8765` → open `/` → no console errors; title “RĀMĀYAṆA” visible.

---

### Task 1: Three scene + player + camera

**Objective:** WASD walk on dusk Lanka ground with orbit camera.

**Files:** `js/world/scene.js`, `player.js`, `camera.js`, `core/input.js`, `core/loop.js`

**Controls:** WASD move, Shift run, Space jump, RMB/Alt+drag orbit.

**Verify:** Capsule moves; camera follows; fog + directional light present.

---

### Task 2: Combat trio + formations

**Objective:** 3 waves of rakshasas; auto-fire arrows; Arc→Vyuha→Chakra.

**Files:** `js/combat/*`

**Verify:** Wave 1 arc spawn, wave 2 rows, wave 3 ring; kills advance waves.

---

### Task 3: Story + portraits + letterbox

**Objective:** Load `yuddhakanda-war`; show cue + portrait; wave clear → complete objective.

**Files:** `js/story/moments.js`, `ui/dialogue.js`, `ui/letterbox.js`, `ui/hud.js`

**Verify:** Portrait of Rama (or playerRole) appears; letterbox bars; day/streak HUD ticks.

---

### Task 4: Title / kāṇḍa picker

**Objective:** Start screen lists 8 acts from corpus; Accept starts act.

**Files:** `js/ui/title.js`

**Verify:** Click Ayodhya loads that act’s first objective.

---

### Task 5: Ad-hoc verifiers + smoke

**Objective:** `node --check` all JS; hermes-verify script counts surfaces.

**Files:** `.hermes/verify-boot.js`

**Verify:** Script exit 0; label ad-hoc only.

---

### Task 6 (later): Audio juice

Web Audio bow pluck + war drum on wave start; optional Howler only if needed (prefer pure Web Audio — zero deps).

---

### Task 7 (later): Deploy

GitHub Pages or Hertree embed under `/game/ramayana-web/`. Cache-bust `?v=` on script tags.

---

## Port map (Unity → Web)

| Unity (RamayanaPS5) | Web module |
|---------------------|------------|
| PlayerSceneBootstrap | `main.js` boot |
| ThirdPersonMotionController | `world/player.js` |
| CinematicThirdPersonCamera | `world/camera.js` |
| MacDesktopInput | `core/input.js` |
| ArcherAutoFire + ArrowProjectile | `combat/arrow.js` + player fire |
| WaveController + FormationStrategy | `combat/wave.js` + `formation.js` |
| RakshasaTarget | `combat/rakshasa.js` |
| StoryMomentPlayer | `story/moments.js` |
| DialogueOverlay + PortraitResolver | `ui/dialogue.js` |
| CinematicLetterbox | `ui/letterbox.js` |
| HudOrchestrator | `ui/hud.js` |
| MainMenuScreenController | `ui/title.js` |
| corpus_data.json | `data/corpus_data.json` |

---

## Risks

1. **Two Ramayana web games (ELGODS + this)** — mitigate: this is **action slice**; ELGODS remains **narrative encyclopedia game**. Link in README.
2. **CDN offline** — pin three version; optional vendor copy later.
3. **CORS / file://** — always serve via http.server.
4. **Scope creep to GTA** — stick to Yuddha vertical slice until fun.

---

## Success metrics (Phase 1)

- [ ] Playable in Chrome/Safari on Mac laptop
- [ ] Zero Unity / zero paid engines
- [ ] 1 full Yuddha combat+story loop ≤ 3 minutes
- [ ] Corpus-driven (not hard-coded plot)
- [ ] Ad-hoc verify green

---

## Immediate next (after plan)

Implement Tasks 0–3 in this session → bootable Yuddha slice.
