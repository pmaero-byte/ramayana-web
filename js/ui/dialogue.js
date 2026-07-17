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

export function buildTitle(corpus, onSelect, onStart) {
  const list = document.getElementById('act-list');
  const btn = document.getElementById('btn-start');
  if (!list || !corpus?.acts) return;
  list.innerHTML = '';
  let selected = corpus.acts.find((a) => a.actId === 'yuddhakanda-war')?.actId || corpus.acts[0]?.actId;
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

function esc(s) {
  return String(s).replace(/[&<>"']/g, (c) => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;',
  }[c]));
}
