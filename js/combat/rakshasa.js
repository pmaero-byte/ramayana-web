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

  // crude horn / crest so they read as enemies, not twin heroes
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
      }
      return true;
    },
    /** Chase player; returns true if contact-damage applied this frame. */
    update(dt, playerPos) {
      if (dead) return false;
      attackCd = Math.max(0, attackCd - dt);
      const dx = playerPos.x - group.position.x;
      const dz = playerPos.z - group.position.z;
      const dist = Math.hypot(dx, dz) || 1;
      // face player
      group.rotation.y = Math.atan2(dx, dz);
      // stop short of body overlap
      if (dist > 1.15) {
        const step = Math.min(speed * dt, dist - 1.1);
        group.position.x += (dx / dist) * step;
        group.position.z += (dz / dist) * step;
      } else if (attackCd <= 0) {
        attackCd = 0.9;
        return true; // melee tick
      }
      // bob
      body.position.y = 1.0 + Math.sin(performance.now() * 0.008 + group.id) * 0.04;
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
}
