import * as THREE from 'three';

const ARENA_STYLES = {
  'bala-birth': { ground: 0x3a5a7a, fog: 0x1a2840, ring: 0x6a8ab0, pillar: 0x4a6a8a, gold: 0xa0c8e0, light: 0x88bbff, env: 'sacred-grove' },
  'ayodhya-dharma': { ground: 0x5a4030, fog: 0x3a2810, ring: 0xb8a070, pillar: 0x7a6040, gold: 0xe0c850, light: 0xffdd88, env: 'palace-courtyard' },
  'panchavati-golden-deer': { ground: 0x2a4a3a, fog: 0x0e2a1c, ring: 0x5a8a70, pillar: 0x3a5a4a, gold: 0x80c0a0, light: 0x66ffaa, env: 'forest-river' },
  'kishkindha-alliance': { ground: 0x4a3020, fog: 0x2a1410, ring: 0x9a6a50, pillar: 0x6a4830, gold: 0xd8a060, light: 0xff8855, env: 'rocky-hills' },
  'sundarakanda-leap': { ground: 0x2a2840, fog: 0x1a1030, ring: 0x6a60a8, pillar: 0x4a3a6a, gold: 0x9080d0, light: 0x8866ff, env: 'ocean-cliffs' },
  'yuddhakanda-war': { ground: 0x3a2010, fog: 0x2a1008, ring: 0x8a5a3a, pillar: 0x5a3620, gold: 0xc88440, light: 0xff6622, env: 'lanka-battle' },
  'return-ayodhya': { ground: 0x4a3a20, fog: 0x2a2010, ring: 0x9a8850, pillar: 0x6a5830, gold: 0xd8ba60, light: 0xffcc55, env: 'palace-courtyard' },
  'uttara-earth-return': { ground: 0x303058, fog: 0x101828, ring: 0x6868a0, pillar: 0x484870, gold: 0x8080c8, light: 0x6688dd, env: 'sacred-grove' },
};

/** Build environment-specific props (trees, water, rocks) for an arena. */
function buildEnv(env, group, palette) {
  const stone = new THREE.MeshStandardMaterial({ color: palette.pillar, roughness: 0.95 });
  const leaf = new THREE.MeshStandardMaterial({ color: 0x3a8a4a, roughness: 0.85 });
  const water = new THREE.MeshStandardMaterial({ color: 0x4080c0, roughness: 0.2, metalness: 0.7, transparent: true, opacity: 0.8 });
  const sand = new THREE.MeshStandardMaterial({ color: 0xc4a574, roughness: 1.0 });

  if (env === 'sacred-grove') {
    for (let i = 0; i < 14; i++) {
      const a = (i / 14) * Math.PI * 2 + 0.4;
      const r = 18 + (i % 3) * 2;
      const trunk = new THREE.Mesh(new THREE.CylinderGeometry(0.18, 0.24, 1.5, 6), stone);
      trunk.position.set(Math.cos(a) * r, 0.75, Math.sin(a) * r);
      const top = new THREE.Mesh(new THREE.SphereGeometry(1.1, 6, 5), leaf);
      top.position.set(trunk.position.x, 2.0, trunk.position.z);
      group.add(trunk, top);
    }
  } else if (env === 'forest-river') {
    for (let i = 0; i < 10; i++) {
      const a = (i / 10) * Math.PI * 2;
      const r = 20 + (i % 2) * 3;
      const trunk = new THREE.Mesh(new THREE.CylinderGeometry(0.22, 0.3, 1.8, 6), stone);
      trunk.position.set(Math.cos(a) * r, 0.9, Math.sin(a) * r);
      const top = new THREE.Mesh(new THREE.ConeGeometry(1.6, 3.5, 6), leaf);
      top.position.set(trunk.position.x, 2.5, trunk.position.z);
      group.add(trunk, top);
    }
    const river = new THREE.Mesh(
      new THREE.PlaneGeometry(6, 36),
      water
    );
    river.rotation.x = -Math.PI / 2;
    river.position.set(15, 0.03, 0);
    group.add(river);
  } else if (env === 'rocky-hills') {
    for (let i = 0; i < 8; i++) {
      const a = (i / 8) * Math.PI * 2;
      const r = 16 + (i % 3) * 2;
      const rock = new THREE.Mesh(new THREE.DodecahedronGeometry(1.0 + (i % 3) * 0.4, 0), stone);
      rock.position.set(Math.cos(a) * r, 0.6 + (i % 2) * 0.5, Math.sin(a) * r);
      rock.rotation.set((i * 0.4) % Math.PI, (i * 0.7) % Math.PI, 0);
      group.add(rock);
    }
  } else if (env === 'ocean-cliffs') {
    for (let i = 0; i < 6; i++) {
      const a = (i / 6) * Math.PI;
      const r = 24 + (i % 2) * 2;
      const cliff = new THREE.Mesh(new THREE.BoxGeometry(3 + (i % 2), 2.5 + (i % 2), 1.5), stone);
      cliff.position.set(Math.cos(a) * r, 1.2, Math.sin(a) * r);
      group.add(cliff);
    }
    const sea = new THREE.Mesh(
      new THREE.CircleGeometry(40, 32),
      water
    );
    sea.rotation.x = -Math.PI / 2;
    sea.position.y = 0.01;
    group.add(sea);
  } else if (env === 'lanka-battle') {
    // burning wreckage — small ember cones
    for (let i = 0; i < 6; i++) {
      const a = (i / 6) * Math.PI * 2;
      const r = 15 + (i % 2) * 2;
      const ruin = new THREE.Mesh(new THREE.BoxGeometry(0.8 + (i % 2) * 0.4, 0.6, 0.4), stone);
      ruin.position.set(Math.cos(a) * r, 0.3, Math.sin(a) * r);
      ruin.rotation.y = (i * 0.5) % Math.PI;
      group.add(ruin);
    }
  } else if (env === 'palace-courtyard') {
    for (let i = 0; i < 4; i++) {
      const a = (i / 4) * Math.PI * 2 + Math.PI / 4;
      const r = 14;
      const flag = new THREE.Mesh(new THREE.CylinderGeometry(0.08, 0.08, 3, 5), stone);
      flag.position.set(Math.cos(a) * r, 1.5, Math.sin(a) * r);
      const banner = new THREE.Mesh(
        new THREE.PlaneGeometry(0.8, 0.5),
        new THREE.MeshStandardMaterial({ color: palette.gold, side: THREE.DoubleSide })
      );
      banner.position.set(flag.position.x + 0.4, 2.5, flag.position.z);
      group.add(flag, banner);
    }
  }
}

export function createWorld(canvas) {
  const renderer = new THREE.WebGLRenderer({ canvas, antialias: true, alpha: false, powerPreference: 'high-performance', preserveDrawingBuffer: true });
  renderer.setClearColor(0x000000, 1.0);
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  renderer.setSize(window.innerWidth, window.innerHeight);
  renderer.shadowMap.enabled = true;

  const scene = new THREE.Scene();
  scene.background = new THREE.Color().setRGB(0.4, 0.45, 0.55, THREE.LinearSRGBColorSpace);
  scene.fog = new THREE.Fog(0x2a1810, 16, 70);

  const camera = new THREE.PerspectiveCamera(55, window.innerWidth / window.innerHeight, 0.1, 200);
  camera.position.set(0, 3.2, -7.5);

  const sun = new THREE.DirectionalLight(0xffe0b8, 3.5);
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
  scene.add(new THREE.AmbientLight(0xffe0c8, 2.2));
  scene.add(new THREE.HemisphereLight(0xffe0c8, 0x402010, 1.4));

  let arenaGroup = new THREE.Group();

  function buildArena(actId) {
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

    // Environment props per act
    buildEnv(s.env, arenaGroup, s);

    // Mood palette — set in linear color space for visible bg
    const moodL = new THREE.Color(s.ground).convertSRGBToLinear();
    const goldL = new THREE.Color(s.gold).convertSRGBToLinear();
    scene.background = moodL.clone().lerp(goldL, 0.45);
    const f = new THREE.Color(s.fog);
    scene.fog.color.copy(f);
  }

  buildArena('yuddhakanda-war');

  // Sky dome — large sphere with subtle gradient, gives the world a ceiling
  const skyGeo = new THREE.SphereGeometry(80, 24, 16);
  const skyMat = new THREE.ShaderMaterial({
    side: THREE.BackSide,
    depthWrite: false,
    uniforms: {
      topColor: { value: new THREE.Color(0x281006).convertSRGBToLinear() },
      bottomColor: { value: new THREE.Color(0x6a2a14).convertSRGBToLinear() },
    },
    vertexShader: `
      varying vec3 vWorldPos;
      void main() {
        vec4 worldPos = modelMatrix * vec4(position, 1.0);
        vWorldPos = worldPos.xyz;
        gl_Position = projectionMatrix * viewMatrix * worldPos;
      }
    `,
    fragmentShader: `
      uniform vec3 topColor;
      uniform vec3 bottomColor;
      varying vec3 vWorldPos;
      void main() {
        float h = normalize(vWorldPos).y;
        float t = smoothstep(-0.1, 0.6, h);
        vec3 col = mix(bottomColor, topColor, t);
        gl_FragColor = vec4(col, 1.0);
      }
    `,
  });
  const sky = new THREE.Mesh(skyGeo, skyMat);
  scene.add(sky);

  // Drifting dust motes — slow ambient particles for atmosphere
  const moteCount = 80;
  const moteGeo = new THREE.BufferGeometry();
  const motePos = new Float32Array(moteCount * 3);
  const moteVel = new Float32Array(moteCount * 3);
  for (let i = 0; i < moteCount; i++) {
    motePos[i * 3] = (Math.random() - 0.5) * 30;
    motePos[i * 3 + 1] = Math.random() * 6 + 0.5;
    motePos[i * 3 + 2] = (Math.random() - 0.5) * 30;
    moteVel[i * 3] = (Math.random() - 0.5) * 0.4;
    moteVel[i * 3 + 1] = (Math.random() - 0.5) * 0.2;
    moteVel[i * 3 + 2] = (Math.random() - 0.5) * 0.4;
  }
  moteGeo.setAttribute('position', new THREE.BufferAttribute(motePos, 3));
  const moteMat = new THREE.PointsMaterial({
    color: 0xffd098,
    size: 0.08,
    transparent: true,
    opacity: 0.55,
    blending: THREE.AdditiveBlending,
    depthWrite: false,
  });
  const motes = new THREE.Points(moteGeo, moteMat);
  scene.add(motes);

  function updateAtmosphere(dt) {
    const p = motes.geometry.attributes.position.array;
    for (let i = 0; i < moteCount; i++) {
      p[i * 3] += moteVel[i * 3] * dt;
      p[i * 3 + 1] += moteVel[i * 3 + 1] * dt;
      p[i * 3 + 2] += moteVel[i * 3 + 2] * dt;
      // wrap back if drifted out
      if (Math.abs(p[i * 3]) > 15) p[i * 3] *= -0.95;
      if (Math.abs(p[i * 3 + 2]) > 15) p[i * 3 + 2] *= -0.95;
      if (p[i * 3 + 1] > 6.5 || p[i * 3 + 1] < 0.3) p[i * 3 + 1] = Math.random() * 6 + 0.5;
    }
    motes.geometry.attributes.position.needsUpdate = true;
  }

  function onResize() {
    const w = window.innerWidth;
    const h = window.innerHeight;
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
    renderer.setSize(w, h);
  }
  window.addEventListener('resize', onResize);

  return { renderer, scene, camera, buildArena, updateAtmosphere };
}
