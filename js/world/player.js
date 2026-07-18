import * as THREE from 'three';

const SKIN_DEFAULT = 0xf2c86a;

export function createPlayer(scene) {
  const group = new THREE.Group();
  group.position.set(0, 0, 0);

  const body = new THREE.Mesh(
    new THREE.CapsuleGeometry(0.35, 0.9, 4, 8),
    new THREE.MeshStandardMaterial({ color: SKIN_DEFAULT, roughness: 0.55, metalness: 0.1 })
  );
  body.position.y = 1.0;
  body.castShadow = true;
  group.add(body);

  // Head
  const head = new THREE.Mesh(
    new THREE.SphereGeometry(0.22, 12, 10),
    new THREE.MeshStandardMaterial({ color: 0xe8b888, roughness: 0.6 })
  );
  head.position.y = 1.7;
  head.castShadow = true;
  group.add(head);

  // Hair / crown cap
  const hair = new THREE.Mesh(
    new THREE.SphereGeometry(0.24, 12, 8, 0, Math.PI * 2, 0, Math.PI * 0.55),
    new THREE.MeshStandardMaterial({ color: 0x1a0e06, roughness: 0.85 })
  );
  hair.position.y = 1.7;
  group.add(hair);

  // Crown band (gold)
  const crown = new THREE.Mesh(
    new THREE.CylinderGeometry(0.235, 0.235, 0.08, 16),
    new THREE.MeshStandardMaterial({ color: 0xd4a017, roughness: 0.3, metalness: 0.7 })
  );
  crown.position.y = 1.78;
  group.add(crown);

  // Legs (two short cylinders)
  const legMat = new THREE.MeshStandardMaterial({ color: 0xc89432, roughness: 0.7 });
  const legL = new THREE.Mesh(new THREE.CylinderGeometry(0.12, 0.14, 0.55, 6), legMat);
  legL.position.set(-0.14, 0.28, 0);
  legL.castShadow = true;
  group.add(legL);
  const legR = legL.clone();
  legR.position.x = 0.14;
  group.add(legR);

  // Boots
  const bootMat = new THREE.MeshStandardMaterial({ color: 0x3a1f0a, roughness: 0.8 });
  const bootL = new THREE.Mesh(new THREE.BoxGeometry(0.28, 0.12, 0.36), bootMat);
  bootL.position.set(-0.14, 0.06, 0.04);
  group.add(bootL);
  const bootR = bootL.clone();
  bootR.position.x = 0.14;
  group.add(bootR);

  const bow = new THREE.Mesh(
    new THREE.CylinderGeometry(0.03, 0.03, 0.9, 6),
    new THREE.MeshStandardMaterial({ color: 0x8b5a2b })
  );
  bow.position.set(0.35, 1.05, 0.1);
  bow.rotation.z = 0.4;
  group.add(bow);

  const quiver = new THREE.Mesh(
    new THREE.CylinderGeometry(0.06, 0.06, 0.5, 6),
    new THREE.MeshStandardMaterial({ color: 0x4a2810 })
  );
  quiver.position.set(-0.32, 1.25, -0.05);
  quiver.rotation.z = -0.15;
  group.add(quiver);

  // ground shadow disc for cheap grounding
  const shadow = new THREE.Mesh(
    new THREE.CircleGeometry(0.6, 24),
    new THREE.MeshBasicMaterial({ color: 0x000000, transparent: true, opacity: 0.35 })
  );
  shadow.rotation.x = -Math.PI / 2;
  shadow.position.y = 0.02;
  group.add(shadow);

  scene.add(group);

  const vel = new THREE.Vector3();
  let yVel = 0;
  let grounded = true;
  let locked = false;
  let iFrames = 0;
  let invulnerableUntil = 0;
  let charColor = SKIN_DEFAULT;
  let lastMoveSpeed = 0;
  let fallT = 0;
  const walk = 4.2;
  const run = 7.4;
  const gravity = 28;

  function update(dt, input, cameraYaw) {
    iFrames = Math.max(0, iFrames - dt);
    if (locked) {
      vel.set(0, 0, 0);
      // Death fall animation: tip backward over 0.6s, then stay
      if (fallT < 0.6) {
        fallT += dt;
        const p = Math.min(1, fallT / 0.6);
        const ease = p * p * (3 - 2 * p); // smoothstep
        group.rotation.x = ease * (Math.PI / 2.4); // tip backward
        body.position.y = -ease * 0.4; // sink slightly
        body.material.opacity = 1 - ease * 0.5;
        body.material.transparent = true;
      }
      return;
    }
    const mv = input.moveVector();
    const speed = input.run ? run : walk;
    const sin = Math.sin(cameraYaw);
    const cos = Math.cos(cameraYaw);
    const fx = mv.x * cos + mv.z * sin;
    const fz = -mv.x * sin + mv.z * cos;
    const target = new THREE.Vector3(fx, 0, fz).multiplyScalar(speed * mv.mag);
    vel.x = THREE.MathUtils.damp(vel.x, target.x, 12, dt);
    vel.z = THREE.MathUtils.damp(vel.z, target.z, 12, dt);
    lastMoveSpeed = Math.hypot(vel.x, vel.z);

    if (input.jump && grounded) {
      yVel = 8.5;
      grounded = false;
    }
    yVel -= gravity * dt;
    group.position.x += vel.x * dt;
    group.position.z += vel.z * dt;
    group.position.y += yVel * dt;
    if (group.position.y <= 0) {
      group.position.y = 0;
      yVel = 0;
      grounded = true;
    }

    if (mv.mag > 0.05) {
      const face = Math.atan2(fx, fz);
      group.rotation.y = THREE.MathUtils.damp(group.rotation.y, face, 10, dt);
    }

    const lim = 28;
    group.position.x = THREE.MathUtils.clamp(group.position.x, -lim, lim);
    group.position.z = THREE.MathUtils.clamp(group.position.z, -lim, lim);

    // bob + blink
    body.position.y = 1.0 + (mv.mag > 0.05 ? Math.sin(performance.now() * 0.012) * 0.05 : 0);
    shadow.material.opacity = 0.35 - Math.min(0.2, group.position.y * 0.04);
    shadow.scale.setScalar(1 + Math.min(0.4, group.position.y * 0.06));
    body.material.opacity = iFrames > 0 && Math.floor(iFrames * 12) % 2 === 0 ? 0.35 : 1;
    body.material.transparent = iFrames > 0;
  }

  function reset() {
    group.position.set(0, 0, 0);
    group.rotation.y = 0;
    group.rotation.x = 0;
    group.rotation.z = 0;
    body.position.set(0, 0, 0);
    body.rotation.set(0, 0, 0);
    vel.set(0, 0, 0);
    yVel = 0;
    grounded = true;
    fallT = 0;
    locked = false;
    iFrames = 0;
    body.material.opacity = 1;
    body.material.transparent = false;
  }

  function setLocked(v) {
    locked = !!v;
    if (locked) vel.set(0, 0, 0);
  }

  function setCharacter(_id, color) {
    charColor = typeof color === 'string' ? parseHex(color) : (color || SKIN_DEFAULT);
    body.material.color = new THREE.Color(charColor);
  }
  /** @returns {boolean} true if damage applied */
  function hurt(amount = 1) {
    if (locked || iFrames > 0) return false;
    if (performance.now() < (invulnerableUntil || 0)) return false;
    iFrames = 0.85;
    body.material.emissive = new THREE.Color(0x661111);
    body.material.emissiveIntensity = 0.8;
    setTimeout(() => { body.material.emissiveIntensity = 0; }, 120);
    return true;
  }

  return {
    group,
    get position() { return group.position; },
    get forward() {
      return new THREE.Vector3(Math.sin(group.rotation.y), 0, Math.cos(group.rotation.y));
    },
    get invulnerable() { return iFrames > 0 || locked; },
    get moveSpeed() { return lastMoveSpeed; },
    update, reset, setLocked, hurt, setCharacter,
  };
}

function parseHex(s) {
  if (typeof s !== 'string') return SKIN_DEFAULT;
  const m = s.replace('#', '');
  if (m.length !== 6) return SKIN_DEFAULT;
  return parseInt(m, 16);
}
