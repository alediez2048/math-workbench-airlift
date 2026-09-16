# Cargo baseline verification checkpoint

September 15, 2026. In progress; not device accepted.

## Latest recovery and approved build

- FINAL BUILD: Succeeded, 0 errors, 8 warnings (warning triage pending).
  APK: artifacts/qa/cargo-20260915-180248/airlift-cargo.apk.
  SHA-256: 0c77ee9e1bfbbc13083d5ca38d6d3ebfba54643cbc4444184f078932db3e7e58.
- First raw-byte scan falsely joined separate IL2CPP culture-name literals into a
  token-like string. Scanner now inserts boundaries from v108 string-literal indices,
  following installed Unity GlobalMetadata.cpp and GlobalMetadataFileInternals.h.
  Unknown layouts fail closed. All bytes remain scanned; real token literals are retained.
- Five verifier tests pass including the boundary regression. Rescanning this exact APK
  passes bounded credential checks. No rebuild needed: scanner-only change after build.
- The wrapper originally exited at that false-positive scan; the successful rescan was
  separate, not claimed as an uninterrupted green wrapper run. No install/device review.

- Owner explicitly approved a local APK from uncommitted files, leaving Git unchanged.
- Repeat run returned zero PlayMode tests even though list_tests and a direct inspection
  of Pipeline's selection both found the exact test. Exact-name and explicit-filter
  changes did not resolve it. No vendor code was modified.
- A script-domain reload through official Pipeline run_script restored execution.
  Underlying stale-state cause inside Unity/Pipeline remains unproven; this is a
  verified recovery, not a permanent vendor fix. Temporary diagnostic script removed.
- Fresh full run: 12 onboarding + 3 baseline + 1 scene tests passed.
  Evidence: artifacts/qa/cargo-20260915-180248.
- Source snapshot includes the temporary diagnostic used at build start; scanner fix
  occurred afterward and does not alter the APK. Device acceptance remains pending.
- Prior authorization-pending statements below are historical, superseded by approval.

## Earlier checkpoint

- Owner authorized CC-P1-01 only and batched headset checks.
- Unity 6000.6.0f1, existing package tuple, already-open official Pipeline connection.
- Created CargoCrew with AssetDatabase.CopyAsset; reference hashes remained unchanged.
- Compilation completed without errors.
- PASS: 12 OnboardingFlowTests, 3 CargoBaselineTests, 1 CargoBaselineTestsSceneTests.
- PASS: four Python verifier tests including missing/zero/failed/skipped results and unsafe APK fixtures.
- Evidence: `artifacts/qa/cargo-20260915-173318/`, ignored local JSON and generated test XML.
- Commands: `python3 scripts/test_verify_cargo.py`; `bash scripts/verify-cargo.sh --suite CargoBaselineTests --build`.
- Wrapper exited nonzero at the uncommitted-work build gate after tests passed.
- No new APK, build result, install, headset check, commit or push is claimed.
- Build polling and real-artifact scanning await actual exercise. Bounded credential patterns
  do not prove absence of every secret; preserve ProofBuildSafety and review package before installation.
- Initial zero-test failure preceded Unity importing the new files; explicit recompilation
  and rerun produced the passing results above.

## Why

The baseline is copied rather than rebuilt so its known interaction behavior and reference scenes remain intact. Explicit scene selection and fail-closed test/build checks prevent a successful command from disguising missing tests or the wrong scene. Device acceptance remains pending, and the uncommitted-work guard requires owner direction before an APK build.
