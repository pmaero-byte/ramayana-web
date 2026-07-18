#!/usr/bin/env node
/**
 * hermes-verify-ramayana-web-test.mjs
 *
 * Pure-ESM unit smoke for the formation logic + i18n + slot helpers.
 * No test runner — just a hand-rolled, ad-hoc, NOT-suite-green checker.
 */
import { spawnPoints, kindForWave } from '../js/combat/formation.js';
import { setLocale, t, translateCorpus, availableLocales } from '../js/core/i18n.js';

let pass = 0, fail = 0;
const ok = (name, cond) => {
  if (cond) { console.log('  OK', name); pass++; }
  else { console.log('  FAIL', name); fail++; }
};

// formation
const o = { x: 0, y: 0, z: 0 };
const f = { x: 0, y: 0, z: 1 };
ok('arc: 4 pts', spawnPoints('arc', 4, o, f, 6).length === 4);
ok('chakra: 8 pts', spawnPoints('chakra', 8, o, f, 6).length === 8);
ok('vyuha: 6 pts', spawnPoints('vyuha', 6, o, f, 6).length === 6);
ok('kindForWave 1 = arc', kindForWave(1) === 'arc');
ok('kindForWave 2 = vyuha', kindForWave(2) === 'vyuha');
ok('kindForWave 3+ = chakra', kindForWave(3) === 'chakra');
ok('unknown kind fallback arc', spawnPoints('unknown', 3, o, f, 6).length === 3);

// i18n
ok('en default', t('hp.label') === 'HP {hp}/{max}' || t('hp.label', { hp: 5, max: 5 }) === 'HP 5/5');
setLocale('sa');
ok('sa translated', t('hp.label', { hp: 3, max: 5 }) === 'प्राणाः 3/5');
setLocale('en');
ok('switched back', t('hp.label') === 'HP {hp}/{max}' || t('hp.label', { hp: 5, max: 5 }) === 'HP 5/5');
ok('locales >= 2', availableLocales().length >= 2);
ok('translateCorpus char', translateCorpus('Rama', 'char') === 'Rama');
ok('translateCorpus char sa', (() => {
  setLocale('sa');
  const r = translateCorpus('Rama', 'char');
  setLocale('en');
  return r === 'रामः';
})());

console.log(`\n  ${pass} passed, ${fail} failed`);
console.log('  AD-HOC ESM smoke — NOT suite green');
process.exit(fail ? 1 : 0);
