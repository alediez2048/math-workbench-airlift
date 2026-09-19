# nerdy-guide-proxy

The one server-side piece. A Vercel function that mints short-lived, budgeted OpenAI Realtime client secrets
for the headset, so the provider key never leaves this environment. Dee's persona, her language and her tools
are authored here (`lib/sessionConfig.js`) and bound into every session; the app cannot change them.

```
api/session.js        POST /session → { client_secret, expires_at, ... } or 4xx
lib/mint.js           calls OpenAI to create the ephemeral secret with the server-side session config
lib/sessionConfig.js  persona, voice, language (en|es) and the tool schema Dee may call
lib/validate.js       request validation: x-nerdy-client header, nonce, build id, language
dev-mint.mjs          the same mint on a LAN port for editor testing
test/                 node --test
```

## Deploy

```bash
cd services/guide-proxy
vercel link            # once; the project is nerdy-guide-proxy
vercel env add OPENAI_API_KEY production
vercel env add SESSIONS_PER_DAY production      # budget, e.g. 200
vercel env add SESSIONS_PER_MINUTE production   # burst, e.g. 6
vercel --prod
```

Production URL: `https://nerdy-guide-proxy.vercel.app/session`. The Unity client (`GuideEndpoints.MintUrl`)
sends `x-nerdy-client` plus a nonce and the language; anything else is refused. Never put the key in the
repo, the APK, or a document.

## Test

```bash
npm test
```
