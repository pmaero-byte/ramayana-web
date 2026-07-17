import * as THREE from 'three';

export function createWorld(canvas) {
  const renderer = new THREE.WebGLRenderer({ canvas, antialias: true, powerPreference: 'high-performance' });
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  renderer.setSize(window.innerWidth, window.innerHeight);
  renderer.shadowMap.enabled = true;

  const scene = new THREE.Scene();
  scene.background = new THREE.Color(0x140e18);
  scene.fog = new THREE.Fog(0x2a1810, 18, 85);

  const camera = new THREE.PerspectiveCamera(55, window.innerWidth / window.innerHeight, 0.1, 200);
  camera.position.set(0, 3.2, 7.5);

  const sun = new THREE.DirectionalLight(0xffe0b8, 1.35);
  sun.position.set(12, 22, 8);
  sun.castShadow = true;
  sun.shadow.mapSize.set(1024, 1024);
  scene.add(sun);
  scene.add(new THREE.AmbientLight(0x382820, 0.55));

  const ground = new THREE.Mesh(
    new THREE.PlaneGeometry(80, 80),
    new THREE.MeshStandardMaterial({ color: 0x332016, roughness: 0.92, metalness: 0.05 })
  );
  ground.rotation.x = -Math.PI / 2;
  ground.receiveShadow = true;
  scene.add(ground);

  // Simple Lanka ruins blocks for depth
  const mat = new THREE.MeshStandardMaterial({ color: 0x4a3020, roughness: 0.9 });
  for (let i = 0; i < 8; i++) {
    const h = 1.2 + Math.random() * 2.5;
    const box = new THREE.Mesh(new THREE.BoxGeometry(1.2 + Math.random(), h, 1.2 + Math.random()), mat);
    const a = (i / 8) * Math.PI * 2;
    box.position.set(Math.cos(a) * 14, h / 2, Math.sin(a) * 14);
    box.castShadow = true;
    scene.add(box);
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
