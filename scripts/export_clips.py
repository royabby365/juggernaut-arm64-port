#!/usr/bin/env python3
"""
Export Unity 4.x legacy AnimationClips to compact JSON:
  { clip: name, fps: 30, curves: { path: { pos: [[t,x,y,z],...], rot: [[t,x,y,z,w],...] } } }

Usage: export_clips.py <bundle> <out.json> [clip1 clip2 ...]
"""
import UnityPy, json, sys

def main():
    bundle = sys.argv[1]
    out = sys.argv[2]
    only = sys.argv[3:] or None

    env = UnityPy.load(bundle)
    clips = []
    for o in env.objects:
        if o.type.name != "AnimationClip":
            continue
        d = o.read()
        nm = d.m_Name
        if only and nm not in only:
            continue
        clip = {"clip": nm, "fps": d.m_SampleRate, "curves": {}}
        for pc in d.m_PositionCurves:
            path = pc.path
            keys = []
            for k in pc.curve.m_Curve:
                keys.append([k.time, k.value.x, k.value.y, k.value.z])
            clip["curves"].setdefault(path, {})["pos"] = keys
        for rc in d.m_RotationCurves:
            path = rc.path
            keys = []
            for k in rc.curve.m_Curve:
                q = k.value
                keys.append([k.time, q.x, q.y, q.z, q.w])
            clip["curves"].setdefault(path, {})["rot"] = keys
        for sc in d.m_ScaleCurves:
            path = sc.path
            keys = []
            for k in sc.curve.m_Curve:
                keys.append([k.time, k.value.x, k.value.y, k.value.z])
            clip["curves"].setdefault(path, {})["scl"] = keys
        clips.append(clip)
        print(f"  {nm}: {len(clip['curves'])} paths")

    with open(out, "w") as f:
        json.dump(clips, f)
    print(f"Wrote {len(clips)} clips -> {out}")

if __name__ == "__main__":
    main()
