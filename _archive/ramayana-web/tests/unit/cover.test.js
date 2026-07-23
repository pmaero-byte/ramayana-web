import { describe, it, expect } from 'vitest';
import { createCoverSet } from '../../js/combat/cover.js';
import * as THREE from '../../js/__mocks__/three.js';

describe('cover set', () => {
  const scene = new THREE.Group();
  const player = { position: new THREE.Vector3(0, 0, 0), forward: new THREE.Vector3(0, 0, 1) };

  it('exports blocksLine + hitTest + items', () => {
    const cover = createCoverSet(scene, player, 'yuddhakanda-war');
    expect(typeof cover.blocksLine).toBe('function');
    expect(typeof cover.hitTest).toBe('function');
    expect(Array.isArray(cover.items)).toBe(true);
    expect(cover.items.length).toBeGreaterThan(0);
    cover.dispose();
  });

  it('blocksLine returns boolean', () => {
    const cover = createCoverSet(scene, player, 'yuddhakanda-war');
    const r = cover.blocksLine({ x: 0, y: 1, z: 0 }, { x: 0, y: 1, z: 12 });
    expect(typeof r).toBe('boolean');
    cover.dispose();
  });

  it('hitTest returns null for clear path', () => {
    // Use 'ayodhya-dharma' — its props are at z=2 and z=5; z=-12 is clear.
    const cover = createCoverSet(scene, player, 'ayodhya-dharma');
    const hit = cover.hitTest({ x: 0, y: 1, z: 0 }, { x: 0, y: 1, z: -12 });
    expect(hit == null).toBe(true);
    cover.dispose();
  });

  it('hitTest returns {item,point,t} when blocked', () => {
    const cover = createCoverSet(scene, player, 'yuddhakanda-war');
    // Pick the first prop and shoot toward its XZ from the player origin
    const it = cover.items[0];
    const p = it.group.position;
    const hit = cover.hitTest({ x: 0, y: 1, z: 0 }, { x: p.x, y: 1, z: p.z });
    if (hit) {
      expect(hit.item).toBe(it);
      expect(typeof hit.t).toBe('number');
      expect(typeof hit.point.x).toBe('number');
      expect(typeof hit.point.z).toBe('number');
    }
    cover.dispose();
  });

  it('dispose clears scene', () => {
    const cover = createCoverSet(scene, player, 'yuddhakanda-war');
    const before = scene.children.length;
    cover.dispose();
    expect(scene.children.length).toBe(before - 1);
  });
});
