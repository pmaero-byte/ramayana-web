/** Story moment walker over corpus acts */
export function createStory(corpus) {
  let act = null;
  let index = -1;
  const listeners = { enter: [], complete: [], actDone: [] };

  function on(ev, fn) {
    listeners[ev]?.push(fn);
  }
  function emit(ev, payload) {
    for (const fn of listeners[ev] || []) fn(payload);
  }

  function loadAct(actId) {
    act = (corpus.acts || []).find((a) => a.actId === actId) || null;
    index = -1;
    if (!act) return false;
    advance();
    return true;
  }

  function advance() {
    if (!act?.objectives?.length) return false;
    index += 1;
    if (index >= act.objectives.length) {
      emit('actDone', act.actId);
      return false;
    }
    const obj = act.objectives[index];
    emit('enter', obj);
    return true;
  }

  function completeCurrent() {
    if (!act || index < 0 || index >= act.objectives.length) return;
    const obj = act.objectives[index];
    emit('complete', obj);
  }

  return {
    loadAct,
    advance,
    completeCurrent,
    on,
    get act() { return act; },
    get current() {
      return act?.objectives?.[index] || null;
    },
    get playerRole() {
      return act?.playerRole || 'Rama';
    },
  };
}

export function portraitUrl(speaker) {
  const id = normalizeSpeaker(speaker);
  return `assets/portraits/${id}.png`;
}

function normalizeSpeaker(s) {
  let k = String(s || 'rama').trim().toLowerCase();
  k = k.replace(/lord |prince |king /g, '');
  if (k === 'raghava' || k === 'ramachandra') k = 'rama';
  if (k === 'janaki' || k === 'vaidehi') k = 'sita';
  if (k === 'anjaneya' || k === 'maruti') k = 'hanuman';
  if (k === 'saumitri') k = 'lakshmana';
  const known = ['rama', 'sita', 'hanuman', 'lakshmana', 'ravana'];
  if (!known.includes(k)) k = 'rama';
  return k;
}
