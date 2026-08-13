#!/usr/bin/env bash
# Background Android build for the arm64 port (license pre-seeded + skip activation).
# Awaits the Android platform compile stage; polls itself so we can tail while it runs.
set -uo pipefail
cd /home/rabby/juggernaut-arm64-port || exit 90
export UNITY_EMAIL=roy.u.abernathy@gmail.com
export UNITY_PASSWORD="$(cat /home/rabby/.unity-pw 2>/dev/null)"
export UNITY_LICENSE="$(cat /home/rabby/Unity_lic.ulf)"
export SKIP_ACTIVATION=true
LOG=/tmp/jb31.log
: > "$LOG"
nohup timeout 590 ./local_build.sh >> "$LOG" 2>&1 &
BGPID=$!
echo "build bg pid=$BGPID"
for i in $(seq 1 50); do
  sleep 12
  if ! kill -0 "$BGPID" 2>/dev/null; then
    echo "bg process finished after ~$((i * 12))s"
    break
  fi
done
echo "=== tail log ==="
tail -15 "$LOG"