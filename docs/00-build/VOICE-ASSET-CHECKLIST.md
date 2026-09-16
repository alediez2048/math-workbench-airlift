# Narration acquisition checklist

This checklist covers rights-cleared audio acquisition. Reviewed speech assets
can serve the live AI-selected catalog or the labeled local fallback; audio alone
does not satisfy the core live-model requirement in AI-VOICE-GUIDE.md. Owner decision required before fallback asset
acceptance in CC-P1-03. Suggested provider-independent route:
the adult owner supplies their own recordings, or authorizes another adult narrator
with written redistribution permission. The coding agent integrates and validates
those clips; it cannot invent delivery or rights clearance.

1. Review STORYBOARD.md's exact cue script and pronunciation.
2. Record one clean WAV per cue ID, no music/background conversation or child voice.
3. Provide caption/script match and permission covering the intended app distribution.
4. Record source/speaker permission, license, recording date and hash in
   ASSET-LICENSES.md when assets actually arrive.
5. Agent imports, trims silence if authorized, sets appropriate audio import settings,
   maps clips to GuideCueCatalog and verifies every required cue for that milestone.
6. Owner listens on Quest at comfortable volume, checks captions/replay and approves.
7. For the live AI catalog, record exact textHash/audioHash, locale, voice/version,
   approved contexts and adult listening approval in VerifiedSpeechManifest.json.
   Runtime rejects unreviewed or mismatched assets; no dynamic synthesis is implied.
8. Missing clips or rights: ticket can be code-complete but not narrated accepted.
   Do not silently ship captions-only as fulfillment of the requested guide.

ElevenLabs is a candidate only after recorded-output eligibility review. A paid plan,
stock voice, adult account or locally bundled file does not itself clear the
under-13-targeted product restriction. No generation or purchase occurred here.
