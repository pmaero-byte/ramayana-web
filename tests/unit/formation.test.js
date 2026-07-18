import { describe, it, expect } from 'vitest';
import { spawnPoints, kindForWave } from '../../js/combat/formation.js';

describe('formation spawnPoints', () => {
  const origin = { x: 0, y: 0, z: 0 };
  const forward = { x: 0, y: 0, z: 1 };

  it('arc: returns n points within ring radius', () => {
    const pts = spawnPoints('arc', 5, origin, forward, 6);
    expect(pts).toHaveLength(5);
    for (const p of pts) {
      const r = Math.hypot(p.x, p.z);
      expect(r).toBeGreaterThan(0);
    }
  });

  it('chakra: full 360° ring', () => {
    const pts = spawnPoints('chakra', 8, origin, forward, 6);
    expect(pts).toHaveLength(8);
    // Should be spread around the full ring
    const angles = pts.map(p => Math.atan2(p.x, p.z)).sort((a, b) => a - b);
    for (let i = 1; i < angles.length; i++) {
      expect(angles[i] - angles[i - 1]).toBeLessThan(Math.PI);
    }
  });

  it('vyuha: row layout in front of player', () => {
    const pts = spawnPoints('vyuha', 6, origin, forward, 6);
    expect(pts).toHaveLength(6);
    // All points should be in front of the player (positive Z)
    for (const p of pts) expect(p.z).toBeGreaterThan(0);
  });

  it('unknown kind falls back to arc', () => {
    const pts = spawnPoints('unknown', 3, origin, forward, 6);
    expect(pts).toHaveLength(3);
  });

  it('handles count=1 without divide-by-zero', () => {
    const pts = spawnPoints('arc', 1, origin, forward, 6);
    expect(pts).toHaveLength(1);
    const r = Math.hypot(pts[0].x, pts[0].z);
    expect(r).toBeCloseTo(6, 1);
  });
});

describe('kindForWave', () => {
  it('wave 1 = arc', () => expect(kindForWave(1)).toBe('arc'));
  it('wave 2 = vyuha', () => expect(kindForWave(2)).toBe('vyuha'));
  it('wave 3+ = chakra', () => {
    expect(kindForWave(3)).toBe('chakra');
    expect(kindForWave(4)).toBe('chakra');
  });
});
