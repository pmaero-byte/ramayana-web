/** On-screen joystick + jump for touch / trackpad-challenged laptops */
export function createTouchPad(inputApi) {
  const root = document.createElement('div');
  root.id = 'touch-pad';
  root.innerHTML = `
    <div class="stick-base" id="stick-base"><div class="stick-knob" id="stick-knob"></div></div>
    <button type="button" class="touch-btn jump" id="btn-jump" aria-label="Jump">⤒</button>
  `;
  document.getElementById('app')?.appendChild(root);

  const base = root.querySelector('#stick-base');
  const knob = root.querySelector('#stick-knob');
  let active = false;
  let cx = 0;
  let cy = 0;
  const maxR = 42;
  const mobile = { x: 0, z: 0 };
  let jumpQueued = false;

  function setKnob(dx, dy) {
    const len = Math.hypot(dx, dy) || 1;
    const c = Math.min(1, len / maxR);
    const nx = (dx / len) * c * maxR;
    const ny = (dy / len) * c * maxR;
    knob.style.transform = `translate(${nx}px, ${ny}px)`;
    mobile.x = nx / maxR;
    mobile.z = ny / maxR;
  }

  function onStart(e) {
    const t = e.changedTouches ? e.changedTouches[0] : e;
    const r = base.getBoundingClientRect();
    cx = r.left + r.width / 2;
    cy = r.top + r.height / 2;
    active = true;
    setKnob(t.clientX - cx, t.clientY - cy);
    e.preventDefault();
  }
  function onMove(e) {
    if (!active) return;
    const t = e.changedTouches ? e.changedTouches[0] : e;
    setKnob(t.clientX - cx, t.clientY - cy);
    e.preventDefault();
  }
  function onEnd() {
    active = false;
    mobile.x = 0;
    mobile.z = 0;
    knob.style.transform = 'translate(0,0)';
  }

  base.addEventListener('touchstart', onStart, { passive: false });
  base.addEventListener('touchmove', onMove, { passive: false });
  base.addEventListener('touchend', onEnd);
  base.addEventListener('mousedown', onStart);
  window.addEventListener('mousemove', onMove);
  window.addEventListener('mouseup', onEnd);

  document.getElementById('btn-jump')?.addEventListener('pointerdown', (e) => {
    e.preventDefault();
    jumpQueued = true;
  });

  const orig = inputApi.moveVector.bind(inputApi);
  inputApi.moveVector = () => {
    const k = orig();
    let x = k.x + mobile.x;
    let z = k.z + mobile.z;
    const mag = Math.min(1, Math.hypot(x, z));
    const len = Math.hypot(x, z) || 1;
    if (mag < 0.001) return { x: 0, z: 0, mag: 0 };
    return { x: (x / len) * mag, z: (z / len) * mag, mag };
  };

  Object.defineProperty(inputApi, 'jump', {
    configurable: true,
    get() {
      if (jumpQueued) {
        jumpQueued = false;
        return true;
      }
      return inputApi.keys.has('Space');
    },
  });

  return { root };
}
