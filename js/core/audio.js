/** Minimal Web Audio SFX — zero deps */
let ctx = null;

function ac() {
  if (!ctx) {
    const AC = window.AudioContext || window.webkitAudioContext;
    if (!AC) return null;
    ctx = new AC();
  }
  if (ctx.state === 'suspended') ctx.resume();
  return ctx;
}

export function unlockAudio() {
  ac();
}

function beep({ freq = 440, dur = 0.08, type = 'sine', gain = 0.08, slide = 0 }) {
  const c = ac();
  if (!c) return;
  const t0 = c.currentTime;
  const o = c.createOscillator();
  const g = c.createGain();
  o.type = type;
  o.frequency.setValueAtTime(freq, t0);
  if (slide) o.frequency.exponentialRampToValueAtTime(Math.max(40, freq + slide), t0 + dur);
  g.gain.setValueAtTime(gain, t0);
  g.gain.exponentialRampToValueAtTime(0.001, t0 + dur);
  o.connect(g);
  g.connect(c.destination);
  o.start(t0);
  o.stop(t0 + dur + 0.02);
}

export function sfxBow() {
  beep({ freq: 880, dur: 0.06, type: 'triangle', gain: 0.05, slide: -400 });
}
export function sfxHit() {
  beep({ freq: 160, dur: 0.1, type: 'square', gain: 0.07, slide: -80 });
}
export function sfxWave() {
  beep({ freq: 120, dur: 0.25, type: 'sawtooth', gain: 0.04, slide: 80 });
  setTimeout(() => beep({ freq: 90, dur: 0.2, type: 'sine', gain: 0.05 }), 80);
}
export function sfxCue() {
  beep({ freq: 520, dur: 0.12, type: 'sine', gain: 0.045 });
  setTimeout(() => beep({ freq: 660, dur: 0.14, type: 'sine', gain: 0.04 }), 90);
}
export function sfxWin() {
  [523, 659, 784].forEach((f, i) => setTimeout(() => beep({ freq: f, dur: 0.18, type: 'triangle', gain: 0.05 }), i * 120));
}
