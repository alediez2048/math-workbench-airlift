#!/bin/bash
# One command: build the work-in-progress lounge, install it beside the release app, launch it.
#
#   bash scripts/lounge-preview.sh            build, install, launch
#   bash scripts/lounge-preview.sh --install  skip the build, install the APK already on disk
#
# It exists because driving the Unity pipeline by hand cost more time than the build itself. Polls every 5s,
# installs inline, and ALWAYS puts the package id back to com.nerdy.vr so nothing leaks into a release build.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_PROJECT="$ROOT/unity"
APK="$ROOT/artifacts/lounge-preview/nerdy-lounge.apk"
ADB=/Applications/Unity/Hub/Editor/6000.6.0f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb
PKG=com.nerdy.vr.lounge

cd "$UNITY_PROJECT"
run_script() { unity command run_script --file "$UNITY_PROJECT/$1" --entry "$2" --timeout_ms 25000 --format json >/dev/null 2>&1 || true; }
restore() { run_script AgentScripts/LoungePreviewSettings.cs LoungePreviewSettings.Restore; }
trap restore EXIT          # a failed or interrupted build must never leave the preview id behind

if [ "${1:-}" != "--install" ]; then
  echo "→ preview package id"
  run_script AgentScripts/LoungePreviewSettings.cs LoungePreviewSettings.Apply
  mkdir -p "$(dirname "$APK")"; rm -f "$APK"
  echo "→ building (usually 3-4 min)"
  unity command build Android "$APK" '' '["DetailedBuildReport"]' \
    '["Assets/Airlift/Scenes/CargoCrew.unity"]' true false --format json >/dev/null
  for _ in $(seq 1 240); do [ -f "$APK" ] && break; sleep 5; done
  [ -f "$APK" ] || { echo "build produced no APK"; exit 1; }
  sleep 3                                   # let the writer finish
fi

echo "→ installing $(du -h "$APK" | cut -f1)"
"$ADB" install -r "$APK" | tail -1
"$ADB" logcat -c || true
"$ADB" shell am force-stop "$PKG" || true
"$ADB" shell monkey -p "$PKG" -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1
echo "→ launched on the headset"
