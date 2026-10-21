import { validateSessionRequest, createLimiter, loadClientKey } from '../lib/validate.js';
import { mintClientSecret } from '../lib/mint.js';

const limiter = createLimiter({ perMinute: Number(process.env.SESSIONS_PER_MINUTE || 6), perDay: Number(process.env.SESSIONS_PER_DAY || 200) });

/** Throws when GUIDE_CLIENT_KEY is missing or a placeholder, so a misconfigured proxy never serves. */
export function createHandler({ apiKey = process.env.OPENAI_API_KEY, clientKey = loadClientKey(), fetchImpl = fetch, limit = limiter, log = console.log } = {}) {
  return async function handler(req, res) {
    const check = validateSessionRequest(req.method, req.headers || {}, req.body, clientKey);
    if (!check.ok) { res.status(check.status).json({ error: check.error }); return; }
    if (!apiKey) { res.status(503).json({ error: 'not configured' }); return; }
    if (!limit.allow()) { res.status(429).json({ error: 'budget' }); return; }
    const minted = await mintClientSecret(apiKey, fetchImpl, req.body.language || 'en');
    if (!minted.ok) { log('mint failed', minted.status, minted.reason); res.status(502).json({ error: 'mint failed' }); return; }
    log('minted', req.body.build, req.body.language || 'en', 'expires', minted.expires_at); // no nonce, no secret, no audio
    res.status(200).json({ value: minted.value, expires_at: minted.expires_at, model: minted.model });
  };
}

// A serverless module has no process to stop, so a missing key turns every request
// into a 503 and logs why, instead of minting for anyone.
function createDefaultHandler() {
  try {
    return createHandler();
  } catch (err) {
    console.error('guide proxy misconfigured:', err.message);
    return async (req, res) => { res.status(503).json({ error: 'not configured' }); };
  }
}

export default createDefaultHandler();
