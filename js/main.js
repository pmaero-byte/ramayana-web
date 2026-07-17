import * as THREE from 'three';
import { state, setHud } from './core/state.js';
import { createInput } from './core/input.js';
import { createTouchPad } from './core/touch.js';
import { unlockAudio, sfxBow, sfxHit, sfxWave, sfxCue, sfxWin } from './core/audio.js';
import { createWorld } from './world/scene.js';
import { createPlayer } from './world/player.js';
import { createCameraRig } from './world/camera.js';
import { createWaveController, createArcher } from './combat/wave.js';
import { createStory } from './story/moments.js';
import { showDialogue, buildTitle, hideTitle } from './ui/dialogue.js';

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

  // hit flash overlay
  let flashEl = document.getElementById('hit-flash');
  if (!flashEl) {
    flashEl = document.createElement('div');
    flashEl.id = 'hit-flash';
    document.getElementById('app')?.appendChild(flashEl);
  }

  buildTitle(
    corpus,
    (id, act) => {
      state.selectedActId = id;
      document.getElementById('btn-start').textContent = `Begin · ${act.title || id}`;
    },
    (id) => {
      unlockAudio();
      startGame(id);
    }
  );

  function startGame(actId) {
    hideTitle();
    unlockAudio();
    state.running = true;
    state.actId = actId || state.selectedActId;
    kills = 0;
    story = createStory(corpus);
    waves = createWaveController(
      world.scene,
      player,
      (w, total, kind, n) => {
        setHud({ wave: `Wave ${w}/${total} · ${kind} · ${n} rakshasas · ${kills} kills` });
        sfxWave();
      },
      () => {
        setHud({ wave: `All waves cleared · ${kills} kills` });
        sfxWin();
        story?.completeCurrent();
      }
    );
    archer = createArcher(world.scene, player, waves, {
      onFire: () => sfxBow(),
      onHit: () => {
        kills += 1;
        sfxHit();
        flash = 0.12;
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
      setTimeout(() => story.advance(), 3200);
    });
    story.on('actDone', () => {
      setHud({ obj: 'Act complete — dharma upheld' });
      showDialogue('Valmiki', 'Thus ends this kāṇḍa. Reload the page to choose another act.', 4);
      sfxWin();
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
      player.update(dt, input, input.yaw);
      camRig.update(dt, player, input.yaw, input.pitch);
      waves?.update(dt);
      archer?.update(dt);
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

  window.RamaWeb = { state, startGame, THREE };
}

boot().catch((err) => {
  console.error(err);
  const t = document.getElementById('hud-obj');
  if (t) t.textContent = 'Boot failed — see console (serve via http, not file://)';
});
