import * as THREE from 'three';

/**
 * Cinematic third-person rig — over-the-shoulder lean, FOV punch on sprint,
 * shake on hit, slow-motion on kill. GTA-style camera drama on capsules.
 */
export function createCameraRig(camera) {
  const offset = new THREE.Vector3(0, 2.35, -7.2);
  const look = new THREE.Vector3(0, 1.15, 0);
  const pos = new THREE.Vector3();
  const lookAt = new THREE.Vector3();

  // Camera state
  const baseFov = 55;
  const baseOffset = offset.clone();
  let shake = 0;
  let fovBoost = 0;       // temporary FOV widening (sprint)
  let lean = new THREE.Vector2(0, 0); // shoulder offset lean
  let hitFlash = 0;       // red vignette
  let killFreeze = 0;     // bullet-time timer
  let timeScale = 1;      // slow-mo multiplier

  function shakeAdd(amount) { shake = Math.min(shake + amount, 1.0); }
  function punchFov(amount) { fovBoost = Math.min(fovBoost + amount, 14); }
  function killHit() { killFreeze = 0.22; punchFov(8); shakeAdd(0.6); }
  function damageHit() { shakeAdd(0.35); hitFlash = 0.18; }

  function update(dt, target, yaw, pitch, opts = {}) {
    const speed = opts.moveSpeed || 0; // 0..1
    const isSprinting = opts.sprinting || false;

    // Over-the-shoulder lean: shift camera right when moving right, slightly down
    const leanTargetX = 0.35 + speed * 0.25; // always slightly right shoulder
    const leanTargetY = -0.15 - (isSprinting ? 0.25 : 0);
    lean.x = THREE.MathUtils.damp(lean.x, leanTargetX, 8, dt);
    lean.y = THREE.MathUtils.damp(lean.y, leanTargetY, 8, dt);

    // FOV punch on sprint
    const targetFov = baseFov + (isSprinting ? 10 : 0) + fovBoost;
    camera.fov = THREE.MathUtils.damp(camera.fov, targetFov, 5, dt);
    camera.updateProjectionMatrix();
    fovBoost = Math.max(0, fovBoost - dt * 18);

    const q = new THREE.Quaternion().setFromEuler(new THREE.Euler(pitch, yaw, 0, 'YXZ'));
    const right = new THREE.Vector3(1, 0, 0).applyQuaternion(q);

    // Time-scale slow-mo on kill
    const ts = killFreeze > 0 ? 0.18 : 1;
    if (killFreeze > 0) killFreeze = Math.max(0, killFreeze - dt);
    timeScale = ts;

    // Apply lean offset (camera right & slightly down)
    const desired = offset.clone()
      .addScaledVector(right, lean.x)
      .add(new THREE.Vector3(0, lean.y, 0))
      .applyQuaternion(q)
      .add(target.position);

    // Shake (decaying random offset, applied AFTER position calc)
    if (shake > 0) {
      desired.x += (Math.random() - 0.5) * shake * 0.35;
      desired.y += (Math.random() - 0.5) * shake * 0.25;
      desired.z += (Math.random() - 0.5) * shake * 0.35;
      shake = Math.max(0, shake - dt * 2.5);
    }

    pos.lerp(desired, 1 - Math.exp(-6 * dt));
    lookAt.copy(target.position).add(look);
    camera.position.copy(pos);

    // Look slightly ahead of player motion for cinematic drift
    const lookAhead = new THREE.Vector3(Math.sin(yaw) * 0.6, 0, Math.cos(yaw) * 0.6).multiplyScalar(speed * 0.6);
    lookAt.add(lookAhead);
    camera.lookAt(lookAt);

    // Hit flash: tint via small DOM overlay (cheap cinematic)
    if (hitFlash > 0) {
      const el = document.getElementById('hit-flash');
      if (el) {
        el.style.background = 'radial-gradient(circle at 50% 45%, rgba(255,40,30,0.45), transparent 60%)';
        el.style.opacity = String(hitFlash * 4);
      }
      hitFlash = Math.max(0, hitFlash - dt);
    } else {
      const el = document.getElementById('hit-flash');
      if (el && el.style.background.indexOf('255,40,30') > -1) {
        el.style.background = 'radial-gradient(circle at 50% 40%, rgba(255, 180, 60, 0.35), transparent 55%)';
        el.style.opacity = '0';
      }
    }
  }

  function setMoveSpeed(s) { /* stored via opts in update */ }
  function getTimeScale() { return timeScale; }

  return {
    update,
    shake: shakeAdd,
    punchFov,
    killHit,
    damageHit,
    getTimeScale,
  };
}
