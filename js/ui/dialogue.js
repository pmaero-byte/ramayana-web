import { portraitUrl } from '../story/moments.js';
import { getHighScore, deleteSave } from '../core/state.js';

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
  const delBtn = document.getElementById('btn-delete-save');
  const scoreEl = document.getElementById('high-score');
  if (!list || !corpus?.acts) return;
  list.innerHTML = '';
  let selected = corpus.acts.find((a) => a.actId === 'yuddhakanda-war')?.actId || corpus.acts[0]?.actId;

  // High score
  const hs = getHighScore();
  if (scoreEl && hs) scoreEl.textContent = `🏆 Best: ${hs.kills} kills · ${hs.waves || '-'} waves`;

  // Continue / Delete buttons
  try {
    const saved = JSON.parse(window.localStorage.getItem('ramayana_web_save') || 'null');
    if (saved && saved.actId) {
      const act = corpus.acts.find(a => a.actId === saved.actId);
      if (cont) {
        cont.style.display = '';
        cont.textContent = `▶ Continue · ${act?.title || saved.actId}`;
        cont.onclick = () => onContinue?.(saved);
      }
      if (delBtn) delBtn.style.display = '';
    } else {
      if (cont) cont.style.display = 'none';
      if (delBtn) delBtn.style.display = 'none';
    }
  } catch {
    if (cont) cont.style.display = 'none';
    if (delBtn) delBtn.style.display = 'none';
  }
  delBtn?.addEventListener('click', () => {
    deleteSave();
    if (cont) cont.style.display = 'none';
    delBtn.style.display = 'none';
    if (scoreEl && !getHighScore()) scoreEl.textContent = '';
  });

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

export function hideTitle() {
  document.getElementById('title')?.classList.add('hidden');
}

export function showTitle() {
  document.getElementById('title')?.classList.remove('hidden');
  hideDialogue();
  // Refresh high score + continue on show
  const hs = getHighScore();
  const se = document.getElementById('high-score');
  if (se) se.textContent = hs ? `🏆 Best: ${hs.kills} kills · ${hs.waves || '-'} waves` : '';
  try {
    const cont = document.getElementById('btn-continue');
    const del = document.getElementById('btn-delete-save');
    const saved = JSON.parse(window.localStorage.getItem('ramayana_web_save') || 'null');
    if (saved && saved.actId) {
      if (cont) { cont.style.display = ''; cont.textContent = `▶ Continue · ${saved.actId}`; }
      if (del) del.style.display = '';
    } else {
      if (cont) cont.style.display = 'none';
      if (del) del.style.display = 'none';
    }
  } catch {}
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
