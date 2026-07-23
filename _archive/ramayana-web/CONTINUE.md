# Continuation Prompt — Ramayana Web

Copy-paste this prompt into a fresh Hermes session to continue development.

---

## Working Environment

- **Repo:** `https://github.com/pmaero-byte/ramayana-web`
- **Local path:** `/Users/prabaharan/Other_projects/ramayana-web`
- **Branch:** `main` (already pushed to origin)
- **Runtime:** macOS, browser-first, vanilla ES modules + Three.js (CDN), no Unity, no build step
- **Server (for testing):** `cd /Users/prabaharan/Other_projects/ramayana-web && python3 -m http.server 8765` → `http://127.0.0.1:8765/`
- **Cache-busting:** index.html uses `?v=N` on `js/main.js`; bump on every commit
- **User profile:** Mac, plays in browser, prefers GTA-quality cinematic action over narrative walls
- **Token mode:** user has explicitly authorized heavy-token work — do NOT ask "should I continue?"; just keep shipping

## Standing Goal

**Make the Ramayana game quality equivalent with GTA games.** The slice is now feature-complete at prototype quality (~14 commits this session). Real GTA-quality requires months of asset work (real 3D character meshes, full audio library, missions, open world, vehicles). Work incrementally within session boundaries; ship one feature at a time, verify, commit, push.

## Current State at Tip `a5f536f` (what's already done)

**HUD/UI:**
- Circular minimap/radar (top-right, gold border, compass labels, player at center, enemies as red dots, boss as orange)
- Mission-complete banner (centered, gold pulse glow, fades after 4s)
- HP bar, wave counter, kill counter, streak indicator (x3+)
- Dialogue overlay with portrait + speaker + sanskrit tagline
- Death overlay with sanskrit tagline + Retry/Return buttons
- Cinematic letterbox bars, touch joystick, save/load slots, i18n EN/SA

**Visual:**
- Procedural avatars: Rama has crown + hair cap + legs + boots + bow + quiver; rakshasa has horns + fangs + glowing red emissive eyes + claws + crest
- Boss tier (hp≥6): 1.45x scale, dark crimson body, red aura points (32 of them)
- Per-wave size variation: wave 1 grunts at 0.78x, wave 2 warriors at 1.0x, wave 3 boss at 1.45x
- Floating HP bars above each rakshasa (1.2x0.16 background, 1.12x0.1 fill, billboarded counter-rotation, color shifts red→orange as HP drops, hidden on death)
- Golden shockwave ring on every hit (RingGeometry, scales 1×→5× over 0.5s)
- Running bob (amplitude 0.12), forward lean (-0.08 rad), side-sway
- Sky dome (80-radius, gradient horizon→zenith)
- Drifting dust motes (80 Points, random per-frame movement)
- Cinematic camera (FOV punch, slow-mo capped at 0.5, screen shake on hits/kills)

**Audio (procedural, zero deps):**
- Tanpura drone (5 oscillators, LFO-modulated)
- sfxBow, sfxHit, sfxWave, sfxCue, sfxWin
- sfxArrow (whoosh), sfxDeath (thud + sub-bass), sfxBossRoar (3-frequency growl), sfxLevelUp (ascending arpeggio)
- Wired: boss roar on wave 3 spawn, death sfx on player death, level-up chime at 3-streak milestone

**Narrative:**
- All 8 acts have WAVE_QUOTES (bala-birth, ayodhya-dharma, panchavati-golden-deer, kishkindha-alliance, sundarakanda-leap, yuddhakanda-war, return-ayodhya, uttara-earth-return) — each with 3 verses + speaker
- Per-act wave-clear dialogue interlude (2.4s on dialogue overlay)
- Valmiki dialogue on death overlay ("Even the greatest heroes rise again")
- Story progression state (objectives, lessons) wired into HUD

**Combat system:**
- Cover-aware combat (8 act palettes, 4 layouts, 3 prop types, blocksLine() ray-shape occlusion with per-kind radius)
- Arrow physics (kinematic projectiles, proximity hit detection, no Physics.* APIs)
- Auto-fire archer with accuracy penalty (60% moving, 90% still)
- Bow quiver visual on player
- Death fall animation (75° rotation + sink + fade, called every frame even when dead)
- Squash-and-stretch body language on enemies

**System:**
- Save/load (5 slots, localStorage, autosave every 30s)
- i18n (EN/SA)
- Boot watchdog (3s fallback)
- ESM unit smoke tests (`js/main.js` + `js/core/state.js`)
- Ad-hoc verifiers per commit (`hermes-verify-ramayana-web-*.sh` in temp dirs)

## Key Files & Module Layout

```
ramayana-web/
├── index.html                  # Entry point. Cache-bust with ?v=N
├── css/game.css                # All UI styles (HP bar, dialogue overlay, death overlay, minimap, mission-complete)
├── data/corpus_data.json       # 8 acts × 3 verses + lessons + objectives (used as source of truth for narrative)
├── js/main.js                  # Frame loop, wiring, state, WAVE_QUOTES, minimap draw, mission-complete banner
├── js/core/
│   ├── audio.js                # Procedural Web Audio SFX + tanpura drone
│   ├── input.js                # WASD + RMB orbit + shift-run
│   ├── touch.js                # Mobile joystick
│   ├── state.js                # Save/load slots
│   └── i18n.js                 # EN/SA translations
├── js/world/
│   ├── scene.js                # Three.js world, pillars, plinth, sky dome, dust motes, updateAtmosphere
│   ├── player.js               # Avatar (crown, hair, legs, bow, quiver), fall animation
│   └── camera.js               # Cinematic rig (FOV punch, slow-mo floor 0.5, screen shake)
├── js/combat/
│   ├── wave.js                 # Wave controller, archer, spawnSparks/spawnRing/spawnParticles
│   ├── rakshasa.js             # Procedural enemy mesh (horns, eyes, fangs, claws, crest), damage(), HP bar
│   └── cover.js                # Procedural cover props + blocksLine()
├── js/ui/
│   └── dialogue.js             # showDialogue/hideDialogue
├── .hermes/
│   ├── verify-boot.cjs         # Boot surface smoke test (28 checks)
│   └── verify-test.mjs         # ESM unit smoke (13 checks)
└── CONTINUE.md                 # This file
```

## Hard Rules (do NOT violate)

1. **No Unity, no build step.** Pure vanilla ES modules + Three.js CDN. No bundler, no npm.
2. **No Physics.* APIs.** Use proximity-based hit detection + kinematic projectiles.
3. **Mac-first input.** WASD + Alt orbit, RMB orbit, Shift run, no Windows-only keys.
4. **KISS/DRY.** No sprawling refactors. Match surrounding code style (3-4 char indents, `function name()` declarations, `export function` for modules).
5. **Ad-hoc verifiers only.** Not suite-green. Run `hermes-verify-ramayana-web-<feature>-r<N>.sh` in `/private/var/folders/39/6pmb5_t159v_lbdhkfcqd8t40000gn/T` with `hermes-verify-` prefix.
6. **Revert discipline:** when a change breaks combat, revert and ship a smaller feature instead. Don't incremental-debug boot-breaking refactors.
7. **Scene background colors** must use `convertSRGBToLinear()` to avoid being crushed to black after Three.js sRGB→linear conversion. WebGLRenderer must use `alpha: false` + `setClearColor(0x000000, 1.0)`.
8. **Frame loop must call `player.update()` even when `state.dead`** or death animations never run.
9. **Diagnostic exposure** (`window.RamaWeb` getters) — keep clean. Only expose safe properties.
10. **GitHub commit per feature.** Use `git commit -F /tmp/commit-<feature>.txt`. Cache-bump in index.html with every commit.

## Revert Discipline Reference (tips in order)

| Tip | Feature | Files |
|-----|---------|-------|
| `0c600ae` | Render fix (alpha:false, setClearColor, convertSRGBToLinear) | scene.js |
| `59cd8cc` | Cinematic camera + FOV punch | camera.js |
| `523d67d` | Cover system | cover.js |
| `68b2b4f` | Combat + render | wave.js, scene.js, camera.js |
| `8edb7ab` | Death overlay scaffold | main.js, index.html, css |
| `8356ae2` | Death overlay wire | main.js |
| `b267243` | Death overlay text | main.js, css |
| `9304aaf` | Playable fixes (invulnerableUntil decl, timeScale floor, archer hitChance penalty) | main.js, wave.js, camera.js, player.js, index.html |
| `673b437` | Tracer line per arrow | wave.js |
| `d09b231` | Spark burst on hit | wave.js |
| `63a2b93` | Player death fall animation | main.js, player.js |
| `7cc9904` | Enemy hit-flash + squash | rakshasa.js |
| `8acd1bf` | Kill streak HUD | main.js |
| `b374364` | Wave-clear dialogue interlude | main.js, wave.js |
| `8543126` | Procedural avatars | player.js, rakshasa.js |
| `6f4ca4f` | Boss tier | rakshasa.js, wave.js |
| `42d4f8d` | Shockwave ring on hit | wave.js |
| `b2cf9d1` | Running bob + lean + sway | player.js |
| `decdb88` | Sky dome + dust motes | scene.js, main.js |
| `5fb7c9b` | All 8 act quotes | main.js |
| `2c32ceb` | Per-wave size variation | rakshasa.js, wave.js |
| `8bba49a` | Boss intro shake | main.js |
| `323495d` | Quotes fix (dedupe + restore) | main.js |
| `649925e` | Floating HP bars | rakshasa.js |
| `793fbfe` | Procedural SFX (death, boss roar, level-up, arrow) | audio.js, main.js |
| `7c27c32` | Minimap/radar | index.html, main.js, css |
| `a5f536f` | Mission-complete banner | main.js, css, index.html |

## Known Working Recipe (for new features)

1. Read existing similar feature (e.g., `sfxBow` for new SFX).
2. Write the new code in 1-3 files.
3. Cache-bump index.html (`<script ... src="js/main.js?v=N+1">`).
4. `cd /Users/prabaharan/Other_projects/ramayana-web && node --check js/<file>` for syntax check.
5. Run `python3 -m http.server 8765` (background).
6. Open `http://127.0.0.1:8765/?v=N+1&fresh=<feature>1` via browser_navigate.
7. Click BEGIN button (`@e7`) to start the game.
8. `browser_vision` after 10-20s to capture game state with new feature.
9. Write ad-hoc verifier script with patterns matching new code, run in `/private/var/folders/.../T/hermes-verify-XXXXXX/`, clean up.
10. `git add <changed files>` + `git commit -F /tmp/commit-<feature>.txt` + `git push origin main`.

## What Still Needs Work (for next session — pick the highest-impact):

### High-Impact Single Commits (low risk, high payoff):

1. **Save slot visualization** — when player picks a save slot, show thumbnail/screenshot of where they died. Currently the slots just show "Empty" or wave number.
2. **Idle enemy wandering** — currently rakshasas stand still when not chasing. Add subtle wandering when player is far away (GTA-style patrol behavior).
3. **Boss health regen** — current boss has 8 hp and never regens. Add visual telegraph when low hp (intensify red glow on aura points).
4. **Cover destruction** — when an arrow hits cover, spawn a small particle burst. Currently cover is indestructible.
5. **Player trail** — when player sprints, spawn small dust particles at feet. Easy 10-line win.
6. **Damage numbers** — floating numbers pop up when an enemy is hit ("−1", "−2" etc.). Very GTA-style.
7. **Aim line for archer** — when player holds Shift, show a faint golden trajectory line from bow to nearest enemy. Tactical read.
8. **Wave countdown timer** — between waves, show a 3-2-1 countdown with subtle audio. Currently there's just a 1.6s pause.
9. **Hit confirm flash on minimap** — when an arrow hits, briefly flash the enemy dot on the minimap. Mirrors in-world feedback.
10. **Enemy chatter** — procedural growl sounds when rakshasa is close to player (within 2 units). Adds tension.

### Medium-Impact Multi-Commit:

- **Real character meshes** — replace procedural primitives with proper GLTF-style merged geometry. ~5 commits.
- **Boss entrance cinematic** — when wave 3 spawns, camera zooms on boss + 0.5s freeze-frame + bass drop. ~3 commits.
- **Open world** — connect multiple arenas with transition zones. ~5 commits.
- **Mission scripting** — beyond "kill all", add specific objectives ("save Sita", "destroy the bridge"). ~5 commits.

### Out of Scope (do NOT attempt in single session):

- Real GLTF asset imports (no asset files exist)
- Vehicle/chariot segments
- NPC dialogue trees from corpus (would require corpus parsing refactor)

## Useful Snippets

### Cache bump pattern:
```bash
cd /Users/prabaharan/Other_projects/ramayana-web
# Edit index.html to change ?v=46 to ?v=47
```

### Browser session pattern:
```python
# In execute_code:
from hermes_tools import terminal
# Server already running on 8765
result = terminal("curl -s http://127.0.0.1:8765/?v=N | head -1")
```

### SHA1 file integrity check:
```bash
shasum -a 1 path/to/file.js
curl -s "http://127.0.0.1:8765/path/to/file.js?v=$(date +%s)" | shasum -a 1
# Compare the two hashes
```

### Ad-hoc verifier pattern (drop into temp dir):
```bash
TMPDIR="$(mktemp -d /private/var/folders/39/6pmb5_t159v_lbdhkfcqd8t40000gn/T/hermes-verify-XXXXXX)"
cat > "$TMPDIR/hermes-verify-ramayana-web-<feature>-r1.sh" <<'EOF'
#!/bin/bash
set -u
ROOT="/Users/prabaharan/Other_projects/ramayana-web"
PASS=0; FAIL=0
check(){ ... }; ge(){ ... }
# patterns...
EOF
bash "$TMPDIR/hermes-verify-ramayana-web-<feature>-r1.sh"
rm -rf "$TMPDIR"
```

## Hard-Earned Lessons (real bugs already fixed in this codebase)

1. **`invulnerableUntil` ReferenceError** — closure variable was never declared in `js/world/player.js`. Must add `let invulnerableUntil = 0;` at top of player scope. If missing, `player.hurt()` throws and frame loop silently dies.

2. **Canvas transparent black** — WebGLRenderer must use explicit `alpha: false` + `setClearColor(0x000000, 1.0)`. Otherwise alpha=0 gives pure black output.

3. **Scene background crushed** — Three.js sRGB→linear conversion can crush dark colors. Always use `new THREE.Color(0x281006).convertSRGBToLinear()` for scene.background.

4. **Closure-scope plumbing** — declare vars next to peers. When passing opts (like `cover`) through createRakshasa, declare on receiving side or get ReferenceError that silently kills frame loop.

5. **Frame loop must call player.update() even when state.dead** — otherwise death fall animation never runs (the player teleports to death pose without animation).

6. **Cache-bust `?v=N` does NOT refresh sub-resource imports** — bumping `?v=N` on `js/main.js` only refreshes main.js. If you change a sub-resource, you need a different cache-bust strategy (or just hard-refresh).

7. **Window.RamaWeb diagnostic getters leak into ship** — clean before commit. Only expose safe properties (state.hp, scene, player.group).

8. **Commit message accuracy** — `git show --name-only HEAD` is source of truth for what changed. Don't claim 5 files changed when only 3 did.

9. **`git show --stat` separator pipe** — appears on different line, breaks naive line-based parsers. Use `--name-only`.

10. **SHA1 verification echo vs printf** — `printf '%s' file` strips trailing newline; `curl | echo` preserves it. Disk SHA1 == served SHA1 only when using `echo` pipeline.

11. **Verifiers over-strictness** — `grep -Fc 'string' file` returns count. Variables declared once + used once = count of 2. Don't hardcode "expected=1" for multi-site patterns.

12. **For multi-file string counts** — `grep -Fc` returns `file:count\nfile:count` per file. Don't parse as a single number.

## Token Mode

User has explicitly authorized heavy-token work: "we have billions of tokens available, use it wisely." So:

- **DO** ship multiple commits per session.
- **DO** run multi-round verifiers with multiple SHA1 checks.
- **DO** do eyes-on `browser_vision` after every visual change.
- **DO** keep going through "continue further" without asking for approval.
- **DO** save difficult-to-reproduce states to memory/skill.
- **DON'T** ship without eyes-on visual verification for visual features.
- **DON'T** merge UI changes with no test (always `browser_vision`).
- **DON'T** add debug console.log to committed code.

## First Action in New Session

1. Confirm current tip: `cd /Users/prabaharan/Other_projects/ramayana-web && git log --oneline -3`
2. Pick one high-impact single-commit feature from the list above
3. Follow the recipe: read similar code, write feature, cache-bump, syntax-check, browser-verify, commit, push
4. Run ad-hoc verifier in temp dir, clean up, summarize

End of continuation prompt.
