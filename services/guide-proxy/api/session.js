import { validateSessionRequest, createLimiter } from '../lib/validate.js';
import { mintClientSecret } from '../lib/mint.js';

const limiter = createLimiter({ perMinute: Number(process.env.SESSIONS_PER_MINUTE || 6), perDay: Number(process.env.SESSIONS_PER_DAY || 200) });

export function createHandler({ apiKey = process.env.OPENAI_API_KEY, fetchImpl = fetch, limit = limiter, log = console.log } = {}) {
  return async function handler(req, res) {
    const check = validateSessionRequest(req.method, req.headers || {}, req.body);
    if (!check.ok) { res.status(check.status).json({ error: check.error }); return; }
    if (!apiKey) { res.status(503).json({ error: 'not configured' }); return; }
    if (!limit.allow()) { res.status(429).json({ error: 'budget' }); return; }
    const minted = await mintClientSecret(apiKey, fetchImpl, req.body.language || 'en');
    if (!minted.ok) { log('mint failed', minted.status, minted.reason); res.status(502).json({ error: 'mint failed' }); return; }
    log('minted', req.body.build, req.body.language || 'en', 'expires', minted.expires_at); // no nonce, no secret, no audio
    res.status(200).json({ value: minted.value, expires_at: minted.expires_at, model: minted.model });
  };
}

export default createHandler();
