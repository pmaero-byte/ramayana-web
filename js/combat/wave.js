import * as THREE from 'three';
import { createRakshasa } from './rakshasa.js';
import { spawnPoints, kindForWave } from './formation.js';

export function createWaveController(scene, player, onWave, onAllDone, onMelee) {
  let alive = [];
  let wave = 0;
  let total = 3;
  let running = false;
  let pause = 0;

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
    const pts = spawnPoints(kind, 3 + wave, origin, forward, 6.5);
    alive = pts.map((p, i) => createRakshasa(scene, p, 2 + wave));
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

/** Kinematic arrows */
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
    const to = new THREE.Vector3(
      target.position.x - origin.x,
      0.9 - origin.y + 1,
      target.position.z - origin.z
    ).normalize();

    const mesh = new THREE.Mesh(
      new THREE.CylinderGeometry(0.04, 0.04, 0.7, 6),
      new THREE.MeshStandardMaterial({ color: 0xd9b44a, metalness: 0.4, roughness: 0.4 })
    );
    mesh.position.copy(origin);
    mesh.quaternion.setFromUnitVectors(new THREE.Vector3(0, 1, 0), to);
    scene.add(mesh);
    arrows.push({ mesh, dir: to, life: 2.2, hit: false });
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
        arrows.splice(i, 1);
      }
    }
  }

  return { update };
}
