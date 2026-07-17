import * as THREE from 'three';

export function createCameraRig(camera) {
  const offset = new THREE.Vector3(0, 2.35, -7.2);
  const look = new THREE.Vector3(0, 1.15, 0);
  const pos = new THREE.Vector3();
  const lookAt = new THREE.Vector3();

  function update(dt, target, yaw, pitch) {
    const q = new THREE.Quaternion().setFromEuler(new THREE.Euler(pitch, yaw, 0, 'YXZ'));
    const desired = offset.clone().applyQuaternion(q).add(target.position);
    pos.lerp(desired, 1 - Math.exp(-6 * dt));
    lookAt.copy(target.position).add(look);
    camera.position.copy(pos);
    camera.lookAt(lookAt);
  }

  return { update };
}
