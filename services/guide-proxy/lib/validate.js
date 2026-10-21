import { createHash, timingSafeEqual } from 'node:crypto';
import { LANGUAGES } from './sessionConfig.js';

export const CLIENT_MARKER = 'nerdy-quest';
export const CLIENT_KEY_HEADER = 'x-nerdy-key';
const NONCE = /^[A-Za-z0-9-]{8,64}$/;
const MIN_KEY_LENGTH = 32;
const PLACEHOLDER = /change|example|placeholder|your[-_]?key|secret-here|xxx/i;

/**
 * GUIDE_CLIENT_KEY is the shared key every app build sends in x-nerdy-key.
 * It is required: the proxy refuses to start without one, and refuses obvious
 * placeholders or short values. The key ships inside the APK, so it is a
 * rotatable gate rather than a true secret; the rate limit and the OpenAI
 * project spend cap are the hard ceiling behind it.
 */
export function loadClientKey(env = process.env) {
  const key = (env.GUIDE_CLIENT_KEY || '').trim();
  if (!key) throw new Error('GUIDE_CLIENT_KEY is not set');
  if (key.length < MIN_KEY_LENGTH) throw new Error(`GUIDE_CLIENT_KEY must be at least ${MIN_KEY_LENGTH} characters`);
  if (PLACEHOLDER.test(key)) throw new Error('GUIDE_CLIENT_KEY looks like a placeholder');
  return key;
}

/** Constant-time comparison: hash both sides so length differences leak nothing. */
export function clientKeyMatches(presented, expected) {
  if (typeof presented !== 'string' || !presented || !expected) return false;
  const a = createHash('sha256').update(presented).digest();
  const b = createHash('sha256').update(expected).digest();
  return timingSafeEqual(a, b);
}

export function validateSessionRequest(method, headers, body, clientKey) {
  if (method !== 'POST') return { ok: false, status: 405, error: 'method' };
  if ((headers['x-nerdy-client'] || '') !== CLIENT_MARKER) return { ok: false, status: 403, error: 'client' };
  if (!clientKeyMatches(headers[CLIENT_KEY_HEADER], clientKey)) return { ok: false, status: 401, error: 'key' };
  if (!body || typeof body !== 'object') return { ok: false, status: 400, error: 'body' };
  if (typeof body.launchNonce !== 'string' || !NONCE.test(body.launchNonce)) return { ok: false, status: 400, error: 'nonce' };
  if (typeof body.build !== 'string' || body.build.length > 40) return { ok: false, status: 400, error: 'build' };
  if (body.language !== undefined && !Object.hasOwn(LANGUAGES, body.language)) return { ok: false, status: 400, error: 'language' };
  return { ok: true };
}

// Best-effort limits held in memory, so each serverless instance counts on its own.
// The hard ceiling is the OpenAI project spend cap.
export function createLimiter({ perMinute = 6, perDay = 200, now = () => Date.now() } = {}) {
  const minute = []; let dayCount = 0; let dayStart = now();
  return { allow() {
    const t = now();
    if (t - dayStart > 86400000) { dayStart = t; dayCount = 0; }
    while (minute.length && t - minute[0] > 60000) minute.shift();
    if (minute.length >= perMinute || dayCount >= perDay) return false;
    minute.push(t); dayCount++; return true;
  } };
}
