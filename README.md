# Juggernaut: Revenge of Sovering — 64-bit (arm64) Unity Port

Decompiled (2026-07-17) and lifted into a **Unity 2021.3.45f1** project targeting
**Android arm64-v8a + IL2CPP**, so it runs on 64-bit-only devices (Pixel 10 Pro,
Android 16). Private repo — proprietary MY.GAMES title, personal/archival use only.

---

## What's in this repo (all automated work done)
- `Assets/Scripts/` — 689 decompiled `.cs` files, **all Unity 4.x→2021 API fixes applied**
  (73 sites: 18 mechanical + 55 semantic; zero `GetComponent("string")` leftovers)
- `Assets/GameData/` — reassembled original Unity 4.x asset containers
  (`sharedassets0.assets`, `mainData`, `unity default resources`) — see caveats
- `ProjectSettings/` — ARM64 (arm64-v8a) ONLY + IL2CPP + minSdk 22, pre-set
- `Packages/manifest.json`, `Assets/Plugins/Android/AndroidManifest.xml`
- `.github/workflows/android.yml` — GameCI build → signed arm64 APK artifact
- `extract_assets.sh` — reassembles the asset containers (already run)
- `ASSETS_SETUP.md` — the one remaining manual bridge (asset format)

---

## Turnkey build — 3 steps

### 1. Add GitHub Secrets (Settings → Secrets and variables → Actions → New repository secret)
| Secret | Value |
|---|---|
| `UNITY_LICENSE` | Contents of your `Unity.lic` / `unity-editor.license` file |
| `UNITY_EMAIL` | Your Unity account email |
| `UNITY_PASSWORD` | Your Unity account password |

Generate `UNITY_LICENSE` once (free Personal license): run the
[game-ci/activation](https://github.com/game-ci/activation) step, open the issued
`Unity` PR on your fork, download `unity-editor.lic`, paste its contents as the secret.

Optional — for a **signed** APK (needed to install outside Play Store):
| Secret | Value |
|---|---|
| `ANDROID_KEYSTORE_BASE64` | `base64 -w0 my.keystore` |
| `ANDROID_KEYSTORE_PASS` | keystore password |
| `ANDROID_KEYALIAS_NAME` | alias name |
| `ANDROID_KEYALIAS_PASS` | alias password |

### 2. Run the workflow
**Actions → "Build Android (arm64) APK" → Run workflow.**
GameCI installs Unity 2021.3 + Android (SDK/NDK), builds IL2CPP/arm64, uploads the
APK as artifact `juggernaut-arm64-apk`. Download → `adb install` on the Pixel.

### 3. (Only if the build is an empty shell) Fix asset import
The `Assets/GameData/` containers are Unity **4.x** format. Unity 2021 *may* import
`sharedassets0.assets` directly; if it rejects it, bridge via UABE or re-save in
4.x/5.x — full steps in `ASSETS_SETUP.md`. After bridging, commit & push; re-run
the workflow.

---

## Local build (alternative to CI)
Open this folder in **Unity 2021.3.45f1** (with Android Build Support).
Build Settings → Android → IL2CPP → ARM64 only → Build.

---

## Honest status / risks
- ✅ Code fully ported & pre-fixed. ✅ Build config + CI ready. ✅ Assets extracted.
- ⚠️ Unity 4.x→2021 **asset format** is the only real unknown — may need a manual
  bridge (UABE / re-save). Cannot be validated headless (no Unity Editor here).
- ⚠️ First compile may surface a few decompile-artifact issues (type-name casing,
  any `WWW` usage). Normal — quick to clear in the Editor/CI log.
- ⚠️ Proprietary title — do not redistribute.
