// CC-P0-03 THROWAWAY dev mint server. Mints OpenAI Realtime ephemeral client secrets for the
// spike on the LAN. Reads OPENAI_API_KEY from the environment; never logs secrets.
import http from 'node:http';
const key = process.env.OPENAI_API_KEY;
if (!key) { console.error('OPENAI_API_KEY missing'); process.exit(1); }
import { buildSessionConfig, LANGUAGES } from './lib/sessionConfig.js';
let minted = 0;
http.createServer(async (req, res) => {
  if (req.method === 'GET' && req.url === '/health') { res.end(JSON.stringify({ ok: true, minted })); return; }
  if (req.method !== 'POST' || req.url !== '/session') { res.statusCode = 404; res.end(); return; }
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
}).listen(8787, '0.0.0.0', () => console.log('dev-mint listening on 0.0.0.0:8787'));
