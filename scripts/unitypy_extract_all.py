#!/usr/bin/env python3
"""
Juggernaut ARM64 — UnityPy Batch Asset Extraction Pipeline

Extracts all assets from the 772 Unity 4.x OBB bundles:
  Textures → Resources/__textures/ as PNG
  Meshes   → Assets/Models/ as OBJ
  Audio    → Assets/Audio/ as WAV/OGG  (TBD)
  Animations → AnimationClips exported (TBD)

Usage:
  source /tmp/jug_venv/bin/activate
  python3 scripts/unitypy_extract_all.py [--obb /path/to/obb] [--output /path/to/output] [--dry-run]

Requirements:
  pip install UnityPy Pillow
"""

import UnityPy
import argparse
import json
import os
import sys
from collections import Counter

EXTRACTORS = {}


def register(type_name):
    def wrapper(fn):
        EXTRACTORS[type_name] = fn
        return fn
    return wrapper


@register("Texture2D")
def extract_texture(obj, output_dir, stats):
    data = obj.read()
    tex_dir = os.path.join(output_dir, "__textures")
    os.makedirs(tex_dir, exist_ok=True)
    fname = data.m_Name.replace("/", "_") if data.m_Name else f"tex_{obj.path_id}"
    fpath = os.path.join(tex_dir, f"{fname}.png")
    if os.path.exists(fpath):
        stats["textures_skipped"] += 1
        return
    img = data.image
    if img:
        img.save(fpath, format="PNG")
        stats["textures_written"] += 1
        stats["texture_bytes"] += os.path.getsize(fpath)
    else:
        stats["textures_null"] += 1


@register("Mesh")
def extract_mesh(obj, output_dir, stats):
    data = obj.read()
    mesh_dir = os.path.join(output_dir, "Models")
    os.makedirs(mesh_dir, exist_ok=True)
    fname = data.m_Name.replace("/", "_") if data.m_Name else f"mesh_{obj.path_id}"
    fpath = os.path.join(mesh_dir, f"{fname}.obj")
    if os.path.exists(fpath):
        stats["meshes_skipped"] += 1
        return
    try:
        obj_data = data.export()
        if obj_data:
            with open(fpath, "w") as f:
                f.write(obj_data)
            stats["meshes_written"] += 1
            stats["mesh_bytes"] += os.path.getsize(fpath)
        else:
            stats["meshes_empty"] += 1
    except Exception as e:
        stats["meshes_errors"] += 1
        stats["mesh_errors"].append(f"{obj.path_id} ({data.m_Name}): {e}")


# Audio format mapping for Unity 4.x
AUDIO_FORMATS = {
    0: ".raw",    # PCM RAW
    1: ".wav",    # ADPCM
    2: ".mp3",    # MPEG/MP3 (Unity 4 Android)
    3: ".xm",
    4: ".mod",
    5: ".it",
    6: ".s3m",
    7: ".fsb",    # FMOD Sound Bank
    8: ".mp3",    # MPEG/MP3
    9: ".wav",    # PCM WAV
}


@register("AudioClip")
def extract_audio(obj, output_dir, stats):
    data = obj.read()
    audio_dir = os.path.join(output_dir, "Audio")
    os.makedirs(audio_dir, exist_ok=True)
    fname = data.m_Name.replace("/", "_") if data.m_Name else f"audio_{obj.path_id}"
    fmt = getattr(data, "m_Format", -1)
    ext = AUDIO_FORMATS.get(fmt, ".bin")
    fpath = os.path.join(audio_dir, f"{fname}{ext}")
    if os.path.exists(fpath):
        stats["audio_skipped"] += 1
        return
    raw = data.m_AudioData
    if raw and isinstance(raw, (list, tuple)):
        raw_bytes = bytes(raw)
        if raw_bytes:
            with open(fpath, "wb") as f:
                f.write(raw_bytes)
            stats["audio_written"] += 1
            stats["audio_bytes"] += len(raw_bytes)
        else:
            stats["audio_empty"] += 1
    elif isinstance(raw, (bytes, bytearray)):
        with open(fpath, "wb") as f:
            f.write(raw)
        stats["audio_written"] += 1
        stats["audio_bytes"] += len(raw)
    else:
        stats["audio_skipped"] += 1


def extract_bundle(bundle_path, output_dir, stats):
    """Extract all assets from a single Unity 4.x bundle."""
    try:
        env = UnityPy.load(bundle_path)
    except Exception as e:
        stats["bundles_errors"] += 1
        return

    stats["bundles_ok"] += 1
    for obj in env.objects:
        t = obj.type.name
        if t in EXTRACTORS:
            try:
                EXTRACTORS[t](obj, output_dir, stats)
            except Exception as e:
                stats["extract_errors"] += 1
                if "extract_error_list" not in stats:
                    stats["extract_error_list"] = []
                stats["extract_error_list"].append(f"{bundle_path} #{obj.path_id} ({t}/{obj.type.name}): {e}")

    # Report bundle type distribution
    if stats["bundles_ok"] <= 50:  # Track types for first 50 bundles
        env_types = Counter(o.type.name for o in env.objects)
        for t, c in env_types.items():
            stats.setdefault("type_counts", Counter())[t] += c


def main():
    parser = argparse.ArgumentParser(description="UnityPy batch extractor for Juggernaut OBB")
    parser.add_argument("--obb", default="/tmp/juggernaut_obb/obb_contents/assets/android",
                        help="Path to OBB android/ directory")
    parser.add_argument("--output", default="extracted_unitypy",
                        help="Output directory for extracted assets")
    parser.add_argument("--dry-run", action="store_true",
                        help="Scan bundles without extracting")
    args = parser.parse_args()

    stats = {
        "bundles_found": 0,
        "bundles_ok": 0,
        "bundles_errors": 0,
        "textures_written": 0,
        "textures_skipped": 0,
        "textures_null": 0,
        "texture_bytes": 0,
        "meshes_written": 0,
        "meshes_skipped": 0,
        "meshes_empty": 0,
        "meshes_errors": 0,
        "mesh_bytes": 0,
        "mesh_errors": [],
        "audio_written": 0,
        "audio_skipped": 0,
        "audio_empty": 0,
        "audio_bytes": 0,
        "extract_errors": 0,
    }

    # Walk all bundles recursively
    bundle_paths = []
    for root, dirs, files in os.walk(args.obb):
        for f in sorted(files):
            if f.endswith(".unity3d"):
                bundle_paths.append(os.path.join(root, f))

    stats["bundles_found"] = len(bundle_paths)
    print(f"Found {len(bundle_paths)} bundles in {args.obb}")

    if args.dry_run:
        # Just scan and report
        cat_sizes = Counter()
        for bp in bundle_paths:
            rel = os.path.relpath(bp, args.obb)
            cat = rel.split(os.sep)[0] if os.sep in rel else "root"
            cat_sizes[cat] += os.path.getsize(bp)
        print(f"\nBundle categories:")
        for cat, sz in sorted(cat_sizes.items()):
            print(f"  {cat}: {sz / 1024 / 1024:.1f} MB")
        print(f"\nTotal: {sum(cat_sizes.values()) / 1024 / 1024:.1f} MB")
        return

    os.makedirs(args.output, exist_ok=True)
    stats_log = os.path.join(args.output, "extraction_stats.json")

    for i, bp in enumerate(bundle_paths):
        rel = os.path.relpath(bp, args.obb)
        if i % 10 == 0:
            print(f"  [{i}/{len(bundle_paths)}] {rel}...")
            # Save progress periodically
            with open(stats_log, "w") as f:
                json.dump(stats, f, indent=2, default=str)

        extract_bundle(bp, args.output, stats)

    # Final save
    with open(stats_log, "w") as f:
        json.dump(stats, f, indent=2, default=str)

    print(f"\n=== Extraction Complete ===")
    print(f"Bundles: {stats['bundles_ok']} OK, {stats['bundles_errors']} errors of {stats['bundles_found']}")
    print(f"Textures: {stats['textures_written']} written, {stats['textures_skipped']} skipped, {stats['textures_null']} null")
    if stats['texture_bytes']:
        print(f"  Total: {stats['texture_bytes'] / 1024 / 1024:.1f} MB")
    print(f"Meshes: {stats['meshes_written']} written, {stats['meshes_skipped']} skipped, {stats['meshes_empty']} empty")
    if stats['mesh_bytes']:
        print(f"  Total: {stats['mesh_bytes'] / 1024 / 1024:.1f} MB")
    print(f"Audio: {stats['audio_written']} written, {stats['audio_skipped']} skipped, {stats['audio_empty']} empty")
    if stats['audio_bytes']:
        print(f"  Total: {stats['audio_bytes'] / 1024 / 1024:.1f} MB")
    if stats['extract_errors']:
        print(f"Extract errors: {stats['extract_errors']}")
    if stats['mesh_errors']:
        print(f"\nMesh errors ({len(stats['mesh_errors'])}):")
        for e in stats['mesh_errors'][:5]:
            print(f"  {e}")

    if "type_counts" in stats:
        print(f"\nAsset type distribution (first {min(50, stats['bundles_found'])} bundles):")
        for t, c in stats["type_counts"].most_common(20):
            print(f"  {t}: {c}")


if __name__ == "__main__":
    main()