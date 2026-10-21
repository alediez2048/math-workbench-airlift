// Local dev helper: the same mint as api/session.js on a LAN port, for testing from the editor or a
// development build before deploying. Needs OPENAI_API_KEY and GUIDE_CLIENT_KEY in the environment;
// callers must send the same x-nerdy-client and x-nerdy-key headers as the app. Never logs secrets.
import http from 'node:http';
import { buildSessionConfig, LANGUAGES } from './lib/sessionConfig.js';
import { loadClientKey, clientKeyMatches, CLIENT_MARKER, CLIENT_KEY_HEADER } from './lib/validate.js';

const key = process.env.OPENAI_API_KEY;
if (!key) { console.error('OPENAI_API_KEY missing'); process.exit(1); }
let clientKey;
try { clientKey = loadClientKey(); } catch (e) { console.error(e.message); process.exit(1); }
const port = Number(process.env.DEV_MINT_PORT || 8787);
const host = process.env.DEV_MINT_HOST || '0.0.0.0';

let minted = 0;
http.createServer(async (req, res) => {
  if (req.method === 'GET' && req.url === '/health') { res.end(JSON.stringify({ ok: true, minted })); return; }
  if (req.method !== 'POST' || req.url !== '/session') { res.statusCode = 404; res.end(); return; }
  if (req.headers['x-nerdy-client'] !== CLIENT_MARKER || !clientKeyMatches(req.headers[CLIENT_KEY_HEADER], clientKey)) {
    res.statusCode = 401; res.end(JSON.stringify({ error: 'key' })); return;
  }
  try {
    let raw = ''; for await (const part of req) raw += part;
    let language = 'en'; try { const b = JSON.parse(raw || '{}'); if (b.language && LANGUAGES[b.language]) language = b.language; } catch { }
    const session = buildSessionConfig(language);
    const r = await fetch('https://api.openai.com/v1/realtime/client_secrets', { method: 'POST',
      headers: { Authorization: 'Bearer ' + key, 'Content-Type': 'application/json' }, body: JSON.stringify(session) });
    const body = await r.json();
    if (!r.ok || !body.value) { console.log('mint failed', r.status, body.error?.message); res.statusCode = 502; res.end(JSON.stringify({ error: 'mint failed' })); return; }
    minted++; console.log(new Date().toISOString(), 'minted session', minted, language, 'expires', body.expires_at);
    res.setHeader('Content-Type', 'application/json'); res.end(JSON.stringify({ value: body.value, expires_at: body.expires_at }));
  } catch (e) { console.log('mint error', e.message); res.statusCode = 500; res.end(JSON.stringify({ error: 'mint error' })); }
}).listen(port, host, () => console.log(`dev-mint listening on ${host}:${port}`));
