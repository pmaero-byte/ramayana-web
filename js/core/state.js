/** Shared mutable game state */
export const state = {
  running: false,
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
};

export function setHud({ title, obj, wave }) {
  const t = document.getElementById('hud-title');
  const o = document.getElementById('hud-obj');
  const w = document.getElementById('hud-wave');
  if (title != null && t) t.textContent = title;
  if (obj != null && o) o.textContent = obj;
  if (wave != null && w) w.textContent = wave;
}
