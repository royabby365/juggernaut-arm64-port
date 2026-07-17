#!/usr/bin/env bash
# extract_assets.sh — reassemble the original game's Unity asset containers
# from the decompressed APK (expects /root/juggernaut/extract/assets/bin/Data/).
#
# Unity 4.x packed all game content into serialized .assets containers that were
# split into 1MB chunks (sharedassets0.assets.split0..4) plus mainData and
# "unity default resources". This script reassembles the split file so the
# container is whole and ready for Unity import / UABE extraction.
#
# IMPORTANT: dropping these into Assets/ will NOT "just work" in Unity 2021 —
# the binary format is 4.x and 2021's importer often rejects it. See ASSETS_SETUP.md
# for the UABE / re-save-in-4.x bridge. This script gets you the raw containers.

set -e
SRC="/root/juggernaut/extract/assets/bin/Data"
OUT="/root/juggernaut/juggernaut-unity-port/Assets/GameData"

mkdir -p "$OUT"

echo "[1/3] Reassembling sharedassets0.assets from split chunks..."
if ls "$SRC"/sharedassets0.assets.split* >/dev/null 2>&1; then
  cat "$SRC"/sharedassets0.assets.split* > "$OUT/sharedassets0.assets"
  echo "      -> $OUT/sharedassets0.assets ($(du -h "$OUT/sharedassets0.assets" | cut -f1))"
else
  echo "      no split chunks found, skipping"
fi

echo "[2/3] Copying mainData + unity default resources..."
[ -f "$SRC/mainData" ]                    && cp "$SRC/mainData" "$OUT/mainData"
[ -f "$SRC/unity default resources" ]     && cp "$SRC/unity default resources" "$OUT/unity_default_resources"
[ -f "$SRC/splash.png" ]                  && cp "$SRC/splash.png" "$OUT/splash.png"
[ -f "$SRC/settings.xml" ]                && cp "$SRC/settings.xml" "$OUT/settings.xml"

echo "[3/3] Done. Raw Unity 4.x containers in: $OUT"
echo
echo "Next steps (see ASSETS_SETUP.md):"
echo "  - Try importing $OUT/sharedassets0.assets into Unity 2021.3"
echo "  - If it fails, use UABE (Unity Asset Bundle Extractor) or re-save in Unity 4.x/5.x"
echo "  - Then commit & push: git add -A && git commit -m 'Add game assets' && git push"
