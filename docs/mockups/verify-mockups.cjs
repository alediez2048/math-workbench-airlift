// Design-reference verification only; this does not test the Unity game.
// Requires Playwright in NODE_PATH (provided by the local Codex runtime here).
const { chromium } = require('playwright');
const assert = require('node:assert/strict');
const { join } = require('node:path');
const { pathToFileURL } = require('node:url');
(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1440, height: 1120 }, deviceScaleFactor: 1 });
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto(pathToFileURL(join(__dirname, 'index.html')).href);
  for (const [index, view] of ['arrival','build','equivalence','compare','departure'].entries()) {
    await page.locator(`[data-view="${view}"]`).click();
    assert.equal(await page.locator(`[data-view="${view}"]`).getAttribute('aria-pressed'), 'true');
    if (view === 'compare') await page.locator('#submit').click();
    await page.screenshot({ path: join(__dirname, `${String(index+1).padStart(2,'0')}-${view}.png`), fullPage: true });
  }
  await page.locator('[data-view="build"]').click();
  await page.locator('#remove').click();
  await page.locator('#submit').click();
  assert.match(await page.locator('#feedback').innerText(), /2\/4 long/);
  await page.locator('#add').click();
  await page.locator('#submit').click();
  assert.match(await page.locator('#feedback').innerText(), /task can advance/);
  await page.locator('[data-view="equivalence"]').click();
  await page.locator('#denominator').selectOption('4');
  await page.locator('#submit').click();
  assert.match(await page.locator('#feedback').innerText(), /Check each field/);
  await page.locator('#denominator').selectOption('8');
  await page.locator('#submit').click();
  assert.match(await page.locator('#feedback').innerText(), /3\/4 = 6\/8/);
  // Both physical lanes must end at the exact same coordinate.
  const endpoints = await page.locator('#scene rect[data-piece]').evaluateAll(pieces => {
    const byY = new Map();
    for (const p of pieces) {
      const y = p.getAttribute('y');
      byY.set(y, Math.max(byY.get(y) || 0, Number(p.getAttribute('x'))+Number(p.getAttribute('width'))));
    }
    return [...byY.values()];
  });
  assert.deepEqual(endpoints, [604,604]);
  await page.locator('[data-view="compare"]').click();
  await page.locator('#submit').click();
  assert.match(await page.locator('#feedback').innerText(), /equal eighth-pieces/);
  await page.locator('#reason').selectOption('benchmark');
  await page.locator('#benchmark').click();
  await page.locator('#submit').click();
  assert.match(await page.locator('#feedback').innerText(), /2\/8 and 1\/8/);
  await page.screenshot({ path: join(__dirname, '06-benchmark-repair.png'), fullPage: true });
  await page.locator('#comparison').selectOption('>');
  await page.locator('#submit').click();
  assert.match(await page.locator('#feedback').innerText(), /task can advance/);
  await page.locator('#reset').click();
  await page.locator('#reason').selectOption('unsure');
  await page.locator('#submit').click();
  assert.match(await page.locator('#feedback').innerText(), /not enough evidence/);
  for (const width of [1440, 800, 390, 320]) {
    await page.setViewportSize({ width, height: 1100 });
    for (const view of ['arrival','build','equivalence','compare','departure']) {
      await page.locator(`[data-view="${view}"]`).click();
      const overflow = await page.evaluate(() => document.documentElement.scrollWidth > innerWidth);
      assert.equal(overflow, false, `${view} overflows at ${width}px`);
    }
  }
  await page.setViewportSize({ width:390, height:844 });
  await page.locator('[data-view="compare"]').click();
  await page.screenshot({ path:join(__dirname, '07-mobile-review.png'), fullPage:true });
  assert.deepEqual(errors, []);
  console.log('PASS: five views, fraction geometry, wrong/repair paths, three comparison scaffolds, reset, four viewport widths, no browser errors. Seven reference PNGs captured.');
  await browser.close();
})().catch(error => { console.error(error); process.exitCode=1; });
