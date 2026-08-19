#!/usr/bin/env bash
# Monitor script for AssetRipper pipeline progress
# Called by cron every 30m - reports via stdout which cron delivers to Telegram
set -euo pipefail

OUTPUT_DIR="/home/rabby/juggernaut-arm64-port/AssetRipperOutput"
DONE_FILE="/tmp/rip_pipeline_done.txt"
RIPPER_PORT=8888

# Check if done marker exists
if [ -f "$DONE_FILE" ]; then
  DONE_AT=$(head -1 "$DONE_FILE" | xargs -I{} date -d @{} '+%Y-%m-%d %H:%M:%S' 2>/dev/null || echo "unknown")
  FILE_COUNT=$(sed -n '3p' "$DONE_FILE" 2>/dev/null || echo "?")
  echo "🟢 AssetRipper pipeline COMPLETED at $DONE_AT"
  echo "   Exported $FILE_COUNT files to $OUTPUT_DIR"
  SIZE=$(du -sh "$OUTPUT_DIR" 2>/dev/null | awk '{print $1}')
  echo "   Total size: $SIZE"
  echo ""
  echo "Next step: review the exported project and merge into Unity"
  echo "   cd /home/rabby/juggernaut-arm64-port"
  echo "   ls AssetRipperOutput/Assets/"
  exit 0
fi

# Check if AssetRipper is still running
RIPPER_ALIVE=0
if curl -sf "http://127.0.0.1:$RIPPER_PORT/" > /dev/null 2>&1; then
  RIPPER_ALIVE=1
fi

# Count exported assets
EXPORTED_FILES=$(find "$OUTPUT_DIR" -type f 2>/dev/null | wc -l)
EXPORTED_SIZE=$(du -sh "$OUTPUT_DIR" 2>/dev/null | awk '{print $1}' || echo "0B")

# Check pipeline log for progress
SCRIPT_RUNNING=0
if pgrep -f rip_pipeline.sh > /dev/null 2>&1; then
  SCRIPT_RUNNING=1
fi
if pgrep -f AssetRipper.GUI > /dev/null 2>&1; then
  ASSET_RIPPER_RUNNING=1
fi

if [ $RIPPER_ALIVE -eq 1 ] || [ $SCRIPT_RUNNING -eq 1 ]; then
  echo "🔄 AssetRipper pipeline IN PROGRESS"
  echo "   Server: $( [ $RIPPER_ALIVE -eq 1 ] && echo '✅ alive' || echo '❌ unreachable')"
  echo "   Script: $( [ $SCRIPT_RUNNING -eq 1 ] && echo '🏃 running' || echo '❌ stopped')"
  echo "   Exported: $EXPORTED_FILES files / $EXPORTED_SIZE"
  echo ""
  echo "   Latest log (tail):"
  tail -5 /tmp/rip_pipeline.log 2>/dev/null | sed 's/^/   /'
  echo ""
  echo "⏱ Next check in ~30 minutes"
else
  echo "⚠️ AssetRipper pipeline seems to have STOPPED"
  echo "   Server: not responding"
  echo "   Script: not running"
  echo "   Exported: $EXPORTED_FILES files / $EXPORTED_SIZE"
  echo ""
  echo "   Latest log:"
  tail -10 /tmp/rip_pipeline.log 2>/dev/null | sed 's/^/   /'
  echo ""
  echo "   ❌ Pipeline may need manual restart from /home/rabby/juggernaut-arm64-port/"
  echo "   Run: bash scripts/rip_pipeline.sh &> /tmp/rip_pipeline.log &"
fi