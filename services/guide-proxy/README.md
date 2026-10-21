# Guide proxy

The one server-side piece. A Vercel function that mints short-lived, budgeted OpenAI Realtime client secrets
for the headset, so the provider key never leaves this environment. Dee's persona, her language and her tools
are authored here (`lib/sessionConfig.js`) and bound into every session; the app cannot change them.

```
api/session.js        POST /session → { client_secret, expires_at, ... } or 4xx
lib/mint.js           calls OpenAI to create the ephemeral secret with the server-side session config
lib/sessionConfig.js  persona, voice, language (en|es) and the tool schema Dee may call
lib/validate.js       request validation: client key, x-nerdy-client header, nonce, build id, language
dev-mint.mjs          local dev helper: the same mint on a LAN port, same key check
.env.example          the environment variables, documented
test/                 node --test
```

## Deploy

```bash
cd services/guide-proxy
vercel link            # once; the project is nerdy-guide-proxy
vercel env add OPENAI_API_KEY production
vercel env add GUIDE_CLIENT_KEY production      # openssl rand -hex 32
vercel env add SESSIONS_PER_DAY production      # budget, e.g. 200
vercel env add SESSIONS_PER_MINUTE production   # burst, e.g. 6
vercel --prod
```

Production URL: `https://nerdy-guide-proxy.vercel.app/session`. The Unity client (`GuideEndpoints.MintUrl`)
sends `x-nerdy-client`, the client key in `x-nerdy-key`, a nonce and the language; anything else is refused.
Never put the OpenAI key in the repo, the APK, or a document.

## Access control

- `GUIDE_CLIENT_KEY` is required. Without it, or with a short or placeholder value, the proxy refuses to
  mint and answers every request with 503. Keys are compared in constant time.
- The app reads the same key from `unity/Assets/Airlift/Resources/LocalGuideConfig/client-key.txt`, which is
  gitignored. Builds without that file run the guide offline.
- The key ships inside the APK, so treat it as a rotatable gate, not a secret: rotate it (new Vercel value,
  new build) if a build leaks. The real ceiling behind it is the rate limit and the OpenAI project spend cap.
- The rate limit (`SESSIONS_PER_MINUTE`, `SESSIONS_PER_DAY`) is held in memory, so each serverless
  instance counts separately. A shared store would make it global.

## Local dev mint

```bash
OPENAI_API_KEY=... GUIDE_CLIENT_KEY=... node dev-mint.mjs     # listens on 0.0.0.0:8787
```

Point an editor session or a development build at it by putting the URL in the gitignored
`unity/Assets/Airlift/Resources/LocalGuideConfig/dev-mint-url.txt`. Release builds ignore that file and
refuse cleartext HTTP.

## Test

```bash
npm test
```
