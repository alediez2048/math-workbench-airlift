import { buildSessionConfig, MODEL } from './sessionConfig.js';

export async function mintClientSecret(apiKey, fetchImpl = fetch, language = 'en') {
  const r = await fetchImpl('https://api.openai.com/v1/realtime/client_secrets', {
    method: 'POST', headers: { Authorization: 'Bearer ' + apiKey, 'Content-Type': 'application/json' },
    body: JSON.stringify(buildSessionConfig(language)) });
  let body = null; try { body = await r.json(); } catch { body = null; }
  if (!r.ok || !body || typeof body.value !== 'string') return { ok: false, status: r.status, reason: body?.error?.message || 'mint failed' };
  return { ok: true, value: body.value, expires_at: body.expires_at, model: MODEL };
}
