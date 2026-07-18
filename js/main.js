import * as THREE from 'three';
import { state, setHud, setHpBar, saveSlot, loadSlot, deleteSlot, listSlots, saveGame, setHighScore } from './core/state.js';
import { setLocale, getLocale, availableLocales } from './core/i18n.js';
import { createInput } from './core/input.js';
import { createTouchPad } from './core/touch.js';
import { unlockAudio, sfxBow, sfxHit, sfxWave, sfxCue, sfxWin, startDrone, stopDrone } from './core/audio.js';
import { createWorld } from './world/scene.js';
import { createPlayer } from './world/player.js';
import { createCameraRig } from './world/camera.js';
import { createWaveController, createArcher } from './combat/wave.js';
import { createCoverSet } from './combat/cover.js';
import { createStory } from './story/moments.js';
import { showDialogue, buildTitle, hideTitle, showTitle, updateContinueBtn, buildCharacterSelect, buildSlotsUi } from './ui/dialogue.js';

async function boot() {
  const canvas = document.getElementById('c');
  const res = await fetch('data/corpus_data.json');
  const corpus = await res.json();
  state.corpus = corpus;

  const world = createWorld(canvas);
  const input = createInput(canvas);
  createTouchPad(input);
  const player = createPlayer(world.scene);
  const camRig = createCameraRig(world.camera);

  let story = null;
  let waves = null;
  let archer = null;

  // Per-act quotes shown after each wave is cleared. First matching line
  // for the cleared wave index; falls back to last line.
  const WAVE_QUOTES = {
    'yuddhakanda-war': {
      speaker: 'Rama',
      lines: [
        'The first stone is laid. The bridge of dharma begins.',
        'Indrajit falls — maya warfare cannot stand against truth.',
        'Ravana is defeated. Dharma is restored.',
      ],
    },
    'panchavati-golden-deer': {
      speaker: 'Lakshmana',
      lines: [
        'The deer was Maya. We must hold the line.',
        'Surpanakha is repelled. Her kin will answer.',
        'Lanka burns. Ravana prepares his answer.',
      ],
    },
    'kishkindha-alliance': {
      speaker: 'Hanuman',
      lines: [
        'Sugriva is crowned. The vanara army is yours, Rama.',
        'The search parties are sent. Every direction covered.',
        'Sampati speaks of Lanka. The path is known.',
      ],
    },
    'sundarakanda-leap': {
      speaker: 'Hanuman',
      lines: [
        'The ocean answered. I leap for Lanka.',
        'I have found Sita. The message is delivered.',
        'Lanka burns and I am unbound. The war begins.',
      ],
    },
    'ayodhya-dharma': {
      speaker: 'Bharata',
      lines: [
        'The kingdom is yours in absence. I will hold it for you.',
        'The people mourn with me. Your sandals guide the throne.',
        'Rama returns. Ayodhya lights ten thousand lamps.',
      ],
    },
    'bala-birth': {
      speaker: 'Vishwamitra',
      lines: [
        'A prince is born to dash the asuras\' pride. Begin.',
        'Tadaka falls. The forest is no longer her dark dominion.',
        'The bow of Shiva sings. Sita is found.',
      ],
    },
    'panchavati-golden-deer': {
      speaker: 'Lakshmana',
      lines: [
        'The deer was Maya. We must hold the line.',
        'Surpanakha is repelled. Her kin will answer.',
        'Lanka burns. Ravana prepares his answer.',
      ],
    },
    'uttara-earth-return': {
      speaker: 'Valmiki',
      lines: [
        'The throne is given back. Sita returns to the earth.',
        'Time takes what it must. The story continues elsewhere.',
        'The seventh avatar rests. The cycle turns.',
      ],
    },
  };
  let cover = null;
  let kills = 0;
  let streak = 0;
  let last = performance.now();
  let flash = 0;
  let deathTimer = 0;
  let autoSaveTimer = 0;

  let flashEl = document.getElementById('hit-flash');
  if (!flashEl) {
    flashEl = document.createElement('div');
    flashEl.id = 'hit-flash';
    document.getElementById('app')?.appendChild(flashEl);
  }

  // expose slot helpers for slots UI
  state.__getSlots = () => {
    const out = {};
    try { Object.assign(out, JSON.parse(window.localStorage.getItem('ramayana_web_slots') || '{}')); } catch {}
    return out;
  };

  function toggleFullscreen() {
    const el = document.documentElement;
    if (!document.fullscreenElement) el.requestFullscreen?.();
    else document.exitFullscreen?.();
  }

  document.getElementById('btn-fullscreen')?.addEventListener('click', toggleFullscreen);
  document.getElementById('btn-menu')?.addEventListener('click', () => returnToTitle());
  document.getElementById('btn-save')?.addEventListener('click', () => {
    const ok = saveGame(state.actId, kills, state.hp, state.maxHp, state.objectiveTitle);
    setHud({ wave: ok ? 'Saved to autoslot ✓' : 'Save failed' });
    setTimeout(() => setHud({ wave: `Wave ${waves?.wave || '-'}/3` }), 1200);
  });
  document.getElementById('btn-slots')?.addEventListener('click', () => toggleSlotsPanel(true));
  document.getElementById('btn-slots-close')?.addEventListener('click', () => toggleSlotsPanel(false));
  document.getElementById('btn-slots-hud')?.addEventListener('click', () => toggleSlotsPanel(true));

  // Death overlay buttons (DOM exists from commit 8edb7ab; wiring here)
  document.getElementById('btn-retry')?.addEventListener('click', () => {
    const ov = document.getElementById('death-overlay');
    if (ov) ov.classList.add('hidden');
    hideDialogue();
    state.dead = false;
    state.deathSince = 0;
    respawn();
  });
  document.getElementById('btn-return')?.addEventListener('click', () => {
    const ov = document.getElementById('death-overlay');
    if (ov) ov.classList.add('hidden');
    hideDialogue();
    state.dead = false;
    state.deathSince = 0;
    returnToTitle();
  });
  document.getElementById('btn-locale')?.addEventListener('click', () => {
    const next = getLocale() === 'en' ? 'sa' : 'en';
    setLocale(next);
    try { window.localStorage.setItem('ramayana_web_locale', next); } catch {}
    document.getElementById('btn-locale').textContent = next === 'en' ? 'अ/En' : 'En/अ';
    setHud({ wave: `Language: ${next === 'en' ? 'English' : 'संस्कृतम्'}` });
    setTimeout(() => setHud({ wave: `Wave ${waves?.wave || '-'}/3` }), 1200);
  });
  try {
    const saved = window.localStorage.getItem('ramayana_web_locale');
    if (saved === 'sa' || saved === 'en') setLocale(saved);
  } catch {}
  // parent iframe can request a locale via postMessage
  window.addEventListener('message', (e) => {
    if (e?.data?.type === 'rama-set-locale' && (e.data.locale === 'en' || e.data.locale === 'sa')) {
      setLocale(e.data.locale);
      try { window.localStorage.setItem('ramayana_web_locale', e.data.locale); } catch {}
    }
  });
  window.addEventListener('keydown', (e) => {
    if (e.key === 'f' || e.key === 'F') toggleFullscreen();
    if (e.key === 'm' || e.key === 'M') { if (state.running) returnToTitle(); }
    if (e.key === 'Escape') toggleSlotsPanel(false);
  });

  buildTitle(
    corpus,
    (id, act) => {
      state.selectedActId = id;
      document.getElementById('btn-start').textContent = `Begin · ${act.title || id}`;
    },
    (id) => {
      unlockAudio();
      toggleSlotsPanel(false);
      startGame(id);
    },
    (saved) => {
      unlockAudio();
      toggleSlotsPanel(false);
      startGame(saved.actId, saved);
    }
  );

  buildCharacterSelect(corpus, state.selectedCharacter, (id) => {
    state.selectedCharacter = id;
    const c = corpus.characters.find(x => x.characterId === id);
    player.setCharacter?.(id, c?.color);
  });

  buildSlotsUi(
    (i, s) => { unlockAudio(); toggleSlotsPanel(false); startGame(s.actId, s); },
    (i) => { deleteSlot(i); refreshSlotsPanel(); },
    (i) => saveToSlot(i)
  );

  function refreshSlotsPanel() {
    buildSlotsUi(
      (ii, s) => { unlockAudio(); toggleSlotsPanel(false); startGame(s.actId, s); },
      (ii) => { deleteSlot(ii); refreshSlotsPanel(); },
      (ii) => saveToSlot(ii)
    );
  }

  function saveToSlot(i) {
    const ok = saveSlot(i, {
      actId: state.actId, kills, hp: state.hp, maxHp: state.maxHp,
      objectiveTitle: state.objectiveTitle, characterId: state.selectedCharacter,
    });
    setHud({ wave: ok ? `Saved to slot ${i} ✓` : 'Save failed' });
    setTimeout(() => setHud({ wave: `Wave ${waves?.wave || '-'}/3` }), 1200);
    refreshSlotsPanel();
  }

  function toggleSlotsPanel(show) {
    const p = document.getElementById('slots-panel');
    if (!p) return;
    p.classList.toggle('hidden', !show);
    if (show) refreshSlotsPanel();
  }

  function applyActMood(actId) {
    world.buildArena?.(actId);
  }

  function returnToTitle() {
    waves?.stop();
    cover?.dispose();
    cover = null;
    archer = null;
    story = null;
    state.running = false;
    state.dead = false;
    player.reset();
    player.setLocked(false);
    setHpBar(state.maxHp, state.maxHp);
    setHud({ title: 'Rāmāyaṇa', obj: '—', wave: 'Wave —' });
    updateContinueBtn();
    showTitle();
    stopDrone();
  }

  function onPlayerDeath() {
    if (state.dead) return;
    state.dead = true;
    state.deathSince = performance.now();
    state.deathWave = waves?.wave || 0;
    state.deathKills = kills;
    player.setLocked(true);
    waves?.stop();
    const dKills = kills;
    const dWave = waves?.wave || 0;
    setHud({ obj: `Fallen at wave ${dWave} — ${dKills} slain`, wave: 'Respawning…' });
    setHighScore(dKills, dWave);
    showDialogue('Valmiki', 'Even the greatest heroes rise again. Hold fast to dharma.', 2.5);
    deathTimer = 2.4;
    camRig.shake?.(0.9);
    // Show death overlay after the dialogue settles — player can retry or quit.
    setTimeout(() => {
      if (!state.dead) return;
      const ov = document.getElementById('death-overlay');
      const stats = document.getElementById('death-stats');
      if (ov && stats) {
        stats.textContent = `${state.deathKills || kills} slain · wave ${state.deathWave || (waves?.wave || 0)}`;
        ov.classList.remove('hidden');
      }
    }, 2400);
  }
  function respawn() {
    state.dead = false;
    player.reset();
    player.setLocked(false);
    setHpBar(state.maxHp, state.maxHp);
    setHud({ obj: state.objectiveTitle || 'Continue the fight' });
    waves?.start(3);
    streak = 0;
    const ov = document.getElementById('death-overlay');
    if (ov) ov.classList.add('hidden');
  }

  function damagePlayer(n = 1) {
    if (state.dead || !state.running) return;
    if (!player.hurt(n)) return;
    const hp = Math.max(0, state.hp - n);
    setHpBar(hp, state.maxHp);
    flash = 0.14;
    sfxHit();
    camRig.damageHit();
    streak = 0; // player took damage — streak breaks
    if (hp <= 0) onPlayerDeath();
  }

  function startGame(actId, saved = null) {
    hideTitle();
    unlockAudio();
    startDrone();
    state.running = true;
    state.dead = false;
    state.actId = actId || state.selectedActId;
    kills = saved?.kills ?? 0;
    deathTimer = 0;
    autoSaveTimer = 0;
    player.reset();
    player.setLocked(false);
    setHpBar(saved?.hp ?? state.maxHp, saved?.maxHp ?? state.maxHp);
    // brief invulnerability at game start so first wave doesn't instakill
    player.invulnerableUntil = performance.now() + 2000;
    if (saved?.characterId) {
      const c = corpus.characters.find(x => x.characterId === saved.characterId);
      player.setCharacter?.(saved.characterId, c?.color);
    } else {
      const c = corpus.characters.find(x => x.characterId === state.selectedCharacter);
      player.setCharacter?.(state.selectedCharacter, c?.color);
    }
    applyActMood(state.actId);

    waves?.stop();
    cover?.dispose();
    cover = createCoverSet(world.scene, player, state.actId);
    story = createStory(corpus);
    waves = createWaveController(
      world.scene,
      player,
      (w, total, kind, n) => {
        setHud({ wave: `Wave ${w}/${total} · ${kind} · ${n} · ${kills} kills` });
        sfxWave();
        // Boss intro: brief slow-mo + extra shake when wave 3 spawns
        if (w === 3) {
          camRig.killHit?.();
        }
      },
      () => {
        setHud({ wave: `All waves cleared · ${kills} kills` });
        sfxWin();
        story?.completeCurrent();
      },
      (w, total, kind) => {
        // Per-act quote when a wave is cleared — small cinematic interlude.
        const q = WAVE_QUOTES[state.actId];
        if (q) showDialogue(q.speaker, q.lines[w - 1] || q.lines[q.lines.length - 1], 2.4);
      },
      () => damagePlayer(1),
      { cover }
    );
    archer = createArcher(world.scene, player, waves, {
      onFire: () => sfxBow(),
      onHit: () => {
        kills += 1;
        streak += 1;
        sfxHit();
        camRig.killHit();
        flash = 0.1;
        setHud({ wave: `Wave ${waves.wave}/3 · ${waves.alive.length} left · ${kills} kills${streak >= 3 ? `  · STREAK x${streak}` : ''}` });
      },
      cover,
    });

    story.on('enter', (obj) => {
      state.objectiveId = obj.id;
      state.objectiveTitle = obj.title || obj.cue || obj.id;
      const title = story.act?.title || state.actId;
      setHud({ title, obj: state.objectiveTitle });
      const speaker = story.playerRole || obj.marker || 'Rama';
      showDialogue(speaker, obj.cue || obj.title || '', 2.8);
      sfxCue();
    });
    story.on('complete', (obj) => {
      const cl = obj.completedLine;
      if (cl) showDialogue(cl.speaker || 'Narrator', cl.text || '', 3.0);
      saveGame(state.actId, kills, state.hp, state.maxHp, story?.current?.title || '');
      setTimeout(() => story.advance(), 3200);
    });
    story.on('actDone', () => {
      setHud({ obj: 'Act complete — dharma upheld' });
      showDialogue('Valmiki', 'Thus ends this kāṇḍa. Returning to the kāṇḍa picker…', 3.2);
      sfxWin();
      waves?.stop();
      setHighScore(kills, 3);
      saveGame(state.actId, kills, state.hp, state.maxHp, '✓ Complete');
      setTimeout(() => returnToTitle(), 3400);
    });

    const ok = story.loadAct(state.actId);
    if (!ok) {
      setHud({ obj: 'Act not found in corpus' });
      return;
    }
    setHud({ title: story.act?.title || state.actId });
    waves.start(3);
  }

  function frame(now) {
    const dt = Math.min(0.05, (now - last) / 1000);
    last = now;
    if (state.running) {
      if (state.dead) {
        player.update(dt, input, input.yaw); // run death fall animation
        deathTimer -= dt;
        if (deathTimer <= 0) respawn();
      } else {
        player.update(dt, input, input.yaw);
        waves?.update(dt * camRig.getTimeScale());
        archer?.update(dt * camRig.getTimeScale());
        world.updateAtmosphere?.(dt);
        autoSaveTimer += dt;
        if (autoSaveTimer > 30) {
          autoSaveTimer = 0;
          saveGame(state.actId, kills, state.hp, state.maxHp, state.objectiveTitle);
        }
      }
      const mv = input.moveVector();
      const moveSpeed = mv.mag;
      camRig.update(dt, player, input.yaw, input.pitch, {
        moveSpeed,
        sprinting: input.run && moveSpeed > 0.1,
      });
    } else {
      const t = now * 0.00015;
      world.camera.position.set(Math.sin(t) * 10, 4, Math.cos(t) * 10);
      world.camera.lookAt(0, 1, 0);
    }
    if (flash > 0) {
      flash -= dt;
      flashEl.style.opacity = String(Math.max(0, flash * 4));
    } else {
      flashEl.style.opacity = '0';
    }
    world.renderer.render(world.scene, world.camera);
    requestAnimationFrame(frame);
  }
  requestAnimationFrame(frame);

  window.RamaWeb = { state, startGame, returnToTitle, THREE, listSlots, saveSlot, loadSlot, setLocale, getLocale, availableLocales, scene: world.scene, camera: world.camera, player };
}

boot().then(() => { window.RAMA_BOOT.loaded = true; }).catch((err) => {
  console.error(err);
  const t = document.getElementById('hud-obj');
  if (t) t.textContent = 'Boot failed — see console (serve via http, not file://)';
});
