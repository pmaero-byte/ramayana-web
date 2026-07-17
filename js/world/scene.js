import * as THREE from 'three';

const ARENA_STYLES = {
  'bala-birth': { ground: 0x3a5a7a, fog: 0x1a2840, ring: 0x6a8ab0, pillar: 0x4a6a8a, gold: 0xa0c8e0, light: 0x88bbff },
  'ayodhya-dharma': { ground: 0x5a4030, fog: 0x3a2810, ring: 0xb8a070, pillar: 0x7a6040, gold: 0xe0c850, light: 0xffdd88 },
  'panchavati-golden-deer': { ground: 0x2a4a3a, fog: 0x0e2a1c, ring: 0x5a8a70, pillar: 0x3a5a4a, gold: 0x80c0a0, light: 0x66ffaa },
  'kishkindha-alliance': { ground: 0x4a3020, fog: 0x2a1410, ring: 0x9a6a50, pillar: 0x6a4830, gold: 0xd8a060, light: 0xff8855 },
  'sundarakanda-leap': { ground: 0x2a2840, fog: 0x1a1030, ring: 0x6a60a8, pillar: 0x4a3a6a, gold: 0x9080d0, light: 0x8866ff },
  'yuddhakanda-war': { ground: 0x3a2010, fog: 0x2a1008, ring: 0x8a5a3a, pillar: 0x5a3620, gold: 0xc88440, light: 0xff6622 },
  'return-ayodhya': { ground: 0x4a3a20, fog: 0x2a2010, ring: 0x9a8850, pillar: 0x6a5830, gold: 0xd8ba60, light: 0xffcc55 },
  'uttara-earth-return': { ground: 0x303058, fog: 0x101828, ring: 0x6868a0, pillar: 0x484870, gold: 0x8080c8, light: 0x6688dd },
};

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

  // Static geo groups — rebuilt per act
  let arenaGroup = new THREE.Group();

  function buildArena(actId) {
    // Remove previous
    arenaGroup.clear();
    scene.remove(arenaGroup);
    arenaGroup = new THREE.Group();
    scene.add(arenaGroup);

    const s = ARENA_STYLES[actId] || ARENA_STYLES['yuddhakanda-war'];

    // Ground
    const ground = new THREE.Mesh(
      new THREE.CircleGeometry(40, 48),
      new THREE.MeshStandardMaterial({ color: s.ground, roughness: 0.95, metalness: 0.05 })
    );
    ground.rotation.x = -Math.PI / 2;
    ground.receiveShadow = true;
    arenaGroup.add(ground);

    // Ring
    const ring = new THREE.Mesh(
      new THREE.RingGeometry(11.5, 12.2, 48),
      new THREE.MeshStandardMaterial({ color: s.ring, roughness: 0.8, side: THREE.DoubleSide })
    );
    ring.rotation.x = -Math.PI / 2;
    ring.position.y = 0.02;
    arenaGroup.add(ring);

    const stone = new THREE.MeshStandardMaterial({ color: s.pillar, roughness: 0.9 });
    const gold = new THREE.MeshStandardMaterial({ color: s.gold, roughness: 0.55, metalness: 0.35 });

    // Pillars
    for (let i = 0; i < 10; i++) {
      const a = (i / 10) * Math.PI * 2 + 0.2;
      const r = 13 + (i % 3) * 0.6;
      const h = 1.6 + (i % 4) * 0.7;
      const col = new THREE.Mesh(new THREE.CylinderGeometry(0.35, 0.45, h, 8), stone);
      col.position.set(Math.cos(a) * r, h / 2, Math.sin(a) * r);
      col.castShadow = true;
      col.receiveShadow = true;
      arenaGroup.add(col);
      if (i % 2 === 0) {
        const cap = new THREE.Mesh(new THREE.BoxGeometry(1.0, 0.25, 1.0), gold);
        cap.position.set(col.position.x, h + 0.1, col.position.z);
        arenaGroup.add(cap);
      }
    }

    // Plinth
    const plinth = new THREE.Mesh(new THREE.CylinderGeometry(1.8, 2.2, 0.5, 8), stone);
    plinth.position.set(0, 0.25, 0);
    plinth.receiveShadow = true;
    arenaGroup.add(plinth);

    // Torch lights
    for (let i = 0; i < 4; i++) {
      const a = (i / 4) * Math.PI * 2 + Math.PI / 4;
      const lc = new THREE.Color(s.light);
      const light = new THREE.PointLight(lc, 0.55, 14, 2);
      light.position.set(Math.cos(a) * 10, 2.2, Math.sin(a) * 10);
      arenaGroup.add(light);
      const flame = new THREE.Mesh(
        new THREE.SphereGeometry(0.15, 8, 8),
        new THREE.MeshBasicMaterial({ color: lc })
      );
      flame.position.copy(light.position);
      arenaGroup.add(flame);
    }

    // Mood palette
    const mood = new THREE.Color(s.ground);
    scene.background = mood.multiplyScalar(0.15);
    const f = new THREE.Color(s.fog);
    scene.fog.color.copy(f);
  }

  buildArena('yuddhakanda-war');

  function onResize() {
    const w = window.innerWidth;
    const h = window.innerHeight;
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
    renderer.setSize(w, h);
  }
  window.addEventListener('resize', onResize);

  return { renderer, scene, camera, buildArena };
}
