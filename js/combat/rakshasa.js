import * as THREE from 'three';

export function createRakshasa(scene, pos, hp = 3) {
  const group = new THREE.Group();
  group.position.set(pos.x, 0, pos.z);
  const mesh = new THREE.Mesh(
    new THREE.CapsuleGeometry(0.4, 0.85, 4, 8),
    new THREE.MeshStandardMaterial({ color: 0x6b2a3a, roughness: 0.75 })
  );
  mesh.position.y = 1.0;
  mesh.castShadow = true;
  group.add(mesh);
  scene.add(group);

  let current = hp;
  let dead = false;

  return {
    group,
    get position() { return group.position; },
    get isDead() { return dead; },
    get hp() { return current; },
    damage(n) {
      if (dead || n <= 0) return;
      current = Math.max(0, current - n);
      mesh.material.emissive = new THREE.Color(0x440000);
      mesh.material.emissiveIntensity = 0.6;
      setTimeout(() => {
        if (!dead) mesh.material.emissiveIntensity = 0;
      }, 80);
      if (current === 0) {
        dead = true;
        mesh.visible = false;
      }
    },
    dispose() {
      scene.remove(group);
      mesh.geometry.dispose();
      mesh.material.dispose();
    },
  };
}
