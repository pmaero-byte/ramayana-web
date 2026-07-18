import * as THREE from 'three';

/** Procedural cover props: barrels, crates, low walls.
 *  Each gets a 3D silhouette the player can dodge behind.
 *  Different combos per wave = different fight geometry. */
export function createCoverSet(scene, player, actId) {
  const group = new THREE.Group();
  scene.add(group);
  const items = [];

  // Per-act color theme
  const palettes = {
    'bala-birth':            { wood: 0xc8a070, stone: 0xa0a0b0, brass: 0xe0c060 },
    'ayodhya-dharma':        { wood: 0xb88a4a, stone: 0xb09878, brass: 0xf0c060 },
    'panchavati-golden-deer':{ wood: 0xa09060, stone: 0x808870, brass: 0xd0c080 },
    'kishkindha-alliance':   { wood: 0x9c7048, stone: 0xa08068, brass: 0xc89058 },
    'sundarakanda-leap':     { wood: 0x604878, stone: 0x6878a0, brass: 0x9080c0 },
    'yuddhakanda-war':       { wood: 0x6a3a28, stone: 0x8a5a3a, brass: 0xc07030 },
    'return-ayodhya':        { wood: 0xc0a070, stone: 0xb8a890, brass: 0xe8c878 },
    'uttara-earth-return':   { wood: 0x707898, stone: 0x8088a8, brass: 0x98a8c8 },
  };
  const pal = palettes[actId] || palettes['yuddhakanda-war'];
  const wood = new THREE.MeshStandardMaterial({ color: pal.wood, roughness: 0.85 });
  const stone = new THREE.MeshStandardMaterial({ color: pal.stone, roughness: 0.9 });
  const brass = new THREE.MeshStandardMaterial({ color: pal.brass, roughness: 0.4, metalness: 0.55 });

  function barrel(x, z, r = 0.45, h = 1.1) {
    const g = new THREE.Group();
    const body = new THREE.Mesh(
      new THREE.CylinderGeometry(r, r, h, 12),
      wood
    );
    body.position.y = h / 2;
    body.castShadow = true;
    body.receiveShadow = true;
    g.add(body);
    const ring1 = new THREE.Mesh(
      new THREE.TorusGeometry(r * 1.04, 0.04, 6, 18),
      brass
    );
    ring1.rotation.x = Math.PI / 2;
    ring1.position.y = h * 0.7;
    g.add(ring1);
    const ring2 = ring1.clone();
    ring2.position.y = h * 0.3;
    g.add(ring2);
    g.position.set(x, 0, z);
    group.add(g);
    items.push({ group: g, kind: 'barrel', r, h, hit: (n) => body.material.emissiveIntensity = n });
    return g;
  }

  function crate(x, z, s = 0.9) {
    const g = new THREE.Group();
    const body = new THREE.Mesh(
      new THREE.BoxGeometry(s, s, s),
      wood
    );
    body.position.y = s / 2;
    body.castShadow = true;
    body.receiveShadow = true;
    g.add(body);
    // cross straps
    const strap = new THREE.Mesh(
      new THREE.BoxGeometry(s * 1.02, s * 0.06, s * 0.18),
      brass
    );
    strap.position.y = s * 0.3;
    g.add(strap);
    const strap2 = strap.clone();
    strap2.position.y = -s * 0.3;
    g.add(strap2);
    g.position.set(x, 0, z);
    g.rotation.y = Math.random() * 0.5 - 0.25;
    group.add(g);
    items.push({ group: g, kind: 'crate', s, hit: (n) => body.material.emissiveIntensity = n });
    return g;
  }

  function lowWall(x, z, w = 2.6, h = 0.9) {
    const g = new THREE.Group();
    const body = new THREE.Mesh(
      new THREE.BoxGeometry(w, h, 0.4),
      stone
    );
    body.position.y = h / 2;
    body.castShadow = true;
    body.receiveShadow = true;
    g.add(body);
    // crenellations
    for (let i = -w / 2 + 0.4; i <= w / 2 - 0.4; i += 0.7) {
      const c = new THREE.Mesh(
        new THREE.BoxGeometry(0.32, 0.32, 0.5),
        stone
      );
      c.position.set(i, h + 0.16, 0);
      g.add(c);
    }
    g.position.set(x, h / 2, z);
    g.rotation.y = Math.random() * Math.PI;
    group.add(g);
    items.push({ group: g, kind: 'wall', w, h, hit: (n) => body.material.emissiveIntensity = n });
    return g;
  }

  // Layout: ring at radius ~5, alternating barrels/crates/walls
  // Deterministic per-act variation
  const layouts = {
    'yuddhakanda-war': [
      ['wall', -4, 2, 2.6], ['barrel', -2, -3, 0.5], ['crate', 2, -3, 1.0],
      ['barrel', 4, 2, 0.45], ['crate', -1, 4, 0.9], ['wall', 3, 1, 2.0],
      ['barrel', -3, -1, 0.5], ['crate', 1, 1, 0.85],
    ],
    'kishkindha-alliance': [
      ['wall', -3, 3, 3.0], ['crate', 3, 2, 1.1], ['crate', -1, -4, 1.0],
      ['barrel', 2, -2, 0.5], ['barrel', -4, 0, 0.5], ['wall', 1, 4, 2.5],
    ],
    'sundarakanda-leap': [
      ['barrel', -2, 2, 0.5], ['barrel', 2, 2, 0.5], ['crate', 0, 4, 1.2],
      ['wall', 4, -2, 2.0], ['wall', -4, -2, 2.0], ['crate', 0, -3, 0.9],
    ],
    'ayodhya-dharma': [
      ['wall', -4, 1, 3.4], ['wall', 4, 1, 3.4], ['crate', 0, 0, 1.1],
      ['barrel', -2, 4, 0.5], ['barrel', 2, 4, 0.5],
    ],
  };
  const defaultLayout = [
    ['crate', -2, -2, 1.0], ['crate', 2, -2, 1.0],
    ['barrel', -3, 2, 0.5], ['barrel', 3, 2, 0.5],
    ['wall', 0, 4, 2.2], ['wall', 0, -4, 2.2],
  ];
  const plan = layouts[actId] || defaultLayout;
  for (const [kind, x, z, sz] of plan) {
    if (kind === 'barrel') barrel(x, z, 0.5, sz || 1.1);
    else if (kind === 'crate') crate(x, z, sz || 0.9);
    else if (kind === 'wall') lowWall(x, z, sz || 2.6, 0.9);
  }

  // Check if cover blocks line of sight between player and target
  function blocksLine(from, to) {
    for (const it of items) {
      const p = it.group.position;
      const dx = to.x - from.x, dz = to.z - from.z;
      const segLen2 = dx * dx + dz * dz;
      if (segLen2 < 0.01) continue;
      // Closest point on segment
      const t = Math.max(0, Math.min(1, ((p.x - from.x) * dx + (p.z - from.z) * dz) / segLen2));
      const cx = from.x + dx * t, cz = from.z + dz * t;
      const ddx = p.x - cx, ddz = p.z - cz;
      const dist2 = ddx * ddx + ddz * ddz;
      const r = it.kind === 'wall' ? 0.4 : (it.kind === 'crate' ? (it.s / 2) + 0.3 : it.r + 0.3);
      if (dist2 < r * r) return true;
    }
    return false;
  }

  return {
    group,
    items,
    blocksLine,
    dispose() {
      scene.remove(group);
      group.traverse(o => {
        if (o.geometry) o.geometry.dispose();
        if (o.material && o.material.dispose) o.material.dispose();
      });
    },
  };
}
