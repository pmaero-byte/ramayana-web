/** Minimal Web Audio SFX + tanpura drone — zero deps */
let ctx = null;
let droneNodes = null;

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

/** Tanpura drone — 5 oscillators, calls unlockAudio internally */
export function startDrone() {
  const c = ac();
  if (!c || droneNodes) return;
  const base = 65; // ~C2 — roughly sa of a low male shruti
  const freqs = [base, base * 2, base * 3, base * 4, base * 6];
  const gains = [0.028, 0.022, 0.018, 0.014, 0.010];
  const t0 = c.currentTime;
  const master = c.createGain();
  master.gain.setValueAtTime(0.25, t0);
  master.gain.linearRampToValueAtTime(0.08, t0 + 2);
  master.connect(c.destination);
  const oscs = freqs.map((f, i) => {
    const o = c.createOscillator();
    o.type = 'sawtooth';
    o.frequency.setValueAtTime(f, t0);
    o.frequency.linearRampToValueAtTime(f + 0.3, t0 + 3.5);
    o.frequency.linearRampToValueAtTime(f - 0.15, t0 + 7);
    const g = c.createGain();
    g.gain.setValueAtTime(gains[i], t0);
    g.gain.linearRampToValueAtTime(gains[i] * 0.7, t0 + 1.5);
    o.connect(g);
    g.connect(master);
    o.start(t0);
    return { osc: o, gain: g };
  });
  droneNodes = { master, oscs };

  // slow LFO on master for movement
  const lfo = c.createOscillator();
  lfo.frequency.value = 2.3;
  const lfoG = c.createGain();
  lfoG.gain.value = 0.012;
  lfo.connect(lfoG);
  lfoG.connect(master.gain);
  lfo.start();
  droneNodes.lfo = lfo;
}

export function stopDrone() {
  if (!droneNodes) return;
  const c = ac();
  if (!c) return;
  const t = c.currentTime + 0.8;
  for (const { osc, gain } of droneNodes.oscs) {
    gain.gain.linearRampToValueAtTime(0.001, t);
    osc.stop(t + 0.05);
  }
  droneNodes.master.disconnect();
  droneNodes.lfo?.stop(t);
  droneNodes = null;
}
