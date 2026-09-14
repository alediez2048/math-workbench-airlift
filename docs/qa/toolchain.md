# Toolchain checkpoint

September 14, 2026. This records setup, **not a passed device build**.

| Component | Observed state |
|---|---|
| Workspace | `/Users/jad/Desktop/math-workbench-airlift`, branch `unity-airlift` |
| Unity Hub | 3.20.1 installed |
| Editor | 6000.6.0f1, revision f7f8ed4d1e24, Apple Silicon |
| Rendering | Universal 3D template; manifest requests URP 17.6.0 |
| Android | Build Support, SDK/NDK Tools and OpenJDK installed for 6.6 |
| Java | Bundled OpenJDK 17.0.18 |
| ADB | Bundled platform-tools 36.0.0, ADB 1.0.41 |
| Headset | ADB identifies Quest 3S; authorized `device` state observed |
| Developer onboarding | Owner reports team creation, verification, Developer Mode, and USB approval complete |
| Automation | Official Unity CLI 1.0.0-beta.9; Pipeline 0.7.0-exp.1 configured in project |
| XR packages | Core/Interaction/OVR integration 205.0.0, OpenXR 1.18.0, Meta OpenXR 2.6.1 resolved; editor reports ready |
| Proof scene | `Assets/Airlift/Scenes/DeviceProof.unity` generated successfully by versioned builder |
| Android configuration | ARM64, IL2CPP, Vulkan, mobile URP, OpenXR/Meta/Touch, required passthrough; first build queued |
| Product tests / APK | No product tests or physical acceptance yet; APK build result pending |

Evidence commands: `unity editors -i --format json`, project
`ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, bundled
`adb version`, `adb devices -l`, and bundled `java -version`.
Device identifiers and account details are intentionally omitted.

## Version decision

The original candidate was Unity 6.3 LTS. Use the already installed 6.6 editor
for the first compatibility proof to avoid another installation blocking setup.
Freeze the editor and exact package lock only after actual Quest acceptance.
Do not describe this provisional tuple as validated. A previously started 6.3
editor download is separate; inspect before resuming it and do not downgrade
or remove either editor automatically.

## Automation and approval boundary

Use [Unity's official CLI and Pipeline](https://docs.unity.com/en-us/unity-production-pipeline/local-tools-cli)
for supported local agent tooling. Package configuration alone does not prove
a functioning editor connection. No third-party editor bridge is installed.

The CLI-opened editor process disappeared during two startup attempts; no cause
was established. Launching a separate editor instance through macOS subsequently
left it running, and the project-local package log recorded Core and Interaction
downloads. Pipeline command discovery and compilation still need rechecking
after import. No user tutorial project was changed or closed.

Subsequent check passed: official `editor_status` reports ready and not compiling.
Both `AgentScripts/CreateDeviceProof.cs` and `ConfigureQuestProof.cs` ran
successfully through Pipeline `run_script`. The first builder creates the scene
using Meta blocks and the public grab wizard API; the second configures Android.
The block installer uses a checked internal SDK method via reflection, pinned to
205; review before any SDK upgrade. This is tooling coupling, not custom tracking.
The scene builder refuses to overwrite an existing proof or discard dirty scene edits.

Meta's registry metadata for Core 205.0.0 says adding or using the package accepts
the [Meta SDK license](https://developers.meta.com/horizon/licenses/oculussdk/).
Owner explicitly authorized acceptance and import in the September 14 voice
conversation. This does not authorize purchases. Submission rights remain separate.

## First physical acceptance checklist

- [ ] XR package tuple resolves and compiles without errors.
- [ ] One rig, passthrough, controllers, and one reusable SDK-grabbable cube.
- [ ] ARM64 APK builds and fresh-installs on the owned Quest 3S.
- [ ] Owner sees passthrough, completes ten grabs, and checks reachable placement.
- [ ] Pause/resume restores a usable scene.
- [ ] Readable ten-second capture saved with private surroundings excluded.

Record actual result, APK checksum, package versions, and any failures in
`docs/qa/device-proof.md` when the test is performed. Do not pre-fill passes.
