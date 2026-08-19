#!/usr/bin/env python3
"""
Export a COMPLETE skinned-rig bundle to JSON for runtime reconstruction:
  - skeleton: Transform hierarchy (parent + local pos/rot/scl per bone)
  - meshes:   vertex data, UVs, triangles, skin weights (BoneInfluence),
              bindposes, bone order (via SMR.m_Bones -> Transform pids)
  - materials: _MainTex name per mesh

Usage: export_rig.py <bundle> <out.json>
"""
import UnityPy, json, sys
from collections import defaultdict

def vec3(v): return [v.x, v.y, v.z]
def vec2(v): return [v.x, v.y]
def v4(q): return [q.x, q.y, q.z, q.w]

def main():
    bundle = sys.argv[1]
    out = sys.argv[2]
    env = UnityPy.load(bundle)
    objs = {o.path_id: o for o in env.objects}

    # --- Transform hierarchy ---
    skeleton = {}
    for pid, o in objs.items():
        if o.type.name != "Transform":
            continue
        d = o.read()
        f = d.m_Father
        skeleton[str(pid)] = {
            "parent": str(f.path_id) if f and f.path_id else "0",
            "pos": vec3(d.m_LocalPosition),
            "rot": v4(d.m_LocalRotation),
            "scl": vec3(d.m_LocalScale),
        }

    # --- GameObject -> Transform (class id 4) ---
    go_tf = {}
    for pid, o in objs.items():
        if o.type.name != "GameObject":
            continue
        d = o.read()
        for c_tuple in d.m_Component:
            cid, pptr = c_tuple
            if cid == 4:
                go_tf[str(pid)] = str(pptr.path_id)

    # --- SkinnedMeshRenderers: mesh, bones order, root ---
    smr_meshes = []
    for oid, o in objs.items():
        if o.type.name != "SkinnedMeshRenderer":
            continue
        d = o.read()
        mp = d.m_Mesh.path_id if getattr(d.m_Mesh, "path_id", None) else None
        if mp is None:
            continue
        smr_meshes.append({
            "mesh": str(mp),
            "gameobject": str(d.m_GameObject.path_id),
            "bones": [str(b.path_id) for b in d.m_Bones],
            "root": str(d.m_RootBone.path_id) if getattr(d.m_RootBone, "path_id", None) else "0",
        })

    # --- Meshes ---
    meshes = {}
    for pid, o in objs.items():
        if o.type.name != "Mesh":
            continue
        d = o.read()
        sh = d.export()
        data = sh if isinstance(sh, str) else sh.decode("latin1")
        verts, uvs, tris = [], [], []
        for line in data.splitlines():
            p = line.split()
            if not p:
                continue
            if p[0] == "v":
                verts.append([float(x) for x in p[1:4]])
            elif p[0] == "vt":
                uvs.append([float(x) for x in p[1:3]])
            elif p[0] == "f":
                for tok in p[1:]:
                    tris.append(int(tok.split("/")[0]) - 1)
        skin = []
        for s in d.m_Skin:
            skin.append([s.boneIndex_0_, s.boneIndex_1_, s.boneIndex_2_, s.boneIndex_3_,
                         s.weight_0_, s.weight_1_, s.weight_2_, s.weight_3_])
        bind = []
        for m in d.m_BindPose:
            bind.append([m.e00, m.e01, m.e02, m.e03,
                         m.e10, m.e11, m.e12, m.e13,
                         m.e20, m.e21, m.e22, m.e23,
                         m.e30, m.e31, m.e32, m.e33])
        meshes[str(pid)] = {
            "name": d.m_Name,
            "verts": verts,
            "uvs": uvs,
            "tris": tris,
            "skin": skin,
            "bindposes": bind,
        }

    rig = {
        "skeleton": skeleton,
        "go_tf": go_tf,
        "smr": smr_meshes,
        "meshes": meshes,
    }
    with open(out, "w") as f:
        json.dump(rig, f)
    print(f"skeleton bones: {len(skeleton)}")
    print(f"smr entries: {len(smr_meshes)}")
    print(f"meshes: {len(meshes)}")
    print(f"Wrote {out}")

if __name__ == "__main__":
    main()
