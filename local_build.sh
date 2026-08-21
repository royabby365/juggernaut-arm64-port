#!/usr/bin/env bash
# Local fast-loop Android build for juggernaut-arm64-port using the
# unityci docker image already on BigDeborah. Replicates exactly what
# game-ci/unity-builder v4 does on the runner:
#   1. extract serial from UNITY_LICENSE (.ulf)
#   2. activate with -serial -username -password
#   3. run BuildScript.BuildPlayer via -executeMethod
#
# Usage:
#   export UNITY_EMAIL=roy.u.abernathy@gmail.com
#   export UNITY_PASSWORD=...
#   ./local_build.sh [buildName] [extra-unity-args...]
set -euo pipefail

REPO=${REPO:-/home/rabby/juggernaut-arm64-port}
ULF=${UNITY_LICENSE_FILE:-/home/rabby/Unity_lic.ulf}
IMAGE=${UNITY_IMAGE:-unityci/editor:ubuntu-2021.3.45f1-android-3}
BUILD_NAME=${1:-Android}
VERSION=${VERSION:-2.4.3}
VERSION_CODE=${ANDROID_VERSION_CODE:-1}

: "${UNITY_EMAIL:?set UNITY_EMAIL (roy.u.abernathy@gmail.com)}"
if [[ -z "${UNITY_PASSWORD:-}" && -f /home/rabby/.unity-pw ]]; then
  UNITY_PASSWORD="$(cat /home/rabby/.unity-pw)"
  echo "[local_build] creds loaded from /home/rabby/.unity-pw"
fi
: "${UNITY_PASSWORD:?set UNITY_PASSWORD}"

# The editor needs the full .ulf license text in-container (builder passes it
# via UNITY_LICENSE); auto-load from ULF if the caller didn't export it.
if [[ -z "${UNITY_LICENSE:-}" && -f "$ULF" ]]; then
  UNITY_LICENSE="$(cat "$ULF")"
  echo "[local_build] license loaded from $ULF (${#UNITY_LICENSE} bytes)"
fi
: "${UNITY_LICENSE:?set UNITY_LICENSE (or place a .ulf at $ULF)}"

SERIAL=$(python3 -c "
import re, base64, sys
lic=open('$ULF').read()
m=re.search(r'<DeveloperData Value=\"([^\"]*)\"', lic)
if not m: sys.exit('serial not found in $ULF')
raw=m.group(1)
try:
    dec=base64.b64decode(raw)
    s=dec.decode('utf-8','replace')
    # strip 4-byte binary header (\x01\x00\x00\x00) if present
    # NOTE: first segment can be short (F4-...); use {2,} not {4,} or the trailing
    # regex match drops the leading F and serial validation fails
    m2=re.search(r'[A-Z0-9]{2,}(?:-[A-Z0-9]{4,})+', s)
    if m2: s=m2.group(0)
except Exception:
    s=raw
if not re.match(r'^[A-Z0-9-]{20,30}$', s): sys.exit('unusable serial: '+s[:8])
print(s)")
echo "[local_build] serial from $ULF (${#SERIAL} chars)"

mkdir -p "$REPO/build"
echo "[local_build] activating license + building (this takes 2-5 min, IL2CPP)..."
docker run --rm \
  -v "$REPO":/github/workspace \
  -w /github/workspace \
  --env UNITY_EMAIL="$UNITY_EMAIL" \
  --env UNITY_PASSWORD="$UNITY_PASSWORD" \
  --env UNITY_SERIAL="$SERIAL" \
  --env UNITY_LICENSE="$UNITY_LICENSE" \
  --env SKIP_ACTIVATION="${SKIP_ACTIVATION:-false}" \
  --cpus="$(nproc)" \
  --memory="$(( $(free -b | awk '/Mem:/{print $7}') / 1024 / 1024 ))m" \
  "$IMAGE" \
  bash -c '
    set -uo pipefail
    # replicate game-ci entrypoint: randomize machine-id for personal (F*) licenses
    if [[ "$UNITY_SERIAL" = F* ]]; then
      echo "[container] Randomizing machine ID for personal license activation"
      # machine-id randomization disabled (use host) && mkdir -p /var/lib/dbus/ && ln -sf /etc/machine-id /var/lib/dbus/machine-id
    fi
    # game-ci createBlankProjectEquivalent: activation needs a valid project dir
        mkdir -p /BlankProject/Assets
        # PRE-CREATE the editor license cache dir (Unity licensing client does
        # Directory.CreateDirectory here; without this the build dies with
        # CreateDirectory /root/.cache/unity3d failed before any license check.)
        mkdir -p /root/.cache/unity3d
        echo "[container] === activation ==="
        unity-editor -logFile /dev/stdout -quit -serial "$UNITY_SERIAL" \
      -username "$UNITY_EMAIL" -password "$UNITY_PASSWORD" \
      -projectPath /BlankProject >/dev/null 2>&1 || true
    echo "[container] === build ==="
    unity-editor -batchmode -nographics -logFile /dev/stdout -quit \
      -projectPath /github/workspace \
      -buildTarget Android -customBuildTarget Android \
      -customBuildPath /github/workspace/build/Android/Android.apk \
      -executeMethod BuildScript.BuildPlayer \
      -buildVersion '"$VERSION"' -androidVersionCode '"$VERSION_CODE"' \
      -androidExportType androidPackage -androidSymbolType none \
      2>&1 | tail -200
  '; echo "[container] done"
RC=$?
echo "[local_build] container exit=$RC"
# Fix root-owned artifacts from Docker build (Library/Bee/Android files are
# created as root inside the unityci container). Next runner cleanup would
# fail with EACCES trying to rmdir root-owned dirs.
if [ -d "$REPO/Library/Bee" ]; then
  chown -R "$(stat -c '%u:%g' "$REPO")" "$REPO/Library/Bee" 2>/dev/null || true
fi
ls -la "$REPO/build/"*.apk 2>/dev/null && echo "[local_build] APK produced (build/juggernaut-arm64.apk)" || echo "[local_build] no APK yet"
exit $RC
