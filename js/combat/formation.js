/** Arc / Chakra / Vyuha spawn layouts (port of Unity FormationStrategy) */
export function spawnPoints(kind, count, origin, forward, radius = 6) {
  const n = Math.max(1, count | 0);
  const pts = [];
  const f = flatten(forward);
  const right = crossY(f);

  if (kind === 'chakra') {
    const start = Math.atan2(f.x, f.z);
    for (let i = 0; i < n; i++) {
      const a = start + (Math.PI * 2 * i) / n;
      pts.push({
        x: origin.x + Math.sin(a) * radius,
        y: 0,
        z: origin.z + Math.cos(a) * radius,
      });
    }
    return pts;
  }

  if (kind === 'vyuha') {
    const row0 = Math.max(1, Math.floor((n + 2) / 3));
    const row1 = Math.max(1, Math.floor((n - row0 + 1) / 2));
    const row2 = Math.max(0, n - row0 - row1);
    const rows = [row0, row1, row2];
    const depths = [radius * 0.85, radius * 1.15, radius * 1.45];
    const spreads = [radius * 0.55, radius * 0.85, radius * 1.15];
    let idx = 0;
    for (let r = 0; r < 3 && idx < n; r++) {
      const c = rows[r];
      for (let i = 0; i < c && idx < n; i++, idx++) {
        const t = c <= 1 ? 0.5 : i / (c - 1);
        const x = lerp(-spreads[r], spreads[r], t);
        pts.push({
          x: origin.x + f.x * depths[r] + right.x * x,
          y: 0,
          z: origin.z + f.z * depths[r] + right.z * x,
        });
      }
    }
    while (pts.length < n) {
      const i = pts.length;
      pts.push({
        x: origin.x + f.x * radius + right.x * ((i - n * 0.5) * 0.8),
        y: 0,
        z: origin.z + f.z * radius + right.z * ((i - n * 0.5) * 0.8),
      });
    }
    return pts;
  }

  // arc default
  const arc = Math.PI * 0.65;
  for (let i = 0; i < n; i++) {
    const t = n <= 1 ? 0.5 : i / (n - 1);
    const angle = lerp(-arc * 0.5, arc * 0.5, t);
    const lx = Math.sin(angle) * radius;
    const lz = Math.cos(angle) * radius;
    pts.push({
      x: origin.x + f.x * lz + right.x * lx,
      y: 0,
      z: origin.z + f.z * lz + right.z * lx,
    });
  }
  return pts;
}

function flatten(v) {
  const x = v.x ?? 0;
  const z = v.z ?? 1;
  const len = Math.hypot(x, z) || 1;
  return { x: x / len, z: z / len };
}
function crossY(f) {
  // up × forward
  return { x: f.z, z: -f.x };
}
function lerp(a, b, t) {
  return a + (b - a) * t;
}

export function kindForWave(waveIndex) {
  if (waveIndex <= 1) return 'arc';
  if (waveIndex === 2) return 'vyuha';
  return 'chakra';
}
