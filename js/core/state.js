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

const SAVE_KEY = 'ramayana_web_save';
const HIGH_KEY = 'ramayana_web_high';

export function saveGame(actId, kills, hp, maxHp, objectiveTitle) {
  try {
    const data = { actId, kills, hp, maxHp, objectiveTitle, ts: Date.now() };
    window.localStorage.setItem(SAVE_KEY, JSON.stringify(data));
    return true;
  } catch { return false; }
}

export function loadGame() {
  try {
    const raw = window.localStorage.getItem(SAVE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch { return null; }
}

export function deleteSave() {
  try { window.localStorage.removeItem(SAVE_KEY); } catch {}
}

export function getHighScore() {
  try {
    const raw = window.localStorage.getItem(HIGH_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch { return null; }
}

export function setHighScore(kills, waves) {
  try {
    const current = getHighScore();
    if (!current || kills > current.kills) {
      window.localStorage.setItem(HIGH_KEY, JSON.stringify({ kills, waves, ts: Date.now() }));
      return true;
    }
    return false;
  } catch { return false; }
}
