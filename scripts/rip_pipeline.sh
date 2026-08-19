#!/usr/bin/env bash
# AssetRipper pipeline: start server, load OBB, export Unity project
# Runs synchronously — run via `nohup ./scripts/rip_pipeline.sh > /tmp/rip_pipeline.log 2>&1 &`
set -euo pipefail

RIPPER_DIR="/tmp/AssetRipper"
OBB_DIR="/tmp/juggernaut_obb/obb_contents/assets"
OUTPUT_DIR="/home/rabby/juggernaut-arm64-port/AssetRipperOutput"
LOG="/tmp/rip_pipeline.log"
PIDFILE="/tmp/assetripper.pid"
API="http://127.0.0.1:8888"

export DISPLAY=:0

cleanup() {
  if [ -f "$PIDFILE" ]; then
    kill "$(cat "$PIDFILE")" 2>/dev/null || true
    rm -f "$PIDFILE"
  fi
}
trap cleanup EXIT

echo "[$(date '+%H:%M:%S')] Starting AssetRipper server..."
cd "$RIPPER_DIR"
./AssetRipper.GUI.Free --headless --port 8888 &
RIPPER_PID=$!
echo "$RIPPER_PID" > "$PIDFILE"
echo "[$(date '+%H:%M:%S')] AssetRipper PID=$RIPPER_PID"

# Wait for server to start
for i in $(seq 1 30); do
  if curl -sf http://127.0.0.1:8888/ > /dev/null 2>&1; then
    echo "[$(date '+%H:%M:%S')] Server ready (attempt $i)"
    break
  fi
  sleep 1
done

echo "[$(date '+%H:%M:%S')] Loading OBB directory: $OBB_DIR"
START=$(date +%s)
curl -sf -X POST "$API/LoadFolder" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "path=$OBB_DIR" || {
    echo "[$(date '+%H:%M:%S')] FAILED to load folder (timed out or error)"
    echo "[$(date '+%H:%M:%S')] Trying again with longer timeout..."
    # Retry with -m max-time
    curl -sf -m 300 -X POST "$API/LoadFolder" \
      -H "Content-Type: application/x-www-form-urlencoded" \
      -d "path=$OBB_DIR"
}
END=$(date +%s)
echo "[$(date '+%H:%M:%S')] Load complete (took $((END - START))s)"

# Check what we got
echo "[$(date '+%H:%M:%S')] Checking Collections/Count..."
curl -sf "$API/Collections/Count" 2>/dev/null | python3 -m json.tool 2>/dev/null || echo "NOT JSON, raw: $(curl -sf "$API/Collections/Count" 2>/dev/null)"
echo "[$(date '+%H:%M:%S')] Collections/View (first 20):"
curl -sf "$API/Collections/View" 2>/dev/null | head -c 2000 || echo "(empty)"

# Check failed files
echo "[$(date '+%H:%M:%S')] FailedFiles..."
curl -sf "$API/FailedFiles/View" 2>/dev/null | head -c 1000 || echo "(none)"

# Export as Unity project
echo "[$(date '+%H:%M:%S')] Exporting Unity project to: $OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"
START=$(date +%s)
curl -sf -m 3600 -X POST "$API/Export/UnityProject" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "path=$OUTPUT_DIR" || {
    echo "[$(date '+%H:%M:%S')] Export may still be in progress (curl timeout)"
}
END=$(date +%s)
echo "[$(date '+%H:%M:%S')] Export phase took $((END - START))s"

# Final report
echo "[$(date '+%H:%M:%S')] === FINAL REPORT ==="
echo "Output directory: $OUTPUT_DIR"
find "$OUTPUT_DIR" -type f 2>/dev/null | wc -l | xargs -I{} echo "Total exported files: {}"
du -sh "$OUTPUT_DIR" 2>/dev/null | awk '{print "Total exported size: " $1}'

# Signal completion
echo "$(date '+%s')" > "/tmp/rip_pipeline_done.txt"
echo "$(date '+%Y-%m-%d %H:%M:%S')" >> "/tmp/rip_pipeline_done.txt"
find "$OUTPUT_DIR" -type f 2>/dev/null | wc -l >> "/tmp/rip_pipeline_done.txt"

echo "[$(date '+%H:%M:%S')] Pipeline complete!"