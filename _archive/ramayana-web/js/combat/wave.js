import * as THREE from 'three';
import { createRakshasa } from './rakshasa.js?v=62';
import { spawnPoints, kindForWave } from './formation.js';

export function createWaveController(scene, player, onWave, onAllDone, onClear, onMelee, opts = {}) {
  let alive = [];
  let wave = 0;
  let total = 3;
  let running = false;
  let pause = 0;
  let prevAliveCount = 0;
  const cover = opts.cover || null;
  const pathfind = opts.pathfind !== false;
  const onGrowl = opts.onGrowl || null;

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
    alive = pts.map((p, i) => {
      // Wave 3 last enemy is the boss-tier (hp=8, scale 1.45)
      const isFinalBoss = wave === 3 && i === pts.length - 1;
      const hp = isFinalBoss ? 8 : 2 + wave;
      return createRakshasa(scene, p, hp, { cover, wave, onGrowl });
    });
    prevAliveCount = alive.length;
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
      if (prevAliveCount > 0) {
        onClear?.(wave, total, kind);
        prevAliveCount = 0;
      }
      pause += dt;
      if (pause > 3.2) {
        pause = 0;
        nextWave();
      }
    } else {
      prevAliveCount = alive.length;
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
  const sparks = [];

  // Expanding golden ground ring on hit (shockwave effect)
  const rings = [];
  function spawnRing(pos) {
    const geo = new THREE.RingGeometry(0.3, 0.4, 32);
    const mat = new THREE.MeshBasicMaterial({
      color: 0xffd060,
      transparent: true,
      opacity: 0.9,
      side: THREE.DoubleSide,
      blending: THREE.AdditiveBlending,
      depthWrite: false,
    });
    const ring = new THREE.Mesh(geo, mat);
    ring.position.set(pos.x, 0.05, pos.z);
    ring.rotation.x = -Math.PI / 2;
    scene.add(ring);
    rings.push({ ring, geo, mat, life: 0.5 });
  }
  function updateRings(dt) {
    for (let i = rings.length - 1; i >= 0; i--) {
      const r = rings[i];
      r.life -= dt;
      if (r.life <= 0) {
        scene.remove(r.ring);
        r.geo.dispose();
        r.mat.dispose();
        rings.splice(i, 1);
      } else {
        const p = 1 - r.life / 0.5;
        const scale = 1 + p * 4;
        r.ring.scale.set(scale, scale, scale);
        r.mat.opacity = 0.9 * (1 - p);
      }
    }
  }

  function spawnSparks(pos) {
    const count = 8;
    const geo = new THREE.BufferGeometry();
    const positions = new Float32Array(count * 3);
    const velocities = [];
    for (let i = 0; i < count; i++) {
      positions[i * 3] = pos.x;
      positions[i * 3 + 1] = pos.y;
      positions[i * 3 + 2] = pos.z;
      velocities.push({
        x: (Math.random() - 0.5) * 6,
        y: 2 + Math.random() * 4,
        z: (Math.random() - 0.5) * 6,
      });
    }
    geo.setAttribute('position', new THREE.BufferAttribute(positions, 3));
    const mat = new THREE.PointsMaterial({ color: 0xffe080, size: 0.18, transparent: true, opacity: 1 });
    const pts = new THREE.Points(geo, mat);
    scene.add(pts);
    sparks.push({ pts, geo, mat, velocities, life: 0.45 });
  }

  // Wood/stone chips when an arrow smacks cover
  function spawnCoverChips(pos) {
    const count = 12;
    const geo = new THREE.BufferGeometry();
    const positions = new Float32Array(count * 3);
    const velocities = [];
    for (let i = 0; i < count; i++) {
      positions[i * 3] = pos.x + (Math.random() - 0.5) * 0.2;
      positions[i * 3 + 1] = (pos.y || 0.8) + Math.random() * 0.3;
      positions[i * 3 + 2] = pos.z + (Math.random() - 0.5) * 0.2;
      velocities.push({
        x: (Math.random() - 0.5) * 5,
        y: 1.5 + Math.random() * 3.5,
        z: (Math.random() - 0.5) * 5,
      });
    }
    geo.setAttribute('position', new THREE.BufferAttribute(positions, 3));
    const mat = new THREE.PointsMaterial({
      color: 0xc4a06a,
      size: 0.14,
      transparent: true,
      opacity: 1,
      depthWrite: false,
    });
    const pts = new THREE.Points(geo, mat);
    scene.add(pts);
    sparks.push({ pts, geo, mat, velocities, life: 0.55 });
  }

  function updateSparks(dt) {
    for (let i = sparks.length - 1; i >= 0; i--) {
      const s = sparks[i];
      s.life -= dt;
      const arr = s.geo.attributes.position.array;
      for (let j = 0; j < s.velocities.length; j++) {
        const v = s.velocities[j];
        v.y -= 12 * dt; // gravity
        arr[j * 3] += v.x * dt;
        arr[j * 3 + 1] += v.y * dt;
        arr[j * 3 + 2] += v.z * dt;
      }
      s.geo.attributes.position.needsUpdate = true;
      s.mat.opacity = Math.max(0, s.life / 0.45);
      if (s.life <= 0) {
        scene.remove(s.pts);
        s.geo.dispose();
        s.mat.dispose();
        sparks.splice(i, 1);
      }
    }
  }

  // Floating damage numbers ("−1") — GTA-style hit confirm
  const dmgNums = [];
  function spawnDamageNumber(pos, amount) {
    const canvas = document.createElement('canvas');
    canvas.width = 128;
    canvas.height = 64;
    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, 128, 64);
    ctx.font = 'bold 42px system-ui, sans-serif';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.lineWidth = 6;
    ctx.strokeStyle = 'rgba(20,8,0,0.85)';
    ctx.fillStyle = '#ffe070';
    const label = '−' + String(amount);
    ctx.strokeText(label, 64, 32);
    ctx.fillText(label, 64, 32);
    const tex = new THREE.CanvasTexture(canvas);
    tex.needsUpdate = true;
    const mat = new THREE.SpriteMaterial({
      map: tex,
      transparent: true,
      depthWrite: false,
      depthTest: false,
      opacity: 1,
    });
    const spr = new THREE.Sprite(mat);
    spr.position.set(pos.x + (Math.random() - 0.5) * 0.35, pos.y + 0.4, pos.z + (Math.random() - 0.5) * 0.35);
    spr.scale.set(1.1, 0.55, 1);
    spr.renderOrder = 20;
    scene.add(spr);
    dmgNums.push({
      spr, mat, tex,
      life: 0.85,
      vy: 1.6 + Math.random() * 0.5,
      vx: (Math.random() - 0.5) * 0.6,
    });
  }
  function updateDamageNumbers(dt) {
    for (let i = dmgNums.length - 1; i >= 0; i--) {
      const d = dmgNums[i];
      d.life -= dt;
      if (d.life <= 0) {
        scene.remove(d.spr);
        d.mat.map?.dispose();
        d.mat.dispose();
        d.tex?.dispose();
        dmgNums.splice(i, 1);
      } else {
        const p = 1 - d.life / 0.85;
        d.spr.position.y += d.vy * dt;
        d.spr.position.x += d.vx * dt;
        d.mat.opacity = Math.max(0, 1 - p * p);
        const s = 1.1 + p * 0.35;
        d.spr.scale.set(s, s * 0.5, 1);
      }
    }
  }

  let cd = 0;
  const interval = 0.55;
  const speed = 22;
  const dmg = 1;

  // Shift-hold aim assist line: faint gold beam to nearest in-cone target
  let aimGeo = null;
  let aimMat = null;
  let aimLine = null;
  function ensureAimLine() {
    if (aimLine) return;
    aimGeo = new THREE.BufferGeometry();
    aimGeo.setAttribute('position', new THREE.BufferAttribute(new Float32Array(6), 3));
    aimMat = new THREE.LineBasicMaterial({
      color: 0xffe080,
      transparent: true,
      opacity: 0.55,
      depthWrite: false,
    });
    aimLine = new THREE.Line(aimGeo, aimMat);
    aimLine.renderOrder = 5;
    aimLine.visible = false;
    scene.add(aimLine);
  }
  function updateAimLine() {
    const aiming = !!hooks.aiming?.();
    if (!aiming) {
      if (aimLine) aimLine.visible = false;
      return;
    }
    ensureAimLine();
    const origin = player.position.clone();
    origin.y = 1.25;
    const target = waves.findTarget(origin, player.forward, 14, 70);
    if (!target) {
      aimLine.visible = false;
      return;
    }
    const arr = aimGeo.attributes.position.array;
    arr[0] = origin.x; arr[1] = origin.y; arr[2] = origin.z;
    arr[3] = target.position.x;
    arr[4] = 1.1;
    arr[5] = target.position.z;
    aimGeo.attributes.position.needsUpdate = true;
    // pulse opacity while held
    aimMat.opacity = 0.4 + Math.sin(performance.now() * 0.008) * 0.2;
    aimLine.visible = true;
  }

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
      0,
      target.position.z - origin.z
    ).normalize();
    // Keep a slight loft so the shaft reads above ground, without breaking hit tests
    to.y = 0.02;
    to.normalize();

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
      // Only fire if there's actually a target in cone (12u, 60°).
      // Stops the "constant attack" feel when there's nothing to shoot at —
      // the bow goes quiet when no rakshasas are facing Rama.
      const origin = player.position.clone();
      origin.y = 1.2;
      const tgt = waves.findTarget(origin, player.forward);
      if (tgt) fire();
      cd = interval;
    }
    updateSparks(dt);
    updateRings(dt);
    updateDamageNumbers(dt);
    updateAimLine();
    for (let i = arrows.length - 1; i >= 0; i--) {
      const a = arrows[i];
      a.life -= dt;
      a.mesh.position.addScaledVector(a.dir, speed * dt);

      // Cover block: only after the arrow has left the muzzle (≥1.6u).
      // Near-origin props used to hard-block every shot at spawn.
      if (!a.blocked && hooks.cover) {
        const tdx = a.mesh.position.x - a.origin.x;
        const tdz = a.mesh.position.z - a.origin.z;
        const traveled2 = tdx * tdx + tdz * tdz;
        if (traveled2 > 2.56) {
          const hit = hooks.cover.hitTest
            ? hooks.cover.hitTest(a.origin, a.mesh.position)
            : (hooks.cover.blocksLine(a.origin, a.mesh.position) ? { point: a.mesh.position } : null);
          if (hit) {
            a.blocked = true;
            a.hit = true;
            a.life = Math.min(a.life, 0.05);
            const p = hit.point || a.mesh.position;
            spawnCoverChips(p);
            spawnSparks(p);
            hit.item?.hit?.(1.4);
            hooks.onCoverHit?.(p, hit.item);
          }
        }
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
          // XZ proximity — Y loft used to make 3D sphere tests always miss
          const dx = r.position.x - a.mesh.position.x;
          const dz = r.position.z - a.mesh.position.z;
          const dy = Math.abs((r.position.y + 1.0) - a.mesh.position.y);
          if (dx * dx + dz * dz < 0.75 * 0.75 && dy < 1.6) {
            r.damage(dmg);
            a.hit = true;
            a.life = 0;
            spawnSparks(a.mesh.position);
            spawnRing(a.mesh.position);
            spawnDamageNumber(a.mesh.position, dmg);
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
