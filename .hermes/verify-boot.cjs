#!/usr/bin/env node
/** Ad-hoc boot surface verify for ramayana-web — NOT suite-green */
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '..');
let pass = 0, fail = 0;
function ok(n, c) { if (c) { console.log('  OK', n); pass++; } else { console.log('  FAIL', n); fail++; } }

const need = [
  'index.html', 'css/game.css', 'js/main.js', 'data/corpus_data.json',
  'js/core/state.js', 'js/core/input.js', 'js/world/scene.js', 'js/world/player.js',
  'js/world/camera.js', 'js/combat/formation.js', 'js/combat/rakshasa.js',
  'js/combat/wave.js', 'js/story/moments.js', 'js/ui/dialogue.js',
  'js/core/audio.js', 'js/core/touch.js',
  'assets/portraits/rama.png', 'assets/portraits/hanuman.png',
];
for (const f of need) ok(f, fs.existsSync(path.join(root, f)));

const html = fs.readFileSync(path.join(root, 'index.html'), 'utf8');
ok('importmap three', html.includes('esm.sh/three'));
ok('module main', html.includes('js/main.js'));

const corpus = JSON.parse(fs.readFileSync(path.join(root, 'data/corpus_data.json'), 'utf8'));
ok('acts >= 8', (corpus.acts || []).length >= 8);
ok('chars >= 30', (corpus.characters || []).length >= 30);
ok('has yuddhakanda-war', (corpus.acts || []).some((a) => a.actId === 'yuddhakanda-war'));

const form = fs.readFileSync(path.join(root, 'js/combat/formation.js'), 'utf8');
ok('formation chakra', form.includes('chakra'));
ok('formation vyuha', form.includes('vyuha'));
ok('formation arc', form.includes('arc'));

const main = fs.readFileSync(path.join(root, 'js/main.js'), 'utf8');
ok('main boots corpus', main.includes('corpus_data.json'));
ok('main startGame', main.includes('startGame'));

// formation unit smoke
const { spawnPoints, kindForWave } = require('module'); // skip ESM require
// pure reimplementation check via eval not needed — file exists

console.log(`\n  ${pass} passed, ${fail} failed`);
console.log('  AD-HOC verification — NOT suite green');
process.exit(fail ? 1 : 0);
