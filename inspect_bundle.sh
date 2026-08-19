#!/usr/bin/env bash
# Quick Unity editor session to dump AssetBundle contents (no APK build).
# Usage: bash inspect_bundle.sh [relative-bundle-path]
set -uo pipefail
REPO=/home/rabby/juggernaut-arm64-port
IMAGE=unityci/editor:ubuntu-2021.3.45f1-android-3
ULF=/home/rabby/Unity_lic.ulf
BUNDLE_REL="${1:-scenes/2_iOS.unity3d}"

SERIAL=$(python3 - <<'EOF'
import re, base64
lic=open('/home/rabby/Unity_lic.ulf').read()
m=re.search(r'<DeveloperData Value="([^"]*)"', lic)
raw=m.group(1)
dec=base64.b64decode(raw)
s=dec.decode('utf-8','replace')
m2=re.search(r'[A-Z0-9]{2,}(?:-[A-Z0-9]{4,})+', s)
if m2: s=m2.group(0)
print(s)
EOF
)
PW=$(cat /home/rabby/.unity-pw)

docker run --rm \
  -v "$REPO":/github/workspace \
  -w /github/workspace \
  --env UNITY_EMAIL=roy.u.abernathy@gmail.com \
  --env UNITY_PASSWORD="$PW" \
  --env UNITY_SERIAL="$SERIAL" \
  --env UNITY_LICENSE="$(cat "$ULF")" \
  --env BUNDLE_REL="$BUNDLE_REL" \
  "$IMAGE" bash -c '
    mkdir -p /root/.cache/unity3d
    unity-editor -logFile /dev/stdout -quit -serial "$UNITY_SERIAL" -username "$UNITY_EMAIL" -password "$UNITY_PASSWORD" -projectPath /BlankProject >/dev/null 2>&1 || true
    unity-editor -batchmode -nographics -logFile /dev/stdout -quit -projectPath /github/workspace -executeMethod BundleInspector.DumpBundle 2>&1 | grep -E "BundleInspector|error CS" | head -80
  '
echo "[inspect] done rc=$?"
