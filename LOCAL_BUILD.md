# Local Build — Juggernaut arm64 APK

GameCI can no longer auto-activate a free Unity personal license, so we build
locally. You need a machine with ~15 GB free and internet. A Windows/Mac/Linux
desktop works; this box (BigDeborah) is headless and can't run the Editor.

---

## Step 1 — Install Unity Hub + the editor

1. Download **Unity Hub**: https://unity.com/download
2. Open it, go to **Installs → Install Editor**.
3. Under "Official Releases" find **Unity 2021.3.45f1** (or any 2021.3.x — the
   project is pinned to 2021.3; a close patch is fine but 2021.3.45f1 is safest).
4. **Critical:** in the "Add modules" step, check **Android Build Support**.
   That auto-installs the Android SDK, NDK, and OpenJDK. (If you skip it, the
   Android build target won't appear.)
5. Sign in with your Unity account (`roy.u.abernathy@gmail.com`) when prompted —
   this activates your license locally (the part CI couldn't do).

## Step 2 — Open the project

1. **Open → Add project from disk** → select this repo folder
   (`juggernaut-unity-port/`).
2. Unity will import. First import recompiles the 689 scripts — expect a few
   seconds to a minute. Watch the **Console** tab:
   - Any red errors are likely decompile artifacts (type-name casing, a stray
     `WWW`). Paste them and we'll fix. Most should be clean since the fixes are
     pre-applied.
3. **Asset warning (expected):** `Assets/GameData/` holds Unity **4.x** asset
   containers. Unity 2021 *may* import `sharedassets0.assets` directly. If the
   scenes/meshes don't show, follow `ASSETS_SETUP.md` (UABE extraction or
   re-save in an older Unity). This is the one manual bridge.

## Step 3 — Build the arm64 APK

1. **File → Build Settings** (Ctrl/Cmd+Shift+B).
2. Platform = **Android** (if not listed, you missed Android Build Support in
   Step 1 — go back and add it via Installs → gear icon → Add modules).
3. Click **Player Settings → Other Settings**:
   - **Scripting Backend** = **IL2CPP** ✅ (already set)
   - **Target Architectures** = ☑ **ARM64** only, uncheck ARMv7 (already set)
   - **Minimum API Level** = Android 5.1 (API 22) or higher (already set)
4. Back in Build Settings → **Build**.
5. Choose an output folder (e.g. `build/`). Unity produces `Juggernaut.apk`.

## Step 4 — Install on the Pixel 10 Pro

```
adb install build/Juggernaut.apk
```
(Enable USB debugging on the phone first: Settings → About → tap Build Number 7×
→ Developer Options → USB Debugging.)

Unsigned/debug APK installs fine for personal testing. For a signed release APK,
create a keystore in Player Settings → Publishing Settings and rebuild.

---

## If the build is an empty shell (no scenes)

The code compiled but the game data didn't import. Open `ASSETS_SETUP.md` and
follow the UABE / re-save bridge, then rebuild. That's the single remaining
unknowable from the headless side.

## Need a CI build instead?

You can still use `.github/workflows/android.yml`, but you must first export a
license file from Unity Hub (Help → Manage License → the `.ulf` in
`%APPDATA%\Unity\` on Windows or `~/Library/Unity/` on Mac) and add it as the
`UNITY_LICENSE` secret. GameCI won't generate it for you anymore.
