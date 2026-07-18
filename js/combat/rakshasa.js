import * as THREE from 'three';

export function createRakshasa(scene, pos, hp = 3, opts = {}) {
  const speed = opts.speed ?? (2.4 + Math.random() * 0.8);
  const group = new THREE.Group();
  group.position.set(pos.x, 0, pos.z);

  const body = new THREE.Mesh(
    new THREE.CapsuleGeometry(0.4, 0.85, 4, 8),
    new THREE.MeshStandardMaterial({ color: 0x6b2a3a, roughness: 0.75 })
  );
  body.position.y = 1.0;
  body.castShadow = true;
  group.add(body);

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

  return {
    group,
    get position() { return group.position; },
    get isDead() { return dead; },
    get hp() { return current; },
    damage(n) {
      if (dead || n <= 0) return false;
      current = Math.max(0, current - n);
      body.material.emissive = new THREE.Color(0x550000);
      body.material.emissiveIntensity = 0.7;
      setTimeout(() => {
        if (!dead) body.material.emissiveIntensity = 0;
      }, 80);
      if (current === 0) {
        dead = true;
        body.visible = false;
        crest.visible = false;
        spawnParticles();
      }
      return true;
    },
    update(dt, playerPos) {
      if (dead) return false;
      attackCd = Math.max(0, attackCd - dt);
      const dx = playerPos.x - group.position.x;
      const dz = playerPos.z - group.position.z;
      const dist = Math.hypot(dx, dz) || 1;
      group.rotation.y = Math.atan2(dx, dz);

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
        const step = Math.min(speed * dt, dist - 1.1);
        group.position.x += (dx / dist) * step;
        group.position.z += (dz / dist) * step;
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
