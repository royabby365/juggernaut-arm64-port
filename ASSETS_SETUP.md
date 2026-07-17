# Asset Setup — required before the build is playable

The code is ported, but **the game's actual content** (scenes, prefabs, textures,
audio, scripts' serialized data) lives in the original APK's `assets/bin/Data/`
folder. None of that is in this repo — you must extract and drop it in, or the
CI/local build compiles an empty shell.

## What you need
From the original `juggernaut.apk` (or any install of the game), the directory:
```
assets/bin/Data/
├── levelN  (or *.asset / scene files — Unity 4.x level format)
├── *.resource
├── Managed/            (we already have the decompiled .cs; ignore the DLLs)
├── Mono/etc/           (not needed)
└── (possibly a separate assets/bin/Data/...) 
```

## How to extract
```bash
# from the original APK
mkdir -p /tmp/jug && cd /tmp/jug
unzip -o /path/to/juggernaut.apk "assets/bin/Data/*" -d extracted/
```
That yields `extracted/assets/bin/Data/`.

## Where it goes
Unity 2021 expects assets under `Assets/`. The cleanest mapping:

| Original path                        | Copy to                                  | Notes |
|--------------------------------------|------------------------------------------|-------|
| `assets/bin/Data/level*`             | `Assets/GameData/Levels/`                | Unity 4.x levels may not import 1:1 — see below |
| `assets/bin/Data/*.resource`         | `Assets/GameData/`                       | Streaming/resource files |
| `assets/bin/Data/Managed/*.dll`      | ❌ DO NOT COPY                           | we already have the C# source |
| textures / audio / bundles           | `Assets/StreamingAssets/` or `Assets/GameData/` | depends on how the code loads them |

Create the target folders first:
```bash
cd unity_project   # or this repo root
mkdir -p Assets/GameData/Levels Assets/StreamingAssets
```

## The honest hard part: Unity 4.x → 2021 asset format
Unity 4.x serialized levels/scenes in a format that **2021 may refuse to import**
(the binary level format changed across 5.x/2018+). If the importer chokes:
1. Open the original project in **Unity 4.x or 5.x** (if you can get one) and
   re-save the scenes — they'll re-serialize in a forward-compatible format.
2. Or use Unity's **Asset Bundle` extraction** (e.g. `UnityAssetsExplorer` /
   `UABE`) to pull textures/models out and re-import as fresh assets.
3. Some gameplay data may be baked into the C# as `const`/serialized fields
   already (we decompiled it) — check `Assets/Scripts/` before assuming loss.

## After adding assets
```bash
git add -A && git commit -m "Add game assets" && git push
```
Then run the GameCI workflow (Actions → Build Android) — it will now have real
content to package into the arm64 APK.

## Verification
- Local: open in Unity 2021.3 → Project window shows the scenes/textures.
- CI: the APK artifact will be meaningfully larger than a few KB.

> Proprietary MY.GAMES content. Personal/archival use only.
