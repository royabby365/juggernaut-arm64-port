#!/usr/bin/env bash
# Local fast-loop Android build for juggernaut-arm64-port using the
# unityci docker image already on BigDeborah. Replicates exactly what
# game-ci/unity-builder v4 does on the runner:
#   1. extract serial from UNITY_LICENSE (.ulf) 2. activate with -serial/-username/-password
#   3. run BuildScript.BuildPlayer via -executeMethod
#
# Usage: ./local_build.sh [buildName]
set -euo pipefail

REPO=/home/rabby/juggernaut-arm64-port
ULF=${UNITY_LICENSE_FILE:-/home/rabby/Unity_lic.ulf}
IMAGE=unityci/editor:ubuntu-2021.3.45f1-android-3
BUILD_NAME=${1:-Android}
VERSION=${VERSION:-0.0.13}
VERSION_CODE=${ANDROID_VERSION_CODE:-13}

# Creds come from env (never echo them)
: "${UNITY_EMAIL:?set UNITY_EMAIL}"
: "${UNITY_PASSWORD:?set UNITY_PASSWORD}"

SERIAL=$(python3 -c "
import re,sys
lic=open('$ULF').read()
m=re.search(r'<DeveloperData Value=\"([^\"]*)\"', lic)
if not m: sys.exit('serial not found in $ULF')
print(m.group(1))")
echo "[local_build] serial extracted from $ULF (${#SERIAL} chars, masked)"

mkdir -p "$REPO/build"
echo "[local_build] running Unity build (image pull-free, IL2CPP arm64)..."

docker run --rm \
  -v "$REPO":/github/workspace \
  -w /github/workspace \
  --env UNITY_EMAIL="$UNITY_EMAIL" \
  --env UNITY_PASSWORD="$UNITY_PASSWORD" \
  --env UNITY_SERIAL="$SERIAL" \
  --cpus="$(nproc)" \
  --memory="$(( $(free -b | awk '/Mem:/{print $7}') / 1024 / 1024 ))m" \
  "$IMAGE" \
  bash -c '
    set -euo pipefail
    echo "[container] activating license..."
    unity-editor -logFile /dev/stdout -quit -serial "$UNITY_SERIAL" \
      -username "$UNITY_EMAIL" -password "$UNITY_PASSWORD" -projectPath /BlankProject 2>&1 | tail -5
    echo "[container] running BuildScript.BuildPlayer..."
    unity-editor -batchmode -nographics -logFile /dev/stdout -quit \
      -projectPath /github/workspace \
      -buildTarget Android -customBuildTarget Android \
      -customBuildPath /github/workspace/build/Android/Android.apk \
      -executeMethod BuildScript.BuildPlayer \
      -buildVersion '"$VERSION"' -androidVersionCode '"$VERSION_CODE"' \
      -androidExportType androidPackage -androidSymbolType none \
      2>&1 | grep -vE "^\[Licensing|Player connection|Desktop is|Initialize udev|memorysetup" | tail -60
  '
echo "[local_build] exit=$?"
ls -la "$REPO/build/Android/" 2>/dev/null | grep -E "apk|aab" || echo "[local_build] no APK produced"
