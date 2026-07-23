import { describe, it, expect, beforeEach } from 'vitest';
import {
  state, setHud, setHpBar,
  saveSlot, loadSlot, listSlots, deleteSlot,
  saveGame, loadGame, setHighScore,
} from '../../js/core/state.js';

// jsdom doesn't pre-build the HUD elements, so seed them.
function seedHudDom() {
  for (const id of ['hud-title', 'hud-obj', 'hud-wave', 'hp-fill', 'hp-label']) {
    if (!document.getElementById(id)) {
      const el = document.createElement('div');
      el.id = id;
      document.body.appendChild(el);
    }
  }
}

describe('state module', () => {
  beforeEach(() => {
    localStorage.clear();
    document.body.innerHTML = '';
    Object.assign(state, {
      running: false, dead: false, wave: 0, totalWaves: 3,
      maxHp: 5, hp: 5, kills: 0, objectiveTitle: '',
      actId: 'yuddhakanda-war',
    });
    seedHudDom();
  });

  it('has sane defaults', () => {
    expect(state.maxHp).toBe(5);
    expect(state.hp).toBe(5);
    expect(state.totalWaves).toBe(3);
    expect(state.kills).toBe(0);
  });

  it('setHud writes DOM elements', () => {
    setHud({ title: 'TEST_TITLE', obj: 'TEST_OBJ', wave: 'TEST_WAVE' });
    expect(document.getElementById('hud-title').textContent).toBe('TEST_TITLE');
    expect(document.getElementById('hud-obj').textContent).toBe('TEST_OBJ');
    expect(document.getElementById('hud-wave').textContent).toBe('TEST_WAVE');
  });

  it('setHud ignores keys it does not receive', () => {
    setHud({ title: 'T1' });
    expect(document.getElementById('hud-title').textContent).toBe('T1');
    expect(document.getElementById('hud-obj').textContent).not.toBe('T1');
  });

  it('setHpBar clamps ratio + writes label', () => {
    setHpBar(2, 5);
    const fill = document.getElementById('hp-fill');
    expect(fill.style.width).toBe('40%');
    expect(document.getElementById('hp-label').textContent).toBe('HP 2/5');
  });

  it('setHpBar toggles low/mid classes', () => {
    setHpBar(1, 5);
    expect(document.getElementById('hp-fill').classList.contains('low')).toBe(true);
    setHpBar(3, 5);
    expect(document.getElementById('hp-fill').classList.contains('mid')).toBe(true);
    setHpBar(5, 5);
    expect(document.getElementById('hp-fill').classList.contains('low')).toBe(false);
    expect(document.getElementById('hp-fill').classList.contains('mid')).toBe(false);
  });

  it('saveGame writes to "auto" slot; loadGame round-trips', () => {
    expect(saveGame('yuddhakanda-war', 7, 3, 5, 'objective-x')).toBeTruthy();
    const r = loadGame();
    expect(r.kills).toBe(7);
    expect(r.hp).toBe(3);
    expect(r.objectiveTitle).toBe('objective-x');
  });

  it('saveSlot/loadSlot — 4 numbered slots (0..3)', () => {
    expect(saveSlot(0, { kills: 11 })).toBeTruthy();
    expect(saveSlot(3, { kills: 33 })).toBeTruthy();
    expect(loadSlot(0).kills).toBe(11);
    expect(loadSlot(3).kills).toBe(33);
    // Out-of-range rejected
    expect(saveSlot(4, { kills: 1 })).toBeFalsy();
    expect(saveSlot(-1, { kills: 1 })).toBeFalsy();
    expect(loadSlot(4)).toBe(null);
    expect(loadSlot(-1)).toBe(null);
  });

  it('listSlots returns 4 entries in order with nulls', () => {
    saveSlot(1, { kills: 9 });
    const list = listSlots();
    expect(list).toHaveLength(4);
    expect(list[0]).toBeNull();
    expect(list[1].kills).toBe(9);
    expect(list[2]).toBeNull();
    expect(list[3]).toBeNull();
  });

  it('deleteSlot clears the slot', () => {
    saveSlot(2, { kills: 9 });
    expect(deleteSlot(2)).toBeTruthy();
    expect(loadSlot(2)).toBeNull();
  });

  it('setHighScore keeps only the best', () => {
    expect(setHighScore(3, 1)).toBe(true);
    expect(setHighScore(10, 2)).toBe(true);
    expect(setHighScore(5, 1)).toBe(false);
    const raw = JSON.parse(localStorage.getItem('ramayana_web_high'));
    expect(raw.kills).toBe(10);
    expect(raw.waves).toBe(2);
  });

  it('localStorage corruption is handled (no throw)', () => {
    localStorage.setItem('ramayana_web_slots', '{ not json');
    expect(() => listSlots()).not.toThrow();
    expect(listSlots()).toHaveLength(4);
  });
});
