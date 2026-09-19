# Nerdy AI + VR landing page

Standalone, static landing page for the Cargo Crew mixed-reality prototype. No Unity changes, backend, analytics, microphone access, signup collection, or embedded provider credentials.

## Design

Reference: https://claude.ai/artifact/5LwzYqPEPoXTJ4PcJv55mr (Live Learning Style Guide).
Indigo #202344, surfaces #161C2C, glass borders #6C6E87, Poppins/Karla, brand and spectrum gradients, rounded cards and pill controls. Muted text increased from 64% to 72% for readability. Body text is 16px minimum. Fonts are locally hosted under their supplied OFL licenses.

Imagery is copied from the project's own Unity scene previews; it is labeled accordingly and is not presented as headset footage. Voice dialogue is explicitly illustrative. No generated product artwork or unverified learning-outcome claims. The site is identified as an independent hackathon prototype, not an official Nerdy or Meta offering.

## Preview and deploy

Serve `dist/` with any static web server. Vercel deploys `dist/` using `vercel.json`; no build dependencies are required. Only deploy this directory, never the Unity repository or guide-proxy credentials.

The chapter buttons switch between the whole, quarters and equivalence previews. All other navigation is in-page. There is deliberately no app download or demo-video button without a public release or supplied video.

## Asset origins

- cargo-whole.png: math-workbench-airlift/artifacts/dock7/1e-side.png
- cargo-one-whole.png: math-workbench-airlift/artifacts/dock7/1b-docked.png (front-facing whole-crate chapter preview)
- cargo-quarters.png: math-workbench-airlift/artifacts/dock7/3b-docked.png
- cargo-equivalence.png: math-workbench-airlift/artifacts/dock7/4b-docked.png
- cafe-sharing.png: math-workbench-airlift/artifacts/cafe/2b-placed.png
- garden-arrays.png: math-workbench-airlift/artifacts/garden/3b-planted.png
- Fonts and licenses: math-workbench-airlift/unity/Assets/Airlift/Fonts/Nerdy/
- nerdy-ai-vr-logo.svg: vector trace of the logo supplied by the project owner in Downloads, with the wordmark gradients recreated as SVG fills. The original PNG remains untouched.
- nerdy-ai-vr-navbar-supplied.svg: archival copy of the owner-supplied Navbar logo from Downloads/Nerdy_AI_VR_logo.svg. The file embeds a PNG with a dark background. The live navbar uses the transparent vector trace of this artwork instead.

All lesson screenshots preserve their full source aspect ratio without cropping on desktop or mobile. Café and Garden snapshots link to their original-resolution images for closer inspection.

The header follows the supplied navigation screenshot: wide translucent rounded bar, left-aligned project wordmark with spectrum badge, and a Lessons dropdown plus AI guide link on the right. The dropdown links to all three lesson sections and supports Escape and outside-click dismissal. The reference's Varsity Tutors logo is not reproduced.
- lounge-arrival.jpg, lounge-tour.jpg, lounge-scenery.jpg, cargo-quarters-headset.jpg, cafe-plates-headset.jpg, garden-sunflowers-headset.jpg, demo-poster.jpg: frames from a Quest 3S recording of the preview app, September 18, 2026 (math-workbench-airlift/docs/media). Labeled "Quest 3S recording".
- nerdy-ai-vr-demo.mp4: the 80-second demo cut from the same recording. Not tracked in git (36 MB); before deploying, copy it from the GitHub release v0.2-hackathon (or from artifacts/demo/) into dist/assets/.
