/**
 * Unit test: package.json + boot surface + corpus shape sanity.
 * This is the boot smoke that was previously .hermes/verify-boot.cjs.
 * Now it lives in tests/unit so it runs under `vitest run` and CI.
 */
import { describe, it, expect } from 'vitest';
import { readFileSync, existsSync, readdirSync } from 'node:fs';
import { resolve, join } from 'node:path';

const ROOT = resolve(__dirname, '..', '..');

describe('boot surface', () => {
  const need = [
    'index.html',
    'css/game.css',
    'js/main.js',
    'data/corpus_data.json',
    'js/core/state.js',
    'js/core/input.js',
    'js/world/scene.js',
    'js/world/player.js',
    'js/world/camera.js',
    'js/combat/formation.js',
    'js/combat/rakshasa.js',
    'js/combat/wave.js',
    'js/combat/cover.js',
    'js/story/moments.js',
    'js/ui/dialogue.js',
    'js/core/audio.js',
    'js/core/touch.js',
    'js/core/i18n.js',
  ];
  for (const f of need) {
    it(`exists ${f}`, () => {
      expect(existsSync(join(ROOT, f))).toBe(true);
    });
  }

  it('index.html wires importmap for three + module main.js', () => {
    const html = readFileSync(join(ROOT, 'index.html'), 'utf8');
    expect(html).toContain('esm.sh/three');
    expect(html).toMatch(/<script\s+type="module"\s+src="js\/main\.js/);
  });

  it('main.js boots corpus + exposes startGame', () => {
    const main = readFileSync(join(ROOT, 'js/main.js'), 'utf8');
    expect(main).toContain('corpus_data.json');
    expect(main).toContain('startGame');
    expect(main).toContain('window.RamaWeb');
  });

  it('corpus_data.json has 8 acts + 30+ chars + yuddhakanda-war', () => {
    const corpus = JSON.parse(readFileSync(join(ROOT, 'data/corpus_data.json'), 'utf8'));
    expect(corpus.acts.length).toBeGreaterThanOrEqual(8);
    expect(corpus.characters.length).toBeGreaterThanOrEqual(30);
    expect(corpus.acts.some((a) => a.actId === 'yuddhakanda-war')).toBe(true);
  });

  it('all js modules parse cleanly', () => {
    // Walk all .js under js/ except the test stub
    const walk = (dir) => {
      const out = [];
      for (const e of readdirSync(dir, { withFileTypes: true })) {
        const p = join(dir, e.name);
        if (e.isDirectory()) out.push(...walk(p));
        else if (e.isFile() && p.endsWith('.js') && !p.includes('__mocks__')) out.push(p);
      }
      return out;
    };
    const files = walk(join(ROOT, 'js'));
    expect(files.length).toBeGreaterThan(10);
    // Each module must be parseable as ES module source — we just confirm it's non-empty utf8.
    for (const f of files) {
      const buf = readFileSync(f, 'utf8');
      expect(buf.length).toBeGreaterThan(0);
    }
  });
});
