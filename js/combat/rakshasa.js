import * as THREE from 'three';

export function createRakshasa(scene, pos, hp = 3, opts = {}) {
  const speed = opts.speed ?? (2.4 + Math.random() * 0.8);
  const cover = opts.cover || null;
  // Per-wave scaling: wave 1 small grunt, wave 3 boss-tier
  // Boss-tier scaling: hp >= 6 → larger mesh + dark red glow aura
  const wave = opts.wave ?? 1;
  const isBoss = hp >= 6;
  const baseScale = isBoss ? 1.45 : (wave === 1 ? 0.78 : wave === 2 ? 1.0 : 1.0);
  const scale = baseScale;
  const baseColor = isBoss ? 0x4a0a14 : 0x6b2a3a;
  const headColor = isBoss ? 0x2a0610 : 0x4a1825;
  const group = new THREE.Group();
  group.position.set(pos.x, 0, pos.z);
  group.scale.setScalar(scale);

  const body = new THREE.Mesh(
    new THREE.CapsuleGeometry(0.4, 0.85, 4, 8),
    new THREE.MeshStandardMaterial({ color: baseColor, roughness: 0.75 })
  );
  body.position.y = 1.0;
  body.castShadow = true;
  group.add(body);

  // Head (rakshasa: demonic)
  const head = new THREE.Mesh(
    new THREE.SphereGeometry(0.26, 12, 10),
    new THREE.MeshStandardMaterial({ color: headColor, roughness: 0.7 })
  );
  head.position.y = 1.72;
  head.castShadow = true;
  group.add(head);

  // Horns (two cones)
  const hornMat = new THREE.MeshStandardMaterial({ color: 0x1a0608, roughness: 0.6 });
  const hornL = new THREE.Mesh(new THREE.ConeGeometry(0.06, 0.3, 6), hornMat);
  hornL.position.set(-0.14, 1.92, 0);
  hornL.rotation.z = 0.35;
  group.add(hornL);
  const hornR = hornL.clone();
  hornR.position.x = 0.14;
  hornR.rotation.z = -0.35;
  group.add(hornR);

  // Fangs (two tiny cones in front of mouth)
  const fangMat = new THREE.MeshStandardMaterial({ color: 0xfff4d0, roughness: 0.5 });
  const fangL = new THREE.Mesh(new THREE.ConeGeometry(0.025, 0.08, 4), fangMat);
  fangL.position.set(-0.06, 1.6, 0.22);
  fangL.rotation.x = Math.PI;
  group.add(fangL);
  const fangR = fangL.clone();
  fangR.position.x = 0.06;
  group.add(fangR);

  // Eyes (two small spheres — emissive red)
  const eyeMat = new THREE.MeshStandardMaterial({
    color: 0xff3000,
    emissive: 0xff2000,
    emissiveIntensity: 1.0,
    roughness: 0.3,
  });
  const eyeL = new THREE.Mesh(new THREE.SphereGeometry(0.05, 6, 6), eyeMat);
  eyeL.position.set(-0.09, 1.78, 0.21);
  group.add(eyeL);
  const eyeR = eyeL.clone();
  eyeR.position.x = 0.09;
  group.add(eyeR);

  // Claws (two pairs of tiny cones as hands)
  const clawMat = new THREE.MeshStandardMaterial({ color: 0xfff4d0, roughness: 0.4 });
  const clawFrontL = new THREE.Mesh(new THREE.ConeGeometry(0.04, 0.12, 4), clawMat);
  clawFrontL.position.set(-0.45, 0.95, 0.18);
  clawFrontL.rotation.z = Math.PI / 2;
  clawFrontL.rotation.x = -0.4;
  group.add(clawFrontL);
  const clawFrontR = clawFrontL.clone();
  clawFrontR.position.x = 0.45;
  group.add(clawFrontR);

  // Boss aura: glowing red point cloud at feet (signals threat)
  let auraMat = null;
  let auraPts = null;
  let auraBaseSize = 0.16;
  if (isBoss) {
    const auraCount = 32;
    const auraGeo = new THREE.BufferGeometry();
    const auraPos = new Float32Array(auraCount * 3);
    for (let i = 0; i < auraCount; i++) {
      const angle = Math.random() * Math.PI * 2;
      const radius = 0.5 + Math.random() * 0.6;
      auraPos[i * 3] = Math.cos(angle) * radius;
      auraPos[i * 3 + 1] = Math.random() * 0.15;
      auraPos[i * 3 + 2] = Math.sin(angle) * radius;
    }
    auraGeo.setAttribute('position', new THREE.BufferAttribute(auraPos, 3));
    auraMat = new THREE.PointsMaterial({
      color: 0xff2200,
      size: auraBaseSize,
      transparent: true,
      opacity: 0.7,
      blending: THREE.AdditiveBlending,
      depthWrite: false,
    });
    auraPts = new THREE.Points(auraGeo, auraMat);
    group.add(auraPts);
  }

  // HP bar (always faces camera by being added to scene with manual billboard in update)
  const barBg = new THREE.Mesh(
    new THREE.PlaneGeometry(1.2, 0.16),
    new THREE.MeshBasicMaterial({ color: 0x111111, transparent: true, opacity: 0.9, depthTest: false })
  );
  barBg.position.y = 2.4;
  barBg.renderOrder = 10;
  group.add(barBg);
  const barFill = new THREE.Mesh(
    new THREE.PlaneGeometry(1.12, 0.1),
    new THREE.MeshBasicMaterial({ color: 0xff3030, depthTest: false })
  );
  barFill.position.z = 0.001;
  barFill.renderOrder = 11;
  barBg.add(barFill);

  const crest = new THREE.Mesh(
    new THREE.ConeGeometry(0.18, 0.4, 6),
    new THREE.MeshStandardMaterial({ color: 0x2a1018, roughness: 0.8 })
  );
  crest.position.set(0, 1.75, 0);
  group.add(crest);

  scene.add(group);

  let current = hp;
  let dead = false;
  let attackCd = 0;
  const rId = Math.random();
  // Idle wander when player is far (GTA-style patrol)
  let wanderT = Math.random() * 2;
  let wanderAng = Math.random() * Math.PI * 2;
  const homeX = pos.x;
  const homeZ = pos.z;
  let growlCd = 0.4 + Math.random() * 1.2;
  const onGrowl = opts.onGrowl || null;

  return {
    group,
    get position() { return group.position; },
    get isDead() { return dead; },
    get hp() { return current; },
    get isBoss() { return isBoss; },
    damage(n) {
      if (dead || n <= 0) return false;
      current = Math.max(0, current - n);
      const pct = Math.max(0, current / hp);
      barFill.scale.x = pct;
      barFill.position.x = -(1 - pct) * 1.12 / 2;
      // Color shift: red -> orange as HP drops
      const r = 1.0;
      const g = pct * 0.6;
      barFill.material.color.setRGB(r, g, 0);
      body.material.emissive = new THREE.Color(0xffaa00);
      body.material.emissiveIntensity = 1.2;
      body.scale.set(1.18, 0.86, 1.18);
      // Boss low-HP telegraph: aura swells + brightens under 40%
      if (isBoss && auraMat) {
        const enrage = pct < 0.4;
        auraMat.opacity = enrage ? 1.0 : 0.7;
        auraMat.size = enrage ? auraBaseSize * (1.6 + (1 - pct) * 1.2) : auraBaseSize;
        auraMat.color.setHex(enrage ? 0xff4400 : 0xff2200);
        if (enrage) {
          body.material.emissive = new THREE.Color(0xff2200);
          body.material.emissiveIntensity = 0.55 + (1 - pct) * 0.9;
        }
      }
      setTimeout(() => {
        if (!dead) {
          if (!(isBoss && auraMat && current / hp < 0.4)) {
            body.material.emissiveIntensity = 0;
          }
          body.scale.set(1, 1, 1);
        }
      }, 100);
      if (current === 0) {
        dead = true;
        body.visible = false;
        crest.visible = false;
        barBg.visible = false;
        if (auraPts) auraPts.visible = false;
        spawnParticles();
      }
      return true;
    },
    update(dt, playerPos) {
      if (dead) return false;
      attackCd = Math.max(0, attackCd - dt);
      growlCd = Math.max(0, growlCd - dt);
      const dx = playerPos.x - group.position.x;
      const dz = playerPos.z - group.position.z;
      const dist = Math.hypot(dx, dz) || 1;
      group.rotation.y = Math.atan2(dx, dz);
      // Counter-rotate HP bar so it always faces camera (simple billboard)
      barBg.rotation.y = -Math.atan2(dx, dz);

      // Close-range chatter — growl when within ~2.4u
      if (dist < 2.4 && growlCd <= 0) {
        onGrowl?.();
        growlCd = 1.1 + Math.random() * 1.6;
      }

      // Boss enrage pulse: aura size breathes when low HP
      if (isBoss && auraMat && current > 0) {
        const pct = current / hp;
        if (pct < 0.4) {
          const pulse = 0.85 + Math.sin(performance.now() * 0.012 + rId) * 0.25;
          auraMat.size = auraBaseSize * (1.6 + (1 - pct) * 1.2) * pulse;
          auraMat.opacity = 0.75 + pulse * 0.2;
        }
      }

      // Squash-and-stretch + forward lean into chase direction (GTA-style body language)
      const moving = dist > 1.15;
      if (moving) {
        const nx = dx / dist;
        const nz = dz / dist;
        // lean forward in chase direction
        body.position.x = nx * 0.08;
        body.position.z = nz * 0.08;
        // squash: stretch taller when running, squash when stopping
        const runT = Math.min(1, speed / 3.2);
        body.scale.set(0.9 + runT * 0.1, 1.1 + runT * 0.15, 0.9 + runT * 0.1);
        crest.position.set(nx * 0.08, 1.75, nz * 0.08);
        crest.rotation.z = Math.sin(performance.now() * 0.012 + rId) * 0.15;
      } else if (attackCd <= 0) {
        // wind-up crouch before melee
        body.scale.set(1.15, 0.78, 1.15);
        body.position.y = 0.85;
        attackCd = 0.9;
        return true;
      } else {
        // recover
        body.position.x = THREE.MathUtils.damp(body.position.x, 0, 10, dt);
        body.position.z = THREE.MathUtils.damp(body.position.z, 0, 10, dt);
        body.position.y = THREE.MathUtils.damp(body.position.y, 1.0, 8, dt);
        body.scale.x = THREE.MathUtils.damp(body.scale.x, 1, 8, dt);
        body.scale.y = THREE.MathUtils.damp(body.scale.y, 1, 8, dt);
        body.scale.z = THREE.MathUtils.damp(body.scale.z, 1, 8, dt);
        crest.rotation.z = THREE.MathUtils.damp(crest.rotation.z, 0, 8, dt);
      }

      if (dist > 1.15) {
        // Chase when close; idle wander when player is far away
        const aggro = dist < 14;
        let nx, nz, step;
        if (aggro) {
          nx = dx / dist;
          nz = dz / dist;
          if (cover && cover.blocksLine(group.position, playerPos)) {
            const phase = rId * 6.28;
            const arc = Math.sin(performance.now() * 0.6 + phase) * 0.6;
            const cs = Math.cos(arc), sn = Math.sin(arc);
            const rx = nx * cs - nz * sn;
            const rz = nx * sn + nz * cs;
            nx = rx; nz = rz;
          }
          step = Math.min(speed * dt, dist - 1.1);
        } else {
          // Patrol around spawn home — slow drift + occasional re-aim
          wanderT -= dt;
          if (wanderT <= 0) {
            wanderT = 1.4 + Math.random() * 2.2;
            // bias back toward home so they don't leave the arena
            const hx = homeX - group.position.x;
            const hz = homeZ - group.position.z;
            const hlen = Math.hypot(hx, hz) || 1;
            if (hlen > 4.5) {
              wanderAng = Math.atan2(hx, hz) + (Math.random() - 0.5) * 0.8;
            } else {
              wanderAng += (Math.random() - 0.5) * 1.8;
            }
          }
          nx = Math.sin(wanderAng);
          nz = Math.cos(wanderAng);
          step = speed * 0.35 * dt;
          group.rotation.y = THREE.MathUtils.damp(group.rotation.y, wanderAng, 4, dt);
        }
        group.position.x += nx * step;
        group.position.z += nz * step;
      }

      // bob while moving (keeps previous alive-y feel)
      body.position.y = (body.position.y || 1.0) + Math.sin(performance.now() * 0.008 + rId * 100) * 0.04;
      return false;
    },
    dispose() {
      scene.remove(group);
      body.geometry.dispose();
      body.material.dispose();
      crest.geometry.dispose();
      crest.material.dispose();
    },
  };

  function spawnParticles() {
    const count = 8 + Math.floor(Math.random() * 6);
    const geo = new THREE.BufferGeometry();
    const pos = new Float32Array(count * 3);
    const vel = [];
    const colors = [0x8b2010, 0xc04020, 0xf08040, 0xf0b050];
    for (let i = 0; i < count; i++) {
      pos[i * 3] = group.position.x + (Math.random() - 0.5) * 0.4;
      pos[i * 3 + 1] = 0.8 + Math.random() * 0.6;
      pos[i * 3 + 2] = group.position.z + (Math.random() - 0.5) * 0.4;
      vel.push({
        x: (Math.random() - 0.5) * 6,
        y: 3 + Math.random() * 4,
        z: (Math.random() - 0.5) * 6,
      });
    }
    geo.setAttribute('position', new THREE.BufferAttribute(pos, 3));
    const mat = new THREE.PointsMaterial({
      color: colors[Math.floor(Math.random() * colors.length)],
      size: 0.2,
      transparent: true,
    });
    const pts = new THREE.Points(geo, mat);
    scene.add(pts);
    let life = 0.7;
    const tick = () => {
      life -= 0.03;
      if (life <= 0) {
        scene.remove(pts);
        geo.dispose();
        mat.dispose();
        return;
      }
      const p = pts.geometry.attributes.position.array;
      for (let i = 0; i < count; i++) {
        p[i * 3] += vel[i].x * 0.03;
        p[i * 3 + 1] += vel[i].y * 0.03 - 0.08;
        p[i * 3 + 2] += vel[i].z * 0.03;
      }
      pts.geometry.attributes.position.needsUpdate = true;
      mat.opacity = Math.max(0, life / 0.7);
      requestAnimationFrame(tick);
    };
    tick();
  }
}
