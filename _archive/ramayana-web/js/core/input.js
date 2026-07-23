/** Keyboard + mouse orbit input */
export function createInput(dom) {
  const keys = new Set();
  const orbit = { active: false, dx: 0, dy: 0 };
  let yaw = 0;
  let pitch = 0.18;

  const onKey = (e, down) => {
    const k = e.key.toLowerCase();
    if (['w', 'a', 's', 'd', 'arrowup', 'arrowdown', 'arrowleft', 'arrowright', ' ', 'shift'].includes(k) ||
        e.code === 'Space') {
      e.preventDefault();
    }
    if (down) keys.add(e.code);
    else keys.delete(e.code);
  };

  window.addEventListener('keydown', (e) => onKey(e, true));
  window.addEventListener('keyup', (e) => onKey(e, false));

  const startOrbit = (e) => {
    if (e.button === 2 || e.button === 1 || e.altKey) {
      orbit.active = true;
      dom.style.cursor = 'grabbing';
    }
  };
  const moveOrbit = (e) => {
    if (!orbit.active) return;
    yaw -= e.movementX * 0.004;
    pitch = Math.max(-0.35, Math.min(0.55, pitch - e.movementY * 0.003));
  };
  const endOrbit = () => {
    orbit.active = false;
    dom.style.cursor = '';
  };

  dom.addEventListener('mousedown', startOrbit);
  window.addEventListener('mousemove', moveOrbit);
  window.addEventListener('mouseup', endOrbit);
  dom.addEventListener('contextmenu', (e) => e.preventDefault());

  function moveVector() {
    let x = 0;
    let z = 0;
    if (keys.has('KeyW') || keys.has('ArrowUp')) z -= 1;
    if (keys.has('KeyS') || keys.has('ArrowDown')) z += 1;
    if (keys.has('KeyA') || keys.has('ArrowLeft')) x -= 1;
    if (keys.has('KeyD') || keys.has('ArrowRight')) x += 1;
    const len = Math.hypot(x, z) || 1;
    return { x: x / len, z: z / len, mag: Math.min(1, Math.hypot(x, z)) };
  }

  return {
    keys,
    moveVector,
    get yaw() { return yaw; },
    get pitch() { return pitch; },
    get run() { return keys.has('ShiftLeft') || keys.has('ShiftRight'); },
    get jump() { return keys.has('Space'); },
  };
}
