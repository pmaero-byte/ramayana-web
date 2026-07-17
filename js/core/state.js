/** Shared mutable game state */
export const state = {
  running: false,
  dead: false,
  actId: 'yuddhakanda-war',
  actTitle: 'Yuddha Kāṇḍa',
  objectiveIndex: -1,
  objectiveId: '',
  objectiveTitle: '',
  wave: 0,
  totalWaves: 3,
  alive: 0,
  corpus: null,
  selectedActId: 'yuddhakanda-war',
  maxHp: 5,
  hp: 5,
  kills: 0,
};

export function setHud({ title, obj, wave }) {
  const t = document.getElementById('hud-title');
  const o = document.getElementById('hud-obj');
  const w = document.getElementById('hud-wave');
  if (title != null && t) t.textContent = title;
  if (obj != null && o) o.textContent = obj;
  if (wave != null && w) w.textContent = wave;
}

export function setHpBar(hp, maxHp) {
  state.hp = hp;
  state.maxHp = maxHp;
  const fill = document.getElementById('hp-fill');
  const label = document.getElementById('hp-label');
  const pct = Math.max(0, Math.min(1, maxHp > 0 ? hp / maxHp : 0));
  if (fill) {
    fill.style.width = `${pct * 100}%`;
    fill.classList.toggle('low', pct <= 0.34);
    fill.classList.toggle('mid', pct > 0.34 && pct <= 0.66);
  }
  if (label) label.textContent = `HP ${hp}/${maxHp}`;
}
