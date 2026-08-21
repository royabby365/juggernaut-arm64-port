#!/usr/bin/env bash
# Compute-gate: cheap batchmode editor pass to catch C# errors before the 10-min APK build.
set -uo pipefail
REPO=/home/rabby/juggernaut-arm64-port
IMAGE=unityci/editor:ubuntu-2021.3.45f1-android-3
UNITY_EMAIL=roy.u.abernathy@gmail.com
UNITY_PW="$(cat /home/rabby/.unity-pw)"
UNITY_LICENSE="$(cat /home/rabby/Unity_lic.ulf)"
SERIAL=$(python3 -c "
import re,base64
lic=open('/home/rabby/Unity_lic.ulf').read()
m=re.search(r'<DeveloperData Value=\"([^\"]*)\"', lic)
raw=m.group(1)
try:
    dec=base64.b64decode(raw).decode('utf-8','replace')
    m2=re.search(r'[A-Z0-9]{2,}(?:-[A-Z0-9]{4,})+', dec)
    if m2: raw=m2.group(0)
except Exception: pass
print(raw)")
echo "serial=${#SERIAL} chars"
docker run --rm \
  -v "$REPO":/workspace \
  --env UNITY_SERIAL="$SERIAL" \
  --env UNITY_EMAIL="$UNITY_EMAIL" \
  --env UNITY_PASSWORD="$UNITY_PW" \
  --env UNITY_LICENSE="$UNITY_LICENSE" \
  "$IMAGE" bash -c 'xvfb-run -a -s "-screen 0 1280x1024x24" /opt/unity/Editor/Unity -batchmode -quit -serial "$UNITY_SERIAL" -username "$UNITY_EMAIL" -password "$UNITY_PASSWORD" -logFile /workspace/build/compile_check2.log -projectPath /workspace -executeMethod CharacterPreview.Render -previewOut /workspace/build/char_preview.png'
RC=$?
echo "GATE_EXIT=$RC"
echo "CS errors: $(grep -c 'error CS' "$REPO/build/compile_check2.log" 2>/dev/null)"
grep -E "error CS|Compilation failed" "$REPO/build/compile_check2.log" 2>/dev/null | head -15
exit $RC