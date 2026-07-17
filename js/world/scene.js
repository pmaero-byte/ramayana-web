import * as THREE from 'three';

export function createWorld(canvas) {
  const renderer = new THREE.WebGLRenderer({ canvas, antialias: true, powerPreference: 'high-performance' });
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  renderer.setSize(window.innerWidth, window.innerHeight);
  renderer.shadowMap.enabled = true;

  const scene = new THREE.Scene();
  scene.background = new THREE.Color(0x140e18);
  scene.fog = new THREE.Fog(0x2a1810, 16, 70);

  const camera = new THREE.PerspectiveCamera(55, window.innerWidth / window.innerHeight, 0.1, 200);
  camera.position.set(0, 3.2, -7.5);

  const sun = new THREE.DirectionalLight(0xffe0b8, 1.35);
  sun.position.set(12, 22, 8);
  sun.castShadow = true;
  sun.shadow.mapSize.set(1024, 1024);
  sun.shadow.camera.near = 1;
  sun.shadow.camera.far = 60;
  sun.shadow.camera.left = -25;
  sun.shadow.camera.right = 25;
  sun.shadow.camera.top = 25;
  sun.shadow.camera.bottom = -25;
  scene.add(sun);
  scene.add(new THREE.AmbientLight(0x382820, 0.55));
  scene.add(new THREE.HemisphereLight(0x4a3050, 0x2a1808, 0.35));

  const ground = new THREE.Mesh(
    new THREE.CircleGeometry(40, 48),
    new THREE.MeshStandardMaterial({ color: 0x2c1a10, roughness: 0.95, metalness: 0.05 })
  );
  ground.rotation.x = -Math.PI / 2;
  ground.receiveShadow = true;
  scene.add(ground);

  // battle ring
  const ring = new THREE.Mesh(
    new THREE.RingGeometry(11.5, 12.2, 48),
    new THREE.MeshStandardMaterial({ color: 0x8a6030, roughness: 0.8, side: THREE.DoubleSide })
  );
  ring.rotation.x = -Math.PI / 2;
  ring.position.y = 0.02;
  scene.add(ring);

  const stone = new THREE.MeshStandardMaterial({ color: 0x4a3020, roughness: 0.9 });
  const gold = new THREE.MeshStandardMaterial({ color: 0xb8892a, roughness: 0.55, metalness: 0.35 });

  // ruined pillars around arena
  for (let i = 0; i < 10; i++) {
    const a = (i / 10) * Math.PI * 2 + 0.2;
    const r = 13 + (i % 3) * 0.6;
    const h = 1.6 + (i % 4) * 0.7;
    const col = new THREE.Mesh(new THREE.CylinderGeometry(0.35, 0.45, h, 8), stone);
    col.position.set(Math.cos(a) * r, h / 2, Math.sin(a) * r);
    col.castShadow = true;
    col.receiveShadow = true;
    scene.add(col);
    if (i % 2 === 0) {
      const cap = new THREE.Mesh(new THREE.BoxGeometry(1.0, 0.25, 1.0), gold);
      cap.position.set(col.position.x, h + 0.1, col.position.z);
      scene.add(cap);
    }
  }

  // central shattered plinth
  const plinth = new THREE.Mesh(new THREE.CylinderGeometry(1.8, 2.2, 0.5, 8), stone);
  plinth.position.set(0, 0.25, 0);
  plinth.receiveShadow = true;
  scene.add(plinth);

  // ember point lights (fake torches)
  for (let i = 0; i < 4; i++) {
    const a = (i / 4) * Math.PI * 2 + Math.PI / 4;
    const light = new THREE.PointLight(0xff6622, 0.55, 14, 2);
    light.position.set(Math.cos(a) * 10, 2.2, Math.sin(a) * 10);
    scene.add(light);
    const flame = new THREE.Mesh(
      new THREE.SphereGeometry(0.15, 8, 8),
      new THREE.MeshBasicMaterial({ color: 0xff8844 })
    );
    flame.position.copy(light.position);
    scene.add(flame);
  }

  function onResize() {
    const w = window.innerWidth;
    const h = window.innerHeight;
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
    renderer.setSize(w, h);
  }
  window.addEventListener('resize', onResize);

  return { renderer, scene, camera, ground };
}
