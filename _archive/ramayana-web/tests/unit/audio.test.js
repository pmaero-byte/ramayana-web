import { describe, it, expect, vi, beforeEach } from 'vitest';

describe('audio module (lazy sfx)', () => {
  beforeEach(() => {
    // Reset the module between tests so unlockAudio is freshly optional.
    vi.resetModules();
  });

  it('exports sfx functions that noop when AudioContext missing', async () => {
    // Force AudioContext to be undefined
    const oldAC = global.AudioContext;
    const oldOW = global.OscillatorNode;
    delete global.AudioContext;
    delete global.OscillatorNode;
    try {
      const audio = await import('../../js/core/audio.js?v=' + Date.now());
      expect(typeof audio.unlockAudio).toBe('function');
      expect(typeof audio.sfxBow).toBe('function');
      expect(typeof audio.sfxHit).toBe('function');
      expect(typeof audio.sfxDeath).toBe('function');
      expect(typeof audio.sfxBossRoar).toBe('function');
      expect(typeof audio.sfxGrowl).toBe('function');
      expect(typeof audio.startDrone).toBe('function');
      expect(typeof audio.stopDrone).toBe('function');
      // Should not throw when no audio context
      expect(() => audio.sfxBow()).not.toThrow();
      expect(() => audio.sfxHit()).not.toThrow();
      expect(() => audio.sfxDeath()).not.toThrow();
      expect(() => audio.sfxBossRoar()).not.toThrow();
      expect(() => audio.sfxGrowl()).not.toThrow();
      expect(() => audio.startDrone()).not.toThrow();
      expect(() => audio.stopDrone()).not.toThrow();
    } finally {
      if (oldAC) global.AudioContext = oldAC;
      if (oldOW) global.OscillatorNode = oldOW;
    }
  });
});
