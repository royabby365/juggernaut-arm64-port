#!/usr/bin/env python3
"""
Bake a full skinned character armor set at bind pose into Unity-ready OBJs.

For each SkinnedMeshRenderer in the bundle:
  - read raw mesh OBJ (verts, uvs, faces)
  - transform verts by the SMR GameObject's world matrix (bind pose assembly)
  - write a complete OBJ (v/vt/f + usemtl) into Assets/Models/__baked/
  - write a small .mtl pointing at the matching __textures PNG

Also writes a rig.json documenting part -> transform + texture.
"""
import UnityPy, numpy as np, os, json, sys

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
    return tr, wm_cache

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

def bake_bundle(bundle_path, out_dir, tex_dir):
    env = UnityPy.load(bundle_path)
    objs = {o.path_id: o for o in env.objects}
    tr, wm_cache = build_world(objs)

    # GameObject -> transform
    go_tf={}
    for pid,o in objs.items():
        if o.type.name!="GameObject": continue
        d=o.read()
        for c_tuple in d.m_Component:
            cid, pptr = c_tuple
            if cid==4: go_tf[pid]=pptr.path_id

    name_of={}
    for pid,o in objs.items():
        try: name_of[pid]=o.read().m_Name
        except Exception: pass

    os.makedirs(out_dir, exist_ok=True)
    rig=[]
    written=[]
    seen=set()
    for oid,o in objs.items():
        # Process BOTH SkinnedMeshRenderer and MeshRenderer+MeshFilter parts
        is_smr = o.type.name=="SkinnedMeshRenderer"
        is_mr = o.type.name=="MeshRenderer"
        if not (is_smr or is_mr): continue
        d=o.read()
        gp=d.m_GameObject.path_id
        tfp=go_tf.get(gp)
        if tfp is None or tfp not in tr: continue
        W=wm_cache[tfp]
        # resolve mesh: SMR uses m_Mesh; MR needs the GO's MeshFilter
        mp=None
        if is_smr:
            mp=d.m_Mesh.path_id
        else:
            go_d=objs[gp].read()
            for c_tuple in go_d.m_Component:
                cid, pptr = c_tuple
                if cid==33:  # MeshFilter
                    mf=pptr.read()
                    mp=getattr(mf.m_Mesh,'path_id',None)
        if mp is None or mp not in objs: continue
        mesh_o=objs[mp]
        md=mesh_o.read()
        sh=md.export()
        if sh is None: continue
        vs, vts, faces = parse_export(sh)
        if len(vs)==0: continue
        Vh=np.hstack([vs, np.ones((len(vs),1))])
        posed=(W@Vh.T).T[:,:3]

        go_name=name_of.get(gp,'') or md.m_Name
        safe=go_name.replace(' ','_')
        # texture resolution: material _MainTex name if valid, else mesh-name_ds, else part_ds
        tex_name=""
        try:
            mats=d.m_Materials
            if mats:
                mt=mats[0].read()
                tt=getattr(mt,'m_SavedProperties',None)
                if tt is not None and tt.m_TexEnvs:
                    for kv in tt.m_TexEnvs:
                        fpn, texenv = kv
                        if getattr(fpn,'name',None)=="_MainTex" or getattr(fpn,'m_Name',None)=="_MainTex":
                            tex_name=getattr(texenv.m_Texture.read(),"m_Name","")
        except Exception:
            pass
        def tex_exists(nm):
            return os.path.exists(os.path.join(tex_dir, nm+".png"))
        if not (tex_name and tex_exists(tex_name)):
            cand=md.m_Name+"_ds"
            tex_name = cand if tex_exists(cand) else (safe+"_ds" if tex_exists(safe+"_ds") else tex_name)
        # dedupe: same mesh + same transform + same texture -> skip
        key=(mp, tuple(np.round(W,3).flatten().tolist()), tex_name)
        if key in seen:
            print(f"  (dup skip) {go_name}")
            continue
        seen.add(key)
        obj_path=os.path.join(out_dir, safe+".obj")
        lines=[f"mtllib {safe}.mtl", f"o {go_name}", f"usemtl {safe}"]
        for p in posed:
            lines.append(f"v {p[0]:.6f} {p[1]:.6f} {p[2]:.6f}")
        for t in vts:
            lines.append(f"vt {t[0]:.6f} {t[1]:.6f}")
        for f in faces:
            lines.append("f "+" ".join(f))
        with open(obj_path,"w") as f: f.write("\n".join(lines)+"\n")
        # mtl
        tex_png = tex_name if tex_name else (safe+"_ds")
        mtl_path=os.path.join(out_dir, safe+".mtl")
        with open(mtl_path,"w") as f:
            f.write(f"newmtl {safe}\nKa 1 1 1\nKd 1 1 1\nKs 0 0 0\nd 1\nillum 1\nmap_Kd {tex_dir}/{tex_png}.png\n")
        rig.append({"part":go_name,"obj":safe+".obj","texture":tex_png,"verts":int(len(vs))})
        written.append((go_name, posed))
        lo,hi=posed.min(0),posed.max(0)
        print(f"  {go_name:34} x[{lo[0]:6.2f},{hi[0]:6.2f}] y[{lo[1]:6.2f},{hi[1]:6.2f}] z[{lo[2]:6.2f},{hi[2]:6.2f}] n={len(vs)} tex={tex_png}")
    with open(os.path.join(out_dir,"rig.json"),"w") as f:
        json.dump(rig,f,indent=1)
    return written

if __name__=="__main__":
    bundle=sys.argv[1] if len(sys.argv)>1 else "/tmp/juggernaut_obb/obb_contents/assets/android/characters/1/armors/10/4_torso.unity3d"
    out=sys.argv[2] if len(sys.argv)>2 else "/home/rabby/juggernaut-arm64-port/Assets/Models/__baked"
    tex=sys.argv[3] if len(sys.argv)>3 else "../__textures"
    print(f"Baking {bundle}")
    bake_bundle(bundle, out, tex)
    print("done")