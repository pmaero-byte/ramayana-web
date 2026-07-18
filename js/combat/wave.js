import * as THREE from 'three';
import { createRakshasa } from './rakshasa.js';
import { spawnPoints, kindForWave } from './formation.js';

export function createWaveController(scene, player, onWave, onAllDone, onMelee, opts = {}) {
  let alive = [];
  let wave = 0;
  let total = 3;
  let running = false;
  let pause = 0;
  const cover = opts.cover || null;
  const pathfind = opts.pathfind !== false;

  function clearAlive() {
    for (const r of alive) r.dispose();
    alive = [];
  }

  function start(totalWaves = 3) {
    clearAlive();
    total = Math.max(1, totalWaves);
    wave = 0;
    running = true;
    pause = 0;
    nextWave();
  }

  function stop() {
    running = false;
    pause = 0;
    clearAlive();
  }

  function nextWave() {
    if (!running) return;
    wave += 1;
    if (wave > total) {
      running = false;
      onAllDone?.();
      return;
    }
    const kind = kindForWave(wave);
    const origin = player.position;
    const forward = player.forward;
    const pts = spawnPoints(kind, 3 + wave, origin, forward, 9 + wave * 2);
    alive = pts.map((p, i) => createRakshasa(scene, p, 2 + wave, { cover }));
    onWave?.(wave, total, kind, alive.length);
  }

  function update(dt) {
    if (!running) return;
    const ppos = player.position;
    alive = alive.filter((r) => {
      if (r.isDead) {
        r.dispose();
        return false;
      }
      const hit = r.update?.(dt, ppos);
      if (hit) onMelee?.(1);
      return true;
    });
    if (alive.length === 0) {
      pause += dt;
      if (pause > 1.6) {
        pause = 0;
        nextWave();
      }
    }
  }

  function findTarget(origin, forward, range = 12, coneDeg = 60) {
    const cosLimit = Math.cos((coneDeg * 0.5 * Math.PI) / 180);
    let best = null;
    let bestD = range * range;
    for (const r of alive) {
      if (r.isDead) continue;
      const dx = r.position.x - origin.x;
      const dz = r.position.z - origin.z;
      const d2 = dx * dx + dz * dz;
      if (d2 > bestD) continue;
      const len = Math.sqrt(d2) || 1;
      const dot = (forward.x * dx + forward.z * dz) / len;
      if (dot < cosLimit) continue;
      bestD = d2;
      best = r;
    }
    return best;
  }

  return {
    start,
    stop,
    update,
    findTarget,
    get alive() { return alive; },
    get wave() { return wave; },
    get running() { return running; },
  };
}

/** Kinematic arrows + ember trail */
export function createArcher(scene, player, waves, hooks = {}) {
  const arrows = [];
  let cd = 0;
  const interval = 0.55;
  const speed = 22;
  const dmg = 1;

  function fire() {
    const origin = player.position.clone();
    origin.y = 1.2;
    const target = waves.findTarget(origin, player.forward);
    if (!target) return;
    // Accuracy penalty: stationary = 90% hit, moving = 60% hit.
    // Lets a few rakshasas slip through to melee so the player
    // actually sees combat instead of standing still watching arrows.
    const moving = (player.moveSpeed || 0) > 0.1;
    const hitChance = moving ? 0.6 : 0.9;
    if (Math.random() > hitChance) return; // miss — no arrow fired this cycle
    const to = new THREE.Vector3(
      target.position.x - origin.x,
      0.9 - origin.y + 1,
      target.position.z - origin.z
    ).normalize();

    const mesh = new THREE.Mesh(
      new THREE.CylinderGeometry(0.04, 0.04, 0.7, 6),
      new THREE.MeshStandardMaterial({ color: 0xd9b44a, metalness: 0.4, roughness: 0.4, emissive: 0x331a00, emissiveIntensity: 0.4 })
    );
    mesh.position.copy(origin);
    mesh.quaternion.setFromUnitVectors(new THREE.Vector3(0, 1, 0), to);
    scene.add(mesh);

    const trailCount = 12;
    const trailGeo = new THREE.BufferGeometry();
    const tpos = new Float32Array(trailCount * 3);
    for (let i = 0; i < trailCount; i++) {
      tpos[i * 3] = origin.x;
      tpos[i * 3 + 1] = origin.y;
      tpos[i * 3 + 2] = origin.z;
    }
    trailGeo.setAttribute('position', new THREE.BufferAttribute(tpos, 3));
    const trailMat = new THREE.PointsMaterial({ color: 0xffb060, size: 0.12, transparent: true, opacity: 0.85 });
    const trail = new THREE.Points(trailGeo, trailMat);
    scene.add(trail);

    // Bright tracer line from origin to current arrow position — makes every
    // shot readable as a streak of light, not just dots.
    const lineGeo = new THREE.BufferGeometry();
    const linePos = new Float32Array([origin.x, origin.y, origin.z, origin.x, origin.y, origin.z]);
    lineGeo.setAttribute('position', new THREE.BufferAttribute(linePos, 3));
    const lineMat = new THREE.LineBasicMaterial({ color: 0xffd070, transparent: true, opacity: 0.95 });
    const tracer = new THREE.Line(lineGeo, lineMat);
    scene.add(tracer);

    arrows.push({
      mesh, trail, trailGeo, trailMat, tracer, lineGeo, lineMat, dir: to,
      life: 2.2, hit: false, tIdx: 0, tAcc: 0,
      origin: origin.clone(),
      blocked: false,
    });
    hooks.onFire?.();
  }

  function update(dt) {
    cd -= dt;
    if (cd <= 0) {
      fire();
      cd = interval;
    }
    for (let i = arrows.length - 1; i >= 0; i--) {
      const a = arrows[i];
      a.life -= dt;
      a.mesh.position.addScaledVector(a.dir, speed * dt);

      // Cover block: raycast each frame, if blocked, kill arrow without hit
      if (!a.blocked && hooks.cover && hooks.cover.blocksLine(a.origin, a.mesh.position)) {
        a.blocked = true;
        a.hit = true;        // mark consumed
        a.life = Math.min(a.life, 0.05);
      }

      // Tracer line update: from origin to current arrow position
      const lp = a.lineGeo.attributes.position.array;
      lp[0] = a.origin.x;
      lp[1] = a.origin.y;
      lp[2] = a.origin.z;
      lp[3] = a.mesh.position.x;
      lp[4] = a.mesh.position.y;
      lp[5] = a.mesh.position.z;
      a.lineGeo.attributes.position.needsUpdate = true;
      a.lineMat.opacity = Math.max(0, 0.95 * (a.life / 2.2));

      // ember trail update (every 2 frames ~ 30hz)
      a.tAcc += dt;
      if (a.tAcc > 0.033) {
        a.tAcc = 0;
        const p = a.trailGeo.attributes.position.array;
        const idx = a.tIdx % 12;
        p[idx * 3] = a.mesh.position.x - a.dir.x * 0.15;
        p[idx * 3 + 1] = a.mesh.position.y - a.dir.y * 0.15;
        p[idx * 3 + 2] = a.mesh.position.z - a.dir.z * 0.15;
        a.tIdx++;
        a.trailGeo.attributes.position.needsUpdate = true;
        a.trailMat.opacity = Math.max(0, 0.85 * (a.life / 2.2));
      }

      if (!a.hit) {
        for (const r of waves.alive) {
          if (r.isDead) continue;
          const dx = r.position.x - a.mesh.position.x;
          const dy = 1.0 - a.mesh.position.y;
          const dz = r.position.z - a.mesh.position.z;
          if (dx * dx + dy * dy + dz * dz < 0.55 * 0.55) {
            r.damage(dmg);
            a.hit = true;
            a.life = 0;
            hooks.onHit?.(r);
            break;
          }
        }
      }
      if (a.life <= 0) {
        scene.remove(a.mesh);
        a.mesh.geometry.dispose();
        a.mesh.material.dispose();
        scene.remove(a.trail);
        a.trailGeo.dispose();
        a.trailMat.dispose();
        scene.remove(a.tracer);
        a.lineGeo.dispose();
        a.lineMat.dispose();
        arrows.splice(i, 1);
      }
    }
  }

  return { update };
}
