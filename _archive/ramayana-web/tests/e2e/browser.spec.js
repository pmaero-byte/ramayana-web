// Playwright smoke test for ramayana-web (real browser)
// Boots the served URL, verifies title screen + click-through + combat.
// Requires a static server on http://127.0.0.1:8765 (see README).
import { chromium } from 'playwright';

const BASE = process.env.RAMAWEB_BASE || 'http://127.0.0.1:8765';

let passed = 0, failed = 0;
const ok = (name, cond, info = '') => {
  if (cond) { console.log('  PASS', name); passed++; }
  else { console.log('  FAIL', name, info); failed++; }
};

const wait = (page, fn, timeout = 6000, label = '') => page.waitForFunction(fn, null, { timeout }).catch((e) => {
  ok(label || 'waitForFunction', false, String(e).slice(0, 200));
  throw e;
});

(async () => {
  const browser = await chromium.launch({ headless: true });

  // Test 1: boot → title visible → Begin → wave 1 → damage sprite
  {
    const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });
    const consoleErrors = [];
    page.on('pageerror', (e) => consoleErrors.push(String(e)));
    page.on('console', (m) => { if (m.type() === 'error') consoleErrors.push(m.text()); });
    await page.goto(BASE + '/?v=' + Date.now(), { waitUntil: 'domcontentloaded' });
    const titleVisible = await page.evaluate(() => !!document.querySelector('#title h1'));
    ok('title screen visible', titleVisible);
    await wait(page, () => !!window.RamaWeb && !!window.RamaWeb.state, 6000, 'RamaWeb ready');
    const preState = await page.evaluate(() => ({
      running: window.RamaWeb?.state?.running,
      wave: window.RamaWeb?.state?.wave,
      titleHidden: document.getElementById('title')?.classList.contains('hidden'),
      btnText: document.getElementById('btn-start')?.textContent,
      charSelected: !!document.querySelector('#char-grid button'),
      actSelected: !!document.querySelector('.act-list [aria-selected], .act-list .act-item.selected'),
    }));
    console.log('  pre-click', JSON.stringify(preState));
    await page.locator('button:has-text("Begin")').click();
    await wait(page, () => document.getElementById('title')?.classList.contains('hidden'), 3000, 'title hidden');
    // Wave 1 fires via HUD label — the closure-local wave isn't mirrored on state.
    await wait(page, () => /Wave 1\//.test(document.getElementById('hud-wave')?.textContent || ''), 6000, 'wave 1 reached');
    const spriteCount = await page.waitForFunction(() => {
      let n = 0;
      window.RamaWeb.scene.traverse((o) => { if (o.isSprite) n++; });
      return n;
    }, null, { timeout: 8000 }).then((h) => h.jsonValue());
    ok('damage number sprite fires', spriteCount >= 1, `count=${spriteCount}`);
    const fatal = consoleErrors.filter((e) => !/Failed to fetch|net::ERR|abort/i.test(e));
    ok('no fatal JS errors', fatal.length === 0, fatal.join('|').slice(0, 200));
    await page.close();
  }

  // Test 2: shift hold shows aim line
  {
    const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });
    await page.goto(BASE + '/?v=' + Date.now(), { waitUntil: 'domcontentloaded' });
    await wait(page, () => !!window.RamaWeb && !!window.RamaWeb.state, 6000, 'RamaWeb ready');
    await page.locator('button:has-text("Begin")').click();
    await wait(page, () => /Wave 1\//.test(document.getElementById('hud-wave')?.textContent || ''), 6000, 'wave 1');
    await page.keyboard.down('Shift');
    const aimVisible = await page.waitForFunction(() => {
      let has = false;
      window.RamaWeb.scene.traverse((o) => {
        if (o.type === 'Line' && o.visible && o.material && o.material.opacity > 0) has = true;
      });
      return has;
    }, null, { timeout: 5000 }).then((h) => h.jsonValue());
    ok('shift shows aim line', !!aimVisible);
    await page.keyboard.up('Shift');
    await page.close();
  }

  // Test 3: combat scoring
  {
    const page = await browser.newPage({ viewport: { width: 1280, height: 720 } });
    await page.goto(BASE + '/?v=' + Date.now(), { waitUntil: 'domcontentloaded' });
    await wait(page, () => !!window.RamaWeb && !!window.RamaWeb.state, 6000, 'RamaWeb ready');
    await page.locator('button:has-text("Begin")').click();
    await wait(page, () => /Wave 1\//.test(document.getElementById('hud-wave')?.textContent || ''), 6000, 'wave 1');
    await page.waitForFunction(() => {
      const t = document.getElementById('hud-wave')?.textContent || '';
      const m = t.match(/(\d+)\s*kills/);
      return m && Number(m[1]) > 0;
    }, null, { timeout: 15000 });
    const kills = await page.evaluate(() => {
      const t = document.getElementById('hud-wave')?.textContent || '';
      return Number((t.match(/(\d+)\s*kills/) || [0, 0])[1]);
    });
    ok('kills > 0 after wave start', kills > 0, `kills=${kills}`);
    await page.close();
  }

  await browser.close();
  console.log(`\n  ${passed} passed, ${failed} failed`);
  console.log('  ramayana-web e2e smoke — RUN by `npm run test:e2e` against a live static server');
  process.exit(failed ? 1 : 0);
})().catch((err) => {
  console.error('e2e suite crashed:', err);
  process.exit(1);
});
