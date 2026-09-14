# WORKBENCH — technical brief

> **Legacy WebXR architecture.** Do not use these dependencies or implementation
> instructions. The project has moved to Unity; see `UNITY_PLAN.md`.

Verified against the npm registry live on Sep 14 2026. Every version below was checked,
not recalled.

---

## 1. The decision

WebXR, React Three Fiber, @react-three/xr v6, deployed to Vercel. The demo is a URL.
Do not touch Unity, Godot, Unreal, or Lens Studio.

Four reasons, in order of weight:

1. The iteration loop is seconds, not minutes. Unity's realistic first-timer path is 6 to
   12 hours to see a cube on a headset (toolchain install, Android build support, OpenJDK,
   SDK/NDK, XR Plug-in Management, Meta XR Core SDK, Project Setup Tool, developer mode,
   USB trust), and 2 to 6 minutes per build-and-deploy after that. Over five days that
   deploy loop is the entire project.
2. The judge gets a link instead of an APK. Meta Horizon Store review takes days to weeks.
   Release channels skip review but require the judge to have a Meta account, own a Quest,
   accept an invite, and install through the Horizon app. Nobody judging a hackathon will
   sideload through SideQuest.
3. @react-three/xr ships the Meta Quest 3 emulator (IWER) as a bundled dependency, so a
   judge on a laptop can enter the actual same scene. Verified: @pmndrs/xr@6.6.30 depends
   on iwer ^2.1.0, @iwer/devui ^1.1.1, @iwer/sem ~0.2.5.
4. R3F and three.js have by far the most model training coverage of any XR stack, which
   matters enormously when the plan is to build with an AI agent. Claude writes correct R3F
   first try and writes broken Unity C# plus Meta XR SDK glue on the third.

### On the Snap advice you were given

Discard it. Lens Studio does not target Meta Quest at all. Its three deployment targets
are Snapchat on phones, web and third-party apps via Camera Kit, and Snap Spectacles via
the Spectacles Interaction Kit. Spectacles run Snap OS, which is Android-derived but not
APK-compatible, with a sandboxed Lens runtime, no Quest target, and no WebXR export.
Camera Kit for Web is phone and webcam AR on a 2D camera feed, not a WebXR runtime. The
advice probably came from "WebAR" sounding like "WebXR." Following it would cost the whole
week and produce something that does not run on a Quest 3.

---

## 2. Exact install, with the traps called out

```bash
npm create vite@latest workbench -- --template react-ts
cd workbench

# PIN THESE. Do not use @latest on any of them.
npm i react@19.2.8 react-dom@19.2.8
npm i three@0.185.1 @react-three/fiber@9.7.0 @react-three/xr@6.6.30 @react-three/drei@10.7.8
npm i -D @types/three@0.185.1

npm i @react-three/uikit@1.0.76      # spatial UI panels, instanced, XR-safe
npm i zustand@5.0.15                 # game state
# @react-three/rapier@2.2.0 only if you actually need physics; snapping does not

npm i -D @vitejs/plugin-basic-ssl    # required for LAN on-headset testing
npm i -D @gltf-transform/cli@4.5.0   # asset compression
```

Traps, all verified today:

- `react@latest` is 19.3.0, released Sep 9. R3F 9.7.0's peer range is `>=19 <19.3`.
  Installing latest puts you in a peer conflict on hour one. Use 19.2.8.
- `three@latest` is 0.186.0, released Sep 8, six days ago. R3F 9.7.0 shipped Jul 31 when
  0.185.1 was current, and nothing in the R3F, drei, or xr chain has been validated
  against 0.186. @react-three/xr declares `three: "*"` so it will not warn you.
- Commit the lockfile immediately. Do not run `npm update` this week.
- Do NOT use WebGPURenderer. The three.js docs push WebGPU hard and several 2026 posts
  falsely claim Quest supports WebXR plus WebGPU. It does not. The WebXR/WebGPU Binding is
  an Editor's Draft with no Quest implementation. Your scene will render on desktop and
  produce a black or failed session on the headset. Reject any suggestion to "upgrade to
  WebGPU."
- Skip @react-three/postprocessing entirely. Bloom and depth of field in stereo are a
  framerate cliff.

---

## 3. The API patterns you actually need

@react-three/xr v5 patterns are all dead. If Claude Code suggests `<VRButton>`,
`<Interactive>`, `<Controllers>`, `<Hands>`, `useXREvent`, or `useController`, it is
writing v5 and you should reject it.

### The store, configured for this project

```tsx
import { createXRStore } from '@react-three/xr'

export const store = createXRStore({
  // This is the judge-without-a-headset feature. Default gate is
  // inject: { hostname: 'localhost' }, which means it will NOT activate on your
  // Vercel domain unless you set inject: true explicitly.
  emulate: {
    type: 'metaQuest3',
    inject: true,
    syntheticEnvironment: false,  // show YOUR scene, not IWER's office room
    primaryInputMode: 'controller',
  },
  frameRate: 'high',
  originReferenceSpace: 'local-floor',
  handTracking: true,
  planeDetection: true,
  // Self-host these. The default pulls controller GLBs live from jsDelivr at session
  // start, which is a network dependency on venue wifi.
  baseAssetPath: '/webxr-input-profiles/',
  defaultControllerProfileId: 'generic-trigger',
})
```

Real hardware is unaffected by `emulate`; IWER only injects when native WebXR is absent.

### Entering a passthrough session

```tsx
<button onClick={async () => {
  await navigator.mediaDevices.getUserMedia({ audio: true }) // BEFORE the session
  await audioContext.resume()                                // same user gesture
  await store.enterAR()                                      // immersive-ar = passthrough
}}>Start</button>
```

Session entry requires a user gesture. Auto-calling it on load silently fails.

### Interaction

There is no XR-specific event API. XR pointers emit the same R3F pointer events as a
mouse: `onClick`, `onPointerDown/Up/Move/Enter/Leave`, plus `setPointerCapture` for drags.
That is why desktop and VR are one codebase. Per-object control via `pointerEvents`,
`pointerEventsType={{ deny: 'grab' }}`, and `pointerEventsOrder`.

### Finding the real table

```tsx
const tables = useXRPlanes('table')
const horizontals = useXRPlanes('horizontal')  // fallback label
```

Critical: the plane array is EMPTY for the first 2 to 3 seconds of every session. Show a
"finding your table" state and fall back to a fixed-height virtual surface at 0.75m if
nothing arrives in 5 seconds. Never assume geometry exists on frame one.

Anchors: max 8 simultaneous per site, lost on history clear, unavailable in private
browsing. You need one.

### Hand tracking

25 joints per hand. Palm-pinch on both hands is reserved by the system, so never bind a
mechanic to it. Hand tracking degrades in low light, on hand overlap, and on fast motion.
Table-scale, well-lit, slow near-field grabbing is the best case for it, which is why this
design can afford hands-primary where most VR projects cannot. Always keep controllers as
a working fallback path, and test both every day.

### Text and panels

drei's `<Text>` wraps troika-three-text for SDF labels. For anything panel-shaped, use
@react-three/uikit, which is a flexbox layout engine that compiles to instanced geometry,
so a whole UI is a handful of draw calls. Load exactly one custom font; each face is a
separate SDF atlas.

### The flat fallback

```tsx
<IfInSessionMode deny={['immersive-vr', 'immersive-ar']}>
  <DesktopControls />   {/* pointer-lock or orbit, plus a 2D crosshair */}
</IfInSessionMode>
```

Same scene, same interactions, same game logic. Budget two hours. It is the highest-ROI
two hours in the project, because most judges will never put on a headset, and macOS and
iOS Safari have no WebXR at all.

---

## 4. Performance budgets for Quest 3

Frame budget: 90 FPS = 11.1 ms, 72 FPS = 13.7 ms. Target 72.

- Triangles: stay under about 150k total for a WebXR build. This scene should be far under.
- Draw calls: under about 100. This is the real killer, not triangles. Use InstancedMesh
  for the coin and tile pools.
- Textures: compress to KTX2. Most of this scene should have no textures at all.
- Lights: one directional plus mild ambient. No real-time shadows; fake them with a dark
  radial-gradient plane under each object.
- Per-frame CPU logic over 2 ms needs optimizing.

What tanks framerate, in order: post-processing in stereo, real-time shadows,
transparency and particle overdraw, uncompressed textures, draw-call count, and only then
triangle count.

Panic levers for Friday morning: `foveation: 1`, `frameBufferScaling: 0.8`, delete every
shadow, swap meshStandardMaterial for meshBasicMaterial with baked vertex colors.

---

## 5. Dev loop and deployment

### On-headset dev, set this up in hour one

```bash
adb devices                     # headset shows as "device"
adb reverse tcp:5173 tcp:5173
```

Then open `http://localhost:5173` in Horizon Browser. localhost is a secure context, so
WebXR activates over plain HTTP with no certificate. Remote-debug from desktop Chrome via
`chrome://inspect/#devices` for a real DevTools console against the headset.

No-cable alternative: `server.host: '0.0.0.0'` in vite.config.ts plus
@vitejs/plugin-basic-ssl. Setting `https: true` alone does not work, you need the plugin.

### Deployment

Vercel, static Vite output plus serverless API routes for the tutor. HTTPS is not
optional: Quest Browser will not report XR capability at all over HTTP, so no Enter button
appears and the app looks broken with no error.

Deploy on Day 1 and redeploy continuously. The URL should exist before the game does.

---

## 6. The AI tutor architecture

Chosen: push-to-talk, request/response. Not streaming realtime.

```
[grip held]  MediaRecorder captures webm/opus
[released]   POST blob to /api/tutor
             -> Whisper (or Deepgram) transcribe
             -> Claude with tutor system prompt + job state + last 10 action events
             -> ElevenLabs Flash TTS
             <- { text, audioUrl, strategyTags[] }
             play via Web Audio
```

Why this and not OpenAI Realtime over WebRTC: push-to-talk sidesteps every hard problem.
No voice activity detection, no barge-in, no echo cancellation, no persistent socket to
drop, no WebRTC negotiation inside an XR session. Nobody has validated WebRTC audio plus a
live XRSession on Horizon Browser, and finding out on Thursday night is not a plan.
Latency lands at 1.5 to 3 seconds, which is fine and actually reads correctly for a
"thinking" character if you cover it with an idle animation and a soft chime.

Model: claude-haiku-4.5 for tutor turns (fast, cheap), claude-sonnet-5 if the strategy
classification needs more reasoning. Stream the Claude response and start TTS on the first
sentence to cut perceived latency.

### Do not depend on the Web Speech API on Quest

`webkitSpeechRecognition` in Chromium is a thin client to a Google cloud service requiring
an API key baked into the browser build. Chromium forks without that key expose the object
and then fail with `error: 'network'` on every call. This is why Opera never supported it
despite being Chromium. Horizon Browser is in that category. Meta's own browser
specifications page documents WebXR in detail and says nothing about Web Speech.
`speechSynthesis.getVoices()` is separately unreliable on Android-derived Chromium. Do the
STT and TTS over the network.

### Microphone ordering

Request `getUserMedia({audio:true})` from the landing page, before entering the session. A
permission prompt raised during an immersive session may drop the user out of immersion or
render behind the scene. Permission persists per-origin, so repeat visits are silent.

### Audio playback

Use the Web Audio API, never `<audio>` elements, inside an XR session. `AudioContext`
starts suspended under the autoplay policy, so `await audioContext.resume()` in the same
click handler as session entry, and verify `ctx.state === 'running'` before entering.

Spatial audio gotcha: in an XR session three.js swaps in an ArrayCamera from
`renderer.xr.getCamera()`, so an AudioListener parented to the original camera will not
track head movement. Either update a listener group from `renderer.xr.getCamera()` each
frame, or just accept non-spatialized tutor audio, which is arguably the better creative
choice anyway.

### The guaranteed fallback

Build a `TUTOR_MODE = 'voice' | 'text'` flag on day one and wire the text path first.
Claude responses on a uikit panel driven by game events, one to two hours of work. If
voice is broken three hours before the deadline, flip the flag and the tutor still exists.

---

## 7. Assets

Flat-shaded low poly, baked vertex-color lighting, one directional light, a tight four to
six colour palette, gradient-sky or plain passthrough background. Cheap, fast, forgiving,
and it reads as deliberate art direction rather than a shortcut. It will hold 90 FPS with
room to spare.

Most of this scene is primitives you generate in code: discs, boxes, a plane, a ruler.
Resist buying assets you can write.

For the few things you do need: Quaternius (CC0, one studio's hand so packs look
consistent), Kenney (CC0, also has UI and audio packs), Poly Pizza (direct GLB download,
hosts Quaternius). Filter Sketchfab to CC0 and check every license. Skip AI text-to-3D
unless you pay, because free tiers do not grant commercial ownership.

Run everything through:
```bash
npx @gltf-transform/cli@4.5.0 optimize in.glb out.glb --compress draco --texture-compress ktx2
```

---

## 8. Recording

Set up Meta Quest Developer Hub on Day 1, not on Friday. MQDH over USB-C records up to
2160p at 60 fps, 40 Mbps, and the file lands on your computer. The in-headset Camera app
is 1080p at 36 fps variable, 20 Mbps, which is noticeably worse. Casting to a phone or PC
is the worst option; use it for spectating only.

Caveats: recording costs framerate, so profile with capture running. Native capture is
monoscopic from a virtual camera, wider and flatter than what your eye sees. Move your
head slowly, it is Meta's own advice and it matters. Colour-correct in post, VR footage
reads too dark. Skip full mixed-reality compositing with a green screen; it looks great
and it will eat a day you do not have.

---

## 9. Risk register

| # | Risk | Impact | Mitigation |
|---|------|--------|------------|
| 1 | Hand tracking too flaky for coin and tile grabbing | High | Controllers as a first-class fallback path, tested daily. Bright room. Never bind to palm pinch. Decide by Tuesday noon which is primary for the video. |
| 2 | WebGPURenderer suggested by an agent or a doc | Fatal | WebGLRenderer only. Reject any WebGPU suggestion. |
| 3 | No HTTPS, so no Enter button and no error | High | Vercel from Day 1. adb reverse plus localhost for dev. `https: true` alone in Vite does not work. |
| 4 | Peer dependency conflict on hour one | Medium | react 19.2.8, three 0.185.1, lockfile committed. |
| 5 | Audio silent in session | Medium | Web Audio, resume in the same gesture as session entry, verify state. |
| 6 | Loading assets unmounts `<XR>` and kills the session | High | Keep `<XR>` mounted, put `<Suspense>` inside it around content only, preload before showing the Enter button. |
| 7 | Plane detection returns nothing | Medium | Empty for 2 to 3 seconds always. Timeout to a virtual table at 0.75m. |
| 8 | Flat fallback throws on Safari | Fatal to scoring | Safari has zero WebXR. Test macOS Safari and an iPhone explicitly before submitting. Never let `navigator.xr` being undefined throw. |
| 9 | Voice tutor eats the schedule | High | Text path first, behind a flag. Voice is an upgrade, not a dependency. |
| 10 | Performance cliff on Friday | High | On-device check at the end of every day, not at the end of the week. Panic levers in section 4. |
| 11 | Blind debugging on device | Medium | chrome://inspect set up hour one. In-scene debug panel on a controller button. |
| 12 | Motion sickness in a judge's headset | High | Seated, table-scale, passthrough, zero locomotion. This is a three-word answer and it is already the design. |
| 13 | Cannot film a child in a headset | Medium | Meta's capture policy prohibits footage showing anyone who appears under 15 in a headset. Adult demonstrator with an on-screen note, or first-person with a child voiceover. Plan Monday, not Friday. |
