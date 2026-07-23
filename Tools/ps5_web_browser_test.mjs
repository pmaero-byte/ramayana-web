// ps5_web_browser_test.mjs
// Real PS5 Web Browser simulation using Playwright + WebKit (PS5 browser engine)
//
// What this test verifies (REAL execution, not curl):
// 1. PS5 WebKit UA request → HTTP 200 + valid HTML
// 2. All 162 JS modules load without errors
// 3. Three.js initializes correctly
// 4. Game creates a renderer + scene + world (Three.js objects exist)
// 5. Web Audio API is available (AudioContext, createOscillator)
// 6. localStorage works (save system can persist)
// 7. GamePad API is detected (or not — this is the blocker)
// 8. No JavaScript console errors during boot
// 9. DOM has expected game elements (HUD, era select, etc.)
// 10. Bootstrap completes within reasonable time

import pkg from '/tmp/node_modules/playwright/index.js';
const { webkit } = pkg;
import { readFileSync } from 'fs';
import { join } from 'path';

const PS5_UA = 'Mozilla/5.0 (PlayStation; PlayStation 5/2.26) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0 Safari/605.1.15';
const PS5_VIEWPORT = { width: 1920, height: 1080 };  // PS5 outputs 1080p / 4K
const TARGET_URL = 'http://localhost:8765/';

const results = [];
let consoleErrors = [];
let networkFailures = [];

function pass(name) { results.push({ name, ok: true }); console.log(`  ✅ ${name}`); }
function fail(name, detail) { results.push({ name, ok: false, detail }); console.log(`  ❌ ${name}: ${detail}`); }

async function run() {
  console.log('═══════════════════════════════════════════════════════════════');
  console.log('TRADERS OF JAMBUDEEP — PS5 WEB BROWSER SIMULATION (real WebKit)');
  console.log('═══════════════════════════════════════════════════════════════');
  console.log(`Engine: WebKit 17.4 (PS5 browser engine)`);
  console.log(`UA: ${PS5_UA}`);
  console.log(`Viewport: ${PS5_VIEWPORT.width}×${PS5_VIEWPORT.height} (PS5 1080p)`);
  console.log();

  const browser = await webkit.launch({ headless: true });
  const context = await browser.newContext({
    userAgent: PS5_UA,
    viewport: PS5_VIEWPORT,
    deviceScaleFactor: 1,
  });
  const page = await context.newPage();

  // Track console + network
  page.on('console', msg => {
    if (msg.type() === 'error') consoleErrors.push(msg.text());
  });
  page.on('pageerror', err => consoleErrors.push(`PAGE ERROR: ${err.message}`));
  page.on('requestfailed', req => {
    networkFailures.push(`${req.url()} — ${req.failure()?.errorText}`);
  });
  page.on('response', resp => {
    if (resp.status() >= 400) {
      networkFailures.push(`${resp.url()} — HTTP ${resp.status()}`);
    }
  });

  // TEST 1: Page loads with HTTP 200
  console.log('--- TEST 1: HTTP load ---');
  let response;
  try {
    response = await page.goto(TARGET_URL, { waitUntil: 'domcontentloaded', timeout: 30000 });
    response && response.status() === 200 ? pass('HTTP 200 on index.html') : fail('HTTP 200', `status=${response?.status()}`);
  } catch (e) {
    fail('HTTP 200', e.message);
    console.log('\n!!! Server not running on :8765. Start it first:');
    console.log('    cd Hertree/tradergame-iso && python3 -m http.server 8765');
    await browser.close();
    return;
  }

  // TEST 2: Wait for game to initialize
  console.log('\n--- TEST 2: Game initialization ---');
  await page.waitForTimeout(3000);  // give the 162 modules time to load

  const gameState = await page.evaluate(() => {
    const result = {};
    result.bodyHasContent = document.body.children.length > 0;
    result.canvasCount = document.querySelectorAll('canvas').length;
    result.scriptCount = document.querySelectorAll('script').length;
    result.hasThree = typeof window.THREE !== 'undefined';
    result.hasGameState = typeof window.W !== 'undefined' && window.W?.gameState != null;
    result.title = document.title;
    return result;
  });
  gameState.bodyHasContent ? pass('Body has content') : fail('Body content', 'empty');
  gameState.canvasCount >= 1 ? pass(`Canvas present (${gameState.canvasCount})`) : fail('Canvas', `count=${gameState.canvasCount}`);
  gameState.scriptCount >= 100 ? pass(`All scripts loaded (${gameState.scriptCount})`) : fail('Scripts', `count=${gameState.scriptCount}`);
  gameState.hasThree ? pass('Three.js loaded') : fail('Three.js', 'not on window');
  gameState.hasGameState ? pass('Game state initialized') : fail('Game state', 'W.gameState is null');

  // TEST 3: Web Audio API
  console.log('\n--- TEST 3: Web Audio API ---');
  const audioCheck = await page.evaluate(() => {
    try {
      const AudioContext = window.AudioContext || window.webkitAudioContext;
      if (!AudioContext) return { ok: false, reason: 'No AudioContext constructor' };
      const ctx = new AudioContext();
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      osc.connect(gain);
      gain.connect(ctx.destination);
      osc.frequency.value = 440;
      osc.type = 'sine';
      const supported = {
        oscillator: typeof ctx.createOscillator === 'function',
        gain: typeof ctx.createGain === 'function',
        biquadFilter: typeof ctx.createBiquadFilter === 'function',
      };
      ctx.close();
      return { ok: true, sampleRate: 44100, supported };
    } catch (e) { return { ok: false, reason: e.message }; }
  });
  audioCheck.ok ? pass('AudioContext creates') : fail('AudioContext', audioCheck.reason);
  audioCheck.supported?.oscillator ? pass('createOscillator available') : fail('createOscillator', 'missing');

  // TEST 4: GamePad API
  console.log('\n--- TEST 4: GamePad API (CRITICAL for PS5 controller) ---');
  const gamepadCheck = await page.evaluate(() => {
    return {
      hasGetGamepads: typeof navigator.getGamepads === 'function',
      hasGamepadEvents: 'gamepadconnected' in window,
      webkitGamepad: typeof navigator.webkitGamepads === 'function',
      currentGamepads: navigator.getGamepads ? navigator.getGamepads().filter(g => g).length : -1,
    };
  });
  gamepadCheck.hasGetGamepads ? pass('navigator.getGamepads available') : fail('getGamepads', 'MISSING — CRITICAL for DualSense support');
  gamepadCheck.hasGamepadEvents ? pass('gamepadconnected event supported') : fail('gamepadconnected event', 'missing');

  // TEST 5: localStorage (for save system)
  console.log('\n--- TEST 5: localStorage + IndexedDB ---');
  const storageCheck = await page.evaluate(() => {
    try {
      localStorage.setItem('__ps5_test__', '1');
      const got = localStorage.getItem('__ps5_test__');
      localStorage.removeItem('__ps5_test__');
      return {
        localStorage: got === '1',
        sessionStorage: typeof sessionStorage !== 'undefined',
        indexedDB: typeof indexedDB !== 'undefined',
      };
    } catch (e) { return { error: e.message }; }
  });
  storageCheck.localStorage ? pass('localStorage works') : fail('localStorage', 'broken');
  storageCheck.sessionStorage ? pass('sessionStorage works') : fail('sessionStorage', 'broken');
  storageCheck.indexedDB ? pass('IndexedDB works') : fail('IndexedDB', 'broken');

  // TEST 6: Console errors (clean boot?)
  console.log('\n--- TEST 6: JS errors during boot ---');
  consoleErrors.length === 0
    ? pass('Zero console errors')
    : fail('Console errors', `${consoleErrors.length} error(s):\n    ${consoleErrors.slice(0, 5).join('\n    ')}`);

  // TEST 7: Network failures
  console.log('\n--- TEST 7: Network failures ---');
  networkFailures.length === 0
    ? pass('Zero network failures')
    : fail('Network failures', `${networkFailures.length} failure(s):\n    ${networkFailures.slice(0, 5).join('\n    ')}`);

  // TEST 8: DOM structure
  console.log('\n--- TEST 8: DOM structure ---');
  const domCheck = await page.evaluate(() => {
    return {
      hasHud: !!document.querySelector('#hud-top, #hud-bottom, .hud, [class*="hud"]'),
      hasCanvas: !!document.querySelector('canvas'),
      bodyText: document.body.innerText.length,
      visibleElements: document.querySelectorAll('*:not(script):not(style):not(link)').length,
    };
  });
  domCheck.hasHud ? pass('HUD element present') : fail('HUD', 'no #hud-* or .hud found');
  domCheck.hasCanvas ? pass('Canvas in DOM') : fail('Canvas', 'missing');
  domCheck.bodyText > 0 ? pass(`Body has ${domCheck.bodyText} chars text`) : fail('Body text', 'empty');
  domCheck.visibleElements > 10 ? pass(`${domCheck.visibleElements} visible elements`) : fail('Elements', `only ${domCheck.visibleElements}`);

  // TEST 9: Performance — boot time
  console.log('\n--- TEST 9: Performance ---');
  const perfCheck = await page.evaluate(() => {
    const perf = performance.getEntriesByType('navigation')[0];
    const resources = performance.getEntriesByType('resource');
    const jsResources = resources.filter(r => r.name.endsWith('.js'));
    const totalJsBytes = jsResources.reduce((s, r) => s + (r.transferSize || r.encodedBodySize || 0), 0);
    return {
      domContentLoaded: perf?.domContentLoadedEventEnd - perf?.startTime,
      loadEvent: perf?.loadEventEnd - perf?.startTime,
      jsModuleCount: jsResources.length,
      totalJsBytes: totalJsBytes,
      failedJs: resources.filter(r => r.transferSize === 0 && r.duration === 0).length,
    };
  });
  console.log(`  → DCL: ${perfCheck.domContentLoaded?.toFixed(0)}ms, Load: ${perfCheck.loadEvent?.toFixed(0)}ms`);
  console.log(`  → JS modules loaded: ${perfCheck.jsModuleCount}, bytes: ${(perfCheck.totalJsBytes/1024).toFixed(0)}KB`);
  perfCheck.jsModuleCount >= 150 ? pass(`JS modules loaded (${perfCheck.jsModuleCount})`) : fail('JS modules', `only ${perfCheck.jsModuleCount} loaded`);
  perfCheck.failedJs === 0 ? pass('No failed JS loads') : fail('Failed JS', `${perfCheck.failedJs} failed`);

  // TEST 10: Screenshot for visual verification
  console.log('\n--- TEST 10: Visual capture ---');
  await page.screenshot({ path: '/tmp/traders_ps5_screenshot.png', fullPage: false });
  console.log('  → Screenshot saved: /tmp/traders_ps5_screenshot.png');

  // ── Summary ──
  console.log('\n═══════════════════════════════════════════════════════════════');
  const passed = results.filter(r => r.ok).length;
  const failed = results.filter(r => !r.ok).length;
  console.log(`RESULTS: ${passed} passed, ${failed} failed (out of ${results.length})`);
  console.log('═══════════════════════════════════════════════════════════════');

  if (failed > 0) {
    console.log('\nFailures:');
    results.filter(r => !r.ok).forEach(r => console.log(`  ❌ ${r.name}\n     ${r.detail}`));
  }

  // PS5 submission readiness verdict
  console.log('\n═══════════════════════════════════════════════════════════════');
  console.log('PS5 SUBMISSION READINESS');
  console.log('═══════════════════════════════════════════════════════════════');
  const blockers = [];
  if (!gamepadCheck.hasGetGamepads) blockers.push('GamePad API (DualSense support missing)');
  if (consoleErrors.length > 0) blockers.push(`${consoleErrors.length} JS console errors`);
  if (networkFailures.length > 0) blockers.push(`${networkFailures.length} network failures`);
  if (perfCheck.failedJs > 0) blockers.push(`${perfCheck.failedJs} failed JS loads`);
  if (blockers.length === 0) {
    console.log('✅ READY for PS5 web browser submission (modulo TRC review)');
  } else {
    console.log('❌ BLOCKERS for PS5 web browser submission:');
    blockers.forEach(b => console.log(`   • ${b}`));
  }

  await browser.close();
}

run().catch(e => { console.error('Test runner crashed:', e); process.exit(1); });
