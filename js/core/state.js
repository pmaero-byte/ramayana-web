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
  selectedCharacter: 'rama',
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

const HIGH_KEY = 'ramayana_web_high';
const SLOTS_KEY = 'ramayana_web_slots';
const SLOT_COUNT = 4;

function readSlots() {
  try { return JSON.parse(window.localStorage.getItem(SLOTS_KEY) || '{}') || {}; }
  catch { return {}; }
}
function writeSlots(o) {
  try { window.localStorage.setItem(SLOTS_KEY, JSON.stringify(o)); return true; }
  catch { return false; }
}

export function saveSlot(slot, data) {
  const all = readSlots();
  all[`slot_${slot}`] = { ...data, ts: Date.now() };
  return writeSlots(all);
}
export function loadSlot(slot) {
  const all = readSlots();
  return all[`slot_${slot}`] || null;
}
export function deleteSlot(slot) {
  const all = readSlots();
  delete all[`slot_${slot}`];
  return writeSlots(all);
}
export function listSlots() {
  const all = readSlots();
  const out = [];
  for (let i = 0; i < SLOT_COUNT; i++) out.push(all[`slot_${i}`] || null);
  return out;
}

export function saveGame(actId, kills, hp, maxHp, objectiveTitle) {
  return saveSlot('auto', { actId, kills, hp, maxHp, objectiveTitle });
}
export function loadGame() {
  return loadSlot('auto');
}
export function deleteSave() {
  return deleteSlot('auto');
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
