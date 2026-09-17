import test from 'node:test';
import assert from 'node:assert/strict';
import { validateSessionRequest, createLimiter, CLIENT_MARKER } from '../lib/validate.js';
import { buildSessionConfig, TOOLS, AGE_BANDS, CARD_IDS, LESSON_TOOL_NAMES, CAFE_TOOL_NAMES, GARDEN_TOOL_NAMES } from '../lib/sessionConfig.js';
import { createHandler } from '../api/session.js';

const good = { method: 'POST', headers: { 'x-nerdy-client': CLIENT_MARKER }, body: { launchNonce: 'abc12345-nonce', build: '0.2.0' } };
function res() { const r = { code: 0, body: null, status(c) { r.code = c; return r; }, json(b) { r.body = b; return r; } }; return r; }
const okFetch = async () => ({ ok: true, status: 200, json: async () => ({ value: 'ek_test_secret', expires_at: 123, session: {} }) });

test('validation rejects wrong method, client marker, nonce and build', () => {
  assert.equal(validateSessionRequest('GET', good.headers, good.body).status, 405);
  assert.equal(validateSessionRequest('POST', {}, good.body).status, 403);
  assert.equal(validateSessionRequest('POST', good.headers, { launchNonce: 'x', build: '1' }).status, 400);
  assert.equal(validateSessionRequest('POST', good.headers, { launchNonce: 'abc12345-nonce', build: 'x'.repeat(50) }).status, 400);
  assert.equal(validateSessionRequest('POST', good.headers, good.body).ok, true);
});

test('session config is server-authored: English, no surroundings, tools present, VAD tuned', () => {
  const cfg = buildSessionConfig().session;
  assert.match(cfg.instructions, /English only/);
  assert.match(cfg.instructions, /Never describe surroundings/);
  assert.match(cfg.instructions, /Never ask for the learner's name/);
  assert.match(cfg.instructions, /Do not ask the welcome questions yourself/);
  assert.match(cfg.instructions, /Only speak when the app prompts you/);
  assert.deepEqual(cfg.tools.map(t => t.name), ['record_profile', 'end_welcome', 'describe_card', 'request_help', 'advance_step', 'open_lesson',
    'replay_demo', 'split_cargo', 'check_load', 'reset_cargo', 'next_chapter', 'restart_chapter', 'back_to_lessons',
    'cafe_deal_round', 'cafe_check_order', 'cafe_clear_table', 'garden_turn_bed', 'garden_split_bed', 'garden_check_bed', 'garden_clear_bed']);
  assert.match(cfg.instructions, /call advance_step/);
  assert.equal(cfg.audio.input.turn_detection.threshold, 0.6);
  assert.equal(cfg.audio.input.transcription.language, 'en');
  const profile = TOOLS[0].parameters.properties;
  assert.deepEqual(profile.ageBand.enum, AGE_BANDS);
  assert.equal(profile.interests.maxItems, 5);
});

test('voice can open a lesson card and speech is grounded in the current step', () => {
  const cfg = buildSessionConfig().session;
  const open = cfg.tools.find(t => t.name === 'open_lesson');
  assert.ok(open, 'open_lesson tool');
  assert.deepEqual(open.parameters.properties.cardId.enum, CARD_IDS);
  assert.deepEqual(open.parameters.required, ['cardId']);
  assert.match(cfg.instructions, /call open_lesson/);
  assert.match(cfg.instructions, /lesson_state/);
  assert.match(cfg.instructions, /Never tell the learner to grab/);
});

test('every Dock 7 lesson action has a parameterless tool mapped from learner phrases', () => {
  const cfg = buildSessionConfig().session;
  const byName = Object.fromEntries(cfg.tools.map(t => [t.name, t]));
  for (const name of LESSON_TOOL_NAMES) {
    const t = byName[name];
    assert.ok(t, name + ' tool');
    assert.equal(t.type, 'function');
    assert.deepEqual(t.parameters, { type: 'object', properties: {}, additionalProperties: false }, name + ' takes no arguments');
    assert.match(t.description, /^Call when the learner says/, name + ' description starts with learner phrases');
    assert.ok(t.description.length < 400, name + ' description stays short');
  }
  assert.match(byName.replay_demo.description, /show me the demo again/);
  assert.match(byName.split_cargo.description, /split it/);
  assert.match(byName.check_load.description, /load it/);
  assert.match(byName.check_load.description, /only source of whether a load is correct/);
  assert.match(byName.reset_cargo.description, /put everything back/);
  assert.match(byName.next_chapter.description, /next chapter/);
  assert.match(byName.restart_chapter.description, /start this chapter over/);
  assert.match(byName.back_to_lessons.description, /back to the lessons/);
  assert.match(byName.advance_step.description, /start loading/);
  assert.match(byName.advance_step.description, /next_chapter/);
  const names = cfg.tools.map(t => t.name);
  assert.equal(new Set(names).size, names.length, 'tool names are unique');
});

test('voice actions are grounded: tool results decide what happened and whether a load is right', () => {
  const cfg = buildSessionConfig().session;
  for (const name of LESSON_TOOL_NAMES) assert.ok(cfg.instructions.includes(name), 'instructions name ' + name);
  assert.match(cfg.instructions, /Never say an action happened unless its tool returned ok true/);
  assert.match(cfg.instructions, /If a tool returns ok false, say its reason in one sentence/);
  assert.match(cfg.instructions, /unless check_load returned the verdict/);
  assert.match(cfg.instructions, /never add a verdict of your own/);
  assert.match(cfg.instructions, /Dock 7/);
  for (const word of ['crates', 'one container', 'trucks', 'pickups', 'vans']) assert.ok(cfg.instructions.includes(word), 'story word ' + word);
  assert.match(cfg.instructions, /Never give real loading or safety advice/);
  assert.match(cfg.instructions, /Cranes unload full crates/);
  assert.match(cfg.instructions, /backed up to the dock/);
  assert.match(cfg.instructions, /drive away only after check_load accepts/);
  const everything = JSON.stringify(cfg);
  assert.doesNotMatch(everything, /aircraft|airplane|plane\b|container floor/i, 'no aircraft or container floor anywhere in the session');
  // Existing rules stay.
  assert.match(cfg.instructions, /English only/);
  assert.match(cfg.instructions, /Be brief/);
  assert.match(cfg.instructions, /Never ask for the learner's name/);
  assert.match(cfg.instructions, /on_table_now, can_grab_now/);
});

test('café and garden tools exist, are mapped from learner phrases and only one takes an argument', () => {
  const cfg = buildSessionConfig().session;
  const byName = Object.fromEntries(cfg.tools.map(t => [t.name, t]));
  for (const name of [...CAFE_TOOL_NAMES, ...GARDEN_TOOL_NAMES]) {
    const t = byName[name];
    assert.ok(t, name + ' tool');
    assert.equal(t.type, 'function');
    assert.match(t.description, /^Call when the learner says/, name + ' description starts with learner phrases');
    assert.ok(t.description.length < 400, name + ' description stays short');
    assert.ok(cfg.instructions.includes(name), 'instructions name ' + name);
    if (name !== 'garden_split_bed') assert.deepEqual(t.parameters, { type: 'object', properties: {}, additionalProperties: false }, name + ' takes no arguments');
  }
  const split = byName.garden_split_bed.parameters;
  assert.deepEqual(split.required, ['columns']);
  assert.equal(split.additionalProperties, false);
  assert.equal(split.properties.columns.type, 'integer');
  assert.equal(split.properties.columns.minimum, 1);
  assert.equal(split.properties.columns.maximum, 6);
  assert.match(byName.cafe_deal_round.description, /deal one round/);
  assert.match(byName.cafe_deal_round.description, /deal a round/);
  assert.match(byName.cafe_check_order.description, /check the order/);
  assert.match(byName.cafe_clear_table.description, /clear the table/);
  assert.match(byName.cafe_check_order.description, /only source of whether an order is correct/);
  assert.match(byName.garden_turn_bed.description, /turn the bed/);
  assert.match(byName.garden_split_bed.description, /split it at five/);
  assert.match(byName.garden_check_bed.description, /only source of whether a bed is correct/);
  for (const name of [...CAFE_TOOL_NAMES, ...GARDEN_TOOL_NAMES]) assert.ok(!LESSON_TOOL_NAMES.includes(name));
});

test('inside a lesson the guide calls only tools_now, and each story stays in its own lesson', () => {
  const cfg = buildSessionConfig().session;
  assert.match(cfg.instructions, /Inside a lesson call only tools listed in tools_now/);
  assert.match(cfg.instructions, /in Cargo Crew, which lists none, use only the Cargo Crew tools/);
  assert.match(cfg.instructions, /Only inside Cargo Crew: the learner is the load planner at Dock 7/);
  assert.match(cfg.instructions, /Only inside Neighborhood Café: /);
  assert.match(cfg.instructions, /Only inside Community Garden: /);
  for (const word of ['head barista', 'plates', 'boxes', 'pastries']) assert.ok(cfg.instructions.includes(word), 'café word ' + word);
  for (const word of ['head gardener', 'seedling strips', 'rows times columns', 'fence']) assert.ok(cfg.instructions.includes(word), 'garden word ' + word);
  assert.match(cfg.instructions, /comes only from cafe_check_order/);
  assert.match(cfg.instructions, /comes only from garden_check_bed/);
  assert.match(cfg.instructions, /A tool from another lesson returns ok false with a reason/);
  assert.match(cfg.instructions, /never give real food, allergy or nutrition advice/);
  assert.match(cfg.instructions, /never give real gardening, plant safety or pesticide advice/);
});

test('handler returns only the ephemeral secret, never the API key', async () => {
  const logs = []; const h = createHandler({ apiKey: 'sk-live-should-not-leak', fetchImpl: okFetch, limit: createLimiter(), log: (...a) => logs.push(a.join(' ')) });
  const r = res(); await h(good, r);
  assert.equal(r.code, 200); assert.equal(r.body.value, 'ek_test_secret'); assert.equal(r.body.model, 'gpt-realtime');
  assert.ok(!JSON.stringify(r.body).includes('sk-live')); assert.ok(!logs.join('\n').includes('sk-live')); assert.ok(!logs.join('\n').includes('ek_test'));
});

test('handler maps upstream failure to 502 and missing key to 503', async () => {
  const bad = async () => ({ ok: false, status: 401, json: async () => ({ error: { message: 'bad key' } }) });
  const r1 = res(); await createHandler({ apiKey: 'k', fetchImpl: bad, limit: createLimiter(), log() {} })(good, r1); assert.equal(r1.code, 502);
  const r2 = res(); await createHandler({ apiKey: '', fetchImpl: okFetch, limit: createLimiter(), log() {} })(good, r2); assert.equal(r2.code, 503);
});

test('limiter enforces per-minute and per-day caps', async () => {
  let t = 0; const lim = createLimiter({ perMinute: 2, perDay: 3, now: () => t });
  assert.equal(lim.allow(), true); assert.equal(lim.allow(), true); assert.equal(lim.allow(), false);
  t += 61000; assert.equal(lim.allow(), true); assert.equal(lim.allow(), false, 'daily cap');
  const h = createHandler({ apiKey: 'k', fetchImpl: okFetch, limit: { allow: () => false }, log() {} });
  const r = res(); await h(good, r); assert.equal(r.code, 429);
});
