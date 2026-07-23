import { portraitUrl } from '../story/moments.js';

let hideTimer = 0;

export function showDialogue(speaker, line, holdSec = 2.6) {
  const root = document.getElementById('dialogue');
  const sp = document.getElementById('dlg-speaker');
  const ln = document.getElementById('dlg-line');
  const img = document.getElementById('dlg-portrait');
  if (!root) return;
  sp.textContent = speaker || '—';
  ln.textContent = line || '';
  img.src = portraitUrl(speaker);
  img.onerror = () => { img.src = portraitUrl('rama'); };
  root.classList.remove('hidden');
  clearTimeout(hideTimer);
  hideTimer = setTimeout(() => root.classList.add('hidden'), holdSec * 1000);
}

export function hideDialogue() {
  document.getElementById('dialogue')?.classList.add('hidden');
}

export function buildTitle(corpus, onSelect, onStart, onContinue) {
  const list = document.getElementById('act-list');
  const btn = document.getElementById('btn-start');
  const cont = document.getElementById('btn-continue');
  if (!list || !corpus?.acts) return;
  list.innerHTML = '';
  let selected = corpus.acts.find((a) => a.actId === 'yuddhakanda-war')?.actId || corpus.acts[0]?.actId;

  try {
    const saved = JSON.parse(window.localStorage.getItem('ramayana_web_save') || 'null');
    if (saved && saved.actId && cont) {
      const act = corpus.acts.find(a => a.actId === saved.actId);
      cont.style.display = '';
      cont.textContent = `▶ Continue · ${act?.title || saved.actId}`;
      cont.onclick = () => onContinue?.(saved);
    } else if (cont) {
      cont.style.display = 'none';
    }
  } catch { if (cont) cont.style.display = 'none'; }

  for (const act of corpus.acts) {
    const el = document.createElement('div');
    el.className = 'act-card' + (act.actId === selected ? ' selected' : '');
    el.innerHTML = `<div class="t">${esc(act.title || act.actId)}</div><div class="l">${esc(act.location || '')}</div>`;
    el.addEventListener('click', () => {
      selected = act.actId;
      list.querySelectorAll('.act-card').forEach((c) => c.classList.remove('selected'));
      el.classList.add('selected');
      onSelect?.(selected, act);
    });
    list.appendChild(el);
  }
  btn?.addEventListener('click', () => onStart?.(selected));
}

export function buildCharacterSelect(corpus, currentId, onPick) {
  const grid = document.getElementById('char-grid');
  if (!grid || !corpus?.characters) return;
  grid.innerHTML = '';
  const playable = ['rama', 'sita', 'lakshmana', 'hanuman'];
  const chars = corpus.characters.filter(c => playable.includes(c.characterId));
  for (const c of chars) {
    const el = document.createElement('button');
    el.type = 'button';
    el.className = 'char-card' + (c.characterId === currentId ? ' selected' : '');
    el.style.setProperty('--char-color', c.color || '#888');
    el.innerHTML = `<img src="assets/portraits/${c.characterId}.png" alt="${esc(c.displayName)}" onerror="this.style.display='none'" /><div class="ct">${esc(c.displayName)}</div><div class="cr">${esc(c.role || '')}</div>`;
    el.addEventListener('click', () => {
      grid.querySelectorAll('.char-card').forEach((x) => x.classList.remove('selected'));
      el.classList.add('selected');
      onPick?.(c.characterId, c);
    });
    grid.appendChild(el);
  }
}

export function buildSlotsUi(onLoad, onDelete, onSaveCurrent) {
  const wrap = document.getElementById('slot-list');
  if (!wrap) return;
  wrap.innerHTML = '';
  const slots = (window.RamaWeb?.state && window.RamaWeb.state.__getSlots)
    ? window.RamaWeb.state.__getSlots()
    : JSON.parse(window.localStorage.getItem('ramayana_web_slots') || '{}');
  for (let i = 0; i < 4; i++) {
    const s = slots[`slot_${i}`];
    const row = document.createElement('div');
    row.className = 'slot-row' + (s ? '' : ' empty');
    row.dataset.slot = String(i);
    const head = document.createElement('div');
    head.className = 'slot-head';
    head.textContent = s ? `Slot ${i} · ${s.actId} · ${s.kills} kills` : `Slot ${i} · empty`;
    row.appendChild(head);
    if (s) {
      const dt = document.createElement('div');
      dt.className = 'slot-sub';
      const d = new Date(s.ts || 0);
      dt.textContent = `${d.toLocaleDateString()} ${d.toLocaleTimeString()} · ${s.objectiveTitle || ''}`;
      row.appendChild(dt);
    }
    const actions = document.createElement('div');
    actions.className = 'slot-actions';
    const ld = document.createElement('button');
    ld.className = 'slot-btn slot-load'; ld.type = 'button'; ld.textContent = 'Load';
    ld.disabled = !s;
    ld.onclick = () => s && onLoad?.(i, s);
    const sv = document.createElement('button');
    sv.className = 'slot-btn slot-save'; sv.type = 'button'; sv.textContent = 'Save here';
    sv.onclick = () => onSaveCurrent?.(i);
    const dl = document.createElement('button');
    dl.className = 'slot-btn slot-del'; dl.type = 'button'; dl.textContent = '✕';
    dl.disabled = !s;
    dl.onclick = () => s && onDelete?.(i);
    actions.append(ld, sv, dl);
    row.appendChild(actions);
    wrap.appendChild(row);
  }
}

export function hideTitle() {
  document.getElementById('title')?.classList.add('hidden');
}

export function showTitle() {
  document.getElementById('title')?.classList.remove('hidden');
  hideDialogue();
}

export function updateContinueBtn() {
  const cont = document.getElementById('btn-continue');
  if (cont) cont.style.display = 'none';
  try {
    const saved = JSON.parse(window.localStorage.getItem('ramayana_web_save') || 'null');
    if (saved && saved.actId && cont) {
      cont.style.display = '';
      cont.textContent = `▶ Continue · ${saved.actId}`;
    }
  } catch {}
}

function esc(s) {
  return String(s).replace(/[&<>"']/g, (c) => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;',
  }[c]));
}
