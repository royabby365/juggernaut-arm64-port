#!/usr/bin/env python3
"""
Bake a Juggernaut ARENA scene (static MeshRenderer + MeshFilter parts) at world
transforms into Unity-ready OBJs. Mirrors bake_bindpose.py but for scenes:
each part's final transform = W(GameObject that owns the MeshRenderer).

Usage: bake_arena.py <scene_bundle> <out_dir> <tex_dir>
"""
import UnityPy, numpy as np, os, sys

def rotmat(q):
    x, y, z, w = q
    xx=x*x;yy=y*y;zz=z*z;xy=x*y;xz=x*z;yz=y*z;wx=w*x;wy=w*y;wz=w*z
    return np.array([[1-2*(yy+zz),2*(xy-wz),2*(xz+wy)],
                     [2*(xy+wz),1-2*(xx+zz),2*(yz-wx)],
                     [2*(xz-wy),2*(yz+wx),1-2*(xx+yy)]])
def mktrs(p, q, s):
    M = np.eye(4); M[:3,:3] = np.diag([s[0],s[1],s[2]]) @ rotmat(q); M[:3,3] = p; return M
def v3(v): return (v.x, v.y, v.z)

def build_world(objs):
    tr = {}
    for pid, o in objs.items():
        if o.type.name!="Transform": continue
        d=o.read()
        p=v3(d.m_LocalPosition); q=(d.m_LocalRotation.x,d.m_LocalRotation.y,d.m_LocalRotation.z,d.m_LocalRotation.w)
        s=v3(d.m_LocalScale); f=d.m_Father
        tr[pid] = (f.path_id if f else 0, mktrs(p, q, s))
    wm_cache = {0: np.eye(4)}
    def wm(pid):
        if pid in wm_cache: return wm_cache[pid]
        stack=[]; cur=pid; seen=set()
        while cur!=0 and cur not in seen:
            seen.add(cur); fp,L=tr[cur]; stack.append((fp,L)); cur=fp
        m=np.eye(4)
        for fp,L in reversed(stack): m=m@L
        wm_cache[pid]=m; return m
    for pid in tr: wm(pid)
    return wm_cache

def parse_export(sh):
    data = sh if isinstance(sh, str) else sh.decode('latin1')
    vs=[]; vts=[]; faces=[]
    for line in data.splitlines():
        if line.startswith('v '):
            vs.append([float(x) for x in line.split()[1:4]])
        elif line.startswith('vt '):
            vts.append([float(x) for x in line.split()[1:3]])
        elif line.startswith('f '):
            faces.append(line.split()[1:])
    return np.array(vs,float), np.array(vts,float), faces

def tex_name_of_material(mtrl, objs):
    """Return (texture_name, uv_scale_xy) from a Material's _MainTex TexEnv."""
    try:
        tt = getattr(mtrl, 'm_SavedProperties', None)
        if tt is not None and tt.m_TexEnvs:
            for kv in tt.m_TexEnvs:
                fpn, texenv = kv
                if getattr(fpn,'name',None) == "_MainTex" or getattr(fpn,'m_Name',None) == "_MainTex":
                    scale = texenv.m_Scale
                    t = texenv.m_Texture
                    nm = ""
                    if t is not None and t.path_id:
                        try: nm = getattr(t.read(), "m_Name", "")
                        except Exception: pass
                    return nm, (scale.x, scale.y)
    except Exception:
        pass
    return "", (1.0, 1.0)

def bake_scene(bundle_path, out_dir, tex_dir):
    env = UnityPy.load(bundle_path)
    objs = {o.path_id: o for o in env.objects}
    wm_cache = build_world(objs)

    # GameObject -> its Transform (class id 4)
    go_tf = {}
    for pid,o in objs.items():
        if o.type.name != "GameObject": continue
        d = o.read()
        for c_tuple in d.m_Component:
            try:
                cid, pptr = c_tuple
                if cid == 4: go_tf[pid] = pptr.path_id
            except Exception:
                pass

    name_of = {}
    for pid,o in objs.items():
        try: name_of[pid] = o.read().m_Name
        except Exception: pass

    os.makedirs(out_dir, exist_ok=True)
    rig = []
    seen = set()
    written = 0
    for oid,o in objs.items():
        if o.type.name != "MeshRenderer": continue
        d = o.read()
        gp = d.m_GameObject.path_id
        tfp = go_tf.get(gp)
        if tfp is None or tfp not in wm_cache: continue
        W = wm_cache[tfp]
        # resolve mesh from GO's MeshFilter (class id 33)
        mp = None
        go_d = objs[gp].read()
        for c_tuple in go_d.m_Component:
            try:
                cid, pptr = c_tuple
                if cid == 33:
                    mf = pptr.read()
                    mp = getattr(mf.m_Mesh,'path_id',None)
            except Exception:
                pass
        if mp is None or mp not in objs: continue
        md = objs[mp].read()
        sh = md.export()
        if sh is None: continue
        vs, vts, faces = parse_export(sh)
        if len(vs) == 0: continue
        # dedupe: same mesh+transform
        key = (mp, tuple(np.round(W,3).flatten().tolist()))
        if key in seen: continue
        seen.add(key)

        # material -> texture + UV tiling (MeshRenderer m_Materials[0])
        tex_name = ""
        uv_scale = (1.0, 1.0)
        mat_name = ""
        try:
            mats = d.m_Materials
            if mats:
                try:
                    m0 = mats[0].read()
                    mat_name = getattr(m0, "m_Name", "")
                    tex_name, uv_scale = tex_name_of_material(m0, objs)
                except Exception:
                    pass
        except Exception:
            pass
        # Fallback: material-name convention (shared-bundle materials can't be
        # dereferenced, but their names map to textures we DO have)
        if not tex_name and mat_name:
            conv = {
                "1_bg": "arena_01_bg_01",
                "1_bg_02": "arena_01_bg_02",
                "ground_b": "01_tile",
                "ground_c": "01_tile",
                "ground_d": "arena_08_floor_decals",
                "ani_fire_01": "ani_fire_01",
                "ani_smoke_01": "ani_smoke_01",
            }
            if mat_name in conv:
                tex_name = conv[mat_name]
                if mat_name in ("ground_b", "ground_c"):
                    uv_scale = (24.0, 24.0)  # tile floor pattern
        go_name = name_of.get(gp,'') or md.m_Name
        safe = go_name.replace(' ','_') or f"part_{oid}"
        # unique filename if duplicates
        if safe in [r["part"] for r in rig]:
            safe = f"{safe}_{oid}"
        if not tex_name:
            # try mesh-name-based fallback
            cand = md.m_Name + ".png"
            tex_name = md.m_Name if os.path.exists(os.path.join(tex_dir, cand)) else ""

        Vh = np.hstack([vs, np.ones((len(vs),1))])
        posed = (W @ Vh.T).T[:,:3]
        obj_path = os.path.join(out_dir, safe + ".obj")
        lines = [f"mtllib {safe}.mtl", f"o {go_name}", f"usemtl {safe}"]
        for p in posed:
            lines.append(f"v {p[0]:.6f} {p[1]:.6f} {p[2]:.6f}")
        for t in vts:
            lines.append(f"vt {t[0]*uv_scale[0]:.6f} {t[1]*uv_scale[1]:.6f}")
        for f in faces:
            lines.append("f " + " ".join(f))
        with open(obj_path,"w") as f: f.write("\n".join(lines)+"\n")
        with open(os.path.join(out_dir, safe+".mtl"),"w") as f:
            f.write(f"newmtl {safe}\nKa 1 1 1\nKd 1 1 1\nKs 0 0 0\nd 1\nillum 1\n")
            if tex_name:
                f.write(f"map_Kd {tex_dir}/{tex_name}.png\n")
        rig.append({"part":go_name,"obj":safe+".obj","texture":tex_name,"mesh":md.m_Name,"verts":int(len(vs))})
        written += 1
        lo,hi = posed.min(0), posed.max(0)
        print(f"  {go_name:24} mesh={md.m_Name:14} tex={tex_name:20} "
              f"x[{lo[0]:7.2f},{hi[0]:7.2f}] y[{lo[1]:7.2f},{hi[1]:7.2f}] z[{lo[2]:7.2f},{hi[2]:7.2f}] n={len(vs)}")
    with open(os.path.join(out_dir,"rig.json"),"w") as f:
        json.dump(rig,f,indent=1)
    print(f"  -> {written} parts baked")

if __name__ == "__main__":
    import json
    bundle = sys.argv[1] if len(sys.argv)>1 else "/tmp/juggernaut_obb/obb_contents/assets/android/scenes/1_iOS.unity3d"
    out = sys.argv[2] if len(sys.argv)>2 else "/home/rabby/juggernaut-arm64-port/Assets/Models/__baked_arena"
    tex = sys.argv[3] if len(sys.argv)>3 else "../__textures"
    print(f"Baking {bundle}")
    bake_scene(bundle, out, tex)