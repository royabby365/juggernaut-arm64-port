# Juggernaut: Revenge of Sovering — 64-bit (arm64) Unity Port

Decompiled (2026-07-17) and lifted into a Unity 2021.3.45f1 project targeting
**Android arm64-v8a + IL2CPP** so it runs on 64-bit-only devices (Pixel 10 Pro).

## Build locally
Open this folder in Unity 2021.3.45f1 (with Android Build Support module).
See `unity_project/README.md` for the full procedure.

## Build via CI (no local Unity needed)
A GameCI workflow (`.github/workflows/android.yml`) builds a signed arm64 APK
in GitHub Actions. Push to a repo, enable Actions, run the workflow.

## Status
- Mechanical + semantic API fixes applied (73 sites across 33 files).
- 2 ambiguous `GetComponent("character")` sites remain — resolve in `assets.cs:137,145`
  (see `Assets/Scripts/SEMANTIC_FIXES.md`).
- Asset reimport from the original game's `assets/bin/Data/` still required.

> Proprietary MY.GAMES title. Personal/archival use only — do not redistribute.
