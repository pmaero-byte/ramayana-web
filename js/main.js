import * as THREE from 'three';
import { state, setHud, setHpBar, saveGame } from './core/state.js';
import { createInput } from './core/input.js';
import { createTouchPad } from './core/touch.js';
import { unlockAudio, sfxBow, sfxHit, sfxWave, sfxCue, sfxWin, startDrone, stopDrone } from './core/audio.js';
import { createWorld } from './world/scene.js';
import { createPlayer } from './world/player.js';
import { createCameraRig } from './world/camera.js';
import { createWaveController, createArcher } from './combat/wave.js';
import { createStory } from './story/moments.js';
import { showDialogue, buildTitle, hideTitle, showTitle, updateContinueBtn } from './ui/dialogue.js';

const ACT_MOOD = {
  'bala-birth': ['#4f8cff', '#1a2840'],
  'ayodhya-dharma': ['#f0bd5e', '#3a2810'],
  'panchavati-golden-deer': ['#5fd1a5', '#0e2a1c'],
  'kishkindha-alliance': ['#e46445', '#2a1410'],
  'sundarakanda-leap': ['#6c4dc2', '#1a1030'],
  'yuddhakanda-war': ['#b85a3a', '#2a1008'],
  'return-ayodhya': ['#d4a843', '#2a2010'],
  'uttara-earth-return': ['#5d6fb7', '#101828'],
};

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
  let kills = 0;
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

  document.getElementById('btn-fullscreen')?.addEventListener('click', () => {
    const el = document.documentElement;
    if (!document.fullscreenElement) el.requestFullscreen?.();
    else document.exitFullscreen?.();
  });
  document.getElementById('btn-menu')?.addEventListener('click', () => returnToTitle());
  document.getElementById('btn-save')?.addEventListener('click', () => {
    const ok = saveGame(state.actId, kills, state.hp, state.maxHp, state.objectiveTitle);
    setHud({ wave: ok ? 'Game saved ✓' : 'Save failed' });
    setTimeout(() => setHud({ wave: `Wave ${waves?.wave || '-'}/3` }), 1200);
  });

  buildTitle(
    corpus,
    (id, act) => {
      state.selectedActId = id;
      document.getElementById('btn-start').textContent = `Begin · ${act.title || id}`;
    },
    (id) => {
      unlockAudio();
      startGame(id);
    },
    (saved) => {
      unlockAudio();
      startGame(saved.actId, saved);
    }
  );

  function applyActMood(actId) {
    const mood = ACT_MOOD[actId];
    if (mood) world.setMood?.(mood[0], mood[1]);
    else world.setMood?.('#b85a3a', '#2a1008');
  }

  function returnToTitle() {
    waves?.stop();
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
    player.setLocked(true);
    waves?.stop();
    setHud({ obj: 'Fallen — respawning…' });
    showDialogue('Valmiki', 'Even the greatest heroes rise again. Hold fast to dharma.', 2.5);
    deathTimer = 2.4;
  }

  function respawn() {
    state.dead = false;
    player.reset();
    player.setLocked(false);
    setHpBar(state.maxHp, state.maxHp);
    setHud({ obj: state.objectiveTitle || 'Continue the fight' });
    waves?.start(3);
  }

  function damagePlayer(n = 1) {
    if (state.dead || !state.running) return;
    if (!player.hurt(n)) return;
    const hp = Math.max(0, state.hp - n);
    setHpBar(hp, state.maxHp);
    flash = 0.14;
    sfxHit();
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
    applyActMood(state.actId);

    waves?.stop();
    story = createStory(corpus);
    waves = createWaveController(
      world.scene,
      player,
      (w, total, kind, n) => {
        setHud({ wave: `Wave ${w}/${total} · ${kind} · ${n} · ${kills} kills` });
        sfxWave();
      },
      () => {
        setHud({ wave: `All waves cleared · ${kills} kills` });
        sfxWin();
        story?.completeCurrent();
      },
      () => damagePlayer(1)
    );
    archer = createArcher(world.scene, player, waves, {
      onFire: () => sfxBow(),
      onHit: () => {
        kills += 1;
        sfxHit();
        flash = 0.1;
        setHud({ wave: `Wave ${waves.wave}/3 · ${waves.alive.length} left · ${kills} kills` });
      },
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
        deathTimer -= dt;
        if (deathTimer <= 0) respawn();
      } else {
        player.update(dt, input, input.yaw);
        waves?.update(dt);
        archer?.update(dt);
        // auto-save every ~30s of active play
        autoSaveTimer += dt;
        if (autoSaveTimer > 30) {
          autoSaveTimer = 0;
          saveGame(state.actId, kills, state.hp, state.maxHp, state.objectiveTitle);
        }
      }
      camRig.update(dt, player, input.yaw, input.pitch);
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

  window.RamaWeb = { state, startGame, returnToTitle, THREE };
}

boot().catch((err) => {
  console.error(err);
  const t = document.getElementById('hud-obj');
  if (t) t.textContent = 'Boot failed — see console (serve via http, not file://)';
});
