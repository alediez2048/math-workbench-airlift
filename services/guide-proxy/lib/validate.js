export const CLIENT_MARKER = 'nerdy-quest';
const NONCE = /^[A-Za-z0-9-]{8,64}$/;

export function validateSessionRequest(method, headers, body) {
  if (method !== 'POST') return { ok: false, status: 405, error: 'method' };
  if ((headers['x-nerdy-client'] || '') !== CLIENT_MARKER) return { ok: false, status: 403, error: 'client' };
  if (!body || typeof body !== 'object') return { ok: false, status: 400, error: 'body' };
  if (typeof body.launchNonce !== 'string' || !NONCE.test(body.launchNonce)) return { ok: false, status: 400, error: 'nonce' };
  if (typeof body.build !== 'string' || body.build.length > 40) return { ok: false, status: 400, error: 'build' };
  return { ok: true };
}

// Best-effort limits per serverless instance. The hard ceiling is the OpenAI project spend cap.
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
